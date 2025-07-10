using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(PackageVersionOverrideDataType))]
	class PackageVersionOverrideDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PackageVersionOverrideDataType>
	{
		#region Implementation

		protected override string ExpectedEditorName => "PackageVersionOverrideRegistryItemEditor";

		protected override PackageVersionOverrideDataType GetNewDataType()
			=> new PackageVersionOverrideDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new PackageVersionOverrideCollection();
			collection1.Add(new PackageVersionOverride()
			{
				Code = "ZZABC",
				PackageName = "Pkg1",
				PackageVersion = "1.0.1",
				AccountNumber = "131824"
			});
			collection1.Add(new PackageVersionOverride()
			{
				Code = "ZZABC",
				PackageName = "Pkg1",
				PackageVersion = "1.0.3",
				AccountNumber = "235667"
			});

			var collection2 = new PackageVersionOverrideCollection();
			collection2.Add(new PackageVersionOverride()
			{
				Code = "ZZPQR",
				PackageName = "Pkg2",
				PackageVersion = "2.0.0",
				AccountNumber = "131824"
			});
			collection2.Add(new PackageVersionOverride()
			{
				Code = "ZZPQR",
				PackageName = "Pkg2",
				PackageVersion = "2.0.0",
				AccountNumber = "235667"
			});

			var byteArrayValue1 = new PackageVersionOverrideDataType().Serialise(collection1);
			var byteArrayValue2 = new PackageVersionOverrideDataType().Serialise(collection2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(collection2, byteArrayValue2)
			};
		}

		#endregion
	}
}
