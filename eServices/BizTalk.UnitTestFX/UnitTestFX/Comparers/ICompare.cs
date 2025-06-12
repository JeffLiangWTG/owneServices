using System.IO;

namespace CargoWise.BizTalk.UnitTestFX
{
	/// <summary>
	/// Used to delegate document comparison to the implementer of this interface
	/// </summary>
	public interface ICompare
	{
		/// <summary>
		/// Execute a document comparision
		/// </summary>
		/// <param name="actual">Document to compare</param>
		/// <param name="expected">Document that <paramref name="actual"/> is expected to be a likeness of</param>
		/// <param name="ignoreComments">ignore the xml comments in comparison</param>
		/// <returns>Success or failure result and detail on failure</returns>
		MapResult Execute(Stream actual, Stream expected, bool ignoreComments = true);
	}
}
