using Microsoft.XmlDiffPatch;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace CargoWise.BizTalk.UnitTestFX
{
	/// <summary>
	/// Leverages the XmlDiff tool from Microsoft to compare two Xml documents
	/// </summary>
	public class XmlDiffTool : ICompare
	{
		/// <summary>
		/// Compare the Xml document streams
		/// </summary>
		/// <param name="actual">Xml document to compare</param>
		/// <param name="expected">Xml document that <paramref name="actual"/> is expected to be a likeness of</param>
		/// <param name="ignoreComments">ignore the xml comments in comparison</param>
		/// <returns>Result of comparison including the actual output and updategram Xml if a difference is found</returns>
		public MapResult Execute(Stream actual, Stream expected, bool ignoreComments = true)
		{
			using (XmlReader actualReader = XmlReader.Create(actual))
			{
				using (XmlReader expectedReader = XmlReader.Create(expected))
				{
					using (MemoryStream updateGram = new MemoryStream())
					{
						using (XmlWriter updateGramWriter = XmlHelper.CreateFormattedXmlWriter(updateGram))
						{
							XmlDiff comparer = new XmlDiff
							{
								Options = ignoreComments ? XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreXmlDecl : XmlDiffOptions.IgnoreXmlDecl
							};
							MapResult result = new MapResult();
							// Pass the readers in this order so the updategram makes more sense to the test result output on failure	
							try
							{
								result.Success = comparer.Compare(expectedReader, actualReader, updateGramWriter);
								result.OutputUpdateGram = Encoding.UTF8.GetString(updateGram.ToArray());
							}
							catch (IndexOutOfRangeException) // By handling the exception the developer can actually see the MapOutput that caused the compare to explode
							{
								result.Success = false;
								result.OutputUpdateGram = "The combination of the actual output and the expected output caused the call to Microsoft.XmlDiffPatch.XmlDiff.Compare to randomly explode with an IndexOutOfRangeException. This is not an issue, just manually compare the actual and expected output yourself.";
							}
							catch (XmlException) // By handling the exception the developer can actually see the MapOutput that caused the compare to fail.
							{
								result.Success = false;
								result.OutputUpdateGram = "The actual output or the expected output is invalid XML.";
							}
							if (!result.Success)
							{
								actual.Position = 0;
								result.MapOutput = new StreamReader(actual).ReadToEnd(); // Encoding.UTF8.GetString(actual.ToArray());
							}
							return result;
						}
					}
				}
			}
		}
	}
}
