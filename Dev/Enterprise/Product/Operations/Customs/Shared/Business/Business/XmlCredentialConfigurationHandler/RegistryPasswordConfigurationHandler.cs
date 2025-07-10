using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Business.XmlCredential
{
	[CodeAlive("Will be use")]
	public abstract class RegistryPasswordConfigurationHandler : XmlCredentialConfigurationHandler
	{
		protected RegistryPasswordConfigurationHandler(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ProcessGroupOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group otherGroup, ItemData otherItems)
		{
			logger.LogError(Res.GetString("{D6F4F30A-5751-4ED7-A561-72B427CF40A0}", "Group Type is not supported"));
			return false;
		}

		protected override bool ProcessStaffOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group staffGroup, ItemData staffItems, Group otherGroup, ItemData otherItems)
		{
			logger.LogError(Res.GetString("{3156BC17-D670-47B6-8933-81333CBB5015}", "Staff Type is not supported"));
			return false;
		}
	}
}
