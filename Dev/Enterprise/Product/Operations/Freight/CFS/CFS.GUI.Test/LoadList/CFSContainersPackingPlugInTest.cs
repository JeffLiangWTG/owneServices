using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class CFSContainersPackingPlugInTest : ContainersPackingPlugInTest
	{
		public void TestPRAPluginIsAdded()
		{
			var loadList = GetConsolToUnpack();
			var container2 = loadList.Containers.AddNew();
			container2.JC_ContainerNum = "FFDU3928392";

			using (var form = CreateForm(loadList))
			{
				form.Show();
				form.SelectContainersTab();
				form.ContainersModuleButtonGrid.InnerGrid.Select(0);
				var pRAFound = false;
				foreach (MenuItem menu in form.Menu.MenuItems)
				{
					if (menu.Text.Contains("PRA Messaging"))
					{
						pRAFound = true;
						break;
					}
				}

				Assert(pRAFound);
			}
		}

		[ExpectNoExceptions]
		public void TestValidationOnPackLinesGrid()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = new ZString("234");
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = new ZString("AUSYD");
			voyage.Origins.Add(origin);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = new ZString("USLAX");
			voyage.Destinations.Add(destination);
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.Transports[0].JW_JX = sailing.PK;

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment1.JS_OuterPacks = 8;
			shipment1.OuterPackLines[0].JL_PackageCount = 7;
			shipment1.JS_ActualWeight = 0;
			shipment1.JS_ActualVolume = 0;
			consol1.RunPreSaveValidation();

			using (var form = CreateForm(consol1))
			{
				form.Show();
				form.SelectContainersTab();
				var elements = ((BusinessObjectCollection)form.UnAllocatedPackLinesGrid.List).ToArray();
				AssertEquals("Element should have a warning on it the first time the container tab is selected", true, elements[0].HasRowWarnings);
			}
		}

		#region ExportReferenceNumber HeaderText

		public new void TestExportReferenceNumberHeaderText()
		{
			foreach (var countryCode in new[]
			{
				Core.Constants.CountryCodes.China,
				Core.Constants.CountryCodes.Taiwan,
				Core.Constants.CountryCodes.HongKong,
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.UnitedStates
			})
			{
				AssertExportReferenceNumberHeaderText(countryCode, Constants.TransportModes.Sea, "Export Reference Number");
				AssertExportReferenceNumberHeaderText(countryCode, Constants.TransportModes.Air, "Export Reference Number");
			}
		}

		public new void TestExportReferenceNumberHeaderText_WhenTransportModeChanged()
		{
			foreach (var countryCode in new[]
			{
				Core.Constants.CountryCodes.China,
				Core.Constants.CountryCodes.Taiwan,
				Core.Constants.CountryCodes.HongKong,
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.UnitedStates
			})
			{
				AssertExportReferenceNumberHeaderText(countryCode,
					Constants.TransportModes.Sea, "Export Reference Number",
					Constants.TransportModes.Air, "Export Reference Number");
			}
		}

		#endregion

		protected override HelperForm CreateForm(CommonConsol consol)
		{
			return new HelperForm(consol, ControllerIDs.LoadListContainersPacking);
		}

		protected override CommonConsol CreateConsol()
		{
			return Factory.New<CFSLoadListConsol>();
		}
	}
}
