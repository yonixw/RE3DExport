using System;
using System.IO;

namespace NVXConverter
{
    /// <summary>
    /// Main converter class for NVX to OBJ conversion
    /// </summary>
    public class NVXConverter
    {
        /// <summary>
        /// Converts a NVX file (NVX1 or NVX2) to OBJ format
        /// </summary>
        /// <param name="inputFile">Path to the input NVX file</param>
        /// <param name="outputFile">Path to the output OBJ file</param>
        /// <returns>True if conversion was successful</returns>
        public static bool Convert(string inputFile, string outputFile)
        {
            try
            {
                Console.WriteLine($"Converting {inputFile} to {outputFile}");
                
                // Determine file type by reading the magic number
                string fileType = DetermineFileType(inputFile);
                Console.WriteLine($"Detected file type: {fileType}");
                
                // Load the mesh based on file type
                Mesh mesh = null;
                switch (fileType)
                {
                    case "NVX1":
                        mesh = NVX1Reader.Read(inputFile);
                        break;
                    case "NVX2":
                        mesh = NVX2Reader.Read(inputFile);
                        break;
                    default:
                        Console.WriteLine($"Unsupported file type: {fileType}");
                        return false;
                }
                
                Console.WriteLine($"Loaded mesh with {mesh.Vertices.Count} vertices and {mesh.Triangles.Count} triangles");
                
                // Write the mesh to OBJ format
                OBJWriter.Write(mesh, outputFile);
                Console.WriteLine($"Successfully wrote OBJ file to {outputFile}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during conversion: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }
        
        /// <summary>
        /// Determines the file type by reading the magic number
        /// </summary>
        private static string DetermineFileType(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                int magic = reader.ReadInt32();
                
                if (magic == 0x4E565831) // 'NVX1'
                    return "NVX1";
                else if (magic == 0x4E565832) // 'NVX2'
                    return "NVX2";
                else
                    return "Unknown";
            }
        }
    }
}