using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Parsing.CommandLineParsing;



namespace CargoWise.DPS.ContentManagement
{
    public class GrepWithLinq : Command
    {

        public override void Execute()
        {
            // Modify this path as necessary so that it accesses your version of Visual Studio.
            string startFolder = @"C:\Program Files (x86)\Microsoft Visual Studio 10.0";
            // One of the following paths may be more appropriate on your computer.
            //string startFolder = @"c:\program files (x86)\Microsoft Visual Studio 9.0\";
            //string startFolder = @"c:\program files\Microsoft Visual Studio 10.0\";
            //string startFolder = @"c:\program files (x86)\Microsoft Visual Studio 10.0\";

            // Take a snapshot of the file system.
            IEnumerable<System.IO.FileInfo> fileList = GetFiles(startFolder);

            // Create the regular expression to find all things "Visual".
            System.Text.RegularExpressions.Regex searchTerm =
                new System.Text.RegularExpressions.Regex(@"Visual (Basic|C#|C\+\+|J#|SourceSafe|Studio)");

            // Search the contents of each .htm file.
            // Remove the where clause to find even more matchedValues!
            // This query produces a list of files where a match
            // was found, and a list of the matchedValues in that file.
            // Note: Explicit typing of "Match" in select clause.
            // This is required because MatchCollection is not a 
            // generic IEnumerable collection.
            var queryMatchingFiles =
                from file in fileList
                where file.Extension == ".htm"
                let fileText = System.IO.File.ReadAllText(file.FullName)
                let matches = searchTerm.Matches(fileText)
                where matches.Count > 0
                select new
                {
                    name = file.FullName,
                    matchedValues = from System.Text.RegularExpressions.Match match in matches
                                    select match.Value
                };

            // Execute the query.
            Console.WriteLine("The term \"{0}\" was found in:", searchTerm.ToString());

            foreach (var v in queryMatchingFiles)
            {
                // Trim the path a bit, then write 
                // the file name in which a match was found.
                string s = v.name.Substring(startFolder.Length - 1);
                Console.WriteLine(s);

                // For this file, write out all the matching strings
                foreach (var v2 in v.matchedValues)
                {
                    Console.WriteLine("  " + v2);
                }
            }

            // Keep the console window open in debug mode
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        // This method assumes that the application has discovery 
        // permissions for all folders under the specified path.
        static IEnumerable<System.IO.FileInfo> GetFiles(string path)
        {
            if (!System.IO.Directory.Exists(path))
                throw new System.IO.DirectoryNotFoundException();

            string[] fileNames = null;
            List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();

            fileNames = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (string name in fileNames)
            {
                files.Add(new System.IO.FileInfo(name));
            }
            return files;
        }


    }
}
