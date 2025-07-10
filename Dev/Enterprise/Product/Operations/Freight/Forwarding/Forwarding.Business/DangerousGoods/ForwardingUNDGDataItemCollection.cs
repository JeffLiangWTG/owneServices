using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingUNDGDataItemCollection : UNDGDataItemCollection
	{
		public ForwardingUNDGDataItemCollection(IUNDGDataItemProvider master, params OrgHeader[] contactSourceOrganisations)
			: base(master, contactSourceOrganisations)
		{
		}

		public new ForwardingUNDGDataItem this[int index]
		{
			get
			{
				return (ForwardingUNDGDataItem)base[index];
			}
		}

		public new ForwardingUNDGDataItem AddNew()
		{
			return (ForwardingUNDGDataItem)base.AddNew();
		}

		protected override UNDGDataItemStandAloneCollection GetNewStandaloneCollection()
		{
			return new ForwardingUNDGDataItemStandAloneCollection(Factory, typeof(ForwardingUNDGDataItem), Relationship.Master as ForwardingPackLine);
		}

		// Being used by UNDGDataItemFormManager to get the collection for the grid lookup. The manager is being used everywhere and currently only considers whether or not it is IMO, we will need to return the correct lookup collection here and only affect our module.//
		// Because transport mode of relative shipment could be changed at anytime, so it is better to get it dynamically, not store it to local property of this instance
		public override UNDGSubstanceCollection AllUNDGSubstances
		{
			get
			{
				var packLine = Relationship.Master as ForwardingPackLine;
				if (packLine?.Shipment != null)
				{
					return AddStandardFilter(base.AllUNDGSubstances, packLine.Shipment);
				}
				return base.AllUNDGSubstances;
			}
		}

		// Being used by UNDGDataItemFormManager to get the collection for the grid lookup. The manager is being used everywhere and currently only considers whether or not it is IMO, we will need to return the correct lookup collection here and only affect our module.//
		// Because transport mode of relative shipment could be changed at anytime, so it is better to get it dynamically, not store it to local property of this instance
		public override UNDGSubstanceCollection UNDGSubstances
		{
			get
			{
				var packLine = Relationship.Master as ForwardingPackLine;
				if (packLine?.Shipment != null)
				{
					return AddStandardFilter(base.UNDGSubstances, packLine.Shipment);
				}
				return base.UNDGSubstances;
			}
		}

		UNDGSubstanceCollection AddStandardFilter(UNDGSubstanceCollection collection, ForwardingShipment shipment)
		{
			var standard = DGStandardCalculator.GetCorrespondingStandardForShipmentMode(shipment);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Standard", "Property", standard, true));
			return collection;
		}

		public class ForwardingUNDGDataItemStandAloneCollection : UNDGDataItemStandAloneCollection
		{
			public ForwardingUNDGDataItemStandAloneCollection(BusinessObjectFactory factory, Type type, ForwardingPackLine packLine)
				: base(factory, type)
			{
				parentPackline = packLine;
			}

			readonly ForwardingPackLine parentPackline;

			public new ForwardingUNDGDataItem this[int index]
			{
				get
				{
					return (ForwardingUNDGDataItem)base[index];
				}
			}

			protected override void SetDefaultsForNewElementCore(UNDGDataItem newElement)
			{
				base.SetDefaultsForNewElementCore(newElement);
				((ForwardingUNDGDataItem)newElement).SetParentOnAutoAddedItem(parentPackline);
			}

			public new ForwardingUNDGDataItem AddNew()
			{
				return (ForwardingUNDGDataItem)base.AddNew();
			}

			protected override void OnAdded(UNDGDataItem businessObject)
			{
				base.OnAdded(businessObject);
				((ForwardingUNDGDataItem)businessObject).SetParentOnAutoAddedItem(parentPackline);
			}
		}
	}
}
