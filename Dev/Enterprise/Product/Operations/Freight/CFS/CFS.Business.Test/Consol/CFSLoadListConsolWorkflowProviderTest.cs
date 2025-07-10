using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsol))]
	public class CFSLoadListConsolWorkflowProviderTest : WorkflowProviderTest<CFSLoadListConsol, CFSLoadListConsolProcessTaskCollection>
	{
		#region GetColumnValueRanker

		public void TestGetTemplateFilterCriteria_ForTransportMode()
		{
			LoadList.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(LoadList.JK_TransportModeInfo, ProcessTaskTemplate.P0_SubType1Info, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForLoadPort()
		{
			LoadList.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(LoadList.JK_RL_NKLoadPortInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForDischargePort()
		{
			LoadList.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(LoadList.JK_RL_NKDischargePortInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		#endregion

		#region Implementation

		CFSLoadListConsol LoadList
		{
			get { return BusinessObject; }
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return JobInvoicingConsumerTypes.CFSLoadList.Code; }
		}

		#endregion
	}
}
