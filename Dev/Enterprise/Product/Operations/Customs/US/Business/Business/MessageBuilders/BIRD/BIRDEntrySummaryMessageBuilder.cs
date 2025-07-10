using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class BIRDEntrySummaryMessageBuilder : EntrySummaryMessageBuilderBase<BIRDInputBlockControlGenerator>
	{
		public BIRDEntrySummaryMessageBuilder(ICusEntryHeaderMessageAttachee entryHeader)
			: base(entryHeader, UpdateActionCode.Add, true)
		{
		}

		protected override BIRDInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			JobDeclaration declaration = entryHeader.Factory.Load<JobDeclaration>(entryHeader.DeclarationPK);

			ZString refNo = declaration.US_BRDRefNo.IsEmpty ? declaration.JE_DeclarationReference : declaration.US_BRDRefNo;
			return new BIRDInputBlockControlGenerator(entryHeader, BIRDApplicationCodeList.Codes.EntrySummary, refNo.Right(20));
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.BIRDTransaction; }
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDEntrySummary;

			JobDeclaration declaration = entryHeader.Factory.Load<JobDeclaration>(entryHeader.DeclarationPK);
			message.EM_Status = declaration.HasBIRDCommunicationMode() ? MQEDIMessage.Status.Pending : MQEDIMessage.Status.Acknowledged;
		}

		protected override EntrySummaryBlockBuilder GetNewEntrySummaryBlockBuilder()
		{
			return new BIRDEntrySummaryBlockBuilder(entryHeader, ApplicationIdentifier);
		}
	}

	public class BIRDEntrySummaryBlockBuilder : EntrySummaryBlockBuilder
	{
		public BIRDEntrySummaryBlockBuilder(ICusEntryHeaderMessageAttachee entryHeader, string applicationIdentifier)
			: base(entryHeader, applicationIdentifier)
		{
		}

		protected override void BuildCore(UpdateActionCode action, bool certifyCargoRelease)
		{
			base.BuildCore(action, certifyCargoRelease);

			IAddressDetails ultimateConsignee = entryHeader.UltimateConsignee;

			if (ultimateConsignee != null)
			{
				EntryHeaderBlock.Add(CreateZA(ultimateConsignee));
				EntryHeaderBlock.Add(CreateZB(ultimateConsignee));
			}

			var declaration = entryHeader.Factory.Load<JobDeclaration>(entryHeader.DeclarationPK);
			if (declaration != null)
			{
				var invoices = declaration.Invoices.Cast<JobComInvoiceHeader>();
				invoices = invoices.OrderBy(x => x.JZ_InvoiceDisplaySequence);

				foreach (JobComInvoiceHeader invoice in invoices)
				{
					EntryHeaderBlock.Add(CreateZI(invoice));
				}
			}

			EntryHeaderBlock.AddRange(new TypedEnumerable<MessageBlock>(CreateZCs()));
		}

		BRDZA CreateZA(IAddressDetails ultimateConsignee)
		{
			BRDZA result = new BRDZA();

			result.ConsigneeName = ultimateConsignee.CompanyName.Left(35);
			result.ConsigneeAddress1 = ultimateConsignee.AddressLine1.Left(35);

			return result;
		}

		BRDZB CreateZB(IAddressDetails ultimateConsignee)
		{
			BRDZB result = new BRDZB();

			result.ConsigneeCity = ultimateConsignee.City.Left(30);
			result.ConsigneeState = ultimateConsignee.State.Left(2);
			result.ConsigneePostalCode = ultimateConsignee.PostCode.Left(9);

			ZString addressline2 = ultimateConsignee.AddressLine1.SubstringSafe(35) + ultimateConsignee.AddressLine2;
			result.ConsigneeAddress2 = addressline2.Left(35);

			return result;
		}

		BRDZI CreateZI(JobComInvoiceHeader invoice)
		{
			BRDZI result = new BRDZI();

			result.InvoiceSequence = invoice.JZ_InvoiceDisplaySequence;
			result.CurrencyCode = invoice.JZ_RX_NKInvoice_Currency;
			result.InvoiceValue = invoice.JZ_InvoiceAmount;
			result.ExchangeRates = invoice.JZ_InvoiceCurrExRate.Round(6);

			return result;
		}

		IEnumerable<BRDZC> CreateZCs()
		{
			List<BRDZC> result = new List<BRDZC>();

			BRDZC zc = null;

			foreach (IContainer container in entryHeader.Containers)
			{
				if (!container.ContainerNumber.IsEmpty)
				{
					if (zc == null)
					{
						zc = new BRDZC();

						result.Add(zc);
					}

					if (zc.ContainerNumber.IsEmpty)
					{
						zc.ContainerNumber = container.ContainerNumber;
						zc.ContainerType = container.ContainerType;
					}
					else if (zc.ContainerNumber2.IsEmpty)
					{
						zc.ContainerNumber2 = container.ContainerNumber;
						zc.ContainerType2 = container.ContainerType;
					}
					else if (zc.ContainerNumber3.IsEmpty)
					{
						zc.ContainerNumber3 = container.ContainerNumber;
						zc.ContainerType3 = container.ContainerType;
					}
					else if (zc.ContainerNumber4.IsEmpty)
					{
						zc.ContainerNumber4 = container.ContainerNumber;
						zc.ContainerType4 = container.ContainerType;
					}
				}
			}

			return result;
		}
	}
}
