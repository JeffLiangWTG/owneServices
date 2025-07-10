using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	class AddInfoWrapperWithBaseAddInfoParentTest : AddInfoWrapperTest
	{
		public void TestUpdateAddInfoFromString_CallLoadPropertiesFromString()
		{
			var bo = Factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			var testAddInfo = new TestAddInfo(bo.JE_AddInfoInfo);

			var wrapper = new AddInfoWrapperWithBaseAddInfoParent<JobDeclarationWithIAddInfoManagerWithSchema>(
					bo,
					testAddInfo,
					BaseJobDeclaration.Schema.JE_AddInfo,
					() => bo.AddInfoNamesMapping,
					BaseJobDeclaration.Schema.JE_NAddInfo,
					() => bo.NAddInfoNamesMapping
				) as IAddInfo;
			wrapper.UpdateAddInfoFromString("FieldA1=value1*FieldN1=价值1*FieldA2=value2*FieldN2=价值2*String=B1");

			CombineAssertions(() =>
			{
				AssertEquals("JE_AddInfo", "FieldA1=value1*FieldA2=value2*String=B1", bo.JE_AddInfo);
				AssertEquals("JE_NAddInfo", "FieldN1=价值1*FieldN2=价值2", bo.JE_NAddInfo);
				AssertEquals("JE_FieldA1", "value1", bo.JE_FieldA1);
				AssertEquals("JE_FieldA2", "value2", bo.JE_FieldA2);
				AssertEquals("JE_FieldN1", "价值1", bo.JE_FieldN1);
				AssertEquals("JE_FieldN2", "价值2", bo.JE_FieldN2);
				AssertEquals("UZ_String", "B1", testAddInfo.UZ_String);
			});
		}

		public void TestUpdateAddInfoFromString_CallLoadPropertiesFromString2()
		{
			var bo = Factory.New<JobDeclarationWithIAddInfoManagerWithSchemaWithNAddInfo>();
			var testAddInfo = new TestAddInfo(bo.JE_AddInfoInfo);

			var wrapper = new AddInfoWrapperWithBaseAddInfoParent<JobDeclarationWithIAddInfoManagerWithSchemaWithNAddInfo>(
					bo,
					testAddInfo,
					BaseJobDeclaration.Schema.JE_AddInfo,
					() => bo.AddInfoNamesMapping,
					BaseJobDeclaration.Schema.JE_NAddInfo,
					() => bo.NAddInfoNamesMapping
				) as IAddInfo;
			wrapper.UpdateAddInfoFromString("FieldA1=value1*FieldN1=价值1*FieldA2=value2*FieldN2=价值2*String=B1*NString=价值3");

			AssertEquals(
				"Enterprise.Customs.Business.Testing.AddInfoWrapperWithBaseAddInfoParentTest+JobDeclarationWithIAddInfoManagerWithSchemaWithNAddInfo has implement INAddInfoSupporter, it should not be used in conjuction with AddInfoWrapperWithBaseAddInfoParent`1 as INAddInfoSupporter will cause _AddInfo and _NAddInfo to be combine; see AddInfoParser.ConcatAddInfoStrings usage.",
				ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		class JobDeclarationWithIAddInfoManagerWithSchemaWithNAddInfo : JobDeclarationWithIAddInfoManagerWithSchema, INAddInfoSupporter
		{
			public JobDeclarationWithIAddInfoManagerWithSchemaWithNAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{ }

			public ZPropertyInfoString NAddInfoProperty => (ZPropertyInfoString)JE_NAddInfoInfo;
		}
	}
}
