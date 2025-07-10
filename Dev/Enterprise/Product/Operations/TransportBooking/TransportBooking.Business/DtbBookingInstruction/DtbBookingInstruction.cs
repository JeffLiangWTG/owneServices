using System;
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
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	[UserDefinedValues]
	[UniversalCopyWithExtendedEntities]
	public sealed class DtbBookingInstruction : Common.AutoDtbBookingInstruction,
		IDtbBookingInstruction,
		IAddress,
		IWorkflowProvider,
		ICustomFieldProvider,
		IDocAddresses,
		IConsignmentAddress,
		ITopLevelBizOProviderForJobDocAddress,
		IDtbMasterBookingEntity
	{
		public DtbBookingInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			MasterBookingHelper = new DtbMasterBookingHelper(this);
		}

		public new class Schema : Common.AutoDtbBookingInstruction.Schema
		{
			public const string Note = "Note";
			public const string Estimated = "Estimated";
			public const string Actual = "Actual";
			public const string SignedBy = "SignedBy";
			public const string PackageCategoryDescription = "PackageCategoryDescription";
			public const string OrganisationType = "OrganisationType";
			public const int OrganisationTypeMaxLength = 3;
			public const string ReqFrom = "ReqFrom";
			public const string ReqTo = "ReqTo";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			KN_Status = TransportStatuses.Codes.Available;
		}

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

		public bool IsContainerised
		{
			get { return DivotsWithPackages.Packages.Any() && DivotsWithPackages.Packages.All(p => p.IsContainer); }
		}

		public bool IsEmptyYard
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CYD; }
		}

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

		public DtbBookingConsolidation TransportBooking
		{
			get { return Booking != null ? Booking.ConsolidationSingleJob : null; }
		}

		Common.AutoDtbBooking IConsignmentAddress.Booking => Booking;

		public DtbBooking Booking
		{
			get { return Factory.Load<DtbBooking>(KN_KM_BookingMovement); }
		}

		IDtbBooking IDtbBookingInstruction.Booking => Booking;

		public DtbBookingInstruction MasterBookingInstruction
		{
			get
			{
				if (masterBookingInstruction == null)
				{
					masterBookingInstruction = Factory.Load<DtbBookingInstruction>(KN_KN_MasterBookingInstruction);
				}

				return masterBookingInstruction;
			}
		}

		DtbBookingInstruction masterBookingInstruction;

		IEnumerable<IDtbBookingConfirmation> IDtbBookingInstruction.Confirmations => Confirmations.ToArray();

		[ChildEditable]
		public DtbBookingConfirmationCollection Confirmations
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

		IEnumerable<DtbBookingConfirmation> ConfirmationsTyped
		{
			get { return Confirmations.Cast<DtbBookingConfirmation>(); }
		}

		DtbBookingConfirmationCollection confirmations;

		DtbBookingConfirmationCollection GetNewConfirmationsCollection()
		{
			return new DtbBookingConfirmationCollection(this);
		}

		public DtbBookingConfirmation FirstPickupConfirmation
		{
			get
			{
				foreach (DtbBookingConfirmation confirmation in Confirmations)
				{
					if (confirmation.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp)
					{
						return confirmation;
					}
				}
				return null;
			}
		}

		public DtbBookingConfirmation LastDeliveryConfirmation
		{
			get
			{
				foreach (DtbBookingConfirmation confirmation in Confirmations)
				{
					if (confirmation.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery)
					{
						return confirmation;
					}
				}
				return null;
			}
		}

		PkgPackageJob PackageJob
		{
			get { return TransportBooking.PackageJob; }
		}

		DivotsWithPackagesCollection GetNewDivotsWithPackagesCollection()
		{
			return new DivotsWithPackagesCollection(this);
		}

		public DivotsWithPackagesCollection DivotsWithPackages
		{
			get { return divotsWithPackages ?? (divotsWithPackages = GetNewDivotsWithPackagesCollection()); }
		}

		DivotsWithPackagesCollection divotsWithPackages;

		[ChildEditable]
		IActiveBusinessObjectCollection<IDtbBookingInstructionPkgDivot> IDtbBookingInstruction.PackageDivots => PackageDivots;

		[ChildEditable]
		public DtbBookingInstructionPkgDivotCollection PackageDivots
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

		DtbBookingInstructionPkgDivotCollection packageDivots;

		DtbBookingInstructionPkgDivotCollection GetNewInstructionPkgDivotsCollection()
		{
			return new DtbBookingInstructionPkgDivotCollection(this);
		}

		public RateTransportZone Zone
		{
			get { return Factory.Load<RateTransportZone>(KN_TZ_DomesticZone); }
		}

		// persistent

		[RelatedBusinessObject("Booking")]
		public override ZGuid KN_KM_BookingMovement
		{
			get { return base.KN_KM_BookingMovement; }
			set { base.KN_KM_BookingMovement = value; }
		}

		[ResourceStringData("DtbBookingInstruction|DropMode", Caption = "Drop Mode")]
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

		void BeforeKN_DropModeSet()
		{
			SplitInstructionAndAssignAllUnselectedPackages();
		}

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
					Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(KN_InstructionTypeInfo), IsCopying);
				}
			}
		}

		void BeforeKN_InstructionTypeSet()
		{
			SplitInstructionAndAssignAllUnselectedPackages();
		}

		void AfterKN_InstructionTypeSet()
		{
			UpdateStatus();
			CreateDefaultConfirmation();
			ConNoteNoInfo.RefreshBinding();

			if (Booking != null)
			{
				Booking.UpdateConfirmationsEstimateTime();
			}
		}

		[ReadOnly(true)]
		public override ZInt KN_Sequence
		{
			get { return base.KN_Sequence; }
			set { base.KN_Sequence = value; }
		}

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

		public override ZGuid KN_KN_MasterBookingInstruction
		{
			get { return base.KN_KN_MasterBookingInstruction; }
			set
			{
				if (base.KN_KN_MasterBookingInstruction != value)
				{
					base.KN_KN_MasterBookingInstruction = value;
					masterBookingInstruction = null;
					docAddresses = null;
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

		ZString GetExpectedStatus()
		{
			ZString result = "";

			if (IsMulti)
			{
				if (PickupConfirmationsComplete)
				{
					result = TransportStatuses.Codes.PickedUp;
				}
				else if (DeliveryConfirmationsComplete)
				{
					result = TransportStatuses.Codes.Delivered;
				}
			}
			else if (IsDelivery && DeliveryConfirmationsComplete)
			{
				result = TransportStatuses.Codes.Delivered;
			}
			else if (IsPickUp && PickupConfirmationsComplete)
			{
				result = TransportStatuses.Codes.PickedUp;
			}

			return result;
		}

		bool DeliveryConfirmationsComplete
		{
			get { return Confirmations.Where(c => c.IsDelivery && c.PackageDivot == null).IsConfirmationsComplete() || PackageDivots.IsDeliveryComplete; }
		}

		bool PickupConfirmationsComplete
		{
			get { return Confirmations.Where(c => c.IsPickUp && c.PackageDivot == null).IsConfirmationsComplete() || PackageDivots.IsPickUpComplete; }
		}

		public void MarkAsNeedingStatusCheck()
		{
			var booking = Booking;
			if (booking != null)
			{
				booking.MarkAsNeedingStatusCheck();
			}
		}

		// calculated

		[MaxLength(DtbBookingConfirmation.Schema.KK_ReferenceNumMaxLength)]
		[ReadOnlyMember(nameof(ConNoteNoReadOnly))]
		[ResourceStringData("DtbBookingInstruction|ConNoteNo", Caption = "Connote #")]
		public ZString ConNoteNo
		{
			get
			{
				ZString result = "";

				if (!IsPickUp)
				{
					result = ConNoteNoConfirmations.GetSingleValueOrManyText(o => o.KK_ReferenceNum);
				}

				return result;
			}
			set
			{
				var conNotes = ConNoteNoConfirmations;
				if (conNotes.Count() > 1)
				{
					throw new ArgumentException("Cannot set ConNoteNo when there are > 1 ConNote Confirmations.");
				}

				var conNote = conNotes.FirstOrDefault() ?? Confirmations.AddNew(ConfirmationTypes.Codes.ConNoteNo);
				conNote.KK_ReferenceNum = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is to address the unit test: Enterprise.TransportBookings.Business.Test.DtbBookingInstructionBizOTest.TestConNoteNo_ReadOnly()")]
		bool ConNoteNoReadOnly
		{
			get { return IsPickUp || ConNoteNoConfirmations.Count() > 1; }
		}

		public ZPropertyInfo ConNoteNoInfo
		{
			get { return GetZPropertyInfo(nameof(ConNoteNo)); }
		}

		IEnumerable<DtbBookingConfirmation> ConNoteNoConfirmations
		{
			get { return Confirmations.Where(c => c.IsConNoteNo); }
		}

		public override ZBool KN_IsContainerRateable
		{
			get { return base.KN_IsContainerRateable; }
			set
			{
				base.KN_IsContainerRateable = value;

				var booking = Booking;
				if (booking != null)
				{
					Array.ForEach(booking.Instructions.Where(i => i != this).ToArray(), i => i.Validation.ValidateKN_IsContainerRateable());
				}
			}
		}

		public override ZBool KN_IsLooseRateable
		{
			get { return base.KN_IsLooseRateable; }
			set
			{
				base.KN_IsLooseRateable = value;

				var booking = Booking;
				if (booking != null)
				{
					Array.ForEach(booking.Instructions.Where(i => i != this).ToArray(), i => i.Validation.ValidateKN_IsLooseRateable());
				}
			}
		}

		// calculated

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

		internal DtbBookingInstruction FindRelatedConsignorInstruction()
		{
			var booking = Booking;
			return booking != null
				? booking.Instructions.Cast<DtbBookingInstruction>().FirstOrDefault(i => i.OrganisationType == OrganisationTypesList.Codes.CNR && (i.IsPickUp || i.IsMulti))
				: null;
		}

		IEnumerable<DtbBookingInstruction> FindRelatedConsigneeInstructions()
		{
			var booking = Booking;
			return booking != null
				? booking.Instructions.Cast<DtbBookingInstruction>().Where(i => i.IsAuthorisedToLeaveAvailable)
				: Enumerable.Empty<DtbBookingInstruction>();
		}

		public bool IsAuthorisedToLeaveAvailable
		{
			get { return (IsDelivery || IsMulti) && OrganisationType == OrganisationTypesList.Codes.CNE; }
		}

		public IDisposable SuspendMarkingAsNeedingAtlLog()
		{
			return new DisposableAction(() => markingAsNeedingAtlLog++, () => markingAsNeedingAtlLog--);
		}

		public ZBool IsMarkingAsNeedingAtlLogSuspended
		{
			get { return markingAsNeedingAtlLog > 0; }
		}

		int markingAsNeedingAtlLog;

		[ResourceStringData("DtbBookingInstruction|Description", Caption = "Instruction")]
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

		[ResourceStringData("DtbBookingInstruction|OrganisationType", ShortCaption = "Org. Type", Caption = "Organization Type")]
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

					var previousValue = OrganisationType;

					CheckMaximumLength(OrganisationTypeInfo, value);
					SetNonPersistentPropertyValue(OrganisationTypeInfo, ref organisationType, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrganisationType();
					}
					Address.DocAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(organisationType);

					AfterOrganisationTypeSet();

					UpdateAuthorisedToLeave();

					UpdateCO2eStatusToNotCurrentWhenOrgTypeChanged(previousValue, OrganisationType);
				}
			}
		}

		void UpdateCO2eStatusToNotCurrentWhenOrgTypeChanged(ZString previousValue, ZString newValue)
		{
			if (previousValue != newValue && (previousValue == LocalCartageJobOrgTypeList.Codes.CYD || newValue == LocalCartageJobOrgTypeList.Codes.CYD))
			{
				Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(OrganisationTypeInfo, previousValue), IsCopying);
			}
		}

		void BeforeOrganisationTypeSet()
		{
			SplitInstructionAndAssignAllUnselectedPackages();
			SetEventsNeedToBeSentOnConsolidationLevel();
		}

		void AfterOrganisationTypeSet()
		{
			DefaultAddressFromParent();
			DefaultDatesAndReferencesFromParent();
			CreateDefaultConfirmation();
			OrganisationTypeInfo.RefreshBinding();
			SetEventsNeedToBeSentOnConsolidationLevel();
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

		ZString GetPackageCategory()
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

		/// <summary>
		/// Attempt to assign packages from those passed in
		/// Otherwise fall back to package job packages
		/// </summary>
		/// <param name="possiblePackages"></param>
		public void DefaultPackages(IEnumerable<PkgPackage> possiblePackages)
		{
			DefaultPackagesCore(possiblePackages, shouldRemoveExistingPackages: true); // will remove all packages if !possiblePackages.Any()

			if (!DivotsWithPackages.Typed.Any())
			{
				DefaultPackagesCore(PackageJob.Packages, shouldRemoveExistingPackages: true);
			}
		}

		public void DefaultPackagesWithoutFallback(IEnumerable<PkgPackage> possiblePackages, bool shouldRemoveExistingPackages = true)
		{
			DefaultPackagesCore(possiblePackages, shouldRemoveExistingPackages);
		}

		public void DefaultPackageCategoryIfEmpty()
		{
			PackageCategory = (PackageCategory == ZString.Empty) ? GetNewPackageCategoryForRefresh() : PackageCategory;
		}

		ZString GetNewPackageCategoryForRefresh()
		{
			if (Booking.BookingTemplate is DtbBookingTmpl bookingTemplate &&
				bookingTemplate.Instructions?.Find(i => i.K2_InstructionType == KN_InstructionType && i.K2_Sequence == KN_Sequence).FirstOrDefault() is DtbBookingInstructionTmpl matchingBookingInstructionTmpl &&
				matchingBookingInstructionTmpl.K2_PackageType != ZString.Empty)
			{
				return matchingBookingInstructionTmpl.K2_PackageType;
			}

			return PackageCategories.Codes.Outers;
		}

		/// <summary>
		/// [Category]		[Add]
		/// Containers		Containers
		/// Both			Containers and their child Loose + Top Level Loose
		/// Outer			Containers and Top Level Loose
		/// Loose			Loose inside containers and Top Level Loose
		/// </summary>
		void DefaultPackagesCore(IEnumerable<PkgPackage> possiblePackages, bool shouldRemoveExistingPackages)
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

#if NETFRAMEWORK
				packagesToProcess = packagesToProcess.DistinctBy(p => p.PK).ToList();
#else
				packagesToProcess = IEnumerableExtensions.DistinctBy(packagesToProcess, p => p.PK).ToList();
#endif
				using (DelayDefaultPackagesUpdates(packagesToProcess))
				{
					if (shouldRemoveExistingPackages)
					{
						var packagesToRemove = DivotsWithPackages.Packages.Except(packagesToProcess).ToArray();
						Array.ForEach(packagesToRemove, p => DivotsWithPackages.RemovePackage(p));
					}
					Array.ForEach(packagesToProcess.ToArray(), p => DivotsWithPackages.AddPackage(p));
				}
			}
		}

		IDisposable DelayDefaultPackagesUpdates(List<PkgPackage> packages)
		{
			IDisposable delayListChangedEventsDisposable = null;
			return new DisposableAction(
				() =>
				{
					delayListChangedEventsDisposable = ActiveBusinessObjectCollection.DelayListChangedEvents(Factory);
					Booking.DelayInstructionUpdatesFromAddingDivots = true;
				},
				() =>
				{
					delayListChangedEventsDisposable?.Dispose();
					delayListChangedEventsDisposable = null;
					Booking.DelayInstructionUpdatesFromAddingDivots = false;
					DtbBookingInstructionPkgDivot.AfterDivotsAssignedToInstruction(this);
					DivotsWithPackagesCollection.AfterPackagesAdded(this, packages);
				});
		}

		[ResourceStringData("DtbBookingInstruction|StatusDescription", Caption = "Status")]
		public ZString StatusDescription
		{
			get { return Lookups.BindToLists.Statuses.GetDescriptionFromCode(KN_Status); }
		}

		public ZPropertyInfo<ZString> StatusDescriptionInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(nameof(StatusDescription)); }
		}

		public ZString PackageContainerID
		{
			get
			{
				ZString result = "";

				var packages = DivotsWithPackages.Packages.ToArray();
				var package = packages.FirstOrDefault();
				if (package != null)
				{
					result = packages.All(p => p.KP_F3_NKPackType == package.KP_F3_NKPackType)
						? string.Join(", ", packages.Where(p => !p.KP_PackageID.IsEmpty).Select(p => p.KP_PackageID))
						: Res.GetString("6f95d795-b3a8-45e5-995e-ab91bfa65381", "Many");
				}

				return result;
			}
		}

		public ZDecimal PackageQty
		{
			get { return DivotsWithPackages.Packages.Sum(p => p.KP_PackageQty); }
		}

		public ZString PackageType => DivotsWithPackages.Packages.GetSingleValueOrManyText(o => o.KP_F3_NKPackType);

		public ZString ContainerType
		{
			get
			{
				ZString result = "";

				var containers = DivotsWithPackages.Packages.Where(p => p.IsContainer && p.Container.ContainerType != null).ToArray();
				var container = containers.FirstOrDefault();
				if (container != null)
				{
					var containerType = container.Container.ContainerType.RC_Code;
					result = containers.All(p => p.Container.ContainerType.RC_Code == containerType)
						? containerType
						: ZString.Empty;
				}

				return result;
			}
		}

		public ZString ContainerMode
		{
			get
			{
				ZString result = "";

				var packages = DivotsWithPackages.Packages.ToArray();
				var package = packages.FirstOrDefault(p => p.IsContainer);
				if (package != null)
				{
					var containerMode = package.Container.K0_ContainerMode;
					result = packages.Where(p => p.IsContainer).All(p => p.Container.K0_ContainerMode == containerMode)
						? containerMode
						: ZString.Empty;
				}

				return result;
			}
		}

		// panel view

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbBookingInstruction|Estimated", Caption = "Estimated")]
		public ZDateTime Estimated
		{
			get
			{
				var lastConfirm = DefaultConfirmations.OrderBy(c => c.KK_Estimated).LastOrDefault();
				return lastConfirm != null ? lastConfirm.KK_Estimated : ZDateTime.Empty;
			}
			set
			{
				GetAndCreateDefaultConfirmationsIfNotExists.ForEach(c => c.KK_Estimated = value);

				if (!IsValidationSuspended)
				{
					ValidateEstimatedOnAllInstructions();
				}

				EstimatedInfo.RefreshBinding();
			}
		}

		void ValidateEstimatedOnAllInstructions()
		{
			var booking = Booking;
			if (booking != null)
			{
				foreach (var instruction in booking.Instructions)
				{
					instruction.Validation.ValidateEstimated(); // tested in DtbBookingInstructionValidation
				}
			}
		}

		public ZPropertyInfo<ZDateTime> EstimatedInfo
		{
			get { return (ZPropertyInfo<ZDateTime>)GetZPropertyInfo(Schema.Estimated); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbBookingInstruction|Actual", Caption = "Actual")]
		public ZDateTime Actual
		{
			get
			{
				var lastConfirm = DefaultConfirmations.OrderBy(c => c.KK_Actual).LastOrDefault();
				return lastConfirm != null ? lastConfirm.KK_Actual : ZDateTime.Empty;
			}
			set
			{
				GetAndCreateDefaultConfirmationsIfNotExists.ForEach(c => c.KK_Actual = value);

				if (!IsValidationSuspended)
				{
					ValidateActualOnAllInstructions();
				}

				ActualInfo.RefreshBinding();
			}
		}

		void ValidateActualOnAllInstructions()
		{
			var booking = Booking;
			if (booking != null)
			{
				foreach (var instruction in booking.Instructions)
				{
					instruction.Validation.ValidateActual(); // tested in DtbBookingInstructionValidation
				}
			}
		}

		public ZPropertyInfo<ZDateTime> ActualInfo
		{
			get { return (ZPropertyInfo<ZDateTime>)GetZPropertyInfo(Schema.Actual); }
		}

		DtbBookingConfirmation CreateBookingConfirmation()
		{
			var confirmation = Confirmations.AddNew();

			confirmation.KK_ConfirmationType = IsPickUp ? ConfirmationTypes.Codes.PickUp : ConfirmationTypes.Codes.Delivery;

			return confirmation;
		}

		public IEnumerable<DtbBookingConfirmation> DefaultConfirmations
		{
			get
			{
				var defaultConfirmations = Confirmations.Cast<DtbBookingConfirmation>();
				return IsPickUp ? defaultConfirmations.Where(c => c.IsPickUp) : defaultConfirmations.Where(c => c.IsDelivery);
			}
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbBookingInstruction|ReqFrom", Caption = "Required From", ShortCaption = "Req. From")]
		public ZDateTime ReqFrom
		{
			get
			{
				var lastConfirm = DefaultConfirmations.OrderBy(c => c.KK_RequiredFrom).LastOrDefault();
				return lastConfirm != null ? lastConfirm.KK_RequiredFrom : ZDateTime.Empty;
			}
			set
			{
				GetAndCreateDefaultConfirmationsIfNotExists.ForEach(c => c.KK_RequiredFrom = value);
				ReqFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo<ZDateTime> ReqFromInfo
		{
			get { return (ZPropertyInfo<ZDateTime>)GetZPropertyInfo(Schema.ReqFrom); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbBookingInstruction|ReqTo", Caption = "Required To", ShortCaption = "Req. To")]
		public ZDateTime ReqTo
		{
			get
			{
				var lastConfirm = DefaultConfirmations.OrderBy(c => c.KK_RequiredTo).LastOrDefault();
				return lastConfirm != null ? lastConfirm.KK_RequiredTo : ZDateTime.Empty;
			}
			set
			{
				GetAndCreateDefaultConfirmationsIfNotExists.ForEach(c => c.KK_RequiredTo = value);
				ReqToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo<ZDateTime> ReqToInfo
		{
			get { return (ZPropertyInfo<ZDateTime>)GetZPropertyInfo(Schema.ReqTo); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbBookingInstruction|SignedBy", Caption = "Signed By")]
		public ZString SignedBy
		{
			get
			{
				var lastConfirm = Confirmations.OrderBy(c => c.KK_Actual).LastOrDefault();
				return lastConfirm != null ? lastConfirm.KK_ReceivedBy : ZString.Empty;
			}
			set
			{
				Array.ForEach(Confirmations.ToArray(), c => c.KK_ReceivedBy = value);
				SignedByInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo<ZString> SignedByInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(Schema.SignedBy); }
		}

		[ResourceStringData("DtbBookingInstruction|PackageCategoryDescription", Caption = "Package Type")]
		public ZString PackageCategoryDescription
		{
			get { return Lookups.PackageCategories.GetDescriptionFromCode(PackageCategory); }
		}

		public ZPropertyInfo<ZString> PackageCategoryDescriptionInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(Schema.PackageCategoryDescription); }
		}

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

		// Defaulting From Parent

		void DefaultAddressFromParent()
		{
			var booking = TransportBooking;
			if (booking != null)
			{
				var parent = booking.Parent;
				if (parent != null && (Address != null && Address.DocAddressType != DocAddressType.None))
				{
					parent.UpdateAddress(Address, true, Booking.ContainerLinks, Booking.ContainerNumbers);
				}
			}
		}

		public void DefaultDatesAndReferencesFromParent(DtbBookingInstructionPkgDivot newlyAddedDivot = null)
		{
			if (TransportBooking != null && TransportBooking.Parent != null && !OrganisationType.IsEmpty)
			{
				var packages = GetPackagesIncludingNewDivotPackage(newlyAddedDivot);

				// for each confirmation type for this Organisation Type - get all dates and references for each package on this instruction
				var possibleConfirmationsForOrgType = TransportRegistry.Instance.DateAndReference.Value.GetDateAndReferenceByOrganisationType(OrganisationType);
				foreach (DateAndReference dateAndReferenceRegistry in possibleConfirmationsForOrgType)
				{
					var confirmationTypeCode = dateAndReferenceRegistry.Code;

					var datesAndRefsPerPackage = new Dictionary<PkgPackage, DatesAndReference>();

					foreach (var package in packages)
					{
						var matchingDatesAndRefsForPackageFromParent = TransportBooking.Parent.GetDatesAndReferences(dateAndReferenceRegistry.Code, OrganisationType, package, ReleaseNumbersByPackage);
						datesAndRefsPerPackage.Add(package, matchingDatesAndRefsForPackageFromParent);
					}

					if (datesAndRefsPerPackage.Count > 0 && !datesAndRefsPerPackage.Values.All(d => d.IsEmpty))
					{
						var distinct = datesAndRefsPerPackage.Values.Distinct();
						if (distinct.Count() == 1) // all packages have the same date and ref values, so add one confirmation for the entire Instruction (means all packages for that instruction)
						{
							AssignAllConfirmationsTheSameDatesAndReferences(confirmationTypeCode, distinct.First());
						}
						else // otherwise assign a confirmation and their different date and ref values to each package (divot)
						{
							AssignAConfirmationToEachDivot(datesAndRefsPerPackage, confirmationTypeCode, newlyAddedDivot);
						}
					}
				}

				if (Confirmations.Count == 0 && !((ISupportDataImporting)Booking).IsImportingData)
				{
					var defaultConfirmationCode = GetDefaultConfirmationCode();
					var dates = TransportBooking.Parent.GetDatesAndReferences(defaultConfirmationCode, OrganisationType, null, null);
					if (!defaultConfirmationCode.IsEmpty)
					{
						SetupConfirmation(Confirmations.AddNew(), defaultConfirmationCode, dates);
					}
				}
			}
		}

		internal IDisposable SetReleaseNumbersByPackage(IReadOnlyDictionary<PkgPackage, ZString> releaseNumbersByPackage)
		{
			return new DisposableAction(
				() => ReleaseNumbersByPackage = releaseNumbersByPackage,
				() => ReleaseNumbersByPackage = null);
		}

		IReadOnlyDictionary<PkgPackage, ZString> ReleaseNumbersByPackage;

		/// <summary>
		/// bug - Packages don't include newly attached divot with a package
		/// </summary>
		/// <returns></returns>
		List<PkgPackage> GetPackagesIncludingNewDivotPackage(DtbBookingInstructionPkgDivot newlyAddedDivot)
		{
			var packages = new List<PkgPackage>(DivotsWithPackages.Packages);
			if (newlyAddedDivot != null && newlyAddedDivot.Package != null && !packages.Contains(newlyAddedDivot.Package))
			{
				packages.Add(newlyAddedDivot.Package);
			}

			return packages;
		}

		void AssignAllConfirmationsTheSameDatesAndReferences(ZString confirmationTypeCode, DatesAndReference same)
		{
			// if there is currently an existing ALL confirmation, do nothing - do not try to update it, UNLESS the all confirmation is empty - see else
			var existingAllConfirmation = Confirmations.Where(c => c.PackageDivot == null).FirstOrDefault(c => c.KK_ConfirmationType == confirmationTypeCode);
			if (existingAllConfirmation == null)
			{
				// if there are NO existing divot confirmations, add and setup an ALL confirmation
				// otherwise, add and setup a confirmation on those divots that don't have one
				var existingDivotConfirmations = Confirmations.Where(c => c.KK_ConfirmationType == confirmationTypeCode && !c.KK_KD_BookingInstructionPkgDivot.IsEmpty);
				if (!existingDivotConfirmations.Any())
				{
					SetupConfirmation(Confirmations.AddNew(), confirmationTypeCode, same);
				}
				else
				{
					foreach (var divot in PackageDivots)
					{
						// only add and setup if NO existing confirmation
						var existingConfirmation = divot.Confirmations.FirstOrDefault(c => c.KK_ConfirmationType == confirmationTypeCode);
						if (existingConfirmation == null)
						{
							SetupConfirmation(divot.Confirmations.AddNew(), confirmationTypeCode, same);
						}
					}
				}
			}
			else if (existingAllConfirmation.IsEmptyValues)
			{
				SetupConfirmation(existingAllConfirmation, confirmationTypeCode, same);
			}
		}

		void AssignAConfirmationToEachDivot(Dictionary<PkgPackage, DatesAndReference> datesAndRefsPerPackage, ZString confirmationTypeCode, DtbBookingInstructionPkgDivot newlyAddedDivot)
		{
			// if there is currently an existing ALL confirmation, split it across all existing divots
			// DO NOT APPLY IT TO NEW DIVOT if there is one, instead give it a fresh (default values) confirmation and set it up with the values pulled from the parent
			var existingAllConfirmation = Confirmations.Where(c => c.PackageDivot == null).FirstOrDefault(c => c.KK_ConfirmationType == confirmationTypeCode);
			if (existingAllConfirmation != null && newlyAddedDivot != null)
			{
				existingAllConfirmation.SplitFromInstructionToPackageDivots(null, newlyAddedDivot);
			}

			foreach (var divot in PackageDivots)
			{
				// only add and setup if NO existing confirmation
				var existingConfirmation = divot.Confirmations.FirstOrDefault(c => c.KK_ConfirmationType == confirmationTypeCode);
				if (existingConfirmation == null)
				{
					var packageDatesAndReference = DatesAndReference.Empty;
					if (datesAndRefsPerPackage.TryGetValue(divot.Package, out packageDatesAndReference) && (!packageDatesAndReference.IsEmpty || confirmationTypeCode == divot.Instruction.KN_InstructionType || (divot.Instruction.KN_InstructionType == "MLT" && confirmationTypeCode == "DLV"))) // is emtpy or exact type ie PIC = PIC or DLV = DLV .. slot and connote don't default unless they have values
					{
						SetupConfirmation(divot.Confirmations.AddNew(), confirmationTypeCode, packageDatesAndReference);
					}
				}
			}
		}

		void SetupConfirmation(DtbBookingConfirmation confirmation, ZString confirmationTypeCode, DatesAndReference datesAndReference)
		{
			confirmation.KK_ConfirmationType = confirmationTypeCode;
			confirmation.KK_Estimated = datesAndReference.Estimated;
			confirmation.KK_Actual = datesAndReference.Actual;
			confirmation.KK_RequiredFrom = datesAndReference.ReqFrom;
			confirmation.KK_RequiredTo = datesAndReference.ReqTo;
			confirmation.KK_ReferenceNum = datesAndReference.Reference;
			confirmation.KK_SlotDateTime = datesAndReference.SlotTime;
			confirmation.KK_SlotReference = datesAndReference.SlotReference;
		}

		//DJB check packages perhaps
		bool IsAir
		{
			get { return false; }
		}

		public bool HasAContainer
		{
			get { return DivotsWithPackages.Packages.Any(p => p.IsContainer); }
		}

		public bool IsDelivery
		{
			get { return KN_InstructionType.EqualsIgnoringCase(InstructionTypes.Codes.Delivery); }
		}

		public bool IsLoose
		{
			get { return DivotsWithPackages.Count > 0 && !HasAContainer; }
		}

		public bool IsMulti
		{
			get { return KN_InstructionType.EqualsIgnoringCase(InstructionTypes.Codes.Multi); }
		}

		public bool IsPickUp
		{
			get { return KN_InstructionType.EqualsIgnoringCase(InstructionTypes.Codes.PickUp); }
		}

		public ZBool IsDepot
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CFS; }
		}

		// When support is added for containers and Local Transport is migrated,
		// this will have to be changed to check if the organisation is the
		// OrgProxy as Depot multis that are not auto added will be valid.
		public ZBool IsOwnDepot
		{
			get { return IsMulti && IsDepot; }
		}

		public ZBool IsCTO
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CTO; }
		}

		public ZBool IsCYD
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CYD; }
		}

		public bool IsComplete
		{
			get
			{
				return
					(IsDelivery && KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.Delivered)) ||
					(IsPickUp && KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.PickedUp)) ||
					(IsMulti && KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.PickedUp));
			}
		}

		public bool IsCompleteDeliveryOnly
		{
			get
			{
				return
					(IsDelivery && KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.Delivered)) ||
					(IsMulti && (KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.Delivered) || KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.PickedUp)));
			}
		}

		public bool IsSub
		{
			get { return KN_KN_MasterBookingInstruction != ZGuid.Empty; }
		}

		public ZString GetDefaultConfirmationCode()
		{
			var result = "";

			switch (KN_InstructionType)
			{
				case InstructionTypes.Codes.PickUp:
					result = ConfirmationTypes.Codes.PickUp;
					break;

				case InstructionTypes.Codes.Delivery:
					result = ConfirmationTypes.Codes.Delivery;
					break;

				case InstructionTypes.Codes.Multi: //DeliveryAndPickup:
					if (Booking != null && Booking.IsPickupDirection)
					{
						result = ConfirmationTypes.Codes.PickUp;
					}
					else
					{
						result = ConfirmationTypes.Codes.Delivery;
					}
					break;
			}

			return result;
		}

		public override bool ReadOnly
		{
			// view mode is tested once in DtbBookingConsolidationTest.TestReadOnlyForChildren()
			get { return ConsolidationViewModeService.GetViewMode(Factory) == ConsolidationViewMode.MultiJob || base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		public new DtbBookingInstructionLookups Lookups
		{
			get { return (DtbBookingInstructionLookups)base.Lookups; }
		}

		protected override Common.DtbBookingInstructionLookups GetNewLookups()
		{
			return new DtbBookingInstructionLookups(this);
		}

		public new DtbBookingInstructionValidation Validation
		{
			get { return (DtbBookingInstructionValidation)base.Validation; }
		}

		protected override Common.DtbBookingInstructionValidation GetNewValidation()
		{
			return new DtbBookingInstructionValidation(this);
		}

		// split and duplicate this instruction if other packages reference this and we are viewing it from a specific package

		bool ValidToSplit
		{
			get
			{
				return Booking != null
					&& Booking.InstructionView == TransportBookingInstructionView.Package
					&& Booking.SelectedPackage_PackageView != null
					&& DivotsWithPackages.Count > 1
					&& !IsCopying;
			}
		}

		void SplitInstructionAndAssignAllUnselectedPackages()
		{
			if (ValidToSplit)
			{
				var newInstruction = Clone();

				// move all packages that are not selected
				foreach (var divot in PackageDivots.ToArray())
				{
					if (divot.KD_KP_Package != Booking.SelectedPackage_PackageView.Package.PK)
					{
						var divotConfirmations = divot.Confirmations.Where(c => c.PackageDivot != null);

						divot.KD_KN_BookingInstruction = newInstruction.PK;

						foreach (var divotConfirmation in divotConfirmations)
						{
							divotConfirmation.KK_KN_BookingInstruction = newInstruction.PK;
						}
					}
				}
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (DtbBookingInstruction)base.CloneInternal(args);

			Booking.Instructions.Sequence();

			CloneDocAddresses(result);
			CloneConfirmations(result);

			return result;
		}

		void CloneDocAddresses(DtbBookingInstruction result)
		{
			result.DocAddresses.RemoveAndDeleteAll();
			foreach (var docAddress in DocAddresses)
			{
				result.DocAddresses.Add(docAddress.Clone());
			}
		}

		void CloneConfirmations(DtbBookingInstruction result)
		{
			// confirmations - only those that are for ALL packages on this instruction (those Assigned to the package divots remain)

			result.Confirmations.DeleteAll();
			foreach (var confirmation in Confirmations.Where(c => c.PackageDivot == null).ToArray())
			{
				result.Confirmations.Add((DtbBookingConfirmation)confirmation.Clone());
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbBookingInstructionFetchStrategy(this);
		}

		void SetEventsNeedToBeSentOnConsolidationLevel()
		{
			var consolidation = Booking?.ConsolidationSingleJob;
			if (consolidation != null)
			{
				if (IsPickUp || IsMulti)
				{
					consolidation.CheckIfPUPNeedsToBeSent = true;
				}

				if (IsDelivery || IsMulti)
				{
					consolidation.CheckIfDLVNeedsToBeSent = true;
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			// tested by WorkFlowTests

			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			if (markedAsNeedingAtlLog && ((!IsInDatabase && KN_IsAuthorisedToLeave) || KN_IsAuthorisedToLeaveInfo.HasChanges))
			{
				Logs.CreateOrRecreateEventLog(KN_IsAuthorisedToLeave ? Events.Authorised : Events.AuthorisationWithdrawn, EstimateActual.Actual, ZDateTimeOffset.Now, "",
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceActionAuthorisedTypes.AuthorisedToLeave));
			}

			InitialiseMasterFieldsIfNew();
			MasterBookingHelper.UpdateMasterBookingVersion();

			base.OnSaving();
		}

		public BusinessObject GetTopBusinessObject()
		{
			return Factory.Load(DtbBookingSchema.Constants.Prefix, KN_KM_BookingMovement);
		}

		public override void Delete()
		{
			Confirmations.DeleteAll();
			PackageDivots.DeleteAll(); // tested by SaveAndDeleteBusinessObject()
			DocAddresses.RemoveAndDeleteAll();

			var query = new ZQuery(DtbBookingInstructionSchema.KN_KN_MasterBookingInstruction, this.PK);
			var subs = new ActiveBusinessObjectCollection<DtbBookingInstruction>(Factory, query);
			subs.DeleteAll();

			var bookingPriorToDelete = Booking;

			UpdateRelatedConsigneeATL();

			// tested by WorkflowProvider tests
			WorkflowItems.RemoveAndDeleteAll();

			this.KN_KM_BookingMovement = ZGuid.Empty; // to update parent status
			SetEventsNeedToBeSentOnConsolidationLevel();
			base.Delete();

			OnAfterDelete(bookingPriorToDelete);
		}

		void OnAfterDelete(DtbBooking booking)
		{
			booking?.Instructions?.OnInstructionDeleted();
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

				var bookingPriorToDelete = Booking;
				base.DeleteForDataRefresh();
				OnAfterDelete(bookingPriorToDelete);
			}
		}

		public void CreateDefaultConfirmation()
		{
			if (Booking != null && !IsSub)
			{
				var direction = Booking.KM_Direction;
				var instructionType = KN_InstructionType;
				var orgType = OrganisationType;

				if (!direction.IsEmpty && !instructionType.IsEmpty && !orgType.IsEmpty)
				{
					var dateAndReferenceDefaults = TransportRegistry.Instance.DateAndReference.Value.FindDefaultConfirmations(direction, instructionType, orgType, string.Empty);
					foreach (var defaultValue in dateAndReferenceDefaults)
					{
						var code = defaultValue.Code;
						var hasConfirmationForAllPackages = Confirmations.Where(c => c.PackageDivot == null).Any(c => c.KK_ConfirmationType == code);
						if (!hasConfirmationForAllPackages)
						{
							var divotConfirms = Confirmations.Where(c => c.PackageDivot != null);
							if (divotConfirms.Any(c => c.KK_ConfirmationType == code))
							{
								var divotsWithoutConfirmation = PackageDivots.Where(d => !d.Confirmations.Any(c => c.KK_ConfirmationType == code));
								foreach (var packageDivot in divotsWithoutConfirmation)
								{
									packageDivot.Confirmations.AddNew(code);
								}
							}
							else
							{
								Confirmations.AddNew(code);
							}
						}
					}
				}
			}
		}

		IEnumerable<DtbBookingConfirmation> GetAndCreateDefaultConfirmationsIfNotExists
		{
			get
			{
				if (!DefaultConfirmations.Any())
				{
					CreateBookingConfirmation();
				}
				return DefaultConfirmations;
			}
		}

		public IDisposable SuspendOnDocAddressChanged()
		{
			return new SemaphoreManager(OnDocAddressChangedSemaphore);
		}

		Semaphore OnDocAddressChangedSemaphore
		{
			get { return onDocAddressChangedSemaphore ?? (onDocAddressChangedSemaphore = new Semaphore()); }
		}

		Semaphore onDocAddressChangedSemaphore;

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

		[ChildEditable()]
		[UniversalCopyCollectionEntity(JobDocAddressSchema.Constants.TableName, JobDocAddressSchema.Constants.E2_ParentID)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					if (IsSub)
					{
						docAddresses = MasterBookingInstruction.DocAddresses;
					}
					else
					{
						docAddresses = new JobDocAddressDependentCollection(this);
						docAddresses.Load();
					}
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

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

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			var addressType = (docAddress != null) ? docAddress.E2_AddressType : ZString.Empty;

			switch (addressType)
			{
				case DocAddressTypes.Codes.LocalCartageCFS:
					return Env.Security.DtbBookingMISCDetailsCFS;
				case DocAddressTypes.Codes.LocalCartageCTO:
					return Env.Security.DtbBookingMISCDetailsCTO;
				case DocAddressTypes.Codes.LocalCartageExporter:
					return Env.Security.DtbBookingMISCDetailsConsignor;
				case DocAddressTypes.Codes.LocalCartageImporter:
					return Env.Security.DtbBookingMISCDetailsConsignee;
				case DocAddressTypes.Codes.LocalCartageYard:
					return Env.Security.DtbBookingMISCDetailsContainerYard;
				case DocAddressTypes.Codes.LocalCartageService:
					return Env.Security.DtbBookingMISCDetailsOtherServiceProvider;
				case DocAddressTypes.Codes.LocalCartageMSC:
					return Env.Security.DtbBookingMISCDetailsMiscAddress;
				default:
					return Env.Security.DtbBookingMISCDetails;
			}
		}

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

		void OnDocAddressChanged(JobDocAddress docAddress)
		{
			if (!OnDocAddressChangedSemaphore.IsSuspended)
			{
				SetRelatedOrganisationFromBuyerSupplierLink();
				SetDomesticZone(docAddress);
				UpdateAuthorisedToLeave();
				DefaultDropMode();

				Booking?.UpdateConfirmationsEstimateTime();
				Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(freeTextReason: (NoResString)"Address"), IsCopying);
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
					var relatedInstruction = Booking.Instructions.Cast<DtbBookingInstruction>().FirstOrDefault(i => i.OrganisationType == relatedOrgType);
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

		void OnAnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
			SplitInstructionAndAssignAllUnselectedPackages();
		}

		void OnOrgAddressBeforeChange(JobDocAddress docAddress)
		{
			SplitInstructionAndAssignAllUnselectedPackages();
		}

		ZString GetParentDropMode()
		{
			var result = ZString.Empty;

			if (KN_DropMode.IsEmpty
				&& Booking != null
				&& Booking.ConsolidationSingleJob != null
				&& Booking.ConsolidationSingleJob.Parent != null
				&& Booking.ConsolidationSingleJob.Parent.DropMode.HasValue
				&& (OrganisationType == OrganisationTypesList.Codes.CNR || OrganisationType == OrganisationTypesList.Codes.CNE || OrganisationType == OrganisationTypesList.Codes.WHS))
			{
				result = Booking.ConsolidationSingleJob.Parent.DropMode.Value;
			}

			return result;
		}

		public void DefaultDropMode(DtbBookingInstructionTmpl instructionTemplate = null)
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

		public IDisposable SuspendDefaultingDropMode()
		{
			return new SemaphoreManager(SuspendDefaultingDropModeSemaphore);
		}

		Semaphore SuspendDefaultingDropModeSemaphore => suspendDefaultingDropModeSemaphore ?? (suspendDefaultingDropModeSemaphore = new Semaphore());
		Semaphore suspendDefaultingDropModeSemaphore;

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return new JobDocAddressRequirement(addressType, OrganisationAddressType, ContactType.LocalTransport, true);
		}

		AddressType OrganisationAddressType
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

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

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

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new DtbBookingInstructionProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		DtbBookingInstructionProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZGuid IWorkflowProviderCore.PK
		{
			get { return PK; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode; }
		}

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this);
			var loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
			properties.Add(loader.FindMatches(this));

			return new CustomBusinessObject(Factory, this, properties);
		}

		IJobDocAddress IDtbBookingInstruction.Address
		{
			get { return Address; }
		}

		IEnumerable<IPkgPackage> IDtbBookingInstruction.Packages
		{
			get { return GetPackages; }
		}

		ZDateTime IDtbBookingInstruction.EstimatedDate(bool isPickUp)
		{
			var confirmations = Confirmations.Cast<DtbBookingConfirmation>();
			var defaultConfirmations = isPickUp ? confirmations.Where(c => c.IsPickUp) : confirmations.Where(c => c.IsDelivery);
			var lastConfirm = defaultConfirmations.OrderBy(c => c.KK_Estimated).LastOrDefault();
			return lastConfirm != null ? lastConfirm.KK_Estimated : ZDateTime.Empty;
		}

		ZString IDtbBookingInstruction.OrganisationType
		{
			get { return OrganisationType; }
			set { OrganisationType = value; }
		}

		public JobDocAddress GetAddress(DocAddressType docAddressType)
		{
			return Address;
		}

		void InitialiseMasterFieldsIfNew()
		{
			if (!IsInDatabase && !IsDeleted)
			{
				if (KN_KN_MasterBookingInstruction != ZGuid.Empty)
				{
					if (KN_MasterBookingVersion == ZShort.Zero)
					{
						KN_MasterBookingVersion = (short)1;
					}
				}
				else
				{
					if (Booking != null)
					{
						if (!KN_IsMaster && Booking.KM_IsMaster)
						{
							KN_IsMaster = true;
						}
						else if (KN_IsMaster && !Booking.KM_IsMaster)
						{
							KN_IsMaster = false;
						}
						KN_MasterBookingVersion = (short)(KN_IsMaster ? 1 : 0);
					}
				}
			}
		}

		bool IDtbMasterBookingEntity.IsMaster => KN_IsMaster;

		short IDtbMasterBookingEntity.GetMasterBookingVersion() => KN_MasterBookingVersion;

		void IDtbMasterBookingEntity.SetMasterBookingVersion(short newMasterBookingVersion)
		{
			KN_MasterBookingVersion = newMasterBookingVersion;
		}

		IEnumerable<ZPropertyInfo> IDtbMasterBookingEntity.ReplicationFieldInfos
		{
			get
			{
				if (replicationFieldInfos == null)
				{
					replicationFieldInfos = DtbMasterBookingReplication.GetPropertyInfosFromListOfColumns(this, DtbMasterBookingReplication.DtbBookingInstructionReplicatedColumns);
				}

				return replicationFieldInfos;
			}
		}
		IEnumerable<ZPropertyInfo> replicationFieldInfos;

		DtbMasterBookingHelper MasterBookingHelper { get; }
	}
}
