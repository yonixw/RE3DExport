using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace NVXConverter
{
    /// <summary>
    /// Writer for Wavefront OBJ format files
    /// </summary>
    public class OBJWriter
    {
        /// <summary>
        /// Writes a Mesh object to an OBJ file
        /// </summary>
        public static void Write(Mesh mesh, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.ASCII))
            {
                // Write header comment
                writer.WriteLine("# OBJ file exported from NVXConverter");
                writer.WriteLine("# https://github.com/yourusername/NVXConverter");
                writer.WriteLine($"# Vertices: {mesh.Vertices.Count}, Faces: {mesh.Triangles.Count}, Groups: {mesh.Groups.Count}");
                writer.WriteLine();

                // Write vertex positions
                foreach (var vertex in mesh.Vertices)
                {
                    writer.WriteLine($"v {vertex.Position.X} {vertex.Position.Y} {vertex.Position.Z}");
                }
                writer.WriteLine();

                // Write vertex normals if present
                if ((mesh.Components & Mesh.VertexComponents.Normal) != 0)
                {
                    foreach (var vertex in mesh.Vertices)
                    {
                        writer.WriteLine($"vn {vertex.Normal.X} {vertex.Normal.Y} {vertex.Normal.Z}");
                    }
                    writer.WriteLine();
                }

                // Write texture coordinates if present
                if ((mesh.Components & Mesh.VertexComponents.Uv0) != 0)
                {
                    foreach (var vertex in mesh.Vertices)
                    {
                        writer.WriteLine($"vt {vertex.UVs[0].X} {vertex.UVs[0].Y}");
                    }
                    writer.WriteLine();
                }

                // Write vertex colors as comments if present
                if ((mesh.Components & Mesh.VertexComponents.Color) != 0)
                {
                    writer.WriteLine("# Vertex colors (not standard OBJ, provided as comments)");
                    for (int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        var color = mesh.Vertices[i].Color;
                        writer.WriteLine($"# vc {i+1} {color.X} {color.Y} {color.Z} {color.W}");
                    }
                    writer.WriteLine();
                }

                // Write faces grouped by mesh groups
                int currentGroup = -1;
                
                foreach (var triangle in mesh.Triangles)
                {
                    // If we're entering a new group, write the group header
                    if (triangle.GroupId != currentGroup)
                    {
                        currentGroup = triangle.GroupId;
                        writer.WriteLine($"g Group{currentGroup}");
                    }

                    // OBJ indices are 1-based, so add 1 to all indices
                    // Format depends on what data we have
                    if ((mesh.Components & Mesh.VertexComponents.Normal) != 0 && 
                        (mesh.Components & Mesh.VertexComponents.Uv0) != 0)
                    {
                        // Position/UV/Normal
                        writer.WriteLine($"f {triangle.VertexIndices[0]+1}/{triangle.VertexIndices[0]+1}/{triangle.VertexIndices[0]+1} " +
                                        $"{triangle.VertexIndices[1]+1}/{triangle.VertexIndices[1]+1}/{triangle.VertexIndices[1]+1} " +
                                        $"{triangle.VertexIndices[2]+1}/{triangle.VertexIndices[2]+1}/{triangle.VertexIndices[2]+1}");
                    }
                    else if ((mesh.Components & Mesh.VertexComponents.Uv0) != 0)
                    {
                        // Position/UV
                        writer.WriteLine($"f {triangle.VertexIndices[0]+1}/{triangle.VertexIndices[0]+1} " +
                                        $"{triangle.VertexIndices[1]+1}/{triangle.VertexIndices[1]+1} " +
                                        $"{triangle.VertexIndices[2]+1}/{triangle.VertexIndices[2]+1}");
                    }
                    else if ((mesh.Components & Mesh.VertexComponents.Normal) != 0)
                    {
                        // Position//Normal
                        writer.WriteLine($"f {triangle.VertexIndices[0]+1}//{triangle.VertexIndices[0]+1} " +
                                        $"{triangle.VertexIndices[1]+1}//{triangle.VertexIndices[1]+1} " +
                                        $"{triangle.VertexIndices[2]+1}//{triangle.VertexIndices[2]+1}");
                    }
                    else
                    {
                        // Position only
                        writer.WriteLine($"f {triangle.VertexIndices[0]+1} {triangle.VertexIndices[1]+1} {triangle.VertexIndices[2]+1}");
                    }
                }
            }
        }
    }
}