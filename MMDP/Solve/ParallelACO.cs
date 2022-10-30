using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MMDP.Solve
{
    public class ParallelACO
    {
        public int nOfCores = 4;
        public List<List<float>> matriceList;
        public List<Task> taskList;

        private delegate void ACODelegate(ACOParams parameters);

        public Solution BestSolution { get; set; } = new Solution();

        public ParallelACO(List<List<float>> matriceList)
        {
            this.matriceList = matriceList;
            ThreadPool.SetMaxThreads(nOfCores, nOfCores);
        }

        internal Solution ACO(int instanceIndex, float mRatio, int maxIter, int nAnts, int nOfColonies)
        {
            taskList = new List<Task>();
            BestSolution.Reset();
            for (int i = 0; i < nOfColonies; i++)
            {
                ThreadACO tACO = new ThreadACO(matriceList, this);
                ACODelegate acoDelegate = new ACODelegate(tACO.ACO);
                Task task = Task.Run(() => acoDelegate(new ACOParams(instanceIndex, mRatio, maxIter, nAnts)));
                taskList.Add(task);
            }

            //Wait all
            Task.WaitAll(taskList.ToArray());

            return BestSolution;
        }
    }

    public struct ACOParams
    {
        public int instanceIndex;
        public float mRatio;
        public int maxIter;
        public int nAnts;

        public ACOParams(int instanceIndex, float mRatio, int maxIter, int nAnts)
        {
            this.instanceIndex = instanceIndex;
            this.mRatio = mRatio;
            this.maxIter = maxIter;
            this.nAnts = nAnts;
        }
    }
}
