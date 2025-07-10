using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI.Stocktake;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsStocktakeProductFilterAttacherTest : ZRecordAttacherTest
	{
		#region TestAttach

		public void TestAttach()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			using (var form = new ZForm())
			{
				form.Show();
				var attacher = new WhsStocktakeProductFilterAttacher(stocktake.ProductFilterCollection, stocktake.Lookups.SupplierParts, ModuleIDs.SupplierPart);
				attacher.Show(form);

				AssertEquals("Precondition: ProductFilterCollection is empty ", 0, stocktake.ProductFilterCollection.Count);
				// we attach product for the first time
				AssertProductCouldBeAttached(data.Part1, attacher, true);

				AssertEquals(1, stocktake.ProductFilterCollection.Count);
				AssertProductCouldBeAttached(data.Part1, attacher, false);
			}
		}

		static void AssertProductCouldBeAttached(OrgSupplierPart product, WhsStocktakeProductFilterAttacher attacher, bool canAttach)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var popup = attacher.LastShownAttachPopupForTesting)
			{
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { product });

				if (canAttach)
				{
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
				else
				{
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
					AssertContains(attacher.UnableToAttachText, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}