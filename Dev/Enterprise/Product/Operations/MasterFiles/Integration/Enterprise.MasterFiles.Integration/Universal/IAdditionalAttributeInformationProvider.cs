using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAdditionalAttributeInformationProvider
	{
		ZString AdditionalDescription(ZString attributeName, ZString attributeValue);
		bool AdditionalDescriptionVisible { get; }
	}
}
