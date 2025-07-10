using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using Constants = Enterprise.eTail.DataTransfer.Universal.Constants;

namespace Enterprise.eTail.DataTransfer
{
	public abstract class CustomsRelatedBusinessObjectConverter : IDisposable
	{
		public CustomsRelatedBusinessObjectConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
		{
			shipmentPK = relatedJobCommand.Shipment.PK;
			tokenSource = new CancellationTokenSource();
			this.relatedJobCommand = relatedJobCommand;
		}

		public void Cancel()
		{
			tokenSource.Cancel();
		}

		public void SetUpProgressUpdate(Action<string, string, int> progressUpdate)
		{
			this.progressUpdate = progressUpdate;
		}

		readonly ZGuid shipmentPK;
		readonly CancellationTokenSource tokenSource;
		Action<string, string, int> progressUpdate;
		readonly BaseHVLVRelatedJobCommand relatedJobCommand;

		public ReadOnlyCollection<BusinessObject> CustomsRelatedBusinessCollection { get; private set; }

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		public ForwardingShipment Shipment => shipment ?? (shipment = Factory.Load<ForwardingShipment>(shipmentPK));
		ForwardingShipment shipment;

		public ZString TargetBusinessObjectDisplayName => relatedJobCommand.RelatedJobName;

		public string UsageCode => relatedJobCommand.UsageCode;

		public virtual RecipientRoleType? RecipientRoleType => null;

		public virtual DataContextType MasterBillDataContextType => default;

		public virtual CodeDescriptionPair ManifestType => new CodeDescriptionPair();

		protected virtual bool IncludeAdditionalReferenceCollectionOverride => false;

		protected virtual void OnConversionSucceeded()
		{
		}

		public bool TryConvert(out string errorMessage, BusinessObjectFactory factory = null)
		{
			var result = false;
			errorMessage = null;

			if (factory != null)
			{
				this.factory = factory;
			}

			using (factory == null ? Db.DisposableActionForDbConnection() : null)
			{
				result = TryConvertCore(out errorMessage);
			}

			return result;
		}

		bool TryConvertCore(out string errorMessage)
		{
			var result = false;
			errorMessage = null;
			try
			{
				if (ShouldStripNonWesternEuropeanCharacters)
				{
					Factory.ServiceContainer.AddService(new NonWesternEuropeanCharactersRemovalService());
				}

				ConvertCore();
				CreatePivotBetweenConsignmentHeaderAndCustomsRelatedBusinessObjects();
				SetItemLastUsageCode();
				OnConversionSucceeded();
				result = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errorMessage = ex.Message;
			}
			finally
			{
				Factory.ServiceContainer.RemoveService<NonWesternEuropeanCharactersRemovalService>();
			}

			return result;
		}

		void ConvertCore()
		{
			var tracker = new ConvertTracker(Shipment.HVLVConsignments.Count(), tokenSource.Token, progressUpdate);
			Shipment dataObject = null;
			BusinessObject[] masterBills = null;
			using (PerformanceStatisticsCollector.StartMonitoring(GetType().Name, nameof(ExportShipmentAsUniversalDataObject)))
			{
				dataObject = ExportShipmentAsUniversalDataObject(tracker, checkSubShipments: true);
				CreateManifestStyleEntryInstruction(dataObject, Shipment);
			}

			using (PerformanceStatisticsCollector.StartMonitoring(GetType().Name, nameof(ReadIntoCustomsRelatedBusiness)))
			{
				masterBills = ReadIntoCustomsRelatedBusiness(dataObject, MasterBillDataContextType, tracker);
			}

			CustomsRelatedBusinessCollection = new ReadOnlyCollection<BusinessObject>(masterBills);

			if (tokenSource == default || !tokenSource.IsCancellationRequested)
			{
				HookConversionFactoryEventHandlers();
				tracker.UpdateCaption(Res.GetString("ee9421fb-0c90-4d26-b70a-0987ee280f90", "{0} generated successfully.", TargetBusinessObjectDisplayName));
			}
		}

		protected virtual void CreateManifestStyleEntryInstruction(Shipment dataObject, ForwardingShipment forwardingShipment)
		{
		}

		void HookConversionFactoryEventHandlers()
		{
			Factory.Saving -= OnConversionFactorySaving;
			Factory.Saving += OnConversionFactorySaving;
			Factory.Saved -= OnConversionFactorySaved;
			Factory.Saved += OnConversionFactorySaved;
		}

		protected virtual bool ShouldStripNonWesternEuropeanCharacters => GetRegistrySettingFromCurrentBranchAndShipment();

		bool GetRegistrySettingFromCurrentBranchAndShipment()
		{
			var result = false;
			switch (GlbBranch.CurrentBranch.Country.Code)
			{
				case CountryCodes.UnitedStates:
					result = HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSLowValueEntries.Value;
					break;
				case CountryCodes.Australia:
					if (Shipment.IsExport())
					{
						result = HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUExportSubManifest.Value;
					}
					else if (Shipment.IsAir)
					{
						result = HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUAirCargoReport.Value;
					}
					else if (Shipment.IsSea)
					{
						result = HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUSeaCargoReport.Value;
					}
					break;
				case CountryCodes.NewZealand:
					if (Shipment.IsAir)
					{
						result = HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersNZAirCargoReport.Value;
					}
					else if (Shipment.IsSea)
					{
						result = HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersNZSeaCargoReport.Value;
					}
					break;
				case CountryCodes.Singapore:
					result = HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersSGSGAccessImportManifest.Value;
					break;
			}
			return result;
		}

		protected virtual void OnConversionFactorySaving(BusinessObjectFactory factory)
		{
			var reason = EventReferenceParameterReasons.CargoReportCreated;
			if (Shipment.IsCargoReportCreated())
			{
				reason = EventReferenceParameterReasons.CargoReportAmended;
			}

			Shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason));

			foreach (var customsRelatedBusiness in CustomsRelatedBusinessCollection)
			{
				customsRelatedBusiness.OnSaving();
				var eventParameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Mode, Shipment.TransportMode),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, ((ICodeDescription)customsRelatedBusiness).Code),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, TargetBusinessObjectDisplayName)
				};
				Shipment.Logs.AddNew(AutoEvents.Transferred, eventParameters.ToArray());

				if (customsRelatedBusiness is IStmALogParent logParent)
				{
					var manifestParameters = new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Shipment.IsHighVolumeLowValue ? ShipmentTypes.HighVolumeLowValue : ZString.Empty.ToString()),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, Shipment.JobNumber)
					};
					logParent.Logs.AddNew(AutoEvents.Transferred, manifestParameters.ToArray());
				}
			}
		}

		protected virtual void OnConversionFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				Factory.Saving -= OnConversionFactorySaving;
				Factory.Saved -= OnConversionFactorySaved;
			}
		}

		void CreatePivotBetweenConsignmentHeaderAndCustomsRelatedBusinessObjects()
		{
			var header = Shipment.GetHVLVConsignmentHeader();
			if (header != null)
			{
				foreach (var bizo in CustomsRelatedBusinessCollection)
				{
					header.GenPivotCollection.AddRelatedIfNotExist(bizo);
				}
			}
		}

		void SetItemLastUsageCode()
		{
			if (!string.IsNullOrEmpty(UsageCode))
			{
				Shipment.SetLastUsageCodeForAllItems(UsageCode);
			}
			else
			{
				var errorMessage = $@"Usage Code should not be null or empty.

-- Additional Information --

ShipmentPK: {shipmentPK}
Type of CustomsRelatedBusinessObjectConverter: {GetType().Name}
TargetBusinessObjectDisplayName: {TargetBusinessObjectDisplayName}
Type of relatedJobCommand: {relatedJobCommand.GetType().Name}
";

				ErrorReporter.ReportOnce("CustomsRelatedBusinessObjectConverter|SetItemLastUsageCodeEmpty",
					errorMessage);
			}
		}

		BusinessObject[] ReadIntoCustomsRelatedBusiness(Shipment dataObject, DataContextType masterBillDataContextType, ConvertTracker tracker)
		{
			var result = default(BusinessObject[]);
			ITopLevelDataObjectReader customsRelatedBusinessReader;
			var universalFactory = new UniversalObjectFactory(Factory);

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.UnitedStates
				&& Shipment.IsRoad
				&& masterBillDataContextType == DataContextType.USCustomsLowValueEntriesClearance)
			{
				customsRelatedBusinessReader = ObjectFactory.Get<ITopLevelDataObjectReader>("eTailUSLVClearanceDataObjectReader", dataObject, dataObject, tracker, universalFactory);
			}
			else
			{
				customsRelatedBusinessReader = GetCustomsRelatedBusinessReader(dataObject, masterBillDataContextType, tracker, universalFactory);
			}

			tracker.UpdateCaption(Res.GetString("7c0f620e-1b4a-4ce7-a573-968442642e9b", "Reading into {0}", TargetBusinessObjectDisplayName));

			using (new DisposableAction(() => Factory.SuspendValidation(), () => Factory.ResumeValidation()))
			using (DataObjectReader.IgnoreInactiveTargetForMatching())
			{
				var populatedObject = customsRelatedBusinessReader.ReadIntoTopLevelBusinessObject();

				if (populatedObject == null && customsRelatedBusinessReader is MultipleTopLevelObjectReadersWrapper multipleReadersWraper)
				{
					var populatedObjects = multipleReadersWraper.PopulatedBusinessObjects;
					if (populatedObjects == null || populatedObjects.Any(n => n == null))
					{
						ThrowException(Res.GetString("06117cd4-b294-4d94-a8e3-6e5f4b72c302", "Failed to read into multiple {0} from Shipment [{1}]", masterBillDataContextType, Shipment.JS_UniqueConsignRef) + "\r\n" + tracker.GetErrorDetail());
					}

					result = populatedObjects.ToArray();
				}
				else if (populatedObject != null)
				{
					result = new[] { populatedObject };
				}
			}

			if (result == null || tracker.HasError)
			{
				ThrowException(Res.GetString("c5edc6a0-8294-4c38-8943-ed965e954be1", "Failed to read into {0} from Shipment [{1}]", masterBillDataContextType, Shipment.JS_UniqueConsignRef) + "\r\n" + tracker.GetErrorDetail());
			}

			return result;
		}

		static ITopLevelDataObjectReader GetCustomsRelatedBusinessReader(Shipment dataObject, DataContextType masterBillDataContextType, ConvertTracker tracker, UniversalObjectFactory universalFactory)
		{
			var customsRelatedBusinessContextManager = masterBillDataContextType.GetUniversalDataContextManager() as IShipmentDataContextManagerInternal;
			return customsRelatedBusinessContextManager.GetShipmentDataObjectReader(dataObject, tracker, universalFactory);
		}

		public Shipment ExportShipmentAsUniversalDataObject(ConvertTracker tracker, bool checkSubShipments = false)
		{
			Shipment result;

			tracker.UpdateCaption(Res.GetString("55cd8343-07d6-4754-848b-0afa4bfbdc85", "Loading HVLV Consignment data"));
			tracker.UpdateProgress(Res.GetString("2c6a8d0d-f606-4ea1-bb5e-d6b952a6be9a", "Preparing data source"));

			var recipientRole = RecipientRoleType;

			var writingManager = new DataWritingManager(new ActionInfo(new[] { recipientRole.GetValueOrDefault(), PickupOrDeliveryCartageRole }.ToRecipientRoleDetails(), Shipment), tracker);
			writingManager.FilteredDataContextType = MasterBillDataContextType;
			var shipmentWriter = new ShipmentDataObjectWriter(writingManager, checkSubShipments, true, includeAdditionalReferenceCollectionForSubshipments: IncludeAdditionalReferenceCollectionOverride);

			using (writingManager.SetIsPublishingInternally())
			{
				result = shipmentWriter.GetDataObject(Shipment) ??
					throw new InvalidOperationException(Res.GetString("f33f2bfb-5343-4400-bfac-cfbd0104c11c", "Unable to export Shipment [{0}]", Shipment.JS_UniqueConsignRef) + "\r\n" + tracker.GetErrorDetail());
			}

			result.MessageType = ManifestType;
			result.DataContext.CodesMappedToTarget = true;
			result.DataContext.SetWorkflowInfo(new WorkflowInfo
			{
				EventType = new CodeDescriptionPair { Code = AutoEvents.HVLVReadyCode },
				RecipientRoles = recipientRole?.ToRecipientRoleDetails()
			});

			OnDataObjectExported(result);
			tracker.TopLevelDataObject = result;
			tracker.ResetCount();

			return result;
		}

		public virtual void OnDataObjectExported(Shipment result)
		{
			PopulateDataTargetForMatchingExistingJob(result);
			PopulateAddInfoCollection(Shipment, result);
			PopulateDeclarantType(Shipment.ArrivalConsol, result);

			ClearJobDocAddress();
			PopulateEntryHeaderCollection(result);
		}

		protected virtual RecipientRoleType PickupOrDeliveryCartageRole => Shipment.JobDirection == Directions.Import ? UniversalDataBuss.Integration.RecipientRoleType.DCA : UniversalDataBuss.Integration.RecipientRoleType.PCA;

		void ThrowException(string errorMessage)
		{
			throw new InvalidOperationException(errorMessage);
		}

		public void Dispose()
		{
			tokenSource.Dispose();
		}

		void ClearJobDocAddress()
		{
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var jobDocAddresses = factory.Load<JobDocAddress>(query);

			foreach (var jobDocAddress in jobDocAddresses)
			{
				if (!jobDocAddress.IsInDatabase)
				{
					jobDocAddress.Delete();
				}
			}
		}

		void PopulateDataTargetForMatchingExistingJob(ITopLevelDataObject dataObject)
		{
			var existingJob = relatedJobCommand.ActiveRelatedCustomsJobs.FirstOrDefault();
			if (existingJob != null)
			{
				var contextManager = MasterBillDataContextType.GetUniversalDataContextManager();
				contextManager.Init(existingJob);
				PopulateDataTarget(dataObject, MasterBillDataContextType, contextManager.DataContextKey);
			}
		}

		protected virtual void PopulateDataTarget(ITopLevelDataObject dataObject, DataContextType dataContextType, ZString dataContextKey)
		{
			dataObject.DataContext.AddDataTarget(dataContextType, dataContextKey);
		}

		void PopulateEntryHeaderCollection(Shipment dataObject)
		{
			dataObject.SetEntryHeaderCollection(() => new List<EntryHeader>()
			{
				new EntryHeader(DefaultDataObjectWriterStrategy.Instance)
				{
					Type = new EntryType() { Code = EntryCountryCode },
				}
			});
		}

		void PopulateAddInfoCollection(ForwardingShipment shipmentBO, Shipment dataObject)
		{
			if (IsUSFreight(shipmentBO))
			{
				var scacCode = GetSCACCode(shipmentBO);
				if (scacCode.HasValue)
				{
					var addInfoCollection = dataObject.AddInfoCollection ?? new List<AddInfo>();
					addInfoCollection.Add(AddInfo.New(Constants.AddInfoKeys.MasterWayBillIssuerSCAC, scacCode.Value));
					addInfoCollection.Add(AddInfo.New(Constants.AddInfoKeys.CarrierSCAC, scacCode.Value));
					dataObject.SetAddInfoCollection(() => { return addInfoCollection; });
				}
			}
		}

		protected virtual void PopulateDeclarantType(ForwardingConsol consolBO, Shipment dataObject)
		{
			dataObject.DeclarantType = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_AgentType, consolBO.JK_AgentType_List);
		}

		bool IsUSFreight(ForwardingShipment shipmentBO)
		{
			return shipmentBO.Destination != null && shipmentBO.Destination.Country.Code == CountryCodes.UnitedStates;
		}

		ZString? GetSCACCode(ForwardingShipment shipmentBO)
		{
			var consolBO = shipmentBO.ArrivalConsol;
			var scacCode = consolBO?.ShippingLine?.SCACCode;

			if (shipmentBO.TransportMode == TransportModes.Air)
			{
				scacCode = consolBO?.TwoLetterAirlineCode;
			}
			else if (shipmentBO.TransportMode == TransportModes.Sea || shipmentBO.TransportMode == TransportModes.Rail)
			{
				if (!scacCode.HasValue || scacCode.Value.IsEmpty)
				{
					scacCode = GlbCompany.CurrentCompany.OrgProxy?.SCACCode;
				}
			}
			return scacCode;
		}

		protected virtual string EntryCountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public bool TryAcquireApplicationLock(Action actionForLockedRows, out string errorMessage)
		{
			return shipmentPK.TryAcquireApplicationLock<ForwardingShipment>(relatedJobCommand.GetType().Name, actionForLockedRows, out errorMessage);
		}
	}
}
