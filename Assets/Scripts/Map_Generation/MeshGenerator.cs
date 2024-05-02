using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter) , typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;

    List<Vector3> vertices;

    List<int> triangles;

    Dictionary<int, List<Triangle>> trianglesDict = new Dictionary<int, List<Triangle>>();
    public void GenerateMesh(int[,] map, float SquareSize)
    {
        squareGrid = new SquareGrid(map, SquareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.CentreLeft, square.CentreBottom, square.BottomLeft);
                break;
            case 2:
                MeshFromPoints(square.BottomRight, square.CentreBottom, square.CentreRight);
                break;
            case 4:
                MeshFromPoints(square.TopRight, square.CentreRight, square.CentreTop);
                break;
            case 8:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.CentreRight, square.BottomRight, square.BottomLeft, square.CentreLeft);
                break;
            case 6:
                MeshFromPoints(square.CentreTop, square.TopRight, square.BottomRight, square.CentreBottom);
                break;
            case 9:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreBottom, square.BottomLeft);
                break;
            case 12:
                MeshFromPoints(square.TopLeft, square.TopRight, square.CentreRight, square.CentreLeft);
                break;
            case 5:
                MeshFromPoints(square.CentreTop, square.TopRight, square.CentreRight, square.CentreBottom, square.BottomLeft, square.CentreLeft);
                break;
            case 10:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreRight, square.BottomRight, square.CentreBottom, square.CentreLeft);
                break;

            // 3 points:
            case 7:
                MeshFromPoints(square.CentreTop, square.TopRight, square.BottomRight, square.BottomLeft, square.CentreLeft);
                break;
            case 11:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreRight, square.BottomRight, square.BottomLeft);
                break;
            case 13:
                MeshFromPoints(square.TopLeft, square.TopRight, square.CentreRight, square.CentreBottom, square.BottomLeft);
                break;
            case 14:
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.CentreBottom, square.CentreLeft);
                break;

            // 4 points:
            case 15:
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.BottomLeft);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangles(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangles(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangles(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangles(points[0], points[4], points[5]);


    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangles(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);

        AddTrianglesToDictionary(triangle.vertexIndexA, triangle);
        AddTrianglesToDictionary(triangle.vertexIndexB, triangle);
        AddTrianglesToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTrianglesToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (trianglesDict.ContainsKey(vertexIndexKey))
        {
            trianglesDict[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();

            triangleList.Add(triangle);

            trianglesDict.Add(vertexIndexKey, triangleList);
        }
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = trianglesDict[vertexA];
        int sharedTriangleCount = 0;
        for (int i = 0; i< trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
        }
        public bool Contains(int vertexIndex)
        {
            return vertexIndex== vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;
        public SquareGrid(int[,] map, float SquareSize)
        {
            int NodeCountX = map.GetLength(0);
            int NodeCountY = map.GetLength(1);
            float mapWidth = NodeCountX * SquareSize;
            float mapHeight = NodeCountY * SquareSize;

            ControlNode[,] controlNodes = new ControlNode[NodeCountX, NodeCountY];

            for (int x = 0; x < NodeCountX; x++)
            {
                for (int y = 0; y < NodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * SquareSize + SquareSize / 2, 0, -mapHeight / 2 + y * SquareSize + SquareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, SquareSize);
                }
            }
            squares = new Square[NodeCountX - 1, NodeCountY - 1];
            for (int x = 0; x < NodeCountX - 1; x++)
            {
                for (int y = 0; y < NodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode TopLeft, TopRight, BottomRight, BottomLeft;
        public Node CentreTop, CentreRight, CentreBottom, CentreLeft;
        public int configuration;
        public Square(ControlNode _TopLeft, ControlNode _TopRight, ControlNode _BottomRight, ControlNode _BottomLeft)
        {
            TopLeft = _TopLeft;
            TopRight = _TopRight;
            BottomRight = _BottomRight;
            BottomLeft = _BottomLeft;

            CentreTop = TopLeft.right;
            CentreRight = BottomRight.above;
            CentreBottom = BottomLeft.right;
            CentreLeft = BottomLeft.above;

            if (TopLeft.active)
                configuration += 8;
            if (TopRight.active)
                configuration += 4;
            if (BottomRight.active)
                configuration += 2;
            if (BottomLeft.active)
                configuration += 1;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _position)
        {
            position = _position;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;
        public ControlNode(Vector3 _position, bool _active, float SquareSize) : base(_position)
        {
            active = _active;
            above = new Node(position + Vector3.forward * SquareSize / 2f);
            right = new Node(position + Vector3.right * SquareSize / 2f);
        }
    }
}
