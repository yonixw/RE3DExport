using System;
using System.Collections.Generic;
using System.Numerics;

namespace NVXConverter
{
    /// <summary>
    /// Represents a 3D mesh with vertices, triangles, and optional components
    /// </summary>
    public class Mesh
    {
        // Vertex component flags (matching the Nebula2 engine)
        [Flags]
        public enum VertexComponents
        {
            None     = 0,
            Coord    = (1 << 0),  // Position (x,y,z)
            Normal   = (1 << 1),  // Normal vector
            Uv0      = (1 << 2),  // First UV set
            Uv1      = (1 << 3),  // Second UV set
            Uv2      = (1 << 4),  // Third UV set
            Uv3      = (1 << 5),  // Fourth UV set
            Color    = (1 << 6),  // Vertex color
            Tangent  = (1 << 7),  // Tangent vector
            Binormal = (1 << 8),  // Binormal vector
            Weights  = (1 << 9),  // Skinning weights
            JIndices = (1 << 10), // Joint indices
        }

        // Vertex structure
        public class Vertex
        {

            public Vector3 Position { get; set; }
            public Vector3 Normal { get; set; }
            public Vector2[] UVs { get; set; } = new Vector2[4]; // 4 UV sets
            public Vector4 Color { get; set; }
            public Vector3 Tangent { get; set; }
            public Vector3 Binormal { get; set; }
            public Vector4 Weights { get; set; }
            public Vector4 JointIndices { get; set; }

            public Vertex()
            {
                Position = Vector3.Zero;
                Normal = Vector3.Zero;
                Color = Vector4.One;
                Tangent = Vector3.Zero;
                Binormal = Vector3.Zero;
                Weights = Vector4.Zero;
                JointIndices = Vector4.Zero;
                
                for (int i = 0; i < 4; i++)
                {
                    UVs[i] = Vector2.Zero;
                }
            }
        }

        // Triangle structure
        public class Triangle
        {
            public int[] VertexIndices { get; set; } = new int[3];
            public int GroupId { get; set; }

            public Triangle()
            {
                GroupId = 0;
            }
        }

        // Mesh group structure
        public class Group
        {
            public int Id { get; set; }
            public int FirstVertex { get; set; }
            public int NumVertices { get; set; }
            public int FirstTriangle { get; set; }
            public int NumTriangles { get; set; }
            public int FirstEdge { get; set; }
            public int NumEdges { get; set; }
        }

        // Edge structure
        public class Edge
        {
            public ushort[] FaceIndices { get; set; } = new ushort[2];
            public ushort[] VertexIndices { get; set; } = new ushort[2];
        }

        // Mesh data
        public List<Vertex> Vertices { get; set; } = new List<Vertex>();
        public List<Triangle> Triangles { get; set; } = new List<Triangle>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<Edge> Edges { get; set; } = new List<Edge>();
        public VertexComponents Components { get; set; }

        public Mesh()
        {
            Components = VertexComponents.None;
        }
    }
}