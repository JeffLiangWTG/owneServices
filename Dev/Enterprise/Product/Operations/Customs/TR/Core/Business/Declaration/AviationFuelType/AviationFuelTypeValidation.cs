using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AviationFuelTypeValidation : Customs.Business.CusSupportingInfoValidation
	{
		public AviationFuelTypeValidation(AviationFuelType parent) : base(parent)
		{
		}

		protected new AviationFuelType Parent => (AviationFuelType)base.Parent;

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			if (!Parent.CSI_ReferenceNumber2.IsEmpty && (Parent.CSI_ReferenceNumber2.Length != 11 || !Parent.CSI_ReferenceNumber2.IsNumbersOnlyOrEmpty))
			{
				Parent.CSI_ReferenceNumber2Info.AddMessageError(Res.GetString("9C3D21D5-A7F1-42A8-96D3-EB865FE0191C", "Tax ID must be 11 digit long."));
			}
			else if (!Parent.CSI_ReferenceNumber.IsEmpty || !Parent.CSI_Value.IsEmpty || !Parent.CSI_DateOfIssue.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
			}
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();
			if (!Parent.CSI_ReferenceNumber.IsEmpty || !Parent.CSI_Value.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DateOfIssueInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (!Parent.CSI_Value.IsEmpty || !Parent.CSI_DateOfIssue.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_Value()
		{
			base.CheckCSI_Value();
			if (!Parent.CSI_ReferenceNumber.IsEmpty || !Parent.CSI_DateOfIssue.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ValueInfo);
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			if (!Parent.CSI_ReferenceNumber.IsEmpty || !Parent.CSI_DateOfIssue.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}
	}
}
