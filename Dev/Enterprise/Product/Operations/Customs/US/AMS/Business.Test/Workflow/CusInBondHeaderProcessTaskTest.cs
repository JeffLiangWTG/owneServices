using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderProcessTask))]
	class CusInBondHeaderProcessTaskTest : ProcessTaskTest
	{
		public void TestOverrides()
		{
			var processTask = (ProcessTask)GetNewBusinessObject();
			AssertEquals("ParentType", typeof(CusInBondHeader), processTask.Parent.GetType());
			AssertEquals("ParentControllerID", ControllerIDs.Customs.US.AMS, processTask.ParentControllerID);
		}

		#region ProcessTask Property Overrides

		public void TestETA_EmptyInBase()
		{
			var task = Factory.NewWithValidTestData<CusInBondHeaderProcessTask>();
			task.P9_ParentID = Header.PK;
			task.P9_ParentTableCode = Header.TablePrefix;

			Factory.Save();

			AssertEquals("ETA field should return CusInBondHeader.BH_ETA for current Exception if its ParentTableCode is BH", ZDateTime.BrettsBirthday, task.ETA);
		}

		public void TestLoadOrOriginPort_EmptyInBase()
		{
			var task = Factory.NewWithValidTestData<CusInBondHeaderProcessTask>();
			task.P9_ParentID = Header.PK;
			task.P9_ParentTableCode = Header.TablePrefix;

			Factory.Save();

			AssertEquals("LoadOrOriginPort field should return CusInBondHeader.Sailing.JX_JA_RL_NKPortOfLoading for current Exception if its ParentTableCode is BH", "12!AU", task.LoadOrOriginPort);
		}

		public void TestDischargeOrDestinationPort_EmptyInBase()
		{
			var task = Factory.NewWithValidTestData<CusInBondHeaderProcessTask>();
			task.P9_ParentID = Header.PK;
			task.P9_ParentTableCode = Header.TablePrefix;

			Factory.Save();

			AssertEquals("DischargeOrDestinationPort field should return CusInBondHeader.BH_RL_NKPortUnlading for current Exception if its ParentTableCode is BH", "34!US", task.DischargeOrDestinationPort);
		}

		#endregion
		#region Implementation

		ProcessTaskCollection WorkflowItems
		{
			get { return ((IWorkflowProvider)Header).WorkflowItems; }
		}

		CusInBondHeader Header
		{
			get { return header ?? (header = Factory.New<CusInBondHeader>()); }
		}

		CusInBondHeader header;

		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowItems.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AU!12";
			var loadPortMap = loadPort.RefLocoMaps.AddNew();
			loadPortMap.RY_RN = Core.Constants.CountryGuids.Australia;
			loadPortMap.RY_LocalPortCode = "12!AU";
			loadPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			var dischargePort = Factory.New<RefUNLOCO>();
			dischargePort.RL_Code = "US!34";
			var dischargePortMap = dischargePort.RefLocoMaps.AddNew();
			dischargePortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			dischargePortMap.RY_LocalPortCode = "34!US";
			dischargePortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "12!AU";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "34!US";
			destination.JB_E_ARV = ZDateTime.BrettsBirthday;

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			Header.ChangeSailing(sailing.PK);
		}

		#endregion
	}
}
