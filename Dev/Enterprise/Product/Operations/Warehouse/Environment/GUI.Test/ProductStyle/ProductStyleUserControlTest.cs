using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	public class ProductStyleUserControlTest : TestCaseWithFactory
	{
		#region TestMoveSelectedSize

		public void TestMoveSelectedSize()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			var size1 = productStyle.Sizes.AddNew();
			var size2 = productStyle.Sizes.AddNew();
			var size3 = productStyle.Sizes.AddNew();
			AssertProductSizeSequence(size1, size2, size3);

			using (var form = new ZForm(productStyle))
			{
				var userControl = new ProductStyleUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var upButton = userControl.MoveUpButton;
				var downButton = userControl.MoveDownButton;
				var sizesGrid = userControl.ProductStyleSizesGrid;
				AssertEquals("Precondition - Style should have 4 sizes.", 4, sizesGrid.VisibleRowCount);

				sizesGrid.SelectSingleElement(size1);
				AssertEquals(false, upButton.Enabled);
				AssertEquals(true, downButton.Enabled);
				AssertProductSizeSequence(size1, size2, size3);

				downButton.PerformClick();
				AssertEquals(true, upButton.Enabled);
				AssertEquals(true, downButton.Enabled);
				AssertProductSizeSequence(size2, size1, size3);

				downButton.PerformClick();
				AssertEquals(true, upButton.Enabled);
				AssertEquals(false, downButton.Enabled);
				AssertProductSizeSequence(size2, size3, size1);

				upButton.PerformClick();
				AssertEquals(true, upButton.Enabled);
				AssertEquals(true, downButton.Enabled);
				AssertProductSizeSequence(size2, size1, size3);
			}
		}

		void AssertProductSizeSequence(WhsProductStyleSize firstSize, WhsProductStyleSize secondSize, WhsProductStyleSize thirdSize)
		{
			AssertEquals(1, (ZInt)firstSize.WSZ_Sequence);
			AssertEquals(2, (ZInt)secondSize.WSZ_Sequence);
			AssertEquals(3, (ZInt)thirdSize.WSZ_Sequence);
		}

		#endregion
	}
}
