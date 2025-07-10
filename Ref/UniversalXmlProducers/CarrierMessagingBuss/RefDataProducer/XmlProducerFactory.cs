using System;
using System.Globalization;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer
{
	public sealed class XmlProducerFactory
	{
		public static IXmlProducer CreateProducer(string type, IAccessTokenProvider accessTokenProvider)
		{
			switch (type.ToUpper(CultureInfo.CurrentCulture))
			{
				case "REFACCESSORIAL":
					var httpWebHelper = new HttpWebHelper<ResponseResult<AccessorialInfo[]>>(new HttpClientFactory());
					return new RefAccessorialXmlProducer(httpWebHelper, accessTokenProvider);
				default:
					throw new ArgumentException($"Invalid type: {type}", nameof(type));
			}
		}
	}
}
