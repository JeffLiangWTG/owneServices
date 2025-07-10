using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCusCode))]
	public class OrgCusCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShouldMakeUpperCaseGeneric()
		{
			AssertShouldMakeUpperCase(new HashSet<string>() { USACodeTypes.FIRMSCode, USACodeTypes.TTBPermitNumber, USACodeTypes.DDTCRegistrationNumber, USACodeTypes.ACASOriginatorCode, USACodeTypes.AirAMSOriginatorCode });
		}

		public void TestShouldMakeUpperCaseES()
		{
			AssertShouldMakeUpperCase(new HashSet<string>() { CodeTypes.ControlledPremisesID }, Constants.CountryCodes.Spain);
		}

		public void TestShouldMakeUpperCaseUSA()
		{
			AssertShouldMakeUpperCase(new HashSet<string>() { CodeTypes.CarrierCode, CodeTypes.TruckCarrierCode }, Constants.CountryCodes.UnitedStates);
		}

		public void TestShouldMakeUpperCaseTW()
		{
			AssertShouldMakeUpperCase(new HashSet<string>() { TaiwanCodeTypes.EPZ, TaiwanCodeTypes.CBF, TaiwanCodeTypes.FTZ, CodeTypes.WarehouseControlledPremisesID }, Constants.CountryCodes.Taiwan);
		}
		public void TestShouldMakeUpperCaseNO()
		{
			AssertShouldMakeUpperCase(new HashSet<string>() { NorwayCodeTypes.MVA, CodeTypes.GovBusinessCode }, Constants.CountryCodes.Norway);
		}

		public void TestShouldMakeUpperCaseTR()
		{
			AssertShouldMakeUpperCase(new HashSet<string>() { CodeTypes.WarehouseControlledPremisesID, CodeTypes.TerminalControlledPremisesID }, Constants.CountryCodes.Turkey);
		}

		public void TestICodeDescriptionWithCodeTypeMembers()
		{
			var cusCode = CreateCodeForTest("CCC");
			cusCode.OK_CodeType = "EIN";
			ICodeTypeCodeDescription codeWithCodeType = cusCode;
			AssertEquals("CodeType", "EIN", codeWithCodeType.CodeType);
		}

		public void TestOrganisation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew();

			AssertEquals(orgHeader, orgCusCode.Organisation);
		}

		public void TestAMOCodeChanged()
		{
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Code = "US1";

			var usBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			usBranch1.GB_GC = usCompany.PK.ToGuid();
			usBranch1.GB_Code = "CH1";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG1";
			usBranch1.GB_OH_OrgProxy = orgHeader.PK;
			Factory.Save();

			var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			interchangeQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var createdInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNull(createdInterchange);

			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.AirAMSOriginatorCode;
			orgCusCode.OK_CustomsRegNo = "00000001";
			orgCusCode.OK_CustomsRegNo = "00000002";
			Factory.Save();
			var interchanges = Factory.Load<EDIInterchange>(interchangeQuery);
			AssertNotNull(interchanges);
			AssertEquals(1, interchanges.Length);

			interchanges[0].Delete();
			orgHeader.CustomsCodes.RemoveAndDelete(orgCusCode);
			Factory.Save();
			createdInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNotNull(createdInterchange);
			createdInterchange.Delete();

			orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.GlobalLocationNumber;
			orgCusCode.OK_CustomsRegNo = "00000001";
			Factory.Save();
			createdInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNull(createdInterchange);
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.AirAMSOriginatorCode;
			Factory.Save();
			createdInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNotNull(createdInterchange);

			createdInterchange.Delete();
			orgCusCode.OK_CustomsRegNo = "00000002";
			Factory.Save();
			createdInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNotNull(createdInterchange);

			createdInterchange.Delete();
			orgCusCode.OK_CodeType = OrgCusCode.USACodeTypes.GlobalLocationNumber;
			Factory.Save();
			createdInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNotNull(createdInterchange);
		}

		public void TestOrgCusCodeValidity()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = CACodeTypes.CustomsOfficeCode;
			orgCusCode.OK_CustomsRegNo = "TEST";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			AssertEquals(ZString.Empty, orgCusCode.VerificationStatus);
			AssertEquals(ZString.Empty, orgCusCode.VerificationAuthority);
			AssertEquals(nameof(FieldType.Text), orgCusCode.VerificationAuthorityFieldType);

			orgCusCode.MarkOrgCusCodeVerifiedOrUnVerified(true, ZString.Empty, ZString.Empty);
			AssertEquals("VERIFIED", orgCusCode.VerificationStatus);
			AssertEquals("USER ENTRY", orgCusCode.VerificationAuthority);
			AssertEquals(nameof(FieldType.Text), orgCusCode.VerificationAuthorityFieldType);

			orgCusCode.MarkOrgCusCodeVerifiedOrUnVerified(false, ZString.Empty, ZString.Empty);
			AssertEquals("NOT VERIFIED", orgCusCode.VerificationStatus);
			AssertEquals(ZString.Empty, orgCusCode.VerificationAuthority);
			AssertEquals(nameof(FieldType.Text), orgCusCode.VerificationAuthorityFieldType);

			orgCusCode.MarkOrgCusCodeVerifiedOrUnVerified(true, "CBP", ZString.Empty);
			AssertEquals("VERIFIED", orgCusCode.VerificationStatus);
			AssertEquals("CBP", orgCusCode.VerificationAuthority);
			AssertEquals(nameof(FieldType.Text), orgCusCode.VerificationAuthorityFieldType);

			orgCusCode.MarkOrgCusCodeVerifiedOrUnVerified(true, "CBP", "ABC");
			AssertEquals(nameof(FieldType.LinkLabel), orgCusCode.VerificationAuthorityFieldType);

			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals(ZString.Empty, orgCusCode.VerificationStatus);
			AssertEquals(ZString.Empty, orgCusCode.VerificationAuthority);
			AssertEquals(nameof(FieldType.Text), orgCusCode.VerificationAuthorityFieldType);
		}

		public void TestOrgAddressCusCodeCollectionIsRefreshed()
		{
			cusCode.Delete();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_Address1 = "TEST";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = CACodeTypes.CustomsOfficeCode;
			orgCusCode.OK_CustomsRegNo = "TEST";
			orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();
			AssertEquals(0, address.CustomsCodes.Count);

			var newFactory = new BusinessObjectFactory();
			var newOrgHeader = newFactory.Load<OrgHeader>(orgHeader.PK);
			AssertEquals(0, newOrgHeader.Addresses[0].CustomsCodes.Count);

			orgCusCode.OK_OA_PremisesAddress = address.PK;
			Factory.Save();
			AssertEquals(1, orgCusCode.PremisesAddress.CustomsCodes.Count);

			AssertEquals(1, newOrgHeader.CustomsCodes[0].PremisesAddress.CustomsCodes.Count);
		}

		public void TestCompanyCodeAndPremisesAddresses()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "ABC123SYD";
			organisation.MainAddress.OA_Code = "OFC: ADDRESS 1";
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "XZX";
			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;

			AssertEquals("CompanyCodeAndPremisesAddresses", "ABC123SYD (OFC: ADDRESS 1)", cusCode.CompanyCodeAndPremisesAddresses);
		}

		public void TestLoadMethod()
		{
			var refCountryCode = "ZA";

			var organisation = Factory.New<OrgHeader>();
			var cusCode1 = organisation.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "XZX";
			cusCode1.OK_RN_NKCodeCountry = refCountryCode;
			cusCode1.OK_CustomsRegNo = "88754";
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "XZX";
			cusCode2.OK_RN_NKCodeCountry = refCountryCode;
			cusCode2.OK_CustomsRegNo = "98754";
			cusCode2.OK_OA_PremisesAddress = organisation.MainAddress.PK;

			AssertEquals(cusCode1, Load(Factory, "XZX", refCountryCode, organisation.PK, ZGuid.Empty));
			AssertEquals(cusCode2, Load(Factory, "XZX", refCountryCode, organisation.PK, organisation.MainAddress.PK));
		}

		public void TestGetDatabaseCount()
		{
			CreateOrganisation(USACodeTypes.EmployerIdentificationNumber, "48-0920709WM");
			CreateOrganisation(USACodeTypes.EmployerIdentificationNumber, "48-0920709WM");
			CreateOrganisation(USACodeTypes.EmployerIdentificationNumber, "48-0920709WM");
			CreateOrganisation(USACodeTypes.EmployerIdentificationNumber, "12-3456789");
			CreateOrganisation(USACodeTypes.SocialSecurityNumber, "123-12-1234");
			CreateOrganisation(USACodeTypes.CBPAssignedNumber, "061234-12345");
			CreateOrganisation(USACodeTypes.SocialSecurityNumber, "123-12-1234");
			CreateOrganisation(USACodeTypes.CBPAssignedNumber, "061234-72345");
			Factory.Save();

			var iDTypes = new string[] { USACodeTypes.EmployerIdentificationNumber, USACodeTypes.SocialSecurityNumber, USACodeTypes.CBPAssignedNumber };
			AssertEquals(3, new Loader(Factory).GetDatabaseCount(Core.Constants.CountryCodes.UnitedStates, "48-0920709WM", iDTypes));
			AssertEquals(1, new Loader(Factory).GetDatabaseCount(Core.Constants.CountryCodes.UnitedStates, "12-3456789", iDTypes));
			AssertEquals(2, new Loader(Factory).GetDatabaseCount(Core.Constants.CountryCodes.UnitedStates, "123-12-1234", iDTypes));
			AssertEquals(1, new Loader(Factory).GetDatabaseCount(Core.Constants.CountryCodes.UnitedStates, "061234-12345", iDTypes));
			AssertEquals(1, new Loader(Factory).GetDatabaseCount(Core.Constants.CountryCodes.UnitedStates, "061234-72345", iDTypes));
			AssertEquals(0, new Loader(Factory).GetDatabaseCount(Core.Constants.CountryCodes.UnitedStates, "123", iDTypes));
		}

		public void TestValidation()
		{
			CombineAssertions(() =>
			{
				var organisation = Factory.New<OrgHeader>();
				var cusCode = organisation.CustomsCodes.AddNew();
				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
				AssertEquals("Australia", ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
				AssertEquals("UnitedStates", ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.PuertoRico;
				AssertEquals("Puerto Rico", ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Spain;
				AssertEquals("Spain", ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Croatia;
				AssertEquals("EU Countries without their own validation implemented, should use EU's validation", ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Turkey;
				AssertEquals("Turkey", ObjectFactory.GetType<Enterprise.Integration.Customs.TR.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
				AssertEquals("Germany", ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
				AssertEquals("South Africa", ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.China;
				AssertEquals("China", ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
				AssertEquals("Canada", ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.NewZealand;
				AssertEquals("New Zealand", ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Taiwan;
				AssertEquals("Taiwan", ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Norway;
				AssertEquals("Norway", ObjectFactory.GetType<Enterprise.Integration.Customs.NO.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Italy;
				AssertEquals("Italy", ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Japan;
				AssertEquals("Japan", ObjectFactory.GetType<Enterprise.Integration.Customs.JP.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
				AssertEquals("Brazil", ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
				AssertEquals("India", ObjectFactory.GetType<Enterprise.Integration.Customs.IN.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
				AssertEquals("Switzerland", ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IOrgCusCodeValidation>(), cusCode.Validation.GetType());

				cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Liechtenstein;
				AssertEquals("Liechtenstein (same as CH)", ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IOrgCusCodeValidation>(), cusCode.Validation.GetType());
			});
		}

		public void TestRegistrationNumber()
		{
			var refCountryCode = Constants.CountryCodes.NewZealand;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = NZCodeTypes.RegistrationNumber;
			cusCode.OK_RN_NKCodeCountry = refCountryCode;
			cusCode.OK_CustomsRegNo = "35234";

			AssertEquals(cusCode, Load(Factory, NZCodeTypes.RegistrationNumber, refCountryCode, organisation.PK, ZGuid.Empty));
		}

		public void TestExistingRegistrationNumberWhenGrantedNoSecurityRight()
		{
			var viewDeniedMessage = "** View Denied due to Security Access **";
			var originalOrgDetailsViewPersonalInformation = Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;
			try
			{
				var refCountryCode = Constants.CountryCodes.UnitedStates;
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;

				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var ssnCusCode = organisation.CustomsCodes.AddNew(USACodeTypes.SocialSecurityNumber, "111-11-1111", Constants.CountryCodes.UnitedStates);

				var normalCusCode = organisation.CustomsCodes.AddNew();
				normalCusCode.OK_CodeType = USACodeTypes.ABIRoutingCode;
				normalCusCode.OK_RN_NKCodeCountry = refCountryCode;
				normalCusCode.OK_CustomsRegNo = "11111";
				Factory.Save();

				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;

				AssertEquals(viewDeniedMessage, ssnCusCode.SecuredCustomsRegNo);
				AssertEquals("11111", normalCusCode.SecuredCustomsRegNo);
				AssertOrgCusCodeFieldsReadOnly(ssnCusCode, true);
				AssertOrgCusCodeFieldsReadOnly(normalCusCode, false);

				normalCusCode.OK_CodeType = USACodeTypes.SocialSecurityNumber;
				AssertEquals(true, normalCusCode.OK_CodeTypeInfo.HasErrors());
				AssertEquals("11111", normalCusCode.SecuredCustomsRegNo);
				AssertOrgCusCodeFieldsReadOnly(normalCusCode, false);
			}
			finally
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = originalOrgDetailsViewPersonalInformation;
			}
		}

		public void TestCreateRegistrationNumberWhenGrantedNoSecurityRight()
		{
			var originalOrgDetailsViewPersonalInformation = Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;
			try
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;

					var ssnCusCode = org.CustomsCodes.AddNew(USACodeTypes.SocialSecurityNumber, "111-11-1111", Constants.CountryCodes.UnitedStates);
					AssertEquals(true, ssnCusCode.OK_CodeTypeInfo.HasErrors());
					AssertOrgCusCodeFieldsReadOnly(ssnCusCode, false);
				}
			}
			finally
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = originalOrgDetailsViewPersonalInformation;
			}
		}

		public void TestCHCCustomsCodeMaxLength()
		{
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = IcelandCodeTypes.CustomsOfficeCode;

			AssertEquals(5, cusCode.OK_CustomsRegNoInfo.MaxLength);
		}

		public void TestOK_CustomsRegNoMaxLength_APICodeType()
		{
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber;

			AssertEquals(25, cusCode.OK_CustomsRegNoInfo.MaxLength);
		}

		public void TestYFKCustomsCodeMaxLength()
		{
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.YFK;

			AssertEquals(13, cusCode.OK_CustomsRegNoInfo.MaxLength);
		}

		public void TestCustomsRegNoForDisplay()
		{
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			cusCode.OK_CodeType = USACodeTypes.EmployerIdentificationNumber;
			cusCode.OK_CustomsRegNo = "12345678";
			AssertEquals("12345678", cusCode.CustomsRegNoForDisplay);
			cusCode.OK_CodeType = USACodeTypes.SocialSecurityNumber;
			cusCode.OK_CustomsRegNo = "123-45-6789";
			AssertEquals("123-45-6789", cusCode.CustomsRegNoForDisplay);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			cusCode.OK_CodeType = USACodeTypes.EmployerIdentificationNumber;
			cusCode.OK_CustomsRegNo = "12345678";
			AssertEquals("12345678", cusCode.CustomsRegNoForDisplay);
			cusCode.OK_CodeType = USACodeTypes.SocialSecurityNumber;
			cusCode.OK_CustomsRegNo = "123-45-6789";
			AssertEquals("***-**-****", cusCode.CustomsRegNoForDisplay);
		}

		public void TestCapitalisationOfCCP()
		{
			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_CodeType = "lkl";
			code.OK_CustomsRegNo = "abc123";
			AssertEquals("Not CCP so allows lower-case", "abc123", code.OK_CustomsRegNo);

			code.OK_CodeType = "CCP";
			AssertEquals("Changed to CCP so now upper-case", "ABC123", code.OK_CustomsRegNo);

			code.OK_CustomsRegNo = "zzz123";
			AssertEquals("Changed value of CCP so again upper-case", "ZZZ123", code.OK_CustomsRegNo);
		}

		public void TestUpperCaseFIRMS()
		{
			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_CodeType = USACodeTypes.FIRMSCode;
			code.OK_CustomsRegNo = "test";
			AssertEquals("Now upper-case", "TEST", code.OK_CustomsRegNo);
		}

		public void TestUpperCaseACASCode()
		{
			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_CodeType = USACodeTypes.ACASOriginatorCode;
			code.OK_CustomsRegNo = "test";
			AssertEquals("Now upper-case", "TEST", code.OK_CustomsRegNo);
		}

		public void TestLogging()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			AssertEquals("Initial Logs count", 1, orgHeader.Logs.GetAllLogs().Count);

			var codeForDelete = orgHeader.CustomsCodes.AddNew();
			orgHeader.CustomsCodes.RemoveAndDelete(codeForDelete);

			Factory.Save();
			AssertEquals("Delete data not in database will not add log", 1, orgHeader.Logs.GetAllLogs().Count);

			var code = orgHeader.CustomsCodes.AddNew();
			var country = Factory.NewWithValidTestData<RefCountry>();
			code.OK_CustomsRegNo = "9999";
			code.OK_CodeType = "lkl";
			code.OK_RN_NKCodeCountry = country.Code;
			Factory.Save();

			AssertEquals("Logs count", 2, orgHeader.Logs.GetAllLogs().Count);
			var expectedReference = "Registration No. 9999 Country: " + country.RN_Code + " Type: lkl";
			AssertEquals("Log should have reference", expectedReference, orgHeader.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);

			code.OK_CustomsRegNo = "888";
			code.OK_CodeType = "lll";

			Factory.Save();

			AssertEquals("Logs count", 3, orgHeader.Logs.GetAllLogs().Count);
			expectedReference = "Registration No. 888(9999) Country: " + country.RN_Code + " Type: lll(lkl)";
			AssertEquals("Log should have reference", expectedReference, orgHeader.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);

			orgHeader.CustomsCodes.RemoveAndDelete(code);

			Factory.Save();

			AssertEquals("Logs count", 4, orgHeader.Logs.GetAllLogs().Count);
			expectedReference = "Registration No. 888 Country: " + country.RN_Code + " Type: lll";
			AssertEquals("Log should have reference", expectedReference, orgHeader.Logs.MostRecentLogByEventTime(Events.DeletedARecordInTheSystem).SL_Reference);
		}

		public void TestDeduplicationStarted()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			cusCode.Delete(); // from setup

			var country = Factory.NewWithValidTestData<RefCountry>();

			var code = Factory.NewWithValidTestData<OrgCusCode>();
			var isFindingDuplicates = false;
			var orgHeader = code.Header;
			((IDeduplicatable)orgHeader).ShouldRunDeduplication = true;
			orgHeader.DeduplicationStarted += (o, e) => isFindingDuplicates = true;
			code.OK_CustomsRegNo = "9999";
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			code.OK_CodeType = "lkl";
			code.OK_RN_NKCodeCountry = country.Code;
			code.OK_CustomsRegNo = "888";
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			code.OK_CodeType = "lll";
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			code.Delete();
			AssertEquals(true, isFindingDuplicates);
			isFindingDuplicates = false;

			var code2 = orgHeader.CustomsCodes.AddNew();
			code2.OK_OH = ZGuid.Empty;
			code2.OK_CodeType = "lll";
			AssertEquals(false, isFindingDuplicates);
			code2.OK_CustomsRegNo = "lll";
			AssertEquals(false, isFindingDuplicates);
			code2.Delete();
			AssertEquals(false, isFindingDuplicates);

			var code3 = orgHeader.CustomsCodes.AddNew();
			((IDeduplicatable)orgHeader).ShouldRunDeduplication = false;
			code3.OK_CodeType = "111";
			AssertEquals(false, isFindingDuplicates);
			code3.OK_CustomsRegNo = "lll";
			AssertEquals(false, isFindingDuplicates);
			code3.Delete();
			AssertEquals(false, isFindingDuplicates);
		}

		public void TestOnSavingDeletesNewObject()
		{
			cusCode.Delete(); // from setup

			var country = Factory.NewWithValidTestData<RefCountry>();

			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_CustomsRegNo = "";
			code.OK_CodeType = "lkl";
			code.OK_RN_NKCodeCountry = country.Code;

			Factory.Save();

			Assert(code.IsDeleted);
		}

		public void TestOnSavingDeletesExistingObject()
		{
			cusCode.Delete(); // from setup

			var country = Factory.NewWithValidTestData<RefCountry>();

			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_CustomsRegNo = "123";
			code.OK_CodeType = "lkl";
			code.OK_RN_NKCodeCountry = country.Code;

			Factory.Save();

			Assert(code.IsInDatabase);
			Assert(!code.IsDeleted);
			code.OK_CustomsRegNo = "";
			Factory.Save();
			Assert(code.IsDeleted);
		}

		public void TestCountryIs()
		{
			var testCode1 = CreateCodeForTest("IN");
			var testCode2 = CreateCodeForTest("AU");
			var testCode3 = CreateCodeForTest("XX");
			var testCode4 = CreateCodeForTest("");

			Assert("TestCode1 should be true", testCode1.CountryIs("IN"));
			Assert("TestCode1 should be false", !testCode1.CountryIs("AU"));

			Assert("TestCode2 should be true", testCode2.CountryIs("AU"));
			Assert("TestCode2 should be false", !testCode2.CountryIs("IN"));

			Assert("TestCode3 should be false", !testCode3.CountryIs("XX"));
			Assert("TestCode3 should be false", !testCode3.CountryIs("AU"));

			Assert("TestCode4 should be false", !testCode4.CountryIs(""));
			Assert("TestCode4 should be false", !testCode4.CountryIs("AU"));
		}

		public void TestOK_CustomsRegNoIsTrimmed()
		{
			var testCode = CreateCodeForTest("CCC");
			testCode.OK_CustomsRegNo = ' ' + "TestCode" + ' ';
			AssertEquals("TestCode", testCode.OK_CustomsRegNo);
		}

		public void TestOK_CustomsRegNoMaxLength()
		{
			AssertNoExceptionThrown(() =>
			{
				cusCode.OK_CustomsRegNo = new string('a', 254);
				Factory.Save();
			});

			AssertEquals(254, cusCode.OK_CustomsRegNoInfo.MaxLength);
		}

		public void TestOK_CustomsRegNoTrimMaxLengthBeforeSetForSpecificType()
		{
			var longCode = new string('a', 300);

			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(254, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(35, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_CodeType = CodeTypes.TruckCarrierCode;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(35, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(4, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_CodeType = IcelandCodeTypes.CustomsOfficeCode;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(5, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_CodeType = SouthAfricaCodeTypes.CustomsApprovedExporter;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(8, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(25, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.YFK;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(13, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;

			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(4, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_CodeType = CodeTypes.TruckCarrierCode;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(4, cusCode.OK_CustomsRegNo.Length);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Turkey;

			cusCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.EOR;
			cusCode.OK_CustomsRegNo = longCode;
			AssertEquals(17, cusCode.OK_CustomsRegNo.Length);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestTRICusCodeShouldBeReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.BoleroTitleRegisterID;
			cusCode.OK_CustomsRegNo = "TRI123456";
			Factory.Save();

			AssertOrgCusCodeFieldsReadOnly(cusCode, false);

			var maaLog = org.Logs.AddNew();
			using (maaLog.LockForUpdatingKeyFieldsForTesting())
			{
				maaLog.SL_SE_NKEvent = Events.MessageAccepted.Code;
				maaLog.SL_Reference = "|DEP=Bolero|LOC=AU|MST=Enrollment Request|RFN=TRI123456";
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				AssertOrgCusCodeFieldsReadOnly(cusCode, false);
			}

			AssertOrgCusCodeFieldsReadOnly(cusCode, true);
		}

		public void TestReadOnlySecurity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var oldSecurityModifyFinancialARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyFinancialNonARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyNonFinancialARAPRegistrationNumbers = Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyNonFinancialNonARAPRegistrationNumbers = Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = false;

			try
			{
				var cusCode1 = org.CustomsCodes.AddNew();
				org.OH_IsDebtor = true;
				org.OH_IsCreditor = true;
				cusCode1.OK_CustomsRegNo = "24097959902";
				cusCode1.OK_RN_NKCodeCountry = "AU";
				cusCode1.OK_CodeType = AustraliaCodeTypes.AustralianBusinessNumber;
				Factory.Save();
				AssertOrgCusCodeFieldsReadOnly(cusCode1, true);

				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = true;
				AssertOrgCusCodeFieldsReadOnly(cusCode1, false);

				var cusCode2 = org.CustomsCodes.AddNew();
				cusCode2.OK_CustomsRegNo = "123456";
				cusCode2.OK_RN_NKCodeCountry = "AU";
				cusCode2.OK_CodeType = CodeTypes.WorldCargoAssociationNumber;
				Factory.Save();
				AssertOrgCusCodeFieldsReadOnly(cusCode2, true);

				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = true;
				AssertOrgCusCodeFieldsReadOnly(cusCode2, false);

				org.OH_IsDebtor = false;
				org.OH_IsCreditor = false;
				var cusCode3 = Factory.Load<OrgCusCode>(cusCode1.PK);
				AssertOrgCusCodeFieldsReadOnly(cusCode3, true);

				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				AssertOrgCusCodeFieldsReadOnly(cusCode3, false);

				var cusCode4 = Factory.Load<OrgCusCode>(cusCode2.PK);
				AssertOrgCusCodeFieldsReadOnly(cusCode4, true);

				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				AssertOrgCusCodeFieldsReadOnly(cusCode4, false);
			}
			finally
			{
				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = oldSecurityModifyFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyFinancialNonARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialNonARAPRegistrationNumbers;
			}
		}

		public void TestReadOnlySecurityForExternal()
		{
			var oldExternalDebtorValue = Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed;
			var oldExternalCreditorValue = Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed;

			try
			{
				var testCode = OrgInDB.CustomsCodes.AddNew();
				testCode.OK_CustomsRegNo = "32326326236";
				testCode.OK_CodeType = CodeTypes.GovBusinessCode;
				OrgInDB.Factory.Save();

				Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed = false;
				testCode.OK_CodeType = CodeTypes.ExternalDebtorAccountCode;
				AssertOrgCusCodeFieldsReadOnly(testCode, true);

				Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed = false;
				testCode.OK_CodeType = CodeTypes.ExternalCreditorAccountCode;
				AssertOrgCusCodeFieldsReadOnly(testCode, true);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed = oldExternalDebtorValue;
				Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed = oldExternalCreditorValue;
			}
		}

		public void TestOK_CodeType()
		{
			var uSTerritories = GetUsaTerritories();
			foreach (var country in uSTerritories)
			{
				cusCode.OK_CodeType = CodeTypes.GSTCode;
				AssertEquals("OK_CustomsRegNoInfo.MaxLength", 254, cusCode.OK_CustomsRegNoInfo.MaxLength);
				var expectedMaxLength = country.Code == Constants.CountryCodes.UnitedStates ? 4 : 35;
				cusCode.OK_RN_NKCodeCountry = country.Code;
				cusCode.OK_CodeType = CodeTypes.CarrierCode;
				AssertEquals("OK_CustomsRegNoInfo.MaxLength", expectedMaxLength, cusCode.OK_CustomsRegNoInfo.MaxLength);

				cusCode.OK_CodeType = CodeTypes.TruckCarrierCode;
				AssertEquals("OK_CustomsRegNoInfo.MaxLength", expectedMaxLength, cusCode.OK_CustomsRegNoInfo.MaxLength);
			}

			var puertoRicoCode = Constants.CountryCodes.PuertoRico;
			cusCode.OK_CodeType = CodeTypes.GSTCode;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength", 254, cusCode.OK_CustomsRegNoInfo.MaxLength);

			cusCode.OK_RN_NKCodeCountry = puertoRicoCode;
			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength", 35, cusCode.OK_CustomsRegNoInfo.MaxLength);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedArabEmirates;
			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength", 3, cusCode.OK_CustomsRegNoInfo.MaxLength);

			cusCode.OK_CodeType = CodeTypes.TruckCarrierCode;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength", 35, cusCode.OK_CustomsRegNoInfo.MaxLength);

			cusCode.OK_CodeType = CodeTypes.BrokerageSiteID;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength", 254, cusCode.OK_CustomsRegNoInfo.MaxLength);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength with US for CargoWiseOneCarrierCode", 4, cusCode.OK_CustomsRegNoInfo.MaxLength);

			cusCode.OK_RN_NKCodeCountry = string.Empty;
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength without country code for CargoWiseOneCarrierCode", 4, cusCode.OK_CustomsRegNoInfo.MaxLength);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = CodeTypes.CargoWiseRoadTransportProviderCode;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength with US for CargoWiseRoadTransportProviderCode", 15, cusCode.OK_CustomsRegNoInfo.MaxLength);

			cusCode.OK_RN_NKCodeCountry = string.Empty;
			cusCode.OK_CodeType = CodeTypes.CargoWiseRoadTransportProviderCode;
			AssertEquals("OK_CustomsRegNoInfo.MaxLength without country code for CargoWiseRoadTransportProviderCode", 15, cusCode.OK_CustomsRegNoInfo.MaxLength);
		}

		public void TestSCACAddedInRegistrationCodes_WithUSCountryAndValidSCACCodeAndNVO_ShouldCreateC1C()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_IsNVO = true;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "1234";

			var newC1C = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CargoWiseOneCarrierCode).FirstOrDefault();

			AssertNotNull(newC1C);

			var result = newC1C;

			AssertEquals(2, organisation.CustomsCodes.Count);

			AssertEquals(CodeTypes.CargoWiseOneCarrierCode, result.OK_CodeType);
			AssertEquals(shippingLine.RSL_CargoWiseOneCode, result.OK_CustomsRegNo);
			AssertEquals(organisation.PK, result.OK_OH);
			AssertEquals(Constants.CountryCodes.UnitedStates, result.OK_RN_NKCodeCountry);
			AssertEquals("IsSeaWholesaler", true, organisation.OH_IsSeaWholesaler);
			AssertEquals("IsShippingLine", false, organisation.OH_IsShippingLine);
		}

		public void TestSCACAddedInRegistrationCodes_WithUSCountryAndValidSCACCodeAndNotNVO_ShouldCreateC1C()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_IsNVO = false;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "1234";

			var newC1C = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CargoWiseOneCarrierCode).FirstOrDefault();

			AssertNotNull(newC1C);

			var result = newC1C;

			AssertEquals(CodeTypes.CargoWiseOneCarrierCode, result.OK_CodeType);
			AssertEquals(shippingLine.RSL_CargoWiseOneCode, result.OK_CustomsRegNo);
			AssertEquals(organisation.PK, result.OK_OH);
			AssertEquals(Constants.CountryCodes.UnitedStates, result.OK_RN_NKCodeCountry);
			AssertEquals("IsSeaWholesaler", false, organisation.OH_IsSeaWholesaler);
			AssertEquals("IsShippingLine", true, organisation.OH_IsShippingLine);
		}

		public void TestSCACAddedInRegistrationCodes_WithNonUSCountryAndValidSCACCodeAndNVO_ShouldNotCreateC1C()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_IsNVO = true;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CustomsRegNo = "1234";

			var newC1C = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CargoWiseOneCarrierCode).FirstOrDefault();

			AssertNull(newC1C);

			AssertEquals("IsSeaWholesaler", false, organisation.OH_IsSeaWholesaler);
			AssertEquals("IsShippingLine", false, organisation.OH_IsShippingLine);
		}

		public void TestSCACAddedInRegistrationCodes_WithNonUSCountryAndValidSCACCodeAndNotNVO_ShouldNotCreateC1C()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_IsNVO = false;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CustomsRegNo = "1234";

			var newC1C = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CargoWiseOneCarrierCode).FirstOrDefault();

			AssertNull(newC1C);

			AssertEquals("IsSeaWholesaler", false, organisation.OH_IsSeaWholesaler);
			AssertEquals("IsShippingLine", false, organisation.OH_IsShippingLine);
		}

		public void TestSCACAddedInRegistrationCodes_IsInvalidSCACCode()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "1235";
			shippingLine.RSL_IsNVO = false;
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "1234";

			var newC1C = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CargoWiseOneCarrierCode).FirstOrDefault();

			AssertNull(newC1C);
			AssertHasWarnings(cusCode.OK_CustomsRegNoInfo);
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, "This is not a valid SCAC. Refer to Shipping Lines under Reference Files for a list of valid codes.");
		}

		public void TestC1CAddedInRegistrationCodes_WhenNonUS_And_IsValidSCACCodeAndNVO_ShouldCreateUSSCAC()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_IsNVO = true;
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CustomsRegNo = "c123";

			var newSCAC = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CarrierCode).FirstOrDefault();

			AssertNotNull(newSCAC);

			var result = newSCAC;

			AssertEquals(CodeTypes.CarrierCode, result.OK_CodeType);
			AssertEquals(shippingLine.RSL_StandardCarrierAlphaCode, result.OK_CustomsRegNo);
			AssertEquals(organisation.PK, result.OK_OH);
			AssertEquals(Constants.CountryCodes.UnitedStates, result.OK_RN_NKCodeCountry);
			AssertEquals("IsSeaWholesaler", true, organisation.OH_IsSeaWholesaler);
			AssertEquals("IsShippingLine", false, organisation.OH_IsShippingLine);
		}

		public void TestC1CAddedInRegistrationCodes_WhenNonUS_And_IsValidSCACCodeAndNotNVO_ShouldCreateUSSCAC()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_IsNVO = false;
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CustomsRegNo = "c123";

			var newSCAC = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CarrierCode).FirstOrDefault();

			AssertNotNull(newSCAC);

			var result = newSCAC;

			AssertEquals(CodeTypes.CarrierCode, result.OK_CodeType);
			AssertEquals(shippingLine.RSL_StandardCarrierAlphaCode, result.OK_CustomsRegNo);
			AssertEquals(organisation.PK, result.OK_OH);
			AssertEquals(Constants.CountryCodes.UnitedStates, result.OK_RN_NKCodeCountry);
			AssertEquals("IsSeaWholesaler", false, organisation.OH_IsSeaWholesaler);
			AssertEquals("IsShippingLine", true, organisation.OH_IsShippingLine);
		}

		public void TestC1CAddedInRegistrationCodes_WhenUS_And_IsValidSCACCodeAndNVO_ShouldCreateUSSCAC()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_IsNVO = true;
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "c123";

			var newSCAC = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CarrierCode).FirstOrDefault();

			AssertNotNull(newSCAC);

			var result = newSCAC;

			AssertEquals(CodeTypes.CarrierCode, result.OK_CodeType);
			AssertEquals(shippingLine.RSL_StandardCarrierAlphaCode, result.OK_CustomsRegNo);
			AssertEquals(organisation.PK, result.OK_OH);
			AssertEquals(Constants.CountryCodes.UnitedStates, result.OK_RN_NKCodeCountry);
			AssertEquals("IsSeaWholesaler", true, organisation.OH_IsSeaWholesaler);
			AssertEquals("IsShippingLine", false, organisation.OH_IsShippingLine);
		}

		public void TestC1CAddedInRegistrationCodes_WhenUS_And_IsValidSCACCodeAndNotNVO_ShouldCreateUSSCAC()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_IsNVO = false;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "c123";

			var newSCAC = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CarrierCode).FirstOrDefault();

			AssertNotNull(newSCAC);

			var result = newSCAC;

			AssertEquals(CodeTypes.CarrierCode, result.OK_CodeType);
			AssertEquals(shippingLine.RSL_StandardCarrierAlphaCode, result.OK_CustomsRegNo);
			AssertEquals(organisation.PK, result.OK_OH);
			AssertEquals(Constants.CountryCodes.UnitedStates, result.OK_RN_NKCodeCountry);
			AssertEquals("IsSeaWholesaler", false, organisation.OH_IsSeaWholesaler);
			AssertEquals("IsShippingLine", true, organisation.OH_IsShippingLine);
		}

		public void TestC1CAddedInRegistrationCodes_IsValidC1C_InvalidSCACCodeAndNotNVO()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c124";
			shippingLine.RSL_StandardCarrierAlphaCode = ZString.Empty;
			shippingLine.RSL_IsNVO = false;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_CustomsRegNo = "c124";

			var newSCAC = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CarrierCode).FirstOrDefault();

			AssertNull(newSCAC);
			AssertHasWarnings(cusCode.OK_CustomsRegNoInfo);
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, "This SCAC does not match the chosen C1C code. Please delete or correct this entry.");
		}

		public void TestC1CAddedInRegistrationCodes_IsValidC1C_InvalidSCACCodeAndNVO()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_StandardCarrierAlphaCode = ZString.Empty;
			shippingLine.RSL_IsNVO = true;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_CustomsRegNo = "c123";

			var newSCAC = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CarrierCode).FirstOrDefault();

			AssertNull(newSCAC);
			AssertHasWarnings(cusCode.OK_CustomsRegNoInfo);
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, "This SCAC does not match the chosen C1C code. Please delete or correct this entry.");
		}

		public void TestC1CAddedInRegistrationCodes_IsInvalidC1C_InvalidSCACCodeAndNVO()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c123";
			shippingLine.RSL_StandardCarrierAlphaCode = ZString.Empty;
			shippingLine.RSL_IsNVO = true;

			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_CustomsRegNo = "c134";

			var newSCAC = organisation.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.CarrierCode).FirstOrDefault();

			AssertNull(newSCAC);
			AssertHasWarnings(cusCode.OK_CustomsRegNoInfo);
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, "This is not a valid SCAC. Refer to Shipping Lines under Reference Files for a list of valid codes.");
		}

		public void TestCustomsRegNoFieldType()
		{
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.Text), cusCode.CustomsRegNoFieldType);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.NewZealand;
			cusCode.OK_CodeType = CodeTypes.AgentCode;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.Text), cusCode.CustomsRegNoFieldType);

			cusCode.OK_CodeType = CodeTypes.SupplierCode;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.TextCodeFindBox), cusCode.CustomsRegNoFieldType);

			var uSTerritories = GetUsaTerritories();
			foreach (var country in uSTerritories)
			{
				cusCode.OK_RN_NKCodeCountry = country.Code;
				AssertEquals("CustomsRegNoFieldType", nameof(FieldType.Text), cusCode.CustomsRegNoFieldType);
			}
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.PuertoRico;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.Text), cusCode.CustomsRegNoFieldType);

			cusCode.OK_CodeType = USACodeTypes.NMFCParticipant;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.TextDropEdit), cusCode.CustomsRegNoFieldType);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.Text), cusCode.CustomsRegNoFieldType);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Singapore;
			cusCode.OK_CodeType = SingaporeCodeTypes.PartyStatusType;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.TextDropEdit), cusCode.CustomsRegNoFieldType);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CodeType = CodeTypes.CustomsClientCode;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.TextDropEdit), cusCode.CustomsRegNoFieldType);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit;
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.TextCodeFindBox), cusCode.CustomsRegNoFieldType);
		}

		public void TestCustomsRegNoLookupList()
		{
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			AssertEquals("CustomsRegNoLookupList should be SupplierList for non US countries and non nmfc participant", cusCode.Lookups.NZCSupplier_List, cusCode.CustomsRegNoLookupList);

			var uSTerritories = GetUsaTerritories();
			foreach (var country in uSTerritories)
			{
				cusCode.OK_RN_NKCodeCountry = country.Code;
				AssertEquals("CustomsRegNoLookupList should be SupplierList for US countries but non nmfc participant", cusCode.Lookups.NZCSupplier_List, cusCode.CustomsRegNoLookupList);
			}
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.PuertoRico;
			AssertEquals("CustomsRegNoLookupList should be SupplierList for US and PuertoR Rico countries but non nmfc participant", cusCode.Lookups.NZCSupplier_List, cusCode.CustomsRegNoLookupList);

			cusCode.OK_CodeType = USACodeTypes.NMFCParticipant;
			AssertEquals("CustomsRegNoLookupList should be NMFCParticipantList for US and nmfc participant", cusCode.Lookups.NMFCParticipantList, cusCode.CustomsRegNoLookupList);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			AssertEquals("CustomsRegNoLookupList should be SupplierList for non US countries but nmfc participant", cusCode.Lookups.NZCSupplier_List, cusCode.CustomsRegNoLookupList);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Singapore;
			cusCode.OK_CodeType = SingaporeCodeTypes.PartyStatusType;
			AssertEquals("CustomsRegNoLookupList should be SGPartyStatusTypeList for Singapore", cusCode.Lookups.SGPartyStatusTypeList, cusCode.CustomsRegNoLookupList);

			cusCode.OK_CodeType = SingaporeCodeTypes.DirectDelivery;
			AssertEquals("CustomsRegNoLookupList should be DIRList for Singapore", cusCode.Lookups.SGDirectDeliveryList, cusCode.CustomsRegNoLookupList);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CodeType = CodeTypes.CustomsClientCode;
			AssertEquals("CustomsRegNoLookupList should be CustomsSupervisingOfficeList for United Kingdom", cusCode.Lookups.CustomsSupervisingOfficeList, cusCode.CustomsRegNoLookupList);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit;
			AssertEquals("CustomsRegNoLookupList should be CustomsSupervisingOfficeList for United Kingdom", cusCode.Lookups.CustomsOfficeOfExitList, cusCode.CustomsRegNoLookupList);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.KoreaSouth;
			cusCode.OK_CodeType = KoreaSouthComplianceInfo.CodeTypes.IndustrialParkCode;
			AssertEquals("CustomsRegNoLookupList should be KRIndustrialParkCodeList for South Korea", cusCode.Lookups.KRIndustrialParkCodeList, cusCode.CustomsRegNoLookupList);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.KoreaSouth;
			cusCode.OK_CodeType = CodeTypes.ControlledPremisesID;
			AssertEquals("CustomsRegNoLookupList should be IsKRBondedAreaCodeList for South Korea", cusCode.Lookups.KRBondedAreaCodeList, cusCode.CustomsRegNoLookupList);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Japan;
			cusCode.OK_CodeType = CodeTypes.ControlledPremisesID;
			AssertEquals("CustomsRegNoLookupList should be JPCustomsControlledPremisesCodeList for Japan", cusCode.Lookups.JPCustomsControlledPremisesCodeList, cusCode.CustomsRegNoLookupList);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.KoreaSouth;
			cusCode.OK_CodeType = CodeTypes.CarrierCode;
			AssertEquals("CustomsRegNoLookupList should be IsKRCustomsCarrierCodeList for South Korea", cusCode.Lookups.KRCustomsCarrierCodeList, cusCode.CustomsRegNoLookupList);
		}

		public void TestSettingPatternRequiresRegen()
		{
			cusCode.Header.PatternMatchRequiresRegen = false;
			Assert(!cusCode.Header.PatternMatchRequiresRegen);
			cusCode.OK_CodeType = "ABC";
			Assert(cusCode.Header.PatternMatchRequiresRegen);

			cusCode.Header.PatternMatchRequiresRegen = false;
			Assert(!cusCode.Header.PatternMatchRequiresRegen);
			cusCode.OK_CustomsRegNo = "ABC";
			Assert(cusCode.Header.PatternMatchRequiresRegen);
		}

		public void TestPremiseAddressIsRequired()
		{
			cusCode.OK_RN_NKCodeCountry = ZString.Empty;
			AssertEquals("Default false if no country defined", false, cusCode.PremisesAddressIsRequired);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Eritrea;
			AssertEquals("Default false if no Validator defined for the country", false, cusCode.PremisesAddressIsRequired);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			AssertEquals("false if the type is not defined as required in the country's Validator", false, cusCode.PremisesAddressIsRequired);

			cusCode.OK_CodeType = AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			AssertEquals("true if the type is defined as required in the country's Validator", true, cusCode.PremisesAddressIsRequired);

			cusCode.OK_CodeType = AustraliaCodeTypes.ApprovedArrangementNumber;
			AssertEquals("ApprovedArrangementNumber premise id is defined as required in the country's Validator", true, cusCode.PremisesAddressIsRequired);

			cusCode.OK_CodeType = EuropeanUnionSharedCodeTypes.CustomsOfficeForExit;
			AssertEquals("In EU country,If CodeType is CEX,PremiseAddress is required", true, cusCode.PremisesAddressIsRequired);

			cusCode.OK_CodeType = EuropeanUnionSharedCodeTypes.TrustedTrader;
			AssertEquals("In EU country,If CodeType is TTD,PremiseAddress is required", true, cusCode.PremisesAddressIsRequired);
		}

		public void TestPremisesAddressIsAllowed()
		{
			var validPremiseAddressCodes = new HashSet<string>()
			{
				CodeTypes.CommercialAndGovernmentEntity,
				CodeTypes.ControlledPremisesID,
				CodeTypes.DepotControlledPremisesID,
				CodeTypes.GS1,
				CodeTypes.NorthAmericanIndustryClassificationSystem,
				CodeTypes.RegulatedAgentID,
				CodeTypes.WarehouseControlledPremisesID,
				CodeTypes.VGMRegistrationNumber,
				CodeTypes.DataUniversalNumberingSystem,
				CodeTypes.TerminalControlledPremisesID,
				CodeTypes.PortSystemNumber,
				CodeTypes.PortServiceReference,
				CodeTypes.NVOCCReference,
				CodeTypes.CustomsOfficeForTransit,
				CodeTypes.BoleroTitleRegisterID,
				CodeTypes.GSTCode,
				CodeTypes.ContainerChainCommunityCode
			};
			AssertPremisesAddressIsAllowed(typeof(CodeTypes), validPremiseAddressCodes, Constants.CountryCodes.AlandIslands); // Use CountryCodes.AlandIslands here as there's no specific rule implemented for Aland Islands yet.
		}

		public void TestPremisesAddressIsAllowedForDE()
		{
			var validPremiseAddressCodes = new HashSet<string>()
			{
				GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix,
				GermanyOrgCusCodeInfo.OrgCusCodes.CWC,
				GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode,
				GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode,
				GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber,
				GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber,
				GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice,
				GermanyOrgCusCodeInfo.OrgCusCodes.UST
			};
			AssertPremisesAddressIsAllowed(typeof(GermanyOrgCusCodeInfo.OrgCusCodes), validPremiseAddressCodes, Constants.CountryCodes.Germany);
		}

		public void TestPremisesAddressIsAllowedForNZ()
		{
			AssertPremisesAddressIsAllowed(typeof(NZCodeTypes), new HashSet<string>() { NZCodeTypes.ApprovedTransitionalFacility }, Constants.CountryCodes.NewZealand);
		}

		public void TestPremisesAddressIsAllowedForUS()
		{
			var validPremiseAddressCodes = new HashSet<string>()
			{
				USACodeTypes.ABIRoutingCode,
				USACodeTypes.CPSCAccreditedLabId,
				CodeTypes.DataUniversalNumberingSystem,
				USACodeTypes.DataUniversalNumberingSystemPlus4,
				USACodeTypes.DepartmentOfDefenseActivityAddressCode,
				CodeTypes.FDAEstablishmentIdentifier,
				USACodeTypes.FIRMSCode,
				USACodeTypes.FoodFacilityRegistrationNumber,
				USACodeTypes.ForeignProducerIdentifier,
				USACodeTypes.GlazingManufacturerCode,
				USACodeTypes.ManufacturerID,
				USACodeTypes.TireManufacturerCode,
				USACodeTypes.TTIRegistrationNumber,
				USACodeTypes.CarrierPrefixCode,
				USACodeTypes.CertifiedCargoScreening,
				USACodeTypes.LegalEntityIdentifier,
				USACodeTypes.GlobalLocationNumber,
				USACodeTypes.ForeignProducerIdentifierBeer,
				USACodeTypes.ForeignProducerIdentifierSpirits,
				USACodeTypes.ForeignProducerIdentifierWine,
				USACodeTypes.AirAMSOriginatorCode
			};
			AssertPremisesAddressIsAllowed(typeof(USACodeTypes), validPremiseAddressCodes, Constants.CountryCodes.UnitedStates);
		}

		public void TestPremisesAddressIsAllowedForEU()
		{
			AssertPremisesAddressIsAllowed(typeof(EuropeanUnionSharedCodeTypes), new HashSet<string>() { EuropeanUnionSharedCodeTypes.TraderID, EuropeanUnionSharedCodeTypes.CustomsOfficeForExit, EuropeanUnionSharedCodeTypes.TrustedTrader }, ZString.Empty);
		}

		public void TestPremisesAddressIsAllowedForGB()
		{
			AssertPremisesAddressIsAllowed(typeof(UnitedKingdomCodeTypes), new HashSet<string>() { UnitedKingdomCodeTypes.CTOShed, UnitedKingdomCodeTypes.EoriBranchSuffix }, Constants.CountryCodes.UnitedKingdom);
		}

		public void TestPremisesAddressIsAllowedForCA()
		{
			AssertPremisesAddressIsAllowed(typeof(CACodeTypes), new HashSet<string>() { CACodeTypes.CustomsOfficeCode, CACodeTypes.CSAReferenceID, CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax }, Constants.CountryCodes.Canada);
		}

		public void TestPremisesAddressIsAllowedForAU()
		{
			AssertPremisesAddressIsAllowed(typeof(AustraliaCodeTypes), new HashSet<string>() { AustraliaCodeTypes.AustralianBusinessNumber, AustraliaCodeTypes.ApprovedArrangementNumber }, Constants.CountryCodes.Australia);
		}

		public void TestPremisesAddressIsAllowedForID()
		{
			AssertPremisesAddressIsAllowed(typeof(IndonesiaCodeTypes), new HashSet<string>() { IndonesiaCodeTypes.PPN }, Constants.CountryCodes.Indonesia);
		}

		public void TestPremisesAddressIsAllowedForIN()
		{
			var validPremiseAddressCodes = new HashSet<string>()
			{
				OrgCusCode.CodeTypes.GSTCode,
				IndiaOrgCusCodeInfo.OrgCusCodes.BSN,
				IndiaOrgCusCodeInfo.OrgCusCodes.ADC
			};
			AssertPremisesAddressIsAllowed(typeof(IndiaOrgCusCodeInfo.OrgCusCodes), validPremiseAddressCodes, Constants.CountryCodes.India);
		}

		public void TestPremisesAddressIsAllowedForIT()
		{
			AssertPremisesAddressIsAllowed(typeof(ItalyOrgCusCodeInfo.OrgCusCodes), new HashSet<string>() { ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail }, Constants.CountryCodes.Italy);
		}

		public void TestPremisesAddressIsAllowedForCN()
		{
			cusCode.OK_CodeType = CodeTypes.VATCode;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.China;
			AssertEquals(true, cusCode.PremisesAddressIsAllowed);

			new Customs.CN.EnterpriseQualificationList().Cast<ICodeDescription>().ForEach(x =>
			{
				cusCode.OK_CodeType = x.Code;
				Assert(cusCode.PremisesAddressIsAllowed);
			});
		}

		public void TestPremisesAddressIsAllowedForTW()
		{
			var validPremiseAddressCodes = new HashSet<string>()
			{
				TaiwanCodeTypes.EPZ,
				TaiwanCodeTypes.CBF,
				TaiwanCodeTypes.FTZ,
				TaiwanCodeTypes.FactoryRegistrationNumber,
				TaiwanCodeTypes.AgriculturalTechnologyPark,
				TaiwanCodeTypes.SciencePark
			};
			AssertPremisesAddressIsAllowed(typeof(TaiwanCodeTypes), validPremiseAddressCodes, Constants.CountryCodes.Taiwan);

			cusCode.OK_CodeType = CodeTypes.FDAEstablishmentIdentifier;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Taiwan;
			AssertEquals(true, cusCode.PremisesAddressIsAllowed);
		}

		public void TestPremisesAddressIsAllowedForFR()
		{
			var validPremiseAddressCodes = new HashSet<string>()
			{
				FranceCodeTypes.CI5,
				FranceCodeTypes.SON,
				FranceCodeTypes.SOA,
				FranceCodeTypes.SOW,
				FranceCodeTypes.TVA,
				FranceCodeTypes.CIN,
				FranceCodeTypes.EoriBranchSuffix,
			};
			AssertPremisesAddressIsAllowed(typeof(FranceCodeTypes), validPremiseAddressCodes, Constants.CountryCodes.France);
		}

		public void TestIsPrimaryCodeTypeWithNoCodeOrCountry()
		{
			var code = CreateCodeForTest("");
			AssertEquals(false, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsPrimaryCodeTypeWithCountryAndNoCode()
		{
			var code = CreateCodeForTest("AU");
			AssertEquals(false, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsPrimaryCodeTypeWithCountryAndPrimaryCode()
		{
			var code = CreateCodeForTest("AU");
			code.OK_CodeType = AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsPrimaryCodeTypeWithCountryAndNonPrimaryCode()
		{
			var code = CreateCodeForTest("DE");
			code.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.GermanCivilAviationAuthority;
			AssertEquals(false, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsPrimaryCodeWithNoCountryAndCountrySpecificPrimaryCode()
		{
			var code = CreateCodeForTest("");
			code.OK_CodeType = AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals(false, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsPrimaryCodeWithNoCountryAndNonCountrySpecificPrimaryCode()
		{
			var code = CreateCodeForTest("");
			code.OK_CodeType = CodeTypes.GSTCode;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsPrimaryCodeWithCountryAndNonCountrySpecificPrimaryCode()
		{
			var code = CreateCodeForTest("AU");
			code.OK_CodeType = CodeTypes.CreditAgencyCode;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestForAllCountriesDefaultOrgCusCodesAreMarkedAsPrimary()
		{
			var countries = new RefCountryCollection(Factory);

			AssertNotEquals("Ref Country Collection should not be empty.", 0, countries.Count);

			CombineAssertions(() =>
			{
				foreach (var country in countries)
				{
					var countryCode = country.RN_Code;

					var cusCode = Factory.New<OrgCusCode>();
					cusCode.OK_RN_NKCodeCountry = countryCode;
					var validCusCodes = cusCode.Lookups.OK_CodeType_List;

					if (!consumptionCodeIsNotValidOrgCusCode_CountryList.Contains(countryCode))
					{
						var consumptionTaxRegistrationOrgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
						Assert(countryCode + ": consumptionTaxRegistrationOrgCusCode must be valid for each country, otherwise it should not be tested for this country and add the country to consumptionCodeIsNotValidOrgCusCode_CountryList", validCusCodes.ContainsCode(consumptionTaxRegistrationOrgCusCode));
						cusCode.OK_CodeType = consumptionTaxRegistrationOrgCusCode;
						Assert(countryCode + ": " + consumptionTaxRegistrationOrgCusCode + " should be primary\nIf this assertion is failing and GetPrimaryCusCodes() is used then check the following:\n1. base.GetPrimaryCusCodes() is called in the implementation of GetPrimaryCusCodes() \n2. If it still fails then see if GetConsumptionTaxRegistrationCode() is overriden with correct value", cusCode.IsCurrentCompanyCodeTypePrimary);
					}

					if (!localBusinessRegCodeIsNotValidOrgCusCode_CountryList.Contains(countryCode))
					{
						var localBusinessRegNoCodeType = country.LocalBusinessRegNoCodeType;
						Assert(countryCode + ": localBusinessRegNoCodeType must be valid for each country, otherwise it should not be tested for this country and add the country to localBusinessRegCodeIsNotValidOrgCusCode_CountryList", validCusCodes.ContainsCode(localBusinessRegNoCodeType));
						cusCode.OK_CodeType = localBusinessRegNoCodeType;
						Assert(countryCode + ": " + localBusinessRegNoCodeType + " should be primary\nIf this assertion is failing and GetPrimaryCusCodes() is used then check the following:\n1. base.GetPrimaryCusCodes() is called in the implementation of GetPrimaryCusCodes() \n2. If it still fails then see if GetLocalBusinessRegNoCodeType() is overriden with correct value", cusCode.IsCurrentCompanyCodeTypePrimary);
					}
				}
			});
		}

		public void TestIsPrimaryCodeWithCountryAndMainBusinessCode()
		{
			var code = CreateCodeForTest("AR");
			code.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsPrimaryCode_TR()
		{
			var code = CreateCodeForTest(Constants.CountryCodes.Turkey);
			code.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTE;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);

			code.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VDM;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);

			code.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.TradeRegistryNumber;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);

			code.OK_CodeType = CodeTypes.VATCode;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);

			code.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.YFK;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);

			code.OK_CodeType = CodeTypes.AccountsPayableSuppliersReference;
			AssertEquals(false, code.IsCurrentCompanyCodeTypePrimary);

			code.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.EOR;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsPrimaryCode_SV()
		{
			var code = CreateCodeForTest(Constants.CountryCodes.ElSalvador);
			code.OK_CodeType = ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);

			code.OK_CodeType = ElSalvadorOrgCusCodeInfo.OrgCusCodes.NIT;
			AssertEquals(true, code.IsCurrentCompanyCodeTypePrimary);

			code.OK_CodeType = CodeTypes.AccountsPayableSuppliersReference;
			AssertEquals(false, code.IsCurrentCompanyCodeTypePrimary);
		}

		public void TestIsOriginalCompanyCodeTypePrimary_Primary()
		{
			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_RN_NKCodeCountry = "AU";
			code.OK_CodeType = AustraliaCodeTypes.AustralianBusinessNumber;
			code.OK_CustomsRegNo = "24097959902";
			AssertEquals(true, code.IsOriginalCompanyCodeTypePrimary);
			Factory.Save();

			code.OK_CodeType = CodeTypes.WorldCargoAssociationNumber;
			AssertEquals(true, code.IsOriginalCompanyCodeTypePrimary);
		}

		public void TestIsOriginalCompanyCodeTypePrimary_NonPrimary()
		{
			var code = Factory.NewWithValidTestData<OrgCusCode>();
			code.OK_RN_NKCodeCountry = "AU";
			code.OK_CodeType = CodeTypes.WorldCargoAssociationNumber;
			code.OK_CustomsRegNo = "123456";
			AssertEquals(false, code.IsOriginalCompanyCodeTypePrimary);
			Factory.Save();

			code = Factory.Load<OrgCusCode>(code.PK);
			code.OK_CodeType = AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals(false, code.IsOriginalCompanyCodeTypePrimary);
		}

		public void TestIsNMFCParticipant()
		{
			var code = Factory.New<OrgCusCode>();
			AssertEquals("Precondition.", false, code.OK_CodeType.EqualsIgnoringCase(USACodeTypes.NMFCParticipant));
			AssertEquals("Precondition.", false, code.OK_CustomsRegNo.EqualsIgnoringCase(OrgConstants.NMFCParticipantCodes.Code.Yes));
			AssertEquals("Default should be false.", false, code.IsNMFCParticipant);

			code.OK_CodeType = USACodeTypes.NMFCParticipant;
			AssertEquals("Code is NMFC but RegNo is NO.", false, code.IsNMFCParticipant);

			code.OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.Yes;
			AssertEquals("Code is NMFC and RegNo is YES.", true, code.IsNMFCParticipant);

			code.OK_CodeType = USACodeTypes.ManufacturerID;
			AssertEquals("Code is Not NMFC and RegNo is YES.", false, code.IsNMFCParticipant);

			code.OK_CodeType = USACodeTypes.NMFCParticipant;
			code.OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.No;
			AssertEquals("Code is NMFC but RegNo is NO.", false, code.IsNMFCParticipant);
		}

		public void TestUppercaseTheRegCode()
		{
			var code = Factory.New<OrgCusCode>();

			AssertUppercaseRegCodeResult(Constants.CountryCodes.Australia, CodeTypes.ControlledPremisesID, "ABCD");
			AssertUppercaseRegCodeResult(Constants.CountryCodes.UnitedStates, USACodeTypes.FIRMSCode, "ABCD");
			AssertUppercaseRegCodeResult(Constants.CountryCodes.UnitedStates, USACodeTypes.TTBPermitNumber, "ABCD");
			AssertUppercaseRegCodeResult(Constants.CountryCodes.UnitedStates, USACodeTypes.DDTCRegistrationNumber, "ABCD");

			AssertUppercaseRegCodeResult(Constants.CountryCodes.Australia, CodeTypes.CarrierCode, "abcd");
			AssertUppercaseRegCodeResult(Constants.CountryCodes.UnitedStates, CodeTypes.CarrierCode, "ABCD");
			AssertUppercaseRegCodeResult(Constants.CountryCodes.UnitedStates, CodeTypes.TruckCarrierCode, "ABCD");

			code.OK_CodeType = "";
			code.OK_CustomsRegNo = "abcdef";
			AssertNoExceptionThrown(() => code.OK_CodeType = CodeTypes.CarrierCode);
			AssertEquals("Should not set OK_CustomsRegNo to upper when length larger than max length", "abcdef", code.OK_CustomsRegNo);

			void AssertUppercaseRegCodeResult(ZString countryCode, ZString codeType, ZString expectResult)
			{
				code.OK_RN_NKCodeCountry = countryCode;
				code.OK_CodeType = codeType;
				code.OK_CustomsRegNo = "abcd";
				AssertEquals(expectResult, code.OK_CustomsRegNo);
			}
		}

		public void TestCanDelete()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = CodeTypes.IVA;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Portugal;
			cusCode.OK_CustomsRegNo = "BJU39V";

			var ptCompany = Factory.NewWithValidTestData<GlbCompany>();
			ptCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;
			ptCompany.GC_IsGSTRegistered = false;

			var ptBranch = Factory.NewWithValidTestData<GlbBranch>();
			ptBranch.GB_GC = ptCompany.PK;

			var cc1 = Factory.NewWithValidTestData<AccChargeCode>();
			cc1.AC_Code = "CC1";
			cc1.AC_Desc = "CC1 Desc";
			cc1.AC_GC = ptBranch.GB_GC;
			cc1.AC_ChargeType = "MRG";

			Factory.Save();

			AssertEquals(true, cusCode.CanDelete);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				arInvoice.AH_TransactionType = TransactionTypes.Invoice;
				arInvoice.AH_OH = newOrg.PK;

				var arInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
				arInvoiceLine.AL_AH = arInvoice.PK;
				arInvoiceLine.AL_AT = ZGuid.Empty;
				arInvoiceLine.AL_AC = cc1.PK;
				arInvoiceLine.AL_LineAmount = 44;

				Factory.Save();
			}

			AssertEquals(false, cusCode.CanDelete);
			AssertEquals("You cannot delete this Code.At least one transaction has been posted in a Portugal Login Company in this database using this Organization.", cusCode.ReasonForNotAbleToDelete);
		}

		public void TestRequiresUniqueRegistrationNumberForDE()
		{
			AssertRequiresUniqueRegistrationNumber(typeof(GermanyOrgCusCodeInfo.OrgCusCodes), new HashSet<string>() { GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber, GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice });
		}

		public void TestIEInvoicingEligibilityLiteRegistrationCodeMembers()
		{
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			var asEligibility = (IEInvoicingEligibilityLiteRegistrationCode)cusCode;

			AssertEquals("OK_RN_NKCodeCountry / CountryCode", cusCode.OK_RN_NKCodeCountry, asEligibility.CountryCode);
			AssertEquals("OK_CodeType / CodeType", cusCode.OK_CodeType, asEligibility.CodeType);
			AssertEquals("OK_CustomsRegNo / RegistrationNumber", cusCode.OK_CustomsRegNo, asEligibility.RegistrationNumber);

			cusCode.OK_RN_NKCodeCountry = "12";
			cusCode.OK_CodeType = "999";
			cusCode.OK_CustomsRegNo = "NotARealRegistrationNumber";
			AssertEquals("OK_RN_NKCodeCountry / CountryCode", "12", asEligibility.CountryCode);
			AssertEquals("OK_CodeType / CodeType", "999", asEligibility.CodeType);
			AssertEquals("OK_CustomsRegNo / RegistrationNumber", "NotARealRegistrationNumber", asEligibility.RegistrationNumber);
		}

		public void TestOK_CustomsRegNoInfoReadOnly()
		{
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			AssertEquals("OK_CustomsRegNoInfo.ReadOnly with BR code for CNPJ", false, cusCode.OK_CustomsRegNoInfo.ReadOnly);

			cusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ;
			AssertEquals("OK_CustomsRegNoInfo.ReadOnly without country code for RootCNPJ", true, cusCode.OK_CustomsRegNoInfo.ReadOnly);

			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			AssertEquals("OK_CustomsRegNoInfo.ReadOnly without BR code for RootCNPJ", false, cusCode.OK_CustomsRegNoInfo.ReadOnly);
		}

		public void TestSyncBrazilRootCNPJFromCNPJIfNeeded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "COMPANY";

			var cnpjCode = org.CustomsCodes.AddNew();
			cnpjCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cnpjCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			cnpjCode.OK_CustomsRegNo = "03.142.520/0001-27";

			var cnpjRootCode = org.CustomsCodes.AddNew();
			cnpjRootCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cnpjRootCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ;
			AssertEquals("BR RTC sync from CNPJ", "03142520", cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.OK_CustomsRegNo = "25043511000111";
			AssertEquals("BR CNPJ number changed", "25043511", cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			AssertEquals("BR CNPJ changed to CA CNPJ", ZString.Empty, cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.OK_CustomsRegNo = "03.142.520/0001-27";
			AssertEquals("BR RTC not sync from CA CNPJ", ZString.Empty, cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			AssertEquals("BR RTC sync from BR CNPJ", "03142520", cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CMT;
			AssertEquals("BR RTC cleared when no BR CNPJ exists", ZString.Empty, cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.OK_CustomsRegNo = "25043511000111";
			AssertEquals("BR RTC not sync from BR CMT", ZString.Empty, cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			AssertEquals("BR RTC sync from BR CNPJ", "25043511", cnpjRootCode.OK_CustomsRegNo);

			cnpjRootCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.AEO;
			AssertEquals("BR RTC changed to BR AEO", "25043511", cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.OK_CustomsRegNo = "03.142.520/0001-27";
			AssertEquals("BR AEO not sync from BR CNPJ", "25043511", cnpjRootCode.OK_CustomsRegNo);

			cnpjRootCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ;
			AssertEquals("BR RTC sync from CNPJ", "03142520", cnpjRootCode.OK_CustomsRegNo);

			cnpjRootCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			cnpjCode.OK_CustomsRegNo = "25043511000111";
			AssertEquals("CA RTC not sync from BR CNPJ", "03142520", cnpjRootCode.OK_CustomsRegNo);

			cnpjRootCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			AssertEquals("BR RTC sync from BR CNPJ", "25043511", cnpjRootCode.OK_CustomsRegNo);

			cnpjCode.Delete();
			AssertEquals("BR RTC cleared when no BR CNPJ exists", ZString.Empty, org.CustomsCodes.GetCustomsRegNo(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ));
		}

		public void TestDeleteOrgHeaderWithBRCodes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cnpjCode = org.CustomsCodes.AddNew();
			cnpjCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cnpjCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			cnpjCode.OK_CustomsRegNo = "03.142.520/0001-27";

			var cnpjRootCode = org.CustomsCodes.AddNew();
			cnpjRootCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cnpjRootCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ;

			var mexicoCode = org.CustomsCodes.AddNew();
			mexicoCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Mexico;
			mexicoCode.OK_CodeType = MexicoOrgCusCodeInfo.OrgCusCodes.RFC;
			mexicoCode.OK_CustomsRegNo = "03142520000127";

			Factory.Save();

			org = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			AssertNoExceptionThrown(org.Delete);
		}

		public void TestPremisesAddressReadOnly()
		{
			CombineAssertions(() =>
			{
				var orgCusCode = CreateCodeForTest(CountryCodes.Germany);
				orgCusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.IMA;
				Assert("Premises Adress is disabled", orgCusCode.OK_OA_PremisesAddressInfo.ReadOnly);
				orgCusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice;
				Assert("Premises Adress is enabled", !orgCusCode.OK_OA_PremisesAddressInfo.ReadOnly);
			});
		}

		#region TestScreeningStatuses_InvalidatedByLocalDataChanges

		public void TestInvalidateByLocalDataChanges_ShouldInvalidateScreeningStatus()
		{
			var logCount = 0;
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = CodeTypes.BrokeragePrinter;
			orgCusCode.OK_CustomsRegNo = "AUSCUS_AU";
			orgCusCode.OK_RN_NKCodeCountry = "AU";

			Factory.Save();

			CombineAssertions("Should invalidate screening status and create screening log", () =>
			{
				AssertHasInvalidatedScreeningStatuses(orgCusCode.OK_CodeTypeInfo, CodeTypes.BrokerageRegistration, ScreeningStatusesList.Codes.Clear);
				AssertHasInvalidatedScreeningStatuses(orgCusCode.OK_CustomsRegNoInfo, "AUSCUSOS_AU", ScreeningStatusesList.Codes.Clear);
				AssertHasInvalidatedScreeningStatuses(orgCusCode.OK_RN_NKCodeCountryInfo, "US", ScreeningStatusesList.Codes.Matched);
			});

			void AssertHasInvalidatedScreeningStatuses(ZPropertyInfo propertyInfo, string value, string screeningStatus)
			{
				logCount++;
				header.OH_ScreeningStatus = screeningStatus;
				propertyInfo.SetValueFromString(value);

				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges, header.ScreeningLogCollection[logCount - 1].PJ_Status);
				AssertEquals(logCount, header.ScreeningLogCollection.Count);
			}
		}

		public void TestInvalidateByLocalDataChanges_ShouldNotInvalidateScreeningStatus()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = CodeTypes.BrokeragePrinter;
			orgCusCode.OK_CustomsRegNo = "AUSCUS_AU";
			orgCusCode.OK_RN_NKCodeCountry = "AU";

			Factory.Save();

			CombineAssertions("Should not invalidate screening status", () =>
			{
				AssertHasInvalidatedScreeningStatuses(orgCusCode.OK_OA_PremisesAddressInfo, ScreeningStatusesList.Codes.Unknown);
				AssertHasInvalidatedScreeningStatuses(orgCusCode.OK_OA_PremisesAddressInfo, ScreeningStatusesList.Codes.Clear);
				AssertHasInvalidatedScreeningStatuses(orgCusCode.OK_OA_PremisesAddressInfo, ScreeningStatusesList.Codes.Matched);
			});

			void AssertHasInvalidatedScreeningStatuses(ZPropertyInfo propertyInfo, string screeningStatus)
			{
				header.OH_ScreeningStatus = screeningStatus;
				propertyInfo.SetValueFromString("hello");
				Factory.Save();

				AssertEquals(screeningStatus, header.OH_ScreeningStatus);
				AssertEquals(0, header.ScreeningLogCollection.Count);
			}
		}

		#endregion

		#region TestScreeningStatus_ShouldChangeAfterDeleteCusCode

		public void TestScreeningStatus_NotClearOrg_DeleteCusCode()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = CodeTypes.BrokeragePrinter;
			orgCusCode.OK_CustomsRegNo = "AUSCUS_AU";
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			Factory.Save();

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Matched, header.OH_ScreeningStatus);

			orgCusCode.Delete();
			Factory.Save();

			AssertEquals("Screening status reset after delete", ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
		}

		public void TestScreeningStatus_ClearOrg_DeleteCusCode()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = CodeTypes.BrokeragePrinter;
			orgCusCode.OK_CustomsRegNo = "AUSCUS_AU";
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			Factory.Save();

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			orgCusCode.Delete();
			Factory.Save();

			AssertEquals("Screening status remains clear after delete", ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);
		}

		#endregion

		#region TestScreeningStatus_AddCusCode

		public void TestScreeningStatus_MatchedOrg_AddCusCode()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = CodeTypes.BrokeragePrinter;
			orgCusCode.OK_CustomsRegNo = "AUSCUS_AU";
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			Factory.Save();

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("Pre-condition", ScreeningStatusesList.Codes.Matched, header.OH_ScreeningStatus);

			var orgCusCode2 = header.CustomsCodes.AddNew();
			orgCusCode2.OK_CodeType = CodeTypes.BrokerageRegistration;
			orgCusCode2.OK_CustomsRegNo = "AUSCUSOS_AU";
			orgCusCode2.OK_RN_NKCodeCountry = "AU";
			Factory.Save();

			AssertEquals("Screening status still Matched after adding cusCode", ScreeningStatusesList.Codes.Matched, header.OH_ScreeningStatus);
		}

		public void TestScreeningStatus_NotMatchedOrg_AddCusCode()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = CodeTypes.BrokeragePrinter;
			orgCusCode.OK_CustomsRegNo = "AUSCUS_AU";
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			Factory.Save();

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			AssertEquals("Pre-condition", ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			var orgCusCode2 = header.CustomsCodes.AddNew();
			orgCusCode2.OK_CodeType = CodeTypes.BrokerageRegistration;
			orgCusCode2.OK_CustomsRegNo = "AUSCUSOS_AU";
			orgCusCode2.OK_RN_NKCodeCountry = "AU";
			Factory.Save();

			AssertEquals("Not Matched screening status resets after adding cusCode", ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			return org.CustomsCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var result = (OrgCusCode)base.GetNewBusinessObjectForDefaultLightValidationTest();
			_ = result.Lookups.NZCSupplier_List;

			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CustomsRegNo = "213";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "I have events";
			org.OH_RL_NKClosestPort = "AU";
			org.CustomsCodes.Add(orgCusCode);

			var validity = Factory.New<OrgCusCodeValidity>();
			validity.OCV_OK_OrgCusCode = orgCusCode.PK;
			validity.OCV_Verified = true;
			Factory.Save();

			return Factory.Load<OrgCusCode>(orgCusCode.PK);
		}

		OrgCusCode cusCode;

		protected override void SetUp()
		{
			base.SetUp();
			cusCode = (OrgCusCode)GetNewBusinessObject();
		}

		OrgCusCode CreateCodeForTest(string countryCode)
		{
			var result = Factory.New<OrgCusCode>();
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			result.OK_RN_NKCodeCountry = country != null ? (ZString)countryCode : ZString.Empty;

			return result;
		}

		OrgHeader CreateOrganisation(ZString codeType, ZString number)
		{
			var result = Factory.NewWithValidTestData<OrgHeader>();
			result.CustomsCodes.AddNew(codeType, number, USCountry);
			return result;
		}

		RefCountry USCountry => usCountry ?? (usCountry = new RefCountry.Loader(Factory).LoadForCountry(Core.Constants.CountryCodes.UnitedStates));
		RefCountry usCountry;

		void AssertRequiresUniqueRegistrationNumber(Type type, HashSet<string> validCodes)
		{
			var constantsList = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(x => x.IsLiteral && !x.IsInitOnly);
			CombineAssertions(() =>
			{
				foreach (var constant in constantsList)
				{
					var constantValue = (string)constant.GetRawConstantValue();
					cusCode.OK_CodeType = constantValue;
					AssertEquals($"Requires Unique Registration Number for {constantValue}", validCodes.Contains(constantValue), cusCode.RequiresUniqueRegistrationNumber);
				}
			});
		}

		void AssertShouldMakeUpperCase(HashSet<string> validCodes) => AssertShouldMakeUpperCase(validCodes, ZString.Empty);

		void AssertShouldMakeUpperCase(HashSet<string> validCodes, ZString countryCode)
		{
			CombineAssertions(() =>
			{
				if (!countryCode.IsEmpty)
				{
					cusCode.OK_RN_NKCodeCountry = countryCode;
				}
				foreach (var code in validCodes)
				{
					cusCode.OK_CodeType = code;
					AssertEquals($"OK_CustomsRegNo is upper case for {code}", true, cusCode.ShouldMakeUpperCase);
				}
				cusCode.OK_CodeType = ZString.Empty;
				AssertEquals("OK_CustomsRegNo is not upper case for invalid code", false, cusCode.ShouldMakeUpperCase);
			});
		}

		void AssertOrgCusCodeFieldsReadOnly(OrgCusCode orgCusCode, bool readOnly)
		{
			AssertEquals(readOnly, orgCusCode.OK_CodeTypeInfo.ReadOnly);
			AssertEquals(readOnly, orgCusCode.OK_CountryDefaultInfo.ReadOnly);
			AssertEquals(readOnly, orgCusCode.OK_CustomsRegNoInfo.ReadOnly);
			AssertEquals(readOnly, orgCusCode.OK_OA_PremisesAddressInfo.ReadOnly);
			AssertEquals(readOnly, orgCusCode.OK_OHInfo.ReadOnly);
			AssertEquals(readOnly, orgCusCode.OK_RN_NKCodeCountryInfo.ReadOnly);
		}

		OrgHeader OrgInDB => fOrgInDB ?? (fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG"));
		OrgHeader fOrgInDB;

		RefCountry[] GetUsaTerritories()
		{
			var query = new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.UsaAndTerritoriesList);
			return Factory.Load<RefCountry>(query);
		}

		/// <summary>
		/// Make sure countryCode is passed when a specific country is tested.
		/// Otherwise GetConsumptionTaxRegistrationOrgCusCode(countryCode) which is generically allowed to have an premise Address returns nothing.
		/// </summary>
		void AssertPremisesAddressIsAllowed(Type type, HashSet<string> validCodes, ZString countryCode)
		{
			var constantsList = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(x => x.IsLiteral && !x.IsInitOnly);
			CombineAssertions(() =>
			{
				if (!countryCode.IsEmpty)
				{
					cusCode.OK_RN_NKCodeCountry = countryCode;
				}
				foreach (var constant in constantsList)
				{
					var constantValue = (string)constant.GetRawConstantValue();
					cusCode.OK_CodeType = constantValue;
					AssertEquals($"Premise Address Is Allowed for {constant.Name}", validCodes.Contains(constantValue), cusCode.PremisesAddressIsAllowed);
				}
			});
		}

		// Please keep the list below in an alphabetical order
		readonly List<string> consumptionCodeIsNotValidOrgCusCode_CountryList = new List<string>()
		{
			Constants.CountryCodes.Afghanistan,
			Constants.CountryCodes.AmericanSamoa,
			Constants.CountryCodes.Aruba,
			Constants.CountryCodes.Bermuda,
			Constants.CountryCodes.Brunei,
			Constants.CountryCodes.CaymanIslands,
			Constants.CountryCodes.Cuba,
			Constants.CountryCodes.Gibraltar,
			Constants.CountryCodes.Guam,
			Constants.CountryCodes.Haiti,
			Constants.CountryCodes.HongKong,
			Constants.CountryCodes.Iraq,
			Constants.CountryCodes.LibyanArabJamahiriya,
			Constants.CountryCodes.Macau,
			Constants.CountryCodes.MarshallIslands,
			Constants.CountryCodes.Mayotte,
			Constants.CountryCodes.Micronesia,
			Constants.CountryCodes.NorfolkIsland,
			Constants.CountryCodes.NorthernMarianaIslands,
			Constants.CountryCodes.Palau,
			Constants.CountryCodes.SaintKittsAndNevis,
			Constants.CountryCodes.SintMaarten,
			Constants.CountryCodes.SolomonIslands,
			Constants.CountryCodes.SouthSudan,
			Constants.CountryCodes.StPierreEtMiquelon,
			Constants.CountryCodes.TimorLeste,
			Constants.CountryCodes.TurksAndCaicosIslands,
			Constants.CountryCodes.UnitedStates,
			Constants.CountryCodes.VirginIslands,
		};
		// Please keep the list above in an alphabetical order

		readonly List<string> localBusinessRegCodeIsNotValidOrgCusCode_CountryList = new List<string>();
	}
}
