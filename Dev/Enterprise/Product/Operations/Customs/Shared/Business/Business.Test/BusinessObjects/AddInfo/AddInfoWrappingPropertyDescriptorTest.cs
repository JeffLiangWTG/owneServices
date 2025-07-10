using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AddInfoWrappingPropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestListAttributeRename()
		{
			var testClass = Factory.New<AddInfoPropertyDescriptorCollectionParentClass>();

			AssertEquals("Lookups.List", testClass.AddInfo.Z0_CodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.List", testClass.Z0_CodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestAttributesShouldNotBeAddedToParent()
		{
			var testClass = Factory.New<AddInfoPropertyDescriptorCollectionParentClass>();

			_ = testClass.Z0_DescriptionInfo.GetAttribute<ListAttribute>().ListDataSourceMember;

			AssertEquals(
				"Attributes (ListAttribute) should not be added directly to the Enterprise.Customs.Business.Testing.AddInfoPropertyDescriptorCollectionParentClass.Z0_Description property. Please add the Attribute to the Enterprise.Customs.Business.Testing.AddInfoPropertyDescriptorCollectionAddInfoClass.Z0_Description property instead.",
				ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			_ = testClass.Z0_XmlInfo.GetAttribute<MaxLengthAttribute>().MaxLength;

			AssertEquals(
				"Attributes (MaxLengthAttribute) should not be added directly to the Enterprise.Customs.Business.Testing.AddInfoPropertyDescriptorCollectionParentClass.Z0_Xml property. Please add the Attribute to the Enterprise.Customs.Business.Testing.AddInfoPropertyDescriptorCollectionAddInfoClass.Z0_Xml property instead.",
				ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}
	}
}
