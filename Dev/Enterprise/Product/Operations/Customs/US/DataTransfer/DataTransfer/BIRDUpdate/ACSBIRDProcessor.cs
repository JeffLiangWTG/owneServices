using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.DataTransfer
{
	class ACSBIRDProcessor : BIRDProcessor<BRDAA, BRDZZ>
	{
		internal ACSBIRDProcessor(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		protected override bool IsHeaderBlock(string oneBlock)
		{
			return oneBlock.Substring(0, 2) == "AA";
		}

		protected override string GetBIRDApplicationID(string oneBlock)
		{
			return oneBlock.Substring(2, 4).Trim();
		}

		protected override bool IsFooterBlock(string oneBlock)
		{
			return oneBlock.Substring(0, 2) == "ZZ";
		}

		protected override string NoHeaderBlock
		{
			get { return NoAABlock; }
		}
		internal const string NoAABlock = "This file does not have an AA record to start an application.";
		protected override string NoFooterBlock
		{
			get { return NoZZBlock; }
		}
		internal const string NoZZBlock = "This file does not have a ZZ record to end an application.";

		protected override IEnumerable<string> GetBIRDApplicationIdentifierCodes()
		{
			return ApplicationIdentifierCodeList.GetBIRDApplicationIdentifierCodes();
		}

		protected override BlockControlGenerator<BRDAA, BRDZZ> GetMessageBlockGenerator(string birdText, string birdApplicationID)
		{
			switch (birdApplicationID)
			{
				case BIRDApplicationCodeList.Codes.EntrySummary:
				case BIRDApplicationCodeList.Codes.CargoRelease:
				case BIRDApplicationCodeList.Codes.EntrySummaryQueryInput:
					return new ImportInputBlockControlGenerator<BRDAA, BRDZZ>();

				case BIRDApplicationCodeList.Codes.Status:
				case BIRDApplicationCodeList.Codes.CourtesyNoticeOfLiquidation:
				case BIRDApplicationCodeList.Codes.EntrySummaryQueryOutput:
					return new ImportOutputBlockControlGenerator<BRDAA, BRDZZ>();

				default:
					return null;
			}
		}

		protected override void ProcessFromHeaderToFooter(string birdApplicationID, JobDeclaration declaration, BlockControlGenerator<BRDAA, BRDZZ> generator, INotifications notifications)
		{
			switch (birdApplicationID)
			{
				case BIRDApplicationCodeList.Codes.EntrySummary:
				case BIRDApplicationCodeList.Codes.CargoRelease:
					ImportDeclarationData(declaration, generator, notifications);
					break;

				case BIRDApplicationCodeList.Codes.EntrySummaryQueryInput:
					SendEntrySummaryQueryToCustoms(declaration, generator);
					break;

				case BIRDApplicationCodeList.Codes.CourtesyNoticeOfLiquidation:
				case BIRDApplicationCodeList.Codes.EntrySummaryQueryOutput:
					ConvertIntoQueuedMQMessageToBeProcessed(declaration, ApplicationIdentifierCodeList.GetApplicationIdentifierCodeFrom(generator.B.ApplicationCode), generator);
					break;

				case BIRDApplicationCodeList.Codes.Status:
					bool isRR = generator.MessageBlocks.Exists(x => x.GetType() == typeof(Business.MessageBuildingBlocks.Output.CRLR1));
					if (isRR)
					{
						ConvertIntoQueuedMQMessageToBeProcessed(declaration, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults, generator);
					}
					else
					{
						ImportStatus(declaration, generator, notifications);
					}
					break;
			}
		}

		#region Convert into MQMessage

		void ConvertIntoQueuedMQMessageToBeProcessed(JobDeclaration declaration, string abiApplicationID, BlockControlGenerator<BRDAA, BRDZZ> generator)
		{
			var aaRecord = generator.B;

			var abiMessage = new ABIOutputBlockControlGenerator();
			abiMessage.AddMessageBlocks(generator.MessageBlocks);

			abiMessage.B.ApplicationIdentifier = abiApplicationID;
			abiMessage.B.EntryFilerCode = aaRecord.SendersFilerCode;
			abiMessage.B.UserData = aaRecord.UserArea;

			var message = abiMessage.CreateMessage<MQEDIMessage>(declaration.Factory);
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_Status = MQEDIMessage.Status.Queued;
			message.EM_MessageOwner = aaRecord.UserArea;
			message.EM_ApplicationReference = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
		}

		void SendEntrySummaryQueryToCustoms(JobDeclaration declaration, BlockControlGenerator<BRDAA, BRDZZ> generator)
		{
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			if (entry != null)
			{
				var ji = (BRDJ1)generator.MessageBlocks.Find(x => x.GetType() == typeof(BRDJ1));

				if (ji != null)
				{
					var message = new EntrySummaryQueryMessageBuilder(entry, ji.CollectionBillCode).PopulateMessage();
					message.EM_MessageOwner = generator.B.UserArea;
					message.EM_ApplicationReference = ApplicationIdentifierCodeList.Codes.BIRDTransaction;
				}
			}
		}

		#endregion

		#region Status

		void ImportStatus(JobDeclaration declaration, BlockControlGenerator<BRDAA, BRDZZ> generator, INotifications notifications)
		{
			NotificationCollection declarationNotifications = new NotificationCollection();

			foreach (MessageBlock block in generator.MessageBlocks)
			{
				IBIRDStatusRecord statusRecord = block as IBIRDStatusRecord;

				if (statusRecord != null)
				{
					statusRecord.Update(declaration, declarationNotifications);
				}
			}

			if (declarationNotifications.Count > 0)
			{
				notifications.AddRange(declarationNotifications);
			}
		}

		#endregion

		protected override void ImportData(JobDeclaration declaration, BlockControlGenerator<BRDAA, BRDZZ> generator, NotificationCollection declarationNotifications)
		{
			new BIRDDeclarationDataAdapter().DoImport(declaration, (InputBlockControlGenerator<BRDAA, BRDZZ>)generator, declarationNotifications);
		}

		protected override ZString GetBrokerReference(BlockControlGenerator<BRDAA, BRDZZ> generator)
		{
			return generator.B.OriginatingBrokerRef;
		}

		protected override string GetApplicationIdentifierCode(BlockControlGenerator<BRDAA, BRDZZ> generator)
		{
			return generator.B.ApplicationCode;
		}

		protected override bool ShoudSendAcknowledgementRecord(string applicationCode)
		{
			return applicationCode == BIRDApplicationCodeList.Codes.EntrySummary || applicationCode == BIRDApplicationCodeList.Codes.CargoRelease;
		}

		protected override bool ShouldCreateNewDeclaration(string birdApplication)
		{
			return birdApplication == BIRDApplicationCodeList.Codes.EntrySummary || birdApplication == BIRDApplicationCodeList.Codes.CargoRelease;
		}
	}
}
