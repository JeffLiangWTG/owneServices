using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CommonOrganisationsLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonOrganisationsControlBag> where T : Business.BaseJobDeclaration
	{
		public override CommonOrganisationsControlBag CommonBag { get; } = CommonOrganisationsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.BondedWarehouseDocAddressControl, jobDec => jobDec.BondedWarehouseEditable, jobDec => jobDec.JE_MessageTypeInfo);
		}
	}
}
