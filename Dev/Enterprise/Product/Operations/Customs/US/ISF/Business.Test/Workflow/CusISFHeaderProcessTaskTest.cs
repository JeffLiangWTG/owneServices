using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeaderProcessTask))]
	sealed class CusISFHeaderProcessTaskTest : ProcessTaskTest
	{
		public void TestDischargeOrDestinationPort_EmptyInBase()
		{
			var header = Factory.New<CusISFHeader>();
			var task = Factory.NewWithValidTestData<CusISFHeaderProcessTask>();
			task.P9_ParentID = header.PK;
			task.P9_ParentTableCode = header.TablePrefix;
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			Factory.Save();
			AssertEquals("DischargeOrDestinationPort field should return CusISFHeader.BF_RL_NKPlaceOfDelivery", "USLAX", task.DischargeOrDestinationPort);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusISFHeader>();
			return ((IWorkflowProvider)header).WorkflowItems.AddNew();
		}
	}
}
