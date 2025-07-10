using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(JobTradeLaneForm))]
	sealed class JobTradeLaneFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			tradeLane.EJ_Code = "AAAA";
			using (JobTradeLaneForm jobTradeLaneForm = new JobTradeLaneForm(tradeLane))
			{
				AssertEquals("Trade Lane AAAA", jobTradeLaneForm.FormCaption);
			}
		}

		public void TestNotSupportsEDocs()
		{
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			using (JobTradeLaneForm jobTradeLaneForm = new JobTradeLaneForm(tradeLane))
			{
				AssertNull(jobTradeLaneForm.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			return new JobTradeLaneForm(tradeLane);
		}

		#endregion
	}
}
