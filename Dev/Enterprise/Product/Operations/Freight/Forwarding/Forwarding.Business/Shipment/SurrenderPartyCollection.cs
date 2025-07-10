using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SurrenderPartyCollection : OrganisationsFindBoxCollection
	{
		public SurrenderPartyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			errors.Add(Res.GetString("4b8a6eea-8cdf-4ff3-b449-1a029c71d1b9", "An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded."));
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));

			var cusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.BoleroTitleRegisterID);
			query.AddSubQuery(cusCodeSubQuery, JoinCondition.And);

			query.AddToFilter(base.CreateAdditionalFilter());

			return query;
		}
	}
}
