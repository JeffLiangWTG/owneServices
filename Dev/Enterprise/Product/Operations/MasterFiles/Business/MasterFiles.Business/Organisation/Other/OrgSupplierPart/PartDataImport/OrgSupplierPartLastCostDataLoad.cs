using System.Collections.Generic;
using System.Globalization;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartLastCostDataLoad : DataLoad
	{
		public OrgSupplierPartLastCostDataLoad()
		{
		}

		public void ImportPartLastCostData(string dataLocation, bool legacyCodes)
		{
			UseLegacyCodes = legacyCodes;
			ImportData(dataLocation, (NoResString)"Part Last Cost");
		}

		#region ImportFrom .csv file

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			if (ValidateLineAndDisplayError(line))
			{
				ExtractPartLastCostData(line);
				UpdatePartLastCost();
			}
			else
			{
				RunCounters.RecsExcluded++;
			}

			OnProgressChanged();
		}

		protected override void OutputFinalTotals(string dataType)
		{
			DisplayLogMessage("\r\n" + Res.GetString("14e4b877-2597-451f-baed-64444fad6d94", "T O T A L : Products Updated = {0}, {1} Data Records Excluded = {2}", RunCounters.RecsUpdated, dataType, RunCounters.RecsExcluded) + "\r\n");
		}

		bool EmptyLine(OCsvLine line)
		{
			return line.FieldValues.Length == 0;
		}

		protected bool ValidateLineAndDisplayError(OCsvLine line)
		{
			var isValid = false;
			if (EmptyLine(line))
			{
				DisplayLogMessage(Res.GetString("011f81ca-e685-4c6f-becc-890679608658", "Empty line is ignored and not processed. Row {0}.", RunCounters.CurrentRow.ToString(CultureInfo.InvariantCulture)));
			}
			else
			{
				if (line.FieldValues.Length != 4)
				{
					DisplayLogMessage(Res.GetString("bde5a224-5f15-45c9-aa5e-25bc90ed8b39", "File contains inconsistent data. Row {0} contains the following data : {1}", RunCounters.CurrentRow.ToString(CultureInfo.InvariantCulture), line.ToString()));
				}
				else
				{
					if (!ZDecimal.TryParse(line.FieldValues[3], out PartLastCost))
					{
						DisplayLogMessage(Res.GetString("3ec7e917-838f-4a2f-a837-99056ff8766a", "Last Cost is not valid. Row {0} contains the following data : {1} Last Cost: {2}", RunCounters.CurrentRow.ToString(CultureInfo.InvariantCulture), line.ToString(), line.FieldValues[3]));
					}
					else
					{
						isValid = true;
					}
				}
			}
			return isValid;
		}

		protected virtual void ExtractPartLastCostData(OCsvLine line)
		{
			PartNo = new ZString(line.FieldValues[0]).Trim().SubstringSafe(0, 35);
			PartOwnerCode = new ZString(line.FieldValues[1]).Trim();
			PartSupplierCode = new ZString(line.FieldValues[2]).Trim();
		}

		void UpdatePartLastCost()
		{
			GetOwnerAndSupplierPKs();
			if (PartHasValidOwnerOrSupplier)
			{
				OrgSupplierPart enterpriseProduct = GetPartIfItExists(PartNo, PartOwnerCodePK, PartSupplierCodePK);
				if (enterpriseProduct != null)
				{
					enterpriseProduct.OP_LastCost = PartLastCost;
					Factory.Save();
					RunCounters.RecsUpdated++;
				}
				else
				{
					RunCounters.RecsExcluded++;
					DisplayFormattedLogMessage(PartNo, Res.GetString("42e71fa5-05e8-49a1-a5af-1654a8982c6f", "Product record not found in {0}", BrandingFactory.Instance.ProductName));
				}
			}
			else
			{
				RunCounters.RecsExcluded++;
				DisplayFormattedLogMessage(PartNo, Res.GetString("bf76b131-782e-4594-9525-c1cbd8110c56", "Owner &/or Supplier not found"));
			}
		}

		OrgSupplierPart GetPartIfItExists(ZString partNo, ZGuid ownerPK, ZGuid supplierPK)
		{
			OrgSupplierPart enterprisePart = null;
			var partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, partNo);
			partFilter.OrderBy = OrgSupplierPartSchema.Constants.OP_IsActive + OrderByClause.Descending;

			var enterpriseParts = Factory.Load<OrgSupplierPart>(partFilter);

			foreach (OrgSupplierPart checkPart in enterpriseParts)  // check owner & supplier
			{
				var partOwner = Factory.LoadTop1<OrgPartRelation>(GetPartFilter(checkPart, ownerPK, OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both));

				if (partOwner == null) // Check if part exists for supplier
				{
					var partSupplier = Factory.LoadTop1<OrgPartRelation>(GetPartFilter(checkPart, supplierPK, OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelation.RelationshipTypes.Both));

					if (partSupplier != null)
					{
						enterprisePart = checkPart;
						break;
					}
				}
				else if (supplierPK.IsEmpty) // part exists for this owner only
				{
					enterprisePart = checkPart;
					break;
				}
				else // check if part exists for owner & supplier
				{
					var partSupplier = Factory.LoadTop1<OrgPartRelation>(GetPartFilter(checkPart, supplierPK, OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelation.RelationshipTypes.Both));

					if (partSupplier != null)
					{
						enterprisePart = checkPart;
						break;
					}
				}
			}

			return enterprisePart;
		}

		ZQuery GetPartFilter(OrgSupplierPart part, ZGuid organisationPK, params ZString[] relationship)
		{
			var filter = new ZQuery(OrgPartRelationSchema.OU_OP, part.PK);
			filter.AddToFilter(OrgPartRelationSchema.OU_OH, organisationPK);
			filter.AddToFilter(OrgPartRelationSchema.OU_Relationship, relationship);
			return filter;
		}

		protected virtual bool ArePartLastCostDetailsValid()
		{
			bool detailsAreValid = false;
			if (!PartOwnerCode.IsEmpty && !PartNo.IsEmpty)
			{
				detailsAreValid = true;
			}

			return detailsAreValid;
		}

		void DisplayFormattedLogMessage(ZString partNo, string detailedExceptionMessage)
		{
			var ouputRowNo = Res.GetString("b40fa4bf-d078-4061-89bd-3f318ba3f30f", "Line {0}:", RunCounters.CurrentRow.ToString());
			var logMessage = Res.GetString("bc5e1333-abca-4bbc-9158-704dd747a131", "{0} PART NO: {1}  {2}", ouputRowNo, partNo, detailedExceptionMessage);

			DisplayLogMessage(logMessage);
		}

		#endregion

		#region Validation

		public override string CSVTemplateHeading
		{
			get { return string.Join(",", CSVTemplateHeaders); }
		}

		IEnumerable<string> CSVTemplateHeaders
		{
			get
			{
				yield return "CODE";
				yield return "OWNER";
				yield return "SUPPLIER";
				yield return "LAST_COST";
			}
		}

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			return line.FieldValues.Length == 4
				&& line.FieldValues[0].ToUpper() == "CODE"
				&& line.FieldValues[1].ToUpper() == "OWNER"
				&& line.FieldValues[2].ToUpper() == "SUPPLIER"
				&& line.FieldValues[3].ToUpper() == "LAST_COST";
		}

		#endregion

		#region Owner Supplier Validation

		void GetOwnerAndSupplierPKs()
		{
			PartHasValidOwnerOrSupplier = true;
			PartOwnerCodePK = ZGuid.Empty;
			PartSupplierCodePK = ZGuid.Empty;

			if (PartOwnerCode.IsEmpty && PartSupplierCode.IsEmpty)
			{
				PartHasValidOwnerOrSupplier = false;
			}

			if (!PartOwnerCode.IsEmpty)
			{
				if (PartOwnerCode == SavedPartOwnerCode)
				{
					PartOwnerCodePK = SavedPartOwnerCodePK;
				}
				else
				{
					PartOwnerCodePK = SavedPartOwnerCodePK = GetOwnerSupplierCodePK(PartOwnerCode);
					SavedPartOwnerCode = PartOwnerCode;
				}
			}

			if (!PartSupplierCode.IsEmpty)
			{
				if (PartSupplierCode == SavedPartSupplierCode)
				{
					PartSupplierCodePK = SavedPartSupplierCodePK;
				}
				else
				{
					PartSupplierCodePK = SavedPartSupplierCodePK = GetOwnerSupplierCodePK(PartSupplierCode);
					SavedPartSupplierCode = PartSupplierCode;
				}
			}

			if (PartOwnerCodePK.IsEmpty && PartSupplierCodePK.IsEmpty)
			{
				PartHasValidOwnerOrSupplier = false;
			}
		}

		ZGuid GetOwnerSupplierCodePK(string ownerSupplierCode)
		{
			if (UseLegacyCodes)
			{
				return GetOwnerSupplierCodeFromLegacyCode(ownerSupplierCode);
			}
			else
			{
				OrgHeader ownerSupplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, ownerSupplierCode);
				return (ownerSupplier != null) ? ownerSupplier.PK : ZGuid.Empty;
			}
		}

		ZGuid GetOwnerSupplierCodeFromLegacyCode(string ownerSupplierCode)
		{
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, ownerSupplierCode);
			OrgCusCode orgLegacyCodeRecord = Factory.LoadTop1<OrgCusCode>(codeFilter);
			return (orgLegacyCodeRecord != null) ? orgLegacyCodeRecord.OK_OH : ZGuid.Empty;
		}

		#endregion

		#region PartLastCostData

		ZString PartNo;
		ZString PartOwnerCode;
		ZString PartSupplierCode;
		ZDecimal PartLastCost;
		ZGuid PartOwnerCodePK;
		ZGuid PartSupplierCodePK;
		ZBool PartHasValidOwnerOrSupplier;
		ZString SavedPartOwnerCode;
		ZGuid SavedPartOwnerCodePK;
		ZString SavedPartSupplierCode;
		ZGuid SavedPartSupplierCodePK;

		#endregion

		bool UseLegacyCodes;
	}
}
