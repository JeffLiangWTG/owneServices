using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(EDICodeMappingForm))]
	public class EDICodeMappingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new EDICodeMappingForm(Factory.New<OrgPatternMatchOverride>());
		}
	}
}
