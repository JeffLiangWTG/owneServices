using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class BIRDStatusMessageBuilder : MessageBuilder<BIRDInputBlockControlGenerator>
	{
		public BIRDStatusMessageBuilder(IMessageAttacheeInDeclaration messageAttachee)
			: base(messageAttachee)
		{
			additionalBlocks = new List<MessageBlock>();
		}
		readonly List<MessageBlock> additionalBlocks;

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.BIRDTransaction; }
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDStatusRecords;

			JobDeclaration declaration = messageAttachee.TopLevelBusinessObject as JobDeclaration;
			message.EM_Status = declaration.HasBIRDCommunicationMode() ? MQEDIMessage.Status.Pending : MQEDIMessage.Status.Acknowledged;
		}

		public MQEDIMessage BuildENRecord()
		{
			MQEDIMessage result = null;

			ICusEntryHeaderMessageAttachee entryHeader = messageAttachee as ICusEntryHeaderMessageAttachee;

			if (entryHeader != null)
			{
				BRDEN en = new BRDEN();

				en.CorrespondingRefNumber = entryHeader.JobReferenceNumber;
				en.FilerCode = entryHeader.EntryFilerCode;
				en.EntryNumber = ((ICusEntryHeader)entryHeader).EntryNumber;
				en.LocationCode = entryHeader.LocationOfGoods;

				additionalBlocks.Add(en);

				result = PopulateMessage();
			}

			return result;
		}

		public MQEDIMessage BuildTheLatestCargoProcessingResult()
		{
			IMessageAttacheeInDeclaration declaration = (IMessageAttacheeInDeclaration)messageAttachee;

			//RR is attached to declaration
			if (!(declaration is JobDeclaration))
			{
				ErrorReporter.ReportOnce("BIRDStatusMessageBuilder should be built with declaration to attach messages to declaration", "BIRDStatusMessageBuilder should be built with declaration to attach messages to declaration");
			}

			MQEDIMessage message = (MQEDIMessage)declaration.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults, EDIMessage.Direction.Receive);
			additionalBlocks.AddRange(message.MessageBlock.MessageBlocks);

			return PopulateMessage();
		}

		public MQEDIMessage BuildDTRecords()
		{
			return BuildDTRecords(new string[]
			{
				BIRDDateQualifierList.Codes.ArrivalAtFirstPortUnlading,
				BIRDDateQualifierList.Codes.ArrivalAtPortOfEntry,
				BIRDDateQualifierList.Codes.CargoAvailability,
				BIRDDateQualifierList.Codes.CustomsRelease,
				BIRDDateQualifierList.Codes.Delivery,
				BIRDDateQualifierList.Codes.DutyDueDate,
				BIRDDateQualifierList.Codes.DutyPaid,
				BIRDDateQualifierList.Codes.Entry,
				BIRDDateQualifierList.Codes.FreightPaid,
				BIRDDateQualifierList.Codes.Liquidation,
				BIRDDateQualifierList.Codes.PickUp,
				BIRDDateQualifierList.Codes.Statement,
				BIRDDateQualifierList.Codes.SteamshipRelease,
			});
		}

		public MQEDIMessage BuildDTRecords(IEnumerable<string> qualifiers)
		{
			MQEDIMessage result = null;

			ICusEntryHeaderMessageAttachee entryHeader = messageAttachee as ICusEntryHeaderMessageAttachee;

			if (entryHeader != null)
			{
				int index = 1;
				BRDDT dt = null;

				foreach (string qualifier in qualifiers)
				{
					if (dt == null)
					{
						dt = new BRDDT();
					}

					ZDate date = GetDate(entryHeader, qualifier);
					if (!date.IsEmpty)
					{
						switch (index)
						{
							case 1:
								dt.Date1 = date;
								dt.Date1Qualifier = qualifier;
								break;

							case 2:
								dt.Date2 = date;
								dt.Date2Qualifier = qualifier;
								break;

							case 3:
								dt.Date3 = date;
								dt.Date3Qualifier = qualifier;
								break;
						}

						index++;
						if (index > 3)
						{
							index = 1;
							additionalBlocks.Add(dt);
							dt = null;
						}
					}
				}

				result = PopulateMessage();
			}

			return result;
		}

		ZDate GetDate(ICusEntryHeaderMessageAttachee entryHeader, string qualifier)
		{
			JobDeclaration declaration = entryHeader.Factory.Load<JobDeclaration>(entryHeader.DeclarationPK);

			switch (qualifier)
			{
				case BIRDDateQualifierList.Codes.ArrivalAtFirstPortUnlading:
					return entryHeader.DateOfImportation;

				case BIRDDateQualifierList.Codes.ArrivalAtPortOfEntry:
					return entryHeader.EstimatedDateOfArrival;

				case BIRDDateQualifierList.Codes.CustomsRelease:
					return entryHeader.ReleaseDate.Date;

				case BIRDDateQualifierList.Codes.DutyDueDate:
					return declaration.US_PaymentDueDate.Date;

				case BIRDDateQualifierList.Codes.DutyPaid:
					return declaration.US_PaymentDate.Date;

				case BIRDDateQualifierList.Codes.Liquidation:
					return declaration.LiquidationDate.Date;

				case BIRDDateQualifierList.Codes.Statement:
					return declaration.RelatedStatement != null ? declaration.RelatedStatement.B2_PrintDate.Date : declaration.US_PreliminaryStatementPrintDate.Date;

				default:
					return ZDate.Empty;
			}
		}

		protected override void UpdateMessageBlocks(BIRDInputBlockControlGenerator block)
		{
			IMessageAttacheeInDeclaration msgAttachee = (IMessageAttacheeInDeclaration)messageAttachee;

			block.AddMessageBlocks(additionalBlocks);
		}

		protected override BIRDInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			IMessageAttacheeInDeclaration entry = (IMessageAttacheeInDeclaration)messageAttachee;

			JobDeclaration declaration = entry.Factory.Load<JobDeclaration>(entry.DeclarationPK);
			ZString refNo = declaration.US_BRDRefNo.IsEmpty ? declaration.JE_DeclarationReference : declaration.US_BRDRefNo;
			return new BIRDInputBlockControlGenerator(entry, BIRDApplicationCodeList.Codes.Status, refNo.Right(20));
		}
	}
}
