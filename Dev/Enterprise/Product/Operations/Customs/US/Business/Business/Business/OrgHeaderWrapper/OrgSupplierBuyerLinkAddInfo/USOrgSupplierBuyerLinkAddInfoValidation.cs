using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USOrgSupplierBuyerLinkAddInfoValidation : ZValidation
	{
		public USOrgSupplierBuyerLinkAddInfoValidation(USOrgSupplierBuyerLinkAddInfo parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected USOrgSupplierBuyerLinkAddInfo Parent { get; }

		public override Type AutoValidationType
		{
			get { return typeof(USOrgSupplierBuyerLinkAddInfoValidation); }
		}

		public override void ValidateAll()
		{
			ValidateZO_FirstSale();
			ValidateZO_OtherReconIndicator();
			ValidateZO_AESUltConsigneeType();
			ValidateZO_EntryType();
		}

		public void ValidateZO_FirstSale()
		{
			ValidateCalculatedProperty(Parent.ZO_FirstSaleInfo);
		}

		protected void CheckZO_FirstSale()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_FirstSaleInfo, Parent.Lookups.YesNoList);
		}

		public void ValidateZO_OtherReconIndicator()
		{
			ValidateCalculatedProperty(Parent.ZO_OtherReconIndicatorInfo);
		}

		protected void CheckZO_OtherReconIndicator()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_OtherReconIndicatorInfo, Parent.Lookups.OtherReconIssueList);
		}

		public void ValidateZO_AESUltConsigneeType()
		{
			ValidateCalculatedProperty(Parent.ZO_AESUltConsigneeTypeInfo);
		}

		protected void CheckZO_AESUltConsigneeType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_AESUltConsigneeTypeInfo, Parent.Lookups.UltConsigneeTypeList);
		}

		public void ValidateZO_EntryType()
		{
			ValidateCalculatedProperty(Parent.ZO_EntryTypeInfo);
		}

		protected void CheckZO_EntryType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_EntryTypeInfo, Parent.Lookups.EntryTypes);
		}
	}
}
