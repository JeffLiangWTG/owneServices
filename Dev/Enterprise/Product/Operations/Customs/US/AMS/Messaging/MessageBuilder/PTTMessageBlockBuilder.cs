using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class PTTMessageBlockBuilder : ACECommonMessageBlockBuilder
	{
		public PTTMessageBlockBuilder(IACEBillManifestMessageAttachee attachee)
			: base(attachee)
		{
		}

		protected override IEnumerable<MessageBlock> BuildCore()
		{
			var messageBlocks = new List<MessageBlock>();
			messageBlocks.AddRange(base.BuildCore());

			messageBlocks.Add(new INPJ01() { IssuerCode = attachee.BillOfLadingDetails.IssuerCode });
			AddPermitToTransferGrouping(messageBlocks, attachee.BillOfLadingDetails);
			return messageBlocks;
		}

		void AddPermitToTransferGrouping(List<MessageBlock> messageBlocks, IACEBillOfLading billOfLading)
		{
			messageBlocks.Add(new PTTT01()
			{
				BillOfLadingSequenceNumber = billOfLading.BillOfLadingSequenceNumber,
				FIRMSCode = billOfLading.FIRMS,
				BondedCarrierID = billOfLading.MovemenDetails.BondedCarrierID
			});

			var inBondQuantity = billOfLading.MovemenDetails.InBondQuantity;
			if (inBondQuantity > ZInt.Zero && billOfLading.ManifestQuantity.ToZInt() != inBondQuantity)
			{
				messageBlocks.Add(new PTTT02()
				{
					PTTQuantity = new ZDecimal(billOfLading.MovemenDetails.InBondQuantity)
				});
			}
		}
	}
}
