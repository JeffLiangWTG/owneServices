using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ForwardingShipmentDocumentSupporterGuiQueryProvider : ShipmentDocumentSupporterGuiQueryProvider, IForwardingShipmentDocumentSupporterQueryProvider
	{
		public static new void Register(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ShipmentDocumentSupporterGuiQueryProvider.Register(factory);
				factory.SetValue<IForwardingShipmentDocumentSupporterQueryProvider, ForwardingShipmentDocumentSupporterGuiQueryProvider>();
			}
		}

		DebtorToSelectFromForPrinting[] IForwardingShipmentDocumentSupporterQueryProvider.GetDebtorsToPrint(DocumentShipment documentShipment)
		{
			DebtorToSelectFromForPrinting[] debtors = null;

			if (documentShipment != null && documentShipment.DebtorsToPrint.Count > 0)
			{
				if (documentShipment.DebtorsToPrint.Count == 1)
				{
					debtors = new[] { documentShipment.DebtorsToPrint[0] };
				}
				else if (documentShipment.DebtorsToPrint.Count > 1 && ZFormModaliser.ShowDialogAndDispose(new DocumentChargeSheet(documentShipment)) == DialogResult.Yes)
				{
					debtors = documentShipment.DebtorsToPrint.Cast<DebtorToSelectFromForPrinting>()
						.Where(debtor => debtor.OH_Calc_PrintDebtor == ZBool.True).ToArray();
				}
			}

			return debtors;
		}

		DocumentImportCargoLabel IForwardingShipmentDocumentSupporterQueryProvider.GetImportCargoLabelToPrint(DocumentImportCargoLabel documentImportCargoLabel)
		{
			if (documentImportCargoLabel != null)
			{
				using (var frm = new DocumentImportCargoForm(documentImportCargoLabel))
				{
					if (ZFormModaliser.ShowDialogAndDispose(frm) == DialogResult.Yes)
					{
						return documentImportCargoLabel;
					}
				}
			}

			return null;
		}

		LetterOfIndemnityOptions IForwardingShipmentDocumentSupporterQueryProvider.GetLetterOfIndemnityOptions(DocumentShipment documentShipment)
		{
			LetterOfIndemnityOptions options = null;

			if (documentShipment != null && ZFormModaliser.ShowDialogAndDispose(new DocumentNewDetailsForm(documentShipment)) == DialogResult.Yes)
			{
				options = new LetterOfIndemnityOptions();

				if (documentShipment.ChangeMarksAndNumbers)
				{
					options.ChangeMarksAndNumbers = documentShipment.ChangeMarksAndNumbers;
					options.NewMarksAndNumbers = documentShipment.NewMarksAndNumbers;
				}

				if (documentShipment.ChangeGoodsDescription)
				{
					options.ChangeGoodsDescription = documentShipment.ChangeGoodsDescription;
					options.NewGoodsDescription = documentShipment.NewGoodsDescription;
				}

				if (documentShipment.ChangeWeight)
				{
					options.ChangeWeight = documentShipment.ChangeWeight;
					options.NewWeight = documentShipment.NewWeight;
					options.NewWeightUnit = documentShipment.NewWeightUnit;
				}

				if (documentShipment.ChangeVolume)
				{
					options.ChangeVolume = documentShipment.ChangeVolume;
					options.NewVolume = documentShipment.NewVolume;
					options.NewVolumeUnit = documentShipment.NewVolumeUnit;
				}
			}

			return options;
		}

		Transport IForwardingShipmentDocumentSupporterQueryProvider.GetTransportToPrint(DocumentShipment documentShipment)
		{
			Transport transport = null;

			if (documentShipment != null)
			{
				if (documentShipment.Shipment.TransportsIncludingRelated.Count == 1)
				{
					transport = documentShipment.Shipment.TransportsIncludingRelated[0];
				}
				else if (documentShipment.Shipment.TransportsIncludingRelated.Count > 1 &&
						 ZFormModaliser.ShowDialogAndDispose(new DocumentSelectTransportForm(documentShipment)) == DialogResult.Yes)
				{
					transport = documentShipment.SelectedTransport;
				}
			}

			return transport;
		}

		ZBool IForwardingShipmentDocumentSupporterQueryProvider.PrintAWBBarcodeLabel()
		{
			return true;
		}

		ZBool IForwardingShipmentDocumentSupporterQueryProvider.PrintShiLianDan(ForwardingShipment shipment)
		{
			if (shipment != null)
			{
				var shipmentSLDEntryNum = shipment.Numbers.GetFirstReferenceNumberByType(ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber);
				var shipmentSLD = shipmentSLDEntryNum != null ? shipmentSLDEntryNum.CE_EntryNum : ZString.Empty;

				if (!shipmentSLD.IsEmpty
					&& shipment.OuterPackLines.Cast<PackLine>().Any(p => !p.JL_ExportRefNumber.IsEmpty && p.JL_ExportRefNumber != shipmentSLD))
				{
					return Globals.Message.Show(Res.GetString("f038532d-e3fe-4c6b-91c1-e3a27aea9da7", "SLD number has been entered on the Shipment and also on the pack lines. The Shi Lian Dan will only use the SLD entered at the shipment level (refer to References > SLD type). If you’d like to print the document per Shi Lian Dan number at the pack level, remove the SLD from the Shipment and enter the number on each packline using the Shipping Order/Shi Lian Dan column."),
						Res.GetString("da85dbf1-9051-4fd4-846d-e7b47520632c", "{0}: Print Shi Lian Dan", shipment.HumanReadableName),
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Question,
						DialogResult.OK) == DialogResult.OK;
				}
				else if (shipmentSLD.IsEmpty
					&& shipment.OuterPackLines.Cast<PackLine>().Any(p => p.JL_ExportRefNumber.IsEmpty))
				{
					return Globals.Message.Show(Res.GetString("3e8ea15e-06ee-4b4c-8ff4-cad3f63ec2c9", "At least one packline has no Shipping Order/Shi Lian Dan number. The Shi Lian Dan document will only be issued for the packs where the Shipping Order/Shi Lian Dan number exists."),
						Res.GetString("db5c993a-3d11-4ffa-a699-3dfd5d2fc0be", "{0}: Print Shi Lian Dan", shipment.HumanReadableName),
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Question,
						DialogResult.OK) == DialogResult.OK;
				}
			}

			return true;
		}
	}
}
