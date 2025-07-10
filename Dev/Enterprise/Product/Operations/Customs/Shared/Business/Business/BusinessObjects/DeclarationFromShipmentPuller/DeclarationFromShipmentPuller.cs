using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public class DeclarationFromShipmentPuller : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constants

		public abstract class Schema
		{
			public const string ShipmentPK = "ShipmentPK";
		}

		#endregion

		public DeclarationFromShipmentPuller(BusinessObjectFactory factory) : base(factory)
		{
		}

		public BaseJobDeclaration CreateOrLoadDeclarationForShipment(BusinessObjectFactory factoryToCreateFor)
		{
			return CreateOrLoadDeclarationForShipmentCore(factoryToCreateFor);
		}

		protected virtual BaseJobDeclaration CreateOrLoadDeclarationForShipmentCore(BusinessObjectFactory factoryToCreateFor)
		{
			BaseJobDeclaration result = null;
			ForwardingShipment shipmentInPassedInFactory = factoryToCreateFor.Load<ForwardingShipment>(ShipmentPK);
			if (shipmentInPassedInFactory != null)
			{
				result = BaseJobDeclaration.Load(shipmentInPassedInFactory);

				if (result == null)
				{
					result = shipmentInPassedInFactory.Factory.New<BaseJobDeclaration>();
					result.JE_JS = Shipment.PK;
				}
			}
			return result;
		}

		public ForwardingShipment Shipment
		{
			get { return Factory.Load<ForwardingShipment>(ShipmentPK); }
		}

		public ForwardingShipmentCollection Shipments
		{
			get
			{
				if (fShipments == null)
				{
					fShipments = GetNewForwardingShipmentCollection();
				}

				return fShipments;
			}
		}
		ForwardingShipmentCollection fShipments;

		protected virtual ForwardingShipmentCollection GetNewForwardingShipmentCollection()
		{
			return new ForwardingShipmentCollection(Factory);
		}

		#region ShipmentPK

		ZGuid fShipmentPK;
		public ZGuid ShipmentPK
		{
			get { return fShipmentPK; }
			set
			{
				SetNonPersistentPropertyValue(ShipmentPKInfo, ref fShipmentPK, value);
				ValidateShipmentPK();
			}
		}

		public ZPropertyInfo ShipmentPKInfo
		{
			get { return this.GetZPropertyInfo(Schema.ShipmentPK); }
		}

		public void ValidateShipmentPK()
		{
			ShipmentPKInfo.ClearAllNotifications();

			if (Shipment == null)
			{
				ShipmentPKInfo.AddError(InvalidShipment);
			}
		}

		internal static string InvalidShipment
		{
			get { return Res.GetString("7a7627b2-5a23-45ee-97cf-747d8318dc06", "The specified Shipment does not exist. Please choose a valid Shipment."); }
		}

		#endregion
	}
}
