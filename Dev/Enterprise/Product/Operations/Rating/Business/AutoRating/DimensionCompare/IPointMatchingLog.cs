using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Interface for adding to the PointMatchingLog
	/// </summary>
	public interface IPointMatchingLog
	{
		/// <summary>
		/// Log a mismatch between part and line.
		/// </summary>
		/// <param name="lineDisplayInfo">line display info. Typically the output of line.DisplayInfo()</param>
		/// <param name="partName">part name. Can be set to the result from GetPartName()</param>
		/// <param name="dimensionName">dimension that didn't match</param>
		/// <param name="partValueAsText">part value for the dimension</param>
		/// <param name="lineValueAsText">line value for the dimension</param>
		void LogPartDidNotMatchLine(string lineDisplayInfo, string partName, string dimensionName, string partValueAsText, string lineValueAsText);

		/// <summary>
		/// Determine a part's name.
		/// The logic is here, since that was how it was originally implemented.
		/// If it needs extending to support more parts then the more scalable design is to put it in IRateablePartList itself.
		/// </summary>
		string GetPartName(IHasPartDimensions partList, IRateablePart part);

		/// <summary>
		/// Factory to use in converting PKs to codes, etc.
		/// </summary>
		BusinessObjectFactory Factory { get; }

		/// <summary>
		/// Current MeasureType being matched. Affects how values are converted to text.
		/// </summary>
		MeasureType CurrentMeasureType { get; }
	}
}
