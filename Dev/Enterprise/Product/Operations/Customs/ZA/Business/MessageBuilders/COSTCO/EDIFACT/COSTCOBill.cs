using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public class COSTCOBill : ICOSTCOLineLevelInformation
	{
		public COSTCOBill(AsycudaBill bill, ZInt index)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.index = index;
		}

		ZString ICNI_ConsignmentInformation.LineNumber => (index + 1).ToString(CultureInfo.InvariantCulture);

		ZString ICNI_ConsignmentInformation.TransportDocumentNumber => bill.ABL_BillNumber;

		ZString IRFF_CargoCarrierCode.CargoCarrierCode => bill.ABL_BillIssuer;

		ZString IRFF_MasterCargoCarrierCode.MasterCargoCarrierCode => bill.Header?.CarrierCode ?? ZString.Empty;

		ZString IRFF_ExternalReference.ExternalReference => bill.Header?.AMA_JobReference ?? ZString.Empty;

		ZString IRFF_MasterTransportDocumentNumber.MasterBillOfLadingNumber => bill.Header?.MasterBill.ABL_BillNumber ?? ZString.Empty;

		ZString IRFF_MasterTransportDocumentNumber.ConsolidationIndicator
		{
			get
			{
				var agentType = bill.Header?.AMA_AgentType ?? ZString.Empty;
				return agentType == Core.Constants.AgentType.Agent ||
						agentType == Core.Constants.AgentType.CoLoad ||
						agentType == Core.Constants.AgentType.Other
					? COSTCO.Contants.ConsolidationIndicator.Consol
					: COSTCO.Contants.ConsolidationIndicator.Straight;
			}
		}

		ZString IRFF_LRNExit.LRNExit => bill.LRN;

		ZString IRFF_LRNExit.ExportProcedure => bill.CustomsCPC;

		IEnumerable<ICOSTCOPackLineInformation> ICOSTCOLineLevelInformation.Packs => bill.Packs.Cast<AsycudaPack>().Select((x, i) => new COSTCOPack(x, i)).ToList();

		readonly AsycudaBill bill;
		readonly ZInt index;
	}
}
