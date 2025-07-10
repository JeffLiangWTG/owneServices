using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class ModeConverter
	{
		public void AddConversion(string freightMode, string customsMode)
		{
			freightToCustoms[freightMode] = customsMode;
			customsToFreight[customsMode] = freightMode;
		}

		public string ConvertFreightToCustoms(string freightMode)
		{
			string result;
			if (freightMode == Enterprise.Core.Constants.ContainerModes.Groupage)
			{
				result = Enterprise.Core.Constants.ContainerModes.LCL;
			}
			else if (!freightToCustoms.TryGetValue(freightMode, out result))
			{
				result = freightMode;
			}
			return result;
		}

		public string ConvertFreightToCustomsForSCNContainer(ZBool isMultiShipmentsPackedIntoAContainer, ZBool isImport, ZString loginCountry)
		{
			string result;
			if (loginCountry == Core.Constants.CountryCodes.Australia)
			{
				if (isImport || isMultiShipmentsPackedIntoAContainer)
				{
					result = Enterprise.Core.Constants.ContainerModes.LCL;
				}
				else
				{
					result = Enterprise.Core.Constants.ContainerModes.FCL;
				}
			}
			else
			{
				if (isMultiShipmentsPackedIntoAContainer)
				{
					result = Enterprise.Core.Constants.ContainerModes.LCL;
				}
				else
				{
					result = Enterprise.Core.Constants.ContainerModes.FCL;
				}
			}

			return result;
		}

		public string ConvertCustomsToFreight(string customsMode)
		{
			string result;
			if (!customsToFreight.TryGetValue(customsMode, out result))
			{
				result = customsMode;
			}
			return result;
		}

		readonly Dictionary<string, string> freightToCustoms = new Dictionary<string, string>();
		readonly Dictionary<string, string> customsToFreight = new Dictionary<string, string>();
	}
}
