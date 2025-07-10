using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ExpireCommissionAgreementForm))]
	sealed class ExpireCommissionAgreementFormTest : ZFormBasherTest
	{
		#region Buttons

		[RequiresSTA]
		public void TestOkButton()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var action = new ExpireCommissionAgreementAction(new[] { agreement });

			using (var form = new ExpireCommissionAgreementForm(action))
			{
				form.Show();

				var formClosed = false;
				form.FormClosed += (object sender, FormClosedEventArgs e) =>
					{
						formClosed = true;
					};

				action.Date = ZDateTime.Empty;
				form.OkButton.PerformClick();
				AssertEquals(false, formClosed);
				AssertMultilineASCIIEquals("Text", "Unable to continue", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(false, agreement.IsExpired(new ZDate(2000, 1, 1)));

				UnitTestUserNotification.Instance.ClearMessages();
				action.Date = new ZDateTime(2000, 1, 1);
				form.OkButton.PerformClick();
				AssertEquals(true, formClosed);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(true, agreement.IsExpired(new ZDate(2000, 1, 1)));
			}
		}

		#endregion

		#region Form Captions

		[RequiresSTA]
		public void TestFormVerb()
		{
			var action = new ExpireCommissionAgreementAction(Enumerable.Empty<OrgCommissionAgreement>());
			using (var form = new ExpireCommissionAgreementForm(action))
			{
				form.Show();
				AssertEquals("", form.FormVerb);
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var action = new ExpireCommissionAgreementAction(new[] { agreement });
			return new ExpireCommissionAgreementForm(action);
		}

		#endregion
	}
}
