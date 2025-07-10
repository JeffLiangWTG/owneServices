using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class MiscellaneousMessageRequester
	{
		public MiscellaneousMessageRequester(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public MiscellaneousMessageRequester()
			: this(new BusinessObjectFactory())
		{
			autoSave = true;
		}
		readonly BusinessObjectFactory factory;
		readonly bool autoSave;

		public MQEDIMessage RequestStatement(bool isACE, ZString applicationID, MessageBlock qrBlock, ZString processingPortCode)
		{
			var block = isACE ? new ACEInputBlockControlGenerator(GlbBranch.CurrentBranch, processingPortCode) : (BlockControlGenerator)new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch, processingPortCode);
			block.B.ApplicationIdentifier = applicationID;
			block.AddMessageBlock(qrBlock);
			return Finalise(block);
		}

		#region Implementation

		MQEDIMessage Finalise(BlockControlGenerator blockGenerator)
		{
			var result = blockGenerator.CreateMessage<MQEDIMessage>(factory);
			SaveIfAutoSave();
			return result;
		}

		void SaveIfAutoSave()
		{
			if (autoSave)
			{
				factory.Save();
			}
		}

		#endregion

	}
}
