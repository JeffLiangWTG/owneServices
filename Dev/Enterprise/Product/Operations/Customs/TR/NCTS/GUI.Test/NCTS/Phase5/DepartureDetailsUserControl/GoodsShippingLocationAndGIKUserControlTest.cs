using CargoWise.Windows.UI;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.Customs.TR.NCTS.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class GoodsShippingLocationAndGIKUserControlTest : TestCase
	{
		public void TestGoodsShippingLocationDropEdit()
		{
			var goodsShippingLocationDropEdit = userControl.GoodsShippingLocationDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_LocationOfGoodsCode), goodsShippingLocationDropEdit.GetBindingMember());
				AssertEquals("TabIndex", 0, goodsShippingLocationDropEdit.TabIndex);
			});
		}

		public void TestGIKEnabledCheckBox()
		{
			var isGIKEnabledCheckBox = userControl.GIKEnabledCheckBox;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.IsGIKEnabled), isGIKEnabledCheckBox.GetBindingMember());
				AssertEquals("TabIndex", 1, isGIKEnabledCheckBox.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new GoodsShippingLocationAndGIKUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		GoodsShippingLocationAndGIKUserControl userControl;
	}
}
