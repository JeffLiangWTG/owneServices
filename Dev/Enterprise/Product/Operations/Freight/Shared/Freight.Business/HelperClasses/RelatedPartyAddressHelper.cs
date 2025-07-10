using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class RelatedPartyAddressHelper
	{
		public static OrgRelatedParty MatchRelatedParty(ZString transportMode, ZString containerMode, ZString location, ZString importCountry, OrgRelatedParty[] parties)
		{
			if (parties.Length == 0)
			{
				return null;
			}

			var comparer = new RelatedPartyComparer(transportMode, containerMode, location, importCountry);
			Array.Sort(parties, comparer);

			return comparer.GetScore(parties[0]) > 0 ? parties[0] : null;
		}

		#region RelatedPartyComparer

		class RelatedPartyComparer : IComparer<OrgRelatedParty>, IComparer
		{
			public RelatedPartyComparer(ZString transportMode, ZString containerMode, ZString location, ZString importCountry)
			{
				this.transportMode = transportMode;
				this.containerMode = containerMode;
				this.location = location;
				this.importCountry = importCountry;
			}

			readonly ZString transportMode;
			readonly ZString location;
			readonly ZString importCountry;
			readonly ZString containerMode;

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
				Company = 128
			}

			#region GetScore

			public int GetScore(OrgRelatedParty party)
			{
				var score = 0;

				if (!party.PR_GC.IsEmpty)
				{
					score |= (int)Scores.Company;
				}

				if (party.PR_FreightTransportMode == Core.Constants.TransportModes.All)
				{
					score |= (int)Scores.TransportModeAll;
				}
				else if (party.PR_FreightTransportMode == transportMode)
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
				else if (party.PR_FreightContainerMode == containerMode)
				{
					score |= (int)Scores.ContainerMode;
				}
				else
				{
					return 0;
				}

				if (party.PR_Location.Length == 5 && party.PR_Location == location)
				{
					score |= (int)Scores.LocationPort;
				}
				else if (party.PR_Location.Length == 2 && party.PR_Location == location.SubstringSafe(0, 2))
				{
					score |= (int)Scores.LocationCountry;
				}
				else if (!party.PR_Location.IsEmpty)
				{
					return 0;
				}

				if (party.PR_RN_NKImporterCountry == importCountry)
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
	}
}
