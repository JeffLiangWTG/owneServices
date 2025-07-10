using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Business
{
	[CodeProperty(DtbLinehaulManifestSchema.Constants.LHM_ManifestID)]
	[DescriptionProperty(DtbLinehaulManifestSchema.Constants.LHM_ManifestID)]
	public class DtbLinehaulManifest : AutoDtbLinehaulManifest,
		IDtbLinehaulManifest,
		IJobCostingPlugIn,
		IRatingSupporter,
		IDocumentSupportable,
		IDocManagerSupport
	{
		public DtbLinehaulManifest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(LHM_PackageString), ConcurrencyPolicy.Ignore);
		}

		#region Defaults

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LHM_StartDateTimeUtc = ZDateTime.UtcNow;
		}

		#endregion

		#region Properties

		[ResourceStringData("DtbLinehaulManifest|HasDangerousPackages", Caption = "Dangerous Goods", ShortCaption = "DG")]
		public ZBool HasDangerousPackages
		{
			get { return Packages.Any(x => x.UNDGs.Any()); }
		}

		public ZPropertyInfo HasDangerousPackagesInfo
		{
			get { return GetZPropertyInfo(nameof(HasDangerousPackages)); }
		}

		[ResourceStringData("DtbLinehaulManifest|HasTemperatureControlledPackages", Caption = "Temp. Controlled Packages", ShortCaption = "Temp. Control")]
		public ZBool HasTemperatureControlledPackages
		{
			get { return Packages.Any(x => x.IsTemperatureControlled); }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return LHM_ManifestID.IsEmpty
					? Res.GetString("551e3d88-b815-4219-80b7-6a918e2e1210", "Linehaul Manifest")
					: Res.GetString("c9a5bf4b-17d9-41bd-adf9-d139095b24f6", "Linehaul Manifest {0}", LHM_ManifestID);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsAdHocDriversNameAndLicenseReadOnly")]
		[ReadOnlyMember("IsAdHocDriversNameAndLicenseReadOnly")]
		public override ZString LHM_AdhocDriversLicence
		{
			get { return base.LHM_AdhocDriversLicence; }
			set { base.LHM_AdhocDriversLicence = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsAdHocDriversNameAndLicenseReadOnly")]
		[ReadOnlyMember("IsAdHocDriversNameAndLicenseReadOnly")]
		public override ZString LHM_AdhocDriversName
		{
			get { return base.LHM_AdhocDriversName; }
			set { base.LHM_AdhocDriversName = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsAdHocTransportCoNameReadOnly")]
		[ReadOnlyMember("IsAdHocTransportCoNameReadOnly")]
		public override ZString LHM_AdhocTransportCoName
		{
			get { return base.LHM_AdhocTransportCoName; }
			set { base.LHM_AdhocTransportCoName = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsAdHocTruckRegistrationReadOnly")]
		[ReadOnlyMember("IsAdHocTruckRegistrationReadOnly")]
		public override ZString LHM_AdhocTruckRegistration
		{
			get { return base.LHM_AdhocTruckRegistration; }
			set { base.LHM_AdhocTruckRegistration = value; }
		}

		public override ZString LHM_GS_NKDriver1
		{
			get { return base.LHM_GS_NKDriver1; }
			set
			{
				base.LHM_GS_NKDriver1 = value;
				if (Driver1 != null)
				{
					LHM_AdhocDriversName = ZString.Empty;
					LHM_AdhocDriversLicence = ZString.Empty;
				}
			}
		}

		#region DriversName

		public ZString DriversName
		{
			get { return Driver1 != null ? Driver1.GS_FullName : LHM_AdhocDriversName; }
		}

		#endregion

		#region TruckRegistration

		public ZString TruckRegistration
		{
			get { return LHM_AdhocTruckRegistration; } //(PrimaryEquipment != null) ? PrimaryEquipment.RQ_Registration : LHM_AdhocTruckRegistration; }
		}

		#endregion

		[ResourceStringData("DtbLinehaulManifest|TransportCompanyPK", Caption = "Transport Company", ShortCaption = "Transport Co.")]
		[RelatedBusinessObject("TransportCompany")]
		[List("Lookups.TransportCompanies")]
		public ZGuid TransportCompanyPK
		{
			get
			{
				if (transportCompanyPK.IsEmpty && !transportCompanyLoaded)
				{
					transportCompanyLoaded = true;
					var transportCompany = GetExistingTransportCompanyDocAddress();
					if (transportCompany != null && transportCompany.Address != null)
					{
						transportCompanyPK = transportCompany.Address.Header.PK;
					}
				}

				return transportCompanyPK;
			}
			set
			{
				SetNonPersistentPropertyValue(TransportCompanyPKInfo, ref transportCompanyPK, value);
				TransportCompanyDocAddress.E2_OA_Address = TransportCompany != null ? TransportCompany.MainAddress.PK : ZGuid.Empty;
				if (TransportCompany != null)
				{
					LHM_AdhocTransportCoName = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateTransportCompanyPK();
				}
			}
		}

		bool transportCompanyLoaded;

		ZGuid transportCompanyPK;

		public ZPropertyInfo TransportCompanyPKInfo
		{
			get { return GetZPropertyInfo(nameof(TransportCompanyPK)); }
		}

		[ResourceStringData("DtbLinehaulManifest|Description", Caption = "Description")]
		public ZString Description
		{
			get { return Invariant($"{(OriginDepot != null ? OriginDepot.Header.OH_Code : ZString.Empty)} - {(DestinationDepot != null ? DestinationDepot.Header.OH_Code : ZString.Empty)}"); }
		}

		[List("Lookups.Depots")]
		public override ZGuid LHM_OA_OriginDepot
		{
			get { return base.LHM_OA_OriginDepot; }
			set { base.LHM_OA_OriginDepot = value; }
		}

		[List("Lookups.Depots")]
		public override ZGuid LHM_OA_DestinationDepot
		{
			get { return base.LHM_OA_DestinationDepot; }
			set { base.LHM_OA_DestinationDepot = value; }
		}

		[List("Lookups.StatusList")]
		public override ZString LHM_Status
		{
			get { return base.LHM_Status; }
			set { base.LHM_Status = value; }
		}

		[ResourceStringData("DtbLinehaulManifest|EndDateTimeUTC", Caption = "End Date/Time UTC", ShortCaption = "End (UTC)")]
		public ZDateTime EndDateTimeUTC
		{
			get { return LHM_StartDateTimeUtc; } //.IsValid ? LHM_StartDateTimeUtc.AddMinutes(Leg != null ? Leg.LHL_TripTimeMinutes : ZInt.Zero) : ZDateTime.Empty; }
		}

		[ResourceStringData("DtbLinehaulManifest|StartDateTimeLocal", Caption = "Start Date/Time", ShortCaption = "Start")]
		public ZDateTime StartDateTimeLocal
		{
			get { return LHM_StartDateTimeUtc; } //OriginDepot != null ? LHM_StartDateTimeUtc.ToLocationTime(OriginDepot.EffectiveRelatedPortCode) : LHM_StartDateTimeUtc; }
		}

		[ResourceStringData("DtbLinehaulManifest|EndDateTimeLocal", Caption = "End Date/Time", ShortCaption = "End")]
		public ZDateTime EndDateTimeLocal
		{
			get { return EndDateTimeUTC; } //DestinationDepot != null ? EndDateTimeUTC.ToLocationTime(DestinationDepot.EffectiveRelatedPortCode) : EndDateTimeUTC; }
		}

		#endregion

		#region Adding Package

		[MaxLength(46)]
		[ResourceStringData("DtbLinehaulManifest|PackageToAdd", Caption = "Package to add")]
		public ZString PackageToAdd
		{
			get { return packageToAdd; }
			set { SetNonPersistentPropertyValue(PackageToAddInfo, ref packageToAdd, value); }
		}

		ZString packageToAdd;

		public ZPropertyInfo PackageToAddInfo
		{
			get { return GetZPropertyInfo(nameof(PackageToAdd)); }
		}

		[MaxLength(46)]
		[ResourceStringData("DtbLinehaulManifest|ConsignmentToAdd", Caption = "Consignment to add")]
		[List("Lookups.Consignments")]
		public ZGuid ConsignmentToAddPk
		{
			get { return consignmentToAddPk; }
			set { SetNonPersistentPropertyValue(ConsignmentToAddPkInfo, ref consignmentToAddPk, value); }
		}

		ZGuid consignmentToAddPk;

		public ZPropertyInfo ConsignmentToAddPkInfo
		{
			get { return GetZPropertyInfo(nameof(ConsignmentToAddPk)); }
		}

		internal static PkgPackage LoadPackage(string packageId, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(PkgPackage));

			var packageIdSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageIdSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, packageId);

			var jobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageSchema.KP_KJ_ParentPackageJob);
			jobSubQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, DtbBookingSchema.Constants.Prefix);

			var consignmentSubQuery = new ZDBOnlySubQuery(typeof(DtbTransport), PkgPackageJobSchema.KJ_ParentID);

			var consolSubQuery = new ZDBOnlySubQuery(typeof(DtbTransportConsolidation), DtbBookingSchema.KM_KB_Booking);
			consolSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, TransportConsolidationJobTypes.Codes.Consignment);
			consignmentSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);

			jobSubQuery.AddSubQuery(consignmentSubQuery, JoinCondition.And);

			query.AddSubQuery(packageIdSubQuery, JoinCondition.And);
			query.AddSubQuery(jobSubQuery, JoinCondition.And);

			return factory.LoadTop1<PkgPackage>(query);
		}

		public string AddConsignment()
		{
			var result = string.Empty;

			if (!LHM_ManifestID.IsEmpty)
			{
				if (!ConsignmentToAddPk.IsEmpty)
				{
					var consignment = Factory.Load<DtbBookingConsignment>(ConsignmentToAddPk);
					if (consignment != null)
					{
						foreach (var package in consignment.AssignedPackages)
						{
							AddPackageToManifest(package, false);
						}

						ConsignmentToAddPk = ZGuid.Empty;
					}
					else
					{
						result = Res.GetString("12008fbd-068d-4d85-9bb8-1c96fe08487d", "Please specify a valid consignment ID.");
					}
				}
				else
				{
					result = Res.GetString("c0a65c93-54ee-4a8c-919f-521b8395c1e6", "Please specify a consignment ID.");
				}
			}
			else
			{
				result = Res.GetString("34b63604-8ab0-46d0-8726-43c3a5bc9f7a", "Please save this manifest before adding packages or consignments to it.");
			}

			return result;
		}

		public string AddPackage()
		{
			var result = string.Empty;

			if (!LHM_ManifestID.IsEmpty)
			{
				if (!PackageToAdd.IsEmpty)
				{
					var package = LoadPackage(PackageToAdd, Factory);
					if (package != null)
					{
						if (!AddPackageToManifest(package))
						{
							result = Res.GetString("12008fbd-068d-4d35-9bb8-1c96fe08488c", "Package {0} already exists on this manifest.", PackageToAdd);
						}

						PackageToAdd = ZString.Empty;
					}
					else
					{
						result = Res.GetString("12008fbd-068d-4d85-9bb8-1c96fe08488c", "Package {0} was not found.", PackageToAdd);
					}
				}
				else
				{
					result = Res.GetString("c0a65c93-54ee-4a8c-919f-521b8395c0f7", "Please specify a package ID.");
				}
			}
			else
			{
				result = Res.GetString("34b63604-8ab0-46d0-8726-43c3a5bc9f7a", "Please save this manifest before adding packages or consignments to it.");
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		bool AddPackageToManifest(PkgPackage package, bool individualPackage = true)
		{
			if (!Packages.Contains(package))
			{
				package.Logs.AddNew(
					Events.FreightLoaded,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, individualPackage ? Res.GetString("94542597-252c-4f8e-aa9e-1a776cdccc5f", "Package") : Res.GetString("5432d852-3a0e-47e7-9534-2cf13bb4fac9", "Consignment")),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.VoyageFlightNumber, LHM_ManifestID),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, OriginDepot != null ? OriginDepot.OA_City : ZString.Empty),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, (NoResString)"Depot")); // Event reference parameter value

				Packages.Add(package);
				return true;
			}

			return false;
		}

		public void RemovePackage(PkgPackage package)
		{
			var loadedEvent = package.Logs.GetAllLogs()
				.ToArray<StmALog>()
				.OrderBy(x => x.SL_EventTime)
				.LastOrDefault(x => x.SL_SE_NKEvent == Events.FreightLoadedCode && x.Parameters.Any(p => p.Key == CargoWise.EventReference.Constants.EventReferenceParameters.Codes.VoyageFlightNumber && p.Value == LHM_ManifestID));
			if (loadedEvent != null)
			{
				loadedEvent.Cancel();
			}

			Packages.RemoveFromRelationship(package);
		}

		#endregion

		#region Related Entities

		public OrgHeader TransportCompany
		{
			get { return Factory.Load<OrgHeader>(transportCompanyPK); }
		}

		JobDocAddress GetExistingTransportCompanyDocAddress()
		{
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, PK);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

			return Factory.LoadTop1<JobDocAddress>(query);
		}

		public JobDocAddress TransportCompanyDocAddress
		{
			get
			{
				if (transportCompanyDocAddress == null)
				{
					transportCompanyDocAddress = GetExistingTransportCompanyDocAddress();
					if (transportCompanyDocAddress == null)
					{
						transportCompanyDocAddress = Factory.New<JobDocAddress>();
						transportCompanyDocAddress.E2_AddressType = DocAddressTypes.Codes.TransportCompanyDocumentaryAddress;
						transportCompanyDocAddress.E2_ParentID = PK;
						transportCompanyDocAddress.E2_ParentTableCode = TablePrefix;
					}
				}

				return transportCompanyDocAddress;
			}
		}

		JobDocAddress transportCompanyDocAddress;

		[ChildEditable]
		public DtbLinehaulManifestPackageCollection Packages
		{
			get
			{
				if (packages == null)
				{
					packages = new DtbLinehaulManifestPackageCollection(this);
					packages.CountChanged += (sender, e) => HasDangerousPackagesInfo.RefreshBinding();
					RegisterEditableChildObject(packages);
				}
				return packages;
			}
		}

		DtbLinehaulManifestPackageCollection packages;

		public IEnumerable<DtbBookingConsignment> Consignments
		{
			get
			{
				var list = new List<DtbBookingConsignment>();
				foreach (var package in Packages)
				{
					var query = new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KP_Package, package.PK);
					var divots = Factory.Load<DtbConsignmentInstructionPkgDivot>(query);
					if (divots.Any(divot => divot.Instruction.Booking.ConsolidationSingleJob.KB_JobType == TransportConsolidationJobTypes.Codes.Consignment && !list.Contains(divot.Instruction.Booking)))
					{
						list.Add(divots.First().Instruction.Booking);
					}
				}

				return list;
			}
		}

		#endregion

		#region Saving

		public override void Delete()
		{
			base.Delete();
			TransportCompanyDocAddress.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (LHM_ManifestID.IsEmpty)
			{
				LHM_ManifestID = Env.NumberFountains.LinehaulManifestNumber.GetNextFormatted(Factory);
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IJobCostingPlugIn Members

		void IJobCostingPlugIn.AddNewToLogs(Event @event, ZString reference)
		{
			Logs.AddNew(@event, reference);
		}

		ZString IJobCostingPlugIn.GetPrepaidCollect(IJobInvoicingPlugIn plugin)
		{
			return ZString.Empty;
		}

		RefCurrency IJobCostingPlugIn.ConsolCurrency
		{
			get { return null; }
		}

		decimal IJobCostingPlugIn.ConsolExchangeRate
		{
			get { return 0m; }
		}

		RefUNLOCO IJobCostingPlugIn.DischargePort
		{
			get { return null; }
		}

		decimal IJobCostingPlugIn.ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK)
		{
			return 0m;
		}

		bool IJobCostingPlugIn.IsMasterCollect
		{
			get { return false; }
		}

		ZString IJobCostingPlugIn.JK_UniqueConsignRef
		{
			get { return LHM_ManifestID; }
		}

		RefUNLOCO IJobCostingPlugIn.LoadPort
		{
			get { return null; }
		}

		JobProfitLossCollection IJobCostingPlugIn.ProfitLossContainer
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgent
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgentAPInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgentARInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgent
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgentAPInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgentARInvoicingParty
		{
			get { return null; }
		}

		ZString IJobCostingPlugIn.TransportMode
		{
			get { return ZString.Empty; }
		}

		ZString IJobCostingPlugIn.ContainerMode => ZString.Empty;

		ZString IJobCostingPlugIn.ConsolType => ZString.Empty;

		ZString IJobCostingPlugIn.Module => ApportionmentMethodModules.TransportBooking;

		ZString IJobCostingPlugIn.Direction => ZString.Empty;

		CodeDescriptionPairList IJobCostingPlugIn.PrepaidCollectList
		{
			get { return new CodeDescriptionPairList(); }
		}

		IGenericJobCostSupporter IGenericJobCostPlugIn.CostSupporter
		{
			get { return new DtbLinehaulManifestCostSupporter(this); }
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new DtbLinehaulManifestConsolRatingAdaptersProvider(this); }
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new DtbLinehaulManifestDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DtbLinehaulManifestDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region CarrierServiceLevel

		// Test Failure : DefaultClientServiceLevelFromParent
		// LoadFromNaturalKey was used on a table + field that does not have a unique index
		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return Factory.LoadTop1<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, LHM_PL_NKCarrierServiceLevel)); }
		}

		#endregion

	}
}
