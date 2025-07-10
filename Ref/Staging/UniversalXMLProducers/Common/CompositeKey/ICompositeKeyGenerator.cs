namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey
{
	public interface ICompositeKeyGenerator
	{
		ICompositeKeyGeneratorResult GenerateCompositeKeys(ICompositeKeyNode rootNode);
	}
}
