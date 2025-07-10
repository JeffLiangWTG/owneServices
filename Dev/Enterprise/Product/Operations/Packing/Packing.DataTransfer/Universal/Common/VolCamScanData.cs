using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Packing.DataTransfer.Universal.UnitHelper;
using EventConstants = CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public static class VolCamScanData
	{
		public static ZString GetBarcode(IXmlEventValueObject valueObject) => valueObject.Context.GoodsItemID;

		public static ZString GetWarehouseCode(IXmlEventValueObject valueObject) => valueObject.Context.WarehouseCode;

		public static void PopulatePackageFromEvent(UniversalEvent valueObject, PkgPackage package)
		{
			var context = ((IXmlEventValueObject)valueObject).Context;

			if (context.FailureReason.IsEmpty)
			{
				var referenceParameters = StmALog.GetParametersFromReference(valueObject.EventReference);

				referenceParameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Length, out var rawLength);
				referenceParameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Width, out var rawWidth);
				referenceParameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Height, out var rawHeight);

				referenceParameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Weight, out var rawWeight);
				referenceParameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Volume, out var rawVolume);

				var weight = new ZWeight(GetValueFromValueAndUnit(rawWeight), GetUnitFromValueAndUnit(rawWeight));
				var volume = new ZVolume(GetValueFromValueAndUnit(rawVolume), GetUnitFromValueAndUnit(rawVolume));

				var width = GetValueFromValueAndUnit(rawWidth);
				var widthUQ = GetUnitFromValueAndUnit(rawWidth);

				var height = GetValueFromValueAndUnit(rawHeight);
				var heightUQ = GetUnitFromValueAndUnit(rawHeight);

				var length = GetValueFromValueAndUnit(rawLength);
				var lengthUQ = GetUnitFromValueAndUnit(rawLength);

				if (weight.IsValid)
				{
					package.KP_Weight = weight.Amount;
					package.KP_WeightUQ = weight.Unit;
				}

				var isDimensionsValidUQ = (widthUQ == heightUQ && heightUQ == lengthUQ) && Core.Constants.Length.ContainsCode(widthUQ);
				var isDimensionsValid = isDimensionsValidUQ &&
					width > 0m && length > 0m && height > 0m;

				if (isDimensionsValid)
				{
					package.KP_Width = width;
					package.KP_Length = length;
					package.KP_Height = height;
					package.KP_DimensionUQ = widthUQ;
				}

				if (volume.IsValid)
				{
					package.KP_Volume = volume.Amount;
					package.KP_VolumeUQ = volume.Unit;
				}
			}
		}
	}
}
