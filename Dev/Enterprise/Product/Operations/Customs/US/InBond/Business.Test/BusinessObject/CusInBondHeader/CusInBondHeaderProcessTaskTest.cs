using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderProcessTask))]
	sealed class CusInBondHeaderProcessTaskTest : ProcessTaskTest
	{
		public void TestOverrides()
		{
			var processTask = (ProcessTask)GetNewBusinessObject();
			AssertEquals("ParentType", typeof(CusInBondHeader), processTask.Parent.GetType());
			AssertEquals("ParentControllerID", ControllerIDs.Customs.US.InBond, processTask.ParentControllerID);
		}

		public void TestETA_EmptyInBase()
		{
			Header.BH_ETA = ZDateTime.BrettsBirthday;
			var task = Factory.NewWithValidTestData<CusInBondHeaderProcessTask>();
			task.P9_ParentID = Header.PK;
			task.P9_ParentTableCode = Header.TablePrefix;
			Factory.Save();
			AssertEquals("ETA field should return CusInBondHeader.BH_ETA for current Exception if its ParentTableCode is BH", Header.BH_ETA, task.ETA);
		}

		public void TestETD_EmptyInBase()
		{
			Header.BH_SailingDate = ZDateTime.BrettsBirthday;
			var task = Factory.NewWithValidTestData<CusInBondHeaderProcessTask>();
			task.P9_ParentID = Header.PK;
			task.P9_ParentTableCode = Header.TablePrefix;
			Factory.Save();
			AssertEquals("ETD field should return CusInBondHeader.BH_SailingDate for current Exception if its ParentTableCode is BH", Header.BH_SailingDate, task.ETD);
		}

		public void TestLoadOrOriginPort_EmptyInBase()
		{
			var loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AU!23";
			var loadPortMap = loadPort.RefLocoMaps.AddNew();
			loadPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			loadPortMap.RY_LocalPortCode = "12!AU";
			loadPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			Header.BH_ImportLoadPortKCode = "12!AU";
			var task = Factory.NewWithValidTestData<CusInBondHeaderProcessTask>();
			task.P9_ParentID = Header.PK;
			task.P9_ParentTableCode = Header.TablePrefix;
			Factory.Save();
			AssertEquals("LoadOrOriginPort field should return CusInBondHeader.BH_Calc_ImportLoadPortUNLOCO for current Exception if its ParentTableCode is BH", "AU!23", task.LoadOrOriginPort);
		}

		public void TestDischargeOrDestinationPort_EmptyInBase()
		{
			var discPort = Factory.New<RefUNLOCO>();
			discPort.RL_Code = "US!23";
			var discPortMap = discPort.RefLocoMaps.AddNew();
			discPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			discPortMap.RY_LocalPortCode = "1!US";
			discPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
			Header.BH_PortUnladingDCode = "1!US";
			var task = Factory.NewWithValidTestData<CusInBondHeaderProcessTask>();
			task.P9_ParentID = Header.PK;
			task.P9_ParentTableCode = Header.TablePrefix;
			Factory.Save();
			AssertEquals("DischargeOrDestinationPort field should return CusInBondHeader.BH_Calc_PortUnladingUNLOCO for current Exception if its ParentTableCode is BH", "US!23", task.DischargeOrDestinationPort);
		}

		protected override BusinessObject GetNewBusinessObject() => WorkflowItems.AddNew();

		ProcessTaskCollection WorkflowItems => ((IWorkflowProvider)Header).WorkflowItems;

		CusInBondHeader header;
		CusInBondHeader Header => header ?? (header = Factory.New<CusInBondHeader>());
	}
}
