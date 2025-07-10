using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class NZReleaseOrderShipmentValidation : ReleaseImportOrderShipmentValidation
	{
		public NZReleaseOrderShipmentValidation(BillOfLading parent)
			: base(parent) { }

		#region JS_OA_BookedShippingLineAddress

		protected override void CheckJS_OA_BookedShippingLineAddress()
		{
			base.CheckJS_OA_BookedShippingLineAddress();

			if (Parent.BookedShippingLine == null)
			{
				Parent.JS_OA_BookedShippingLineAddressInfo.AddMessageError(Res.GetString("ece6805e-a2ab-11e4-8588-902b34dc814a", "Please enter a carrier."));
			}
			else if (Parent.JS_NKDischargePort.IsEmpty)
			{
				Parent.JS_OA_BookedShippingLineAddressInfo.AddMessageError(Res.GetString("aa463241-a375-4f26-9372-07ff520f76ee", "Please enter a discharge port."));
			}
			else if (Parent.BookedShippingLine.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierPrincipalCode, Parent.JS_NKDischargePort.SubstringSafe(0, 2)).IsEmpty)
			{
				Parent.JS_OA_BookedShippingLineAddressInfo.AddMessageError(Res.GetString("d4c3feb6-b21e-48b7-a847-b4528c9eb5d1", "The carrier does not have a CAR code for {0} entered.", Parent.JS_NKDischargePort.SubstringSafe(0, 2)));
			}
		}

		#endregion

		#region CheckJS_OH_DeliveryAgent

		protected override void CheckJS_OH_DeliveryAgent()
		{
			base.CheckJS_OH_DeliveryAgent();

			if (Parent.JS_OH_DeliveryAgent.IsEmpty)
			{
				Parent.JS_OH_DeliveryAgentInfo.AddMessageError(Res.GetString("ff2d9f88-a2ab-11e4-91b9-902b34dc814a", "Please enter a principal."));
			}
			else if (Parent.JS_NKDischargePort.IsEmpty)
			{
				Parent.JS_OH_DeliveryAgentInfo.AddMessageError(Res.GetString("b4dfb2be-2bf0-4c3f-abe4-afb18e50d911", "Please enter a discharge port."));
			}
			else
			{
				var deliveryAgent = Parent.Factory.Load<OrgHeader>(Parent.JS_OH_DeliveryAgent);
				if (deliveryAgent != null && deliveryAgent.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierPrincipalCode, Parent.JS_NKDischargePort.SubstringSafe(0, 2)).IsEmpty)
				{
					Parent.JS_OH_DeliveryAgentInfo.AddMessageError(Res.GetString("de69b525-867e-4550-a7d4-9a27b9076448", "The principal does not have a CAR code for {0} entered.", Parent.JS_NKDischargePort.SubstringSafe(0, 2)));
				}
			}
		}

		#endregion

		#region CHeckJS_NKDischargePort

		protected override void CheckJS_NKDischargePort()
		{
			base.CheckJS_NKDischargePort();

			if (Parent.Sailing != null && Parent.Sailing.ArrivalCTO == null
				&& (AgencyRegistry.Instance.ImportReleaseOrderPorts.Value.OfType<PortMessagingPort>().FirstOrDefault(x => x.Port == Parent.JS_NKDischargePort)?.Enabled ?? false))
			{
				Parent.JS_NKDischargePortInfo.AddMessageError(Res.GetString("40f5dc70-a1f2-11e4-8070-902b34dc814a", "There is no arrival CTO address specified"));
			}
		}

		#endregion
	}
}





