using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeader))]
	sealed class CusInBondMoveHeaderWorkflowProviderTest : WorkflowProviderTest<CusInBondMoveHeader, ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>>
	{
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
			var moveHeader = header.MovementHeaders.AddNew();
			var implementation = (IWorkflowProvider)moveHeader;
			var ranker = (ColumnValueRanker)implementation.GetTemplateSelectionCriteria();
			AssertEquals("P0_GB", header.BH_GB, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB)[0]);
			AssertEquals("P0_OH_Client", header.Importer.OA_OH, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
			AssertEquals("P0_LoadPortCountry", "AU!23", ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[0]);
			AssertEquals("P0_DischargePortCountry", "US!23", ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[0]);
		}

		public override void TestProcessTasksCascadeDeleted()
		{
			Assert("The process task belong to InBondHeader so this test is n/a.", true);
		}

		protected override Type ParentProxyType => typeof(CusInBondHeader);

		protected override BusinessObject GetParent(CusInBondMoveHeader bizo) => bizo.Header;

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode;

		protected override CusInBondMoveHeader GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			return header.MovementHeaders.AddNew();
		}
	}
}
