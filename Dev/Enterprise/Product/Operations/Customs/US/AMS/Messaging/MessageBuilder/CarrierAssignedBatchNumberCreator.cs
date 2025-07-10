using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public static class CarrierAssignedBatchNumberCreator
	{
		public static ZString CreateNumber(ZString billOfLading)
		{
			return ZString.Format("{0}_{1}", billOfLading, AMSEDIMessage.AMSMessageNumberPlaceHolder);
		}

		public static ZString GetMessageNumber(IINPM02 inpm02)
		{
			var result = ZString.Empty;
			var regex = new Regex("[a-zA-Z0-9]+_(\\w+)");
			var match = regex.Match(inpm02.CarrierAssignedBatchNumber);
			if (match.Success)
			{
				result = match.Groups[1].Value;
			}
			else
			{
				regex = new Regex("[a-zA-Z0-9]+_" + AMSEDIMessage.AMSMessageNumberPlaceHolder);
				match = regex.Match(inpm02.CarrierAssignedBatchNumber);
				if (match.Success)
				{
					result = AMSEDIMessage.AMSMessageNumberPlaceHolder;
				}
			}
			return result;
		}

		public static ZString GetBillOfLading(IINPM02 inpm02)
		{
			var result = ZString.Empty;
			var regex = new Regex("([a-zA-Z0-9]+)_\\w+");
			var match = regex.Match(inpm02.CarrierAssignedBatchNumber);
			if (match.Success)
			{
				result = match.Groups[1].Value;
			}
			else
			{
				regex = new Regex("([a-zA-Z0-9]+)_" + AMSEDIMessage.AMSMessageNumberPlaceHolder);
				match = regex.Match(inpm02.CarrierAssignedBatchNumber);
				if (match.Success)
				{
					result = match.Groups[1].Value;
				}
			}
			return result;
		}
	}
}
