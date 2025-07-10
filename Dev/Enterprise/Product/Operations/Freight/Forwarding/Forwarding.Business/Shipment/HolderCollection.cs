using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class HolderCollection : OrganisationsFindBoxCollection
	{
		public HolderCollection(ForwardingShipment shipment) : base(shipment.Factory)
		{
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			errors.Add(Res.GetString("34449a56-ac44-464c-b41e-d17098949b95", "An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded."));
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
