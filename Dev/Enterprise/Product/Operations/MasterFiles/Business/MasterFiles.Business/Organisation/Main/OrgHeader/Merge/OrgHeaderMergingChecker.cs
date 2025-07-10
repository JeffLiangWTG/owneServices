using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgHeaderMergingChecker
	{
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static bool IsAllowedToMergeOrgs(ZGuid retainedOrgPK, ZGuid dissolvedOrgPK, out string reasons)
		{
			reasons = string.Empty;
			var result = true;
			var factory = new BusinessObjectFactory();

			var retainedOrg = factory.Load<OrgHeader>(retainedOrgPK);
			var dissolvedOrg = factory.Load<OrgHeader>(dissolvedOrgPK);

			if (retainedOrg == null || dissolvedOrg == null)
			{
				result = false;
				reasons = Res.GetString("05f53de3-36eb-4dc3-9e62-4dddbc5cc25d", "Unknown organization(s).");
			}

			result = result && CheckPortugalMergingRules(retainedOrg, dissolvedOrg, out reasons);

			result = result && CheckForDraftCommissionAgreements(factory, retainedOrg, dissolvedOrg, out reasons);

			result = result && CheckForDuplicateContracts(retainedOrgPK, dissolvedOrgPK, out reasons);

			return result;
		}

		static bool CheckPortugalMergingRules(OrgHeader retainedOrg, OrgHeader dissolvedOrg, out string reasons)
		{
			reasons = string.Empty;
			var result = true;

			var dissolvedOrgPTIVACustomCodes = GetPTIVAOrgCustomCodes(dissolvedOrg);
			var retainedOrgPTIVACustomCodes = GetPTIVAOrgCustomCodes(retainedOrg);

			if (dissolvedOrgPTIVACustomCodes.Any())
			{
				if (!retainedOrgPTIVACustomCodes.Select(r => r.OK_CustomsRegNo).ContainsAll(dissolvedOrgPTIVACustomCodes.Select(r => r.OK_CustomsRegNo)))
				{
					if (dissolvedOrgPTIVACustomCodes.Any(c => c.IsThereAnyTransactionForOrgWithRestrictedCusCode()))
					{
						if (retainedOrgPTIVACustomCodes.Any(c => c.IsThereAnyTransactionForOrgWithRestrictedCusCode()))
						{
							result = false;
							reasons = Res.GetString("fabc5c9e-1326-4e96-9f37-84531cb94f63", "The organizations have different PT IVA registration codes and posted transactions linked to these codes. Please mark the organization to dissolve as inactive.");
						}
						else
						{
							result = false;
							reasons = Res.GetString("7a30df89-f4a2-4a8c-bb95-a2150f3d41c5", "The retained organization does not have posted transaction. The dissolved organization has posted transactions. Please, merge into the organization that has transactions.");
						}
					}
				}
			}
			return result;
		}

		static IEnumerable<OrgCusCode> GetPTIVAOrgCustomCodes(OrgHeader header)
		{
			return header.CustomsCodes.Cast<OrgCusCode>().Where(
				c => c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Portugal &&
				c.OK_CodeType == OrgCusCode.CodeTypes.IVA &&
				c.OK_CustomsRegNo != AccountingCountrySpecificValidationHelper.EmptyPortugalIVA);
		}

		static bool ContainsAll<T>(this IEnumerable<T> containingList, IEnumerable<T> lookupList)
		{
			return !lookupList.Except(containingList).Any();
		}

		static bool CheckForDraftCommissionAgreements(BusinessObjectFactory factory, OrgHeader retainedOrg, OrgHeader dissolvedOrg, out string reasons)
		{
			factory.RefreshEnabled = false;
			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));

			var opportunitySubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
			opportunitySubQuery.AddToFilter(OrgOpportunitySchema.P8_OH, new ZGuid[] { retainedOrg.PK, dissolvedOrg.PK });

			query.AddSubQuery(OrgCommissionAgreementSchema.CA0_P8, opportunitySubQuery, JoinCondition.And);
			query.AddToFilter(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, SQLComparisonOperator.Equal, DBNull.Value);

			var agreements = factory.Load<OrgCommissionAgreement>(query);
			if (agreements.Length == 0)
			{
				reasons = string.Empty;
				return true;
			}

			var agreementIds = string.Join(", ", agreements.Select(a => a.AgreementId));

			reasons = Res.GetString("666406a8-580a-474a-94fa-bee5eab2b56a", "There are pending commission agreements for the merging organizations, {0} and {1}. These must be approved or disapproved prior to merge. These are: {2}", retainedOrg.OH_Code, dissolvedOrg.OH_Code, agreementIds);
			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "MergingChecker uses complex SQL scripts that can't be accomplished by using Business Objects")]
		static bool CheckForDuplicateContracts(ZGuid retainedOrgPK, ZGuid dissolvedOrgPK, out string reasons)
		{
			reasons = string.Empty;

			var sql = @"SELECT OLD.RCT_ContractNumber, OLD.RCT_ContractType
FROM (SELECT * FROM dbo.RatingContract WHERE RCT_OH = @oldPK) AS OLD,
(SELECT * FROM dbo.RatingContract WHERE RCT_OH = @newPK) AS NEW
WHERE OLD.RCT_ContractNumber = NEW.RCT_ContractNumber";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@newPK", SqlDbType.UniqueIdentifier, retainedOrgPK.ToGuid());
				command.AddParameter("@oldPK", SqlDbType.UniqueIdentifier, dissolvedOrgPK.ToGuid());

				var factory = new BusinessObjectFactory();
				var retainedOrg = factory.Load<OrgHeader>(retainedOrgPK);
				var dissolvedOrg = factory.Load<OrgHeader>(dissolvedOrgPK);

				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						var contractNumber = reader.GetString(0);
						var contractType = reader.GetString(1);
						var carrierOrClient = contractType == "PRO" ? "Carrier" : "Client";

						reasons = Res.GetString("cc2df059-f0d6-4764-8762-20e2c4a49dfb", "Cannot merge organizations since duplicate Contract ID {0} was found under {1} Contracts & Allocations of {2} and {3}.", contractNumber, carrierOrClient, dissolvedOrg.OH_Code, retainedOrg.OH_Code);
						return false;
					}
				}
			}

			return true;
		}
	}
}
