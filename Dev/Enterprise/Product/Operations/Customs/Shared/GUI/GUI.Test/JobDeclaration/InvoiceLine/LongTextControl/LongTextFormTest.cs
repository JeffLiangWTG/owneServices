using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(LongTextForm))]
	sealed class LongTextFormTest : ZFormBasherTest
	{
		public void TestBeepPlay()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var captionResourceString = NoResourceStringData.GetData("Desc.", string.Empty, "Goods Description", "Description of goods.");
			using (var form = new LongTextFormForTest(invoiceLine, AutoJobComInvoiceLine.Schema.JI_Description, captionResourceString, CharacterCasing.Normal))
			{
				form.Show();
				var longTextTextBox = form.FindSingleOrDefault<ZTextBox>(c => c.Name == "LongTextTextBox");
				longTextTextBox.Focus();
				var maxLength = invoiceLine.JI_DescriptionInfo.MaxLength;
				invoiceLine.JI_Description = new ZString('R', maxLength - 1);
				form.BeepPlay = false;
				KeySender.PostKeyDown(longTextTextBox, Keys.R);
				Application.DoEvents();
				Assert(!form.BeepPlay);
				KeySender.PostKeyUp(longTextTextBox, longTextTextBox.Handle, Keys.R);
				Application.DoEvents();
				Assert(!form.BeepPlay);
				invoiceLine.JI_Description = new ZString('R', maxLength);
				form.BeepPlay = false;
				KeySender.PostKeyDown(longTextTextBox, Keys.R);
				Application.DoEvents();
				Assert(!form.BeepPlay);
				KeySender.PostKeyUp(longTextTextBox, longTextTextBox.Handle, Keys.R);
				Application.DoEvents();
				Assert(form.BeepPlay);
			}
		}

		public void TestCharacterCasing()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var captionResourceString = NoResourceStringData.GetData("Desc.", string.Empty, "Goods Description", "Description of goods.");
			using (var form = new LongTextFormForTest(invoiceLine, AutoJobComInvoiceLine.Schema.JI_Description, captionResourceString, CharacterCasing.Normal))
			{
				form.Show();
				var longTextTextBox = form.FindSingleOrDefault<ZTextBox>(c => c.Name == "LongTextTextBox");
				longTextTextBox.Text = "r";
				AssertEquals("r", longTextTextBox.Text);
				longTextTextBox.Text = "R";
				AssertEquals("R", longTextTextBox.Text);
			}

			using (var form = new LongTextFormForTest(invoiceLine, AutoJobComInvoiceLine.Schema.JI_Description, captionResourceString, CharacterCasing.Upper))
			{
				form.Show();
				var longTextTextBox = form.FindSingleOrDefault<ZTextBox>(c => c.Name == "LongTextTextBox");
				longTextTextBox.Text = "r";
				AssertEquals("R", longTextTextBox.Text);
			}

			using (var form = new LongTextFormForTest(invoiceLine, AutoJobComInvoiceLine.Schema.JI_Description, captionResourceString, CharacterCasing.Lower))
			{
				form.Show();
				var longTextTextBox = form.FindSingleOrDefault<ZTextBox>(c => c.Name == "LongTextTextBox");
				longTextTextBox.Text = "R";
				AssertEquals("r", longTextTextBox.Text);
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900298. Cannot hardcode string for a form that requires ResourceString.")]
		protected override Form GetFormToBashCore()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var captionResourceString = Res.GetData("LongTextFormTest", "Desc.", string.Empty, "Goods Description", "Description of goods.");
			return new LongTextForm(invoiceLine, AutoJobComInvoiceLine.Schema.JI_Description, captionResourceString, CharacterCasing.Normal);
		}

		sealed class LongTextFormForTest : LongTextForm
		{
			internal LongTextFormForTest(BusinessObject dataSource, string bindingMember, ResourceStringData captionResourceString, CharacterCasing characterCasing) : base(dataSource, bindingMember, captionResourceString, characterCasing)
			{
			}

			public bool BeepPlay { get; set; }

			protected override void OverMaxLengthCallBack()
			{
				BeepPlay = true;
			}
		}
	}
}
