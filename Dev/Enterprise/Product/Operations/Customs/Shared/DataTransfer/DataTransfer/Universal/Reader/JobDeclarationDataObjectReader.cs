using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
 using Enterprise.ComplianceRisk.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.UniversalDataBuss.Management.ShipmentProcessing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using ValueSetter = Enterprise.UniversalDataBuss.DataObjects.Core.ValueSetter;

namespace Enterprise.Customs.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class JobDeclarationDataObjectReader : JobDeclarationDataObjectReader<BaseJobDeclaration, Bill, BaseCusContainer, BaseJobComInvoiceGroupHeader>
	{
		public JobDeclarationDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
			: base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, BaseJobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		protected override AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			return new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
		}

		protected override CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, UniversalShipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}
	}

	public abstract class JobDeclarationDataObjectReader<TDeclaration, TBill, TContainer, TInvoiceGroupHeader> : ShipmentDataObjectReader<TDeclaration>, IOrganisationDataObjectReaderSupporter, ITopLevelDataObjectReader
		where TDeclaration : BaseJobDeclaration
		where TBill : Bill
		where TContainer : BaseCusContainer
		where TInvoiceGroupHeader : BaseJobComInvoiceGroupHeader
	{
		protected JobDeclarationDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null)
			: base(declarationDataObject, logger, factory)
		{
			this.Shipment = shipment;
			this.shipmentDataSource = declarationDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment);
			this.consolDataSource = declarationDataObject.GetMatchingDataSource(DataContextType.ForwardingConsol);
			this.shipmentDataObject = IsFromFreight ? GetShipmentDataObject() : null;
			this.allowUpdateOfCustomsDeclarationAfterCommencement = declarationDataObject.AllowUpdateOfCustomsDeclarationAfterCommencement.GetValueOrDefault();
			this.commericalInvoiceDataOnly = declarationDataObject.GetMatchingDataTarget(DataContextType.ForwardingShipment) != null
																		&& declarationDataObject.GetMatchingDataTarget(DataContextType.CustomsCommercialInvoice) != null
																		&& declarationDataObject.GetMatchingDataTarget(DataContextType.CustomsDeclaration) == null;
		}

		protected ForwardingShipment Shipment { get; private set; }
		protected readonly UniversalShipment shipmentDataObject;
		protected readonly IDataSourceDataObject shipmentDataSource;
		protected readonly IDataSourceDataObject consolDataSource;
		protected readonly bool commericalInvoiceDataOnly;
		protected readonly bool allowUpdateOfCustomsDeclarationAfterCommencement;

		protected UniversalShipment consolDataObject
		{
			get { return HasConsolDataObject ? dataObject : null; }
		}

		protected UniversalDataObjectReaderHelper Helper
		{
			get { return helper ?? (helper = CreateNewUniversalDataObjectReaderHelper()); }
		}
		UniversalDataObjectReaderHelper helper;

		UniversalDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper()
		{
			var targetCountryCode = dataObject.GetTargetCountryCode();
			var sourceCountryCode = dataObject.GetSourceCountryCode();
			var dataProviderForCodeMapping = dataObject.GetDataProviderForCodeMapping();

			IUniversalCustomsDataObjectProvider provider = null;
			var applicationCode = dataObject.MessagingApplicationCode?.Code.GetValueOrDefault() ?? ZString.Empty;
			if (!applicationCode.IsEmpty)
			{
				provider = factory.BOFactory.GetApplicationSpecificUniversalCustomsDataObjectProvider(applicationCode);
			}

			if (provider == null)
			{
				provider = factory.BOFactory.GetUniversalCustomsDataObjectProvider(targetCountryCode);
			}
			UniversalDataObjectReaderHelper result = null;
			if (provider != null)
			{
				result = provider.GetNewUniversalDataObjectReaderHelper(factory, sourceCountryCode, dataProviderForCodeMapping);
			}
			return result ?? new UniversalDataObjectReaderHelper(factory, targetCountryCode, sourceCountryCode, dataProviderForCodeMapping);
		}

		protected virtual TDeclaration[] FilterUsingCountrySpecificBusinessRules(TDeclaration[] declarations, BillDetail masterBillDetail)
		{
			return declarations;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override TDeclaration GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			if (Shipment != null)
			{
				return GetDeclarationFromShipment();
			}

			TDeclaration[] existingDeclarations;

			if (Helper.IsInterfaceEnabledCompany)
			{
				entryDetails = GetEntryDetails();
				if (entryDetails != null && entryDetails.Any())
				{
					var declarationResult = new ZDBOnlyQuery(typeof(TDeclaration));
					declarationResult.AddToFilter(JobDeclarationSchema.JE_IsCancelled, ZBool.False);
					var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
					foreach (var entryDetail in entryDetails)
					{
						var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
						entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, entryDetail.MessageType);
						entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, entryDetail.EntryReference);
						entryQuery.AddToFilter(entryHeaderQuery, JoinCondition.Or);
					}
					declarationResult.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, entryQuery, JoinCondition.And);
					existingDeclarations = GetExistingMatchingAnyBranch(declarationResult).Where(x => x.IsDeclarationIntegrated).ToArray();
					if (existingDeclarations.Length > 1)
					{
						isMultipleDeclarationsMatchedWithEntryDetails = true;
						return null;
					}
					if (existingDeclarations.Length == 1)
					{
						return existingDeclarations[0];
					}
				}
			}

			if (CanMatchByCountrySpecificBusinessRules)
			{
				return GetExistingBusinessObjectUsingCountrySpecificBusinessRules();
			}

			return GetExistingBusinessObjectUsingBills();
		}

		protected TDeclaration GetExistingBusinessObjectUsingBills()
		{
			var masterBillDetail = GetPrimaryMasterBillDetail();
			if (masterBillDetail != null && !masterBillDetail.BillNumber.GetValueOrDefault().IsEmpty)
			{
				var query = new ZQuery(JobDeclarationSchema.JE_IsCancelled, ZBool.False);
				var mAWBRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
				if (mAWBRecyclePeriod > 0)
				{
					query.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddMonths(-mAWBRecyclePeriod));
				}

				query.AddToFilter(JobDeclarationSchema.JE_MasterBill, masterBillDetail.BillNumber.Value);
				var houseBillDetail = GetPrimaryHouseBillDetail();
				if (houseBillDetail != null && houseBillDetail.BillNumber.HasValue)
				{
					query.AddToFilter(JobDeclarationSchema.JE_HouseBill, houseBillDetail.BillNumber.Value);
				}

				var existingDeclarations = GetExistingMatchingAnyBranch(query);

				existingDeclarations = FilterUsingCountrySpecificBusinessRules(existingDeclarations, masterBillDetail);
				if (existingDeclarations == null)
				{
					return null;
				}

				if (existingDeclarations.Length == 1 || ((entryDetails == null || entryDetails.Length == 0) && existingDeclarations.Length > 1))
				{
					return existingDeclarations[0];
				}

				if (entryDetails != null && entryDetails.Length > 0 && existingDeclarations.Length > 1)
				{
					isMultipleDeclarationsMatchedWitBillDetails = true;
				}
			}

			return null;
		}

		ZBool isMultipleDeclarationsMatchedWithEntryDetails;
		ZBool isMultipleDeclarationsMatchedWitBillDetails;

		protected virtual TDeclaration GetDeclarationFromShipment()
		{
			return Shipment.DeclarationForDocuments as TDeclaration;
		}

		protected virtual bool CanMatchByCountrySpecificBusinessRules
		{
			get { return false; }
		}

		protected virtual TDeclaration GetExistingBusinessObjectUsingCountrySpecificBusinessRules()
		{
			return null;
		}

		protected override IMatchingBusinessEntityFinder<TDeclaration> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for Customs. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		TDeclaration[] GetExistingMatchingAnyBranch(ZQuery declarationQuery)
		{
			// Universal Architecture has already setup the correct environment
			var declarationBranchQuery = new ZQuery(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			declarationBranchQuery.AddToFilter(declarationQuery);
			declarationBranchQuery.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc + " desc";
			return factory.Load<TDeclaration>(declarationBranchQuery);
		}

		protected virtual TDeclaration GetNewBusinessObjectCore()
		{
			return base.GetNewBusinessObject();
		}

		protected sealed override TDeclaration GetNewBusinessObject()
		{
			if (Shipment != null)
			{
				var mutex = Enterprise.Customs.Common.DeclarationBeingCreatedForShipmentMutexCreator.Create(Shipment.PK);
				if (!mutex.Lock())
				{
					var message = Res.GetString(
						"1f9666f4-50bb-4617-8bd9-d253621636fb",
						"Could not create a new declaration; someone else is already in the process of creating a declaration for related Shipment ({0}).",
						Shipment.JS_UniqueConsignRef);

					throw new MessageProcessingBusinessFailureException(message, Shipment.JS_UniqueConsignRef, true); // TODO: replace this with MessageProcessingConcurrencyException
				}
				else
				{
					var declaration = Shipment.GetDeclaration() as TDeclaration;
					if (declaration != null)
					{
						var message = Res.GetString(
							"ad7945f7-0110-4407-8066-e9037165174e",
							"Could not create a new declaration; someone else has already created a declaration for related Shipment ({0}).",
							Shipment.JS_UniqueConsignRef);

						UnlockMutex();
						throw new MessageProcessingBusinessFailureException(message, Shipment.JS_UniqueConsignRef, true); // TODO: replace this with MessageProcessingConcurrencyException
					}
					else
					{
						AddActionForCleanupAfterSaving(UnlockMutex);
					}
				}

				void UnlockMutex()
				{
					if (mutex.HasLock)
					{
						mutex.Unlock();
					}
				}
			}

			return GetNewBusinessObjectCore();
		}

		protected void AddActionForCleanupAfterSaving(Action action)
		{
			if (action != null)
			{
				CleanupAfterSavingActions.Add(action);
			}
		}

		void CleanupAfterSavingFired(EventArgs args, object source)
		{
			factory.CleanupAfterSaving -= CleanupAfterSavingFired;
			if (cleanupAfterSavingActions != null)
			{
				cleanupAfterSavingActions.ForEach(x => x.Invoke());
				cleanupAfterSavingActions = null;
			}
		}

		List<Action> CleanupAfterSavingActions
		{
			get
			{
				if (cleanupAfterSavingActions == null)
				{
					cleanupAfterSavingActions = new List<Action>();
					factory.CleanupAfterSaving += CleanupAfterSavingFired;
				}

				return cleanupAfterSavingActions;
			}
		}
		List<Action> cleanupAfterSavingActions;

		protected override bool IsImportJobCostingAllowed(TDeclaration targetBO)
		{
			return IsStandAlone(targetBO);
		}

		protected bool IsStandAlone(IColumnIndexer declaration)
		{
			var shipment = Helper.Load<ForwardingShipment>(declaration, JobDeclarationSchema.JE_JS);
			return shipment == null;
		}

		protected sealed override void PopulateBusinessObject(TDeclaration declaration)
		{
			using (UnitConverter.TemporarySetupCachedConvertion(factory.BOFactory))
			using (SuspendSetters(declaration))
			{
				if (CanPopulateDeclaration(declaration))
				{
					SetupJobApplicationData(declaration);
					var declarationRow = GetColumnIndexer(declaration);
					var addInfoManager = declaration as IAddInfoManager;
					var isAddInfoSerialisationEnabled = addInfoManager != null && !IsDefaultingEnabled;
					ComplianceUniversalDataObjectReader complianceUniversalDataObjectReader = null;

					try
					{
						declarationRow.SetValue(JobDeclarationSchema.JE_AutoWeightApportion, ZBool.False);//to stop weight apportionment from invoice header to lines, declaration weight apportionment is disabled via IsImportinData which is set to true
						if (isAddInfoSerialisationEnabled)
						{
							addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
						}

						if (Shipment != null && !declaration.IsInDatabase)
						{
							SetValue(declarationRow, JobDeclarationSchema.JE_JS, Shipment.PK);
						}

						if (IsStandAlone(declaration))
						{
							complianceUniversalDataObjectReader = new ComplianceUniversalDataObjectReader(declaration);
							complianceUniversalDataObjectReader.InitializeComplianceMaterialChangesSnapshotIfNeeded();
						}

						var dataObject = shipmentDataObject ?? this.dataObject;
						AddFetchHintsForDeclarationUpdate(declaration);
						if (commericalInvoiceDataOnly)
						{
							if (!declaration.JE_OverrideFreightDefaults)
							{
								declaration.ShipmentSynchroniser.Synchronise(true);
							}

							FillCommercialInfoOnly(declaration, dataObject);
						}
						else
						{
							BillDetail primaryMasterBillDetail;
							BillDetail primaryHouseBillDetail;
							Dictionary<string, ValueSetter> xmlPersistentSetters;

							PopulateDeclaration(declaration, dataObject, addInfoManager, out primaryMasterBillDetail, out primaryHouseBillDetail, out xmlPersistentSetters);

							FillRelatedBusinessObjects(declaration, dataObject, addInfoManager, primaryMasterBillDetail, primaryHouseBillDetail);
							if (xmlPersistentSetters != null)
							{
								xmlPersistentSetters.SetValueInSpecificOrder(null);
							}
						}
					}
					finally
					{
						if (isAddInfoSerialisationEnabled)
						{
							addInfoManager.UpdateAddInfoFromString(declarationRow.GetValue(JobDeclarationSchema.JE_AddInfo));
							addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
						}
					}

					OnDeclarationPopulated(declaration);
					ImportCountrySpecificRelatedData(declaration);
					MergedAfterPopulateDeclaration(declaration);

					complianceUniversalDataObjectReader?.SynchronizeComplianceRiskStatusIfNeeded();
				}
				else if (allowUpdateOfCustomsDeclarationAfterCommencement)
				{
					var dataObject = shipmentDataObject ?? this.dataObject;
					FillRelatedBusinessObjectsForCustomsCommencedDeclaration(declaration, dataObject);
				}
			}
		}

		protected virtual void FillRelatedBusinessObjectsForCustomsCommencedDeclaration(TDeclaration declaration, UniversalShipment shipment)
		{
			FillAdditionalReferences(declaration, shipment);
			FillNotes(declaration, shipment);
		}

		IDisposable SuspendSetters(TDeclaration declaration)
		{
			suspendedSetterDisposables = new List<IDisposable>();
			suspendedSetterDisposables.Add(declaration.SetterSuspender.SuspendSetting(GetDeclarationPropertiesToSuspendSetting().ToArray()));
			return new DisposableAction(() =>
			{
				suspendedSetterDisposables.ForEach(x => x.Dispose());
				suspendedSetterDisposables = null;
			});
		}
		List<IDisposable> suspendedSetterDisposables;

		protected void AddSuspendSetterDisposable(IEnumerable<IDisposable> suspendedSetters) => suspendedSetterDisposables.AddRange(suspendedSetters);

		protected virtual IEnumerable<ZString> GetDeclarationPropertiesToSuspendSetting()
		{
			if (dataObject.CustomsContainerMode != null)
			{
				yield return BaseJobDeclaration.Schema.JE_ContainerMode;
			}

			if (dataObject.LocationAtClearance != null)
			{
				yield return BaseJobDeclaration.Schema.JE_LocationOfGoods;
			}

			if (dataObject.SubLocationAtClearance != null)
			{
				yield return BaseJobDeclaration.Schema.JE_SubLocationOfGoods;
			}

			if (dataObject.MessageType != null)
			{
				yield return BaseJobDeclaration.Schema.JE_MessageType;
			}
		}

		protected virtual void SetupJobApplicationData(TDeclaration declaration)
		{
		}

		void AddFetchHintsForDeclarationUpdate(TDeclaration declaration)
		{
			factory.BOFactory.AddFetchHint(CusDecHouseBillSchema.CU_JE, declaration.PK);
			factory.BOFactory.AddFetchHint(CusDecHouseBillSchema.CU_ClusterKey, declaration.JE_ClusterKey);
			factory.BOFactory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
			factory.BOFactory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_ClusterKey, declaration.JE_ClusterKey);
			factory.BOFactory.AddFetchHint(JobComInvoiceLineSchema.JI_ClusterKey, declaration.JE_ClusterKey);
		}

		void AddFetchHintsRelatedToCommerialInvoice(CommercialInfo commercialInfo, TDeclaration declaration)
		{
			if (IsDefaultingEnabled)
			{
				var partNos = new List<ZString>();
				var harmonisedCodes = new List<ZString>();
				AddFetchHintsRelatedToCommerialInvoice(commercialInfo, partNos, harmonisedCodes);
				if (partNos.Count > 0)
				{
					var parts = BatchLoad<MasterFiles.Business.OrgSupplierPart, ZString>(factory, OrgSupplierPartSchema.OP_PartNum, partNos);
					if (parts.Length > 0)
					{
						var partPKs = parts.Select(x => x.PK).ToArray();
						factory.BOFactory.AddFetchHint(OrgPartRelationSchema.Instance, new ZQuery(OrgPartRelationSchema.OU_OP, partPKs));
						var pivots = factory.Load<BaseCusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, partPKs));
						if (pivots.Length > 0)
						{
							var classPKs = pivots.Select(x => x.CI_CC).Where(x => !x.IsEmpty).ToArray();
							if (classPKs.Length > 0)
							{
								factory.BOFactory.AddFetchHint(CusClassificationSchema.Instance, new ZQuery(CusClassificationSchema.PK, classPKs));
							}

							var pivotPKs = pivots.Select(x => x.PK).ToArray();
							var childPivots = BatchLoad<BaseCusClassPartPivot, ZGuid>(factory, CusClassPartPivotSchema.CI_CI_Parent, pivotPKs);
							var allPivotPKs = pivotPKs.Concat(childPivots.Select(x => x.PK)).ToArray();
							foreach (var pivotKeysChunk in IEnumerableExtensions.Chunk(allPivotPKs, ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION))
							{
								var pivotKeys = pivotKeysChunk.ToArray();
								factory.BOFactory.AddFetchHint(CusUSClassificationSchema.Instance, new ZQuery(CusUSClassificationSchema.CD_ParentID, pivotKeys));
								factory.BOFactory.AddFetchHint(CusCodeDataSchema.Instance, new ZQuery(CusCodeDataSchema.CY_ParentID, pivotKeys));
							}
						}

						AddFetchHintsRelatedToCommerialInvoiceLinePart(declaration, parts);
					}
				}

				if (harmonisedCodes.Count > 0)
				{
					AddFetchHintsRelatedToCommerialInvoiceLineTariff(declaration, harmonisedCodes);
				}
			}
		}

		T[] BatchLoad<T, V>(UniversalObjectFactory factory, SchemaColumn column, ICollection<V> values) where T : BusinessObject
		{
			return IEnumerableExtensions.Chunk(values.Distinct(), ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION)
					.SelectMany(chunk => factory.Load<T>(new ZQuery(column, chunk)))
					.ToArray();
		}

		void AddFetchHintsRelatedToCommerialInvoice(CommercialInfo commercialInfo, List<ZString> partNos, List<ZString> harmonisedCodes)
		{
			if (commercialInfo != null && commercialInfo.CommercialInvoiceCollection != null)
			{
				foreach (var commercialInvoice in commercialInfo.CommercialInvoiceCollection)
				{
					if (commercialInvoice.CommercialInvoiceLineCollection != null)
					{
						foreach (var commercialInvoiceLine in commercialInvoice.CommercialInvoiceLineCollection)
						{
							var partNo = commercialInvoiceLine.PartNo.GetValueOrDefault();
							if (!partNo.IsEmpty && !partNos.Contains(partNo))
							{
								partNos.Add(partNo);
							}

							var harmonisedCode = commercialInvoiceLine.HarmonisedCode.GetValueOrDefault();
							if (!harmonisedCode.IsEmpty && !harmonisedCodes.Contains(harmonisedCode))
							{
								harmonisedCodes.Add(harmonisedCode);
							}
						}
					}
				}
			}
		}

		protected virtual void AddFetchHintsRelatedToCommerialInvoiceLinePart(TDeclaration declaration, IEnumerable<MasterFiles.Business.OrgSupplierPart> parts)
		{
		}

		protected virtual void AddFetchHintsRelatedToCommerialInvoiceLineTariff(TDeclaration declaration, List<ZString> harmonisedCodes)
		{
		}

		protected virtual void OnDeclarationPopulated(TDeclaration declaration)
		{
		}

		void MergedAfterPopulateDeclaration(TDeclaration declaration)
		{
			if (declaration.IsInDatabase && IsDefaultingEnabled && declaration.IsMergeDone)
			{
				try
				{
					var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.MessageInitiator = messageInitiator;
					AddActionForCleanupAfterSaving(() =>
					{
						declaration.UnlockDoMergeMutex();
					});

					if (!declaration.DoMerge())
					{
						var messageError = new ZStringBuilder();
						messageError.AppendIfNotEmpty(messageInitiator.InvalidOperationText);
						var lastErrors = messageInitiator.LastErrors;
						if (lastErrors != null)
						{
							lastErrors.OfType<string>().ForEach(x => messageError.AppendIfNotEmpty(x));
						}

						messageError.AppendIfNotEmpty(declaration.MergeManager.InvalidOperationText);
						SetMergeFails((EZC.NoResString)"Error : " + messageError.ToStringWithNewLineBetweenAppends());  // This string will be passed to GetString function later.
					}
				}
				catch (ApplicationException e)
				{
					SetMergeFails(e.Message);
				}
			}
		}

		void SetMergeFails(ZString mergeError)
		{
			throw new DataObjectReadFailureException(Res.GetString("D43D42DB-9630-4C2B-9517-8ABEB2CCEA4B", @"Cannot be updated, Merge failed. {0}", mergeError));
		}

		protected virtual void ImportCountrySpecificRelatedData(TDeclaration declaration)
		{
		}

		protected virtual bool CanPopulateDeclaration(TDeclaration declaration)
		{
			var result = true;

			if (declaration.IsInDatabase)
			{
				var applicationCode = GetApplicationCode();
				if (!applicationCode.IsEmpty && declaration.JE_ApplicationCode != applicationCode)
				{
					logger.Log(LogType.Error, Res.GetString("0A4465A1-B2B0-4C26-B0E2-7FBACFB4791B", @"The value of MessagingApplicationCode tag({0}) is different from the application code of the target declaration({1}).", applicationCode, declaration.JE_ApplicationCode));
					result = false;
				}
			}

			return result && !declaration.HasWHSTransaction && (CanPopulateDeclarationWhenMessageSent || !declaration.DeclarationMessagesHaveBeenSent());
		}

		protected virtual bool CanPopulateDeclarationWhenMessageSent => false;

		void FillRelatedBusinessObjects(TDeclaration declaration, UniversalShipment dataObject, IAddInfoManager addInfoManager, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			var declarationRow = GetColumnIndexer(declaration);
			ReadInCustomFields(declaration, dataObject);

			FillAdditionalBills(declaration, dataObject, primaryMasterBillDetail, primaryHouseBillDetail);
			FillCollections(declaration, dataObject, IsStandAlone(declarationRow));

			if (Helper.IsSourceAndTargetCountrySame)
			{
				var declarationPK = declarationRow.GetValue(JobDeclarationSchema.PK);
				var tablePrefix = JobDeclarationSchema.Constants.Prefix;
				var declarationIsInDatabase = declaration.IsInDatabase;
				new AddInfoGroupCollectionDataObjectReader(logger, Helper).ReadIntoDataRows(declarationPK, tablePrefix, declarationIsInDatabase, dataObject);
				new CustomsReferenceCollectionDataObjectReader(logger, Helper).ReadIntoDataRows(declarationPK, tablePrefix, declarationIsInDatabase, dataObject);
				if (declaration is Integration.Customs.ICusSupportingInfoTypeSupporter)
				{
					CreateNewCustomsSupportingInformationCollectionDataObjectReader().ReadIntoDataRows(declarationPK, tablePrefix, declarationIsInDatabase, dataObject);
				}
			}
		}

		protected virtual CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader() => new CustomsSupportingInformationCollectionDataObjectReader(logger, Helper);

		protected virtual void PopulateDeclaration(TDeclaration declaration, UniversalShipment dataObject, IAddInfoManager addInfoManager, out BillDetail primaryMasterBillDetail, out BillDetail primaryHouseBillDetail, out Dictionary<string, ValueSetter> xmlPersistentSetters)
		{
			xmlPersistentSetters = null;
			var declarationRow = GetColumnIndexer(declaration);
			var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
			var isStandalone = IsStandAlone(declarationRow);
			if (!isStandalone && !declarationRow.GetValue<ZBool>(JobDeclarationSchema.JE_OverrideFreightDefaults))
			{
				SetValue(declarationRow, JobDeclarationSchema.JE_OverrideFreightDefaults, ZBool.True, delaySetters);
				SetUntickOverrideFreightDefaultsIfSameWithXML(declarationRow.GetValue(JobDeclarationSchema.PK));
			}

			ZGuid branchPK = dataObject.GetBranchPK(factory.BOFactory);
			if (branchPK.IsValid)
			{
				SetValue(declarationRow, JobDeclarationSchema.JE_GB, branchPK, delaySetters);
			}

			SetValue(declarationRow, JobDeclarationSchema.JE_CustomsOffice, dataObject.CustomsOffice, delaySetters);
			CalculateMessageType(dataObject, declaration, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_MessageSubType, dataObject.MessageSubType, delaySetters);
			PopulateApplicationCode(dataObject, declaration, delaySetters);
			PopulateTransportMode(dataObject, declarationRow, delaySetters);
			SetValueWithDelay(declarationRow, JobDeclarationSchema.JE_ContainerMode, () => GetCustomsContainerMode(declaration, dataObject), delaySetters);

			SetValue(declarationRow, JobDeclarationSchema.JE_RS_NKServiceLevel, dataObject.ServiceLevel, delaySetters);
			primaryMasterBillDetail = GetPrimaryMasterBillDetail();
			primaryHouseBillDetail = GetPrimaryHouseBillDetail();
			SetValue(declarationRow, JobDeclarationSchema.JE_MasterBill, primaryMasterBillDetail == null ? null : primaryMasterBillDetail.BillNumber, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_HouseBill, primaryHouseBillDetail == null ? null : primaryHouseBillDetail.BillNumber, delaySetters);

			PopulateTransportDetailsOnDeclaration(declarationRow, declaration, delaySetters);

			SetValue(declarationRow, JobDeclarationSchema.JE_Folio, dataObject.Folio, delaySetters);

			PopulateLocationAtClearanceForReader(dataObject, declaration, delaySetters);
			PopulateSubLocationAtClearanceForReader(dataObject, declaration, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_RL_NKPortOfLoading, dataObject.PortOfLoading, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_RL_NKPortOfArrival, dataObject.PortOfDischarge, delaySetters);
			if (FirstArrivalDateAndPortUseDecider.IsUsed(Helper.TargetCountryCode, MessageTypeCode))
			{
				SetValue(declarationRow, JobDeclarationSchema.JE_RL_NKPortOfFirstArrival, GetFallbackValue(dataObject.PortOfFirstArrival, x => x.PortOfFirstArrival), delaySetters);
			}

			SetValue(declarationRow, JobDeclarationSchema.JE_RL_NKOrigin, dataObject.PortOfOrigin, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_RL_NKFinalDestination, dataObject.PortOfDestination, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_GoodsDestination, dataObject.GoodsDestination, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_GoodsOrigin, dataObject.GoodsOrigin, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_GoodsDescription, dataObject.GoodsDescription, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_GS_NKCusAgent, dataObject.CustomsBroker, delaySetters);

			if (dataObject.ContainerCount.HasValue)
			{
				SetValue(declarationRow, JobDeclarationSchema.JE_ContainerCount, (ZShort)dataObject.ContainerCount.Value, delaySetters);
			}

			PopulateNoOfPacksAndPackType(declaration, dataObject, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalVolume, dataObject.TotalVolume, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalVolumeUnit, dataObject.TotalVolumeUnit, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalWeight, dataObject.TotalWeight, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalWeightUnit, dataObject.TotalWeightUnit, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_OwnerRef, GetOwnerRef(declaration, dataObject), delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_RN_NKTransportNationality, dataObject.TransportNationality, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_IsPersonalEffects, dataObject.IsPersonalEffects, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_EFTMode, dataObject.EFTMode, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_MergeBy, dataObject.MergeBy, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_OperationalStatus, dataObject.OperationalStatus, delaySetters);
			//SetValue(declarationRow, JobDeclarationSchema.JE_ConsolidatedCargoStatus, dataObject.ConsolidatedCargoStatus, delaySetters); TODO: Determine whether we should import this data
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalNoOfPieces, dataObject.TotalNoOfPieces, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_LandedPieces, dataObject.TotalNoOfPiecesLanded, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_ExportGoodsType, dataObject.ExportGoodsType, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_AgentsReference, dataObject.AgentsReference, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_PaymentMethod, dataObject.PaymentMethod, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_PaidBy, dataObject.PaidBy, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_DefermentAccountNumber, dataObject.DefermentAccountNumber, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_ShipmentIncoTerm, dataObject.ShipmentIncoTerm, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_ShipmentIncoTermPlace, dataObject.AdditionalTerms, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_DeclarantType, dataObject.DeclarantType, delaySetters);
			PopulateCustomsProfile(dataObject, declaration, delaySetters);
			//SetValue(declarationRow, JobDeclarationSchema.JE_ScreeningStatus, dataObject.ScreeningStatus, delaySetters); TODO: Determine whether we should import this data
			SetValue(declarationRow, JobDeclarationSchema.JE_IATALoadPort, dataObject.CustomsValuationPort, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_UCR, dataObject.UniqueConsignmentReference, delaySetters);

			if (isStandalone)
			{
				ReadInJobDocsAndCartageData(declaration, delaySetters);
			}

			if (addInfoManager != null && Helper.IsSourceAndTargetCountrySame)
			{
				GetNewAddInfoDataObjectReaderForDeclaration(declaration).ReadIntoRow(addInfoManager, declarationRow, dataObject, delaySetters, dataObject);
			}

			FillRealFieldFromAddInfo(declaration, dataObject, delaySetters);
			FillDates(declaration, dataObject, delaySetters);
			FillOrganizations(declaration, dataObject, delaySetters);
			FillCountrySpecificDetails(declaration, dataObject, delaySetters);

			if (delaySetters != null)
			{
				xmlPersistentSetters = new Dictionary<string, ValueSetter>();
				delaySetters.SetValueOnSetterSupenderParentInSpecificOrder(declaration.SetterSuspender, GetSettingOrder(declaration));
				GatherFieldsThatNeedToBePersisted(xmlPersistentSetters, delaySetters, declaration);
			}

			if (IsDefaultingEnabled)
			{
				addInfoManager.UpdateRelatedPropertyInfo();
			}

			if (isStandalone)
			{
				var docsIndexer = GetColumnIndexer(declaration.DocsAndCartage);
				if (declaration.IsImport)
				{
					ClearPickupProperties(docsIndexer, delaySetters);
				}
				else if (declaration.IsExport)
				{
					ClearDeliveryProperties(docsIndexer, delaySetters);
				}
			}
		}

		void ClearPickupProperties(IColumnIndexer docsIndexer, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded, ZString.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_EstimatedPickup, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_PickupCartageAdvised, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_PickupCartageCompleted, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_PickupLabourTime, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_PickupLabourCharge, 0, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_PickupTruckWaitTime, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_PickupTruckWaitCharge, 0, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_PickupRequiredBy, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_LCLAirStorageDaysOrHours, 0, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_PickupRequiredFrom, ZDateTime.Empty, delaySetters);
		}

		void ClearDeliveryProperties(IColumnIndexer docsIndexer, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded, ZString.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_FCLStorageCommences, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_EstimatedDelivery, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_DeliveryCartageAdvised, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_DeliveryCartageCompleted, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_DeliveryLabourTime, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_DeliveryLabourCharge, 0, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_DeliveryTruckWaitTime, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_DeliveryTruckWaitCharge, 0, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_DeliveryRequiredFrom, ZDateTime.Empty, delaySetters);
			SetValue(docsIndexer, JobDocsAndCartageSchema.JP_DeliveryRequiredBy, ZDateTime.Empty, delaySetters);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004: Cast is redundant", Justification = "Failed unit tests: without ZString? casting, its value will be string.Empty instead of null")]
		ZString? GetOwnerRef(TDeclaration declaration, UniversalShipment dataObject)
		{
			var ownerRefInXml = dataObject.OwnerRef;
			var maxLength = declaration.GetPossiblyCustomPropertyMaxLength(JobDeclarationSchema.Constants.JE_OwnerRef);
			return ownerRefInXml.HasValue ? (ZString?)ownerRefInXml.Value.Left(maxLength) : null;
		}

		protected virtual void PopulateTransportDetailsOnDeclaration(IColumnIndexer declarationRow, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValueWithDelay(declarationRow, JobDeclarationSchema.JE_VesselName, () => GetVesselNameCore(declaration), delaySetters);
			SetValueWithDelay(declarationRow, JobDeclarationSchema.JE_VoyageFlightNo, () => GetVoyageFlightNoCore(declaration), delaySetters);
			SetValueWithDelay(declarationRow, JobDeclarationSchema.JE_LloydsIMO, () => GetVesselLloydsIMOCore(declaration), delaySetters);
		}

		const string E2EMessagePurposeCode = "E2E";

		protected virtual TransportLeg GetTransportLegCore(TDeclaration declaration)
		{
			TransportLeg result = null;

			if (dataObject.DataContext?.ActionPurposeCode.EqualsIgnoringCase(E2EMessagePurposeCode) ?? false)
			{
				var transports = dataObject.TransportLegCollection;
				if (transports != null && transports.Any())
				{
					var countryCode = declaration.CountryCode;
					var transportMode = declaration.TransportMode;
					var isImport = declaration.IsImport;

					var converter = new TransportModeConverter();

					bool IsLocalPort(UNLOCO port) => port?.Code.GetValueOrDefault().StartsWith(countryCode, StringComparison.OrdinalIgnoreCase) ?? false;

					bool IsMatchedLeg(TransportLeg leg)
					{
						return (string.IsNullOrEmpty(transportMode) || converter.FromEnumValue(leg.TransportMode).EqualsIgnoringCase(transportMode))
							&& (isImport ? (!IsLocalPort(leg.PortOfLoading) && IsLocalPort(leg.PortOfDischarge)) : (IsLocalPort(leg.PortOfLoading) && !IsLocalPort(leg.PortOfDischarge)));
					}

					result = isImport
						? transports.OrderBy(c => c.LegOrder.GetValueOrDefault()).FirstOrDefault(IsMatchedLeg)
						: transports.OrderByDescending(c => c.LegOrder.GetValueOrDefault()).FirstOrDefault(IsMatchedLeg);
				}
			}

			return result;
		}

		protected virtual ZString? GetVesselNameCore(TDeclaration declaration)
		{
			var matchedTransportLeg = GetTransportLegCore(declaration);
			return matchedTransportLeg?.VesselName ?? dataObject.VesselName;
		}

		protected virtual ZString? GetVoyageFlightNoCore(TDeclaration declaration)
		{
			var matchedTransportLeg = GetTransportLegCore(declaration);
			return matchedTransportLeg?.VoyageFlightNo ?? dataObject.VoyageFlightNo;
		}

		protected virtual ZString? GetVesselLloydsIMOCore(TDeclaration declaration)
		{
			var matchedTransportLeg = GetTransportLegCore(declaration);
			return matchedTransportLeg?.VesselLloydsIMO ?? dataObject.LloydsIMO;
		}

		protected virtual void PopulateLocationAtClearanceForReader(UniversalShipment dataObject, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			using (declaration.SetterSuspender.ResumeSetting(nameof(declaration.JE_LocationOfGoods)))
			{
				var declarationRow = GetColumnIndexer(declaration);
				SetValue(declarationRow, JobDeclarationSchema.JE_LocationOfGoods, GetLocationAtClearanceCore(dataObject.LocationAtClearance), delaySetters);
			}
		}

		protected virtual ZString? GetLocationAtClearanceCore(CodeDescriptionPair35Char locationAtClearance)
		{
			ZString? location = null;
			if (locationAtClearance != null)
			{
				var code = locationAtClearance.Code;
				var description = locationAtClearance.Description;
				location = description.HasValue && description.Value.StartsWith(code.GetValueOrDefault(), StringComparison.OrdinalIgnoreCase) ? description : code;
			}

			return location;
		}

		protected virtual void PopulateSubLocationAtClearanceForReader(UniversalShipment dataObject, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			using (declaration.SetterSuspender.ResumeSetting(nameof(declaration.JE_SubLocationOfGoods)))
			{
				var declarationRow = GetColumnIndexer(declaration);
				SetValue(declarationRow, JobDeclarationSchema.JE_SubLocationOfGoods, GetSubLocationAtClearance(dataObject.SubLocationAtClearance), delaySetters);
			}
		}

		protected ZString? GetSubLocationAtClearance(CodeDescriptionPair35Char subLocationAtClearance)
		{
			return subLocationAtClearance == null ? null : GetSubLocationAtClearanceCore(subLocationAtClearance);
		}

		protected virtual ZString? GetSubLocationAtClearanceCore(CodeDescriptionPair35Char subLocationAtClearance)
		{
			var code = subLocationAtClearance.Code;
			ZString? description = subLocationAtClearance.Description;
			var maxLength = JobDeclarationSchema.JE_SubLocationOfGoods.MaxLength;
			return description.HasValue && description.Value.StartsWith(code.GetValueOrDefault(), StringComparison.OrdinalIgnoreCase) ? description.Value.Left(maxLength) : code;
		}

		void SetUntickOverrideFreightDefaultsIfSameWithXML(ZGuid declarationPK)
		{
			if (Shipment != null)
			{
				declarationPKForOverrideFreightDefaults = declarationPK;
				factory.CleanupAfterSaving -= UntickOverrideFreightDefaultsIfSameWithXML;
				factory.CleanupAfterSaving += UntickOverrideFreightDefaultsIfSameWithXML;
			}
		}
		ZGuid declarationPKForOverrideFreightDefaults;

		void UntickOverrideFreightDefaultsIfSameWithXML(EventArgs args, object source)
		{
			factory.CleanupAfterSaving -= UntickOverrideFreightDefaultsIfSameWithXML;
			if (!declarationPKForOverrideFreightDefaults.IsEmpty)
			{
				var declaration = factory.Load<BaseJobDeclaration>(declarationPKForOverrideFreightDefaults);
				if (declaration != null && declaration.JE_OverrideFreightDefaults && !declaration.IsStandAlone && !declaration.HasChanges)
				{
					var shipmentSynchroniser = declaration.ShipmentSynchroniser;
					try
					{
						shipmentSynchroniser.DetectEnabled = true;
						shipmentSynchroniser.Synchronise(true);
						if (!shipmentSynchroniser.SyncChangesDetected)
						{
							var changedBizObjs = GetListIfChangedBizObj(declaration);
							if (!changedBizObjs.IsEmpty)
							{
								ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Declaration {0} Synchroniser Detection has caused the following changes:\r\n{1}", declaration.JE_DeclarationReference, changedBizObjs));
							}
							else
							{
								UntickOverrideFreightDefaults();
							}
						}
					}
					finally
					{
						shipmentSynchroniser.DetectEnabled = false;
					}
				}
			}
		}

		ZString GetListIfChangedBizObj(BusinessObject bizObj)
		{
			var result = new ZStringBuilder();
			if (bizObj != null)
			{
				if (((IBusinessObjectState)bizObj).HasChangesNotIncludingChildren)
				{
					result.Append(string.Format(CultureInfo.InvariantCulture, "{0} ({1})", bizObj.HumanReadableName, bizObj.GetType().FullName));
				}

				foreach (var child in ((IBusiness)bizObj).Children)
				{
					var childBizObj = child as BusinessObject;
					var collection = child as IBusinessObjectCollection;
					if (childBizObj != null)
					{
						result.AppendIfNotEmpty(GetListIfChangedBizObj(childBizObj));
					}
					else if (collection != null)
					{
						foreach (BusinessObject collectionBizObj in collection)
						{
							result.AppendIfNotEmpty(GetListIfChangedBizObj(collectionBizObj));
						}
					}
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		void UntickOverrideFreightDefaults()
		{
			if (!declarationPKForOverrideFreightDefaults.IsEmpty)
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var declaration = newFactory.Load<BaseJobDeclaration>(declarationPKForOverrideFreightDefaults);
				if (declaration.JE_OverrideFreightDefaults)
				{
					declaration.JE_OverrideFreightDefaults = ZBool.False;
					try
					{
						newFactory.Save();
					}
					catch (ZSaveException e)
					{
						ZExceptionReporting.HandleSaveException(e);
					}
				}
			}
		}

		void GatherFieldsThatNeedToBePersisted(Dictionary<string, ValueSetter> xmlPersistentSetters, Dictionary<string, ValueSetter> delaySetters, TDeclaration declaration)
		{
			foreach (var field in FieldsThatNeedToBePersisted(declaration))
			{
				ValueSetter setter;
				if (!xmlPersistentSetters.ContainsKey(field) && delaySetters.TryGetValue(field, out setter))
				{
					xmlPersistentSetters.Add(field, setter);
				}
			}
		}

		protected virtual IEnumerable<string> FieldsThatNeedToBePersisted(TDeclaration declaration)
		{
			var docsAndCartage = declaration.DocsAndCartage;
			if (docsAndCartage != null)
			{
				var docsAndCartageKey = docsAndCartage.PK.ToStringKey();
				yield return docsAndCartageKey + JobDocsAndCartage.Schema.JP_FCLDeliveryEquipmentNeeded;
				yield return docsAndCartageKey + JobDocsAndCartage.Schema.JP_FCLPickupEquipmentNeeded;
			}
		}

		protected ContainerMode GetCustomsContainerMode(TDeclaration declaration, UniversalShipment dataObject)
		{
			ContainerMode result = null;
			if (dataObject.CustomsContainerMode != null)
			{
				result = dataObject.CustomsContainerMode;
			}
			else
			{
				if (!declaration.IsInDatabase)
				{
					var containerModeList = declaration.Lookups.CargoIdTypeList;
					var shouldApplyFallbackPredicate = new Func<UniversalShipment, bool>(shipment =>
					{
						var containerCollection = shipment.ContainerCollection;
						return containerCollection != null && containerCollection.Any();
					});

					var subConverterGetter = new Func<UniversalShipment, CustomsContainerModeConverter<UniversalShipment>>(universalShipment =>
					{
						CustomsContainerModeConverter<UniversalShipment> subConverter = null;
						var subShipmentDataObject = universalShipment.SubShipmentCollection?.FirstOrDefault(x => x.DataContext?.GetMatchingDataSource(DataContextType.ForwardingShipment) != null);
						if (subShipmentDataObject != null)
						{
							subConverter = new CustomsContainerModeConverter<UniversalShipment>(subShipmentDataObject, containerModeList, subShipmentDataObject.ContainerMode, null, shouldApplyFallbackPredicate);
						}
						return subConverter;
					});

					var containerModeConverter = new CustomsContainerModeConverter<UniversalShipment>(dataObject, containerModeList, dataObject.ContainerMode, subConverterGetter, shouldApplyFallbackPredicate);
					result = containerModeConverter.Convert();
				}
			}

			return result;
		}

		protected string MessageTypeCode
		{
			get { return messageTypeCode; }
		}
		string messageTypeCode;

		void CalculateMessageType(UniversalShipment dataObject, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (declaration.IsInDatabase && declaration.DeclarationMessagesHaveBeenSent())
			{
				messageTypeCode = declaration.JE_MessageType;
			}
			else
			{
				messageTypeCode = CalculateMessageCode(dataObject, declaration);
				var messageType = ListHelper.GetWithDescription<CodeDescriptionPair>(messageTypeCode, factory.BOFactory.GetCachedValue<ImportExportCodeList>());
				SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_MessageType, messageType, delaySetters);
			}
		}

		protected ZString CalculateMessageCode(UniversalShipment dataObject, TDeclaration declaration)
		{
			string result = null;
			var messageType = dataObject.MessageType;
			if (messageType != null)
			{
				result = messageType.Code;
			}
			else if (declaration.IsInDatabase)
			{
				result = declaration.JE_MessageType;
			}
			else
			{
				if (logger.TopLevelDataContext != null)
				{
					var recipientRoles = logger.TopLevelDataContext.RecipientRoleCollection;
					if (recipientRoles != null)
					{
						if (recipientRoles.Any(o => o.Code == RecipientRoleType.BRI))
						{
							result = ImportExportCodeList.Codes.Import;
						}

						if (recipientRoles.Any(o => o.Code == RecipientRoleType.BRE))
						{
							result = ImportExportCodeList.Codes.Export;
						}
					}
				}

				if (result == null)
				{
					var origin = dataObject.PortOfOrigin != null ? dataObject.PortOfOrigin.Code :
						dataObject.PortOfLoading != null ? dataObject.PortOfLoading.Code : ZString.Empty;

					var destination = dataObject.PortOfDestination != null ? dataObject.PortOfDestination.Code :
						dataObject.PortOfDischarge != null ? dataObject.PortOfDischarge.Code : ZString.Empty;

					result = ImportExportHelper.IsImport(origin.Value, destination.Value) ? ImportExportCodeList.Codes.Import : ImportExportCodeList.Codes.Export;
				}
			}

			return result;
		}

		protected virtual IEnumerable<ZString> GetSettingOrder(TDeclaration declaration)
		{
			// Main Declaration Fields
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OverrideFreightDefaults);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GB);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_DeclarantAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_SupplierAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OA_ImporterAddress);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Supplier);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Importer);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MessageType);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MessageSubType);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_CustomsProfile);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_LocationOfGoods);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_SubLocationOfGoods);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_CustomsOffice);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TransportMode);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_ContainerMode);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RS_NKServiceLevel);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MasterBill);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_VesselName);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_VoyageFlightNo);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_Folio);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RL_NKPortOfLoading);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_ExportDate);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RL_NKPortOfArrival);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_DateOfArrival);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_HouseBill);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RL_NKOrigin);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_DateAtOrigin);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_RL_NKFinalDestination);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_DateAtFinalDestination);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GoodsDescription);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OwnerRef);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalWeight);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalWeightUnit);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalVolume);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalVolumeUnit);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalNoOfPieces);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_LandedPieces);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_ContainerCount);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalNoOfPacks);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_TotalNoOfPacksPackType);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_ShipmentIncoTerm);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_ShippingLine);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Forwarder);
			// Misc Fields
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_GS_NKCusAgent);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MergeBy);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_PaymentMethod);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_PaidBy);
		}

		protected BillDetail GetPrimaryMasterBillDetail()
		{
			BillDetail result = null;
			if (IsFromFreight)
			{
				result = GetPrimaryMasterBillCore(shipmentDataObject);
			}

			if (result == null)
			{
				result = GetPrimaryMasterBillCore(dataObject);
			}

			return result;
		}

		protected virtual BillDetail GetPrimaryMasterBillCore(UniversalShipment dataObject)
		{
			ZString? masterWayBillNumber = null;
			if (dataObject != null)
			{
				var billType = dataObject.WayBillType.GetCodeAsUpperCase();
				if (billType == WayBillTypeList.Codes.Master)
				{
					masterWayBillNumber = dataObject.WayBillNumber;
				}
				else if (billType == WayBillTypeList.Codes.House)
				{
					if (dataObject.AdditionalBillCollection != null)
					{
						masterWayBillNumber = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.MasterWayBillNumber);
					}

					if (!masterWayBillNumber.HasValue)
					{
						var billNumber = dataObject.WayBillNumber.GetValueOrDefault();
						if (!billNumber.IsEmpty)
						{
							var additionalBill = dataObject.AdditionalBillCollection.GetAdditionalBill(billNumber, billType);
							if (additionalBill != null)
							{
								masterWayBillNumber = additionalBill.ParentBillNumber;
							}
						}
					}
				}
			}

			return masterWayBillNumber.HasValue ? new BillDetail() { BillNumber = masterWayBillNumber } : null;
		}

		protected BillDetail GetPrimaryHouseBillDetail()
		{
			return GetPrimaryHouseBillDetailCore(IsFromFreight ? shipmentDataObject : this.dataObject);
		}

		protected virtual BillDetail GetPrimaryHouseBillDetailCore(UniversalShipment dataObject)
		{
			BillDetail result = null;
			if (dataObject != null)
			{
				var billType = dataObject.WayBillType.GetCodeAsUpperCase();
				if (billType == WayBillTypeList.Codes.House)
				{
					result = new BillDetail() { BillNumber = dataObject.WayBillNumber };
				}
			}

			return result;
		}

		protected EntryDetail[] GetEntryDetails()
		{
			return GetEntryDetailsCore(IsFromFreight ? shipmentDataObject : this.dataObject);
		}

		protected virtual EntryDetail[] GetEntryDetailsCore(UniversalShipment dataObject)
		{
			var result = new List<EntryDetail>();
			if (dataObject != null && dataObject.EntryHeaderCollection != null && dataObject.EntryHeaderCollection.Count > 0)
			{
				result.AddRange(dataObject.EntryHeaderCollection
					.Where(entryHeader => entryHeader.EntryNumberCollection != null && entryHeader.EntryNumberCollection.Count > 0)
					.Select(entryHeader => new EntryDetail
					{
						MessageType = entryHeader.Type.GetCodeAsUpperCase(),
						EntryReference = entryHeader.Reference.GetValueOrDefault()
					}));
			}

			return result.ToArray();
		}
		EntryDetail[] entryDetails;

		protected bool IsFromFreight
		{
			get { return shipmentDataSource != null; }
		}

		protected bool HasConsolDataObject
		{
			get { return shipmentDataSource != null && consolDataSource != null; }
		}

		UniversalShipment GetShipmentDataObject()
		{
			UniversalShipment result = dataObject;
			var consolSource = consolDataSource;
			if (consolSource != null)
			{
				result = shipmentDataSource.Key.GetValueOrDefault().IsEmpty ? null : GetForwardingShipmentMatchingKey(shipmentDataSource.Key.Value, dataObject.SubShipmentCollection);
			}

			return result;
		}

		protected UniversalShipment GetForwardingShipmentMatchingKey(ZString key, DataObjectList<UniversalShipment> shipments)
		{
			UniversalShipment result = null;
			foreach (var shipment in shipments)
			{
				var source = shipment.GetMatchingDataSource(DataContextType.ForwardingShipment);
				if (source != null && source.Key.GetValueOrDefault() == key)
				{
					result = shipment;
				}
				else
				{
					result = GetForwardingShipmentMatchingKey(key, shipment.SubShipmentCollection);
				}

				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		protected IDictionary<TransportTypeCode, List<TransportMeans>> TransportMeansGroupByTransportType => dataObject.TransportMeansCollection.GetOrCreateCodeDictionary(ref transportMeansGroupByTransportType, x => x.TransportType ?? TransportTypeCode.Inland);
		IDictionary<TransportTypeCode, List<TransportMeans>> transportMeansGroupByTransportType;

		protected virtual void PopulateTransportMode(UniversalShipment dataObject, IColumnIndexer declarationRow, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(declarationRow, JobDeclarationSchema.JE_TransportMode, GetFallbackValue(dataObject.TransportMode, x => x.TransportMode), delaySetters);
		}

		protected virtual void PopulateApplicationCode(UniversalShipment dataObject, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (!declaration.IsInDatabase)
			{
				var applicationCode = GetApplicationCode();
				if (!applicationCode.IsEmpty)
				{
					if (IsValidApplicationCode(declaration, applicationCode))
					{
						if (applicationCode == DeclarationApplicationCodeList.Codes.Interfaced || applicationCode == DeclarationApplicationCodeList.Codes.Builtin)
						{
							var customsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
							var submissionType = customsInterface?.SubmissionType ?? ZString.Empty;

							switch (submissionType)
							{
								case DeclarationApplicationCodeList.Codes.Interfaced:
									if (!applicationCode.EqualsIgnoringCase(DeclarationApplicationCodeList.Codes.Interfaced))
									{
										var msg = Res.GetString("A1B91B3C-8BD5-421F-91BF-41271A0D38C1", "Invalid MessagingApplicationCode '{0}': the registry 'Local Country Customs Interface' for Company '{1}' is set to '{2}', only application code '{2}' is supported.", applicationCode, GlbCompany.CurrentCompany.GC_Code, DeclarationApplicationCodeList.Codes.Interfaced);
										throw new MessageProcessingBusinessFailureException(msg);
									}
									break;
								case DeclarationApplicationCodeList.Codes.Builtin:
									if (applicationCode.EqualsIgnoringCase(DeclarationApplicationCodeList.Codes.Interfaced))
									{
										var msg = Res.GetString("A0E45491-F19B-4096-B026-BD08B619E80D", "Invalid MessagingApplicationCode '{0}': the registry 'Local Country Customs Interface' for Company '{1}' is set to '{2}', application code '{3}' is not supported.", applicationCode, GlbCompany.CurrentCompany.GC_Code, DeclarationApplicationCodeList.Codes.Builtin, DeclarationApplicationCodeList.Codes.Interfaced);
										throw new MessageProcessingBusinessFailureException(msg);
									}
									break;
								case "":
									var targetCountry = Helper.TargetCountryCode;
									if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(targetCountry) && !IntegratedCountryHelper.CountryHasDeclarationInDevelopment(targetCountry) && !applicationCode.EqualsIgnoringCase(DeclarationApplicationCodeList.Codes.Interfaced))
									{
										var msg = Res.GetString("6B59F525-4059-454A-A7E9-26D3727A12CE", "Invalid MessagingApplicationCode '{0}': only application code '{1}' is supported for Company '{2}'.", applicationCode, DeclarationApplicationCodeList.Codes.Interfaced, GlbCompany.CurrentCompany.GC_Code);
										throw new MessageProcessingBusinessFailureException(msg);
									}
									break;
							}
						}

						var declarationRow = GetColumnIndexer(declaration);
						SetValueWithDelay(declarationRow, JobDeclarationSchema.JE_ApplicationCode, () => applicationCode, delaySetters);
					}
					else
					{
						var msg = Res.GetString("B5B1D3B5-FB08-416A-A7F6-6D8D225D2C79", "MessagingApplicationCode '{0}' is not a valid application code for {1} Customs Declaration with message type '{2}' based on registry 'Local Country Customs Interface' setting.", applicationCode, Helper.TargetCountryCode, MessageTypeCode);
						throw new MessageProcessingBusinessFailureException(msg);
					}
				}
			}
		}

		protected virtual bool IsValidApplicationCode(TDeclaration declaration, ZString applicationCode)
		{
			return declaration.Lookups.ApplicationCodeList.ContainsCode(applicationCode);
		}

		ZString GetApplicationCode() => dataObject.MessagingApplicationCode?.Code.GetValueOrDefault() ?? ZString.Empty;

		protected virtual void PopulateCustomsProfile(UniversalShipment dataObject, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var declarationRow = GetColumnIndexer(declaration);
			SetValue(declarationRow, JobDeclarationSchema.JE_CustomsProfile, dataObject.CustomsProfileIdentifier?.Value.GetValueOrDefault(), delaySetters);
		}

		protected V GetFallbackValue<V>(V value, Func<UniversalShipment, V> getFallbackValue)
			where V : ICodeDataObject
		{
			var consolDataObject = this.consolDataObject;
			return consolDataObject != null && (value == null || !value.Code.HasValue) ? getFallbackValue(consolDataObject) : value;
		}

		protected UNLOCO GetFallbackValue(UNLOCO value, Func<UniversalShipment, UNLOCO> getFallbackValue)
		{
			var consolDataObject = this.consolDataObject;
			return consolDataObject != null && (value == null || !value.Code.HasValue) ? getFallbackValue(consolDataObject) : value;
		}

		protected virtual void PopulateNoOfPacksAndPackType(TDeclaration declaration, UniversalShipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			var declarationRow = GetColumnIndexer(declaration);
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalNoOfPacks, dataObject.OuterPacks, delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalNoOfPacksPackType, Helper.GetCustomsUnitForPackType(dataObject.OuterPacksPackageType), delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalNoOfPacksDecimal, dataObject.TotalNoOfPacksDecimal, delaySetters);
		}

		protected virtual AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForDeclaration(TDeclaration declaration)
		{
			var addInfoSchema = (declaration as IAddInfoManagerWithSchema)?.AddInfoSchema;
			if (addInfoSchema == null)
			{
				return new DeclarationAddInfoDataObjectReader(logger, Helper);
			}
			else
			{
				return new BusinessObjectAddInfoDataObjectReader(declaration.GetType(), logger, Helper, JobDeclarationSchema.JE_AddInfo, addInfoSchema);
			}
		}

		protected virtual OrgAddress FillImporter(List<OrganizationAddress> organizationAddressCollection, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			OrgAddress result = null;
			var importerAddress = organizationAddressCollection.FindBestImporterMatch();
			if (importerAddress != null)
			{
				var declarationRow = GetColumnIndexer(declaration);
				var addressType = importerAddress.AddressType.GetValueOrDefault();
				var reader = new OrganisationDataObjectReader(importerAddress, logger, factory);
				var importer = reader.GetMatched(declaration, OrganisationTypes.Consignee);
				if (importer != null)
				{
					if (declaration.UseImporterAddress)
					{
						SetValue(declarationRow, JobDeclarationSchema.JE_OA_ImporterAddress, importer.PK, delaySetters);
						if (!IsDefaultingEnabled)
						{
							SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Importer, importer, delaySetters);
						}
					}
					else
					{
						SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Importer, importer, delaySetters);
					}

					result = importer;
				}
				else if (ShouldCreateDocAddress(declaration, DocAddressType.ImporterDocumentaryAddress) && (addressType == nameof(DocAddressType.ImporterDocumentaryAddress) || addressType == nameof(DocAddressType.ConsigneeDocumentaryAddress)))
				{
					var jobDocAddress = reader.GetMatchedOrNew(declaration, OrganisationTypes.Consignee, docAddressTypeOverride: DocAddressType.ImporterDocumentaryAddress);
					if (jobDocAddress != null && jobDocAddress.OrganisationPK == OrgHeader.UnmatchedOrganisationPK)
					{
						result = jobDocAddress.Address;
						SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Importer, result, delaySetters);
					}
				}
			}

			return result;
		}

		protected virtual OrgAddress FillSupplier(List<OrganizationAddress> organizationAddressCollection, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			OrgAddress result = null;
			var supplierAddress = organizationAddressCollection.FindBestSupplierMatch();
			if (supplierAddress != null)
			{
				var declarationRow = GetColumnIndexer(declaration);
				var addressType = supplierAddress.AddressType.GetValueOrDefault();
				var reader = new OrganisationDataObjectReader(supplierAddress, logger, factory);
				var supplier = reader.GetMatched(declaration, OrganisationTypes.Consignor);
				if (supplier != null)
				{
					if (declaration.UseSupplierAddress)
					{
						SetValue(declarationRow, JobDeclarationSchema.JE_OA_SupplierAddress, supplier.PK, delaySetters);
						if (!IsDefaultingEnabled)
						{
							SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Supplier, supplier, delaySetters);
						}
					}
					else
					{
						SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Supplier, supplier, delaySetters);
					}

					result = supplier;
				}
				else if (ShouldCreateDocAddress(declaration, DocAddressType.SupplierDocumentaryAddress) && (addressType == nameof(DocAddressType.SupplierDocumentaryAddress) || addressType == nameof(DocAddressType.ConsignorDocumentaryAddress)))
				{
					var jobDocAddress = reader.GetMatchedOrNew(declaration, OrganisationTypes.Consignor, docAddressTypeOverride: DocAddressType.SupplierDocumentaryAddress);
					if (jobDocAddress != null && jobDocAddress.OrganisationPK == OrgHeader.UnmatchedOrganisationPK)
					{
						result = jobDocAddress.Address;
						SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Supplier, result, delaySetters);
					}
				}
			}

			return result;
		}

		protected virtual void FillSeller(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(Constants.AddressTypes.Seller);
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignee);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OA_SellerAddress, orgAddress.PK, delaySetters);
				}
			}
		}

		protected virtual void FillManufacturer(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(nameof(DocAddressType.Manufacturer));
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignor);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OA_ManufacturerAddress, orgAddress.PK, delaySetters);
				}
			}
		}

		protected virtual void FillShipToParty(List<OrganizationAddress> organizationAddressCollection, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = organizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ShipToParty));
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignee);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OA_ShipToPartyAddress, orgAddress.PK, delaySetters);
				}
			}
		}

		protected virtual void FillSoldToPartyAddress(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(Constants.AddressTypes.SoldToParty);
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignee);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OA_SoldToPartyAddress, orgAddress.PK, delaySetters);
				}
			}
		}

		protected virtual void FillBuyingAgent(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(Constants.AddressTypes.BuyingAgent);
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignee);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OH_BuyingAgent, orgAddress.OA_OH, delaySetters);
				}
			}
		}

		protected virtual void FillSellingAgent(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(Constants.AddressTypes.SellingAgent);
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignor);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OH_SellingAgent, orgAddress.OA_OH, delaySetters);
				}
			}
		}

		protected virtual void FillExporter(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(nameof(DocAddressType.Exporter));
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignor);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OH_Exporter, orgAddress.OA_OH, delaySetters);
				}
			}
		}

		protected virtual void FillConsigneeAddress(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(Constants.AddressTypes.UltimateConsignee);
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignee);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OA_ConsigneeAddress, orgAddress.PK, delaySetters);
				}
			}
		}

		protected virtual void FillConsignee(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(Constants.AddressTypes.IntermediateConsignee);
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignee);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OH_Consignee, orgAddress.OA_OH, delaySetters);
				}
			}
		}

		protected virtual void FillBuyer(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(nameof(DocAddressType.BuyerDocumentaryAddress));
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Consignee);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OH_Buyer, orgAddress.OA_OH, delaySetters);
				}
			}
		}

		protected virtual void FillDeclarant(List<OrganizationAddress> organizationAddressCollection, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			ZGuid organisationPK, addressPK;
			if (this.TryGetMatchedOrganisationData(out organisationPK, out addressPK, dataObject, declaration, AddressTypes.Declarant, OrganisationTypes.None))
			{
				var declarationRow = GetColumnIndexer(declaration);
				SetValue(declarationRow, JobDeclarationSchema.JE_OA_DeclarantAddress, addressPK, delaySetters);
			}
		}

		protected virtual void FillRepresentative(List<OrganizationAddress> orgAddresses, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(nameof(DocAddressType.Representative));
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.None);
				if (orgAddress != null)
				{
					SetValue(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OA_Representative, orgAddress.PK, delaySetters);
				}
			}
		}

		protected virtual void FillExternalBroker(List<OrganizationAddress> orgAddresses, TDeclaration declaration, IColumnIndexer declarationRow, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(nameof(DocAddressType.ExternalBroker));
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Broker);
				if (orgAddress != null)
				{
					SetValue(declarationRow, JobDeclarationSchema.JE_OH_ExternalBroker, orgAddress.OA_OH, delaySetters);
				}
			}
		}

		protected virtual void FillControllingAgent(List<OrganizationAddress> orgAddresses, TDeclaration declaration, IColumnIndexer declarationRow, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(nameof(DocAddressType.ControllingAgent));
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.ControllingAgent);
				if (orgAddress != null)
				{
					SetValue(declarationRow, JobDeclarationSchema.JE_OH_ControllingAgent, orgAddress.OA_OH, delaySetters);
				}
			}
		}

		protected virtual void FillControllingCustomer(List<OrganizationAddress> orgAddresses, TDeclaration declaration, IColumnIndexer declarationRow, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = orgAddresses.FirstOrDefault(nameof(DocAddressType.ControllingCustomer));
			if (address != null)
			{
				var orgReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.ControllingCustomer);
				if (orgAddress != null)
				{
					SetValue(declarationRow, JobDeclarationSchema.JE_OH_ControllingCustomer, orgAddress.OA_OH, delaySetters);
				}
			}
		}

		void FillOrganizations(TDeclaration declaration, UniversalShipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			var organizationAddressCollection = dataObject.OrganizationAddressCollection;
			var consolDataObject = this.consolDataObject;
			if (consolDataObject != null)
			{
				organizationAddressCollection = organizationAddressCollection.MergeCollection(consolDataObject.OrganizationAddressCollection, true, UniversalDataObjectReaderHelper.IsOrganizationAddressTypeMatched);
			}

			if (organizationAddressCollection != null)
			{
				var declarationRow = GetColumnIndexer(declaration);
				factory.ClearUnmatchedOrgDetailsNotes(declarationRow.GetValue(JobDeclarationSchema.PK), declarationRow.TableName, declaration.IsInDatabase);
				FillOrganizationsCore(new List<OrganizationAddress>(organizationAddressCollection), declaration, delaySetters);
			}
		}

		protected virtual void FillOrganizationsCore(List<OrganizationAddress> organizationAddressCollection, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var declarationColumnIndexer = GetColumnIndexer(declaration);
			var isStandAlone = IsStandAlone(declarationColumnIndexer);
			var declarationRow = GetColumnIndexer(declaration);

			FillSupplier(organizationAddressCollection, declaration, delaySetters);
			FillImporter(organizationAddressCollection, declaration, delaySetters);
			FillManufacturer(organizationAddressCollection, declaration, delaySetters);
			FillSoldToPartyAddress(organizationAddressCollection, declaration, delaySetters);
			FillSeller(organizationAddressCollection, declaration, delaySetters);
			FillBuyingAgent(organizationAddressCollection, declaration, delaySetters);
			FillSellingAgent(organizationAddressCollection, declaration, delaySetters);
			FillDeclarant(organizationAddressCollection, declaration, delaySetters);
			FillShipToParty(organizationAddressCollection, declaration, delaySetters);
			FillExporter(organizationAddressCollection, declaration, delaySetters);
			FillConsigneeAddress(organizationAddressCollection, declaration, delaySetters);
			FillConsignee(organizationAddressCollection, declaration, delaySetters);
			FillBuyer(organizationAddressCollection, declaration, delaySetters);
			FillRepresentative(organizationAddressCollection, declaration, delaySetters);

			FillExternalBroker(organizationAddressCollection, declaration, declarationRow, delaySetters);
			FillControllingAgent(organizationAddressCollection, declaration, declarationRow, delaySetters);
			FillControllingCustomer(organizationAddressCollection, declaration, declarationRow, delaySetters);

			var carrierAddress = organizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Carrier));
			if (carrierAddress != null)
			{
				var declarationCountryCode = declaration.CountryCode;
				var targetCarrierCCode = carrierAddress.RegistrationNumberCollection?.FirstOrDefault(x => x.CountryOfIssue.GetCodeAsUpperCase() == declarationCountryCode && x.Type.GetCodeAsUpperCase() == OrgCusCode.CodeTypes.CarrierCode)?.Value;
				SetValue(declarationColumnIndexer, JobDeclarationSchema.JE_CarrierCode, targetCarrierCCode, delaySetters);
			}

			var shippingLineAddress = organizationAddressCollection.FirstOrDefault(AddressTypes.ShippingLine, nameof(DocAddressType.ShippingLineAddress));
			if (shippingLineAddress != null)
			{
				var shippingLine = new OrganisationDataObjectReader(shippingLineAddress, logger, factory).GetMatched(declaration, OrganisationTypes.Carrier);
				SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_ShippingLine, shippingLine, delaySetters);
			}

			PopulateForwarder(organizationAddressCollection, declaration, delaySetters);
			PopulateOrganisation(organizationAddressCollection, declaration, AddressTypes.CTOAddress);
			PopulateOrganisation(organizationAddressCollection, declaration, AddressTypes.DepotAddress);
			PopulateOrganisation(organizationAddressCollection, declaration, AddressTypes.ContainerYardAddress);

			var deliveryLocalCartageForwarderAddress = organizationAddressCollection.FirstOrDefault(AddressTypes.DeliveryLocalCartage);
			if (deliveryLocalCartageForwarderAddress != null)
			{
				var deliveryLocalCartageForwarder = new OrganisationDataObjectReader(deliveryLocalCartageForwarderAddress, logger, factory).GetMatched();
				if (deliveryLocalCartageForwarder != null)
				{
					if (declaration.DocsAndCartage == null)
					{
						JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(declaration);
					}

					var jobDocsAndCartage = declaration.DocsAndCartage;
					SetValue(GetColumnIndexer(jobDocsAndCartage), JobDocsAndCartageSchema.JP_OA_DeliveryCartageCoAddr, deliveryLocalCartageForwarder.PK, delaySetters);
				}
			}

			var pickupLocalCartageForwarderAddress = organizationAddressCollection.FirstOrDefault(AddressTypes.PickupLocalCartage);
			if (pickupLocalCartageForwarderAddress != null)
			{
				var pickupLocalCartageForwarder = new OrganisationDataObjectReader(pickupLocalCartageForwarderAddress, logger, factory).GetMatched();
				if (pickupLocalCartageForwarder != null)
				{
					if (declaration.DocsAndCartage == null)
					{
						JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(declaration);
					}

					var jobDocsAndCartage = declaration.DocsAndCartage;
					SetValue(GetColumnIndexer(jobDocsAndCartage), JobDocsAndCartageSchema.JP_OA_PickupCartageCoAddr, pickupLocalCartageForwarder.PK, delaySetters);
				}
			}

			var docAddressTypeOverrides = GetDocAddressTypeOverrides();
			var addressTypesHandleSeparately = GetAddressTypesHandleSeparately();
			foreach (var orgAddressDataObject in organizationAddressCollection)
			{
				var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
				if (!addressTypesHandleSeparately.Contains(addressType))
				{
					if (addressType == nameof(DocAddressType.LocalClient))
					{
						var localClientAddress = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory).GetMatched();
						if (localClientAddress != null)
						{
							var job = declaration.Job;
							var loader = new JobHeader.Loader(declaration);
							if (job == null)
							{
								job = loader.TryCreateWithMutex(false);
								var jobTrackerService = factory.BOFactory.ServiceContainer.GetService<JobTrackerService>()
									?? factory.BOFactory.ServiceContainer.AddService(new JobTrackerService(factory.BOFactory));

								jobTrackerService.Add(job);
							}

							if (job != null)
							{
								SetValue(GetColumnIndexer(job), JobHeaderSchema.JH_OA_LocalChargesAddr, localClientAddress.PK, delaySetters);
							}
							else
							{
								string errorMessage = loader.GetJobCreationErrorForService();
								logger.Log(LogType.Warning, errorMessage);
							}
						}
					}
					else
					{
						DocAddressType docAddressTypeOverride;
						if (docAddressTypeOverrides.TryGetValue(addressType, out docAddressTypeOverride))
						{
							if (ShouldCreateDocAddress(declaration, docAddressTypeOverride))
							{
								if (organizationAddressCollection.FirstOrDefault(docAddressTypeOverride.ToString()) == null)
								{
									OrganisationDataObjectReader.MatchedOrNew(declaration, orgAddressDataObject, logger, factory, delaySetters, docAddressTypeOverride);
								}
							}
						}
						else if (Enum.TryParse(addressType, false, out docAddressTypeOverride))
						{
							if (ShouldCreateDocAddress(declaration, docAddressTypeOverride))
							{
								OrganisationDataObjectReader.MatchedOrNew(declaration, orgAddressDataObject, logger, factory, delaySetters);
							}
						}
					}
				}
			}
		}

		protected virtual bool ShouldCreateDocAddress(TDeclaration declaration, DocAddressType addressType)
		{
			return IsStandAlone(GetColumnIndexer(declaration)) || IsAddressTypeSupportedOnShipmentDeclaration(addressType);
		}

		bool IsAddressTypeSupportedOnShipmentDeclaration(DocAddressType addressType)
		{
			return addressType != DocAddressType.SupplierDocumentaryAddress && addressType != DocAddressType.ImporterDocumentaryAddress &&
				addressType != DocAddressType.SupplierPickupDeliveryAddress && addressType != DocAddressType.ImporterPickupDeliveryAddress &&
				addressType != DocAddressType.NotifyParty &&
				addressType != DocAddressType.NotifyParty2 &&
				addressType != DocAddressType.NotifyParty3;
		}

		void PopulateForwarder(List<OrganizationAddress> organizationAddressCollection, TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var declarationRow = GetColumnIndexer(declaration);
			if (!SetForwarderAddress(AddressTypes.Forwarder, organizationAddressCollection, declaration, declarationRow, delaySetters))
			{
				if (MessageTypeCode == ImportExportCodeList.Codes.Export)
				{
					SetForwarderAddress(AddressTypes.SendingForwarderAddress, organizationAddressCollection, declaration, declarationRow, delaySetters);
				}
				else if (MessageTypeCode == ImportExportCodeList.Codes.Import)
				{
					SetForwarderAddress(AddressTypes.ReceivingForwarderAddress, organizationAddressCollection, declaration, declarationRow, delaySetters);
				}
			}
		}

		bool SetForwarderAddress(string addressType, List<OrganizationAddress> organizationAddressCollection, TDeclaration declaration, IColumnIndexer declarationRow, Dictionary<string, ValueSetter> delaySetters)
		{
			var address = organizationAddressCollection.FirstOrDefault(addressType);
			if (address != null)
			{
				var orgAddress = new OrganisationDataObjectReader(address, logger, factory).GetMatched(declaration, OrganisationTypes.Forwarder);
				SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Forwarder, orgAddress, delaySetters);
				return true;
			}

			return false;
		}

		void PopulateOrganisation(List<OrganizationAddress> organizationAddressCollection, TDeclaration declaration, string addressType)
		{
			var docAddress = GetDocAddress(declaration, addressType);
			var departureAndArrival = GetDocAddressDepartureAndArrivalName(addressType);
			if (!SetOrganisationAddress(addressType, declaration, organizationAddressCollection, docAddress) && departureAndArrival != null)
			{
				if (MessageTypeCode == ImportExportCodeList.Codes.Export)
				{
					SetOrganisationAddress(departureAndArrival.Item1, declaration, organizationAddressCollection, docAddress);
				}
				else if (MessageTypeCode == ImportExportCodeList.Codes.Import)
				{
					SetOrganisationAddress(departureAndArrival.Item2, declaration, organizationAddressCollection, docAddress);
				}
			}
		}

		JobDocAddress GetDocAddress(TDeclaration declaration, string addressType)
		{
			switch (addressType)
			{
				case AddressTypes.CTOAddress:
					return declaration.ContainerTerminalOperatorDocAddress;
				case AddressTypes.DepotAddress:
					return declaration.DepotDocAddress;
				case AddressTypes.ContainerYardAddress:
					return declaration.ContainerYardDocAddress;
				default:
					return null;
			}
		}

		Tuple<string, string> GetDocAddressDepartureAndArrivalName(string addressType)
		{
			switch (addressType)
			{
				case AddressTypes.CTOAddress:
					return new Tuple<string, string>(nameof(DocAddressType.DepartureCTOAddress), nameof(DocAddressType.ArrivalCTOAddress));
				case AddressTypes.DepotAddress:
					return new Tuple<string, string>(AddressTypes.DepartureCFSAddress, AddressTypes.ArrivalCFSAddress);
				case AddressTypes.ContainerYardAddress:
					return new Tuple<string, string>(AddressTypes.ContainerYardEmptyPickupAddress, AddressTypes.ContainerYardEmptyReturnAddress);
				default:
					return null;
			}
		}

		bool SetOrganisationAddress(string type, TDeclaration declaration, List<OrganizationAddress> organizationAddressCollection, JobDocAddress jobDocAddress)
		{
			var ctoAddress = organizationAddressCollection.FirstOrDefault(type);
			if (ctoAddress != null)
			{
				var orgReader = new OrganisationDataObjectReader(ctoAddress, logger, factory);
				var orgAddress = orgReader.GetMatched(declaration, OrganisationTypes.Services);
				orgReader.PopulateJobDocAddress(orgAddress, jobDocAddress);
				return true;
			}

			return false;
		}

		protected virtual List<ZString> GetAddressTypesHandleSeparately()
		{
			var result = new List<ZString>();
			result.Add(AddressTypes.Supplier);
			result.Add(AddressTypes.Importer);
			result.Add(nameof(DocAddressType.ConsigneeAddress));
			result.Add(AddressTypes.Forwarder);
			result.Add(AddressTypes.ShippingLine);
			result.Add(nameof(DocAddressType.ShippingLineAddress));
			result.Add(AddressTypes.DeliveryLocalCartage);
			result.Add(AddressTypes.PickupLocalCartage);
			return result;
		}

		protected virtual Dictionary<string, DocAddressType> GetDocAddressTypeOverrides()
		{
			var result = new Dictionary<string, DocAddressType>();
			result.Add(nameof(DocAddressType.ConsigneeDocumentaryAddress), DocAddressType.ImporterDocumentaryAddress);
			result.Add(nameof(DocAddressType.ConsigneePickupDeliveryAddress), DocAddressType.ImporterPickupDeliveryAddress);
			result.Add(nameof(DocAddressType.ConsignorDocumentaryAddress), DocAddressType.SupplierDocumentaryAddress);
			result.Add(nameof(DocAddressType.ConsignorPickupDeliveryAddress), DocAddressType.SupplierPickupDeliveryAddress);
			return result;
		}

		protected void SetOrganisationPK(IColumnIndexer row, SchemaGuidColumn column, OrgAddress address, Dictionary<string, ValueSetter> delaySetters)
		{
			if (address != null)
			{
				SetValue(row, column, address.OA_OH, delaySetters);
			}
		}

		void CreateJobDocsAndCartageIfRequired(TDeclaration declaration)
		{
			if (declaration.DocsAndCartage == null)
			{
				JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(declaration);
			}
		}

		void ReadInJobDocsAndCartageData(TDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var declarationDataObject = IsFromFreight ? shipmentDataObject : this.dataObject;
			if (declarationDataObject.LocalProcessing != null)
			{
				CreateJobDocsAndCartageIfRequired(declaration);
				if (declarationDataObject.LocalProcessing != null)
				{
					new LocalProcessingDataObjectReader(declarationDataObject, logger, factory).PopulateBusinessObject(declaration.DocsAndCartage, delaySetters);
				}
			}
		}

		void ReadInCustomFields(TDeclaration declaration, UniversalShipment dataObject)
		{
			if (dataObject.CustomizedFieldCollection != null && dataObject.CustomizedFieldCollection.Count > 0)
			{
				CreateJobDocsAndCartageIfRequired(declaration);
				var jobDocsAndCartage = declaration.DocsAndCartage;

				var reader = new Freight.DataTransfer.Universal.CustomFieldsDataObjectReader<JobDocsAndCartage>(logger, jobDocsAndCartage, new JobDocsAndCartageCustomFieldsDescriptor());
				var usedCustomFields = reader.ReadCustomFields(dataObject.CustomizedFieldCollection);
				PopulateWorkflowCustomFields(declaration, dataObject, usedCustomFields);
			}
		}

		protected virtual void FillCollections(TDeclaration declaration, UniversalShipment dataObject, bool isStandalone)
		{
			if (isStandalone)
			{
				FillNotes(declaration, dataObject);
			}

			var landedCostDataReader = GetLandedCostDataReader(declaration);
			FillContainers(declaration, dataObject, landedCostDataReader);

			if (isStandalone)
			{
				FillTransports(declaration, dataObject);
			}

			FillPackingLines(declaration, dataObject);
			if (isStandalone)
			{
				PopulateOrders(declaration, dataObject);
			}

			FillAdditionalReferences(declaration, dataObject);
			FillEntryInstructions(declaration, dataObject);
			FillEntryNumbers(declaration);
			if (declaration.IsDeclarationIntegrated)
			{
				FillEntryHeadersAndCommercialInfos(declaration, dataObject, landedCostDataReader);
			}
			else
			{
				FillCommercialInfo(declaration, dataObject, landedCostDataReader, null);
			}

			if (landedCostDataReader != null)
			{
				landedCostDataReader.ReadIntoBusinessObject();
			}
		}

		protected virtual CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, TDeclaration declaration)
		{
			return new CustomsEntryInstructionDataObjectReader(entryInstructionDataObject, logger, Helper, factory, declaration);
		}

		void FillEntryInstructions(TDeclaration declaration, UniversalShipment dataObject)
		{
			var sourceCollection = dataObject.EntryInstructionCollection;
			if (sourceCollection != null && !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction)
			{
				var existingEntryInstructions = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().ToList();
				foreach (var entryInstruction in existingEntryInstructions)
				{
					entryInstruction.FetchForLoadChildEditableObjectsIfNeeded();
					AddEntryInstructionFetchHint(entryInstruction);
				}

				foreach (var entryInstructionDataObject in sourceCollection)
				{
					var customsEntryInstructionDataObjectReader = CreateCustomsEntryInstructionDataObjectReader(entryInstructionDataObject, declaration);
					var entryInstructionBO = customsEntryInstructionDataObjectReader.ReadIntoBusinessObject();
					if (entryInstructionBO != null)
					{
						declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Add(entryInstructionBO);
						existingEntryInstructions.Remove(entryInstructionBO);
						AddSuspendSetterDisposable(customsEntryInstructionDataObjectReader.GetSuspendSetterDisposables());
					}
				}
				existingEntryInstructions.LoadChildrenForDeletion(getAdditionalChildren: AdditionalChildrenHelper.GetAdditionalChildrenIfSupported);
				foreach (var instruction in existingEntryInstructions.ToArray())
				{
					if (instruction.CanDelete)
					{
						instruction.Delete();
					}
					else
					{
						throw new DataObjectReadFailureException(instruction.ReasonForNotAbleToDelete);
					}
				}
			}

			if (sourceCollection != null)
			{
				AddFetchHintsRelatedToEntryInstruction(declaration, sourceCollection, dataObject);
			}
		}

		protected virtual void FillEntryNumbers(TDeclaration declaration)
		{
		}

		protected virtual void AddEntryInstructionFetchHint(CusEntryInstruction instruction)
		{
		}

		protected virtual void AddFetchHintsRelatedToEntryInstruction(TDeclaration declaration, List<EntryInstruction> sourceCollection, UniversalShipment dataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected void FillPackingLines(TDeclaration declaration, UniversalShipment dataObject)
		{
			if (dataObject.PackingLineCollection != null)
			{
				var isPackingInformationRelevant = declaration.IsPackingInformationRelevant;
				if (isPackingInformationRelevant)
				{
					var wayBillRow = GetColumnIndexerFromRow(FindFirstCusDecHouseBillByBillNumberAndType(declaration.PK, dataObject.WayBillNumber.GetValueOrDefault(), GetCustomsBillType(dataObject.WayBillType), dataObject.AddInfoCollection)) ??
						GetColumnIndexer(declaration.PrimaryHouseBill) ??
						GetColumnIndexer(declaration.PrimaryMasterBill);
					var packingGroups = declaration.PackingGroups;
					RemoveUnwantedPackingGroupOrChildrenPackage(packingGroups);
					var supportsParentPackage = declaration.SupportsParentPackage;
					foreach (var packingLineDataObject in dataObject.PackingLineCollection)
					{
						IColumnIndexer billRow = null;
						ZBool multipleBillRowsMatched = false;
						if (packingLineDataObject.BillNumber.HasValue || packingLineDataObject.BillType != null)
						{
							var billNumber = packingLineDataObject.BillNumber.GetValueOrDefault();
							var billType = GetCustomsBillType(packingLineDataObject.BillType);
							var matchedBillRows = factory.RowFactory.FindCusDecHouseBillByBillNumberAndType(declaration.PK, billNumber, billType);
							if (matchedBillRows.Count() > 1)
							{
								logger.Log(Integration.LogType.Warning, Enterprise.Customs.DataTransfer.Res.GetString("62F90136-789E-420B-93AF-20FDC97CAC6D", "Cannot add a package as there are multiple bill records (Type:'{0}', Number:'{1}') matched.", billType, billNumber));
								multipleBillRowsMatched = true;
							}
							else
							{
								billRow = GetColumnIndexerFromRow(matchedBillRows.FirstOrDefault());
								if (billRow == null)
								{
									logger.Log(Integration.LogType.Warning, Enterprise.Customs.DataTransfer.Res.GetString("4BB3990E-9688-4ED9-A3AA-1434458DE2D2", "Cannot find Bill (Type:'{0}', Number:'{1}') for packing line; new Bill added.", billType, billNumber));
									billRow = GetColumnIndexer(declaration.Bills.AddNew());
									var supportDataImporting = billRow as ISupportDataImporting;
									if (supportDataImporting != null)
									{
										supportDataImporting.IsImportingData = true;
									}

									SetValue(billRow, CusDecHouseBillSchema.CU_BillNum, billNumber);
									SetValue(billRow, CusDecHouseBillSchema.CU_BillType, billType);
								}
							}
						}
						if (!multipleBillRowsMatched)
						{
							if (billRow == null)
							{
								billRow = wayBillRow;
							}
							if (billRow == null)
							{
								logger.Log(Integration.LogType.Warning, Enterprise.Customs.DataTransfer.Res.GetString("087C6D24-EE0E-417C-B681-244A2561D0F9", "Cannot add a package as there is no bill record to attach the packages to"));
							}
							else
							{
								CreateNewCustomsPackingLineDataObjectReader(packingLineDataObject, declaration, billRow, supportsParentPackage).ReadIntoBusinessObject();
							}
						}
					}
				}
				else
				{
					foreach (var packingLine in dataObject.PackingLineCollection)
					{
						IColumnIndexer containerRow = null;
						var containerNumber = packingLine.ContainerNumber.GetValueOrDefault();
						if (!containerNumber.IsEmpty)
						{
							var container = declaration.CusContainers.OfType<BaseCusContainer>().FirstOrDefault(x => string.Equals(x.CO_ContainerNumber, containerNumber, StringComparison.OrdinalIgnoreCase));
							if (container == null)
							{
								logger.Log(Integration.LogType.Warning, Enterprise.Customs.DataTransfer.Res.GetString("12345678-B4F8-4214-A2DF-F61274975612", "Cannot find Container ({0}) for invoice-container link; new Container added.", containerNumber));
								containerRow = declaration.CusContainers.AddNew();
								SetValue(containerRow, CusContainerSchema.CO_ContainerNumber, containerNumber);
							}
							else
							{
								containerRow = GetColumnIndexer(container);
							}
						}
						var containerPK = containerRow != null ? containerRow.GetValue(CusContainerSchema.PK) : ZGuid.Empty;
						Helper.AddContainerInvoiceLineMap(containerPK, packingLine.PackedItemCollection);
					}
				}
			}
		}

		protected virtual CustomsPackingLineDataObjectReader CreateNewCustomsPackingLineDataObjectReader(PackingLine packingLineDataObject, BaseJobDeclaration declaration, IColumnIndexer billRow, bool supportsParentPackage)
		{
			return new CustomsPackingLineDataObjectReader(packingLineDataObject, logger, Helper, declaration, billRow, supportsParentPackage);
		}

		protected virtual void RemoveUnwantedPackingGroupOrChildrenPackage(BaseDeclarationLevelPackingGroupCollection packingGroups)
		{
			packingGroups.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData();
			packingGroups.RemoveAndDeleteAll();
		}

		ZString GetCustomsBillType(WayBillType wayBillType)
		{
			return Helper.GetCustomsBillType(wayBillType);
		}

		#region PopulateOrders

		void PopulateOrders(TDeclaration declaration, UniversalShipment dataObject)
		{
			var supportDataImporting = declaration as ISupportDataImporting;
			using ((supportDataImporting == null || supportDataImporting.IsImportingData) ? DisposableAction.NoAction : new DisposableAction(new Action(() => supportDataImporting.IsImportingData = true), new Action(() => supportDataImporting.IsImportingData = false)))
			{
				var orderReadingHelper = new OrderDataObjectReadingHelper(dataObject, logger, factory, declaration);
				orderReadingHelper.PopulateOrders();
			}
		}

		#endregion

		protected virtual DataRow FindFirstCusDecHouseBillByBillNumberAndType(ZGuid declarationPK, ZString billNumber, ZString billType, List<AddInfo> addInfoCollection)
		{
			return factory.RowFactory.FindFirstCusDecHouseBillByBillNumberAndType(declarationPK, billNumber, billType);
		}

		protected virtual void FillCountrySpecificDetails(TDeclaration declaration, UniversalShipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
		}

		void FillRealFieldFromAddInfo(TDeclaration declaration, UniversalShipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			var hasAddInfoData = (dataObject.AddInfoCollection?.Count ?? 0) > 0;
			if (hasAddInfoData)
			{
				var declarationRow = GetColumnIndexer(declaration);
				FillRealFieldFromAddInfoCore(declarationRow, declaration, dataObject.AddInfoCollection, delaySetters);
			}
		}

		protected virtual void FillRealFieldFromAddInfoCore(IColumnIndexer declarationRow, TDeclaration declaration, List<AddInfo> addInfos, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(declarationRow, JobDeclarationSchema.JE_TransportModeInland, addInfos.GetZStringValue(Constants.AddInfoKeys.Declaration.InlandModeOfTransport), delaySetters);
			if (declaration.SupportUseOwnerRefAsQuarantineRefUsage)
			{
				SetValue(declarationRow, JobDeclarationSchema.JE_UseOwnerRefAsQuarantineRef, addInfos.GetZBoolValue(Constants.AddInfoKeys.Declaration.UseOwnerRefAsQuarantineRef), delaySetters);
			}
		}

		protected virtual void FillDates(TDeclaration declaration, UniversalShipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			var dateCollection = dataObject.DateCollection;
			var consolDataObject = this.consolDataObject;
			if (consolDataObject != null)
			{
				dateCollection = dateCollection.MergeCollection(consolDataObject.DateCollection, true, (x, y) => x != null && y != null && x.Type == y.Type);
			}
			if (dateCollection != null && dateCollection.Count > 0)
			{
				var declarationRow = GetColumnIndexer(declaration);
				FillDates(declarationRow, delaySetters, dateCollection, ZBool.True, GetEstimatedDateFieldsToRead());
				FillDates(declarationRow, delaySetters, dateCollection, ZBool.False, GetDateFieldsToReadWhichDontNeedEstimatedFirst());
			}
		}

		protected virtual DateTypeSchemaColumnMap[] GetEstimatedDateFieldsToRead()
		{
			return new DateTypeSchemaColumnMap[]
			{
				new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_DateAtFinalDestination, DateType.Arrival),
				new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_DateAtOrigin, DateType.Departure)
			};
		}

		protected virtual DateTypeSchemaColumnMap[] GetDateFieldsToReadWhichDontNeedEstimatedFirst()
		{
			return new DateTypeSchemaColumnMap[]
			{
				new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_DateOfArrival, DateType.DischargeDate),
				new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_DateOfFirstArrival, DateType.FirstArrivalInCountry),
				new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_ExportDate, DateType.LoadingDate),
				new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_EntryDate, DateType.EntryDate)
			};
		}

		protected void FillDates(IColumnIndexer row, Dictionary<string, ValueSetter> delaySetters, List<Date> dateCollection, ZBool useEstimatedFirst, params DateTypeSchemaColumnMap[] dateFields)
		{
			if (dateFields != null)
			{
				foreach (var dateField in dateFields)
				{
					foreach (var dateType in dateField.DateTypes)
					{
						var date = dateCollection.FirstOrDefault(dateType, useEstimatedFirst) ?? dateCollection.FirstOrDefault(dateType, !useEstimatedFirst);
						if (date != null)
						{
							SetValue(row, dateField.SchemaColumn, date.Value, delaySetters);
							break;
						}
					}
				}
			}
		}

		protected ILandedCostDataReader GetLandedCostDataReader(TDeclaration declaration)
		{
			ILandedCostDataReader landedCostDataReader = null;
			if (((ILandedCostHeader)declaration).IsLCSupported)
			{
				landedCostDataReader = ObjectFactory.New<ILandedCostDataReader>(logger, factory, declaration);
			}

			return landedCostDataReader;
		}

		void FillCommercialInfoOnly(TDeclaration declaration, UniversalShipment dataObject)
		{
			var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
			CalculateMessageType(dataObject, declaration, delaySetters);

			var landedCostDataReader = GetLandedCostDataReader(declaration);
			FillCommercialInfo(declaration, dataObject, landedCostDataReader, null);
			if (landedCostDataReader != null)
			{
				landedCostDataReader.ReadIntoBusinessObject();
			}
		}

		protected void FillCommercialInfo(TDeclaration declaration, UniversalShipment dataObject, ILandedCostDataReader landedCostDataReader, List<CommonJobComInvoiceHeader> existingInvoiceList)
		{
			var commercialInfo = dataObject.CommercialInfo;
			if (commercialInfo != null)
			{
				AddFetchHintsRelatedToCommerialInvoice(commercialInfo, declaration);

				CommercialInfoMatcher infoMatcher = null;

				var topGroupInvoice = declaration.TopGroupInvoice;
				if (topGroupInvoice == null)
				{
					topGroupInvoice = declaration.JobComInvoiceGroupHeaders.AddNew();
				}
				else
				{
					bool shouldAddFetchHints = IsDefaultingEnabled && declaration.IsInDatabase;
					if (commercialInfo.CommercialChargeCollection != null)
					{
						if (shouldAddFetchHints)
						{
							topGroupInvoice.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData(new IBusiness[] { topGroupInvoice.Charges });
						}
						topGroupInvoice.Charges.RemoveAndDeleteAll();
					}

					infoMatcher = Helper.SetupCommercialInfoMatcher(declaration, logger, commercialInfo, commercialInvoiceCollectionContent);
					DeleteInvoiceHeadersAndGroups(declaration, infoMatcher, topGroupInvoice, commercialInfo, existingInvoiceList, shouldAddFetchHints);
				}

				if (existingInvoiceList == null && infoMatcher != null && infoMatcher.HasMatchedResult)
				{
					existingInvoiceList = infoMatcher.GetInvoices().Cast<CommonJobComInvoiceHeader>().ToList();
				}

				FillCommercialInfo(commercialInfo, (TInvoiceGroupHeader)topGroupInvoice, landedCostDataReader, existingInvoiceList, false);
			}
		}

		void DeleteInvoiceHeadersAndGroups(TDeclaration declaration, CommercialInfoMatcher infoMatcher, BaseJobComInvoiceGroupHeader topGroupInvoice, CommercialInfo commercialInfo, List<CommonJobComInvoiceHeader> existingInvoiceList, bool shouldAddFetchHints)
		{
			var shouldDeleteTransportLogisticsCost = false;

			if (existingInvoiceList == null)
			{
				if ((!commercialInvoiceCollectionContent.HasValue || commercialInvoiceCollectionContent.Value == CollectionContent.Complete)
					&& (commercialInfo.CommercialInvoiceCollection != null || commercialInfo.SubGroupCollection != null))
				{
					var invoiceHeaders = topGroupInvoice.JobComInvoiceHeaders;
					var invoiceGroupHeaders = topGroupInvoice.JobComInvoiceGroupHeaders;

					if (shouldAddFetchHints)
					{
						var children = new List<IBusiness>
						{
							invoiceHeaders,
							invoiceGroupHeaders
						};

						var invoiceLineQuery = new ZQuery(JobComInvoiceLineSchema.JI_ClusterKey, declaration.JE_ClusterKey);
						invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_JZ, topGroupInvoice.JobComInvoiceHeaders.GetPKs());
						children.AddRange(factory.Load<BaseJobComInvoiceLine>(invoiceLineQuery));

						topGroupInvoice.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData(children.ToArray());
					}

					var excludedInvoiceHeaders = infoMatcher.GetInvoices().ToList();

					var invoiceHeadersForDelete = excludedInvoiceHeaders.Any()
						? invoiceHeaders.Where(c => excludedInvoiceHeaders.All(d => d.PK != c.PK))
						: invoiceHeaders;

					var invoiceGroupHeadersForDelete = excludedInvoiceHeaders.Any()
						? invoiceGroupHeaders.Cast<BaseJobComInvoiceGroupHeader>().Where(c => excludedInvoiceHeaders.All(d => !c.IsThisInvoicePartOfThisGroup(d)))
						: invoiceGroupHeaders.Cast<BaseJobComInvoiceGroupHeader>();

					if (shouldAddFetchHints)
					{
						invoiceHeadersForDelete.ForEach(c => c.FetchStrategy.FetchForDelete());
						invoiceGroupHeadersForDelete.ForEach(c => c.FetchStrategy.FetchForDelete());
					}

					invoiceHeadersForDelete.DeleteAll();
					invoiceGroupHeadersForDelete.DeleteAll();

					shouldDeleteTransportLogisticsCost = true;
				}
			}
			else
			{
				DeleteInvoiceLinesNotLinkToEntry(declaration, existingInvoiceList);
			}

			if (shouldDeleteTransportLogisticsCost || commercialInfo.TransportLogisticsCostCollection != null)
			{
				DeleteTransportLogisticsCost(declaration, new ZQuery(LandCostInputSchema.LI_ParentTableCode, new[]
				{
					JobComInvoiceLineSchema.Constants.Prefix,
					JobComInvoiceHeaderSchema.Constants.Prefix
				}));
			}
		}

		void DeleteInvoiceLinesNotLinkToEntry(TDeclaration declaration, List<CommonJobComInvoiceHeader> existingInvoiceList)
		{
			if (existingInvoiceList.Count > 0)
			{
				var invoices = existingInvoiceList.OfType<BaseJobComInvoiceHeader>().ToArray();
				if (invoices.Length > 0)
				{
					var invoiceLineQuery = new ZQuery(JobComInvoiceLineSchema.JI_ClusterKey, declaration.JE_ClusterKey);
					invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_JZ, invoices.Select(x => x.PK));
					invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_CL, DBNull.Value);
					var matchingKeys = GetAllInvoiceLineMatchingKeysFromDataObject(dataObject, helper);
					matchingKeys.ForEach(mk => invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_MatchingKey, SQLComparisonOperator.NotEqual, mk));
					var invoiceLines = Helper.Load<BaseJobComInvoiceLine>(invoiceLineQuery);
					if (invoiceLines.Length > 0)
					{
						var query = new ZQuery(LandCostInputSchema.LI_ParentTableCode, JobComInvoiceLineSchema.Constants.Prefix);
						query.AddToFilter(LandCostInputSchema.LI_ParentID, invoiceLines.Select(x => x.PK));
						invoiceLines.DeleteAll(true, getAdditionalChildren: AdditionalChildrenHelper.GetAdditionalChildrenIfSupported);
						DeleteTransportLogisticsCost(declaration, query);
					}
				}
			}
		}

		void DeleteTransportLogisticsCost(TDeclaration declaration, ZQuery additionalQuery)
		{
			var landedCostHeader = declaration.LandedCostHeaderForDocuments as LandedCosting.ILandedCostHeader;
			if (landedCostHeader != null)
			{
				var query = new ZQuery(LandCostInputSchema.LI_LT, landedCostHeader.PK);
				query.AddToFilter(additionalQuery);
				Helper.Load<LandedCosting.ILandCostInput>(query).OfType<BusinessObject>().DeleteAll();
			}
		}

		void FillAdditionalReferences(TDeclaration declaration, UniversalShipment dataObject)
		{
			if (dataObject.AdditionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new JobDeclarationAdditionalReferenceCollectionReader(dataObject.AdditionalReferenceCollection, logger, factory, declaration);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}
		}

		void FillTransports(TDeclaration declaration, UniversalShipment dataObject)
		{
			if (dataObject.TransportLegCollection != null)
			{
				var reader = new TransportLegCollectionReader<Transport>(dataObject.TransportLegCollection, logger, factory, declaration);
				reader.ReadIntoCollection();
			}
		}

		void FillContainers(TDeclaration declaration, UniversalShipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			if (dataObject.ContainerCollection != null)
			{
				var containerMode = GetCustomsContainerMode(declaration, dataObject).GetNullableCodeAsUpperCase();
				var xmlContainerMode = ZString.Empty;
				if (containerMode.HasValue && !containerMode.Value.IsEmpty && declaration.JE_ContainerMode == containerMode.Value)
				{
					xmlContainerMode = containerMode.Value;
				}
				if (dataObject.ContainerCollection.Content == null || dataObject.ContainerCollection.Content == CollectionContent.Complete)
				{
					if (IsDefaultingEnabled && declaration.IsInDatabase)
					{
						declaration.LoadChildEditableObjectsForChild(new IBusiness[] { declaration.CusContainers });
					}

					var existingContainers = new List<BaseCusContainer>(new TypedEnumerable<BaseCusContainer>(declaration.CusContainers));
					foreach (var containerDataObject in dataObject.ContainerCollection)
					{
						var container = GetNewCustomsContainerDataObjectReader(containerDataObject, declaration, landedCostDataReader).ReadIntoBusinessObject();
						existingContainers.Remove(container);
					}
					if (existingContainers.Count > 0)
					{
						DeleteTransportLogisticsCost(declaration, new ZQuery(LandCostInputSchema.LI_ParentID, existingContainers.Select(x => x.PK)));
						existingContainers.DeleteAll(true, getAdditionalChildren: AdditionalChildrenHelper.GetAdditionalChildrenIfSupported);
					}
				}
				else if (dataObject.ContainerCollection.Content == CollectionContent.Partial)
				{
					foreach (var containerDataObject in dataObject.ContainerCollection)
					{
						GetNewCustomsContainerDataObjectReader(containerDataObject, declaration, landedCostDataReader).ReadIntoBusinessObject();
					}
				}
				if (!xmlContainerMode.IsEmpty && declaration.JE_ContainerMode != xmlContainerMode)
				{
					declaration.JE_ContainerMode = xmlContainerMode;
				}
			}
		}

		protected abstract CustomsContainerDataObjectReader<TDeclaration, TContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, TDeclaration declaration, ILandedCostDataReader landedCostDataReader);

		void FillAdditionalBills(TDeclaration declaration, UniversalShipment dataObject, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			var isDefaultingEnabled = IsDefaultingEnabled;
			if (dataObject.AdditionalBillCollection != null && dataObject.AdditionalBillCollection.Count > 0)
			{
				if (isDefaultingEnabled && declaration.IsInDatabase)
				{
					declaration.LoadChildEditableObjectsForChild(new IBusiness[] { declaration.PackingGroups, declaration.Bills });
				}

				using var additionalBillDataProvider = new AdditionalBillDataProvider<TBill>(declaration);
				var collection = new List<AdditionalBill>(dataObject.AdditionalBillCollection);
				collection.Sort(SortByBillTypeAndBillNumber);
				var billsNeedRefresh = new List<TBill>();
				foreach (var additionalBillDataObject in collection)
				{
					billsNeedRefresh.Add(GetNewAdditionalBillDataObjectReader(additionalBillDataObject, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail).ReadIntoBusinessObject());
				}
				var existingBills = new List<TBill>(additionalBillDataProvider.GetUnprocessedExistingBills());
				if (declaration.PrimaryHouseBill is TBill primaryHouseBill)
				{
					billsNeedRefresh.Add(primaryHouseBill);
					existingBills.Remove(primaryHouseBill);
				}
				if (declaration.PrimaryMasterBill is TBill primaryMasterBill)
				{
					billsNeedRefresh.Add(primaryMasterBill);
					existingBills.Remove(primaryMasterBill);
				}
				existingBills.DeleteAll();
				if (!isDefaultingEnabled)
				{
					billsNeedRefresh.ForEach(x => x.ChildBills.Rebuild());
				}
			}
			else if (!isDefaultingEnabled)
			{
				if (primaryMasterBillDetail != null && primaryMasterBillDetail.BillNumber.HasValue)
				{
					FindOrCreateNewPrimaryBill(declaration, primaryMasterBillDetail, BillTypeList.Codes.MasterBill);
				}
				if (primaryHouseBillDetail != null && primaryHouseBillDetail.BillNumber.HasValue)
				{
					FindOrCreateNewPrimaryBill(declaration, primaryHouseBillDetail, BillTypeList.Codes.HouseBill);
				}
			}
			if (!isDefaultingEnabled)
			{
				declaration.LowestBills.Rebuild();
			}
		}

		IColumnIndexer FindOrCreateNewPrimaryBill(TDeclaration declaration, BillDetail billDetail, ZString billType)
		{
			var subFilter = new ZQuery(CusDecHouseBillSchema.CU_GUIPresentationRecord, ZBool.True);
			subFilter.AddToFilter(JoinCondition.Or, CusDecHouseBillSchema.CU_BillNum, billDetail.BillNumber.Value);
			var query = new ZQuery(CusDecHouseBillSchema.CU_ClusterKey, declaration.JE_ClusterKey);
			query.AddToFilter(CusDecHouseBillSchema.CU_JE, declaration.PK);
			query.AddToFilter(CusDecHouseBillSchema.CU_BillType, billType);
			query.AddToFilter(subFilter);
			query.OrderBy = CusDecHouseBillSchema.CU_GUIPresentationRecord.Name;
			IColumnIndexer primaryBill = null;
			foreach (IColumnIndexer bill in factory.RowFactory.Load(CusDecHouseBillSchema.Constants.TableName, query))
			{
				primaryBill = bill;
				if (bill.GetValue(CusDecHouseBillSchema.CU_GUIPresentationRecord))
				{
					break;
				}
			}
			if (primaryBill == null)
			{
				primaryBill = GetColumnIndexer(factory.New(declaration.Bills.TypeOfElements));
				SetValue(primaryBill, CusDecHouseBillSchema.CU_JE, declaration.PK);
				SetValue(primaryBill, CusDecHouseBillSchema.CU_ClusterKey, declaration.JE_ClusterKey);
				SetValue(primaryBill, CusDecHouseBillSchema.CU_BillType, billType);
			}
			SetValue(primaryBill, CusDecHouseBillSchema.CU_BillNum, billDetail.BillNumber.Value);
			SetValue(primaryBill, CusDecHouseBillSchema.CU_GUIPresentationRecord, ZBool.True);

			return primaryBill;
		}

		protected abstract AdditionalBillDataObjectReader<TBill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<TBill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail);

		void FillNotes(TDeclaration declaration, UniversalShipment dataObject)
		{
			if (dataObject.NoteCollection != null)
			{
				// TODO : Handle Linked to Shipment
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, declaration).ReadIntoCollection();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		void FillCommercialInfo(CommercialInfo commercialInfo, TInvoiceGroupHeader groupHeader, ILandedCostDataReader landedCostDataReader, List<CommonJobComInvoiceHeader> existingInvoiceList, bool setInvoiceNumber)
		{
			var groupHeaderRow = GetColumnIndexer(groupHeader);
			var addInfoManager = groupHeader as IAddInfoManager;
			var isAddInfoSerialisationEnabled = addInfoManager != null && !IsDefaultingEnabled;
			try
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
				}
				if (setInvoiceNumber)
				{
					SetValue(groupHeaderRow, JobComInvoiceHeaderSchema.JZ_InvoiceNumber, commercialInfo.Name);
				}
				var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
				if (addInfoManager != null && Helper.IsSourceAndTargetCountrySame)
				{
					GetNewAddInfoDataObjectReaderForInvoiceGroupHeader(groupHeader).ReadIntoRow(addInfoManager, groupHeaderRow, dataObject, delaySetters);
				}

				delaySetters.SetValueInSpecificOrder(GetSettingOrder(groupHeader));
				if (IsDefaultingEnabled)
				{
					addInfoManager.UpdateRelatedPropertyInfo();
				}
				if (Helper.IsSourceAndTargetCountrySame)
				{
					var groupHeaderPK = groupHeaderRow.GetValue(JobComInvoiceHeaderSchema.PK);
					var groupHeaderIsInDatabase = groupHeader.IsInDatabase;
					var tablePrefix = JobComInvoiceHeaderSchema.Constants.Prefix;
					new AddInfoGroupCollectionDataObjectReader(logger, Helper, dataContext: Constants.DataContext.InvoiceGroup).ReadIntoDataRows(groupHeaderPK, tablePrefix, groupHeaderIsInDatabase, commercialInfo);
					new CustomsReferenceCollectionDataObjectReader(logger, Helper, dataContext: Constants.DataContext.InvoiceGroup).ReadIntoDataRows(groupHeaderPK, tablePrefix, groupHeaderIsInDatabase, commercialInfo);
				}
				CommercialChargeDataObjectReader<BaseGroupInvoiceCharge>.FillCommercialInfo(commercialInfo.CommercialChargeCollection, groupHeader, logger, factory);
			}
			finally
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.UpdateAddInfoFromString(groupHeaderRow.GetValue(JobComInvoiceHeaderSchema.JZ_AddInfo));
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
				}
			}
			if (landedCostDataReader != null)
			{
				landedCostDataReader.CollectTransportLogisticsCost(groupHeader, commercialInfo);
			}

			if (commercialInfo.CommercialInvoiceCollection != null)
			{
				foreach (var invoiceData in commercialInfo.CommercialInvoiceCollection)
				{
					var invoice = CreateNewCommercialInvoiceHeaderDataObjectReader(groupHeader, invoiceData, dataObject, landedCostDataReader).ReadIntoBusinessObject(existingInvoiceList != null, commercialInfo.CommercialInvoiceCollection.Content);
					if (existingInvoiceList != null)
					{
						existingInvoiceList.Remove(invoice);
					}
				}
			}

			if (commercialInfo.SubGroupCollection != null)
			{
				foreach (var group in commercialInfo.SubGroupCollection)
				{
					var groupName = group.Name.GetValueOrDefault();
					TInvoiceGroupHeader subGroupHeader = null;
					subGroupHeader = groupHeader.JobComInvoiceGroupHeaders.Cast<TInvoiceGroupHeader>().FirstOrDefault(x => x.JZ_InvoiceNumber == groupName);
					if (subGroupHeader == null)
					{
						subGroupHeader = (TInvoiceGroupHeader)groupHeader.JobComInvoiceGroupHeaders.AddNew();
					}
					else if (existingInvoiceList != null)
					{
						existingInvoiceList.Remove(subGroupHeader);
					}
					FillCommercialInfo(group, subGroupHeader, landedCostDataReader, existingInvoiceList, true);
				}
			}
		}

		protected virtual AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForInvoiceGroupHeader(TInvoiceGroupHeader groupHeader)
		{
			return AddInfoDataObjectReader.New(groupHeader, logger, helper, JobComInvoiceHeaderSchema.JZ_AddInfo);
		}

		protected virtual IEnumerable<ZString> GetSettingOrder(TInvoiceGroupHeader groupHeader)
		{
			return Enumerable.Empty<ZString>();
		}

		protected abstract CommercialInvoiceHeaderDataObjectReader<TInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(TInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, UniversalShipment dataObject, ILandedCostDataReader landedCostDataReader);

		void FillEntryHeadersAndCommercialInfos(TDeclaration declaration, UniversalShipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			bool partialDataReplace = declaration.IsInDatabase && declaration.CustomsEntryHeaders.Count > 0 && dataObject.CommercialInfo != null;
			if (dataObject.EntryHeaderCollection != null && dataObject.EntryHeaderCollection.Count > 0)
			{
				foreach (var entryHeaderDataObject in dataObject.EntryHeaderCollection)
				{
					var entryHeaderBO = CreateCustomsEntryHeaderDataObjectReader(entryHeaderDataObject, declaration, GetAllInvoiceLineMatchingKeysFromDataObject(dataObject, helper)).ReadIntoBusinessObject();
					if (entryHeaderBO != null)
					{
						declaration.CustomsEntryHeaders.Add(entryHeaderBO);
					}
				}
			}
			List<CommonJobComInvoiceHeader> existingInvoiceList = null;
			if (partialDataReplace)
			{
				var invoiceQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_ClusterKey, declaration.JE_ClusterKey);
				invoiceQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
				existingInvoiceList = Helper.Load<CommonJobComInvoiceHeader>(invoiceQuery).ToList();
				existingInvoiceList.Remove(declaration.TopGroupInvoice); // existing TopGroup should not be deleted
			}

			FillCommercialInfo(declaration, dataObject, landedCostDataReader, existingInvoiceList);
			if (existingInvoiceList != null)
			{
				foreach (var invoice in existingInvoiceList.OfType<BaseJobComInvoiceHeader>()) // Delete existing invoices without lines... need to keep new invoices without line as per xml
				{
					if (factory.Load<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_JZ, invoice.PK)).Length == 0)
					{
						invoice.Delete();
					}
				}
				var existingSubGroupHeaders = existingInvoiceList.OfType<BaseJobComInvoiceGroupHeader>().ToList();
				if (existingSubGroupHeaders.Count > 0)
				{
					DeleteGroupHeaderIfNoInvoice(declaration.TopGroupInvoice, existingSubGroupHeaders);
				}
			}
		}

		void DeleteGroupHeaderIfNoInvoice(BaseJobComInvoiceGroupHeader groupHeader, List<BaseJobComInvoiceGroupHeader> existingSubGroupHeaders)
		{
			var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_ClusterKey, groupHeader.JZ_ClusterKey);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK, groupHeader.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.True);
			query.FetchOnlyFromLocalCache = true;

			foreach (var subGroupHeader in Helper.Load<BaseJobComInvoiceGroupHeader>(query))
			{
				DeleteGroupHeaderIfNoInvoice(subGroupHeader, existingSubGroupHeaders);
			}

			if (existingSubGroupHeaders.Contains(groupHeader) && !groupHeader.JZ_JZ_GroupInvoiceFK.IsEmpty) // Declaration.TopGroupInvoice should never be deleted
			{
				var invoiceQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_ClusterKey, groupHeader.JZ_ClusterKey);
				invoiceQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK, groupHeader.PK);
				invoiceQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
				invoiceQuery.FetchOnlyFromLocalCache = true;
				if (factory.LoadTop1<BaseJobComInvoiceHeader>(invoiceQuery) == null)
				{
					query = new ZQuery(JobComInvoiceHeaderSchema.JZ_ClusterKey, groupHeader.JZ_ClusterKey);
					query.AddToFilter(JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK, groupHeader.PK);
					query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.True);
					query.FetchOnlyFromLocalCache = true;
					if (factory.LoadTop1<BaseJobComInvoiceGroupHeader>(query) == null)
					{
						groupHeader.Delete();
					}
				}
			}
		}

		protected virtual CustomsEntryHeaderDataObjectReader CreateCustomsEntryHeaderDataObjectReader(EntryHeader entryHeaderDataObject, TDeclaration declaration, List<ZString> matchingKeys = null)
		{
			return new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, Helper, declaration, ZGuid.Empty, matchingKeys);
		}

		protected static List<ZString> GetAllInvoiceLineMatchingKeysFromDataObject(UniversalShipment dataObj, UniversalDataObjectReaderHelper helper)
		{
			var matchingKeys = new List<ZString>();
			if (dataObj.CommercialInfo?.CommercialInvoiceCollection is DataObjectList<CommercialInvoiceHeader> headers)
			{
				foreach (var invHeader in headers)
				{
					matchingKeys.AddRange(helper.GetDataImportMatchingKeys(invHeader));
				}
			}

			return matchingKeys;
		}

		static int SortByBillTypeAndBillNumber(AdditionalBill x, AdditionalBill y)
		{
			int result = 0;
			if (x == null && y != null)
			{
				result = -1;
			}
			else if (x != null && y == null)
			{
				result = 1;
			}
			else
			{
				var xValue = GetBillTypeValue(x.BillType.GetCodeAsUpperCase()) + "_" + x.BillNumber.GetValueOrDefault();
				var yValue = GetBillTypeValue(y.BillType.GetCodeAsUpperCase()) + "_" + y.BillNumber.GetValueOrDefault();
				result = xValue.CompareTo(yValue);
			}
			return result;
		}

		static string GetBillTypeValue(ZString billType)
		{
			switch (billType)
			{
				case WayBillTypeList.Codes.Master:
					return "1";
				case WayBillTypeList.Codes.House:
					return "2";
				default:
					return "3";
			}
		}

		void GetAllCommercialInvoiceCollection(CommercialInfo commercialInfo, List<DataObjectList<CommercialInvoiceHeader>> commercialInvoiceCollectionList)
		{
			var commercialInvoiceCollection = commercialInfo.CommercialInvoiceCollection;
			if (commercialInvoiceCollection != null)
			{
				commercialInvoiceCollectionList.Add(commercialInvoiceCollection);
			}

			if (commercialInfo.SubGroupCollection != null)
			{
				foreach (var group in commercialInfo.SubGroupCollection)
				{
					GetAllCommercialInvoiceCollection(group, commercialInvoiceCollectionList);
				}
			}
		}

		protected virtual ZInt MaxInvoiceLineCount => short.MaxValue - 1;

		void CheckSourceDataIsValid(List<DataObjectList<CommercialInvoiceHeader>> commercialInvoiceCollectionList = null)
		{
			if (commercialInvoiceCollectionList == null)
			{
				commercialInvoiceCollectionList = new List<DataObjectList<CommercialInvoiceHeader>>();
				if (dataObject.CommercialInfo != null)
				{
					GetAllCommercialInvoiceCollection(dataObject.CommercialInfo, commercialInvoiceCollectionList);
				}
			}

			if (IsDefaultingEnabled && commercialInvoiceCollectionList.Cast<DataObjectList<CommercialInvoiceHeader>>().Any(c => c.Any(i => (i.CommercialInvoiceLineCollection?.Count ?? 0) > MaxInvoiceLineCount)))
			{
				throw new DataObjectReadFailureException(Res.GetString("29382951-C28D-4948-AE5D-A8190D1A18E4", "Exceeded maximum number of invoice lines of {0}", MaxInvoiceLineCount));
			}
		}

		CollectionContent? commercialInvoiceCollectionContent;
		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TDeclaration declaration)
		{
			if (declaration != null && !allowUpdateOfCustomsDeclarationAfterCommencement)
			{
				declaration.LoadChildEditableObjectsForChild(new IBusiness[] { declaration.CustomsEntryHeaders });
				if (ShouldConsiderDeclarationReason)
				{
					ZString declarationReasonForNotAbleToUpdate = declaration.GetReasonForNotAbleToUpdate();
					if (!declarationReasonForNotAbleToUpdate.IsEmpty)
					{
						return declarationReasonForNotAbleToUpdate;
					}
				}
				if (declaration.HasWHSTransaction)
				{
					return Res.GetString("4704ded1-3367-409c-a5ba-2444947bc043", "There are active warehouse transactions.");
				}

				if (dataObject.CommercialInfo != null)
				{
					var commercialInvoiceCollectionList = new List<DataObjectList<CommercialInvoiceHeader>>();
					GetAllCommercialInvoiceCollection(dataObject.CommercialInfo, commercialInvoiceCollectionList);
					CheckSourceDataIsValid(commercialInvoiceCollectionList);

					if (commercialInvoiceCollectionList.Count > 0)
					{
						commercialInvoiceCollectionContent = commercialInvoiceCollectionList[0].Content;
						if (commercialInvoiceCollectionList.Any(x => x.Content != commercialInvoiceCollectionContent))
						{
							return Res.GetString("34711205-0742-428C-85B9-0E61E12FBB93", "All Commercial Invoice Collection's Content attribute must be identical.");
						}

						if (commercialInvoiceCollectionList.Count > 1)
						{
							var invoiceNumbers = new HashSet<ZString>();
							foreach (var commercialInvoiceCollection in commercialInvoiceCollectionList)
							{
								foreach (var commercialInvoice in commercialInvoiceCollection)
								{
									var invoiceNumber = commercialInvoice.InvoiceNumber.GetValueOrDefault();
									if (!invoiceNumber.IsEmpty)
									{
										if (invoiceNumbers.Contains(invoiceNumber))
										{
											return Res.GetString("AC7710CE-A782-495E-8D4B-AF37750135AE", "There is a duplicate Commercial Invoice having same Invoice Number '{0}'.", invoiceNumber);
										}
										else
										{
											invoiceNumbers.Add(invoiceNumber);
										}
									}
								}
							}
						}
					}
				}
			}
			else
			{
				CheckSourceDataIsValid();
			}
			if (shipmentDataSource != null && consolDataSource != null)
			{
				if (shipmentDataObject == null)
				{
					return Res.GetString("B4B87982-C499-461A-9987-99A8D1821225", "Data Source contains both {1} and {2} but no Universal Shipment data was found for {2} with key '{0}'.", shipmentDataSource.Key.GetValueOrDefault(), nameof(DataContextType.ForwardingConsol), nameof(DataContextType.ForwardingShipment));
				}
			}
			if (isMultipleDeclarationsMatchedWithEntryDetails || isMultipleDeclarationsMatchedWitBillDetails)
			{
				return Res.GetString("336BF864-C780-4B3D-8E25-BB27D6B8E670", "Cannot import Entry Details when there are multiple declarations matched with same {0} details.", isMultipleDeclarationsMatchedWithEntryDetails
					? Res.GetString("0feb0804-175a-4bc7-8793-cb912979abe2", "entry")
					: Res.GetString("a19b97e5-750b-45dc-aa5c-448492a03c2a", "bill"));
			}
			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(declaration);
		}

		protected virtual bool ShouldConsiderDeclarationReason => true;

		#region IOrganisationDataObjectReaderSupporter Members

		OrganisationDataObjectReader IOrganisationDataObjectReaderSupporter.CreateNewReader(OrganizationAddress addressData)
		{
			return new OrganisationDataObjectReader(addressData, logger, factory);
		}

		#endregion

		public override DataContextType DataContextType
		{
			get { return DataContextType.CustomsDeclaration; }
		}

		protected override IModuleMatcher<IShipmentDataObjectReader, TDeclaration> GetReferenceAndPartyIDMatcher(UniversalObjectFactory factory)
		{
			var companyFilter = new ZQuery(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);
			var matcher = new ModuleMatcher<IShipmentDataObjectReader, TDeclaration>(factory, companyFilter);

			matcher.AddPossibleMatchReferenceAndOrgHeaderPKs(ReferenceElementName.OwnerRef, JobDeclarationSchema.JE_OwnerRef, MatchableOrganizationType.ImporterDocumentaryAddress, GetImporterPKs, new Score() { FullMatch = 95, ReferenceOnlyMatch = 30, Conflict = 0 });

			matcher.AddPossibleMatchReferenceAndOrgHeaderPKs(ReferenceElementName.OwnerRef, JobDeclarationSchema.JE_OwnerRef, MatchableOrganizationType.SupplierDocumentaryAddress, GetSupplierPKs, new Score() { FullMatch = 90, ReferenceOnlyMatch = 30, Conflict = 0 });

			return matcher;
		}

		protected override bool TryGetExistingBusinessObjectMatchedOnDataTargetKeyCore(ITopLevelDataObject universalShipment, IXmlImportLogger logger, IDataTargetDataObject dataTarget, BusinessObjectFactory factory, out BusinessObject foundBusinessObject)
		{
			var result = base.TryGetExistingBusinessObjectMatchedOnDataTargetKeyCore(universalShipment, logger, dataTarget, factory, out foundBusinessObject);
			if (result && foundBusinessObject == null)
			{
				result = !ShouldCreateDeclarationForMatchingShipmentUsingDataTargetKey(logger, dataTarget, factory);
			}
			return result;
		}

		ZGuid[] GetImporterPKs(BaseJobDeclaration matchingBO)
		{
			ZGuid[] result = null;
			var company = matchingBO.Company;
			if (company != null && company.PK == GlbCompany.CurrentCompany.PK)
			{
				result = new ZGuid[] { matchingBO.JE_OH_Importer };
			}
			return result;
		}

		ZGuid[] GetSupplierPKs(BaseJobDeclaration matchingBO)
		{
			ZGuid[] result = null;
			var company = matchingBO.Company;
			if (company != null && company.PK == GlbCompany.CurrentCompany.PK)
			{
				result = new ZGuid[] { matchingBO.JE_OH_Supplier };
			}
			return result;
		}

		bool ShouldCreateDeclarationForMatchingShipmentUsingDataTargetKey(IXmlImportLogger logger, IDataTargetDataObject dataTarget, BusinessObjectFactory factory)
		{
			bool result = false;
			if (this.Shipment == null)
			{
				logger.LogBoth(LogType.Information, Res.GetString("b74f68ca-8267-422f-ab7a-5358c0e7eb5c", "Fail to find Declaration, try locating matching Shipment using Data Target key."));
				var shipmentQuery = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, dataTarget.Key.GetValueOrDefault());
				var targetShipments = factory.Load<ForwardingShipment>(shipmentQuery);
				if (targetShipments != null && targetShipments.Length > 0)
				{
					if (targetShipments.Length > 1)
					{
						logger.LogBoth(LogType.Error, Res.GetString("db8dad9f-2337-4e49-8500-70540e7f8996", "Found multiple shipments, declaration will not be created."));
					}
					else
					{
						this.Shipment = targetShipments[0];
						logger.LogVerboseOnly(LogType.Information, Res.GetString("a37ef947-9aee-415c-8597-97bcff666685", "Found matching Shipment {0}", Shipment.JS_UniqueConsignRef));

						result = true;
					}
				}
				else
				{
					logger.LogVerboseOnly(LogType.Warning, Res.GetString("85124f16-68af-4bdf-835d-acf7fe1d70b0", "Fail to find matching Shipment"));
				}
			}
			return result;
		}
	}
}
