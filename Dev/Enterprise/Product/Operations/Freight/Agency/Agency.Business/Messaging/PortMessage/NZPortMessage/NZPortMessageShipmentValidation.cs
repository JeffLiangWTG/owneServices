using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class NZPortMessageShipmentValidation : JobShipmentValidation
	{
		public NZPortMessageShipmentValidation(BillOfLading shipment) : base(shipment) { }

		#region JS_OA_BookedShippingLineAddress

		protected override void CheckJS_OA_BookedShippingLineAddress()
		{
			base.CheckJS_OA_BookedShippingLineAddress();

			if (Parent.BookedShippingLine == null)
			{
				Parent.JS_OA_BookedShippingLineAddressInfo.AddMessageError(Res.GetString("ece6805e-a2ab-11e4-8588-902b34dc814a", "Please enter a carrier."));
			}
			else if (Parent.BookedShippingLine.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierPrincipalCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).IsEmpty)
			{
				Parent.JS_OA_BookedShippingLineAddressInfo.AddMessageError(Res.GetString("236eb833-6ee5-41ac-ad2b-047b76beba74", "The carrier does not have a CAR code for current country."));
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
			else
			{
				var deliveryAgent = Parent.Factory.Load<OrgHeader>(Parent.JS_OH_DeliveryAgent);
				if (deliveryAgent != null && deliveryAgent.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierPrincipalCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).IsEmpty)
				{
					Parent.JS_OH_DeliveryAgentInfo.AddMessageError(Res.GetString("3aaf1bb3-183e-489d-baa8-176dc90da392", "The principal does not have a CAR code for current country."));
				}
			}
		}

		#endregion

		#region JS_NKDischargePort

		public void ValidateJS_NKDischargePort()
		{
			ValidateCalculatedProperty(Parent.JS_NKDischargePortInfo);
		}

		#endregion

		#region Implementation

		new BillOfLading Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLading)base.Parent; }
		}

		#endregion
	}
}




