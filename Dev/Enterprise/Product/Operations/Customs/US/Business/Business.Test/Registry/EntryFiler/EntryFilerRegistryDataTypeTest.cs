using System;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(EntryFilerRegistryDataType))]
	sealed class EntryFilerRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EntryFilerRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "EntryFilerRegistryItemEditor"; }
		}

		protected override EntryFilerRegistryDataType GetNewDataType()
		{
			return new EntryFilerRegistryDataType();
		}

		ValidSampleAndBinaryValueInDB GetValidSample(Guid companyPk, string entryFilerCode, bool isABICertified, byte[] binaryValue)
		{
			var filer = new EntryFiler
			{
				companyPK = companyPk,
				EntryFilerCode = entryFilerCode,
				IsABICertified = isABICertified
			};

			return new ValidSampleAndBinaryValueInDB(filer, binaryValue);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = GetValidSample(Env.CurrentCompany.PK, "SV2", true, new byte[]
			{
				60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102,
				0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 69, 0, 110, 0, 116, 0, 114, 0, 121, 0, 70, 0, 105, 0, 108, 0, 101, 0, 114, 0, 62, 0, 60, 0, 67, 0, 111, 0, 109, 0, 112, 0, 97, 0, 110, 0, 121, 0, 80, 0, 75, 0, 62, 0, 56, 0, 55, 0, 56, 0, 100,
				0, 55, 0, 97, 0, 99, 0, 97, 0, 45, 0, 102, 0, 102, 0, 99, 0, 51, 0, 45, 0, 52, 0, 57, 0, 102, 0, 99, 0, 45, 0, 57, 0, 55, 0, 49, 0, 48, 0, 45, 0, 57, 0, 54, 0, 57, 0, 99, 0, 97, 0, 48, 0, 99, 0, 48, 0, 102, 0, 50, 0, 97, 0, 99, 0, 60,
				0, 47, 0, 67, 0, 111, 0, 109, 0, 112, 0, 97, 0, 110, 0, 121, 0, 80, 0, 75, 0, 62, 0, 60, 0, 69, 0, 110, 0, 116, 0, 114, 0, 121, 0, 70, 0, 105, 0, 108, 0, 101, 0, 114, 0, 67, 0, 111, 0, 100, 0, 101, 0, 62, 0, 83, 0, 86, 0, 50, 0, 60, 0, 47, 0, 69,
				0, 110, 0, 116, 0, 114, 0, 121, 0, 70, 0, 105, 0, 108, 0, 101, 0, 114, 0, 67, 0, 111, 0, 100, 0, 101, 0, 62, 0, 60, 0, 73, 0, 115, 0, 65, 0, 66, 0, 73, 0, 67, 0, 101, 0, 114, 0, 116, 0, 105, 0, 102, 0, 105, 0, 101, 0, 100, 0, 62, 0, 89, 0, 60, 0, 47,
				0, 73, 0, 115, 0, 65, 0, 66, 0, 73, 0, 67, 0, 101, 0, 114, 0, 116, 0, 105, 0, 102, 0, 105, 0, 101, 0, 100, 0, 62, 0, 60, 0, 47, 0, 69, 0, 110, 0, 116, 0, 114, 0, 121, 0, 70, 0, 105, 0, 108, 0, 101, 0, 114, 0, 62, 0
			});

			var second = GetValidSample(Env.CurrentCompany.PK, "SV3", false, new byte[]
			{
				60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102,
				0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 69, 0, 110, 0, 116, 0, 114, 0, 121, 0, 70, 0, 105, 0, 108, 0, 101, 0, 114, 0, 62, 0, 60, 0, 67, 0, 111, 0, 109, 0, 112, 0, 97, 0, 110, 0, 121, 0, 80, 0, 75, 0, 62, 0, 56, 0, 55, 0, 56, 0, 100,
				0, 55, 0, 97, 0, 99, 0, 97, 0, 45, 0, 102, 0, 102, 0, 99, 0, 51, 0, 45, 0, 52, 0, 57, 0, 102, 0, 99, 0, 45, 0, 57, 0, 55, 0, 49, 0, 48, 0, 45, 0, 57, 0, 54, 0, 57, 0, 99, 0, 97, 0, 48, 0, 99, 0, 48, 0, 102, 0, 50, 0, 97, 0, 99, 0, 60,
				0, 47, 0, 67, 0, 111, 0, 109, 0, 112, 0, 97, 0, 110, 0, 121, 0, 80, 0, 75, 0, 62, 0, 60, 0, 69, 0, 110, 0, 116, 0, 114, 0, 121, 0, 70, 0, 105, 0, 108, 0, 101, 0, 114, 0, 67, 0, 111, 0, 100, 0, 101, 0, 62, 0, 83, 0, 86, 0, 51, 0, 60, 0, 47, 0, 69,
				0, 110, 0, 116, 0, 114, 0, 121, 0, 70, 0, 105, 0, 108, 0, 101, 0, 114, 0, 67, 0, 111, 0, 100, 0, 101, 0, 62, 0, 60, 0, 73, 0, 115, 0, 65, 0, 66, 0, 73, 0, 67, 0, 101, 0, 114, 0, 116, 0, 105, 0, 102, 0, 105, 0, 101, 0, 100, 0, 62, 0, 78, 0, 60, 0, 47,
				0, 73, 0, 115, 0, 65, 0, 66, 0, 73, 0, 67, 0, 101, 0, 114, 0, 116, 0, 105, 0, 102, 0, 105, 0, 101, 0, 100, 0, 62, 0, 60, 0, 47, 0, 69, 0, 110, 0, 116, 0, 114, 0, 121, 0, 70, 0, 105, 0, 108, 0, 101, 0, 114, 0, 62, 0
			});

			return new[] { first, second };
		}
	}
}
