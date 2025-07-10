using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSBlockControlReaderHelper : IBlockControlReaderHelper
	{
		#region IBlockControlReaderHelper Members

		public bool IsYBlockData(char[] buffer)
		{
			return false;
		}

		public void SetBAndYBlocks(ZString first80, ZString applicationIdentifier, out IControlMessageBlockB b, out IControlMessageBlockY y)
		{
			b = null;
			y = null;
		}

		#endregion
	}
}
