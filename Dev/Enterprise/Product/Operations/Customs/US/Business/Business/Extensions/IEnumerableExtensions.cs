using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class IEnumerableExtensions
	{
		public static IEnumerable<IEnumerable<ZString>> ToGroupsOf(this IEnumerable<ZString> aList, int noOfMembersInAGroup)
		{
			var aGroup = new List<ZString>();
			foreach (var element in aList)
			{
				aGroup.Add(element);
				if (aGroup.Count == noOfMembersInAGroup)
				{
					yield return aGroup;
					aGroup = new List<ZString>();
				}
			}
			if (aGroup.Count > 0)
			{
				yield return aGroup;
			}
		}

		public static IEnumerable<ZString> GetValidSCACs(this IEnumerable<OrgHeader> orgs, ZString transportMode)
		{
			var isTruck = transportMode == TransportModeCodes.Codes.TruckContainer
				|| transportMode == TransportModeCodes.Codes.TruckNonContainer
				|| transportMode == TransportTypeList.Codes.Truck;
			foreach (var org in orgs.Distinct())
			{
				foreach (var code in org.USLocalCustomsCarrierCodes(isTruck))
				{
					if (!code.IsEmpty && code.IsValidSCAC(org.Factory, transportMode))
					{
						yield return code;
					}
				}
			}
		}

		public static ZString GetSCAC(this OrgHeader org, ZString transportMode)
		{
			var isTruck = transportMode == TransportModeCodes.Codes.TruckContainer
				|| transportMode == TransportModeCodes.Codes.TruckNonContainer
				|| transportMode == TransportTypeList.Codes.Truck;

			foreach (var code in org.USLocalCustomsCarrierCodes(isTruck))
			{
				if (!code.IsEmpty && code.IsValidSCAC(org.Factory, transportMode))
				{
					return code;
				}
			}
			return ZString.Empty;
		}
	}
}
