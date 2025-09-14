using NVXConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE3DExport
{
    public class NVXConverterInput
    {

        public string InputFile { get; set; }

        public string OutputFile { get; set; }

        public NVXConverterInput(string inputFile, string outputFile = null)
        {
            InputFile = inputFile;
            OutputFile = outputFile ?? Path.ChangeExtension(inputFile, ".obj");
        }
    }

    public class NVXConverterResult 
    {   
        public bool Success { get; set; }

        public string OutputFile { get; set; }
        
        public int VertexCount { get; set; }
      
        public int TriangleCount { get; set; }
   
        public string FileType { get; set; }

        public TimeSpan ProcessingTime { get; set; }
    }

    class NVXConverter
    {
        private static string DetermineFileType(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                int magic = reader.ReadInt32(); // Will show in reverse in file

                if (magic == 0x4E565831) // 'NVX1'
                    return "NVX1";
                else if (magic == 0x4E565832) // 'NVX2'
                    return "NVX2";
                else
                    return "Unknown";
            }
        }

    public static void printResult (NVXConverterResult result)
        {
            if (result.Success)
            {
                Console.WriteLine($"Conversion completed successfully!\n\n" +
                    $"File type: {result.FileType}\n" +
                    $"Vertices: {result.VertexCount}\n" +
                    $"Triangles: {result.TriangleCount}\n" +
                    $"Processing time: {result.ProcessingTime.TotalSeconds:F2} seconds\n\n" +
                    $"Output file: {result.OutputFile}");
            }
            else
            {
                Console.WriteLine("Conversion failed.");
            }
        }
    
    public static NVXConverterResult Convert(NVXConverterInput input)
        {
            NVXConverterResult result = new NVXConverterResult();
            DateTime startTime = DateTime.Now;

            try
            {
                // Validate input file
                if (!File.Exists(input.InputFile))
                {
                    throw new FileNotFoundException($"Input file '{input.InputFile}' does not exist.");
                }

                Console.WriteLine("Validating input file...");

                // Determine file type
                string fileType = DetermineFileType(input.InputFile);
                result.FileType = fileType;

                Console.WriteLine($"Detected file type: {fileType}");

                Console.WriteLine("Loading mesh...");

                Mesh mesh = new Func<Mesh>(() =>
                {
                    switch (fileType)
                    {
                        case "NVX1":
                            return NVX1Reader.Read(input.InputFile);
                        case "NVX2":
                            return NVX2Reader.Read(input.InputFile);
                        default:
                            throw new InvalidOperationException($"Unsupported file type: {fileType}");
                    }
                })();

                // Store mesh information in result
                result.VertexCount = mesh.Vertices.Count;
                result.TriangleCount = mesh.Triangles.Count;

                Console.WriteLine($"Loaded mesh with {mesh.Vertices.Count} vertices and {mesh.Triangles.Count} triangles");
                
                Console.WriteLine("Writing OBJ file...");

                OBJWriter.Write(mesh, input.OutputFile);
                Console.WriteLine("Conversion completed successfully!");

                result.OutputFile = input.OutputFile;
                result.ProcessingTime = DateTime.Now - startTime;
                result.Success = true;
                printResult(result);
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error converting:\n {ex.Message}\n {ex.StackTrace}");
                result.ProcessingTime = DateTime.Now - startTime;
                result.Success = false;
                printResult(result);
                return result;
            }
        }
    }
}
