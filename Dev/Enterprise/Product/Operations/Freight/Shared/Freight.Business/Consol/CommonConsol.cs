using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Business.ShipmentVsConsolHelper;
using static Enterprise.Freight.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;
using IDirectionIsDomesticFreight = Enterprise.Integration.Forwarding.IDirectionIsDomesticFreight;
using INctsHeader = Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Freight.Business
{
	[UserDefinedValues]
	[CodeProperty(CommonConsol.Schema.JK_UniqueConsignRef), DescriptionProperty(CommonConsol.Schema.JK_MasterBillNum)]
	[UniversalCopyWithExtendedEntities]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.CommonConsol)]
	[UniversalCopyIgnoreElement("JobConsolSummaries")]
	[UniversalCopyIgnoreElement("JK_UniqueConsignRef")]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class CommonConsol : AutoJobConsol,
		ICommonConsol,
		IFlightDetailsSuppression,
		ITemplateCopyable,
		ISupportDataImporting,
		IMovementLeg,
		IManifestProvider,
		ICDArchive,
		IEDocsProvider,
		IRoutingSupport,
		IDocAddresses,
		ILocalShippingLineProvider,
		ICreditControlledDocumentDelivery,
		IAdditionalReferenceNumberSupporter,
		IAdditionalReferenceNumberTypeProvider,
		ICusEntryNumberValidationDeciderOfType,
		IBillGenerationSupport,
		IRatingSupporterWithAdapter,
		IPhaseSecuritySupportable,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn,
		IBillingPlugin,
		IContainerParent,
		ISupplyChainSecurityImportExportSupporter,
		IOriginDestinationForDocumentDeliveryRestriction,
		IPortMatchingSupport,
		IComplianceJobDirectionProvider,
		IJobAddressAdditionalInfoSupport,
		IDirectionIsDomesticFreight
	{
		public CommonConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CheckIsSubclass();
		}

		#region Type Decider

		public static readonly ConsolTypeDecider TypeDecider = new ConsolTypeDecider();

		#endregion

		#region New

		public static CommonConsol New(BusinessObjectFactory factory)
		{
			return factory.New<CommonConsol>();
		}

		#endregion

		#region Schema

		public new abstract class Schema : AutoJobConsol.Schema
		{
			public const string JK_JX_JA_RL_NKPortOfLoading = "JK_JX_JA_RL_NKPortOfLoading";
			public const string JK_JX_JB_RL_NKPortOfDischarge = "JK_JX_JB_RL_NKPortOfDischarge";
			public const string JK_JX_JV_NKVessel = "JK_JX_JV_NKVessel";
			public const string JK_JX_JV_VoyageFlight = "JK_JX_JV_VoyageFlight";
			public const string JK_JX_JV_CharterFlightNumber = "JK_JX_JV_CharterFlightNumber";
			public const string JK_JX_JA_E_DEP = "JK_JX_JA_E_DEP";
			public const string JK_JX_JA_E_FirstDEP = "JK_JX_JA_E_FirstDEP";
			public const string JK_JX_JB_E_LastARV = "JK_JX_JB_E_LastARV";
			public const string JK_JX_JB_E_ARV = "JK_JX_JB_E_ARV";
			public const string JK_JX_JA_A_DEP = "JK_JX_JA_A_DEP";
			public const string JK_JX_JA_A_FirstDEP = "JK_JX_JA_A_FirstDEP";
			public const string JK_JX_JB_A_ARV = "JK_JX_JB_A_ARV";
			public const string JK_CTOReceivalCommences = "JK_CTOReceivalCommences";
			public const string JK_DepotReceivalCommences = "JK_DepotReceivalCommences";
			public const string JK_CTOCutOff = "JK_CTOCutOff";
			public const string JK_DepotCutOff = "JK_DepotCutOff";
			public const string JK_DocsCutOff = "JK_DocsCutOff";
			public const string JK_ConsolCutOffDateLocal = "JK_ConsolCutOffDateLocal";
			public const string JK_CTOAvailabilityDate = "JK_CTOAvailabilityDate";
			public const string JK_DepotAvailabilityDate = "JK_DepotAvailabilityDate";
			public const string JK_CTOStorageDate = "JK_CTOStorageDate";
			public const string JK_DepotStorageDate = "JK_DepotStorageDate";
			public const string JK_TransportModeForVoyage = "JK_TransportModeForVoyage";
			public const string JK_VGMCutOff = "JK_VGMCutOff";

			public const string JK_OA_ShippingLineAddress_ReadOnly = "JK_OA_ShippingLineAddress_ReadOnly";
			public const string JK_OA_SendingForwarderAddress_ReadOnly = "JK_OA_SendingForwarderAddress_ReadOnly";
			public const string JK_OA_ReceivingForwarderAddress_ReadOnly = "JK_OA_ReceivingForwarderAddress_ReadOnly";
			public const string JK_Calc_BookingReference_ReadOnly = "JK_Calc_BookingReferenceRead_Only";
			public const string JK_Calc_ContainerCount = "JK_Calc_ContainerCount";
			public const string JK_CRN = "JK_CRN";
			public const string JK_EntryStatus = "JK_EntryStatus";
			public const string JK_EntryType = "JK_EntryType";
			public const string JK_SZB = "JK_SZB";

			public const string JK_CustomString1 = "JK_CustomString1";
			public const string JK_CustomString2 = "JK_CustomString2";
			public const string JK_CustomNumber1 = "JK_CustomNumber1";
			public const string JK_CustomNumber2 = "JK_CustomNumber2";

			public const string ShippingLinePK = "ShippingLinePK";
			public const string SendingForwarderPK = "SendingForwarderPK";
			public const string ReceivingForwarderPK = "ReceivingForwarderPK";

			public const string CreditorPK = "CreditorPK";
			public const string DeparturePackCFSTransportPK = "DeparturePackCFSTransportPK";
			public const string ArrivalUnpackCFSTransportPK = "ArrivalUnpackCFSTransportPK";

			public const string JK_TotalShipmentWeight = "JK_TotalShipmentWeight";
			public const string JK_TotalDocumentedWeight = "JK_TotalDocumentedWeight";
			public const string JK_TotalManifestedWeight = "JK_TotalManifestedWeight";
			public const string JK_TotalShipmentVolume = "JK_TotalShipmentVolume";
			public const string JK_TotalDocumentedVolume = "JK_TotalDocumentedVolume";
			public const string JK_TotalManifestedVolume = "JK_TotalManifestedVolume";
			public const string JK_TotalShipmentChargeable = "JK_TotalShipmentChargeable";
			public const string JK_TotalDocumentedChargeable = "JK_TotalDocumentedChargeable";
			public const string JK_TotalManifestedChargeable = "JK_TotalManifestedChargeable";

			public const string JK_TotalShipmentWeightUnit = "JK_TotalShipmentWeightUnit";
			public const string JK_TotalShipmentVolumeUnit = "JK_TotalShipmentVolumeUnit";
			public const string JK_Calc_TotalShipmentChargeableUnit = "JK_Calc_TotalShipmentChargeableUnit";
			public const string JK_ConsolChargeableUnit = "JK_ConsolChargeableUnit";

			public const string JK_TotalShipmentQuantity = "JK_TotalShipmentQuantity";
			public const string JK_TotalShipmentPackageCount = "JK_TotalShipmentPackageCount";
			public const string JK_Calc_FreeSpace = "JK_Calc_FreeSpace";
			public const string JK_Calc_ActualVolumeWeight = "JK_Calc_ActualVolumeWeight";
			public const string JK_Calc_ActualVolumeWeightUnit = "JK_Calc_ActualVolumeWeightUnit";

			public const string JK_RL_NKLoadForFirstImportTransport = "JK_RL_NKLoadForFirstImportTransport";
			public const string JK_RL_NKDiscForFirstImportTransport = "JK_RL_NKDiscForFirstImportTransport";
			public const string JK_RL_NKDiscForLastImportTransport = "JK_RL_NKDiscForLastImportTransport";

			public const string CFSDepartureByTransportMode = "CFSDepartureByTransportMode";
			public const string CFSArrivalByTransportMode = "CFSArrivalByTransportMode";
		}

		#endregion

		#region Validation

		protected override JobConsolValidation GetNewValidation()
		{
			return new CommonConsolValidation(this);
		}

		public new CommonConsolValidation Validation
		{
			get { return (CommonConsolValidation)base.Validation; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			CheckAllShipmentsForDuplicateLoadingAndDischarge();
			CheckIsCargoOnly();
		}

		protected virtual void CheckAllShipmentsForDuplicateLoadingAndDischarge()
		{
			Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
		}

		void CheckIsCargoOnly()
		{
			foreach (Transport transport in Transports)
			{
				transport.Validation.ValidateJW_IsCargoOnly();
			}
		}

		#endregion

		#region CheckIsSubclass

		void CheckIsSubclass()
		{
			if (this.GetType().Equals(typeof(CommonConsol))
#if DEBUG
 && (ValidTestDataGenerationException.ShouldThrow)
#endif
)
			{
				var errorMessage = string.Format("A CommonConsol should never be constructed in production code. JK_PK = '{0}'", PK);
				ErrorReporter.ReportOnce("ConsolConcreteTypeShouldNotBeCommonConsol", errorMessage);
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CommonConsolFetchStrategy(this);
		}

		#endregion

		#region Read Only Propertiees

		protected internal virtual ZString ConsolType
		{
			get { return "JK"; }
		}

		protected bool JK_RL_NKLastForeignPort_ReadOnly
		{
			get { return !IsSea; }
		}

		protected bool JK_DateLastForeignPort_ReadOnly
		{
			get { return !IsSea; }
		}

		#endregion

		#region Saving

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && this is IJobHeaderParent && fJob != null && fJob.IsDeleted)
			{
				fJob = null;
			}

			if (!saveSucceeded && !IsInDatabase)
			{
				foreach (CommonShipment shipment in Shipments)
				{
					shipment.Logs.UpdateEventReferenceNumbers(Events.Attached, JK_UniqueConsignRef, PK.ToString(), deferFiringWorkflow: true);
				}

				JK_UniqueConsignRef = ZString.Empty;
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded && IsAttachedToStandAloneShipment)
			{
				IsAttachedToStandAloneShipment = false;
			}
		}

		public virtual void PopulateJK_UniqueConsignRefIfNeeded()
		{
			if (!IsDeleted)
			{
				PopulateNumberPropertyIfRequired(JK_UniqueConsignRefInfo, GetNewJK_UniqueConsignRef);
			}
		}

		public ZString GetNewJK_UniqueConsignRef(BusinessObjectFactory factory)
		{
			var consolTarget = new ConsolNumberGeneratorTarget();
			var generator = CreateConsolNumberGenerator(factory, consolTarget);

			var masterBillNumberTarget = NewMasterBillNumberGeneratorTarget();

			if (masterBillNumberTarget != null)
			{
				generator.AdditionalTargets.Add(masterBillNumberTarget);
			}

			generator.Generate();
			generator.EnforceMaxLengths();

			if (masterBillNumberTarget != null)
			{
				JK_MasterBillNum = masterBillNumberTarget.Value.ToUpper();
			}

			numberGenerator = generator;
			ConsignRefHandler = new UniqueIndexHandler(consolTarget);

			return consolTarget.Value;
		}

		protected NumberGenerator CreateConsolNumberGenerator(BusinessObjectFactory factory, NumberGeneratorTarget primaryTarget)
		{
			var generator = new NumberGenerator();
			generator.Factory = factory;
			generator.Context = new NumberGeneratorContext();
			generator.BaseFountain = NumberFountainForUniqueConsignRef;
			generator.FountainGetter = Env.NumberFountains.GetForwardingConsolGeneratorFountain;
			generator.PrimaryTarget = primaryTarget;
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(this));

			return generator;
		}

		protected virtual NumberGeneratorTarget NewMasterBillNumberGeneratorTarget()
		{
			return null;
		}

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			base.OnSaving();

			PopulateJK_UniqueConsignRefIfNeeded();
			UpdateNumbersInShipmentEvents();
			CascadeFreshFreightLoadedUnloadedEvents();

			if (!JK_IsCFS && needRecalculateJK_IsCFS)
			{
				SetTypeFlags();
			}
			needRecalculateJK_IsCFS = false;

			if (JK_ConsolModeInfo.HasChanges)
			{
				foreach (CommonContainer container in Containers)
				{
					container.CalculateJC_EmptyReturnedBy();
				}
			}

			SetDefaultLastForeignPortFirstArrivalPortBasedOnSailingSchedule();

			OnConsolSaving();
		}

		protected virtual void OnConsolSaving()
		{
			ConsolSaving?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler ConsolSaving;

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				var jobHeaderParent = this as IJobHeaderParent;
				if (jobHeaderParent != null)
				{
					JobHeader.DeactivateAllJobs(jobHeaderParent, true);
				}
			}
		}

		void UpdateNumbersInShipmentEvents()
		{
			if (fShipments != null && !IsInDatabase)
			{
				foreach (CommonShipment shipment in Shipments)
				{
					shipment.Logs.UpdateEventReferenceNumbers(Events.Attached, PK.ToString(), this.LogReference(false));
				}
			}
		}

		void CascadeFreshFreightLoadedUnloadedEvents()
		{
			FreightEventsHelper.CascadeIfApplicable(this,
				Events.FreightLoadedCode,
				Shipments.Cast<CommonShipment>().Where(shipment => shipment.JS_ShippedOnBoardDate.IsEmpty));

			FreightEventsHelper.CascadeIfApplicable(this,
				Events.FreightLoadedCode,
				Containers.Cast<CommonContainer>().Where(container => container.JC_FCLOnBoardVessel.IsEmpty));

			FreightEventsHelper.CascadeIfApplicable(this,
				Events.FreightUnloadedCode,
				Containers);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				string result = Res.GetString("65ab5a37-f88d-49d5-8279-09b4fe8e0c63", "Consol");
				if (!JK_UniqueConsignRef.IsEmpty)
				{
					result += " " + JK_UniqueConsignRef;
				}
				if (!JK_MasterBillNum.IsEmpty)
				{
					result += " " + Res.GetString("1b172800-47f7-4ce9-925b-c23447db0558", "(Master Bill='{0}')", JK_MasterBillNum);
				}
				return result;
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = string.Empty;

				if (!JK_UniqueConsignRef.IsEmpty)
				{
					result = JK_UniqueConsignRef;
				}

				if (!JK_RL_NKLoadPort.IsEmpty)
				{
					result += " - " + JK_RL_NKLoadPort;
				}

				if (!JK_RL_NKDischargePort.IsEmpty)
				{
					result += " - " + JK_RL_NKDischargePort;
				}

				return result;
			}
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return fUniqueIndexFailureHandler ?? (fUniqueIndexFailureHandler = new ConsolNumberFountainUniqueIndexFailureHandler(this)); }
		}

		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;

		protected class ConsolNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public ConsolNumberFountainUniqueIndexFailureHandler(CommonConsol consol)
				: base(JobConsolSchema.Constants.Indexes.NR_UX__JK_UniqueConsignRef, consol)
			{
				this.Consol = consol;
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Consol.ConsignRefHandler != null && Consol.ConsignRefHandler.NumberFountain != null ? Consol.ConsignRefHandler.NumberFountain : Consol.NumberFountainForUniqueConsignRef; }
			}

			protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
			{
				if (Consol.ConsignRefHandler == null)
				{
					return base.CommandToFindMaxValueInDatabase(connection);
				}
				else
				{
					return Consol.ConsignRefHandler.FindMaxValueInDatabase(connection, JobConsolSchema.JK_UniqueConsignRef);
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Error Exception")]
			protected override string RetrieveAdditionalFountainInformationForErrorReport(IDbConnected connected)
			{
				var consolFountainInformation = (NoResString)"\n\n" +
					(NoResString)"Number Fountain Information on Consol\r\n" +
					(NoResString)"  Consol.consignRefHandler is null                                      : {0}\r\n" +
					(NoResString)"  Consol.consignRefHandler.NumberFountain is null                       : {1}\r\n" +
					(NoResString)"  Consol.consignRefHandler.NumberFountain.PeekPreliminaryFormatted      : {2}\r\n" +
					(NoResString)"  Consol.FountainUsedForGeneration.PeekPreliminaryFormatted             : {3}\r\n" +
					(NoResString)"  Consol.NumberFountainForUniqueConsignRef.PeekPreliminaryFormatted     : {4}\r\n";

				var isConsignRefHandlerNull = Consol.ConsignRefHandler == null;
				var isConsignRefHandlerFountainNull = Consol.ConsignRefHandler?.NumberFountain == null;

				var consolFountainInformationMessage = string.Format(consolFountainInformation,
					isConsignRefHandlerNull.ToString(),
					isConsignRefHandlerFountainNull.ToString(),
					Consol.ConsignRefHandler?.NumberFountain?.PeekPreliminaryFormatted(connected) ?? "Null",
					Consol.numberGenerator?.PrimaryTarget?.FountainUsedForGeneration?.PeekPreliminaryFormatted(connected) ?? "Null",
					Consol.NumberFountainForUniqueConsignRef.PeekPreliminaryFormatted(connected) ?? "Null"
				);

				if (isConsignRefHandlerNull)
				{
					consolFountainInformationMessage += "\n\nConsignRefHandler Stack Trace:\n" +
						(Consol.consignRefHandlerStackTrace.IsNullOrEmpty() ? "ConsignRefHandler has not been set and has no recorded stack trace." : Consol.consignRefHandlerStackTrace);
				}

				return consolFountainInformationMessage;
			}

			readonly CommonConsol Consol;
		}

		protected UniqueIndexHandler ConsignRefHandler
		{
			get { return consignRefHandler; }
			set
			{
				consignRefHandler = value;
				consignRefHandlerStackTrace = System.Environment.StackTrace;
			}
		}

		protected UniqueIndexHandler consignRefHandler;

		protected string consignRefHandlerStackTrace;

		protected NumberGenerator numberGenerator;

		protected virtual INumberFountainProxy NumberFountainForUniqueConsignRef
		{
			get { return Env.NumberFountains.JobConsolNumber; }
		}

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				DeleteContainersWhenConsolIsDeleted();
				Transports.ParentDeleting();

				(JobAddressAdditionalInfoCollection as JobAddressAdditionalInfoCollection).DeleteAll();

				try
				{
					base.Delete();
				}
				catch (CannotDeleteException)
				{
					Transports.CancelParentDeleting();
					throw;
				}
			}
		}

		protected virtual void DeleteContainersWhenConsolIsDeleted()
		{
			Containers.RemoveAndDeleteAll();
		}
		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if ((kind & TestBusinessObjectKind.PopulateRelatedObjects) != 0)
			{
				TestBusinessObjectKind kindExceptDependentCollections = kind & ~TestBusinessObjectKind.PopulateDependentCollections;
				TestBusinessObjectKind kindExceptRelatedObjects = kind & ~TestBusinessObjectKind.PopulateRelatedObjects;
				TestBusinessObjectKind kindExceptDependentOrRelatedObjects = kind & ~TestBusinessObjectKind.PopulateDependentCollections & ~TestBusinessObjectKind.PopulateRelatedObjects;

				Transport transport = Transports[0];
				transport.JW_JX = ZGuid.Empty;

				JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>(kindExceptDependentCollections);
				VoyageOrigin origin = voyage.Origins.AddNew();
				VoyageDestination destination = voyage.Destinations.AddNew();
				origin.FillWithValidTestData(kindExceptRelatedObjects, propertyPath);
				destination.FillWithValidTestData(kindExceptRelatedObjects, propertyPath);

				JobSailing sailing = voyage.Sailings.AddNew();
				sailing.FillWithValidTestData(kindExceptDependentOrRelatedObjects, Array.Empty<PropertyDescriptor>());
				sailing.JX_JA = origin.PK;
				sailing.JX_JB = destination.PK;
				transport.JW_JX = sailing.PK;
			}
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				// temporary until problem with CIMEDIMessage is fixed
				if (collectionProperty.Name != "Messages" && collectionProperty.Name != "Cartages")
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}

			protected override void PopulateString(ZPropertyInfo property)
			{
				if (property.Value.IsEmpty ||
					property.Name != CommonConsol.Schema.JK_TransportMode)
				{
					base.PopulateString(property);
				}
			}
		}
#endif
		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			fSettingDefaultValues = true;

			try
			{
				base.SetDefaultValues();

				JK_TransportMode = GlbDepartment.CurrentDepartment.TransportMode;
				IsDomesticFreight = GlbDepartment.CurrentDepartment.GE_Domestic;

				if (!Factory.IsConstructingNullBusinessObject)
				{
					var transport = Transports[0];

					using (transport.SuspendSettingHasChanges())
					{
						if (GlbDepartment.CurrentDepartment.GE_Export)
						{
							JK_RL_NKLoadPort = FreightDefaultPortHelper.GetDefaultConsolFirstLoadPort(GlbBranch.CurrentBranch, JK_TransportMode, JK_ConsolMode);
							SetDefaultSendingForwarderAddressWithFallbackToProxy(GlbBranch.CurrentBranch.OrgProxy);
						}
						else if (GlbDepartment.CurrentDepartment.GE_Import)
						{
							JK_RL_NKDischargePort = FreightDefaultPortHelper.GetDefaultConsolLastDischargePort(GlbBranch.CurrentBranch, JK_TransportMode, JK_ConsolMode);
							SetDefaultReceivingForwarderAddressWithFallbackToProxy(GlbBranch.CurrentBranch.OrgProxy);
						}
					}
				}

				if (!string.IsNullOrEmpty(Env.Registry.ConsolPaymentTerm))
				{
					JK_PrepaidCollect = Env.Registry.ConsolPaymentTerm;
				}

				JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;
				JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;

				JK_IsCFS = ZBool.False;
				JK_IsForwarding = ZBool.True;
			}
			finally
			{
				fSettingDefaultValues = false;
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			using (SuspendSettingHasChanges())
			{
				if (JK_IsCancelled)
				{
					UpdateReadOnlyForWhenCancelled();
				}
			}
		}

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>();

				if (ReceivingForwarder != null)
				{
					result.Add(ReceivingForwarder);
				}

				if (SendingForwarder != null)
				{
					result.Add(SendingForwarder);
				}

				if (Creditor != null)
				{
					result.Add(Creditor);
				}

				if (ShippingLine != null)
				{
					result.Add(ShippingLine);
				}

				result.AddRange(Containers.ToArray());

				return result.ToArray();
			}
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = new StmNoteContexts();

				result = StmNoteContextUtils.GetContextFromTransportModeImportExport("", "", JK_TransportMode, JK_ConsolMode);
				result.Module |= base.NoteContextsForRelatedNotes.Module;
				result.Direction |= base.NoteContextsForRelatedNotes.Direction;
				result.FreightMode |= base.NoteContextsForRelatedNotes.FreightMode;

				result.Module |= StmNoteContextModule.F;
				result.Module |= StmNoteContextModule.E;
				foreach (CommonShipment shipment1 in Shipments)
				{
					Factory.AddFetchHint(JobDeclarationSchema.JE_JS, shipment1.PK);
				}

				if (this.IsExport())
				{
					result.Direction |= StmNoteContextDirection.E;
				}
				else if (this.IsImport())
				{
					result.Direction |= StmNoteContextDirection.I;
				}

				foreach (CommonShipment shipment in Shipments)
				{
					if (shipment.Declarations.Length > 0)
					{
						result.Module |= StmNoteContextModule.D;
					}
					if (shipment.IsImport())
					{
						result.Direction |= StmNoteContextDirection.I;
					}

					if (shipment.IsExport())
					{
						result.Direction |= StmNoteContextDirection.E;
					}
				}

				return result;
			}
		}

		public override Notes Notes
		{
			get { return notes ?? (notes = new CommonConsolNotes(this)); }
		}
		Notes notes;

		#endregion

		#region Events

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				foreach (Transport transport in Transports)
				{
					result.Add(transport);
					if (transport.Voyage != null)
					{
						result.Add(transport.Voyage);
					}
				}

				result.AddRange(Numbers);
				result.AddRange(Containers);
				if (IsInDatabase)
				{
					result.AddRange(new InvoiceLoader(Factory).GetInvoicesForUniqueRef(JK_UniqueConsignRef));
				}

				return result.ToArray();
			}
		}

		#endregion

		#region RegisterEditableChildObject

		protected virtual void RegisterEditableChildObject(IBusiness child, ZString childName)
		{
			RegisterEditableChildObject(child);
		}

		#endregion

		#region Phase Security

		public bool IsReadOnlyDueToPhase
		{
			get { return IsReadOnlyDueToPhaseCore; }
		}

		protected virtual bool IsReadOnlyDueToPhaseCore
		{
			get { return false; }
		}

		public bool IsPropertyReadOnlyDueToPhase(ZString propertyName)
		{
			return IsPropertyReadOnlyDueToPhaseCore(propertyName);
		}

		protected virtual bool IsPropertyReadOnlyDueToPhaseCore(ZString propertyName)
		{
			return false;
		}

		#endregion

		#region Properties

		public ICanSyncroniseFromConsol Syncroniser
		{
			get { return fSyncroniser; }
			set { fSyncroniser = value; }
		}
		ICanSyncroniseFromConsol fSyncroniser;

		public ZBool CreditorIsNVOCC
		{
			get
			{
				return Creditor?.ShippingLine?.RSL_IsNVO ?? Creditor?.OH_IsSeaWholesaler ?? false;
			}
		}

		public ZBool CreditorIsCW1User
		{
			get
			{
				return Creditor?.ShippingLine?.RSL_IsCW1User ?? false;
			}
		}

		public ZBool CreditorHasBookingRequestIntegration
		{
			get
			{
				return Creditor?.ShippingLine?.RSL_BookingRequestAvailable ?? false;
			}
		}

		public ZBool ShippingLineIsNVOCC
		{
			get
			{
				var isShippingLine = ShippingLine?.ShippingLine?.RSL_IsShippingLine ?? ShippingLine?.OH_IsShippingLine ?? false;
				var isNVOCC = ShippingLine?.ShippingLine?.RSL_IsNVO ?? ShippingLine?.OH_IsSeaWholesaler ?? false;
				return isNVOCC && (!isShippingLine || JK_ConsolMode == Constants.ContainerModes.LCL);
			}
		}

		public ZBool ShippingLineIsShippingLine
		{
			get
			{
				var isShippingLine = ShippingLine?.ShippingLine?.RSL_IsShippingLine ?? ShippingLine?.OH_IsShippingLine ?? false;
				var isNVOCC = ShippingLine?.ShippingLine?.RSL_IsNVO ?? ShippingLine?.OH_IsSeaWholesaler ?? false;
				return isShippingLine && (!isNVOCC || JK_ConsolMode != Constants.ContainerModes.LCL);
			}
		}

		public ZBool PrintColoadsOnManifest
		{
			get { return JK_PrintOptionForColoadsOnManifest != FreightConstants.PrintOptionForCoLoads.MastersOnly; }
		}

		public ZBool PrintColoadsOnOtherDocs
		{
			get { return JK_PrintOptionForColoadsOnOtherDocs != FreightConstants.PrintOptionForCoLoads.MastersOnly; }
		}

		public ZBool PrintMastersOnManifest
		{
			get { return (JK_PrintOptionForColoadsOnManifest != FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly); }
		}

		public ZBool PrintMastersOnOtherDocs
		{
			get { return (JK_PrintOptionForColoadsOnOtherDocs != FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly); }
		}

		public ZBool IsSavingFromPlanningBoard { get; set; }

		[List("NeutralAirWaybillServiceLevelList")]
		public override ZString JK_AWBServiceLevel
		{
			get { return base.JK_AWBServiceLevel; }
			set { base.JK_AWBServiceLevel = value; }
		}

		[List("JK_ConsolMode_List")]
		public override ZString JK_ConsolMode
		{
			get { return base.JK_ConsolMode; }
			set
			{
				if (JK_ConsolMode != value)
				{
					base.JK_ConsolMode = value;
					GridShipments.Rebuild();
					DefaultCoLoadWithAddress();
				}
			}
		}

		#region JK_JX_Sailing

		public ZGuid JK_JX_Sailing
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZGuid.Empty : transport.JW_JX;
			}
		}

		#endregion

		#region JK_MasterBillNum

		public override ZString JK_MasterBillNum
		{
			get { return base.JK_MasterBillNum; }
			set
			{
				if (IsAir)
				{
					base.JK_MasterBillNum = value.Replace(" ", "").Replace("-", "");
				}
				else
				{
					base.JK_MasterBillNum = value;
				}
			}
		}

		#endregion

		#region JK_MasterBillIssueDate

		public override ZDateTime JK_MasterBillIssueDate
		{
			get { return new ZDateTime(base.JK_MasterBillIssueDate, DateTimeKind.Unspecified); }
			set { base.JK_MasterBillIssueDate = value; }
		}

		#endregion

		#region JK_DateFirstForeignPort

		public override ZDateTime JK_DateFirstForeignPort
		{
			get { return new ZDateTime(base.JK_DateFirstForeignPort, DateTimeKind.Unspecified); }
			set { base.JK_DateFirstForeignPort = value; }
		}

		#endregion

		#region JK_CustomDate1

		public override ZDateTime JK_CustomDate1
		{
			get { return new ZDateTime(base.JK_CustomDate1, DateTimeKind.Unspecified); }
			set { base.JK_CustomDate1 = value; }
		}

		#endregion

		#region JK_CustomDate2

		public override ZDateTime JK_CustomDate2
		{
			get { return new ZDateTime(base.JK_CustomDate2, DateTimeKind.Unspecified); }
			set { base.JK_CustomDate2 = value; }
		}

		#endregion

		#region JK_DateLastForeignPort

		public override ZDateTime JK_DateLastForeignPort
		{
			get { return new ZDateTime(base.JK_DateLastForeignPort, DateTimeKind.Unspecified); }
			set { base.JK_DateLastForeignPort = value; }
		}

		#endregion

		#region JK_DatePortOfFirstArrival

		public override ZDateTime JK_DatePortOfFirstArrival
		{
			get { return new ZDateTime(base.JK_DatePortOfFirstArrival, DateTimeKind.Unspecified); }
			set { base.JK_DatePortOfFirstArrival = value; }
		}

		#endregion

		#region SetIsDomestic

		void SetIsDomestic()
		{
			if (!this.IsUnknown())
			{
				IsDomesticFreight = this.IsDomestic();
			}
		}

		#endregion

		#region SettingDefaultValues

		protected bool fSettingDefaultValues;
		public bool SettingDefaultValues
		{
			get { return fSettingDefaultValues; }
		}

		#endregion

		#region PortOfLoading

		public RefUNLOCO MostInterestingTransportPortOfLoading
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport != null ? transport.LoadPort : null;
			}
		}

		#endregion

		#region PortOfDischarge

		public RefUNLOCO MostInterestingTransportPortOfDischarge
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport != null ? transport.DiscPort : null;
			}
		}

		#endregion

		#region Is Specific Transport Mode

		public bool IsAir
		{
			get { return JK_TransportMode == Constants.TransportModes.Air; }
		}

		public bool IsSea
		{
			get { return JK_TransportMode == Constants.TransportModes.Sea; }
		}

		public bool IsRail
		{
			get { return JK_TransportMode == Constants.TransportModes.Rail; }
		}

		public bool IsRoad
		{
			get { return JK_TransportMode == Constants.TransportModes.Road; }
		}

		#endregion

		#region Is Specific Agent Type

		public ZBool IsDirect
		{
			get { return JK_AgentType == Constants.AgentType.Direct; }
		}

		public ZBool IsCoLoad
		{
			get { return JK_AgentType == Constants.AgentType.CoLoad; }
		}

		public ZBool IsAgent
		{
			get { return JK_AgentType == Constants.AgentType.Agent; }
		}

		public ZBool IsCharter
		{
			get { return JK_AgentType == Constants.AgentType.Charter; }
		}

		public bool IsAWBCoload
		{
			get { return JK_AgentType == Constants.AgentType.AWBCoload; }
		}

		public bool IsMultiAWBMaster
		{
			get { return JK_AgentType == Constants.AgentType.AWBMaster; }
		}

		public bool IsCourier
		{
			get { return JK_AgentType == Constants.AgentType.Courier; }
		}

		public bool IsAgentOrDirect
		{
			get { return IsAgent || IsCourier || IsDirect; }
		}

		public bool IsSendingOrReceivingForwarderGateway
		{
			get => !JK_SendingForwarderHandlingType.IsEmpty || !JK_ReceivingForwarderHandlingType.IsEmpty;
		}

		public bool IsGatewayConsol => IsSendingOrReceivingForwarderGateway
					&& (IsAgent || IsCoLoad || IsAWBCoload || IsDirect);

		public bool CouldBeAttachedToMultiAWBMaster => IsAWBCoload || IsDirect;

		#endregion

		#region Is Specific Consol Mode

		public ZBool IsBuyersConsol
		{
			get { return JK_ConsolMode == Constants.ContainerModes.BuyersConsol; }
		}

		public ZBool IsFCL
		{
			get { return JK_ConsolMode == Constants.ContainerModes.FCL; }
		}

		public ZBool IsGroupage
		{
			get { return JK_ConsolMode == Constants.ContainerModes.Groupage; }
		}

		#endregion

		#region Domestic Freight

		public ZBool IsDomesticFreight
		{
			get
			{
				if (isDomesticFreight == null)
				{
					isDomesticFreight = this.IsDomestic();
				}

				return (ZBool)isDomesticFreight;
			}
			set
			{
				if (isDomesticFreight != value && !isSettingDomesticFreight)
				{
					isSettingDomesticFreight = true;
					try
					{
						SetNonPersistentPropertyValue(IsDomesticFreightInfo, ref isDomesticFreight, value);

						if (value)
						{
							if (JK_RL_NKLoadPort.IsEmpty)
							{
								JK_RL_NKLoadPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							}

							if (JK_RL_NKDischargePort.IsEmpty)
							{
								JK_RL_NKDischargePort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							}
						}

						if (!IsValidationSuspended)
						{
							Validation.ValidateIsDomesticFreight();
						}
					}
					finally
					{
						isSettingDomesticFreight = false;
					}
				}
			}
		}

		ZBool? isDomesticFreight;
		bool isSettingDomesticFreight;

		public ZPropertyInfo IsDomesticFreightInfo
		{
			get { return GetZPropertyInfo(nameof(IsDomesticFreight)); }
		}

		#endregion

		#region Schedule Properties

		#region JK_JX_JA_RL_NKPortOfLoading

		[List("RefUNLOCO_List")]
		[MaxLength(VoyageOrigin.Schema.JA_RL_NKPortOfLoadingMaxLength)]
		public ZString JK_JX_JA_RL_NKPortOfLoading
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZString.Empty : transport.JW_RL_NKLoadPort;
			}
		}

		public ZPropertyInfo JK_JX_JA_RL_NKPortOfLoadingInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JA_RL_NKPortOfLoading); }
		}

		#endregion

		#region JK_JX_JB_RL_NKPortOfDischarge

		[List("RefUNLOCO_List")]
		[MaxLength(VoyageDestination.Schema.JB_RL_NKPortOfDischargeMaxLength)]
		public ZString JK_JX_JB_RL_NKPortOfDischarge
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZString.Empty : transport.JW_RL_NKDiscPort;
			}
		}

		public ZPropertyInfo JK_JX_JB_RL_NKPortOfDischargeInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JB_RL_NKPortOfDischarge); }
		}

		#endregion

		#region JK_JX_JV_NKVessel
		[List("RefVesselList")]
		[MaxLength(JobVoyage.Schema.JV_RV_NKVesselMaxLength)]
		public virtual ZString JK_JX_JV_NKVessel
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZString.Empty : transport.JW_Vessel;
			}
		}

		public ZPropertyInfo JK_JX_JV_NKVesselInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JV_NKVessel); }
		}

		public ZString JK_VesselOfLastImportTransport
		{
			get
			{
				return LastImportTransport == null ? JK_JX_JV_NKVessel : LastImportTransport.JW_Vessel;
			}
		}

		public ZPropertyInfo JK_VesselOfLastImportTransportInfo
		{
			get { return GetZPropertyInfo(nameof(JK_VesselOfLastImportTransport)); }
		}

		#endregion

		#region JK_JX_JV_VoyageFlight

		[MaxLength(JobVoyage.Schema.JV_VoyageFlightMaxLength)]
		public virtual ZString JK_JX_JV_VoyageFlight
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZString.Empty : transport.JW_VoyageFlight;
			}
		}

		public ZPropertyInfo JK_JX_JV_VoyageFlightInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JV_VoyageFlight); }
		}

		public ZString JK_VoyageOfLastImportTransport
		{
			get
			{
				return LastImportTransport == null ? JK_JX_JV_VoyageFlight : LastImportTransport.JW_VoyageFlight;
			}
		}

		public ZPropertyInfo JK_VoyageOfLastImportTransportInfo
		{
			get { return GetZPropertyInfo(nameof(JK_VoyageOfLastImportTransport)); }
		}

		#endregion

		#region JK_RL_NKLoadForFirstImportTransport
		public ZString JK_RL_NKLoadForFirstImportTransport
		{
			get
			{
				Transport transport = Transports.FirstImportTransportByLoadAndDischarge();
				return transport == null ? JK_RL_NKLoadPort : transport.JW_RL_NKLoadPort;
			}
		}

		public ZPropertyInfo JK_RL_NKLoadForFirstImportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_RL_NKLoadForFirstImportTransport); }
		}
		#endregion

		#region JK_RL_NKDiscForFirstImportTransport
		public ZString JK_RL_NKDiscForFirstImportTransport
		{
			get
			{
				Transport transport = Transports.FirstImportTransportByLoadAndDischarge();
				if (transport == null)
				{
					return JK_RL_NKDischargePort;
				}
				else if (!transport.JW_RL_NKDiscPort.IsEmpty)
				{
					return transport.JW_RL_NKDiscPort;
				}
				else
				{
					var newTransport = Transports.ElementAfter(transport) as Transport;
					if (newTransport == null)
					{
						return JK_RL_NKDischargePort;
					}
					else
					{
						return newTransport.JW_RL_NKLoadPort;
					}
				}
			}
		}

		public ZPropertyInfo JK_RL_NKDiscForFirstImportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_RL_NKDiscForFirstImportTransport); }
		}
		#endregion

		#region FirstExportTransport

		protected Transport FirstExportTransport
		{
			get
			{
				return Transports.FirstTransportWithTransportModeAndExportVessel(JK_TransportMode);
			}
		}

		#endregion

		#region LastImportTransport

		public ZString JK_RL_NKDiscForLastImportTransport
		{
			get
			{
				return LastImportTransport == null ? JK_RL_NKDischargePort : LastImportTransport.JW_RL_NKDiscPort;
			}
		}

		public ZPropertyInfo JK_RL_NKDiscForLastImportTransportInfo
		{
			get { return GetZPropertyInfo(Schema.JK_RL_NKDiscForLastImportTransport); }
		}

		protected Transport LastImportTransport
		{
			get
			{
				return Transports.LastTransportWithTransportModeAndImportVessel(JK_TransportMode);
			}
		}

		#endregion

		#region JK_ArrivalForLastImportTransport

		public ZDateTime JK_ArrivalForLastImportTransport
		{
			get
			{
				if (LastImportTransport == null)
				{
					return ZDateTime.Empty;
				}

				return LastImportTransport.JW_ATA.IsEmpty ? LastImportTransport.JW_ETA : LastImportTransport.JW_ATA;
			}
		}

		#endregion

		#region JK_JX_JA_E_DEP

		public ZDateTime JK_JX_JA_E_DEP
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_ETD;
			}
		}

		public ZPropertyInfo JK_JX_JA_E_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JA_E_DEP); }
		}

		#endregion

		#region JK_JX_JA_E_FirstDEP

		public ZDateTime JK_JX_JA_E_FirstDEP
		{
			get
			{
				Transport transport = Transports.FirstTransportWithTransportMode(JK_TransportMode);
				return transport == null ? ZDateTime.Empty : transport.JW_ETD;
			}
		}

		public ZPropertyInfo JK_JX_JA_E_FirstDEPInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JA_E_FirstDEP); }
		}

		#endregion

		#region JK_JX_JB_E_LastARV

		public ZDateTime JK_JX_JB_E_LastARV
		{
			get
			{
				var lastTransport = Transports.LastTransportWithTransportMode(JK_TransportMode);
				return lastTransport == null ? ZDateTime.Empty : lastTransport.JW_ETA;
			}
		}

		public ZPropertyInfo JK_JX_JB_E_LastARVInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JB_E_LastARV); }
		}

		#endregion

		#region JK_JX_JB_E_ARV

		public ZDateTime JK_JX_JB_E_ARV
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_ETA;
			}
		}

		public ZPropertyInfo JK_JX_JB_E_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JB_E_ARV); }
		}

		#endregion

		#region JK_JX_JA_A_DEP

		public ZDateTime JK_JX_JA_A_DEP
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_ATD;
			}
		}

		public ZPropertyInfo JK_JX_JA_A_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JA_A_DEP); }
		}

		#endregion

		#region JK_JX_JA_A_FirstDEP

		public ZDateTime JK_JX_JA_A_FirstDEP
		{
			get
			{
				Transport transport = Transports.FirstTransportWithTransportMode(JK_TransportMode);
				return transport == null ? ZDateTime.Empty : transport.JW_ATD;
			}
		}

		public ZPropertyInfo JK_JX_JA_A_FirstDEPInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JA_A_FirstDEP); }
		}

		#endregion

		#region JK_JX_JB_A_ARV

		public ZDateTime JK_JX_JB_A_ARV
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_ATA;
			}
		}

		public ZPropertyInfo JK_JX_JB_A_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JK_JX_JB_A_ARV); }
		}

		#endregion

		#region JK_JX_JA_E_ARV

		public ZDateTime JK_JX_JA_E_ARV
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_JX_Load_ETA;
			}
		}

		#endregion

		#region JK_JX_JA_A_ARV

		public ZDateTime JK_JX_JA_A_ARV
		{
			get
			{
				Transport transport = Transports.MostInterestingTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_JX_Load_ATA;
			}
		}

		#endregion

		#region JK_JX_JV_AircraftType

		public ZString JK_JX_JV_AircraftType => Transports.MostInterestingTransport?.JW_AircraftType ?? ZString.Empty;

		public ZPropertyInfo JK_JX_JV_AircraftTypeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_JX_JV_AircraftType)); }
		}

		#endregion

		#region JK_CTOReceivalCommences

		public ZDateTime JK_CTOReceivalCommences
		{
			get
			{
				Transport transport = Transports.DepartureTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_TerminalReceivalCommences;
			}
		}

		public ZPropertyInfo JK_CTOReceivalCommencesInfo
		{
			get { return GetZPropertyInfo(Schema.JK_CTOReceivalCommences); }
		}

		#endregion

		#region JK_DepotReceivalCommences

		public ZDateTime JK_DepotReceivalCommences
		{
			get
			{
				Transport transport = Transports.DepartureTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_DepotReceivalCommences;
			}
		}

		public ZPropertyInfo JK_DepotReceivalCommencesInfo
		{
			get { return GetZPropertyInfo(Schema.JK_DepotReceivalCommences); }
		}

		#endregion

		#region JK_CTOCutOff

		public ZDateTime JK_CTOCutOff
		{
			get
			{
				Transport transport = Transports.DepartureTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_TerminalCutOff;
			}
		}

		public ZPropertyInfo JK_CTOCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JK_CTOCutOff); }
		}

		#endregion

		#region JK_DepotCutOff

		public ZDateTime JK_DepotCutOff
		{
			get
			{
				Transport transport = Transports.DepartureTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_DepotCutOff;
			}
		}

		public ZPropertyInfo JK_DepotCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JK_DepotCutOff); }
		}

		#endregion

		#region JK_DocsCutOff

		public ZDateTime JK_DocsCutOff
		{
			get
			{
				Transport transport = Transports.DepartureTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_DocumentaryCutOff;
			}
		}

		public ZPropertyInfo JK_DocsCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JK_DocsCutOff); }
		}

		#endregion

		#region JK_CTOAvailabilityDate

		public ZDateTime JK_CTOAvailabilityDate
		{
			get
			{
				Transport transport = Transports.ArrivalTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_TerminalAvailabilityDate;
			}
		}

		public ZPropertyInfo JK_CTOAvailabilityDateInfo
		{
			get { return GetZPropertyInfo(Schema.JK_CTOAvailabilityDate); }
		}

		#endregion

		#region JK_DepotAvailabilityDate

		public ZDateTime JK_DepotAvailabilityDate
		{
			get
			{
				Transport transport = Transports.ArrivalTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_DepotAvailabilityDate;
			}
		}

		public ZPropertyInfo JK_DepotAvailabilityDateInfo
		{
			get { return GetZPropertyInfo(Schema.JK_DepotAvailabilityDate); }
		}

		#endregion

		#region JK_CTOStorageDate

		public ZDateTime JK_CTOStorageDate
		{
			get
			{
				Transport transport = Transports.ArrivalTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_TerminalStorageDate;
			}
		}

		public ZPropertyInfo JK_CTOStorageDateInfo
		{
			get { return GetZPropertyInfo(Schema.JK_CTOStorageDate); }
		}

		#endregion

		#region JK_DepotStorageDate

		public ZDateTime JK_DepotStorageDate
		{
			get
			{
				Transport transport = Transports.ArrivalTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_DepotStorageDate;
			}
		}

		public ZPropertyInfo JK_DepotStorageDateInfo
		{
			get { return GetZPropertyInfo(Schema.JK_DepotStorageDate); }
		}

		#endregion

		#region JK_VGMCutOff

		public ZDateTime JK_VGMCutOff => Transports.DepartureTransport?.JW_VGMCutOff ?? ZDateTime.Empty;

		public ZPropertyInfo JK_VGMCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JK_VGMCutOff); }
		}

		#endregion

		#endregion

		#region Properties for Module Grid

		public ZString JK_Calc_SendingAgentCode
		{
			get { return SendingForwarder != null ? SendingForwarder.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo JK_Calc_SendingAgentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_SendingAgentCode)); }
		}

		public ZString JK_Calc_ReceivingAgentCode
		{
			get { return ReceivingForwarder != null ? ReceivingForwarder.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo JK_Calc_ReceivingAgentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_ReceivingAgentCode)); }
		}

		#endregion

		#region JK_Calc_Container*

		public ZDecimal JK_Calc_TEUCount
		{
			get { return Containers.Cast<CommonContainer>().Sum(container => container.JC_Calc_TEUCount); }
		}

		public ZInt JK_Calc_ContainerCount
		{
			get { return GetSumOfCalc_ContainerCount(container => true); }
		}

		public ZPropertyInfo JK_Calc_ContainerCountInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_ContainerCount)); }
		}

		public ZInt JK_Calc_20GPCount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_Is20GP); }
		}

		public ZInt JK_Calc_40GPCount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_Is40GP); }
		}

		public ZInt JK_Calc_20RECount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_Is20RE); }
		}

		public ZInt JK_Calc_40RECount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_Is40RE); }
		}

		public ZInt JK_Calc_OtherContainerCount
		{
			get { return GetSumOfCalc_ContainerCount(container => container.JC_IsOtherContainerType); }
		}

		ZInt GetSumOfCalc_ContainerCount(Predicate<CommonContainer> predicate)
		{
			return Containers.Cast<CommonContainer>().Where(container => predicate(container)).Sum(container => container.JC_Calc_ContainerCount);
		}

		#endregion

		#region JK_ReleaseType_List

		public CodeDescriptionPairList JK_ReleaseType_List
		{
			get { return FreightDataRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		#region JK_CarrierBookingStatus_List

		public CodeDescriptionPairList JK_CarrierBookingStatus_List
		{
			get { return Factory.GetCachedValue("FreightCodePairLists.CarrierBookingStatusList", FreightCodePairLists.CarrierBookingStatusList); }
		}

		#endregion

		#region JK_PackageGrouping_List

		public CodeDescriptionPairList JK_PackageGrouping_List
		{
			get { return Factory.GetCachedValue("FreightCodePairLists.PackageGroupingList", FreightCodePairLists.PackageGroupingList); }
		}

		#endregion

		#region ArrivalUnpackCFSTransport
		[RelatedBusinessObject("ArrivalUnpackCFSTransport")]
		[List("Lookups.ArrivalUnpackCFSTransports")]
		public ZGuid ArrivalUnpackCFSTransportPK
		{
			get { return this.ArrivalUnpackCFSTransport != null ? this.ArrivalUnpackCFSTransport.PK : ZGuid.Empty; }
			set
			{
				var org = Factory.Load<OrgHeader>(value);
				JK_OA_ArrivalUnpackCFSTransportAddress = org != null ? org.MainAddress.PK : ZGuid.Empty;
				ArrivalUnpackCFSTransportPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ArrivalUnpackCFSTransportPKInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalUnpackCFSTransportPK); }
		}

		public override OrgHeader ArrivalUnpackCFSTransport
		{
			get { return ArrivalUnpackCFSTransportAddress != null ? Factory.Load<OrgHeader>(ArrivalUnpackCFSTransportAddress.OA_OH) : null; }
		}

		protected override ZAddress GetNewJK_OA_ArrivalUnpackCFSTransportAddress_ZAddress()
		{
			ZAddress result = base.GetNewJK_OA_ArrivalUnpackCFSTransportAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		#endregion

		#region DeparturePackCFSTransport
		[RelatedBusinessObject("DeparturePackCFSTransport")]
		[List("Lookups.DeparturePackCFSTransports")]
		public ZGuid DeparturePackCFSTransportPK
		{
			get { return this.DeparturePackCFSTransport != null ? this.DeparturePackCFSTransport.PK : ZGuid.Empty; }
			set
			{
				var org = Factory.Load<OrgHeader>(value);
				JK_OA_DeparturePackCFSTransportAddress = org != null ? org.MainAddress.PK : ZGuid.Empty;
				DeparturePackCFSTransportPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeparturePackCFSTransportPKInfo
		{
			get { return GetZPropertyInfo(Schema.DeparturePackCFSTransportPK); }
		}

		public override OrgHeader DeparturePackCFSTransport
		{
			get { return DeparturePackCFSTransportAddress != null ? Factory.Load<OrgHeader>(DeparturePackCFSTransportAddress.OA_OH) : null; }
		}

		protected override ZAddress GetNewJK_OA_DeparturePackCFSTransportAddress_ZAddress()
		{
			ZAddress result = base.GetNewJK_OA_DeparturePackCFSTransportAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		#endregion

		#region JK_OA_ShippingLineAddress

		[List("ShippingProviderList")]
		public ZGuid ShippingLinePK
		{
			get { return ShippingLine != null ? ShippingLine.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo ShippingLinePKInfo
		{
			get { return GetZPropertyInfo(Schema.ShippingLinePK); }
		}

		public OrgHeader ShippingLine
		{
			get { return ShippingLineAddress != null ? Factory.Load<OrgHeader>(ShippingLineAddress.OA_OH) : null; }
		}

		public void SetDefaultShippingLineAddress(ZGuid shippingLinePK)
		{
			SetDefaultShippingLineAddress(Factory.Load<OrgHeader>(shippingLinePK));
		}

		public void SetDefaultShippingLineAddress(OrgHeader shippingLine)
		{
			if (shippingLine == null)
			{
				JK_OA_ShippingLineAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
				JK_OA_ShippingLineAddress = ZGuid.Empty;
			}
			else
			{
				JK_OA_ShippingLineAddress = GetDefaultConsolShippingLineAddressPK(shippingLine);
			}
		}

		public void SetDefaultCreditor(OrgHeader creditor)
		{
			if (creditor != null)
			{
				JK_OA_CreditorAddress = GetDefaultConsolShippingLineAddressPK(creditor);
			}
		}

		public void SetDefaultShippingLineAddress(OrgAddress shippingLineAddress)
		{
			if (shippingLineAddress == null)
			{
				JK_OA_ShippingLineAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
				JK_OA_ShippingLineAddress = ZGuid.Empty;
			}
			else
			{
				JK_OA_ShippingLineAddress = shippingLineAddress.PK;
			}
		}

		ZGuid GetDefaultConsolShippingLineAddressPK(OrgHeader header)
		{
			return header != null ? GetDefaultAddressPKBasedOnDirectionAndPorts(header) : ZGuid.Empty;
		}

		ZGuid GetDefaultShippingLineAddress(ZGuid shippingLinePK)
		{
			return GetDefaultConsolShippingLineAddressPK(Factory.Load<OrgHeader>(shippingLinePK));
		}

		protected override ZAddress GetNewJK_OA_ShippingLineAddress_ZAddress()
		{
			ZAddress result = base.GetNewJK_OA_ShippingLineAddress_ZAddress();
			result.DefaultAddressType = AddressType.NoDefault;
			result.GetDefaultAddress = header => GetDefaultConsolShippingLineAddressPK(header as OrgHeader);

			return result;
		}

		public override ZGuid JK_OA_ShippingLineAddress
		{
			get { return base.JK_OA_ShippingLineAddress; }
			set
			{
				if (value != base.JK_OA_ShippingLineAddress)
				{
					base.JK_OA_ShippingLineAddress = value;

					DefaultDepartureCTOAddressFromCarrier();
					DefaultArrivalCTOAddressFromCarrier();
					DefaultPickupContainerYardFromCarrier();
					DefaultReturnContainerYardFromCarrier();

					DefaultCarrierOnLinkedSeaTransport();
					SetDefaultCreditorFromCarrier(JK_OA_ShippingLineAddress);

					Transports.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public override ZGuid JK_OA_CreditorAddress
		{
			get { return base.JK_OA_CreditorAddress; }
			set
			{
				if (value != base.JK_OA_CreditorAddress)
				{
					base.JK_OA_CreditorAddress = value;
					Transports.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		protected void SetDefaultCreditorFromCarrier(ZGuid orgAddressId)
		{
			if (!IsCoLoad)
			{
				var newCreditor = GetDefaultCreditorFromCarrier(orgAddressId);

				if (!newCreditor.IsEmpty)
				{
					this.JK_OA_CreditorAddress = newCreditor;
				}
			}
		}

		protected ZGuid GetDefaultCreditorFromCarrier(ZGuid orgAddressId)
		{
			var isApplicableTransportation = this.IsExport() || this.IsImport() || this.IsDomestic() || this.IsCrossTrade();

			if (orgAddressId.IsValid && isApplicableTransportation)
			{
				var transport = Transports.MostInterestingTransport;

				var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter()
				{
					OrgAddress = orgAddressId,
					OrgPartyAddress = IsCoLoad ? JK_OA_CreditorAddress : JK_OA_ShippingLineAddress,
					TransportMode = JK_TransportMode,
					ContainerMode = JK_ConsolMode,
					PaymentType = JK_PrepaidCollect
				};

				if (this.IsExport())
				{
					orgRelatedPartyFilter.CreditorType = DefaultCreditorHelper.CreditorType.ExportConsol;
					orgRelatedPartyFilter.UNLOCO = transport?.JW_RL_NKLoadPort ?? ZString.Empty;
				}
				else if (this.IsImport())
				{
					orgRelatedPartyFilter.CreditorType = DefaultCreditorHelper.CreditorType.ImportConsol;
					orgRelatedPartyFilter.UNLOCO = transport?.JW_RL_NKDiscPort ?? ZString.Empty;
				}
				else if (this.IsDomestic())
				{
					orgRelatedPartyFilter.CreditorType = DefaultCreditorHelper.CreditorType.DomesticConsol;
					orgRelatedPartyFilter.UNLOCO = transport?.JW_RL_NKLoadPort ?? ZString.Empty;
				}
				else if (this.IsCrossTrade())
				{
					orgRelatedPartyFilter.CreditorType = DefaultCreditorHelper.CreditorType.CrossTradeConsol;
					orgRelatedPartyFilter.UNLOCO = transport?.JW_RL_NKDiscPort ?? ZString.Empty;
				}

				return DefaultCreditorHelper.GetCreditorAddress(orgRelatedPartyFilter, Factory);
			}

			return ZGuid.Empty;
		}

		void DefaultDepartureCTOAddressFromCarrier()
		{
			if (!IsDepartureCTOAddressDefaultedFromVoyage)
			{
				if (ShippingLine == null)
				{
					if (IsDepartureCTOAddressDefaultedFromCarrier)
					{
						JK_OA_DepartureCTOAddress = ZGuid.Empty;
						IsDepartureCTOAddressDefaultedFromCarrier = false;
					}
				}
				else
				{
					IsDepartureCTOAddressDefaultedFromCarrier = false;
					if (!JK_OA_DepartureCTOAddress.IsValid)
					{
						CTOFromCarrierDefaulter.SetCTOFromCarrier(this, JK_RL_NKLoadPort, OrgConstants.CarrierAgentDirections.Code.Departure, JK_OA_DepartureCTOAddressInfo);
						IsDepartureCTOAddressDefaultedFromCarrier = true;
					}
				}
			}
		}

		bool IsDepartureCTOAddressDefaultedFromCarrier;

		void DefaultArrivalCTOAddressFromCarrier()
		{
			if (!IsArrivalCTOAddressDefaultedFromVoyage)
			{
				if (ShippingLine == null)
				{
					if (IsArrivalCTOAddressDefaultedFromCarrier)
					{
						JK_OA_ArrivalCTOAddress = ZGuid.Empty;
						IsArrivalCTOAddressDefaultedFromCarrier = false;
					}
				}
				else
				{
					IsArrivalCTOAddressDefaultedFromCarrier = false;
					if (!JK_OA_ArrivalCTOAddress.IsValid)
					{
						CTOFromCarrierDefaulter.SetCTOFromCarrier(this, JK_RL_NKDischargePort, OrgConstants.CarrierAgentDirections.Code.Arrival, JK_OA_ArrivalCTOAddressInfo);
						IsArrivalCTOAddressDefaultedFromCarrier = true;
					}
				}
			}
		}

		bool IsArrivalCTOAddressDefaultedFromCarrier;

		void DefaultReturnContainerYardFromCarrier()
		{
			if (ShippingLine == null)
			{
				if (isReturnContainerDefaultedFromCarrier)
				{
					JK_OA_ContainerYardEmptyReturnAddress = ZGuid.Empty;
					isReturnContainerDefaultedFromCarrier = false;
				}
			}
			else
			{
				isReturnContainerDefaultedFromCarrier = false;
				if (!JK_OA_ContainerYardEmptyReturnAddress.IsValid)
				{
					var containerReturnAddress = ShippingLine.CarrierAppointedAgentPorts_ContainerYardPark.FindAddress(JK_RL_NKDischargePort, ZString.Empty, OrgConstants.CarrierAgentDirections.Code.Arrival);
					if (containerReturnAddress != null)
					{
						JK_OA_ContainerYardEmptyReturnAddress = containerReturnAddress.PK;
						isReturnContainerDefaultedFromCarrier = true;
					}
				}
			}
		}

		bool isReturnContainerDefaultedFromCarrier;

		void DefaultPickupContainerYardFromCarrier()
		{
			if (ShippingLine == null)
			{
				if (isPickupContainerDefaultedFromCarrier)
				{
					JK_OA_ContainerYardEmptyPickupAddress = ZGuid.Empty;
					isPickupContainerDefaultedFromCarrier = false;
				}
			}
			else
			{
				isPickupContainerDefaultedFromCarrier = false;
				if (!JK_OA_ContainerYardEmptyPickupAddress.IsValid)
				{
					var containerPickupAddress = ShippingLine.CarrierAppointedAgentPorts_ContainerYardPark.FindAddress(JK_RL_NKLoadPort, ZString.Empty, OrgConstants.CarrierAgentDirections.Code.Departure);
					if (containerPickupAddress != null)
					{
						JK_OA_ContainerYardEmptyPickupAddress = containerPickupAddress.PK;
						isPickupContainerDefaultedFromCarrier = true;
					}
				}
			}
		}

		bool isPickupContainerDefaultedFromCarrier;

		void DefaultCarrierOnLinkedSeaTransport()
		{
			if (!ShippingLinePK.IsEmpty
				&& Transports.Count == 1
				&& Transports[0].IsSea
				&& Transports[0].JW_IsLinked
				&& Transports[0].CarrierPK.IsEmpty)
			{
				Transports[0].CarrierPK = ShippingLinePK;
			}
		}

		#endregion

		[List("JK_OA_List")]
		public override ZGuid JK_OA_DepartureCTOAddress
		{
			get { return base.JK_OA_DepartureCTOAddress; }
			set { base.JK_OA_DepartureCTOAddress = value; }
		}

		[List("JK_OA_List")]
		public override ZGuid JK_OA_ArrivalCTOAddress
		{
			get { return base.JK_OA_ArrivalCTOAddress; }
			set { base.JK_OA_ArrivalCTOAddress = value; }
		}

		#region JK_TransportMode

		[List("JK_TransportMode_List")]
		public override ZString JK_TransportMode
		{
			get { return base.JK_TransportMode; }
			set
			{
				if (value != JK_TransportMode)
				{
					ResetListsOnTransportModeChanged();

					bool wasAir = IsAir;

					base.JK_TransportMode = value;

					if (!fSettingDefaultValues)
					{
						((IDefaultNumberOfDecimalsSupporter)this).RoundMeasurePropertiesOnTransportModeChanged();
					}

					if (!IsCopying && !IsImportingData)
					{
						SetConsolTypeBasedOnTransportMode();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_ConsolMode();
					}

					Containers.MarkAsNeedingValidation();

					if (!IsCopying)
					{
						if (Transports.Count == 1)
						{
							Transport transport = Transports[0];

							IDisposable hasChangesSuspender = IsSettingHasChangesSuspended ? transport.SuspendSettingHasChanges() : null;
							try
							{
								transport.JW_TransportMode = value;
							}
							finally
							{
								if (hasChangesSuspender != null)
								{
									hasChangesSuspender.Dispose();
								}
							}
						}

						if (!JK_TransportMode.IsEmpty)
						{
							DefaultSendingForwarder();
							DefaultReceivingForwarder();
							DefaultCoLoadWithAddress();
						}

						if (wasAir != IsAir)
						{
							if (!IsImportingData && !JK_MasterBillNum.IsEmpty)
							{
								JK_MasterBillNum = "";
							}
						}

						JK_RL_NKLastForeignPort = "";
						JK_DateLastForeignPort = ZDateTime.Empty;

						ResetContainerTrainWagonNumberForNonRail();
					}
				}
			}
		}

		protected void SetConsolTypeBasedOnTransportMode()
		{
			if (IsCourier)
			{
				JK_ConsolMode = Constants.ContainerModes.Other;
			}
			else
			{
				switch (JK_TransportMode)
				{
					case Constants.TransportModes.Air:
						if (JK_AgentType != Constants.AgentType.Other)
						{
							JK_ConsolMode = Constants.ContainerModes.Loose;
						}

						break;

					case Constants.TransportModes.Sea:
						if (JK_AgentType != Constants.AgentType.Other)
						{
							JK_ConsolMode = Constants.ContainerModes.FCL;
						}

						break;

					case Constants.TransportModes.Other:
						JK_ConsolMode = Constants.ContainerModes.Other;
						break;

					default:
						JK_ConsolMode = "";
						break;
				}
			}
		}

		#endregion

		#region JK_AgentType

		[List("JK_AgentType_List")]
		public override ZString JK_AgentType
		{
			get { return base.JK_AgentType; }
			set
			{
				if (value != JK_AgentType)
				{
					base.JK_AgentType = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_MasterBillNum();
						Validation.ValidateJK_ConsolMode();
					}

					if (IsCourier)
					{
						SetForCourier();
					}

					DefaultCoLoadWithAddress();
				}
			}
		}

		void SetForCourier()
		{
			JK_ConsolMode = Constants.ContainerModes.Other;

			if (!IsInDatabase && Transports.Count == 1)
			{
				Transports[0].JW_IsLinked = false;
			}
		}

		protected bool fIsUpdatingFromShipment;

		#endregion

		#region JK_PrepaidCollect

		[List("JK_PrepaidCollect_List")]
		public override ZString JK_PrepaidCollect
		{
			get { return base.JK_PrepaidCollect; }
			set
			{
				if (base.JK_PrepaidCollect != value)
				{
					if (JK_PrepaidCollect_InitValue == null)
					{
						JK_PrepaidCollect_InitValue = base.JK_PrepaidCollect;
					}

					base.JK_PrepaidCollect = value;
					SetDefaultCreditorFromCarrier(JK_OA_ShippingLineAddress);

					if (!IsValidationSuspended && IsGatewayServiceLevelValidationNecessary(JK_PrepaidCollect_InitValue, JK_PrepaidCollect))
					{
						Validation.ValidateJK_RS_NKGatewayServiceLevel();
					}
				}
			}
		}

		string JK_PrepaidCollect_InitValue;

		#endregion

		#region JK_RS_NKGatewayServiceLevel

		protected bool IsGatewayServiceLevelValidationNecessary(string initValue, ZString newValue) =>
			RequestPermissionByImpersonation == null || // means, it's not been called from GUI
			initValue != null && !newValue.IsEmpty && initValue != newValue;

		protected bool IsGatewayServiceLevelValidationNecessary(ZGuid? initValue, ZGuid newValue) =>
			RequestPermissionByImpersonation == null || // means, it's not been called from GUI
			initValue.HasValue && !newValue.IsEmpty && initValue != newValue;

		public RequestPermissionByImpersonation RequestPermissionByImpersonation { get; set; }

		public ExclusiveGatewayServiceChecker ExclusiveGatewayServiceChecker
			=> exclusiveGatewayServiceChecker ?? (exclusiveGatewayServiceChecker = new ExclusiveGatewayServiceChecker());
		ExclusiveGatewayServiceChecker exclusiveGatewayServiceChecker;

		#endregion

		#region Sending / Receiving Forwarder Addresses

		#region JK_OA_SendingForwarderAddress

		[List("SendingForwarderList")]
		public ZGuid SendingForwarderPK
		{
			get { return SendingForwarder != null ? SendingForwarder.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo SendingForwarderPKInfo
		{
			get { return GetZPropertyInfo(Schema.SendingForwarderPK); }
		}

		public OrgHeader SendingForwarder
		{
			get
			{
				var sendingForwarderAddress = SendingForwarderAddress;
				var sendingOrgHeader = sendingForwarderAddress != null ? Factory.Load<OrgHeader>(sendingForwarderAddress.OA_OH) : null;

				if (sendingForwarderSubscriptionManager == null)
				{
					SendingForwarderSubscriptionManager.BusinessObject = sendingOrgHeader;
				}

				return sendingOrgHeader;
			}
		}

		public void SetDefaultSendingForwarderAddress()
		{
			JK_OA_SendingForwarderAddress = GetDefaultSendingForwarderAddress();
		}

		public void SetDefaultSendingForwarderAddress(ZGuid orgHeaderToFallback)
		{
			SetDefaultSendingForwarderAddress(Factory.Load<OrgHeader>(orgHeaderToFallback));
		}

		public void SetDefaultSendingForwarderAddress(OrgHeader orgHeaderToFallback)
		{
			JK_OA_SendingForwarderAddress = GetDefaultSendingForwarderAddress(orgHeaderToFallback);
		}

		public virtual void SetDefaultSendingForwarderAddressWithFallbackToProxy(OrgHeader proxy)
		{
			ZGuid address = GetDefaultSendingForwarderAddress();
			JK_OA_SendingForwarderAddress = !address.IsEmpty ? address : GetDefaultSendingForwarderAddress(proxy);
		}

		ZGuid GetDefaultSendingForwarderAddress()
		{
			OrgAddress agentAddress = GetAgentAddress(LoadPort, AgentDirectionList.Codes.Export);
			return agentAddress != null ? agentAddress.PK : ZGuid.Empty;
		}

		protected ZGuid GetDefaultSendingForwarderAddress(OrgHeader orgHeaderToFallback, bool includeHandlesStatus = false)
		{
			return GetAgentAddressWithFallbackToMainAddress(orgHeaderToFallback, LoadPort, AgentDirectionList.Codes.Export, includeHandlesStatus);
		}

		protected override ZAddress GetNewJK_OA_SendingForwarderAddress_ZAddress()
		{
			ZAddress result = base.GetNewJK_OA_SendingForwarderAddress_ZAddress();
			result.GetDefaultAddress = x => GetDefaultSendingForwarderAddress(x as OrgHeader);
			return result;
		}

		[List("SendingForwarderList")]
		[UniversalCopyListOverride("Lookups.SendingForwarderAddresses")]
		public override ZGuid JK_OA_SendingForwarderAddress
		{
			get { return base.JK_OA_SendingForwarderAddress; }
			set
			{
				var previousSendingForwarderAddress = JK_OA_SendingForwarderAddress;
				if (value != JK_OA_SendingForwarderAddress)
				{
					base.JK_OA_SendingForwarderAddress = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_OA_ReceivingForwarderAddress();
					}
				}

				if (!IsCopying)
				{
					SetDepartureDepot();

					if (!IsImportingData)
					{
						if (SendingForwarderPK.IsValid)
						{
							SetConsolCreditorFromForwardingAgents(SendingForwarderPK, previousSendingForwarderAddress);
						}

						DefaultCoLoadWithAddress();
						SetDepartureTransport();
						SetDepartureCFSContainerClient();
					}
				}

				OnSendingForwarderChanged();
				SendingForwarderSubscriptionManager.BusinessObject = SendingForwarder;
			}
		}

		/// <summary>
		/// Set Departure Depot Address to Default Air/Sea Depot from SendingFowarder OrgMiscServ (Forwarder tab)
		/// </summary>
		protected virtual void SetDepartureDepot()
		{
			if (SendingForwarder != null && !JK_RL_NKLoadPort.IsEmpty)
			{
				OrgHeader defaultDepot = SendingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, JK_TransportMode, JK_ConsolMode, JK_RL_NKLoadPort);
				if (defaultDepot != null)
				{
					JK_OA_PackDepotAddress = defaultDepot.GetAddressWithFallback(JK_OA_PackDepotAddress_ZAddress.DefaultAddressType)?.PK ?? ZGuid.Empty;
				}
			}
		}

		protected virtual void SetDepartureTransport()
		{
			if (SendingForwarder != null)
			{
				OrgHeader defaultLocalTransport = SendingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, JK_TransportMode, JK_ConsolMode);
				if (defaultLocalTransport != null)
				{
					JK_OA_DeparturePackCFSTransportAddress = defaultLocalTransport.MainAddress.PK;
				}
			}
		}

		protected virtual void SetCoLoadWith()
		{
			if (SendingForwarder != null)
			{
				var defaultCoLoadWith = SendingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCoLoadWith, RelatedPartyDirectionList.Codes.Pickup, JK_TransportMode, JK_ConsolMode);
				if (defaultCoLoadWith != null)
				{
					JK_OA_CreditorAddress = defaultCoLoadWith.MainAddress.PK;
				}
			}
		}

		protected virtual void SetDepartureCFSContainerClient()
		{
			if (SendingForwarder != null && JK_IsCFS && (this.IsExport() || HasDepartureTransportLoadingAtBranchLoadPort()))
			{
				foreach (CommonContainer container in Containers)
				{
					container.JC_OH_CFSClient = SendingForwarder.PK;
				}
			}
		}

		DataRefreshSubscriptionManager SendingForwarderSubscriptionManager
		{
			get
			{
				if (sendingForwarderSubscriptionManager == null)
				{
					sendingForwarderSubscriptionManager = new DataRefreshSubscriptionManager((o, args) => OnSendingForwarderChanged());
					sendingForwarderSubscriptionManager.Enabled = true;
				}
				return sendingForwarderSubscriptionManager;
			}
		}
		DataRefreshSubscriptionManager sendingForwarderSubscriptionManager;

		void OnSendingForwarderChanged()
		{
			var sendingForwarderChanged = SendingForwarderChanged;

			if (sendingForwarderChanged != null)
			{
				sendingForwarderChanged(this, new EventArgs());
			}
		}

		public event EventHandler SendingForwarderChanged;

		#endregion

		#region JK_OA_ReceivingForwarderAddress

		[List("ReceivingForwarderList")]
		public ZGuid ReceivingForwarderPK
		{
			get { return ReceivingForwarder != null ? ReceivingForwarder.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo ReceivingForwarderPKInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivingForwarderPK); }
		}

		public OrgHeader ReceivingForwarder
		{
			get
			{
				var receivingForwarderAddress = ReceivingForwarderAddress;
				var receivingOrgHeader = receivingForwarderAddress != null ? Factory.Load<OrgHeader>(receivingForwarderAddress.OA_OH) : null;

				if (receivingForwarderSubscriptionManager == null)
				{
					ReceivingForwarderSubscriptionManager.BusinessObject = receivingOrgHeader;
				}

				return receivingOrgHeader;
			}
		}

		public void SetDefaultReceivingForwarderAddress()
		{
			JK_OA_ReceivingForwarderAddress = GetDefaultReceivingForwarderAddress();
		}

		public void SetDefaultReceivingForwarderAddress(ZGuid orgHeaderToFallback)
		{
			SetDefaultReceivingForwarderAddress(Factory.Load<OrgHeader>(orgHeaderToFallback));
		}

		public void SetDefaultReceivingForwarderAddress(OrgHeader orgHeaderToFallback)
		{
			JK_OA_ReceivingForwarderAddress = GetDefaultReceivingForwarderAddress(orgHeaderToFallback);
		}

		public virtual void SetDefaultReceivingForwarderAddressWithFallbackToProxy(OrgHeader proxy)
		{
			ZGuid address = GetDefaultReceivingForwarderAddress();
			JK_OA_ReceivingForwarderAddress = !address.IsEmpty ? address : GetDefaultReceivingForwarderAddress(proxy);
		}

		ZGuid GetDefaultReceivingForwarderAddress()
		{
			OrgAddress agentAddress = GetAgentAddress(DischargePort, AgentDirectionList.Codes.Import);
			return agentAddress != null ? agentAddress.PK : ZGuid.Empty;
		}

		protected ZGuid GetDefaultReceivingForwarderAddress(OrgHeader orgHeaderToFallback, bool includeHandlesStatus = false)
		{
			return GetAgentAddressWithFallbackToMainAddress(orgHeaderToFallback, DischargePort, AgentDirectionList.Codes.Import, includeHandlesStatus);
		}

		protected override ZAddress GetNewJK_OA_ReceivingForwarderAddress_ZAddress()
		{
			ZAddress result = base.GetNewJK_OA_ReceivingForwarderAddress_ZAddress();
			result.GetDefaultAddress = x => GetDefaultReceivingForwarderAddress(x as OrgHeader);
			return result;
		}

		[List("ReceivingForwarderList")]
		[UniversalCopyListOverride("Lookups.ReceivingForwarderAddresses")]
		public override ZGuid JK_OA_ReceivingForwarderAddress
		{
			get { return base.JK_OA_ReceivingForwarderAddress; }
			set
			{
				var previousReceivingForwarderAddress = base.JK_OA_ReceivingForwarderAddress;

				if (base.JK_OA_ReceivingForwarderAddress != value)
				{
					base.JK_OA_ReceivingForwarderAddress = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_OA_SendingForwarderAddress();
					}
				}

				if (!IsCopying)
				{
					if (!IsImportingData)
					{
						SetArrivalCFSContainerClient();
					}

					SetArrivalDepot();

					if (!IsImportingData)
					{
						SetArrivalLocalTransport();

						if (ReceivingForwarderPK.IsValid)
						{
							SetConsolCreditorFromForwardingAgents(ReceivingForwarderPK, previousReceivingForwarderAddress);
						}
					}
				}

				OnReceivingForwarderChanged();
				ReceivingForwarderSubscriptionManager.BusinessObject = ReceivingForwarder;
			}
		}

		/// <summary>
		/// Set Arrival Depot Address to Default Air/Sea Depot from ReceivingFowarder OrgMiscServ (Forwarder tab)
		/// </summary>
		protected virtual void SetArrivalDepot()
		{
			if (ReceivingForwarder != null && !JK_RL_NKDischargePort.IsEmpty)
			{
				OrgHeader defaultDepot = ReceivingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, JK_TransportMode, JK_ConsolMode, JK_RL_NKDischargePort);
				if (defaultDepot != null)
				{
					JK_OA_UnpackDepotAddress = defaultDepot.GetAddressWithFallback(JK_OA_UnpackDepotAddress_ZAddress.DefaultAddressType)?.PK ?? ZGuid.Empty;
				}
			}
		}

		protected virtual void SetArrivalLocalTransport()
		{
			if (ReceivingForwarder != null)
			{
				OrgHeader defaultLocalTransport = ReceivingForwarder.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, JK_TransportMode, JK_ConsolMode);
				if (defaultLocalTransport != null)
				{
					JK_OA_ArrivalUnpackCFSTransportAddress = defaultLocalTransport.MainAddress.PK;
				}
			}
		}

		protected virtual void SetArrivalCFSContainerClient()
		{
			if (ReceivingForwarder != null && JK_IsCFS && !this.IsExport() && !HasDepartureTransportLoadingAtBranchLoadPort())
			{
				foreach (CommonContainer container in Containers)
				{
					container.JC_OH_CFSClient = ReceivingForwarder.PK;
				}
			}
		}

		bool HasDepartureTransportLoadingAtBranchLoadPort()
		{
			return Transports.DepartureTransport != null && Transports.DepartureTransport.JW_RL_NKLoadPort == GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		}

		DataRefreshSubscriptionManager ReceivingForwarderSubscriptionManager
		{
			get
			{
				if (receivingForwarderSubscriptionManager == null)
				{
					receivingForwarderSubscriptionManager = new DataRefreshSubscriptionManager((o, args) => OnReceivingForwarderChanged());
					receivingForwarderSubscriptionManager.Enabled = true;
				}
				return receivingForwarderSubscriptionManager;
			}
		}
		DataRefreshSubscriptionManager receivingForwarderSubscriptionManager;

		void OnReceivingForwarderChanged()
		{
			var receivingForwarderChanged = ReceivingForwarderChanged;

			if (receivingForwarderChanged != null)
			{
				receivingForwarderChanged(this, new EventArgs());
			}
		}

		public event EventHandler ReceivingForwarderChanged;

		#endregion

		ZGuid GetAgentAddressWithFallbackToMainAddress(OrgHeader headerToFallback, RefUNLOCO port, ZString agentDirection, bool includeHandlesStatus)
		{
			if (headerToFallback != null)
			{
				OrgAddress agentAddress = GetAgentAddress(headerToFallback, port, agentDirection, includeHandlesStatus);
				if (agentAddress != null)
				{
					return agentAddress.PK;
				}

				return headerToFallback.MainAddress.PK;
			}

			return ZGuid.Empty;
		}

		OrgAddress GetAgentAddress(RefUNLOCO port, ZString agentDirection, bool includeHandlesStatus = false)
		{
			return GetAgentAddress(null, port, agentDirection, includeHandlesStatus);
		}

		OrgAddress GetAgentAddress(OrgHeader header, RefUNLOCO port, ZString agentDirection, bool includeHandlesStatus)
		{
			if (port == null)
			{
				return null;
			}

			switch (JK_TransportMode)
			{
				case Constants.TransportModes.Air:
				case Constants.TransportModes.Rail:
				case Constants.TransportModes.Road:
				case Constants.TransportModes.Sea:
					{
						OrgAddress bestAddress = port.GetBestAgent(header, JK_TransportMode, agentDirection, includeHandlesStatus);
						return bestAddress;
					}
			}

			return null;
		}

		#endregion

		#region JK_OA_CreditorAddress

		[RelatedBusinessObject("Creditor")]
		[List("CreditorList")]
		public ZGuid CreditorPK
		{
			get { return JK_OA_CreditorAddress_ZAddress.OrgPK; }
			set
			{
				JK_OA_CreditorAddress_ZAddress.OrgPK = value;
				CreditorPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CreditorPKInfo
		{
			get { return GetZPropertyInfo(Schema.CreditorPK); }
		}

		public override OrgHeader Creditor
		{
			get { return CreditorAddress != null ? CreditorAddress.Header : null; }
		}

		protected override ZAddress GetNewJK_OA_CreditorAddress_ZAddress()
		{
			ZAddress result = base.GetNewJK_OA_CreditorAddress_ZAddress();
			result.DefaultAddressType = AddressType.NoDefault;
			result.GetDefaultAddress = GetDefaultAddressForCreditor;
			return result;
		}

		ZGuid GetDefaultAddressForCreditor(IOrgHeader org)
		{
			OrgAddress result = null;
			OrgHeader header = org as OrgHeader;
			if (header != null)
			{
				if (IsCoLoad)
				{
					return GetDefaultAddressPKBasedOnDirectionAndPorts(header);
				}
				else
				{
					result = header.Addresses.DefaultAddressOfType(OrgAddressType.Payables)
						?? header.Addresses.DefaultAddressOfType(OrgAddressType.Postal)
						?? header.Addresses.DefaultAddressOfType(OrgAddressType.Office)
						?? header.MainAddress;
				}
			}

			return result == null ? ZGuid.Empty : result.PK;
		}

		/// <summary>
		/// we should only call this if sending/receiving agents is a valid organization
		/// </summary>
		protected void SetConsolCreditorFromForwardingAgents(ZGuid newOrgPK, ZGuid oldAddressPk)
		{
			var previousAgentOrgPK = Factory.Load<OrgAddress>(oldAddressPk)?.OA_OH ?? ZGuid.Empty;

			if (previousAgentOrgPK == newOrgPK)
			{
				return;
			}

			if (JK_OA_CreditorAddress.IsValid)
			{
				return;
			}

			if (JK_OA_ShippingLineAddress.IsValid)
			{
				SetDefaultCreditorFromCarrier(JK_OA_ShippingLineAddress);
				return;
			}

			if (!IsCoLoad)
			{
				JK_OA_CreditorAddress = GetConsolCreditorAddressBasedOnSendingForwarderAndShippingLine(ZGuid.Empty);
			}
		}

		public virtual ZGuid GetCreditorPKForConsolCost(ZGuid rateProviderOrgPK)
		{
			var addressPK = GetConsolCreditorAddressBasedOnSendingForwarderAndShippingLine(rateProviderOrgPK);
			var address = Factory.Load<OrgAddress>(addressPK);
			return address != null && address.IsInDatabase
				? address.OA_OH
				: ZGuid.Empty;
		}

		ZGuid GetConsolCreditorAddressBasedOnSendingForwarderAndShippingLine(ZGuid rateProviderOrgPK)
		{
			var result = JK_OA_CreditorAddress;

			bool isImportIncludingDomestic = this.IsImport() || JK_RL_NKDischargePort == GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			bool isExportIncludingDomestic = this.IsExport() || JK_RL_NKLoadPort == GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			bool isCollect = JK_PrepaidCollect == Core.Constants.PaymentType.Collect;
			bool isPrepaid = JK_PrepaidCollect == Core.Constants.PaymentType.Prepaid;
			var preferedOrg = ShippingLine;
			var preferedAddressPK = JK_OA_ShippingLineAddress;

			if (IsCoLoad && !CreditorPK.IsEmpty)
			{
				if (rateProviderOrgPK.IsEmpty) // In the case of Standard Cost or manual input Charge, we follow legacy logic (Agreed in WI00427865)
				{
					return result;
				}
				else if (CreditorPK == rateProviderOrgPK)
				{
					preferedOrg = Creditor;
					preferedAddressPK = JK_OA_CreditorAddress;
				}
			}

			if (isImportIncludingDomestic
				&& isPrepaid
				&& SendingForwarder != null
				&& SendingForwarder.OH_IsCreditor)
			{
				result = SendingForwarderAddress.PK;
			}
			else if (((isImportIncludingDomestic && isCollect) || (isExportIncludingDomestic && isPrepaid))
					&& preferedOrg != null)
			{
				result = GetCreditorAddress(null);
			}
			else if (isExportIncludingDomestic
					&& isCollect
					&& ReceivingForwarder != null
					&& ReceivingForwarder.OH_IsCreditor)
			{
				result = ReceivingForwarderAddress.PK;
			}

			return result;

			ZGuid GetCreditorAddress(OrgAddress address) =>
				(address?.Header.OH_IsCreditor ?? false)
					? address.PK
					: (preferedOrg?.OH_IsCreditor ?? false)
						? preferedAddressPK
						: ZGuid.Empty;
		}

		#endregion

		#region Set Type Flags

		bool needRecalculateJK_IsCFS;

		public void SetTypeFlags()
		{
			SetTypeFlagsCore();
		}

		protected virtual void SetTypeFlagsCore()
		{
			if (!JK_IsCFS && !IsDeleted)
			{
				ZBool packIsOwnCFS = ZBool.False;

				if (PackDepotAddress != null && PackDepotAddress.Header != null)
				{
					packIsOwnCFS = PackDepotAddress.Header.IsProxyOrgOfAnyCompany();
				}

				ZBool unpackIsOwnCFS = ZBool.False;

				if (UnpackDepotAddress != null && UnpackDepotAddress.Header != null)
				{
					unpackIsOwnCFS = UnpackDepotAddress.Header.IsProxyOrgOfAnyCompany();
				}

				bool isLoadPortLocal = ImportExportHelper.IsBranchCountry(JK_RL_NKLoadPort);
				bool isDischargePortLocal = ImportExportHelper.IsBranchCountry(JK_RL_NKDischargePort);
				bool isCFS = false;

				if (isLoadPortLocal)
				{
					isCFS = packIsOwnCFS;
				}

				if (!isCFS && isDischargePortLocal)
				{
					isCFS = unpackIsOwnCFS;
				}

				if (!isCFS && packIsOwnCFS)
				{
					isCFS = ImportExportHelper.IsAnyBranchCountry(Factory, JK_RL_NKLoadPort);
				}

				if (!isCFS && unpackIsOwnCFS)
				{
					isCFS = ImportExportHelper.IsAnyBranchCountry(Factory, JK_RL_NKDischargePort);
				}

				JK_IsCFS = isCFS;
			}
		}

		#endregion

		#region JK_IsCFS

		public override ZBool JK_IsCFS
		{
			get { return base.JK_IsCFS; }
			set
			{
				if (value || !IsInDatabase || !(ZBool)JK_IsCFSInfo.OriginalValue)
				{
					base.JK_IsCFS = value;

					if (value || !IsInDatabase)
					{
						foreach (CommonShipment shipment in Shipments)
						{
							if (value || !shipment.IsInDatabase)
							{
								shipment.JS_IsCFSRegistered = value;
							}
							shipment.JS_TranshipToOtherCFS = value && shipment.IsImport() && !shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster;
						}

						foreach (CommonContainer container in Containers)
						{
							if (value || !container.IsInDatabase)
							{
								container.JC_IsCFSRegistered = value;
							}
						}
					}
				}
			}
		}

		#endregion

		#region JK_RL_NKLoadPort

		[List("RefUNLOCO_List")]
		public override ZString JK_RL_NKLoadPort
		{
			get { return base.JK_RL_NKLoadPort; }
			set
			{
				if (JK_RL_NKLoadPort != value)
				{
					bool wasImport = this.IsImport();

					base.JK_RL_NKLoadPort = value;

					SetIsDomestic();

					if (!IsCopying && !IsImportingData)
					{
						ResetListsOnPortOfLoadingChanged();
						DefaultSendingForwarder();
					}

					if (Transports.Count == 1)
					{
						Transport transport = Transports[0];

						IDisposable hasChangesSuspender = IsSettingHasChangesSuspended ? transport.SuspendSettingHasChanges() : null;
						try
						{
							transport.JW_RL_NKLoadPort = value;
						}
						finally
						{
							if (hasChangesSuspender != null)
							{
								hasChangesSuspender.Dispose();
							}
						}
					}

					needRecalculateJK_IsCFS = true;

					if (!IsCopying)
					{
						if (!IsImportingData)
						{
							DefaultDepartureCTOAddressFromCarrier();
							DefaultPickupContainerYardFromCarrier();
						}

						SetDepartureDepot();

						if (!IsImportingData)
						{
							DefaultCoLoadWithAddress();
						}
					}
				}

				Transports.MarkAsNeedingValidation();
			}
		}

		protected virtual void DefaultSendingForwarder()
		{
			if (!IsDirect && !SettingDefaultValues)
			{
				SetDefaultSendingForwarderAddress();
			}
		}

		#endregion

		#region JK_RL_NKDischargePort

		[List("RefUNLOCO_List")]
		public override ZString JK_RL_NKDischargePort
		{
			get { return base.JK_RL_NKDischargePort; }
			set
			{
				if (JK_RL_NKDischargePort != value)
				{
					bool wasImport = this.IsImport();

					base.JK_RL_NKDischargePort = value;

					SetIsDomestic();

					if (!IsCopying && !IsImportingData)
					{
						ResetListsOnPortOfDischargeChanged();
						DefaultReceivingForwarder();

						foreach (CommonShipment shipment in Shipments)
						{
							shipment.UpdateETAWithPortDefaultDeliveryTime();
							shipment.UpdateETDeliveryWithPortDefaultDeliveryTime();
						}
					}

					if (Transports.Count == 1)
					{
						Transport transport = Transports[0];

						IDisposable hasChangesSuspender = IsSettingHasChangesSuspended ? transport.SuspendSettingHasChanges() : null;
						try
						{
							transport.JW_RL_NKDiscPort = value;
						}
						finally
						{
							if (hasChangesSuspender != null)
							{
								hasChangesSuspender.Dispose();
							}
						}
					}

					needRecalculateJK_IsCFS = true;

					if (!IsCopying)
					{
						if (!IsImportingData)
						{
							DefaultArrivalCTOAddressFromCarrier();
							DefaultReturnContainerYardFromCarrier();
						}

						SetArrivalDepot();

						if (!IsImportingData)
						{
							DefaultCoLoadWithAddress();
						}
					}
				}

				Transports.MarkAsNeedingValidation();
			}
		}

		protected virtual void DefaultReceivingForwarder()
		{
			if (!IsDirect && !SettingDefaultValues)
			{
				SetDefaultReceivingForwarderAddress();
			}
		}

		#endregion

		#region JK_RL_NKLastForeignPort

		[ActionField(ReadOnly = false)]
		public override ZString JK_RL_NKLastForeignPort
		{
			get => base.JK_RL_NKLastForeignPort;
			set => base.JK_RL_NKLastForeignPort = value;
		}

		#endregion

		#region JK_OA_PackDepotAddress

		[List("JK_OA_List")]
		public override ZGuid JK_OA_PackDepotAddress
		{
			get { return base.JK_OA_PackDepotAddress; }
			set
			{
				if (JK_OA_PackDepotAddress != value)
				{
					base.JK_OA_PackDepotAddress = value;
					needRecalculateJK_IsCFS = true;
				}
			}
		}

		#endregion

		#region JK_OA_UnpackDepotAddress

		[List("JK_OA_List")]
		public override ZGuid JK_OA_UnpackDepotAddress
		{
			get { return base.JK_OA_UnpackDepotAddress; }
			set
			{
				if (JK_OA_UnpackDepotAddress != value)
				{
					base.JK_OA_UnpackDepotAddress = value;
					needRecalculateJK_IsCFS = true;
				}
			}
		}

		#endregion

		#region JK_JX_DepartOrArriveReference

		public ZString JK_JX_DepartOrArriveReference
		{
			get { return Transports.MostInterestingTransport?.JW_JX_DepartOrArriveReference ?? ZString.Empty; }
		}

		public ZPropertyInfo JK_JX_DepartOrArriveReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(JK_JX_DepartOrArriveReference)); }
		}

		#endregion

		#region JK_JX_DepartOrArriveBerth

		public ZString JK_JX_DepartOrArriveBerth
		{
			get { return Transports.MostInterestingTransport?.JW_JX_DepartOrArriveBerth ?? ZString.Empty; }
		}

		public ZPropertyInfo JK_JX_DepartOrArriveBerthInfo
		{
			get { return GetZPropertyInfo(nameof(JK_JX_DepartOrArriveBerth)); }
		}

		#endregion

		#region JK_ConsolCutOffDate

		public override ZDateTime JK_ConsolCutOffDate
		{
			get { return base.JK_ConsolCutOffDate; }
			set
			{
				if (JK_ConsolCutOffDate != value)
				{
					base.JK_ConsolCutOffDate = value;

					var localTime = value.IsValid ? (ZDateTime)Env.Time.GetLocalTimeFromUtc(value.ToDateTime()) : value;
					Logs.CreateRecreateOrUpdateEventLog(Events.PreAllocationCutOffDate, EstimateActual.Estimate, localTime.ToOffset());
				}
			}
		}

		#endregion

		#region JK_ConsolCutOffDateLocal

		[EventDateProperty(Events.PreAllocationCutOffDateCode, EstimateActual.Estimate)]
		public ZDateTime JK_ConsolCutOffDateLocal
		{
			get { return JK_ConsolCutOffDate.IsValid ? Env.Time.GetLocalTimeFromUtc(JK_ConsolCutOffDate.ToDateTime()) : JK_ConsolCutOffDate; }
			set
			{
				var utcTime = value.IsValid ? (ZDateTime)Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
				if (utcTime != JK_ConsolCutOffDate)
				{
					SetPropertyValue(JK_ConsolCutOffDateInfo, utcTime);

					Logs.CreateRecreateOrUpdateEventLog(Events.PreAllocationCutOffDate, EstimateActual.Estimate, value.ToOffset());

					if (!IsValidationSuspended)
					{
						Validation.ValidateJK_ConsolCutOffDate();
					}
				}
			}
		}

		public ZPropertyInfo JK_ConsolCutOffDateLocalInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.JK_ConsolCutOffDateLocal, x => JK_ConsolCutOffDateInfo);
			}
		}

		#endregion

		#region JK_MaximumAllowablePackageUnit

		[List("JK_PackageUnit_List")]
		public override ZString JK_MaximumAllowablePackageUnit
		{
			get { return base.JK_MaximumAllowablePackageUnit; }
			set { base.JK_MaximumAllowablePackageUnit = value; }
		}

		#endregion

		#region JK_JK_MasterConsol

		[RelatedBusinessObject("MasterConsol")]
		public override ZGuid JK_JK_MasterConsol
		{
			get { return base.JK_JK_MasterConsol; }
			set { base.JK_JK_MasterConsol = value; }
		}

		public CommonConsol MasterConsol
		{
			get { return Factory.Load<CommonConsol>(JK_JK_MasterConsol); }
		}

		#endregion

		#region Advance Cargo Reporting Self-Filer

		public ZBool IsAdvanceCargoReportingSelfFiler
		{
			get
			{
				return (ReceivingForwarder?.MiscServ is OrgMiscServ recevForwarderMiscServ && recevForwarderMiscServ.OM_FWAdvanceCargoReportingSelfFiler);
			}
		}

		#endregion

		#region Flight Tracking Subscription

		public bool IsAllDataValidForFlightTrackingSubscription
		{
			get
			{
				return IsAir && IsCoLoad
					&& Creditor?.ShippingLine != null && !Creditor.ShippingLine.RSL_CargoWiseOneCode.IsEmpty
					&& (!JK_CoLoadMasterBill.IsEmpty || !JK_CoLoadBookingReference.IsEmpty);
			}
		}

		#endregion

		#region New Properties

		public ZBool HasETDPassed
		{
			get { return (!JK_JX_JA_E_DEP.IsEmpty && ZDateTime.Now > JK_JX_JA_E_DEP); }
		}

		public bool AutomaticallyUpdatePackLineContainers
		{
			get
			{
				if (autoUpdatePackLineContainersDisabled)
				{
					return false;
				}

				if (automaticallyUpdatePackLineContainers == null)
				{
					automaticallyUpdatePackLineContainers = FreightConfigurationRegistry.Instance.AutoPackFreightContainers.Value;
				}
				return (bool)automaticallyUpdatePackLineContainers;
			}
			set { automaticallyUpdatePackLineContainers = value; }
		}

		bool? automaticallyUpdatePackLineContainers;

		public IDisposable DisableAutoUpdatePackLineContainers()
		{
			autoUpdatePackLineContainersDisabled = true;
			return new DisposableAction(() => autoUpdatePackLineContainersDisabled = false);
		}
		bool autoUpdatePackLineContainersDisabled;

		#region Calculated Properties

		#region Total Chargeable for Collect

		protected ZBool CollectShipmentsAreMultiCurrency
		{
			get
			{
				if (CollectShipments.Count > 1)
				{
					ZString currency = CollectShipments[0].JS_RX_NKFrtRateCurrency;
					foreach (CommonShipment shipment in CollectShipments)
					{
						if (shipment.JS_RX_NKFrtRateCurrency != currency)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		RefCurrency TotalCollectShipmentChargeableAmountCurrency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency; }
		}

		#endregion

		#region Total Chargeable for Prepaid shipments

		protected ZBool PrepaidShipmentsAreMultiCurrency
		{
			get
			{
				if (PrePaidShipments.Count > 1)
				{
					ZString currency = PrePaidShipments[0].JS_RX_NKFrtRateCurrency;
					foreach (CommonShipment shipment in PrePaidShipments)
					{
						if (shipment.JS_RX_NKFrtRateCurrency != currency)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		RefCurrency TotalPrepaidShipmentChargeableAmountCurrency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency; }
		}

		#endregion

		#region AllowsShipmentsWithApprovedForCargoOnlyInspectionType

		public bool AllowsShipmentsWithApprovedForCargoOnlyInspectionType
		{
			get
			{
				if (SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(this))
				{
					var flight1 = GetTransportByPlanningType(Constants.TransportPlanningType.Flight1);
					if (flight1 != null && !flight1.JW_IsCargoOnly)
					{
						return false;
					}
				}

				return true;
			}
		}

		#endregion

		#region DepartureFlightOriginLoco

		public RefUNLOCO DepartureFlightOriginLoco
		{
			get
			{
				var flight1 = GetTransportByPlanningType(TransportPlanningType.Flight1);

				RefUNLOCO originPort = null;
				if (flight1 != null)
				{
					var sortedTransports = Transports.ToArray<Transport>();
					MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
					var firstFlightIndex = Array.IndexOf(sortedTransports, flight1);
					for (var i = firstFlightIndex - 1; i >= 0; i--)
					{
						if (!sortedTransports[i].JW_OA_CarrierAddress.IsEmpty && sortedTransports[i].Carrier != ShippingLine)
						{
							originPort = sortedTransports[i + 1].LoadPort;
							break;
						}
					}
				}

				if (originPort == null)
				{
					originPort = LoadPort;
				}

				if ((originPort == null || originPort.RL_IATA.IsEmpty) && flight1 != null)
				{
					originPort = flight1.LoadPort;
				}

				return originPort;
			}
		}

		public Transport GetTransportByPlanningType(string planningType)
		{
			return Transports.Cast<Transport>().FirstOrDefault(x => x.JW_TransportMode == Core.Constants.TransportModes.Air && x.JW_TransportType == planningType);
		}

		#endregion

		#endregion

		#region ShipmentConsignor

		ZGuid fShipmentConsignor = ZGuid.Empty;
		public ZGuid ShipmentConsignor
		{
			get { return fShipmentConsignor; }
			set { fShipmentConsignor = value; }
		}

		#endregion

		#region Creditor Addresses

		public virtual OrgHeader CarrierExportCreditor { get; }

		public virtual JobDocAddress CarrierExportCreditorAddress { get; }

		public virtual OrgHeader CarrierImportCreditor { get; }

		public virtual JobDocAddress CarrierImportCreditorAddress { get; }

		#endregion

		#endregion

		#region New Bound Properties

		#region Calculated Properties

		#region JK_TotalShipmentWeight

		public ZDecimal JK_TotalShipmentWeight
		{
			get
			{
				var result = TotalCalculation.GetTotalWeight(ShipmentsForTotalling, CommonShipment.Schema.JS_ActualWeight, CommonShipment.Schema.JS_UnitOfWeight, JK_TotalShipmentWeightUnit);
				return this.GetRoundedValue(JK_TotalShipmentWeightInfo, result);
			}
		}

		public ZPropertyInfo JK_TotalShipmentWeightInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalShipmentWeight)); }
		}

		#endregion

		#region JK_TotalDocumentedWeight

		public ZDecimal JK_TotalDocumentedWeight
		{
			get
			{
				var result = TotalCalculation.GetTotalWeight(TopLevelShipments, CommonShipment.Schema.JS_DocumentedWeight, CommonShipment.Schema.JS_UnitOfWeight, JK_TotalShipmentWeightUnit);
				return this.GetRoundedValue(JK_TotalDocumentedWeightInfo, result);
			}
		}

		public ZPropertyInfo JK_TotalDocumentedWeightInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalDocumentedWeight)); }
		}

		#endregion

		#region JK_TotalManifestedWeight

		public ZDecimal JK_TotalManifestedWeight
		{
			get
			{
				var result = TotalCalculation.GetTotalWeight(TopLevelShipments, CommonShipment.Schema.JS_ManifestedWeight, CommonShipment.Schema.JS_UnitOfWeight, JK_TotalShipmentWeightUnit);
				return this.GetRoundedValue(JK_TotalManifestedWeightInfo, result);
			}
		}

		public ZPropertyInfo JK_TotalManifestedWeightInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalManifestedWeight)); }
		}

		#endregion

		#region JK_TotalShipmentWeightUnit

		public ZString JK_TotalShipmentWeightUnit
		{
			get
			{
				if (ShipmentsForTotalling.Any() && ShipmentsForTotalling.Cast<CommonShipment>().All(shipment => Constants.Weight.IsImperial(shipment.JS_UnitOfWeight)))
				{
					return Constants.Weight.Pounds;
				}
				return Constants.Weight.Kilograms;
			}
		}

		public ZPropertyInfo JK_TotalShipmentWeightUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalShipmentWeightUnit)); }
		}

		#endregion

		#region JK_TotalShipmentVolume

		public ZDecimal JK_TotalShipmentVolume
		{
			get
			{
				var result = TotalCalculation.GetTotalVolume(ShipmentsForTotalling, CommonShipment.Schema.JS_ActualVolume, CommonShipment.Schema.JS_UnitOfVolume, JK_TotalShipmentVolumeUnit);
				return this.GetRoundedValue(JK_TotalShipmentVolumeInfo, result);
			}
		}

		public ZPropertyInfo JK_TotalShipmentVolumeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalShipmentVolume)); }
		}

		#endregion

		#region JK_TotalDocumentedVolume

		public ZDecimal JK_TotalDocumentedVolume
		{
			get
			{
				var result = TotalCalculation.GetTotalVolume(TopLevelShipments, CommonShipment.Schema.JS_DocumentedVolume, CommonShipment.Schema.JS_UnitOfVolume, JK_TotalShipmentVolumeUnit);
				return this.GetRoundedValue(JK_TotalDocumentedVolumeInfo, result);
			}
		}

		public ZPropertyInfo JK_TotalDocumentedVolumeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalDocumentedVolume)); }
		}

		#endregion

		#region JK_TotalManifestedVolume

		public ZDecimal JK_TotalManifestedVolume
		{
			get
			{
				var result = TotalCalculation.GetTotalVolume(TopLevelShipments, CommonShipment.Schema.JS_ManifestedVolume, CommonShipment.Schema.JS_UnitOfVolume, JK_TotalShipmentVolumeUnit);
				return this.GetRoundedValue(JK_TotalManifestedVolumeInfo, result);
			}
		}

		public ZPropertyInfo JK_TotalManifestedVolumeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalManifestedVolume)); }
		}

		#endregion

		#region JK_TotalShipmentVolumeUnit

		public ZString JK_TotalShipmentVolumeUnit
		{
			get
			{
				if (ShipmentsForTotalling.Any() && ShipmentsForTotalling.Cast<CommonShipment>().All(shipment => Constants.Volume.IsImperial(shipment.JS_UnitOfVolume)))
				{
					return Constants.Volume.CubicFeet;
				}
				return Constants.Volume.CubicMetres;
			}
		}

		public ZPropertyInfo JK_TotalShipmentVolumeUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalShipmentVolumeUnit)); }
		}

		#endregion

		#region JK_TotalShipmentLoadingMeters

		[DecimalPlaces(3)]
		public ZDecimal JK_TotalShipmentLoadingMeters
		{
			get { return TotalCalculation.GetTotal(ShipmentsForTotalling, CommonShipment.Schema.JS_LoadingMeters); }
		}

		public ZPropertyInfo JK_TotalShipmentLoadingMetersInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalShipmentLoadingMeters)); }
		}

		public bool IsRoadLoadingMetersEnabled
		{
			get { return IsRoad && FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value; }
		}

		#endregion

		#region JK_TotalDocumentedLoadingMeters

		[DecimalPlaces(3)]
		public ZDecimal JK_TotalDocumentedLoadingMeters
		{
			get { return TotalCalculation.GetTotal(ShipmentsForTotalling, CommonShipment.Schema.JS_DocumentedLoadingMeters); }
		}

		public ZPropertyInfo JK_TotalDocumentedLoadingMetersInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalDocumentedLoadingMeters)); }
		}

		#endregion

		#region JK_TotalManifestedLoadingMeters

		[DecimalPlaces(3)]
		public ZDecimal JK_TotalManifestedLoadingMeters
		{
			get { return TotalCalculation.GetTotal(ShipmentsForTotalling, CommonShipment.Schema.JS_ManifestedLoadingMeters); }
		}

		public ZPropertyInfo JK_TotalManifestedLoadingMetersInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalManifestedLoadingMeters)); }
		}

		#endregion

		#region JK_TotalShipmentChargeable

		public ZDecimal JK_TotalShipmentChargeable
		{
			get
			{
				ZDecimal result;

				if (Volume.ContainsCode(JK_Calc_TotalShipmentChargeableUnit))
				{
					result = TotalCalculation.GetTotalVolume(ShipmentsForTotalling.Cast<CommonShipment>(), GetActualChargeable, GetChargeableUnit, JK_Calc_TotalShipmentChargeableUnit);
				}
				else
				{
					result = TotalCalculation.GetTotalWeight(ShipmentsForTotalling.Cast<CommonShipment>(), GetActualChargeable, GetChargeableUnit, JK_Calc_TotalShipmentChargeableUnit);
				}

				return this.GetRoundedValue(JK_TotalShipmentChargeableInfo, result);
			}
		}

		object GetActualChargeable(CommonShipment shipment)
		{
			return JK_TransportMode == TransportModes.Air && shipment.IsSeaAir
				? ChargeableAmountCalculator.Convert(new Quantity(shipment.JS_ActualChargeable, shipment.JS_ChargeableUnit), JK_Calc_TotalShipmentChargeableUnit, ChargeableAmountCalculator.GetDefaultConversionFactors(this.IsDomestic(), JK_TransportMode, JK_Calc_TotalShipmentChargeableUnit)).Amount
				: shipment.JS_ActualChargeable;
		}

		object GetChargeableUnit(CommonShipment shipment)
		{
			return JK_TransportMode == TransportModes.Air && shipment.IsSeaAir
				? JK_Calc_TotalShipmentChargeableUnit
				: shipment.JS_ChargeableUnit;
		}

		public ZPropertyInfo JK_TotalShipmentChargeableInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalShipmentChargeable)); }
		}

		#endregion

		#region JK_Calc_TotalShipmentChargeableUnit

		[BusinessObjectTestExclude]
		public ZString JK_Calc_TotalShipmentChargeableUnit
		{
			get { return ChargeableAmountCalculator.GetChargeableUnit(JK_TransportMode, JK_TotalShipmentWeightUnit, JK_TotalShipmentVolumeUnit); }
		}

		public ZPropertyInfo JK_Calc_TotalShipmentChargeableUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalShipmentChargeableUnit)); }
		}

		#endregion

		#region JK_Calc_FreeSpace

		[BusinessObjectTestExclude]
		public ZDecimal JK_Calc_FreeSpace
		{
			get { return JK_TotalShipmentChargeable - JK_ConsolChargeable; }
		}

		public ZPropertyInfo JK_Calc_FreeSpaceInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_FreeSpace)); }
		}

		#endregion

		#region JK_Calc_ActualVolumeWeight

		[BusinessObjectTestExclude]
		public ZDecimal JK_Calc_ActualVolumeWeight
		{
			get
			{
				var conversionFactor = GetConsolConversionFactor();
				if (conversionFactor.IsEmpty
				|| JK_CorrectedConsolWeightUnit.IsEmpty
				|| JK_CorrectedConsolVolumeUnit.IsEmpty
				|| !Constants.Weight.ContainsCode(JK_CorrectedConsolWeightUnit)
				|| !Constants.Volume.ContainsCode(JK_CorrectedConsolVolumeUnit))
				{
					return 0m;
				}

				var quantityToConvert = IsConsolChargeableByWeight
					? new ZVolume(JK_CorrectedConsolVolume, JK_CorrectedConsolVolumeUnit)
					: (IQuantity)new ZWeight(JK_CorrectedConsolWeight, JK_CorrectedConsolWeightUnit);

				return quantityToConvert.Convert(JK_Calc_ActualVolumeWeightUnit, new[] { conversionFactor }, true).Amount;
			}
		}

		public ConversionFactor GetConsolConversionFactor()
		{
			var chargeableFactor = ChargeableFactor.GetDefault(IsDomesticFreight ? ChargeableFactorSource.Domestic : ChargeableFactorSource.International, JK_TransportMode);
			if (chargeableFactor == null)
			{
				return ConversionFactor.Empty;
			}

			var isImperial = IsConsolChargeableByWeight
				? Constants.Weight.IsImperial(JK_ConsolChargeableUnit)
				: Constants.Volume.IsImperial(JK_ConsolChargeableUnit);

			return isImperial ? chargeableFactor.ImperialFactor : chargeableFactor.MetricFactor;
		}

		public bool IsConsolChargeableByWeight
		{
			get { return Constants.Weight.ContainsCode(JK_ConsolChargeableUnit); }
		}

		public ZPropertyInfo JK_Calc_ActualVolumeWeightInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_ActualVolumeWeight)); }
		}

		public ZString JK_Calc_ActualVolumeWeightUnit
		{
			get
			{
				return IsConsolChargeableByWeight
					? JK_CorrectedConsolWeightUnit
					: JK_CorrectedConsolVolumeUnit;
			}
		}

		public ZPropertyInfo JK_Calc_ActualVolumeWeightUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_ActualVolumeWeightUnit)); }
		}

		#endregion

		#region JK_OverrideConsolChargeable

		public override ZBool JK_OverrideConsolChargeable
		{
			get { return base.JK_OverrideConsolChargeable; }
			set
			{
				if (value != base.JK_OverrideConsolChargeable)
				{
					if (value)
					{
						base.JK_OverrideConsolChargeable = value;
						SetCorrectedMeasuresToDefaults();
					}
					else
					{
						SetCorrectedMeasuresToZero();
						base.JK_OverrideConsolChargeable = value;
					}
				}
			}
		}

		void SetCorrectedMeasuresToDefaults()
		{
			base.JK_ConsolChargeable = this.GetRoundedValue(JobConsolSchema.JK_ConsolChargeable, JK_ConsolChargeableInfo, CalculateConsolChargeable());
			base.JK_CorrectedConsolVolume = JK_TotalShipmentVolume;
			base.JK_CorrectedConsolWeight = JK_TotalShipmentWeight;
			base.JK_CorrectedConsolWeightUnit = JK_TotalShipmentWeightUnit;
			base.JK_CorrectedConsolVolumeUnit = JK_TotalShipmentVolumeUnit;
		}

		void SetCorrectedMeasuresToZero()
		{
			base.JK_ConsolChargeable = 0m;
			base.JK_CorrectedConsolVolume = 0m;
			base.JK_CorrectedConsolWeight = 0m;
		}

		#endregion

		#region JK_ConsolChargeable

		public override ZDecimal JK_ConsolChargeable
		{
			get
			{
				return JK_OverrideConsolChargeable
					? base.JK_ConsolChargeable
					: GetConsolChargeble(CalculateConsolChargeable());
			}
			set
			{
				if (isSettingConsolChargeable)
				{
					return;
				}

				base.JK_ConsolChargeable = GetConsolChargeble(value);

				if (!fIsImportingData)
				{
					isSettingConsolChargeable = true;
					try
					{
						UpdateCorrectedMeasuresFromChargeable();
					}
					finally
					{
						isSettingConsolChargeable = false;
					}
				}
			}
		}

		ZDecimal GetConsolChargeble(ZDecimal value)
		{
			ZDecimal newValue = this.GetRoundedValue(JobConsolSchema.JK_ConsolChargeable, JK_ConsolChargeableInfo, value);
			return IsAir ? (ZDecimal)ChargeableWeightRoundingHelper.GetRoundedValueAir(JobConsolSchema.JK_ConsolChargeable, newValue) : newValue;
		}

		protected bool isSettingConsolChargeable;

		protected bool JK_ConsolChargeable_ReadOnly
		{
			get { return !JK_OverrideConsolChargeable; }
		}

		ZDecimal CalculateConsolChargeable()
		{
			return CalculateChargeable(JK_TotalShipmentWeight, JK_TotalShipmentWeightUnit, JK_TotalShipmentVolume, JK_TotalShipmentVolumeUnit, JK_TotalShipmentLoadingMeters);
		}

		ZDecimal CalculateConsolChargeableFromCorrected()
		{
			return CalculateChargeable(JK_CorrectedConsolWeight, JK_CorrectedConsolWeightUnit, JK_CorrectedConsolVolume, JK_CorrectedConsolVolumeUnit, JK_TotalShipmentLoadingMeters);
		}

		void UpdateCorrectedMeasuresFromChargeable()
		{
			if (!JK_OverrideConsolChargeable || JK_ConsolChargeable.IsEmpty || !JK_CorrectedConsolVolume.IsEmpty && !JK_CorrectedConsolWeight.IsEmpty)
			{
				return;
			}

			if (FreightDataRegistry.Instance.WeightChargableTransportModes.Contains((string)JK_TransportMode) && JK_CorrectedConsolVolume.IsEmpty && !isSettingCorrectedVolume)
			{
				ZVolume? newCorrectedVolume = ChargeableAmountCalculator.GetActualFromChargeable(
					JK_TransportMode, IsDomesticFreight, new ZWeight(JK_ConsolChargeable, JK_ConsolChargeableUnit),
					new ZWeight(JK_CorrectedConsolWeight, JK_CorrectedConsolWeightUnit), new ZVolume(JK_CorrectedConsolVolume, JK_CorrectedConsolVolumeUnit));

				if (newCorrectedVolume.HasValue)
				{
					JK_CorrectedConsolVolume = newCorrectedVolume.Value.Amount;
				}
			}
			else if (FreightDataRegistry.Instance.VolumeChargableTransportModes.Contains((string)JK_TransportMode) && JK_CorrectedConsolWeight.IsEmpty && !isSettingCorrectedWeight)
			{
				ZWeight? newCorrectedWeight = ChargeableAmountCalculator.GetActualFromChargeable(
					 JK_TransportMode, IsDomesticFreight, new ZVolume(JK_ConsolChargeable, JK_ConsolChargeableUnit),
					 new ZWeight(JK_CorrectedConsolWeight, JK_CorrectedConsolWeightUnit), new ZVolume(JK_CorrectedConsolVolume, JK_CorrectedConsolVolumeUnit));

				if (newCorrectedWeight.HasValue)
				{
					JK_CorrectedConsolWeight = newCorrectedWeight.Value.Amount;
				}
			}
		}

		#endregion

		#region JK_ConsolChargeableRate

		[DecimalPlaces(2)]
		public override ZDecimal JK_ConsolChargeableRate
		{
			get { return base.JK_ConsolChargeableRate; }
			set { base.JK_ConsolChargeableRate = value; }
		}

		#endregion

		#region JK_CorrectedConsolVolume

		[MeasureUnit(Schema.JK_CorrectedConsolVolumeUnit, MeasureUnitType.Volume)]
		public override ZDecimal JK_CorrectedConsolVolume
		{
			get { return JK_OverrideConsolChargeable ? base.JK_CorrectedConsolVolume : JK_TotalShipmentVolume; }
			set
			{
				if (isSettingCorrectedVolume)
				{
					return;
				}

				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_CorrectedConsolVolume, JK_CorrectedConsolVolumeInfo, value);
				if (base.JK_CorrectedConsolVolume != roundedValue)
				{
					try
					{
						isSettingCorrectedVolume = true;

						base.JK_CorrectedConsolVolume = roundedValue;

						if (!isSettingConsolChargeable)
						{
							JK_ConsolChargeable = CalculateConsolChargeableFromCorrected();
						}
					}
					finally
					{
						isSettingCorrectedVolume = false;
					}
				}
			}
		}

		protected bool isSettingCorrectedVolume;

		protected bool JK_CorrectedConsolVolume_ReadOnly
		{
			get { return !JK_OverrideConsolChargeable; }
		}

		#endregion

		#region JK_CorrectedConsolVolumeUnit

		[BusinessObjectTestExclude]
		[List("JK_Calc_ShipmentVolumeUnit_List")]
		public override ZString JK_CorrectedConsolVolumeUnit
		{
			get { return JK_OverrideConsolChargeable ? base.JK_CorrectedConsolVolumeUnit : JK_TotalShipmentVolumeUnit; }
			set
			{
				if (base.JK_CorrectedConsolVolumeUnit != value)
				{
					base.JK_CorrectedConsolVolumeUnit = value;
					this.SetRoundedValue(JobConsolSchema.JK_CorrectedConsolVolume, JK_CorrectedConsolVolumeInfo);

					if (!isSettingConsolChargeable)
					{
						JK_ConsolChargeable = CalculateConsolChargeableFromCorrected();
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_CorrectedConsolVolumeUnit();
				}
			}
		}

		protected bool JK_CorrectedConsolVolumeUnit_ReadOnly
		{
			get { return !JK_OverrideConsolChargeable; }
		}

		#endregion

		#region JK_CorrectedConsolWeight

		[MeasureUnit(Schema.JK_CorrectedConsolWeightUnit, MeasureUnitType.Weight)]
		public override ZDecimal JK_CorrectedConsolWeight
		{
			get { return JK_OverrideConsolChargeable ? base.JK_CorrectedConsolWeight : JK_TotalShipmentWeight; }
			set
			{
				if (isSettingCorrectedWeight)
				{
					return;
				}

				var roundedValue = this.GetRoundedValue(JobConsolSchema.JK_CorrectedConsolWeight, JK_CorrectedConsolWeightInfo, value);
				if (base.JK_CorrectedConsolWeight != roundedValue)
				{
					try
					{
						isSettingCorrectedWeight = true;

						base.JK_CorrectedConsolWeight = roundedValue;

						if (!isSettingConsolChargeable)
						{
							JK_ConsolChargeable = CalculateConsolChargeableFromCorrected();
						}
					}
					finally
					{
						isSettingCorrectedWeight = false;
					}
				}
			}
		}

		protected bool isSettingCorrectedWeight;

		protected bool JK_CorrectedConsolWeight_ReadOnly
		{
			get { return !JK_OverrideConsolChargeable; }
		}

		#endregion

		#region JK_CorrectedConsolWeightUnit

		[BusinessObjectTestExclude]
		[List("JK_Calc_ShipmentWeightUnit_List")]
		public override ZString JK_CorrectedConsolWeightUnit
		{
			get { return JK_OverrideConsolChargeable ? base.JK_CorrectedConsolWeightUnit : JK_TotalShipmentWeightUnit; }
			set
			{
				if (base.JK_CorrectedConsolWeightUnit != value)
				{
					base.JK_CorrectedConsolWeightUnit = value;
					this.SetRoundedValue(JobConsolSchema.JK_CorrectedConsolWeight, JK_CorrectedConsolWeightInfo);

					if (!isSettingConsolChargeable)
					{
						JK_ConsolChargeable = CalculateConsolChargeableFromCorrected();
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_CorrectedConsolWeightUnit();
				}
			}
		}

		protected bool JK_CorrectedConsolWeightUnit_ReadOnly
		{
			get { return !JK_OverrideConsolChargeable; }
		}

		#endregion

		#region JK_TotalPrepaidShipmentChargeableAmount

		[DecimalPlaces(2)]
		public ZDecimal JK_TotalPrepaidShipmentChargeableAmount
		{
			get
			{
				ZDecimal result = 0;
				if (PrePaidShipments.Any())
				{
					if (TotalPrepaidShipmentChargeableAmountCurrency.RX_Code != PrePaidShipments[0].JS_RX_NKFrtRateCurrency || PrepaidShipmentsAreMultiCurrency)
					{
						foreach (CommonShipment shipment in PrePaidShipments)
						{
							if (shipment.FrtRateCurrency != null && TotalPrepaidShipmentChargeableAmountCurrency != null)
							{
								result += shipment.FrtRateCurrency.ConvertUsingSellRate(ZDateTime.Now, shipment.ChargeableAmount, TotalPrepaidShipmentChargeableAmountCurrency);
							}
						}
					}
					else
					{
						result = TotalCalculation.GetTotal(PrePaidShipments, "ChargeableAmount");
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JK_TotalPrepaidShipmentChargeableAmountInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalPrepaidShipmentChargeableAmount)); }
		}

		public ZString JK_TotalPrepaidShipmentChargeableAmountCurrencyCode
		{
			get { return TotalPrepaidShipmentChargeableAmountCurrency.RX_Code; }
		}

		public ZPropertyInfo JK_TotalPrepaidShipmentChargeableAmountCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalPrepaidShipmentChargeableAmountCurrencyCode)); }
		}

		public ZInt JK_TotalPrepaidShipmentChargeableAmountDecimals
		{
			get { return TotalPrepaidShipmentChargeableAmountCurrency.Decimals; }
		}

		public ZPropertyInfo JK_TotalPrepaidShipmentChargeableAmountDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalPrepaidShipmentChargeableAmountDecimals)); }
		}

		#endregion

		#region JK_TotalCollectShipmentChargeableAmount

		[DecimalPlaces(2)]
		public ZDecimal JK_TotalCollectShipmentChargeableAmount
		{
			get
			{
				ZDecimal result = 0;
				if (CollectShipments.Any())
				{
					if (CollectShipments[0].JS_RX_NKFrtRateCurrency != TotalCollectShipmentChargeableAmountCurrency.RX_Code || CollectShipmentsAreMultiCurrency)
					{
						foreach (CommonShipment shipment in CollectShipments)
						{
							if (shipment.FrtRateCurrency != null && TotalCollectShipmentChargeableAmountCurrency != null)
							{
								result += shipment.FrtRateCurrency.ConvertUsingSellRate(ZDateTime.Now, shipment.ChargeableAmount, TotalCollectShipmentChargeableAmountCurrency);
							}
						}
					}
					else
					{
						result = TotalCalculation.GetTotal(CollectShipments, "ChargeableAmount");
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JK_TotalCollectShipmentChargeableAmountInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalCollectShipmentChargeableAmount)); }
		}

		public ZString JK_TotalCollectShipmentChargeableAmountCurrencyCode
		{
			get { return TotalCollectShipmentChargeableAmountCurrency.RX_Code; }
		}

		public ZPropertyInfo JK_TotalCollectShipmentChargeableAmountCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalCollectShipmentChargeableAmountCurrencyCode)); }
		}

		public ZInt JK_TotalCollectShipmentChargeableAmountDecimals
		{
			get { return TotalCollectShipmentChargeableAmountCurrency.Decimals; }
		}

		public ZPropertyInfo JK_TotalCollectShipmentChargeableAmountDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalCollectShipmentChargeableAmountDecimals)); }
		}

		#endregion

		#region JK_TotalDocumentedChargeable

		public ZDecimal JK_TotalDocumentedChargeable
		{
			get
			{
				var result = CalculateChargeable(JK_TotalDocumentedWeight, JK_TotalShipmentWeightUnit, JK_TotalDocumentedVolume, JK_TotalShipmentVolumeUnit, JK_TotalDocumentedLoadingMeters);
				return this.GetRoundedValue(JK_TotalDocumentedChargeableInfo, result);
			}
		}

		public ZPropertyInfo JK_TotalDocumentedChargeableInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalDocumentedChargeable)); }
		}

		#endregion

		#region JK_TotalManifestedChargeable

		public ZDecimal JK_TotalManifestedChargeable
		{
			get
			{
				var result = CalculateChargeable(JK_TotalManifestedWeight, JK_TotalShipmentWeightUnit, JK_TotalManifestedVolume, JK_TotalShipmentVolumeUnit, JK_TotalManifestedLoadingMeters);
				return this.GetRoundedValue(JK_TotalManifestedChargeableInfo, result);
			}
		}

		public ZPropertyInfo JK_TotalManifestedChargeableInfo
		{
			get { return GetZPropertyInfo(nameof(JK_TotalManifestedChargeable)); }
		}

		#endregion

		#region JK_ConsolChargeableUnit

		public ZString JK_ConsolChargeableUnit
		{
			get { return ChargeableAmountCalculator.GetChargeableUnit(JK_TransportMode, JK_CorrectedConsolWeightUnit, JK_CorrectedConsolVolumeUnit); }
		}

		public ZPropertyInfo JK_TotalChargeableUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JK_ConsolChargeableUnit)); }
		}

		#endregion

		#region JK_TotalShipmentQuantity

		[DecimalPlaces(0)]
		public ZDecimal JK_TotalShipmentQuantity
		{
			get { return TotalCalculation.GetTotal(ShipmentsForTotalling, CommonShipment.Schema.JS_OuterPacks); }
		}

		public ZPropertyInfo JK_TotalShipmentQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.JK_TotalShipmentQuantity); }
		}

		#endregion

		#region JK_TotalShipmentPackageCount

		[DecimalPlaces(0)]
		public ZDecimal JK_TotalShipmentPackageCount
		{
			get { return TotalCalculation.GetTotal(TopLevelShipments, CommonShipment.Schema.JS_TotalPackageCount); }
		}

		public ZPropertyInfo JK_TotalShipmentPackageCountInfo
		{
			get { return GetZPropertyInfo(Schema.JK_TotalShipmentPackageCount); }
		}

		#endregion

		#region JK_ShipmentTotalQuantityPackType

		public ZString JK_ShipmentTotalQuantityPackType
		{
			get
			{
				ZString result = TopLevelShipments.Count > 0 ? TopLevelShipments[0].JS_F3_NKPackType.ToString() : FreightPacksDataRegistry.Instance.OuterPackUnit.Value;

				foreach (CommonShipment shipment in TopLevelShipments)
				{
					if (shipment.JS_F3_NKPackType != result)
					{
						result = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
						break;
					}
					else
					{
						result = shipment.JS_F3_NKPackType;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JK_ShipmentTotalQuantityPackTypeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_ShipmentTotalQuantityPackType)); }
		}

		#endregion

		#region JK_ShipmentTotalPackageCountPackType

		public ZString JK_ShipmentTotalPackageCountPackType
		{
			get
			{
				ZString result = TopLevelShipments.Count > 0 ? TopLevelShipments[0].JS_F3_NKTotalCountPackType.ToString() : FreightPacksDataRegistry.Instance.InnerPackUnit.Value;

				foreach (CommonShipment shipment in TopLevelShipments)
				{
					if (shipment.JS_F3_NKTotalCountPackType != result)
					{
						result = FreightPacksDataRegistry.Instance.InnerPackUnit.Value;
						break;
					}
					else
					{
						result = shipment.JS_F3_NKTotalCountPackType;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JK_ShipmentTotalPackageCountPackTypeInfo
		{
			get { return GetZPropertyInfo(nameof(JK_ShipmentTotalPackageCountPackType)); }
		}

		#endregion

		#region JK_Calc_VoyageLabel

		public ZString JK_Calc_VoyageLabel
		{
			get { return VesselVoyageCaptionProvider.GetVoyageCaption(JK_TransportMode); }
		}

		public ZPropertyInfo JK_Calc_VoyageLabelInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_VoyageLabel)); }
		}

		#endregion

		#region JK_Calc_VesselLabel

		public ZString JK_Calc_VesselLabel
		{
			get { return VesselVoyageCaptionProvider.GetVesselCaption(JK_TransportMode); }
		}

		public ZPropertyInfo JK_Calc_VesselLabelInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_VesselLabel)); }
		}

		#endregion

		#region JK_Calc_BookingReference_ReadOnly

		public ZString JK_Calc_BookingReference_ReadOnly
		{
			get { return JK_BookingReference; }
		}

		public ZPropertyInfo JK_Calc_BookingReference_ReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_BookingReference_ReadOnly)); }
		}

		#endregion

		#endregion

		#region ShowSubHouseBillShipments

		public ZBool ShowSubHouseBillShipments
		{
			get
			{
				if (showSubHouseBillShipments == null)
				{
					showSubHouseBillShipments = FreightDataRegistry.Instance.ShowSubHouseBills.Value;
				}

				return showSubHouseBillShipments.Value;
			}
			set
			{
				if (value != showSubHouseBillShipments)
				{
					showSubHouseBillShipments = value;
					GridShipments.ShouldShowChildShipments = value;
				}
				ShowSubHouseBillShipmentsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShowSubHouseBillShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(ShowSubHouseBillShipments)); }
		}

		bool? showSubHouseBillShipments;

		#endregion

		#region Read Only Properties

		#region JK_OA_ShippingLineAddress_ReadOnly

		public ZGuid JK_OA_ShippingLineAddress_ReadOnly
		{
			get { return JK_OA_ShippingLineAddress; }
		}

		public ZPropertyInfo JK_OA_ShippingLineAddress_ReadOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.JK_OA_ShippingLineAddress_ReadOnly); }
		}

		#endregion

		#region JK_OA_SendingForwarderAddress_ReadOnly

		public ZGuid JK_OA_SendingForwarderAddress_ReadOnly
		{
			get { return JK_OA_SendingForwarderAddress; }
		}

		public ZPropertyInfo JK_OA_SendingForwarderAddress_ReadOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.JK_OA_SendingForwarderAddress_ReadOnly); }
		}

		#endregion

		#region JK_OA_ReceivingForwarderAddress_ReadOnly

		public ZGuid JK_OA_ReceivingForwarderAddress_ReadOnly
		{
			get { return JK_OA_ReceivingForwarderAddress; }
		}

		public ZPropertyInfo JK_OA_ReceivingForwarderAddress_ReadOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.JK_OA_ReceivingForwarderAddress_ReadOnly); }
		}

		#endregion

		#region JK_CRN

		public ZString JK_EntryType
		{
			get
			{
				ZString result = "CRN:";
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.Australia:
						result = "CRN:";
						break;
					case Core.Constants.CountryCodes.NewZealand:
						result = "ORN:";
						break;
				}
				return result;
			}
		}

		public ZPropertyInfo JK_EntryTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JK_EntryType); }
		}

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public virtual ZString JK_CRN
		{
			get { return GetMostRecentCusEntryNumber()?.CE_EntryNum ?? ZString.Empty; }
			set
			{
				var entryNumber = GetMostRecentCusEntryNumber();
				if (entryNumber == null)
				{
					entryNumber = CusEntryNums.AddNew();
					entryNumber.CE_ParentTable = TableName;
					entryNumber.CE_EntryType = GetCusEntryNumType();
					entryNumber.CE_EntryIsSystemGenerated = false;
					entryNumber.CE_ParentID = PK;
					entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}
				entryNumber.CE_EntryNum = value;
				JK_CRNInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJK_CRN();
				}
			}
		}

		CusEntryNumber GetMostRecentCusEntryNumber() => CusEntryNums.OfType<CusEntryNumber>().OrderByDescending(n => n.CE_SystemLastEditTimeUtc).FirstOrDefault();

		protected virtual ZString GetCusEntryNumType()
		{
			return CusEntryNumberTypes.Standard.ClearancePermitNumber;
		}

		public ZPropertyInfo JK_CRNInfo
		{
			get { return GetZPropertyInfo(Schema.JK_CRN); }
		}

		protected bool JK_CRN_ReadOnly
		{
			get
			{
				if (fJK_CRNReadonly == null)
				{
					fJK_CRNReadonly = GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Constants.CountryCodes.Iceland;
				}

				return fJK_CRNReadonly.Value;
			}
		}

		bool? fJK_CRNReadonly;

		#endregion

		#region CusEntryNumCollection

		CusEntryNumCollection fCusEntryNums;

		[ChildEditable(true)]
		public CusEntryNumCollection CusEntryNums
		{
			get
			{
				if (fCusEntryNums == null)
				{
					ZQuery query = GetCusEntryQuery(true);
					fCusEntryNums = new CusEntryNumCollection(this.Factory, query);
					RegisterEditableChildObject(fCusEntryNums);
					fCusEntryNums.Load();
				}
				return fCusEntryNums;
			}
		}

		CusEntryNumCollection fCusEntryNumsForAllCountries;

		[ChildEditable(true)]
		public CusEntryNumCollection CusEntryNumsForAllCountries
		{
			get
			{
				if (fCusEntryNumsForAllCountries == null)
				{
					ZQuery query = GetCusEntryQuery(false);
					fCusEntryNumsForAllCountries = new CusEntryNumCollection(this.Factory, query);
					RegisterEditableChildObject(fCusEntryNumsForAllCountries);
					fCusEntryNumsForAllCountries.Load();
				}
				return fCusEntryNumsForAllCountries;
			}
		}

		ZQuery GetCusEntryQuery(bool forCurrentCountry)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);
			if (forCurrentCountry)
			{
				query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_Category,
					SQLComparisonOperator.NotEqual, CusEntryNumber.Categories.AdditionalReferenceNumber);
			return query;
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList PreCarriageOnCarriageTransportMode_List
		{
			get { return Factory.GetCachedValue<JobAddressAdditionalInfoTransportModeCodeDescriptionPairList>(); }
		}

		#region Vessel-CodeFindBox

		RefVesselCollection fRefVesselList;
		public RefVesselCollection RefVesselList
		{
			get
			{
				if (fRefVesselList == null)
				{
					fRefVesselList = new RefVesselCollection(Factory);
				}
				return fRefVesselList;
			}
		}

		#endregion

		#region OrgHeader-ZAddressControl-OrgList

		#region Sending/Receiving Forwarder Organisations

		public OrganisationsFindBoxCollection SendingForwarderList
		{
			get
			{
				if (sendingForwarderList == null)
				{
					sendingForwarderList = new ForwarderCollection(Factory, GetForwarderFilter(AgentDirectionList.Codes.Export));
					sendingForwarderList.OrganisationType = OrganisationTypes.Forwarder;
					sendingForwarderList.OrganisationSubType = OrganisationsSubTypeList.Descriptions.SendingForwarder;
					SetForwarderListDefaults(sendingForwarderList, JK_RL_NKLoadPort);
				}

				return sendingForwarderList;
			}
		}
		OrganisationsFindBoxCollection sendingForwarderList;

		public OrganisationsFindBoxCollection ReceivingForwarderList
		{
			get
			{
				if (receivingForwarderList == null)
				{
					receivingForwarderList = new ForwarderCollection(Factory, GetForwarderFilter(AgentDirectionList.Codes.Import));
					receivingForwarderList.OrganisationType = OrganisationTypes.Forwarder;
					receivingForwarderList.OrganisationSubType = OrganisationsSubTypeList.Descriptions.ReceivingForwarder;
					SetForwarderListDefaults(receivingForwarderList, JK_RL_NKDischargePort);
				}

				return receivingForwarderList;
			}
		}
		OrganisationsFindBoxCollection receivingForwarderList;

		ZDBOnlyQuery GetForwarderFilter(ZString direction)
		{
			var forwarderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			forwarderQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			forwarderQuery.AddToFilter(OrgHeaderSchema.OH_IsForwarder, true);

			if (!FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.Value)
			{
				forwarderQuery.AddSubQuery(GetAgentSubQuery(direction), JoinCondition.And);

				if (!GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
				{
					var currentBranchOrgFilter = new ZQuery(OrgHeaderSchema.PK, GlbBranch.CurrentBranch.GB_OH_OrgProxy);
					forwarderQuery.AddToFilter(currentBranchOrgFilter, JoinCondition.Or);
				}
			}

			return forwarderQuery;
		}

		ZDBOnlySubQuery GetAgentSubQuery(ZString direction)
		{
			ZDBOnlySubQuery agentQuery = new ZDBOnlySubQuery(typeof(OrgAppointedAgentPorts), OrgAppointedAgentPortsSchema.O5_OH);
			object agentStatuses = new string[] { AgentStatusList.Codes.Handles, AgentStatusList.Codes.Appointed, AgentStatusList.Codes.Published };
			switch (JK_TransportMode)
			{
				case Constants.TransportModes.Air:
					agentQuery.AddToFilter(OrgAppointedAgentPortsSchema.O5_AirAgentStatus, agentStatuses);
					break;

				case Constants.TransportModes.Rail:
					agentQuery.AddToFilter(OrgAppointedAgentPortsSchema.O5_RailAgentStatus, agentStatuses);
					break;

				case Constants.TransportModes.Road:
					agentQuery.AddToFilter(OrgAppointedAgentPortsSchema.O5_RoadAgentStatus, agentStatuses);
					break;

				case Constants.TransportModes.Sea:
					agentQuery.AddToFilter(OrgAppointedAgentPortsSchema.O5_SeaAgentStatus, agentStatuses);
					break;
			}

			if (direction != AgentDirectionList.Codes.Both)
			{
				agentQuery.AddToFilter(OrgAppointedAgentPortsSchema.O5_AgentDirection, new string[] { AgentDirectionList.Codes.Both, direction });
			}

			return agentQuery;
		}

		#region SetForwarderListDefaults

		void SetForwarderListDefaults(OrganisationsFindBoxCollection forwarderList, ZString port)
		{
			switch (JK_TransportMode)
			{
				case Constants.TransportModes.Air:
					forwarderList.DefaultsForNewChild.Add(OrgAppointedAgentPorts.Schema.O5_IsHandlesAirAgent, true);
					forwarderList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.HandlesAirFreight));
					forwarderList.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("0bdc85fe-e850-4f68-8cd4-4321c08f458c", "Forwarder must have the Handles Air Freight flag ticked."));
					break;

				case Constants.TransportModes.Rail:
					forwarderList.DefaultsForNewChild.Add(OrgAppointedAgentPorts.Schema.O5_IsHandlesRailAgent, true);
					forwarderList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.HandlesRailFreight));
					forwarderList.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("333a7e51-98b9-4f72-830d-70adfd91a48f", "Forwarder must have the Handles Rail Freight flag ticked."));
					break;

				case Constants.TransportModes.Road:
					forwarderList.DefaultsForNewChild.Add(OrgAppointedAgentPorts.Schema.O5_IsHandlesRoadAgent, true);
					forwarderList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.HandlesRoadFreight));
					forwarderList.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("f019d0ff-f7cd-4e47-a68b-4a9a1662dafb", "Forwarder must have the Handles Road Freight flag ticked."));
					break;

				case Constants.TransportModes.Sea:
					forwarderList.DefaultsForNewChild.Add(OrgAppointedAgentPorts.Schema.O5_IsHandlesSeaAgent, true);
					forwarderList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.HandlesSeaFreight));
					forwarderList.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("baeeb80a-6f52-4341-aa5b-b2267bd22f61", "Forwarder must have the Handles Sea Freight flag ticked."));
					break;
			}

			forwarderList.DefaultsForNewChild.Add(OrgAppointedAgentPorts.Schema.O5_PortOrCountry, port, true);
			forwarderList.DefaultsForNewChild.Add(OrgHeader.Schema.OH_RL_NKClosestPort, port, false);
			forwarderList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", port, true));
		}

		#endregion

		#endregion

		#region Creditor List

		OrganisationsFindBoxCollection fCreditorList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Creditor)]
		public OrganisationsFindBoxCollection CreditorList
		{
			get
			{
				if (IsCoLoad)
				{
					return CoLoadForwarderList;
				}
				else
				{
					if (fCreditorList == null)
					{
						fCreditorList = new CreditorCollection(Factory);
					}
					return fCreditorList;
				}
			}
		}

		CarrierAndOrForwarderCollection fCoLoadCarrierForwarderList;
		public CarrierAndOrForwarderCollection CoLoadForwarderList
		{
			get
			{
				if (fCoLoadCarrierForwarderList == null)
				{
					fCoLoadCarrierForwarderList = new CarrierAndOrForwarderCollection(Factory);

					if (!JK_RL_NKLoadPort.IsEmpty)
					{
						fCoLoadCarrierForwarderList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", JK_RL_NKLoadPort));
					}
				}
				return fCoLoadCarrierForwarderList;
			}
		}

		#endregion

		#region CTO Organisations

		#region OrgDepartureCtoList

		protected OrgHeaderCollection fOrgDepartureCtoList;
		public OrgHeaderCollection OrgDepartureCtoList
		{
			get
			{
				if (fOrgDepartureCtoList == null)
				{
					switch (JK_TransportMode)
					{
						case Constants.TransportModes.Air:
							fOrgDepartureCtoList = new AirCTOCollection(Factory);
							break;

						case Constants.TransportModes.Sea:
							fOrgDepartureCtoList = new SeaCTOCollection(Factory);
							break;

						default:
							fOrgDepartureCtoList = new CTOCollection(Factory);
							break;
					}

					if (!JK_RL_NKLoadPort.IsEmpty)
					{
						fOrgDepartureCtoList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", JK_RL_NKLoadPort));
					}
				}
				return fOrgDepartureCtoList;
			}
		}

		#endregion

		#region OrgArrivalCtoList

		protected OrgHeaderCollection fOrgArrivalCtoList;
		public OrgHeaderCollection OrgArrivalCtoList
		{
			get
			{
				if (fOrgArrivalCtoList == null)
				{
					switch (JK_TransportMode)
					{
						case Constants.TransportModes.Air:
							fOrgArrivalCtoList = new AirCTOCollection(Factory);
							break;

						case Constants.TransportModes.Sea:
							fOrgArrivalCtoList = new SeaCTOCollection(Factory);
							break;

						default:
							fOrgArrivalCtoList = new CTOCollection(Factory);
							break;
					}

					if (!JK_RL_NKDischargePort.IsEmpty)
					{
						fOrgArrivalCtoList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", JK_RL_NKDischargePort));
					}
				}
				return fOrgArrivalCtoList;
			}
		}

		#endregion

		#endregion

		#region Depot Organisations

		#region OrgPackDepotList

		protected PackDepotCollection fOrgPackDepotList;
		public PackDepotCollection OrgPackDepotList
		{
			get
			{
				if (fOrgPackDepotList == null)
				{
					fOrgPackDepotList = new PackDepotCollection(Factory);

					if (!JK_RL_NKLoadPort.IsEmpty)
					{
						fOrgPackDepotList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", JK_RL_NKLoadPort));
					}
				}
				return fOrgPackDepotList;
			}
		}

		#endregion

		#region OrgUnpackDepotList

		protected UnpackDepotCollection fOrgUnpackDepotList;
		public UnpackDepotCollection OrgUnpackDepotList
		{
			get
			{
				if (fOrgUnpackDepotList == null)
				{
					fOrgUnpackDepotList = new UnpackDepotCollection(Factory);

					if (!JK_RL_NKDischargePort.IsEmpty)
					{
						fOrgUnpackDepotList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", JK_RL_NKDischargePort));
					}
				}
				return fOrgUnpackDepotList;
			}
		}

		#endregion

		#endregion

		#region Container Yard Organisations

		#region OrgDepartureContainerYardList

		protected ContainerYardCollection fOrgDepartureContainerYardList;
		public ContainerYardCollection OrgDepartureContainerYardList
		{
			get
			{
				if (fOrgDepartureContainerYardList == null)
				{
					fOrgDepartureContainerYardList = new ContainerYardCollection(Factory);

					if (!JK_RL_NKLoadPort.IsEmpty)
					{
						fOrgDepartureContainerYardList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", JK_RL_NKLoadPort));
					}
				}
				return fOrgDepartureContainerYardList;
			}
		}

		#endregion

		#region OrgArrivalContainerYardList

		protected ContainerYardCollection fOrgArrivalContainerYardList;
		public ContainerYardCollection OrgArrivalContainerYardList
		{
			get
			{
				if (fOrgArrivalContainerYardList == null)
				{
					fOrgArrivalContainerYardList = new ContainerYardCollection(Factory);

					if (!JK_RL_NKDischargePort.IsEmpty)
					{
						fOrgArrivalContainerYardList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", JK_RL_NKDischargePort));
					}
				}
				return fOrgArrivalContainerYardList;
			}
		}

		#endregion

		#endregion

		#region ShippingProvider List

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public ShippingProviderCollection ShippingProviderList
		{
			get
			{
				ShippingProviderCollection result = BindingLists.ShippingProvider_List;

				switch (JK_TransportMode)
				{
					case Constants.TransportModes.Air:
						result = BindingLists.AirShippingProvider_List;
						break;

					case Constants.TransportModes.Sea:
						result = BindingLists.SeaShippingProvider_List;
						break;

					case Constants.TransportModes.Road:
						result = BindingLists.LineHaulShippingProvider_List;
						break;

					case Constants.TransportModes.Rail:
						result = BindingLists.RailShippingProvider_List;
						break;

					case Constants.TransportModes.Mail:
						result = BindingLists.AirShippingProvider_List;
						break;

					case Constants.TransportModes.Other:
						result = BindingLists.ShippingProvider_List;
						break;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Shipments_List

		ModuleShipmentCollection fShipments_List;
		public ModuleShipmentCollection Shipments_List
		{
			get
			{
				if (fShipments_List == null)
				{
					fShipments_List = GetModuleShipmentCollection();
					fShipments_List.ParentConsol = this;
					fShipments_List.Sort(CommonShipment.Schema.JS_UniqueConsignRef, System.ComponentModel.ListSortDirection.Descending);
					SetFiltersOnModuleShipmentCollection(fShipments_List);
				}
				return fShipments_List;
			}
		}

		protected virtual ModuleShipmentCollection GetModuleShipmentCollection()
		{
			return new ModuleShipmentCollection(Factory);
		}

		protected virtual void SetFiltersOnModuleShipmentCollection(ModuleShipmentCollection collection)
		{
		}

		#endregion

		#endregion

		#region PopulateFromCartage

		public void PopulateFromCartage(ICommonCartage cartage, CommonContainer[] selectedContainers)
		{
			PopulateFromCartageCore(cartage, selectedContainers);
		}

		protected virtual void PopulateFromCartageCore(ICommonCartage cartage, CommonContainer[] selectedContainers)
		{
		}

		#endregion

		#region Related Business Objects

		#region Vessel

		public RefVessel Vessel
		{
			get { return Transports.MostInterestingTransport?.Vessel; }
		}

		#endregion

		#region Voyage

		public JobVoyage Voyage
		{
			get
			{
				JobSailing schedule = this.Schedule;
				if (schedule != null)
				{
					schedule.Voyage.ParentConsol = this;
					return schedule == null ? null : schedule.Voyage;
				}
				return null;
			}
		}

		#endregion

		#region VoyOrigin

		public VoyageOrigin VoyOrigin
		{
			get
			{
				JobSailing schedule = this.Schedule;
				return schedule == null ? null : schedule.Origin;
			}
		}

		#endregion

		#region VoyDestination

		public VoyageDestination VoyDestination
		{
			get
			{
				JobSailing schedule = this.Schedule;
				return schedule == null ? null : schedule.Destination;
			}
		}

		#endregion

		#region Schedule

		public JobSailing Schedule
		{
			get { return Factory.Load<JobSailing>(JK_JX_Sailing); }
		}

		#endregion

		#region Containers

		[ChildEditable(false)]
		public virtual CommonContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = GetNewContainerCollection();
					fContainers.Load();
					fContainers.Sort(JobContainerSchema.JC_ContainerNum.Name, ListSortDirection.Ascending);
					if (ChildEditableService.GetState(Factory) == ChildEditableServiceStates.Consol)
					{
						RegisterEditableChildObject(fContainers, (NoResString)"Containers");
					}
					OnContainersCreated();
				}

				return fContainers;
			}
		}

		protected virtual void OnContainersCreated()
		{
		}

		protected virtual CommonContainerCollection GetNewContainerCollection()
		{
			return new CommonContainerCollection(this, Factory);
		}

		CommonContainerCollection fContainers;

		#endregion

		#region Transports

		[ChildEditable(false)]
		public virtual ConsolTransportCollection Transports
		{
			get
			{
				if (fTransports == null)
				{
					using (SuspendSettingHasChanges())
					using (GetValidationSuspender())
					{
						fTransports = new ConsolTransportCollection(this);
#if DEBUG
		BeforeTransportsLoad?.Invoke(this);
#endif
						fTransports.Load();
						RegisterEditableChildObject(fTransports, (NoResString)"Routing");    // Name identifier; both Transports & TransportsIncludingRelated should be readonly when "Routing" is selected in registry
						fTransports.DepartureTransport.JW_TerminalCutOffInfo.ValueChanged += new EventHandler(JW_TerminalCutOffInfo_ValueChanged);
						fTransports.DepartureTransport.JW_DepotCutOffInfo.ValueChanged += new EventHandler(JW_DepotCutOffInfo_ValueChanged);
						fTransports.CountChanged += new CollectionCountChangedEventHandler(OnTransports_CountChanged);
					}

					IsTransportsLoaded = true;
				}

				return fTransports;
			}
		}

		ConsolTransportCollection fTransports;
		bool IsTransportsLoaded;
#if DEBUG
		public delegate void DelegateForBeforeTransportsLoad(CommonConsol currentConsol);
		public DelegateForBeforeTransportsLoad BeforeTransportsLoad { get; set; }
#endif

		void OnTransports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			Containers.Cast<CommonContainer>().Where(container => !container.IsDeleted).ForEach(x => x.Validation.ValidateJC_GrossWeightVerificationType());
		}

		public ZBool ConsolPassesThroughCountry(ZString country)
		{
			bool result = false;

			if (JK_RL_NKDischargePort.Left(2) == country || JK_RL_NKLoadPort.Left(2) == country)
			{
				result = true;
			}
			else
			{
				foreach (Transport transport in Transports)
				{
					if ((transport.JW_RL_NKLoadPort.Left(2) == country) || (transport.JW_RL_NKDiscPort.Left(2) == country))
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Gets an export routing leg for the specified country.
		/// </summary>
		/// <param name="country">The 2 letter code of the country.</param>
		/// <returns>Export transport, if found; otherwise null.</returns>
		public Transport GetExportTransport(ZString country)
		{
			Transport result = null;
			foreach (Transport transport in Transports)
			{
				if (transport.JW_RL_NKLoadPort.StartsWith(country) &&
					!transport.JW_RL_NKDiscPort.StartsWith(country))
				{
					result = transport;
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Gets an import routing leg for the specified country.
		/// </summary>
		/// <param name="country">The 2 letter code of the country.</param>
		/// <returns>Import transport, if found; otherwise null.</returns>
		public Transport GetImportTransport(ZString country)
		{
			Transport result = null;
			foreach (Transport transport in Transports)
			{
				if (transport.JW_RL_NKDiscPort.StartsWith(country) &&
					!transport.JW_RL_NKLoadPort.StartsWith(country))
				{
					result = transport;
					break;
				}
			}
			return result;
		}

		#endregion

		#region ResetContainerTrainWagonNumberForNonRail

		void ResetContainerTrainWagonNumberForNonRail()
		{
			if (JK_TransportMode != Constants.TransportModes.Rail)
			{
				foreach (CommonContainer container in Containers)
				{
					container.JC_TrainWagonNumber = "";
				}
			}
		}

		#endregion

		#region Shipments

		/// <summary>
		/// This is the main CommonShipment collection used as base for the others.
		/// </summary>
		ConsolShipmentCollection fShipments;

		[ChildEditable(false)]
		public virtual ConsolShipmentCollection Shipments
		{
			get
			{
				if (fShipments == null)
				{
					fShipments = GetNewConsolShipmentCollection();
					fShipments.Load();
					fShipments.CountChanged += new CollectionCountChangedEventHandler(OnShipments_CountChanged);
					if (ChildEditableService.GetState(Factory) == ChildEditableServiceStates.Consol)
					{
						RegisterEditableChildObject(fShipments, (NoResString)"Shipments");
					}
					AfterConsolShipmentCollectionCreated();
				}
				return fShipments;
			}
		}

		public IDisposable SuspendShipmentsCountChanged()
		{
			return new ShipmentsCountChangedSuspender(this);
		}

		#region Suspending Shipments Related Events

		class ShipmentsCountChangedSuspender : IDisposable
		{
			public ShipmentsCountChangedSuspender(CommonConsol consol)
			{
				this.consol = consol;
				shipmentsCountChangedSuspender = consol.Shipments.SuspendCountChanged(consol.ShimentsCountChangedSuspendedEventsProcessor);
				shipmentsForTotallingCountChangedSuspender = consol.ShipmentsForTotalling.SuspendCountChanged(null);
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in ShipmentsCountChangedSuspender")]
			readonly CommonConsol consol;
			readonly IDisposable shipmentsCountChangedSuspender;
			readonly IDisposable shipmentsForTotallingCountChangedSuspender;
			bool disposed;

			public void Dispose()
			{
				if (!disposed)
				{
					try
					{
						shipmentsCountChangedSuspender.Dispose();
						shipmentsForTotallingCountChangedSuspender.Dispose();
					}
					finally
					{
						DisposableLeakListener.Instance.UnRegisterDisposable(this);
						disposed = true;
					}
				}
			}
		}

		#endregion

		void ShimentsCountChangedSuspendedEventsProcessor(IEnumerable<CollectionCountChangedEventArgs> eventArgs)
		{
			if (eventArgs.Any())
			{
				foreach (var eventArg in eventArgs)
				{
					var shipment = (CommonShipment)eventArg.BizObject;
					if (eventArg.ItemAdded)
					{
						RefreshRelatedPackLinesWhenShipmentIsAdded(shipment, false);
					}
					else
					{
						RefreshRelatedPackLinesWhenShipmentIsRemoved(shipment, false);
					}
				}

				RebuildUnAllocatedPackLines();

				if (eventArgs.Any(ea => ea.ItemRemoved))
				{
					RefreshShipmentsForTotalling();
				}

				OnShipmentsForTotalling_CountChanged(null, null);
			}
		}

		void OnShipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CommonShipment shipment = (CommonShipment)e.BizObject;

			if (e.ItemAdded)
			{
				RefreshRelatedPackLinesWhenShipmentIsAdded(shipment);
			}
			else // item is removed
			{
				foreach (CommonContainer container in Containers.ToList())
				{
					container.UnpackPackLinesFrom(shipment);
				}

				RefreshRelatedPackLinesWhenShipmentIsRemoved(shipment);
				RefreshShipmentsForTotalling();
			}
		}

		protected virtual ConsolShipmentCollection GetNewConsolShipmentCollection()
		{
			return new ConsolShipmentCollection(this);
		}

		protected virtual void AfterConsolShipmentCollectionCreated()
		{
		}

		/// <summary>
		/// This CommonShipment collection is used for total calculations.
		/// It should always exclude sub coload shipments.
		/// </summary>
		public TopLevelShipmentCollection TopLevelShipments
		{
			get { return topLevelShipments ?? (topLevelShipments = GetNewTopLevelShipmentCollection()); }
		}
		TopLevelShipmentCollection topLevelShipments;

		protected void ResetTotalsAndTopLevels()
		{
			topLevelShipments = null;
			shipmentsForTotalling = null;
			fGridShipments = null;
		}

		/// <summary>
		/// This CommonShipment collection is used for total calculations.
		/// It should always exclude Co-Load and Assembly sub-shipments.
		/// </summary>
		ShipmentsForTotallingCollection shipmentsForTotalling;
		public ShipmentsForTotallingCollection ShipmentsForTotalling
		{
			get
			{
				if (shipmentsForTotalling == null)
				{
					shipmentsForTotalling = GetNewShipmentsForTotallingCollection();
					shipmentsForTotalling.CountChanged += OnShipmentsForTotalling_CountChanged;
				}
				return shipmentsForTotalling;
			}
		}

		protected virtual ShipmentsForTotallingCollection GetNewShipmentsForTotallingCollection()
		{
			return new ShipmentsForTotallingCollection(Shipments, this);
		}

		void RefreshShipmentsForTotalling()
		{
			if (shipmentsForTotalling != null)
			{
				shipmentsForTotalling.Rebuild();
			}
		}

		public virtual bool IsLinkedTo(CommonShipment shipment)
		{
			return shipment.Consols.Contains(this);
		}

		void OnShipmentsForTotalling_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			JK_TotalShipmentQuantityInfo.RefreshBinding();
			JK_TotalShipmentWeightInfo.RefreshBinding();
			JK_TotalShipmentVolumeInfo.RefreshBinding();
			JK_TotalShipmentLoadingMetersInfo.RefreshBinding();
			JK_TotalShipmentChargeableInfo.RefreshBinding();
			JK_Calc_FreeSpaceInfo.RefreshBinding();
			JK_Calc_ActualVolumeWeightInfo.RefreshBinding();
			PrePaidShipments.RefreshBinding();
			CollectShipments.RefreshBinding();
		}

		/// <summary>
		/// This CommonShipment collection changes dynamically to:
		///   - show all Shipments
		///   - show only TopLevel Shipments
		/// Based on the ShowSubHouseBillShipments property.
		/// </summary>
		TopLevelShipmentCollection fGridShipments;
		public TopLevelShipmentCollection GridShipments
		{
			get
			{
				if (fGridShipments == null)
				{
					fGridShipments = GetNewTopLevelShipmentCollection();
					fGridShipments.ShouldShowChildShipments = ShowSubHouseBillShipments;
				}
				return fGridShipments;
			}
		}

		protected virtual TopLevelShipmentCollection GetNewTopLevelShipmentCollection()
		{
			return new TopLevelShipmentCollection(Shipments, this);
		}

		/// <summary>
		/// Collection of Shipments on this consol that are CoLoads (ie have a master)
		/// </summary>
		public CoLoadShipmentCollectionView CoLoadShipments
		{
			get
			{
				if (fCoLoadShipments == null)
				{
					fCoLoadShipments = new CoLoadShipmentCollectionView(Shipments, this);
					fCoLoadShipments.IsManagedForDataRefresh = true;
				}
				return fCoLoadShipments;
			}
		}
		CoLoadShipmentCollectionView fCoLoadShipments;

		/// <summary>
		/// This CommonShipment collection view is based on ShipmentsForTotalling filtered to return only Prepaid.
		/// It is used for total calculations.
		/// </summary>
		protected ShipmentCollectionView PrePaidShipments
		{
			get { return prePaidShipments ?? (prePaidShipments = new ShipmentCollectionView(ShipmentsForTotalling, (shipment) => shipment.IsPrepaid)); }
		}
		ShipmentCollectionView prePaidShipments;

		/// <summary>
		/// This CommonShipment collection view is based on ShipmentsForTotalling filtered to return only Collect shipments.
		/// It is used for total calculations.
		/// </summary>
		protected ShipmentCollectionView CollectShipments
		{
			get { return collectShipments ?? (collectShipments = new ShipmentCollectionView(ShipmentsForTotalling, (shipment) => shipment.IsCollect)); }
		}
		ShipmentCollectionView collectShipments;

		/// <summary>
		/// Returns true if any sub (co-loaded) shipments (possibly hidden in the grid) have errors.
		/// </summary>
		public bool SubShipmentsHaveErrors
		{
			get { return CoLoadShipments.Any(subShipment => subShipment.HasErrors); }
		}

		#endregion

		#region MostInterestingTransportForBinding

		public MostInterestingTransportBindingCollection MostInterestingTransportForBinding
		{
			get
			{
				if (mostInterestingTransportForBinding == null)
				{
					mostInterestingTransportForBinding = new MostInterestingTransportBindingCollection(this);
					OnMostInterestingTransportBindingCollectionCreated();
				}
				return mostInterestingTransportForBinding;
			}
		}

		MostInterestingTransportBindingCollection mostInterestingTransportForBinding;

		protected virtual void OnMostInterestingTransportBindingCollectionCreated()
		{
		}

		#endregion

		#region MostInterestingTransportDescription

		[MaxLength(50)]
		public ZString MostInterestingTransportDescription
		{
			get
			{
				ZString descriptionForMostInterestingTransport;

				if (Transports == null || Transports.MostInterestingTransport == null)
				{
					return string.Empty;
				}

				switch (Transports.MostInterestingTransport.JW_TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						descriptionForMostInterestingTransport = Res.GetString("Freight.TransportDescription.Flight", "Flight");
						break;

					case Core.Constants.TransportModes.Road:
					case Core.Constants.TransportModes.Rail:
						descriptionForMostInterestingTransport = Res.GetString("Freight.TransportDescription.Journey", "Journey");
						break;

					case Core.Constants.TransportModes.Sea:
					default:
						descriptionForMostInterestingTransport = Res.GetString("Freight.TransportDescription.Voyage", "Voyage");
						break;
				}

				if (Transports.Count > 1)
				{
					ZString legDescription = ZString.Empty;

					TransportOrderHelper orderHelper = new TransportOrderHelper(Transports);

					if (ImportExportHelper.IsImport(JK_RL_NKLoadPort, JK_RL_NKDischargePort))
					{
						if (orderHelper.LastLegWithTransportMode(JK_TransportMode, null, ZString.Empty) != null)
						{
							legDescription = Res.GetString("99b3e4de-3db6-419b-9161-11a3d0409fab", "Last '{0}' Leg", JK_TransportMode);
						}
						else
						{
							legDescription = Res.GetString("503b1780-857a-46b4-8bda-f856213bfa6a", "Arrival Leg");
						}
					}
					else
					{
						if (orderHelper.FirstLegWithTransportMode(JK_TransportMode) != null)
						{
							legDescription = Res.GetString("998a7e76-d058-4cb4-9979-b2e5fe55cf12", "First '{0}' Leg", JK_TransportMode);
						}
						else
						{
							legDescription = Res.GetString("51b570fc-df45-4f02-9788-627b5090ab9b", "Departure Leg");
						}
					}

					if (!legDescription.IsEmpty)
					{
						descriptionForMostInterestingTransport += " (" + legDescription + ")";
					}
				}

				return descriptionForMostInterestingTransport;
			}
		}

		public ZPropertyInfo MostInterestingTransportDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(MostInterestingTransportDescription)); }
		}

		#endregion

		#region Job

		public JobHeader Job
		{
			get
			{
				if (fJob == null || fJob.IsDeleted)
				{
					var jobHeaderParent = this as IJobHeaderParent;
					if (jobHeaderParent != null)
					{
						fJob = new JobHeader.Loader(jobHeaderParent).Load();
					}
				}
				return fJob;
			}
		}
		JobHeader fJob;

		#endregion

		#region Numbers

		/// <summary>
		/// Additional Customs References
		/// </summary>
		[ChildEditable(true)]
		public CusEntryNumAdditionalReferenceCollection Numbers
		{
			get
			{
				if (fNumbers == null)
				{
					fNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					fNumbers.Load();
					RegisterEditableChildObject(fNumbers);
					OnNumbersLoaded();
					RaiseNumbersLoaded();
				}
				return fNumbers;
			}
		}

		CusEntryNumAdditionalReferenceCollection fNumbers;

		public ZString NumbersAsString => Numbers.AllNumbersAsString;

		public event EventHandler NumbersLoaded;

		void RaiseNumbersLoaded()
		{
			if (NumbersLoaded != null)
			{
				NumbersLoaded(this, EventArgs.Empty);
			}
		}

		protected virtual void OnNumbersLoaded()
		{
			foreach (var cusEntryNumber in Numbers.OfType<CusEntryNumber>())
			{
				if (AdditionalReferenceNumberCannotBeDeleted(cusEntryNumber))
				{
					AddCannotDeleteNumberHandler(cusEntryNumber, ResString.GetMultilingualString("9132b955-8b67-4f17-9d58-3e81575c6498", "The {0} is system generated and cannot be deleted.", cusEntryNumber.CE_EntryType));
				}
			}
		}

		protected virtual bool AdditionalReferenceNumberCannotBeDeleted(CusEntryNumber cusEntryNumber)
		{
			return cusEntryNumber.CE_EntryIsSystemGenerated && cusEntryNumber.CE_EntryType == CusEntryNumLookups.HIR;
		}

		protected void AddCannotDeleteNumberHandler(CusEntryNumber number, MultilingualString reasonForNotAbleToDelete)
		{
			if (number != null)
			{
				number.ReadOnly = true;
				number.CanDeleteHandler += (s, eventArgs) =>
				{
					eventArgs.CanDelete = false;
					eventArgs.ReasonForNotAbleToDelete = reasonForNotAbleToDelete;
				};
			}
		}

		#endregion

		#region Pack Lines

		public PackLineNonDependentCollection RelatedPackLines
		{
			get
			{
				if (relatedPackLines == null)
				{
					relatedPackLines = new PackLineNonDependentCollection(Factory);
					foreach (CommonShipment shipment in GetShipmentsWithRelatedPacklines())
					{
						shipment.OuterPackLines.CountChanged -= new CollectionCountChangedEventHandler(ShipmentsOuterPackLines_CountChanged);
						this.relatedPackLines.AddRange(shipment.OuterPackLines);
						shipment.OuterPackLines.CountChanged += new CollectionCountChangedEventHandler(ShipmentsOuterPackLines_CountChanged);
					}
				}

				return relatedPackLines;
			}
		}
		PackLineNonDependentCollection relatedPackLines;

		protected virtual IEnumerable<CommonShipment> GetShipmentsWithRelatedPacklines()
		{
			return Shipments.Cast<CommonShipment>();
		}

		void ShipmentsOuterPackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			PackLine packLine = e.BizObject as PackLine;
			if (packLine != null)
			{
				if (e.ItemAdded && this.Shipments.Any(x => x.PK == packLine.JL_JS))
				{
					RelatedPackLines.Add(packLine);
				}
				else if (e.ItemRemoved && RelatedPackLines.Contains(packLine))
				{
					RelatedPackLines.Remove(packLine);
				}
			}

			RebuildUnAllocatedPackLines();
		}

		[ChildEditable(true)]
		public UnAllocatedPackLinesView UnAllocatedPackLines
		{
			get
			{
				if (this.unAllocatedPackLines == null)
				{
					this.unAllocatedPackLines = CreateUnAllocatedPackLines();
					this.unAllocatedPackLines.Sort(PackLine.Schema.JL_JS_HouseBill, ListSortDirection.Descending);
					this.unAllocatedPackLines.ShowOnlyReceived = false;
					RegisterEditableChildObject(this.unAllocatedPackLines);
				}

				return this.unAllocatedPackLines;
			}
		}
		protected UnAllocatedPackLinesView unAllocatedPackLines;

		protected void RebuildUnAllocatedPackLines()
		{
			if (this.unAllocatedPackLines != null)
			{
				this.unAllocatedPackLines.Rebuild();
			}
		}

		protected virtual UnAllocatedPackLinesView CreateUnAllocatedPackLines()
		{
			return new ConsolUnAllocatedPackLinesView(this, RelatedPackLines);
		}

		protected void RefreshRelatedPackLinesWhenShipmentIsRemoved(CommonShipment shipment, bool shouldRebuildUnAllocatedPackLines = true)
		{
			if (this.relatedPackLines != null && shipment.OuterPackLines.Count > 0 && !shipment.IsMasterShipmentRepresentingAllChildShipments)
			{
				bool anyPackLinesRemoved = false;

				for (int i = shipment.OuterPackLines.Count - 1; i >= 0; i--)
				{
					if (RelatedPackLines.Contains(shipment.OuterPackLines[i]))
					{
						RelatedPackLines.Remove(shipment.OuterPackLines[i]);
						anyPackLinesRemoved = true;
					}
				}

				if (anyPackLinesRemoved && shouldRebuildUnAllocatedPackLines)
				{
					RebuildUnAllocatedPackLines();
				}
			}

			shipment.OuterPackLines.CountChanged -= new CollectionCountChangedEventHandler(ShipmentsOuterPackLines_CountChanged);
		}

		protected void RefreshRelatedPackLinesWhenShipmentIsAdded(CommonShipment shipment, bool shouldRebuildUnAllocatedPackLines = true)
		{
			shipment.OuterPackLines.CountChanged -= new CollectionCountChangedEventHandler(ShipmentsOuterPackLines_CountChanged);
			if (this.relatedPackLines != null)
			{
				if (shipment.OuterPackLines.Count > 0)
				{
					RelatedPackLines.AddRange(shipment.OuterPackLines);
					if (shouldRebuildUnAllocatedPackLines)
					{
						RebuildUnAllocatedPackLines();
					}
				}

				shipment.OuterPackLines.CountChanged += new CollectionCountChangedEventHandler(ShipmentsOuterPackLines_CountChanged);
			}
		}

		#endregion

		public INctsHeader NctsHeaderForDocuments
		{
			get
			{
				if (nctsHeaderForDocuments == null || !IsTargetBranchCompanyEqualToCurrentCompany(nctsHeaderForDocuments.BH_GB))
				{
					var linkedNctsHeaders = LoadNctsHeaders(Factory, !IsDeleted && !JK_IsCancelled);
					nctsHeaderForDocuments = linkedNctsHeaders.FirstOrDefault(nctsHeader => nctsHeader is IDocumentSupportable && IsTargetBranchCompanyEqualToCurrentCompany(nctsHeader.BH_GB));
				}

				return nctsHeaderForDocuments;
			}
		}
		INctsHeader nctsHeaderForDocuments;

		bool IsTargetBranchCompanyEqualToCurrentCompany(ZGuid branchPk)
		{
			var branch = Factory.Load<GlbBranch>(branchPk);
			return branch != null && branch.GB_GC == GlbCompany.CurrentCompany.PK;
		}

		protected INctsHeader[] LoadNctsHeaders(BusinessObjectFactory factory, bool activeOnly)
		{
			var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, PK);
			query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, new[] { CusInBondApplicationCodeList.Codes.NCTS4, CusInBondApplicationCodeList.Codes.NCTS5 } );
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.IgnoreActiveFilter = !activeOnly;
			return factory.Load<INctsHeader>(query);
		}

		public Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader[] GetGlobalManifestHeaders()
		{
			var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, PK);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, TablePrefix);
			query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, Enterprise.Integration.Customs.ASYCUDA.ApplicationCodeTypes.Consolidator);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			return Factory.Load<Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader>(query);
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList JK_PrepaidCollect_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.PaymentType); }
		}

		public CodeDescriptionPairList JK_TransportMode_List
		{
			get { return Factory.GetCachedValue<ConsolTransportModeCodeDescriptionPairList>(); }
		}

		public CodeDescriptionPairList JK_ConsolMode_List
		{
			get
			{
				return Factory.GetCachedValue("FreightCodePairLists.ConsolModeList_" + JK_TransportMode + "_" + JK_AgentType,
					() => FreightCodePairLists.ConsolModeList(JK_AgentType, JK_TransportMode));
			}
		}

		public RefCommodityCodeCollection JK_RH_NKConsolCommodity_List
		{
			get { return Factory.GetCachedValue("ConsolCommodityList", () => new RefCommodityCodeCollection(Factory)); }
		}

		public CodeDescriptionPairList JK_AgentType_List
		{
			get
			{
				return FreightCodePairLists.AgentTypeList(IsAir, Factory);
			}
		}

		public CodeDescriptionPairList JK_Calc_ShipmentWeightUnit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList JK_Calc_ShipmentVolumeUnit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList JK_PrintOptionForColoads_List
		{
			get
			{
				return Factory.GetCachedValue(
					"CommonConsol.JK_PrintOptionForColoads_List",
					delegate
					{
						CodeDescriptionPairList list = new CodeDescriptionPairList();
						list.AddPair(FreightConstants.PrintOptionForCoLoads.All, Res.GetString("c05c2ad3-1b5c-40b3-8791-8b8b4f0460aa", "Print Masters and Sub House bills"));
						list.AddPair(FreightConstants.PrintOptionForCoLoads.MastersOnly, Res.GetString("fff2d13f-f15a-4de8-894c-123adfabc9c9", "Print Masters only"));
						list.AddPair(FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly, Res.GetString("c16fa1b0-db71-4344-bbe3-6c92153fc4b0", "Print Sub House bills only"));
						return list;
					});
			}
		}

		#region JK_ElectronicBillOfLadingType_List

		public CodeDescriptionPairList JK_ElectronicBillOfLadingType_List => Factory.GetCachedValue("FreightCodePairLists.BillOfLadingBillTypeList", FreightCodePairLists.BillOfLadingBillTypeList);

		#endregion

		#region JK_ElectronicBillOfLadingTerms_List

		public CodeDescriptionPairList JK_ElectronicBillOfLadingTerms_List => Factory.GetCachedValue("FreightCodePairLists.BillOfLadingBillTermsList", FreightCodePairLists.BillOfLadingBillTermsList);

		#endregion

		#region BindingLists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region RefUNLOCO_List

		public RefUNLOCOCollection RefUNLOCO_List
		{
			get { return BindingLists.RefUNLOCO_List; }
		}

		#endregion

		#region OrgHeader_List

		public OrgHeaderCollection OrgHeader_List
		{
			get { return BindingLists.OrgHeader_List; }
		}

		#endregion

		#region JK_OA_List

		public OrgAddressCollection JK_OA_List
		{
			get { return BindingLists.OrgAddress_List; }
		}

		#endregion

		#region JK_PackageUnit_List

		public CodeDescriptionPairList JK_PackageUnit_List
		{
			get
			{
				return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length);
			}
		}

		#endregion

		#region RefCurrency_List

		public RefCurrencyCollection RefCurrency_List
		{
			get { return BindingLists.RefCurrency_List; }
		}

		#endregion

		#region JK_RS_List

		public RefServiceLevelCollection JK_RS_List
		{
			get { return BindingLists.RefServiceLevel_List; }
		}

		#endregion

		#region RefVessel_List

		public RefVesselCollection RefVessel_List
		{
			get { return BindingLists.RefVessel_List; }
		}

		#endregion

		#region JobSailingList

		protected JobSailingCollection fJobSailingList;
		public JobSailingCollection JobSailingList
		{
			get
			{
				if (fJobSailingList == null)
				{
					fJobSailingList = new JobSailingCollection(Factory);
				}
				return fJobSailingList;
			}
		}

		#endregion

		#endregion

		#region Weight/Volume/Chargeable/Loading Meters for documents

		public ZDecimal GetTotalShipmentWeightForDoc(string displayType)
		{
			return GetTotalShipmentWeightWithScaleForDoc(displayType).Item1;
		}

		public Tuple<ZDecimal, ZByte> GetTotalShipmentWeightWithScaleForDoc(string displayType)
		{
			return GetMeasureForDoc(displayType, JK_TotalDocumentedWeight, JobShipmentSchema.JS_DocumentedWeight.Scale,
									JK_TotalManifestedWeight, JobShipmentSchema.JS_ManifestedWeight.Scale,
									JK_CorrectedConsolWeight, JobShipmentSchema.JS_ActualWeight.Scale);
		}

		public ZDecimal GetTotalShipmentVolumeForDoc(string displayType)
		{
			return GetTotalShipmentVolumeWithScaleForDoc(displayType).Item1;
		}

		public Tuple<ZDecimal, ZByte> GetTotalShipmentVolumeWithScaleForDoc(string displayType)
		{
			return GetMeasureForDoc(displayType, JK_TotalDocumentedVolume, JobShipmentSchema.JS_DocumentedVolume.Scale,
									JK_TotalManifestedVolume, JobShipmentSchema.JS_ManifestedVolume.Scale,
									JK_CorrectedConsolVolume, JobShipmentSchema.JS_ActualVolume.Scale);
		}

		public ZDecimal GetTotalShipmentChargeableForDoc(string displayType)
		{
			return GetTotalShipmentChargeableWithScaleForDoc(displayType).Item1;
		}

		public Tuple<ZDecimal, ZByte> GetTotalShipmentChargeableWithScaleForDoc(string displayType)
		{
			return GetMeasureForDoc(displayType, JK_TotalDocumentedChargeable, JobShipmentSchema.JS_DocumentedChargeable.Scale,
									JK_TotalManifestedChargeable, JobShipmentSchema.JS_ManifestedChargeable.Scale,
									JK_ConsolChargeable, JobShipmentSchema.JS_ActualChargeable.Scale);
		}

		public ZDecimal GetTotalShipmentLoadingMetersForDoc(string displayType)
		{
			return GetTotalShipmentLoadingMetersWithScaleForDoc(displayType).Item1;
		}

		public Tuple<ZDecimal, ZByte> GetTotalShipmentLoadingMetersWithScaleForDoc(string displayType)
		{
			return GetMeasureForDoc(displayType, JK_TotalDocumentedLoadingMeters, JobShipmentSchema.JS_DocumentedLoadingMeters.Scale,
									JK_TotalManifestedLoadingMeters, JobShipmentSchema.JS_ManifestedLoadingMeters.Scale,
									JK_TotalShipmentLoadingMeters, JobShipmentSchema.JS_LoadingMeters.Scale);
		}

		Tuple<ZDecimal, ZByte> GetMeasureForDoc(string displayType, ZDecimal documented, ZByte documentsScale, ZDecimal manifested, ZByte manifesteScale, ZDecimal actual, ZByte actualScale)
		{
			switch (displayType)
			{
				case Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Client:
					return new Tuple<ZDecimal, ZByte>(documented, documentsScale);
				case Enterprise.Core.WeightAndVolumeDisplayTypes.Codes.Carrier:
					return new Tuple<ZDecimal, ZByte>(manifested, manifesteScale);
				default:
					return new Tuple<ZDecimal, ZByte>(actual, actualScale);
			}
		}

		#endregion

		#region Sailing Field Changed Notifications

		public bool ShouldDefaultFlightDetailsFrom(Transport transport)
		{
			return JK_TransportMode == Core.Constants.TransportModes.Air
				&& transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1;
		}

		#endregion

		#region Default Address Type

		protected override ZAddress GetNewJK_OA_DepartureCTOAddress_ZAddress()
		{
			var result = base.GetNewJK_OA_DepartureCTOAddress_ZAddress();
			result.DefaultAddressType = AddressType.PIC;
			return result;
		}

		protected override ZAddress GetNewJK_OA_PackDepotAddress_ZAddress()
		{
			var result = base.GetNewJK_OA_PackDepotAddress_ZAddress();
			result.DefaultAddressType = AddressType.PIC;
			return result;
		}

		protected override ZAddress GetNewJK_OA_ContainerYardEmptyPickupAddress_ZAddress()
		{
			var result = base.GetNewJK_OA_ContainerYardEmptyPickupAddress_ZAddress();
			result.DefaultAddressType = AddressType.PIC;
			return result;
		}

		protected override ZAddress GetNewJK_OA_ArrivalCTOAddress_ZAddress()
		{
			var result = base.GetNewJK_OA_ArrivalCTOAddress_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			return result;
		}

		protected override ZAddress GetNewJK_OA_UnpackDepotAddress_ZAddress()
		{
			var result = base.GetNewJK_OA_UnpackDepotAddress_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			return result;
		}

		protected override ZAddress GetNewJK_OA_ContainerYardEmptyReturnAddress_ZAddress()
		{
			var result = base.GetNewJK_OA_ContainerYardEmptyReturnAddress_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			return result;
		}

		#endregion

		#region Schedule Methods

		#region DefaultDepartureCTOAddressFromSailing

		public void DefaultDepartureCTOAddressFromSailing(JobSailing sailing)
		{
			if (sailing == null)
			{
				if (IsDepartureCTOAddressDefaultedFromVoyage)
				{
					JK_OA_DepartureCTOAddress = ZGuid.Empty;
					IsDepartureCTOAddressDefaultedFromVoyage = false;
				}
			}
			else
			{
				IsDepartureCTOAddressDefaultedFromVoyage = false;
				if (sailing != null && sailing.Origin != null && sailing.Origin.DepartureCTOAddress != null && !JK_OA_DepartureCTOAddress.IsValid)
				{
					JK_OA_DepartureCTOAddress = sailing.Origin.JA_OA_DepartureCTOAddress;
					IsDepartureCTOAddressDefaultedFromVoyage = true;
					IsDepartureCTOAddressDefaultedFromCarrier = false;
				}
			}
		}

		bool IsDepartureCTOAddressDefaultedFromVoyage;

		#endregion

		#region DefaultArrivalCTOAddressFromSailing

		public void DefaultArrivalCTOAddressFromSailing(JobSailing sailing)
		{
			if (sailing == null)
			{
				if (IsArrivalCTOAddressDefaultedFromVoyage)
				{
					JK_OA_ArrivalCTOAddress = ZGuid.Empty;
					IsArrivalCTOAddressDefaultedFromVoyage = false;
				}
			}
			else
			{
				IsArrivalCTOAddressDefaultedFromVoyage = false;
				if (sailing != null && sailing.Destination != null && sailing.Destination.ArrivalCTOAddress != null && !JK_OA_ArrivalCTOAddress.IsValid)
				{
					JK_OA_ArrivalCTOAddress = sailing.Destination.JB_OA_ArrivalCTOAddress;
					IsArrivalCTOAddressDefaultedFromVoyage = true;
					IsArrivalCTOAddressDefaultedFromCarrier = false;
				}
			}
		}

		bool IsArrivalCTOAddressDefaultedFromVoyage;

		#endregion

		#endregion

		#region Implementation

		void JW_DepotCutOffInfo_ValueChanged(object sender, EventArgs e)
		{
			if (JK_MasterBillIssueDate.IsEmpty && FreightConfigurationRegistry.Instance.AWBIssueDate.Value == FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate &&
				JK_ConsolMode != Constants.ContainerModes.FCL)
			{
				JK_MasterBillIssueDate = Transports.DepartureTransport.JW_DepotCutOff;
			}
		}

		void JW_TerminalCutOffInfo_ValueChanged(object sender, EventArgs e)
		{
			if (JK_MasterBillIssueDate.IsEmpty && FreightConfigurationRegistry.Instance.AWBIssueDate.Value == FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate &&
				JK_ConsolMode == Constants.ContainerModes.FCL)
			{
				JK_MasterBillIssueDate = Transports.DepartureTransport.JW_TerminalCutOff;
			}
		}

		decimal CalculateChargeable(decimal totalWeight, string weightUnit, decimal totalVolume, string volumeUnit, decimal totalLoadingMeters)
		{
			return ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Weight = new ZWeight(totalWeight, weightUnit),
				Volume = new ZVolume(totalVolume, volumeUnit),
				LoadingLength = new Quantity(totalLoadingMeters, Constants.LoadingLength.LoadingMeters),
				TargetUnit = JK_ConsolChargeableUnit,
				ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(IsDomesticFreight, JK_TransportMode, JK_ConsolChargeableUnit)
			}).Chargeable.Amount;
		}

		public void AllocatePackLine(PackLine line)
		{
			if (line == null || Containers.Count == 0 || line.GetContainer(this) != null)
			{
				return;
			}

			var wasLineAllocated = false;

			if (CheckCanAllocateShipmentsToContainers())
			{
				var container = FindContainerWithCapacity(line.JL_ActualVolume);
				if (container != null)
				{
					AllocatePackLineToContainer(line, container);
					wasLineAllocated = true;
				}
			}

			if (!wasLineAllocated)
			{
				var emptyContainer = Containers.OfType<CommonContainer>().LastOrDefault(x => !x.JC_IsEmptyContainer);
				if (emptyContainer != null)
				{
					AllocatePackLineToContainer(line, emptyContainer);
				}
			}

			line.RefreshBinding();
		}

		public CommonContainer GetAvailableContainer(CommonShipment shipment)
		{
			var container = FindContainerWithCapacity(shipment.JS_ActualVolume);
			if (container != null)
			{
				return container;
			}

			return Containers.ToArray<CommonContainer>().LastOrDefault(x => !x.JC_IsEmptyContainer);
		}

		CommonContainer FindContainerWithCapacity(ZDecimal requiredCapacity)
		{
			foreach (var container in Containers.Cast<CommonContainer>()
				.Where(x => !x.JC_IsEmptyContainer)
				.OrderByDescending(x => x.JC_Calc_ContainerCapacity))
			{
				var newPotentialVolume = requiredCapacity + container.JC_Calc_TotalVolume;
				if (container.JC_Calc_ContainerCapacity >= newPotentialVolume)
				{
					return container;
				}
			}

			return null;
		}

		void AllocatePackLineToContainer(PackLine line, CommonContainer container)
		{
			if (!this.IsSavingFromPlanningBoard)
			{
				container.AdjustOverriddenGrossWeightWithUserConfirmation(line);
			}
			line.SetContainer(this, container);
			line.RefreshBinding();
		}

		public virtual void AllocateShipment(CommonShipment shipment)
		{
			if (shipment.OuterPackLines.Count > 0 && Containers.Count > 0 && !shipment.IsMasterShipmentRepresentingAllChildShipments)
			{
				foreach (PackLine line in shipment.OuterPackLines.Cast<PackLine>().Where(x => x.JL_PackageCount > 0))
				{
					AllocatePackLine(line);
				}
			}

			RebuildUnAllocatedPackLines();
		}

		public ZBool CheckCanAllocateShipmentsToContainers()
		{
			ZBool result = Containers.Count > 0;
			foreach (CommonContainer container in Containers)
			{
				if (container.JC_Calc_ContainerCapacity <= 0)
				{
					result = ZBool.False;
					break;
				}
			}
			return result;
		}

		#region Reset Lists

		protected void ResetListsOnTransportModeChanged()
		{
			sendingForwarderList = null;
			receivingForwarderList = null;
			fCreditorList = null;
			fCoLoadCarrierForwarderList = null;
			fOrgDepartureCtoList = null;
			fOrgArrivalCtoList = null;
			fShipments_List = null;
		}

		protected void ResetListsOnPortOfLoadingChanged()
		{
			sendingForwarderList = null;
			fCreditorList = null;
			fCoLoadCarrierForwarderList = null;
			fOrgDepartureCtoList = null;
			fOrgPackDepotList = null;
			fOrgDepartureContainerYardList = null;
		}

		protected void ResetListsOnPortOfDischargeChanged()
		{
			receivingForwarderList = null;
			fOrgArrivalCtoList = null;
			fOrgUnpackDepotList = null;
			fOrgArrivalContainerYardList = null;
		}

		#endregion

		#region Default JK_OA_ShippingLineAddress

		public void SetShippingLineBaseOnAirline(RefAirline airline)
		{
			ZGuid newOrgPK = GetShippingLineOrgBaseOnAirline(airline);
			ZGuid newAddrPK = GetDefaultShippingLineAddress(newOrgPK);

			if (JK_OA_ShippingLineAddress != newAddrPK && (JK_OA_ShippingLineAddress.IsEmpty || JK_OA_ShippingLineAddress == oldAddrPK))
			{
				SetDefaultShippingLineAddress(newOrgPK);
				oldAddrPK = JK_OA_ShippingLineAddress;
			}

			Transport transport = Transports.FirstTransportWithTransportMode(Core.Constants.TransportModes.Air);
			UpdateTransportCarrierFromAirline(transport, airline);
		}

		ZGuid oldAddrPK;

		public void UpdateTransportCarrierFromAirline(Transport transport, RefAirline airline)
		{
			ZGuid newOrgPK = GetShippingLineOrgBaseOnAirline(airline);

			if (transport != null && transport.CarrierPK != newOrgPK && (transport.CarrierPK.IsEmpty || transport.CarrierPK == oldOrgPK_JW))
			{
				transport.CarrierPK = newOrgPK;
				oldOrgPK_JW = transport.CarrierPK;
			}
		}

		ZGuid oldOrgPK_JW;

		ZGuid GetShippingLineOrgBaseOnAirline(RefAirline airline)
		{
			ZGuid result = ZGuid.Empty;
			if (airline != null)
			{
				OrgHeader carrier = airline.GetCorrespondingCarrierOrganisation();
				if (carrier != null)
				{
					result = carrier.PK;
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Costing / Invoicing

		public RefCurrency FreightCostsCurrency
		{
			get
			{
				RefCurrency result = null;

				var consolFreightCosts = GetConsolFreightCosts();
				var currency = consolFreightCosts.SameOrDefault(x => (ZString)(x[JobConsolCostSchema.E6_RX_NKCurrency]));
				if (!currency.IsEmpty)
				{
					result = RefCurrency.LoadFromCurrencyCode(Factory, currency);
				}

				if (result == null)
				{
					ZGuid currencyToLoad = IsSea ? ZArchitecture.Core.Utilities.CurrencyUSD : GlbCompany.CurrentCompany.LocalCurrency.PK;
					result = Factory.Load<RefCurrency>(currencyToLoad);
				}

				return result;
			}
		}

		public ZDecimal FreightCostsExchangeRate
		{
			get
			{
				ZDecimal result = 0m;

				RefCurrency freightCostCurrency = FreightCostsCurrency;
				if (freightCostCurrency != null)
				{
					result = GetExchangeRateFromFreightCostsOrSchedule(freightCostCurrency.RX_Code);
				}

				return result;
			}
		}

		public ZDecimal GetExchangeRateFromFreightCostsOrSchedule(ZString currencyCode)
		{
			return GetExchangeRateFromFreightCostsOrSchedule(currencyCode, ZGuid.Empty);
		}

		public bool HasConsolCosts(GlbCompany company)
		{
			var consolCostQuery = new ZQuery(JobConsolCostSchema.E6_GC, company.PK);
			consolCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, PK);
			consolCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);

			return Factory.LoadTop1<IJobConsolCost>(consolCostQuery) != null;
		}

		public bool HasGatewaySellApportionments(GlbCompany company)
		{
			var sellApportionmentQuery = new ZQuery(JobConsolCostSchema.E6_GC, company.PK);
			sellApportionmentQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, PK);
			sellApportionmentQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);
			sellApportionmentQuery.AddToFilter(JobConsolCostSchema.E6_GatewaySellChargeID, SQLComparisonOperator.NotEqual, DBNull.Value);

			return Factory.LoadTop1<IJobConsolCost>(sellApportionmentQuery) != null;
		}

		public ZDecimal FreightCostsAmount
		{
			get
			{
				var consolFreightCosts = GetConsolFreightCosts();
				return consolFreightCosts.Any() && FreightCostsCurrency != null ? (ZDecimal)consolFreightCosts.Sum(x => (ZDecimal)(x[JobConsolCostSchema.E6_OSCostAmount])) : (ZDecimal)0m;
			}
		}

		public IEnumerable<BusinessObject> GetConsolFreightCosts()
		{
			return GetConsolFreightCosts(ZGuid.Empty);
		}

		public BusinessObject GetConsolFreightCharge()
		{
			return GetConsolFreightChargeCore();
		}

		#region Implementation

		protected virtual BusinessObject GetConsolFreightChargeCore()
		{
			if (Job == null)
			{
				return null;
			}

			var chargeQuery = new ZQuery(JobChargeSchema.JR_AC, Env.Registry.FreightChargeCode);
			chargeQuery.AddToFilter(JobChargeSchema.JR_JH, Job.PK);
			chargeQuery.AddToFilter(JobChargeSchema.JR_JH_InternalJob, Job.PK);

			var charges = Factory.Load<JobCharge>(chargeQuery);
			var result = charges.FirstOrDefault(x => !x.IsCostPosted) ?? charges.FirstOrDefault(x => x.IsCostPosted);

			return result;
		}

		IEnumerable<BusinessObject> GetConsolFreightCosts(ZGuid costToExcludePK)
		{
			var results = new List<BusinessObject>();

			var costQuery = new ZQuery(JobConsolCostSchema.E6_AC_ChargeCode, Env.Registry.FreightChargeCode);
			costQuery.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
			costQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, PK);
			costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);

			if (!costToExcludePK.IsEmpty && costToExcludePK.IsValid)
			{
				costQuery.AddToFilter(JobConsolCostSchema.PK, SQLComparisonOperator.NotEqual, costToExcludePK);  //TODO: Check if this exclusion is used elsewhere
			}

			var unpostedQuery = new ZQuery(costQuery);
			unpostedQuery.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, null);

			results.AddRange(Factory.Load<IJobConsolCost>(unpostedQuery).Cast<BusinessObject>()); // look for unposted first
			if (!results.Any())
			{
				results.AddRange(Factory.Load<IJobConsolCost>(costQuery).Cast<BusinessObject>()); // then look for posted
			}

			return results;
		}

		protected ZDecimal GetExchangeRateFromFreightCostsOrSchedule(ZString currencyCode, ZGuid costToExcludePK)
		{
			ZDecimal exchangeRate = 0m;
			if (!currencyCode.IsEmpty)
			{
				if (currencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					var consolFreightCosts = GetConsolFreightCosts(costToExcludePK);
					if (consolFreightCosts.Any() && consolFreightCosts.All(x => (ZString)(x[JobConsolCostSchema.E6_RX_NKCurrency]) == currencyCode))
					{
						exchangeRate = (ZDecimal)consolFreightCosts.First()[JobConsolCostSchema.E6_ExchangeRate];
					}

					if (exchangeRate == 0m)
					{
						VoyageExRate scheduleRate = GetScheduleExchangeRate(currencyCode);
						if (scheduleRate != null)
						{
							exchangeRate = scheduleRate.E8_VoyageExchangeRate;
						}
					}
				}
				else
				{
					exchangeRate = 1m;
				}
			}

			return exchangeRate;
		}

		VoyageExRate GetScheduleExchangeRate(ZString currencyCode)
		{
			VoyageExRate result = null;

			Transport transportToLookup = this.IsImport() ? Transports.ArrivalTransport : Transports.DepartureTransport;

			var port = ZString.Empty;
			switch (JK_PrepaidCollect)
			{
				case Core.Constants.PaymentType.Prepaid:
					port = JK_RL_NKLoadPort;
					break;
				case Core.Constants.PaymentType.Collect:
					port = JK_RL_NKDischargePort;
					break;
				default:
					port = ZString.Empty;
					break;
			}

			if (transportToLookup != null && transportToLookup.Sailing != null && transportToLookup.Sailing.Voyage != null)
			{
				result = transportToLookup.Sailing.Voyage.ExRates.GetRateForCurrency(currencyCode, port);
			}

			return result;
		}

		#endregion

		#endregion

		#region IManifestProvider Members

		protected EDIMessageCollection fMessages;
		public virtual EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		event EventHandler IManifestProvider.CustomsManifestVisibilityChanged
		{
			add
			{
				JK_RL_NKLoadPortInfo.ValueChanged += value;
				JK_RL_NKDischargePortInfo.ValueChanged += value;
				JK_TransportModeInfo.ValueChanged += value;
				JK_MasterBillNumInfo.ValueChanged += value;

				foreach (Transport transport in Transports)
				{
					transport.JW_TransportModeInfo.ValueChanged += value;
					transport.JW_RL_NKDiscPortInfo.ValueChanged += value;
					transport.JW_RL_NKLoadPortInfo.ValueChanged += value;
				}

				TransportsCountChangedHandler += value;
			}
			remove
			{
				JK_RL_NKLoadPortInfo.ValueChanged -= value;
				JK_RL_NKDischargePortInfo.ValueChanged -= value;
				JK_TransportModeInfo.ValueChanged -= value;
				JK_MasterBillNumInfo.ValueChanged -= value;

				foreach (Transport transport in Transports)
				{
					transport.JW_TransportModeInfo.ValueChanged -= value;
					transport.JW_RL_NKDiscPortInfo.ValueChanged -= value;
					transport.JW_RL_NKLoadPortInfo.ValueChanged -= value;
				}

				TransportsCountChangedHandler -= value;
			}
		}
		EventHandler TransportsCountChangedHandler;

		void HookUpCustomVisiblityChanged(Transport transport)
		{
			if (TransportsCountChangedHandler != null)
			{
				transport.JW_TransportModeInfo.ValueChanged += TransportsCountChangedHandler;
				transport.JW_RL_NKDiscPortInfo.ValueChanged += TransportsCountChangedHandler;
				transport.JW_RL_NKLoadPortInfo.ValueChanged += TransportsCountChangedHandler;
			}
		}

		void UnHookUpCustomVisiblityChanged(Transport transport)
		{
			transport.JW_TransportModeInfo.ValueChanged -= TransportsCountChangedHandler;
			transport.JW_RL_NKDiscPortInfo.ValueChanged -= TransportsCountChangedHandler;
			transport.JW_RL_NKLoadPortInfo.ValueChanged -= TransportsCountChangedHandler;
		}

		public void OnTransportAdded(Transport transport)
		{
			if (transport != null)
			{
				HookUpCustomVisiblityChanged(transport);
			}
		}

		public virtual void OnTransportRemoved(Transport transport)
		{
			if (transport != null)
			{
				UnHookUpCustomVisiblityChanged(transport);
			}
		}

		#endregion

		#region Properties for Subclasses' ISendEmailSource Members

		protected string GetEmailSubject()
		{
			return Res.GetString("ed119468-6e99-414b-bcc2-4d260f430dd9", "Consol - {0}", JK_UniqueConsignRef);
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(JobConsolSchema.JK_RCA_AllocationLine.Name);
			result.Add(JobConsolSchema.JK_CarrierContractNumber.Name);
			result.Add(JobConsolSchema.JK_ElectronicBillOfLadingType.Name);
			result.Add(JobConsolSchema.JK_ElectronicBillOfLadingTerms.Name);
			result.Add(JobConsolSchema.JK_ElectronicBillOfLadingReference.Name);
			return result;
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return TemplateCopy(true, true);
		}

		public virtual CommonConsol TemplateCopy(ZBool copyShipments, ZBool copySecondaryTransports, ZBool includeDatesAndPorts = default, BusinessObjectFactory alternativeFactory = null)
		{
			var cloneArgs = alternativeFactory == null
				? new BusinessObjectCloneArgs(Array.Empty<string>())
				: new BusinessObjectCloneArgs(alternativeFactory, Array.Empty<string>(), null, true);
			var clonedConsol = (CommonConsol)Clone(cloneArgs);

			using (clonedConsol.GetValidationSuspender())
			{
				if (!includeDatesAndPorts)
				{
					foreach (ZPropertyInfo propertyInfo in clonedConsol.ZPropertyInfoHash)
					{
						if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
						{
							propertyInfo.Value = ZDateTime.Empty;
						}
					}
				}

				clonedConsol.JK_UniqueConsignRef = ZString.Empty;

				if (!JK_MasterBillNum.IsEmpty && clonedConsol.JK_MasterBillNum == JK_MasterBillNum)
				{
					clonedConsol.JK_MasterBillNum = JK_MasterBillNum.SubstringSafe(0, 3);
				}

				if (!includeDatesAndPorts)
				{
					clonedConsol.JK_RL_NKLastForeignPort = ZString.Empty;
				}

				if (copyShipments)
				{
					CopyShipmentsToClonedConsol(clonedConsol);
				}

				CopyTransportsToClonedConsol(clonedConsol, copySecondaryTransports, alternativeFactory);
				CopyNumbersToClonedConsol(clonedConsol, alternativeFactory);
				CopyNotesToClonedConsol(clonedConsol, alternativeFactory);
			}

			return clonedConsol;
		}

		void CopyShipmentsToClonedConsol(CommonConsol clonedConsol)
		{
			foreach (CommonShipment shipment in Shipments.ToList())
			{
				if (shipment.JS_JS_ColoadMasterShipment.IsEmpty)
				{
					clonedConsol.Shipments.AddRange(shipment.TemplateCopy(ZGuid.Empty));
				}
			}
		}

		void CopyTransportsToClonedConsol(CommonConsol clonedConsol, bool copySecondaryTransports, BusinessObjectFactory alternativeFactory)
		{
			clonedConsol.Transports.SuspendValidation();
			try
			{
				var autoAddedTransport = clonedConsol.Transports[0];

				if (copySecondaryTransports)
				{
					foreach (Transport transport in Transports)
					{
						CopyTransport(clonedConsol, transport, alternativeFactory);
					}
				}
				else
				{
					CopyTransport(clonedConsol, Transports.MostInterestingTransport, alternativeFactory);
				}

				autoAddedTransport.Delete();
			}
			finally
			{
				clonedConsol.Transports.ResumeValidation();
			}
		}

		void CopyTransport(CommonConsol clonedConsol, Transport transport, BusinessObjectFactory alternativeFactory)
		{
			var clonedTransport = transport.TemplateCopy(alternativeFactory);
			clonedConsol.Transports.Add(clonedTransport);
			if (clonedConsol is ICO2eProvider parentConsol && !clonedTransport.JW_IsLinked)
			{
				parentConsol.UpdateCO2eStatusToNotCurrent();
			}
		}

		void CopyNumbersToClonedConsol(CommonConsol clonedConsol, BusinessObjectFactory alternativeFactory)
		{
			clonedConsol.Numbers.SuspendValidation();
			try
			{
				var cloneArgs = new BusinessObjectCloneArgs(alternativeFactory, Array.Empty<string>(), typeof(CusEntryNumber), false);

				var toBeExcludeEntryTypes = new List<ZString>
				{
					ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference,
					ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference,
					CustomsReferenceNumberType.eHubInterchangeReference.HIR,
					GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber
				};

				if (clonedConsol.JK_RL_NKDischargePort.Left(2) == CountryCodes.Israel)
				{
					toBeExcludeEntryTypes.Add(IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber);
				}

				foreach (CusEntryNumber number in Numbers)
				{
					if (!toBeExcludeEntryTypes.Contains(number.CE_EntryType))
					{
						clonedConsol.Numbers.Add(number.Clone(cloneArgs));
					}
				}
			}
			finally
			{
				clonedConsol.Numbers.ResumeValidation();
			}
		}

		protected virtual void CopyNotesToClonedConsol(CommonConsol clonedConsol, BusinessObjectFactory alternativeFactory)
		{
			clonedConsol.Notes.SuspendValidation();
			try
			{
				var cloneArgs = new BusinessObjectCloneArgs(alternativeFactory, Array.Empty<string>(), typeof(StmNote), false);
				foreach (var note in Notes.GetAllNotes().Cast<StmNote>().Where(n => n.ST_Description != PredefinedNoteTypes.Instance.OriginalBillNotes.Description))
				{
					clonedConsol.Notes.Add(note.Clone(cloneArgs));
				}
			}
			finally
			{
				clonedConsol.Notes.ResumeValidation();
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public virtual DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new ConsolDocManagerInfo(this, DocManagerCode);
				}
				return docManagerInfo;
			}
		}
		ConsolDocManagerInfo docManagerInfo;

		protected ZString DocManagerCode
		{
			get { return "CON"; }
		}
		#endregion

		#region ICDArchive Members

		public virtual CDArchiveInfo CDArchiveInfo
		{
			get { return new ConsolCDArchiveInfo(this); }
		}

		#region ConsolCDArchiveInfo

		public class ConsolCDArchiveInfo : CDArchiveInfo
		{
			public ConsolCDArchiveInfo(CommonConsol consol)
				: base(consol)
			{
			}

			CommonConsol Consol
			{
				get { return (CommonConsol)BusinessEntity; }
			}

			public override ZString ConsigneeCode
			{
				get
				{
					OrgHeader organisation = Consol.ReceivingForwarder;
					return organisation != null ? organisation.OH_Code : ZString.Empty;
				}
			}

			public override ZString ConsignorCode
			{
				get
				{
					OrgHeader organisation = Consol.SendingForwarder;
					return organisation != null ? organisation.OH_Code : ZString.Empty;
				}
			}

			public override ZString[] ContainerNumbersList
			{
				get
				{
					ZString[] containers = new ZString[Consol.Containers.Count];
					for (int i = 0; i < Consol.Containers.Count; i++)
					{
						containers[i] = Consol.Containers[i].JC_ContainerNum;
					}
					return containers;
				}
			}

			public override ZString Destination
			{
				get
				{
					var destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Consol.JK_JX_JB_RL_NKPortOfDischarge);
					return (destination != null) ? destination.RL_Code : ZString.Empty;
				}
			}

			public override ZString Origin
			{
				get
				{
					var origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Consol.JK_JX_JA_RL_NKPortOfLoading);
					return (origin != null) ? origin.RL_Code : ZString.Empty;
				}
			}

			public override ZString[] EntryNumbersList
			{
				get
				{
					return GetCombinedList(Consol.Shipments, delegate(CDArchiveInfo info)
						{
							return info.EntryNumbersList;
						});
				}
			}

			public override ZDateTime ETA
			{
				get { return Consol.JK_JX_JB_E_ARV; }
			}

			public override ZDateTime ETD
			{
				get { return Consol.JK_JX_JA_E_DEP; }
			}

			public override ZString HouseBill
			{
				get { return ZString.Empty; }
			}

			public override ZString[] InvoiceNumbersList
			{
				get { return Array.Empty<ZString>(); }
			}

			public override ZString JobNumber
			{
				get { return Consol.JK_UniqueConsignRef; }
			}

			public override ZString[] OrderNumbersList
			{
				get
				{
					return GetCombinedList(Consol.Shipments, delegate(CDArchiveInfo info)
					{
						return info.OrderNumbersList;
					});
				}
			}

			public override ZString MasterBill
			{
				get { return Consol.JK_MasterBillNum; }
			}

			public override ZString Vessel
			{
				get { return Consol.JK_JX_JV_NKVessel; }
			}

			public override ZString VoyageFlight
			{
				get { return Consol.JK_JX_JV_VoyageFlight; }
			}
		}

		#endregion

		#endregion

		#region IDocumentSupportable Members

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return CommonConsolDocumentSupporter.New(this); }
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IImportExport Members

		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(JK_RL_NKLoadPort, JK_RL_NKDischargePort); }
		}

		#endregion

		#region IRoutingSupport Members

		RoutingCollection IRoutingSupport.TransportsIncludingRelated
		{
			get
			{
				if (transportsIncludingRelated == null)
				{
					transportsIncludingRelated = new RoutingCollection(this);
					OnTransportsIncludingRelatedCreated();
				}

				if (IsTransportsLoaded && IsPropertyReadOnlyDueToPhase((NoResString)"Routing"))
				{
					transportsIncludingRelated.SetReadOnlyIncludingChildren(true);
				}

				return transportsIncludingRelated;
			}
		}
		RoutingCollection transportsIncludingRelated;

		protected virtual void OnTransportsIncludingRelatedCreated()
		{
		}

		TransportCollection IRoutingSupport.Transports
		{
			get { return Transports; }
		}

		ZString IRoutingSupport.TransportMode
		{
			get { return JK_TransportMode; }
		}

		string IRoutingSupport.AdditionalETAUpdateMsg
		{
			get { return CommonConsol.AdditionalETAUpdateMsg; }
		}

		string IRoutingSupport.AdditionalETDUpdateMsg
		{
			get { return CommonConsol.AdditionalETDUpdateMsg; }
		}

		public static string AdditionalETDUpdateMsg
		{
			get { return Res.GetString("014cc801-e8c0-4547-b68f-6ea5947f90f7", "Note: If the ETD is changed on a Consol, then any Shipments on this Consol will be automatically set to have their ETDs no later than the Consol's ETD."); }
		}

		public static string AdditionalETAUpdateMsg
		{
			get { return Res.GetString("77712c23-47f0-4c9f-92e9-a989b526bb54", "Note: If the ETA is changed on a Consol, then any Shipments on this Consol will be automatically set to have their ETAs no earlier than the Consol's ETA."); }
		}

		#endregion

		#region IContainerLegParent Members

		public JobDocAddress GetConsigneeDocAddress
		{
			get { return null; }
		}

		public JobDocAddress GetConsigneeDeliveryDocAddress
		{
			get { return null; }
		}

		public JobDocAddress GetConsignorDocAddress
		{
			get { return null; }
		}

		public JobDocAddress GetConsignorPickupDocAddress
		{
			get { return null; }
		}

		public JobDocAddress GetArrivalCFSDocAddress
		{
			get
			{
				ZGuid arrivalCFSAddressPK = JK_OA_UnpackDepotAddress;
				DocAddressType addressType = DocAddressType.ArrivalCFSAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, arrivalCFSAddressPK);
			}
		}

		public JobDocAddress GetArrivalCTODocAddress
		{
			get
			{
				ZGuid arrivalCTOAddressPK = JK_OA_ArrivalCTOAddress;
				DocAddressType addressType = DocAddressType.ArrivalCTOAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, arrivalCTOAddressPK);
			}
		}

		public JobDocAddress GetArrivalContainerYardDocAddress
		{
			get
			{
				ZGuid arrivalCYDAddressPK = JK_OA_ContainerYardEmptyReturnAddress;
				DocAddressType addressType = DocAddressType.ArrivalCYDAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, arrivalCYDAddressPK);
			}
		}

		public JobDocAddress GetDepartureCFSDocAddress
		{
			get
			{
				ZGuid departureCFSAddressPK = JK_OA_PackDepotAddress;
				DocAddressType addressType = DocAddressType.DepartureCFSAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, departureCFSAddressPK);
			}
		}

		public JobDocAddress GetDepartureCTODocAddress
		{
			get
			{
				ZGuid departureCTOAddressPK = JK_OA_DepartureCTOAddress;
				DocAddressType addressType = DocAddressType.DepartureCTOAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, departureCTOAddressPK);
			}
		}

		public JobDocAddress GetDepartureContainerYardDocAddress
		{
			get
			{
				ZGuid departureCYDAddressPK = JK_OA_ContainerYardEmptyPickupAddress;
				DocAddressType addressType = DocAddressType.DepartureCYDAddress;

				return JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, departureCYDAddressPK);
			}
		}

		//public JobDocAddress GetDocAddressByOrgType(LocalCartageJobOrg OrgType)
		//{
		//    throw new Exception("The method or operation is not implemented.");
		//}

		#endregion

		#region IDocAddresses Members

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
					DocAddressesOnLoad(fDocAddresses);
					fDocAddresses.DocAddressOnAddedEvent += DocAddressOnAddedEvent;
				}

				return fDocAddresses;
			}
		}

		protected virtual void DocAddressesOnLoad(JobDocAddressDependentCollection docAddresses)
		{
		}

		protected virtual void DocAddressOnAddedEvent(DocAddressType docAddressType)
		{
		}

		JobDocAddressDependentCollection fDocAddresses;

		public SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.MaintainConsol;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return SupportedAddressTypes; }
		}

		protected virtual DocAddressType[] SupportedAddressTypes
		{
			get { return Array.Empty<DocAddressType>(); }
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return CanDeleteAddress(docAddress);
		}

		protected virtual bool CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region IRelatedJobNumber Members

		string[] IRelatedJobNumber.JobNumber
		{
			get
			{
				List<string> jobNumberList = new List<string>(Shipments.Count + 1);
				jobNumberList.Add(JK_UniqueConsignRef);
				foreach (CommonShipment shipment in Shipments)
				{
					jobNumberList.Add(shipment.JS_UniqueConsignRef);
				}

				return jobNumberList.ToArray();
			}
		}

		#endregion

		#region IFlightDetailsSuppression Members

		ZBool IFlightDetailsSuppression.IsPassengerFlight
		{
			get
			{
				Transport theLeg = Transports.MostInterestingTransport;
				return theLeg == null || !theLeg.JW_IsCargoOnly;
			}
		}

		ZBool IFlightDetailsSuppression.HasFinalRoutingLegATDPassed
		{
			get
			{
				Transport lastLeg = new TransportOrderHelper(((IRoutingSupport)this).TransportsIncludingRelated).LastLegWithTransportMode(Constants.TransportModes.Air, null, null);
				return lastLeg == null || !lastLeg.JW_ATD.IsEmpty && ZDateTime.Now > lastLeg.JW_ATD;
			}
		}

		ZBool IFlightDetailsSuppression.HasActualRCVPassed
		{
			get { return (!JK_JX_JB_A_ARV.IsEmpty && ZDateTime.Now > JK_JX_JB_A_ARV); }
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { fGetDocumentLogin += value; }
			remove { fGetDocumentLogin -= value; }
		}
		event EventHandler<SecurityLoginEventArgs> fGetDocumentLogin;

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get
			{
				return IsDPSFreightMovementRestrictedCore();
			}
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted
		{
			get
			{
				return IsAviationSecurityFreightMovementRestrictedCore();
			}
		}

		protected virtual bool IsAviationSecurityFreightMovementRestrictedCore()
		{
			return false;
		}

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return GetScreeningPartiesCore();
		}

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				var list = new List<OrgHeader>();
				if (SendingForwarder != null && ShouldCheckCreditOnHold(OrgCodes.SendingAgent))
				{
					list.Add(SendingForwarder);
				}
				if (ReceivingForwarder != null && ShouldCheckCreditOnHold(OrgCodes.ReceivingAgent))
				{
					list.Add(ReceivingForwarder);
				}
				foreach (CommonShipment shipment in Shipments)
				{
					list.AddRange(((ICreditControlledDocumentDelivery)shipment).OrganisationsForCreditChecks);
				}
				return list.ToArray();
			}
		}

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (fGetDocumentLogin != null)
			{
				fGetDocumentLogin(this, e);
			}
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return Res.GetString("5CEF2366-FAE9-499E-974E-6B3DCA27F293", "Sending Agent, Receiving Agent or Consignee, Consignor, Local Client or any Debtors in any associated Shipment"); }
		}

		protected virtual bool ShouldCheckCreditOnHold(ZString organizationType)
		{
			return true;
		}

		protected virtual bool IsDPSFreightMovementRestrictedCore()
		{
			return false;
		}

		protected virtual ScreeningParty[] GetScreeningPartiesCore()
		{
			return Array.Empty<ScreeningParty>();
		}

		#endregion

		#region IAdditionalReferenceNumberSupporter Members

		void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
		{
			var additionalReferenceNumberTypes = ((IAdditionalReferenceNumberTypeProvider)this).GetAdditionalReferenceNumberTypeList(
				CusEntryNumber.Categories.AdditionalReferenceNumber,
				countryCode);

			if (!additionalReferenceNumberTypes.ContainsCode(numberType))
			{
				notify.Notify(new WarningNotification(WarningType.Warning, Res.GetString("0e983027-fa06-4aa0-8c20-8ae8d287cdd8", "No support for additional reference number type '{0}'", numberType)));
				return;
			}

			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			if (country != null)
			{
				CusEntryNumber number = GetCusEntryNumber(numberType, country, CusEntryNumber.Categories.AdditionalReferenceNumber);
				if (number != null)
				{
					number.CE_EntryNum = value;
				}
				else
				{
					CreateCusEntryNumber(numberType, country, value, CusEntryNumber.Categories.AdditionalReferenceNumber);
				}
			}
		}

		bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
		{
			get { return true; }
		}

		void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
		{
		}

		CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
		{
			get { return Numbers; }
		}

		void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
		{
		}

		CusEntryNumber GetCusEntryNumber(ZString numberType, RefCountry country, string category)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, numberType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country.Code);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_Category, category);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, TableName);

			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		void CreateCusEntryNumber(ZString numberType, RefCountry country, ZString value, string category)
		{
			CusEntryNumber number;
			if (fNumbers == null)
			{
				number = Factory.New<CusEntryNumber>();
				number.Parent = this;
			}
			else
			{
				number = Numbers.AddNew();
			}
			number.CE_Category = category;
			number.CE_EntryNum = value;
			number.CE_EntryType = numberType;
			number.CE_RN_NKCountryCode = country.Code;
		}

		#endregion

		#region IAdditionalReferenceNumberTypeProvider

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			return category.ToString() switch
			{
				CusEntryNumber.Categories.AdditionalReferenceNumber => GetAdditionalReferenceNumberTypeListForOTH(),
				_ => Factory.GetCachedValue<CodeDescriptionPairList>(),
			};

			CodeDescriptionPairList GetAdditionalReferenceNumberTypeListForOTH()
			{
				var parameters = new AdditionalReferenceNumberTypesParameters(countryCode) { Parent = this, DischargeCountryCode = JK_RL_NKDischargePort.Left(2) };
				return Factory.GetCachedValue($"GetAdditionalReferenceNumberTypeList_CommonConsol_{parameters.Key}", () =>
				{
					var result = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(parameters);
					result.AddPairsIfNotExist(new ConsolNonCustomsAdditionalReferenceCodesCodeList().ToArray());
					return result;
				});
			}
		}

		#endregion

		#region ICusEntryNumberValidationDeciderOfType

		Type ICusEntryNumberValidationDeciderOfType.GetCusEntryNumberValidationType()
		{
			return typeof(CommonAdditionalRefEntryNumValidation);
		}

		#endregion

		#region ISupportDataImporting Members

		protected bool IsImportingData
		{
			get { return fIsImportingData; }
		}

		bool fIsImportingData;

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion

		#region IMovementLeg Members

		public ZString TransportMode
		{
			get { return JK_TransportMode; }
		}

		ZString IMovementLeg.Load
		{
			get { return JK_RL_NKLoadPort; }
		}

		ZString IMovementLeg.Discharge
		{
			get { return JK_RL_NKDischargePort; }
		}

		ZDateTime IMovementLeg.DepartureDate
		{
			get
			{
				Transport transport = Transports.DepartureTransport;
				return transport == null ? ZDateTime.Empty :
					  (transport.JW_ATD.IsValid ? transport.JW_ATD : transport.JW_ETD);
			}
		}

		ZDateTime IMovementLeg.ArrivalDate
		{
			get
			{
				Transport transport = Transports.ArrivalTransport;
				return transport == null ? ZDateTime.Empty :
					  (transport.JW_ATA.IsValid ? transport.JW_ATA : transport.JW_ETA);
			}
		}

		#endregion

		#region IsGoingVia

		public bool IsGoingViaIgnoringDomesticRoute(ZString countryCode)
		{
			return IsGoingViaIgnoringDomesticRoute(RefCountry.LoadFromCountryCode(Factory, countryCode));
		}

		public bool IsGoingViaIgnoringDomesticRoute(RefCountry country)
		{
			bool result = false;
			if (country != null)
			{
				result = country.ContainsUNLOCO(DischargePort) || country.ContainsUNLOCO(FirstForeignPort) || country.ContainsUNLOCO(LastForeignPort) || country.ContainsUNLOCO(PortOfFirstArrival);
				if (!result)
				{
					foreach (Transport transport in Transports)
					{
						if (!transport.IsDomestic && country.ContainsUNLOCO(transport.DiscPort))
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		#endregion

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new CommonConsolTransportSupporter<CommonConsol>(this); }
		}

		ZString ITransportParentCommon.TypeCode
		{
			get { return Constants.TransportParentTypes.Consol; }
		}

		TransportCollection ITransportParent.Transports
		{
			get { return Transports; }
		}

		#endregion

		#region ITransportChangeNotifier Members

		void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
			TransportChangeNotifyHandler handler;
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out handler))
			{
				if (handler != null)
				{
					handler(transport, previousValue);
				}
			}
		}

		Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler> TransportChangeNotifierDictionary
		{
			get { return transportChangeNotifierDictionary ?? (transportChangeNotifierDictionary = new Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler>()); }
		}
		Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler> transportChangeNotifierDictionary;

		public void AddTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier)
		{
			TransportChangeNotifyHandler handler;
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out handler))
			{
				handler -= notifier;
				handler += notifier;
			}
			else
			{
				TransportChangeNotifierDictionary.Add(notifyType, notifier);
			}
		}

		public void RemoveTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier)
		{
			TransportChangeNotifyHandler handler;
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out handler))
			{
				handler -= notifier;
			}
		}

		#endregion

		#region ICancellable Members

		public override string CanCancel()
		{
			string result = base.CanCancel();

			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			if (Shipments.Count > 0)
			{
				result = Res.GetString("fc9862a3-0380-40df-b35c-c95eee1397e7", "You cannot deactivate {0} since it has shipments attached.", HumanReadableName);
			}
			else
			{
				result = JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(PK, HumanReadableName);
			}

			return result;
		}

		#endregion

		#region IBillGenerationSupport

		ZString IBillGenerationSupport.TranshipmentIndicator
		{
			get { return ""; }
		}

		RefUNLOCO IBillGenerationSupport.Discharge
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JK_RL_NKDischargePort); }
		}

		RefUNLOCO IBillGenerationSupport.Load
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JK_RL_NKLoadPort); }
		}

		ZString IBillGenerationSupport.TransportMode
		{
			get { return JK_TransportMode; }
		}

		ZString IBillGenerationSupport.ServiceLevel
		{
			get { return JK_AWBServiceLevel; }
		}

		OrgHeader IBillGenerationSupport.CarrierPrincipal
		{
			get { return ShippingLine; }
		}

		RefUNLOCO IBillGenerationSupport.Origin
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JK_RL_NKLoadPort); }
		}

		RefUNLOCO IBillGenerationSupport.Destination
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JK_RL_NKDischargePort); }
		}

		#endregion

		public override ZBool JK_IsCancelled
		{
			get
			{
				return base.JK_IsCancelled;
			}
			set
			{
				if (base.JK_IsCancelled != value)
				{
					base.JK_IsCancelled = value;
					UpdateReadOnlyForWhenCancelled();
				}
			}
		}

		void UpdateReadOnlyForWhenCancelled()
		{
			if (!IsDeleted)
			{
				SetReadOnlyIncludingChildren(JK_IsCancelled);
			}
		}

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new ConsolRatingAdaptersProvider<CommonConsol>(this); }
		}

		public JobInvoicingConsumerType ConsumerType
		{
			get { return GetConsumerTypeCore(); }
		}

		protected virtual JobInvoicingConsumerType GetConsumerTypeCore()
		{
			return JobInvoicingConsumerTypes.ForwardingConsol;
		}

		public IAutoRating RatingAdapter
		{
			get { return ratingAdapter ?? (ratingAdapter = GetRatingAdapterCore()); }
		}
		IAutoRating ratingAdapter;

		protected virtual IAutoRating GetRatingAdapterCore()
		{
			return new ConsolRatingAdapter<CommonConsol>(new ConsolRatingRoute(this));
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return JK_TransportMode; }
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return GetDefaultNumberOfDecimalsCore(property);
		}

		protected virtual int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			return GetUnitOfMeasureCore(property);
		}

		protected virtual ZString GetUnitOfMeasureCore(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.JK_TotalShipmentWeight:
				case Schema.JK_TotalDocumentedWeight:
				case Schema.JK_TotalManifestedWeight:
					unitOfMeasure = JK_TotalShipmentWeightUnit;
					break;

				case Schema.JK_TotalShipmentVolume:
				case Schema.JK_TotalDocumentedVolume:
				case Schema.JK_TotalManifestedVolume:
					unitOfMeasure = JK_TotalShipmentVolumeUnit;
					break;

				case Schema.JK_TotalShipmentChargeable:
					unitOfMeasure = JK_Calc_TotalShipmentChargeableUnit;
					break;

				case Schema.JK_ConsolChargeable:
				case Schema.JK_TotalDocumentedChargeable:
				case Schema.JK_TotalManifestedChargeable:
					unitOfMeasure = JK_ConsolChargeableUnit;
					break;

				case Schema.JK_CorrectedConsolVolume:
					unitOfMeasure = JK_CorrectedConsolVolumeUnit;
					break;

				case Schema.JK_Calc_ActualVolumeWeight:
					unitOfMeasure = JK_Calc_ActualVolumeWeightUnit;
					break;

				case Schema.JK_Calc_FreeSpace:
					unitOfMeasure = JK_Calc_TotalShipmentChargeableUnit;
					break;

				case Schema.JK_CorrectedConsolWeight:
					unitOfMeasure = JK_CorrectedConsolWeightUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		ZDecimal IDefaultNumberOfDecimalsSupporterWithSchemaColumn.GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			RoundMeasurePropertiesOnTransportModeChangedCore();
		}

		protected virtual void RoundMeasurePropertiesOnTransportModeChangedCore()
		{
			this.SetRoundedValue(JobConsolSchema.JK_ConsolChargeable, JK_ConsolChargeableInfo);
			this.SetRoundedValue(JobConsolSchema.JK_CorrectedConsolVolume, JK_CorrectedConsolVolumeInfo);
			this.SetRoundedValue(JobConsolSchema.JK_CorrectedConsolWeight, JK_CorrectedConsolWeightInfo);

			if (fContainers != null)
			{
				foreach (CommonContainer container in fContainers)
				{
					((IDefaultNumberOfDecimalsSupporter)container).RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
		}

		#endregion

		#region IContainerParent Members

		IEnumerable<CommonContainer> IContainerParent.Containers
		{
			get { return Containers.Cast<CommonContainer>(); }
		}

		IEnumerable<JobDocsAndCartage> IContainerParent.DocsAndCartage(CommonContainer container)
		{
			foreach (CommonShipment shipment in Shipments)
			{
				if (IsContainerPackedForShipment(container, shipment))
				{
					yield return shipment.DocsAndCartage;
				}
			}
		}

		bool IsContainerPackedForShipment(CommonContainer container, CommonShipment shipment)
		{
			bool result = false;

			if (container != null && shipment != null)
			{
				if (container.JC_JK == PK && shipment.OuterPackLines.Count == 0 && Containers.Count == 1)
				{
					result = true;
				}
				else if (shipment.Containers.Contains(container))
				{
					result = true;
				}
			}

			return result;
		}

		OrgAddress IContainerParent.ArrivalCFSAddress
		{
			get { return UnpackDepotAddress; }
		}

		OrgAddress IContainerParent.DepartureCFSAddress
		{
			get { return PackDepotAddress; }
		}

		#endregion

		#region IsAttachedToStandAloneShipment

		public bool IsAttachedToStandAloneShipment { get; set; }

		#endregion

		#region Supply Chain Security

		internal ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration => supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = FreightUtilities.SupplyChainSecurityConfiguration);
		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		ZString ISupplyChainSecurityImportExportSupporter.LoadCountryForSupplyChainSecurity
		{
			get { return DepartureFlightOriginLoco != null ? DepartureFlightOriginLoco.RL_RN_NKCountryCode : JK_RL_NKLoadPort.SubstringSafe(0, 2); }
		}

		ZString ISupplyChainSecurityImportExportSupporter.DischargeCountryForSupplyChainSecurity => JK_RL_NKDischargePort.SubstringSafe(0, 2);

		IEnumerable<ITransport> ISupplyChainSecurityImportExportSupporter.SupplyChainSecurityRelatedTransports
		{
			get { return Transports.Cast<ITransport>(); }
		}

		#endregion

		#region AllowsForShipmentsDangerousGoods

		public virtual bool AllowsForShipmentsDangerousGoods(CommonShipment shipment)
		{
			return true;
		}

		#endregion

		#region AllowsForShipmentsLithiumBatteries

		public virtual bool AllowsForShipmentsLithiumBatteries(CommonShipment shipment)
		{
			return true;
		}

		#endregion

		#region AllShipmentsContainPermissibleQuantities

		public virtual bool AllShipmentsContainPermissibleQuantities(IEnumerable<CommonShipment> shipments)
		{
			return true;
		}

		#endregion

		#region ShipmentTemperatureRangeIsValid

		public virtual bool ShipmentTemperatureRangeIsValid(CommonShipment shipment)
		{
			return true;
		}

		#endregion

		#region ShipmentContainsAllowableCargoDimensions

		public virtual bool ShipmentContainsAllowableCargoDimensions(CommonShipment shipment, bool checkIfDimensionsAreRestricted) => true;

		#endregion

		#region TemplateRecordProviderConsol

		public CommonConsol TemplateRecordProviderConsol
		{
			get
			{
				if (templateRecordProviderConsol == null && this is ITemplateRecordProvider provider && TemplateRecord != null)
				{
					var templateRecordFactory = new TemplateRecordBusinessObjectFactory();
					var consolProviderType = CargoWise.Application.ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>();
					templateRecordProviderConsol = provider.InstantiateFromTemplateRecord(templateRecordFactory, consolProviderType, provider.TemplateRecord) as CommonConsol;
				}
				return templateRecordProviderConsol;
			}
		}

		CommonConsol templateRecordProviderConsol;

		#endregion

		#region Default Address

		internal ZGuid GetDefaultAddressPKBasedOnDirectionAndPorts(OrgHeader header)
		{
			var defaultAddress = header.MainAddress;
			var headerAddresses = header.Addresses.Cast<OrgAddress>().Where(a => a.OA_IsActive).ToArray();

			if (this.IsExport())
			{
				defaultAddress = headerAddresses.SingleAddressMatchesPortCode(JK_RL_NKLoadPort)
					?? headerAddresses.SingleAddressMatchesCountryCode(JK_RL_NKLoadPort.Left(2));
			}
			else if (this.IsImport())
			{
				defaultAddress = headerAddresses.SingleAddressMatchesPortCode(JK_RL_NKDischargePort)
					?? headerAddresses.SingleAddressMatchesCountryCode(JK_RL_NKDischargePort.Left(2));
			}
			else if (this.IsDomestic())
			{
				defaultAddress = headerAddresses.SingleAddressMatchesPortCode(JK_RL_NKLoadPort)
					?? headerAddresses.SingleAddressMatchesPortCode(JK_RL_NKDischargePort)
					?? headerAddresses.SingleAddressMatchesCountryCode(JK_RL_NKLoadPort.Left(2))
					?? headerAddresses.SingleAddressMatchesCountryCode(JK_RL_NKDischargePort.Left(2));
			}
			else
			{
				defaultAddress = headerAddresses.SingleAddressMatchesPortCode(JK_RL_NKLoadPort)
					?? headerAddresses.SingleAddressMatchesCountryCode(JK_RL_NKLoadPort.Left(2));
			}

			return defaultAddress != null ? defaultAddress.PK : header.MainAddress.PK;
		}

		void DefaultCoLoadWithAddress()
		{
			if (IsCoLoad && JK_OA_CreditorAddress.IsEmpty
				&& !JK_AgentType.IsEmpty && !JK_TransportMode.IsEmpty
				&& !JK_ConsolMode.IsEmpty && !JK_RL_NKLoadPort.IsEmpty
				&& !JK_RL_NKDischargePort.IsEmpty)
			{
				JK_OA_CreditorAddress = this.GetDefaultColoadWithAddressBasedOnSendingAgent();
			}
		}

		#endregion

		#region IOriginDestinationForDocumentDeliveryRestriction

		string IOriginDestinationForDocumentDeliveryRestriction.OriginCountryCode => CountryCode(JK_RL_NKLoadPort);

		string IOriginDestinationForDocumentDeliveryRestriction.DestinationCountryCode => CountryCode(JK_RL_NKDischargePort);

		ZString CountryCode(ZString unloco)
		{
			return unloco.SubstringSafe(0, 2);
		}

		#endregion

		#region Compliance Risk

		public ComplianceRiskStatusObject ComplianceRiskStatus => ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus((IComplianceItemRiskStatusProvider)this);

		[CargoWise.Macros.MacroIgnore]
		public ZString OverallComplianceRisk => ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus((IComplianceItemRiskStatusProvider)this).GetOverallRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString PartyComplianceRisk => ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus((IComplianceItemRiskStatusProvider)this).GetPartyRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString LocationComplianceRisk => ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus((IComplianceItemRiskStatusProvider)this).GetLocationRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString CommodityComplianceRisk => ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus((IComplianceItemRiskStatusProvider)this).GetCommodityRiskDescription();

		ZBool IComplianceJobDirectionProvider.IsInternational => (this.IsCrossTrade() || this.IsExport() || this.IsImport());

		#endregion

		#region ConsolJobDatesProvider Support

		public virtual ZDate AutoratingDate { get; set; }

		#endregion

		#region ShipmentDeliveredTime

		public ZDateTime ShipmentDeliveredTime { get; set; }

		#endregion

		#region BookingConfirmations

		public BookingConfirmationCollection BookingConfirmations
		{
			get
			{
				if (bookingConfirmations == null)
				{
					bookingConfirmations = new BookingConfirmationCollection(this);
				}

				return bookingConfirmations;
			}
		}
		BookingConfirmationCollection bookingConfirmations;

		#endregion

		#region ActualEvents

		public ActualEventCollection ActualEvents
		{
			get
			{
				if (actualEvents == null)
				{
					actualEvents = new ActualEventCollection(this);
				}

				return actualEvents;
			}
		}
		ActualEventCollection actualEvents;

		#endregion

		#region IPortMatchingSupport

		ILocationReference IPortMatchingSupport.PortOfDischarge => DischargePort;

		void IPortMatchingSupport.SetFirstArrivalPort(ZString unloco, ZDateTimeOffset dateTime)
		{
			if (!JK_RL_NKPortOfFirstArrivalInfo.ReadOnly && JK_RL_NKPortOfFirstArrival.IsEmpty
				&& !JK_DatePortOfFirstArrivalInfo.ReadOnly && JK_DatePortOfFirstArrival.IsEmpty)
			{
				if (!unloco.IsEmpty)
				{
					JK_RL_NKPortOfFirstArrival = unloco;
				}
				if (dateTime.IsValid)
				{
					JK_DatePortOfFirstArrival = dateTime.ToDateTime();
				}
			}
		}

		void IPortMatchingSupport.SetLastForeignPort(ZString unloco, ZDateTimeOffset dateTime)
		{
			if (!JK_RL_NKLastForeignPortInfo.ReadOnly && JK_RL_NKLastForeignPort.IsEmpty
				&& !JK_DateLastForeignPortInfo.ReadOnly && JK_DateLastForeignPort.IsEmpty)
			{
				if (!unloco.IsEmpty)
				{
					JK_RL_NKLastForeignPort = unloco;
				}
				if (dateTime.IsValid)
				{
					JK_DateLastForeignPort = dateTime.ToDateTime();
				}
			}
		}

		void IPortMatchingSupport.ReportException(Exception exception) { }

		#endregion

		#region SetDefaultLastForeignPortFirstArrivalPortBasedOnSailingSchedule

		public bool SeaImportLinkedTransportLegHasLastForeignPort
		{
			get
			{
				var sailing = SeaImportLinkedTransportLegSailing;
				return !sailing?.Destination?.JB_RL_NKLastForeignPort.IsEmpty ?? false;
			}
		}

		JobSailing SeaImportLinkedTransportLegSailing
		{
			get
			{
				return Transports.Cast<Transport>()
						   .Where(t => t.TransportMode == TransportModes.Sea && t.JW_IsLinked && ImportExportHelper.IsLocal(t.JW_RL_NKDiscPort, ((IPortMatchingSupport)this).PortOfDischarge))
						   .OrderBy(t => t.JW_ETA)
						   .FirstOrDefault(t => !t.IsInDatabase || t.JW_IsLinkedInfo.HasChanges || t.JW_JXInfo.HasChanges)?.Sailing;
			}
		}

		void SetDefaultLastForeignPortFirstArrivalPortBasedOnSailingSchedule()
		{
			var sailing = SeaImportLinkedTransportLegSailing;
			if (sailing != null && sailing.Destination != null)
			{
				var destination = sailing.Destination;
				IPortMatchingSupport portMatchingSupport = this;
				portMatchingSupport.SetFirstArrivalPort(destination.JB_RL_NKFirstDischargePort, destination.JB_FirstDischargePortETA);
				portMatchingSupport.SetLastForeignPort(destination.JB_RL_NKLastForeignPort, destination.JB_LastForeignPortETD);
			}
		}

		#endregion

		#region TryLogWhileClearTimeOfTransports

		public void TryLogWhileClearTimeOfTransports(IXmlImportLogger logger, string errorKeySuffix = "", IStmALog eventCancelled = null)
		{
			if (IsInDatabase && IsAir && Transports.HasChanges)
			{
				var currentLegs = Transports.ToArray<Transport>();
				var query = new ZDBOnlyQuery(typeof(Transport));
				query.AddToFilter(JobConsolTransportSchema.JW_ParentGUID, this.PK);
				var newFactory = new BusinessObjectFactory();
				var originalLegs = newFactory.Load<Transport>(query);

				var (message, messageWithStackTrace) = GetTransportTimeClearedLog(currentLegs, originalLegs);

				if (!message.IsNullOrEmpty())
				{
					logger?.Log(LogType.Information, message);

					var useNewXMLSchema = SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE;
					var sourceMessage = (logger as IXmlSessionTracker)?.SourceMessage;

					ErrorReporter.ReportOnce($"TransportTimeUnexpectedlyClearedOut{errorKeySuffix}",
						messageWithStackTrace +
						"\nUseDate2012_11NamespaceAndFormat = " + useNewXMLSchema +
						"\nEM_MessageNum = " + sourceMessage?.EM_MessageNum +
						"\nEM_MessageText = " + sourceMessage?.EM_MessageText +
						"\nOriginal Transports:\n" + GetTransportLegsLog(originalLegs) +
						"\nCurrent Transports:\n" + GetTransportLegsLog(currentLegs) +
						"\nOriginal Schedules:\n" + GetSailingLog(originalLegs, newFactory) +
						"\nCurrent Schedules:\n" + GetSailingLog(currentLegs) +
						"\nEvent Cancelled Context: " + eventCancelled?.SL_SE_NKEvent ?? (NoResString)"<null>");
				}
			}
		}

		static (string Message, string MessageWithStackTrace) GetTransportTimeClearedLog(Transport[] currentLegs, Transport[] originalLegs)
		{
			var message = new ZStringBuilder();
			var messageWithStackTrace = new ZStringBuilder();

			foreach (Transport currentLeg in currentLegs)
			{
				var originalLeg = originalLegs.FirstOrDefault(leg => leg.JW_TransportMode == currentLeg.JW_TransportMode && leg.JW_RL_NKLoadPort == currentLeg.JW_RL_NKLoadPort && leg.JW_RL_NKDiscPort == currentLeg.JW_RL_NKDiscPort);
				if (originalLeg != null)
				{
					var compareResults = CompareTransportTime(currentLeg, originalLeg).ToArray();
					if (compareResults.Any())
					{
						messageWithStackTrace.Append(string.Join(System.Environment.NewLine,
							compareResults.Select(
								x => $"{originalLeg.JW_RL_NKLoadPort}->{originalLeg.JW_RL_NKDiscPort}:{x.Message}:{System.Environment.NewLine}{x.StackTrace}")));
						message.Append(string.Format("{0}->{1}:{2}", originalLeg.JW_RL_NKLoadPort, originalLeg.JW_RL_NKDiscPort,
								string.Join(",", compareResults.Select(x => x.Message))));
					}
				}
			}

			return !message.IsEmpty ? (message.ToStringWithDelimiterBetweenAppends(";"), string.Join(System.Environment.NewLine, messageWithStackTrace)) : (string.Empty, string.Empty);
		}

		string GetSailingLog(Transport[] legs, BusinessObjectFactory factory = null)
		{
			var result = string.Empty;
			foreach (Transport leg in legs.Where(x => !x.JW_JX.IsEmpty))
			{
				result += !string.IsNullOrEmpty(result) ? "\n" : string.Empty;
				var sailing = factory != null ? factory.Load<JobSailing>(leg.JW_JX) : leg.Sailing;
				if (sailing != null)
				{
					result += GetSailingLog(sailing);
				}
			}

			return result;
		}

		string GetSailingLog(JobSailing sailing)
		{
			return $"Sailing PK = {sailing.PK}, JX_JA_RL_NKPortOfLoading = {sailing.JX_JA_RL_NKPortOfLoading}, JX_JB_RL_NKPortOfDischarge = {sailing.JX_JB_RL_NKPortOfDischarge}, " +
				$"JX_JA_E_DEP = {sailing.JX_JA_E_DEP}, JX_JA_A_DEP = {sailing.JX_JA_A_DEP}, JX_JA_S_DEP = {sailing.JX_JA_S_DEP}, " +
				$"JX_JB_E_ARV = {sailing.JX_JB_E_ARV}, JX_JB_A_ARV = {sailing.JX_JB_A_ARV}, JX_JB_S_ARV= {sailing.JX_JB_S_ARV}, " +
				$"JX_JV_NKVessel = {sailing.JX_JV_NKVessel}, JX_JV_VoyageFlight = {sailing.JX_JV_VoyageFlight}";
		}

		static string GetTransportLegsLog(Transport[] legs)
		{
			var result = string.Empty;
			foreach (Transport leg in legs)
			{
				result += !string.IsNullOrEmpty(result) ? "\n" : string.Empty;
				result += $"Transport PK = {leg.PK}, JW_RL_NKLoadPort = {leg.JW_RL_NKLoadPort}, JW_RL_NKDiscPort = {leg.JW_RL_NKDiscPort}, " +
					$"JW_ETD = {leg.JW_ETD}, JW_ETA = {leg.JW_ETA}, JW_ATD = {leg.JW_ATD}, JW_ATA = {leg.JW_ATA}, " +
					$"JW_JX = {leg.JW_JX}, JW_IsLinked = {leg.JW_IsLinked}, JW_Vessel = {leg.JW_Vessel}, JW_VoyageFlight = {leg.JW_VoyageFlight}, " +
					$"JW_IsCharter = {leg.JW_IsCharter}, JW_IsCargoOnly = {leg.JW_IsCargoOnly}, JW_AircraftType = {leg.JW_AircraftType};";
			}

			return result;
		}

		static IEnumerable<(string Message, string StackTrace)> CompareTransportTime(Transport currentLeg, Transport matchedLeg)
		{
			if (!matchedLeg.JW_ETDForBinding.IsEmpty && currentLeg.JW_ETDForBinding.IsEmpty)
			{
				yield return ((NoResString)"ETD is cleared", currentLeg.JW_ETD_ResetStackTrace);
			}

			if (!matchedLeg.JW_ETAForBinding.IsEmpty && currentLeg.JW_ETAForBinding.IsEmpty)
			{
				yield return ((NoResString)"ETA is cleared", currentLeg.JW_ETA_ResetStackTrace);
			}
		}

		#endregion

		#region Pre-Allocation Status

		protected bool forceUpdatePreAllocatedAmountExceededStatus;

		public void UpdatePreAllocatedAmountExceededStatusIfNecessary()
		{
			if (forceUpdatePreAllocatedAmountExceededStatus || !IsInDatabase || AnyPreAllocatedAmountPropertyHasChanges)
			{
				UpdatePreAllocatedAmountExceededStatus();
			}
		}

		protected virtual bool AnyPreAllocatedAmountPropertyHasChanges => false;

		public virtual void UpdatePreAllocatedAmountExceededStatus() { }

		#endregion

		#region AddressAdditionalInfo

		[JobAddressAdditionalInfoAddressTypes(AutoDocAddressTypes.Codes.DepartureCFSAddress, AutoDocAddressTypes.Codes.ArrivalCFSAddress)]
		public IJobAddressAdditionalInfoCollection JobAddressAdditionalInfoCollection
		{
			get
			{
				if (jobAddressAdditionalInfoCollection == null)
				{
					jobAddressAdditionalInfoCollection = new JobAddressAdditionalInfoCollection(this, Factory);
					if (!IsDeleted && !IsDeleting)
					{
						this.HookEventsToDependentAddresses();
					}
					RegisterEditableChildObject(jobAddressAdditionalInfoCollection as JobAddressAdditionalInfoCollection);
				}
				return jobAddressAdditionalInfoCollection;
			}
		}

		IJobAddressAdditionalInfoCollection jobAddressAdditionalInfoCollection { get; set; }

		ZPropertyInfo IJobAddressAdditionalInfoSupport.GetDependentAddress(string addressType)
		{
			return addressType switch
			{
				AutoDocAddressTypes.Codes.DepartureCFSAddress => JK_OA_PackDepotAddress_ZAddress.OrgPKInfo,
				AutoDocAddressTypes.Codes.ArrivalCFSAddress => JK_OA_UnpackDepotAddress_ZAddress.OrgPKInfo,
				_ => null
			};
		}

		[BusinessObjectTestExclude]
		public virtual ZString CFSDepartureByTransportMode
		{
			get => this.GetTransportModeOrDefault(AutoDocAddressTypes.Codes.DepartureCFSAddress);
			set => this.SetTransportMode(AutoDocAddressTypes.Codes.DepartureCFSAddress, value);
		}

		public ZPropertyInfo CFSDepartureByTransportModeInfo => GetZPropertyInfo(Schema.CFSDepartureByTransportMode);

		public bool CFSDepartureByTransportMode_ReadOnly => JK_OA_PackDepotAddress_ZAddress.OrgPK == Guid.Empty;

		[BusinessObjectTestExclude]
		public virtual ZString CFSArrivalByTransportMode
		{
			get => this.GetTransportModeOrDefault(AutoDocAddressTypes.Codes.ArrivalCFSAddress);
			set => this.SetTransportMode(AutoDocAddressTypes.Codes.ArrivalCFSAddress, value);
		}

		public ZPropertyInfo CFSArrivalByTransportModeInfo => GetZPropertyInfo(Schema.CFSArrivalByTransportMode);

		public bool CFSArrivalByTransportMode_ReadOnly => JK_OA_UnpackDepotAddress_ZAddress.OrgPK == Guid.Empty;

		public ZGuid JobAddressAdditionalInfoParentID => PK;

		public ZString JobAddressAdditionalInfoTableCode => TablePrefix;

		#endregion
	}
}
