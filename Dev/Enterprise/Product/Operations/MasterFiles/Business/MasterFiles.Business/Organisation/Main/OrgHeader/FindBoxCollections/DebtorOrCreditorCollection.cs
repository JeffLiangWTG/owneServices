using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DebtorOrCreditorCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public DebtorOrCreditorCollection(BusinessObjectFactory readOnlyFactory)
			: base(readOnlyFactory)
		{
		}

		public DebtorOrCreditorCollection(BusinessObjectFactory readOnlyFactory, ZQuery filter)
			: base(readOnlyFactory, filter)
		{
		}

		public DebtorOrCreditorCollection(BusinessObjectFactory readOnlyFactory, OrganisationDefaults defaults)
			: base(readOnlyFactory, defaults)
		{
		}

		public DebtorOrCreditorCollection(BusinessObjectFactory readOnlyFactory, ZQuery filter, OrganisationDefaults defaults)
			: base(readOnlyFactory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

			ZQuery creditorOrDebtor = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			creditorOrDebtor.AddToFilter(JoinCondition.Or, OrgCompanyDataSchema.OB_IsDebtor, SQLComparisonOperator.Equal, ZBool.True);

			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(creditorOrDebtor);
			query.AddSubQuery(subQuery, JoinCondition.And);

			ZQuery mainQuery = base.CreateAdditionalFilter();
			mainQuery.AddToFilter(query);
			return mainQuery;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("34bd71e5-83f6-45d1-899a-c3e7e06f7e3a", "An Organization selected from here must have an Organization type of Receivables or Payables selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetAtleastOneOrgTypeAsExpectedAndValidate(organisation.CompanyData.OB_IsDebtorInfo, organisation.CompanyData.OB_IsCreditorInfo);
		}

		#endregion
	}
}
