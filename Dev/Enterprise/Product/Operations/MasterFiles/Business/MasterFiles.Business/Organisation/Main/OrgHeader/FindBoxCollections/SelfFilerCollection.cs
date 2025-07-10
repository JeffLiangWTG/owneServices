using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SelfFilerCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public SelfFilerCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public SelfFilerCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public SelfFilerCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public SelfFilerCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var forwarderSelfFilerQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			forwarderSelfFilerQuery.AddToFilter(OrgMiscServSchema.OM_FWAdvanceCargoReportingSelfFiler, true);

			var consigneeSelfFilerQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			consigneeSelfFilerQuery.AddToFilter(JoinCondition.Or, OrgMiscServSchema.OM_IMAdvanceCargoReportingSelfFiler, true);

			var selectForwarderSelfFilerQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			selectForwarderSelfFilerQuery.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsForwarder, true);
			selectForwarderSelfFilerQuery.AddSubQuery(forwarderSelfFilerQuery, JoinCondition.And);

			var selectConsigneeSelfFilerQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			selectConsigneeSelfFilerQuery.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsConsignee, true);
			selectConsigneeSelfFilerQuery.AddSubQuery(consigneeSelfFilerQuery, JoinCondition.And);

			var query = new ZQuery();
			query.AddToFilter(selectForwarderSelfFilerQuery);
			query.AddToFilter(selectConsigneeSelfFilerQuery, JoinCondition.Or);

			var result = new ZQuery();
			result.AddToFilter(query);
			result.AddToFilter(base.CreateAdditionalFilter());

			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var orgHeader = (OrgHeader)child;
			orgHeader.OH_IsForwarder = true;
			orgHeader.OH_IsConsignee = true;
			orgHeader.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;
			orgHeader.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("DF980AAF-AD13-42D3-ADEC-9717C34E0727", "This organization is not a Self-Filer organization."));
			}
		}
	}
}
