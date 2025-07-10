using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class TripUserControlTest : TestCaseWithFactory
	{
		public void TestOrderOfPortBoxes()
		{
			using (var uc = new TripUserControl())
			{
				var unloco = uc.Controls.Find("FirstExpectedPortOfArrivalCodeFindBox", true)[0];
				var schedD = uc.Controls.Find("FirstExpectedPortOfArrivalDCodeFindBox", true)[0];
				Assert(unloco.Location.X > schedD.Location.X);
				Assert(unloco.TabIndex > schedD.TabIndex);
			}
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			CreateLocoMapIfNotExists("4003", "USLAX", USLocoMapSystemUsageList.Codes.All);
			CreateLocoMapIfNotExists("4004", "USLAX", USLocoMapSystemUsageList.Codes.All);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4004", "Test Name", new DateTime(1900, 01, 01), new DateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4003", "Test Name", new DateTime(1900, 01, 01), new DateTime(2079, 06, 06));
			Factory.Save();

			var trip = Factory.NewWithValidTestData<Trip>();
			using (var control = new TripUserControl())
			{
				control.SetDataBinding(trip, "");
				control.Show();
				var dropEdit = control.FindSingle<ZDropEdit>("FirstExpectedPortOfArrivalDDropEdit");
				var findBox = control.FindSingle<ZCodeFindBox>("FirstExpectedPortOfArrivalDCodeFindBox");

				AssertEquals(false, dropEdit.Visible);
				AssertEquals(true, findBox.Visible);

				trip.BH_RL_NKPortUnlading = "USLAX";
				AssertEquals(true, dropEdit.Visible);
				AssertEquals(false, findBox.Visible);
			}
		}

		RefLocoMap CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, bool isSystem = false)
		{
			var codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);

			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
			}

			return locoMap;
		}
	}
}
