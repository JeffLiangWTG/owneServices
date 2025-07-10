using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefPackTypeCollection
	{
		ICodeDescriptionPairList GetAsCodeDescriptionPair();
		MultilingualString GetDescriptionFromCode(string code);

		ICodeDescriptionPairList GetAsCodeDescriptionPairFull();
	}
}
