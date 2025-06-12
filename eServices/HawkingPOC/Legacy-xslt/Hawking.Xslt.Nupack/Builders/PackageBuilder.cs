using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Hawking.Xslt.Nupack
{
    internal class PackageBuilder
    {
        const string XslFileExtension = "*.xsl";
        const string ProjectFilesPattern = "*.btproj";

        readonly Args args;

        public PackageBuilder(Args args)
        {
            this.args = args;

            ZipFileName = $"Hawking-Xslt-{DateTime.Now.ToString("yyyyMMdd-HH.mm.ss")}";
            OutputDir = args.OutputDirectory;
        }

        public string OutputDir { get; set; }
        public string ZipFileName { get; }

        public void Pack()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), ZipFileName);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            var projectPackBuilder = new ProjectPackBuilder(tempDir);
            var projFiles = Directory.GetFiles(args.TargetDir, ProjectFilesPattern, SearchOption.AllDirectories);
            if (projFiles.Any())
            {
                Directory.CreateDirectory(tempDir);
            }

            Console.WriteLine($"Total {projFiles.Count()} {ProjectFilesPattern} projects");
            foreach ( var projFile in projFiles)
            {
                projectPackBuilder.PackProject(projFile);
            }

            var zipFilePath = Path.Combine(OutputDir, $"{ZipFileName}.zip");
            ZipFile.CreateFromDirectory(tempDir, zipFilePath);

            Console.WriteLine("Pack completed successfully");
        }
    }
}
