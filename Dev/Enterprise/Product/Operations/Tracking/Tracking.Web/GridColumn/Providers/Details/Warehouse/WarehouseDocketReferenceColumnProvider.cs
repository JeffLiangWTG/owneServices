using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class WarehouseDocketReferenceColumnProvider : GridColumnProvider
	{
		public WarehouseDocketReferenceColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("dc494a2d-66f8-4c82-89b7-22f31c835ca7", "Ref Type"), WhsDocketReferenceSchema.WX_RefType.Name, "Lookups.ReferenceTypes") { ColumnKey = WebTracker.Grids.WarehouseDocketReference.ReferenceType });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("3119fbc9-057a-49c6-b327-d37312fb446d", "Reference"), WhsDocketReferenceSchema.WX_Reference.Name) { ColumnKey = WebTracker.Grids.WarehouseDocketReference.Reference });
		}
	}
}
