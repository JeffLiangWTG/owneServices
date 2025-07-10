using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ReceivablesConfigurationTest : TestCaseWithFactory
	{
		public void TestOM_ARVATSplitPaymentApplicableBoundCheckEdit()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (ZForm form = new ZForm(header))
			using (ReceivablesConfigurationUserControl control = new ReceivablesConfigurationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.OM_ARVATSplitPaymentApplicableBoundCheckEdit.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			using (ZForm form = new ZForm(header))
			using (ReceivablesConfigurationUserControl control = new ReceivablesConfigurationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.OM_ARVATSplitPaymentApplicableBoundCheckEdit.Visible);
			}
		}

		public void TestOB_GSTRegisteredCheckboxVisibility()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			OrgHeader header = Factory.New<OrgHeader>();
			using (ZForm form = new ZForm(header))
			using (ReceivablesConfigurationUserControl control = new ReceivablesConfigurationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.OB_ARVATConfigDropEdit.Visible);
				AssertEquals(true, control.OB_ARVATConfigLabel.Visible);
				AssertEquals(true, control.OM_ARDontShowTaxOnDocsBoundCheckEdit.Visible);
				AssertEquals(true, control.OB_ARGoodsOwnershipLabel.Visible);
				AssertEquals(true, control.OB_ARGoodsOwnershipDropEdit.Visible);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (ZForm form = new ZForm(header))
			using (ReceivablesConfigurationUserControl control = new ReceivablesConfigurationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.OB_ARVATConfigDropEdit.Visible);
				AssertEquals(false, control.OB_ARVATConfigLabel.Visible);
				AssertEquals(false, control.OM_ARDontShowTaxOnDocsBoundCheckEdit.Visible);
				AssertEquals(false, control.OB_ARGoodsOwnershipLabel.Visible);
				AssertEquals(false, control.OB_ARGoodsOwnershipDropEdit.Visible);
			}
		}

		public void TestOB_ARCreateVATComplianceDocumentOnPostingLabelAndOverridePayToAccountCheckBoxAndPayToAccountGuidFindBoxCaption()
		{
			using (var control = new ReceivablesConfigurationUserControl())
			{
				var label = control.Controls.Find("OB_ARCreateVATComplianceDocumentOnPostingLabel", true);
				var checkBox = control.Controls.Find("OverridePayToAccountCheckBox", true);
				var findBox = control.Controls.Find("PayToAccountGuidFindBox", true);

				AssertEquals(1, label.Length);
				AssertEquals(1, checkBox.Length);
				AssertEquals(1, findBox.Length);
				var aRCreateVATComplianceDocumentOnPostingLabel = label[0] as ZLabel;
				var overridePayToAccountCheckBox = checkBox[0] as ZCheckBox;
				var payToAccountGuidFindBox = findBox[0] as ZGuidFindBox;
				AssertEquals("Create Compliance Document Record on Posting", aRCreateVATComplianceDocumentOnPostingLabel.CaptionResourceString.Caption);
				AssertEquals("Do Not Use Account Group or Registry Bank Account Defaults", overridePayToAccountCheckBox.CaptionResourceString.Caption);
				AssertEquals("A single, specific bank account for this client's AR documents and receipting can be set by ticking this checkbox.  When not ticked, the AR bank accounts used for this client will come from the AR Account Group falling back to Registry and Default Receipting bank account setups.", overridePayToAccountCheckBox.CaptionResourceString.FullDescription);
				AssertEquals("Bank to This Account", payToAccountGuidFindBox.CaptionResourceString.Caption);
				AssertEquals("A single, specific bank account for this client's AR documents and receipting can be nominated here. The bank account nominated here will be used on all AR documents and when creating new AR receipts.  When blank, the bank accounts used for this client will come from the AR Account Group falling back to Registry and Default Receipting setups.", payToAccountGuidFindBox.CaptionResourceString.FullDescription);
			}
		}

		[RequiresSTA]
		public void TestOB_ARCreateVATComplianceDocumentOnPostingBoundCheckEditVisibility()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (ZForm form = new ZForm(header))
			using (ReceivablesConfigurationUserControl control = new ReceivablesConfigurationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.Visible);
				AssertEquals(true, control.OB_ARCreateVATComplianceDocumentOnPostingLabel.Visible);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ZForm form = new ZForm(header))
			using (ReceivablesConfigurationUserControl control = new ReceivablesConfigurationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.OB_ARCreateVATComplianceDocumentOnPostingDropEdit.Visible);
				AssertEquals(false, control.OB_ARCreateVATComplianceDocumentOnPostingLabel.Visible);
			}
		}

		public void TestTransCreationRestrictionDropEditVisibility()
		{
			var cachedValue = Env.Security.OrgReceivablesModifyConfigTransCreationRestriction.IsAllowed;

			foreach (var isAllowed in new bool[] { true, false })
			{
				using (new DisposableAction(
						() => Env.Security.OrgReceivablesModifyConfigTransCreationRestriction.IsAllowed = isAllowed,
						() => Env.Security.OrgReceivablesModifyConfigTransCreationRestriction.IsAllowed = cachedValue))
				{
					var header = Factory.NewWithValidTestData<OrgHeader>();
					using (var form = new ZForm(header))
					using (var control = new ReceivablesConfigurationUserControl())
					{
						form.Controls.Add(control);
						form.Show();
						Assert(control.zDropEdit_TransCreationRestriction.Visible);
						AssertEquals("Control ReadOnly should follow the relative security right",
									 isAllowed, !control.zDropEdit_TransCreationRestriction.ReadOnly);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestApplicableSurchargesVisibility()
		{
			var testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.OrgReceivablesModifySurchargeConfiguration.IsAllowed = false;
			Env.SetTemporarySecurityInstanceForTest(testSecurity);
			var cachedValue = Env.Security.OrgReceivablesModifySurchargeConfiguration.IsAllowed;

			foreach (var isAllowed in new bool[] { true, false })
			{
				using (new DisposableAction(
						() => Env.Security.OrgReceivablesModifySurchargeConfiguration.IsAllowed = isAllowed,
						() => Env.Security.OrgReceivablesModifySurchargeConfiguration.IsAllowed = cachedValue))
				{
					var header = Factory.NewWithValidTestData<OrgHeader>();
					using (var form = new ZForm(header))
					using (var control = new ReceivablesConfigurationUserControl())
					{
						form.Controls.Add(control);
						form.Show();
						Assert(control.ApplicableSurchargesPanel.Visible);
						Assert(control.ApplicableSurchargesTextBox.Visible);
						Assert(control.ApplicableSurchargesEditButton.Visible);
						AssertEquals("Control ReadOnly should follow the relative security right",
									 isAllowed, !control.ApplicableSurchargesTextBox.ReadOnly);
						AssertEquals("Control Enabled should follow the relative security right",
									 isAllowed, control.ApplicableSurchargesEditButton.Enabled);
					}
				}
			}
		}

		public void TestApplicableSurchargesEditButtonButtonClick()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(header))
			using (var control = new ReceivablesConfigurationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.ApplicableSurchargesEditButtonButton_Click(null, null);

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertType(typeof(AccSurchargeConfigurationCollectionForm), ZFormModaliser.LastFormShownDialogForTest);
				Assert("AccSurchargeConfigurationCollectionForm should be disposed", ZFormModaliser.LastFormShownDialogForTest.IsDisposed);
			}
		}
	}
}
