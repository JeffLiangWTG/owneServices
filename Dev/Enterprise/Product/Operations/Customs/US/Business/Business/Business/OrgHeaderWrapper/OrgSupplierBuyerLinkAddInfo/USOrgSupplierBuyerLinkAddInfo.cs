using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class USOrgSupplierBuyerLinkAddInfo : NonPersistentBusinessObject
	{
		public USOrgSupplierBuyerLinkAddInfo(OrgSupplierBuyerLinkAddInfo addInfoBO)
			: base(addInfoBO.Factory)
		{
			this.AddInfoBO = Argument.NotNull(addInfoBO, nameof(addInfoBO));
		}

		OrgSupplierBuyerLinkAddInfo AddInfoBO { get; }

		public ZGuid ParentPK => AddInfoBO.Parent.PK;

		[List(nameof(Lookups) + "." + nameof(USOrgSupplierBuyerLinkAddInfoLookups.YesNoList))]
		[MaxLength(1)]
		public ZString ZO_FirstSale
		{
			get { return AddInfoBO.ZO_FirstSale; }
			set
			{
				AddInfoBO.ZO_FirstSale = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_FirstSale();
				}
				ZO_FirstSaleInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_FirstSaleInfo
		{
			get { return GetZPropertyInfo(OrgSupplierBuyerLinkAddInfo.Schema.ZO_FirstSale); }
		}

		[List(nameof(Lookups) + "." + nameof(USOrgSupplierBuyerLinkAddInfoLookups.OtherReconIssueList))]
		[MaxLength(OrgSupplierBuyerLinkAddInfo.Schema.ZO_OtherReconIndicatorMaxLength)]
		public ZString ZO_OtherReconIndicator
		{
			get { return AddInfoBO.ZO_OtherReconIndicator; }
			set
			{
				AddInfoBO.ZO_OtherReconIndicator = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_OtherReconIndicator();
				}
				ZO_OtherReconIndicatorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_OtherReconIndicatorInfo
		{
			get { return GetZPropertyInfo(OrgSupplierBuyerLinkAddInfo.Schema.ZO_OtherReconIndicator); }
		}

		[List(nameof(Lookups) + "." + nameof(USOrgSupplierBuyerLinkAddInfoLookups.UltConsigneeTypeList))]
		[MaxLength(1)]
		public ZString ZO_AESUltConsigneeType
		{
			get { return AddInfoBO.ZO_AESUltConsigneeType; }
			set
			{
				AddInfoBO.ZO_AESUltConsigneeType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_AESUltConsigneeType();
				}
				ZO_AESUltConsigneeTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_AESUltConsigneeTypeInfo
		{
			get { return GetZPropertyInfo(OrgSupplierBuyerLinkAddInfo.Schema.ZO_AESUltConsigneeType); }
		}

		[List(nameof(Lookups) + "." + nameof(USOrgSupplierBuyerLinkAddInfoLookups.EntryTypes))]
		[MaxLength(OrgSupplierBuyerLinkAddInfo.Schema.ZO_EntryTypeMaxLength)]
		public ZString ZO_EntryType
		{
			get { return AddInfoBO.ZO_EntryType; }
			set
			{
				AddInfoBO.ZO_EntryType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateZO_EntryType();
				}
				ZO_EntryTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_EntryTypeInfo
		{
			get { return GetZPropertyInfo(OrgSupplierBuyerLinkAddInfo.Schema.ZO_EntryType); }
		}

		public ZBool ZO_NAFTAReconIndicator
		{
			get { return AddInfoBO.ZO_NAFTAReconIndicator; }
			set
			{
				AddInfoBO.ZO_NAFTAReconIndicator = value;
				ZO_NAFTAReconIndicatorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ZO_NAFTAReconIndicatorInfo
		{
			get { return GetZPropertyInfo(OrgSupplierBuyerLinkAddInfo.Schema.ZO_NAFTAReconIndicator); }
		}

		#region Lookups

		public USOrgSupplierBuyerLinkAddInfoLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new USOrgSupplierBuyerLinkAddInfoLookups(this);
				}

				return fLookups;
			}
		}

		USOrgSupplierBuyerLinkAddInfoLookups fLookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public USOrgSupplierBuyerLinkAddInfoValidation Validation
		{
			get { return new USOrgSupplierBuyerLinkAddInfoValidation(this); }
		}

		#endregion
	}
}
