using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingPackLine : ForwardingPackLine
	{
		#region Schema

		public abstract new class Schema : PackLine.Schema
		{
			public const string CurrencyCode = "CurrencyCode";
		}

		#endregion

		public TrackingPackLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public RefCurrency Currency
		{
			get { return (this.Shipment != null) ? this.Shipment.GoodsValueCurr : GlbCompany.CurrentCompany.LocalCurrency; }
		}

		#region CurrencyCode

		public ZString CurrencyCode
		{
			get { return (Currency != null) ? Currency.RX_Code : ZString.Empty; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CurrencyCode); }
		}

		#endregion

		#region Related BusinessObjects

		protected override CommonContainerManyToManyCollection GetNewContainersCollection()
		{
			return new TrackingContainerManyToManyCollection(this);
		}

		protected override CommonShipment GetParentShipment()
		{
			return (TrackingShipment)Factory.Load(typeof(TrackingShipment), JL_JS);
		}

		protected override CommonContainer LoadContainer(ZGuid containerPK)
		{
			return (TrackingContainer)Factory.Load(typeof(TrackingContainer), containerPK);
		}

		#endregion
	}
}
