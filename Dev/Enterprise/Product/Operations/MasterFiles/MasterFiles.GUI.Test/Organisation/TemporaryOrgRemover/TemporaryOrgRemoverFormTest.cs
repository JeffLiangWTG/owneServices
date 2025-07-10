using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.TemporaryOrgRemover;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TemporaryOrgRemoverForm))]
	sealed class TemporaryOrgRemoverFormTest : ZFormBasherTest
	{
		#region Remover For Testing

		class RemoverForTesting : Remover
		{
			public RemoverForTesting()
				: base()
			{
			}

			public override TemporaryOrgCollection Organisations
			{
				get
				{
					TemporaryOrgCollection result = new TemporaryOrgCollection(Factory);
					result.Add(new TemporaryOrg(HeaderForTesting.PK, HeaderForTesting.OH_Code, HeaderForTesting.OH_FullName));
					return result;
				}
			}

			public OrgHeader HeaderForTesting
			{
				get
				{
					if (fHeaderForTesting == null)
					{
						fHeaderForTesting = CreateTestHeader();
					}

					return fHeaderForTesting;
				}
			}
			OrgHeader fHeaderForTesting;

			OrgHeader CreateTestHeader()
			{
				OrgHeader result = OrgHeader.New(Factory);
				result.OH_FullName = "ZZAAZZ";
				result.OH_RL_NKClosestPort = "AUSYD";
				result.MainAddress.OA_Address1 = "Address1";
				result.BrandsOrRelatedNames.AddNew();

				Factory.Save();
				return result;
			}
		}

		#endregion

		#region Remover Form For Testing

		class TemporaryOrgRemoverFormForTesting : TemporaryOrgRemoverForm
		{
			public TemporaryOrgRemoverFormForTesting(Remover removerObject)
				: base(removerObject)
			{
			}

			protected override void ShowErrorsFormWihoutDispose(TemporaryOrgRemoverErrorsForm errorsForm)
			{
				ErrorsFormShown = true;
			}

			public bool ErrorsFormShown;
		}

		#endregion

		public void TestConfirmationDialog()
		{
			using (TemporaryOrgRemoverFormForTesting form = new TemporaryOrgRemoverFormForTesting(new Remover()))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.DeleteButton.PerformClick();
				Assert(form.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.DeleteButton.PerformClick();
				Assert(!form.Visible);
			}
		}

		public void TestOrgsFailedToDeleteForm()
		{
			using (TemporaryOrgRemoverFormForTesting form = new TemporaryOrgRemoverFormForTesting(new RemoverForTesting()))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				Assert(!form.ErrorsFormShown);

				form.DeleteButton.PerformClick();
				Assert(!form.Visible);
				Assert(form.ErrorsFormShown);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new TemporaryOrgRemoverForm(new Remover());
		}
	}
}
