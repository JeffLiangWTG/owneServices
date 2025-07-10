using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DebtorCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public DebtorCollection(BusinessObjectFactory readOnlyFactory) : base(readOnlyFactory)
		{
		}

		public DebtorCollection(BusinessObjectFactory readOnlyFactory, ZQuery filter) : base(readOnlyFactory, filter)
		{
		}

		public DebtorCollection(BusinessObjectFactory readOnlyFactory, OrganisationDefaults defaults) : base(readOnlyFactory, defaults)
		{
		}

		public DebtorCollection(BusinessObjectFactory readOnlyFactory, ZQuery filter, OrganisationDefaults defaults) : base(readOnlyFactory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			ZQuery mainQuery = base.CreateAdditionalFilter();
			mainQuery.AddToFilter(query);
			return mainQuery;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.CompanyData.OB_IsDebtor = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagAR;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property0", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("743ee549-c532-4fc8-95f6-039bedee78a7", "An Organization selected from here must have an Organization type of Receivables selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.CompanyData.OB_IsDebtorInfo);
		}

		#endregion

	}
}
