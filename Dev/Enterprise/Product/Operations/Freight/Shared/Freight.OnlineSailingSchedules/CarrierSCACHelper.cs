using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	static class CarrierSCACHelper
	{
		const int SCACCodeLength = 4;

		public static void CheckCarrierSCAC(ZPropertyInfo carrierSCACInfo, BusinessObjectFactory factory)
		{
			var carrierSCAC = (ZString)carrierSCACInfo.Value;

			if (string.IsNullOrEmpty(carrierSCAC))
			{
				carrierSCACInfo.AddError(ResString.GetMultilingualString("732AF5B1-DE03-4A28-BC02-56FF9190A9EC",
					"Carrier SCAC must not be empty."));
			}
			else
			{
				if (carrierSCAC.Length != SCACCodeLength)
				{
					carrierSCACInfo.AddError(ResString.GetMultilingualString("4F614083-DA68-4BCC-8BE8-66FC3D3552EC",
						"SCAC code must consist of {0} symbols.", SCACCodeLength));
				}

				var orgCusCodes = GetExistingOrgCustCodes(carrierSCAC, factory);

				if (orgCusCodes.Length == 0)
				{
					carrierSCACInfo.AddError(ResString.GetMultilingualString("E7F3FC7C-B1C8-4637-BD35-134FBFFDB347",
						"No organization found for SCAC {0}.", carrierSCAC));
				}
				else if (orgCusCodes.Length > 1)
				{
					carrierSCACInfo.AddError(ResString.GetMultilingualString("785C80DD-25DC-400D-BE97-23C4B57337D4",
						"Multiple organizations with SCAC {0} found.", carrierSCAC));
				}
			}
		}

		public static OrgCusCode[] GetExistingOrgCustCodes(ZString carrierSCAC, BusinessObjectFactory factory)
		{
			var orgCusCodeQuery = new ZDBOnlyQuery(typeof(OrgCusCode));
			orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
			orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, carrierSCAC);
			orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			orgCusCodeQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, orgHeaderSubQuery, JoinCondition.And);

			return factory.Load<OrgCusCode>(orgCusCodeQuery);
		}
	}
}
