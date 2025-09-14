using System;
using System.IO;
using System.Numerics;

namespace NVXConverter
{
    /// <summary>
    /// Reader for NVX1 format files from Nebula2 engine
    /// </summary>
    public class NVX1Reader
    {
        private const int NVX1_MAGIC = 0x4E565831; // 'NVX1' in ASCII

        // Vertex component flags for NVX1 format
        [Flags]
        private enum NVX1VertexComponents
        {
            None  = 0,
            Coord = (1 << 0),  // Position (x,y,z)
            Norm  = (1 << 1),  // Normal vector
            RGBA  = (1 << 2),  // Vertex color
            UV0   = (1 << 3),  // First UV set
            UV1   = (1 << 4),  // Second UV set
            UV2   = (1 << 5),  // Third UV set
            UV3   = (1 << 6),  // Fourth UV set
            JW    = (1 << 7),  // Joint weights
        }

        /// <summary>
        /// Reads a NVX1 file and returns a Mesh object
        /// </summary>
        public static Mesh Read(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                Mesh mesh = new Mesh();

                // Read header
                int magic = reader.ReadInt32();
                if (magic != NVX1_MAGIC)
                {
                    throw new InvalidDataException($"Not a valid NVX1 file. Magic number: {magic:X}");
                }

                int numVertices = reader.ReadInt32();
                int numIndices = reader.ReadInt32();
                int numEdges = reader.ReadInt32();
                int vertexType = reader.ReadInt32();
                int dataStart = reader.ReadInt32();
                int dataSize = reader.ReadInt32();

                NVX1VertexComponents nvx1Components = (NVX1VertexComponents)vertexType;

                // Map NVX1 components to our mesh components
                if ((nvx1Components & NVX1VertexComponents.Coord) != 0)
                    mesh.Components |= Mesh.VertexComponents.Coord;
                if ((nvx1Components & NVX1VertexComponents.Norm) != 0)
                    mesh.Components |= Mesh.VertexComponents.Normal;
                if ((nvx1Components & NVX1VertexComponents.RGBA) != 0)
                    mesh.Components |= Mesh.VertexComponents.Color;
                if ((nvx1Components & NVX1VertexComponents.UV0) != 0)
                    mesh.Components |= Mesh.VertexComponents.Uv0;
                if ((nvx1Components & NVX1VertexComponents.UV1) != 0)
                    mesh.Components |= Mesh.VertexComponents.Uv1;
                if ((nvx1Components & NVX1VertexComponents.UV2) != 0)
                    mesh.Components |= Mesh.VertexComponents.Uv2;
                if ((nvx1Components & NVX1VertexComponents.UV3) != 0)
                    mesh.Components |= Mesh.VertexComponents.Uv3;
                if ((nvx1Components & NVX1VertexComponents.JW) != 0)
                {
                    mesh.Components |= Mesh.VertexComponents.Weights;
                    mesh.Components |= Mesh.VertexComponents.JIndices;
                }

                // Seek to data start position
                reader.BaseStream.Seek(dataStart, SeekOrigin.Begin);

                // Read vertices
                for (int i = 0; i < numVertices; i++)
                {
                    Mesh.Vertex vertex = new Mesh.Vertex();

                    // Read vertex components based on the component mask
                    if ((nvx1Components & NVX1VertexComponents.Coord) != 0)
                    {
                        vertex.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((nvx1Components & NVX1VertexComponents.Norm) != 0)
                    {
                        vertex.Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((nvx1Components & NVX1VertexComponents.RGBA) != 0)
                    {
                        uint color = reader.ReadUInt32();
                        float b = ((color >> 24) & 0xff) / 255.0f;
                        float g = ((color >> 16) & 0xff) / 255.0f;
                        float r = ((color >> 8) & 0xff) / 255.0f;
                        float a = (color & 0xff) / 255.0f;
                        vertex.Color = new Vector4(r, g, b, a);
                    }

                    if ((nvx1Components & NVX1VertexComponents.UV0) != 0)
                    {
                        vertex.UVs[0] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((nvx1Components & NVX1VertexComponents.UV1) != 0)
                    {
                        vertex.UVs[1] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((nvx1Components & NVX1VertexComponents.UV2) != 0)
                    {
                        vertex.UVs[2] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((nvx1Components & NVX1VertexComponents.UV3) != 0)
                    {
                        vertex.UVs[3] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((nvx1Components & NVX1VertexComponents.JW) != 0)
                    {
                        // Read joint indices
                        short ji0 = reader.ReadInt16();
                        short ji1 = reader.ReadInt16();
                        short ji2 = reader.ReadInt16();
                        short ji3 = reader.ReadInt16();
                        vertex.JointIndices = new Vector4(ji0, ji1, ji2, ji3);

                        // Read weights
                        float w0 = reader.ReadSingle();
                        float w1 = reader.ReadSingle();
                        float w2 = reader.ReadSingle();
                        float w3 = reader.ReadSingle();
                        vertex.Weights = new Vector4(w0, w1, w2, w3);
                    }

                    mesh.Vertices.Add(vertex);
                }

                // Skip edges
                for (int i = 0; i < numEdges; i++)
                {
                    reader.ReadUInt16(); // we0
                    reader.ReadUInt16(); // we1
                    reader.ReadUInt16(); // we2
                    reader.ReadUInt16(); // we3
                }

                // Read triangle indices
                int numTriangles = numIndices / 3;
                for (int i = 0; i < numTriangles; i++)
                {
                    Mesh.Triangle triangle = new Mesh.Triangle();
                    triangle.VertexIndices[0] = reader.ReadUInt16();
                    triangle.VertexIndices[1] = reader.ReadUInt16();
                    triangle.VertexIndices[2] = reader.ReadUInt16();
                    triangle.GroupId = 0; // NVX1 doesn't have groups, so assign all to group 0
                    mesh.Triangles.Add(triangle);
                }

                // Create a single group for all triangles
                Mesh.Group group = new Mesh.Group
                {
                    Id = 0,
                    FirstVertex = 0,
                    NumVertices = numVertices,
                    FirstTriangle = 0,
                    NumTriangles = numTriangles,
                    FirstEdge = 0,
                    NumEdges = numEdges
                };
                mesh.Groups.Add(group);

                return mesh;
            }
        }
    }
}