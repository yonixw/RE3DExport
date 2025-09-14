using System;
using System.IO;
using System.Numerics;

namespace NVXConverter
{
    /// <summary>
    /// Reader for NVX2 format files from Nebula2 engine
    /// </summary>
    public class NVX2Reader
    {
        private const int NVX2_MAGIC = 0x4E565832; // 'NVX2' in ASCII

        /// <summary>
        /// Reads a NVX2 file and returns a Mesh object
        /// </summary>
        public static Mesh Read(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                Mesh mesh = new Mesh();

                // Read header
                int magic = reader.ReadInt32();
                if (magic != NVX2_MAGIC)
                {
                    throw new InvalidDataException($"Not a valid NVX2 file. Magic number: {magic:X}");
                }

                int numGroups = reader.ReadInt32();
                int numVertices = reader.ReadInt32();
                int vertexWidth = reader.ReadInt32();
                int numTriangles = reader.ReadInt32();
                int numEdges = reader.ReadInt32();
                int vertexComponents = reader.ReadInt32();

                mesh.Components = (Mesh.VertexComponents)vertexComponents;

                // Read groups
                for (int i = 0; i < numGroups; i++)
                {
                    Mesh.Group group = new Mesh.Group
                    {
                        FirstVertex = reader.ReadInt32(),
                        NumVertices = reader.ReadInt32(),
                        FirstTriangle = reader.ReadInt32(),
                        NumTriangles = reader.ReadInt32(),
                        FirstEdge = reader.ReadInt32(),
                        NumEdges = reader.ReadInt32(),
                        Id = i
                    };
                    mesh.Groups.Add(group);
                }

                // Read vertices
                for (int i = 0; i < numVertices; i++)
                {
                    Mesh.Vertex vertex = new Mesh.Vertex();

                    // Read vertex components based on the component mask
                    if ((vertexComponents & (int)Mesh.VertexComponents.Coord) != 0)
                    {
                        vertex.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Normal) != 0)
                    {
                        vertex.Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Uv0) != 0)
                    {
                        vertex.UVs[0] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Uv1) != 0)
                    {
                        vertex.UVs[1] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Uv2) != 0)
                    {
                        vertex.UVs[2] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Uv3) != 0)
                    {
                        vertex.UVs[3] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Color) != 0)
                    {
                        vertex.Color = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Tangent) != 0)
                    {
                        vertex.Tangent = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Binormal) != 0)
                    {
                        vertex.Binormal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.Weights) != 0)
                    {
                        vertex.Weights = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((vertexComponents & (int)Mesh.VertexComponents.JIndices) != 0)
                    {
                        vertex.JointIndices = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    mesh.Vertices.Add(vertex);
                }

                // Read triangle indices
                for (int i = 0; i < numTriangles; i++)
                {
                    Mesh.Triangle triangle = new Mesh.Triangle();
                    triangle.VertexIndices[0] = reader.ReadUInt16();
                    triangle.VertexIndices[1] = reader.ReadUInt16();
                    triangle.VertexIndices[2] = reader.ReadUInt16();

                    // Assign group ID based on triangle index
                    foreach (var group in mesh.Groups)
                    {
                        if (i >= group.FirstTriangle && i < group.FirstTriangle + group.NumTriangles)
                        {
                            triangle.GroupId = group.Id;
                            break;
                        }
                    }

                    mesh.Triangles.Add(triangle);
                }

                // Read edges if present
                if (numEdges > 0)
                {
                    for (int i = 0; i < numEdges; i++)
                    {
                        Mesh.Edge edge = new Mesh.Edge();
                        edge.FaceIndices[0] = reader.ReadUInt16();
                        edge.FaceIndices[1] = reader.ReadUInt16();
                        edge.VertexIndices[0] = reader.ReadUInt16();
                        edge.VertexIndices[1] = reader.ReadUInt16();
                        mesh.Edges.Add(edge);
                    }
                }

                return mesh;
            }
        }
    }
}