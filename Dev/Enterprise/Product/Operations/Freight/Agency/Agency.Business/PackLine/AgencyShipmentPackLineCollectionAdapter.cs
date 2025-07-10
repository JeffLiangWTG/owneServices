using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentPackLineCollectionAdapter : BusinessObjectCollection<AgencyShipmentPackLineAdapter>
	{
		public AgencyShipmentPackLineCollectionAdapter(AgencyShipmentContainersView containerCollection)
			: this(containerCollection.CollectionToFilter)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AgencyShipmentPackLineCollectionAdapter(AgencyShipmentContainerDependentCollection containerCollection)
			: base(containerCollection.Factory)
		{
			this.containerCollection = containerCollection;

			foreach (AgencyShipmentContainer container in containerCollection)
			{
				Add(AgencyShipmentPackLineAdapter.New(container));
			}
		}

		readonly AgencyShipmentContainerDependentCollection containerCollection;

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			var adapter = (AgencyShipmentPackLineAdapter)Factory.CreateBusinessObject(row, TypeOfElements, Factory);

			var container = containerCollection.AddNew();
			adapter.Adaptee = container;

			return adapter;
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			if (elementToRemove != null)
			{
				base.Remove(elementToRemove);

				var adaptee = ((AgencyShipmentPackLineAdapter)elementToRemove).Adaptee;

				if (adaptee != null)
				{
					containerCollection.Remove(adaptee);
				}
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			if (businessObject != null)
			{
				base.Add(businessObject);

				var adaptee = ((AgencyShipmentPackLineAdapter)businessObject).Adaptee;

				if (adaptee != null)
				{
					containerCollection.Add(adaptee);
				}
			}
		}
	}
}
