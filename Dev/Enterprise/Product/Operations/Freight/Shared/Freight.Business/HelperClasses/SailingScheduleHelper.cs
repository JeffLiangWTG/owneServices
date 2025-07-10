using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public static class SailingScheduleHelper
	{
		public static ZGuid GetLineOperatorPKFromExternalCode(ZString externalCode, ZString dataProvider, BusinessObjectFactory factory)
		{
			switch (dataProvider)
			{
				case FreightConstants.VesselDataProviders.DAKOSY:

					var org_DAKOSY = GetLineOperator(factory, OrgCusCode.CodeTypes.CarrierCode, externalCode)
									 ?? GetLineOperator(factory, GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, externalCode);

					return (org_DAKOSY == null) ? ZGuid.Empty : org_DAKOSY.PK;

				case FreightConstants.VesselDataProviders.OneStop:

					var org_1ST = GetLineOperator(factory, OrgCusCode.CodeTypes.OneStopCode, externalCode);

					return (org_1ST == null) ? ZGuid.Empty : org_1ST.PK;

				default:
					return ZGuid.Empty;
			}
		}

		static OrgHeader GetLineOperator(BusinessObjectFactory factory, ZString codeType, ZString code)
		{
			var cusCodeQuery = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			cusCodeQuery.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_CustomsRegNo, code);

			if (codeType == GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode)
			{
				cusCodeQuery.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Germany);
			}
			else if (codeType == OrgCusCode.CodeTypes.OneStopCode)
			{
				cusCodeQuery.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Australia);
			}

			var orgCusCodes = factory.Load<OrgCusCode>(cusCodeQuery);

			var activeOrgs = from OrgCusCode cusCode in orgCusCodes
							 where cusCode.Organisation != null && cusCode.Organisation.OH_IsActive
							 select cusCode.Organisation;

			return activeOrgs.FirstOrDefault();
		}

		public static CodeDescriptionPairList GetDataSourceList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(FreightConstants.VesselDataProviders.OneStop, FreightConstants.VesselDataProviderNames.OneStop);
			list.AddPair(FreightConstants.VesselDataProviders.DBH, FreightConstants.VesselDataProviderNames.DBH);
			list.AddPair(FreightConstants.VesselDataProviders.DAKOSY, FreightConstants.VesselDataProviderNames.DAKOSY);
			return list;
		}
	}
}
