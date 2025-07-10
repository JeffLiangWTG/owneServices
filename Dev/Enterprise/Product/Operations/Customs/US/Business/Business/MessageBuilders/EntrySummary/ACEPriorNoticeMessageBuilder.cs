using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEPriorNoticeMessageBuilder
	{
		public ACEPriorNoticeMessageBuilder(IStandAlonePriorNoticeHeader header, ZString actionCode)
		{
			this.header = header;
			this.actionCode = actionCode;

			this.block = GetNewABIInputBlockControlGenerator();
			block.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.PriorNotice;
			GenerateBlocks();
		}

		readonly ACEInputBlockControlGenerator block;
		readonly IStandAlonePriorNoticeHeader header;
		readonly ZString actionCode;

		ACEInputBlockControlGenerator GetNewABIInputBlockControlGenerator()
		{
			var processingPortCode = ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(GlbBranch.CurrentBranch);

			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var officeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);

			return new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), processingPortCode, officeCode);
		}

		public void GenerateMessage()
		{
			var message = block.CreateMessage<MQEDIMessage>(header.Factory);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.FDAPriorNotice;
			header.Messages.Add(message);
		}

		public ZString GenerateHumanReadableMessageContent()
		{
			return block.Serialise(true);
		}

		void GenerateBlocks()
		{
			var pe10 = new SEPAPE10();
			pe10.ActionCode = actionCode;
			pe10.FilingType = FilingTypeAdd;
			pe10.ReferenceQualifierCode = header.ReferenceQualifierCode;
			pe10.FilerOrIssuerCodeForReferenceIdentifier = header.FilerOrIssuerCode;
			pe10.ReferenceIdentifierNumber = header.ReferenceIdentifierNumber;
			pe10.BillTypeIndicator = header.BillTypeIndicator;
			pe10.Carrier = header.ImportingCarrierSCAC;
			pe10.EntryType = header.EntryType;
			pe10.ModeOfTransportationMOTCode = header.ModeOfTransportationCode;
			block.AddMessageBlock(pe10);

			foreach (IBillOfLadingDetail bill in header.Bills)
			{
				GeneratePE15BlocksForOneBill(bill);
			}

			var creator = new PGABlocksCreator();
			foreach (var line in header.ACEStandalonePriorNoticeLines)
			{
				block.AddMessageBlock(AEPAPGBlockHelper.MakePGAOI(line.CommercialDescription));

				foreach (MessageBlock fdaBlock in creator.GetFDABlocksForOneLine(line, true))
				{
					block.AddMessageBlock(fdaBlock);
				}
			}
		}
		const string FilingTypeAdd = "A";

		void GeneratePE15BlocksForOneBill(IBillOfLadingDetail bill)
		{
			var billType = bill.BillType;
			var billTypeIndicator = billType == Customs.Business.BillTypeList.Codes.MasterBill
									? (bill.ChildBills.Any() ? SEBillTypesList.Codes.MasterBill : SEBillTypesList.Codes.RegularBill)
									: billType == Customs.Business.BillTypeList.Codes.HouseBill
									? SEBillTypesList.Codes.HouseBill
									: SEBillTypesList.Codes.SubHouseBill;

			if (billType != Customs.Business.BillTypeList.Codes.MasterBill || header.ReferenceQualifierCode == PriorNoticeReferenceQualifierCodeList.Codes.ENT || header.ReferenceQualifierCode == PriorNoticeReferenceQualifierCodeList.Codes.FTZ)
			{
				block.AddMessageBlock(GeneratePE15Block(billTypeIndicator, bill.IssuerCodeOfBillOfLading, bill.BillOfLadingNumber));
			}

			foreach (IBillOfLadingDetail childBill in bill.ChildBills)
			{
				GeneratePE15BlocksForOneBill(childBill);
			}
		}

		SEPAPE15 GeneratePE15Block(ZString billTypeIndicator, ZString issuerCode, ZString billNumber)
		{
			var pe15 = new SEPAPE15();
			pe15.BillTypeIndicator = billTypeIndicator;
			pe15.IssuerCodeOfBillOfLadingNumber = issuerCode;
			pe15.BillOfLadingNumber = billNumber;
			return pe15;
		}
	}
}
