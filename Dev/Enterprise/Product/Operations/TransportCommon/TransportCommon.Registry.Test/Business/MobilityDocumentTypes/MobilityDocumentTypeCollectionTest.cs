using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(MobilityDocumentTypeCollection))]
	sealed class MobilityDocumentTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MobilityDocumentTypeCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override MobilityDocumentTypeCollection GetCollectionToTest()
		{
			return new MobilityDocumentTypeCollection(NewFallback(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MobilityDocumentType(NewFallback(), Factory);
		}

		FallbackLevel NewFallback()
		{
			return new FallbackLevel(Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
		}
	}
}
