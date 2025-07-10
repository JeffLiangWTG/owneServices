using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Module.Testing
{
	public class CreateConsolFromLoadListModuleDecisionProviderTest : TestCaseWithFactory
	{
		public void TestValidateSelection()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var originDepot1 = Factory.NewWithValidTestData<OrgAddress>();
			var originDepot2 = Factory.NewWithValidTestData<OrgAddress>();
			var destinationDepot1 = Factory.NewWithValidTestData<OrgAddress>();
			var destinationDepot2 = Factory.NewWithValidTestData<OrgAddress>();

			var loadList1 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier1, originDepot1, destinationDepot1);
			var loadList2 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier1, originDepot1, destinationDepot1);
			var loadList3 = CreateLoadList("AIR", "VESSEL1", "VOYAGE1", "BILL1", carrier1, originDepot1, destinationDepot1);
			var loadList4 = CreateLoadList("SEA", "VESSEL2", "VOYAGE1", "BILL1", carrier1, originDepot1, destinationDepot1);
			var loadList5 = CreateLoadList("SEA", "VESSEL1", "VOYAGE2", "BILL1", carrier1, originDepot1, destinationDepot1);
			var loadList6 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL2", carrier1, originDepot1, destinationDepot1);
			var loadList7 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier2, originDepot1, destinationDepot1);
			var loadList8 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier1, originDepot2, destinationDepot1);
			var loadList9 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier1, originDepot1, destinationDepot2);

			Factory.Save();

			AssertLoadListSelectionIsValid(new[] { loadList1, loadList2 }, true);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList3 });
			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList4 });
			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList5 });
			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList6 });
			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList7 });
			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList8 });
			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList9 });
		}

		public void TestValidateSelection_DateRange()
		{
			var now = ZDateTime.Now;
			var within24Hours = now.AddHours(23);
			var notWithin24Hours = now.AddHours(25);

			var loadList1 = CreateLoadList(now, now);
			var loadList2 = CreateLoadList(within24Hours, now);
			var loadList3 = CreateLoadList(now, within24Hours);
			var loadList4 = CreateLoadList(notWithin24Hours, now);
			var loadList5 = CreateLoadList(now, notWithin24Hours);

			Factory.Save();

			AssertLoadListSelectionIsValid(new[] { loadList1, loadList2 }, true);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertLoadListSelectionIsValid(new[] { loadList1, loadList3 }, true);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList4 });
			AssertUserPromptedForConfirmation(mismatchingDetailsMessage, new[] { loadList1, loadList5 });
		}

		public void TestValidateSelection_Items()
		{
			const string expectedError = @"The following eLoadLists do not have any Items attached:
TL00001003, TL00001004";

			var loadList1 = CreateLoadList("TL00001001", true);
			var loadList2 = CreateLoadList("TL00001002", true);
			var loadList3 = CreateLoadList("TL00001003", false);
			var loadList4 = CreateLoadList("TL00001004", false);

			Factory.Save();

			AssertLoadListSelectionIsValid(new[] { loadList1, loadList2 }, true);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			AssertUserDeniedSelection(expectedError, new[] { loadList1, loadList2, loadList3, loadList4 });
		}

		#region Implementation

		protected HVLVOriginLoadList CreateLoadList(string transportMode, string vessel, string voyage, string billNumber, OrgHeader carrier, OrgAddress originDepot, OrgAddress destinationDepot)
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = Core.Constants.ELoadListStatuses.Lodged;
			loadList.HVL_TransportMode = transportMode;
			loadList.HVL_VesselName = vessel;
			loadList.HVL_VoyageFlight = voyage;
			loadList.HVL_MasterBillNumber = billNumber;
			loadList.HVL_OH_Carrier = carrier.PK;
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;

			Factory.NewWithValidTestData<HVLVItem>().HVI_HVL_LoadList = loadList.PK;

			return loadList;
		}

		HVLVOriginLoadList CreateLoadList(ZDateTime etd, ZDateTime eta)
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = Core.Constants.ELoadListStatuses.Lodged;
			loadList.HVL_E_Dep = etd;
			loadList.HVL_E_Arv = eta;

			Factory.NewWithValidTestData<HVLVItem>().HVI_HVL_LoadList = loadList.PK;

			return loadList;
		}

		HVLVOriginLoadList CreateLoadList(string loadListNumber, bool attachItem)
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_UniqueReference = loadListNumber;
			loadList.HVL_Status = Core.Constants.ELoadListStatuses.Lodged;

			if (attachItem)
			{
				Factory.NewWithValidTestData<HVLVItem>().HVI_HVL_LoadList = loadList.PK;
			}

			return loadList;
		}

		const string mismatchingDetailsMessage = "Selected eLoadLists do not have compatible Voyage/Flight, Master Bill, Carrier, Origin & Destination, and ETD & ETA Details. Are you sure you want to continue (the Consol will be created using one of the selected eLoadLists details)?";

		void AssertUserPromptedForConfirmation(string expectedMessage, HVLVOriginLoadList[] loadLists)
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			AssertLoadListSelectionIsValid(loadLists, false);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertLoadListSelectionIsValid(loadLists, false);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void AssertUserDeniedSelection(string expectedMessage, HVLVOriginLoadList[] loadLists)
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertLoadListSelectionIsValid(loadLists, false);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void AssertLoadListSelectionIsValid(HVLVOriginLoadList[] loadLists, bool isValid)
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.HVLVOriginLoadList))
			using (var popupForm = new EmbeddedModulePopup(module))
			{
				var findBox = new DummyFindBox(popupForm);
				var decisionProvider = new CreateConsolFromLoadListModuleDecisionProvider(Factory, findBox);
				decisionProvider.HandleFindBoxOKButton(loadLists);
				AssertEquals(isValid, findBox.PopupFormHandledSelection);
			}
		}

		protected class DummyFindBox : IFindBox
		{
			public DummyFindBox(EmbeddedModulePopup popupForm)
			{
				PopupForm = popupForm;
				PopupForm.Selected += (s, e) => PopupFormHandledSelection = true;
			}

			EmbeddedModulePopup PopupForm { get; }
			public bool PopupFormHandledSelection { get; private set; }

			IFindBoxPopup IFindBox.PopupForm => PopupForm;

			string IFindBox.Code
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			string IFindBox.Description
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			IFindBoxListProvider IFindBox.ListProvider
			{
				get { throw new NotImplementedException(); }
			}
		}

		#endregion
	}
}
