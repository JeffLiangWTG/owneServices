using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Module.Testing
{
	public class AttachLoadListToConsolModuleDecisionProviderTest : CreateConsolFromLoadListModuleDecisionProviderTest
	{
		public void TestValidateLoadListConsolSelection()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = "MBN20181220";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();

			var loadList1 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier, originDepot, destinationDepot);
			loadList1.HVL_MasterBillNumber = forwardingConsol.JK_MasterBillNum;
			var loadList2 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier, originDepot, destinationDepot);
			loadList2.HVL_MasterBillNumber = forwardingConsol.JK_MasterBillNum;

			var loadList3 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier, originDepot, destinationDepot);
			loadList3.HVL_MasterBillNumber = "MBN20000000";
			var loadList4 = CreateLoadList("SEA", "VESSEL1", "VOYAGE1", "BILL1", carrier, originDepot, destinationDepot);
			loadList3.HVL_MasterBillNumber = "MBN20000002";

			Factory.Save();

			AssertLoadListSelectionIsValid(forwardingConsol, new[] { loadList1, loadList2 }, true);

			AssertUserPromptedForConfirmation(MismatchingMasterBillMessage, forwardingConsol, [loadList3, loadList4]);
			AssertUserPromptedForConfirmation(MismatchingMasterBillMessage, forwardingConsol, [loadList1, loadList3]);
		}

		public void TestHasFilterForMasterBill()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = "MBN20181220";

			using (new AssertHasFilter(forwardingConsol, filter => filter.FilterName.Equals(MasterBill)))
			{
			}
		}

		public void TestHasFilterForVoyage()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport = forwardingConsol.Transports.Cast<Transport>().Single();
			transport.JW_VoyageFlight = "VO0001";

			using (new AssertHasFilter(forwardingConsol, filter => filter.FilterName.Equals(FlightVoyageVessel) && filter.PropertyName.Equals("VoyageFlightNo")))
			{
			}
		}

		public void TestHasFilterForVessel()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = forwardingConsol.Transports.Cast<Transport>().Single();
			transport.JW_Vessel = "VE0001";

			using (new AssertHasFilter(forwardingConsol, filter => filter.FilterName.Equals(FlightVoyageVessel) && filter.PropertyName.Equals("Vessel")))
			{
			}
		}

		public void TestHasFilterForETD()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transport = forwardingConsol.Transports.Cast<Transport>().Single();
			transport.JW_ETD = ZDateTime.Today;

			using (new AssertHasFilter(forwardingConsol, filter => filter.FilterName.Equals(ETD)))
			{
			}
		}

		public void TestHasFilterForETA()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transport = forwardingConsol.Transports.Cast<Transport>().Single();
			transport.JW_ETA = ZDateTime.Today.AddDays(1);

			using (new AssertHasFilter(forwardingConsol, filter => filter.FilterName.Equals(ETA)))
			{
			}
		}

		public void TestAttachLoadListToConsolModuleDecisionProvider_UsesAttachedHVLVOriginLoadListCollection()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (var filterModule = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.HVLVOriginLoadList))
			using (var embeddedModulePopup = new EmbeddedModulePopup(filterModule))
			{
				var dummyFindBox = new DummyFindBox(embeddedModulePopup);
				var attachLoadListToConsolModuleDecisionProvider = new AttachLoadListToConsolModuleDecisionProvider(forwardingConsol, dummyFindBox);
				var createConsolFromLoadListModuleDecisionProvider = new CreateConsolFromLoadListModuleDecisionProvider(Factory, dummyFindBox);

				var attachedHVLVOriginLoadListCollection = attachLoadListToConsolModuleDecisionProvider.List;
				var createdConsolHVLVOriginLoadListCollection = createConsolFromLoadListModuleDecisionProvider.List;

				AssertType<AttachedHVLVOriginLoadListCollection>(attachedHVLVOriginLoadListCollection);
				AssertNotEquals(createdConsolHVLVOriginLoadListCollection.GetType(), attachedHVLVOriginLoadListCollection.GetType());
			}
		}
		void AssertLoadListSelectionIsValid(ForwardingConsol consol, HVLVOriginLoadList[] loadLists, bool isValid)
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.HVLVOriginLoadList))
			using (var popupForm = new EmbeddedModulePopup(module))
			{
				var findBox = new DummyFindBox(popupForm);
				var decisionProvider = new AttachLoadListToConsolModuleDecisionProvider(consol, findBox);
				decisionProvider.HandleFindBoxOKButton(loadLists);
				AssertEquals(isValid, findBox.PopupFormHandledSelection);
			}
		}

		void AssertUserPromptedForConfirmation(string expectedMessage, ForwardingConsol consol, HVLVOriginLoadList[] loadLists)
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			AssertLoadListSelectionIsValid(consol, loadLists, false);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertLoadListSelectionIsValid(consol, loadLists, false);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		const string MasterBill = "Master Bill #";
		const string FlightVoyageVessel = "Voyage / Vessel";
		const string ETD = "ETD";
		const string ETA = "ETA";
		const string MismatchingMasterBillMessage = "Selected HVLV Origin Load List(s) do not have matching Master Bill with this Consol, do you wish to proceed?";

		class AssertHasFilter : IDisposable
		{
			public AssertHasFilter(ForwardingConsol forwardingConsol, Func<FilterBusinessObjectDefault, bool> assertTrueFunc)
			{
				filterModule = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.HVLVOriginLoadList);
				embeddedModulePopup = new EmbeddedModulePopup(filterModule);

				var dummyFindBox = new DummyFindBox(embeddedModulePopup);
				var attachLoadListToConsolModuleDecisionProvider = new AttachLoadListToConsolModuleDecisionProvider(forwardingConsol, dummyFindBox);
				var attachedHVLVOriginLoadListCollection = attachLoadListToConsolModuleDecisionProvider.List as AttachedHVLVOriginLoadListCollection;
				AssertNotNull(attachedHVLVOriginLoadListCollection);

				var filterBusinessObjectDefaults = attachedHVLVOriginLoadListCollection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>();
				AssertNotNull(filterBusinessObjectDefaults);
				Assert(filterBusinessObjectDefaults.Any(filter => assertTrueFunc(filter)));
			}

			public void Dispose()
			{
				embeddedModulePopup.Dispose();
				filterModule.Dispose();
			}

			readonly ZFilterModule filterModule;
			readonly EmbeddedModulePopup embeddedModulePopup;
		}
	}
}
