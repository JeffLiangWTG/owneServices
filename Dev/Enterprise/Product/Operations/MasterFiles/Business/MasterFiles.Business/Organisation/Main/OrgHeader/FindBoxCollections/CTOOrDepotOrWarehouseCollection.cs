using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class CTOOrDepotOrWarehouseCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public CTOOrDepotOrWarehouseCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CTOOrDepotOrWarehouseCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(new OrgHeaderFilterProvider().GetCTOOrDepotOrWarehouseFilter());
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("0462bea2-d577-4826-b929-2f9f9ba9fd6b", "An Organization selected from here must be a CTO, Depot or Warehouse."));
			}
		}
	}
}
