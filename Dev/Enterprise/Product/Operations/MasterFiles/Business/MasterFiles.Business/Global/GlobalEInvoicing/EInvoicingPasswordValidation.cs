using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingPasswordValidation : GlbExternalPasswordValidation
	{
		public EInvoicingPasswordValidation(EInvoicingPasswordCredential parent)
			: base(parent)
		{
		}

		protected new EInvoicingPasswordCredential Parent
		{
			get { return (EInvoicingPasswordCredential)base.Parent; }
		}

		public override void ValidateDuplicateConstraint()
		{
			if (!Parent.IsInDatabase
				|| Parent.GP_GSInfo.HasChanges
				|| Parent.GP_GGInfo.HasChanges
				|| Parent.GP_GCInfo.HasChanges
				|| Parent.GP_GBInfo.HasChanges
				|| Parent.GP_PasswordTypeInfo.HasChanges
				|| Parent.GP_MailBoxIDInfo.HasChanges
				|| Parent.GP_UserIDInfo.HasChanges)
			{
				var duplicateQuery = new ZQuery(GlbExternalPasswordSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				duplicateQuery.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GS, Parent.GP_GS);
				duplicateQuery.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GG, Parent.GP_GG);
				duplicateQuery.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GC, Parent.GP_GC);
				duplicateQuery.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GB, Parent.GP_GB);
				duplicateQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, Parent.GP_PasswordType);
				duplicateQuery.AddToFilter(GlbExternalPasswordSchema.GP_UserID, Parent.GP_UserID);
				duplicateQuery.AddToFilter(GlbExternalPasswordSchema.GP_MailBoxID, Parent.GP_MailBoxID);

				if (Parent.Factory.LoadTop1<GlbExternalPassword>(duplicateQuery) != null)
				{
					Parent.AddRowError(DuplicateCredentialFoundMessage);
				}
			}
		}
	}
}
