using Enterprise.Customs.US.ISF.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ISFContainerColumnProvider : GridColumnProvider
	{
		public ISFContainerColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("b376f80f-cca1-4f4a-9573-4de96aa496dd", "Desc.Code"), CusISFEquip.Schema.BE_EquipCode) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly, ColumnKey = WebTracker.Grids.ISFContainer.DescriptionCode });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("30c1c276-f872-4df2-aeda-8f075d202e50", "Container Number"), CusISFEquip.Schema.BE_ContainerNum) { ColumnKey = WebTracker.Grids.ISFContainer.ContainerNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f443bb7a-92c0-437b-bb2d-8204d36c2181", "ISO"), CusISFEquip.Schema.BE_ContainerISO) { ColumnKey = WebTracker.Grids.ISFContainer.ISO });
		}
	}
}
