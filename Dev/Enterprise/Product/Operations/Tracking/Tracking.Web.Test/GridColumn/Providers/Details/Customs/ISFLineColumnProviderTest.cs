using System.Collections.Generic;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ISFLineColumnProvider))]
	sealed class ISFLineColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZCodeFindBoxColumn("Origin", CusISFLine.Schema.BL_RN_NKGoodsOrigin)
			{
				ColumnKey = WebTracker.Grids.ISFLine.Origin,
				ValueFieldName = RefCountry.Schema.RN_Code,
				BindToList = "Lookups.GoodsOrigins",
				ModuleID = WebModuleIDs.RefCountry
			});

			AddDefaultsColumn(new ZTextEditColumn("Tariff", CusISFLine.Schema.BL_HarmonisedNum)
			{
				ColumnKey = WebTracker.Grids.ISFLine.Tariff
			});

			AddDefaultsColumn(new ZFindBoxColumn("Manufacturer", CusISFLine.Schema.BL_ManufacturerDocAddressPK)
			{
				ColumnKey = WebTracker.Grids.ISFLine.Manufacturer,
				ValueFieldName = "PK",
				TextFieldName = JobDocAddress.Schema.E2_CompanyName,
				AutoPostBack = true
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Product", CusISFLine.Schema.BL_TextProductCode)
			{
				ColumnKey = WebTracker.Grids.ISFLine.Product,
				ModuleID = WebModuleIDs.OrgSupplierPartTracking,
				AutoPostBack = true
			});
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.ISFLine.Origin,
			WebTracker.Grids.ISFLine.Manufacturer,
			WebTracker.Grids.ISFLine.Product
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ISFLineColumnProvider();
		}
	}
}
