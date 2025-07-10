using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.DataTransfer
{
	class ACEBIRDProcessor : BIRDProcessor<BRDAABIB, BRDAABIY>
	{
		internal ACEBIRDProcessor(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		protected override string NoHeaderBlock
		{
			get { return NoBBlock; }
		}
		internal const string NoBBlock = "This file does not have a B record to start an application.";
		protected override string NoFooterBlock
		{
			get { return NoYBlock; }
		}
		internal const string NoYBlock = "This file does not have a Y record to end an application.";

		protected override IEnumerable<string> GetBIRDApplicationIdentifierCodes()
		{
			return ApplicationIdentifierCodeList.GetACEBIRDApplicationIdentifierCodes();
		}

		protected override BlockControlGenerator<BRDAABIB, BRDAABIY> GetMessageBlockGenerator(string birdText, string birdApplicationID)
		{
			switch (birdApplicationID)
			{
				case ACEApplicationIdentifierCodeList.Codes.EntrySummary:
				case ACEApplicationIdentifierCodeList.Codes.CargoRelease:
					return new ImportInputBlockControlGenerator<BRDAABIB, BRDAABIY>();

				default:
					return null;
			}
		}

		protected override void ProcessFromHeaderToFooter(string birdApplicationID, JobDeclaration declaration, BlockControlGenerator<BRDAABIB, BRDAABIY> generator, INotifications notifications)
		{
			switch (birdApplicationID)
			{
				case ACEApplicationIdentifierCodeList.Codes.EntrySummary:
				case ACEApplicationIdentifierCodeList.Codes.CargoRelease:
					ImportDeclarationData(declaration, generator, notifications);
					break;
			}
		}

		protected override string GetApplicationIdentifierCode(BlockControlGenerator<BRDAABIB, BRDAABIY> generator)
		{
			return generator.B.ApplicationIdentifierCode;
		}

		protected override void ImportData(JobDeclaration declaration, BlockControlGenerator<BRDAABIB, BRDAABIY> generator, NotificationCollection declarationNotifications)
		{
			new ACEBIRDDeclarationDataAdapter().DoImport(declaration, (InputBlockControlGenerator<BRDAABIB, BRDAABIY>)generator, declarationNotifications);
		}

		protected override bool ShoudSendAcknowledgementRecord(string applicationCode)
		{
			return applicationCode == ACEApplicationIdentifierCodeList.Codes.EntrySummary || applicationCode == ACEApplicationIdentifierCodeList.Codes.CargoRelease;
		}

		protected override ZString GetAnyReasonForNotAbleToProcessData(BlockControlGenerator<BRDAABIB, BRDAABIY> generator)
		{
			return IsCertifyForACSCargoRelease(generator) ? CannotProcessACSData : (string)base.GetAnyReasonForNotAbleToProcessData(generator);
		}
		internal const string CannotProcessACSData = "Cannot process data that is certified for ACS Cargo Release.";

		bool IsCertifyForACSCargoRelease(BlockControlGenerator<BRDAABIB, BRDAABIY> generator)
		{
			var aens10 = generator.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			return aens10 != null && aens10.CargoReleaseCertificationRequestIndicator == "Y";
		}

		protected override bool ShouldCreateNewDeclaration(string birdApplication)
		{
			return birdApplication == ACEApplicationIdentifierCodeList.Codes.CargoRelease || birdApplication == ACEApplicationIdentifierCodeList.Codes.EntrySummary;
		}

		protected override bool IsHeaderBlock(string oneBlock)
		{
			return oneBlock.Substring(0, 3) == "B  ";
		}

		protected override string GetBIRDApplicationID(string oneBlock)
		{
			return oneBlock.Substring(10, 2).Trim();
		}

		protected override bool IsFooterBlock(string oneBlock)
		{
			return oneBlock.Substring(0, 3) == "Y  ";
		}

		protected override ZString GetBrokerReference(BlockControlGenerator<BRDAABIB, BRDAABIY> generator)
		{
			var brokerRefernceRecord = generator.MessageBlocks.OfType<IBIRDBrokerRefernceRecord>().FirstOrDefault();
			return brokerRefernceRecord == null ? ZString.Empty : brokerRefernceRecord.BrokerReferenceNumber;
		}
	}
}
