using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	public class GPSCartageLegUpdater_Radius_JobDocAddressOverrideTest : GPSCartageLegUpdater_RadiusTest
	{
		protected override CommonCartageLeg CreateAndDispatchLegWithPickupDeliveryTime(OrgHeader fromAddress, OrgHeader waitAddress, OrgHeader toAddress, CommonWorkSheet workSheet, ZDateTime plannedPickupAndDispatchedTime, ZGuid truckPK)
		{
			var leg = HelperGPS.CreateAndDispatchLegWithPickupDeliveryTime(fromAddress, waitAddress, toAddress, workSheet, plannedPickupAndDispatchedTime, truckPK);
			OverrideAddress(leg.PickupFromDocAddress);
			OverrideAddress(leg.WaitPointDocAddress);
			OverrideAddress(leg.DeliverToDocAddress);
			return leg;
		}

		protected override ZString GetAddressCode(OrgHeader org)
		{
			var freeTextAddress = new ZStringBuilder();
			freeTextAddress.AppendIfNotEmpty(org.MainAddress.OA_Address1);
			freeTextAddress.AppendIfNotEmpty(org.MainAddress.OA_Address2);
			freeTextAddress.AppendIfNotEmpty(org.MainAddress.OA_City);
			return org.OH_FullName + GPSConstants.FenceSeperator + freeTextAddress.ToStringWithDelimiterBetweenAppends(" ");
		}
	}
}
