using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectReader : DataObjectReader<UniversalCustoms.EntryHeader, CusEntryHeader>
	{
		public CustomsEntryHeaderDataObjectReader(UniversalCustoms.EntryHeader entryHeaderDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, BaseJobDeclaration declaration, ZGuid primeEntryPK, List<ZString> matchingKeys = null)
			: base(entryHeaderDataObject, logger, helper.Factory)
		{
			this.declaration = Argument.NotNull(declaration, "JobDeclaration declaration");
			this.helper = helper;
			this.primeEntryPK = primeEntryPK;
			this.matchingKeys = matchingKeys ?? new List<ZString>();
		}

		protected override CusEntryHeader GetExistingBusinessObject()
		{
			CusEntryHeader[] results = null;

			if (dataObject.EntryNumberCollection != null && dataObject.EntryNumberCollection.Count > 0)
			{
				var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_ClusterKey, declaration.JE_ClusterKey);
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_JE, declaration.PK);
				GetBaseCusEntryHeaderQuery(entryHeaderQuery);

				var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, dataObject.EntryNumberCollection.Select(x => x.Number));
				entryHeaderQuery.AddSubQuery(entryNumberSubQuery, JoinCondition.And);
				results = factory.Load<CusEntryHeader>(entryHeaderQuery);
			}

			if ((results == null || results.Length == 0) && dataObject.Reference.HasValue)
			{
				var query = new ZQuery(CusEntryHeaderSchema.CH_ClusterKey, declaration.JE_ClusterKey);
				query.AddToFilter(CusEntryHeaderSchema.CH_JE, declaration.PK);
				GetBaseCusEntryHeaderQuery(query);

				var orQuery = new ZQuery();
				orQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, dataObject.Reference.GetValueOrDefault().ToUpper());
				orQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_BGMReference, declaration.JE_DeclarationReference + "/" + dataObject.Reference.GetValueOrDefault().ToUpper());
				query.AddToFilter(orQuery);

				query.FetchOnlyFromLocalCache = !declaration.IsInDatabase;
				results = factory.Load<CusEntryHeader>(query);
			}

			duplicateMatchesFound = results != null && results.Length > 1;
			return results != null && results.Length == 1 ? results[0] : null;
		}
		ZBool duplicateMatchesFound = false;

		void GetBaseCusEntryHeaderQuery(ZQuery query)
		{
			if (primeEntryPK.IsEmpty)
			{
				query.AddToFilter(CusEntryHeaderSchema.CH_CH_PrimeEntry, null);
			}
			else
			{
				query.AddToFilter(CusEntryHeaderSchema.CH_CH_PrimeEntry, primeEntryPK);
			}

			if (dataObject.Type != null)
			{
				query.AddToFilter(CusEntryHeaderSchema.CH_MessageType, dataObject.Type.GetCodeAsUpperCase());
			}
		}

		protected override CusEntryHeader GetNewBusinessObject()
		{
			// use this to handle correct type
			return (CusEntryHeader)factory.New(declaration.CustomsEntryHeaders.TypeOfElements);
		}

		protected override void PopulateBusinessObject(CusEntryHeader entryHeader)
		{
			var entryHeaderRow = GetColumnIndexer(entryHeader);
			var addInfoManager = entryHeader as IAddInfoManager;
			var isAddInfoSerialisationEnabled = addInfoManager != null && !IsDefaultingEnabled;
			try
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
				}
				var entryHeaderPK = entryHeaderRow.GetValue(CusEntryHeaderSchema.PK);
				var entryHeaderIsInDatabase = entryHeader.IsInDatabase;
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_JE, declaration.PK);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_ClusterKey, declaration.JE_ClusterKey);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_CH_PrimeEntry, primeEntryPK);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_CEI_Instruction, helper.GetEntryInstructionPK(dataObject.EntryInstructionLink));
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_MessageType, dataObject.Type);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_Status, dataObject.MessageStatus);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_EntryStatus, dataObject.EntryStatus);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_BGMReference, dataObject.Reference);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_TotalPaid, dataObject.TotalAmountPaid);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_EntrySubmittedDate, dataObject.EntrySubmittedDate);
				SetValue(entryHeaderRow, CusEntryHeaderSchema.CH_EntryReleaseDate, dataObject.EntryReleaseDate);
				if (addInfoManager != null)
				{
					FillAddInfos(addInfoManager, entryHeaderRow);
					if (IsDefaultingEnabled)
					{
						addInfoManager.UpdateRelatedPropertyInfo();
					}
				}
				FillEntryNumbers(entryHeader);
				FillEntryCharges(entryHeader);
				FillAddInfoGroups(entryHeaderPK, entryHeaderIsInDatabase);
				FillCustomsReferences(entryHeaderPK, entryHeaderIsInDatabase);
				FillEntryLines(entryHeader);
				FillRelatedEntryHeaderCollection(entryHeader);
				FillEntryPayInfo(entryHeader);
				FillCountrySpecificData(entryHeader);
			}
			finally
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.UpdateAddInfoFromString(entryHeaderRow.GetValue(CusEntryHeaderSchema.CH_AddInfo));
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
				}
			}
		}

		protected virtual void FillAddInfoGroups(ZGuid entryHeaderPK, bool entryHeaderIsInDatabase)
		{
			new AddInfoGroupCollectionDataObjectReader(logger, helper).ReadIntoDataRows(entryHeaderPK, CusEntryHeaderSchema.Constants.Prefix, entryHeaderIsInDatabase, dataObject);
		}

		protected virtual void FillCustomsReferences(ZGuid entryHeaderPK, bool entryHeaderIsInDatabase)
		{
			new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(entryHeaderPK, CusEntryHeaderSchema.Constants.Prefix, entryHeaderIsInDatabase, dataObject);
		}

		protected virtual CustomsEntryLineDataObjectReader CreateNewCustomsEntryLineDataObjectReader(UniversalCustoms.EntryLine entryLineDataObject, CusEntryHeader entryHeader) => new CustomsEntryLineDataObjectReader(entryLineDataObject, logger, helper, entryHeader);

		void FillEntryLines(CusEntryHeader entryHeader)
		{
			DeleteExistingEntryLinesAndInvoiceLines(entryHeader);
			if (dataObject.EntryLineCollection != null)
			{
				foreach (var entryLineDataObject in dataObject.EntryLineCollection)
				{
					var entryLineBO = CreateNewCustomsEntryLineDataObjectReader(entryLineDataObject, entryHeader).ReadIntoBusinessObject();
					if (entryLineBO != null)
					{
						entryHeader.AllEntryLines.Add(entryLineBO);
					}
				}
			}
		}

		void DeleteExistingEntryLinesAndInvoiceLines(CusEntryHeader entryHeader)
		{
			if (entryHeader.IsInDatabase)
			{
				var existingLines = factory.Load<CusEntryLine>(new ZQuery(CusEntryLineSchema.CL_CH, entryHeader.PK));

				foreach (var entryLine in existingLines)
				{
					entryLine.FetchStrategy.FetchForDelete();
				}

				if (existingLines.Length > 0)
				{
					var invoiceHeaders = new HashSet<BaseJobComInvoiceHeader>();
					var invoiceLines = factory.Load<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_CL, existingLines.Select(x => x.PK)));

					foreach (var invoiceLine in invoiceLines)
					{
						invoiceLine.FetchStrategy.FetchForDelete();
						invoiceHeaders.Add(invoiceLine.InvoiceHeader);
					}

					IEnumerable<IDisposable> lineNumberRenumberingForDataImportSuspenders = null;
					try
					{
						lineNumberRenumberingForDataImportSuspenders = invoiceHeaders.Select(x => x.SuspendLineNumberRenumberingForDataImport()).ToArray();
						var shouldDeleteLines = new List<BaseJobComInvoiceLine>();
						foreach (var invoiceLine in invoiceLines)
						{
							if (invoiceLine.JI_MatchingKey.In(matchingKeys))
							{
								invoiceLine.JI_CL = ZGuid.Empty;
							}
							else
							{
								shouldDeleteLines.Add(invoiceLine);
							}
						}

						shouldDeleteLines.DeleteAll(true);
					}
					finally
					{
						if (lineNumberRenumberingForDataImportSuspenders != null)
						{
							lineNumberRenumberingForDataImportSuspenders.ForEach(x =>
							{
								if (x != null)
								{
									x.Dispose();
								}
							});
						}
					}

					foreach (var invoiceHeader in invoiceHeaders)
					{
						invoiceHeader.InvoiceLineLineNumberGenerator.ReCalculateAll();
					}
				}
				existingLines.DeleteAll(true); // no entry line level matching should be done to reduce pontential linking of JobComInvoiceLine
			}
		}

		protected virtual void FillAddInfos(IAddInfoManager addInfoManager, IColumnIndexer entryHeaderRow)
		{
			AddInfoDataObjectReader.New(addInfoManager as BusinessObject, logger, helper, CusEntryHeaderSchema.CH_AddInfo).ReadIntoRow(addInfoManager, entryHeaderRow, dataObject, null);
		}

		protected virtual void FillEntryCharges(CusEntryHeader entryHeader)
		{
			if (dataObject.EntryHeaderChargeCollection != null)
			{
				var existingNumbers = new List<CusEntryHeaderCharges>(factory.Load<CusEntryHeaderCharges>(new ZQuery(CusEntryHeaderChargesSchema.C1_CH, entryHeader.PK)));
				foreach (var entryHeaderChargeDataObject in dataObject.EntryHeaderChargeCollection)
				{
					var entryCharge = new CustomsEntryHeaderChargeDataObjectReader(entryHeaderChargeDataObject, logger, helper, entryHeader).ReadIntoBusinessObject();
					existingNumbers.Remove(entryCharge);
				}
				existingNumbers.DeleteAll(true);
			}
		}

		protected virtual void FillCountrySpecificData(CusEntryHeader entryHeader)
		{
		}

		protected virtual void FillEntryNumbers(CusEntryHeader entryHeader)
		{
			if (dataObject.EntryNumberCollection != null)
			{
				var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
				var existingNumbers = new List<CusEntryNumber>(factory.Load<CusEntryNumber>(query));
				foreach (var entryNumberDataObject in dataObject.EntryNumberCollection)
				{
					var entryNumber = CreateCustomsEntryNumberDataObjectReader(entryNumberDataObject, logger, helper, entryHeader).ReadIntoBusinessObject();
					existingNumbers.Remove(entryNumber);
				}
				existingNumbers.DeleteAll(true);
			}
		}

		protected virtual CustomsEntryNumberDataObjectReader<CusEntryHeader> CreateCustomsEntryNumberDataObjectReader(
			UniversalCustoms.EntryNumber entryNumberDataObject,
			IXmlImportLogger logger,
			UniversalDataObjectReaderHelper helper,
			CusEntryHeader entryHeader)
		{
			return new CustomsEntryNumberDataObjectReader<CusEntryHeader>(entryNumberDataObject, logger, helper, entryHeader);
		}

		protected virtual void FillRelatedEntryHeaderCollection(CusEntryHeader entryHeader)
		{
			if (dataObject.RelatedEntryHeaderCollection != null)
			{
				var query = new ZQuery(CusEntryHeaderSchema.CH_ClusterKey, entryHeader.CH_ClusterKey);
				query.AddToFilter(CusEntryHeaderSchema.CH_JE, declaration.PK);
				query.AddToFilter(CusEntryHeaderSchema.CH_CH_PrimeEntry, entryHeader.PK);
				var existingEntries = new List<CusEntryHeader>(helper.Load<CusEntryHeader>(query));
				foreach (var relatedEntryHeaderDataObject in dataObject.RelatedEntryHeaderCollection)
				{
					var relatedEntryHeader = new CustomsEntryHeaderDataObjectReader(relatedEntryHeaderDataObject, logger, helper, declaration, entryHeader.PK, matchingKeys).ReadIntoBusinessObject();
					existingEntries.Remove(relatedEntryHeader);
				}
				existingEntries.DeleteAll(true);
			}
		}

		void FillEntryPayInfo(CusEntryHeader entryHeader)
		{
			if (!ShouldPopulatePaymentInformationData)
			{
				return;
			}

			var universalPaymentInformationCollection = dataObject.PaymentInformationCollection;
			if (universalPaymentInformationCollection is null)
			{
				return;
			}

			var existingAndNotMatchedEntryPayInfo = entryHeader.EntryPayInfos.ToList();
			foreach (var paymentInformation in universalPaymentInformationCollection)
			{
				var entryPayInfo = new CusEntryPayInfoDataObjectReader(paymentInformation, logger, factory, entryHeader).ReadIntoBusinessObject();
				existingAndNotMatchedEntryPayInfo.Remove(entryPayInfo);
			}
			existingAndNotMatchedEntryPayInfo.DeleteAll(addFetchHints: true);
		}

		protected virtual bool ShouldPopulatePaymentInformationData => false;

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusEntryHeader targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (result.IsEmpty && duplicateMatchesFound)
			{
				result = Res.GetString("96D4785B-DA49-4E51-A1D2-D0FA827364CE", "Cannot import when there are multiple entry headers matched.");
			}
			return result;
		}

		protected readonly BaseJobDeclaration declaration;
		protected readonly UniversalDataObjectReaderHelper helper;
		protected readonly ZGuid primeEntryPK;
		protected readonly List<ZString> matchingKeys;
	}
}
