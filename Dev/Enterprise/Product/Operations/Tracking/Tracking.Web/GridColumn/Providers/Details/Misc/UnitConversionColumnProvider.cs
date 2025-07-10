using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class UnitConversionColumnProvider : GridColumnProvider
	{
		public UnitConversionColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("7612e327-efb1-4eaa-a5d7-6c17d1ad6c2d", "Qty in Parent"), OrgPartUnitSchema.OF_QuantityInParent.Name) { ColumnKey = WebTracker.Grids.UnitConversion.QuantityInParent });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("cf9042c4-c287-4920-9226-aaaba0ded6cf", "Package"), OrgPartUnitSchema.OF_PackType.Name) { ColumnKey = WebTracker.Grids.UnitConversion.Package });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("82c7f104-4fa5-4e3d-9022-be14fd22c7de", "Parent Package"), OrgPartUnitSchema.OF_ParentPackType.Name) { ColumnKey = WebTracker.Grids.UnitConversion.ParentPackage });
		}
	}
}
