using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.Test
{
	public class AssignUniversalChargeCodeCommandTest : RatingTestCase
	{
		public void TestAssignGlobalChargeCode()
		{
			var chargeCode = CreateChargeCode("ZZZ", true);
			Factory.Save();

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddYesAnswer();
			cmd.AssignUniversalCode(chargeCode, "UNI");

			chargeCode.Reload();

			AssertEquals("During the operation the Universal Charge Code UNI will be assigned to Global Charge Code ZZZ. Are you sure you want to proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Universal charge code 'UNI' is now assigned to Charge code 'ZZZ'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("UNI", chargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignGlobalChargeCodeWhenItIsInvalid()
		{
			var chargeCode = CreateChargeCode("ZZZ", true);
			Factory.Save();

			using (chargeCode.GetValidationSuspender())
			{
				foreach (var item in chargeCode.ChildChargeCodes.ToArray())
				{
					item.AC_AT_GSTRate = ZGuid.Empty;
				}

				Factory.Save();
			}

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddYesAnswer();
			cmd.AssignUniversalCode(chargeCode, "UNI");

			chargeCode.Reload();

			AssertEquals(@"Error - AC_AT_GSTRate: Please enter a GST Tax ID.
The invalid charge code 'ZZZ' can be found in Maintain > Account > Global Charge Codes.
Please edit the charge code to correct the error shown above.
To highlight errors to be corrected choose ""File"" then ""Validate All"" after editing the charge code(s).", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("No Change", "", chargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignLocalChargeCode()
		{
			var chargeCode = CreateChargeCode("ZZZ", false);
			Factory.Save();

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddYesAnswer();
			cmd.AssignUniversalCode(chargeCode, "UNI");

			chargeCode.Reload();

			AssertEquals("During the operation the Universal Charge Code UNI will be assigned to Charge Code ZZZ. Are you sure you want to proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Universal charge code 'UNI' is now assigned to Charge code 'ZZZ'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("UNI", chargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignLocalChargeCodeWhenItIsInvalid()
		{
			var chargeCode = CreateChargeCode("ZZZ", false);
			chargeCode.AC_AT_GSTRate = ZGuid.Empty;

			Factory.Save();

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddYesAnswer();
			cmd.AssignUniversalCode(chargeCode, "UNI");

			chargeCode.Reload();

			AssertEquals(@"Error - AC_AT_GSTRate: Please enter a GST Tax ID.
The invalid charge code 'ZZZ' can be found in Maintain > Account > Charge Codes.
Please edit the charge code to correct the error shown above.
To highlight errors to be corrected choose ""File"" then ""Validate All"" after editing the charge code(s).", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("No Change", "", chargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignGlobalChargeCode_AlreadyMapped()
		{
			var universalChargeCodes = new ChargeCodeWithMappingInfo[]
			{
				new ChargeCodeWithMappingInfo() { Code = "FOO", Description = "FOO Desc" },
				new ChargeCodeWithMappingInfo() { Code = "UNI", Description = "UNI Desc" }
			};
			using (TestHelper.MockUniversalChargeCodes(universalChargeCodes))
			{
				var chargeCode = CreateChargeCode("ZZZ", true, "FOO");
				Factory.Save();

				var cmd = new AssignUniversalChargeCodeCommand();
				UnitTestUserNotification.Instance.AddYesAnswer();
				cmd.AssignUniversalCode(chargeCode, "UNI");

				chargeCode.Reload();

				AssertEquals("New chargecode will also be assigned", "FOO, UNI", chargeCode.UniversalChargeCodeMappings);
			}
		}

		public void TestAssignLocalChargeCode_AlreadyMapped()
		{
			var universalChargeCodes = new ChargeCodeWithMappingInfo[]
			{
				new ChargeCodeWithMappingInfo() { Code = "FOO", Description = "FOO Desc" },
				new ChargeCodeWithMappingInfo() { Code = "UNI", Description = "UNI Desc" }
			};
			using (TestHelper.MockUniversalChargeCodes(universalChargeCodes))
			{
				var chargeCode = CreateChargeCode("ZZZ", false, "FOO");
				Factory.Save();

				var cmd = new AssignUniversalChargeCodeCommand();
				UnitTestUserNotification.Instance.AddYesAnswer();
				cmd.AssignUniversalCode(chargeCode, "UNI");

				chargeCode.Reload();

				AssertEquals("New chargecode will also be assigned", "FOO, UNI", chargeCode.UniversalChargeCodeMappings);
			}
		}

		public void TestAssignGlobalChargeCode_NoSecurity()
		{
			var chargeCode = CreateChargeCode("ZZZ", true);
			Factory.Save();

			Env.Security.GlobalChargeCodesModify.IsAllowed = false;

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
			cmd.AssignUniversalCode(chargeCode, "UNI");

			chargeCode.Reload();

			AssertEquals((string)Env.Security.GlobalChargeCodesModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No Change", "", chargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignLocalChargeCode_NoSecurity()
		{
			var chargeCode = CreateChargeCode("ZZZ", false);
			Factory.Save();

			Env.Security.ChargeCodesModify.IsAllowed = false;

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
			cmd.AssignUniversalCode(chargeCode, "UNI");

			chargeCode.Reload();

			AssertEquals((string)Env.Security.ChargeCodesModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No Change", "", chargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignLocalChargeCodeLinkedToGlobal_NoSecurity()
		{
			var globalChargeCode = CreateChargeCode("ZZZ", true);
			Factory.Save();
			var localChargeCode = LoadLocalChargeCode("ZZZ");

			Env.Security.ChargeCodesLTGModify.IsAllowed = false;

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
			cmd.AssignUniversalCode(localChargeCode, "UNI");

			localChargeCode.Reload();

			AssertEquals(Env.Security.ChargeCodesLTGModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No Change", "", localChargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignGlobalChargeCode_NoConfirmation()
		{
			var chargeCode = CreateChargeCode("ZZZ", true);
			Factory.Save();

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);
			cmd.AssignUniversalCode(chargeCode, "UNI");

			chargeCode.Reload();

			AssertEquals("During the operation the Universal Charge Code UNI will be assigned to Global Charge Code ZZZ. Are you sure you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No Change", "", chargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignLocalChargeCode_NoConfirmation()
		{
			var chargeCode = CreateChargeCode("ZZZ", false);
			Factory.Save();

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);
			cmd.AssignUniversalCode(chargeCode, "UNI");

			chargeCode.Reload();

			AssertEquals("During the operation the Universal Charge Code UNI will be assigned to Charge Code ZZZ. Are you sure you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No Change", "", chargeCode.UniversalChargeCodeMappings);
		}

		public void TestAssignLocalChargeCodeLinkedToGlobal_DifferentUniversalCode()
		{
			var globalChargeCode = CreateChargeCode("ZZZ", true, "UNI1");
			Factory.Save();
			var localChargeCode = LoadLocalChargeCode("ZZZ");

			var cmd = new AssignUniversalChargeCodeCommand();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
			cmd.AssignUniversalCode(localChargeCode, "UNI2");

			localChargeCode.Reload();

			AssertEquals("Universal Charge code will be assigned to local charge without any warnings", "UNI2", localChargeCode.UniversalChargeCodeMappings);
		}

		public void TestInitialFilter()
		{
			// Local
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AccChargeCode))
			{
				var moduleFilters = module.FilterBusinessObject.ModuleFilters;
				var universalChargeCodefilter = moduleFilters.FirstOrDefault(x => x.Description == "Universal Charge Code");
				AssertNotNull("Filter on Universal Charge Code column exists", universalChargeCodefilter);
			}

			// Global
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AccGlobalChargeCode))
			{
				var moduleFilters = module.FilterBusinessObject.ModuleFilters;
				var universalChargeCodefilter = moduleFilters.FirstOrDefault(x => x.Description == "Universal Charge Code");
				AssertNotNull("Filter on Universal Charge Code column exists", universalChargeCodefilter);
			}
		}

		#region Helpers

		AccChargeCode CreateChargeCode(string code, bool isGlobal, string universalChargeCode = "")
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = code + " Description";
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_GC = isGlobal ? ZGuid.Empty : Env.CurrentCompanyPK;

			if (!isGlobal)
			{
				chargeCode.AC_AT_GSTRate = CreateTaxRate("TAX1", "Test Charge Code. GST1 Rate #1", 10).PK;
			}

			if (!string.IsNullOrWhiteSpace(universalChargeCode))
			{
				var mapping = chargeCode.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Code = universalChargeCode;
			}

			return chargeCode;
		}

		AccTaxRate CreateTaxRate(string code, string description, int rate)
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
		}

		AccChargeCode LoadLocalChargeCode(string code)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, code);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			return Factory.LoadTop1<AccChargeCode>(query);
		}

		#endregion
	}
}

