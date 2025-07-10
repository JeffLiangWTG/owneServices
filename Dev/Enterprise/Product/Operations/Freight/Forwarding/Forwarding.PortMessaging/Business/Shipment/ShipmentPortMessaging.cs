using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	[ProvideMetaDataProperty("PropertiesReadOnlyState", MetaDataTypes.ReadOnly)]
	public class ShipmentPortMessaging : AutoJobShipmentPortMessaging, IPortMessaging
	{
		public ShipmentPortMessaging(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public static ShipmentPortMessaging Load(ForwardingShipment shipment)
		{
			return shipment.Factory.LoadTop1<ShipmentPortMessaging>(new ZQuery(JobShipmentPortMessagingSchema.JSM_JS_Shipment, shipment.PK));
		}

		public static ShipmentPortMessaging LoadOrCreate(ForwardingShipment shipment)
		{
			var result = Load(shipment);
			if (result == null)
			{
				result = shipment.Factory.New<ShipmentPortMessaging>();
				using (result.SuspendSettingHasChanges())
				{
					result.JSM_JS_Shipment = shipment.PK;
				}
			}

			return result;
		}

		#endregion

		#region Related Business Objects

		[RelatedBusinessObject("Shipment")]
		public override ZGuid JSM_JS_Shipment
		{
			get { return base.JSM_JS_Shipment; }
			set { base.JSM_JS_Shipment = value; }
		}

		public ForwardingShipment Shipment
		{
			get { return Factory.Load<ForwardingShipment>(JSM_JS_Shipment); }
		}

		#endregion

		#region Properties

		[List("Lookups.EntryTypeList")]
		public override ZString JSM_EntryType
		{
			get { return base.JSM_EntryType; }
			set
			{
				if (base.JSM_EntryType != value)
				{
					base.JSM_EntryType = value;
					ResetReadOnlyPropertiesDependingOnEntryType();
					if (PortMessagingHelper.IsEORIAndLRNEffectiveDate() && JSM_EntryType == EntryTypeList.Codes.AE1ExportDeclaration && JSM_LocalReferenceNumber.IsEmpty)
					{
						DefaultLocalReferenceNumberDependingOnEntryType();
					}
				}
			}
		}

		[List("Lookups.ExemptionReasonList")]
		public override ZString JSM_ExemptionReason
		{
			get { return base.JSM_ExemptionReason; }
			set
			{
				base.JSM_ExemptionReason = value;
			}
		}

		[List("Lookups.Annex30ATypeList")]
		public override ZString JSM_Annex30AType
		{
			get { return base.JSM_Annex30AType; }
			set
			{
				base.JSM_Annex30AType = value;
				ResetAnnex30AFailureProcess();
			}
		}

		[MaxLength(18)]
		public override ZString JSM_ExportDeclarationReference
		{
			get { return base.JSM_ExportDeclarationReference; }
			set { base.JSM_ExportDeclarationReference = value; }
		}

		#endregion

		#region Validation

		protected override JobShipmentPortMessagingValidation GetNewValidation()
		{
			var result = base.GetNewValidation();

			if (ShipmentPortMessagingManager.IsValidShipmentForDakosyPortMessaging(Shipment))
			{
				result.Add(new ShipmentPortMessagingForDakosyValidation(this));
			}

			return result;
		}

		#endregion

		#region ReadOnly State

		protected bool GetPropertiesReadOnlyState(PropertyDescriptor property)
		{
			return PortMessagingHelper.GetPropertiesReadOnlyState(this, property);
		}

		#endregion

		#region Implementation

		void ResetReadOnlyPropertiesDependingOnEntryType()
		{
			JSM_ExemptionReason = ZString.Empty;

			if (JSM_ATBNumberInfo.ReadOnly)
			{
				JSM_ATBNumber = ZString.Empty;
			}

			if (JSM_Annex30ATypeInfo.ReadOnly)
			{
				JSM_Annex30AType = ZString.Empty;
			}

			if (JSM_ExportDeclarationReferenceInfo.ReadOnly)
			{
				JSM_ExportDeclarationReference = ZString.Empty;
			}

			if (JSM_MovementReferenceNumberInfo.ReadOnly)
			{
				JSM_MovementReferenceNumber = ZString.Empty;
			}

			if (JSM_MovementReferenceNumberCompleteInfo.ReadOnly)
			{
				JSM_MovementReferenceNumberComplete = ZBool.False;
			}

			if (JSM_LocalReferenceNumberInfo.ReadOnly)
			{
				JSM_LocalReferenceNumber = ZString.Empty;
			}

			if (JSM_LocalReferenceNumberCompleteInfo.ReadOnly)
			{
				JSM_LocalReferenceNumberComplete = ZBool.False;
			}

			ResetAnnex30AFailureProcess();

			if (!IsValidationSuspended)
			{
				Validation.ValidateJSM_ATBNumber();
				Validation.ValidateJSM_ExemptionReason();
				Validation.ValidateJSM_Annex30AType();
				Validation.ValidateJSM_MovementReferenceNumber();
				Validation.ValidateJSM_MovementReferenceNumberComplete();
				Validation.ValidateJSM_ExportDeclarationReference();
				Validation.ValidateJSM_CustomsReleaseDate();
			}
		}

		void ResetAnnex30AFailureProcess()
		{
			if (JSM_Annex30AFailureProcessInfo.ReadOnly)
			{
				JSM_Annex30AFailureProcess = false;
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateJSM_Annex30AFailureProcess();
			}
		}

		void DefaultLocalReferenceNumberDependingOnEntryType()
		{
			var entryNumberFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, JSM_JS_Shipment);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Germany);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_IsValid, true);

			JSM_LocalReferenceNumber = Factory.LoadTop1<CusEntryNumber>(entryNumberFilter)?.CE_EntryNum ?? string.Empty;
		}

		#endregion

		#region IPortMessaging Members

		ZString IPortMessaging.EntryType
		{
			get { return JSM_EntryType; }
		}

		ZString IPortMessaging.MovementReferenceNumber
		{
			get { return JSM_MovementReferenceNumber; }
		}

		ZBool IPortMessaging.MovementReferenceNumberComplete
		{
			get { return JSM_MovementReferenceNumberComplete; }
		}

		ZString IPortMessaging.ATBNumber
		{
			get { return JSM_ATBNumber; }
		}

		ZString IPortMessaging.ExemptionReason
		{
			get { return JSM_ExemptionReason; }
		}

		ZString IPortMessaging.Annex30AType
		{
			get { return JSM_Annex30AType; }
		}

		ZBool IPortMessaging.Annex30AFailureProcess
		{
			get { return JSM_Annex30AFailureProcess; }
		}

		ZString IPortMessaging.ExportDeclarationReference
		{
			get { return JSM_ExportDeclarationReference; }
		}

		ZDateTime IPortMessaging.CustomsReleaseDate
		{
			get { return JSM_CustomsReleaseDate; }
		}

		ZString IPortMessaging.MarksAndNumbers
		{
			get
			{
				return Shipment != null ? Shipment.JS_MarksAndNumbers : ZString.Empty;
			}
		}

		#endregion
	}
}
