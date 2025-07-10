using System.Collections.Generic;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(ShoppingCartInventoryModule))]
	sealed class ShoppingCartInventoryModuleTest : TrackingInventoryModuleTest
	{
		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				ColumnDetailsForTest[] baseDetails = base.ExpectedColumnDetails;
				return new List<ColumnDetailsForTest>(baseDetails)
						   {
							new ColumnDetailsForTest("Details", baseDetails.Length, typeof(ZNewRowColumn))
						   }.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				ShoppingCartInventoryModule module = FilterGridModule as ShoppingCartInventoryModule;
				var result = new List<DataGridColumn>(base.ExpectedRequiredGridColumns);
				result.Add(module.AllColumns["Details"]);
				return result.ToArray();
			}
		}

		#region Overrides

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.ShoppingCartInventory; }
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new ShoppingCartInventoryModuleForTest(TestPage);
		}

		protected override FilterStripBusinessObject GetFilterStripBusinessObject()
		{
			return ((ShoppingCartInventoryModuleForTest)TestZWebModule).GetNewFilterStripBizOForTest();
		}

		#endregion

		class ShoppingCartInventoryModuleForTest : ShoppingCartInventoryModule
		{
			public ShoppingCartInventoryModuleForTest(ZPage page) : base(new BusinessObjectFactory(), page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}
	}
}
