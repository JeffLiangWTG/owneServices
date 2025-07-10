using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	static class DefaultCoLoadWithAddressHelper
	{
		internal static ZGuid GetDefaultColoadWithAddressBasedOnSendingAgent(this CommonConsol consol)
		{
			if (consol.SendingForwarder != null)
			{
				var forwarderParties = consol.SendingForwarder.ForwarderRelatedParties
					.Cast<OrgRelatedParty>()
					.Where(p => p.PR_PartyType == RelatedPartyTypeList.Codes.ForwarderCoLoadWith
						&& p.PR_FreightDirection == RelatedPartyDirectionList.Codes.Pickup)
					.ToArray();

				if (forwarderParties.Length > 0)
				{
					var forwarderParty = consol.MatchRelatedParty(forwarderParties);
					if (forwarderParty?.RelatedParty != null)
					{
						return FindRelatedPartyAddress(consol, forwarderParty);
					}
				}
			}

			return ZGuid.Empty;
		}

		#region Implemention

		#region CoLoadWithRelatedPartyComparer

		class CoLoadWithRelatedPartyComparer : IComparer<OrgRelatedParty>, IComparer
		{
			public CoLoadWithRelatedPartyComparer(CommonConsol consol)
			{
				this.consol = consol;
			}
			readonly CommonConsol consol;

			public int Compare(object x, object y)
			{
				return Compare((OrgRelatedParty)x, (OrgRelatedParty)y);
			}

			public int Compare(OrgRelatedParty x, OrgRelatedParty y)
			{
				return GetScore(y) - GetScore(x);
			}

			[WTG.StaticAnalysis.Annotation.CodeAlive("Developer friendly enum")]
			[Flags]
			enum Scores
			{
				ImportCountry = 1,
				LocationCountry = 2,
				LocationPort = 4,
				ContainerModeAll = 8,
				ContainerMode = 16,
				TransportModeAll = 32,
				TransportMode = 64,
			}

			#region GetScore

			public int GetScore(OrgRelatedParty party)
			{
				var score = 0;

				if (party.PR_FreightTransportMode == Core.Constants.TransportModes.All)
				{
					score |= (int)Scores.TransportModeAll;
				}
				else if (party.PR_FreightTransportMode == consol.JK_TransportMode)
				{
					score |= (int)Scores.TransportMode;
				}
				else
				{
					return 0;
				}

				if (party.PR_FreightContainerMode.IsEmpty)
				{
					score |= (int)Scores.ContainerModeAll;
				}
				else if (party.PR_FreightContainerMode == consol.JK_ConsolMode)
				{
					score |= (int)Scores.ContainerMode;
				}
				else
				{
					return 0;
				}

				if (party.PR_Location.Length == 5 && party.PR_Location == consol.JK_RL_NKLoadPort)
				{
					score |= (int)Scores.LocationPort;
				}
				else if (party.PR_Location.Length == 2 && party.PR_Location == consol.JK_RL_NKLoadPort.SubstringSafe(0, 2))
				{
					score |= (int)Scores.LocationCountry;
				}
				else if (!party.PR_Location.IsEmpty)
				{
					return 0;
				}

				if (party.PR_RN_NKImporterCountry == consol.JK_RL_NKDischargePort.SubstringSafe(0, 2))
				{
					score |= (int)Scores.ImportCountry;
				}
				else if (!party.PR_RN_NKImporterCountry.IsEmpty)
				{
					return 0;
				}

				return score;
			}

			#endregion
		}

		#endregion

		static OrgRelatedParty MatchRelatedParty(this CommonConsol consol, OrgRelatedParty[] parties)
		{
			var comparer = new CoLoadWithRelatedPartyComparer(consol);
			Array.Sort(parties, comparer);

			return comparer.GetScore(parties[0]) > 0 ? parties[0] : null;
		}

		static ZGuid FindRelatedPartyAddress(this CommonConsol consol, OrgRelatedParty party)
		{
			var addresses = party.RelatedParty.Addresses.Cast<OrgAddress>()
				.Where(a => a.AddressCapability.GetCapabilityEnabled(party.PR_FreightDirection));
			if (addresses.Count() == 1)
			{
				return addresses.First().PK;
			}
			else
			{
				return consol.GetDefaultAddressPKBasedOnDirectionAndPorts(party.RelatedParty);
			}
		}

		#endregion
	}
}
