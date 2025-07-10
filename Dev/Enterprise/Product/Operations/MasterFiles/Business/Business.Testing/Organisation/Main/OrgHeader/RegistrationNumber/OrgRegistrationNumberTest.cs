using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRegistrationNumber))]
	sealed class OrgRegistrationNumberTest : NonPersistentBusinessObjectTestCase
	{
		OrgRegistrationNumber regNo;

		RefCountry AU
		{
			get { return Factory.Load<RefCountry>(Constants.CountryGuids.Australia); }
		}

		OrgRegistrationNumber RegNo
		{
			get { return regNo ?? (regNo = (OrgRegistrationNumber)GetNewBusinessObject()); }
		}

		RefCountry SG
		{
			get { return Factory.Load<RefCountry>(Constants.CountryGuids.Singapore); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader organization = GetTestOrganization(Factory, Constants.CountryCodes.Singapore);
			return new OrgRegistrationNumber(organization);
		}

		public static OrgHeader GetTestOrganization(BusinessObjectFactory factory, ZString countryCode)
		{
			OrgHeader result = factory.New<OrgHeader>();
			result.OH_Code = "QWERTY";
			result.OH_IsConsignee = true;

			RefUNLOCO port = factory.New<RefUNLOCO>();
			port.RL_Code = "XXX";
			port.RL_RN_NKCountryCode = countryCode;
			result.OH_RL_NKClosestPort = port.RL_Code;

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
		}

		public void TestCalculateReadOnlyOnFirstAccess()
		{
			var oldSecurityModifyFinancialARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyFinancialNonARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;

			try
			{
				var readOnlySecurity = RegNo;
				var readOnlyProperty = TypeDescriptor.GetProperties(readOnlySecurity)["Number"];
				Factory.Save();

				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = false;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;
				AssertEquals("CalculateReadOnlyOnFirstAccess()", true, MetaData.GetReadOnly(readOnlySecurity, readOnlyProperty));

				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = true;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				AssertEquals("CalculateReadOnlyOnFirstAccess()", false, MetaData.GetReadOnly(readOnlySecurity, readOnlyProperty));
			}
			finally
			{
				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = oldSecurityModifyFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyFinancialNonARAPRegistrationNumbers;
			}
		}

		public void TestCountryCode()
		{
			RegNo.NumberTypeForDisplay = OrgCusCode.CodeTypes.MedicareID;
			AssertEquals("CountryCode", Constants.CountryCodes.Australia, RegNo.CountryCode);
			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, SG);
			AssertEquals("CountryCode", Constants.CountryCodes.Singapore, RegNo.CountryCode);
		}

		public void TestCusCode()
		{
			OrgCusCode cusCode1 = RegNo.Organization.CustomsCodes.AddNew();
			OrgCusCode cusCode2 = RegNo.Organization.CustomsCodes.AddNew();
			OrgCusCode cusCode3 = RegNo.Organization.CustomsCodes.AddNew();

			cusCode1.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;

			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			cusCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.Singapore;

			cusCode3.OK_CodeType = OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber;
			cusCode3.OK_RN_NKCodeCountry = Constants.CountryCodes.Singapore;

			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, AU);
			AssertEquals("CusCode", cusCode1, RegNo.CusCode);

			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.CodeTypes.GSTCode, SG);
			AssertEquals("CusCode", cusCode2, RegNo.CusCode);

			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, SG);
			AssertEquals("CusCode", cusCode3, RegNo.CusCode);
		}

		public void TestOrganization()
		{
			OrgHeader organization = Factory.New<OrgHeader>();
			OrgRegistrationNumber regNo = new OrgRegistrationNumber(organization);
			AssertEquals("Organization", organization, regNo.Organization);
		}

		public void TestNumber()
		{
			RegNo.Organization.PatternMatchRequiresRegen = false;

			AssertEquals("Number", "", RegNo.Number);
			RegNo.Number = "";
			AssertEquals("Organization.PatternMatchRequiresRegen", false, RegNo.Organization.PatternMatchRequiresRegen);
			AssertEquals("Number", "", RegNo.Number);
			AssertEquals("Organization.CustomsCodes.Count", 0, RegNo.Organization.CustomsCodes.Count);

			RegNo.Number = "123";
			AssertEquals("Organization.PatternMatchRequiresRegen", true, RegNo.Organization.PatternMatchRequiresRegen);
			AssertEquals("Number", "123", RegNo.Number);
			AssertEquals("Organization.CustomsCodes.Count", 1, RegNo.Organization.CustomsCodes.Count);
			AssertEquals("Organization.CustomsCodes[0].OK_CodeType", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RegNo.Organization.CustomsCodes[0].OK_CodeType);
			AssertEquals("Organization.CustomsCodes[0].OK_CustomsRegNo", "123", RegNo.Organization.CustomsCodes[0].OK_CustomsRegNo);
			AssertEquals("Organization.CustomsCodes[0].OK_RN_NKCodeCountry", Constants.CountryCodes.Australia, RegNo.Organization.CustomsCodes[0].OK_RN_NKCodeCountry);

			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, SG);
			AssertEquals("Number", "", RegNo.Number);
			RegNo.Number = "456";
			AssertEquals("Number", "456", RegNo.Number);
			AssertEquals("Organization.CustomsCodes.Count", 2, RegNo.Organization.CustomsCodes.Count);
			AssertEquals("Organization.CustomsCodes[0].OK_CodeType", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RegNo.Organization.CustomsCodes[0].OK_CodeType);
			AssertEquals("Organization.CustomsCodes[0].OK_CustomsRegNo", "123", RegNo.Organization.CustomsCodes[0].OK_CustomsRegNo);
			AssertEquals("Organization.CustomsCodes[0].OK_RN_NKCodeCountry", Constants.CountryCodes.Australia, RegNo.Organization.CustomsCodes[0].OK_RN_NKCodeCountry);
			AssertEquals("Organization.CustomsCodes[1].OK_CodeType", OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, RegNo.Organization.CustomsCodes[1].OK_CodeType);
			AssertEquals("Organization.CustomsCodes[1].OK_CustomsRegNo", "456", RegNo.Organization.CustomsCodes[1].OK_CustomsRegNo);
			AssertEquals("Organization.CustomsCodes[1].OK_RN_NKCodeCountry", Constants.CountryCodes.Singapore, RegNo.Organization.CustomsCodes[1].OK_RN_NKCodeCountry);

			RegNo.Number = "";
			AssertEquals("Number", "", RegNo.Number);
			AssertEquals("Organization.CustomsCodes.Count", 1, RegNo.Organization.CustomsCodes.Count);
			AssertEquals("Organization.CustomsCodes[0].OK_CodeType", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RegNo.Organization.CustomsCodes[0].OK_CodeType);
			AssertEquals("Organization.CustomsCodes[0].OK_CustomsRegNo", "123", RegNo.Organization.CustomsCodes[0].OK_CustomsRegNo);
			AssertEquals("Organization.CustomsCodes[0].OK_RN_NKCodeCountry", Constants.CountryCodes.Australia, RegNo.Organization.CustomsCodes[0].OK_RN_NKCodeCountry);

			RegNo.NumberTypeForDisplay = "XYZ";
			RegNo.Number = "789";
			AssertEquals("Number", "", RegNo.Number);
			AssertEquals("Organization.CustomsCodes.Count", 1, RegNo.Organization.CustomsCodes.Count);
			AssertEquals("Organization.CustomsCodes[0].OK_CodeType", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RegNo.Organization.CustomsCodes[0].OK_CodeType);
			AssertEquals("Organization.CustomsCodes[0].OK_CustomsRegNo", "123", RegNo.Organization.CustomsCodes[0].OK_CustomsRegNo);
			AssertEquals("Organization.CustomsCodes[0].OK_RN_NKCodeCountry", Constants.CountryCodes.Australia, RegNo.Organization.CustomsCodes[0].OK_RN_NKCodeCountry);
		}

		public void TestDeduplicationStarted()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var isFindingDuplicates = false;
			((IDeduplicatable)RegNo.Organization).ShouldRunDeduplication = true;
			RegNo.Organization.DeduplicationStarted += (o, e) => isFindingDuplicates = true;
			RegNo.Number = "";
			AssertEquals(false, isFindingDuplicates);
			isFindingDuplicates = false;

			RegNo.Number = "123";
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, SG);
			AssertEquals("Number", "", RegNo.Number);
			RegNo.Number = "456";
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			RegNo.Number = "";
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			RegNo.NumberTypeForDisplay = "XYZ";
			RegNo.Number = "789";
			AssertEquals("Number is invalid for type and won't invoke FindDuplicates method", false, isFindingDuplicates);
		}

		public void TestDeduplicationDoesNotStart()
		{
			var isFindingDuplicates = false;
			((IDeduplicatable)RegNo.Organization).ShouldRunDeduplication = false;
			RegNo.Organization.DeduplicationStarted += (o, e) => isFindingDuplicates = true;
			RegNo.Number = "";
			AssertEquals(false, isFindingDuplicates);
			isFindingDuplicates = false;

			RegNo.Number = "123";
			AssertEquals(false, isFindingDuplicates);
			isFindingDuplicates = false;

			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, SG);
			AssertEquals("Number", "", RegNo.Number);
			RegNo.Number = "456";
			AssertEquals(false, isFindingDuplicates);
			isFindingDuplicates = false;

			RegNo.Number = "";
			AssertEquals(false, isFindingDuplicates);
			isFindingDuplicates = false;

			RegNo.NumberTypeForDisplay = "XYZ";
			RegNo.Number = "789";
			AssertEquals("Number is invalid for type and won't invoke FindDuplicates method", false, isFindingDuplicates);
		}

		public void TestNumberType()
		{
			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, AU);
			AssertEquals("NumberType", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, RegNo.NumberType);
			RegNo.NumberTypeForDisplay = OrgCusCode.CodeTypes.MedicareID;
			AssertEquals("NumberType", OrgCusCode.CodeTypes.MedicareID, RegNo.NumberType);
		}

		public void TestNumberTypeDescription()
		{
			AssertEquals("NumberTypeDescription", "Australian Business Number", RegNo.NumberTypeDescription);
			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.CodeTypes.GSTCode, SG);
			AssertEquals("NumberTypeDescription", "Government GST Number", RegNo.NumberTypeDescription);
			RegNo.NumberTypeForDisplay = OrgCusCode.CodeTypes.DriverLicenceID;
			AssertEquals("NumberTypeDescription", "Driver's License Number", RegNo.NumberTypeDescription);
		}

		public void TestNumberTypeDescription_WithoutMatchingParentheses()
		{
			RegNo.NumberTypeForDisplay = "123";
			var args = new object[] { Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"), "123", "Something )(" };

			typeof(OrgRegistrationNumberTypeList)
				.GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(RegNo.Lookups.NumberTypes, args);
			AssertEquals("Something )(", RegNo.NumberTypeDescription);
		}

		public void TestNumberTypeForDisplay()
		{
			AssertEquals("NumberTypeForDisplay", RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, AU), RegNo.NumberTypeForDisplay);

			RegNo.NumberTypeForDisplay = OrgCusCode.CodeTypes.MedicareID;
			AssertEquals("NumberTypeForDisplay", OrgCusCode.CodeTypes.MedicareID, RegNo.NumberTypeForDisplay);

			OrgCusCode cusCode1 = RegNo.Organization.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber;
			cusCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.Singapore;
			AssertEquals("NumberTypeForDisplay", RegNo.Lookups.NumberTypes.GetDisplayCode(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, SG), RegNo.NumberTypeForDisplay);

			OrgCusCode cusCode2 = RegNo.Organization.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.DriverLicenceID;
			cusCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			AssertEquals("NumberTypeForDisplay", OrgCusCode.CodeTypes.DriverLicenceID, RegNo.NumberTypeForDisplay);
		}

		public void TestNumber_ReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var regNum = new OrgRegistrationNumber(org);
			var readOnlyProperty = TypeDescriptor.GetProperties(regNum)["Number"];

			CombineAssertions(() =>
			{
				SetFinancialNumbersSecurityAndAssertReadOnly(false, true, false, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(false, true, true, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(false, true, false, true, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(false, true, true, true, regNum, readOnlyProperty);

				SetFinancialNumbersSecurityAndAssertReadOnly(true, false, false, true, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, false, true, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, false, false, true, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, false, true, true, regNum, readOnlyProperty);
			});

			Factory.Save();

			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;
			CombineAssertions(() =>
			{
				SetFinancialNumbersSecurityAndAssertReadOnly(false, false, true, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(false, true, true, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, false, false, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, true, false, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, false, false, true, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, true, false, false, regNum, readOnlyProperty);
			});

			org.OH_IsDebtor = false;
			org.OH_IsCreditor = false;
			CombineAssertions(() =>
			{
				SetFinancialNumbersSecurityAndAssertReadOnly(false, false, false, true, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(false, true, false, true, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, false, false, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, true, false, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, false, true, false, regNum, readOnlyProperty);
				SetFinancialNumbersSecurityAndAssertReadOnly(true, true, true, false, regNum, readOnlyProperty);
			});
		}

		public void TestNumber_ReadOnlyWhenCusCOdeIsRestricted()
		{
			var ptCompany = Factory.NewWithValidTestData<GlbCompany>();
			ptCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;
			ptCompany.GC_IsGSTRegistered = false;

			var ptBranch = Factory.NewWithValidTestData<GlbBranch>();
			ptBranch.GB_GC = ptCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				var regNum = new OrgRegistrationNumber(org);

				OrgCusCode cusCode = regNum.Organization.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Portugal;
				cusCode.OK_CustomsRegNo = "BJU39V";

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Code = "CC1";
				chargeCode.AC_Desc = "CC1 Desc";
				chargeCode.AC_GC = ptCompany.PK;
				chargeCode.AC_ChargeType = "MRG";

				Factory.Save();

				AssertEquals("Should be allowed to delete", true, regNum.CusCode.CanDelete);
				AssertEquals("Should Not be Readonly", false, regNum.NumberInfo.ReadOnly);

				var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				arInvoice.AH_TransactionType = TransactionTypes.Invoice;
				arInvoice.AH_OH = org.PK;

				var arInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
				arInvoiceLine.AL_AH = arInvoice.PK;
				arInvoiceLine.AL_AT = ZGuid.Empty;
				arInvoiceLine.AL_AC = chargeCode.PK;
				arInvoiceLine.AL_LineAmount = 44;

				Factory.Save();

				org.RunPreSaveValidation();
				AssertEquals("Should be allowed to delete", false, regNum.CusCode.CanDelete);
				AssertEquals("Should Not be Readonly", true, regNum.NumberInfo.ReadOnly);
			}
		}

		public void TestNumber_ReadOnlyWhenCusCodeIsSSNAndNoGrantedSecurityRight()
		{
			var originalOrgDetailsViewPersonalInformation = Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;
			try
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
				{
					var org = GetTestOrganization(Factory, ZString.Empty);
					var ssnCusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "111-11-1111", Constants.CountryCodes.UnitedStates);
					var ssnRegNo = new OrgRegistrationNumber(org);
					ssnRegNo.NumberTypeForDisplay = ssnCusCode.OK_CodeType;
					AssertEquals("NumberForDisplay Should Not be Readonly", false, ssnRegNo.NumberForDisplayInfo.ReadOnly);
					AssertEquals("111-11-1111", ssnRegNo.NumberForDisplay);
					Factory.Save();

					Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
					AssertEquals("NumberForDisplay Should be Readonly", true, ssnRegNo.NumberForDisplayInfo.ReadOnly);
					AssertEquals("** View Denied due to Security Access **", ssnRegNo.NumberForDisplay);
				}
			}
			finally
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = originalOrgDetailsViewPersonalInformation;
			}
		}

		void SetFinancialNumbersSecurityAndAssertReadOnly(bool shouldBeReadOnly, bool modifyFinancialCodesNewRecordAllowed, bool modifyFinancialARAPCodesAllowed, bool modifyFinancialNonARAPCodesAllowed, OrgRegistrationNumber regNum, PropertyDescriptor readOnlyProperty)
		{
			Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = modifyFinancialARAPCodesAllowed;
			Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = modifyFinancialNonARAPCodesAllowed;
			Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed = modifyFinancialCodesNewRecordAllowed;

			AssertEquals(shouldBeReadOnly, MetaData.GetReadOnly(regNum, readOnlyProperty));
		}

		public void TestRunPreSaveValidation()
		{
			using (RegNo.SuspendValidationTesting())
			{
				RegNo.NumberTypeForDisplayInfo.AddError("x");
				RegNo.NumberInfo.AddError("x");
			}
			RegNo.RunPreSaveValidation();
			AssertNoErrors(RegNo);
		}
	}
}
