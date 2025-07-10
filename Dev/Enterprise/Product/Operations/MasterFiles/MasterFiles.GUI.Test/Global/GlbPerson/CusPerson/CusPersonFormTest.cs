using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CusPersonForm))]
	sealed class CusPersonFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			return new CusPersonForm(person);
		}
	}
}
