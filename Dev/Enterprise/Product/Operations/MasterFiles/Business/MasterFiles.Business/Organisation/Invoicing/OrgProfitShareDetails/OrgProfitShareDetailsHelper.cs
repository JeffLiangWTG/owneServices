using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgProfitShareDetailsHelper
	{
		public OrgProfitShareDetailsHelper(BusinessObjectFactory factory, AccChargeCode[] userDefinedChargeCodes = null)
		{
			Argument.NotNull(factory, nameof(factory));

			this.factory = factory;
			this.userDefinedChargeCodes = userDefinedChargeCodes;
		}

		readonly BusinessObjectFactory factory;
		readonly AccChargeCode[] userDefinedChargeCodes;

		/// <summary>
		/// Is the charge code group passed in relevant to the current Profit Share Agreement.
		/// </summary>
		public bool IsChargeGroupProfitShared(string agreementType, AccChargeCode chargeCode, PaymentTermInfos paymentTerm)
		{
			switch (agreementType)
			{
				case OrgProfitShareDetailsLookups.AgreementTypeAll:
					return true;

				case OrgProfitShareDetailsLookups.AgreementTypeFreight:
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight;

				case OrgProfitShareDetailsLookups.AgreementTypeCollectFreight:

					var prepaidCollect = paymentTerm.GetPrepaidCollect(CostSell.Revenue, ChargeCodeGroupList.Codes.Freight);
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight && prepaidCollect == Constants.PaymentType.Collect;

				case OrgProfitShareDetailsLookups.AgreementTypeFreightChargeOnly:
					return chargeCode.PK == Env.Registry.FreightChargeCode;

				case OrgProfitShareDetailsLookups.AgreementTypeOrigin:
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Origin ||
						chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Loading;

				case OrgProfitShareDetailsLookups.AgreementTypeDestination:
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Destination ||
						chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Unloading;

				case OrgProfitShareDetailsLookups.AgreementTypeFreightOrigin:
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight ||
						chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Origin;

				case OrgProfitShareDetailsLookups.AgreementTypeFreightDestination:
					return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight ||
						chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Destination;

				case OrgProfitShareDetailsLookups.AgreementTypeCustom:
					return Array.Exists(OrganisationRegistry.Instance.CustomProfitShareAgreementTypeChargeCodes.GetAsGuidArray(),
						x => x == chargeCode.PK);

				case OrgProfitShareDetailsLookups.AgreementTypeUserDefined:
					var result = userDefinedChargeCodes != null && userDefinedChargeCodes.Any(x => x.PK == chargeCode.PK);

					if (!result && userDefinedChargeCodes != null)
					{
						var userDefinedChargeCodesArray = userDefinedChargeCodes.Select(x => x.AC_Code).ToArray();
						result = IsChargeLinkedViaGlobalChargeCode(chargeCode, userDefinedChargeCodes)
							  || (chargeCode.AC_GC == GlbCompany.CurrentCompany.PK && (IsChargeLinkedViaMatchingCode(chargeCode.AC_Code, userDefinedChargeCodesArray) || IsChargeCodeLinkedViaInterCompanyMappings(chargeCode.PK, userDefinedChargeCodesArray)));
					}

					return result;
			}

			return false;
		}

		bool IsChargeLinkedViaGlobalChargeCode(AccChargeCode chargeCode, AccChargeCode[] chargeCodesToCompare)
		{
			return chargeCode.IsLinkedToGlobalChargeCode && chargeCodesToCompare.Where(x => x.IsLinkedToGlobalChargeCode).SelectMany(x => x.GlobalChargeCode.ChildChargeCodes.Select(c => c.PK)).Contains(chargeCode.PK);
		}

		bool IsChargeLinkedViaMatchingCode(ZString code, ZString[] chargeCodesToCompare)
		{
			return chargeCodesToCompare.Contains(code);
		}

		#region SuppressResourceStringsCheckRegion

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "SQL query doesn't need IFormatting in this case")]
		bool IsChargeCodeLinkedViaInterCompanyMappings(ZGuid chargeCodePK, ZString[] chargeCodesToShareProfitFrom)
		{
			var chargeCodes = new DynamicBusinessObjectCollection(factory);
			var chargeCodesToShareProfitFromAsString = "'" + string.Join("', '", chargeCodesToShareProfitFrom) + "'";

			var yP_AC = AccGlobalChargeCodeMapPivotSchema.Constants.YP_AC;
			var accGlobalChargeCodeMap = AccGlobalChargeCodeMapSchema.Constants.TableName;
			var accGlobalChargeCodeMapPivot = AccGlobalChargeCodeMapPivotSchema.Constants.TableName;
			var yP_YG = AccGlobalChargeCodeMapPivotSchema.Constants.YP_YG;
			var yG_PK = AccGlobalChargeCodeMapSchema.Constants.PK;
			var yG_Code = AccGlobalChargeCodeMapSchema.Constants.YG_Code;
			var yG_IsActive = AccGlobalChargeCodeMapSchema.Constants.YG_IsActive;
			var yG_OH = AccGlobalChargeCodeMapSchema.Constants.YG_OH;
			var yP_OH_LocalClientOverride = AccGlobalChargeCodeMapPivotSchema.Constants.YP_OH_LocalClientOverride;

			var query = $@"
SELECT {yP_AC} 
FROM {accGlobalChargeCodeMapPivot}
JOIN {accGlobalChargeCodeMap} ON {yP_YG} = {yG_PK}
WHERE
	{yP_AC} = '{chargeCodePK}'
	AND {yG_Code} IN ({chargeCodesToShareProfitFromAsString})
	AND {yG_OH} IS NULL
	AND {yP_OH_LocalClientOverride} IS NULL	
	AND {yG_IsActive} = 1	
";

			chargeCodes.Load(query);
			return chargeCodes.Any();
		}

		#endregion
	}
}
