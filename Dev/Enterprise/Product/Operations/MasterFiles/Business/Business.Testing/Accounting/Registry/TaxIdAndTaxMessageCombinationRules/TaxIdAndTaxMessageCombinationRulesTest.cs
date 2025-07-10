using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxIdAndTaxMessageCombinationRules))]
	sealed class TaxIdAndTaxMessageCombinationRulesTest : Registry.Business.Testing.RegistryBusinessObjectTemplateTestCase<TaxIdAndTaxMessageCombinationRules>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var registryCollection = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection;
			return registryCollection.AddNew();
		}

		protected override TaxIdAndTaxMessageCombinationRules GetBusinessObjectToClone() => (TaxIdAndTaxMessageCombinationRules)GetNewBusinessObject();

		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		protected override TaxIdAndTaxMessageCombinationRules GetBusinessObjectToSerialise() => (TaxIdAndTaxMessageCombinationRules)GetNewBusinessObject();

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		public void TestValueReload()
		{
			var config = new TaxIdAndTaxMessageCombinationRulesConfiguration();
			config.TaxIdAndTaxMessageCombinationRulesCollection.RemoveAndDeleteAll();
			var newTaxIdAndTaxMessageCombinationRules = new TaxIdAndTaxMessageCombinationRules
			{
				LineType = "CST",
				TaxRate = ValidTaxRate.PK,
				TaxMessage = ValidTaxMessage.PK,
			};
			newTaxIdAndTaxMessageCombinationRules.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			config.TaxIdAndTaxMessageCombinationRulesCollection.Add(newTaxIdAndTaxMessageCombinationRules);
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var registryReload = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection.Cast<TaxIdAndTaxMessageCombinationRules>();
			var reloadTaxIdAndTaxMessageCombinationRule = registryReload.FirstOrDefault(x =>
				x.LineType == newTaxIdAndTaxMessageCombinationRules.LineType
				&& x.TaxRate == newTaxIdAndTaxMessageCombinationRules.TaxRate
				&& x.TaxMessage == newTaxIdAndTaxMessageCombinationRules.TaxMessage);

			AssertEquals(1, registryReload.Count());
			AssertNotNull($"Reload rule must be existed and unchanged.", reloadTaxIdAndTaxMessageCombinationRule);
		}

		public void TestLineType()
		{
			var registryCollection = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection;
			var currentItem = registryCollection.AddNew();

			currentItem.LineType = "";
			AssertHasError(currentItem.LineTypeInfo, "Please enter a value.");

			currentItem.LineType = "AAA";
			AssertHasError(currentItem.LineTypeInfo, "Enter a valid selection.");

			foreach (ICodeDescription validLineType in currentItem.Lookups.LineTypes)
			{
				currentItem.LineType = validLineType.Code;
				AssertNoErrors(currentItem.LineTypeInfo);
			}

			currentItem.TaxRate = ValidTaxRate.PK;
			currentItem.TaxMessage = ValidTaxMessage.PK;
			AssertValidateDubplicated(currentItem.LineTypeInfo);
		}

		public void TestTaxRate()
		{
			var registryCollection = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection;
			var currentItem = registryCollection.AddNew();

			currentItem.TaxRate = ZGuid.Empty;
			AssertHasError(currentItem.TaxRateInfo, "Please enter a value.");

			currentItem.TaxRate = InValidTaxRatePK;
			AssertHasError(currentItem.TaxRateInfo, "Enter a valid selection.");

			var taxRates = currentItem.Lookups.TaxRates;
			taxRates.Load();
			foreach (var taxRate in taxRates)
			{
				currentItem.TaxRate = taxRate.PK;
				AssertNoErrors(currentItem.TaxRateInfo);
			}

			currentItem.LineType = "REV";
			currentItem.TaxMessage = ValidTaxMessage.PK;
			AssertValidateDubplicated(currentItem.TaxRateInfo);
		}

		public void TestTaxMessage()
		{
			var registryCollection = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection;
			var currentItem = registryCollection.AddNew();

			currentItem.TaxMessage = ZGuid.Empty;
			AssertHasError(currentItem.TaxMessageInfo, "Please enter a value.");

			currentItem.TaxMessage = InValidTaxMessagePK;
			AssertHasError(currentItem.TaxMessageInfo, "Enter a valid selection.");

			foreach (var taxMessage in currentItem.Lookups.TaxMessages.Find(new ZQuery()))
			{
				currentItem.TaxMessage = taxMessage.PK;
				AssertNoErrors(currentItem.TaxMessageInfo);
			}

			currentItem.LineType = "REV";
			currentItem.TaxRate = ValidTaxRate.PK;
			AssertValidateDubplicated(currentItem.TaxMessageInfo);
		}

		public void TestTaxMessage_NoValidationForInvalidPKWhenCompanyIsNull()
		{
			var registryCollection = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection;
			var currentItem = registryCollection.AddNew();

			currentItem.CurrentFallbackLevel = null;
			currentItem.TaxMessage = InValidTaxMessagePK;
			AssertNoErrors(currentItem.TaxMessageInfo);

			currentItem.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			currentItem.TaxMessage = InValidTaxMessagePK;
			AssertHasError(currentItem.TaxMessageInfo, "Enter a valid selection.");
		}

		public void AssertValidateDubplicated(ZPropertyInfo propInfo)
		{
			var currentItem = (TaxIdAndTaxMessageCombinationRules)propInfo.BizObj;
			var duplicatedItem = currentItem.ParentCollection.AddNew();
			duplicatedItem.LineType = currentItem.LineType;
			duplicatedItem.TaxRate = currentItem.TaxRate;
			duplicatedItem.TaxMessage = currentItem.TaxMessage;

			currentItem.ClearRowNotifications();
			var prop = typeof(TaxIdAndTaxMessageCombinationRules).GetProperty(propInfo.Name);
			prop.SetValue(currentItem, propInfo.Value);

			AssertEquals(1, currentItem.RowErrors.Count());
			AssertHasRowError(currentItem, "This row has been duplicated and must be unique.");
		}

		public void TestTaxGroupCodeRelatedField()
		{
			var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection();
			taxMessageGroupsManagement.Add("N1", (NoResString)"Description N1", true, "N1.0");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

			ValidTaxMessage.A9_TaxGroupCode = "N1";

			Factory.Save();

			var registryCollection = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection;
			var currentItem = registryCollection.AddNew();

			currentItem.LineType = "REV";
			currentItem.TaxRate = ValidTaxRate.PK;
			currentItem.TaxMessage = ValidTaxMessage.PK;
			AssertEquals("N1", currentItem.TaxGroupCode);
			AssertEquals("Description N1", currentItem.TaxGroupDescription);
			AssertEquals("N1.0", currentItem.GovernmentCode);
			AssertEquals("RAT", currentItem.TaxRateType);
			AssertEquals((ZDecimal)10, currentItem.TaxRateValue);
			AssertEquals("QST", currentItem.AuxiliaryType);
			AssertEquals((ZDecimal)2, currentItem.ExtraTaxRate);

			var currentItem2 = registryCollection.AddNew();
			currentItem2.LineType = "REV";
			currentItem2.TaxRate = ValidTaxRate2.PK;
			currentItem2.TaxMessage = ValidTaxMessage.PK;

			AssertHasRowError(currentItem2, "All combinations with the same Government Code must have the same Tax Rate.");

			var currentItem3 = registryCollection.AddNew();
			currentItem3.LineType = "REV";
			currentItem3.TaxRate = ValidTaxRate3.PK;
			currentItem3.TaxMessage = ValidTaxMessage.PK;

			AssertHasRowError(currentItem3, "All combinations with the same Government Code must have the same Tax Rate.");
		}

		public void TestResetValueAfteTaxRateReset()
		{
			var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection();
			taxMessageGroupsManagement.Add("N1", (NoResString)"Description N1", true, "N1.0");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

			ValidTaxMessage.A9_TaxGroupCode = "N1";

			Factory.Save();

			var registryCollection = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection;
			var currentItem = registryCollection.AddNew();

			currentItem.LineType = "REV";
			currentItem.TaxRate = ValidTaxRate.PK;

			AssertEquals("RAT", currentItem.TaxRateType);
			AssertEquals((ZDecimal)10, currentItem.TaxRateValue);
			AssertEquals("QST", currentItem.AuxiliaryType);
			AssertEquals((ZDecimal)2, currentItem.ExtraTaxRate);

			currentItem.TaxRate = ValidTaxRate2.PK;

			AssertEquals("RAT", currentItem.TaxRateType);
			AssertEquals((ZDecimal)20, currentItem.TaxRateValue);
			AssertEquals("QCT", currentItem.AuxiliaryType);
			AssertEquals((ZDecimal)4, currentItem.ExtraTaxRate);
		}

		public void TestPreSaveValidation()
		{
			var registryCollection = RegistryValue.TaxIdAndTaxMessageCombinationRulesCollection;

			var validRow = registryCollection.AddNew();
			validRow.LineType = "REV";
			validRow.TaxRate = ValidTaxRate.PK;
			validRow.TaxMessage = ValidTaxMessage.PK;

			var duplicatedItem1 = registryCollection.AddNew();
			duplicatedItem1.LineType = "CST";
			duplicatedItem1.TaxRate = ValidTaxRate.PK;
			duplicatedItem1.TaxMessage = ValidTaxMessage.PK;

			var duplicatedItem2 = registryCollection.AddNew();
			duplicatedItem2.LineType = "CST";
			duplicatedItem2.TaxRate = ValidTaxRate.PK;
			duplicatedItem2.TaxMessage = ValidTaxMessage.PK;

			var invalidRow = registryCollection.AddNew();
			invalidRow.LineType = "AA";
			invalidRow.TaxRate = InValidTaxRatePK;
			invalidRow.TaxMessage = InValidTaxMessagePK;

			var emptyRow = registryCollection.AddNew();
			emptyRow.LineType = ZString.Empty;
			emptyRow.TaxRate = ZGuid.Empty;
			emptyRow.TaxMessage = ZGuid.Empty;

			validRow.RunPreSaveValidation();
			AssertNoErrors(validRow);

			duplicatedItem1.RunPreSaveValidation();
			AssertEquals(1, duplicatedItem1.RowErrors.Count());
			AssertHasRowError(duplicatedItem1, "This row has been duplicated and must be unique.");

			duplicatedItem2.RunPreSaveValidation();
			AssertEquals(1, duplicatedItem2.RowErrors.Count());
			AssertHasRowError(duplicatedItem2, "This row has been duplicated and must be unique.");

			invalidRow.RunPreSaveValidation();
			AssertHasError(invalidRow.LineTypeInfo, "Enter a valid selection.");
			AssertHasError(invalidRow.TaxRateInfo, "Enter a valid selection.");
			AssertHasError(invalidRow.TaxMessageInfo, "Enter a valid selection.");

			emptyRow.RunPreSaveValidation();
			AssertHasError(emptyRow.LineTypeInfo, "Please enter a value.");
			AssertHasError(emptyRow.TaxRateInfo, "Please enter a value.");
			AssertHasError(emptyRow.TaxMessageInfo, "Please enter a value.");
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
			ValidTaxRate.AT_ExtraTaxRateType = "QST";
			ValidTaxRate.SetExtraRate_ForTestOnly(4, 2);

			ValidTaxRate2 = taxRateCollection.AddNew();
			ValidTaxRate2.AT_Code = "TST2";
			ValidTaxRate2.AT_Type = "RAT";
			ValidTaxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ValidTaxRate2.SetRateNumerator_ForTestOnly(20);
			ValidTaxRate2.AT_ExtraTaxRateType = "QCT";
			ValidTaxRate2.SetExtraRate_ForTestOnly(8, 2);

			ValidTaxRate3 = taxRateCollection.AddNew();
			ValidTaxRate3.AT_Code = "TST3";
			ValidTaxRate3.AT_Type = "RAT";
			ValidTaxRate3.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ValidTaxRate3.SetRateNumerator_ForTestOnly(10);
			ValidTaxRate3.AT_ExtraTaxRateType = "QST";
			ValidTaxRate3.SetExtraRate_ForTestOnly(6, 2);

			do
			{
				InValidTaxRatePK = ZGuid.NewZGuid();
			} while (taxRateCollection.FindByPK(InValidTaxRatePK) != null);

			var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			ValidTaxMessage = taxMessageCollection.AddNew();
			ValidTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			do
			{
				InValidTaxMessagePK = ZGuid.NewZGuid();
			} while (taxMessageCollection.FindByPK(InValidTaxMessagePK) != null);

			Factory.Save();
		}

		AccTaxRate ValidTaxRate;
		AccTaxRate ValidTaxRate2;
		AccTaxRate ValidTaxRate3;

		ZGuid InValidTaxRatePK;

		AccInvMsg ValidTaxMessage;

		ZGuid InValidTaxMessagePK;

		TaxIdAndTaxMessageCombinationRulesConfiguration RegistryValue
		{
			get
			{
				var result = AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.Value;
				result.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				return result;
			}
		}
	}
}
