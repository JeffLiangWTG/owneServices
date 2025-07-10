using System;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

sealed class TaricUpdateCollector(IMessageSender messageSender) : ITaricUpdateCollector
{
	readonly IMessageSender messageSender = messageSender;

	public byte[] GenerateAndSendTariffGetRequest(string sysRef)
	{
		try
		{
			return messageSender.GetTariffUpdate(sysRef);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.Message);
			return null;
		}
	}
}
