using CargoWise.Types;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class UniversalPackingLineExtension
	{
		public static bool HasInnerPackingLines(this UniversalPackingLine packingLine)
		{
			return packingLine?.PackingLineCollection != null && packingLine.PackingLineCollection.Count > 0;
		}

		public static ZDateTime GetLastKnownTransitWarehouseStatusDateTime(this UniversalPackingLine packingLine)
		{
			var loadDate = packingLine.LoadDate.GetValueOrDefault();
			return loadDate.IsEmpty ? packingLine.UnloadDate.GetValueOrDefault() : loadDate;
		}

		/// <summary>
		/// PackageID should be unique to the Job and only allowed when QTY = 1
		/// PacklineID should also be unique to the Job and should be used when PackageID can't
		/// </summary>
		/// <param name="packingLine"></param>
		/// <returns></returns>
		public static ZString GetPackageIDWithFallbackToPacklineID(this UniversalPackingLine packingLine)
		{
			var packageID = packingLine.ReferenceNumber.GetValueOrDefault();
			return !packageID.IsEmpty ? packageID : packingLine.PackingLineID.GetValueOrDefault();
		}
	}
}
