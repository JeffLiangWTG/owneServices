using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxIdAndTaxMessageCombinationRulesConfiguration))]
	class TaxIdAndTaxMessageCombinationRulesConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestTaxIdAndTaxMessageCombinationRulesDefault()
		{
			var config = new TaxIdAndTaxMessageCombinationRulesConfiguration();
			AssertEquals("Default settings should be empty", 0, config.TaxIdAndTaxMessageCombinationRulesCollection.Count);
			AssertEquals("Default ValidationOption should be MSG", AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage, config.ValidationOption);
		}

		public void TestReadElementsShouldSetDefaultValidationOption()
		{
			var dataType = new TaxIdAndTaxMessageCombinationRulesRegistryItemDataType();
			var config = new TaxIdAndTaxMessageCombinationRulesConfiguration() { ValidationOption = null };
			AssertNullOrEmpty("Precondition", config.ValidationOption);
			var newConfig = dataType.Deserialise(dataType.Serialise(config));
			AssertEquals("Default ValidationOption should be MSG", AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage, newConfig.ValidationOption);
		}

		public void TestValidateValidationOption()
		{
			BizObj.ValidationOption = string.Empty;
			AssertHasErrorContaining(BizObj.ValidationOptionInfo, "Please enter a value.");

			BizObj.ValidationOption = "ABC";
			AssertHasErrorContaining(BizObj.ValidationOptionInfo, "Enter a valid selection.");

			BizObj.ValidationOption = AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage;
			AssertNoErrors(BizObj.ValidationOptionInfo);

			using (BizObj.GetValidationSuspender())
			{
				AssertEquals("Precondition", true, BizObj.IsValidationSuspended);

				BizObj.ValidationOption = "ABC";
				AssertNoErrors(BizObj.ValidationOptionInfo);

				BizObj.RunPreSaveValidation();
				AssertHasErrorContaining(BizObj.ValidationOptionInfo, "Enter a valid selection.");

				BizObj.ValidationOption = AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage;
				BizObj.RunPreSaveValidation();
				AssertNoErrors(BizObj.ValidationOptionInfo);
			}
		}

		public void TestValidationOptionsList()
		{
			AssertEquals(2, BizObj.ValidationOptionsList.Count);
			var listCodes = BizObj.ValidationOptionsList.GetAllCodes();
			AssertCollectionContains(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage, listCodes);
			AssertCollectionContains(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxID, listCodes);
		}

		public void TestReloadSort()
		{
			var config = new TaxIdAndTaxMessageCombinationRulesConfiguration();
			config.TaxIdAndTaxMessageCombinationRulesCollection.Add(new TaxIdAndTaxMessageCombinationRules()
			{
				LineType = "REV",
				TaxRate = ValidTaxRate1.PK,
				TaxMessage = ValidTaxMessage.PK
			});
			config.TaxIdAndTaxMessageCombinationRulesCollection.Add(new TaxIdAndTaxMessageCombinationRules()
			{
				LineType = "CST",
				TaxRate = ValidTaxRate1.PK,
				TaxMessage = ValidTaxMessage.PK
			});
			config.TaxIdAndTaxMessageCombinationRulesCollection.Add(new TaxIdAndTaxMessageCombinationRules()
			{
				LineType = "REV",
				TaxRate = ValidTaxRate2.PK,
				TaxMessage = ValidTaxMessage.PK
			});
			config.TaxIdAndTaxMessageCombinationRulesCollection.Add(new TaxIdAndTaxMessageCombinationRules()
			{
				LineType = "CST",
				TaxRate = ValidTaxRate2.PK,
				TaxMessage = ValidTaxMessage.PK
			});
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
			Factory.Save();

			var valuesLineTyp = AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.Value.TaxIdAndTaxMessageCombinationRulesCollection
				.OfType<TaxIdAndTaxMessageCombinationRules>()
				.Select(x => x.LineType)
				.ToArray();

			AssertArrayEqualsByElements(new ZString[] {
				"CST",
				"CST",
				"REV",
				"REV",
			}, valuesLineTyp);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			ValidTaxRate1 = taxRateCollection.AddNew();
			ValidTaxRate1.AT_Code = "TGST";
			ValidTaxRate1.AT_Type = "RAT";
			ValidTaxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ValidTaxRate1.SetRateNumerator_ForTestOnly(10);

			ValidTaxRate2 = taxRateCollection.AddNew();
			ValidTaxRate2.AT_Code = "TGST2";
			ValidTaxRate2.AT_Type = "RAT";
			ValidTaxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ValidTaxRate2.SetRateNumerator_ForTestOnly(10);

			var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			ValidTaxMessage = taxMessageCollection.AddNew();
			ValidTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();
		}

		AccTaxRate ValidTaxRate1;
		AccTaxRate ValidTaxRate2;

		AccInvMsg ValidTaxMessage;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new TaxIdAndTaxMessageCombinationRulesConfiguration();
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new TaxIdAndTaxMessageCombinationRulesConfiguration BizObj
		{
			get { return (TaxIdAndTaxMessageCombinationRulesConfiguration)base.BizObj; }
		}

		#endregion Implementation
	}
}
