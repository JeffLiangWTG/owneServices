using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class CommodityWrapper : ICommodity
	{
		public CommodityWrapper(Commodity commodity)
		{
			this.commodity = commodity;
		}

		#region Implementation of ICommodity

		public ZDecimal CargoGrossWeight
		{
			get { return commodity.BY_GrossWeight; }
		}

		public ZString WeightUnitOfMeasure
		{
			get { return commodity.BY_GrossWeightUnit; }
		}

		public ZString DescriptionOfCargo
		{
			get { return commodity.BY_Description; }
		}

		public ZInt NumberOfPackages
		{
			get { return commodity.BY_PieceCount; }
		}

		public ZString TypeOfPackages
		{
			get { return commodity.BY_ManifestUnitCode; }
		}

		public ZString ShippingMarks
		{
			get { return commodity.BY_MarksAndNumbers; }
		}

		public IEnumerable<ZString> HarmonizedNumbers
		{
			get { return from HarmonizedNumber number in commodity.HarmonizedNumbers select number.CY_Data; }
		}

		public IEnumerable<IHazardousGoods> HazardousGoodsDetails
		{
			get { return from hazmat in commodity.UNDGs select (IHazardousGoods)new HazardousGoodsWrapper(hazmat); }
		}

		public IEnumerable<ZString> VehicleIdentificationNumbers
		{
			get { return from VehicleIdentificationNumber number in commodity.VehicleIdentificationNumbers select number.CY_Data; }
		}

		public IEnumerable<ZString> C4Codes
		{
			get { return from C4Code code in commodity.C4Codes select code.CY_Data; }
		}

		public ZInt CustomsValue
		{
			get { return commodity.BY_MonetaryValue.ToZInt(); }
		}

		public ZString CountryOfOrigin
		{
			get { return commodity.BY_RN_NKCountryOfOrigin; }
		}

		public IEquipment Equipment
		{
			get { return commodity.Equipment == null ? null : new EquipmentWrapper(commodity.Equipment); }
		}

		#endregion

		readonly Commodity commodity;
	}
}
