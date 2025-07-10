using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class MessageUserControlTest : MessageUserControlForVirtualPropertiesTest<MessageUserControl>
{
	public void TestEntryLineAdditionalDataUserControlType()
	{
		var testDec = Factory.New<JobDeclaration>();

		using (var form = new ZForm(testDec))
		using (var userControl = new MessageUserControl())
		{
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			var entryLineAdditionalDataUserControl = userControl.Controls.Find("EntryLineAdditionalDataUserControl", true).Single();
			AssertType<EntryLineAdditionalDataUserControl>(entryLineAdditionalDataUserControl);
		}
	}

	protected override ZBool DefaultDynamicLayoutApplied => ZBool.True;
}
