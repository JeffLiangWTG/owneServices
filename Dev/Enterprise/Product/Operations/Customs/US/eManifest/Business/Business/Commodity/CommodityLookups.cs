using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CommodityLookups : CusInBondCargoDescLookups
	{
		public CommodityLookups(Commodity parent)
			: base(parent)
		{
		}

		new Commodity Parent
		{
			get { return (Commodity)base.Parent; }
		}

		public IBusinessObjectCollection Equipment
		{
			get { return Parent.Shipment.Trip.AllEquipmentIncludingMainConveyance; }
		}

		public ICodeDescriptionPairList WeightUnits
		{
			get { return Parent.Shipment.Lookups.WeightUnits; }
		}

		public ICodeDescriptionPairList QuantityUnits
		{
			get { return Parent.Shipment.Lookups.QuantityUnits; }
		}

		public IBusinessObjectCollection Countries
		{
			get { return Parent.Shipment.Trip.Lookups.Countries; }
		}

		public new IBusinessObjectCollection Currencies
		{
			get { return Parent.Shipment.Trip.Lookups.Currencies; }
		}

		public IBusinessObjectCollection UNDGSubs
		{
			get { return new UNDGSubstanceCollection(Factory); }
		}

		public IBusinessObjectCollection Contacts
		{
			get { return new OrgContactCollection(Factory); }
		}
	}
}
