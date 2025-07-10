using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class RelatedDeclarationForExportValidation : Customs.Business.CusSupportingInfoValidation
	{
		public RelatedDeclarationForExportValidation(RelatedDeclarationForExport parent)
			: base(parent)
		{
		}
		public new RelatedDeclarationForExport Parent => (RelatedDeclarationForExport)base.Parent;
		protected override void CheckCSI_Procedure()
		{
			base.CheckCSI_Procedure();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_ProcedureInfo, ResString.GetMultilingualString("9BA44DDC-18CC-4B16-94E8-A25411E1DFEA", "valid type"));
		}
		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo, ResString.GetMultilingualString("A169E6F2-F678-4F99-9A3D-2B7BFCED4C0B", "valid Declaration No"));
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo, ResString.GetMultilingualString("4CB55F7C-80EF-45DE-8134-80B21D2F3307", "Enter a valid choice"));
		}
		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			var parent = Parent;
			if (parent.CSI_Quantity.IsEmpty || parent.CSI_Quantity > 100000000000)
			{
				parent.CSI_QuantityInfo.AddMessageError(ResString.GetMultilingualString("28C69C34-CDE3-457C-B613-77E54BDFFF8A", "Enter a valid Box Quantity."));
			}
			else if (parent.CSI_Quantity < 0)
			{
				parent.CSI_QuantityInfo.AddError(NegativeAmountNotAllowed);
			}
		}

		internal string NegativeAmountNotAllowed
		{
			get { return ResString.GetMultilingualString("0CAF5D12-2A72-44F5-9F0A-551AED960636", "Please enter a non-negative value."); }
		}
		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();
			var parent = Parent;
			if (parent.CSI_Quantity2.IsEmpty || parent.CSI_Quantity2 > 100000000000.000)
			{
				parent.CSI_Quantity2Info.AddMessageError(ResString.GetMultilingualString("228D0225-893E-4EF1-8323-125156E0002F", "Enter a valid Gross Weight."));
			}
			else if (parent.CSI_Quantity2 < 0)
			{
				parent.CSI_Quantity2Info.AddError(NegativeAmountNotAllowed);
			}
		}
	}
}
