using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	[UniversalCopyWithExtendedEntities]
	public abstract class DtbTransportInstruction : AutoDtbBookingInstruction, IDocAddresses, IConsignmentAddress, ITopLevelBizOProviderForJobDocAddress
	{
		protected DtbTransportInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoDtbBookingInstruction.Schema
		{
			public const string OrganisationType = "OrganisationType";
			public const int OrganisationTypeMaxLength = 3;
			public const string ReqFrom = "ReqFrom";
			public const string ReqTo = "ReqTo";
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			KN_Status = TransportStatuses.Codes.Available;
		}

		#endregion

		#region Related Entities

		#region Relevant Depot

		public GlbBranch DepotForAddress
		{
			get
			{
				GlbBranch result = null;

				if (Zone != null)
				{
					var portHub = PortHubSelection.FindPortHubForZone(Zone, KN_InstructionType);
					if (portHub != null)
					{
						var depot = portHub.Depot;

						if (depot != null)
						{
							var branchOrgProxyQuery = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, depot.PK);
							branchOrgProxyQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
							branchOrgProxyQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
							result = Factory.LoadTop1<GlbBranch>(branchOrgProxyQuery);

							if (result == null && depot.CompanyData != null)
							{
								result = depot.CompanyData.ControllingBranch;
							}
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region IsContainerised

		public bool IsContainerised
		{
			get { return DivotsWithPackages.Packages.Any() && DivotsWithPackages.Packages.All(p => p.IsContainer); }
		}

		#endregion

		#region IsEmptyYard

		public bool IsEmptyYard
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CYD; }
		}

		#endregion

		#region FindDepotAddress

		public OrgAddress FindDepotAddress()
		{
			OrgAddress result = null;

			if (Address.Address != null)
			{
				ZString rawQuery = OrgAddressSchema.Constants.PK + (NoResString)" in (SELECT DepotAddressPk FROM dbo.GetDepotAddress(@ServiceLevelList, @Direction, @CountryCode, @PostCode, @CityName, @StateCode))";

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@ServiceLevelList", Booking.KM_RS_NKServiceLevel, PortHubSelectionSchema.TY_RS_NKServiceLevel);
				sqlParams.Add("@Direction", IsPickUp ? InstructionTypes.Codes.PickUp : InstructionTypes.Codes.Delivery, PortHubSelectionSchema.TY_Direction);
				sqlParams.Add("@CountryCode", Address.E2_RN_NKCountryCode, OrgAddressSchema.OA_RN_NKCountryCode);
				sqlParams.Add("@PostCode", Address.E2_Postcode, OrgAddressSchema.OA_PostCode);
				sqlParams.Add("@CityName", Address.E2_City, OrgAddressSchema.OA_City);
				sqlParams.Add("@StateCode", Address.E2_State, OrgAddressSchema.OA_State);

				var query = new ZDBOnlyQuery(typeof(OrgAddress));
				query.AddFilterAndZSQLParameterCollection(rawQuery, sqlParams);

				result = Factory.LoadTop1<OrgAddress>(query);
			}

			return result;
		}

		#endregion

		#region PortHubZonePivotPickupCutOffTime

		public ZDateTime PortHubZonePivotPickupCutOffTime
		{
			get
			{
				var result = ZDateTime.Empty;
				var depotAddress = FindDepotAddress();
				if (depotAddress != null)
				{
					var today = ZDateTime.Today;
					result = new ZDateTime(today.Year, today.Month, today.Day, 13, 0, 0);

					var pivot = PortHubZonePivot.GetPortHubZonePivot(Factory, IsPickUp ? InstructionTypes.Codes.PickUp : InstructionTypes.Codes.Delivery, Booking.KM_RS_NKServiceLevel, depotAddress.PK, KN_TZ_DomesticZone);

					if (pivot != null)
					{
						result = result.AddMinutes(-pivot.TX_PickupCutOffTimeVariance);
					}
				}

				return result;
			}
		}

		#endregion

		#region Address

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "E2_ParentID", DisableCopyMethodLink = true)]
		public JobDocAddress Address
		{
			get
			{
				if (address == null || address.IsDeleted)
				{
					var docAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationType);
					var requirement = (((IDocAddresses)this).GetDocAddressRequirement(docAddressType));
					address = DocAddresses.FindOrCreateWithRequirement(requirement);
					address.MakePersistentEvenIfEmpty();
				}

				return address;
			}
		}

		JobDocAddress address;

		#endregion

		#region Booking

		AutoDtbBooking IConsignmentAddress.Booking => Booking;

		public DtbTransport Booking
		{
			get { return (DtbTransport)Factory.Load(TransportType, KN_KM_BookingMovement); }
		}

		protected abstract Type TransportType { get; }

		#endregion

		#region Confirmations

		[ChildEditable]
		public IDtbTransportConfirmationCollection Confirmations
		{
			get
			{
				if (confirmations == null)
				{
					confirmations = GetNewConfirmationsCollection();
					RegisterEditableChildObject(confirmations);
				}

				return confirmations;
			}
		}

		protected IEnumerable<DtbTransportConfirmation> ConfirmationsTyped
		{
			get { return Confirmations.Cast<DtbTransportConfirmation>(); }
		}

		protected abstract IDtbTransportConfirmationCollection GetNewConfirmationsCollection();

		IDtbTransportConfirmationCollection confirmations;

		#endregion

		#region DivotsWithPackages

		public IDivotsWithPackagesCollection DivotsWithPackages
		{
			get { return divotsWithPackages ?? (divotsWithPackages = GetNewDivotsWithPackagesCollection()); }
		}

		protected abstract IDivotsWithPackagesCollection GetNewDivotsWithPackagesCollection();

		IDivotsWithPackagesCollection divotsWithPackages;

		#endregion

		#region PackageDivots

		[ChildEditable]
		public IDtbTransportInstructionPkgDivotCollection PackageDivots
		{
			get
			{
				if (packageDivots == null)
				{
					packageDivots = GetNewInstructionPkgDivotsCollection();
					RegisterEditableChildObject(packageDivots);
				}

				return packageDivots;
			}
		}

		protected abstract IDtbTransportInstructionPkgDivotCollection GetNewInstructionPkgDivotsCollection();

		IDtbTransportInstructionPkgDivotCollection packageDivots;

		#endregion

		#region Zone

		public RateTransportZone Zone
		{
			get { return Factory.Load<RateTransportZone>(KN_TZ_DomesticZone); }
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region KN_KM_BookingMovement

		[RelatedBusinessObject("Booking")]
		public override ZGuid KN_KM_BookingMovement
		{
			get { return base.KN_KM_BookingMovement; }
			set { base.KN_KM_BookingMovement = value; }
		}

		#endregion

		#region KN_DropMode

		[ResourceStringData("DtbTransportInstruction|DropMode", Caption = "Drop Mode")]
		[List("Lookups.DropModes")]
		public override ZString KN_DropMode
		{
			get { return base.KN_DropMode; }
			set
			{
				if (KN_DropMode != value)
				{
					BeforeKN_DropModeSet();
					base.KN_DropMode = value;
				}
			}
		}

		protected virtual void BeforeKN_DropModeSet()
		{
		}

		#endregion

		#region KN_InstructionType

		[List("Lookups.InstructionTypes")]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString KN_InstructionType
		{
			get { return base.KN_InstructionType; }
			set
			{
				bool hasValueChanged = KN_InstructionType != value;
				if (hasValueChanged)
				{
					BeforeKN_InstructionTypeSet();
				}

				base.KN_InstructionType = value;

				if (hasValueChanged)
				{
					AfterKN_InstructionTypeSet();
					Address.DefaultAddressType = OrganisationAddressType;
					UpdateAuthorisedToLeave();
				}
			}
		}

		protected virtual void BeforeKN_InstructionTypeSet()
		{
		}

		protected virtual void AfterKN_InstructionTypeSet()
		{
		}

		#endregion

		#region KN_Sequence

		[ReadOnly(true)]
		public override ZInt KN_Sequence
		{
			get { return base.KN_Sequence; }
			set { base.KN_Sequence = value; }
		}

		#endregion

		#region KN_Status

		[ReadOnly(true)]
		public override ZString KN_Status
		{
			get { return base.KN_Status; }
			set
			{
				var previousValue = KN_Status;
				base.KN_Status = value;

				if (previousValue != KN_Status)
				{
					UpdateBookingStatus();
				}
			}
		}

		void UpdateBookingStatus()
		{
			var booking = Booking;
			if (booking != null)
			{
				booking.UpdateStatus();
			}
		}

		#endregion

		// calculated

		#region KN_IsAuthorisedToleave

		public override ZBool KN_IsAuthorisedToLeave
		{
			get { return base.KN_IsAuthorisedToLeave; }
			set
			{
				base.KN_IsAuthorisedToLeave = value;
				markedAsNeedingAtlLog = !IsMarkingAsNeedingAtlLogSuspended; // only log if the User was the last to set ATL. If the system calculated it last, then don't log
			}
		}

		public ZBool DefaultAuthorisedToLeave
		{
			get
			{
				var result = false;

				if (IsAuthorisedToLeaveAvailable && !HasAContainer) // should not default if ATL is unavailable OR booking includes containers
				{
					var localClientOrgAddress = Booking.BillingPartyOrLocalClientAddress;
					var localClientATL = AuthorityToLeaveHelper.GetClientAuthorityToLeave(localClientOrgAddress);

					if (!localClientATL)
					{
						result = false; // should not default if client ATL is false
					}
					else
					{
						var consigneeDocAddress = Address;
						var consignorDocAddress = FindRelatedConsignorInstructionDocAddress();

						if (consigneeDocAddress != null && consignorDocAddress != null)
						{
							var consigneeATL = AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeDocAddress.Address, consignorDocAddress.Address);
							if (!consigneeATL)
							{
								result = false; // should not default if consignee ATL is false
							}
							else
							{
								result = AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeDocAddress.Address); // should not default if consignor ATL is false
							}
						}
					}
				}

				return result;
			}
		}

		bool markedAsNeedingAtlLog;

		JobDocAddress FindRelatedConsignorInstructionDocAddress()
		{
			JobDocAddress consignorDocAddress = null;
			var consignorInstruction = FindRelatedConsignorInstruction();
			if (consignorInstruction != null)
			{
				consignorDocAddress = consignorInstruction.Address;
			}

			return consignorDocAddress;
		}

		internal void UpdateAuthorisedToLeave()
		{
			if (Booking != null)
			{
				if (OrganisationType == OrganisationTypesList.Codes.CNR)
				{
					UpdateRelatedConsigneeATL();
				}

				using (SuspendMarkingAsNeedingAtlLog())
				{
					KN_IsAuthorisedToLeave = DefaultAuthorisedToLeave;
				}
			}
		}

		void UpdateRelatedConsigneeATL()
		{
			foreach (var instruction in FindRelatedConsigneeInstructions())
			{
				instruction.UpdateAuthorisedToLeave();
			}
		}

		internal DtbTransportInstruction FindRelatedConsignorInstruction()
		{
			var booking = Booking;
			return booking != null
				? booking.Instructions.Cast<DtbTransportInstruction>().FirstOrDefault(i => i.OrganisationType == OrganisationTypesList.Codes.CNR && (i.IsPickUp || i.IsMulti))
				: null;
		}

		IEnumerable<DtbTransportInstruction> FindRelatedConsigneeInstructions()
		{
			var booking = Booking;
			return booking != null
				? booking.Instructions.Cast<DtbTransportInstruction>().Where(i => i.IsAuthorisedToLeaveAvailable)
				: Enumerable.Empty<DtbTransportInstruction>();
		}

		public bool IsAuthorisedToLeaveAvailable
		{
			get { return (IsDelivery || IsMulti) && OrganisationType == OrganisationTypesList.Codes.CNE; }
		}

		#region SuspendMarkingAsNeedingAtlLog

		public IDisposable SuspendMarkingAsNeedingAtlLog()
		{
			return new DisposableAction(() => markingAsNeedingAtlLog++, () => markingAsNeedingAtlLog--);
		}

		public ZBool IsMarkingAsNeedingAtlLogSuspended
		{
			get { return markingAsNeedingAtlLog > 0; }
		}

		int markingAsNeedingAtlLog;

		#endregion

		#endregion

		#region Description

		[ResourceStringData("DtbTransportInstruction|Description", Caption = "Instruction")]
		public ZString Description
		{
			get
			{
				var noValue = (ZString)"???";
				var instructionType = !KN_InstructionType.IsEmpty ? KN_InstructionType : noValue;
				var orgType = !OrganisationType.IsEmpty ? OrganisationType : noValue;
				var orgCode = noValue;

				if (Address.E2_AddressOverride)
				{
					orgCode = !Address.E2_CompanyName.IsEmpty ? Address.E2_CompanyNameTruncated : noValue;
				}
				else if (Address.Organisation != null)
				{
					orgCode = Address.Organisation.OH_Code;
				}

				return ZString.Format("{0} @ {1} - {2}", instructionType, orgType, orgCode);
			}
		}

		#endregion

		#region OrganisationType

		[ResourceStringData("DtbTransportInstruction|OrganisationType", ShortCaption = "Org. Type", Caption = "Organization Type")]
		[List("Lookups.OrganisationTypes")]
		[MaxLength(Schema.OrganisationTypeMaxLength)]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public ZString OrganisationType
		{
			get { return organisationType.IsEmpty ? (organisationType = ExistingDocAddressOrganisationType) : organisationType; }
			set
			{
				if (OrganisationType != value)
				{
					BeforeOrganisationTypeSet();

					CheckMaximumLength(OrganisationTypeInfo, value);
					SetNonPersistentPropertyValue(OrganisationTypeInfo, ref organisationType, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrganisationType();
					}
					Address.DocAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(organisationType);

					AfterOrganisationTypeSet();

					UpdateAuthorisedToLeave();
				}
			}
		}

		protected virtual void BeforeOrganisationTypeSet()
		{
		}

		protected virtual void AfterOrganisationTypeSet()
		{
		}

		public ZPropertyInfo OrganisationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationType); }
		}

		ZString ExistingDocAddressOrganisationType
		{
			get { return DocAddresses.Count > 0 ? CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddresses[0].DocAddressType) : ZString.Empty; }
		}

		ZString organisationType;

		#endregion

		#region PackageCategory

		[List("Lookups.PackageCategories")]
		public ZString PackageCategory
		{
			get { return packageCategory ?? (packageCategory = GetPackageCategory()); }
			set
			{
				packageCategory = value;

				DefaultPackages(Array.Empty<PkgPackage>());
			}
		}

		protected ZString GetPackageCategory()
		{
			var result = "";
			var packages = DivotsWithPackages.Packages.ToArray();
			var isCNT = packages.Any(p => p.IsContainer);
			var isLoose = packages.Any(p => !p.IsContainer);

			if (isCNT && isLoose)
			{
				result = PackageCategories.Codes.Both;
			}
			else if (isCNT)
			{
				result = PackageCategories.Codes.Containers;
			}
			else if (isLoose)
			{
				result = PackageCategories.Codes.Loose;
			}

			return result;
		}

		string packageCategory;

		#region DefaultPackages

		/// <summary>
		/// Attempt to assign packages from those passed in
		/// Otherwise fall back to package job packages
		/// </summary>
		/// <param name="possiblePackages"></param>
		public void DefaultPackages(IEnumerable<PkgPackage> possiblePackages)
		{
			DefaultPackagesCore(possiblePackages); // will remove all packages if !possiblePackages.Any()

			if (!DivotsWithPackages.Typed.Any())
			{
				DefaultPackagesCore(PackageJob.Packages);
			}
		}

		/// <summary>
		/// [Category]		[Add]
		/// Containers		Containers
		/// Both			Containers and their child Loose + Top Level Loose
		/// Outer			Containers and Top Level Loose			
		/// Loose			Loose inside containers and Top Level Loose
		/// </summary>
		void DefaultPackagesCore(IEnumerable<PkgPackage> possiblePackages)
		{
			if (!Booking.IsSettingPackagesSuspended) // universal is populating template code, universal packages for just this booking will be added later
			{
				var packagesToProcess = new List<PkgPackage>();

				var addContainers = PackageCategory == PackageCategories.Codes.Containers || PackageCategory == PackageCategories.Codes.Both || PackageCategory == PackageCategories.Codes.Outers;
				if (addContainers)
				{
					var matchingPackageLinks = new List<ZInt?>();

					if (Booking != null && Booking.ParentContainerLinksAndAddresses != null && OrganisationType == OrganisationTypesList.Codes.CYD)
					{
						ZString? addressTypeToMatchOn = null;
						if (KN_InstructionType == InstructionTypes.Codes.PickUp)
						{
							addressTypeToMatchOn = AddressTypes.ContainerYardEmptyPickupAddress;
						}
						else if (KN_InstructionType == InstructionTypes.Codes.Delivery)
						{
							addressTypeToMatchOn = AddressTypes.ContainerYardEmptyReturnAddress;
						}

						foreach (var container in Booking.ParentContainerLinksAndAddresses)
						{
							var shouldMatchOnOnContainerAddress = !container.shouldFallbackToShipmentContainerYard && Address.RealAddress != null && container.addressDetails.Any(a => a.addressCode == Address?.Address?.AddressCode && a.addressType == addressTypeToMatchOn);
							var shouldMatchOnShipmentAddress = container.shouldFallbackToShipmentContainerYard && Address.RealAddress != null && container.addressDetails.Any(a => a.addressCode == Address?.Address?.AddressCode && a.addressType == (ZString?)nameof(DocAddressType.CustomsContainerYardAddress));
							var shouldMatchOnBlankAddress = Address.RealAddress == null && container.addressDetails.Any(a => a.addressCode == (ZString?)null && a.addressType == addressTypeToMatchOn);

							if (shouldMatchOnOnContainerAddress || shouldMatchOnShipmentAddress || shouldMatchOnBlankAddress)
							{
								if (Booking.PackageContainerLinks != null && Booking.PackageContainerLinks.Count > 0)
								{
									matchingPackageLinks.Add(container.link);
								}
							}
						}
					}

					var shouldMatchOnAddress = OrganisationType == OrganisationTypesList.Codes.CYD && matchingPackageLinks != null && matchingPackageLinks.Any(l => l != null) && Booking.PackageContainerLinks != null && Booking.PackageContainerLinks.Count > 0;

					packagesToProcess.AddRange(possiblePackages.Where(p => p.IsContainer && (!shouldMatchOnAddress || matchingPackageLinks.Contains(Booking.PackageContainerLinks.Where(l => l.Value != null && l.Value.PK == p.PK).Select(l => l.Key).FirstOrDefault()))));
				}

				var addTopLevelLoose = PackageCategory == PackageCategories.Codes.Outers || PackageCategory == PackageCategories.Codes.Loose || PackageCategory == PackageCategories.Codes.Both;
				if (addTopLevelLoose)
				{
					packagesToProcess.AddRange(possiblePackages.Where(p => !p.IsContainer));
				}

				var addOuterLooseInsideContainers = PackageCategory == PackageCategories.Codes.Both || PackageCategory == PackageCategories.Codes.Loose;
				if (addOuterLooseInsideContainers)
				{
					packagesToProcess.AddRange(possiblePackages.Where(p => p.IsContainer).SelectMany(p => p.Packages));
				}

				var packagesToRemove = DivotsWithPackages.Packages.Except(packagesToProcess).ToArray();
				Array.ForEach(packagesToRemove, p => DivotsWithPackages.RemovePackage(p));
				Array.ForEach(packagesToProcess.ToArray(), p => DivotsWithPackages.AddPackage(p));
			}
		}

		protected abstract PkgPackageJob PackageJob { get; }

		#endregion

		#endregion

		#region ReqFrom

		protected DtbTransportConfirmation CreateTransportConfirmation()
		{
			var confirmation = (DtbTransportConfirmation)Confirmations.AddNew();

			confirmation.KK_ConfirmationType = IsPickUp ? ConfirmationTypes.Codes.PickUp : ConfirmationTypes.Codes.Delivery;

			return confirmation;
		}

		public IEnumerable<DtbTransportConfirmation> DefaultConfirmations
		{
			get
			{
				var defaultConfirmations = Confirmations.Cast<DtbTransportConfirmation>();
				return IsPickUp ? defaultConfirmations.Where(c => c.IsPickUp) : defaultConfirmations.Where(c => c.IsDelivery);
			}
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbTransportInstruction|ReqFrom", Caption = "Required From", ShortCaption = "Req. From")]
		public virtual ZDateTime ReqFrom
		{
			get
			{
				var lastConfirm = DefaultConfirmations.OrderBy(c => c.KK_RequiredFrom).LastOrDefault();
				return lastConfirm != null ? lastConfirm.KK_RequiredFrom : ZDateTime.Empty;
			}
			set
			{
				DefaultConfirmations.ForEach(c => c.KK_RequiredFrom = value);
				ReqFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo<ZDateTime> ReqFromInfo
		{
			get { return (ZPropertyInfo<ZDateTime>)GetZPropertyInfo(Schema.ReqFrom); }
		}

		#endregion

		#region ReqTo

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbTransportInstruction|ReqTo", Caption = "Required To", ShortCaption = "Req. To")]
		public virtual ZDateTime ReqTo
		{
			get
			{
				var lastConfirm = DefaultConfirmations.OrderBy(c => c.KK_RequiredTo).LastOrDefault();
				return lastConfirm != null ? lastConfirm.KK_RequiredTo : ZDateTime.Empty;
			}
			set
			{
				DefaultConfirmations.ForEach(c => c.KK_RequiredTo = value);
				ReqToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo<ZDateTime> ReqToInfo
		{
			get { return (ZPropertyInfo<ZDateTime>)GetZPropertyInfo(Schema.ReqTo); }
		}

		#endregion

		#endregion

		#region Flags

		#region IsAir

		//DJB check packages perhaps
		bool IsAir
		{
			get { return false; }
		}

		#endregion

		#region HasAContainer

		public bool HasAContainer
		{
			get { return DivotsWithPackages.Packages.Any(p => p.IsContainer); }
		}

		#endregion

		#region IsDelivery

		public bool IsDelivery
		{
			get { return KN_InstructionType.EqualsIgnoringCase(InstructionTypes.Codes.Delivery); }
		}

		#endregion

		#region IsLoose

		public bool IsLoose
		{
			get { return DivotsWithPackages.Count > 0 && !HasAContainer; }
		}

		#endregion

		#region IsMulti

		public bool IsMulti
		{
			get { return KN_InstructionType.EqualsIgnoringCase(InstructionTypes.Codes.Multi); }
		}

		#endregion

		#region IsPickUp

		public bool IsPickUp
		{
			get { return KN_InstructionType.EqualsIgnoringCase(InstructionTypes.Codes.PickUp); }
		}

		#endregion

		#region IsDepot

		public ZBool IsDepot
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CFS; }
		}

		#endregion

		#region IsOwnDepot

		// When support is added for containers and Local Transport is migrated,
		// this will have to be changed to check if the organisation is the
		// OrgProxy as Depot multis that are not auto added will be valid.
		public ZBool IsOwnDepot
		{
			get { return IsMulti && IsDepot; }
		}

		#endregion

		#region IsCTO

		public ZBool IsCTO
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CTO; }
		}

		#endregion

		#region IsCYD

		public ZBool IsCYD
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CYD; }
		}

		#endregion

		#endregion

		#region DefaultDropMode

		public void DefaultDropMode(DtbTransportInstructionTmpl instructionTemplate = null)
		{
			if (!SuspendDefaultingDropModeSemaphore.IsSuspended)
			{
				var parentDropMode = GetParentDropMode();
				if (!parentDropMode.IsEmpty)
				{
					KN_DropMode = parentDropMode;
				}

				if (KN_DropMode.IsEmpty && instructionTemplate != null)
				{
					var instructionDropMode = instructionTemplate.K2_DropMode;
					if (!instructionDropMode.IsEmpty)
					{
						KN_DropMode = instructionDropMode;
					}
				}

				if (KN_DropMode.IsEmpty)
				{
					var addressDropMode = AddressDropMode;
					if (!addressDropMode.IsEmpty)
					{
						KN_DropMode = addressDropMode;
					}
				}
			}
		}

		protected virtual ZString GetParentDropMode()
		{
			return "";
		}

		ZString AddressDropMode
		{
			get
			{
				ZString result = "";

				var orgAddress = Address?.Address;
				if (orgAddress != null)
				{
					if (IsAir)
					{
						result = orgAddress.OA_AIREquipmentNeeded;
					}
					else if (IsLoose)
					{
						result = orgAddress.OA_LCLEquipmentNeeded;
					}
					else if (DivotsWithPackages.Packages.Any(p => p.IsContainer))
					{
						result = orgAddress.OA_FCLEquipmentNeeded;
					}
				}

				return result;
			}
		}

		#endregion

		#region SuspendDefaultingDropMode

		public IDisposable SuspendDefaultingDropMode()
		{
			return new SemaphoreManager(SuspendDefaultingDropModeSemaphore);
		}

		Semaphore SuspendDefaultingDropModeSemaphore => suspendDefaultingDropModeSemaphore ?? (suspendDefaultingDropModeSemaphore = new Semaphore());
		Semaphore suspendDefaultingDropModeSemaphore;

		#endregion

		#region Lookups

		public new DtbTransportInstructionLookups Lookups
		{
			get { return (DtbTransportInstructionLookups)base.Lookups; }
		}

		protected sealed override DtbBookingInstructionLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		protected abstract DtbTransportInstructionLookups GetNewLookupsCore();

		#endregion

		#region Validation

		public new DtbTransportInstructionValidation Validation
		{
			get { return (DtbTransportInstructionValidation)base.Validation; }
		}

		protected sealed override DtbBookingInstructionValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		protected abstract DtbTransportInstructionValidation GetNewValidationCore();

		#endregion

		#region Delete

		public sealed override void Delete()
		{
			Confirmations.DeleteAll();
			PackageDivots.DeleteAll(); // tested by SaveAndDeleteBusinessObject()
			DocAddresses.RemoveAndDeleteAll();

			var transportPriorToDelete = Booking;

			UpdateRelatedConsigneeATL();

			DeleteCore();
			base.Delete();

			OnAfterDelete(transportPriorToDelete);
		}

		protected virtual void DeleteCore()
		{
		}

		void OnAfterDelete(DtbTransport transport)
		{
			if (transport != null)
			{
				transport.Instructions.Sequence();
			}
		}

		/// <summary>
		/// Occurs during Concurrency Merging - Package View wraps Package Divots and needs to be handled
		/// </summary>
		protected override void DeleteForDataRefresh()
		{
			if (!IsDeleted)
			{
				Array.ForEach(Confirmations.ToArray(), c => ((IBusiness)c).DeleteForDataRefresh());
				Array.ForEach(PackageDivots.ToArray(), d => ((IBusiness)d).DeleteForDataRefresh());
				Array.ForEach(DocAddresses.ToArray(), a => ((IBusiness)a).DeleteForDataRefresh());

				var transportPriorToDelete = Booking;
				base.DeleteForDataRefresh();
				OnAfterDelete(transportPriorToDelete);
			}
		}

		#endregion

		#region SuspendOnDocAddressChanged

		public IDisposable SuspendOnDocAddressChanged()
		{
			return new SemaphoreManager(OnDocAddressChangedSemaphore);
		}

		Semaphore OnDocAddressChangedSemaphore
		{
			get { return onDocAddressChangedSemaphore ?? (onDocAddressChangedSemaphore = new Semaphore()); }
		}

		Semaphore onDocAddressChangedSemaphore;

		#endregion

		#region UpdateStatus

		public void UpdateStatus()
		{
			if (!IsDeleted)
			{
				var result = TransportStatuses.Codes.Available;

				var expectedStatus = GetExpectedStatus();
				if (!expectedStatus.IsEmpty)
				{
					result = expectedStatus;
				}

				KN_Status = result;
			}
		}

		protected abstract ZString GetExpectedStatus();

		#endregion

		#region IDocAddresses Members

		#region DocAddresses

		[ChildEditable()]
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
					DocAddressType.LocalCartageCFS,
					DocAddressType.LocalCartageCTO,
					DocAddressType.LocalCartageExporter,
					DocAddressType.LocalCartageImporter,
					DocAddressType.LocalCartageYard,
					DocAddressType.LocalCartageService,
					DocAddressType.LocalCartageMSC,
					DocAddressType.LocalCartageWarehouse,
					DocAddressType.None,
				};
			}
		}

		#endregion

		#region Events

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
			OnAnyAddressFieldBeforeChange(docAddress);
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			OnDocAddressChanged(docAddress);
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
			OnOrgAddressBeforeChange(docAddress);
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		protected virtual void OnAnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void OnDocAddressChanged(JobDocAddress docAddress)
		{
			if (!OnDocAddressChangedSemaphore.IsSuspended)
			{
				SetRelatedOrganisationFromBuyerSupplierLink();
				SetDomesticZone(docAddress);
				UpdateAuthorisedToLeave();
				OnDocAddressChangedCore(docAddress);
			}
		}

		void SetRelatedOrganisationFromBuyerSupplierLink()
		{
			var org = Address.Organisation;
			if (org != null)
			{
				var orgType = OrganisationType;
				var isConsignor = orgType == OrganisationTypesList.Codes.CNR;
				var isConsignee = orgType == OrganisationTypesList.Codes.CNE;
				var relatedSupplier = org.SupplierLinks.Count == 1 ? org.SupplierLinks.Cast<OrgSupplierBuyerLink>().First() : null;
				var relatedBuyer = org.BuyerLinks.Count == 1 ? org.BuyerLinks.Cast<OrgSupplierBuyerLink>().First() : null;
				if ((isConsignor && relatedBuyer != null) || (isConsignee && relatedSupplier != null))
				{
					var relatedOrgType = isConsignor ? OrganisationTypesList.Codes.CNE : OrganisationTypesList.Codes.CNR;
					var relatedInstruction = Booking.Instructions.Cast<DtbTransportInstruction>().FirstOrDefault(i => i.OrganisationType == relatedOrgType);
					var relatedAddress = relatedInstruction != null ? relatedInstruction.Address : null;
					if (relatedAddress != null && relatedAddress.IsEmpty)
					{
						relatedAddress.OrganisationPK = isConsignor ? relatedBuyer.OL_OH_Buyer : relatedSupplier.OL_OH_Supplier;
					}
				}

				foreach (var confirmation in ConfirmationsTyped)
				{
					confirmation.SetKK_RequiredFromUtc();
					confirmation.SetKK_RequiredToUtc();
					confirmation.SetKK_EstimatedUtc();
				}
			}
		}

		void SetDomesticZone(JobDocAddress docAddress)
		{
			var location = LocationHelper.GetLocationFromIDocAddress(docAddress, Factory);
			var zone = RateTransportZone.GetOperationZone(Factory, null, location, docAddress.E2_RN_NKCountryCode, null, docAddress.E2_Postcode, docAddress.E2_City);
			KN_TZ_DomesticZone = (zone != null) ? zone.PK : ZGuid.Empty;
		}

		protected virtual void OnDocAddressChangedCore(JobDocAddress docAddress)
		{
		}

		protected virtual void OnOrgAddressBeforeChange(JobDocAddress docAddress)
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
			return GetCanOverrideCheckpointCore(docAddress);
		}
		protected abstract SecurityCheckpoint GetCanOverrideCheckpointCore(JobDocAddress docAddress);

		#endregion

		#region GetDocAddressRequirement

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return new JobDocAddressRequirement(addressType, OrganisationAddressType, ContactType.LocalTransport, true);
		}

		protected AddressType OrganisationAddressType
		{
			get
			{
				var result = AddressType.NoDefault;

				switch (KN_InstructionType)
				{
					case InstructionTypes.Codes.PickUp:
						result = AddressType.PIC;
						break;

					case InstructionTypes.Codes.Delivery:
						result = AddressType.DLV;
						break;

					case InstructionTypes.Codes.Multi:
						if (Booking != null)
						{
							if (Booking.IsPickupDirection)
							{
								result = AddressType.PIC;
							}
							else if (Booking.IsDeliveryDirection)
							{
								result = AddressType.DLV;
							}
						}
						break;
				}

				return result;
			}
		}

		#endregion

		#region PiggyBackedDocAddressValidation

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return PiggyBackedDocAddressValidationCore(addressToValidate);
		}

		protected virtual ZValidation PiggyBackedDocAddressValidationCore(JobDocAddress addressToValidate)
		{
			return null;
		}

		#endregion

		#region GetOrgHeaderList

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.LocalCartageCFS:
					return Lookups.CFSOrganisations;
				case DocAddressType.LocalCartageCTO:
					return Lookups.CTOOrganisations;
				case DocAddressType.LocalCartageYard:
					return Lookups.ContainerYardOrganisations;
				case DocAddressType.LocalCartageImporter:
					return Lookups.ConsigneeOrganisations;
				case DocAddressType.LocalCartageExporter:
					return Lookups.ConsignorOrganisations;

				default:
					return Lookups.AllOrganisations;
			}
		}

		#endregion

		#endregion

		#region IConsignmentAddress

		public ZString DropMode
		{
			get { return KN_DropMode; }
		}

		public ZString Status
		{
			get { return KN_Status; }
		}

		public ZString ServiceInstruction
		{
			get { return KN_ServiceInstruction; }
		}

		public ZString ConsignmentAddressType
		{
			get { return KN_InstructionType; }
		}

		public IEnumerable<PkgPackage> GetPackages
		{
			get { return DivotsWithPackages.Packages; }
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			if (markedAsNeedingAtlLog && ((!IsInDatabase && KN_IsAuthorisedToLeave) || KN_IsAuthorisedToLeaveInfo.HasChanges))
			{
				Logs.CreateOrRecreateEventLog(KN_IsAuthorisedToLeave ? Events.Authorised : Events.AuthorisationWithdrawn, EstimateActual.Actual, ZDateTimeOffset.Now, "",
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceActionAuthorisedTypes.AuthorisedToLeave));
			}

			base.OnSaving();
		}

		public BusinessObject GetTopBusinessObject()
		{
			return Factory.Load(DtbBookingSchema.Constants.Prefix, KN_KM_BookingMovement);
		}

		#endregion
	}
}
