using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class EIDOShipmentValidation : ReleaseImportOrderShipmentValidation
	{
		public EIDOShipmentValidation(BillOfLading parent)
			: base(parent) { }

		protected override void CheckJS_NKDischargePort()
		{
			base.CheckJS_NKDischargePort();

			OrgAddress destinationCTO;

			if (Parent.Sailing != null && ImportExportHelper.IsBranchCountry(Parent.JS_NKDischargePort))
			{
				if ((destinationCTO = Parent.Sailing.Destination.ArrivalCTOAddress) == null)
				{
					Parent.JS_NKDischargePortInfo.AddMessageError(Res.GetString("68d5f6dc-b08a-41ac-b45f-2d2a9269a75e", "This discharge port does not have a CTO address specified."));
				}
				else if (GetOneStopCode(destinationCTO).IsEmpty)
				{
					Parent.JS_NKDischargePortInfo.AddMessageError(Res.GetString("be3322f5-450f-40f9-be40-0a645b79d0f7", "The CTO for this discharge port does not have a 1-Stop code."));
				}
			}
		}

		protected override void CheckJS_OH_DeliveryAgent()
		{
			base.CheckJS_OH_DeliveryAgent();

			OrgHeader principal = Parent.Principal;

			if (principal != null && GetOneStopCode(principal).IsEmpty)
			{
				Parent.JS_OH_DeliveryAgentInfo.AddMessageError(Res.GetString("4eddfcd6-dafa-4f33-857e-2990849756c5", "This principal does not have a 1-Stop code."));
			}
		}

		#region Implementation

		ZString GetOneStopCode(OrgAddress address)
		{
			return address == null ? ZString.Empty : GetOneStopCode(address.Header);
		}

		ZString GetOneStopCode(OrgHeader header)
		{
			return header == null ? ZString.Empty : header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.OneStopCode, Constants.CountryCodes.Australia);
		}

		protected new BillOfLading Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Parent; }
		}

		#endregion
	}
}


