using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class MovementHeaderWrapper : NonPersistentBusinessObject<MovementHeaderWrapperValidation>
	{
		public MovementHeaderWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema
		public static class Schema
		{
			public const string EntryType = "EntryType";
			public const string CarrierOrgPK = "CarrierOrgPK";
			public const string InBondCarrierAddress = "InBondCarrierAddress";
			public const string Destination = "Destination";
			public const int EntryTypeMaxLength = 2;
			public const int DestinationMaxLength = 4;
		}

		#endregion

		#region In-Bond Number

		[ReadOnly(true)]
		[ResourceStringData("29FB18A9-249F-4FF1-A905-009B839CBFDD", Caption = "In-Bond Number")]
		public ZString InBondNumber
		{
			get => inBondNumber;
			set => SetNonPersistentPropertyValue(InBondNumberInfo, ref inBondNumber, value);
		}
		ZString inBondNumber;

		public ZPropertyInfo InBondNumberInfo => GetZPropertyInfo(nameof(InBondNumber));

		#endregion

		#region Entry Type

		[MaxLength(Schema.EntryTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(MovementHeaderWrapperLookups.EntryTypeList))]
		[ResourceStringData("09C46F52-A001-4540-9D92-522CD4A92DF0", Caption = "Entry Type")]
		public ZString EntryType
		{
			get => entryType;
			set
			{
				SetNonPersistentPropertyValue(EntryTypeInfo, ref entryType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryType();
				}
			}
		}
		ZString entryType;

		public ZPropertyInfo EntryTypeInfo => GetZPropertyInfo(Schema.EntryType);

		#endregion

		#region In-Bond Carrier

		[RelatedBusinessObject(nameof(Carrier))]
		[List(nameof(Lookups) + "." + nameof(MovementHeaderWrapperLookups.Carriers))]
		[ResourceStringData("8D425308-A28F-4FA4-92D9-B7BE47DE4D05", Caption = "In-Bond Carrier")]
		public ZGuid CarrierOrgPK
		{
			get { return InBondCarrierAddress_ZAddress.OrgPK; }
			set
			{
				InBondCarrierAddress_ZAddress.OrgPK = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCarrierOrgPK();
				}
				CarrierOrgPKInfo.RefreshBinding();
			}
		}

		public OrgHeader Carrier => Factory.Load<OrgHeader>(CarrierOrgPK);

		public ZPropertyInfo CarrierOrgPKInfo => GetZPropertyInfo(Schema.CarrierOrgPK);

		[RelatedBusinessObject(nameof(CarrierAddress))]
		[List(nameof(Lookups) + "." + nameof(MovementHeaderWrapperLookups.Carriers))]
		[ResourceStringData("CD9687CA-E3B0-4E7E-A033-B61947BB5D5C", Caption = "In-Bond Carrier Address")]
		public ZGuid InBondCarrierAddress
		{
			get => inBondCarrierAddress;
			set
			{
				SetNonPersistentPropertyValue(InBondCarrierAddressInfo, ref inBondCarrierAddress, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateInBondCarrierAddress();
				}
			}
		}
		ZGuid inBondCarrierAddress;

		public ZPropertyInfo InBondCarrierAddressInfo => GetZPropertyInfo(Schema.InBondCarrierAddress);

		public OrgAddress CarrierAddress
		{
			get { return Factory.Load<OrgAddress>(InBondCarrierAddress); }
		}

		#region ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress InBondCarrierAddress_ZAddress
		{
			get
			{
				if (inBondCarrierAddress_ZAddress == null)
				{
					inBondCarrierAddress_ZAddress = new ZAddress(InBondCarrierAddressInfo);
					inBondCarrierAddress_ZAddress.IsOrgVisible = true;
					inBondCarrierAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return inBondCarrierAddress_ZAddress;
			}
		}
		ZAddress inBondCarrierAddress_ZAddress;

		#endregion

		#endregion

		#region US Destination

		[MaxLength(Schema.DestinationMaxLength)]
		[List(nameof(Lookups) + "." + nameof(MovementHeaderWrapperLookups.RegionDistrictPorts))]
		[ResourceStringData("7BFC904F-CD04-4CC5-8A95-4064BD9AA7E7", Caption = "US Destination")]
		public ZString Destination
		{
			get => destination;
			set
			{
				SetNonPersistentPropertyValue(DestinationInfo, ref destination, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDestination();
				}
			}
		}
		ZString destination;

		public ZPropertyInfo DestinationInfo => GetZPropertyInfo(Schema.Destination);

		#endregion

		#region AllocatedShipmentsForMovement

		public CusInBondShipmentWrapperCollection AllocatedShipmentsForMovement => allocatedShipmentsForMovement ?? (allocatedShipmentsForMovement = new CusInBondShipmentWrapperCollection(null));
		CusInBondShipmentWrapperCollection allocatedShipmentsForMovement;

		#endregion

		#region Validation

		public override MovementHeaderWrapperValidation GetNewValidation()
		{
			return new MovementHeaderWrapperValidation(this);
		}

		#endregion

		#region Lookups

		public MovementHeaderWrapperLookups Lookups
		{
			get
			{
				return new MovementHeaderWrapperLookups(this);
			}
		}

		#endregion
	}
}
