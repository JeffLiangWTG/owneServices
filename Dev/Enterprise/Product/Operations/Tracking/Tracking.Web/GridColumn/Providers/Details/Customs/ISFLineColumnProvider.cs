using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public class ISFLineColumnProvider : GridColumnProvider
	{
		public ISFLineColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZCodeFindBoxColumn(Res.GetString("ba5e2935-7bd8-4ffd-8aa4-1c3a1ab8c0bc", "Origin"), CusISFLine.Schema.BL_RN_NKGoodsOrigin)
			{
				ColumnKey = WebTracker.Grids.ISFLine.Origin,
				ValueFieldName = RefCountry.Schema.RN_Code,
				BindToList = "Lookups.GoodsOrigins",
				ModuleID = WebModuleIDs.RefCountry
			});

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("735119a6-a97f-4c7c-859a-322306c95c06", "Tariff"), CusISFLine.Schema.BL_HarmonisedNum)
			{
				ColumnKey = WebTracker.Grids.ISFLine.Tariff
			});

			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("53037541-59bc-42a1-bb97-049e597af343", "Manufacturer"), CusISFLine.Schema.BL_ManufacturerDocAddressPK)
			{
				ColumnKey = WebTracker.Grids.ISFLine.Manufacturer,
				ValueFieldName = "PK",
				TextFieldName = JobDocAddress.Schema.E2_CompanyName,
				AutoPostBack = true
			});

			AddToDictionaryAsDefault(new ZCodeFindBoxColumn(Res.GetString("c2c0276d-4b76-49a3-b608-4716603dec05", "Product"), CusISFLine.Schema.BL_TextProductCode)
			{
				ColumnKey = WebTracker.Grids.ISFLine.Product,
				ModuleID = WebModuleIDs.OrgSupplierPartTracking,
				AutoPostBack = true
			});
		}
	}
}
