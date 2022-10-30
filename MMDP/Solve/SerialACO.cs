using System;
using System.Collections.Generic;
using System.Text;
using MMDP.Util;

namespace MMDP.Solve
{
    class SerialACO
    {
        private int _nOfAnts = 10;
        private float _alpha = 0.5f;
        private float _belta = 1f;
        private float _rho = 0.5f;
        //private float _gbAdditionalWeight = 0.5f;

        public List<List<float>> matriceList;
        private List<float> _curMatrice;

        //select a subset M of m elements (|M|=m) from a set N of n elements
        private int _curN;
        private int _curM;
        private List<float> _pheromones = new List<float>();
        private List<float> _heuristicVals = new List<float>();
        private Random _rnd;
        private List<Solution> _solutions = new List<Solution>();
        private List<float> _probabilities = new List<float>();
        private Solution _bestSolution = new Solution();

        public SerialACO(List<List<float>> matriceList)
        {
            this.matriceList = matriceList;
            _rnd = new Random();
        }

        public Solution ACO(int instanceIndex, float mRatio, float maxIter, int nAnts)
        {
            _curMatrice = matriceList[instanceIndex];
            _curN = (int) Math.Sqrt(_curMatrice.Count);
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
            return _bestSolution;
        }

        

        private void init()
        {
            _bestSolution.Reset();
            //init pheromones
            //init probability
            _pheromones.Clear();
            _probabilities.Clear();
            for (int i = 0; i < _curN; i++)
            {
                _pheromones.Add(1f);
                _probabilities.Add(0);
            }

            //init solutions for ants
            _solutions.Clear();
            for (int i = 0; i < _nOfAnts; i++)
            {
                _solutions.Add(new Solution());
            }

            _heuristicVals.Clear();
            for (int i = 0; i < _curN; i++)
            {
                _heuristicVals.Add(int.MaxValue);
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

        private void constructAntSolution()
        {
            for (int iAnt = 0; iAnt < _nOfAnts; iAnt++)
            {
                //reset heuristic value
                for (int i = 0; i < _curN; i++)
                {
                    _heuristicVals[i] = int.MaxValue;
                }
                
                var elements = _solutions[iAnt].elements;
                elements.Clear();
                //randomly select first element
                int nextSelectIndex = _rnd.Next(_curN);
                elements.Add(new Element(nextSelectIndex));


                while (elements.Count < _curM)
                {
                    //update heuristic value
                    for (int i = 0; i < _curN; i++)
                    {
                        var matriceIndex = i * _curN + nextSelectIndex;
                        var minD = _curMatrice[matriceIndex]; //distance
                        if (minD < _heuristicVals[i])
                        {
                            _heuristicVals[i] = minD;
                        }
                    }

                    //update probabiliy
                    var denominator = 0f;
                    for (int i = 0; i < _curN; i++)
                    {
                        _probabilities[i] = (float)(Math.Pow(_pheromones[i], _alpha) * Math.Pow(_heuristicVals[i], _belta)); //store numerator temporarily  
                        denominator += _probabilities[i];
                    }
                    for (int i = 0; i < _curN; i++)
                    {
                        _probabilities[i] /= denominator;
                    }

                    nextSelectIndex = randomByProbability(_probabilities, _rnd);

                    //keep order
                    insertIndexToSolOrderByIndex(elements, nextSelectIndex);
                    
                    //Debug assert
                    //System.Diagnostics.Debug.Assert(testNoEqualIndex(elements));
                }
            }
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
        //        if (elements[i].index == elements[i+1].index)
        //        {
        //            flag = false;
        //            break;
        //        }
        //    }
        //    return flag;
        //}
        

        //store unselected index of element temporarily
        private List<int> tempUnselectedList = new List<int>();
        private void applyLS()
        {
            for (int iAnt = 0; iAnt < _nOfAnts; iAnt++)
            {
                var solution = _solutions[iAnt];
                //unselected list
                fillUnselectedList(tempUnselectedList, solution, _curN);

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
                                var dij = _curMatrice[iIndex * _curN + jIndex];
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
                    int rndStartPos = _rnd.Next(solution.elements.Count);
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
                    rndStartPos = _rnd.Next(tempUnselectedList.Count);
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
            }

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
            if (_bestSolution == null || _bestSolution.maxMinD < iterBestSolution.maxMinD)
            {
                //deep copy
                iterBestSolution.DeepCopyTo(_bestSolution);
            }
            genPheromones(_bestSolution);
        }

        private void genPheromones(Solution s)
        {
            for (int j = 0; j < _curM; j++)
            {
                var e = s.elements[j];
                _pheromones[e.index] += s.maxMinD;
            }
        }

        //private void genGBPheromones(Solution s)
        //{
        //    for (int j = 0; j < _curM; j++)
        //    {
        //        var e = s.elements[j];
        //        _pheromones[e.index] += _gbAdditionalWeight * _nOfAnts * s.maxMinD;
        //    }
        //}

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

        private void insertElementToSolOrderByIndex(List<Element> elements, Element e)
        {
            int insertPos = 0;
            for (insertPos = 0; insertPos < elements.Count; insertPos++)
            {
                if (e.index < elements[insertPos].index)
                {
                    break;
                }
            }
            elements.Insert(insertPos, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unselectedList"></param>
        /// <param name="solution">solution should be ordered by index</param>
        /// <param name="n"></param>
        protected void fillUnselectedList(List<int> unselectedList, Solution solution, int n)
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
        protected float calMinDWithAllInSol(int index, List<Element> solution)
        {
            var minD = float.MaxValue;
            for (int i = 0; i < solution.Count; i++)
            {
                var indexInMatrice = index * _curN + solution[i].index;
                var tempD = _curMatrice[indexInMatrice];
                if (tempD < minD)
                {
                    minD = tempD;
                }
            }
            return minD;

        }

        protected int randomByProbability(List<float> probabilities, Random rnd)
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

    public class Solution
    {
        public List<Element> elements;
        public float maxMinD;

        public Solution()
        {
            elements = new List<Element>();
            maxMinD = 0;
        }

        public void Reset()
        {
            elements.Clear();
            maxMinD = 0;
        }

        public void DeepCopyTo(Solution other)
        {
            other.maxMinD = maxMinD;
            other.elements.Clear();
            elements.ForEach(e => other.elements.Add(e));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MaxMinD: ");
            sb.Append(maxMinD);
            sb.Append(" \ne: ");
            elements.ForEach(e =>
            {
                sb.Append(e.index);
                sb.Append(" ");
            });
            return sb.ToString();
        }
    }

    public struct Element
    {
        /// <summary>
        /// index of points
        /// </summary>
        public int index;

        /// <summary>
        /// distance with closest element in solution
        /// </summary>
        public float minD;

        /// <summary>
        /// k lowest distance values between i and the m elements in the solution
        /// </summary>
        //public int kMinima;

        public Element(int index)
        {
            this.index = index;
            minD = float.MaxValue;
            //kMinima = -1;
        }

        public Element(int index, float minD)
        {
            this.index = index;
            this.minD = minD;
        }

        public override string ToString()
        {
            return index + " ; " + minD;
        }
    }
}
