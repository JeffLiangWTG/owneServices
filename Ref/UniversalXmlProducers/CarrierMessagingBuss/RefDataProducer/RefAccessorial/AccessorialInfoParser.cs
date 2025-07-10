using System.Collections.Generic;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer
{
	public sealed class AccessorialInfoParser
	{
		public static List<RefAccessorial> Convert(IEnumerable<ResponseResult<AccessorialInfo[]>> responseResults)
		{
			var data = new List<RefAccessorial>();
			foreach (var responseResult in responseResults)
			{
				foreach (var accessorial in responseResult.Value)
				{
					data.Add(new()
					{
						ASI_Code = accessorial.Code,
						ASI_Description = accessorial.Description
					});
				}
			}
			return data;
		}
	}
}
