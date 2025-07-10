using System;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public interface IRequestContentHeader
	{
		DateTime TransmitionDateTime { get; }
		int RecieverID { get; }
		int SenderID { get; }
	}
}
