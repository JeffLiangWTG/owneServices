using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingUNDGDataItem : UNDGDataItem, ICanDelete
	{
		public ForwardingUNDGDataItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool DI_PackingInstructionSection_ReadOnly => Substance == null || Substance.DG_Standard != UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA || !LithiumBatteryConstants.UNNOCodes.CodesList.Contains(Substance.DG_UNNO);

		public override ZString DI_UnitOfWeight
		{
			get => base.DI_UnitOfWeight;
			set
			{
				if (base.DI_UnitOfWeight != value)
				{
					base.DI_UnitOfWeight = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDI_DGWeight();
					}
				}
			}
		}

		public override ZString DI_UnitOfVolume
		{
			get => base.DI_UnitOfVolume;
			set
			{
				if (base.DI_UnitOfVolume != value)
				{
					base.DI_UnitOfVolume = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDI_DGVolume();
					}
				}
			}
		}

		public override ZGuid DI_DG
		{
			get => base.DI_DG;
			set
			{
				if (base.DI_DG != value)
				{
					base.DI_DG = value;

					if (IsDeleted)
					{
						return;
					}

					if (Substance == null || Substance.DG_FlashPoint.IsEmpty)
					{
						DI_DGFlashPoint = ZDecimal.Zero;
					}

					ClearRadioactiveFieldsIfNecessary();

					DI_IsNotOtherwiseSpecified = ZBool.False;

					DI_PackingInstructionSection = ZString.Empty;

					if (ApprovalCertificate_ReadOnly)
					{
						DI_ApprovalCertificateType = ZString.Empty;
						DI_ApprovalCertificateIDMark = ZString.Empty;
					}
				}
			}
		}

		#region Approval Certificate ReadOnly

		protected override bool ApprovalCertificate_ReadOnly => !UNDGSubstance.IsRadioactiveInExceptedQuantities();

		#endregion

		void ClearRadioactiveFieldsIfNecessary()
		{
			if (!IsSubstanceRadioactive())
			{
				DI_RadioactiveLabelCategory = ZString.Empty;
				DI_IsHighwayRouteControlledQuantity = false;
				DI_RadionuclideElement = ZString.Empty;
				DI_RadionuclideElementSuffix = ZString.Empty;
				DI_RadioactiveMaximumActivity = 0m;
				DI_RadioactiveMaximumActivityUnit = ZString.Empty;
				DI_RadioactiveTransportIndex = 0;
				DI_IsExclusiveUse = false;
			}

			if (!IsMaterialFormDescriptionRequired())
			{
				DI_MaterialFormDescription = ZString.Empty;
			}

			if (!IsFissileExceptedRequired())
			{
				DI_IsFissileExcepted = false;
			}
		}

		bool IsAbleToEditRadioactiveColumn()
		{
			return Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed && IsSubstanceRadioactive();
		}

		bool IsSubstanceRadioactive()
		{
			if (Substance == null)
			{
				return false;
			}

			return Substance.DG_Class == RadioactiveConstants.RadioactiveClass;
		}

		public override ZGuid DI_ParentID
		{
			get => base.DI_ParentID;
			set
			{
				if (base.DI_ParentID != value)
				{
					var isOriginalParentEmpty = base.DI_ParentID.IsEmpty;

					base.DI_ParentID = value;

					if (isOriginalParentEmpty)
					{
						TrySetDefaultUNDGContact();
					}
				}
			}
		}

		public override ZString DI_PackingInstructionSection
		{
			get => base.DI_PackingInstructionSection;
			set
			{
				if (base.DI_PackingInstructionSection != value)
				{
					base.DI_PackingInstructionSection = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDI_DGWeight();
						Validation.ValidateDI_DG();
					}
				}
			}
		}

		public override ZInt DI_PackageCount
		{
			get => base.DI_PackageCount;
			set
			{
				base.DI_PackageCount = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDI_DGWeight();
					Validation.ValidateDI_DG();
				}
			}
		}

		[List("Lookups.RadioactiveLabelCategoryList")]
		public override ZString DI_RadioactiveLabelCategory
		{
			get => base.DI_RadioactiveLabelCategory;
			set
			{
				if (base.DI_RadioactiveLabelCategory != value)
				{
					base.DI_RadioactiveLabelCategory = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDI_RadioactiveLabelCategory();
					}
				}
			}
		}

		protected override bool DI_RadioactiveLabelCategory_ReadOnly => !IsAbleToEditRadioactiveColumn();

		protected override bool DI_RadioactiveTransportIndex_ReadOnly => !IsAbleToEditRadioactiveColumn();

		bool ContainsHAZAllocation(OrgContact contact) =>
			contact.Allocations.OfType<OrgContactAllocation>().Any(allocation => allocation.PC_Type == OrgConstants.ContactAllocationType.HAZ);

		bool IsHVLV => DI_ParentTableCode == HVLVItemSchema.Constants.Prefix;

		void TrySetDefaultUNDGContact()
		{
			if (!IsInDatabase && DI_OC_DGContact.IsEmpty && ParentPackLine?.Shipment is ForwardingShipment shipment)
			{
				var pickupContacts = shipment?.ConsignorPickupAddress?.Organisation?.ContactsActive ?? new OrgContactDependentCollection(Factory);
				var shipperContacts = shipment?.ConsignorDocumentaryAddress?.Organisation?.ContactsActive ?? new OrgContactDependentCollection(Factory);

				var pickupContacts_withHAZ = pickupContacts
					.OfType<OrgContact>()
					.Where(ContainsHAZAllocation)
					.OrderBy(c => c.OC_ContactName);

				DI_OC_DGContact = pickupContacts_withHAZ.Count() == 1
					? pickupContacts_withHAZ.First().PK
					: pickupContacts_withHAZ.FirstOrDefault(contact => contact.WorkingAddressPK == shipment.ConsignorPickupAddress.E2_OA_Address)?.PK
						?? ZGuid.Empty;

				if (DI_OC_DGContact == ZGuid.Empty)
				{
					var shipperContacts_withHAZ = shipperContacts
						.OfType<OrgContact>()
						.Where(ContainsHAZAllocation)
						.OrderBy(c => c.OC_ContactName);

					DI_OC_DGContact = shipperContacts_withHAZ.Count() == 1
						? shipperContacts_withHAZ.First().PK
						: shipperContacts_withHAZ.FirstOrDefault(contact => contact.WorkingAddressPK == shipment.ConsignorDocumentaryAddress.E2_OA_Address)?.PK
							?? ZGuid.Empty;
				}
			}
		}

		protected override bool DI_DGFlashPoint_ReadOnly
		{
			get
			{
				if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
				{
					return !DI_IsCombustible;
				}
				else
				{
					return Substance == null;
				}
			}
		}

		public override ZDecimal DI_DGWeight
		{
			get => base.DI_DGWeight;
			set
			{
				if (base.DI_DGWeight != value)
				{
					base.DI_DGWeight = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDI_DG();
					}
				}
			}
		}

		public override ZBool DI_IsHighwayRouteControlledQuantity
		{
			get => base.DI_IsHighwayRouteControlledQuantity;
			set
			{
				if (base.DI_IsHighwayRouteControlledQuantity != value)
				{
					base.DI_IsHighwayRouteControlledQuantity = value;

					if (base.DI_IsHighwayRouteControlledQuantity)
					{
						DI_RadioactiveLabelCategory = RadioactiveLabelCategoryList.Codes.YellowIII;
					}
				}
			}
		}

		[List("Lookups.RadionuclideElementList")]
		public override ZString DI_RadionuclideElement
		{
			get { return base.DI_RadionuclideElement; }
			set
			{
				if (base.DI_RadionuclideElement != value)
				{
					base.DI_RadionuclideElement = value;
				}
			}
		}

		public override bool DI_RadionuclideElement_ReadOnly => !IsAbleToEditRadioactiveColumn();

		[List("Lookups.RadionuclideElementSuffixList")]
		public override ZString DI_RadionuclideElementSuffix
		{
			get { return base.DI_RadionuclideElementSuffix; }
			set
			{
				if (base.DI_RadionuclideElementSuffix != value)
				{
					base.DI_RadionuclideElementSuffix = value;
				}
			}
		}

		public override bool DI_RadionuclideElementSuffix_ReadOnly => !IsAbleToEditRadioactiveColumn();

		public override bool DI_RadioactiveMaximumActivity_ReadOnly => !IsAbleToEditRadioactiveColumn();

		[List("Lookups.RadioactiveMaximumActivityUnitList")]
		public override ZString DI_RadioactiveMaximumActivityUnit
		{
			get { return base.DI_RadioactiveMaximumActivityUnit; }
			set
			{
				if (base.DI_RadioactiveMaximumActivityUnit != value)
				{
					base.DI_RadioactiveMaximumActivityUnit = value;
				}
			}
		}

		public override bool DI_RadioactiveMaximumActivityUnit_ReadOnly => !IsAbleToEditRadioactiveColumn();

		#region DI_MaterialFormDescription

		public bool IsMaterialFormDescriptionRequired()
		{
			if (Substance == null)
			{
				return false;
			}

			var specialFormString = (NoResString)"SPECIAL FORM"; // Dangerous Goods Proper Shipping Name is not translatable
			var isAbleToEdit = IsAbleToEditRadioactiveColumn();
			var psnContainsSpecialForm = Substance.DG_PSN.ToUpper().Contains(specialFormString);
			var isMaterialFormDescriptionRequired = isAbleToEdit && !psnContainsSpecialForm;
			return isMaterialFormDescriptionRequired;
		}

		protected override bool DI_MaterialFormDescription_ReadOnly => !IsMaterialFormDescriptionRequired();

		#endregion

		protected override bool DI_IsHighwayRouteControlledQuantity_ReadOnly => !IsAbleToEditRadioactiveColumn();

		#region DI_IsFissileExcepted

		bool IsFissileExceptedRequired()
		{
			if (Substance == null)
			{
				return false;
			}

			var isAbleToEdit = IsAbleToEditRadioactiveColumn();
			var fissileString = "FISSILE"; // Dangerous Goods Proper Shipping Name is not translatable
			var psnContainsFissile = Substance.DG_PSN.ToUpper().Contains(fissileString);
			var isFissileExceptedRequired = isAbleToEdit && !psnContainsFissile;
			return isFissileExceptedRequired;
		}

		protected override bool DI_IsFissileExcepted_ReadOnly => !IsFissileExceptedRequired();

		#endregion

		protected override bool DI_IsExclusiveUse_ReadOnly => !IsAbleToEditRadioactiveColumn();

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return (!IsInDatabase || !IsClass7RadioactiveSubstance_WithoutSecurityRights()) && base.CanDelete;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return IsClass7RadioactiveSubstance_WithoutSecurityRights()
					? ResString.GetMultilingualString(
						"7f2aa53a-7f13-4878-8b50-65a4569fb90c",
						"You do not have the security rights to add or remove Class 7 (Radioactive) materials on a Shipment."
					)
					: base.ReasonForNotAbleToDelete;
			}
		}

		bool IsClass7RadioactiveSubstance_WithoutSecurityRights() =>
			!Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed &&
			Validation.IsClass7RadioactiveSubstance();

		#endregion

		#region N.O.S

		protected override bool DI_IsNotOtherwiseSpecified_ReadOnly => Substance == null || Substance.DG_Standard != UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA || !Substance.DG_IsNotOtherwiseSpecified;

		#endregion

		public override bool IsPSAGroupApplicable => (ParentPackLine?.Shipment?.IsLoadingIn(Core.Constants.CountryCodes.Singapore) ?? false)
			|| (ParentPackLine?.Shipment?.IsDischargingIn(Core.Constants.CountryCodes.Singapore) ?? false);

		public void SetParentOnAutoAddedItem(ForwardingPackLine packLine)
		{
			newAddedParentPackLine = packLine;
		}

		public ForwardingPackLine ParentPackLine => IsAutoAddedItem ? newAddedParentPackLine : ExistingUndgParentPackline;  //DI_ParentID will not be set when the data item is created by UNDGDataItemStandAloneCollection.

		ForwardingPackLine ExistingUndgParentPackline => Factory.Load<ForwardingPackLine>(DI_ParentID);
		ForwardingPackLine newAddedParentPackLine;

		public IHVLVItem ParentHVLVItem
		{
			get
			{
				if (IsHVLV)
				{
					return Factory.Load<IHVLVItem>(DI_ParentID);
				}

				return null;
			}
		}

		public ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					if (IsHVLV && ParentHVLVItem != null)
					{
						var query = new ZQuery(JobShipmentSchema.PK, ParentHVLVItem.HVI_JS_LoadedOnShipment);
						shipment = Factory.LoadTop1<ForwardingShipment>(query);
					}
					else
					{
						shipment = ParentPackLine?.Shipment;
					}
				}

				return shipment;
			}
		}

		ForwardingShipment shipment;

		public ForwardingShipment ParentPackLineShipment => ExistingUndgParentPackline?.Shipment ?? newAddedParentPackLine?.Shipment;

		public new ForwardingUNDGDataItemValidation Validation
		{
			get
			{
				return (ForwardingUNDGDataItemValidation)base.Validation;
			}
		}

		protected override UNDGDataItemValidation GetNewValidation()
		{
			return ForwardingUNDGDataItemValidation.New(this);
		}

		protected override UNDGDataItemLookups GetNewLookups()
		{
			return new ForwardingUNDGDataItemLookups(this);
		}

		public override ZString TryGetPSNWithAdditionalTextIfSupportForNOS()
		{
			return this.GetPSNWithAdditionalTextIfSupportForNOS();
		}

		#region Check Forbidden For Aircraft

		public override bool IsForbiddenForPassengerAircraft()
		{
			return base.IsForbiddenForPassengerAircraft()
				&& !(Substance.SpecialProvisionDescriptors.Contains(UNDGSubstanceLookups.SpecialProvisionType.Code.PassengerAndCargo)
						&& (Shipment?.ContainsDocType(Core.Constants.RefDocTypes.CompetentAuthorityApproval) ?? false));
		}

		public override bool IsForbiddenForCargoAircraft()
		{
			return base.IsForbiddenForCargoAircraft()
				&& !((Substance.SpecialProvisionDescriptors.Contains(UNDGSubstanceLookups.SpecialProvisionType.Code.CargoOnly)
						|| Substance.SpecialProvisionDescriptors.Contains(UNDGSubstanceLookups.SpecialProvisionType.Code.PassengerAndCargo))
						&& (Shipment?.ContainsDocType(Core.Constants.RefDocTypes.CompetentAuthorityApproval) ?? false));
		}

		#endregion
	}
}
