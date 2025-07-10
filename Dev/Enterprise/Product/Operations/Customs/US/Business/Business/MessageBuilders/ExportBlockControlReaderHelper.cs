using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;

namespace Enterprise.Customs.US.Business
{
	public class ExportBlockControlReaderHelper : IBlockControlReaderHelper
	{
		#region IBlockControlReaderHelper Members

		public bool IsYBlockData(char[] buffer)
		{
			return buffer[0] == 'Y';
		}

		public void SetBAndYBlocks(ZString first80, ZString applicationIdentifier, out IControlMessageBlockB b, out IControlMessageBlockY y)
		{
			if (first80[0] == 'B')
			{
				switch (applicationIdentifier)
				{
					case ApplicationIdentifierCodeList.AES.CommodityShipmentResponse:
						b = new AESCommShipBXT();
						y = new AESCommShipYXT();
						break;
					case ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder:
						b = new AESCommWarnBXN();
						y = new AESCommWarnYXN();
						break;
					default:
						throw new InvalidMessageFormatException("cannot deserialise B and Y blocks for application code 'USE' and message type '" + applicationIdentifier + "'");
				}
			}
			else
			{
				throw new InvalidMessageFormatException("message does not start with a 'B'");
			}
		}

		#endregion
	}
}
