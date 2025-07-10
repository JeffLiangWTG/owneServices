using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions
{
	public static class DataReaderExtensions
	{
		public static void ReadIntoBusinessObject(this GenAddOnDetail genAddOnColumn, bool isDefaultingEnabled, BusinessObject parentBO)
		{
			if (isDefaultingEnabled)
			{
				var setterSuspenderSupporter = parentBO as ISetterSuspenderSupporter;
				using (setterSuspenderSupporter == null ? DisposableAction.NoAction : setterSuspenderSupporter.SetterSuspender.ResumeSetting(genAddOnColumn.PropertyName))
				{
					parentBO[genAddOnColumn.PropertyName] = genAddOnColumn.Value;
				}
			}
			else
			{
				parentBO.SetSystemDefinedValue(genAddOnColumn.GenAddOnColumnName, genAddOnColumn.Value);
			}
		}

		public static bool TryGetMatchedOrganisationData(this IOrganisationDataObjectReaderSupporter supporter, out ZGuid organisationPK, out ZGuid addressPK, IOrganizationAddressCollectionParent dataObject, BusinessObject bizObj, ZString addressType, OrganisationTypes orgCategory, string orgType = null)
		{
			organisationPK = ZGuid.Empty;
			addressPK = ZGuid.Empty;
			var result = false;
			OrgAddress addressBO;
			if (supporter.TryGetMatchedOrganisation(out addressBO, dataObject, bizObj, addressType, orgCategory, orgType))
			{
				result = true;
				if (addressBO != null)
				{
					var addressRow = (IColumnIndexer)((IBusinessObjectInternals)addressBO).Row;
					organisationPK = addressRow.GetValue(OrgAddressSchema.OA_OH);
					addressPK = addressRow.GetValue(OrgAddressSchema.PK);
				}
			}
			return result;
		}

		public static bool TryGetMatchedOrganisation(this IOrganisationDataObjectReaderSupporter supporter, out OrgAddress addressBO, IOrganizationAddressCollectionParent dataObject, BusinessObject bizObj, ZString addressType, OrganisationTypes orgCategory, string orgType = null)
		{
			bool result = false;
			addressBO = null;
			if (dataObject.OrganizationAddressCollection != null)
			{
				var addressData = dataObject.OrganizationAddressCollection.FirstOrDefault(addressType);
				if (addressData != null)
				{
					result = true;
					addressBO = supporter.CreateNewReader(addressData).GetMatched(bizObj, orgCategory, orgType);
				}
			}
			return result;
		}

		public static ZString GetCustomsBillType(this WayBillType billType)
		{
			ZString result = billType.GetCodeAsUpperCase();
			switch (result)
			{
				case WayBillTypeList.Codes.Master:
					result = BillTypeList.Codes.MasterBill;
					break;
				case WayBillTypeList.Codes.House:
					result = BillTypeList.Codes.HouseBill;
					break;
				case WayBillTypeList.Codes.SubHouse:
					result = BillTypeList.Codes.SubHouseBill;
					break;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters")]
		public static ZString GetTargetCountryCode(this ITopLevelDataObject dataObject)
		{
			return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static ZString GetSourceCountryCode(this ITopLevelDataObject dataObject)
		{
			var countryCode = dataObject.DataContext == null ? ZString.Empty : dataObject.DataContext.CountryCodeToImportInto;
			countryCode = countryCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryCode;
			return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
		}

		public static string GetDataProviderForCodeMapping(this Shipment dataObject)
		{
			var dataContext = dataObject.DataContext;
			return dataContext != null ? dataContext.DataProviderForCodeMapping : null;
		}

		public static ZGuid GetBranchPK(this Shipment dataObject, BusinessObjectFactory factory)
		{
			var result = ZGuid.Empty;

			var branchCode = dataObject.Branch.GetCodeAsUpperCase();
			if (branchCode.IsEmpty && dataObject.DataContext is IDataContextDataObject dataContext)
			{
				branchCode = dataContext.EventBranchCode;
			}

			if (!branchCode.IsEmpty)
			{
				result = factory.GetCachedValue("CustomsDeclarationBranch" + branchCode, delegate
				{
					var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					branchQuery.AddToFilter(GlbBranchSchema.GB_Code, branchCode);
					var branch = factory.LoadTop1<GlbBranch>(branchQuery);
					return branch == null ? ZGuid.Invalid : branch.PK;
				});
			}

			return result;
		}

		public static IEnumerable<DataRow> FindCusDecHouseBillByBillNumberAndType(this RowFactory rowFactory, ZGuid declarationPK, ZString billNumber, ZString billType, Func<DataRow, bool> additionalMatch = null)
		{
			var query = new ZQuery(CusDecHouseBillSchema.CU_JE, declarationPK);
			query.AddToFilter(CusDecHouseBillSchema.CU_BillNum, billNumber);
			query.AddToFilter(CusDecHouseBillSchema.CU_BillType, billType);
			var rows = rowFactory.Load(CusDecHouseBillSchema.Constants.TableName, query);
			return rows.Where(x => additionalMatch == null || additionalMatch(x)).ToList();
		}

		public static DataRow FindFirstCusDecHouseBillByBillNumberAndType(this RowFactory rowFactory, ZGuid declarationPK, ZString billNumber, ZString billType, Func<DataRow, bool> additionalMatch = null)
		{
			return FindCusDecHouseBillByBillNumberAndType(rowFactory, declarationPK, billNumber, billType, additionalMatch).FirstOrDefault();
		}

		public static void ClearUnmatchedOrgDetailsNotes(this UniversalObjectFactory factory, ZGuid parentPK, string parentTableName, bool isInDatabase = true)
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, parentPK);
			query.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Code);
			query.AddToFilter(StmNoteSchema.ST_Table, parentTableName);
			query.FetchOnlyFromLocalCache = !isInDatabase;
			factory.Load<StmNote>(query).DeleteAll(true);
		}

		public static ZString? GetResponsiblePartyID(this Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			ZString? result = null;
			if (shipment.AdditionalReferenceCollection != null)
			{
				var responsiblePartyIDData = shipment.AdditionalReferenceCollection.FirstOrDefault(
					reference => reference.Type.GetCodeAsUpperCase() == Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID);
				if (responsiblePartyIDData != null)
				{
					result = responsiblePartyIDData.ReferenceNumber.GetValueOrDefault();
				}
			}
			if (!result.HasValue && shipment.OrganizationAddressCollection != null)
			{
				var responsiblePartyAddress = shipment.OrganizationAddressCollection.FirstOrDefault(AddressTypes.ResponsibleParty);
				if (responsiblePartyAddress != null)
				{
					var responsibleParty = new OrganisationDataObjectReader(responsiblePartyAddress, logger, factory).GetMatched();
					if (responsibleParty != null && responsibleParty.OA_OH != OrgHeader.UnmatchedOrganisationPK)
					{
						result = responsibleParty.Header.PrimaryRegistrationNumber.Number;
					}
				}
			}
			if (!result.HasValue)
			{
				var branchPK = shipment.GetBranchPK(factory.BOFactory);
				if (branchPK.IsValid)
				{
					var branch = factory.Load<GlbBranch>(branchPK);
					if (branch != null)
					{
						result = branch.Company.OrgProxy.PrimaryRegistrationNumber.Number;
					}
				}
			}
			return result;
		}
	}
}
