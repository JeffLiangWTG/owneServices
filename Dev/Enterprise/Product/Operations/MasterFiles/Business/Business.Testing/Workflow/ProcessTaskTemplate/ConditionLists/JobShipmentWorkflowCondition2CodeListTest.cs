using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobShipmentWorkflowCondition2CodeListTest : TestCaseWithFactory
	{
		public void TestItems()
		{
			AssertEquals(JobShipmentWorkflowCondition2CodeList.Codes.Import, CodeList[0].Code);
			AssertEquals("Import", CodeList[0].Description);
			AssertEquals(JobShipmentWorkflowCondition2CodeList.Codes.Export, CodeList[1].Code);
			AssertEquals("Export", CodeList[1].Description);
			AssertEquals(JobShipmentWorkflowCondition2CodeList.Codes.Domestic, CodeList[2].Code);
			AssertEquals("Domestic", CodeList[2].Description);

			foreach (CodeDescriptionPair pair in ContainerModes)
			{
				if (pair.Code != Core.Constants.ContainerModes.BuyersConsol && pair.Code != Core.Constants.ContainerModes.Other)
				{
					AssertEquals("Container mode " + pair.Code, true, CodeList.ContainsCode(pair.Code));
				}
			}

			AssertEquals("Buyer's Consol is replaced with Buyer's Consol Master and Sub (below)", false, CodeList.ContainsCode(Core.Constants.ContainerModes.BuyersConsol));
			AssertEquals("Container mode 'Other' not included", false, CodeList.ContainsCode(Core.Constants.ContainerModes.Other));

			AssertEquals("Buyer's Consol Master", CodeList[JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster].Description);
			AssertEquals("Buyer's Consol Sub", CodeList[JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub].Description);
			AssertEquals("Co-Load Master", CodeList[JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster].Description);
			AssertEquals("Co-Load Sub", CodeList[JobShipmentWorkflowCondition2CodeList.Codes.ColoadSub].Description);
			AssertEquals("Assembly Master", CodeList[JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster].Description);
			AssertEquals("Assembly Sub", CodeList[JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub].Description);
			AssertEquals("Release Type", CodeList[JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType].Description);
		}

		#region Implementation

		JobShipmentWorkflowCondition2CodeList CodeList
		{
			get { return codeList ?? (codeList = new JobShipmentWorkflowCondition2CodeList()); }
		}
		JobShipmentWorkflowCondition2CodeList codeList;

		CodeDescriptionPairList ContainerModes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.FreightContainerMode); }
		}

		#endregion
	}
}
