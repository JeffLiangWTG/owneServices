using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class AllocationQuantityPerFPI : CusCodeData, Integration.Customs.US.IAllocationQuantityPerFPI
	{
		public AllocationQuantityPerFPI(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				LoadFromCY_Data();
			}
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string ManufacturerOrgPK = "ManufacturerOrgPK";
			public const string US_OA_ManufacturerAddress = "US_OA_ManufacturerAddress";
			public const string US_ForeignProducerIdentifier = "US_ForeignProducerIdentifier";
			public const string US_AllocationQuantity = "US_AllocationQuantity";
			public const int US_ForeignProducerIdentifierMaxLength = 14;
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.AllocationQuantityPerFPI;
		}

		public new AllocationQuantityPerFPIValidation Validation => (AllocationQuantityPerFPIValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new AllocationQuantityPerFPIValidation(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(OrgCountryData)); }
		}

		public new AllocationQuantityPerFPILookups Lookups => (AllocationQuantityPerFPILookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new AllocationQuantityPerFPILookups(this);
		}

		#endregion

		#region New Properties

		#region US_OA_ManufacturerAddress

		[ResourceStringData("Enterprise.Customs.US.Business.AllocationQuantityPerFPI|US_OA_ManufacturerAddress", Caption = "Address")]
		[List(nameof(US_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_ManufacturerAddress
		{
			get => fUS_OA_ManufacturerAddress;
			set
			{
				if (SetNonPersistentPropertyValue(US_OA_ManufacturerAddressInfo, ref fUS_OA_ManufacturerAddress, value))
				{
					UpdateCY_Data();

					if (Lookups.ForeignProducerIdentifiers.Count == 1)
					{
						US_ForeignProducerIdentifier = Lookups.ForeignProducerIdentifiers[0].Code;
					}
					else
					{
						US_ForeignProducerIdentifier = ZString.Empty;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_OA_ManufacturerAddress();
				}
			}
		}
		ZGuid fUS_OA_ManufacturerAddress;

		public ZPropertyInfo US_OA_ManufacturerAddressInfo => GetZPropertyInfo(Schema.US_OA_ManufacturerAddress);

		public OrgAddress ManufacturerAddress => Factory.Load<OrgAddress>(US_OA_ManufacturerAddress);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ManufacturerAddress_ZAddress
		{
			get
			{
				if (manufacturerAddress_ZAddress == null)
				{
					manufacturerAddress_ZAddress = GetNewUS_ManufacturerAddress_ZAddress();
					manufacturerAddress_ZAddress.IsOrgVisible = true;
					manufacturerAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
				}
				return manufacturerAddress_ZAddress;
			}
		}
		ZAddress manufacturerAddress_ZAddress;

		protected ZAddress GetNewUS_ManufacturerAddress_ZAddress()
		{
			return new ZAddress(US_OA_ManufacturerAddressInfo);
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AllocationQuantityPerFPI|ManufacturerOrgPK", Caption = "Manufacturer")]
		[List(nameof(Lookups) + "." + nameof(AllocationQuantityPerFPILookups.Organizations))]
		public ZGuid ManufacturerOrgPK
		{
			get { return US_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { US_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ManufacturerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerOrgPK, x => US_OA_ManufacturerAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_ForeignProducerIdentifier

		[ResourceStringData("Enterprise.Customs.US.Business.AllocationQuantityPerFPI|US_ForeignProducerIdentifier", ShortCaption = "FPI", Caption = "Foreign Producer Identifier")]
		[MaxLength(Schema.US_ForeignProducerIdentifierMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AllocationQuantityPerFPILookups.ForeignProducerIdentifiers))]
		public ZString US_ForeignProducerIdentifier
		{
			get => CY_Code;
			set
			{
				var newValue = value.ToUpper();
				if (US_ForeignProducerIdentifier != newValue)
				{
					CheckMaximumLength(US_ForeignProducerIdentifierInfo, newValue);
					CY_Code = newValue;
					US_ForeignProducerIdentifierInfo.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCY_Code();
				}
			}
		}

		public ZPropertyInfo US_ForeignProducerIdentifierInfo => GetWrappedZPropertyInfo(Schema.US_ForeignProducerIdentifier, x => CY_CodeInfo);

		#endregion

		#region US_AllocationQuantity

		[ResourceStringData("Enterprise.Customs.US.Business.AllocationQuantityPerFPI|US_AllocationQuantity", Caption = "Allocation Quantity")]
		[DecimalPrecision(12)]
		[DecimalPlaces(4)]
		public ZDecimal US_AllocationQuantity
		{
			get => fUS_AllocationQuantity;
			set
			{
				if (SetNonPersistentPropertyValue(US_AllocationQuantityInfo, ref fUS_AllocationQuantity, value))
				{
					UpdateCY_Data();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_AllocationQuantity();
				}
			}
		}
		ZDecimal fUS_AllocationQuantity;

		public ZPropertyInfo US_AllocationQuantityInfo => GetZPropertyInfo(Schema.US_AllocationQuantity);

		#endregion

		#region US_ManufacturerCompanyName

		[ResourceStringData("Enterprise.Customs.US.Business.AllocationQuantityPerFPI|US_ManufacturerCompanyName", Caption = "Company Name")]
		public ZString US_ManufacturerCompanyName => ManufacturerAddress?.CompanyName ?? ZString.Empty;

		#endregion

		#endregion

		#region New Methods

		void LoadFromCY_Data()
		{
			if (!updatingCY_DataInProgress)
			{
				try
				{
					loadingFromCY_DataInProgress = true;
					var manufacturerAddressPKString = ZString.Empty;
					var allocationQuantityString = ZString.Empty;
					var elements = CY_Data.Split(seperator);
					if (elements.Length == 2)
					{
						manufacturerAddressPKString = elements[0];
						allocationQuantityString = elements[1];
					}

					ZGuid.TryParse(manufacturerAddressPKString, out fUS_OA_ManufacturerAddress);
					ZDecimal.TryParse(allocationQuantityString, out fUS_AllocationQuantity);
				}
				finally
				{
					loadingFromCY_DataInProgress = false;
				}
			}
		}

		void UpdateCY_Data()
		{
			if (!loadingFromCY_DataInProgress)
			{
				try
				{
					updatingCY_DataInProgress = true;
					CY_Data = US_OA_ManufacturerAddress.ToString() + seperator + US_AllocationQuantity.ToString();
				}
				finally
				{
					updatingCY_DataInProgress = false;
				}
			}
		}

		ZBool loadingFromCY_DataInProgress;
		ZBool updatingCY_DataInProgress;
		readonly char seperator = '*';

		#endregion
	}
}
