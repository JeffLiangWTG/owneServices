using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	class ExpressECIDelegator : IProcessorDelegator
	{
		#region IProcessorDelegator Members

		public string MessageFriendlyName
		{
			get { return "Declaration response"; }
		}

		public bool CanProcess(Declaration.NZCMessage message)
		{
			//ZString sendersReference = message.MessageAsCUSRESD98A.UNH[0].CommonAccessReference;
			//return sendersReference.Length == 9 && sendersReference.Left(1) == NumberFountains.ExpressECIWriteOffReferencePrefix; 

			ZString sendersReference = message.MessageAsCUSRESD98A.UNH[0].CommonAccessReference;
			ZQuery query = new ZQuery(CusMAWBSchema.CM_ApplicationCode, Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff);
			query.AddToFilter(CusMAWBSchema.CM_MessageReference, sendersReference);
			mawb = message.Factory.LoadTop1<CusMAWB>(query);
			return mawb != null;
		}
		CusMAWB mawb;

		public void Process(LoggingInformation logger, Declaration.NZCMessage message)
		{
			mawb.Messages.Add(message);
			MessageProcessor messageProcessor = new ECIWriteOff.Express.MessageProcessor(logger);

			logger.Log("Processing " + MessageFriendlyName + "...");
			message.EM_MessageType = messageProcessor.GetMessageTypeDelegate(message);
			message.EM_MessageSubType = messageProcessor.GetMessageTypeDelegate(message);

			messageProcessor.ProcessMessage(message);
		}

		#endregion
	}
}
