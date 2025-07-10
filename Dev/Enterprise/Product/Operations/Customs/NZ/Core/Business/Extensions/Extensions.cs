using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business
{
	public static class Extensions
	{
		public static ZString GetApprovedTransitionalFacilityCode(this OrgAddress address)
		{
			var org = address?.Header;

			var codes = org?.CustomsCodes.Find(c => c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.NewZealand && c.OK_CodeType == HeaderOtherInfoList.Codes.ApprovedTransitionalFacility);
			var code = codes?.FirstOrDefault(c => c.OK_OA_PremisesAddress == address.PK) ?? codes?.FirstOrDefault(c => c.OK_OA_PremisesAddress.IsEmpty);
			return code?.OK_CustomsRegNo ?? ZString.Empty;
		}

		public static ZString GetHumanReadableName(this ZPropertyInfo propertyInfo)
		{
			return propertyInfo?.HasHumanReadableName ?? false ? propertyInfo.HumanReadableName.ToString() : @"value";
		}

		public static ZString GetResponsiblePartyPremiseID(this OrgAddress orgAddress)
		{
			var addressPremiseID = orgAddress?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
			if (addressPremiseID.IsEmpty)
			{
				addressPremiseID = orgAddress?.Header?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, Core.Constants.CountryCodes.NewZealand, ZGuid.Empty) ?? ZString.Empty;
			}
			return addressPremiseID;
		}

		public static ZString GetCCPOrATFCode(this ZAddress address)
		{
			return NZ.TradeSingleWindow.Extensions.GetCCPOrATFCode(address?.OrgAddress as OrgAddress, address?.OrgHeader as OrgHeader);
		}

		public static CusSCAHouse LoadLinkedCusSCAHouse(this ForwardingShipment shipment)
		{
			var oceanBillSubQuery = new ZDBOnlySubQuery(typeof(CusSCAOceanBill), CusSCAOceanBillSchema.PK);
			oceanBillSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff);
			var query = new ZDBOnlyQuery(typeof(CusSCAHouse));
			query.AddToFilter(CusSCAHouseSchema.CA_JS, shipment.PK);
			query.AddSubQuery(CusSCAHouseSchema.CA_CB, oceanBillSubQuery, JoinCondition.And);

			return shipment.Factory.LoadTop1<CusSCAHouse>(query);
		}

		public static ZString RemoveConsolidatedStatus(this ZString entryStatus)
		{
			var result = entryStatus;
			if (new ConsolidatedEntryStatusList().ContainsCode(entryStatus))
			{
				result = FormalEntryStatusList.Codes.NotSentToCustoms;
			}

			return result;
		}
	}
}
