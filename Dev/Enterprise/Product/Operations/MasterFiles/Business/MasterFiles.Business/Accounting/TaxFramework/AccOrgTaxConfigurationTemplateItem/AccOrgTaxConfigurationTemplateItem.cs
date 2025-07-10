using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxConfigurationTemplateItem : AccOrgTaxConfiguration
	{
		public new class Schema : AutoAccOrgTaxConfiguration.Schema
		{
			public const string OTC_Code = "OTC_Code";
			public const string OTC_TaxSystem = "OTC_TaxSystem";
			public const string OTC_SuperType = "OTC_SuperType";
			public const string OTC_BranchCode = "OTC_BranchCode";
			public const string OTC_TaxAuthority = "OTC_TaxAuthority";
			public const string OTC_Ledger = "OTC_Ledger";
		}

		public AccOrgTaxConfigurationTemplateItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString OTC_Code => TaxConfiguration?.ETC_Code ?? ZString.Empty;
		public ZString OTC_TaxSystem => TaxConfiguration?.ETC_TaxSystemCode ?? ZString.Empty;
		public ZString OTC_SuperType => TaxConfiguration?.TaxSystem?.TaxSuperType ?? ZString.Empty;
		public ZString OTC_BranchCode => TaxConfiguration?.ParentBranch?.GB_Code ?? ZString.Empty;
		public ZString OTC_TaxAuthority => TaxConfiguration?.ETC_TaxAuthorityCode ?? ZString.Empty;
		public ZString OTC_Ledger => TaxConfiguration?.ETC_Ledger ?? ZString.Empty;

		#region Overrides

		public override ZString Ledger
		{
			get => OrgTaxConfigurationTemplate?.Ledger ?? base.Ledger;
			set => throw new InvalidOperationException("Ledger must not be changed in AccOrgTaxConfigurationTemplateItem.");
		}

		public override GlbCompany GetParentCompany() => OrgTaxConfigurationTemplate?.Company;

		protected override AccOrgTaxConfigurationValidation GetNewValidation() => new AccOrgTaxConfigurationTemplateItemValidation(this);

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (OTC_OCT.IsEmpty)
			{
				var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
				OTC_OCT = template.PK;
			}

			if (OTC_ETC.IsEmpty)
			{
				var config = new BusinessObjectFactory().New<AccTaxConfiguration>();
				if (!Ledger.IsEmpty)
				{
					config.ETC_Ledger = Ledger;
				}
				config.FillWithValidTestData();

				var companyPK = GlbCompany.CurrentCompany.PK;
				// To avoid violate [NR_UC__ETC_ParentId_ETC_Code] unique index in PersistentBusinessObjectTestCase UTs.
				if (Factory.Exists(typeof(AccTaxConfiguration), new ZQuery(AccTaxConfigurationSchema.ETC_ParentId, companyPK)))
				{
					companyPK = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK) { OrderBy = GlbCompanySchema.GC_Code.Name }).PK;
				}

				config.ETC_ParentId = companyPK;
				config.ETC_Code = GlbCompanySchema.Constants.Prefix;
				config.Factory.Save();

				OTC_ETC = config.PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);

			OTC_OB = ZGuid.Empty; // To avoid violate [Constraint_OrgOrTemplate] constraint.
		}
#endif
	}
}
