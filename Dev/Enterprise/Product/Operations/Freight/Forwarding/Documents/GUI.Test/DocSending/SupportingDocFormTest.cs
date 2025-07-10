using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters;
using Enterprise.Freight.Forwarding.Documents.GUI.Actions;
using Enterprise.Freight.Forwarding.Documents.GUI.DocSending;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.GUI.DocSending
{
	[TestedType(typeof(SupportingDocForm))]
	sealed class SupportingDocFormTest : ZFormBasherTest
	{
		public void TestFormShownText()
		{
			using (var form = new SupportingDocForm(new NZCFTA("ForwardingShipment", "C00001015")))
			{
				form.Show();
				AssertEquals("SupportingDocForm", form.Text);
				form.Close();
				form.Dispose();
			}
		}

		public void TestForm_OkButton_CheckTermAcknowledgedIsTrue_PopulateNZCFTA_FormIsClosed()
		{
			const bool HasBeenAcknowledged = true;
			const int VersionNo = 1337;

			var nzcfta = new NZCFTA("ForwardingShipment", "C00001015")
			{
				AgreementInfo = new AgreementInfo()
			};

			using (DocumentsDataRegistry.Instance.EnableCertOfOriginIndemnity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (ObjectFactory.Substitute<ICertificateOfOriginIndemnityTermsAgreementChecker>(new CertificateOfOriginIndemnityTermsAgreementCheckerForTests(new TermAcknowledged(HasBeenAcknowledged, VersionNo))))
				using (var form = new SupportingDocFormForTests(nzcfta))
				{
					var formClosed = false;
					form.FormClosed += (_, __) => { formClosed = true; };
					form.Show();

					form.OkButton.PerformClick();

					AssertEquals(form.DialogResult, DialogResult.OK);
					AssertEquals(expected: true, formClosed);
				}

				AssertEquals("HasBeenAcknowledged", HasBeenAcknowledged, nzcfta.AgreementInfo.HasBeenAcknowledged);
				AssertEquals("VersionNo", VersionNo, nzcfta.AgreementInfo.VersionNo);
			}

			nzcfta.AgreementInfo = new AgreementInfo();
			using (DocumentsDataRegistry.Instance.EnableCertOfOriginIndemnity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (ObjectFactory.Substitute<ICertificateOfOriginIndemnityTermsAgreementChecker>(new CertificateOfOriginIndemnityTermsAgreementCheckerForTests(new TermAcknowledged(HasBeenAcknowledged, VersionNo))))
				using (var form = new SupportingDocFormForTests(nzcfta))
				{
					var formClosed = false;
					form.FormClosed += (_, __) => { formClosed = true; };
					form.Show();

					form.OkButton.PerformClick();

					AssertEquals(form.DialogResult, DialogResult.OK);
					AssertEquals(expected: true, formClosed);
				}

				AssertNotEquals("HasBeenAcknowledged", HasBeenAcknowledged, nzcfta.AgreementInfo.HasBeenAcknowledged);
				AssertNotEquals("VersionNo", VersionNo, nzcfta.AgreementInfo.VersionNo);
			}
		}

		public void TestForm_OkButton_CheckTermAcknowledgedIsFalse_PopulateNZCFTA_DoNotCloseForm()
		{
			const bool HasBeenAcknowledged = false;
			const int VersionNo = 0;

			var nzcfta = new NZCFTA("ForwardingShipment", "C00001015")
			{
				AgreementInfo = new AgreementInfo()
			};

			using (DocumentsDataRegistry.Instance.EnableCertOfOriginIndemnity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (ObjectFactory.Substitute<ICertificateOfOriginIndemnityTermsAgreementChecker>(new CertificateOfOriginIndemnityTermsAgreementCheckerForTests(new TermAcknowledged(HasBeenAcknowledged, VersionNo))))
				using (var form = new SupportingDocFormForTests(nzcfta))
				{
					var formClosed = false;
					form.FormClosed += (_, __) => { formClosed = true; };
					form.Show();

					form.OkButton.PerformClick();

					AssertEquals(form.DialogResult, DialogResult.None);
					AssertEquals(expected: false, formClosed);
				}

				AssertEquals("HasBeenAcknowledged", HasBeenAcknowledged, nzcfta.AgreementInfo.HasBeenAcknowledged);
				AssertEquals("VersionNo", VersionNo, nzcfta.AgreementInfo.VersionNo);
			}

			using (DocumentsDataRegistry.Instance.EnableCertOfOriginIndemnity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (ObjectFactory.Substitute<ICertificateOfOriginIndemnityTermsAgreementChecker>(new CertificateOfOriginIndemnityTermsAgreementCheckerForTests(new TermAcknowledged(HasBeenAcknowledged, VersionNo))))
				using (var form = new SupportingDocFormForTests(nzcfta))
				{
					var formClosed = false;
					form.FormClosed += (_, __) => { formClosed = true; };
					form.Show();

					form.OkButton.PerformClick();

					AssertEquals(form.DialogResult, DialogResult.OK);
					AssertEquals(expected: true, formClosed);
				}

				AssertEquals("HasBeenAcknowledged", HasBeenAcknowledged, nzcfta.AgreementInfo.HasBeenAcknowledged);
				AssertEquals("VersionNo", VersionNo, nzcfta.AgreementInfo.VersionNo);
			}
		}

		public void TestForm_CancelButton_SetDialogResult()
		{
			var nzcfta = new NZCFTA("ForwardingShipment", "C00001015")
			{
				AgreementInfo = new AgreementInfo()
			};

			using (var form = new SupportingDocFormForTests(nzcfta))
			{
				form.Show();

				form.CancelButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.Cancel);
			}
		}

		public void TestForm_ShowDocSendingObjectsInGrid()
		{
			var nzcfta = new NZCFTA("ForwardingShipment", "C00001015")
			{
				AgreementInfo = new AgreementInfo(),
				DocSendingCollection = GetDocSendingCollection()
			};

			using (var form = new SupportingDocFormForTests(nzcfta))
			{
				form.Show();

				AssertEquals(3, form.SupportingDocumentsGrid.ListManager.Count);
			}
		}

		DocSendingBusinessObjectCollection GetDocSendingCollection()
		{
			var docSendingObjects = new DocSendingBusinessObjectCollection();

			docSendingObjects.Add(new DocSendingBusinessObject
			{
				Id = Guid.NewGuid(),
				Name = "Name1",
				Description = "Description",
				DocumentType = "DocumentType",
				Certify = true,
				Include = true
			});

			docSendingObjects.Add(new DocSendingBusinessObject
			{
				Id = Guid.NewGuid(),
				Name = "Name2",
				Description = "Description",
				DocumentType = "DocumentType",
				Certify = false,
				Include = true
			});

			docSendingObjects.Add(new DocSendingBusinessObject
			{
				Id = Guid.NewGuid(),
				Name = "Name3",
				Description = "Description",
				DocumentType = "DocumentType",
				Certify = false,
				Include = false
			});

			return docSendingObjects;
		}

		#region Implement

		protected override Form GetFormToBashCore()
			=> new SupportingDocForm(new NZCFTA("ForwardingShipment", "C00001015"));

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		#endregion
	}

	public sealed class SupportingDocFormForTests : SupportingDocForm
	{
		public SupportingDocFormForTests(ISupportingDocDataObject supportingDocDataObject) : base(supportingDocDataObject)
		{
		}

		public ZButton OkButton => OkBtn;
		public new ZButton CancelButton => CancelBtn;

		public ZGrid SupportingDocumentsGrid => supportingDocumentsGrid;
	}

	sealed class CertificateOfOriginIndemnityTermsAgreementCheckerForTests : ICertificateOfOriginIndemnityTermsAgreementChecker
	{
		readonly TermAcknowledged termAcknowledged;

		public CertificateOfOriginIndemnityTermsAgreementCheckerForTests(TermAcknowledged termAcknowledged)
		{
			this.termAcknowledged = termAcknowledged;
		}

		public Task<TermAcknowledged> CheckTermAcknowledged(Form parentForm)
			=> Task.FromResult(termAcknowledged);
	}
}
