using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class SwitzerlandGDRNumberAWBHeaderValidator : ExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public SwitzerlandGDRNumberAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable() => IsDepartureFlightInSwitzerlandButNotInBasel;

		#region EH_ECNCRNNumber

		protected override void CheckEH_ECNCRNNumber()
		{
			base.CheckEH_ECNCRNNumber();

			var shipmentExportParent = Parent as ShipmentExportAWBHeader;

			if (Parent.IsHAWB)
			{
				if (!Parent.ShipmentNumberWithEmptyHSCode.IsEmpty && !IsAnyGDRNumberNotEmpty(shipmentExportParent.Shipment))
				{
					AddECNCRNNumberErrorMessage(shipmentExportParent.Shipment.JS_UniqueConsignRef);
				}
			}
			else if (Parent.IsMAWB)
			{
				foreach (var shipment in Parent.Consol.Shipments.OfType<ForwardingShipment>())
				{
					if (shipment.IsAnyPacklineHSCodeEmpty && !IsAnyGDRNumberNotEmpty(shipment))
					{
						AddECNCRNNumberErrorMessage(shipment.JS_UniqueConsignRef);
						break;
					}
				}
			}
		}

		bool IsAnyGDRNumberNotEmpty(ForwardingShipment shipment) => shipment.CusEntryNumbers.OfType<CusEntryNumber>().Any(c => c.CE_EntryType == CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber && !c.CE_EntryNum.IsEmpty);

		void AddECNCRNNumberErrorMessage(string uniqueConsignRef)
		{
			Parent.EH_ECNCRNNumberInfo.AddMessageError(Res.GetString("7f1eec1e-21d5-412b-82d5-d1670b7bc6fd", "For exports from Switzerland by airfreight, to meet RFS (Road Feeder Service) customs filing requirements, either the GDRN (Goods Declaration Reference Number) or HS code(s) are required. The GDRN or HS Code is missing from at least one Packline of {0}.", uniqueConsignRef));
		}

		#endregion

		bool IsDepartureFlightInSwitzerlandButNotInBasel => !Parent.SwitzerlandDepartureFlightCode.IsNullOrEmpty() && Parent.SwitzerlandDepartureFlightCode != "CHBSL";
	}
}
