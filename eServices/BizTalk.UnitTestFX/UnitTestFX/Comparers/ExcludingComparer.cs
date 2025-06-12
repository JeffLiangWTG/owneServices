using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

namespace CargoWise.BizTalk.UnitTestFX
{
	/// <summary>
	/// Allows comparision of two documents with the exclusion of the required Xpaths
	/// </summary>
	/// <remarks>
	/// Currently edits the actual output with the values from the 'expected' stream.  This is not
	/// a very good approach.
	/// Other options:
	/// Use dual Xml readers to parse the actual and expected streams manually, skipping past the exclusion xpaths
	/// </remarks>
	public class ExcludingComparer : ICompare
	{
		private IEnumerable<string> _exclusionXpaths;

		public ExcludingComparer(params string[] exclusionXpaths)
			: this(exclusionXpaths.ToList())
		{
		}

		public ExcludingComparer(IEnumerable<string> exclusionXpaths)
		{
			_exclusionXpaths = exclusionXpaths;
		}

		/// <summary>
		/// Compare the Xml document streams but exclude checks on an xpath list
		/// </summary>
		/// <remarks>Retrieves values from expected document at the exclusion xpaths and edits the
		/// <paramref name="actual"/> document setting the same xpath values to to those in the
		/// expected document</remarks>
		/// <param name="actual">Xml document to compare</param>
		/// <param name="expected">Xml document that <paramref name="actual"/> is expected to be a likeness of</param>
		/// <param name="ignoreComments">ignore the xml comments in comparison</param>
		/// <returns>Result of comparison including the actual output and updategram Xml if a difference is found</returns>
		public MapResult Execute(Stream actual, Stream expected, bool ignoreComments = true)
		{
			XmlDocument expectedDocument = new XmlDocument();
			XmlDocument actualDocument = new XmlDocument();

			expectedDocument.Load(expected);
			actualDocument.Load(actual);

			foreach (string xpath in _exclusionXpaths)
			{
				XmlNodeList expectedNodes = expectedDocument.DocumentElement.SelectNodes(xpath);
				XmlNodeList actualNodes = actualDocument.DocumentElement.SelectNodes(xpath);
				for (int i = 0; i < Math.Min(expectedNodes.Count, actualNodes.Count); i++)
				{
					actualNodes[i].InnerText = expectedNodes[i].InnerText;
				}
			}

			MapResult result = new MapResult();
			using (MemoryStream editedActual = new MemoryStream())
			{
				using (XmlWriter editedRewriter = XmlHelper.CreateFormattedXmlWriter(editedActual))
				{
					actualDocument.WriteContentTo(editedRewriter);
					editedRewriter.Flush();
				}
				editedActual.Position = 0;
				expected.Position = 0;

				XmlDiffTool comparer = new XmlDiffTool();
				result = comparer.Execute(editedActual, expected);
			}
			return result;
		}
	}
}
