using System.Collections.Generic;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class OrgSupplierPartColumnProvider : GridColumnProvider
	{
		readonly bool IsUsedAsLookup;
		public OrgSupplierPartColumnProvider(bool isUsedAsLookup)
		{
			IsUsedAsLookup = isUsedAsLookup;
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			if (IsUsedAsLookup)
			{
				AddButtonColumn(Res.GetString("44940c04-2c20-4199-97df-5d6270bb4ca8", "Product#"), OrgSupplierPartSchema.OP_PartNum.Name, WebTracker.Grids.OrgSupplierParts.ProductNumber);
			}
			else
			{
				ZHyperLinkColumn productColumn = new ZHyperLinkColumn(Res.GetString("44940c04-2c20-4199-97df-5d6270bb4ca8", "Product#"), OrgSupplierPartSchema.OP_PartNum.Name)
				{
					ColumnKey = WebTracker.Grids.OrgSupplierParts.ProductNumber,
					DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.ProductProfileDetailsPage) + (NoResString)"?Ref={0}", // Partial URL
					DataNavigateUrlFields = new string[1] { "PK" }
				};
				AddToDictionaryAsRequired(productColumn);
			}
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("1d458e31-cc2a-4041-ab9f-2c6b1b79352c", "Description"), OrgSupplierPartSchema.OP_Desc.Name) { ColumnKey = WebTracker.Grids.OrgSupplierParts.Description });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.OrgSupplierParts.ProductNumber);
			result.Add((int)WebTracker.Grids.OrgSupplierParts.Description);
			return result;
		}
	}
}
