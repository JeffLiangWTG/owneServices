using CargoWise.Types;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.Business
{
	public interface IDeclaratonToGlobalManifestPublisher
	{
		PublishToUniversalResult Publish(BaseJobDeclaration declaration, ZDateTimeOffset eventTime);
	}
}
