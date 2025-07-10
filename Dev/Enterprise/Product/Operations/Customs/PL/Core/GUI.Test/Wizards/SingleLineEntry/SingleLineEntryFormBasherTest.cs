using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(SingleLineEntryForm))]
sealed class SingleLineEntryFormBasherTest : ZFormBasherTest
{
	public void TestLIC99Control_NotVisible()
	{
		using (var form = GetFormToBashCore())
		{
			form.Show();
			var control = form.Controls.Find("CheckBoxLic99", true).First();
			AssertEquals(false, control.Visible);
		}
	}

	protected override Form GetFormToBashCore() => new SingleLineEntryForm(new SingleLineEntryManager(Factory.New<JobDeclaration>()));
}
