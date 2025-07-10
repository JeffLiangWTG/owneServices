using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Core;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public static class UQHelper
	{
		public static List<ICodeDescription> VolumeUQList
		{
			get { return volumeUQList ?? (volumeUQList = new StringListWrapperToICodeDescription(Constants.Volume.Codes)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "This is a testing tool.")]
		static List<ICodeDescription> volumeUQList;

		public static List<ICodeDescription> WeightUQList
		{
			get { return weightUQList ?? (weightUQList = new StringListWrapperToICodeDescription(Constants.Weight.Codes)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "This is a testing tool.")]
		static List<ICodeDescription> weightUQList;

		public static List<ICodeDescription> DimensionUQList
		{
			get { return dimensionUQList ?? (dimensionUQList = new StringListWrapperToICodeDescription(Constants.Length.Codes)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "This is a testing tool.")]
		static List<ICodeDescription> dimensionUQList;
	}
}

