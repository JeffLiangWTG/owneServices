using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondHeader))]
	sealed class CusInBondHeaderWorkflowProviderTest : WorkflowProviderTest<CusInBondHeader, ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>>
	{
		public void TestResetValueOnContainer()
		{
			var loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AU!23";
			var loadPortMap = loadPort.RefLocoMaps.AddNew();
			loadPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			loadPortMap.RY_LocalPortCode = "12!AU";
			loadPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			var discPort = Factory.New<RefUNLOCO>();
			discPort.RL_Code = "US!23";
			var discPortMap = discPort.RefLocoMaps.AddNew();
			discPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			discPortMap.RY_LocalPortCode = "1!US";
			discPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_ImportLoadPortKCode = "12!AU";
			header.BH_PortUnladingDCode = "1!US";
			header.BH_FTZMove = false;
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew("APLU", "789654");
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "ContainerNo";
			var commodities1 = container.Commodities.AddNew();
			commodities1.BY_PieceCount = 30;
			var commodities2 = container.Commodities.AddNew();
			commodities2.BY_PieceCount = 40;
			AssertEquals(false, header.BH_FTZMove);
			AssertEquals(0, container.BC_PieceCount);
			container.BC_PieceCount = 454;
			AssertEquals(454, container.BC_PieceCount);
			AssertEquals(30, commodities1.BY_PieceCount);
			AssertEquals(40, commodities2.BY_PieceCount);
			header.BH_FTZMove = true;
			AssertEquals(true, header.BH_FTZMove);
			AssertEquals(0, container.BC_PieceCount);
			AssertEquals(30, commodities1.BY_PieceCount);
			AssertEquals(40, commodities2.BY_PieceCount);
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AU!23";
			var loadPortMap = loadPort.RefLocoMaps.AddNew();
			loadPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			loadPortMap.RY_LocalPortCode = "12!AU";
			loadPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			var discPort = Factory.New<RefUNLOCO>();
			discPort.RL_Code = "US!23";
			var discPortMap = discPort.RefLocoMaps.AddNew();
			discPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			discPortMap.RY_LocalPortCode = "1!US";
			discPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			header.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;
			header.BH_ImportLoadPortKCode = "12!AU";
			header.BH_PortUnladingDCode = "1!US";
			var implementation = (IWorkflowProvider)header;
			var ranker = (ColumnValueRanker)implementation.GetTemplateSelectionCriteria();
			AssertEquals("P0_GB", header.BH_GB, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB)[0]);
			AssertEquals("P0_OH_Client", header.Importer.OA_OH, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
			AssertEquals("P0_LoadPortCountry", "AU!23", ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[0]);
			AssertEquals("P0_DischargePortCountry", "US!23", ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[0]);
		}

		public void TestGetNewCusInBondHeaderProcessTaskCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>", typeof(ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>), ((IWorkflowProvider)header).WorkflowItems);
		}

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode;
	}
}
