using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class ConsolidatedDeclarationLayoutBuilder<TConsolidatedDeclaration> : ColumnLayoutBuilder<TConsolidatedDeclaration, ConsolidatedDeclarationControlBag> where TConsolidatedDeclaration : ConsolidatedDeclaration
	{
		public override ConsolidatedDeclarationControlBag CommonBag => commonBag;
		readonly ConsolidatedDeclarationControlBag commonBag = ConsolidatedDeclarationControlBag.Instance;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 3;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.VesselCodeFindBox, consolidateDeclaration => consolidateDeclaration.LeadDeclaration.TransportMode == TransportTypeGenericList.Codes.Sea, _ => null);
		}
	}
}
