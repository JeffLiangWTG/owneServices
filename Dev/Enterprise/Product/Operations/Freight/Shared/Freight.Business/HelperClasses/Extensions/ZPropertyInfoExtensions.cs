using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Extensions
{
	public static class ZPropertyInfoExtensions
	{
		public static void ValidateVesselIsValid(this ZPropertyInfo info, Func<RefVessel> getVessel)
		{
			var value = info.Value;
			if (!value.IsEmpty)
			{
				var vessel = getVessel();
				var shouldShowError = FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.Value;
				if (shouldShowError)
				{
					if (vessel == null)
					{
						info.AddError(ResString.GetMultilingualString("ca2e1c73-552a-4340-8992-09407165566a", "Please enter a valid Vessel."));
					}
					else if (!vessel.RV_IsActive)
					{
						info.AddError(ResString.GetMultilingualString("96706dd8-4a3a-4bc4-9b92-763c2f74fff6", "This Vessel is inactive."));
					}
				}
				else
				{
					if (vessel == null)
					{
						info.AddWarning(ResString.GetMultilingualString("e2137939-586b-46ad-b9ce-d892357aabec", "Warning: No reference file for this Vessel was found."));
					}
					else if (!vessel.RV_IsActive)
					{
						info.AddWarning(ResString.GetMultilingualString("a2243ad7-b9f5-4bf5-9bc1-377e447f633b", "Warning: This Vessel is inactive."));
					}
				}
			}
		}
	}
}
