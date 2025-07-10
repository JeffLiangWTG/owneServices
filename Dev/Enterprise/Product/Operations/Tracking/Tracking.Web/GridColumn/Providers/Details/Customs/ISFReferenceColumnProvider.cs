using Enterprise.Customs.US.ISF.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ISFReferenceColumnProvider : GridColumnProvider
	{
		public ISFReferenceColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("799256f4-1920-49e9-bbd2-58671b14191e", "Description"), CusISFBill.Schema.BB_BillTypeDescription) { ColumnKey = WebTracker.Grids.ISFReference.Description });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6f6a7eb2-b3f9-4c52-8225-2f3ad8c3caee", "Bill Number"), CusISFBill.Schema.BB_BillNum) { ColumnKey = WebTracker.Grids.ISFReference.BillNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("fcba8f96-92ed-4da0-a770-3a40bc66fc52", "Bill Status"), CusISFBill.Schema.BB_CustomsStatusDescription) { ColumnKey = WebTracker.Grids.ISFReference.BillStatus });
		}
	}
}
