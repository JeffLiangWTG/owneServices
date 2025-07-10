using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgARTermsInstallment : AutoOrgARTermsInstallment
	{
		public OrgARTermsInstallment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override void Delete()
		{
			OrgARTerms parentARTerm = null;
			if (!IsDeleted && Terms != null && !Terms.IsDeleted)
			{
				parentARTerm = Terms;
			}

			base.Delete();

			if (parentARTerm != null)
			{
				parentARTerm.MarkAsNeedingValidation();
			}
		}

		protected override OrgARTermsInstallmentValidation GetNewValidation()
		{
			return new OrgARTermsInstallmentValidation(this);
		}

		protected override OrgARTermsInstallmentLookups GetNewLookups()
		{
			return new OrgARTermsInstallmentLookups(this);
		}

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (Terms != null && Terms.CompanyData != null && Terms.CompanyData.Header != null)
			{
				shouldBeReadOnly = Terms.CompanyData.Header.IsInDatabase && !Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		[List("Lookups.AgreedPaymentMethodList")]
		public override ZString ML_AgreedPaymentMethod
		{
			get { return base.ML_AgreedPaymentMethod; }
			set { base.ML_AgreedPaymentMethod = value; }
		}
		#endregion

		protected bool ML_SequenceNumber_ReadOnly => true;
	}
}
