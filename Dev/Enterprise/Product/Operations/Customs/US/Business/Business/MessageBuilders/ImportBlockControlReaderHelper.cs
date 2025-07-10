using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.Business
{
	public class ImportBlockControlReaderHelper : IBlockControlReaderHelper
	{
		#region IBlockControlReaderHelper Members

		public bool IsYBlockData(char[] buffer)
		{
			return buffer[0] == 'Y' || (buffer[0] == 'Z' && buffer[1] == 'Z');
		}

		public void SetBAndYBlocks(ZString first80, ZString applicationIdentifier, out IControlMessageBlockB b, out IControlMessageBlockY y)
		{
			if (first80[0] == 'B')
			{
				if (new ACEApplicationIdentifierCodeList().ContainsCode(applicationIdentifier) && !new ApplicationIdentifierCodeList().ContainsCode(applicationIdentifier))
				{
					b = new AABIOutputB();
					y = new AABIOutputY();
				}
				else
				{
					b = new APLB();
					y = new APLY();
				}
			}
			else if (first80.SubstringSafe(0, 2) == "AA")
			{
				b = new BRDAA();
				y = new BRDZZ();
			}
			else
			{
				throw new InvalidMessageFormatException("message does not start with a 'B' or 'AA'");
			}
		}

		#endregion
	}
}
