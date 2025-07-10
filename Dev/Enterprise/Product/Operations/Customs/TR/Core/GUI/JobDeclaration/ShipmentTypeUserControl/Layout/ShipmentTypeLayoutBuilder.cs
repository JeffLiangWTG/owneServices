using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.GUI
{
	public class ShipmentTypeLayoutBuilder<T> : Customs.GUI.ShipmentTypeLayoutBuilder<T> where T : JobDeclaration
	{
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, h => !h.IsImport, h => h.JE_MessageTypeInfo);
		}
	}
}
