using System.Text;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	static class OutputBlockControlGeneratorTestHelper
	{
		internal static BlockControlGenerator GetBlockControlGenerator(string applicationIdentifier, params string[] blocks)
		{
			BlockControlGenerator result;
			if (new ACEApplicationIdentifierCodeList().ContainsCode(applicationIdentifier))
			{
				result = new ImportOutputBlockControlGenerator<AABIOutputB, AABIOutputY>();
			}
			else if (ApplicationIdentifierCodeList.AES.IsAESApplicationCode(applicationIdentifier))
			{
				switch (applicationIdentifier)
				{
					case ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder:
						result = new ExportOutputBlockControlGenerator<AESCommWarnBXN, AESCommWarnYXN>();
						break;
					default:
						result = new ExportOutputBlockControlGenerator<AESCommShipBXP, AESCommShipYXP>();
						break;
				}
			}
			else
			{
				result = new ABIOutputBlockControlGenerator();
			}

			result.B.ApplicationIdentifier = applicationIdentifier;
			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append(result.B.Serialise());
			foreach (string block in blocks)
			{
				if (block.Length > 80)
				{
					throw new InvalidMessageFormatException("block exceeded 80 characters in length");
				}

				messageBuilder.Append(block.PadRight(80));
			}

			messageBuilder.Append(result.Y.Serialise());
			result.Deserialise(messageBuilder.ToString());
			return result;
		}
	}
}
