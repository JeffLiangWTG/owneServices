using System.Collections.Generic;
using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseHeaderLookups : ZLookups
	{
		public ReleaseHeaderLookups(ReleaseHeader header)
			: base(header) { }

		#region ReleaseNumbers

		public CodeDescriptionPairList ReleaseNumbers
		{
			get { return releaseNumbers ?? (releaseNumbers = NewReleaseNumbers()); }
		}
		CodeDescriptionPairList NewReleaseNumbers()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			SortedDictionary<string, bool> lookup = new SortedDictionary<string, bool>();
			foreach (AgencyShipmentContainer container in Parent.Shipment.BookedContainers)
			{
				if (!container.JC_ReleaseNum.IsEmpty)
				{
					lookup[container.JC_ReleaseNum] = true;
				}
			}

			foreach (string releaseNum in lookup.Keys)
			{
				result.AddPair(releaseNum);
			}

			return result;
		}
		CodeDescriptionPairList releaseNumbers;

		#endregion

		#region Implementation

		public new ReleaseHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseHeader)base.Parent; }
		}

		#endregion
	}
}


