namespace CargoWise.RefDbRepo.MetaDataGenerator;

public interface IFileFinder
{
	IEnumerable<string> GetFiles();
}
