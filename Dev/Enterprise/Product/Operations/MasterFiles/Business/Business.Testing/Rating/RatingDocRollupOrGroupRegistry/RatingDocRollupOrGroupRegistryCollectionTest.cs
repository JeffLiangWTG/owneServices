using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RatingDocRollupOrGroupRegistryCollection))]
	public class RatingDocRollupOrGroupRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RatingDocRollupOrGroupRegistryCollection>
	{
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override RatingDocRollupOrGroupRegistryCollection GetCollectionToTest()
			=> new RatingDocRollupOrGroupRegistryCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new RatingDocRollupOrGroupRegistry();
	}
}
