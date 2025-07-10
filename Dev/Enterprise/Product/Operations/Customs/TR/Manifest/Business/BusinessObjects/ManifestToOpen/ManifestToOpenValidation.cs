using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class ManifestToOpenValidation : Customs.Business.CusSupportingInfoValidation
	{
		public ManifestToOpenValidation(ManifestToOpen parent)
			: base(parent)
		{
		}
		public new ManifestToOpen Parent => (ManifestToOpen)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateBillNo();
		}

		public void ValidateBillNo()
		{
			ValidateCalculatedProperty(Parent.BillNoInfo);
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
			CheckForDuplicates();
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
			CheckForDuplicates();
		}

		protected void CheckBillNo()
		{
			if (Parent.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlevel || Parent.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlinelevel)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BillNoInfo);
			}
			CheckForDuplicates();
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();
			if (Parent.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlinelevel)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_LineNoInfo);
			}
			CheckForDuplicates();
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			var parent = Parent;
			if (Parent.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlinelevel)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
			}

			if (Parent.CSI_Quantity < 0)
			{
				parent.CSI_QuantityInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		internal string NegativeAmountNotAllowed
		{
			get { return ResString.GetMultilingualString("2FF5ED53-8E58-4C7D-AB7D-FD2E2AE6B83C", "Please enter a non-negative value."); }
		}

		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();
			var parent = Parent;
			if (Parent.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlinelevel)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_Quantity2Info);
			}

			if (Parent.CSI_Quantity2 < 0)
			{
				parent.CSI_Quantity2Info.AddError(NegativeAmountNotAllowed);
			}
		}
		protected override void CheckCSI_CustomsOffice()
		{
			base.CheckCSI_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CustomsOfficeInfo);
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			if (Parent.IsFirstRecordForDescription && Parent.Procedure)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}

		void CheckForDuplicates()
		{
			(Parent.Parent as AsycudaManifestHeader).Validation.ValidateManifestToOpenDuplicates();
		}
	}
}
