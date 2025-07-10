using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CreditorCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public CreditorCollection(BusinessObjectFactory readOnlyFactory) : base(readOnlyFactory)
		{
		}

		public CreditorCollection(BusinessObjectFactory readOnlyFactory, bool ignoreCurrentCompanyInFilter) : base(readOnlyFactory)
		{
			IgnoreCurrentCompanyInFilter = ignoreCurrentCompanyInFilter;
		}

		public CreditorCollection(BusinessObjectFactory readOnlyFactory, ZQuery filter) : base(readOnlyFactory, filter)
		{
		}

		public CreditorCollection(BusinessObjectFactory readOnlyFactory, OrganisationDefaults defaults) : base(readOnlyFactory, defaults)
		{
		}

		public CreditorCollection(BusinessObjectFactory readOnlyFactory, ZQuery filter, OrganisationDefaults defaults) : base(readOnlyFactory, filter, defaults)
		{
		}

		bool IgnoreCurrentCompanyInFilter { get; }

		protected override ZQuery CreateAdditionalFilter()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			if (!IgnoreCurrentCompanyInFilter)
			{
				subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			}
			query.AddSubQuery(subQuery, JoinCondition.And);

			ZQuery mainQuery = base.CreateAdditionalFilter();
			mainQuery.AddToFilter(query);
			return mainQuery;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.CompanyData.OB_IsCreditor = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagAP;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property1", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("b7199f23-d9ca-47ad-aad3-16689f0ec406", "An Organization selected from here must have an Organization type of Payables selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.CompanyData.OB_IsCreditorInfo);
		}

		#endregion

	}
}
