using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.MasterFiles.Business
{
	class DocRollupOrGroupBestMatcher<T> where T : IDocRollupOrGroupForBestMatcher
	{
		public static T GetBestMatch(IEnumerable<T> list, ZString serviceDirection, ZString transportMode, ZString mode, params ZString[] jobTypesInOrderOfPreference)
		{
			var jobTypes = new List<string>();

			foreach (var jobType in jobTypesInOrderOfPreference)
			{
				if (jobType != OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code)
				{
					jobTypes.Add(jobType);
					if (jobType == JobInvoicingConsumerTypes.Shipment.Code || jobType == JobInvoicingConsumerTypes.Brokerage.Code)
					{
						jobTypes.Add(OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code);
					}
				}
			}

			jobTypes.Add(OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);

			var serviceDirections = new List<string>();
			if (serviceDirection != OrgConstants.ServiceDirection.Code.All)
			{
				serviceDirections.Add(serviceDirection);
			}
			serviceDirections.Add(OrgConstants.ServiceDirection.Code.All);

			var modes = new List<string>();
			if (mode != OrgConstants.ModesForGroupOrSubTotal.Codes.All)
			{
				modes.Add(mode);
				var seaContainerModeList = ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
				if (transportMode == OrgConstants.ModesForGroupOrSubTotal.Codes.Sea && seaContainerModeList.ContainsCode(mode))
				{
					modes.Add(OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
				}
			}
			modes.Add(OrgConstants.ModesForGroupOrSubTotal.Codes.All);

			foreach (var jobTypeCode in jobTypes)
			{
				foreach (var serviceDirectionCode in serviceDirections)
				{
					foreach (var modeCode in modes)
					{
						var result = GetExactMatch(list, jobTypeCode, serviceDirectionCode, modeCode);
						if (result != null)
						{
							return result;
						}
					}
				}
			}

			return default(T);
		}

		public static T GetExactMatch(IEnumerable<T> list, ZString jobType, ZString serviceDirection, ZString mode)
		{
			foreach (T element in list)
			{
				if (element.JobType == jobType
					&& element.TransportMode == mode
					&& (!element.HasServiceDirection || element.ServiceDirection == serviceDirection))
				{
					return element;
				}
			}
			return default(T);
		}
	}
}
