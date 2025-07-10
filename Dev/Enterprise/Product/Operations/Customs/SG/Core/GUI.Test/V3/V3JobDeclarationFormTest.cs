using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.SG.V3.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V3.GUI.Testing
{
	[TestedType(typeof(V3JobDeclarationForm))]
	sealed class V3JobDeclarationFormTest : ZFormBasherTest
	{
		public void TestFormAddsUserControl()
		{
			using (var form = (V3JobDeclarationForm)GetFormToBash())
			{
				var control = form.MainPanel.Controls[0];
				AssertEquals(DockStyle.Fill, control.Dock);
				Assert(control is V3CustomsBrokerageUserControl);
			}
		}

		public void TestFormHeading()
		{
			using (var form = (V3JobDeclarationForm)GetFormToBash())
			{
				AssertContains("TradeNet V3 Declaration", form.FormHeading);
			}
		}

		public void TestFormHVerb()
		{
			using (var form = (V3JobDeclarationForm)GetFormToBash())
			{
				AssertContains("View", form.FormVerb);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var formToBash = new V3JobDeclarationForm(new V3Brokerage(Factory.New<V3JobDeclaration>().PK, Factory));
			MissingResourceStringChecker.ExcludeFromTest(formToBash.Controls.Find("MessageTextTextBox", true).Single());
			return formToBash;
		}
	}
}
