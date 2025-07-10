using System.Collections.Generic;
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
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AdditionalAddInfoGroupCollectionDataObjectReaderForFDARelatedContainer : DataObjectReader, IAdditionalAddInfoGroupCollectionDataObjectReader
	{
		public AdditionalAddInfoGroupCollectionDataObjectReaderForFDARelatedContainer(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
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
			var invoiceLine = fdaRow == null ? null : helper.Load<JobComInvoiceLine>(fdaRow, CusAddInfoSchema.B7_ParentID);
			var invoiceLineRow = GetColumnIndexer(invoiceLine);
			var invoiceRow = GetColumnIndexer(invoiceLineRow == null ? null : helper.Load<JobComInvoiceHeader>(invoiceLineRow, JobComInvoiceLineSchema.JI_JZ));
			var declaration = invoiceRow == null ? null : helper.Load<JobDeclaration>(invoiceRow, JobComInvoiceHeaderSchema.JZ_JE);
			if (declaration != null)
			{
				var declarationRow = GetColumnIndexer(declaration);
				DeleteAllExistingPivot(fda);
				var declarationPK = declarationRow.GetValue(JobDeclarationSchema.PK);
				var fdaPK = fdaRow.GetValue(CusAddInfoSchema.PK);
				var declarationIsInDatabase = declaration.IsInDatabase;
				var invoiceLineIsInDatabase = invoiceLine.IsInDatabase;
				foreach (var addInfoGroup in addInfoGroupCollection)
				{
					ZString? containerNumber = addInfoGroup.AddInfoCollection.GetZStringValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedContainer.ContainerNumber, logger);
					if (containerNumber.HasValue)
					{
						ZString? sealNumber = addInfoGroup.AddInfoCollection.GetZStringValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.FDARelatedContainer.SealNumber, logger);
						var query = new ZQuery(CusContainerSchema.CO_JE, declarationPK);
						query.AddToFilter(CusContainerSchema.CO_ContainerNumber, containerNumber.Value);
						var matchingCriteriaBuilder = new ZStringBuilder("Number:" + containerNumber.Value);
						if (sealNumber.HasValue)
						{
							query.AddToFilter(CusContainerSchema.CO_Seal, sealNumber.Value);
							matchingCriteriaBuilder.Append("Seal:" + sealNumber.Value);
						}
						query.FetchOnlyFromLocalCache = !declarationIsInDatabase;
						var containers = helper.Load<CusContainer>(query);
						if (containers.Length == 0)
						{
							var fdaAddInfos = fdaRow.GetAddInfos(CusAddInfoSchema.B7_AddInfoData);
							logger.Log(Integration.LogType.Warning, ResString.GetMultilingualString("{D79D3A0F-617E-473D-B152-0BBD9F8970FA}", "Cannot create related container for FDA (Product:{0}, {4}:{1}, {5}:{2}) as no matching container was found ({3}).", fdaAddInfos.GetValue(USFDAAddInfoSchema.US_FDAProductCode), invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_InvoiceNumber), invoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_LineNo), matchingCriteriaBuilder.ToStringWithDelimiterBetweenAppends(", "), "InvoiceNumber", "LineNo"));
						}
						else
						{
							foreach (var container in containers)
							{
								var containerRow = GetColumnIndexer(container);
								var containerPK = containerRow.GetValue(CusContainerSchema.PK);
								var containerPivotQuery = new ZQuery(CusContainerInvoiceLinePivotSchema.C2_CO, containerPK);
								var invoiceLinePK = invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK);
								containerPivotQuery.AddToFilter(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLinePK);
								containerPivotQuery.FetchOnlyFromLocalCache = !container.IsInDatabase || !invoiceLineIsInDatabase;
								var containerPivotRow = GetColumnIndexer(helper.LoadTop1<CusContainerInvoiceLinePivot>(containerPivotQuery));
								if (containerPivotRow == null)
								{
									containerPivotRow = GetColumnIndexer(helper.Factory.New<CusContainerInvoiceLinePivot>());
									containerPivotRow.SetValue(CusContainerInvoiceLinePivotSchema.C2_CO, containerPK);
									containerPivotRow.SetValue(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLinePK);
								}
								var pivotRow = GetColumnIndexer(helper.Factory.New<FDARelatedContainersGenPivot>());
								pivotRow.SetValue(GenPivotSchema.XX_Relation1ID, fdaPK);
								pivotRow.SetValue(GenPivotSchema.XX_Relation1TableCode, (ZString)CusAddInfoSchema.Constants.Prefix);
								pivotRow.SetValue(GenPivotSchema.XX_RelationType, (ZString)FDARelatedContainersGenPivot.RelationType);
								pivotRow.SetValue(GenPivotSchema.XX_Relation2ID, containerPivotRow.GetValue(CusContainerInvoiceLinePivotSchema.PK));
								pivotRow.SetValue(GenPivotSchema.XX_Relation2TableCode, (ZString)CusContainerInvoiceLinePivotSchema.Constants.Prefix);
							}
						}
					}
					else
					{
						var fdaAddInfos = fdaRow.GetAddInfos(CusAddInfoSchema.B7_AddInfoData);
						logger.Log(Integration.LogType.Warning, ResString.GetMultilingualString("{6283F50D-30B1-4724-8492-CC9360362B69}", "Cannot create related container for FDA (Product:{0}, {3}:{1}, {4}:{2}) as container number has not been specified.", fdaAddInfos.GetValue(USFDAAddInfoSchema.US_FDAProductCode), invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_InvoiceNumber), invoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_LineNo), "InvoiceNumber", "LineNo"));
					}
				}
			}
		}

		void DeleteAllExistingPivot(FDA fda)
		{
			var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, (ZString)CusAddInfoSchema.Constants.Prefix);
			query.AddToFilter(GenPivotSchema.XX_RelationType, (ZString)FDARelatedContainersGenPivot.RelationType);
			query.FetchOnlyFromLocalCache = !fda.IsInDatabase;
			helper.Factory.Load<FDARelatedContainersGenPivot>(query).DeleteAll(); // delete pivot added by businessobject
		}

		#endregion
	}
}
