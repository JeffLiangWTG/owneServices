using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class BizObjectPopupFinderTest : TestCaseWithFactory
	{
		#region Tests

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestShowShipmentPopup()
		{
			ZFilterGridModule someModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment);
			ModuleShipmentCollection collectionAsListProvider = new ModuleShipmentCollection(Factory);
			BizObjectPopupFinder finder = new BizObjectPopupFinder(someModule, collectionAsListProvider);

			AssertIsPopupOpened(false);

			using (var someForm = new Form())
			{
				finder.ShowModal(someForm);

				AssertNotNull("ShowModal should have created popup form", finder.PopupForm);
				AssertEquals("Expected to override decission provider", finder.DecisionProvider, someModule.ModuleDecisionProvider);
				AssertIsPopupOpened(true);
			}
		}

		#endregion

		#region Implementation

		void AssertIsPopupOpened(bool expectedIsOpened)
		{
			bool isPopupOpened = false;

			foreach (Form frm in Application.OpenForms)
			{
				if (frm.GetType() == typeof(BizObjectPopupFinder.ModulePopup))
				{
					isPopupOpened = true;
					break;
				}
			}

			Assert(String.Format("Expeced popup form to be {0}", expectedIsOpened ? "opened" : "closed"), isPopupOpened == expectedIsOpened);
		}

		#endregion
	}
}
