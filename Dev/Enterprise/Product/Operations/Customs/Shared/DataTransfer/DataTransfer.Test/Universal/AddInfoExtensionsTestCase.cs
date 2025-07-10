using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions.Testing
{
	sealed class AddInfoExtensionsTestCase : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetAddInfoSchemaWithMaxLengthDictionary()
		{
			var dictionary = Factory.BOFactory.GetAddInfoSchemaWithMaxLengthDictionary(typeof(DummCusContainerWithAddInfo), DummyBizoSchema.Instance);
			AssertEquals(7, dictionary.Count);
			AssertAddInfoSchemaDictionaryValue(dictionary[DummyBizoSchema.Z0_Code.Name.Substring(3)], typeof(ZString), 5);
			AssertAddInfoSchemaDictionaryValue(dictionary[DummyBizoSchema.Z0_Bool.Name.Substring(3)], typeof(ZBool), -1);
			AssertAddInfoSchemaDictionaryValue(dictionary[DummyBizoSchema.Z0_Date.Name.Substring(3)], typeof(ZDateTime), -1);
			AssertAddInfoSchemaDictionaryValue(dictionary[DummyBizoSchema.Z0_Decimal.Name.Substring(3)], typeof(ZDecimal), -1);
			AssertAddInfoSchemaDictionaryValue(dictionary[DummyBizoSchema.Z0_Guid.Name.Substring(3)], typeof(ZGuid), -1);
			AssertAddInfoSchemaDictionaryValue(dictionary[DummyBizoSchema.Z0_Number.Name.Substring(3)], typeof(ZInt), -1);
			AssertAddInfoSchemaDictionaryValue(dictionary[DummyBizoSchema.Z0_Short.Name.Substring(3)], typeof(ZShort), -1);
		}

		public void TestGetAddInfoSchemaDictionary()
		{
			var bizObj = Factory.New<DummCusContainerWithAddInfo>();
			var type = bizObj.GetType();
			var dictionary = Factory.BOFactory.GetAddInfoSchemaDictionary(type, DummyBizoSchema.Instance);
			AssertEquals(7, dictionary.Count);
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[DummyBizoSchema.Z0_Code.Name.Substring(3)], typeof(ZString));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[DummyBizoSchema.Z0_Bool.Name.Substring(3)], typeof(ZBool));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[DummyBizoSchema.Z0_Date.Name.Substring(3)], typeof(ZDateTime));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[DummyBizoSchema.Z0_Decimal.Name.Substring(3)], typeof(ZDecimal));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[DummyBizoSchema.Z0_Guid.Name.Substring(3)], typeof(ZGuid));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[DummyBizoSchema.Z0_Number.Name.Substring(3)], typeof(ZInt));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[DummyBizoSchema.Z0_Short.Name.Substring(3)], typeof(ZShort));

			type = ObjectFactory.GetType<Integration.Customs.US.IJobComInvoiceLine>();
			dictionary = Factory.BOFactory.GetAddInfoSchemaDictionary(type, USAddInfoSchema.Instance);
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[USAddInfoSchema.US_ADDDepositRateIndicator.Name.Substring(3)], typeof(ZString));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[USAddInfoSchema.US_DRWIsForExportSection.Name.Substring(3)], typeof(ZBool));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[USAddInfoSchema.US_DRWImportEntryLine.Name.Substring(3)], typeof(ZInt));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[USAddInfoSchema.US_DRWLineDuty.Name.Substring(3)], typeof(ZDecimal));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[USAddInfoSchema.US_DDTCArrivalDate.Name.Substring(3)], typeof(ZDateTime));
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[USAddInfoSchema.US_WHSEntryLineNo.Name.Substring(3)], typeof(ZShort));
		}

		public void TestGetAddInfoSchemaDictionary_LongPrefix()
		{
			var type = ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryInstruction>();
			var dictionary = Factory.BOFactory.GetAddInfoSchemaDictionary(type, ZACusEntryInstructionSchema.Instance);
			AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(dictionary[ZACusEntryInstructionSchema.CEI_BankCode.Name.Substring(4)], typeof(ZString));
		}

		public void TestGetAddInfoDescAttribute_LongPrefix()
		{
			var bizObj = Factory.New<CusEntryInstructionWithAddInfoDescAttribute>();
			AssertNotNull(AddInfoExtensions.GetAddInfoDescAttribute(bizObj)["BankCode"]);
		}

		public void TestGetAddInfoSchemaDictionary_ExcludeIsSystemColumn()
		{
			var type = ObjectFactory.GetType<Integration.Customs.ZA.IJobDeclaration>();
			var dictionary = Factory.BOFactory.GetAddInfoSchemaDictionary(type, ZAJobDeclarationSchema.Instance);
			AssertEquals(false, dictionary.ContainsKey(ZAJobDeclarationSchema.Constants.JE_ClusterKey.Substring(3)));
		}

		public void TestGetAddInfoSchemaDictionaryWithExtraKeyMapping()
		{
			var type = ObjectFactory.GetType<Integration.Customs.SG.IJobDeclaration>();
			var dictionary = Factory.BOFactory.GetAddInfoSchemaWithMaxLengthDictionary(type, SGAddInfoSchema.Instance);

			Assert(!dictionary.ContainsKey(SGAddInfoSchema.SG_AdditionalRecipientID1.Name.Substring(3)));
			AssertAddInfoSchemaDictionaryValue(dictionary["SG1"], typeof(ZString), 17);
		}

		public void TestGetAddInfos()
		{
			var bizObj = Factory.New<DummCusContainerWithAddInfo>();
			bizObj.CO_AddInfo = "GREETING=HELLO*QUESTION=HOW ARE YOU?";
			var row = (IColumnIndexer)((IBusinessObjectInternals)bizObj).Row;
			var addInfos = row.GetAddInfos(CusContainerSchema.CO_AddInfo);
			AssertEquals(2, addInfos.Count);
			AssertEquals("HELLO", addInfos["GREETING"]);
			AssertEquals("HOW ARE YOU?", addInfos["QUESTION"]);
		}

		public void TestGetValue()
		{
			var guid = ZGuid.NewZGuid();
			var addInfos = new Dictionary<ZString, ZString>();
			addInfos.Add(DummyBizoSchema.Z0_Money.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZDecimal)8635.45m));
			addInfos.Add(DummyBizoSchema.Z0_Bool.Name.Substring(3), BaseAddInfo.GetStringRepresentation(ZBool.True));
			addInfos.Add(DummyBizoSchema.Z0_Code.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZString)"DGD"));
			addInfos.Add(DummyBizoSchema.Z0_Date.Name.Substring(3), BaseAddInfo.GetStringRepresentation(ZDateTime.BrettsBirthday));
			addInfos.Add(DummyBizoSchema.Z0_Decimal.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZDecimal)45.35m));
			addInfos.Add(DummyBizoSchema.Z0_Guid.Name.Substring(3), BaseAddInfo.GetStringRepresentation(guid));
			addInfos.Add(DummyBizoSchema.Z0_Number.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZInt)342));
			addInfos.Add(DummyBizoSchema.Z0_Short.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZShort)8962));

			AssertEquals(ZDecimal.Zero, addInfos.GetValue(DummyBizoSchema.Z0_AnotherDecimal));
			AssertEquals(ZBool.True, addInfos.GetValue(DummyBizoSchema.Z0_Bool));
			AssertEquals((ZString)"DGD", addInfos.GetValue(DummyBizoSchema.Z0_Code));
			AssertEquals(ZDateTime.BrettsBirthday, addInfos.GetValue(DummyBizoSchema.Z0_Date));
			AssertEquals((ZDecimal)45.35m, addInfos.GetValue(DummyBizoSchema.Z0_Decimal));
			AssertEquals(guid, addInfos.GetValue(DummyBizoSchema.Z0_Guid));
			AssertEquals((ZInt)342, addInfos.GetValue(DummyBizoSchema.Z0_Number));
			AssertEquals((ZShort)8962, addInfos.GetValue(DummyBizoSchema.Z0_Short));
		}

		void AssertAddInfoSchemaDictionaryValue(SchemaColumnAndMaxLength value, Type type, int maxLength)
		{
			AssertEquals(type, value.Column.GetEquivalentZType());
			AssertEquals(maxLength, value.MaxLength);
		}

		void AssertAddInfoSchemaWithoutMaxLengthDictionaryValue(SchemaColumn column, Type type)
		{
			AssertEquals(type, column.GetEquivalentZType());
		}

		class CusEntryInstructionWithAddInfoDescAttribute : CusEntryInstruction
		{
			public CusEntryInstructionWithAddInfoDescAttribute(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[AddInfoDesc(nameof(BankCodeDesc))]
			public ZString CEI_BankCode
			{
				get { return ""; }
				set { _ = value; }
			}

			public ZString BankCodeDesc => "ABC";
		}
	}
}
