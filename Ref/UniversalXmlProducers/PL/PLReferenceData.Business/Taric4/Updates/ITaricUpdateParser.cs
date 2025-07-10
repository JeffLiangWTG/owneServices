namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

interface ITaricUpdateParser
{
	bool ParseAndSave(byte[] data);
}
