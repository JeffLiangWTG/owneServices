using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMAWB))]
	sealed class CusMAWBCustomFieldsTest : TestICustomFieldProvider
	{
		class CusMAWBForTesting : CusMAWB
		{
			public CusMAWBForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void ResetCustomBusinessObjectForTesting() => ResetCustomBusinessObject();
		}

		public void TestCustomBusinessObject()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template1.P0_Name = "TEST 1";
			template1.P0_Description = "TEST 1 DESC";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "stringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = template1.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "intField";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			Factory.Save();

			var mawb = Factory.New<CusMAWBForTesting>();
			var resetCount = 0;
			mawb.OnResetCustomBusinessObject = () => resetCount++;
			ICustomFieldProvider customFieldProvider = mawb;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			AssertEquals("resetCount", 0, resetCount);
			mawb.ResetCustomBusinessObjectForTesting();
			AssertEquals("resetCount", 1, resetCount);
			var oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			customField2.Delete();
			AssertEquals("resetCount", 1, resetCount);
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			mawb.ResetCustomBusinessObjectForTesting();
			AssertEquals("resetCount", 2, resetCount);
			oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("STRINGFIELD", properties[0]);
			if (ErrorReporter.LastKeyReported == "CUS-MAWB-WorkflowProviderTypeUnknownCountry")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
