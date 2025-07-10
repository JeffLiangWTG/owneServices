using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(UNDGCommonDataForm))]
	public class UNDGCommonDataFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new UNDGCommonDataForm(Factory.New<UNDGCommonData>());
		}
	}
}
