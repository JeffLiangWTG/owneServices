using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMAWB))]
	sealed class CusMAWBWorkflowProviderTest : WorkflowProviderTest<CusMAWB, ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return JobInvoicingConsumerTypes.CusMAWB.Code; }
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			var mawb = GetNewBusinessObject(Factory);
			if (mawb.SupportsWorkflow)
			{
				base.TestProcessTasksCreatedOnSave();
			}
			else
			{
				Assert(true);
			}
		}

		protected override bool WorkflowProviderDoesNotApplyTemplatesWhenSavingFactory => !BusinessObject.SupportsWorkflow;

		protected override void OnFinishedRunningTestThatLoadsParentJob()
		{
			Assert("Ignore developer test for unsupported countries",
				"CusMAWBProcessTaskLoadStrategy cannot load a CusMAWBProcessTask for this MAWB as the country is not recognised. Add a case for your country".Equals(ErrorReporter.LastMessageReported, StringComparison.InvariantCulture));
			ErrorReporter.Instance.Clear();
		}
	}
}
