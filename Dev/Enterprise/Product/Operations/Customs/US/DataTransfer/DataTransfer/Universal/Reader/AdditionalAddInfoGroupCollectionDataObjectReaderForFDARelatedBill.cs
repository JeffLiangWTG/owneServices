using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using BillTypeList = Enterprise.Customs.Business.BillTypeList;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AdditionalAddInfoGroupCollectionDataObjectReaderForFDARelatedBill : DataObjectReader, IAdditionalAddInfoGroupCollectionDataObjectReader
	{
		public AdditionalAddInfoGroupCollectionDataObjectReaderForFDARelatedBill(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger)
		{
			this.helper = Argument.NotNull(helper, "helper");
		}
		readonly UniversalDataObjectReaderHelper helper;

		#region IAdditionalAddInfoGroupCollectionDataObjectReader Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Process(IEnumerable<UniversalCustoms.AddInfoGroup> addInfoGroupCollection, ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase)
		{
			var fda = parentTableCode == CusAddInfoSchema.Constants.Prefix ? helper.Load<FDA>(parentPK) : null;
			var fdaRow = GetColumnIndexer(fda);
			var invoiceLineRow = GetColumnIndexer(fdaRow == null ? null : helper.Load<JobComInvoiceLine>(fdaRow, CusAddInfoSchema.B7_ParentID));
			var invoiceRow = GetColumnIndexer(invoiceLineRow == null ? null : helper.Load<JobComInvoiceHeader>(invoiceLineRow, JobComInvoiceLineSchema.JI_JZ));
			var declaration = invoiceRow == null ? null : helper.Load<JobDeclaration>(invoiceRow, JobComInvoiceHeaderSchema.JZ_JE);
			if (declaration != null)
			{
				var declarationPK = declaration.PK;
				var declarationIsInDatabase = declaration.IsInDatabase;
				var fdaIsInDatabase = !fda.IsInDatabase;
				foreach (var addInfoGroup in addInfoGroupCollection)
				{
					ZString? masterBill = addInfoGroup.AddInfoCollection.GetZStringValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.MasterBill, logger);
					ZString? houseBill = addInfoGroup.AddInfoCollection.GetZStringValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.HouseBill, logger);

					if (masterBill.HasValue || houseBill.HasValue)
					{
						ZString? masterBillIssuerCode = addInfoGroup.AddInfoCollection.GetZStringValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.MasterBillIssuerCode, logger);
						ZString? houseBillIssuerCode = addInfoGroup.AddInfoCollection.GetZStringValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedBill.HouseBillIssuerCode, logger);
						// As some Bills might not be in the database yet, we cannot use DBOnlyQuery
						ZQuery houseBillQuery = null;
						ZQuery masterBillQuery = null;
						var matchingCriteriaBuilder = new ZStringBuilder();

						if (houseBill.HasValue)
						{
							houseBillQuery = new ZQuery(CusDecHouseBillSchema.CU_JE, declarationPK);
							matchingCriteriaBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}:{1}{2}", BillTypeList.Codes.HouseBill, houseBillIssuerCode.HasValue ? houseBillIssuerCode.Value + " " : "", houseBill.Value));
							AddBillNumberAndIssuerCodeQuery(houseBillQuery, houseBill.Value, BillTypeList.Codes.HouseBill);
							houseBillQuery.FetchOnlyFromLocalCache = !declarationIsInDatabase;
						}

						if (masterBill.HasValue)
						{
							masterBillQuery = new ZQuery(CusDecHouseBillSchema.CU_JE, declarationPK);
							matchingCriteriaBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}:{1}{2}", BillTypeList.Codes.MasterBill, masterBillIssuerCode.HasValue ? masterBillIssuerCode.Value + " " : "", masterBill.Value));
							AddBillNumberAndIssuerCodeQuery(masterBillQuery, masterBill.Value, BillTypeList.Codes.MasterBill);
							masterBillQuery.FetchOnlyFromLocalCache = !declarationIsInDatabase;
						}

						var bills = System.Array.Empty<Bill>();
						if (houseBillQuery == null)
						{
							bills = GetBills(masterBillQuery, masterBillIssuerCode);
						}
						else
						{
							var houseBills = GetBills(houseBillQuery, houseBillIssuerCode);
							if (houseBills.Length > 0)
							{
								if (masterBillQuery == null)
								{
									bills = houseBills;
								}
								else
								{
									masterBillQuery.AddToFilter(CusDecHouseBillSchema.PK, houseBills.Select(x => x.CU_CU_ParentBill));
									var masterBillPKs = GetBills(masterBillQuery, masterBillIssuerCode).Select(x => x.PK).ToArray();
									if (masterBillPKs.Length > 0)
									{
										bills = houseBills.Where(x => masterBillPKs.Contains(x.CU_CU_ParentBill)).ToArray();
									}
								}
							}
						}
						if (bills.Length == 0)
						{
							var fdaAddInfos = fdaRow.GetAddInfos(CusAddInfoSchema.B7_AddInfoData);
							logger.Log(Integration.LogType.Warning, ResString.GetMultilingualString("20878FBF-5245-4FEF-8A56-74C9EBDB6F69", "Cannot create related bill for FDA (Product:{0}, {4}:{1}, {5}:{2}) as no matching container was found ({3}).", fdaAddInfos.GetValue(USFDAAddInfoSchema.US_FDAProductCode), invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_InvoiceNumber), invoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_LineNo), matchingCriteriaBuilder.ToStringWithDelimiterBetweenAppends(", "), "InvoiceNumber", "LineNo"));
						}
						else
						{
							var fdaPK = fdaRow.GetValue(CusAddInfoSchema.PK);
							foreach (var bill in bills)
							{
								var billRow = GetColumnIndexer(bill);
								var billPK = billRow.GetValue(CusDecHouseBillSchema.PK);
								var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, fdaPK);
								pivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, (ZString)FDARelatedBillsGenPivot.RelationType);
								pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, billPK);
								pivotQuery.FetchOnlyFromLocalCache = fdaIsInDatabase || !bill.IsInDatabase;
								var pivotRow = GetColumnIndexer(helper.LoadTop1<FDARelatedBillsGenPivot>(pivotQuery));
								if (pivotRow == null)
								{
									pivotRow = GetColumnIndexer(helper.Factory.New<FDARelatedBillsGenPivot>());
									pivotRow.SetValue(GenPivotSchema.XX_Relation1ID, fdaPK);
									pivotRow.SetValue(GenPivotSchema.XX_Relation1TableCode, (ZString)CusAddInfoSchema.Constants.Prefix);
									pivotRow.SetValue(GenPivotSchema.XX_RelationType, (ZString)FDARelatedBillsGenPivot.RelationType);
									pivotRow.SetValue(GenPivotSchema.XX_Relation2ID, billRow.GetValue(CusDecHouseBillSchema.PK));
									pivotRow.SetValue(GenPivotSchema.XX_Relation2TableCode, (ZString)CusDecHouseBillSchema.Constants.Prefix);
								}
							}
						}
					}
					else
					{
						var fdaAddInfos = fdaRow.GetAddInfos(CusAddInfoSchema.B7_AddInfoData);
						logger.Log(Integration.LogType.Warning, ResString.GetMultilingualString("EE8E974F-9768-4393-99E8-21E8BC47ECB9", "Cannot create related bill for FDA (Product:{0}, {3}:{1}, {4}:{2}) as no bill number was specified.", fdaAddInfos.GetValue(USFDAAddInfoSchema.US_FDAProductCode), invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_InvoiceNumber), invoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_LineNo), "InvoiceNumber", "LineNo"));
					}
				}
			}
		}

		Bill[] GetBills(ZQuery query, ZString? issuerCode)
		{
			return helper.Factory.Load<Bill>(query).Where(y => !issuerCode.HasValue || y.US_UI_NKBillIssuerSCAC == issuerCode.Value).ToArray();
		}

		static void AddBillNumberAndIssuerCodeQuery(ZQuery query, ZString billNumber, ZString billType)
		{
			query.AddToFilter(CusDecHouseBillSchema.CU_BillNum, billNumber);
			query.AddToFilter(CusDecHouseBillSchema.CU_BillType, billType);
		}

		#endregion
	}
}
