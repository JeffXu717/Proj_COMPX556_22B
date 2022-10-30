using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MMDP.Solve
{
    class Solver
    {
        public List<List<float>> matriceList;
        public SerialACO sACO;
        public ParallelACO pACO;

        public Solver()
        {
            matriceList = new List<List<float>>();
        }

        public SolAndElapse RunSerialACOForFirstInstacne(float mRatio, int maxIter, int nAnts)
        {
            return RunSerialACOForOneInstacne(0, mRatio, maxIter, nAnts);
        }

        public SolAndElapse RunSerialACOForOneInstacne(int instanceIndex, float mRatio, int maxIter, int nAnts)
        {
            if (sACO == null)
            {
                sACO = new SerialACO(matriceList);
            }

            Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Solution solution = sACO.ACO(instanceIndex, mRatio, maxIter, nAnts);
            stopwatch.Stop();
            float elapse = (float)stopwatch.Elapsed.TotalSeconds;

            return new SolAndElapse(solution, elapse);
        }

        public SolsAndStatistics RunSerialACOForFirstInstacneNTimes(float mRatio, int maxIter, int nAnts, int nTimes)
        {
            return RunSerialACOForOneInstacneNTimes(0, mRatio, maxIter, nAnts, nTimes);
        }

        public StringBuilder RunSerialACOForInstacnesNTimes(float mRatio, int maxIter, int nAnts, int nTimes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < matriceList.Count; i++)
            {
                var sols = RunSerialACOForOneInstacneNTimes(i, mRatio, maxIter, nAnts, nTimes);
                sb.Append(sols.ToString());
                sb.AppendLine();
            }
            return sb;
        }

        public SolsAndStatistics RunSerialACOForOneInstacneNTimes(int instanceIndex, float mRatio, int maxIter, int nAnts, int nTimes)
        {
            if (sACO == null)
            {
                sACO = new SerialACO(matriceList);
            }

            var n = (int)Math.Sqrt(matriceList[0].Count);
            SolsAndStatistics solsAndStatistics = new SolsAndStatistics(instanceIndex, n, mRatio);
            Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            for (int i = 0; i < nTimes; i++)
            {
                stopwatch.Restart();
                Solution solution = sACO.ACO(instanceIndex, mRatio, maxIter, nAnts);
                stopwatch.Stop();
                float elapse = (float)stopwatch.Elapsed.TotalSeconds;
                solsAndStatistics.solsAndElapse.Add(new SolAndElapse(solution, elapse));
            }
            solsAndStatistics.CalStatistics();

            return solsAndStatistics;
        }

        public SolAndElapse RunParallelACOForFirstInstance(float mRatio, int maxIter, int nAnts, int nOfColonies)
        {
            return RunParallelACOForOneInstance(0, mRatio, maxIter, nAnts, nOfColonies);
        }

        public SolAndElapse RunParallelACOForOneInstance(int instanceIndex, float mRatio, int maxIter, int nAnts, int nOfColonies)
        {
            if (pACO == null)
            {
                pACO = new ParallelACO(matriceList);
            }

            Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Solution solution = pACO.ACO(instanceIndex, mRatio, maxIter, nAnts, nOfColonies);
            stopwatch.Stop();
            float elapse = (float)stopwatch.Elapsed.TotalSeconds;

            return new SolAndElapse(solution, elapse);
        }

        public SolsAndStatistics RunParallelACOForFirstInstacneNTimes(float mRatio, int maxIter, int nAnts, int nColonies, int nTimes)
        {
            return RunParallelACOForOneInstacneNTimes(0, mRatio, maxIter, nAnts, nColonies, nTimes);
        }

        public StringBuilder RunParallelACOForInstacnesNTimes(float mRatio, int maxIter, int nAnts, int nOfColonies, int nTimes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < matriceList.Count; i++)
            {
                var sols = RunParallelACOForOneInstacneNTimes(i, mRatio, maxIter, nAnts, nOfColonies, nTimes);
                sb.Append(sols.ToString());
                sb.AppendLine();
            }
            return sb;
        }

        public SolsAndStatistics RunParallelACOForOneInstacneNTimes(int instanceIndex, float mRatio, int maxIter, int nAnts, int nColonies, int nTimes)
        {
            if (pACO == null)
            {
                pACO = new ParallelACO(matriceList);
            }

            var n = (int)Math.Sqrt(matriceList[0].Count);
            SolsAndStatistics solsAndStatistics = new SolsAndStatistics(instanceIndex, n, mRatio);
            Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            for (int i = 0; i < nTimes; i++)
            {
                stopwatch.Restart();
                Solution solution = pACO.ACO(instanceIndex, mRatio, maxIter, nAnts, nColonies);
                stopwatch.Stop();
                float elapse = (float)stopwatch.Elapsed.TotalSeconds;
                solsAndStatistics.solsAndElapse.Add(new SolAndElapse(solution, elapse));
            }
            solsAndStatistics.CalStatistics();

            return solsAndStatistics;
        }
    }

    /// <summary>
    /// One solution and elapse
    /// </summary>
    public struct SolAndElapse
    {
        public Solution solution;
        public float elapse;

        public SolAndElapse(Solution solution, float elapse)
        {
            this.solution = new Solution();
            solution.DeepCopyTo(this.solution);
            this.elapse = elapse;
        }

        public override string ToString()
        {
            return solution.ToString() + "\nElapse: " + elapse.ToString("0.00");
        }
    }

    /// <summary>
    /// solutions of one instance for many times
    /// </summary>
    public class SolsAndStatistics
    {
        public List<SolAndElapse> solsAndElapse;
        public int indexOfBest;
        public int indexOfWorst;
        public float averageObjVal;
        public float averageElapse;
        public int n;
        public float mRatio;
        public int instanceIndex;

        public SolsAndStatistics(int instanceIndex, int n, float mRatio)
        {
            solsAndElapse = new List<SolAndElapse>();
            this.instanceIndex = instanceIndex;
            this.n = n;
            this.mRatio = mRatio;
        }

        public void CalStatistics()
        {
            float bestObjVal = 0;
            float worstObjVal = float.MaxValue;
            float sumObjVal = 0;
            float sumElapse = 0;
            
            for (int i = 0; i < solsAndElapse.Count; i++)
            {
                var solAndElapse = solsAndElapse[i];
                sumElapse += solAndElapse.elapse;
                sumObjVal += solAndElapse.solution.maxMinD;
                if (solAndElapse.solution.maxMinD > bestObjVal)
                {
                    bestObjVal = solAndElapse.solution.maxMinD;
                    indexOfBest = i;
                }
                if (solAndElapse.solution.maxMinD < worstObjVal)
                {
                    worstObjVal = solAndElapse.solution.maxMinD;
                    indexOfWorst = i;
                }
            }
            averageElapse = sumElapse / solsAndElapse.Count;
            averageObjVal = sumObjVal / solsAndElapse.Count;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(instanceIndex);
            sb.Append(" n = ");
            sb.Append(n);
            sb.Append(" m = ");
            sb.Append(mRatio.ToString("0.0"));
            sb.Append("n\n");
            sb.Append("AverObjVal: ");
            sb.Append(averageObjVal);
            sb.Append(" AverElapse: ");
            sb.Append(averageElapse);
            sb.Append("\n");
            sb.AppendLine("Best case:");
            sb.AppendLine(solsAndElapse[indexOfBest].ToString());
            sb.AppendLine("Worst case:");
            sb.AppendLine(solsAndElapse[indexOfWorst].ToString());
            foreach (var solAndElapse in solsAndElapse)
            {
                sb.Append(solAndElapse.solution.maxMinD.ToString("0.00"));
                sb.Append("|");
                sb.Append(solAndElapse.elapse.ToString("0.00"));
                sb.Append(" ");
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }

    //public class AllSolStatistics
    //{
    //    public List<SolsAndStatistics> solsAndStatistics;

    //    public AllSolStatistics()
    //    {
    //        solsAndStatistics = new List<SolsAndStatistics>();
    //    }

    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        foreach (var solAndStatistics in solsAndStatistics)
    //        {
    //            sb.Append(solAndStatistics.ToString());
    //            sb.AppendLine();
    //        }
    //        return sb.ToString();
    //    }
    //}
}
