using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.PortHubs.Business
{
	public class PortHubZonePivot : AutoPortHubZonePivot, IPortHubZonePivot
	{
		public PortHubZonePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Objects

		public PortHubSelection PortHub
		{
			get { return Factory.Load<PortHubSelection>(TX_TY_Hub); }
		}

		public RateTransportZone TransportZone
		{
			get { return Factory.Load<RateTransportZone>(TX_TZ_Zone); }
		}

		#region CarrierServiceLevel

		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return TX_PL_NKCarrierServiceLevel.Length > 0 ? Lookups.CarrierServiceLevels.Find(c => c.PL_Code == TX_PL_NKCarrierServiceLevel).FirstOrDefault() : null; }
		}

		#endregion

		#endregion

		#region Properties

		[List("Lookups.RelatedParties")]
		[ResourceStringData("PortHubZonePivot|CarrierPK", ShortCaption = "Zone Owner", Caption = "Local Transport Co/ Zone Owner", FullDescription = "Local Transport Company or Zone Owner")]
		public ZGuid CarrierPK
		{
			get { return (TransportZone != null && TransportZone.TransportProvider != null) ? TransportZone.TransportProvider.TP_OH_RelatedParty : ZGuid.Empty; }
		}

		public OrgHeader Carrier
		{
			get { return Factory.Load<OrgHeader>(CarrierPK); }
		}

		[RelatedBusinessObject("PortHub")]
		public override ZGuid TX_TY_Hub
		{
			get { return base.TX_TY_Hub; }
			set { base.TX_TY_Hub = value; }
		}

		[List("Lookups.TransportZones")]
		[RelatedBusinessObject("TransportZone")]
		public override ZGuid TX_TZ_Zone
		{
			get { return base.TX_TZ_Zone; }
			set { base.TX_TZ_Zone = value; }
		}

		[List("Lookups.CarrierAccounts")]
		[ResourceStringData("PortHubZonePivot|TX_CarrierAccountNumber", ShortCaption = "Carrier Number", Caption = "Carrier Account Number")]
		public override ZString TX_CarrierAccountNumber
		{
			get { return base.TX_CarrierAccountNumber; }
			set { base.TX_CarrierAccountNumber = value; }
		}

		public static PortHubZonePivot GetPortHubZonePivot(BusinessObjectFactory factory, ZString isPickup, ZString serviceLevel, ZGuid depotAddressPk, ZGuid transportZone)
		{
			var portHubSelectionSubQuery = new ZDBOnlySubQuery(typeof(PortHubSelection), PortHubSelectionSchema.PK);
			portHubSelectionSubQuery.AddToFilter(PortHubSelectionSchema.TY_Direction, isPickup);
			portHubSelectionSubQuery.AddToFilter(PortHubSelectionSchema.TY_RS_NKServiceLevel, serviceLevel);
			portHubSelectionSubQuery.AddToFilter(PortHubSelectionSchema.TY_OA_DepotAddress, depotAddressPk);

			var query = new ZDBOnlyQuery(typeof(PortHubZonePivot));
			query.AddToFilter(PortHubZonePivotSchema.TX_TZ_Zone, transportZone);
			query.AddSubQuery(PortHubZonePivotSchema.TX_TY_Hub, PortHubSelectionSchema.PK, portHubSelectionSubQuery, JoinCondition.And);

			return factory.LoadTop1<PortHubZonePivot>(query);
		}

		#endregion
	}
}
