using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApproximationAlgorithm
{
    public class Program
    {
        public static string filepath = "C:/Users/werka/Desktop/Algorithms/examples_densities/";
        public static string importFilename1 = "example_12_0.3_A.csv";
        public static string importFilename2 = "example_20_0.3_B.csv";
        public static string exportFilename1 = "results/result_graph1.csv";
        public static string exportFilename2 = "results/result_graph2.csv";
        public static int size1;
        public static int[,] adjacencyMatrix1;
        public static Graph G1;
        public static int size2;
        public static int[,] adjacencyMatrix2;
        public static Graph G2;
        public static Vertex maxDegG1;
        public static Vertex maxDegG2;
        public static List<Vertex> subgraphVertices1;
        public static List<Vertex> subgraphVertices2;
        public static int[,] subgraphAdjacencyMatrix1;
        public static int[,] subgraphAdjacencyMatrix2;
        public static int maxDegree; //for both graphs
        public static int currentDegree; //degree with which neighbours are considered at the moment
        public static List<Vertex> currentNeighbours1 = new List<Vertex>(); //currently considered neighbours
        public static List<Vertex> currentNeighbours2 = new List<Vertex>(); //currently considered neighbours
        public static int[,] cyclesMatrix1;
        public static int[,] cyclesMatrix2;
        public static Vertex head1;
        public static Vertex head2;
        public static List<List<Vertex>> pathsFromVertexWithMaxDegreeList1 = new List<List<Vertex>>();
        public static List<List<Vertex>> pathsFromVertexWithMaxDegreeList2 = new List<List<Vertex>>();
        public static int iterations;
        public static List<Vertex> maxPath1 = new List<Vertex>();
        public static List<Vertex> maxPath2 = new List<Vertex>();

        public static void Main(string[] args)
        {
            graphsInititalization();
            maxDegree = 0;

            maxDegG1 = G1.findVertexWithMaxDegree();
            maxDegG2 = G2.findVertexWithMaxDegree();

            //Console.WriteLine("Max degree in G1: id:" + maxDegG1.id + ", deg:" + maxDegG1.degree);
            //Console.WriteLine("Max degree in G2: id:" + maxDegG2.id + ", deg:" + maxDegG2.degree);

            DateTime begining = DateTime.Now;
            maxDegree = findMaxDegreeForBothGraphs(G1, G2);
            Console.WriteLine("Max degree in both: " + maxDegree);

            currentDegree = maxDegree;
            iterations = maxDegree;

            if(iterations == 0)
            {
                maxPath1.Add(G1.degreesList.First());
                maxPath2.Add(G2.degreesList.First());
            }
            else
            {
                while (iterations > 0)
                {
                    iterations--;
                    algorithm(maxDegree, iterations);
                }

                var paths = getMaxSubgraph();

                maxPath1 = paths.Item1;
                maxPath2 = paths.Item2;
            }

            DateTime finish = DateTime.Now;
            Console.WriteLine("Start: " + begining);
            Console.WriteLine("End: " + finish);
            Console.WriteLine("Total time: " + (finish - begining));


            Console.WriteLine("------------RESULT-------------");
            Console.WriteLine("Max subgraph in G1:");
            printListofVertices(maxPath1);
            Console.WriteLine("Max subgraph in G2:");
            printListofVertices(maxPath2);

            int[,] subAdjMatrix1 = new int[G1.numberOfVertices,G1.numberOfVertices];
            int[,] subAdjMatrix2 = new int[G2.numberOfVertices, G2.numberOfVertices];
            if (maxPath1.Count > 0 && maxPath1.First() != null)
                subAdjMatrix1 = createAdjacencyMatrixForSubgraph(G1.adjacencyMatrix, maxPath1);
            if (maxPath2.Count > 0 && maxPath2.First() != null)
                subAdjMatrix2 = createAdjacencyMatrixForSubgraph(G2.adjacencyMatrix, maxPath2);
            Console.WriteLine("Max subgraph matrix in G1:");
            printMatrix(subAdjMatrix1, subAdjMatrix1.GetLength(0), subAdjMatrix1.GetLength(1));
            Console.WriteLine("Max subgraph matrix in G2:");
            printMatrix(subAdjMatrix2, subAdjMatrix2.GetLength(0), subAdjMatrix2.GetLength(1));

            saveMatrixToCSV(subAdjMatrix1, filepath + exportFilename1);
            saveMatrixToCSV(subAdjMatrix2, filepath + exportFilename2);

            Console.ReadKey();
        }

        public static Tuple<List<Vertex>, List<Vertex>> getMaxSubgraph()
        {
            List<Vertex> path1 = new List<Vertex>();
            List<Vertex> path2 = new List<Vertex>();
            int maxId = 0;
            int maxDeg = 0;

            foreach(var list in pathsFromVertexWithMaxDegreeList1)
            {
                if (list.Count() > maxDeg)
                {
                    maxId = pathsFromVertexWithMaxDegreeList1.IndexOf(list);
                    maxDeg = list.Count();
                }
            }
            if(pathsFromVertexWithMaxDegreeList1.Count() != 0)
            {
                path1 = pathsFromVertexWithMaxDegreeList1.ElementAt(maxId);
                path2 = pathsFromVertexWithMaxDegreeList2.ElementAt(maxId);
            }

            return Tuple.Create(path1, path2);
        }

        public static int[,] createAdjacencyMatrixForSubgraph(int[,] graph, List<Vertex> subgraph)
        {
            int[,] matrix = new int[graph.GetLength(0), graph.GetLength(1)];

            for (int i = 0; i < graph.GetLength(0); i++)
            {
                for (int j = 0; j < graph.GetLength(1); j++)
                {
                    if (subgraph.Exists(v => v.id == i) && subgraph.Exists(v => v.id == j))
                    {
                        matrix[i, j] = graph[i, j];
                    }
                    else
                    {
                        matrix[i, j] = 0;
                    }
                }
            }

            return matrix;
        }

        private static void saveMatrixToCSV(int[,] matrix, string path)
        {
            var enumerator = matrix.Cast<int>()
                            .Select((s, i) => (i + 1) % matrix.GetLength(0) == 0 ? string.Concat(s, Environment.NewLine) : string.Concat(s, ","));

            var item = String.Join("", enumerator.ToArray<string>());
            File.WriteAllText(path, item);
        }

        private static int[,] getMultidimFromJaggedArray(int[][] jaggedArray)
        {
            int size = jaggedArray[0].Length;
            int[,] multidimArray = new int[size, size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    multidimArray[y, x] = jaggedArray[y][x];
                }
            }
            return (multidimArray);
        }

        private static Boolean validateInputData(int[][] matrix)
        {
            int lastRowLen = -1;
            int rowCount = 0;
            foreach (int[] row in matrix)
            {
                if (lastRowLen != -1 && row.Length != lastRowLen)
                {
                    throw new Exception(String.Format("Uneven row lengths at row {0}!", rowCount.ToString()));
                }

                lastRowLen = row.Length;

                if (rowCount >= lastRowLen)
                {
                    throw new Exception(String.Format("Row count exceeds array width at row {0}!", (rowCount).ToString()));
                }

                foreach (int cell in row)
                {
                    if (cell != 1 && cell != 0)
                    {
                        throw new Exception(String.Format("Values different than 0 or 1 in array at row {0}!", rowCount.ToString()));
                    }
                }
                rowCount++;
            }

            return (true);
        }

        private static int[,] getGraphFromCsv(string filePath = "./test_graph.csv")
        {
            StreamReader sr = new StreamReader(filePath);
            string[] stringSeparators = new string[] { ",", ";" };
            var lines = new List<int[]>();
            int row = 0;
            while (!sr.EndOfStream)
            {
                string[] lineStr = sr.ReadLine().Split(stringSeparators, StringSplitOptions.None);

                int[] lineInt = null;
                try
                {
                    lineInt = Array.ConvertAll<string, int>(lineStr, s => Int32.Parse(s));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Bad input data at row {0}. Parsing to int error.", row.ToString()));
                }
                lines.Add(lineInt);
                row++;
            }

            var data = lines.ToArray();
            Boolean inputValid = validateInputData(data);

            return (getMultidimFromJaggedArray(data));
        }

        public static void graphsInititalization()
        {
            adjacencyMatrix1 = getGraphFromCsv(filepath + importFilename1);
            adjacencyMatrix2 = getGraphFromCsv(filepath + importFilename2);
            G1 = new Graph(adjacencyMatrix1.GetLength(0), adjacencyMatrix1);
            G2 = new Graph(adjacencyMatrix2.GetLength(0), adjacencyMatrix2);
            subgraphVertices1 = new List<Vertex>();
            subgraphVertices2 = new List<Vertex>();

            subgraphAdjacencyMatrix1 = new int[adjacencyMatrix1.GetLength(0), adjacencyMatrix1.GetLength(0)];
            subgraphAdjacencyMatrix2 = new int[adjacencyMatrix2.GetLength(0), adjacencyMatrix2.GetLength(0)];
        }

        public static int findMaxDegreeForBothGraphs(Graph _g1, Graph _g2)
        {
            int maxDeg = 0;
            Vertex max1 = _g1.findVertexWithMaxDegree();
            Vertex max2 = _g2.findVertexWithMaxDegree();
            int i = 0;
            if (max1.degree == max2.degree)
            {
                maxDeg = max1.degree;
            }
            else
            {
                if (max1.degree > max2.degree)
                {
                    while (_g1.degreesList.ElementAt(i).degree > max2.degree && i < _g1.numberOfVertices - 1 && i < _g2.numberOfVertices - 1)
                    {
                        i++;
                    }
                    maxDeg = _g1.degreesList.ElementAt(i).degree;
                }
                else
                {
                    while (_g1.degreesList.ElementAt(i).degree < max2.degree && i < _g1.numberOfVertices - 1 && i < _g2.numberOfVertices - 1)
                    {
                        i++;
                    }
                    maxDeg = _g1.degreesList.ElementAt(i).degree;
                }
            }
            return maxDeg;
        }

        public static void printListofVertices(List<Vertex> list)
        {
            foreach (Vertex v in list)
            {
                if(v != null)
                    Console.WriteLine("id: " + v.id + " deg: " + v.degree);
            }
        }

        public static void printMatrix(int[,] m, int d1, int d2)
        {
            for (int i = 0; i < d1; i++)
            {
                for (int j = 0; j < d2; j++)
                {
                    Console.Write(m[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        public static int findMaxCurrentDegreeForNeighbours(Graph g1, Graph g2, Vertex v1, Vertex v2, int degree)
        {
            int currentDeg = degree;

            List<Vertex> nb1 = g1.findNeighbours(v1, currentDeg, head1.id);
            List<Vertex> nb2 = g2.findNeighbours(v2, currentDeg, head2.id);

            while ((nb1.Count == 0 || nb2.Count == 0) && currentDeg != 0)
            {
                currentDeg--;
                nb1 = g1.findNeighbours(v1, currentDeg, head1.id);
                nb2 = g2.findNeighbours(v2, currentDeg, head2.id);
            }

            currentNeighbours1 = nb1;
            currentNeighbours2 = nb2;

            return currentDeg;
        }

        public static int[,] createCyclesMatrix(Graph g, List<Vertex> subgraphVertices, List<Vertex> neighbours)
        {
            int[,] cycles = new int[neighbours.Count(), subgraphVertices.Count() - 1];
            for (int i = 0; i < neighbours.Count(); i++)
            {
                for (int j = 0; j < subgraphVertices.Count() - 1; j++)
                {
                    cycles[i, j] = 0;
                    if (g.adjacencyMatrix[neighbours.ElementAt(i).id, subgraphVertices.ElementAt(j).id] == 1)
                        cycles[i, j] = 1;
                }
            }

            return cycles;
        }

        public static bool findVerticesWithTheSameNumberOfCycles(List<Vertex> sub1, List<Vertex> sub2, List<Vertex> cnb1, List<Vertex> cnb2, int[,] cycles1, int[,] cycles2)
        {
            bool found = false;
            int limitI1 = sub1.Count;
            int limitI2 = sub2.Count;
            bool row = true;

            for (int i = 0; i < ((cnb1.Count > cnb2.Count) ? cnb2.Count : cnb1.Count); i++)
            {
                row = true;
                for (int j = 0; j < ((limitI1 > limitI2) ? limitI2 : limitI1) - 1; j++)
                {
                    if (cycles1[i, j] != cycles2[i, j])
                    {
                        row = false;
                    }
                }
                if (row == true)
                {
                    sub1.Add(cnb1.ElementAt(i));
                    sub2.Add(cnb2.ElementAt(i));
                    found = true;
                    break;
                }
            }

            return found;
        }

        public static void algorithm(int maxdeg, int currentdeg)
        {
            head1 = G1.degreesList.Find(vertex => vertex.degree == maxdeg);
            subgraphVertices1.Add(head1);
            head2 = G2.degreesList.Find(vertex => vertex.degree == maxdeg);
            subgraphVertices2.Add(head2);

            //Console.WriteLine("Subgraph vertices 1:");
            //printListofVertices(subgraphVertices1);
            //Console.WriteLine("Subgraph vertices 2:");
            //printListofVertices(subgraphVertices2);

            while (currentdeg > 0)
            {
                if (subgraphVertices1.Count() < 2)
                {
                    currentdeg = findMaxCurrentDegreeForNeighbours(G1, G2, subgraphVertices1.Last(), subgraphVertices2.Last(), currentdeg);

                    if (currentdeg != 0)
                    {
                        //Console.WriteLine("Neighbours 1: deg" + currentdeg);
                        //printListofVertices(currentNeighbours1);
                        //Console.WriteLine("Neighbours 2: deg" + currentdeg);
                        //printListofVertices(currentNeighbours2);

                        subgraphVertices1.Add(currentNeighbours1.ElementAt(0));
                        subgraphVertices2.Add(currentNeighbours2.ElementAt(0));

                        //Console.WriteLine("Subgraph vertices 1:");
                        //printListofVertices(subgraphVertices1);
                        //Console.WriteLine("Subgraph vertices 2:");
                        //printListofVertices(subgraphVertices2);
                    }
                }
                else
                {
                    currentdeg = findMaxCurrentDegreeForNeighbours(G1, G2, subgraphVertices1.Last(), subgraphVertices2.Last(), currentdeg);

                    if (currentdeg != 0)
                    {
                        //Console.WriteLine("Neighbours 1: deg" + currentdeg);
                        //printListofVertices(currentNeighbours1);
                        //Console.WriteLine("Neighbours 2: deg" + currentdeg);
                        //printListofVertices(currentNeighbours2);

                        cyclesMatrix1 = createCyclesMatrix(G1, subgraphVertices1, currentNeighbours1);
                        //Console.WriteLine("Cycles 1:");
                        //printMatrix(cyclesMatrix1, currentNeighbours1.Count(), subgraphVertices1.Count - 1);
                        cyclesMatrix2 = createCyclesMatrix(G2, subgraphVertices2, currentNeighbours2);
                        //Console.WriteLine("Cycles 2:");
                        //printMatrix(cyclesMatrix2, currentNeighbours2.Count(), subgraphVertices2.Count - 1);

                        //Console.WriteLine("Subgraph vertices 1:");
                        //printListofVertices(subgraphVertices1);
                        //Console.WriteLine("Subgraph vertices 2:");
                        //printListofVertices(subgraphVertices2);

                        bool found = findVerticesWithTheSameNumberOfCycles(subgraphVertices1, subgraphVertices2, currentNeighbours1, currentNeighbours2, cyclesMatrix1, cyclesMatrix2);

                        if (found == false)
                        {
                            currentdeg--;
                            //currentDegree = currentdeg;
                            //Console.WriteLine("Cycles fit - not found");
                        }

                        //Console.WriteLine("Subgraph vertices 1:");
                        //printListofVertices(subgraphVertices1);
                        //Console.WriteLine("Subgraph vertices 2:");
                        //printListofVertices(subgraphVertices2);
                    }
                }
            }

            pathsFromVertexWithMaxDegreeList1.Add(copyOfSubgraph(subgraphVertices1));
            pathsFromVertexWithMaxDegreeList2.Add(copyOfSubgraph(subgraphVertices2));

            //Console.WriteLine("All paths 1:");
            //foreach (List<Vertex> l in pathsFromVertexWithMaxDegreeList1)
            //{
            //    printListofVertices(l);
            //    Console.WriteLine("------------------------");
            //}
            //Console.WriteLine("All paths 2:");
            //foreach (List<Vertex> l in pathsFromVertexWithMaxDegreeList2)
            //{
            //    printListofVertices(l);
            //    Console.WriteLine("------------------------");
            //}
            //Console.WriteLine("--------------BACKTRACKING------------");
            subgraphVertices1.Clear();
            subgraphVertices2.Clear();
            currentNeighbours1.Clear();
            currentNeighbours2.Clear();
            currentDegree = maxDegree;
        }

        public static List<Vertex> copyOfSubgraph(List<Vertex> org)
        {
            List<Vertex> copy = new List<Vertex>();
            foreach (Vertex v in org)
            {
                copy.Add(v);
            }
            return copy;
        }
    }

    public class Vertex
    {
        public int id { get; set; }
        public int degree { get; set; }

        public Vertex(int _id, int _degree)
        {
            id = _id;
            degree = _degree;
        }
    }

    public class Graph
    {
        public int numberOfVertices { get; set; }
        public int[,] adjacencyMatrix { get; set; }
        public List<Vertex> degreesList { get; set; }

        public Graph(int n, int[,] m)
        {
            numberOfVertices = n;
            adjacencyMatrix = m;
            degreesList = new List<Vertex>();

            int edges = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (adjacencyMatrix[i, j] == 1)
                        edges++;
                }
                degreesList.Add(new Vertex(i, edges - 1));
                edges = 0;
            }

            degreesList.Sort((p, q) => -1 * p.degree.CompareTo(q.degree));
        }

        public Vertex findVertexWithMaxDegree()
        {
            return degreesList.First();
        }

        public List<Vertex> findNeighbours(Vertex v, int degree, int id)
        {
            List<Vertex> neighbours = new List<Vertex>();

            for (int i = 0; i < numberOfVertices; i++)
            {
                if (adjacencyMatrix[v.id, i] == 1 && v.id != i && degreesList.Find(vertex => vertex.id == i).degree == degree && i != id)
                    neighbours.Add(degreesList.Find(vertex => vertex.id == i));
            }

            return neighbours;
        }
    }
}

////Example 1
//size1 = 9;
//adjacencyMatrix1 = new int[9, 9]
//{
//    {1,1,0,0,1,0,0,0,0},
//    {1,1,1,0,1,0,0,0,0},
//    {0,1,1,1,0,0,0,0,0},
//    {0,0,1,1,1,0,0,0,0},
//    {1,1,0,1,1,1,0,0,0},
//    {0,0,0,0,1,1,1,0,1},
//    {0,0,0,0,0,1,1,1,1},
//    {0,0,0,0,0,0,1,1,1},
//    {0,0,0,0,0,1,1,1,1}
//};
//G1 = new Graph(size1, adjacencyMatrix1);

//size2 = 9;
//adjacencyMatrix2 = new int[9, 9]
//{
//    {1,1,0,1,0,0,0,0,0},
//    {1,1,1,0,0,0,0,0,0},
//    {0,1,1,0,1,0,0,0,0},
//    {1,0,0,1,1,0,0,0,0},
//    {0,0,1,1,1,1,0,0,0},
//    {0,0,0,0,1,1,1,1,1},
//    {0,0,0,0,0,1,1,1,0},
//    {0,0,0,0,0,1,1,1,1},
//    {0,0,0,0,0,1,0,1,1}
//};
//G2 = new Graph(size2, adjacencyMatrix2);

//subgraphVertices1 = new List<Vertex>();
//subgraphVertices2 = new List<Vertex>();

//subgraphAdjacencyMatrix1 = new int[9, 9];
//subgraphAdjacencyMatrix2 = new int[9, 9];


//Example 2
//size1 = 5;
//adjacencyMatrix1 = new int[5, 5]
//{
//    {1,1,0,0,0},
//    {1,1,1,0,0},
//    {0,1,1,1,1},
//    {0,0,1,1,1},
//    {0,0,1,1,1},
//};
//G1 = new Graph(size1, adjacencyMatrix1);

//size2 = 5;
//adjacencyMatrix2 = new int[5, 5]
//{
//    {1,1,0,0,0},
//    {1,1,1,0,0},
//    {0,1,1,1,0},
//    {0,0,1,1,1},
//    {0,0,0,1,1},
//};
//G2 = new Graph(size2, adjacencyMatrix2);

//subgraphVertices1 = new List<Vertex>();
//subgraphVertices2 = new List<Vertex>();

//subgraphAdjacencyMatrix1 = new int[5, 5];
//subgraphAdjacencyMatrix2 = new int[5, 5];