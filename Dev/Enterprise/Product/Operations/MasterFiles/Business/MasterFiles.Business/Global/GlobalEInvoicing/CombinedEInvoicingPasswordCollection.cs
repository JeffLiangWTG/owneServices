using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// EInvoicing Password Collection for either Branch or Company
	/// </summary>
	public sealed class CombinedEInvoicingPasswordCollection : EInvoicingPasswordCollection<EInvoicingPasswordCredential, BusinessObject>
	{
		public CombinedEInvoicingPasswordCollection(CombinedEInvoicingMaster master, IEInvoicingPasswordCredentialSettings settings)
			: base(master.Value)
		{
			CredentialSettings = settings;
		}

		protected override bool IsEditAllowedCore
		{
			get
			{
				if (CredentialSettings == null)
				{
					return false;
				}

				return Master is GlbBranch
					? CredentialSettings.IsBranchCredentialsRequired
					: CredentialSettings.IsCompanyCredentialsRequired;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (child is EInvoicingPasswordCredential passwordCredential)
			{
				passwordCredential.GP_PasswordType = CredentialSettings.PasswordType;
				if (Master is GlbBranch branch)
				{
					passwordCredential.GP_GB = branch.PK;
					passwordCredential.GP_GC = branch.GB_GC;
				}
				else if (Master is GlbCompany company)
				{
					passwordCredential.GP_GB = ZGuid.Empty;
					passwordCredential.GP_GC = company.PK;
				}
			}
		}
	}

	public abstract class EInvoicingPasswordCollection<T, MasterT> : DependentBusinessObjectCollection<T, MasterT>
		where T : EInvoicingPasswordCredential
		where MasterT : BusinessObject, ILinkable
	{
		protected EInvoicingPasswordCollection(MasterT master) : base(master)
		{
		}

		IEInvoicingPasswordCredentialSettings credentialSettingsValue;
		public IEInvoicingPasswordCredentialSettings CredentialSettings
		{
			get => credentialSettingsValue;
			protected set
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

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			if (Master is GlbCompany)
			{
				query.AddToFilter(GlbExternalPasswordSchema.GP_GB, SQLComparisonOperator.Equal, null);
			}
			return query;
		}

		protected override ZQuery CreateAdditionalFilter()
			=> CredentialSettings == null
				? ZQuery.NoResultQuery
				: new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordType)
					.AddToFilter(new ZQuery(GlbExternalPasswordSchema.GP_Certificate, SQLComparisonOperator.Equal, null));

		string PasswordType => CredentialSettings?.PasswordType ?? PasswordTypesList.Codes.EIM;

		public bool IsEnabled => CredentialSettings != null;

		public bool IsEditAllowed => IsEditAllowedCore;
		protected abstract bool IsEditAllowedCore { get; }

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override void OnLoaded()
		{
			base.OnLoaded();
			EnsureDefinitionsArePresent();
		}

		void EnsureDefinitionsArePresent()
		{
			if (CredentialSettings?.PasswordDefinitions == null)
			{
				return;
			}

			void PopulateMissingDefinitions(IReadOnlyCollection<IEInvoicingPasswordCredentialDefinition> definitions, Dictionary<string, EInvoicingPasswordCredential> credentialLookup)
			{
				foreach (var passwordDefinition in definitions)
				{
					if (!credentialLookup.TryGetValue(passwordDefinition.UniqueKey, out var credential))
					{
						credential = Factory.New<EInvoicingPasswordCredential>();
						credential.CredentialSettings = CredentialSettings;
						credential.CredentialDefinition = passwordDefinition;
						credential.GP_MailBoxID = passwordDefinition.UniqueKey;
						Add(credential);
					}
				}
			}

			void SetOrDeleteCredentialDefinitions(Dictionary<string, IEInvoicingPasswordCredentialDefinition> definitionsLookup)
			{
				foreach (EInvoicingPasswordCredential c in this)
				{
					if (definitionsLookup.TryGetValue(c.GP_MailBoxID, out var definition))
					{
						c.CredentialDefinition = definition;
					}
					else
					{
						RemoveAndDelete(c);
					}
				}
			}

			var definitions = CredentialSettings.PasswordDefinitions;
			var definitionsLookup = definitions.ToDictionary(x => x.UniqueKey);
			var credentialLookup = this.Cast<EInvoicingPasswordCredential>().ToDictionary(x => x.GP_MailBoxID.ToString());

			SetOrDeleteCredentialDefinitions(definitionsLookup);
			PopulateMissingDefinitions(definitions, credentialLookup);

			Sort(nameof(EInvoicingPasswordCredential.DisplayOrder));
		}
	}
}
