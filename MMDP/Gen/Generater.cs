using System;
using System.Collections.Generic;
using System.Text;

namespace MMDP.Gen
{
    class Generater
    {
        public List<List<Point>> datasetList;
        public List<List<float>> matriceList;

        private int _n;
        private int _nOfInstances;
        private Random _rnd;
        private bool intValFlag = false;

        public int N
        {
            get
            {
                return _n;
            }
        }

        public Generater(int n, int nOfInstances, bool intValFlag = false)
        {
            this._n = n;
            this._nOfInstances = nOfInstances;
            this.intValFlag = intValFlag;
        }

        public void Gen()
        {
            genDatasets();
            calMatrices();
        }

        private void calMatrices()
        {
            matriceList = new List<List<float>>();
            foreach (var dataset in datasetList)
            {
                matriceList.Add(calMatrice(dataset));
            }
        }

        private List<float> calMatrice(List<Point> dataset)
        {
            List<float> matrice = new List<float>();
            for (int row = 0; row < _n; row++)
            {
                for (int col = 0; col < _n; col++)
                {
                    //var index = row * n + col;
                    matrice.Add(calDistance(dataset, matrice, row, col));
                }
            }
            return matrice;
        }

        private float calDistance(List<Point> dataset, List<float> matrice, int row, int col)
        {
            float temp = 0;
            if (row == col)
            {
                temp = 0;
            }
            else if (row > col) //already calculated
            {
                var correspondingIndex = col * _n + row;
                temp = matrice[correspondingIndex];
            }
            else
            {
                var point1 = dataset[row];
                var point2 = dataset[col];
                for (int i = 0; i < point1.coordinates.Count; i++)
                {
                    temp += (float)Math.Pow(point1.coordinates[i] - point2.coordinates[i], 2);
                }
                temp = (float)Math.Sqrt(temp);
            }
            return temp;
        }

        private void genDatasets()
        {
            _rnd = new Random();
            datasetList = new List<List<Point>>();
            for (int i = 0; i < _nOfInstances; i++)
            {
                 datasetList.Add(genDataset());
            }
        }

        /// <summary>
        /// coordinates in 0–100 range, number of coordinates between 2 and 21
        /// </summary>
        private List<Point> genDataset()
        {
            int nOfCoordinates = _rnd.Next(20) + 2;
            List<Point> dataset = new List<Point>();
            for (int i = 0; i < _n; i++)
            {
                var point = new Point();
                for (int j = 0; j < nOfCoordinates; j++)
                {
                    if (intValFlag)
                    {
                        point.coordinates.Add(_rnd.Next(100));
                    }
                    else
                    {
                        point.coordinates.Add((float)_rnd.NextDouble() * 100);
                    }
                }
                dataset.Add(point);
            }
            return dataset;
        }
    }

    public class Point
    {
        public List<float> coordinates;

        public Point()
        {
            coordinates = new List<float>();
        }
    }
}
