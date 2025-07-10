using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZGridChargeCodesFindBoxTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestMultiSelectGuids()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob";

			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummyChild2 = dummy2.Collection.AddNew();
			dummyChild2.Z0_Code = "Bob2";

			Factory.Save();

			using (var findBox = new ZGridChargeCodesFindBox())
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				findBox.CodeBox.Text = string.Empty;

				var popupDecisionProvider = new PopupModuleDecisionProviderWithMultipleSelect(findBox);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild1, dummyChild2 });
				AssertEquals("Bob, Bob2", findBox.Text);
			}
		}

		public void TestGetBizObjsToEditOrView()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob";

			var dummyChild2 = dummy1.Collection.AddNew();
			dummyChild2.Z0_Code = "Bob2";

			using (var findBox = new ZGridChargeCodesFindBoxForTest())
			{
				findBox.List = dummy1.Collection;
				findBox.CodeBox.Text = "Bob, Bob2";
				AssertContainsExactElementsInAnyOrder(new[] { dummyChild1, dummyChild2 }, findBox.GetBizObjsToEditOrView_Exposed());
			}
		}

		class ZGridChargeCodesFindBoxForTest : ZGridChargeCodesFindBox
		{
			public IEnumerable<BusinessObject> GetBizObjsToEditOrView_Exposed()
			{
				return GetBizObjsToEditOrView();
			}
		}
	}
}
