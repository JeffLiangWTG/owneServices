using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.TransportCommon.GUI.Registry;
using Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Testing
{
	[TestedType(typeof(MobilityDocumentTypesControl))]
	class MobilityDocumentTypesControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new MobilityDocumentTypeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((MobilityDocumentTypesControl)control).DocTypesGridReadOnly;
		}
	}
}
