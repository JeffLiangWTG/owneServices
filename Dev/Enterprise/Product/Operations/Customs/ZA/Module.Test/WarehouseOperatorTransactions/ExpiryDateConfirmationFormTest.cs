using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(ExpiryDateConfirmationForm))]
	sealed class ExpiryDateConfirmationFormTest : ZFormBasherTest
	{
		public override Type FormToBashType => typeof(ExpiryDateConfirmationForm);
		protected override Form GetFormToBashCore() => new ExpiryDateConfirmationForm(new NonPersistentExpiryDateObject());

		public void TestDateEdit()
		{
			using (var form = GetFormToBashCore())
			{
				var dateEdit = form.Controls.Find("DateEdit", searchAllChildren: true)?.SingleOrDefault() as ZDateEdit;
				AssertNotNull("DateEdit as ZDateEdit", dateEdit);
				AssertEquals("BindingMember", nameof(NonPersistentExpiryDateObject.Date), dateEdit.GetBindingMember());
				AssertEquals("CaptionResourceString.Caption", "Entry Date before which entries must be cleared", dateEdit.CaptionResourceString.Caption);
			}
		}
	}
}
