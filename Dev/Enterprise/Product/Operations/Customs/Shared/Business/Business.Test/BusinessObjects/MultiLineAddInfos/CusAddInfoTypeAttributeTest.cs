using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	sealed class CusAddInfoTypeAttributeTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			CusAddInfoTypeAttribute addInfoType = new CusAddInfoTypeAttribute("LALA");
			AssertEquals("LALA", addInfoType.TypeCode);
		}

		public void TestNoDumplicatedCodes()
		{
			var tCode = typeof(CusAddInfoTypeAttribute.Codes);
			var fields = tCode.GetFields();

			var groups = from field in fields
						 group field.Name by field.GetRawConstantValue() into cField
						 select new { cField };

			groups.ToList().ForEach(
				group =>
					AssertEquals(group.cField.Key.ToString(), 1, group.cField.Count())
			);
		}

		public void TestNoCLP()
		{
			var tCode = typeof(CusAddInfoTypeAttribute.Codes);
			var fields = tCode.GetFields();

			Assert("The CLP is confused with CusCALPCO in the Ushipment.", !fields.Any(x => x.GetRawConstantValue().ToString() == "CLP"));
		}

		public void TestMakeSureEveryTypeCodeIsCusAddInfoColumnSchemaResolver()
		{
			var typeCodesList = new List<string>();
			var fieldInfos = typeof(CusAddInfoTypeAttribute.Codes).GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static);
			foreach (var fieldInfo in fieldInfos)
			{
				if (fieldInfo.IsLiteral)
				{
					typeCodesList.Add(fieldInfo.GetValue(null).ToString());
				}
			}

			var failureMessage =
				"Make sure every type code has been manually added with a mapping in the schema decider pointing to the correct AddInfoSchema,";
			failureMessage += "\r\nsee Enterprise.ZArchitecture.Schema.CusAddInfoColumnSchemaResolver.";

			var schemaResolver = new CusAddInfoColumnSchemaResolver();
			foreach (var typeCode in typeCodesList)
			{
				if (typeCode == CusAddInfoTypeAttribute.Codes.TypeCodeForTesting)
				{
					continue;
				}

				var addInfoSchema = schemaResolver.GetCusAddInfoSchemaSchema(typeCode);
				AssertNotNull(failureMessage + "\r\nTypeCode: " + typeCode, addInfoSchema);
			}
		}
	}
}
