namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

interface ITaricUpdateCollector
{
	byte[] GenerateAndSendTariffGetRequest(string sysRef);
}
