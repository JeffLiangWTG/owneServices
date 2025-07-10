using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(SADDocumentWatermarkCollection))]
	sealed class SADDocumentWatermarkCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SADDocumentWatermarkCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override SADDocumentWatermarkCollection GetCollectionToTest()
		{
			return new SADDocumentWatermarkCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SADDocumentWatermark();
		}
	}
}
