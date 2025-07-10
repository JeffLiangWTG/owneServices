using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentNumberEntry : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ShipmentNumberEntry(ForwardingShipment shipment)
			: base(shipment.Factory)
		{
			this.Shipment = shipment;
			shipmentNumber = shipment.JS_UniqueConsignRef;

			#region for Testing
#if DEBUG
			if (Globals.IsTest && AutoAcceptNextForTesting)
			{
				Accept();
				AutoAcceptNextForTesting = false;
			}
#endif
			#endregion
		}

		public readonly ForwardingShipment Shipment;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateShipmentNumber();
		}

		#region Accept

		public void Accept()
		{
			fAccepted = true;
			Shipment.JS_UniqueConsignRef = ShipmentNumber;
		}

		#endregion

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get { return shipmentNumber; }
			set
			{
				CheckMaximumLength(ShipmentNumberInfo, value);
				shipmentNumber = value;
				if (!IsValidationSuspended)
				{
					ValidateShipmentNumber();
				}

				ShipmentNumberInfo.RefreshBinding();
			}
		}
		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ShipmentNumber)); }
		}

		public int ShipmentNumber_MaxLength
		{
			get { return Shipment == null ? 0 : Shipment.JS_UniqueConsignRefInfo.MaxLength; }
		}

		public void ValidateShipmentNumber()
		{
			ManualShipmentNumberValidation.ValidateManualShipmentNumber(this, allowBlankShipmentNumber);
		}

		public void AllowBlankShipmentNumber()
		{
			allowBlankShipmentNumber = true;
		}
		bool allowBlankShipmentNumber;

		#endregion

		#region Consignee

		public OrgHeader Consignee
		{
			get { return Shipment != null ? Shipment.Consignee : null; }
		}

		[List("OrgHeaders")]
		public ZGuid ConsigneePK
		{
			get { return Shipment != null ? Shipment.ConsigneePK : ZGuid.Empty; }
		}

		public ZPropertyInfo ConsigneePKInfo
		{
			get { return GetZPropertyInfo(nameof(ConsigneePK)); }
		}

		#endregion

		#region Consignor

		public OrgHeader Consignor
		{
			get { return Shipment != null ? Shipment.Consignor : null; }
		}

		[List("OrgHeaders")]
		public ZGuid ConsignorPK
		{
			get { return Shipment != null ? Shipment.ConsignorPK : ZGuid.Empty; }
		}

		public ZPropertyInfo ConsignorPKInfo
		{
			get { return GetZPropertyInfo(nameof(ConsignorPK)); }
		}

		#endregion

		#region OrgHeaders

		public OrgHeaderCollection OrgHeaders
		{
			get { return orgHeaderCollection ?? (orgHeaderCollection = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection orgHeaderCollection;

		#endregion

		#region ManualShipmentNumberValidation

		ManualShipmentNumberValidation ManualShipmentNumberValidation
		{
			get
			{
				if (fManualShipmentNumberValidation == null)
				{
					fManualShipmentNumberValidation = new ManualShipmentNumberValidation();
				}

				return fManualShipmentNumberValidation;
			}
		}

		ManualShipmentNumberValidation fManualShipmentNumberValidation;

		#endregion

		#region Accepted

		public bool Accepted
		{
			get { return fAccepted; }
		}

		bool fAccepted;

		#endregion

		#region for Testing
#if DEBUG
		[SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Justification = "Only used in testing")]
		[ThreadStatic]
		public static bool AutoAcceptNextForTesting;
#endif
		#endregion
	}
}
