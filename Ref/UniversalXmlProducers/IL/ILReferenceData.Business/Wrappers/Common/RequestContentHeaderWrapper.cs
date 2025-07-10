using System;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public sealed class RequestContentHeaderWrapper : IRequestContentHeader
	{
		public RequestContentHeaderWrapper(DateTime transmissionDateTime)
		{
			this.transmissionDateTime = transmissionDateTime;
		}

		DateTime IRequestContentHeader.TransmitionDateTime => transmissionDateTime;

		int IRequestContentHeader.RecieverID => ApplicationConfig.Instance.RecieverID;

		int IRequestContentHeader.SenderID => ApplicationConfig.Instance.SenderID;

		readonly DateTime transmissionDateTime;
	}
}
