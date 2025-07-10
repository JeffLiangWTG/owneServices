using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class N5203MessageSendingObject : DeclarationMessageSendingObject, IN5203Declaration
	{
		public N5203MessageSendingObject(CusEntryHeader header)
			: base(header)
		{
			declaration = header.Declaration;
			jobComInvoiceLines = new List<JobComInvoiceLine>();
			foreach (CusEntryLine entryLine in Header.AllEntryLines)
			{
				foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
				{
					jobComInvoiceLines.Add(invoiceLine);
				}
			}
		}

		readonly List<JobComInvoiceLine> jobComInvoiceLines;
		readonly JobDeclaration declaration;

		public ZDate AcceptanceDateTime
		{
			get
			{
				var result = entryInstruction.CEI_DateForDuty.Date;
				return result.IsValid ? result : ZDate.Today;
			}
		}

		public ZString Authentication => ZString.Empty;

		public ZString ID => Header.EntryNumberForSendingObject;

		public ZDecimal InvoiceAmount => Header.CH_TotalEXPDisbursedAmountInInvoiceCurrency;

		public ZDecimal TotalGrossMassMeasure => declaration.GrossWeightInKilos();

		public ZInt TotalPackageQuantity => declaration?.JE_TotalNoOfPacks ?? 0;

		public ZString AssociatedGovernmentProcedureCode => entryInstruction.CEI_ExamMode;

		public ZString TypeCode => entryInstruction.CEI_Style;

		public IEnumerable<IAdditionalDocument> AdditionalDocuments
		{
			get
			{
				var attachedDoc1 = entryInstruction.TW_AttachedDoc1;
				var attachedDoc2 = entryInstruction.TW_AttachedDoc2;
				var attachedDoc3 = entryInstruction.TW_AttachedDoc3;

				if (!attachedDoc1.IsEmpty)
				{
					yield return new AdditionalDocumentWrapper(attachedDoc1, 0);
				}

				if (!attachedDoc2.IsEmpty)
				{
					yield return new AdditionalDocumentWrapper(attachedDoc2, 0);
				}

				if (!attachedDoc3.IsEmpty)
				{
					yield return new AdditionalDocumentWrapper(attachedDoc3, 0);
				}
			}
		}

		public IAdditionalInformation AdditionalInformation => GetAdditionalInformation();

		public IEnumerable<ZString> GovernmentProcedureDescriptions => GetGovernmentProcedureDescriptions();

		public IPartyDetails Agent => new Agent(declaration, declaration.DeclarantAddress);

		public ITransportMeans BorderTransportMeans => new TransportMeans(Header);

		public ICurrencyExchange CurrencyExchange => new CurrencyExchange(Header);

		public IDutyTaxFee DutyTaxFee => new DutyTaxFee(Header);

		public IGoodsShipment GoodsShipment => GoodsShipmentCore;

		protected virtual IGoodsShipment GoodsShipmentCore => new GoodsShipment(Header, SupportingDocuments, GetAllEDocs());

		public IDeclarationPackaging Packaging => new Packaging(Header);

		public ZString RepresentativePersonName => Header.Declaration?.CusAgentCertificateNumber ?? ZString.Empty;

		public override ZString GetMessageOwner()
		{
			return GoodsShipment?.Exporter?.ID ?? ZString.Empty;
		}

		[BusinessObjectTestExclude]
		public override ZString MessageType { get => MessageTypeList.Codes.ECD; }

		public override ZString SerializeToMessageString()
		{
			return new N5203MessageBuilder().SerializeToMessageString(this, Action);
		}
	}
}
