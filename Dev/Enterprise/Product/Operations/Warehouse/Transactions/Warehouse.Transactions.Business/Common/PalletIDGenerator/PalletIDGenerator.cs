using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PalletIDGenerator : IPalletIDGenerator
	{
		public PalletIDGenerator(IPalletIDFromSSCCGenerator ssccIDGenerator, IPalletIDFromDocketIDGenerator docketIDGenerator, ISSCCPrefixFinder ssccPrefixFinder)
		{
			SSCCIDGenerator = Argument.NotNull(ssccIDGenerator, nameof(ssccIDGenerator));
			DocketIDGenerator = Argument.NotNull(docketIDGenerator, nameof(docketIDGenerator));
			SSCCPrefixFinder = Argument.NotNull(ssccPrefixFinder, nameof(ssccPrefixFinder));
		}

		public IEnumerable<GeneratedID> GenerateIDs(WhsDocket docket, int numberOfIDs, bool shouldPrompt, int startID = 1)
		{
			Argument.NotNull(docket, nameof(docket));

			var ssccPrefix = ZString.Empty;

			if (!DocketPK.IsEmpty && docket.PK != DocketPK)
			{
				throw new ArgumentException("Should not use PalletIDGenerator with multiple different dockets.");
			}
			else
			{
				DocketPK = docket.PK;
				var client = docket.Client;
				if (client?.MiscServ?.OM_WhsGenerateSSCCOnInbound ?? false)
				{
					ssccPrefix = SSCCPrefixFinder.GetSSCCPrefix(SSCCGenerationContext.GeneratingIDsViaUser, () => client, () => docket.Warehouse, docket.NotificationSubscriber, shouldPrompt);
				}
			}

			return string.IsNullOrEmpty(ssccPrefix)
				? DocketIDGenerator.GenerateIDs(docket, numberOfIDs, startID)
				// SSCC uses a number fountain so Id number is not relevant, and not easy to obtain using the current API
				: SSCCIDGenerator.GenerateIDs(ssccPrefix, numberOfIDs).Select(id => new GeneratedID(id, -1));
		}

		IPalletIDFromSSCCGenerator SSCCIDGenerator { get; }
		IPalletIDFromDocketIDGenerator DocketIDGenerator { get; }
		ISSCCPrefixFinder SSCCPrefixFinder { get; }

		ZGuid DocketPK { get; set; }
	}
}
