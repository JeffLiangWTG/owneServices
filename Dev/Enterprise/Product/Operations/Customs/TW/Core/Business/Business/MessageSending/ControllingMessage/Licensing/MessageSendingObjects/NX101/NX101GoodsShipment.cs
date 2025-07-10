using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GoodsShipment : IGoodsShipment
	{
		readonly CusTWControllingMessageHeader header;
		readonly ZString declarationNumber;

		public NX101GoodsShipment(CusTWControllingMessageHeader header)
		{
			this.header = header;
			declarationNumber = header.Declaration?.DeclarationNumber ?? ZString.Empty;
		}

		public IEnumerable<IAdditionalDocument> AdditionalDocuments => null;

		public IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems
		{
			get
			{
				var items = header.IsCertificate15 ? header.ControllingMessageHeaderLinkInvoiceLines.Where(x => x.Link).Take(20) : header.ControllingMessageHeaderLinkInvoiceLines.Where(x => x.Link);
				var index = 1;
				foreach (var item in items)
				{
					var invoiceLine = item.Invoiceline;
					var group = invoiceLine.JI_Group;
					var permitGoodsDescriptionExceeds512Part = invoiceLine.NX101PermitGoodsDescription.SubstringSafe(512).Trim();
					if (!group.IsEmpty)
					{
						yield return new NX101GoodsShipmentGovernmentAgencyGoodsItemOnlyMandatory(index++, header, invoiceLine, group);
					}

					yield return new NX101GoodsShipmentGovernmentAgencyGoodsItem(index++, header, invoiceLine);

					if (!permitGoodsDescriptionExceeds512Part.IsEmpty)
					{
						yield return new NX101GoodsShipmentGovernmentAgencyGoodsItemOnlyMandatory(index++, header, invoiceLine, permitGoodsDescriptionExceeds512Part);
					}
				}
			}
		}

		public ZDateTime ExitDateTime => ZDateTime.Empty;

		public ZDecimal ItemChargeAmount => ZDecimal.Zero;

		public ZDecimal TotalCIFAmount => ZDecimal.Zero;

		public IPartyDetails Consignee => null;

		public IConsignment Consignment => new NX101GoodsShipmentConsignment(header);

		public IPartyDetails Consignor => null;

		public ICustomsValuation CustomsValuation => null;

		public ZString DeliveryDestinationName => ZString.Empty;

		public IEnumerable<IGoodsShipmentDutyTaxFee> DutyTaxFees => null;

		public IPartyDetails NotifyParty => null;

		public IPartyDetails Seller => null;

		public ZString TradeTermsConditionCode => ZString.Empty;

		public ZString UCR => ZString.Empty;

		public IPartyDetails Buyer => null;

		public IPartyDetails Exporter => new LicensingMessagePartyDetailsWrapper(header.SupplierDocumentaryAddress, header.IsCertificate15);

		public IEnumerable<IGoodsMeasure> GoodsMeasures => header.IsCertificate15 ? null : header.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().Select(x => x.Invoiceline).GroupBy(x => x.JI_PermitUQ).Select(g => new { g.Key, Value = g.Sum(s => s.JI_PermitQty) }).Select(x => new GoodsMeasure(x.Value, x.Key));

		public IEnumerable<IAdditionalInformation> AdditionalInformations
		{
			get
			{
				if (ShouldOutputAdditionalInformations())
				{
					yield return new AdditionalInformationWrapper(ZString.Empty, ZString.Empty, declarationNumber, ZString.Empty);
				}
			}
		}

		public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => header.IsCertificate15 || declarationNumber.IsEmpty ? null : header.ControllingMessageHeaderLinkInvoiceLines.Where(x => x.InvoicelinePK.IsValid && x.Link && x.Invoiceline.JI_CL.IsValid).Select(x => x.Invoiceline.CusEntryLine).Distinct().OrderBy(x => x.CL_LineNumber).Select(x => new NX101AdditionalDeclarationWrapper((ZDecimal)x.CL_LineNumber, declarationNumber));

		bool ShouldOutputAdditionalInformations()
		{
			var result = false;
			if (!header.IsCertificate15 && header.EntryInstruction?.EntryHeader is CusEntryHeader cusEntryHeader)
			{
				var entryReleaseDate = cusEntryHeader.CH_EntryReleaseDate;
				result = entryReleaseDate.IsValid && entryReleaseDate.AddDays(180) < ZDateTime.Today;
			}
			return result;
		}
	}
}
