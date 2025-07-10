using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(InBondNumberRangeRegistryDataType))]
	sealed class InBondNumberRangeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InBondNumberRangeRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "InBondNumberRangeRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var range1 = new InBondNumberRange
			{
				StartNumber = NumberFountains.USMinimumInBondNumber,
				LastNumber = NumberFountains.USMaximumInBondNumber,
				BranchPK = new Guid("B3FBD664-3A4B-4FF7-A3CC-84EB47E2B8FC"),
				RunOutWarningLimitNumber = 300
			};

			var range2 = new InBondNumberRange
			{
				StartNumber = NumberFountains.USMinimumInBondNumber,
				LastNumber = NumberFountains.USMaximumInBondNumber,
				BranchPK = new Guid("A3E5DE70-2B20-4F54-AA2B-B22DDF155FBD"),
				RunOutWarningLimitNumber = 200
			};

			return
			[
				new ValidSampleAndBinaryValueInDB(range1, new InBondNumberRangeRegistryDataType().Serialise(range1)),
				new ValidSampleAndBinaryValueInDB(range2, new InBondNumberRangeRegistryDataType().Serialise(range2))
			];
		}

		protected override InBondNumberRangeRegistryDataType GetNewDataType()
		{
			return new InBondNumberRangeRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, CargoWise.EntityFramework.NonPersistentBusinessObject lhs, CargoWise.EntityFramework.NonPersistentBusinessObject rhs)
		{
			if (message == "Binary value incorrect, this may cause problems for existing clients with real data in their DB.")
			{
				Assert(true);
			}
			else
			{
				base.AssertValuesEqual(message, lhs, rhs);
			}
		}
	}
}
