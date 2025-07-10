using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyUNDGDataItemCollection : UNDGDataItemCollection
	{
		public AgencyUNDGDataItemCollection(IUNDGDataItemProvider master, params OrgHeader[] contactSourceOrganisations) : base(master, contactSourceOrganisations)
		{
		}

		public new AgencyUNDGDataItem this[int index] => (AgencyUNDGDataItem)base[index];

		public new AgencyUNDGDataItem AddNew()
		{
			return (AgencyUNDGDataItem)base.AddNew();
		}

		protected override UNDGDataItemStandAloneCollection GetNewStandaloneCollection()
		{
			return new AgencyUNDGDataItemStandAloneCollection(Factory, typeof(AgencyUNDGDataItem));
		}

		public override UNDGSubstanceCollection AllUNDGSubstances
		{
			get
			{
				var packLine = Relationship.Master as PackLine;
				var parentShipment = packLine?.Shipment;
				var cfrShouldBeDefaulted = AgencyUNDGHelper.CFRShouldBeDefaulted(parentShipment);

				return agencyAllUNDGSubstanceCollection ?? (agencyAllUNDGSubstanceCollection = new AgencyUNDGSubstanceCollection(Factory, cfrShouldBeDefaulted));
			}
		}

		AgencyUNDGSubstanceCollection agencyAllUNDGSubstanceCollection;

		public class AgencyUNDGDataItemStandAloneCollection : UNDGDataItemStandAloneCollection
		{
			public AgencyUNDGDataItemStandAloneCollection(BusinessObjectFactory factory, Type type)
				: base(factory, type)
			{
			}

			public new AgencyUNDGDataItem this[int index] => (AgencyUNDGDataItem)base[index];

			public new AgencyUNDGDataItem AddNew() => (AgencyUNDGDataItem)base.AddNew();
		}
	}
}
