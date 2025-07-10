using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Customs.ZA.Business.Business.EDIMessage;
using Enterprise.Edifact.D16A;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	class GENRALMessageProcessor : ZACApplicationTypeMessageProcessor
	{
		public GENRALMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "GENRAL Text Messages";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => [SARSEDIMessage.MessageTypes.GENRAL];

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason, MessageHelper Helper) TryFindLinkedObject(EDIMessage message) => (message.EM_GB, null, (NoResString)string.Empty, null);

		public override ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
			=> message.EM_GB;

		public override ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
			=> new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { message.PK.ToStringKey() });

		protected override void ProcessMessageMain(EDIMessage message)
		{
			var d16AbMessageFactory = new D16AMessageFactory();
			var zaCharSet = new ZACharacterSet();

			if (message.GetAutoEdifactMessageUsingNamedFactory(d16AbMessageFactory, zaCharSet) is Edifact.D16A.Messages.GENRAL.GENRALMessage genralMessage)
			{
				message.EM_MessageInterpretation = genralMessage.GetMessageInterpretation();
				message.EM_MessageOwner = genralMessage.Group2[0].NAD[0].PartyIdentificationDetails.PartyIdentifier;
				message.EM_MessageSubType = genralMessage.GetMessageSubType();
				message.EM_Status = EDIMessage.Status.ProcessedOK;
			}
		}
	}
}
