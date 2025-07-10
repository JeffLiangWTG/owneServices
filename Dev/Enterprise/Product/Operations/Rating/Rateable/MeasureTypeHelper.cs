using System;
using System.Linq;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Helper for the MeasureType enum
	/// </summary>
	internal static class MeasureTypeHelper
	{
		/// <summary>
		/// The maximum integer value found in the MeasureType enum
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static int MeasureTypeMaxValue = GetMeasureTypeMaxValue();
		static int GetMeasureTypeMaxValue()
			=> Enum.GetValues(typeof(MeasureType)).Cast<int>().Max();
	}
}
