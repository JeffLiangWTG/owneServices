using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	internal class ACEOGABlocksCreator
	{
		public IEnumerable<MessageBlock> CreateBlocks(IOGA line, bool certifyCargoRelease)
		{
			OGABlocksCreator fDADOTCreator = new OGABlocksCreator();

			foreach (MessageBlock block in fDADOTCreator.CreateDOTBlocks(line, certifyCargoRelease))
			{
				yield return block;
			}

			foreach (MessageBlock block in fDADOTCreator.CreateFDABlocks(line, certifyCargoRelease))
			{
				yield return block;
			}
		}

		#region Disclaiming Block

		public const string DOTDisclaim = "DT0";
		public const string FDADisclaim = "FD0";

		public MessageBlock CreateDisclaimingBlocksForACSCertification(IOGA line, bool certifyCargoRelease)
		{
			AENSOA result = null;

			if (OGAIndicatorList.IsToBeDisclaimed(line.DOTIndicator) || certifyCargoRelease && OGAIndicatorList.IsToBeDisclaimed(line.FDAIndicator))
			{
				result = new AENSOA();

				if (OGAIndicatorList.IsToBeDisclaimed(line.DOTIndicator))
				{
					result.PGAFormDisclaimerCode1 = DOTDisclaim;
				}

				if (certifyCargoRelease)
				{
					if (OGAIndicatorList.IsToBeDisclaimed(line.FDAIndicator))
					{
						if (result.PGAFormDisclaimerCode1.IsEmpty)
						{
							result.PGAFormDisclaimerCode1 = FDADisclaim;
						}
						else if (result.PGAFormDisclaimerCode2.IsEmpty)
						{
							result.PGAFormDisclaimerCode2 = FDADisclaim;
						}
						else
						{
							result.PGAFormDisclaimerCode3 = FDADisclaim;
						}
					}
				}
			}

			return result;
		}

		#endregion
	}
}
