using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class DocumentDeliveryRestrictionGUIManager : IDocumentDeliveryRestrictionGUIManager
	{
		bool isAuthenticated;

		public DocumentDeliveryRestrictionGUIManager()
		{
			this.documentDescription = Res.GetString("202dc7f7-5f6c-48f8-b642-c0679efde04a", "Document");
			this.documentCaption = Res.GetString("085deee3-ec0c-411d-88ad-e93a02425e83", "Printing Document");
		}

		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "SetDescription method is used in IDocumentDeliveryRestrictionGUIManager, Spring .NET")]
#pragma warning disable IDE0052 // Remove unread private members
		string documentDescription;
#pragma warning restore IDE0052 // Remove unread private members
		string documentCaption;

		public void SetDescription(string description)
		{
			documentDescription = description;
		}

		public void SetCaption(string caption)
		{
			this.documentCaption = caption;
		}

		public void Initialise(ICreditControlledBusinessObject creditControlledBusinessObject)
		{
			this.CreditControlledDocumentDelivery = creditControlledBusinessObject as ICreditControlledDocumentDelivery;
			if (CreditControlledDocumentDelivery != null)
			{
				CreditControlledDocumentDelivery.GetDocumentLogin += GetDocumentLogin;
				CreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback = new CustomMessageBoxCallback(DocumentLoginMessageBoxCallback);
				creditControlledBusinessObject.InitializeComplianceWorkflowPopupIfNeeded();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CreditControlledDocumentDelivery != null)
				{
					CreditControlledDocumentDelivery.GetDocumentLogin -= GetDocumentLogin;
					CreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback = null;

					if (CreditControlledDocumentDelivery is IComplianceItemRiskStatusProvider complianceRiskStatusProvider)
					{
						complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded = null;
					}
				}
			}
		}

		ICreditControlledDocumentDelivery CreditControlledDocumentDelivery;
		string AuthorisingStaffLogin = string.Empty;

		void GetDocumentLogin(object sender, SecurityLoginEventArgs e)
		{
			if (isAuthenticated)
			{
				e.AuthorisingStaffLogin = AuthorisingStaffLogin;
				e.IsAllowedToProceed = !fIsAuthenticationDeclined;
			}
			else
			{
				new SecurityLoginProvider(documentCaption).ShowDocumentLoginForDocuments(e);
				isAuthenticated = true;
				AuthorisingStaffLogin = e.AuthorisingStaffLogin;
				if (e.IsAllowedToProceed && (!e.AuthorisingStaffLogin.IsEmpty
#if DEBUG
					|| Globals.IsTest
#endif
					))
				{
					fIsAuthenticationDeclined = false;
				}
				else
				{
					fIsAuthenticationDeclined = true;
				}
			}
		}

		ZDialogResult DocumentLoginMessageBoxCallback(ZString message, ZString caption)
		{
			return (ZDialogResult)DocLoginMessageWithPIAInvForm.ShowForm(CreditControlledDocumentDelivery, message, caption);
		}

		bool fIsAuthenticationDeclined;
		public bool IsAuthenticationDeclined
		{
			get { return fIsAuthenticationDeclined; }
		}
	}
}
