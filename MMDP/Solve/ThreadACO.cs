using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using MMDP.Util;

namespace MMDP.Solve
{
    public class ThreadACO
    {
        private int _nOfAnts = 5;
        private float alpha = 0.5f;
        private float belta = 1f;
        private float _rho = 0.5f;
        private float _gbAdditionalWeight = 0.5f;

        public List<List<float>> matriceList;
        private List<float> _curMatrice;

        //select a subset M of m elements (|M|=m) from a set N of n elements
        private int _curN;
        private int _curM;
        private List<float> _pheromones = new List<float>();
        private Random _rnd;
        private List<Solution> _solutions = new List<Solution>();
        private List<ManualResetEvent> _doneEvents = new List<ManualResetEvent>();
        private List<ConstructAntSolution> _constructionAntSolutionOpts = new List<ConstructAntSolution>();
        private List<ApplyLS> _applyLSOpts = new List<ApplyLS>();
        private ParallelACO _pACO;

        public int CurN 
        { 
            get
            {
                return _curN;
            }
        }
        public int CurM 
        { 
            get
            {
                return _curM;
            }
        }
        public Random Rnd 
        {
            get
            {
                return _rnd;
            }
        }
        public float Alpha
        {
            get
            {
                return alpha;
            }
        }
        public float Belta
        {
            get
            {
                return belta;
            }
        }
        public List<Solution> Solutions
        {
            get
            {
                return _solutions;
            }
        }
        public List<float> Pheromones
        {
            get
            {
                return _pheromones;
            }
        }
        public List<float> CurMatrice
        {
            get
            {
                return _curMatrice;
            }
        }


        public ThreadACO(List<List<float>> matriceList, ParallelACO pACO)
        {
            this.matriceList = matriceList;
            _rnd = new Random();
            _pACO = pACO;
        }

        public void ACO(ACOParams parameters)
        {
            int instanceIndex = parameters.instanceIndex;
            float mRatio = parameters.mRatio;
            int maxIter = parameters.maxIter;
            int nAnts = parameters.nAnts;

            _curMatrice = matriceList[instanceIndex];
            _curN = (int)Math.Sqrt(_curMatrice.Count);
            _curM = (int)Math.Round(mRatio * _curN);
            _nOfAnts = nAnts;
            init();
            int iter = 0;

            while (iter < maxIter)
            {
                reset();
                constructAntSolution();
                applyLS();
                globalUpdatePheromones();
                iter++;
            }
        }

        private void init()
        {
            //init pheromones
            //init probability
            _pheromones.Clear();
            for (int i = 0; i < _curN; i++)
            {
                _pheromones.Add(1f);
            }

            //init solutions for ants
            _solutions.Clear();
            _doneEvents.Clear();
            _constructionAntSolutionOpts.Clear();
            _applyLSOpts.Clear();
            for (int i = 0; i < _nOfAnts; i++)
            {
                _solutions.Add(new Solution());
                _doneEvents.Add(new ManualResetEvent(false));
                _constructionAntSolutionOpts.Add(new ConstructAntSolution(i, this, _doneEvents[i]));
                _applyLSOpts.Add(new ApplyLS(i, this, _doneEvents[i]));
            }

        }

        private void constructAntSolution()
        {
            for (int iAnt = 0; iAnt < _nOfAnts; iAnt++)
            {
                _doneEvents[iAnt].Reset();
                ThreadPool.QueueUserWorkItem(_constructionAntSolutionOpts[iAnt].ThreadPoolCallback, iAnt);
            }
            WaitHandle.WaitAll(_doneEvents.ToArray());

        }

        private void applyLS()
        {
            for (int iAnt = 0; iAnt < _nOfAnts; iAnt++)
            {
                _doneEvents[iAnt].Reset();
                ThreadPool.QueueUserWorkItem(_applyLSOpts[iAnt].ThreadPoolCallback, iAnt);
            }
            WaitHandle.WaitAll(_doneEvents.ToArray());
        }

        private void globalUpdatePheromones()
        {
            //Evaporation
            for (int i = 0; i < _curN; i++)
            {
                _pheromones[i] *= (1 - _rho);
            }
            for (int i = 0; i < _nOfAnts; i++)
            {
                var solution = _solutions[i];
                genPheromones(solution);
            }
            var iterBestSolution = getIterBestSolution(_solutions);

            //Global best solution
            lock((_pACO.BestSolution.elements as ICollection).SyncRoot)
            {
                if (_pACO.BestSolution == null || _pACO.BestSolution.maxMinD < iterBestSolution.maxMinD)
                {
                    //deep copy
                    iterBestSolution.DeepCopyTo(_pACO.BestSolution);
                }
                genGBPheromones(_pACO.BestSolution);
            }
            
        }

        private void reset()
        {
            //clear solutions
            for (int i = 0; i < _nOfAnts; i++)
            {
                _solutions[i].Reset();
            }
        }

        private void genPheromones(Solution s)
        {
            for (int j = 0; j < _curM; j++)
            {
                var e = s.elements[j];
                _pheromones[e.index] += s.maxMinD;
            }
        }

        private void genGBPheromones(Solution s)
        {
            for (int j = 0; j < _curM; j++)
            {
                var e = s.elements[j];
                _pheromones[e.index] += _gbAdditionalWeight * _nOfAnts * s.maxMinD;
            }
        }

        private Solution getIterBestSolution(List<Solution> solutions)
        {
            float maxMinD = 0;
            int indexOfSols = 0;
            for (int i = 0; i < solutions.Count; i++)
            {
                if (solutions[i].maxMinD > maxMinD)
                {
                    maxMinD = solutions[i].maxMinD;
                    indexOfSols = i;
                }
            }
            return solutions[indexOfSols];
        }
    }

    public class ConstructAntSolution
    {
        private ManualResetEvent _doneEvent;

        private ThreadACO _tACO;

        public ConstructAntSolution(int antIndex, ThreadACO tACO, ManualResetEvent doneEvent)
        {
            AntIndex = antIndex;
            _tACO = tACO;
            _doneEvent = doneEvent;
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            //int threadIndex = (int)threadContext;
            //Console.WriteLine($"Thread {threadIndex} started...");
            constructAntSolution();
            //Console.WriteLine($"Thread {threadIndex} result calculated...");
            //_doneEvent.Set();
        }
        public int AntIndex { get; set; }

        private void constructAntSolution()
        {
            //heuristicVals and probabilities init
            List<float> heuristicVals = new List<float>();
            List<float> probabilities = new List<float>();
            for (int i = 0; i < _tACO.CurN; i++)
            {
                heuristicVals.Add(int.MaxValue);
                probabilities.Add(0);
            }

            var elements = _tACO.Solutions[AntIndex].elements;
            elements.Clear();
            //randomly select first element
            int nextSelectIndex = _tACO.Rnd.Next(_tACO.CurN);
            elements.Add(new Element(nextSelectIndex));


            while (elements.Count < _tACO.CurM)
            {
                //update heuristic value
                for (int i = 0; i < _tACO.CurN; i++)
                {
                    var matriceIndex = i * _tACO.CurN + nextSelectIndex;
                    var minD = _tACO.CurMatrice[matriceIndex]; //distance
                    if (minD < heuristicVals[i])
                    {
                        heuristicVals[i] = minD;
                    }
                }

                //update probabiliy
                var denominator = 0f;
                for (int i = 0; i < _tACO.CurN; i++)
                {
                    probabilities[i] = (float)(Math.Pow(_tACO.Pheromones[i], _tACO.Alpha) * Math.Pow(heuristicVals[i], _tACO.Belta)); //store numerator temporarily  
                    denominator += probabilities[i];
                }
                for (int i = 0; i < _tACO.CurN; i++)
                {
                    probabilities[i] /= denominator;
                }

                nextSelectIndex = randomByProbability(probabilities, _tACO.Rnd);

                //keep order
                insertIndexToSolOrderByIndex(elements, nextSelectIndex);

                //Debug assert
                //System.Diagnostics.Debug.Assert(testNoEqualIndex(elements));

            }
            _doneEvent.Set();
        }

        //private bool testNoEqualIndex(List<Element> elements)
        //{
        //    if (elements.Count < 2)
        //    {
        //        return true;
        //    }
        //    bool flag = true;
        //    for (int i = 0; i < elements.Count - 1; i++)
        //    {
        //        if (elements[i].index == elements[i + 1].index)
        //        {
        //            flag = false;
        //            break;
        //        }
        //    }
        //    return flag;
        //}

        private void insertIndexToSolOrderByIndex(List<Element> elements, int nextSelectIndex)
        {
            int insertPos = 0;
            for (insertPos = 0; insertPos < elements.Count; insertPos++)
            {
                if (nextSelectIndex < elements[insertPos].index)
                {
                    break;
                }
            }
            elements.Insert(insertPos, new Element(nextSelectIndex));
        }

        private int randomByProbability(List<float> probabilities, Random rnd)
        {
            var p = rnd.NextDouble();
            float temp = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                if (probabilities[i] > 0)
                {
                    temp += probabilities[i];
                    if (p < temp)
                    {
                        return i;
                    }
                }
            }

            // select last nonzero one if p is greater than or equal to temp
            var index = 0;
            for (int i = probabilities.Count - 1; i >= 0; i--)
            {
                if (probabilities[i] > 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }

    public class ApplyLS
    {
        private ManualResetEvent _doneEvent;

        private ThreadACO _tACO;

        public ApplyLS(int antIndex, ThreadACO tACO, ManualResetEvent doneEvent)
        {
            AntIndex = antIndex;
            _tACO = tACO;
            _doneEvent = doneEvent;
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            //int threadIndex = (int)threadContext;
            //Console.WriteLine($"Thread {threadIndex} started...");
            applyLS();
            //Console.WriteLine($"Thread {threadIndex} result calculated...");
            //_doneEvent.Set();
        }
        public int AntIndex { get; set; }

        private void applyLS()
        {
            var solution = _tACO.Solutions[AntIndex];
            //unselected list
            List<int> tempUnselectedList = new List<int>();
            fillUnselectedList(tempUnselectedList, solution, _tACO.CurN);

            int toReplaceEleIndex = -1;

            do
            {
                //cal minD for every element in solution
                for (int i = 0; i < solution.elements.Count; i++)
                {
                    float tempMinDij = int.MaxValue;
                    for (int j = 0; j < solution.elements.Count; j++)
                    {
                        if (i != j)
                        {
                            var iIndex = solution.elements[i].index;
                            var jIndex = solution.elements[j].index;
                            var dij = _tACO.CurMatrice[iIndex * _tACO.CurN + jIndex];
                            if (dij < tempMinDij)
                            {
                                tempMinDij = dij;
                            }
                        }
                    }
                    solution.elements[i] = new Element(solution.elements[i].index, tempMinDij);
                }

                //find the minimun
                float minD = float.MaxValue;
                int minDIndexInSol = -1;
                int rndStartPos = _tACO.Rnd.Next(solution.elements.Count);
                int endPos = (rndStartPos - 1).Mod(solution.elements.Count);
                for (int i = rndStartPos; i != endPos; i = (i + 1) % solution.elements.Count)
                {
                    if (solution.elements[i].minD < minD)
                    {
                        minD = solution.elements[i].minD;
                        minDIndexInSol = i;
                    }
                }

                // key means it has the lowest distance, it matters the maxi-min diversity of whole solution
                var keyElement = solution.elements[minDIndexInSol];
                solution.elements.RemoveAt(minDIndexInSol);

                toReplaceEleIndex = -1;
                var toReplaceElePosInUnselected = -1;
                //go through unselected list
                rndStartPos = _tACO.Rnd.Next(tempUnselectedList.Count);
                endPos = (rndStartPos - 1).Mod(tempUnselectedList.Count);
                for (int i = rndStartPos; i != endPos; i = (i + 1) % tempUnselectedList.Count)
                {
                    float tempMinD = calMinDWithAllInSol(tempUnselectedList[i], solution.elements);
                    if (tempMinD > minD)
                    {
                        toReplaceEleIndex = tempUnselectedList[i];
                        toReplaceElePosInUnselected = i;
                        break; //first improvement
                    }
                }
                if (toReplaceEleIndex != -1) //replace keyElement with certain ele in unselected list 
                {
                    //insertIndexToSolOrderByIndex(solution.elements, toReplaceEleIndex);
                    solution.elements.Add(new Element(toReplaceEleIndex, float.MaxValue));
                    tempUnselectedList.RemoveAt(toReplaceElePosInUnselected);
                    tempUnselectedList.Add(keyElement.index);
                }
                else //no improvement found
                {
                    //recover solution
                    //insertElementToSolOrderByIndex(solution.elements, keyElement);
                    solution.elements.Add(keyElement);
                    solution.maxMinD = minD;
                }
            }
            while (toReplaceEleIndex != -1);

            _doneEvent.Set();
        }

        private void fillUnselectedList(List<int> unselectedList, Solution solution, int n)
        {
            unselectedList.Clear();
            int i = 0, j = 0;
            while (i < n)
            {
                if (j >= solution.elements.Count)
                {
                    unselectedList.Add(i);
                }
                else
                {
                    if (i < solution.elements[j].index)
                    {
                        unselectedList.Add(i);
                    }
                    else if (i == solution.elements[j].index)
                    {
                        j++;
                    }
                }
                i++;

            }
        }

        private float calMinDWithAllInSol(int index, List<Element> solution)
        {
            var minD = float.MaxValue;
            for (int i = 0; i < solution.Count; i++)
            {
                var indexInMatrice = index * _tACO.CurN + solution[i].index;
                var tempD = _tACO.CurMatrice[indexInMatrice];
                if (tempD < minD)
                {
                    minD = tempD;
                }
            }
            return minD;

        }
    }
}
