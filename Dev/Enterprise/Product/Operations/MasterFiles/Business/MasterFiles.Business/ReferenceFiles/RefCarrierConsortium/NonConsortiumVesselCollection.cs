using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class NonConsortiumVesselCollection : RefVesselCollection
	{
		public NonConsortiumVesselCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public NonConsortiumVesselCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		#region Filtering

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = new ZQuery(RefVesselSchema.RV_RG, null);
			query.AddToFilter(base.CreateRelationshipFilter(), JoinCondition.And);
			return query;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			errors.Add(Res.GetString("59b0374f-7e8e-48b4-b1c6-4ad7f195e315", "This vessel already belongs to a consortium."));
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}
