using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AddInfoPropertyDescriptorCollectionTest : TestCaseWithFactory
	{
		public void TestAddInfoAttributes()
		{
			var testClass = Factory.New<AddInfoPropertyDescriptorCollectionParentClass>();

			AssertEquals("AddInfoLookups.List", testClass.Z0_CodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals(5, testClass.Z0_CodeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestAddInfoAttributesWithOldTablePrefix()
		{
			var testClass = Factory.New<AddInfoPropertyDescriptorCollectionParentClassWithOldTablePrefix>();

			AssertEquals("AddInfoLookups.List", testClass.Z0_CodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals(5, testClass.Z0_CodeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);

			AssertEquals("AddInfoLookups.List", testClass.XC_CodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals(5, testClass.XC_CodeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		[ExpectNoExceptions]
		public void TestAddInfoAttributesWithSameOldTablePrefix()
		{
			var testClass = Factory.New<AddInfoPropertyDescriptorCollectionParentClassWithSameOldTablePrefix>();

			AssertEquals("AddInfoLookups.List", testClass.Z0_CodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals(5, testClass.Z0_CodeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}
	}
}
