using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CommonMiscOptionsLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonMiscOptionsControlBag> where T : BaseJobDeclaration
	{
		public override CommonMiscOptionsControlBag CommonBag { get; } = CommonMiscOptionsControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.PaidByDropEdit, x => x.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Interfaced);
		}
	}
}
