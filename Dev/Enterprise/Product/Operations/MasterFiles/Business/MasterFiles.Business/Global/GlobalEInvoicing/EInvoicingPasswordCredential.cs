using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingPasswordCredential : GlbExternalPassword
	{
		public EInvoicingPasswordCredential(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		/// <summary>
		/// This defines the whether it is a password credential, but doesn't include details of said password.
		/// It is shared between each EInvoicingPasswordCredential
		/// </summary>
		IEInvoicingPasswordCredentialSettings credentialSettingsValue;
		public IEInvoicingPasswordCredentialSettings CredentialSettings
		{
			get => credentialSettingsValue;
			set
			{
				if (credentialSettingsValue == null)
				{
					credentialSettingsValue = value;
				}
				else
				{
					throw new InvalidOperationException($"{nameof(CredentialSettings)} can only be set once");
				}
			}
		}

		/// <summary>
		/// This defines the detail of the username+password. It is different for each EInvoicingPasswordCredential
		/// </summary>
		IEInvoicingPasswordCredentialDefinition credentialDefinitionValue;
		public IEInvoicingPasswordCredentialDefinition CredentialDefinition
		{
			get => credentialDefinitionValue;
			set
			{
				if (credentialDefinitionValue == null)
				{
					credentialDefinitionValue = value;
				}
				else
				{
					throw new InvalidOperationException($"{nameof(CredentialDefinition)} can only be set once");
				}
			}
		}

		public ZString UsernameLabel => CredentialDefinition.UsernameLabel.ToString();

		public ZString PasswordLabel => CredentialDefinition.PasswordLabel.ToString();

		public ZInt DisplayOrder => CredentialDefinition.DisplayOrder;

		public ZString PasswordStatus
			=> Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		public new EInvoicingCredentialLookups Lookups => (EInvoicingCredentialLookups)base.Lookups;

		protected override GlbExternalPasswordLookups GetNewLookups() => new EInvoicingCredentialLookups(this);

		public new EInvoicingPasswordValidation Validation => (EInvoicingPasswordValidation)GetNewValidation();

		protected override GlbExternalPasswordValidation GetNewValidation() => new EInvoicingPasswordValidation(this);

		public override bool CanDelete => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = CredentialSettings?.PasswordType ?? PasswordTypesList.Codes.EIM;
			GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			GP_GS = ZGuid.Empty;
		}
	}
}
