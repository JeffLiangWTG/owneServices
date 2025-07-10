using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Metadata.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.TransportConsignment.Registry;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.TransportConsignment.Business
{
	[UserDefinedValues]
	[CodeProperty(DtbConsignmentSchema.Constants.LTC_JobID)]
	[DescriptionProperty(DtbConsignmentSchema.Constants.LTC_ConnoteNumber)]
	[UniversalDataContext(DataContextType.LandTransportConsignment)]
	[MetadataContext(MetadataContext.LandTransportConsignment)]
	[VisualizableDocumentsSupportable(nameof(DtbConsignmentVisualizableDocumentSupporter))]
	public class DtbConsignment : AutoDtbConsignment,
		IAdditionalReferenceNumberTypeProvider,
		ITransportAdditionalReferenceNumbers,
		IDocManagerSupport,
		IEDocsProvider,
		IJobNumber,
		IJobInvoicingPlugIn,
		IJobInvoicingAdditionalData,
		IDocAddresses,
		IDocumentSupportable,
		IJobHeaderParent,
		IPackingParent,
		IPackingParentSupportsImportingBookedDimensions,
		IDtbConsignment,
		IRelatedJob,
		IRatingSupporter,
		IStmNoteParent,
		IHaveServicesWithContext,
		IUniversalXMLNoteParent,
		IWorkflowProvider,
		ICustomFieldProvider
	{
		public DtbConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region OnLoaded

		public sealed override void OnLoaded()
		{
			base.OnLoaded();

			if(!LTC_ClosedTime.IsEmpty)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}
		#endregion

		#region OnSaving

		public sealed override void OnSaving()
		{
			PopulateUniqueIDIfNeeded();
			PopulateJobType();
			PopulateDefaultsFromOrganizations();
			PopulateBranch();

			if (IsCancelled && IsCancelledHasChanged)
			{
				DeleteJobHeader();
			}

			base.OnSaving();
		}

		internal void PopulateUniqueIDIfNeeded()
		{
			if (!IsInDatabase && !IsDeleted && LTC_JobID.IsEmpty)
			{
				LTC_JobID = GenerateID();
			}
		}

		internal void PopulateJobType()
		{
			LTC_JobType = GetJobType();
		}

		void PopulateDefaultsFromOrganizations()
		{
			var shouldFillIncoterm = string.IsNullOrWhiteSpace(LTC_Incoterm);
			var shouldFillServiceLevel = string.IsNullOrWhiteSpace(LTC_RS_NKServiceLevel);

			if (!shouldFillIncoterm && !shouldFillServiceLevel)
			{
				return;
			}

			var pickupAddressPK = PickupOrgAddress?.PK ?? Guid.Empty;
			var deliveryAddressPK = DeliveryOrgAddress?.PK ?? Guid.Empty;

			var values = Factory.GetCachedValue(
				$"{nameof(PopulateDefaultsFromOrganizations)}.{pickupAddressPK}.{deliveryAddressPK}",
				() => ConsignmentService.GetDefaultValuesForConsignment(pickupAddressPK, deliveryAddressPK, TablePrefix, Factory));

			if (shouldFillIncoterm)
			{
				LTC_Incoterm = values[DtbConsignmentService.IncoTermKey];
			}

			if (shouldFillServiceLevel)
			{
				LTC_RS_NKServiceLevel = values[DtbConsignmentService.ServiceLevelKey];
			}
		}

		void PopulateBranch()
		{
			if (LTC_GB_Branch.IsEmpty)
			{
				var manager = ObjectFactory.Get<IControllingBranchDefaultingManager>();
				SetDefaultBranch(manager);
			}
		}

		protected IDtbConsignmentService ConsignmentService => consignmentService ??= GetConsignmentService();
		IDtbConsignmentService consignmentService;

		protected virtual IDtbConsignmentService GetConsignmentService()
		{
			return new DtbConsignmentService();
		}

		ZString GenerateID()
		{
			NumberGenerator.Generate();
			NumberGenerator.EnforceMaxLengths();

			return NumberGenerator.PrimaryTarget.Value;
		}

		ZString GetJobType()
		{
			if (PkgPackageJob.LoadPackageJob(this) == null || !Containers.Any())
			{
				return "LTL";
			}
			else
			{
				return "FCL";
			}
		}
		#endregion

		#region OnSaved

		public sealed override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				OnSaveFailed();
			}
		}

		void OnSaveFailed()
		{
			if (!IsInDatabase)
			{
				LTC_JobID = ZString.Empty; // do this last
			}
		}

		#endregion

		#region NumberGenerator

		NumberGenerator NumberGenerator
		{
			get { return numberGenerator ?? (numberGenerator = GetNewNumberGenerator()); }
		}

		NumberGenerator numberGenerator;

		NumberGenerator GetNewNumberGenerator()
		{
			var generator = new NumberGenerator
			{
				Factory = Factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.DtbConsignmentID,
				FountainGetter = Env.NumberFountains.GetDtbTransportGeneratorFountain,
				PrimaryTarget = new ConsignmentNumberGeneratorTarget(this)
			};

			generator.ValueProviders.AddRange(new StandardValueSource().Concat(new LTValueSource(this)));
			return generator;
		}

		#endregion

		#region Delete / Deactivate

		public override void Delete()
		{
			Array.ForEach(Addresses.ToArray(), i => i.Delete());
			DeleteJobHeader();
			DeletePackageJob(); // tested by PackingParentTestCase
			base.Delete();

			// tested by WorkflowProvider tests
			WorkflowItems.RemoveAndDeleteAll();
		}

		void DeletePackageJob()
		{
			var packageJobToDelete = PkgPackageJob.LoadPackageJob(this);
			if (packageJobToDelete != null)
			{
				packageJobToDelete.Delete();
			}
		}

		void DeleteJobHeader()
		{
			var job = Job;
			if (job != null && job.CanDelete && job.JH_ParentID == this.PK)
			{
				job.Delete();
			}
		}

		public override string CanCancel()
		{
			return JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(PK, HumanReadableName);
		}

		public override string CanReactivate()
		{
			var transportBooking = Factory.Load<DtbBooking>(LTC_KM_Booking);
			if (!LTC_IsActive && transportBooking != null && (transportBooking.KM_Status == TransportStatuses.Codes.Booked || transportBooking.KM_Status == TransportStatuses.Codes.ServiceCommenced || !transportBooking.KM_IsActive))
			{
				return Res.GetString("f13ea2ff-cd8a-4d3e-b56c-32c5237de954", "The parent booking of this consignment is not available, so this consignment cannot be reactivated.");
			}

			return base.CanReactivate();
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Properties

		#region LTC_JobID

		public override ZString LTC_JobID
		{
			get { return base.LTC_JobID; }
			set
			{
				var originalCostReference = LTC_JobID;
				base.LTC_JobID = value;
				UpdateChargeCostReferences(originalCostReference);
			}
		}

		void UpdateChargeCostReferences(ZString originalCostReference)
		{
			ObjectFactory.Get<IJobProviderForInvoicing>().ParentOperationalJobRefChanged(originalCostReference, this, IsInDatabase);
		}

		#endregion

		#region LTC_IsActive

		public override ZBool LTC_IsActive
		{
			get => base.LTC_IsActive;
			set
			{
				var originalValue = base.LTC_IsActive;
				base.LTC_IsActive = value;
				if (value != originalValue)
				{
					LogServiceStatusOnActivateDeactivate();
				}
			}
		}

		void LogServiceStatusOnActivateDeactivate()
		{
			StmALog log;
			if (LTC_IsActive)
			{
				log = LogServicesCommenced();
			}
			else
			{
				log = LogServicesCancelled();
			}
			PublishEventsOnSaved.Add(log);
		}

		StmALog LogServicesCommenced()
		{
			return Logs.CreateRecreateOrUpdateEventLog(AutoEvents.ServiceCommenced, EstimateActual.Actual, ZDateTimeOffset.Now, LTC_JobID,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Transport));
		}

		StmALog LogServicesCancelled()
		{
			return Logs.CreateRecreateOrUpdateEventLog(AutoEvents.ServiceCancelled, EstimateActual.Actual, ZDateTimeOffset.Now, LTC_JobID,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Transport));
		}

		#endregion LTC_IsActive

		#endregion Properties

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return LTC_JobID.IsEmpty
					? Res.GetString("6ca9b81d-427c-46cd-b601-73f2c262e218", "Land Transport Consignment")
					: Res.GetString("6f3f83ec-6317-40ed-a818-764c44622acb", "Land Transport Consignment {0}", LTC_JobID);
			}
		}

		#endregion

		#region IAdditionalReferenceNumberTypeProvider Members

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			var referenceTypes = new CodeDescriptionPairList();
			foreach (var regType in LandTransportRegistry.Instance.LandTransportConsignmentAdditionalReferenceNumbers.Value)
			{
				referenceTypes.Add(regType);
			}

			return referenceTypes;
		}

		#endregion

		#region AdditionalReferenceNumbers

		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumAdditionalReferenceCollectionProvider>();
					additionalReferenceNumbers = provider.GetCollection(this);

					var additionalReferenceNumbersBusinessObjectCollection = additionalReferenceNumbers as BusinessObjectCollection;
					if (additionalReferenceNumbersBusinessObjectCollection != null)
					{
						additionalReferenceNumbersBusinessObjectCollection.Load();
					}
				}

				return additionalReferenceNumbers;
			}
		}

		ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		#endregion

		#region Addresses

		public DtbConsignmentAddressCollection Addresses
		{
			get { return new DtbConsignmentAddressCollection(this); }
		}

		#endregion

		#region PickupAddress

		public DtbConsignmentAddress PickupAddress
		{
			get
			{
				var pickups = Addresses.Where(a => a.LTS_InstructionType == ConsignmentAddressTypes.Codes.PickUp).ToArray();

				if (pickups.Length > 1)
				{
					ErrorReporter.ReportOnce(string.Format("Consignment contains {0} Pickup Address.", pickups.Length));
				}

				return pickups.FirstOrDefault();
			}
		}

		public OrgAddress PickupOrgAddress
		{
			get
			{
				return PickupAddress?.DocAddresses.Cast<JobDocAddress>().SingleOrDefault()?.Address;
			}
		}

		#endregion

		#region DeliveryAddress

		public DtbConsignmentAddress DeliveryAddress
		{
			get { return Addresses.SingleOrDefault(a => a.LTS_InstructionType == ConsignmentAddressTypes.Codes.Delivery); }
		}

		public OrgAddress DeliveryOrgAddress
		{
			get
			{
				return DeliveryAddress?.DocAddresses.Cast<JobDocAddress>().SingleOrDefault()?.Address;
			}
		}

		#endregion

		#region Variations

		public DtbConsignmentVariationCollection Variations => variations ??= new DtbConsignmentVariationCollection(this);
		DtbConsignmentVariationCollection variations;

		#endregion

		#region Original Consignor 

		public JobDocAddress OriginalConsignor  => originalConsignor ??= DocAddresses.FindByDocAddressType(DocAddressType.ConsignorAddress);
		JobDocAddress originalConsignor;

		#endregion

		#region Final Consignee 

		public JobDocAddress FinalConsignee => finalConsignee ??= DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress);
		JobDocAddress finalConsignee;

		#endregion

		#region SystemHeld

		public ZBool SystemHeld
		{
			get { return GetSystemHeld(); }
		}

		ZBool GetSystemHeld()
		{
			var hasInvalidAddress = Addresses.Any(address => !IsAddressValid(address.Address));
			var hasNoPackage = PackageJob.Packages.IsNullOrEmpty();

			return hasInvalidAddress || hasNoPackage || CheckMandatoryFieldsAreEmpty();
		}

		bool IsAddressValid(JobDocAddress address)
		{
			return (address.E2_AddressOverride && !address.Address1.IsEmpty && !address.E2_RN_NKCountryCode.IsEmpty) ||
				   (!address.E2_AddressOverride && !address.E2_OA_Address.IsEmpty);
		}

		bool CheckMandatoryFieldsAreEmpty()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;

			return mandatoryCollection
				.Cast<CodeDescriptionBool>()
				.Where(mandatoryField => mandatoryField.Bool)
				.Any(mandatoryField =>
			{
				switch (mandatoryField.Code)
				{
					case LandTransportRegistry.ConsignmentMandatoryFieldOptions.BillToParty when
						ClientRequestedBillingPartyAddress == null || !IsAddressValid(ClientRequestedBillingPartyAddress):
					case LandTransportRegistry.ConsignmentMandatoryFieldOptions.BookingParty when
						BookingPartyAddress == null || !IsAddressValid(BookingPartyAddress):
					case LandTransportRegistry.ConsignmentMandatoryFieldOptions.ControllingBranch when
						LTC_GB_Branch.IsEmpty:
					case LandTransportRegistry.ConsignmentMandatoryFieldOptions.ServiceLevel when
						LTC_RS_NKServiceLevel.IsEmpty:
					case LandTransportRegistry.ConsignmentMandatoryFieldOptions.ConsignmentNote when
						LTC_ConnoteNumber.IsEmpty:
						return true;
					default:
						return false;
				}
			});
		}

		#endregion

		#region BookingPartyAddress

		public JobDocAddress BookingPartyAddress
		{
			get { return DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress); }
		}

		#endregion

		#region ClientRequestedBillingPartyAddress

		public JobDocAddress ClientRequestedBillingPartyAddress
		{
			get { return DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty); }
		}

		#endregion

		#region IsPickUpDirection

		public bool IsPickUpDirection
		{
			get { return LTC_Direction == Constants.CartageDirection.Origin || LTC_Direction == Constants.CartageDirection.Export; }
		}

		#endregion

		#region IsDeliveryDirection

		public bool IsDeliveryDirection
		{
			get { return LTC_Direction == Constants.CartageDirection.Destination || LTC_Direction == Constants.CartageDirection.Import; }
		}

		#endregion

		#region GetAllActions

		public IReadOnlyCollection<DtbConsignmentAction> AllActions
		{
			get { return Addresses?.SelectMany(a => a.Actions).ToArray(); }
		}

		#endregion

		#region Variations

		DtbConsignmentVariation[] GetVariationsFromDatabase()
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignmentVariation));
			query.AddToFilter(DtbConsignmentVariationSchema.LTV_ParentTableCode,
				DtbConsignmentActionPackageDivotSchema.Constants.Prefix);

			var divotSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentActionPackageDivot), DtbConsignmentActionPackageDivotSchema.PK);
			var actionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAction), DtbConsignmentActionSchema.PK);
			var addressSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAddress), DtbConsignmentAddressSchema.PK);
			addressSubQuery.AddToFilter(DtbConsignmentAddressSchema.LTS_LTC_Consignment, PK);

			actionSubQuery.AddSubQuery(DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress, addressSubQuery, JoinCondition.And);
			divotSubQuery.AddSubQuery(DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction, actionSubQuery, JoinCondition.And);
			query.AddSubQuery(DtbConsignmentVariationSchema.LTV_ParentId, divotSubQuery, JoinCondition.And);

			return Factory.Load<DtbConsignmentVariation>(query);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DtbConsignmentDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new DtbConsignmentDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region PackageJob

		public PkgPackageJob PackageJob
		{
			get { return packageJob ?? (packageJob = GetPackageJob()); }
		}

		PkgPackageJob GetPackageJob()
		{
			var packageJobToReturn = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(this);
			return packageJobToReturn;
		}

		PkgPackageJob packageJob;

		#endregion

		#region Containers

		public IEnumerable<PkgPackage> Containers
		{
			get { return PackageJob.Packages.Where(p => p.IsContainer); }
		}

		#endregion

		#region LoosePackages

		public IEnumerable<PkgPackage> LoosePackages
		{
			get { return PackageJob.Packages.Where(p => !p.IsContainer); }
		}

		#endregion

		#region IsLooseOnly

		public bool IsLooseOnly
		{
			get { return true; }
		}

		#endregion

		#region GetTotalPackageVolume

		public ZVolume GetTotalLoosePackageVolume()
		{
			return GetTotalVolume(LoosePackages);
		}

		public ZVolume GetTotalContainerisedVolume()
		{
			return GetTotalVolume(Containers);
		}

		ZVolume GetTotalVolume(IEnumerable<PkgPackage> packages)
		{
			var groupedPackages = packages.Where(p => p.KP_KP_ParentPackage == ZGuid.Empty)
				.GroupBy(p => new { VolumeUnit = p.KP_VolumeUQ })
				.Select(p => new
				{
					Key = p.Key,
					Volume = p.Sum(package => package.KP_Volume)
				});

			var result = new ZVolume(0, DtbTransportTotalsHelper.TotalVolumeUnit);

			foreach (var item in groupedPackages)
			{
				var volume = new ZVolume(item.Volume, item.Key.VolumeUnit);
				if (volume.IsValid)
				{
					result += volume;
				}
			}

			return result;
		}

		#endregion

		#region GetTotalPackageWeight

		public ZWeight GetTotalLoosePackageWeight()
		{
			return GetTotalWeight(LoosePackages);
		}

		public ZWeight GetTotalContainerisedWeight()
		{
			return GetTotalWeight(Containers);
		}

		ZWeight GetTotalWeight(IEnumerable<PkgPackage> packages)
		{
			var groupedPackages = packages.Where(p => p.KP_KP_ParentPackage == ZGuid.Empty)
				.GroupBy(p => new { WeightUnit = p.KP_WeightUQ })
				.Select(p => new
				{
					Key = p.Key,
					Weight = p.Sum(package => package.KP_Weight)
				});

			var result = new ZWeight(0, DtbTransportTotalsHelper.TotalWeightUnit);

			foreach (var item in groupedPackages)
			{
				var volume = new ZWeight(item.Weight, item.Key.WeightUnit);
				if (volume.IsValid)
				{
					result += volume;
				}
			}

			return result;
		}

		#endregion

		#region Port Details

		#region PortOfOrigin

		public ZString PortOfOrigin
		{
			get { return GetUNLOCO(PickupAddress); }
		}

		#endregion

		#region PortOfDestination

		public ZString PortOfDestination
		{
			get { return GetUNLOCO(DeliveryAddress); }
		}

		#endregion

		#region GetUNLOCO

		ZString GetUNLOCO(DtbConsignmentAddress address)
		{
			ZString result;
			var docAddress = address?.Address;
			result = docAddress?.Address?.OA_RL_NKRelatedPortCode ?? "";

			if (result.IsEmpty)
			{
				result = docAddress?.Organisation.OH_RL_NKClosestPort ?? "";
			}

			return result;
		}

		#endregion

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region OnFactorySaved

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (!isInOnFactorySaved)
			{
				if (saveSucceeded && PublishEventsOnSaved.Any(l => l != null))
				{
					var transportBooking = Factory.Load<DtbBooking>(LTC_KM_Booking);
					if (transportBooking != null)
					{
						var old = isInOnFactorySaved;
						isInOnFactorySaved = true;
						try
						{
							var factory = new BusinessObjectFactory();
							using (factory.AddDisposableService())
							{
								foreach (var log in PublishEventsOnSaved.Where(l => !(l?.IsDeleted ?? true)))
								{
									PublishUniversalEventCore.PublishUniversalEvent(factory, this, transportBooking, log);
								}
								factory.Save();
							}
						}
						finally
						{
							isInOnFactorySaved = old;
						}
					}

					PublishEventsOnSaved.Clear();
				}
			}
		}

		[ThreadStatic]
		static bool isInOnFactorySaved;

		List<StmALog> PublishEventsOnSaved
		{
			get { return publishEventsOnSaved ?? (publishEventsOnSaved = new List<StmALog>()); }
		}
		List<StmALog> publishEventsOnSaved;

		#endregion OnFactorySaved

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot => false;

		#endregion

		#region IPackingParent Members

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
		{
			return new PackageActionStrategy(package);
		}

		ControllerID IPackingParent.ControllerID
		{
			get { return ControllerIDs.DtbConsignment; }
		}

		DocumentOptions IPackingParent.DocumentOptions
		{
			get { return DocumentOptions.None; }
		}

		ZString IPackingParent.JobNo
		{
			get { return LTC_JobID; }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return LTC_ConnoteNumber; }
		}

		ZString IPackingParent.JobDescription
		{
			get { return Res.GetString("9d0866f1-2706-4625-b43c-1a0242f0d80d", "Consignment"); }
		}

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context)
		{
			return "";
		}

		void IPackingParent.OnPackageJobReleased()
		{
		}

		void IPackingParent.OnPackageJobCreatedOrLoaded(PkgPackageJob pkgJob)
		{
		}

		void IPackingParent.OnPackageDelete(PkgPackage package)
		{
		}

		void IPackingParent.OnContainerIDChanged(PkgPackage container)
		{
		}

		bool IPackingParent.IsParentJobFinalised
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		bool IPackingParent.IsPackingJobReadOnly
		{
			get { return false; }
		}

		bool IPackingParent.IsAutoPrintAllowed
		{
			get { return false; }
		}

		bool IPackingParent.IsScanEventsVisible
		{
			get { return true; }
		}

		ZString IPackingParent.CarrierServiceLevelCode(PkgPackage package)
		{
			return "";
		}

		OrgHeader IPackingParent.CarrierBookingAgent => null;

		ZString IPackingParent.TransportReference { get => ZString.Empty; set { } }

		OrgHeader IPackingParent.GetCarrier(PkgPackage package) => null;

		bool IPackingParent.IsLoosePackageIDsSupported => true;

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
		{
		}

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => false;

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> IPackingParent.GetAdditionalEventContextValuesFromParent() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		ParentJobType IPackingParent.ParentJobType => ParentJobType.None;

		PackageSequenceType IPackingParent.PackageSequenceType => PackageSequenceType.Outer;

		bool IPackingParent.CanReleasePackage(PkgPackage package) => true;

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
		{
		}

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

		#endregion

		#region Job

		public JobHeader Job
		{
			get { return new JobHeader.Loader(this).Load(); }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		#region InvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn
		{
			get { return invoicingPlugIn ?? (invoicingPlugIn = new DtbConsignmentInvoicingPlugIn(this)); }
		}

		IJobInvoicingPlugIn invoicingPlugIn;

		#endregion

		#region IJobInvoicingPlugIn_InvoicingSupporter

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = InvoicingPlugIn.InvoicingSupporter); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return InvoicingPlugIn.AllowInvoiceDeletion; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			InvoicingPlugIn.OnJobCreating(job);

			if (job != null && !job.IsDeleting)
			{
				if (!LTC_KM_Booking.IsEmpty)
				{
					var transportBooking = Factory.Load<DtbBooking>(LTC_KM_Booking);
					var transportBookingJob = transportBooking?.Job;
					if (transportBookingJob != null)
					{
						job.JH_JH_ParentJob = transportBookingJob.PK;
					}
				}
			}
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
			JobCreated?.Invoke(this, null);
		}
		public event EventHandler JobCreated;

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
			InvoicingPlugIn.OnJobDeleting(job);
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			InvoicingPlugIn.SetJobNumberFieldOnSaving();
		}

		#endregion

		#region IJobHeaderParentCore Members

		ZGuid IJobHeaderParentCore.PK
		{
			get { return InvoicingPlugIn.PK; }
		}

		string IJobHeaderParentCore.TableName
		{
			get { return InvoicingPlugIn.TableName; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return InvoicingPlugIn.JobNumber; }
		}

		#endregion

		#endregion

		#region IEDocsProvider members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IHaveServicesWithContext Members

		ZString IHaveServices.ContainerMode
		{
			get { return ""; }
		}

		IHaveServices[] IHaveServices.DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		BusinessObject IHaveServices.ServiceParent
		{
			get { return this; }
		}

		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new ConsignmentJobServiceDependentCollection(this, Factory);
					services.Load();
				}

				return services;
			}
		}
		ConsignmentJobServiceDependentCollection services;

		ZString IHaveServices.TableCode
		{
			get { return TablePrefix; }
		}

		ZString IHaveServices.TransportMode
		{
			get { return ""; }
		}

		bool IHaveServices.NeedsServiceEvents
		{
			get { return false; }
		}

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Factory.Load<GlbBranch>(LTC_GB_Branch);

		void SetDefaultBranch(IControllingBranchDefaultingManager manager)
		{
			var defaultBranch = manager.FetchDefaultValue(this, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
			if (defaultBranch is ZGuid defaultAsGuid)
			{
				LTC_GB_Branch = defaultAsGuid;
			}
		}

		#region ServiceCurrentContextList

		CodeDescriptionPairList IHaveServicesWithContext.ServiceCurrentContextList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(this.PK, ServiceContextTypes.Code.Consignment, ServiceContextTypes.Description.Consignment);
				foreach (var consignmentAddress in this.Addresses)
				{
					if (consignmentAddress.IsPickUp)
					{
						result.AddPair(consignmentAddress.PK, ServiceContextTypes.Code.PickupAction, ServiceContextTypes.Description.PickupAction);
					}
					else if (consignmentAddress.IsDelivery)
					{
						result.AddPair(consignmentAddress.PK, ServiceContextTypes.Code.DeliveryAction, ServiceContextTypes.Description.DeliveryAction);
					}
				}

				return result;
			}
		}

		static class ServiceContextTypes
		{
			public static class Code
			{
				public const string Consignment = "CSN";
				public const string PickupAction = "PIC";
				public const string DeliveryAction = "DLV";
			}

			public static class Description
			{
				public static MultilingualString Consignment { get { return ResString.GetMultilingualString("89c56b97-ccbe-4ff8-8bd4-0570170c3c11", "Consignment"); } }
				public static MultilingualString PickupAction { get { return ResString.GetMultilingualString("670a71bf-4253-4d40-8dbd-72e2c703fb62", "Pick up action"); } }
				public static MultilingualString DeliveryAction { get { return ResString.GetMultilingualString("{d8a8d1a1-052a-49dc-9a4a-a5d0865c4ac7", "Delivery action"); } }
			}
		}

		#endregion

		#region GetContextTableCode

		ZString IHaveServicesWithContext.GetContextTableCode(ZGuid contextID)
		{
			return (contextID.IsEmpty || contextID == this.PK) ? "" : DtbConsignmentAddressSchema.Constants.Prefix;
		}

		#endregion

		#endregion

		#region IDocAddresses Members

		#region DocAddresses

		[ChildEditable]
		[UniversalCopyCollectionEntity(JobDocAddressSchema.Constants.TableName, JobDocAddressSchema.Constants.E2_ParentID)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		#endregion

		#region SupportedAddressTypes

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.BookingPartyDocumentaryAddress,
					DocAddressType.NotifyParty,
					DocAddressType.ClientRequestedBillingParty,
					DocAddressType.FinalConsigneeAddress,
					DocAddressType.OriginatingConsignorAddress
				};
			}
		}

		#endregion

		#region Events

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		#endregion

		#region CanDeleteAddress

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		#endregion

		#region GetCanOverrideCheckpoint

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		#endregion

		#region GetDocAddressRequirement

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result;

			switch (addressType)
			{
				case DocAddressType.ClientRequestedBillingParty:
					result = new JobDocAddressRequirement(addressType, AddressType.ARM, ContactType.LocalTransport);
					result.CanOverride = false;
					break;
				default:
					result = null;
					break;
			}

			return result;
		}

		#endregion

		#region PiggyBackedDocAddressValidation

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		#endregion

		#region GetOrgHeaderList

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return addressType == DocAddressType.ClientRequestedBillingParty
				? new DebtorCollection(Factory)
				: null;
		}

		#endregion

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new DtbConsignmentRatingAdaptersProvider(this); }
		}

		#endregion

		#region Notes

		#region NoteContextsForRelatedNotes

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				var noteContexts = base.NoteContextsForRelatedNotes;
				noteContexts.Module |= StmNoteContextModule.T; // transport
				AddNoteContextsForRelatedNotes_Directions(noteContexts);

				return noteContexts;
			}
		}

		public StmNoteContexts GetNoteContextsForRelatedNotesForTest()
		{
			return NoteContextsForRelatedNotes;
		}

		#region AddNoteContextsForRelatedNotes_Directions

		void AddNoteContextsForRelatedNotes_Directions(StmNoteContexts noteContexts)
		{
			noteContexts.Direction |= StmNoteContextDirection.D; // domestic

			if (IsDeliveryDirection)
			{
				noteContexts.Direction |= StmNoteContextDirection.I; // import
				noteContexts.Direction |= StmNoteContextDirection.B; // destination
			}
			else if (IsPickUpDirection)
			{
				noteContexts.Direction |= StmNoteContextDirection.E; // export
				noteContexts.Direction |= StmNoteContextDirection.B; // origin
			}
		}

		#endregion

		#endregion

		#endregion

		#region IWorkflowProvider Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var orderedClientPKList = GetClientsInTemplateSelectionOrder();
			result.AddEnumerable(ProcessTaskTemplateSchema.P0_OH_Client, orderedClientPKList.Cast<object>());

			return result;
		}

		IEnumerable<ZGuid> GetClientsInTemplateSelectionOrder()
		{
			var relevantOrgPKs = new List<ZGuid>();
			var registryItem = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var criteria = registryItem.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment);

			if (criteria != null)
			{
				AddFetchHintsForTemplateSelectionCriteria();

				foreach (var criterion in criteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>())
				{
					switch (criterion.OrgTypeCode)
					{
						case ClientInTemplateSelectionOrgTypeList.Codes.BookingParty:
							AddIfValid(BookingPartyAddress?.OrganisationPK);
							break;
						case ClientInTemplateSelectionOrgTypeList.Codes.LocalClient:
							AddIfValid(GetBillingOrganization());
							AddIfValid(ClientRequestedBillingPartyAddress?.OrganisationPK);
							break;
						case ClientInTemplateSelectionOrgTypeList.Codes.PickupAddressOrganization:
							AddIfValid(PickupAddress?.Address?.OrganisationPK);
							break;
						case ClientInTemplateSelectionOrgTypeList.Codes.DeliveryAddressOrganization:
							AddIfValid(DeliveryAddress?.Address?.OrganisationPK);
							break;
						default:
							throw new NotImplementedException("Selected client type is not yet implemented: " + criterion.OrgTypeDescription);
					}
				}
			}

			relevantOrgPKs.Add(ZGuid.Empty);
			return relevantOrgPKs;

			void AddIfValid(ZGuid? pk)
			{
				if (pk.HasValue && pk.Value.IsValid)
				{
					relevantOrgPKs.Add(pk.Value);
				}
			}
		}

		void AddFetchHintsForTemplateSelectionCriteria()
		{
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentTableCode, new ZString(TablePrefix));
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentTableCode, new ZString(DtbConsignmentAddressSchema.Constants.Prefix));

			var bookingPartyAddress = BookingPartyAddress;
			var billingPartyAddressPk = GetBillingAddressPK();
			var pickupAddress = PickupAddress;
			var deliveryAddress = DeliveryAddress;

			if (bookingPartyAddress != null)
			{
				Factory.AddFetchHint(OrgAddressSchema.PK, bookingPartyAddress.E2_OA_Address);
			}

			if (billingPartyAddressPk.HasValue && billingPartyAddressPk.Value.IsValid)
			{
				Factory.AddFetchHint(OrgAddressSchema.PK, billingPartyAddressPk);
			}

			if (pickupAddress != null)
			{
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, pickupAddress.PK);
			}

			if (deliveryAddress != null)
			{
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, deliveryAddress.PK);
			}
		}

		ZGuid? GetBillingOrganization()
		{
			var addressPK = GetBillingAddressPK();

			if (addressPK.HasValue && addressPK.Value.IsValid)
			{
				var address = Factory.Load<OrgAddress>(addressPK.Value);
				return address?.OA_OH;
			}

			return null;
		}

		ZGuid? GetBillingAddressPK()
		{
			return Job?.JH_OA_LocalChargesAddr;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new DtbConsignmentProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		DtbConsignmentProcessTaskCollection workflowItems;

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IDtbConsignment Members

		public ZGuid JobHeaderPK
		{
			get { return Job != null ? Job.PK : ZGuid.Empty; }
		}

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber
		{
			get { return LTC_JobID; }
		}

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableName; }
		}

		ZString IRelatedJob.JobStatus
		{
			get
			{
				if (LTC_IsActive)
				{
					return Lookups.BindToLists.ConsignmentStatuses.GetDescriptionFromCode(LTC_Status);
				}
				else
				{
					return ConsignmentStatuses.Descriptions.Deactivated;
				}
			}
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.DtbConsignment; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		#endregion

		#region BusinessObjectsWithRelatedEventsCore

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var relatedObjects = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				var actions = AllActions;
				relatedObjects.AddRange(actions.Select(x => x.RunSheetInstruction).WhereNotNull());
				relatedObjects.AddRange(actions);
				relatedObjects.AddRange(Addresses);
				relatedObjects.AddRange(PackageJob.Packages);
				relatedObjects.AddRange(GetVariationsFromDatabase());

				if (Job != null)
				{
					relatedObjects.Add(Job);
				}

				if (!LTC_KM_Booking.IsEmpty)
				{
					var transportBooking = Factory.Load<DtbBooking>(LTC_KM_Booking);
					relatedObjects.Add(transportBooking);

					var parentJob = transportBooking.ConsolidationSingleJob?.Parent?.ParentWithWorkflow;
					if (parentJob != null)
					{
						relatedObjects.Add(parentJob);
					}
				}
				return relatedObjects.ToArray();
			}
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		[ChildEditable]
		CustomBusinessObject CustomBusinessObject
		{
			get
			{
				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
					customBusinessObject = new CustomBusinessObject(Factory, this, properties);
					customBusinessObject.SetReadOnlyIncludingChildren(ReadOnly);
					RegisterEditableChildObject(customBusinessObject);
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;
				if (customBusinessObject != null)
				{
					customBusinessObject.SetReadOnlyIncludingChildren(ReadOnly);
				}
			}
		}

		#endregion

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var helper = new Testing.TransportConsignmentTestHelper(Factory);
			var address = helper.CreateConsignmentAddress(this, ConsignmentAddressTypes.Codes.PickUp);
			address.LTS_Sequence = 1;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LTC_ConsignmentType = DtbConsignmentTypes.Codes.LandTransportConsignment;
		}

		#endregion

		#region IJobInvoicingAdditionalData Members

		public CustomPropertyContainer<JobCharge> GetAdditionalProperties()
		{
			return new DtbConsignmentJobInvoicingAdditionalDataPropertyProvider().GetAdditionalProperties();
		}

		#endregion
	}
}
