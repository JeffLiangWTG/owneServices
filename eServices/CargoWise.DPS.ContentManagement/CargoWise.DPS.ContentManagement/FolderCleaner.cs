using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Parsing.CommandLineParsing;


namespace CargoWise.DPS.ContentManagement
{
    public class FolderCleaner : Command
    {

        public string DbfDir { get; set; }
        public override void Execute()
        {

            Regex dataDir =
            new Regex(@"\d\d\d\d_\d\d_\d\d");

            bool matched = dataDir.IsMatch("1111_11_11");
            matched = dataDir.IsMatch("x111_11_11");
            matched = dataDir.IsMatch("x1111_11_11");
            matched = dataDir.IsMatch("1111_11_11y");
            matched = dataDir.IsMatch("x1111_11_11y");


            //The filesystem
            var q = from directory in new DirectoryInfo(@"c:\DPS\AU_DFAT").FlattenHierarchy(x => x.GetDirectories())
                    where dataDir.IsMatch(directory.Name)
                    orderby directory.Name descending
                    select directory;

            foreach (DirectoryInfo dir in q)
            {

                Console.WriteLine(dir.Name + "  " + dir.FullName);
            }
            Console.ReadLine();
        }


    }
    static public class LinqExtensions
    {

        public static void Rename(this DirectoryInfo dirInfo, string newName)
        {
            try
            {
                // "rename" it 
                dirInfo.MoveTo(newName);
            }
            catch (IOException ioe)
            {
                // most likely given the directory exists or isn't empty 
                Console.WriteLine(ioe.ToString());
            }
        } 

        
        static public IEnumerable<T> Descendants<T>(this IEnumerable<T> source,
                                                    Func<T, IEnumerable<T>> DescendBy)
        {
            foreach (T value in source)
            {
                yield return value;

                foreach (T child in DescendBy(value).Descendants<T>(DescendBy))
                {
                    yield return child;
                }
            }
        }

        public static IEnumerable<T> FlattenHierarchy<T>(this T node, Func<T, IEnumerable<T>> getChildEnumerator)
        {
            yield return node;
            if (getChildEnumerator(node) != null)
            {
                foreach (var child in getChildEnumerator(node))
                {
                    foreach (var childOrDescendant in child.FlattenHierarchy(getChildEnumerator))
                    {
                        yield return childOrDescendant;
                    }
                }
            }
        }

    }
}
