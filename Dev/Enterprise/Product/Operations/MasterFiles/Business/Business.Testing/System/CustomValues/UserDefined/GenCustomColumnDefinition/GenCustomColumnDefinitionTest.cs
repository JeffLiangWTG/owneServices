using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenCustomColumnDefinition))]
	sealed class GenCustomColumnDefinitionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCacheRules()
		{
			var taskTemplate = Factory.New<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = "XXX";
			var def = taskTemplate.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "AAA";
			def.XC_Type = "STR";
			Assert(object.ReferenceEquals(((ICustomColumnDefinition)def).GetRules(), ((ICustomColumnDefinition)def).GetRules()));
		}

		[ExpectNoExceptions]
		public void TestGenCustomColumnDefinitionValidation_NoAdditionalValidation()
		{
			var taskTemplate = Factory.New<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = "XXX";
			var def = taskTemplate.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "AAA";
			def.XC_Type = "STR";

			def.Validation.ValidateAll();
		}

		public void TestXC_NameMultilingual()
		{
			var taskTemplate = Factory.New<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = "XXX";
			var def = taskTemplate.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "Test";

			using (var mock = Res.UseMockData())
			{
				var key = def.XC_NameInfo.CustomizableDataResourceStrings.Source.GetKey(null, def.XC_Name);
				mock.Put(key, new ResourceStringData(key, "测试"));

				AssertEquals("测试", def.XC_NameMultilingual);
			}
		}

		public void TestAppendTypeToNameMultilingual()
		{
			string[] typesList =
			{
				AddOnColumnDataType.Codes.ComboBox,
				AddOnColumnDataType.Codes.Byte,
				AddOnColumnDataType.Codes.Integer,
				AddOnColumnDataType.Codes.String,
				AddOnColumnDataType.Codes.Boolean
			};
			var baseName = "dupebox";

			var columnDefinitions = typesList.Select(type =>
			{
				var columnDefinition = Factory.New<GenCustomColumnDefinition>();
				columnDefinition.XC_Name = baseName;
				columnDefinition.XC_Type = type;
				columnDefinition.AppendTypeToCaption = true;
				return (ICustomColumnDefinition)columnDefinition;
			}).ToArray();

			columnDefinitions.Select(columnDefinition =>
			{
				var expectedNameLocalized = $"{baseName} ({columnDefinition.Type})";
				AssertEquals("Type should've been appended to end of localized name", expectedNameLocalized, columnDefinition.NameLocalized);
				return true;
			});

			Assert("Everything should be processed", columnDefinitions.Length == typesList.Length);
		}

		public void TestAppendTypeToNameMultilingual_Multilingual()
		{
			string[] typesList =
			{
				AddOnColumnDataType.Codes.ComboBox,
				AddOnColumnDataType.Codes.Byte
			};
			var baseName = "dupedupe";

			var columnDefinitions = typesList.Select(type =>
			{
				var columnDefinition = Factory.New<GenCustomColumnDefinition>();
				columnDefinition.XC_Name = baseName;
				columnDefinition.XC_Type = type;
				columnDefinition.AppendTypeToCaption = true;
				return columnDefinition;
			}).ToArray();

			using (var mock = Res.UseMockData())
			{
				columnDefinitions.Select(columnDefinition =>
				{
					var key = columnDefinition.XC_NameInfo.CustomizableDataResourceStrings.Source.GetKey(null, columnDefinition.XC_Name);
					mock.Put(key, new ResourceStringData(key, "测试"));

					AssertEquals($"测试 ({((ICustomColumnDefinition)columnDefinition).Type})", columnDefinition.NameMultilingual);
					return true;
				});
			}

			Assert("Everything should be processed", columnDefinitions.Length == typesList.Length);
		}

		public void TestGenCustomColumnDefinitionValidation_WithAdditionalValidation()
		{
			var dummyWorkflowDescriptor = new DummyWorkflowDescriptorWithCustomFieldsAdditionalValidation();
			var workflowDescriptors = new Hashtable
				{
					{ dummyWorkflowDescriptor.Code, new TestObjectHandle(dummyWorkflowDescriptor) }
				};

			using (ObjectFactory.Substitute("WorkflowDescriptors", workflowDescriptors))
			{
				var template1 = Factory.New<ProcessTaskTemplate>();
				template1.P0_ProcessType = "XXX";

				var def = template1.GenCustomColumnDefinitions.AddNew();
				def.XC_Name = "AAA";
				def.XC_Type = "STR";

				const string warning = @"testing additional validation";

				var additionalValidation = new AdditionalGenCustomColumnDefinitionValidation(def);
				additionalValidation.WarningToAdd = warning;

				dummyWorkflowDescriptor.AdditionalValidation = additionalValidation;

				def.Validation.ValidateXC_Name();

				AssertHasWarning(def.XC_NameInfo, warning);

				additionalValidation.WarningToAdd = null;

				def.Validation.ValidateXC_Name();

				AssertNoWarning(def.XC_NameInfo, warning);
			}
		}
	}
}
