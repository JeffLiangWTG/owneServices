using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusHAWB))]
	sealed class CusHAWBWorkflowProviderTest : WorkflowProviderTest<CusHAWB, ProcessTaskCollection>
	{
		class CusHAWBForTesting : CusHAWB
		{
			public CusHAWBForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void ResetCustomBusinessObjectForTesting() => ResetCustomBusinessObject();
		}

		[TestedType(typeof(CusHAWB))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
			public void TestCustomBusinessObject()
			{
				var template1 = Factory.New<ProcessTaskTemplate>();
				template1.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
				template1.P0_Name = "TEST 1";
				template1.P0_Description = "TEST 1 DESC";
				var customField1 = template1.GenCustomColumnDefinitions.AddNew();
				customField1.XC_Name = "stringField";
				customField1.XC_Type = AddOnColumnDataType.Codes.String;

				var customField2 = template1.GenCustomColumnDefinitions.AddNew();
				customField2.XC_Name = "intField";
				customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
				Factory.Save();

				var hawb = Factory.New<CusHAWBForTesting>();
				var resetCount = 0;
				hawb.OnResetCustomBusinessObject = () => resetCount++;
				ICustomFieldProvider customFieldProvider = hawb;
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
				hawb.ResetCustomBusinessObjectForTesting();
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
				hawb.ResetCustomBusinessObjectForTesting();
				AssertEquals("resetCount", 2, resetCount);
				oldCustomBusinessObject = customBusinessObject;
				customBusinessObject = customFieldProvider.GetCustomBusinessObject();
				Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
				properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
				AssertEquals("properties", 1, properties.Length);
				AssertContains("STRINGFIELD", properties[0]);
			}
		}

		#region Implementation

		protected override ZString ExpectedWorkflowType
		{
			get { return "HAC"; }
		}

		protected override CusHAWB GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObject(factory);
			result.CS_CM = factory.New<CusMAWB>().PK;
			return result;
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			var hawb = GetNewBusinessObject(Factory);
			if (hawb.SupportsWorkflow)
			{
				base.TestProcessTasksCreatedOnSave();
			}
			else
			{
				Assert(true);
			}
		}

		protected override bool WorkflowProviderDoesNotApplyTemplatesWhenSavingFactory => !BusinessObject.SupportsWorkflow;

		protected override void OnFinishedRunningTestThatLoadsParentJob()
		{
			AssertEquals("CusHAWBProcessTaskLoadStrategy cannot load a CusHAWBProcessTask for this HAWB as the country is not recognised. Add a case for your country", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		#endregion
	}
}
