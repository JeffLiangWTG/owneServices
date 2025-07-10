using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlobalCreditGroupCollection : OrgHeaderCollection
	{
		public GlobalCreditGroupCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlobalCreditGroupCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subquery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH_ARGlobalCreditGroup);
			subquery.AddToFilter(OrgMiscServSchema.OM_OH_ARGlobalCreditGroup, SQLComparisonOperator.NotEqual, DBNull.Value);
			query.AddSubQuery(OrgHeaderSchema.PK, subquery, JoinCondition.And);
			return new ZQuery(query);
		}
		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("C3675FE6-3507-4EAF-81D7-925CAAC3D78C", "This organization is not a Global Credit Group"));
			}
		}
	}
}
