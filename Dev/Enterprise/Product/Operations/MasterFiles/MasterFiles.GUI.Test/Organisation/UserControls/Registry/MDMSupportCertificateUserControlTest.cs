using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(MDMSupportCertificateUserControl))]
	public class MDMSupportCertificateUserControlTest : RegistryZUserControlTestCase
	{
		public void TestHasChanges()
		{
			using (var control = new MDMSupportCertificateUserControlForTest())
			{
				control.FakeLoadedCertificateBytesForNextTime = ZBlob.FromUTF8("Fake Certificate");
				control.FakeLoadedPrivateKeyForNextTime = "Fake Private Key";

				var actions = new[]
				{
					new Action<MDMSupportCertificateUserControlForTest>(x => x.ClientIdBox.Text = "Fake Client ID"),
					new Action<MDMSupportCertificateUserControlForTest>(x => x.TenantIdBox.Text = "Fake Tenant ID"),
					new Action<MDMSupportCertificateUserControlForTest>(x => x.TargetClientIdBox.Text = "Fake AVS Client ID"),
					new Action<MDMSupportCertificateUserControlForTest>(x => x.LoadCertificateButton.PerformClick()),
					new Action<MDMSupportCertificateUserControlForTest>(x => x.LoadPrivateKeyButton.PerformClick())
				};

				actions.ForEach(x =>
				{
					var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
					control.SetDataBinding(certificateInfo, string.Empty);

					x.Invoke(control);

					AssertEquals(true, certificateInfo.HasChanges);
				});
			}
		}

		public void TestTargetClientIdBoxCaption()
		{
			using (var control = new MDMSupportCertificateUserControlForTest(MDMProductCodes.AVS))
			{
				AssertEquals("Target AVS ID", control.TargetClientIdBox.CaptionResourceString.Caption);
			}

			using (var control = new MDMSupportCertificateUserControlForTest(MDMProductCodes.DPS))
			{
				AssertEquals("Target DPS ID", control.TargetClientIdBox.CaptionResourceString.Caption);
			}
		}

		public void TestLoadCertificateFile()
		{
			using (var zform = new ZForm())
			{
				var control = new MDMSupportCertificateUserControlForTest();
				zform.Controls.Add(control);
				zform.Show();

				control.FakeLoadedCertificateBytesForNextTime = ZBlob.FromUTF8("Fake Certificate");
				AssertEquals(string.Empty, control.CertificateBox.Text);

				var certificateInfo = new SystemToSystemTrustInfo();
				control.SetDataBinding(certificateInfo, string.Empty);
				AssertEquals(string.Empty, control.CertificateBox.Text);

				control.LoadCertificateButton.PerformClick();
				AssertEquals("CERTIFICATE LOADED.", control.CertificateBox.Text);
			}
		}

		public void TestLoadEmptyCertificateFile()
		{
			using (var zform = new ZForm())
			{
				var control = new MDMSupportCertificateUserControlForTest();
				zform.Controls.Add(control);
				zform.Show();

				control.FakeLoadedCertificateBytesForNextTime = ZBlob.Empty;
				AssertEquals(string.Empty, control.CertificateBox.Text);

				var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
				control.SetDataBinding(certificateInfo, string.Empty);
				AssertEquals("CERTIFICATE LOADED.", control.CertificateBox.Text);

				control.LoadCertificateButton.PerformClick();
				AssertEquals(string.Empty, control.CertificateBox.Text);
			}
		}

		public void TestLoadPrivateKeyFile()
		{
			using (var zform = new ZForm())
			{
				var control = new MDMSupportCertificateUserControlForTest();
				zform.Controls.Add(control);
				zform.Show();

				control.FakeLoadedPrivateKeyForNextTime = "Fake PrivateKey";
				AssertEquals(string.Empty, control.PrivateKeyBox.Text);

				var certificateInfo = new SystemToSystemTrustInfo();
				control.SetDataBinding(certificateInfo, string.Empty);
				AssertEquals(string.Empty, control.PrivateKeyBox.Text);

				control.LoadPrivateKeyButton.PerformClick();
				AssertEquals("PRIVATE KEY LOADED.", control.PrivateKeyBox.Text);
			}
		}

		public void TestLoadEmptyPrivateKeyFile()
		{
			using (var zform = new ZForm())
			{
				var control = new MDMSupportCertificateUserControlForTest();
				zform.Controls.Add(control);
				zform.Show();

				control.FakeLoadedPrivateKeyForNextTime = string.Empty;
				AssertEquals(string.Empty, control.PrivateKeyBox.Text);

				var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
				control.SetDataBinding(certificateInfo, string.Empty);
				AssertEquals("PRIVATE KEY LOADED.", control.PrivateKeyBox.Text);

				control.LoadPrivateKeyButton.PerformClick();
				AssertEquals(string.Empty, control.PrivateKeyBox.Text);
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new SystemToSystemTrustInfo();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((MDMSupportCertificateUserControl)control).ReadOnly;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return base.ShouldIgnoreMissingBindingMember(control)
				|| control.Name == "certificateBox"
				|| control.Name == "privateKeyBox";
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new MDMSupportCertificateUserControl(new MDMSupportCertificateRegistryDataType(MDMProductCodes.AVS));
		}
	}

	public class MDMSupportCertificateUserControlForTest : MDMSupportCertificateUserControl
	{
		public MDMSupportCertificateUserControlForTest(MDMProductCodes productCode = MDMProductCodes.AVS) : base(new MDMSupportCertificateRegistryDataType(productCode))
		{
		}

		public byte[] FakeLoadedCertificateBytesForNextTime { get; set; }
		public string FakeLoadedPrivateKeyForNextTime { get; set; }

		public ZTextBox ClientIdBox => clientIdBox;
		public ZTextBox TenantIdBox => tenantIdBox;
		public ZTextBox TargetClientIdBox => targetClientIdBox;
		public ZButton LoadCertificateButton => loadCertificateButton;
		public ZButton LoadPrivateKeyButton => loadPrivateKeyButton;
		public ZTextBox CertificateBox => certificateBox;
		public ZTextBox PrivateKeyBox => privateKeyBox;

		protected override bool TryLoadCertificateFile(out byte[] certificateBytes)
		{
			certificateBytes = FakeLoadedCertificateBytesForNextTime?.ToArray();
			FakeLoadedCertificateBytesForNextTime = null;

			return certificateBytes != null;
		}

		protected override bool TryLoadPrivateKeyFile(out string privateKey)
		{
			privateKey = FakeLoadedPrivateKeyForNextTime;
			FakeLoadedPrivateKeyForNextTime = null;

			return privateKey != null;
		}
	}
}
