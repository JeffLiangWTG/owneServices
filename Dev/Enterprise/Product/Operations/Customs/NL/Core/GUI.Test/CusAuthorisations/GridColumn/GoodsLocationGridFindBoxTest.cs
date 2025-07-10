using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(GoodsLocationGridFindBox))]
class GoodsLocationGridFindBoxTest : TestCaseWithFactory
{
	public void TestCodeBox_ReadOnly()
	{
		using (var findBox = new GoodsLocationGridFindBox())
		{
			AssertEquals(true, findBox.CodeBox.ReadOnly);
		}
	}

	public void TestPopupButton_ReadOnly()
	{
		using (var findBox = new GoodsLocationGridFindBox())
		{
			AssertEquals(false, findBox.PopupButton.ReadOnly);
		}
	}

	public void TestPopupForm()
	{
		var goodsLocationProvider = Factory.New<CusAuthorisationRule>();
		using (var findBox = new GoodsLocationGridFindBox())
		{
			findBox.GoodsLocationProvider = goodsLocationProvider;
			var propertyInfo = typeof(GoodsLocationGridFindBox).GetProperty("PopupForm", BindingFlags.NonPublic | BindingFlags.Instance);
			var popupForm = propertyInfo.GetValue(findBox);
			using ((ZChildForm)popupForm)
			{
				AssertType<CusGoodsLocationForm>(popupForm);
			}
		}
	}
}
