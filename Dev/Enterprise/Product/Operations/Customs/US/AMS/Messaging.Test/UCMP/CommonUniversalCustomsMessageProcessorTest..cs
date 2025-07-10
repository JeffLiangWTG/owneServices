using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	public abstract class CommonUniversalCustomsMessageProcessorTest<T> : UniversalCustomsMessageProcessorTest<T> where T : class, IUniversalCustomsMessageProcessor
	{
		protected abstract CommonUniversalCustomsMessageProcessor CreateProcessor();

		protected void AssertUniversalCustomsMessageProcessorCanHandleMessage(EDIMessage message, string expectStatus = EDIMessage.Status.Received)
		{
			using (EnableUCMP())
			{
				var logger = new LoggingInformation();
				var processor = CreateProcessor();
				AssertEquals("Pre-condition: message queued", EDIMessage.Status.Queued, message.EM_Status.ToString());

				var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(message, logger);
				var branch = processor.GetBranch(message, logger, linkedBusinessObjectMetaData.ReturnValue.BranchPk);
				var serializationKeysResult = processor.GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData.ReturnValue);
				if (string.IsNullOrEmpty(serializationKeysResult.DiscardReason))
				{
					message.EM_Status = EDIMessage.Status.PreProcessedOK;
					message.EM_LinkTable = linkedBusinessObjectMetaData.ReturnValue.LinkTableName;
					message.EM_LinkUniqueID = linkedBusinessObjectMetaData.ReturnValue.LinkUniqueID;

					processor.ProcessMessage(message, logger, default);
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Discarded;
				}

				AssertEquals(expectStatus, message.EM_Status.ToString());
			}

			static IDisposable EnableUCMP() => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.UCMPServiceTaskAMS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: true);
		}
	}
}
