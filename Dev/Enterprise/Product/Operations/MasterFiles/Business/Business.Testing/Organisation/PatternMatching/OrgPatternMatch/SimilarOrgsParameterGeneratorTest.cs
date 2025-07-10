using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SimilarOrgsParameterGeneratorTest : TestCase
	{
		public void TestEmptyParameterListOk()
		{
			AssertNotNull("Parameter Value List automatically created", Generator.ParameterValues);
		}

		public void TestAddNewParam()
		{
			AssertEquals("Collection is empty", 0, Generator.ParameterValues.Count);

			Generator.GetParameterForValue(OrgHeaderSchema.OH_Code, "TEST");
			AssertEquals("Collection has 1 item only", 1, Generator.ParameterValues.Count);
			AssertNotNull("New Element in collection", Generator.ParameterValues[OrgHeaderSchema.OH_Code.Name + "TEST"]);
		}

		public void TestAddExistingParam()
		{
			ZSqlParameter testParam = ZSqlParameter.New("@" + OrgHeaderSchema.OH_Code.Name, "TEST", OrgHeaderSchema.OH_Code);
			AssertEquals("Collection is empty", 0, Generator.ParameterValues.Count);

			Generator.GetParameterForValue(OrgHeaderSchema.OH_Code, "TEST");
			AssertEquals("Collection has one element", 1, Generator.ParameterValues.Count);

			Generator.GetParameterForValue(OrgHeaderSchema.OH_Code, "TEST");
			Generator.GetParameterForValue(OrgHeaderSchema.OH_Code, "TEST");
			Generator.GetParameterForValue(OrgHeaderSchema.OH_Code, "TEST");
			AssertEquals("Collection has one element", 1, Generator.ParameterValues.Count);

			Generator.GetParameterForValue(OrgHeaderSchema.OH_Code, "TEST2");
			AssertEquals("Collection has two elements - same col, different value", 2, Generator.ParameterValues.Count);

			Generator.GetParameterForValue(OrgHeaderSchema.OH_FullName, "TEST");
			AssertEquals("Collection has three elements - same value, different col", 3, Generator.ParameterValues.Count);
		}

		public void TestParameterNameHasAnUnderscoreOnTheEnd()
		{
			AssertEquals("ParameterName", "@OH_Code_0_", Generator.GetParameterForValue(OrgHeaderSchema.OH_Code, "LALALA").ParameterName);
		}

		#region Generator

		SimilarOrgsParameterGenerator Generator
		{
			get { return generator ?? (generator = new SimilarOrgsParameterGenerator(null)); }
		}

		SimilarOrgsParameterGenerator generator;

		#endregion
	}
}
