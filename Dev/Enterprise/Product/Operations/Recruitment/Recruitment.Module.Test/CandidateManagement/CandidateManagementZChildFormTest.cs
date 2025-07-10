using System.Windows.Forms;
using Enterprise.Recruitment.Module;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module
{
	[TestedType(typeof(CandidateManagementZChildForm))]
	sealed class CandidateManagementZChildFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var collection = new CandidateBusinessObjectCollection(Factory);
			var obj = new CandidateModuleBusinessObject(collection);
			return new CandidateManagementZChildForm(obj);
		}
	}
}
