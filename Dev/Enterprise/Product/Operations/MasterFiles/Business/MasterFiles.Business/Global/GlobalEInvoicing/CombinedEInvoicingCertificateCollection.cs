using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public sealed class CombinedEInvoicingCertificateCollection : EInvoicingCertificateCollection<EInvoicingCertificateCredential, BusinessObject>
	{
		public CombinedEInvoicingCertificateCollection(CombinedEInvoicingMaster master, IEInvoicingCertificateCredentialSettings settings)
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

		protected override bool AllowNewCore => IsEditAllowedCore;

		protected override bool AllowRemoveCore => IsEditAllowedCore;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (child is EInvoicingCertificateCredential credentialBizo)
			{
				if (Master is GlbBranch branch)
				{
					credentialBizo.GP_GB = branch.PK;
					credentialBizo.GP_GC = branch.GB_GC;
				}
				else if (Master is GlbCompany company)
				{
					credentialBizo.GP_GB = ZGuid.Empty;
					credentialBizo.GP_GC = company.PK;
				}
			}
		}
	}

	public abstract class EInvoicingCertificateCollection<T, MasterT> : DependentBusinessObjectCollection<T, MasterT>
		where T : EInvoicingCertificateCredential
		where MasterT : BusinessObject
	{
		protected EInvoicingCertificateCollection(MasterT master) : base(master)
		{
		}

		IEInvoicingCertificateCredentialSettings credentialSettingsValue;
		public IEInvoicingCertificateCredentialSettings CredentialSettings
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
					.AddToFilter(new ZQuery(GlbExternalPasswordSchema.GP_Certificate, SQLComparisonOperator.NotEqual, null));

		string PasswordType => CredentialSettings?.PasswordType ?? PasswordTypesList.Codes.EIM;

		public bool IsEnabled => CredentialSettings != null;

		public bool IsEditAllowed => IsEditAllowedCore;

		protected abstract bool IsEditAllowedCore { get; }

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (bizOAdded is EInvoicingCertificateCredential credentialBizo && credentialBizo.CredentialSettings == null)
			{
				credentialBizo.CredentialSettings = CredentialSettings;
			}

			UpdatePreferredSequenceNumberOnChildren();
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			UpdatePreferredSequenceNumberOnChildren();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			UpdatePreferredSequenceNumberOnChildren();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (CredentialSettings != null && child is EInvoicingCertificateCredential credentialBizo)
			{
				credentialBizo.GP_PasswordType = CredentialSettings.PasswordType;
			}
		}

		public void UpdatePreferredSequenceNumberOnChildren()
		{
			if (isUpdatingPreferredSequenceNumber
				|| IsLoading
				|| CredentialSettings == null)
			{
				return;
			}

			try
			{
				isUpdatingPreferredSequenceNumber = true;
				foreach (EInvoicingCertificateCredential credential in this)
				{
					credential.PreferredSequenceNumber = 0;
				}

				var sequence = 1;
				var orderedCredentials = this.Cast<EInvoicingCertificateCredential>()
					.Where(c => c.IsValidCertificate)
					.OrderByDescending(c => c.GP_IssueDate).ThenByDescending(c => c.GP_SystemCreateTimeUtc);
				foreach (var credential in orderedCredentials)
				{
					credential.PreferredSequenceNumber = sequence;
					sequence++;
				}
			}
			finally
			{
				isUpdatingPreferredSequenceNumber = false;
			}
		}

		bool isUpdatingPreferredSequenceNumber;
	}
}
