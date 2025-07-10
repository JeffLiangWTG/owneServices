using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxIdAndTaxMessageCombinationRulesRegistryItemDataType))]
	sealed class TaxIdAndTaxMessageCombinationRulesRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TaxIdAndTaxMessageCombinationRulesRegistryItemDataType>
	{
		protected override TaxIdAndTaxMessageCombinationRulesRegistryItemDataType GetNewDataType()
		{
			return new TaxIdAndTaxMessageCombinationRulesRegistryItemDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "TaxIdAndTaxMessageCombinationRulesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var configuration = new TaxIdAndTaxMessageCombinationRulesConfiguration();

			var copy = configuration.TaxIdAndTaxMessageCombinationRulesCollection.AddNew();
			copy.LineType = "CST";
			copy.TaxRate = ValidTaxRate.PK;
			copy.TaxMessage = ValidTaxMessage.PK;

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,84,0,97,0,120,0,73,0,100,0,65,0,110,0,100,0,84,0,97,0,120
				,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,67,0,111,0,109,0,98,0,105,0,110,0,97,0,116,0,105,0,111,0,110,0,82,0,117,0,108,0,101,0,115,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,84,0,97,0,120,0,73,0,100
				,0,65,0,110,0,100,0,84,0,97,0,120,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,67,0,111,0,109,0,98,0,105,0,110,0,97,0,116,0,105,0,111,0,110,0,82,0,117,0,108,0,101,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119
				,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46
				,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,84,0,97,0,120,0,73,0,100,0,65,0,110,0,100,0,84,0,97,0,120,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,67,0,111
				,0,109,0,98,0,105,0,110,0,97,0,116,0,105,0,111,0,110,0,82,0,117,0,108,0,101,0,115,0,62,0,60,0,76,0,105,0,110,0,101,0,84,0,121,0,112,0,101,0,62,0,67,0,83,0,84,0,60,0,47,0,76,0,105,0,110,0,101,0,84,0,121,0,112,0,101,0,62,0,60,0,84,0,97,0,120,0,82,0,97,0,116,0,101,0,62,0,56,0,52
				,0,50,0,56,0,55,0,54,0,55,0,52,0,45,0,55,0,102,0,52,0,98,0,45,0,52,0,55,0,100,0,50,0,45,0,98,0,56,0,49,0,50,0,45,0,98,0,97,0,54,0,55,0,52,0,99,0,56,0,54,0,50,0,55,0,101,0,101,0,60,0,47,0,84,0,97,0,120,0,82,0,97,0,116,0,101,0,62,0,60,0,84,0,97,0,120,0,77,0,101
				,0,115,0,115,0,97,0,103,0,101,0,62,0,49,0,56,0,53,0,102,0,55,0,48,0,57,0,99,0,45,0,54,0,56,0,52,0,55,0,45,0,52,0,55,0,51,0,53,0,45,0,98,0,99,0,54,0,52,0,45,0,99,0,54,0,56,0,57,0,98,0,48,0,54,0,101,0,98,0,51,0,48,0,51,0,60,0,47,0,84,0,97,0,120,0,77,0,101,0,115
				,0,115,0,97,0,103,0,101,0,62,0,60,0,47,0,84,0,97,0,120,0,73,0,100,0,65,0,110,0,100,0,84,0,97,0,120,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,67,0,111,0,109,0,98,0,105,0,110,0,97,0,116,0,105,0,111,0,110,0,82,0,117,0,108,0,101,0,115,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79
				,0,102,0,84,0,97,0,120,0,73,0,100,0,65,0,110,0,100,0,84,0,97,0,120,0,77,0,101,0,115,0,115,0,97,0,103,0,101,0,67,0,111,0,109,0,98,0,105,0,110,0,97,0,116,0,105,0,111,0,110,0,82,0,117,0,108,0,101,0,115,0,62,0,60,0,47,0,84,0,97,0,120,0,73,0,100,0,65,0,110,0,100,0,84,0,97,0,120,0,77
				,0,101,0,115,0,115,0,97,0,103,0,101,0,67,0,111,0,109,0,98,0,105,0,110,0,97,0,116,0,105,0,111,0,110,0,82,0,117,0,108,0,101,0,115,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(configuration, byteArrayValue)
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			ValidTaxRate = taxRateCollection.AddNew();
			ValidTaxRate.AT_Code = "TGST";
			ValidTaxRate.AT_Type = "RAT";
			ValidTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ValidTaxRate.SetRateNumerator_ForTestOnly(10);

			var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			ValidTaxMessage = taxMessageCollection.AddNew();
			ValidTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();
		}

		AccTaxRate ValidTaxRate;

		AccInvMsg ValidTaxMessage;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
