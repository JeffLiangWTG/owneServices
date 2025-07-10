using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowActivityConfigurationRegistryDataType))]
	sealed class CashFlowActivityConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CashFlowActivityConfigurationRegistryDataType>
	{
		#region Implementation

		protected override CashFlowActivityConfigurationRegistryDataType GetNewDataType()
		{
			return new CashFlowActivityConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CashFlowActivityConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CashFlowActivityConfigurationCollection collection = new CashFlowActivityConfigurationCollection();
			CashFlowActivityConfiguration cashFlowActivityConfiguration = collection.AddNew();
			cashFlowActivityConfiguration.Code = "XXX";
			cashFlowActivityConfiguration.EnglishDescription = "Undefined";
			cashFlowActivityConfiguration.ActivityType = "X";

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,97,0,115,0,104,0,70,0,108,0,111,0,119,0,65,0,99,0,116,0,105,0,118,0,105,0,116,0,121,0,67,0,111,0,110,
0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,
0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,
0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,
0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,97,0,115,0,104,0,70,0,108,0,111,0,119,0,65,0,99,0,116,0,105,0,118,0,105,0,116,0,121,0,67,0,111,0,110,0,102,0,105,
				0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,88,0,88,0,88,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,
				0,116,0,105,0,111,0,110,0,62,0,85,0,110,0,100,0,101,0,102,0,105,0,110,0,101,0,100,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,65,0,99,0,116,0,105,
0,118,0,105,0,116,0,121,0,84,0,121,0,112,0,101,0,62,0,88,0,60,0,47,0,65,0,99,0,116,0,105,0,118,0,105,0,116,0,121,0,84,0,121,0,112,0,101,0,62,0,60,0,65,0,99,0,116,0,105,0,118,0,105,0,116,
0,121,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,85,0,110,0,100,0,101,0,102,0,105,0,110,0,101,0,100,0,32,0,65,0,99,0,116,0,105,0,118,0,105,0,116,0,105,0,101,0,115,
0,60,0,47,0,65,0,99,0,116,0,105,0,118,0,105,0,116,0,121,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,67,0,97,0,115,0,104,0,70,0,108,0,111,0,119,0,65,
0,99,0,116,0,105,0,118,0,105,0,116,0,121,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,97,0,115,
0,104,0,70,0,108,0,111,0,119,0,65,0,99,0,116,0,105,0,118,0,105,0,116,0,121,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0
			};
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		public void TestGetCaptions()
		{
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put("descriptionResKey", new ResourceStringData("descriptionResKey", "测试"));
				mockRes.Put("activityDescriptionKey", new ResourceStringData("activityDescriptionKey", "未定义活动"));

				var configurationCollection = new CashFlowActivityConfigurationCollection();

				var configuration = configurationCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.XXX;
				configuration.Description = (NoResString)"test";
				configuration.ActivityDescription = (NoResString)"Undefined Activities";
				var registryitem = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration;
				var captions = registryitem.GetCaptions(configurationCollection).ToList();
				AssertEquals(2, captions.Count);
				AssertEquals("test", captions[0]);
				AssertEquals("Undefined Activities", captions[1]);
			}
		}

		public void TestDefaultValuesTranslation()
		{
			var cashFlowActivityRegistryItem = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration;
			var resKey = cashFlowActivityRegistryItem.GetKey(null, "Undefined Activities");

			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "未定义活动"));
				var defaultValues = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.DefaultValue;
				var undefindedActivity = defaultValues.FirstOrDefault(x => ((CashFlowActivityConfiguration)x).ActivityType == CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Undefined);
				AssertEquals("未定义活动", ((CashFlowActivityConfiguration)undefindedActivity).ActivityDescription);
			}
		}

		#endregion
	}
}
