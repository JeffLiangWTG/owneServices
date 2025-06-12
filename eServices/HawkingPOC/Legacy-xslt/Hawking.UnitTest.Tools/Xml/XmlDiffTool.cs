using System;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.XmlDiffPatch;

namespace Hawking.UnitTest.Tools.Xml
{
    public static class XmlDiffTool
    {
        /// <summary>
        /// Compare the Xml document streams
        /// </summary>
        /// <param name="actual">Xml document to compare</param>
        /// <param name="expected">Xml document that <paramref name="actual"/> is expected to be a likeness of</param>
        /// <returns>Result of comparison including the actual output and updategram Xml if a difference is found</returns>
        public static XmlDiffResult Execute(Stream actual, Stream expected)
        {
            actual.Position = 0;
            expected.Position = 0;

            using (var actualReader = XmlReader.Create(actual))
            {
                using (var expectedReader = XmlReader.Create(expected))
                {
                    using (var updateGram = new MemoryStream())
                    {
                        using (var updateGramWriter = XmlHelper.CreateFormattedXmlWriter(updateGram))
                        {
                            var comparer = new XmlDiff
                            {
                                IgnoreXmlDecl = true,
                                IgnoreWhitespace = true,
                                IgnoreComments = true
                            };

                            var result = new XmlDiffResult();

                            // Pass the readers in this order so the updategram makes more sense to the test result output on failure	
                            try
                            {
                                result.Success = comparer.Compare(expectedReader, actualReader, updateGramWriter);
                                result.OutputUpdateGram = Encoding.UTF8.GetString(updateGram.ToArray());
                            }
                            catch (IndexOutOfRangeException exception) // By handling the exception the developer can actually see the MapOutput that caused the compare to explode
                            {
                                result.Exception = exception;
                                result.Success = false;
                                result.OutputUpdateGram = "The combination of the actual output and the expected output caused the call to Microsoft.XmlDiffPatch.XmlDiff.Compare to randomly explode with an IndexOutOfRangeException. This is not an issue, just manually compare the actual and expected output yourself.";
                            }
                            catch (XmlException exception) // By handling the exception the developer can actually see the MapOutput that caused the compare to fail.
                            {
                                result.Exception = exception;
                                result.Success = false;
                                result.OutputUpdateGram = "The actual output or the expected output is invalid XML.";
                            }

                            if (!result.Success)
                            {
                                actual.Position = 0;
                                result.Actual = new StreamReader(actual).ReadToEnd(); // Encoding.UTF8.GetString(actual.ToArray());

                                expected.Position = 0;
                                result.Expected = new StreamReader(expected).ReadToEnd();

                                if (0 == string.Compare(
                                    result.Actual, result.Expected,
                                    StringComparison.InvariantCultureIgnoreCase))
                                {
                                    result.Success = true;
                                }
                            }

                            return result;
                        }
                    }
                }
            }
        }
    }
}
