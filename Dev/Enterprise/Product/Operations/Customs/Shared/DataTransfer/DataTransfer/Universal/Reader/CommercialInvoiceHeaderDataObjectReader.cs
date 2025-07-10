using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using IAdditionalLineTariffDetailParent = Enterprise.Customs.Business.IAdditionalLineTariffDetailParent;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using ValueSetter = Enterprise.UniversalDataBuss.DataObjects.Core.ValueSetter;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectReader<TInvoiceGroupHeader> : DataObjectWithWorkflowCustomFieldsReader<UniversalCustoms.CommercialInvoiceHeader>, IOrganisationDataObjectReaderSupporter
		where TInvoiceGroupHeader : BaseJobComInvoiceGroupHeader
	{
		public CommercialInvoiceHeaderDataObjectReader(UniversalCustoms.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, TInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null, Type invoiceType = null)
			: base(invoiceDataObject, logger, helper.Factory)
		{
			this.topLevelObject = topLevelObject;
			this.landedCostDataReader = landedCostDataReader;
			this.groupHeader = groupHeader;
			this.helper = helper;
			this.invoiceType = invoiceType;
			if (invoiceType == null && groupHeader != null)
			{
				invoiceType = ((IBusinessObjectCollection)groupHeader.JobComInvoiceHeaders).TypeOfElements;
			}
			if (invoiceType == null)
			{
				throw new InvalidOperationException("Either groupHeader or invoiceType must be specified");
			}
		}

		protected readonly TInvoiceGroupHeader groupHeader;
		protected readonly UniversalDataObjectReaderHelper helper;
		protected ILandedCostDataReader landedCostDataReader;
		bool invoiceLevelPackagePivotExist;
		protected readonly ITopLevelDataObject topLevelObject;
		readonly Type invoiceType;

		public BaseJobComInvoiceHeader ReadIntoBusinessObject(bool matchExistingInvoice = false, CollectionContent? commercialInvoiceCollectioncontent = null)
		{
			BaseJobComInvoiceHeader invoice = null;

			var infoMatcher = helper.CommercialInfoMatcher;

			if (infoMatcher != null)
			{
				invoice = infoMatcher.GetInvoice(dataObject);
			}

			if (invoice == null && matchExistingInvoice)
			{
				var invoiceNumber = dataObject.InvoiceNumber.GetValueOrDefault();
				var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, groupHeader?.JZ_JE ?? ZGuid.Empty);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber);
				invoice = factory.LoadTop1<BaseJobComInvoiceHeader>(query);

				if (invoice != null && groupHeader != null && invoice.JZ_JZ_GroupInvoiceFK != groupHeader.PK)
				{
					invoice.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
				}
			}

			invoice = invoice ?? GetNewInvoice();
			return ReadIntoBusinessObject(ref invoice, null, commercialInvoiceCollectioncontent);
		}

		internal BaseJobComInvoiceHeader ReadIntoBusinessObject(ref BaseJobComInvoiceHeader invoiceBO, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData, CollectionContent? commercialInvoiceCollectioncontent = null)
		{
			BaseJobComInvoiceHeader result = null;

			var reason = GetReasonForNotAbleToUpdate(invoiceBO);

			if (reason.IsEmpty)
			{
				result = PopulateInvoiceDataAndFinaliseImport(invoiceBO, commercialInvoiceHeaderRelatedData, commercialInvoiceCollectioncontent);
			}
			else
			{
				logger.Log(LogType.Error, Res.GetString("A25A7D39-7741-4C33-85B8-46BD5AFCB0BC", "Cannot populate invoice header data because:{0}{1}", System.Environment.NewLine, reason));
			}

			return result;
		}

		protected virtual ZString GetReasonForNotAbleToUpdate(BaseJobComInvoiceHeader invoice)
		{
			var result = ZString.Empty;

			var lines = dataObject?.CommercialInvoiceLineCollection;

			if (lines != null)
			{
				var content = lines.Content;

				if (content.HasValue
					&& content == CollectionContent.Partial
					&& lines.Any(c => c.DataImportMatchingKey.GetValueOrDefault().IsEmpty))
				{
					result = Res.GetString("B2F75DD1-AEE3-4E54-9E10-6C42852A522D"
						, "A non-empty element <{0}> is required in the case of 'Partial' collection."
						, nameof(UniversalCustoms.CommercialInvoiceLine.DataImportMatchingKey));
				}
				else
				{
					var duplicateKeys = GetDuplicateKeys();

					if (duplicateKeys.Any())
					{
						result = Res.GetString("816E3CE5-246D-47D4-917B-F66326226393"
							, "These matching keys are used on two or more different invoice lines linking to the same invoice header.{0}{1}"
							, System.Environment.NewLine
							, string.Join(", ", duplicateKeys));
					}
				}
			}

			return result;
		}

		protected virtual BaseJobComInvoiceHeader PopulateInvoiceDataAndFinaliseImport(BaseJobComInvoiceHeader invoiceBO, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData, CollectionContent? commercialInvoiceCollectioncontent = null)
		{
			using (SuspendSetters(invoiceBO))
			{
				var invoice = invoiceBO;
				var invoiceRow = GetColumnIndexer(invoice);
				var addInfoManager = invoice as IAddInfoManager;
				var isDefaultingEnabled = IsDefaultingEnabled;
				var shouldAddFetchHint = isDefaultingEnabled && !invoice.IsInDatabase;
				var isAddInfoSerialisationEnabled = addInfoManager != null && !isDefaultingEnabled;
				try
				{
					if (isAddInfoSerialisationEnabled)
					{
						addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
					}

					if (commercialInvoiceHeaderRelatedData != null)
					{
						ClearExistingDataThatWillBeReplaced(invoice, shouldAddFetchHint);
					}

					if (dataObject.CommercialChargeCollection != null || (commercialInvoiceHeaderRelatedData?.HasGroupChargeData ?? false))
					{
						if (shouldAddFetchHint && commercialInvoiceHeaderRelatedData != null)
						{
							invoice.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData(new IBusiness[] { invoice.Charges, invoice.GroupCharges });
						}

						invoice.Charges.RemoveAndDeleteAll();
						invoice.GroupCharges.RemoveAndDeleteAll();
					}

					var invoicePK = invoiceRow.GetValue(JobComInvoiceHeaderSchema.PK);
					var invoiceIsInDatabase = invoice.IsInDatabase;
					var delaySetters = isDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
					FillOrganizations(invoice, commercialInvoiceHeaderRelatedData, delaySetters);

					if (commercialInvoiceHeaderRelatedData == null)
					{
						FillRelatedBill(invoiceRow, delaySetters);
					}
					else
					{
						SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection, commercialInvoiceHeaderRelatedData.MessageType, delaySetters);
						ZGuid branchPK = commercialInvoiceHeaderRelatedData.GetBranchPK(factory.BOFactory);
						if (branchPK.IsValid)
						{
							SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_GB, branchPK, delaySetters);
						}
					}

					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_InvoiceNumber, dataObject.InvoiceNumber, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_InvoiceDate, dataObject.InvoiceDate, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_ValuationDateOverride, dataObject.ValuationDateOverride, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_InvoiceAmount, dataObject.InvoiceAmount, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency, dataObject.InvoiceCurrency, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRateType, dataObject.ExchangeRateType, delaySetters);
					SetValueWithDelay(invoiceRow, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate, () =>
					{
						ZDecimal? result = invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate);
						if (invoice.IsJZ_InvoiceCurrExRateUserEnterable)
						{
							result = dataObject.AgreedExchangeRate;
						}
						else if (invoice.InvoiceCountry != null && invoice.InvoiceCountry.RN_RX_NKLocalCurrency == invoice.JZ_RX_NKInvoice_Currency)
						{
							result = 1m;
						}
						return result;
					}, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_IncoTerm, dataObject.IncoTerm, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_IncoTermPlace, dataObject.AdditionalTerms, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_AdditionalTerms, dataObject.DeliveryTerms, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_Weight, dataObject.Weight, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_WeightUQ, dataObject.WeightUnit, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_Volume, dataObject.Volume, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_VolumeUQ, dataObject.VolumeUnit, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_NetWeight, dataObject.NetWeight, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_NetWeightUQ, dataObject.NetWeightUQ, delaySetters);

					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_InvoiceCurrLandedCostExRate, dataObject.LandedCostExchangeRate, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_PaymentNo, dataObject.PaymentNumber, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_PaymentAmount, dataObject.PaymentAmount, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_PaymentExRate, dataObject.PaymentExchangeRate, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_PaymentDate, dataObject.PaymentDate, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_NoOfPacks, dataObject.NoOfPacks, delaySetters);

					PopulateRelatedIndicator(dataObject.RelatedIndicator, invoiceRow, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_ValuationCode, dataObject.ValuationCode, delaySetters);

					FillMarksAndNumbers(invoice, delaySetters);

					ImportCountrySpecificRelatedData(invoice, delaySetters);
					FillNotes(invoice, GetMergedNoteCollection(dataObject.NoteCollection, commercialInvoiceHeaderRelatedData));

					if (addInfoManager != null && helper.IsSourceAndTargetCountrySame)
					{
						GetNewAddInfoDataObjectReaderForInvoice(invoiceBO).ReadIntoRow(addInfoManager, invoiceRow, dataObject, delaySetters);
					}

					delaySetters.SetValueOnSetterSupenderParentInSpecificOrder(invoice.SetterSuspender, GetSettingOrder(invoice));

					if (isDefaultingEnabled)
					{
						addInfoManager.UpdateRelatedPropertyInfo();
					}

					if (helper.IsSourceAndTargetCountrySame)
					{
						var tablePrefix = JobComInvoiceHeaderSchema.Constants.Prefix;
						GetNewAddInfoGroupCollectionDataObjectReader().ReadIntoDataRows(invoicePK, tablePrefix, invoiceIsInDatabase, dataObject);
						CreateNewCustomsReferenceCollectionDataObjectReader().ReadIntoDataRows(invoicePK, tablePrefix, invoiceIsInDatabase, dataObject);
						if (invoiceBO is Integration.Customs.ICusSupportingInfoTypeSupporter)
						{
							CreateNewCustomsSupportingInformationCollectionDataObjectReader().ReadIntoDataRows(invoicePK, tablePrefix, invoiceIsInDatabase, dataObject);
						}
					}

					var isStandalone = false;
					if (commercialInvoiceHeaderRelatedData != null)
					{
						FillCollectionsDataForStandalone(invoice, commercialInvoiceHeaderRelatedData);
						isStandalone = true;
					}

					var supportsChzPivotBetweenInvoiceHeaderAndPacking = invoice.JobDeclaration?.SupportsChzPivotBetweenInvoiceHeaderAndPacking ?? false;
					if (supportsChzPivotBetweenInvoiceHeaderAndPacking)
					{
						FillPackInvoiceHeaderPivots(invoice, dataObject);
					}

					FillCommercialInvoiceLineData(invoice, isStandalone, commercialInvoiceCollectioncontent);
					FillCommercialInfo(dataObject.CommercialChargeCollection, invoice, logger, factory);
					PopulateWorkflowCustomFields(invoice, dataObject);
				}
				finally
				{
					if (isAddInfoSerialisationEnabled)
					{
						addInfoManager.UpdateAddInfoFromString(invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_AddInfo));
						addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
					}
				}

				if (landedCostDataReader != null)
				{
					landedCostDataReader.CollectTransportLogisticsCost(invoice, dataObject);
				}

				if (ShouldFireDataImportedToBusinessObject)
				{
					logger.FireDataImportedToBusinessObject(invoice);
				}

				return invoice;
			}
		}

		protected virtual bool ShouldFireDataImportedToBusinessObject => false;

		protected virtual CustomsReferenceCollectionDataObjectReader CreateNewCustomsReferenceCollectionDataObjectReader() => new CustomsReferenceCollectionDataObjectReader(logger, helper);

		protected virtual CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader() => new CustomsSupportingInformationCollectionDataObjectReader(logger, helper);

		protected virtual void PopulateRelatedIndicator(ICodeDataObject relatedIndicatorDataObject, IColumnIndexer invoiceRow, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_RelatedIndicator, relatedIndicatorDataObject, delaySetters);
		}

		protected IDisposable SuspendSetters(BaseJobComInvoiceHeader invoiceBO)
		{
			return invoiceBO.SetterSuspender.SuspendSetting(GetJobComInvoiceHeaderPropertiesToSuspendSetting().ToArray());
		}

		protected virtual IEnumerable<ZString> GetJobComInvoiceHeaderPropertiesToSuspendSetting()
		{
			if (dataObject.IncoTerm != null)
			{
				yield return BaseJobComInvoiceHeader.Schema.JZ_IncoTerm;
			}
			if (dataObject.InvoiceCurrency != null)
			{
				yield return BaseJobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency;
			}
			if (dataObject.Supplier != null)
			{
				yield return BaseJobComInvoiceHeader.Schema.JZ_OH_Supplier;
				yield return BaseJobComInvoiceHeader.Schema.JZ_OA_SupplierAddress;
			}
			if (dataObject.Buyer != null)
			{
				yield return BaseJobComInvoiceHeader.Schema.JZ_OH_Buyer;
				if (dataObject.Buyer.Address1.HasValue)
				{
					yield return BaseJobComInvoiceHeader.Schema.JZ_OA_BuyerAddress;
				}
			}
		}

		protected virtual IEnumerable<ZString> GetDuplicateKeys()
		{
			var matchingKeys = helper.GetDataImportMatchingKeys(dataObject);
			var keyGroups = matchingKeys.GroupBy(c => c.ToUpper());

			foreach (var grouping in keyGroups)
			{
				var key = grouping.Key;

				if (!key.IsEmpty && grouping.Count() > 1)
				{
					yield return key;
				}
			}
		}

		protected virtual void ImportCountrySpecificRelatedData(BaseJobComInvoiceHeader invoiceBO, Dictionary<string, ValueSetter> delaySetters)
		{
		}

		protected virtual void FillCommercialInfo(IEnumerable<UniversalCustoms.CommercialCharge> commercialInvoiceChargeCollection, ICommonNonApportionedChargeProvider<BaseInvoiceCharge> provider, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			CommercialChargeDataObjectReader<BaseInvoiceCharge>.FillCommercialInfo(commercialInvoiceChargeCollection, provider, logger, factory);
		}

		protected void FillCommercialInvoiceLineData(BaseJobComInvoiceHeader invoice, bool isStandalone, CollectionContent? commercialInvoiceCollectioncontent = null)
		{
			if (dataObject.CommercialInvoiceLineCollection != null)
			{
				var declaration = isStandalone ? null : helper.Load<BaseJobDeclaration>(invoice.JZ_JE);
				var isDeclarationIntegrated = declaration?.IsDeclarationIntegrated ?? false;
				var supportsChcPivotBetweenInvoiceLineAndPacking = declaration?.SupportsChcPivotBetweenInvoiceLineAndPacking ?? false;
				var invoiceNumber = invoice.JZ_InvoiceNumber;
				using (invoice.SuspendLineNumberRenumberingForDataImport())
				{
					var zeroOrInvalidLineNoInvoiceLineDatas = new DataObjectList<UniversalCustoms.CommercialInvoiceLine>();
					var lineNoDictionary = GetSortedCommercialInvoiceLineLineNoDetails(zeroOrInvalidLineNoInvoiceLineDatas);

					List<BaseJobComInvoiceLine> existingInvoiceLinesInLineNoOrder = null;
					var collectionContent = dataObject.CommercialInvoiceLineCollection?.Content;
					var isDefaultingEnabled = IsDefaultingEnabled;
					var lines = invoice.JobComInvoiceLines;
					if (lines.Count > 0)
					{
						if (collectionContent.HasValue)
						{
							RemoveInvoiceLineIfNeed(lines, collectionContent.Value, lineNoDictionary);
							if (isDefaultingEnabled && collectionContent.Value == CollectionContent.Partial)
							{
								existingInvoiceLinesInLineNoOrder = lines.Cast<BaseJobComInvoiceLine>()
									.OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_ParentID.IsEmpty ? 0 : 1)
									.ThenBy(x => x.JI_ParentLine).ThenBy(x => x.PK).ToList();
							}
						}
						else if (commercialInvoiceCollectioncontent.HasValue && commercialInvoiceCollectioncontent.Value == CollectionContent.Partial)
						{
							lines.RemoveAndDeleteAll();
						}
					}

					AddFetchHintsForFillCommercialInvoiceLineBasedOnLineNoDetails(declaration, invoice, lineNoDictionary);

					var currentLineNo = FillCommercialInvoiceLineBasedOnLineNoDetails(invoice, invoiceNumber, zeroOrInvalidLineNoInvoiceLineDatas, lineNoDictionary, isDefaultingEnabled, isStandalone, isDeclarationIntegrated, supportsChcPivotBetweenInvoiceLineAndPacking, existingInvoiceLinesInLineNoOrder);

					FillCommercialInvoiceLineDataForZeroOrInvalidLineNoDetail(invoice, invoiceNumber, zeroOrInvalidLineNoInvoiceLineDatas, isDefaultingEnabled, currentLineNo, isStandalone, isDeclarationIntegrated, supportsChcPivotBetweenInvoiceLineAndPacking, existingInvoiceLinesInLineNoOrder);

					if (existingInvoiceLinesInLineNoOrder != null && existingInvoiceLinesInLineNoOrder.Count > 0)
					{
						currentLineNo = 1;
						var existingInvoiceLinePKs = new HashSet<BaseJobComInvoiceLine>();
						existingInvoiceLinesInLineNoOrder.ForEach(x =>
						{
							existingInvoiceLinePKs.Add(x);
							x.JI_LineNo = currentLineNo++;
						});

						foreach (var invoiceLine in invoice.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Where(x => !existingInvoiceLinePKs.Contains(x)).OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_ParentID.IsEmpty ? 0 : 1).ThenBy(x => x.JI_ParentLine).ThenBy(x => x.PK).ToArray())
						{
							invoiceLine.JI_LineNo = currentLineNo++;
						}
					}
				}
				invoice.InvoiceLineLineNumberGenerator.ReCalculateAll();
			}
		}

		protected virtual void AddFetchHintsForFillCommercialInvoiceLineBasedOnLineNoDetails(BaseJobDeclaration declaration, BaseJobComInvoiceHeader invoice, SortedDictionary<ZInt, CommercialInvoiceLineDataRelationship> lineNoDictionary)
		{
		}

		void FillCommercialInvoiceLineDataForZeroOrInvalidLineNoDetail(BaseJobComInvoiceHeader invoice, ZString invoiceNumber, DataObjectList<UniversalCustoms.CommercialInvoiceLine> zeroOrInvalidLineNoInvoiceLineDatas, bool isDefaultingEnabled, ZShort currentLineNo, bool isStandalone, bool isDeclarationIntegrated, bool supportsChcPivotBetweenInvoiceLineAndPacking, List<BaseJobComInvoiceLine> existingInvoiceLinesInLineNoOrder)
		{
			foreach (var invoiceLineData in zeroOrInvalidLineNoInvoiceLineDatas)
			{
				var invoiceLine = GetInvoiceLine(invoiceLineData, invoice);
				if (isDefaultingEnabled)
				{
					existingInvoiceLinesInLineNoOrder?.Remove(invoiceLine);
					SetValue(invoiceLine, JobComInvoiceLineSchema.JI_LineNo, currentLineNo++);
				}

				FillCommercialInvoiceLineData(invoiceLineData, invoiceLine, invoiceNumber, null, isStandalone, isDeclarationIntegrated, supportsChcPivotBetweenInvoiceLineAndPacking);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void RemoveInvoiceLineIfNeed(BaseJobComInvoiceLineViewCollection lines, CollectionContent content, SortedDictionary<ZInt, CommercialInvoiceLineDataRelationship> lineNoDictionary)
		{
			var matchingKeys = new HashSet<ZString>(helper.GetDataImportMatchingKeys(dataObject).Select(x => x.ToUpperInvariant()));

			if (matchingKeys.Count > 0)
			{
				if (content == CollectionContent.Complete)
				{
					var invalidLines = lines
						.Cast<BaseJobComInvoiceLine>()
						.Where(c => c.JI_MatchingKey.IsEmpty || !matchingKeys.Contains(c.JI_MatchingKey.ToUpperInvariant()))
						.ToArray();

					foreach (var line in invalidLines)
					{
						if (lines.Contains(line))
						{
							lines.RemoveAndDelete(line);
						}
					}
				}

				var relationships = lineNoDictionary.Select(c => c.Value);
				foreach (var relationship in relationships)
				{
					var parentLines = new List<BaseJobComInvoiceLine>();
					foreach (var invoiceLineData in relationship.ParentInvoiceLineData)
					{
						var matchingKey = invoiceLineData.DataImportMatchingKey.GetValueOrDefault();
						var matchedLine = matchingKey.IsEmpty
						? null
						: lines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(c => IsMatching(c, invoiceLineData));

						if (matchedLine != null)
						{
							if (matchedLine.IsChildLine)
							{
								lines.RemoveAndDelete(matchedLine);
							}
							else
							{
								parentLines.Add(matchedLine);
							}
						}
					}

					var childrenInvoiceLineData = relationship.ChildInvoiceLineData.Select(c => c.Value);
					foreach (var invoiceLineData in childrenInvoiceLineData)
					{
						var matchingKey = invoiceLineData.DataImportMatchingKey.GetValueOrDefault();
						var matchedLine = matchingKey.IsEmpty
							? null
							: lines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(c => IsMatching(c, invoiceLineData));

						if (matchedLine != null)
						{
							var invalidLines = new List<BaseJobComInvoiceLine>();
							var parentLine = matchedLine.ParentTariffLine;
							if (parentLine == null || parentLines.All(c => c != parentLine))
							{
								invalidLines.Add(matchedLine);
							}
							else
							{
								var childrenLines = GetChildInvoiceLinesAddedBySystem(parentLine);
								var linesWithoutMatchedKeys = childrenLines.Where(c => relationship.ChildDataImportMatchingKeys.All(d => !d.EqualsIgnoringCase(c.JI_MatchingKey)));
								invalidLines.AddRange(linesWithoutMatchedKeys);
							}

							foreach (var line in invalidLines)
							{
								if (lines.Contains(line))
								{
									lines.RemoveAndDelete(line);
								}
							}
						}
					}
				}
			}
			else if (content == CollectionContent.Complete)
			{
				lines.RemoveAndDeleteAll();
			}
		}

		ZShort FillCommercialInvoiceLineBasedOnLineNoDetails(BaseJobComInvoiceHeader invoice, ZString invoiceNumber, DataObjectList<UniversalCustoms.CommercialInvoiceLine> zeroOrInvalidLineNoInvoiceLineDatas, SortedDictionary<ZInt, CommercialInvoiceLineDataRelationship> lineNoDictionary, bool isDefaultingEnabled, bool isStandalone, bool isDeclarationIntegrated, bool supportsChcPivotBetweenInvoiceLineAndPacking, List<BaseJobComInvoiceLine> existingInvoiceLinesInLineNoOrder)
		{
			ZShort currentLineNo = 1;
			var lines = invoice.JobComInvoiceLines;
			if (lines.Any())
			{
				currentLineNo = lines.Cast<BaseJobComInvoiceLine>().Max(x => x.JI_LineNo) + 1;
			}

			foreach (var pair in lineNoDictionary)
			{
				var invoiceLineDataRelationship = pair.Value;
				if (invoiceLineDataRelationship.ParentInvoiceLineData.Count == 0)
				{
					foreach (var childLineData in invoiceLineDataRelationship.ChildInvoiceLineData.Select(x => x.Value))
					{
						logger.Log(Integration.LogType.Warning, Res.GetString("0FF2D09C-B82F-4050-BF2C-0EC4595128EE", "Cannot not find {2} ('{0}') for Commercial Invoice Line with {3} ('{1}').", pair.Key, childLineData.LineNo.GetValueOrDefault(), "ParentLineNo", "LineNo"));
						zeroOrInvalidLineNoInvoiceLineDatas.Add(childLineData);
					}
				}
				else
				{
					BaseJobComInvoiceLine firstParentInvoiceLine = null;
					foreach (var invoiceLineData in invoiceLineDataRelationship.ParentInvoiceLineData)
					{
						var parentInvoiceLine = GetInvoiceLine(invoiceLineData, invoice);
						if (isDefaultingEnabled)
						{
							existingInvoiceLinesInLineNoOrder?.Remove(parentInvoiceLine);
							SetValue(parentInvoiceLine, JobComInvoiceLineSchema.JI_LineNo, currentLineNo++);
						}

						FillCommercialInvoiceLineData(invoiceLineData, parentInvoiceLine, invoiceNumber, null, isStandalone, isDeclarationIntegrated, supportsChcPivotBetweenInvoiceLineAndPacking);
						firstParentInvoiceLine = firstParentInvoiceLine ?? parentInvoiceLine;
					}

					var childInvoiceLineBOs = GetChildInvoiceLinesAddedBySystem(firstParentInvoiceLine);
					if (isDefaultingEnabled)
					{
						UpdateChildInvoiceLineBOsRelatedPropertyInfo(childInvoiceLineBOs);
					}

					var childrenMatchingKeys = invoiceLineDataRelationship.ChildDataImportMatchingKeys;
					foreach (var childInvoiceLineData in invoiceLineDataRelationship.ChildInvoiceLineData)
					{
						var invoiceLineData = childInvoiceLineData.Value;
						var invoiceLine = GetMatchingChildInvoiceLine(childInvoiceLineBOs, invoiceLineData, childrenMatchingKeys) ?? GetNewInvoiceLine(invoice);
						if (isDefaultingEnabled)
						{
							existingInvoiceLinesInLineNoOrder?.Remove(invoiceLine);
							SetValue(invoiceLine, JobComInvoiceLineSchema.JI_LineNo, currentLineNo++);
						}

						FillCommercialInvoiceLineData(childInvoiceLineData.Value, invoiceLine, invoiceNumber, firstParentInvoiceLine, isStandalone, isDeclarationIntegrated, supportsChcPivotBetweenInvoiceLineAndPacking);
					}

					if (isDefaultingEnabled)
					{
						foreach (var childInvoiceLineBO in childInvoiceLineBOs)
						{
							existingInvoiceLinesInLineNoOrder?.Remove(childInvoiceLineBO);
							SetValue(childInvoiceLineBO, JobComInvoiceLineSchema.JI_LineNo, currentLineNo++);
						}
					}
				}
			}

			return currentLineNo;
		}

		SortedDictionary<ZInt, CommercialInvoiceLineDataRelationship> GetSortedCommercialInvoiceLineLineNoDetails(DataObjectList<UniversalCustoms.CommercialInvoiceLine> zeroOrInvalidLineNoInvoiceLineDatas)
		{
			var lineNoDictionary = new SortedDictionary<ZInt, CommercialInvoiceLineDataRelationship>();
			foreach (var invoiceLineData in dataObject.CommercialInvoiceLineCollection)
			{
				CommercialInvoiceLineDataRelationship invoiceLineDataRelationship;
				var lineNo = invoiceLineData.LineNo.GetValueOrDefault();
				var parentLineNo = GetParentLineNo(invoiceLineData).GetValueOrDefault();
				if (parentLineNo != ZInt.Zero && parentLineNo != lineNo)
				{
					if (!lineNoDictionary.TryGetValue(parentLineNo, out invoiceLineDataRelationship))
					{
						invoiceLineDataRelationship = new CommercialInvoiceLineDataRelationship();
						lineNoDictionary.Add(parentLineNo, invoiceLineDataRelationship);
					}

					invoiceLineDataRelationship.AddChildInvoiceLineData(lineNo, invoiceLineData);
				}
				else if (lineNo == ZInt.Zero)
				{
					zeroOrInvalidLineNoInvoiceLineDatas.Add(invoiceLineData);
				}
				else
				{
					if (parentLineNo != ZInt.Zero && parentLineNo == lineNo)
					{
						logger.LogBoth(Integration.LogType.Error, Res.GetString("2A3617F3-A285-4CDE-87FD-CB630CD208AC", "Commercial Invoice Line cannot not have the same {1} and {2} ('{0}').", parentLineNo, "LineNo", "ParentLineNo"));
					}

					if (!lineNoDictionary.TryGetValue(lineNo, out invoiceLineDataRelationship))
					{
						invoiceLineDataRelationship = new CommercialInvoiceLineDataRelationship();
						lineNoDictionary.Add(lineNo, invoiceLineDataRelationship);
					}

					invoiceLineDataRelationship.ParentInvoiceLineData.Add(invoiceLineData);
				}
			}
			return lineNoDictionary;
		}

		public sealed class CommercialInvoiceLineDataRelationship
		{
			public CommercialInvoiceLineDataRelationship()
			{
				childInvoiceLineData = new SortedList<ZInt, UniversalCustoms.CommercialInvoiceLine>();
			}

			public DataObjectList<UniversalCustoms.CommercialInvoiceLine> ParentInvoiceLineData
			{
				get { return parentInvoiceLineData ?? (parentInvoiceLineData = new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()); }
			}
			DataObjectList<UniversalCustoms.CommercialInvoiceLine> parentInvoiceLineData;

			public void AddChildInvoiceLineData(ZInt lineNo, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
			{
				var key = lineNo > ZInt.Zero ? lineNo : --zeroValueCounter;
				childInvoiceLineData.Add(key, invoiceLineData);
			}
			ZInt zeroValueCounter;

			public IEnumerable<KeyValuePair<ZInt, UniversalCustoms.CommercialInvoiceLine>> ChildInvoiceLineData
			{
				get { return childInvoiceLineData; }
			}
			readonly SortedList<ZInt, UniversalCustoms.CommercialInvoiceLine> childInvoiceLineData;

			public ZString[] ChildDataImportMatchingKeys => childDataImportMatchingKeys ?? (childDataImportMatchingKeys = ChildInvoiceLineData.Select(c => c.Value.DataImportMatchingKey.GetValueOrDefault()).Where(c => !c.IsEmpty).ToArray());
			ZString[] childDataImportMatchingKeys;
		}

		void UpdateChildInvoiceLineBOsRelatedPropertyInfo(List<BaseJobComInvoiceLine> childInvoiceLineBOs)
		{
			foreach (var addInfoManager in childInvoiceLineBOs.OfType<IAddInfoManager>())
			{
				addInfoManager.UpdateRelatedPropertyInfo();
			}
		}

		BaseJobComInvoiceLine GetMatchingChildInvoiceLine(List<BaseJobComInvoiceLine> childInvoiceLineBOs, UniversalCustoms.CommercialInvoiceLine invoiceLineData, ZString[] matchingKeys)
		{
			var currentMatchingKey = invoiceLineData.DataImportMatchingKey.GetValueOrDefault();

			var filteredMatchingKeys = currentMatchingKey.IsEmpty ? matchingKeys : matchingKeys.Where(c => c != currentMatchingKey);
			var filteredInvoiceLines = childInvoiceLineBOs.Where(c => filteredMatchingKeys.All(d => !d.EqualsIgnoringCase(c.JI_MatchingKey)));

			var result = filteredInvoiceLines.FirstOrDefault(x => IsMatching(x, invoiceLineData) && x.JI_Tariff == invoiceLineData.HarmonisedCode.GetValueOrDefault())
						?? filteredInvoiceLines.FirstOrDefault(x => IsMatching(x, invoiceLineData))
						?? filteredInvoiceLines.FirstOrDefault(x => x.JI_Tariff == invoiceLineData.HarmonisedCode.GetValueOrDefault())
						?? filteredInvoiceLines.FirstOrDefault();

			if (result != null)
			{
				childInvoiceLineBOs.Remove(result);
			}

			return result;
		}

		List<BaseJobComInvoiceLine> GetChildInvoiceLinesAddedBySystem(BaseJobComInvoiceLine parentInvoiceLine)
		{
			return GetChildInvoiceLinesAddedBySystemCore(parentInvoiceLine).OrderBy(x => x.JI_LineNo).ToList();
		}

		protected virtual IEnumerable<BaseJobComInvoiceLine> GetChildInvoiceLinesAddedBySystemCore(BaseJobComInvoiceLine parentInvoiceLine)
		{
			var query = new ZQuery(JobComInvoiceLineSchema.JI_ParentID, parentInvoiceLine.PK);
			query.AddToFilter(JobComInvoiceLineSchema.JI_JZ, parentInvoiceLine.JI_JZ);
			query.FetchOnlyFromLocalCache = true;
			return helper.Load<BaseJobComInvoiceLine>(query);
		}

		protected virtual ZInt? GetParentLineNo(UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			return invoiceLineData.ParentLineNo;
		}

		List<Note> GetMergedNoteCollection(List<Note> invoiceNoteCollection, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData)
		{
			if (commercialInvoiceHeaderRelatedData != null && commercialInvoiceHeaderRelatedData.IsSingleInvoiceData)
			{
				return invoiceNoteCollection.MergeCollection(commercialInvoiceHeaderRelatedData.NoteCollection, true, (Note x, Note y) => { return x.Description == y.Description; });
			}

			return invoiceNoteCollection;
		}

		void ClearExistingDataThatWillBeReplaced(BaseJobComInvoiceHeader invoice, bool shouldAddFetchHint)
		{
			var invoiceLineCollection = dataObject.CommercialInvoiceLineCollection;

			if (invoiceLineCollection != null && invoiceLineCollection.Content == null)
			{
				if (shouldAddFetchHint)
				{
					invoice.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData(new IBusiness[] { invoice.JobComInvoiceLines });
				}

				using (invoice.InvoiceLineLineNumberGenerator.GetLineNumberSuspender())
				{
					invoice.JobComInvoiceLines.RemoveAndDeleteAll();
				}
			}
		}

		void FillRelatedBill(IColumnIndexer invoiceRow, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.BillNumber.HasValue || dataObject.BillType != null)
			{
				var billPK = ZGuid.Empty;
				var billNumber = dataObject.BillNumber.GetValueOrDefault();
				var billType = dataObject.BillType.GetCustomsBillType();
				var billRow = GetColumnIndexerFromRow(factory.RowFactory.FindFirstCusDecHouseBillByBillNumberAndType(invoiceRow.GetValue(JobComInvoiceHeaderSchema.JZ_JE), billNumber, billType));
				if (billRow != null)
				{
					billPK = billRow.GetValue(CusDecHouseBillSchema.PK);
				}

				SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill, billPK, delaySetters);
			}
		}

		void FillCollectionsDataForStandalone(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData)
		{
			if (commercialInvoiceHeaderRelatedData.IsSingleInvoiceData)
			{
				FillTransports(invoice, commercialInvoiceHeaderRelatedData);
			}

			FillInvoiceGroupHeaderCharges(invoice, commercialInvoiceHeaderRelatedData);
			FillContainersForStandAlone(invoice, commercialInvoiceHeaderRelatedData);
			FillBills(invoice, commercialInvoiceHeaderRelatedData);
			FillAdditionalReferences(invoice, commercialInvoiceHeaderRelatedData);
		}

		void FillInvoiceGroupHeaderCharges(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData)
		{
			if (commercialInvoiceHeaderRelatedData.HasGroupChargeData)
			{
				FillCommercialInfo(commercialInvoiceHeaderRelatedData.GroupChargeCollection, invoice, logger, factory);
			}
		}

		protected virtual void FillMarksAndNumbers(BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			var invoiceRow = GetColumnIndexer(invoice);
			SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_MarksAndNumbers, dataObject.MarksAndNumbers, delaySetters);
		}

		void FillNotes(BaseJobComInvoiceHeader invoice, IEnumerable<Note> notes)
		{
			var noteCollection = new DataObjectList<Note>(notes) { Content = CollectionContent.Complete }; // When the InvoiceHeader gets changed across to support CollectionContent, this can be removed.

			new NotesCollectionReader(noteCollection, logger, factory, invoice).ReadIntoCollection();
		}

		void FillTransports(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData dataObject)
		{
			if (dataObject.TransportLegCollection != null)
			{
				var reader = new TransportLegCollectionReader<Transport>(dataObject.TransportLegCollection, logger, factory, invoice);
				reader.ReadIntoCollection();
			}
		}

		void FillContainersForStandAlone(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData)
		{
			ZString[] containerNumbersFromLine = GetDistinctContainerNumbersFromLineLevel();
			if (containerNumbersFromLine.Length > 0 || commercialInvoiceHeaderRelatedData.HasContainerData)
			{
				var existingContainers = new List<JobComInvoiceHeaderRefs>(invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN));
				foreach (var containerNumber in containerNumbersFromLine)
				{
					FillReference(invoice, existingContainers, containerNumber, InvoiceHeaderRefsTypeList.Codes.CN);
				}

				if (commercialInvoiceHeaderRelatedData.HasContainerData)
				{
					foreach (var containerDataObject in commercialInvoiceHeaderRelatedData.ContainerCollection)
					{
						FillReference(invoice, existingContainers, containerDataObject.ContainerNumber.GetValueOrDefault(), InvoiceHeaderRefsTypeList.Codes.CN);
					}
				}

				existingContainers.DeleteAll(true);
			}
		}

		ZString[] GetDistinctContainerNumbersFromLineLevel()
		{
			var result = new List<ZString>();
			if (dataObject.CommercialInvoiceLineCollection != null)
			{
				dataObject.CommercialInvoiceLineCollection.ForEach(x =>
				{
					var containerNumber = x.ContainerNumber.GetValueOrDefault();
					if (!containerNumber.IsEmpty && !result.Contains(containerNumber))
					{
						result.Add(containerNumber);
					}
				});
			}

			return result.ToArray();
		}

		void FillBills(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData)
		{
			var isInvoiceBillDetailSpecified = dataObject.BillNumber.HasValue || dataObject.BillType != null;
			if (isInvoiceBillDetailSpecified || commercialInvoiceHeaderRelatedData.HasBillData)
			{
				var existingBills = new List<JobComInvoiceHeaderRefs>(invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH || x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB || x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB));
				if (commercialInvoiceHeaderRelatedData.HasBillData)
				{
					foreach (var billDataObject in commercialInvoiceHeaderRelatedData.BillCollection)
					{
						FillReference(invoice, existingBills, billDataObject.BillNumber.GetValueOrDefault(), GetBillType(billDataObject.BillType.GetCodeAsUpperCase()));
					}
				}

				if (isInvoiceBillDetailSpecified)
				{
					FillReference(invoice, existingBills, dataObject.BillNumber.GetValueOrDefault(), GetBillType(dataObject.BillType.GetCodeAsUpperCase()));
				}

				existingBills.DeleteAll(true, getAdditionalChildren: AdditionalChildrenHelper.GetAdditionalChildrenIfSupported);
			}
		}

		ZString GetBillType(ZString billType)
		{
			ZString result;
			switch (billType)
			{
				case WayBillTypeList.Codes.Master:
					result = InvoiceHeaderRefsTypeList.Codes.MB;
					break;
				case WayBillTypeList.Codes.House:
					result = InvoiceHeaderRefsTypeList.Codes.HB;
					break;
				case WayBillTypeList.Codes.SubHouse:
					result = InvoiceHeaderRefsTypeList.Codes.SH;
					break;
				default:
					result = ZString.Empty;
					break;
			}

			return result;
		}

		void FillAdditionalReferences(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData)
		{
			if (commercialInvoiceHeaderRelatedData.HasAdditionalReferenceData)
			{
				var newAdditionalReference = commercialInvoiceHeaderRelatedData.AdditionalReferenceCollection.FirstOrDefault(x => x.Type?.Code.GetValueOrDefault().ToString() == InvoiceHeaderRefsTypeList.Codes.RP && !x.ReferenceNumber.GetValueOrDefault().IsEmpty);

				if (newAdditionalReference != null)
				{
					var referenceNumber = newAdditionalReference.ReferenceNumber.GetValueOrDefault();
					var existingAdditionalReference = invoice.InvoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs(InvoiceHeaderRefsTypeList.Codes.RP);

					if (existingAdditionalReference == null)
					{
						CreateNewReference(invoice, referenceNumber, InvoiceHeaderRefsTypeList.Codes.RP);
					}
					else if (existingAdditionalReference.J2_ReferenceNumber != referenceNumber)
					{
						existingAdditionalReference.J2_ReferenceNumber = referenceNumber;
					}
				}
			}
		}

		void FillReference(BaseJobComInvoiceHeader invoice, List<JobComInvoiceHeaderRefs> existingReferences, ZString number, ZString type)
		{
			if (!number.IsEmpty && !type.IsEmpty)
			{
				var query = new ZQuery(JobComInvoiceHeaderRefsSchema.J2_JZ, invoice.PK);
				query.AddToFilter(JobComInvoiceHeaderRefsSchema.J2_ReferenceType, type);
				query.AddToFilter(JobComInvoiceHeaderRefsSchema.J2_ReferenceNumber, number);
				var reference = helper.LoadTop1<JobComInvoiceHeaderRefs>(query);
				if (reference == null)
				{
					CreateNewReference(invoice, number, type);
				}
				else
				{
					existingReferences.Remove(reference);
				}
			}
		}

		void CreateNewReference(BaseJobComInvoiceHeader invoice, ZString number, ZString type)
		{
			var referenceRow = GetColumnIndexer(invoice.InvoiceHeaderRefs.AddNew());
			SetValue(referenceRow, JobComInvoiceHeaderRefsSchema.J2_ReferenceType, type);
			SetValue(referenceRow, JobComInvoiceHeaderRefsSchema.J2_ReferenceNumber, number);
		}

		protected virtual BaseJobComInvoiceLine GetInvoiceLine(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceHeader invoice)
		{
			return invoice.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(c => IsMatching(c, invoiceLineData)) ?? GetNewInvoiceLine(invoice);
		}

		protected virtual BaseJobComInvoiceLine GetNewInvoiceLine(BaseJobComInvoiceHeader invoice)
		{
			return invoice.JobComInvoiceLines.AddNew();
		}

		bool IsMatching(BaseJobComInvoiceLine line, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			return line != null
				&& !line.IsDeleted
				&& !line.JI_MatchingKey.IsEmpty
				&& invoiceLineData != null
				&& line.JI_MatchingKey.EqualsIgnoringCase(invoiceLineData.DataImportMatchingKey.GetValueOrDefault());
		}

		protected virtual BaseJobComInvoiceHeader GetNewInvoice()
		{
			return groupHeader?.JobComInvoiceHeaders.AddNew() ?? (BaseJobComInvoiceHeader)factory.New(invoiceType);
		}

		#region Setting Order - InvoiceHeader

		IEnumerable<ZString> GetSettingOrder(BaseJobComInvoiceHeader invoice)
		{
			return invoice.IsAttachedToPersistentDeclaration ? GetSettingOrderForNormal(invoice) : GetSettingOrderForStandalone(invoice);
		}

		protected virtual IEnumerable<ZString> GetSettingOrderForStandalone(BaseJobComInvoiceHeader invoice)
		{
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Supplier);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Buyer);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceDate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_GB);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceAmount);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRateType);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_IncoTerm);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_Weight);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_WeightUQ);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_NetWeight);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_NetWeightUQ);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_RelatedIndicator);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_ValuationCode);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_MarksAndNumbers);
		}

		protected virtual IEnumerable<ZString> GetSettingOrderForNormal(BaseJobComInvoiceHeader invoice)
		{
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Supplier);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_OH_Buyer);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceAmount);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRateType);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrExRate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_IncoTerm);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_Weight);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_WeightUQ);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_InvoiceCurrLandedCostExRate);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_NoOfPacks);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_NetWeight);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_NetWeightUQ);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_RelatedIndicator);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_ValuationCode);
			yield return ColumnValueSetter.GetKey(invoice.PK, JobComInvoiceHeaderSchema.JZ_MarksAndNumbers);
		}

		#endregion

		protected virtual AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForInvoice(BaseJobComInvoiceHeader invoiceBO)
		{
			return AddInfoDataObjectReader.New(invoiceBO, logger, helper, JobComInvoiceHeaderSchema.JZ_AddInfo);
		}

		protected virtual AddInfoGroupCollectionDataObjectReader GetNewAddInfoGroupCollectionDataObjectReader()
		{
			return new AddInfoGroupCollectionDataObjectReader(logger, helper);
		}

		#region Organization Population - InvoiceHeader

		void FillOrganizations(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.OrganizationAddressCollection != null || dataObject.Supplier != null || dataObject.Buyer != null)
			{
				var invoiceRow = GetColumnIndexer(invoice);
				factory.ClearUnmatchedOrgDetailsNotes(invoiceRow.GetValue(JobComInvoiceHeaderSchema.PK), invoiceRow.TableName, invoice.IsInDatabase);
				FillOrganizationsCore(invoice, commercialInvoiceHeaderRelatedData, delaySetters);
			}
		}

		protected virtual void FillOrganizationsCore(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData, Dictionary<string, ValueSetter> delaySetters)
		{
			var invoiceRow = GetColumnIndexer(invoice);
			if (commercialInvoiceHeaderRelatedData == null) // Buyer and Supplier is already setup by stand alone invoice import
			{
				if (dataObject.Supplier != null)
				{
					OrgAddress supplierAddress = null;
					var infoMatcher = helper.CommercialInfoMatcher;
					if (infoMatcher == null || !infoMatcher.TryGetMatchedSupplier(dataObject.Supplier, out supplierAddress))
					{
						if (!(dataObject.Supplier.AddressOverride ?? false))
						{
							supplierAddress = new OrganisationDataObjectReader(dataObject.Supplier, logger, factory).GetMatched(invoice, OrganisationTypes.Consignor);
						}
						SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OH_Supplier, supplierAddress != null ? supplierAddress.OA_OH : ZGuid.Empty, delaySetters);
						SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, supplierAddress != null ? supplierAddress.PK : ZGuid.Empty, delaySetters);
					}
				}

				if (this.TryGetMatchedOrganisation(out var invoiceSupplierAddress, dataObject, invoice, Constants.AddressTypes.SupplierAddress, OrganisationTypes.Consignor, null))
				{
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OH_Supplier, invoiceSupplierAddress.OA_OH, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, invoiceSupplierAddress.PK, delaySetters);
				}

				if (dataObject.Buyer != null)
				{
					OrgAddress buyerAddress = null;
					if (!(dataObject.Buyer.AddressOverride ?? false))
					{
						buyerAddress = new OrganisationDataObjectReader(dataObject.Buyer, logger, factory).GetMatched(invoice, OrganisationTypes.Consignee);
					}
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OH_Buyer, buyerAddress != null ? buyerAddress.OA_OH : ZGuid.Empty, delaySetters);
				}

				FillBuyerAddress(dataObject, invoice, delaySetters);
			}

			FillManufacturer(dataObject, invoice, delaySetters);
			FillShipToParty(dataObject, invoice, delaySetters);
			FillSeller(dataObject, invoice, delaySetters);
			FillSoldToPartyAddress(dataObject, invoice, delaySetters);
			FillConsigneeAddress(dataObject, invoice, delaySetters);
			FillConsignee(dataObject, invoice, delaySetters);
			FillSellingAgent(dataObject, invoice, delaySetters);
			FillBuyerAgent(dataObject, invoice, delaySetters);
			FillExporterAddress(dataObject, invoice, delaySetters);
			FillIntermConsigneeAddress(dataObject, invoice, delaySetters);
			helper.FillDocAddresses(invoice, dataObject.OrganizationAddressCollection, logger);
		}

		protected void FillManufacturer(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OA_ManufacturerAddress, helper.GetAddressPK(this, invoiceDataObject, invoice, nameof(DocAddressType.Manufacturer), OrganisationTypes.Consignor), delaySetters);
		}

		protected void FillShipToParty(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OA_ShipToPartyAddress, helper.GetAddressPK(this, invoiceDataObject, invoice, nameof(DocAddressType.ShipToParty), OrganisationTypes.Consignee), delaySetters);
		}
		protected void FillSeller(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OA_SellerAddress, helper.GetAddressPK(this, invoiceDataObject, invoice, Constants.AddressTypes.Seller, OrganisationTypes.Consignor), delaySetters);
		}

		protected void FillSoldToPartyAddress(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OA_SoldToPartyAddress, helper.GetAddressPK(this, invoiceDataObject, invoice, Constants.AddressTypes.SoldToParty, OrganisationTypes.Consignee), delaySetters);
		}

		protected void FillBuyerAgent(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OH_BuyerAgent, helper.GetOrganisationPK(this, invoiceDataObject, invoice, Constants.AddressTypes.BuyingAgent, OrganisationTypes.Consignee), delaySetters);
		}

		protected void FillSellingAgent(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OH_SellingAgent, helper.GetOrganisationPK(this, invoiceDataObject, invoice, Constants.AddressTypes.SellingAgent, OrganisationTypes.Consignor), delaySetters);
		}

		protected void FillExporterAddress(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OA_ExporterAddress, helper.GetAddressPK(this, invoiceDataObject, invoice, nameof(DocAddressType.Exporter), OrganisationTypes.Consignor), delaySetters);
		}

		protected void FillConsignee(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OH_Consignee, helper.GetOrganisationPK(this, invoiceDataObject, invoice, Constants.AddressTypes.IntermediateConsignee, OrganisationTypes.Consignee), delaySetters);
		}

		protected void FillConsigneeAddress(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OA_ConsigneeAddress, helper.GetAddressPK(this, invoiceDataObject, invoice, Constants.AddressTypes.UltimateConsignee, OrganisationTypes.Consignee), delaySetters);
		}

		protected void FillBuyerAddress(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OA_BuyerAddress, helper.GetAddressPK(this, invoiceDataObject, invoice, Constants.AddressTypes.BuyerAddress, OrganisationTypes.Consignee), delaySetters);
		}

		protected void FillIntermConsigneeAddress(IOrganizationAddressCollectionParent invoiceDataObject, BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoice), JobComInvoiceHeaderSchema.JZ_OA_IntermediateConsigneeAddress, helper.GetAddressPK(this, invoiceDataObject, invoice, Constants.AddressTypes.IntermediateConsignee, OrganisationTypes.Consignee), delaySetters);
		}

		#endregion

		protected void SetValue(IColumnIndexer row, SchemaGuidColumn column, OrgAddress address, Dictionary<string, ValueSetter> delaySetters)
		{
			if (address != null)
			{
				SetValue(row, column, address.OA_OH, delaySetters);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected virtual void FillCommercialInvoiceLineData(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, ZString invoiceNumber, BaseJobComInvoiceLine parentInvoiceLine, bool isStandalone, bool isDeclarationIntegrated, bool supportsChcPivotBetweenInvoiceLineAndPacking)
		{
			((ISupportDataImporting)invoiceLine).IsImportingData = true;

			var isDefaultingEnabled = IsDefaultingEnabled;
			var invoiceLineIsInDatabase = invoiceLine.IsInDatabase;
			var invoiceLineRow = GetColumnIndexer(invoiceLine);
			var addInfoManager = invoiceLine as IAddInfoManager;
			var isAddInfoSerialisationEnabled = addInfoManager != null && !isDefaultingEnabled;
			IDisposable[] importSettings = null;

			try
			{
				CurrentInvoiceLineData = invoiceLineData;
				currentInvoiceLineDatacustomsReferenceGroupByType = null;
				importSettings = new[]
				{
					GetInvoiceLineImportSettings(invoiceLineData, invoiceLine, isDefaultingEnabled),
					GetInvoiceLineSetterSuspenderSetting(invoiceLineData, invoiceLine)
				};

				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
				}

				var invoiceLinePK = invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK);
				if (invoiceLineData.CommercialChargeCollection != null)
				{
					invoiceLine.ApportionedCharges.RemoveAndDeleteAll();
					invoiceLine.Charges.RemoveAndDeleteAll();
				}

				var delaySetters = isDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CEI, helper.GetEntryInstructionPK(invoiceLineData.EntryInstructionLink), delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_Procedure, invoiceLineData.Procedure, delaySetters);
				if (parentInvoiceLine != null && invoiceLineData.ParentLineNo.HasValue)
				{
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ParentTableCode, parentInvoiceLine.TablePrefix);
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ParentID, parentInvoiceLine.PK);
				}

				if (!isDefaultingEnabled)
				{
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_LineNo, invoiceLineData.LineNo);
				}

				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_PartNo, invoiceLineData.PartNo, delaySetters);
				if (!invoiceLineData.ClassificationCode.GetValueOrDefault().IsEmpty)
				{
					FillClassificationCode(invoiceLineData.ClassificationCode.Value, invoiceLine, delaySetters);
				}

				if (!ImportEmptyHarmonisedCode)
				{
					if (invoiceLineData.HarmonisedCode.HasValue && !invoiceLineData.HarmonisedCode.Value.IsEmpty)
					{
						SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_Tariff, invoiceLineData.HarmonisedCode, delaySetters);
					}
				}
				else
				{
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_Tariff, invoiceLineData.HarmonisedCode, delaySetters);
				}
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_InvoiceQuantity, GetInvoiceQuantity(invoiceLineData.InvoiceQuantity), delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_InvoiceUQ, invoiceLineData.InvoiceQuantityUnit, delaySetters);

				if ((!invoiceLineData.LinePrice.HasValue || invoiceLineData.LinePrice == 0m) && invoiceLineData.UnitPrice.GetValueOrDefault() > 0m && invoiceLineData.InvoiceQuantity.GetValueOrDefault() > 0m)
				{
					SetValueWithDelay(invoiceLineRow, JobComInvoiceLineSchema.JI_LinePrice, () => invoiceLine.CalculateLinePriceFromUnitPrice(invoiceLineData.InvoiceQuantity.Value, invoiceLineData.UnitPrice.Value), delaySetters);
				}
				else
				{
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_LinePrice, invoiceLineData.LinePrice, delaySetters);
				}

				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CountryOfOrigin, invoiceLineData.CountryOfOrigin, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_StateOrRegionOfOrigin, invoiceLineData.StateOfOrigin, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_RN_NKCountryOfExport, invoiceLineData.CountryOfExport, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_Weight, invoiceLineData.Weight, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_WeightUQ, invoiceLineData.WeightUnit, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_Volume, invoiceLineData.Volume, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_VolumeUQ, invoiceLineData.VolumeUnit, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_NetWeight, invoiceLineData.NetWeight, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_NetWeightUQ, invoiceLineData.NetWeightUnit, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_RH_NKCommodity_Code, invoiceLineData.Commodity, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_PrimaryPreference, invoiceLineData.PrimaryPreference, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_SecondaryPreference, invoiceLineData.SecondaryPreference, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_RelatedIndicator, invoiceLineData.RelatedIndicator, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_Description, invoiceLineData.Description, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_OrderNumber, invoiceLineData.OrderNumber, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ContainerMode, invoiceLineData.ContainerMode, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsQuantity, invoiceLineData.CustomsQuantity, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsUnitQty, invoiceLineData.CustomsQuantityUnit, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsSecondQuantity, invoiceLineData.CustomsSecondQuantity, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsSecondUnitQty, invoiceLineData.CustomsSecondQuantityUnit, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsThirdQuantity, invoiceLineData.CustomsThirdQuantity, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsThirdUnitQty, invoiceLineData.CustomsThirdQuantityUnit, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsFourthQuantity, invoiceLineData.CustomsFourthQuantity, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsFourthUnitQty, invoiceLineData.CustomsFourthQuantityUnit, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsFifthQuantity, invoiceLineData.CustomsFifthQuantity, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomsFifthUnitQty, invoiceLineData.CustomsFifthQuantityUnit, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_PreviousEntryNumber, invoiceLineData.PreviousEntryNumber, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_PreviousEntryLineNumber, invoiceLineData.PreviousEntryLineNumber, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ZZF_NKTaxType, invoiceLineData.TaxType, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ValuationCode, invoiceLineData.ValuationCode, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ValuationMarkup, invoiceLineData.ValuationMarkup, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_BrandName, invoiceLineData.BrandName, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_Model, invoiceLineData.Model, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_NDescription, invoiceLineData.LocalDescription, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ConcessionOrder, invoiceLineData.ConcessionOrder, delaySetters);
				FillMatchingKey(invoiceLineData, invoiceLineRow);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ClassUsageComment, invoiceLineData.ClassUsageComment);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_GS_NKClassUsageCommentReviewer, invoiceLineData.ClassUsageCommentStaff);

				FillBondedWarehouseProperties(invoiceLineData, invoiceLine, delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_JO, helper.GetMatchingOrderPK(invoiceLineData, invoiceLine, TopLevelDataObject, logger), delaySetters);
				var hazardousMaterial = invoiceLineData.HazardousMaterial;
				if (hazardousMaterial != null)
				{
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_HazMatCode, hazardousMaterial.Code, delaySetters);
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_HazMatCodeQualifier, hazardousMaterial.CodeType, delaySetters);
					if (hazardousMaterial.UNDGCollection != null)
					{
						var query = new ZQuery(UNDGDataItemSchema.DI_ParentID, invoiceLinePK)
						{
							FetchOnlyFromLocalCache = !invoiceLineIsInDatabase
						};

						helper.DeleteAll(UNDGDataItemSchema.Instance, query);
						foreach (var undgData in hazardousMaterial.UNDGCollection)
						{
							var undgBO = new UNDGDataObjectReader(undgData, logger, factory).ReadIntoBusinessObject();
							var undgRow = GetColumnIndexer(undgBO);
							SetValue(undgRow, UNDGDataItemSchema.DI_ParentID, invoiceLinePK);
							SetValue(undgRow, UNDGDataItemSchema.DI_ParentTableCode, JobComInvoiceLineSchema.Constants.Prefix);
						}
					}
				}

				if (addInfoManager != null && helper.IsSourceAndTargetCountrySame)
				{
					GetNewAddInfoDataObjectReaderForInvoiceLine(invoiceLine, invoiceLineData).ReadIntoRow(addInfoManager, invoiceLineRow, invoiceLineData, delaySetters);
				}

				if (invoiceLineData.AddInfoGroupCollection != null)
				{
					foreach (var addInfoGroup in invoiceLineData.AddInfoGroupCollection)
					{
						if (addInfoGroup.Type.Code.ToString() == CusAddInfoTypeAttribute.Codes.WarehouseAllocationInfo)
						{
							var componentInventory = invoiceLine.ComponentInventoryCollection.AddNew();
							foreach (var addInfo in addInfoGroup.AddInfoCollection)
							{
								switch (addInfo.Key)
								{
									case Constants.AddInfoKeys.AllocationInfo.AllocationKey:
										SetValue(componentInventory, JobComInvLineComponentInventorySchema.JIV_AllocationKey, addInfo.Value, delaySetters);
										break;
									case Constants.AddInfoKeys.AllocationInfo.Quantity:
										if (addInfo.Value.HasValue)
										{
											SetValue(componentInventory, JobComInvLineComponentInventorySchema.JIV_QuantityToDraw, ZDecimal.Parse(addInfo.Value.Value), delaySetters);
										}
										break;
								}
							}
						}
					}
				}

				if (invoiceLineData.CustomAttributeCollection != null)
				{
					foreach (var customAttribute in invoiceLineData.CustomAttributeCollection)
					{
						var key = customAttribute.Key;
						var attributeValue = customAttribute.Value;
						if (key.HasValue && attributeValue.HasValue)
						{
							switch (key.Value)
							{
								case Constants.CustomAttributeKeys.CustomAttribute1:
									SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomAttrib1, attributeValue.Value);
									break;
								case Constants.CustomAttributeKeys.CustomAttribute2:
									SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomAttrib2, attributeValue.Value);
									break;
								case Constants.CustomAttributeKeys.CustomAttribute3:
									SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomAttrib3, attributeValue.Value);
									break;
								case Constants.CustomAttributeKeys.CustomAttribute4:
									SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomAttrib4, attributeValue.Value);
									break;
								case Constants.CustomAttributeKeys.CustomAttribute5:
									SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomAttrib5, attributeValue.Value);
									break;
								case Constants.CustomAttributeKeys.CustomAttribute6:
									SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CustomAttrib6, attributeValue.Value);
									break;
								default:
									logger.Log(LogType.Warning, Res.GetString("c8bf07fb-48eb-4553-9f63-0e52a8fe8749", "Unknown CustomAttribute key '{0}'", key.Value));
									break;
							}
						}
					}
				}

				if (isDeclarationIntegrated)
				{
					LinkToEntryLine(invoiceLineData, invoiceLineRow, delaySetters, invoiceNumber);
				}

				FillContainerOrPackagesPivots(isStandalone, invoiceLine, invoiceLineData, supportsChcPivotBetweenInvoiceLineAndPacking);

				FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);
				FillCountrySpecificAddInfoDetails(invoiceLine);
				FillOrganizations(invoiceLineData, invoiceLine, delaySetters);
				delaySetters.SetValueOnSetterSupenderParentInSpecificOrder(invoiceLine.SetterSuspender, GetSettingOrder(invoiceLine));
				if (addInfoManager != null && isDefaultingEnabled)
				{
					addInfoManager.UpdateRelatedPropertyInfo();
				}

				FillAdditionalLineTariffDetails(invoiceLineData, invoiceLine);
				CommercialChargeDataObjectReader<BaseInvoiceLineCharge>.FillCommercialInfo(invoiceLineData.CommercialChargeCollection, invoiceLine, logger, factory);
				var tablePrefix = JobComInvoiceLineSchema.Constants.Prefix;
				if (helper.IsSourceAndTargetCountrySame)
				{
					GetNewAddInfoGroupCollectionDataObjectReader().ReadIntoDataRows(invoiceLinePK, tablePrefix, invoiceLineIsInDatabase, invoiceLineData);
					new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(invoiceLinePK, tablePrefix, invoiceLineIsInDatabase, invoiceLineData);
					if (invoiceLine.SupportsJobComInvoiceLineTax)
					{
						new TaxOrFeeCollectionDataObjectReader(logger, helper).ReadIntoDataRows(invoiceLinePK, tablePrefix, invoiceLineIsInDatabase, invoiceLineData);
					}

					if (invoiceLine is Integration.Customs.ICusSupportingInfoTypeSupporter)
					{
						CreateNewCustomsSupportingInformationCollectionDataObjectReader().ReadIntoDataRows(invoiceLinePK, tablePrefix, invoiceLineIsInDatabase, invoiceLineData);
					}
				}

				var usedCustomFields = new CustomLabelsCustomizedFieldDataObjectReader(logger).PopulateCustomFields(JobComInvoiceLineSchema.Instance, invoiceLineRow, invoiceLineData, helper.GetJobComInvoiceLineCustomLabelsProvider(invoiceLineRow));
				new CustomFieldsDataObjectReader<BaseJobComInvoiceLine>(invoiceLineData, logger, factory).PopulateCustomFields(invoiceLine, usedCustomFields);
				FillCustomizedFields(invoiceLineData, invoiceLine);
			}
			finally
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.UpdateAddInfoFromString(invoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_AddInfo));
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
				}

				if (importSettings != null)
				{
					importSettings.ForEach(c => c?.Dispose());
					importSettings = null;
				}

				CurrentInvoiceLineData = null;
				currentInvoiceLineDatacustomsReferenceGroupByType = null;
			}

			if (landedCostDataReader != null)
			{
				landedCostDataReader.CollectTransportLogisticsCost(invoiceLine, invoiceLineData);
			}
		}

		protected virtual void FillMatchingKey(UniversalCustoms.CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow)
		{
			SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_MatchingKey, invoiceLineData.DataImportMatchingKey);
		}

		protected UniversalCustoms.CommercialInvoiceLine CurrentInvoiceLineData { get; private set; }
		protected IDictionary<ZString, List<UniversalCustoms.CustomsReference>> CurrentInvoiceLineDataCustomsReferenceGroupByType => CurrentInvoiceLineData?.CustomsReferenceCollection.GetOrCreateCodeDictionary(ref currentInvoiceLineDatacustomsReferenceGroupByType, x => x.Type);
		IDictionary<ZString, List<UniversalCustoms.CustomsReference>> currentInvoiceLineDatacustomsReferenceGroupByType;

		protected virtual bool ImportEmptyHarmonisedCode => true;

		protected virtual ZDecimal? GetInvoiceQuantity(ZDecimal? quantity) => quantity;

		protected IDisposable GetInvoiceLineSetterSuspenderSetting(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
			var columns = GetColumnNamesForSuspendSetting(invoiceLineData);
			return columns.Any() ? invoiceLine.SetterSuspender.SuspendSetting(columns.ToArray()) : DisposableAction.NoAction;
		}

		protected virtual IEnumerable<ZString> GetColumnNamesForSuspendSetting(UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			if (invoiceLineData.HarmonisedCode.HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_Tariff;
			}

			if (invoiceLineData?.InvoiceQuantityUnit?.Code?.IsValid ?? false)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_InvoiceUQ;
			}

			if ((invoiceLineData.CustomsQuantityUnit?.Code).HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty;
			}

			if (invoiceLineData.CustomsQuantity.HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_CustomsQuantity;
			}

			if ((invoiceLineData.CustomsSecondQuantityUnit?.Code).HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty;
			}

			if (invoiceLineData.CustomsSecondQuantity.HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity;
			}

			if ((invoiceLineData.CustomsThirdQuantityUnit?.Code).HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_CustomsThirdUnitQty;
			}

			if (invoiceLineData.CustomsThirdQuantity.HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_CustomsThirdQuantity;
			}

			if ((invoiceLineData.CustomsFourthQuantityUnit?.Code).HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_CustomsFourthUnitQty;
			}

			if (invoiceLineData.CustomsFourthQuantity.HasValue)
			{
				yield return JobComInvoiceLineSchema.Constants.JI_CustomsFourthQuantity;
			}
		}

		protected virtual void FillCustomizedFields(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
		}

		protected virtual void FillAdditionalLineTariffDetails(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
			var targetParent = invoiceLine as IAdditionalLineTariffDetailParent;
			var sourceCollection = invoiceLineData.AdditionalLineTariffDetailCollection;
			if (targetParent?.CusLineTariffDetails != null && sourceCollection != null)
			{
				foreach (var additionalLineTariffDetail in sourceCollection)
				{
					var reader = CreateAdditionalLineTariffDetailDataObjectReader(additionalLineTariffDetail, logger, factory, helper, targetParent);
					reader.ReadIntoBusinessObject();
				}
			}
		}

		protected virtual AdditionalLineTariffDetailDataObjectReader CreateAdditionalLineTariffDetailDataObjectReader(UniversalCustoms.AdditionalLineTariffDetail dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalDataObjectReaderHelper helper, IAdditionalLineTariffDetailParent parent)
		{
			return new AdditionalLineTariffDetailDataObjectReader(dataObject, logger, factory, helper, parent);
		}

		protected virtual IDisposable GetInvoiceLineImportSettings(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, bool isDefaultingEnabled)
		{
			return null;
		}

		protected void FillContainerOrPackagesPivots(bool isStandalone, BaseJobComInvoiceLine invoiceLine, UniversalCustoms.CommercialInvoiceLine invoiceLineData, bool supportsChcPivotBetweenInvoiceLineAndPacking)
		{
			if (!isStandalone)
			{
				FillContainerPivots(invoiceLine, invoiceLineData, supportsChcPivotBetweenInvoiceLineAndPacking);
				if (!invoiceLevelPackagePivotExist)
				{
					if (supportsChcPivotBetweenInvoiceLineAndPacking)
					{
						FillPackagePivots(invoiceLine, invoiceLineData);
					}
				}
				else
				{
					logger.Log(Integration.LogType.Warning, Res.GetString("E62A28E2-C429-4F53-A392-C170E29B25E9", "The invoice line level package pivots will not be imported because invoice level package pivots exist."));
				}
			}
		}

		void FillContainerPivots(BaseJobComInvoiceLine invoiceLine, UniversalCustoms.CommercialInvoiceLine invoiceLineData, bool supportsChcPivotBetweenInvoiceLineAndPacking)
		{
			var containerInvoiceLineMap = helper.GetContainerInvoiceLineMapFor(invoiceLineData.Link.GetValueOrDefault());
			if (containerInvoiceLineMap != null)
			{
				foreach (var mappedData in containerInvoiceLineMap)
				{
					var pivotRow = GetColumnIndexer(invoiceLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().FirstOrDefault(x => x.C2_CO == mappedData.Key));
					if (pivotRow == null)
					{
						pivotRow = GetColumnIndexer(invoiceLine.ContainersPivot.AddNew());
						SetValue(pivotRow, CusContainerInvoiceLinePivotSchema.C2_CO, mappedData.Key);
					}

					if (!supportsChcPivotBetweenInvoiceLineAndPacking)
					{
						var packedItem = mappedData.Value;

						SetValue(pivotRow, CusContainerInvoiceLinePivotSchema.C2_GrossWeight, Core.Constants.Weight.ConvertSafe(packedItem.GrossWeight.GetValueOrDefault(), packedItem.GrossWeightUnit.GetCodeAsUpperCase(), Core.Constants.Weight.Kilograms, false));
						SetValue(pivotRow, CusContainerInvoiceLinePivotSchema.C2_NetWeight, Core.Constants.Weight.ConvertSafe(packedItem.NetWeight.GetValueOrDefault(), packedItem.NetWeightUnit.GetCodeAsUpperCase(), Core.Constants.Weight.Kilograms, false));
						SetValue(pivotRow, CusContainerInvoiceLinePivotSchema.C2_PackQty, packedItem.PackedQuantity.GetValueOrDefault().ToZInt());
						SetValue(pivotRow, CusContainerInvoiceLinePivotSchema.C2_SplitValue, packedItem.GoodsValue);
					}
				}
			}
		}

		void FillPackInvoiceHeaderPivots(BaseJobComInvoiceHeader invoice, UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			if (invoiceData.PackingLinkCollection != null)
			{
				foreach (var packingLink in invoiceData.PackingLinkCollection)
				{
					var chzPackageCountFromXml = packingLink.PackedQuantity.GetValueOrDefault().ToZInt();
					var packagePk = helper.GetPackagePKFromPackageLinkMap(packingLink.PackingLineLink.GetValueOrDefault());
					if (chzPackageCountFromXml > 0 && !packagePk.IsEmpty)
					{
						invoiceLevelPackagePivotExist = true;
						var pivotRow = GetColumnIndexer(invoice.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().FirstOrDefault(x => x.CHZ_CW == packagePk));
						if (pivotRow == null)
						{
							pivotRow = GetColumnIndexer(invoice.PackagesPivot.AddNew());
							SetValue(pivotRow, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_CW, packagePk);
						}

						SetValue(pivotRow, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE, invoice.JobDeclaration.PK);
						SetValue(pivotRow, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_NumberOfPacks, chzPackageCountFromXml);
					}
				}
			}
		}

		void FillPackagePivots(BaseJobComInvoiceLine invoiceLine, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			var packageInvoiceLineMap = helper.GetPackageInvoiceLineMapFor(invoiceLineData.Link.GetValueOrDefault());
			if (packageInvoiceLineMap != null)
			{
				foreach (var mappedData in packageInvoiceLineMap)
				{
					var packedItem = mappedData.Value;
					var pivotRow = GetColumnIndexer(invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault(x => x.CHC_CW == mappedData.Key));
					if (pivotRow == null)
					{
						pivotRow = GetColumnIndexer(invoiceLine.PackagesPivot.AddNew());
						SetValue(pivotRow, CusHouseContPackInvoiceLinePivotSchema.CHC_CW, mappedData.Key);
					}

					SetValue(pivotRow, CusHouseContPackInvoiceLinePivotSchema.CHC_NumberOfPacks, packedItem.PackedQuantity.GetValueOrDefault().ToZInt());
					SetValue(pivotRow, CusHouseContPackInvoiceLinePivotSchema.CHC_JE, invoiceLine.Declaration.PK);
				}
			}
		}

		protected virtual void FillBondedWarehouseProperties(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			var invoiceLineRow = GetColumnIndexer(invoiceLine);
			SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_BondedWhsQuantity, invoiceLineData.BondedWarehouseQuantity, delaySetters);
			SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_BondedWhsUnitQty, invoiceLineData.BondedWarehouseQuantityUnit, delaySetters);
			SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_BondedWarehouseRemarks, invoiceLineData.BondedWarehouseRemarks, delaySetters);
			SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_BondedWHSOrderNumber, invoiceLineData.BondedWHSOrderNumber, delaySetters);
			SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_BondedWHSOrderLineNumber, invoiceLineData.BondedWHSOrderLineNumber, delaySetters);
		}

		#region Setting Order - InvoiceLine
		protected virtual IEnumerable<ZString> GetSettingOrder(BaseJobComInvoiceLine invoiceLine)
		{
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CEI);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Procedure);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_PartNo);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Tariff);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_InvoiceQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_InvoiceUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_LinePrice);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Description);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CountryOfOrigin);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_RH_NKCommodity_Code);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_PrimaryPreference);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Weight);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_WeightUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_NetWeight);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_NetWeightUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Volume);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_VolumeUQ);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_OrderNumber);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsUnitQty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_PreviousEntryNumber);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_PreviousEntryLineNumber);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_ZZF_NKTaxType);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsSecondQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsSecondUnitQty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsThirdQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsThirdUnitQty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsFourthQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_CustomsFourthUnitQty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_BondedWhsQuantity);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_BondedWhsUnitQty);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_ValuationMarkup);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_BrandName);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_Model);
			yield return ColumnValueSetter.GetKey(invoiceLine.PK, JobComInvoiceLineSchema.JI_NDescription);
		}

		#endregion

		void LinkToEntryLine(UniversalCustoms.CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, ZString invoiceNumber)
		{
			if (invoiceLineData.EntryNumber.HasValue || invoiceLineData.EntryLineNumber.HasValue)
			{
				if (!invoiceLineData.EntryNumber.HasValue || invoiceLineData.EntryNumber.Value.IsEmpty)
				{
					logger.LogBoth(Integration.LogType.Error, Res.GetString("D7F6867E-25DC-443E-B960-BA74D89EEC37", "Commercial Invoice Line must specified {0} when {1} is specified.", "EntryNumber", "EntryLineNumber"));
					return;
				}

				if (!invoiceLineData.EntryLineNumber.HasValue || invoiceLineData.EntryLineNumber.Value.IsEmpty)
				{
					logger.LogBoth(Integration.LogType.Error, Res.GetString("E6C93A7A-B792-40E0-9907-CF57C69936EF", "Commercial Invoice Line must specified {0} when {1} is specified.", "EntryLineNumber", "EntryNumber"));
					return;
				}

				var entryNumber = invoiceLineData.EntryNumber.GetValueOrDefault();
				var entryLineNumber = invoiceLineData.EntryLineNumber.GetValueOrDefault();
				var entryLines = helper.GetEntryLines(entryNumber, entryLineNumber);
				if (entryLines.Length > 1)
				{
					throw new DataObjectReadFailureException(Res.GetString("694BE4A3-9FDA-4310-8C76-04444A106CF2", "Multiple Entry Lines were matched to Commercial Invoice Line '{0}' ({1}='{2}', {3}='{4}', {5}='{6}').", invoiceLineData.LineNo.GetValueOrDefault(), "InvoiceNumber", invoiceNumber, "EntryNumber", entryNumber, "EntryLineNumber", entryLineNumber));
				}
				else if (entryLines.Length == 1)
				{
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_CL, entryLines[0].PK, delaySetters);
				}
				else
				{
					throw new DataObjectReadFailureException(Res.GetString("E38DBE5D-743B-4A8B-95F3-1EB889F1FEA4", "No Entry Line was matched to Commercial Invoice Line '{0}' ({1}='{2}', {3}='{4}', {5}='{6}').", invoiceLineData.LineNo.GetValueOrDefault(), "InvoiceNumber", invoiceNumber, "EntryNumber", entryNumber, "EntryLineNumber", entryLineNumber));
				}
			}
		}

		protected virtual void FillCountrySpecificDetails(UniversalCustoms.CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
		{
		}

		protected virtual void FillCountrySpecificAddInfoDetails(BaseJobComInvoiceLine invoiceLine)
		{
		}

		protected virtual AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForInvoiceLine(BaseJobComInvoiceLine invoiceLineBO, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			return AddInfoDataObjectReader.New(invoiceLineBO, logger, helper, JobComInvoiceLineSchema.JI_AddInfo);
		}

		#region Organization Population - InvoiceLine

		protected void FillOrganizations(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			if (invoiceLineData.OrganizationAddressCollection != null)
			{
				var invoiceRow = GetColumnIndexer(invoiceLine);
				factory.ClearUnmatchedOrgDetailsNotes(invoiceRow.GetValue(JobComInvoiceLineSchema.PK), invoiceRow.TableName, invoiceLine.IsInDatabase);
				FillOrganizationsCore(invoiceLineData, invoiceLine, delaySetters);
			}
		}

		protected virtual void FillOrganizationsCore(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			FillManufacturer(invoiceLineData, invoiceLine, delaySetters);
			FillConsignee(invoiceLineData, invoiceLine, delaySetters);
			FillShipToParty(invoiceLineData, invoiceLine, delaySetters);
			FillSeller(invoiceLineData, invoiceLine, delaySetters);
			FillSoldToPartyAddress(invoiceLineData, invoiceLine, delaySetters);
			FillExporterAddress(invoiceLineData, invoiceLine, delaySetters);
			FillConsigneeAddress(invoiceLineData, invoiceLine, delaySetters);
			helper.FillDocAddresses(invoiceLine as IDocAddresses, invoiceLineData.OrganizationAddressCollection, logger);
		}

		protected void FillManufacturer(IOrganizationAddressCollectionParent invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoiceLine), JobComInvoiceLineSchema.JI_OA_ManufacturerAddress, helper.GetAddressPK(this, invoiceLineData, invoiceLine, nameof(DocAddressType.Manufacturer), OrganisationTypes.Consignor), delaySetters);
		}

		protected void FillConsignee(IOrganizationAddressCollectionParent invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoiceLine), JobComInvoiceLineSchema.JI_OA_ConsigneeAddress, helper.GetAddressPK(this, invoiceLineData, invoiceLine, nameof(DocAddressType.ConsigneeAddress), OrganisationTypes.Consignee), delaySetters);
		}

		protected void FillSeller(IOrganizationAddressCollectionParent invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoiceLine), JobComInvoiceLineSchema.JI_OA_Seller, helper.GetAddressPK(this, invoiceLineData, invoiceLine, Constants.AddressTypes.Seller, OrganisationTypes.Consignor), delaySetters);
		}

		protected void FillShipToParty(IOrganizationAddressCollectionParent invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoiceLine), JobComInvoiceLineSchema.JI_OA_ShipToPartyAddress, helper.GetAddressPK(this, invoiceLineData, invoiceLine, nameof(DocAddressType.ShipToParty), OrganisationTypes.Consignee), delaySetters);
		}

		protected void FillSoldToPartyAddress(IOrganizationAddressCollectionParent invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoiceLine), JobComInvoiceLineSchema.JI_OA_SoldToPartyAddress, helper.GetAddressPK(this, invoiceLineData, invoiceLine, Constants.AddressTypes.SoldToParty, OrganisationTypes.Consignee), delaySetters);
		}

		protected void FillExporterAddress(IOrganizationAddressCollectionParent invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoiceLine), JobComInvoiceLineSchema.JI_OA_ExporterAddress, helper.GetAddressPK(this, invoiceLineData, invoiceLine, nameof(DocAddressType.Exporter), OrganisationTypes.Consignor), delaySetters);
		}

		protected virtual void FillConsigneeAddress(IOrganizationAddressCollectionParent invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(GetColumnIndexer(invoiceLine), JobComInvoiceLineSchema.JI_OA_ConsigneeAddress, helper.GetAddressPK(this, invoiceLineData, invoiceLine, Constants.AddressTypes.UltimateConsignee, OrganisationTypes.Consignee), delaySetters);
		}

		#endregion

		#region Classification - InvoiceLine
		void FillClassificationCode(ZString code, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			var countryCode = helper.TargetCountryCode;
			var query = new ZQuery(CusClassificationSchema.CC_LookupCode, code.ToUpper());
			query.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, countryCode);
			query.AddToFilter(CusClassificationSchema.CC_ClassificationType, GetClassificationType(invoiceLine));
			var classification = factory.LoadTop1<BaseCusClassification>(query);
			if (classification == null)
			{
				query = new ZQuery(CusClassificationSchema.CC_LookupCode, code.ToUpper());
				query.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, countryCode);
				classification = factory.LoadTop1<BaseCusClassification>(query);
			}
			if (classification != null)
			{
				SetValue(GetColumnIndexer(invoiceLine), JobComInvoiceLineSchema.JI_CC, classification.PK, delaySetters);
			}
		}

		protected virtual ZString GetClassificationType(BaseJobComInvoiceLine invoiceLine)
		{
			var result = ZString.Empty;
			if ((invoiceLine.UseExportClassification && invoiceLine.UseImportClassification) || (!invoiceLine.UseExportClassification && !invoiceLine.UseImportClassification))
			{
				result = BaseCusClassification.ClassificationType.Both;
			}
			else if (invoiceLine.UseExportClassification)
			{
				result = BaseCusClassification.ClassificationType.EXP;
			}
			else if (invoiceLine.UseImportClassification)
			{
				result = BaseCusClassification.ClassificationType.IMP;
			}
			return result;
		}

		#endregion

		#region IOrganisationDataObjectReaderSupporter Members

		OrganisationDataObjectReader IOrganisationDataObjectReaderSupporter.CreateNewReader(OrganizationAddress addressData)
		{
			return new OrganisationDataObjectReader(addressData, logger, factory);
		}

		#endregion

		public ITopLevelDataObject TopLevelDataObject
		{
			get { return topLevelObject; }
		}
	}
}
