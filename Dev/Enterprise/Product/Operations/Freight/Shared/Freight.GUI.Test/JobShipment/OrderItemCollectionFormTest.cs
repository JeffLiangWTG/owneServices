using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(OrderItemCollectionForm))]
	sealed class OrderItemCollectionFormTest : ZFormBasherTest
	{
		public void TestValidate()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			OrderItemCollection collection = new OrderItemCollection(cartage, Factory);
			OrderItem item = collection.AddNew();
			using (TestOrderItemCollectionForm form = new TestOrderItemCollectionForm(collection))
			{
				CancelEventArgs e = new CancelEventArgs(false);
				item.JT_OrderReference = "xxx";
				form.OnClosing(e);
				AssertEquals("No validation errors, should not be cancelled", false, e.Cancel);

				e = new CancelEventArgs(false);
				item.JT_OrderReference = "x ,&";
				form.OnClosing(e);
				AssertEquals("Validation errors exists, should be cancelled", true, e.Cancel);
			}
		}

		public void TestCancel()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			OrderItemCollection collection = new OrderItemCollection(cartage, Factory);
			using (OrderItemCollectionForm form = new OrderItemCollectionForm(collection))
			{
				OrderItem initialItem = form.OrderItems.AddNew();
				initialItem.JT_OrderReference = "xxx";
				form.Show();
				OrderItem newItem = form.OrderItems.AddNew();
				newItem.JT_OrderReference = "yyy";
				AssertEquals("Should have 2 while editing", 2, form.OrderItems.Count);

				form.DialogResult = DialogResult.Cancel;
				form.Close();
				AssertEquals("Should have 1 again after cancel", 1, form.OrderItems.Count);
			}
		}

		public void TestConcurrencyErrorOnOrderCollection()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(factory1));
			OrderItemCollection collection = cartage.OrderItems;

			OrderItem firstItem = collection.AddNew();
			firstItem.JT_OrderReference = "AAA";
			firstItem.JT_Sequence = 1;
			firstItem.JT_JP = cartage.PK;

			OrderItem secondItem = collection.AddNew();
			secondItem.JT_OrderReference = "BBB";
			secondItem.JT_Sequence = 2;
			secondItem.JT_JP = cartage.PK;

			OrderItem thirdItem = collection.AddNew();
			thirdItem.JT_OrderReference = "CCC";
			thirdItem.JT_Sequence = 3;
			thirdItem.JT_JP = cartage.PK;

			factory1.Save();

			AssertEquals("prerequisite", 1, (int)firstItem.JT_Sequence);
			AssertEquals("prerequisite", 2, (int)secondItem.JT_Sequence);
			AssertEquals("prerequisite", 3, (int)thirdItem.JT_Sequence);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var cartageOnOtherFactory = factory2.Load<JobDocsAndCartage>(cartage.PK);
			AssertEquals("cartage from factory1 will be loaded", cartage.PK, cartageOnOtherFactory.PK);

			using (OrderItemCollectionForm form = new OrderItemCollectionForm(cartageOnOtherFactory.OrderItems))
			{
				form.Show();
				var grid = (ZGrid)form.Controls.Find("OrderItemsGrid", true)[0];

				var propertyDescriptor = grid.TableStyles[0].GridColumnStyles[grid.Columns[0].ColumnName].PropertyDescriptor;

				var columnHeaderClicked = typeof(DataGrid).GetMethod("ColumnHeaderClicked",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic);

				columnHeaderClicked.Invoke(grid, new object[] { propertyDescriptor });
				columnHeaderClicked.Invoke(grid, new object[] { propertyDescriptor });

				factory2.Save();
			}

			thirdItem.Delete();
			AssertNoExceptionThrown("Concurrency exception is not thrown", factory1.Save);
		}

		protected override Form GetFormToBashCore()
		{
			JobDocsAndCartage cartage = JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
			return new OrderItemCollectionForm(cartage.OrderItems);
		}

		internal class TestOrderItemCollectionForm : OrderItemCollectionForm
		{
			public TestOrderItemCollectionForm(OrderItemCollection collection)
				: base(collection)
			{
			}

			public new void OnClosing(CancelEventArgs e)
			{
				base.OnClosing(e);
			}
		}
	}
}
