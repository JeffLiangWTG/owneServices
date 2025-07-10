using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddress))]
	public class JobDocAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAdditionalAddressInformationComeFrom_WhenRegistryIsDisabled()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var additionalInfo1 = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo1.OAI_AdditionalInfo = "Other Additional 1";
			additionalInfo1.OAI_IsPrimary = false;

			var additionalInfo2 = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo2.OAI_AdditionalInfo = "Main Additional 2";
			additionalInfo2.OAI_IsPrimary = true;

			AssertEquals(additionalInfo2.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);

			var jobDocAddress = Factory.New<JobDocAddress>();

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				jobDocAddress.E2_AdditionalAddressInformation = "Override Additional Info";
				AssertEquals("Override Additional Info", jobDocAddress.E2_AdditionalAddressInformation);

				jobDocAddress.E2_AdditionalAddressInformation = ZString.Empty;
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				AssertEquals(additionalInfo2.OAI_AdditionalInfo, jobDocAddress.E2_AdditionalAddressInformation);

				additionalInfo2.OAI_IsPrimary = false;
				additionalInfo1.OAI_IsPrimary = true;
				jobDocAddress.E2_AdditionalAddressInformation = ZString.Empty;
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, jobDocAddress.E2_AdditionalAddressInformation);
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);

				jobDocAddress.E2_AdditionalAddressInformation = "Override Additional Info";
				AssertEquals("Ignored Override One", additionalInfo1.OAI_AdditionalInfo, jobDocAddress.E2_AdditionalAddressInformation);
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);

				jobDocAddress.E2_AdditionalAddressInformation = additionalInfo2.OAI_AdditionalInfo;
				AssertEquals("Ignored Override One", additionalInfo1.OAI_AdditionalInfo, jobDocAddress.E2_AdditionalAddressInformation);
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);
			}
		}

		public void TestAdditionalAddressInformationComeFrom_WhenRegistryIsEnabled()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var additionalInfo1 = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo1.OAI_AdditionalInfo = "Other Additional 1";
			additionalInfo1.OAI_IsPrimary = false;

			var additionalInfo2 = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo2.OAI_AdditionalInfo = "Main Additional 2";
			additionalInfo2.OAI_IsPrimary = true;

			AssertEquals(additionalInfo2.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AdditionalAddressInformation = ZString.Empty;
			jobDocAddress.E2_AddressOverride = false;
			jobDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals(additionalInfo2.OAI_AdditionalInfo, jobDocAddress.E2_AdditionalAddressInformation);

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				jobDocAddress.E2_AdditionalAddressInformation = additionalInfo1.OAI_AdditionalInfo;
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, jobDocAddress.E2_AdditionalAddressInformation);

				jobDocAddress.UnrestrictedAdditionalAddressInformation = "Override Additional Info";
				AssertEquals("Override Additional Info", jobDocAddress.E2_AdditionalAddressInformation);
			}
		}

		public void TestUnrestrictedAdditionalAddressInformationComeFrom_WhenRegistryIsDisabled()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var additionalInfo1 = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo1.OAI_AdditionalInfo = "Other Additional 1";
			additionalInfo1.OAI_IsPrimary = false;

			var additionalInfo2 = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo2.OAI_AdditionalInfo = "Main Additional 2";
			additionalInfo2.OAI_IsPrimary = true;

			AssertEquals(additionalInfo2.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);

			var jobDocAddress = Factory.New<JobDocAddress>();

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				jobDocAddress.UnrestrictedAdditionalAddressInformation = "Unrestricted Additional Info";
				AssertEquals("Unrestricted Additional Info", jobDocAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals("Unrestricted Additional Info", jobDocAddress.E2_AdditionalAddressInformation);
				AssertEquals(jobDocAddress.E2_AdditionalAddressInformationInfo, jobDocAddress.UnrestrictedAdditionalAddressInformationInfo);

				jobDocAddress.UnrestrictedAdditionalAddressInformation = ZString.Empty;
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				AssertEquals(additionalInfo2.OAI_AdditionalInfo, jobDocAddress.UnrestrictedAdditionalAddressInformation);

				additionalInfo2.OAI_IsPrimary = false;
				additionalInfo1.OAI_IsPrimary = true;
				jobDocAddress.UnrestrictedAdditionalAddressInformation = ZString.Empty;
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, jobDocAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);

				jobDocAddress.UnrestrictedAdditionalAddressInformation = additionalInfo2.OAI_AdditionalInfo;
				AssertEquals("Ignored Override One", additionalInfo1.OAI_AdditionalInfo, jobDocAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);

				jobDocAddress.UnrestrictedAdditionalAddressInformation = "Override Additional Info";
				AssertEquals("Ignored Override One", additionalInfo1.OAI_AdditionalInfo, jobDocAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);
			}
		}

		public void TestUnrestrictedAdditionalAddressInformationComeFrom_WhenRegistryIsEnabled()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var additionalInfo1 = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo1.OAI_AdditionalInfo = "Other Additional 1";
			additionalInfo1.OAI_IsPrimary = false;

			var additionalInfo2 = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo2.OAI_AdditionalInfo = "Main Additional 2";
			additionalInfo2.OAI_IsPrimary = true;

			AssertEquals(additionalInfo2.OAI_AdditionalInfo, orgHeader.MainAddress.OA_AdditionalAddressInformation);

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.UnrestrictedAdditionalAddressInformation = ZString.Empty;
			jobDocAddress.E2_AddressOverride = false;
			jobDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals(additionalInfo2.OAI_AdditionalInfo, jobDocAddress.UnrestrictedAdditionalAddressInformation);
			AssertEquals(orgHeader.MainAddress.PrimaryOrgAddressAdditionalInfoDetail, jobDocAddress.UnrestrictedAdditionalAddressInformation);

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				jobDocAddress.UnrestrictedAdditionalAddressInformation = additionalInfo1.OAI_AdditionalInfo;
				AssertEquals(additionalInfo1.OAI_AdditionalInfo, jobDocAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(jobDocAddress.E2_AdditionalAddressInformationInfo, jobDocAddress.UnrestrictedAdditionalAddressInformationInfo);

				jobDocAddress.UnrestrictedAdditionalAddressInformation = "Unrestricted Additional Info";
				AssertEquals("Unrestricted Additional Info", jobDocAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals("Unrestricted Additional Info", jobDocAddress.E2_AdditionalAddressInformation);
				AssertEquals(jobDocAddress.E2_AdditionalAddressInformationInfo, jobDocAddress.UnrestrictedAdditionalAddressInformationInfo);
			}
		}

		public void TestClearE2_AdditionalAddressInformationWhenAddressPkChanged()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AdditionalAddressInformation = "Dummy";
			AssertEquals("Dummy", jobDocAddress.E2_AdditionalAddressInformation);

			var orgHeader = Factory.New<OrgHeader>();
			var address1 = orgHeader.MainAddress;
			var address2 = orgHeader.Addresses.AddNew();
			jobDocAddress.E2_OA_Address = address1.PK;
			AssertEquals(string.Empty, jobDocAddress.E2_AdditionalAddressInformation);

			jobDocAddress.E2_AdditionalAddressInformation = "Another";
			jobDocAddress.E2_OA_Address = address2.PK;
			AssertEquals(string.Empty, jobDocAddress.E2_AdditionalAddressInformation);

			jobDocAddress.E2_AdditionalAddressInformation = "Another";
			jobDocAddress.E2_OA_Address = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress;
			AssertEquals("Another", jobDocAddress.E2_AdditionalAddressInformation);
		}

		public void TestUnrestrictedAdditionalAddressInformationReadOnly()
		{
			var jobDocAddress = Factory.New<JobDocAddressForTest>();
			jobDocAddress.E2_AddressOverride = true;
			AssertEquals(false, jobDocAddress.UnrestrictedAdditionalAddressInformation_ReadOnlyExposed);
		}

		public void TestUnrestrictedAdditionalAddressInformationReadOnly_WhenRegistryIsDisabled()
		{
			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var jobDocAddress = Factory.New<JobDocAddressForTest>();
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_OA_Address = Factory.New<OrgHeader>().MainAddress.PK;

				AssertEquals(true, jobDocAddress.UnrestrictedAdditionalAddressInformation_ReadOnlyExposed);
			}
		}

		public void TestUnrestrictedAdditionalAddressInformationReadOnly_WhenRegistryIsEnabled()
		{
			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var jobDocAddress = Factory.New<JobDocAddressForTest>();
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_OA_Address = Factory.New<OrgHeader>().MainAddress.PK;

				AssertEquals(false, jobDocAddress.UnrestrictedAdditionalAddressInformation_ReadOnlyExposed);

				jobDocAddress.E2_OA_Address = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress;
				AssertEquals(true, jobDocAddress.UnrestrictedAdditionalAddressInformation_ReadOnlyExposed);
			}
		}

		public void TestGetTranslatedAdditionalAddressInSpecificLanguageWithFallback()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			var orgHeader = Factory.New<OrgHeader>();
			var additionalInfo = orgHeader.MainAddress.AdditionalInfos.AddNew();
			additionalInfo.OAI_AdditionalInfo = "Test Additional";
			additionalInfo.OAI_IsPrimary = true;

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				jobDocAddress.E2_AdditionalAddressInformation = "Dummy";
				AssertEquals("Test Additional", jobDocAddress.GetTranslatedAdditionalAddressInSpecificLanguageWithFallback("ZH-CN"));
			}

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Dummy", jobDocAddress.GetTranslatedAdditionalAddressInSpecificLanguageWithFallback("ZH-CN"));
			}

			jobDocAddress.E2_AdditionalAddressInformation = ZString.Empty;
			AssertEquals("Test Additional", jobDocAddress.GetTranslatedAdditionalAddressInSpecificLanguageWithFallback("ZH-CN"));

			var translatedAdditionalInfo = additionalInfo.TranslatedInfos.AddNew();
			translatedAdditionalInfo.OTI_Language = "ZH-CN";
			translatedAdditionalInfo.OTI_AdditionalInfo = "虚拟地址";
			AssertEquals("虚拟地址", jobDocAddress.GetTranslatedAdditionalAddressInSpecificLanguageWithFallback("ZH-CN"));
			AssertEquals("Test Additional", jobDocAddress.GetTranslatedAdditionalAddressInSpecificLanguageWithFallback("En-US"));

			jobDocAddress.E2_AddressOverride = true;
			AssertNull(jobDocAddress.GetTranslatedAdditionalAddressInSpecificLanguageWithFallback("En-US"));
		}

		public void TestDefaultContactAllocationType()
		{
			CombineAssertions(() =>
			{
				var jobDocAddress = Factory.New<JobDocAddress>();
				AssertNull(jobDocAddress.DefaultContactAllocationType);
				jobDocAddress.DefaultContactAllocationType = "CUS";
				AssertEquals("CUS", jobDocAddress.DefaultContactAllocationType);
			});
		}

		public void TestE2_Address1AndE2_Address2()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;

			var fullAddress = "70 Char Address which will be split between E2_Address1 + E2_Address2.";
			jobDocAddress.E2_Address1AndE2_Address2 = fullAddress;

			CombineAssertions(() =>
			{
				AssertEquals("MaxLength", 70, jobDocAddress.E2_Address1AndE2_Address2Info.MaxLength);
				AssertEquals("E2_Address1AndE2_Address2 returns the full stored address", fullAddress, jobDocAddress.E2_Address1AndE2_Address2);
				AssertEquals("First 50 Characters are in E2_Address1", "70 Char Address which will be split between E2_Add", jobDocAddress.E2_Address1);
				AssertEquals("Last 20 Characters are in E2_Address2", "ress1 + E2_Address2.", jobDocAddress.E2_Address2);
			});
		}

		public void TestE2_Address1AndE2_Address2_NoOverride()
		{
			var fullAddress = "70 Char Address which will be split between E2_Address1 + E2_Address2.";

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.Address1 = fullAddress.Substring(0, 50);
			orgAddress.Address2 = fullAddress.Substring(50);

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;

			CombineAssertions(() =>
			{
				AssertEquals("E2_Address1AndE2_Address2 returns the full stored address", fullAddress, jobDocAddress.E2_Address1AndE2_Address2);
				AssertEquals("First 50 Characters are in E2_Address1", "70 Char Address which will be split between E2_Add", jobDocAddress.E2_Address1);
				AssertEquals("Last 20 Characters are in E2_Address2", "ress1 + E2_Address2.", jobDocAddress.E2_Address2);
			});
		}

		public void TestE2_Address1AndE2_Address2_SetEmpty()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;

			jobDocAddress.E2_Address1AndE2_Address2 = string.Empty;

			AssertEquals(string.Empty, jobDocAddress.E2_Address1AndE2_Address2);
		}

		#region TestAddressSummaryWithCompanyName

		public void TestAddressSummaryWithCompanyName()
		{
			var parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			var jda = parent.DocAddresses.CreateWithAddressType(DocAddressType.QuotationClientAddress);
			jda.E2_Address1 = "742 Evergreen Terrace";
			jda.E2_City = "Springfield";
			jda.E2_CompanyName = "Compu Global Hyper Mega Net";
			AssertEquals("Compu Global Hyper Mega Net\r\n742 EVERGREEN TERRACE\r\nSPRINGFIELD", jda.AddressSummaryWithCompanyName);
		}

		#endregion

		#region GetFilter

		public void TestGetFilter_ValidArguments_ReturnValidFilter()
		{
			var parent1 = Factory.New<JobDocAddressPersistentParentForTesting>();
			var docAddress1 = parent1.DocAddresses.CreateWithAddressType(DocAddressType.QuotationClientAddress);
			var docAddress2 = parent1.DocAddresses.CreateWithAddressType(DocAddressType.ReceivingForwarderAddress);
			var docAddress3 = parent1.DocAddresses.CreateWithAddressType(DocAddressType.QuotationClientAddress);

			Factory.Save();

			var addressCode = DocAddressTypes.GetCode(Factory, DocAddressType.QuotationClientAddress);
			var filter = JobDocAddress.GetFilter(addressCode, parent1.TablePrefix, 0);
			var loadedDocAddress = Factory.Load<JobDocAddress>(filter);

			var expectedAddresses = new ZGuid[] { docAddress1.PK };
			var actualAddresses = loadedDocAddress.Select(address => address.PK);
			AssertContainsExactElementsInAnyOrder(expectedAddresses, actualAddresses);
		}

		#endregion

		#region TestIsTheSameDocAddressAndContactAs

		public void TestIsTheSameDocAddressAndContactAs()
		{
			JobDocAddress adr1 = Factory.New<JobDocAddress>();
			JobDocAddress adr2 = Factory.New<JobDocAddress>();

			GenerateAdr(adr1);
			GenerateAdr(adr2);

			AssertEquals(false, adr1.IsTheSameDocAddressAndContactAs(null));

			AssertEquals(true, adr1.IsTheSameDocAddressAndContactAs(adr2));
			adr1.E2_AddressOverride = true;
			AssertEquals(true, adr1.IsTheSameDocAddressAndContactAs(adr2));

			adr2.E2_Contact = "newcontact";
			AssertEquals(false, adr1.IsTheSameDocAddressAndContactAs(adr2));
		}

		void GenerateAdr(JobDocAddress adr)
		{
			adr.E2_CompanyName = "compn";
			adr.E2_AdditionalAddressInformation = "additional address information";
			adr.E2_Address1 = "adr1";
			adr.E2_Address2 = "adr2";
			adr.E2_City = "cty";
			adr.E2_State = "state";

			adr.E2_Postcode = "pc";
			adr.E2_RN_NKCountryCode = "AU";
			adr.E2_Contact = "cont";
			adr.E2_Phone = "phone";
			adr.E2_Fax = "ax";
			adr.E2_Email = "email";
		}

		#endregion

		#region TestIEquatable

		public void TestIEquatable()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Real Company OK";

			OrgAddress realAddress = org.MainAddress;
			realAddress.PrimaryOrgAddressAdditionalInfoDetail = "Leave at reception";
			realAddress.OA_Address1 = "First Street";
			realAddress.OA_Address2 = "2nd Street";
			realAddress.OA_City = "Somewhere";
			realAddress.OA_State = "QLD";
			realAddress.OA_PostCode = "325235";

			var address1 = Factory.New<JobDocAddress>();
			address1.E2_OA_Address = realAddress.PK;

			Assert(!address1.Equals(null));

			var address2 = Factory.New<JobDocAddress>();
			address2.E2_OA_Address = realAddress.PK;

			Assert(address1.Equals(address2));

			address1.E2_AddressOverride = true;
			Assert(address1.Equals(address2));

			address2.E2_AddressOverride = true;
			address2.E2_CompanyName = "company 1";

			Assert(!address1.Equals(address2));

			address2 = address1;
			Assert(address1.Equals(address2));
		}

		#endregion

		#region TestOrganisationNameOrPKMaxLength

		public void TestRaiseAnyAddressFieldBeforeChangeThroughInfo()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			bool triggered = false;
			address.AnyAddressFieldBeforeChange += delegate
			{ triggered = true; };
			address.E2_Address1 = "HEY";
			AssertEquals(true, triggered);
			triggered = false;
			address.E2_Address1Info.Value = new ZString("THERE");
			AssertEquals(true, triggered);
		}

		public void TestOrganisationNameOrPKMaxLength()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();

			address.E2_AddressOverride = true;
			AssertEquals(JobDocAddressSchema.E2_CompanyName.MaxLength, address.OrganisationNameOrPKInfo.MaxLength);

			address.E2_AddressOverride = false;
			AssertEquals(ZGuid.Empty.ToString().Length, address.OrganisationNameOrPKInfo.MaxLength);
		}

		#endregion

		#region TestEnsureSequenceWhenSameType

		public void TestEnsureSequenceWhenSameType()
		{
			var iDocAddresses = (IDocAddresses)Factory.New<JobDocAddressPersistentParentForTesting>();
			JobDocAddress ctoAddress0 = iDocAddresses.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			JobDocAddress ctoAddress1 = iDocAddresses.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			JobDocAddress cfsAddress0 = iDocAddresses.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			AssertEquals("Precondition", (byte)0, ctoAddress0.E2_AddressSequence);
			AssertEquals("Precondition", (byte)1, ctoAddress1.E2_AddressSequence);
			AssertEquals("Precondition", (byte)0, cfsAddress0.E2_AddressSequence);

			cfsAddress0.DocAddressType = DocAddressType.LocalCartageCTO;
			AssertEquals("3rd CTO address, so set sequence to 2", (byte)2, cfsAddress0.E2_AddressSequence);

			cfsAddress0.DocAddressType = DocAddressType.LocalCartageCFS;
			AssertEquals("Only CFS address, should be 0", (byte)0, cfsAddress0.E2_AddressSequence);

			cfsAddress0.DocAddressType = DocAddressType.LocalCartageCFS;
			AssertEquals("Should still be 0", (byte)0, cfsAddress0.E2_AddressSequence);
		}

		public void TestReinitializeAddressSequenceNumber()
		{
			var jobDcAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDcAddress1.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			jobDcAddress1.E2_AddressSequence = 1;
			var jobDcAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDcAddress2.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			jobDcAddress2.E2_AddressSequence = 2;

			JobDocAddress.ReinitializeAddressSequenceNumber(new[] { jobDcAddress1, jobDcAddress2 });
			AssertEquals((byte)0, jobDcAddress1.E2_AddressSequence);
			AssertEquals((byte)2, jobDcAddress2.E2_AddressSequence);
		}

		public void TestReinitializeAddressSequenceNumber_WhenJobDocAddressIsDetached()
		{
			var jobDcAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDcAddress1.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			jobDcAddress1.E2_AddressSequence = 1;
			jobDcAddress1.Delete();
			Factory.Save();

			AssertEquals("Pre-requisite", DataRowState.Detached, (jobDcAddress1 as INeedRow).Row.RowState);
			JobDocAddress.ReinitializeAddressSequenceNumber(new[] { jobDcAddress1 });
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		#endregion

		#region TestIsPassportIDGovRegNumType

		public void TestIsPassportIDGovRegNumType()
		{
			DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertEquals(false, DocAddress.IsPassportIDGovRegNumType);
			DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			AssertEquals(true, DocAddress.IsPassportIDGovRegNumType);
		}

		#endregion

		#region TestE2_GovRegNum

		public void TestE2_GovRegNum_ShouldTruncateLongValues()
		{
			DocAddress.E2_GovRegNum = new string('a', DocAddress.E2_GovRegNumInfo.MaxLength + 5);
			AssertEquals("Value should be truncated to max lenth", new string('a', DocAddress.E2_GovRegNumInfo.MaxLength), DocAddress.E2_GovRegNum);
		}

		#endregion

		#region TestE2_PassportDetails

		public void TestE2_PassportDetails()
		{
			DocAddress.E2_AddressOverride = true;
			DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			AssertEquals("ID: CO: DOB:", DocAddress.E2_PassportDetails);
			DocAddress.E2_PassportID = "NK234423";
			AssertEquals("ID:NK234423 CO: DOB:", DocAddress.E2_PassportDetails);
			DocAddress.E2_PassportCountryOfIssue = "UK";
			AssertEquals("ID:NK234423 CO:UK DOB:", DocAddress.E2_PassportDetails);
			DocAddress.E2_PassportDateOfBirth = new ZDate(1985, 3, 25);
			AssertEquals("ID:NK234423 CO:UK DOB:25MAR1985", DocAddress.E2_PassportDetails);
		}

		#endregion

		#region TestPassportData

		public void TestPassportData()
		{
			DocAddress.E2_AddressOverride = true;
			DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			DocAddress.E2_GovRegNum = "";
			AssertEquals("", DocAddress.E2_PassportID);
			AssertEquals("", DocAddress.E2_PassportCountryOfIssue);
			AssertEquals(ZDateTime.Empty, DocAddress.E2_PassportDateOfBirth);
			DocAddress.E2_GovRegNum = "NK2343232AU25MAR1980";
			AssertEquals("", DocAddress.E2_PassportID);
			AssertEquals("", DocAddress.E2_PassportCountryOfIssue);
			AssertEquals(ZDateTime.Empty, DocAddress.E2_PassportDateOfBirth);
			DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			AssertEquals("NK2343232", DocAddress.E2_PassportID);
			AssertEquals("AU", DocAddress.E2_PassportCountryOfIssue);
			AssertEquals(new ZDateTime(1980, 3, 25), DocAddress.E2_PassportDateOfBirth);
		}

		#endregion

		#region TestDontSaveIfYouDontNeedTo

		public void TestDontSaveIfYouDontNeedTo_NotSynced()
		{
			JobDocAddressPersistentParentForTesting parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			JobDocAddress address = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);

			AssertEquals("Address was never changed and so IsSavedByFactory should be false", false, address.IsSavedByFactory);
			Factory.Save();
			AssertEquals("Address was never changed and so should not be saved", false, address.IsInDatabase);

			address.E2_AddressOverride = true;

			AssertEquals("Address has been changed and so IsSavedByFactory should be true", true, address.IsSavedByFactory);
			Factory.Save();
			AssertEquals("Address has been changed and so can be saved", true, address.IsInDatabase);
		}

		public void TestDontSaveIfYouDontNeedTo_Synced()
		{
			JobDocAddressPersistentParentForTesting parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			JobDocAddress addressMaster = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			JobDocAddress addressChild = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
			addressChild.SynchroniseWithParent(addressMaster);

			addressMaster.E2_AddressOverride = true;
			AssertEquals("precondition: AddressChild.E2_AddressOverride", true, addressChild.E2_AddressOverride);
			AssertEquals("precondition: AddressChild.HasChanges", true, addressChild.HasChanges);

			Factory.Save();

			AssertEquals("AddressMaster should be saved since it was changed", true, addressMaster.IsInDatabase);

			addressChild.E2_City = "Hello World";
			Factory.Save();

			AssertEquals("AddressChild should be saved since it is nolonger the same as it's master", true, addressChild.IsInDatabase);
		}

		#endregion

		#region TestSaveIfMarkedPersistent

		public void TestSaveIfMarkedPersistent_NotSynced()
		{
			JobDocAddressPersistentParentForTesting parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			JobDocAddress addressToSave = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			addressToSave.MakePersistentEvenIfEmpty();
			Factory.Save();
			AssertEquals("Address was marked PersistentEvenIfEmpty so should be saved", true, addressToSave.IsInDatabase);
		}

		public void TestSaveIfMarkedPersistent_Synced()
		{
			JobDocAddressPersistentParentForTesting parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			JobDocAddress addressMaster = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			JobDocAddress addressChild = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
			addressChild.SynchroniseWithParent(addressMaster);

			addressMaster.E2_AddressOverride = true;
			AssertEquals("precondition: AddressChild.E2_AddressOverride", true, addressChild.E2_AddressOverride);
			AssertEquals("precondition: AddressChild.HasChanges", true, addressChild.HasChanges);

			Factory.Save();

			AssertEquals("AddressMaster should be saved since it was changed", true, addressMaster.IsInDatabase);
			AssertEquals("AddressChild should be saved since it was changed", true, addressChild.IsInDatabase);

			addressChild.E2_AddressOverride = false;
			Factory.Save();
			AssertEquals("AddressChild should be deleted cause it's Empty", true, addressChild.IsDeleted);

			addressChild = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
			addressChild.E2_AddressOverride = false;
			addressChild.E2_OA_Address = ZGuid.Empty;
			addressChild.MakePersistentEvenIfEmpty();
			Factory.Save();
			AssertEquals("AddressChild should not be deleted as it's marked MakePersistentEvenIfEmpty", false, addressChild.IsDeleted);
			AssertEquals("AddressChild should be saved since as it's marked MakePersistentEvenIfEmpty", true, addressChild.IsInDatabase);
		}

		public void TestMarkedNonPersistentThenPersistent()
		{
			JobDocAddressPersistentParentForTesting parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			JobDocAddress address = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			address.MakeNonPersistent();
			Factory.Save();

			Assert(!address.IsInDatabase);
			Assert(!address.IsDeleted);

			address = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			address.MakeNonPersistent();
			address.MakePersistentEvenIfEmpty();
			Factory.Save();
			Assert(address.IsInDatabase);
			Assert(!address.IsDeleted);
		}

		public void TestMakePersistentIfNotEmpty()
		{
			var parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			var address = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			address.MakePersistentEvenIfEmpty();
			address.MakePersistentIfNotEmpty();

			Factory.Save();
			Assert(!address.IsDeleted);
		}

		#endregion

		#region TestHumanReadableInfoNames

		public void TestHumanReadableInfoNames()
		{
			JobDocAddress consignor = Factory.New<JobDocAddress>();
			consignor.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;

			var expectedPrefix = consignor.AddressDescription + ": ";
			foreach (ZPropertyInfo info in consignor.ZPropertyInfoHash)
			{
				AssertEquals("Expected human readable name to be prefixed with AddressDescription (" + info.Name + ")",
					expectedPrefix, info.HumanReadableName.SubstringSafe(0, expectedPrefix.Length));
			}

			consignor.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;

			expectedPrefix = consignor.AddressDescription + ": ";
			foreach (ZPropertyInfo info in consignor.ZPropertyInfoHash)
			{
				AssertEquals("Expected human readable name to be prefixed with AddressDescription (" + info.Name + ")",
					expectedPrefix, info.HumanReadableName.SubstringSafe(0, expectedPrefix.Length));
			}
		}

		#endregion

		#region TestPreemptStackOverflow

		[ExpectNoExceptions]
		public void TestPreemptStackOverflow()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.SynchroniseWithParent(address);
			address.SynchroniseWithParent(null);
		}

		#endregion

		#region TestSupportsClone

		[ExpectNoExceptions]
		public void TestSupportsClone()
		{
			DocAddress.E2_ParentID = ZGuid.NewZGuid();
			DocAddress.E2_ParentTableCode = "ZZ";
			DocAddress.OverrideRequirement = new JobDocAddressRequirement(DocAddressType.ImporterDocumentaryAddress);

			JobDocAddress clone = (JobDocAddress)DocAddress.Clone();

			AssertEquals("E2_ParentID should have been cleared.", ZGuid.Empty, clone.E2_ParentID);
			AssertEquals("E2_ParentTableCode should have been cleared.", "", clone.E2_ParentTableCode);
			AssertEquals("Requirement cloned", DocAddressType.ImporterDocumentaryAddress, clone.Requirement.DefaultDocAddressType);
			AssertEquals("Requirement not same instance", false, object.ReferenceEquals(clone.Requirement, DocAddress.Requirement));
		}

		#endregion

		#region TestCloneInternal

		public void TestCloneInternal_RowCopy_AddressOverriden()
		{
			TestCloneInternal_WithAddressOverride(true);
		}

		public void TestCloneInternal_SetPropertiesCopy_AddressOverriden()
		{
			TestCloneInternal_WithAddressOverride(false);
		}

		public void TestCloneInternal_RowCopy_WithOrgAddress()
		{
			TestCloneInternal_WithOrgAddress(true);
		}

		public void TestCloneInternal_SetPropertiesCopy_WithOrgAddress()
		{
			TestCloneInternal_WithOrgAddress(false);
		}

		void TestCloneInternal_WithAddressOverride(bool performRowCopyWithoutTriggeringValidationAndSetter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var docAddress = GetNewDocAddressWithParent();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "1";
			docAddress.E2_Address2 = "2";
			docAddress.E2_City = "3";
			docAddress.E2_State = "4";
			docAddress.E2_Postcode = "5";
			docAddress.E2_Phone = "6";
			docAddress.E2_Fax = "7";
			docAddress.E2_Email = "8";
			docAddress.E2_Mobile = "9";

			Factory.Save();
			var defaultSystemDefinedMiscAddressPK = docAddress.E2_OA_Address;

			var updateAddressSql = string.Format(@"
Update dbo.JobDocAddress 
set 
	E2_OA_Address = '{0}',
	E2_SystemLastEditTimeUtc = GETUTCDATE(),
	E2_SystemLastEditUser = '~BP' 
Where 
	E2_PK = '{1}'", org.MainAddress.PK, docAddress.PK);
			Db.Connection.ExecuteNonQuery(updateAddressSql);

			var docAddressInNewFactory = new BusinessObjectFactory().Load<JobDocAddress>(docAddress.PK);
			var clonedDocAddressInNewFactory = (JobDocAddress)docAddressInNewFactory.Clone(new BusinessObjectCloneArgs(Array.Empty<string>(), performRowCopyWithoutTriggeringValidationAndSetter));
			if (performRowCopyWithoutTriggeringValidationAndSetter)
			{
				AssertEquals("E2_OA_Address should NOT be reset to the current Default Misc System Defined Address PK. Should be cloned as it is.", org.MainAddress.PK, clonedDocAddressInNewFactory.E2_OA_Address);
			}
			else
			{
				AssertEquals("E2_OA_Address should be reset to the current Default Misc System Defined Address PK.", defaultSystemDefinedMiscAddressPK, clonedDocAddressInNewFactory.E2_OA_Address);
			}
			AssertJobAddressValues(docAddressInNewFactory, clonedDocAddressInNewFactory);
		}

		void TestCloneInternal_WithOrgAddress(bool performRowCopyWithoutTriggeringValidationAndSetter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var docAddress = GetNewDocAddressWithParent();
			docAddress.E2_OA_Address = org.MainAddress.PK;
			Factory.Save();

			var clonedDocAddress = (JobDocAddress)docAddress.Clone(new BusinessObjectCloneArgs(Array.Empty<string>(), performRowCopyWithoutTriggeringValidationAndSetter));
			AssertEquals("E2_OA_Address should be the org.MainAddress.PK, it doesn't matter if set via row copy or properties.", org.MainAddress.PK, clonedDocAddress.E2_OA_Address);
			AssertJobAddressValues(docAddress, clonedDocAddress);
		}

		void AssertJobAddressValues(JobDocAddress copyFromJobDocAddress, JobDocAddress copyToJobDocAddress)
		{
			AssertEquals("E2_ParentID should have been cleared.", ZGuid.Empty, copyToJobDocAddress["E2_ParentID"]);
			AssertEquals("E2_ParentTableCode should have been cleared.", "", copyToJobDocAddress["E2_ParentTableCode"]);
			AssertEquals("E2_AdditionalAddressInformation", copyFromJobDocAddress["E2_AdditionalAddressInformation"], copyToJobDocAddress["E2_AdditionalAddressInformation"]);
			AssertEquals("E2_Address1", copyFromJobDocAddress["E2_Address1"], copyToJobDocAddress["E2_Address1"]);
			AssertEquals("E2_Address2", copyFromJobDocAddress["E2_Address2"], copyToJobDocAddress["E2_Address2"]);
			AssertEquals("E2_AddressOverride", copyFromJobDocAddress["E2_AddressOverride"], copyToJobDocAddress["E2_AddressOverride"]);
			AssertEquals("E2_City", copyFromJobDocAddress["E2_City"], copyToJobDocAddress["E2_City"]);
			AssertEquals("E2_State should be overriden", copyFromJobDocAddress["E2_State"], copyToJobDocAddress["E2_State"]);
			AssertEquals("E2_Postcode should be overriden", copyFromJobDocAddress["E2_Postcode"], copyToJobDocAddress["E2_Postcode"]);
			AssertEquals("E2_Phone should be overriden", copyFromJobDocAddress["E2_Phone"], copyToJobDocAddress["E2_Phone"]);
			AssertEquals("E2_Fax should be overriden", copyFromJobDocAddress["E2_Fax"], copyToJobDocAddress["E2_Fax"]);
			AssertEquals("E2_Email should be overriden", copyFromJobDocAddress["E2_Email"], copyToJobDocAddress["E2_Email"]);
			AssertEquals("E2_Mobile should be overriden", copyFromJobDocAddress["E2_Mobile"], copyToJobDocAddress["E2_Mobile"]);
		}

		public void TestCloneInternal_DocAddressNumbers()
		{
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.DocAddressNumbers.AddNew(OrgCusCode.ChinaCodeTypes.CIQ, Core.Constants.CountryCodes.China).E2N_Number = "CNCIQ123";
			var clonedDocAddress = (JobDocAddress)docAddress.Clone();
			AssertEquals("Should not copy DocAddressNumbers when SupportsDocAddressNumbers is false", 0, clonedDocAddress.DocAddressNumbers.Count);

			var docAddressForTest = Factory.NewWithValidTestData<JobDocAddressForTest>();
			docAddressForTest.E2_AddressOverride = false;
			docAddressForTest.DocAddressNumbers.AddNew(OrgCusCode.ChinaCodeTypes.USC, Core.Constants.CountryCodes.China).E2N_Number = "CNUSC789";
			clonedDocAddress = (JobDocAddress)docAddressForTest.Clone();
			AssertEquals("Should not copy DocAddressNumbers when E2_AddressOverride is false", 0, clonedDocAddress.DocAddressNumbers.Count);

			docAddressForTest.E2_AddressOverride = true;
			clonedDocAddress = (JobDocAddress)docAddressForTest.Clone();
			AssertEquals("Should copy DocAddressNumbers when SupportsDocAddressNumbers and E2_AddressOverride are both true", 1, clonedDocAddress.DocAddressNumbers.Count);
			AssertJobDocAddressNumberValues(docAddressForTest.DocAddressNumbers[0], clonedDocAddress.DocAddressNumbers[0]);
		}

		void AssertJobDocAddressNumberValues(JobDocAddressNumber copyFromJobDocAddressNumber, JobDocAddressNumber copyToJobDocAddressNumber)
		{
			AssertNotEquals("Should not copy E2N_E2", copyFromJobDocAddressNumber.E2N_E2, copyToJobDocAddressNumber.E2N_E2);
			AssertEquals("E2N_Number", copyFromJobDocAddressNumber.E2N_RN_NKCountryCode, copyToJobDocAddressNumber.E2N_RN_NKCountryCode);
			AssertEquals("E2N_NumberType", copyFromJobDocAddressNumber.E2N_NumberType, copyToJobDocAddressNumber.E2N_NumberType);
			AssertEquals("E2N_Number", copyFromJobDocAddressNumber.E2N_Number, copyToJobDocAddressNumber.E2N_Number);
		}

		#endregion

		#region Document Delivery Details'

		public void TestGetDocumentDeliveryContact()
		{
			var testMenuItem = Factory.New<StmMenuItem>();

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Real Company OK";

			var realContact = org.Contacts.AddNew();
			realContact.OC_ContactName = "Nibuz Ooppa";
			realContact.OC_Email = "nibuz@hednahshkar.com";
			realContact.OC_Fax = "939393939";
			realContact.OC_Phone = "11122";
			realContact.OC_NotifyMode = "FAX";

			var realAddress = org.MainAddress;
			realAddress.PrimaryOrgAddressAdditionalInfoDetail = "Leave at reception";
			realAddress.OA_Address1 = "First Street";
			realAddress.OA_Address2 = "2nd Street";
			realAddress.OA_City = "Somewhere";
			realAddress.OA_State = "QLD";
			realAddress.OA_PostCode = "325235";

			var parent = new JobDocAddressParentForTesting(Factory);
			var docAddress = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);

			docAddress.ContactPK = realContact.PK;
			docAddress.E2_OA_Address = realAddress.PK;

			var deliveryContact = docAddress.GetDocumentDeliveryContact(testMenuItem);
			AssertEquals(testMenuItem, deliveryContact.MenuItem);
			AssertEquals("Real Company OK", deliveryContact.CompanyName);
			AssertEquals(org.PK, deliveryContact.OrgHeaderPK);
			AssertEquals("Nibuz Ooppa", deliveryContact.Name);
			AssertEquals("nibuz@hednahshkar.com", deliveryContact.Email);
			AssertEquals("939393939", deliveryContact.Fax);
			AssertEquals("11122", deliveryContact.Phone);
			AssertEquals("FAX", deliveryContact.DeliveryMethod);
			AssertEquals("", deliveryContact.AttachmentType);
			AssertEquals("Leave at reception", deliveryContact.AdditionalAddress);
			AssertEquals("First Street", deliveryContact.Address1);
			AssertEquals("2nd Street", deliveryContact.Address2);
			AssertEquals("Somewhere", deliveryContact.City);
			AssertEquals("QLD", deliveryContact.State);
			AssertEquals("325235", deliveryContact.PostCode);

			docAddress.ContactPK = ZGuid.Empty;
			docAddress.E2_OA_Address = ZGuid.Empty;

			docAddress.E2_CompanyName = "Overridden Company Name";
			docAddress.E2_Contact = "New Contact";
			docAddress.E2_Email = "som@som.com";
			docAddress.E2_Fax = "22222222";
			docAddress.E2_Phone = "444444444";
			docAddress.E2_AdditionalAddressInformation = "additionaladdressinfo";
			docAddress.E2_Address1 = "addy1";
			docAddress.E2_Address2 = "addy2";
			docAddress.E2_City = "seeety";
			docAddress.E2_State = "staaaaaate";
			docAddress.E2_Postcode = "postycodie";

			deliveryContact = docAddress.GetDocumentDeliveryContact(null);
			AssertNull(deliveryContact.MenuItem);
			AssertEquals("Overridden Company Name", deliveryContact.CompanyName);
			AssertEquals("New Contact", deliveryContact.Name);
			AssertEquals("som@som.com", deliveryContact.Email);
			AssertEquals("22222222", deliveryContact.Fax);
			AssertEquals("444444444", deliveryContact.Phone);
			AssertEquals("EML", deliveryContact.DeliveryMethod);
			AssertEquals("PDF", deliveryContact.AttachmentType);
			AssertEquals("addy1", deliveryContact.Address1);
			AssertEquals("addy2", deliveryContact.Address2);
			AssertEquals("seeety", deliveryContact.City);
			AssertEquals("staaaaaate", deliveryContact.State);
			AssertEquals("postycodie", deliveryContact.PostCode);

			docAddress.E2_Email = "";
			deliveryContact = docAddress.GetDocumentDeliveryContact(null);
			AssertEquals("", deliveryContact.Email);
			AssertEquals("22222222", deliveryContact.Fax);
			AssertEquals("FAX", deliveryContact.DeliveryMethod);
			AssertEquals("", deliveryContact.AttachmentType);

			docAddress.E2_Fax = "";
			deliveryContact = docAddress.GetDocumentDeliveryContact(null);
			AssertEquals("", deliveryContact.Email);
			AssertEquals("", deliveryContact.Fax);
			AssertEquals("PRN", deliveryContact.DeliveryMethod);
			AssertEquals("", deliveryContact.AttachmentType);
		}

		#endregion

		#region TestDontKeepCreatingNewContactsForTheSystemDefault

		public void TestDontKeepCreatingNewContactsForTheSystemDefault()
		{
			var parent = new JobDocAddressParentForTesting(Factory);
			var docAddress = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			docAddress.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			docAddress.E2_AddressOverride = false;
			docAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			docAddress.E2_Contact = "";
			docAddress.DefaultContactType = ContactType.Consignee;

			var contactPK = docAddress.ContactPK;
			AssertEquals("Should return the same PK", contactPK, docAddress.ContactPK);
		}

		#endregion

		#region Test Static Instantiation

		public void TestStaticCreateNewFromParent()
		{
			JobDocAddressParentForTesting anything = new JobDocAddressParentForTesting(Factory);
			JobDocAddress dA = JobDocAddress.New(anything);

			AssertNotNull("DocAddress should exist.", dA);
			AssertEquals("DocAddress's ParentID should be Parent's PK.", dA.E2_ParentID, anything.PK);
		}

		public void TestStaticLoadFromParent()
		{
			JobDocAddressParentForTesting anything = new JobDocAddressParentForTesting(Factory);
			JobDocAddress dA = JobDocAddress.New(anything);
			AssertNotNull("DocAddress should exist.", dA);
			AssertEquals("DocAddress's ParentID should be Parent's PK.", dA.E2_ParentID, anything.PK);
			dA.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageYard);

			JobDocAddress dA2 = JobDocAddress.Load(anything, DocAddressType.LocalCartageYard);
			AssertNotNull("DocAddress should have been found.", dA2);
		}

		#endregion

		#region Test Field Overrides

		public void TestCanOverride()
		{
			var docAddress = Factory.New<JobDocAddress>();
			Assert(docAddress.CanOverride);
			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			Assert(docAddress.CanOverride);
			docAddress.OverrideRequirement.CanOverride = false;
			Assert(!docAddress.CanOverride);
		}

		public void TestFieldValuesWhenNoOrgAddress()
		{
			foreach (ZPropertyInfo info in AddressOverrideProperties)
			{
				AssertEquals(info.Name + " Field should be empty.", "", (ZString)info.Value);
			}
		}

		public void TestAddressOverride_WhenSettingToFalse_ShouldUpdateValidationStatusToNrq()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.Verified;

			// Act.

			address.E2_AddressOverride = false;

			// Assert.

			Assert("E2_AddressOverride should be FALSE", !address.E2_AddressOverride);
			AssertEquals("E2_ValidationStatus should be NRQ", AddressValidationStatus.NotRequired, address.E2_ValidationStatus);
		}

		public void TestAddressOverride_WhenSettingToTrue_ShouldUpdateValidationStatusToNyv()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = false;
			address.E2_ValidationStatus = AddressValidationStatus.NotRequired;

			// Act.

			address.E2_AddressOverride = true;

			// Assert.

			Assert("E2_AddressOverride should be TRUE", address.E2_AddressOverride);
			AssertEquals("E2_ValidationStatus should be NYV", AddressValidationStatus.ToBeVerified, address.E2_ValidationStatus);
		}

		public void TestFieldValuesWhenOrgAddressPresent()
		{
			JobDocAddress docAddress1 = GetNewDocAddressWithParent();
			RegistrationNumber registrationNumber = new RegistrationNumber() { Number = "ABC23432", NumberType = "ABC" };
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.None);
			requirement.GetRegistrationNumberResult = docAddress => new RegistrationNumberResult(docAddress.Factory, true, delegate { return registrationNumber; });
			docAddress1.OverrideRequirement = requirement;
			OrgAddress address = GetNewOrgAddress();
			OrgContact contact = SetDefaultContactAndCompanyForAddress(address);

			address.PrimaryOrgAddressAdditionalInfoDetail = "0";
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_RL_NKRelatedPortCode = DefaultUNLOCOPortCode;
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			address.OA_Mobile = "9";
			docAddress1.E2_OA_Address = address.PK;

			AssertDefaultValues(docAddress1); // values should be those just set
			AssertEquals(docAddress1.E2_GovRegNum, registrationNumber.Number);
			AssertEquals(docAddress1.E2_GovRegNumType, registrationNumber.NumberType);

			docAddress1.E2_AddressOverride = true;
			AssertDefaultValues(docAddress1); // values should remain from previous orgaddress
			AssertEquals(docAddress1.E2_GovRegNum, registrationNumber.Number);
			AssertEquals(docAddress1.E2_GovRegNumType, registrationNumber.NumberType);

			docAddress1.E2_Contact = "Contact";
			docAddress1.E2_CompanyName = "Company";
			docAddress1.E2_AdditionalAddressInformation = "00";
			docAddress1.E2_Address1 = "11";
			docAddress1.E2_Address2 = "21";
			docAddress1.E2_City = "31";
			docAddress1.E2_State = "41";
			docAddress1.E2_Postcode = "51";
			docAddress1.E2_RN_NKCountryCode = "XY";
			docAddress1.E2_Phone = "61";
			docAddress1.E2_Fax = "71";
			docAddress1.E2_Email = "81";
			docAddress1.E2_Mobile = "91";
			docAddress1.E2_GovRegNum = "BDS2342";

			// values should return manually set values
			AssertEquals(docAddress1.E2_Contact, "Contact");
			AssertEquals(docAddress1.E2_CompanyName, "Company");
			AssertEquals(docAddress1.E2_AdditionalAddressInformation, "00");
			AssertEquals(docAddress1.E2_Address1, "11");
			AssertEquals(docAddress1.E2_Address2, "21");
			AssertEquals(docAddress1.E2_City, "31");
			AssertEquals(docAddress1.E2_State, "41");
			AssertEquals(docAddress1.E2_Postcode, "51");
			AssertEquals(docAddress1.E2_RN_NKCountryCode, "XY");
			AssertEquals(docAddress1.E2_Phone, "61");
			AssertEquals(docAddress1.E2_Fax, "71");
			AssertEquals(docAddress1.E2_Email, "81");
			AssertEquals(docAddress1.E2_Mobile, "91");
			AssertEquals(docAddress1.E2_GovRegNum, "BDS2342");

			docAddress1.E2_AddressOverride = false;
			docAddress1.E2_OA_Address = address.PK;
			AssertDefaultValues(docAddress1); //values should be from previous orgaddress
			AssertEquals(docAddress1.E2_GovRegNum, registrationNumber.Number);
			AssertEquals(docAddress1.E2_GovRegNumType, registrationNumber.NumberType);

			docAddress1.OverrideRequirement = null;
			AssertEquals(docAddress1.E2_GovRegNum, address.Header.PrimaryRegistrationNumber.Number);
			AssertEquals(docAddress1.E2_GovRegNumType, address.Header.PrimaryRegistrationNumber.NumberType);
		}

		void AssertDefaultValues(JobDocAddress docAddress)
		{
			AssertEquals(docAddress.E2_Contact, "");
			AssertEquals(docAddress.E2_CompanyName, DefaultCompanyName);
			AssertEquals(docAddress.E2_AdditionalAddressInformation, "0");
			AssertEquals(docAddress.E2_Address1, "1");
			AssertEquals(docAddress.E2_Address2, "2");
			AssertEquals(docAddress.E2_City, "3");
			AssertEquals(docAddress.E2_State, "4");
			AssertEquals(docAddress.E2_Postcode, "5");
			AssertEquals(docAddress.E2_RN_NKCountryCode, DefaultCountryCode);
			AssertEquals(docAddress.E2_Phone, "6");
			AssertEquals(docAddress.E2_Fax, "7");
			AssertEquals(docAddress.E2_Email, "8");
			AssertEquals(docAddress.E2_Mobile, "9");
		}

		#region Test CompanyNameBehaviour

		public void TestCompanyNameBehaviour()
		{
			JobDocAddress docAddress = GetNewDocAddressWithParent();
			OrgAddress address = GetNewOrgAddress();
			SetDefaultContactAndCompanyForAddress(address);
			docAddress.E2_OA_Address = address.PK;

			AssertEquals(DefaultCompanyName, docAddress.E2_CompanyName);

			docAddress.E2_AddressOverride = true;
			AssertEquals(DefaultCompanyName, docAddress.E2_CompanyName); //value should remain from previous orgaddress

			docAddress.E2_CompanyName = "Company";

			AssertEquals("Company", docAddress.E2_CompanyName); //value should return manually set values

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = address.PK;
			AssertEquals(DefaultCompanyName, docAddress.E2_CompanyName); //value should be from previous orgaddress

			address.OA_CompanyNameOverride = "Alternate Company name";
			AssertEquals("Alternate Company name", docAddress.E2_CompanyName);

			docAddress.E2_AddressOverride = true;
			AssertEquals("Alternate Company name", docAddress.E2_CompanyName); //value should remain from previous orgaddress

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = address.PK;
			AssertEquals(docAddress.E2_CompanyName, "Alternate Company name");

			address.OA_CompanyNameOverride = "";
			AssertEquals(docAddress.E2_CompanyName, DefaultCompanyName);
		}

		public void TestE2_CompanyNameTruncated()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_CompanyName = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the";
			AssertEquals("E2_CompanyNameTruncated is 50 characters", 50, JobDocAddress.Schema.E2_CompanyNameTruncatedLength);
			Assert("Precondition: E2_CompanyName can be set for more than 50 characters upto 100 characters", docAddress.E2_CompanyName.Length > JobDocAddress.Schema.E2_CompanyNameTruncatedLength);
			AssertEquals("E2_CompanyNameTruncated should not exceed 50 characters though", JobDocAddress.Schema.E2_CompanyNameTruncatedLength, docAddress.E2_CompanyNameTruncated.Length);
			AssertEquals(docAddress.E2_CompanyName.Substring(0, JobDocAddress.Schema.E2_CompanyNameTruncatedLength), docAddress.E2_CompanyNameTruncated);
		}

		#endregion

		#region Test SavingBehaviourOfAddress

		public void TestSavingBehaviourOfAddress()
		{
			AssertNull("Related OrgAddress should be null.", DocAddress.Address);
			OrgAddress address = GetNewOrgAddress();
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			SetDefaultContactAndCompanyForAddress(address);
			DocAddress.E2_OA_Address = address.PK;
			DocAddress.FillWithValidTestData();
			OrgHeader org = address.Header;
			org.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave, Array.Empty<PropertyDescriptor>());

			AssertNotNull("Related OrgAddress should not be null.", DocAddress.Address);

			Factory.Save();

			DocAddress.E2_AddressOverride = true;
			AssertEquals("Related OrgAddress should be a MISC addy.", "MISC", DocAddress.Address.Header.OH_Code);
			DocAddress.E2_AddressOverride = false;
			DocAddress.E2_AddressOverride = true;
			Factory.Save();

			AssertEquals("Related OrgAddress should be a MISC addy.", "MISC", DocAddress.Address.Header.OH_Code);

			var newFactory = new BusinessObjectFactory();
			var docAddress2 = newFactory.Load<JobDocAddress>(DocAddress.PK);
			AssertEquals("Related OrgAddress should be a MISC addy.", "MISC", docAddress2.Address.Header.OH_Code);
			docAddress2.E2_AddressOverride = false;
			AssertNull("Related OrgAddress should be null.", docAddress2.Address);
		}

		#endregion

		#endregion

		#region Test Lists

		public void TestAddressTypeDescriptionList()
		{
			DocAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.ConsigneeDocumentaryAddress);
			AssertEquals("Description should be returned from CodePairDescription List.", DocAddress.AddressDescription, DocAddressTypes.Descriptions.ConsigneeDocumentaryAddress);
			DocAddress.E2_AddressType = "123"; //unknown
			AssertEquals("Description should be default to unknown code.", DocAddress.AddressDescription, DocAddress.E2_AddressType);
			DocAddress.E2_AddressType = ""; //unspecified
			AssertEquals("Description should be treated as 'Unspecified'.", DocAddress.AddressDescription, DocAddressTypes.Unspecified);
			AssertEquals("AddressDescription should be readonly.", true, DocAddress.AddressDescriptionInfo.ReadOnly);
		}

		public void TestAdditionalAddressInfoList()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var additionalInfo1 = address.AdditionalInfos.AddNew();
			additionalInfo1.OAI_OA_Address = address.PK;
			additionalInfo1.OAI_AdditionalInfo = "Additional Info 1";
			additionalInfo1.OAI_IsPrimary = true;

			var additionalInfo2 = address.AdditionalInfos.AddNew();
			additionalInfo2.OAI_OA_Address = address.PK;
			additionalInfo2.OAI_AdditionalInfo = "Additional Info 2";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = address.PK;

			Factory.Save();

			AssertEquals(2, address.AdditionalAddressInfoList.Count);
			AssertEquals(2, docAddress.AdditionalAddressInfoList.Count);
			AssertContainsExactElementsInAnyOrder(address.AdditionalAddressInfoList, docAddress.AdditionalAddressInfoList);
		}

		#endregion

		#region TestCountry

		public void TestCountry()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "XX"));
			if (country == null)
			{
				country = Factory.New<RefCountry>();
				country.RN_Code = "XX";
			}
			AssertNotNull("Country should exist.", country);
			DocAddress.E2_RN_NKCountryCode = "XX";
			AssertNotNull("Address Country should exist.", DocAddress.Country);
			AssertEquals("Addrsss Country should be XX.", DocAddress.Country.PK, country.PK);
		}

		public void TestCountryOnDetachedRowThrowsNoException()
		{
			var detachedAddress = Factory.NewWithValidTestData<JobDocAddress>();
			detachedAddress.E2_RN_NKCountryCode = "XX";
			detachedAddress.Delete();
			AssertEquals("New row that hasn't been saved should be detached", DataRowState.Detached, ((INeedRow)detachedAddress).Row.RowState);
			AssertEquals("Accessing Country field on detached JobDocAddress should return null", null, detachedAddress.Country);
		}

		public void TestCountryCodeIsObsoleteAndMacroIgnore()
		{
			CombineAssertions(() =>
			{
				var propertyInfo = typeof(JobDocAddress).GetProperty("CountryCode");
				AssertNotNull("MacroIgnoreAttribute", propertyInfo.GetCustomAttribute<MacroIgnoreAttribute>());
				AssertNotNull("ObsoleteAttribute", propertyInfo.GetCustomAttribute<ObsoleteAttribute>());
			});
		}

		public void TestCountryCodes()
		{
			OrgHeader orgH = Factory.New<OrgHeader>();
			OrgAddress orgA = orgH.MainAddress;
			DocAddress.E2_OA_Address = orgA.PK;

			orgA.OA_RL_NKRelatedPortCode = "SGSNG";
			AssertEquals("Country Code should be from RelatedPort.", "SG", DocAddress.E2_RN_NKCountryCode);

			orgA.OA_RL_NKRelatedPortCode = "";
			orgA.OA_RN_NKCountryCode = "";
			AssertEquals("Country Code should be from RelatedPort.", "", DocAddress.E2_RN_NKCountryCode);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NZAKL"));
			if (uNLOCO == null)
			{
				uNLOCO = Factory.New<RefUNLOCO>();
				uNLOCO.RL_Code = "NZAKL";
			}
			orgH.OH_RL_NKClosestPort = "NZAKL";
			AssertEquals("Country Code should be from ClosestPort.", "NZ", DocAddress.E2_RN_NKCountryCode);

			orgH.OH_RL_NKClosestPort = "12345";
			AssertEquals("Country Code should be from ClosestPort.", "12", DocAddress.E2_RN_NKCountryCode);
		}

		public void TestCountryWhenOverride()
		{
			DocAddress.E2_OA_Address = ZGuid.Empty;

			DocAddress.E2_AddressOverride = true;
			AssertEquals("Default Country Code should be country of current company.", Env.CurrentCompany.Country.Code, DocAddress.E2_RN_NKCountryCode);

			DocAddress.E2_AddressOverride = false;
			DocAddress.GetDefaultCountryCodeIfEmpty = delegate
			{ return "UA"; };
			DocAddress.E2_AddressOverride = true;
			AssertEquals("Country Code is assigned by delegate.", "UA", DocAddress.E2_RN_NKCountryCode);

			DocAddress.E2_AddressOverride = false;
			OrgHeader orgH = Factory.New<OrgHeader>();
			OrgAddress orgA = orgH.MainAddress;
			DocAddress.E2_OA_Address = orgA.PK;

			orgA.OA_RL_NKRelatedPortCode = "SGSNG";
			AssertEquals("Country Code should be from RelatedPort.", "SG", DocAddress.E2_RN_NKCountryCode);

			DocAddress.E2_AddressOverride = true;
			AssertEquals("Country Code should be from RelatedPort.", "SG", DocAddress.E2_RN_NKCountryCode);
		}

		#endregion

		#region TestReadOnlyStrategy

		public void TestReadOnlyStrategy()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertNull("Precondition", jobDocAddress.ReadOnlyStrategy);

			var mock = new Mock<IJobDocAddressReadOnlyStrategy>();
			jobDocAddress.ReadOnlyStrategy = mock.Object;
			AssertEquals(mock.Object, jobDocAddress.ReadOnlyStrategy);
		}

		#endregion

		#region TestReadOnly_ReadOnlyStrategy

		public void TestReadOnly_ReadOnlyStrategy()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertEquals("Precondition", false, jobDocAddress.ReadOnly);

			var mock = new Mock<IJobDocAddressReadOnlyStrategy>();
			mock.SetupGet(x => x.ReadOnly).Returns(true);
			jobDocAddress.ReadOnlyStrategy = mock.Object;
			AssertEquals(true, jobDocAddress.ReadOnly);

			mock.SetupGet(x => x.ReadOnly).Returns(false);
			AssertEquals(false, jobDocAddress.ReadOnly);

			jobDocAddress.ReadOnly = true;
			AssertEquals(true, jobDocAddress.ReadOnly);
		}

		#endregion

		#region TestReadOnlyFieldsWhenNoOverride

		public void TestReadOnlyFieldsWhenNoOverride()
		{
			OrgHeader orgH = Factory.New<OrgHeader>();

			DocAddress.OrganisationPK = orgH.PK;
			AssertReadOnlyFields(true);

			DocAddress.E2_AddressOverride = true;
			AssertReadOnlyFields(false);

			DocAddress.E2_AddressOverride = false;
			AssertReadOnlyFields(true);
		}

		void AssertReadOnlyFields(bool readOnly)
		{
			foreach (ZPropertyInfo info in AddressOverrideProperties)
			{
				AssertEquals(info.Name + " Field should have R/O set to " + readOnly + ".", readOnly, info.ReadOnly);
			}

			foreach (ZPropertyInfo info in AddressNotOverrideProperties)
			{
				AssertEquals(info.Name + " Field should have R/O set to " + !readOnly + ".", !readOnly, info.ReadOnly);
			}
		}

		#endregion

		#region TestSettingAddressOverrideThrowsNoExceptionWhenDeletedInAnotherFactory

		public void TestSettingAddressOverrideThrowsNoExceptionWhenDeletedInAnotherFactory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.OrganisationPK = org.PK;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var docAddressInAnotherFactory = anotherFactory.Load<JobDocAddress>(docAddress.PK);
			docAddressInAnotherFactory.E2_AddressOverride = false;

			docAddress.Delete();
			Factory.Save();

			AssertNoExceptionThrown(() => { docAddressInAnotherFactory.E2_AddressOverride = true; });
		}

		public void TestAccessAddressOverrideOnDetachedRowThrowsNoException()
		{
			var detachedAddress = Factory.NewWithValidTestData<JobDocAddress>();
			detachedAddress.E2_AddressOverride = true;
			AssertEquals(true, detachedAddress.E2_AddressOverride);
			detachedAddress.Delete();

			AssertEquals("New row that hasn't been saved should be detached", DataRowState.Detached, ((INeedRow)detachedAddress).Row.RowState);
			AssertEquals("Accessing E2_AddressOverride field on detached JobDocAddress should return False", false, detachedAddress.E2_AddressOverride);
		}

		#endregion

		#region Test DocAddressType

		public void TestCodeSetWhenPassedActualDocAddressType()
		{
			DocAddress.DocAddressType = DocAddressType.LocalCartageImporter;
			AssertEquals("E2_AddressType wasn't set from DocAddressType.", DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageImporter), DocAddress.E2_AddressType);
		}

		public void TestCodeEmptyWhenPassedNoDocAddressType()
		{
			AssertEquals("E2_AddressType wasn't set from DocAddressType.", "", DocAddress.E2_AddressType);
			DocAddress.DocAddressType = DocAddressType.None;
			AssertEquals("E2_AddressType wasn't set from DocAddressType.", DocAddressTypes.None, DocAddress.E2_AddressType);
		}

		public void TestCodeWhenPassedByDocAddressTypeIncludingTypeNone()
		{
			AssertEquals("E2_AddressType wasn't set from DocAddressType.", "", DocAddress.E2_AddressType);
			DocAddress.DocAddressType = DocAddressType.None;
			AssertEquals("E2_AddressType wasn't set from DocAddressType.", DocAddressTypes.None, DocAddress.E2_AddressType);
			DocAddress.DocAddressType = DocAddressType.LocalCartageYard;
			AssertEquals("E2_AddressType wasn't set from DocAddressType.", DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageYard), DocAddress.E2_AddressType);
		}

		public void TestDocAddressTypeIsSetFromE2_DocAddressType()
		{
			JobDocAddressPersistentParentForTesting parent = Factory.New<JobDocAddressPersistentParentForTesting>();
			JobDocAddress docAddress = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);

			docAddress.DocAddressType = DocAddressType.LocalCartageImporter;
			docAddress.E2_AddressOverride = true;
			Factory.Save();
			AssertEquals("E2_AddressType wasn't set from DocAddressType.", DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageImporter), docAddress.E2_AddressType);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDocAddress loadedDocAddress = Factory.Load<JobDocAddress>(docAddress.PK);
			AssertEquals("E2_AddressType wasn't set from DocAddressType.", DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageImporter), docAddress.E2_AddressType);
		}

		#endregion

		#region Test AddressType

		public void TestWhenPassedActualAddressType()
		{
			AssertEquals("AddressType wasn't set to default", AddressType.OFC, DocAddress.DefaultAddressType);
			DocAddress.DefaultAddressType = AddressType.DLV;
			AssertEquals("AddressType wasn't set to DLV", AddressType.DLV, DocAddress.DefaultAddressType);
		}

		#endregion

		#region Test Organisation

		public void TestSettingOrganisationPK()
		{
			AssertEquals("Address wasn't set to default.", ZGuid.Empty, DocAddress.E2_OA_Address);
			OrgHeader orgH = Factory.New<OrgHeader>();
			OrgAddress orgMain = orgH.GetAddressWithFallback(AddressType.OFC);
			AssertNotNull(orgMain);
			DocAddress.OrganisationPK = orgH.PK;
			AssertEquals("Address wasn't set.", orgH.PK, DocAddress.Organisation.PK);

			OrgHeader orgHToDelete = Factory.New<OrgHeader>();
			orgHToDelete.Addresses.AddNew();
			DocAddress.OrganisationPK = orgHToDelete.PK;
			AssertEquals("Address wasn't set.", orgHToDelete.PK, DocAddress.Organisation.PK);
		}

		public void TestGetOrganisationNullWhenDeleted()
		{
			OrgHeader orgH = Factory.New<OrgHeader>();
			DocAddress.OrganisationPK = orgH.PK;
			AssertNotNull(DocAddress.Organisation);

			DocAddress.Delete();
			AssertNull(DocAddress.Organisation);
		}

		public void TestSettingOrganisationPKUsesDefaultAddressType()
		{
			var orgH = Factory.NewWithValidTestData<OrgHeader>();
			orgH.OH_Code = "~testing~";
			var orgMain = orgH.GetAddressWithFallback(AddressType.OFC);
			orgMain.OA_IsActive = true;
			AssertNotNull(orgMain);

			var orgA = orgH.Addresses.AddNew();
			orgA.OA_IsActive = true;
			orgA.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgA.OA_Address1 = "address line 1";

			Factory.Save();

			var docAddress = new BusinessObjectFactory().New<JobDocAddress>();
			docAddress.DefaultAddressType = AddressType.DLV;
			docAddress.OrganisationPK = orgH.PK;

			AssertEquals("AddressType wasn't set to DLV", AddressType.DLV, docAddress.DefaultAddressType);
			AssertEquals("AddressType wasn't used to get default address from OrgH.", ZBool.True, docAddress.Address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Delivery));
		}

		public void TestOrganisationIsMiscWhenOverridden()
		{
			AssertEquals("Precondition - OA is empty.", ZGuid.Empty, DocAddress.E2_OA_Address);
			AssertNull("Precondition - Org is null.", DocAddress.Organisation);

			ZGuid miscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			DocAddress.E2_AddressOverride = true;
			AssertEquals(miscOrgPK, DocAddress.Organisation.PK);
			AssertEquals("MISC", DocAddress.Organisation.OH_Code);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress orgAddy = org.GetAddressWithFallback(AddressType.OFC);

			DocAddress.E2_AddressOverride = false;
			DocAddress.E2_OA_Address = orgAddy.PK;
			Assert(DocAddress.Organisation.OH_Code != "MISC");
		}

		public void TestOrganisationPKIncludesMiscOrg()
		{
			AssertNull("Precondition - Org is null.", DocAddress.Organisation);

			ZGuid miscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			DocAddress.E2_AddressOverride = true;
			AssertEquals(miscOrgPK, DocAddress.OrganisationPKIncludesMiscOrg);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress orgAddy = org.GetAddressWithFallback(AddressType.OFC);

			DocAddress.E2_AddressOverride = false;
			DocAddress.E2_OA_Address = orgAddy.PK;
			AssertEquals(org.PK, DocAddress.OrganisationPKIncludesMiscOrg);
		}

		public void TestEnsureOrganisationIsMiscWhenOverridden()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var miscOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MISC");
			miscOrg.OH_IsActive = false;
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_State = "NSW";
			Factory.Save();

			ZGuid miscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			address.E2_AddressOverride = true;
			AssertEquals(miscOrgPK, address.Organisation.PK);
			AssertEquals("MISC", address.Organisation.OH_Code);

			Factory.Save();
			AssertNoErrors(address.OrganisationPKInfo);
		}

		public void TestSettingOrganisationPKThrowsNoExceptionWhenDeletedInAnotherFactory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.OrganisationPK = org.PK;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var docAddressInAnotherFactory = anotherFactory.Load<JobDocAddress>(docAddress.PK);

			docAddress.Delete();
			Factory.Save();

			AssertNoExceptionThrown(() => { docAddressInAnotherFactory.OrganisationPK = org2.PK; });
		}

		public void TestSettingInvalidOrganisationAddressDoesNotClearExistingOrganisation()
		{
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			DocAddress.E2_OA_Address = firstAddress.PK;
			AssertEquals("DocAddress should have set the address to first Address.", orgH.PK, DocAddress.OrganisationPK);
			DocAddress.E2_OA_Address = ZGuid.Invalid;
			AssertEquals("The Original Organisation must be retained, even if an invalid address is entered", orgH.PK, DocAddress.OrganisationPK);
		}

		#endregion

		#region Test AddressFull

		public void TestAddressFull()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Address1 = "JobDocAddress1";
			jobDocAddress.E2_Address2 = "JobDocAddress2";
			jobDocAddress.E2_Postcode = "0001";
			jobDocAddress.E2_City = "ALEXANDRIA";
			jobDocAddress.E2_State = "NSW";
			jobDocAddress.E2_RN_NKCountryCode = "AU";
			jobDocAddress.GeoLocation = ZGeography.CreatePoint(1.0, 1.0);
			jobDocAddress.AddressMap = "AddressMap";
			jobDocAddress.E2_ValidationStatus = "INV";
			Factory.Save();

			var expectedValue = "JobDocAddress1, JobDocAddress2, AU, ALEXANDRIA, 0001, NSW";
			AssertEquals(expectedValue, jobDocAddress.AddressFull);
		}

		#endregion

		#region Test Cartage Equipment Requirements

		public void TestCartageEquipmentRequirements()
		{
			JobDocAddress docAddress = GetNewDocAddressWithParent();
			OrgAddress address = GetNewOrgAddress();
			address.OA_Address1 = "1";
			address.OA_City = "3";
			address.OA_FCLEquipmentNeeded = "111";
			address.OA_LCLEquipmentNeeded = "222";
			address.OA_AIREquipmentNeeded = "333";

			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals(OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value, docAddress.FCLCartageEquipmentNeeded);
			AssertEquals(OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.Value, docAddress.LCLCartageEquipmentNeeded);
			AssertEquals(OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.Value, docAddress.AirCartageEquipmentNeeded);

			docAddress.E2_OA_Address = address.PK;
			AssertEquals("111", docAddress.FCLCartageEquipmentNeeded);
			AssertEquals("222", docAddress.LCLCartageEquipmentNeeded);
			AssertEquals("333", docAddress.AirCartageEquipmentNeeded);
		}

		#endregion

		#region Test Phone Numbers

		public void TestPhoneNumbers()
		{
			DocAddress.E2_Mobile = "Mobile101";
			DocAddress.E2_Phone = "Home101";
			DocAddress.E2_Fax = "Fax101";

			AssertEquals("Mobile101", DocAddress.MobilePhoneNumber.FormattedForBinding);
			AssertEquals("Home101", DocAddress.PhoneNumber.FormattedForBinding);
			AssertEquals("Fax101", DocAddress.FaxNumber.FormattedForBinding);

			AssertEquals("", DocAddress.MobilePhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("", DocAddress.PhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("", DocAddress.FaxNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
		}

		public void TestE2_Mobile_Formatted()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			DocAddress.E2_RN_NKCountryCode = ZString.Empty;
			var expectedFormattedPhone = "+61 425 465 800";
			DocAddress.E2_Mobile_Formatted = "+61 425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Mobile_Formatted);
			var expectedNormalizedPhone = "+61425465800";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Mobile);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			DocAddress.E2_RN_NKCountryCode = "AU";
			DocAddress.E2_Mobile_Formatted = "0425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Mobile_Formatted);
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Mobile);
			var expectedLocalNumberIfLoggedInSameCountry = "0425 465 800";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, DocAddress.E2_Mobile_FormattedLocalNumberIfLoggedInSameCountry);

			DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			DocAddress.E2_Mobile_Formatted = "06 01 01 01 01";
			expectedFormattedPhone = "+33 6 01 01 01 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Mobile_Formatted);
			expectedNormalizedPhone = "+33601010101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Mobile);
			AssertEquals("The tooltip should be blank", string.Empty, DocAddress.E2_Mobile_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestE2_Phone_Formatted()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			DocAddress.E2_RN_NKCountryCode = ZString.Empty;
			var expectedFormattedPhone = "+61 425 465 800";
			DocAddress.E2_Phone_Formatted = "+61 425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Phone_Formatted);
			var expectedNormalizedPhone = "+61425465800";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Phone);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			DocAddress.E2_RN_NKCountryCode = "AU";
			DocAddress.E2_Phone_Formatted = "0425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Phone_Formatted);
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Phone);
			var expectedLocalNumberIfLoggedInSameCountry = "0425 465 800";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, DocAddress.E2_Phone_FormattedLocalNumberIfLoggedInSameCountry);

			DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			DocAddress.E2_Phone_Formatted = "06 01 01 01 01";
			expectedFormattedPhone = "+33 6 01 01 01 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Phone_Formatted);
			expectedNormalizedPhone = "+33601010101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Phone);
			AssertEquals("The tooltip should be blank", string.Empty, DocAddress.E2_Phone_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestE2_Fax_Formatted()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			DocAddress.E2_RN_NKCountryCode = ZString.Empty;
			var expectedFormattedPhone = "+61 425 465 800";
			DocAddress.E2_Fax_Formatted = "+61 425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Fax_Formatted);
			var expectedNormalizedPhone = "+61425465800";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Fax);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			DocAddress.E2_RN_NKCountryCode = "AU";
			DocAddress.E2_Fax_Formatted = "0425 465 800";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Fax_Formatted);
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Fax);
			var expectedLocalNumberIfLoggedInSameCountry = "0425 465 800";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, DocAddress.E2_Fax_FormattedLocalNumberIfLoggedInSameCountry);

			DocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			DocAddress.E2_Fax_Formatted = "06 01 01 01 01";
			expectedFormattedPhone = "+33 6 01 01 01 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, DocAddress.E2_Fax_Formatted);
			expectedNormalizedPhone = "+33601010101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, DocAddress.E2_Fax);
			AssertEquals("The tooltip should be blank", string.Empty, DocAddress.E2_Fax_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestE2_Mobile_IsManuallyVerified()
		{
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, JobDocAddress.Schema.E2_Mobile_IsManuallyVerified, JobDocAddressSchema.Constants.Prefix, JobDocAddressSchema.Constants.E2_Mobile, docAddress1, docAddress2);
		}

		public void TestE2_Phone_IsManuallyVerified()
		{
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, JobDocAddress.Schema.E2_Phone_IsManuallyVerified, JobDocAddressSchema.Constants.Prefix, JobDocAddressSchema.Constants.E2_Phone, docAddress1, docAddress2);
		}

		public void TestE2_Fax_IsManuallyVerified()
		{
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, JobDocAddress.Schema.E2_Fax_IsManuallyVerified, JobDocAddressSchema.Constants.Prefix, JobDocAddressSchema.Constants.E2_Fax, docAddress1, docAddress2);
		}

		public void TestSettingPhoneNumbersResetsIsManuallyVerifiedFlag()
		{
			DocAddress.E2_Mobile_IsManuallyVerified = true;
			Assert("Precondition", DocAddress.E2_Mobile_IsManuallyVerified);
			DocAddress.E2_Mobile_Formatted = "+61 425 465 800";
			Assert(!DocAddress.E2_Mobile_IsManuallyVerified);

			DocAddress.E2_Phone_IsManuallyVerified = true;
			Assert("Precondition", DocAddress.E2_Phone_IsManuallyVerified);
			DocAddress.E2_Phone_Formatted = "+61 425 465 800";
			Assert(!DocAddress.E2_Phone_IsManuallyVerified);

			DocAddress.E2_Fax_IsManuallyVerified = true;
			Assert("Precondition", DocAddress.E2_Fax_IsManuallyVerified);
			DocAddress.E2_Fax_Formatted = "+61 425 465 800";
			Assert(!DocAddress.E2_Fax_IsManuallyVerified);
		}

		#endregion

		#region Validation

		public void TestValidationOnOverriddenFields()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.DocAddressType = DocAddressType.LocalCartageYard;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "ABC";
			docAddress.E2_City = "ABC";
			docAddress.E2_State = "ABC";
			docAddress.E2_CompanyName = "ABC";
			docAddress.E2_RN_NKCountryCode = "";

			AssertEquals("Should be valid with minimal data.", true, docAddress.IsValidAddress);

			docAddress.E2_State = "ABCdefghi!!";
			AssertEquals("Should be valid with minimal data.", true, docAddress.IsValidAddress);

			docAddress.E2_Address1 = "";
			AssertEquals("Should be invalid with less than required data.", false, docAddress.IsValidAddress);

			docAddress.E2_Address1 = "1";
			docAddress.E2_City = "";
			AssertEquals("Should be invalid with less than required data.", false, docAddress.IsValidAddress);

			docAddress.E2_City = "1";
			docAddress.E2_State = "";
			AssertEquals("Should be valid as state is not required.", true, docAddress.IsValidAddress);

			docAddress.E2_State = "1";
			docAddress.E2_CompanyName = "";
			AssertEquals("Should be invalid with less than required data.", false, docAddress.IsValidAddress);

			docAddress.E2_CompanyName = "1";
			docAddress.DocAddressType = DocAddressType.None;
			AssertEquals("Should be valid with minimal data.", true, docAddress.IsValidAddress);

			docAddress.E2_AddressType = "";
			AssertEquals("Should be invalid with less than required data.", false, docAddress.IsValidAddress);

			docAddress.E2_AddressOverride = false;
			docAddress.DocAddressType = DocAddressType.LocalCartageYard;
			AssertEquals("Should be invalid with less than required data.", false, docAddress.IsValidAddress);

			docAddress.E2_OA_Address = ZGuid.NewZGuid();
			AssertEquals("Should not be valid with a dodgy guid.", false, docAddress.IsValidAddress);
		}

		#endregion

		#region Test Synchronisation & Copying

		public void TestSynchroniseWithParentWithDetection()
		{
			var docAddress = GetNewDocAddressWithParent();
			var address = GetNewOrgAddress();
			address.PrimaryOrgAddressAdditionalInfoDetail = "0";
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_RL_NKRelatedPortCode = DefaultUNLOCOPortCode;
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			address.OA_Mobile = "9";
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_Contact = "10";

			var synchDocAddress = Factory.New<JobDocAddress>();

			AssertEquals("Bizo should not indicate it's linked.", false, synchDocAddress.HasLinkedDocAddress);

			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", ZGuid.Empty, synchDocAddress.E2_OA_Address);
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", false, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, false));
			AssertEquals("E2_OA_Address", address.PK, synchDocAddress.E2_OA_Address);
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", false, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", address.PK, synchDocAddress.E2_OA_Address);

			synchDocAddress.DeSynchroniseWithParent();
			docAddress.E2_AddressOverride = ZBool.True;
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", address.PK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.False, synchDocAddress.E2_AddressOverride);

			synchDocAddress.DeSynchroniseWithParent();
			synchDocAddress.E2_AddressOverride = ZBool.True;
			var miscAddressPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress;
			AssertEquals("changesDetected", false, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);

			synchDocAddress.E2_Address1 = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);

			synchDocAddress.E2_Address1 = docAddress.E2_Address1;
			synchDocAddress.E2_Address2 = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);

			synchDocAddress.E2_Address2 = docAddress.E2_Address2;
			synchDocAddress.E2_City = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);

			synchDocAddress.E2_City = docAddress.E2_City;
			synchDocAddress.E2_State = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);

			synchDocAddress.E2_State = docAddress.E2_State;
			synchDocAddress.E2_Postcode = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertNotEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);
			AssertEquals("E2_Postcode", ZString.Empty, synchDocAddress.E2_Postcode);

			synchDocAddress.E2_Postcode = docAddress.E2_Postcode;
			synchDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertNotEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);
			AssertNotEquals("E2_Postcode", ZString.Empty, synchDocAddress.E2_Postcode);
			AssertEquals("E2_RN_NKCountryCode", ZString.Empty, synchDocAddress.E2_RN_NKCountryCode);

			synchDocAddress.E2_RN_NKCountryCode = docAddress.E2_RN_NKCountryCode;
			synchDocAddress.E2_Contact = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertNotEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);
			AssertNotEquals("E2_Postcode", ZString.Empty, synchDocAddress.E2_Postcode);
			AssertNotEquals("E2_RN_NKCountryCode", ZString.Empty, synchDocAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_Contact", ZString.Empty, synchDocAddress.E2_Contact);

			synchDocAddress.E2_Contact = docAddress.E2_Contact;
			synchDocAddress.E2_Phone = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertNotEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);
			AssertNotEquals("E2_Postcode", ZString.Empty, synchDocAddress.E2_Postcode);
			AssertNotEquals("E2_RN_NKCountryCode", ZString.Empty, synchDocAddress.E2_RN_NKCountryCode);
			AssertNotEquals("E2_Contact", ZString.Empty, synchDocAddress.E2_Contact);
			AssertEquals("E2_Phone", ZString.Empty, synchDocAddress.E2_Phone);

			synchDocAddress.E2_Phone = docAddress.E2_Phone;
			synchDocAddress.E2_Fax = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertNotEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);
			AssertNotEquals("E2_Postcode", ZString.Empty, synchDocAddress.E2_Postcode);
			AssertNotEquals("E2_RN_NKCountryCode", ZString.Empty, synchDocAddress.E2_RN_NKCountryCode);
			AssertNotEquals("E2_Contact", ZString.Empty, synchDocAddress.E2_Contact);
			AssertNotEquals("E2_Phone", ZString.Empty, synchDocAddress.E2_Phone);
			AssertEquals("E2_Fax", ZString.Empty, synchDocAddress.E2_Fax);

			synchDocAddress.E2_Fax = docAddress.E2_Fax;
			synchDocAddress.E2_Email = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertNotEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);
			AssertNotEquals("E2_Postcode", ZString.Empty, synchDocAddress.E2_Postcode);
			AssertNotEquals("E2_RN_NKCountryCode", ZString.Empty, synchDocAddress.E2_RN_NKCountryCode);
			AssertNotEquals("E2_Contact", ZString.Empty, synchDocAddress.E2_Contact);
			AssertNotEquals("E2_Phone", ZString.Empty, synchDocAddress.E2_Phone);
			AssertNotEquals("E2_Fax", ZString.Empty, synchDocAddress.E2_Fax);
			AssertEquals("E2_Email", ZString.Empty, synchDocAddress.E2_Email);

			synchDocAddress.E2_Email = docAddress.E2_Email;
			synchDocAddress.E2_Mobile = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertNotEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);
			AssertNotEquals("E2_Postcode", ZString.Empty, synchDocAddress.E2_Postcode);
			AssertNotEquals("E2_RN_NKCountryCode", ZString.Empty, synchDocAddress.E2_RN_NKCountryCode);
			AssertNotEquals("E2_Contact", ZString.Empty, synchDocAddress.E2_Contact);
			AssertNotEquals("E2_Phone", ZString.Empty, synchDocAddress.E2_Phone);
			AssertNotEquals("E2_Fax", ZString.Empty, synchDocAddress.E2_Fax);
			AssertNotEquals("E2_Email", ZString.Empty, synchDocAddress.E2_Email);
			AssertEquals("E2_Mobile", ZString.Empty, synchDocAddress.E2_Mobile);

			synchDocAddress.E2_Mobile = docAddress.E2_Mobile;
			synchDocAddress.E2_AdditionalAddressInformation = ZString.Empty;
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("changesDetected", true, synchDocAddress.SynchroniseWithParentWithDetection(docAddress, true));
			AssertEquals("E2_OA_Address", miscAddressPK, synchDocAddress.E2_OA_Address);
			AssertEquals("E2_AddressOverride", ZBool.True, synchDocAddress.E2_AddressOverride);
			AssertNotEquals("E2_Address1", ZString.Empty, synchDocAddress.E2_Address1);
			AssertNotEquals("E2_Address2", ZString.Empty, synchDocAddress.E2_Address2);
			AssertNotEquals("E2_City", ZString.Empty, synchDocAddress.E2_City);
			AssertNotEquals("E2_State", ZString.Empty, synchDocAddress.E2_State);
			AssertNotEquals("E2_Postcode", ZString.Empty, synchDocAddress.E2_Postcode);
			AssertNotEquals("E2_RN_NKCountryCode", ZString.Empty, synchDocAddress.E2_RN_NKCountryCode);
			AssertNotEquals("E2_Contact", ZString.Empty, synchDocAddress.E2_Contact);
			AssertNotEquals("E2_Phone", ZString.Empty, synchDocAddress.E2_Phone);
			AssertNotEquals("E2_Fax", ZString.Empty, synchDocAddress.E2_Fax);
			AssertNotEquals("E2_Email", ZString.Empty, synchDocAddress.E2_Email);
			AssertNotEquals("E2_Mobile", ZString.Empty, synchDocAddress.E2_Mobile);
			AssertEquals("E2_AdditionalAddressInformation", ZString.Empty, synchDocAddress.E2_AdditionalAddressInformation);
		}

		public void TestSynchronisationOfContactBetween2Objects()
		{
			JobDocAddress docAddress = GetNewDocAddressWithParent();
			OrgAddress address = GetNewOrgAddress();
			OrgContact contact = SetDefaultContactAndCompanyForAddress(address);
			address.PrimaryOrgAddressAdditionalInfoDetail = "0";
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_RL_NKRelatedPortCode = DefaultUNLOCOPortCode;
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			docAddress.E2_OA_Address = address.PK;

			JobDocAddress synchDocAddress = Factory.New<JobDocAddress>();

			AssertEquals("Bizo should not indicate it's linked.", false, synchDocAddress.HasLinkedDocAddress);

			synchDocAddress.SynchroniseWithParent(docAddress);

			AssertEquals("Bizo should indicate it's linked.", true, synchDocAddress.HasLinkedDocAddress);

			docAddress.E2_Contact = contact.OC_ContactName;
			AssertEquals("ContactPK should have been set.", contact.PK, docAddress.ContactPK);
			AssertEquals("E2_Contact should have been set.", contact.OC_ContactName, docAddress.E2_Contact);
			AssertEquals("E2_Phone should return.", contact.OC_Phone, docAddress.E2_Phone);

			AssertEquals("SynchDocAddress ContactPK should have been set.", contact.PK, synchDocAddress.ContactPK);
			AssertEquals("SynchDocAddress E2_Contact should have been set.", contact.OC_ContactName, synchDocAddress.E2_Contact);
			AssertEquals("SynchDocAddress E2_Phone should return.", contact.OC_Phone, synchDocAddress.E2_Phone);

			docAddress.E2_AddressOverride = true;
			AssertEquals("ContactPK should have been cleared.", ZGuid.Empty, docAddress.ContactPK);
			AssertEquals("E2_Contact should have stayed", contact.OC_ContactName, docAddress.E2_Contact);
			AssertEquals("E2_Phone should have been set", contact.OC_Phone, docAddress.E2_Phone);

			AssertEquals("SynchDocAddress ContactPK should have been set.", ZGuid.Empty, synchDocAddress.ContactPK);
			AssertEquals("SynchDocAddress E2_Contact should have stayed.", contact.OC_ContactName, synchDocAddress.E2_Contact);
			AssertEquals("SynchDocAddress E2_Phone have been set", contact.OC_Phone, synchDocAddress.E2_Phone);

			docAddress.E2_Contact = "Something else";
			AssertEquals("E2_Contact should now be", "Something else", docAddress.E2_Contact);
			AssertEquals("SynchDocAddress E2_Contact now be", "Something else", synchDocAddress.E2_Contact);

			docAddress.E2_AddressOverride = false;
			AssertEquals("ContactPK should have been set.", contact.PK, docAddress.ContactPK);
			AssertEquals("E2_Contact should have been set.", contact.OC_ContactName, docAddress.E2_Contact);
			AssertEquals("E2_Phone should return.", contact.OC_Phone, docAddress.E2_Phone);

			AssertEquals("SynchDocAddress ContactPK should have been set.", contact.PK, synchDocAddress.ContactPK);
			AssertEquals("SynchDocAddress E2_Contact should have been set.", contact.OC_ContactName, synchDocAddress.E2_Contact);
			AssertEquals("SynchDocAddress E2_Phone should return.", contact.OC_Phone, synchDocAddress.E2_Phone);
		}

		public void TestSynchronisationBetween2Objects()
		{
			JobDocAddress docAddress = GetNewDocAddressWithParent();
			OrgAddress address = GetNewOrgAddress();
			OrgContact contact = SetDefaultContactAndCompanyForAddress(address);

			address.PrimaryOrgAddressAdditionalInfoDetail = "0";
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_RL_NKRelatedPortCode = DefaultUNLOCOPortCode;
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			address.OA_Mobile = "9";
			docAddress.E2_OA_Address = address.PK;

			JobDocAddress synchDocAddress = Factory.New<JobDocAddress>();

			AssertEquals("Bizo should not indicate it's linked.", false, synchDocAddress.HasLinkedDocAddress);

			synchDocAddress.SynchroniseWithParent(docAddress);

			AssertEquals("Bizo should indicate it's linked.", true, synchDocAddress.HasLinkedDocAddress);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Contact = "Contact";
			docAddress.E2_CompanyName = "Company";
			docAddress.E2_AdditionalAddressInformation = "AdditionalInfo";
			docAddress.E2_Address1 = "11";
			docAddress.E2_Address2 = "21";
			docAddress.E2_City = "31";
			docAddress.E2_State = "41";
			docAddress.E2_Postcode = "51";
			docAddress.E2_RN_NKCountryCode = "XY";
			docAddress.E2_Phone = "61";
			docAddress.E2_Fax = "71";
			docAddress.E2_Email = "81";
			docAddress.E2_Mobile = "91";

			AssertEquals(synchDocAddress.E2_AddressOverride, true);
			AssertEquals(synchDocAddress.E2_Contact, "Contact");
			AssertEquals(synchDocAddress.E2_CompanyName, "Company");
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "AdditionalInfo");
			AssertEquals(synchDocAddress.E2_Address1, "11");
			AssertEquals(synchDocAddress.E2_Address2, "21");
			AssertEquals(synchDocAddress.E2_City, "31");
			AssertEquals(synchDocAddress.E2_State, "41");
			AssertEquals(synchDocAddress.E2_Postcode, "51");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XY");
			AssertEquals(synchDocAddress.E2_Phone, "61");
			AssertEquals(synchDocAddress.E2_Fax, "71");
			AssertEquals(synchDocAddress.E2_Email, "81");
			AssertEquals(synchDocAddress.E2_Mobile, "91");

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = address.PK;
			AssertEquals(synchDocAddress.E2_AddressOverride, false);
			AssertEquals(synchDocAddress.E2_OA_Address, address.PK);
			AssertEquals(synchDocAddress.E2_Contact, "");
			AssertEquals(synchDocAddress.E2_CompanyName, DefaultCompanyName);
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "0");
			AssertEquals(synchDocAddress.E2_Address1, "1");
			AssertEquals(synchDocAddress.E2_Address2, "2");
			AssertEquals(synchDocAddress.E2_City, "3");
			AssertEquals(synchDocAddress.E2_State, "4");
			AssertEquals(synchDocAddress.E2_Postcode, "5");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XX");
			AssertEquals(synchDocAddress.E2_Phone, "6");
			AssertEquals(synchDocAddress.E2_Fax, "7");
			AssertEquals(synchDocAddress.E2_Email, "8");
			AssertEquals(synchDocAddress.E2_Mobile, "9");
		}

		public void TestBreakSynchronisationBetween2ObjectsManually()
		{
			JobDocAddress docAddress = GetNewDocAddressWithParent();
			OrgAddress address = GetNewOrgAddress();
			OrgContact contact = SetDefaultContactAndCompanyForAddress(address);

			address.PrimaryOrgAddressAdditionalInfoDetail = "0";
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_RL_NKRelatedPortCode = DefaultUNLOCOPortCode;
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			address.OA_Mobile = "9";
			docAddress.E2_OA_Address = address.PK;

			JobDocAddress synchDocAddress = Factory.New<JobDocAddress>();
			synchDocAddress.SynchroniseWithParent(docAddress);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Contact = "Contact";
			docAddress.E2_CompanyName = "Company";
			docAddress.E2_AdditionalAddressInformation = "00";
			docAddress.E2_Address1 = "11";
			docAddress.E2_Address2 = "21";
			docAddress.E2_City = "31";
			docAddress.E2_State = "41";
			docAddress.E2_Postcode = "51";
			docAddress.E2_RN_NKCountryCode = "XY";
			docAddress.E2_Phone = "61";
			docAddress.E2_Fax = "71";
			docAddress.E2_Email = "81";
			docAddress.E2_Mobile = "91";

			AssertEquals(synchDocAddress.E2_AddressOverride, true);
			AssertEquals(synchDocAddress.E2_Contact, "Contact");
			AssertEquals(synchDocAddress.E2_CompanyName, "Company");
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "00");
			AssertEquals(synchDocAddress.E2_Address1, "11");
			AssertEquals(synchDocAddress.E2_Address2, "21");
			AssertEquals(synchDocAddress.E2_City, "31");
			AssertEquals(synchDocAddress.E2_State, "41");
			AssertEquals(synchDocAddress.E2_Postcode, "51");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XY");
			AssertEquals(synchDocAddress.E2_Phone, "61");
			AssertEquals(synchDocAddress.E2_Fax, "71");
			AssertEquals(synchDocAddress.E2_Email, "81");
			AssertEquals(synchDocAddress.E2_Mobile, "91");

			AssertEquals("Bizo should indicate it's linked.", true, synchDocAddress.HasLinkedDocAddress);

			synchDocAddress.DeSynchroniseWithParent();

			AssertEquals("Bizo should indicate it's not linked.", false, synchDocAddress.HasLinkedDocAddress);

			synchDocAddress.E2_Fax = "99";
			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = address.PK;

			AssertEquals(synchDocAddress.E2_AddressOverride, true);
			AssertEquals(synchDocAddress.E2_Address1, "11");
			AssertEquals(synchDocAddress.E2_Fax, "99");

			synchDocAddress.SynchroniseWithParent(docAddress);

			AssertEquals("Bizo should indicate it's linked.", true, synchDocAddress.HasLinkedDocAddress);

			AssertEquals(synchDocAddress.E2_AddressOverride, false);
			AssertEquals(synchDocAddress.E2_OA_Address, address.PK);
			AssertEquals(synchDocAddress.E2_Contact, "");
			AssertEquals(synchDocAddress.E2_CompanyName, DefaultCompanyName);
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "0");
			AssertEquals(synchDocAddress.E2_Address1, "1");
			AssertEquals(synchDocAddress.E2_Address2, "2");
			AssertEquals(synchDocAddress.E2_City, "3");
			AssertEquals(synchDocAddress.E2_State, "4");
			AssertEquals(synchDocAddress.E2_Postcode, "5");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XX");
			AssertEquals(synchDocAddress.E2_Phone, "6");
			AssertEquals(synchDocAddress.E2_Fax, "7");
			AssertEquals(synchDocAddress.E2_Email, "8");
			AssertEquals(synchDocAddress.E2_Mobile, "9");
		}

		public void TestBreakSynchronisationBetween2ObjectsAutomatically()
		{
			JobDocAddress docAddress = GetNewDocAddressWithParent();
			OrgAddress address = GetNewOrgAddress();
			OrgContact contact = SetDefaultContactAndCompanyForAddress(address);

			address.PrimaryOrgAddressAdditionalInfoDetail = "0";
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_RL_NKRelatedPortCode = DefaultUNLOCOPortCode;
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			address.OA_Mobile = "9";
			docAddress.E2_OA_Address = address.PK;

			JobDocAddress synchDocAddress = Factory.New<JobDocAddress>();
			synchDocAddress.SynchroniseWithParent(docAddress);

			AssertEquals("Bizo should indicate it's linked.", true, synchDocAddress.HasLinkedDocAddress);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Contact = "Contact";
			docAddress.E2_CompanyName = "Company";
			docAddress.E2_AdditionalAddressInformation = "00";
			docAddress.E2_Address1 = "11";
			docAddress.E2_Address2 = "21";
			docAddress.E2_City = "31";
			docAddress.E2_State = "41";
			docAddress.E2_Postcode = "51";
			docAddress.E2_RN_NKCountryCode = "XY";
			docAddress.E2_Phone = "61";
			docAddress.E2_Fax = "71";
			docAddress.E2_Email = "81";
			docAddress.E2_Mobile = "91";

			AssertEquals(synchDocAddress.E2_AddressOverride, true);
			AssertEquals(synchDocAddress.E2_Contact, "Contact");
			AssertEquals(synchDocAddress.E2_CompanyName, "Company");
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "00");
			AssertEquals(synchDocAddress.E2_Address1, "11");
			AssertEquals(synchDocAddress.E2_Address2, "21");
			AssertEquals(synchDocAddress.E2_City, "31");
			AssertEquals(synchDocAddress.E2_State, "41");
			AssertEquals(synchDocAddress.E2_Postcode, "51");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XY");
			AssertEquals(synchDocAddress.E2_Phone, "61");
			AssertEquals(synchDocAddress.E2_Fax, "71");
			AssertEquals(synchDocAddress.E2_Email, "81");
			AssertEquals(synchDocAddress.E2_Mobile, "91");

			synchDocAddress.E2_Fax = "99";

			AssertEquals("Bizo should not indicate it's linked.", false, synchDocAddress.HasLinkedDocAddress);

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = address.PK;

			AssertEquals(synchDocAddress.E2_AddressOverride, true);
			AssertEquals(synchDocAddress.E2_Address1, "11");
			AssertEquals(synchDocAddress.E2_Fax, "99");

			synchDocAddress.SynchroniseWithParent(docAddress);

			AssertEquals(synchDocAddress.E2_AddressOverride, false);
			AssertEquals(synchDocAddress.E2_OA_Address, address.PK);
			AssertEquals(synchDocAddress.E2_Contact, "");
			AssertEquals(synchDocAddress.E2_CompanyName, DefaultCompanyName);
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "0");
			AssertEquals(synchDocAddress.E2_Address1, "1");
			AssertEquals(synchDocAddress.E2_Address2, "2");
			AssertEquals(synchDocAddress.E2_City, "3");
			AssertEquals(synchDocAddress.E2_State, "4");
			AssertEquals(synchDocAddress.E2_Postcode, "5");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XX");
			AssertEquals(synchDocAddress.E2_Phone, "6");
			AssertEquals(synchDocAddress.E2_Fax, "7");
			AssertEquals(synchDocAddress.E2_Email, "8");
			AssertEquals(synchDocAddress.E2_Mobile, "9");
		}

		public void TestCopyingBetween2Objects()
		{
			JobDocAddress docAddress = GetNewDocAddressWithParent();
			OrgAddress address = GetNewOrgAddress();
			OrgContact contact = SetDefaultContactAndCompanyForAddress(address);

			address.PrimaryOrgAddressAdditionalInfoDetail = "0";
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_RL_NKRelatedPortCode = DefaultUNLOCOPortCode;
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			address.OA_Mobile = "9";
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_Contact = "Contact (No Override)";

			JobDocAddress synchDocAddress = Factory.New<JobDocAddress>();
			synchDocAddress.SetActualFieldValuesFromParent(docAddress);

			AssertEquals(synchDocAddress.E2_AddressOverride, false);
			AssertEquals(address.OA_OH, synchDocAddress.OrganisationPK);
			AssertEquals(synchDocAddress.E2_OA_Address, address.PK);
			AssertEquals(synchDocAddress.E2_Contact, "Contact (No Override)");
			AssertEquals(synchDocAddress.E2_CompanyName, DefaultCompanyName);
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "0");
			AssertEquals(synchDocAddress.E2_Address1, "1");
			AssertEquals(synchDocAddress.E2_Address2, "2");
			AssertEquals(synchDocAddress.E2_City, "3");
			AssertEquals(synchDocAddress.E2_State, "4");
			AssertEquals(synchDocAddress.E2_Postcode, "5");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XX");
			AssertEquals(synchDocAddress.E2_Phone, "6");
			AssertEquals(synchDocAddress.E2_Fax, "7");
			AssertEquals(synchDocAddress.E2_Email, "8");
			AssertEquals(synchDocAddress.E2_Mobile, "9");

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Contact = "Contact";
			docAddress.E2_CompanyName = "Company";
			docAddress.E2_AdditionalAddressInformation = "00";
			docAddress.E2_Address1 = "11";
			docAddress.E2_Address2 = "21";
			docAddress.E2_City = "31";
			docAddress.E2_State = "41";
			docAddress.E2_Postcode = "51";
			docAddress.E2_RN_NKCountryCode = "XY";
			docAddress.E2_Phone = "61";
			docAddress.E2_Fax = "71";
			docAddress.E2_Email = "81";
			docAddress.E2_Mobile = "91";

			AssertEquals(synchDocAddress.E2_AddressOverride, false);
			AssertEquals(synchDocAddress.E2_OA_Address, address.PK);
			AssertEquals(synchDocAddress.E2_Contact, "Contact (No Override)");
			AssertEquals(synchDocAddress.E2_CompanyName, DefaultCompanyName);
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "0");
			AssertEquals(synchDocAddress.E2_Address1, "1");
			AssertEquals(synchDocAddress.E2_Address2, "2");
			AssertEquals(synchDocAddress.E2_City, "3");
			AssertEquals(synchDocAddress.E2_State, "4");
			AssertEquals(synchDocAddress.E2_Postcode, "5");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XX");
			AssertEquals(synchDocAddress.E2_Phone, "6");
			AssertEquals(synchDocAddress.E2_Fax, "7");
			AssertEquals(synchDocAddress.E2_Email, "8");
			AssertEquals(synchDocAddress.E2_Mobile, "9");

			synchDocAddress.SetActualFieldValuesFromParent(docAddress);

			AssertEquals(synchDocAddress.E2_AddressOverride, true);
			AssertEquals(synchDocAddress.E2_Contact, "Contact");
			AssertEquals(synchDocAddress.E2_CompanyName, "Company");
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "00");
			AssertEquals(synchDocAddress.E2_Address1, "11");
			AssertEquals(synchDocAddress.E2_Address2, "21");
			AssertEquals(synchDocAddress.E2_City, "31");
			AssertEquals(synchDocAddress.E2_State, "41");
			AssertEquals(synchDocAddress.E2_Postcode, "51");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XY");
			AssertEquals(synchDocAddress.E2_Phone, "61");
			AssertEquals(synchDocAddress.E2_Fax, "71");
			AssertEquals(synchDocAddress.E2_Email, "81");
			AssertEquals(synchDocAddress.E2_Mobile, "91");

			docAddress.OrganisationPK = ZGuid.Empty;
			docAddress.E2_OA_Address = ZGuid.Empty;
			synchDocAddress.SetActualFieldValuesFromParent(docAddress);

			Assert(!synchDocAddress.E2_AddressOverride);
			Assert(synchDocAddress.OrganisationPK.IsEmpty);
			Assert(synchDocAddress.E2_OA_Address.IsEmpty);
			Assert(synchDocAddress.E2_Contact.IsEmpty);
			Assert(synchDocAddress.E2_CompanyName.IsEmpty);
			Assert(synchDocAddress.E2_AdditionalAddressInformation.IsEmpty);
			Assert(synchDocAddress.E2_Address1.IsEmpty);
			Assert(synchDocAddress.E2_Address2.IsEmpty);
			Assert(synchDocAddress.E2_City.IsEmpty);
			Assert(synchDocAddress.E2_State.IsEmpty);
			Assert(synchDocAddress.E2_Postcode.IsEmpty);
			Assert(synchDocAddress.E2_RN_NKCountryCode.IsEmpty);
			Assert(synchDocAddress.E2_Phone.IsEmpty);
			Assert(synchDocAddress.E2_Fax.IsEmpty);
			Assert(synchDocAddress.E2_Email.IsEmpty);
			Assert(synchDocAddress.E2_Mobile.IsEmpty);
		}

		public void TestHasChangesWhenSetActualFieldValuesFromParent()
		{
			var docAddress = GetNewDocAddressWithParent();
			var address = GetNewOrgAddress();
			var contact = SetDefaultContactAndCompanyForAddress(address);

			address.PrimaryOrgAddressAdditionalInfoDetail = "0";
			address.OA_Address1 = "1";
			address.OA_Address2 = "2";
			address.OA_City = "3";
			address.OA_State = "4";
			address.OA_PostCode = "5";
			address.OA_RL_NKRelatedPortCode = DefaultUNLOCOPortCode;
			address.OA_Phone = "6";
			address.OA_Fax = "7";
			address.OA_Email = "8";
			address.OA_Mobile = "9";
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_Contact = "Contact (No Override)";

			var synchDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			synchDocAddress.SetActualFieldValuesFromParent(docAddress);

			AssertEquals(false, synchDocAddress.E2_AddressOverride);
			AssertEquals(synchDocAddress.OrganisationPK, address.OA_OH);
			AssertEquals(address.PK, synchDocAddress.E2_OA_Address);
			AssertEquals("Contact (No Override)", synchDocAddress.E2_Contact);
			AssertEquals(DefaultCompanyName, synchDocAddress.E2_CompanyName);
			AssertEquals("0", synchDocAddress.E2_AdditionalAddressInformation);
			AssertEquals("1", synchDocAddress.E2_Address1);
			AssertEquals("2", synchDocAddress.E2_Address2);
			AssertEquals("3", synchDocAddress.E2_City);
			AssertEquals("4", synchDocAddress.E2_State);
			AssertEquals("5", synchDocAddress.E2_Postcode);
			AssertEquals("XX", synchDocAddress.E2_RN_NKCountryCode);
			AssertEquals("6", synchDocAddress.E2_Phone);
			AssertEquals("7", synchDocAddress.E2_Fax);
			AssertEquals("8", synchDocAddress.E2_Email);
			AssertEquals("9", synchDocAddress.E2_Mobile);

			Factory.Save();
			AssertEquals(false, synchDocAddress.HasChanges);
			synchDocAddress.SetActualFieldValuesFromParent(docAddress);
			AssertEquals(false, synchDocAddress.HasChanges);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Contact = "Contact";
			docAddress.E2_CompanyName = "Company";
			docAddress.E2_AdditionalAddressInformation = "00";
			docAddress.E2_Address1 = "11";
			docAddress.E2_Address2 = "21";
			docAddress.E2_City = "31";
			docAddress.E2_State = "41";
			docAddress.E2_Postcode = "51";
			docAddress.E2_RN_NKCountryCode = "XY";
			docAddress.E2_Phone = "61";
			docAddress.E2_Fax = "71";
			docAddress.E2_Email = "81";
			docAddress.E2_Mobile = "91";

			AssertEquals(false, synchDocAddress.E2_AddressOverride);
			AssertEquals(address.PK, synchDocAddress.E2_OA_Address);
			AssertEquals("Contact (No Override)", synchDocAddress.E2_Contact);
			AssertEquals(DefaultCompanyName, synchDocAddress.E2_CompanyName);
			AssertEquals("0", synchDocAddress.E2_AdditionalAddressInformation);
			AssertEquals("1", synchDocAddress.E2_Address1);
			AssertEquals("2", synchDocAddress.E2_Address2);
			AssertEquals("3", synchDocAddress.E2_City);
			AssertEquals("4", synchDocAddress.E2_State);
			AssertEquals("5", synchDocAddress.E2_Postcode);
			AssertEquals("XX", synchDocAddress.E2_RN_NKCountryCode);
			AssertEquals("6", synchDocAddress.E2_Phone);
			AssertEquals("7", synchDocAddress.E2_Fax);
			AssertEquals("8", synchDocAddress.E2_Email);
			AssertEquals("9", synchDocAddress.E2_Mobile);

			synchDocAddress.SetActualFieldValuesFromParent(docAddress);

			AssertEquals(synchDocAddress.E2_AddressOverride, true);
			AssertEquals(synchDocAddress.E2_Contact, "Contact");
			AssertEquals(synchDocAddress.E2_CompanyName, "Company");
			AssertEquals(synchDocAddress.E2_AdditionalAddressInformation, "00");
			AssertEquals(synchDocAddress.E2_Address1, "11");
			AssertEquals(synchDocAddress.E2_Address2, "21");
			AssertEquals(synchDocAddress.E2_City, "31");
			AssertEquals(synchDocAddress.E2_State, "41");
			AssertEquals(synchDocAddress.E2_Postcode, "51");
			AssertEquals(synchDocAddress.E2_RN_NKCountryCode, "XY");
			AssertEquals(synchDocAddress.E2_Phone, "61");
			AssertEquals(synchDocAddress.E2_Fax, "71");
			AssertEquals(synchDocAddress.E2_Email, "81");
			AssertEquals(synchDocAddress.E2_Mobile, "91");

			Factory.Save();
			AssertEquals(false, synchDocAddress.HasChanges);
			synchDocAddress.SetActualFieldValuesFromParent(docAddress);
			AssertEquals(false, synchDocAddress.HasChanges);

			docAddress.OrganisationPK = ZGuid.Empty;
			docAddress.E2_OA_Address = ZGuid.Empty;
			synchDocAddress.SetActualFieldValuesFromParent(docAddress);

			Assert(!synchDocAddress.E2_AddressOverride);
			Assert(synchDocAddress.OrganisationPK.IsEmpty);
			Assert(synchDocAddress.E2_OA_Address.IsEmpty);
			Assert(synchDocAddress.E2_Contact.IsEmpty);
			Assert(synchDocAddress.E2_CompanyName.IsEmpty);
			Assert(synchDocAddress.E2_AdditionalAddressInformation.IsEmpty);
			Assert(synchDocAddress.E2_Address1.IsEmpty);
			Assert(synchDocAddress.E2_Address2.IsEmpty);
			Assert(synchDocAddress.E2_City.IsEmpty);
			Assert(synchDocAddress.E2_State.IsEmpty);
			Assert(synchDocAddress.E2_Postcode.IsEmpty);
			Assert(synchDocAddress.E2_RN_NKCountryCode.IsEmpty);
			Assert(synchDocAddress.E2_Phone.IsEmpty);
			Assert(synchDocAddress.E2_Fax.IsEmpty);
			Assert(synchDocAddress.E2_Email.IsEmpty);
			Assert(synchDocAddress.E2_Mobile.IsEmpty);

			Factory.Save();
			AssertEquals(false, synchDocAddress.HasChanges);
			synchDocAddress.SetActualFieldValuesFromParent(docAddress);
			AssertEquals(false, synchDocAddress.HasChanges);
		}

		public void TestHasChangesOnDeSynchronising()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();

			JobDocAddress synchDocAddress = Factory.New<JobDocAddress>();
			synchDocAddress.HasChanges = false;
			synchDocAddress.SynchroniseWithParent(docAddress);

			AssertEquals("Bizo should indicate it's linked.", true, synchDocAddress.HasLinkedDocAddress);

			Assert(!synchDocAddress.HasChanges);
			synchDocAddress.DeSynchroniseWithParent();
			AssertEquals("Bizo should indicate it's not linked.", false, synchDocAddress.HasLinkedDocAddress);
			Assert("Should have changes so DocAddress can be saved", synchDocAddress.HasChanges);
		}

		public void TestSynchroniseWithParentNotThrowExceptionWhenDeleted()
		{
			AssertNoExceptionThrown(() =>
			{
				JobDocAddress docAddress = Factory.New<JobDocAddress>();
				JobDocAddress synchDocAddress = Factory.New<JobDocAddress>();

				synchDocAddress.Delete();
				synchDocAddress.SynchroniseWithParent(docAddress);
			});
		}

		#endregion

		#region PopulatingThroughRequirements

		public void TestPopulatingOrganisationThroughRequirements()
		{
			MockJobDocAddressParentForTest parent = new MockJobDocAddressParentForTest(Factory);
			JobDocAddressDependentCollection docAddresses = parent.DocAddresses;
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
			requirement.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.ConsignorPickupDeliveryAddress, AddressType.PIC, ContactType.LocalTransport));

			JobDocAddress parentDocAddress = docAddresses.FindOrCreateWithRequirement(requirement);
			JobDocAddress linkedDocAddress = docAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "COMP1";
			OrgAddress address11 = org1.Addresses.AddNew(OrgAddressType.Office, true);
			address11.OA_Address1 = "Address 11";
			OrgAddress address12 = org1.Addresses.AddNew(OrgAddressType.Pickup, true);
			address12.OA_Address1 = "Address 12";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "COMP2";
			OrgAddress address21 = org1.Addresses.AddNew(OrgAddressType.Office, true);
			address21.OA_Address1 = "Address 21";
			OrgAddress address22 = org1.Addresses.AddNew(OrgAddressType.Pickup, true);
			address22.OA_Address1 = "Address 22";

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "COMP3";
			OrgAddress address31 = org1.Addresses.AddNew(OrgAddressType.Office, true);
			address31.OA_Address1 = "Address 31";
			OrgAddress address32 = org1.Addresses.AddNew(OrgAddressType.Pickup, true);
			address32.OA_Address1 = "Address 32";

			parentDocAddress.OrganisationPK = org1.PK;
			AssertEquals("Blanks orgs, org changed to 1, org should be copied", org1.PK, linkedDocAddress.OrganisationPK);
			AssertEquals("Addresses should be 1", address11.PK, parentDocAddress.E2_OA_Address);
			AssertEquals("linked docaddress addresses should be dif", address12.PK, linkedDocAddress.E2_OA_Address);

			parentDocAddress.OrganisationPK = org2.PK;
			AssertEquals("orgs1, org changed to 2, org should be copied", org2.PK, linkedDocAddress.OrganisationPK);

			linkedDocAddress.OrganisationPK = org3.PK;
			parentDocAddress.OrganisationPK = org1.PK;
			AssertEquals("linked orgs different, don't change", org3.PK, linkedDocAddress.OrganisationPK);

			parentDocAddress.OrganisationPK = org3.PK;
			parentDocAddress.OrganisationPK = org1.PK;
			AssertEquals("orgs back in synch, change", org1.PK, linkedDocAddress.OrganisationPK);
			AssertEquals("Addresses should be 1", address11.PK, parentDocAddress.E2_OA_Address);
			AssertEquals("linked docaddress addresses should be dif", address12.PK, linkedDocAddress.E2_OA_Address);
		}

		public void TestPopulatingAddressFieldsThroughRequirements()
		{
			MockJobDocAddressParentForTest parent = new MockJobDocAddressParentForTest(Factory);
			JobDocAddressDependentCollection docAddresses = parent.DocAddresses;
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
			requirement.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.ConsignorPickupDeliveryAddress, AddressType.PIC, ContactType.LocalTransport));

			JobDocAddress parentDocAddress = docAddresses.FindOrCreateWithRequirement(requirement);
			JobDocAddress linkedDocAddress = docAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "COMP1";
			OrgAddress address11 = org1.Addresses.AddNew(OrgAddressType.Office, true);
			address11.OA_Address1 = "Address 11";
			OrgAddress address12 = org1.Addresses.AddNew(OrgAddressType.Pickup, true);
			address12.OA_Address1 = "Address 12";

			parentDocAddress.OrganisationPK = org1.PK;
			AssertEquals("Addresses should be 1", address11.PK, parentDocAddress.E2_OA_Address);
			AssertEquals("linked docaddress addresses should be dif", address12.PK, linkedDocAddress.E2_OA_Address);

			parentDocAddress.E2_AddressOverride = true;
			AssertEquals("orgs in synch, change", true, linkedDocAddress.E2_AddressOverride);
			AssertEquals("company name should be same", parentDocAddress.E2_CompanyName, linkedDocAddress.E2_CompanyName);
			AssertEquals("additionalAddressInformation should be same", parentDocAddress.E2_AdditionalAddressInformation, linkedDocAddress.E2_AdditionalAddressInformation);
			AssertEquals("address1 should be same", parentDocAddress.E2_Address1, linkedDocAddress.E2_Address1);

			parentDocAddress.E2_City = "cit";
			AssertEquals("city should be same", "cit", linkedDocAddress.E2_City);

			parentDocAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			AssertEquals("validation status should be same", AddressValidationStatus.ManuallyVerified, linkedDocAddress.ValidationStatus);

			parentDocAddress.E2_Postcode = "2000";
			AssertEquals("postcode should be same", "2000", linkedDocAddress.E2_Postcode);

			parentDocAddress.E2_RN_NKCountryCode = "NZ";
			AssertEquals("country code should be same", "NZ", linkedDocAddress.E2_RN_NKCountryCode);

			parentDocAddress.E2_State = "VIC";
			AssertEquals("state should be same", "VIC", linkedDocAddress.E2_State);

			linkedDocAddress.E2_Postcode = "3000";
			parentDocAddress.E2_City = "newcit";
			AssertEquals("addresses not in synch, so should still be cit", "cit", linkedDocAddress.E2_City);

			parentDocAddress.E2_AddressOverride = false;
			AssertEquals("company names are the same so in sync for overriding", false, linkedDocAddress.E2_AddressOverride);
			AssertEquals("orgs back in synch, change", org1.PK, linkedDocAddress.OrganisationPK);

			parentDocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("orgs back in synch, change to empty", ZGuid.Empty, linkedDocAddress.OrganisationPK);
		}

		public void TestPopulatingContactFieldsThroughRequirements()
		{
			MockJobDocAddressParentForTest parent = new MockJobDocAddressParentForTest(Factory);
			JobDocAddressDependentCollection docAddresses = parent.DocAddresses;
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
			requirement.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.ConsignorPickupDeliveryAddress, AddressType.PIC, ContactType.LocalTransport));

			JobDocAddress parentDocAddress = docAddresses.FindOrCreateWithRequirement(requirement);
			JobDocAddress linkedDocAddress = docAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);

			parentDocAddress.E2_AddressOverride = true;
			parentDocAddress.E2_Contact = "contact";
			AssertEquals("contact should be same", "contact", linkedDocAddress.E2_Contact);

			parentDocAddress.E2_Phone = "2000";
			AssertEquals("phone should be same", "2000", linkedDocAddress.E2_Phone);

			parentDocAddress.E2_Fax = "4000";
			AssertEquals("fax should be same", "4000", linkedDocAddress.E2_Fax);

			parentDocAddress.E2_Mobile = "6000";
			AssertEquals("mobile should be same", "6000", linkedDocAddress.E2_Mobile);

			linkedDocAddress.E2_Email = "someone@somewhere.com";
			AssertEquals("email should be same", "someone@somewhere.com", linkedDocAddress.E2_Email);

			linkedDocAddress.E2_Fax = "12345";
			parentDocAddress.E2_Mobile = "09876";
			AssertEquals("Contact no longer in synch, mobile should still be 6000", "6000", linkedDocAddress.E2_Mobile);

			parentDocAddress.E2_AddressOverride = false;
			AssertEquals("company names are the same so in sync for overriding", false, linkedDocAddress.E2_AddressOverride);
			AssertEquals("change to empty", ZGuid.Empty, linkedDocAddress.OrganisationPK);
			AssertEquals("change to empty", ZGuid.Empty, parentDocAddress.OrganisationPK);
		}

		#endregion

		#region TestDocAddressManager

		public void TestDocAddressManager()
		{
			JobDocAddressManager manager = new JobDocAddressManager();
			JobDocAddressManager defaultManager = DocAddress.DocAddressManager;
			AssertNotNull("Address Manager should have defaulted, and be empty.", defaultManager);
			AssertEquals("Address Manager should have defaulted, and be empty.", 0, defaultManager.Requirements.Length);
			defaultManager.AddRequirement(new JobDocAddressRequirement(DocAddressType.LocalCartageExporter));
			AssertEquals("Default Address Manager should have 1 Exporter Requirement.", 1, DocAddress.DocAddressManager.Requirements.Length);
			AssertEquals("Default Address Manager should have 1 Exporter Requirement.", DocAddressType.LocalCartageExporter, DocAddress.DocAddressManager.Requirements[0].DefaultDocAddressType);

			manager.AddRequirement(new JobDocAddressRequirement(DocAddressType.LocalCartageImporter));
			manager.AddRequirement(new JobDocAddressRequirement(DocAddressType.LocalCartageCTO));
			DocAddress.DocAddressManager = manager;
			AssertEquals("Default Address Manager should have 2 Requirements.", 2, DocAddress.DocAddressManager.Requirements.Length);
			AssertEquals("Default Address Manager should have 1 Importer Requirement.", DocAddressType.LocalCartageImporter, DocAddress.DocAddressManager.Requirements[0].DefaultDocAddressType);
			AssertEquals("Default Address Manager should have 1 CTO Requirement.", DocAddressType.LocalCartageCTO, DocAddress.DocAddressManager.Requirements[1].DefaultDocAddressType);
		}

		#endregion

		#region TestExplicitDocAddressTyping

		public void TestExplicitDocAddressTyping()
		{
			DocAddress.DocAddressType = DocAddressType.LocalCartageCTO;
			AssertEquals(DocAddress.DocAddressType, DocAddressType.LocalCartageCTO);
			AssertEquals(DocAddress.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageCTO));

			DocAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageCFS);
			AssertEquals(DocAddress.DocAddressType, DocAddressType.LocalCartageCTO);
			AssertEquals(DocAddress.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageCFS));

			DocAddress.DocAddressType = DocAddressType.LocalCartageCTO;
			AssertEquals(DocAddress.DocAddressType, DocAddressType.LocalCartageCTO);
			AssertEquals(DocAddress.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageCTO));
		}

		#endregion

		#region TestExplicitAddressTyping

		public void TestExplicitAddressTyping()
		{
			AssertEquals(DocAddress.DefaultAddressType, AddressType.OFC);
			DocAddress.DefaultAddressType = AddressType.DLV;
			AssertEquals(DocAddress.DefaultAddressType, AddressType.DLV);

			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			OrgAddress nextAddress = GetNewOrgAddress();
			nextAddress.AddressCapability.DisableAllCapabilities();
			nextAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(nextAddress);

			DocAddress.OrganisationPK = orgH.PK;

			AssertEquals("DocAddress should have defaulted to the Delivery address of the Org.", nextAddress.PK, DocAddress.E2_OA_Address);

			DocAddress.OrganisationPK = ZGuid.Empty;
			nextAddress.AddressCapability.DisableAllCapabilities();
			nextAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			DocAddress.OrganisationPK = orgH.PK;

			AssertEquals("DocAddress should have defaulted to the fallback MAIN address of the Org.", orgH.MainAddress.PK, DocAddress.E2_OA_Address);
		}

		#endregion

		#region TestSettingOrganisations

		public void TestSettingOrganisations()
		{
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			DocAddress.OrganisationPK = orgH.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			OrgAddress nextAddress = GetNewOrgAddress();
			OrgHeader orgH2 = Factory.New<OrgHeader>();
			nextAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH2.Addresses.Add(nextAddress);
			orgH2.OH_FullName = DefaultCompanyName;
			orgH2.OH_Code = DefaultCompanyCode;

			DocAddress.OrganisationPK = orgH2.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", nextAddress.PK, DocAddress.E2_OA_Address);

			DocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);

			DocAddress.E2_OA_Address = firstAddress.PK;
			AssertEquals("DocAddress organisation should have defaulted to that of the address.", orgH.PK, DocAddress.OrganisationPK);
		}

		#endregion

		#region TestSettingOverrideShouldRememberOrganisationAndAddress

		public void TestSettingOverrideShouldRememberOrganisationAndAddress()
		{
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			DocAddress.OrganisationPK = orgH.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			OrgAddress nextAddress = GetNewOrgAddress();
			OrgHeader orgH2 = Factory.New<OrgHeader>();
			nextAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH2.Addresses.Add(nextAddress);
			orgH2.OH_FullName = DefaultCompanyName;
			orgH2.OH_Code = DefaultCompanyCode;

			DocAddress.OrganisationPK = orgH2.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", nextAddress.PK, DocAddress.E2_OA_Address);

			DocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);

			DocAddress.E2_OA_Address = firstAddress.PK;
			AssertEquals("DocAddress organisation should have defaulted to that of the address.", orgH.PK, DocAddress.OrganisationPK);

			DocAddress.E2_AddressOverride = true;
			AssertEquals("DocAddress address should be MISC.", "MISC", DocAddress.Address.Header.OH_Code);
			AssertEquals("DocAddress organisation should be empty.", ZGuid.Empty, DocAddress.OrganisationPK);

			DocAddress.E2_AddressOverride = false;
			AssertEquals("DocAddress address should have been remembered, and set back.", firstAddress.PK, DocAddress.E2_OA_Address);
			AssertEquals("DocAddress organisation should have been remembered, and set back.", orgH.PK, DocAddress.OrganisationPK);
		}

		#endregion

		#region TestClearingAddress_ShouldKeepOrg

		public void TestClearingAddress_ShouldKeepOrg()
		{
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			DocAddress.OrganisationPK = orgH.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			OrgAddress nextAddress = GetNewOrgAddress();
			OrgHeader orgH2 = Factory.New<OrgHeader>();
			nextAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH2.Addresses.Add(nextAddress);
			orgH2.OH_FullName = DefaultCompanyName;
			orgH2.OH_Code = DefaultCompanyCode;

			DocAddress.OrganisationPK = orgH2.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", nextAddress.PK, DocAddress.E2_OA_Address);

			DocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);

			DocAddress.E2_OA_Address = firstAddress.PK;
			AssertEquals("DocAddress organisation should have defaulted to that of the address.", orgH.PK, DocAddress.OrganisationPK);

			DocAddress.E2_AddressOverride = true;
			AssertEquals("DocAddress address should be MISC.", "MISC", DocAddress.Address.Header.OH_Code);
			AssertEquals("DocAddress organisation should be empty.", ZGuid.Empty, DocAddress.OrganisationPK);

			DocAddress.E2_AddressOverride = false;
			AssertEquals("DocAddress address should have been remembered, and set back.", firstAddress.PK, DocAddress.E2_OA_Address);
			AssertEquals("DocAddress organisation should have been remembered, and set back.", orgH.PK, DocAddress.OrganisationPK);

			DocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("DocAddress organisation should have defaulted to that of the address.", orgH.PK, DocAddress.OrganisationPK);
			AssertEquals("DocAddress should not be readonly", false, DocAddress.E2_OA_AddressInfo.ReadOnly);

			AssertEquals("DocAddress should have errors", true, DocAddress.HasErrors);
			DocAddress.E2_OA_Address = firstAddress.PK;
			DocAddress.E2_ParentID = Factory.NewWithValidTestData<DummyBusinessObject>().PK;
			DocAddress.E2_AddressType = DocAddressTypes.Codes.BookingPartyDocumentaryAddress;
			AssertEquals("DocAddress should not have any errors", false, DocAddress.HasErrors);
		}

		#endregion

		#region TestSettingContacts

		public void TestSettingContactReturnsValuesFromContact()
		{
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);

			DocAddress.DefaultContactType = ContactType.NotifyParty;
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgHeader orgH = Factory.New<OrgHeader>();
			OrgAddress firstAddress = GetNewOrgAddress();
			orgH.Addresses.Add(firstAddress);

			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			firstAddress.OA_Phone = "888 111";
			firstAddress.OA_Fax = "888 222";
			firstAddress.OA_Email = "Bull@tiger.com";
			firstAddress.OA_Mobile = "000";

			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			OrgContact orgC = orgH.Contacts.AddNew();
			orgC.OC_ContactName = "Hey Dude";
			orgC.OC_Phone = "123";
			orgC.OC_Fax = "456";
			orgC.OC_Email = "qqq@123.com";
			orgC.OC_Mobile = "777";

			DocAddress.E2_OA_Address = firstAddress.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			DocAddress.ContactPK = orgC.PK;
			AssertEquals("DocAddress should have that of the set Contact's name.", orgC.OC_ContactName, DocAddress.E2_Contact);
			AssertEquals("DocAddress should have that of the set Contact's Phone.", orgC.OC_Phone, DocAddress.E2_Phone);
			AssertEquals("DocAddress should have that of the set Contact's Fax.", orgC.OC_Fax, DocAddress.E2_Fax);
			AssertEquals("DocAddress should have that of the set Contact's Email.", orgC.OC_Email, DocAddress.E2_Email);
			AssertEquals("DocAddress should have that of the set Contact's Mobile.", orgC.OC_Mobile, DocAddress.E2_Mobile);

			DocAddress.E2_Contact = "Wally";
			AssertEquals("DocAddress should have that of the set Contact's name.", "Wally", DocAddress.E2_Contact);
			AssertEquals("DocAddress should have that of the set Address's Phone.", firstAddress.OA_Phone, DocAddress.E2_Phone);
			AssertEquals("DocAddress should have that of the set Address's Fax.", firstAddress.OA_Fax, DocAddress.E2_Fax);
			AssertEquals("DocAddress should have that of the set Address's Email.", firstAddress.OA_Email, DocAddress.E2_Email);
			AssertEquals("DocAddress should have that of the set Address's Mobile.", firstAddress.OA_Mobile, DocAddress.E2_Mobile);
		}

		public void TestSettingContact()
		{
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);
			DocAddress.DefaultContactType = ContactType.NotifyParty;
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			OrgContact orgC = orgH.Contacts.AddNew();
			orgC.OC_ContactName = "Hey Dude";
			OrgDocument ordD = orgC.Documents.AddNew();
			ordD.OD_DocumentGroup = ContactType.NotifyParty.ToString();

			DocAddress.OrganisationPK = orgH.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			DocAddress.E2_Contact = "Go Away";
			AssertEquals("DocAddress should have lost its default to the default Contact.", "Go Away", DocAddress.E2_Contact);

			orgC.OC_ContactName = "Come Back";
			DocAddress.ContactPK = orgC.PK;
			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			orgC.Delete();
			AssertEquals("DocAddress should have not have lost its default Contact as it is stored in E2_Contact.", "Come Back", DocAddress.E2_Contact);
		}

		public void TestSettingContactAndGetDefaultOrgHeader()
		{
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);
			DocAddress.DefaultContactType = ContactType.NotifyParty;
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			OrgContact orgC = orgH.Contacts.AddNew();
			orgC.OC_ContactName = "Hey Dude";
			OrgDocument ordD = orgC.Documents.AddNew();
			ordD.OD_DocumentGroup = ContactType.NotifyParty.ToString();

			DocAddress.ContactPK = orgC.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			DocAddress.E2_Contact = "Go Away";
			AssertEquals("DocAddress should have lost its default to the default Contact.", "Go Away", DocAddress.E2_Contact);

			orgC.OC_ContactName = "Come Back";
			DocAddress.ContactPK = orgC.PK;
			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			orgC.Delete();
			AssertEquals("DocAddress should have not have lost its default Contact as it is stored in E2_Contact.", "Come Back", DocAddress.E2_Contact);
		}

		public void TestSettingContactWithFallback()
		{
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);
			DocAddress.DefaultContactType = ContactType.Consignee;
			DocAddress.FallbackContactType = ContactType.NotifyParty;
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			OrgContact orgC = orgH.Contacts.AddNew();
			orgC.OC_ContactName = "Hey Dude";
			OrgDocument ordD = orgC.Documents.AddNew();
			ordD.OD_DocumentGroup = ContactType.NotifyParty.ToString();

			OrgContact orgC2 = orgH.Contacts.AddNew();
			orgC2.OC_ContactName = "Hey Dude2";
			OrgDocument ordD2 = orgC2.Documents.AddNew();
			ordD2.OD_DocumentGroup = ContactType.Consignee.ToString();

			DocAddress.OrganisationPK = orgH.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC2.OC_ContactName, DocAddress.E2_Contact);

			DocAddress.E2_Contact = "Go Away";
			AssertEquals("DocAddress should have lost its default to the default Contact.", "Go Away", DocAddress.E2_Contact);

			orgC.OC_ContactName = "Come Back";
			DocAddress.ContactPK = orgC.PK;
			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			orgC2.Delete();
			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			orgC.Delete();
			AssertEquals("DocAddress should have not have lost its default Contact as it is stored in E2_Contact.", "Come Back", DocAddress.E2_Contact);
		}

		public void TestSettingContactWithFallbackWithChangedOrgs()
		{
			AssertEquals(DocAddress.E2_OA_Address, ZGuid.Empty);
			DocAddress.DefaultContactType = ContactType.Consignee;
			DocAddress.FallbackContactType = ContactType.NotifyParty;
			DocAddress.DefaultAddressType = AddressType.DLV;
			OrgAddress firstAddress = GetNewOrgAddress();
			OrgHeader orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			OrgContact orgC = orgH.Contacts.AddNew();
			orgC.OC_ContactName = "Hey Dude";
			OrgDocument ordD = orgC.Documents.AddNew();
			ordD.OD_DocumentGroup = ContactType.NotifyParty.ToString();

			OrgContact orgC2 = orgH.Contacts.AddNew();
			orgC2.OC_ContactName = "Hey Dude2";
			OrgDocument ordD2 = orgC2.Documents.AddNew();
			ordD2.OD_DocumentGroup = ContactType.Consignee.ToString();

			DocAddress.OrganisationPK = orgH.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC2.OC_ContactName, DocAddress.E2_Contact);

			DocAddress.E2_Contact = "Go Away";
			AssertEquals("DocAddress should have lost its default to the default Contact.", "Go Away", DocAddress.E2_Contact);

			orgC.OC_ContactName = "Come Back";
			DocAddress.ContactPK = orgC.PK;
			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			orgC2.Delete();
			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			orgC.Delete();
			AssertEquals("DocAddress should have not have lost its default Contact as it is stored in E2_Contact.", "Come Back", DocAddress.E2_Contact);

			firstAddress = GetNewOrgAddress();
			orgH = Factory.New<OrgHeader>();
			firstAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgH.Addresses.Add(firstAddress);
			orgH.OH_FullName = DefaultCompanyName;
			orgH.OH_Code = DefaultCompanyCode;

			orgC = orgH.Contacts.AddNew();
			orgC.OC_ContactName = "Hey Dude";
			ordD = orgC.Documents.AddNew();
			ordD.OD_DocumentGroup = ContactType.NotifyParty.ToString();

			orgC2 = orgH.Contacts.AddNew();
			orgC2.OC_ContactName = "Hey Dude2";
			ordD2 = orgC2.Documents.AddNew();
			ordD2.OD_DocumentGroup = ContactType.Consignee.ToString();

			DocAddress.OrganisationPK = orgH.PK;
			AssertEquals("DocAddress should have defaulted to the address of the Org.", firstAddress.PK, DocAddress.E2_OA_Address);

			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC2.OC_ContactName, DocAddress.E2_Contact);

			DocAddress.E2_Contact = "Go Away";
			AssertEquals("DocAddress should have lost its default to the default Contact.", "Go Away", DocAddress.E2_Contact);

			orgC.OC_ContactName = "Come Back";
			DocAddress.ContactPK = orgC.PK;
			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			orgC2.Delete();
			AssertEquals("DocAddress should have defaulted to the default Contact.", orgC.OC_ContactName, DocAddress.E2_Contact);

			orgC.Delete();
			AssertEquals("DocAddress should have not have lost its default Contact as it is stored in E2_Contact.", "Come Back", DocAddress.E2_Contact);
		}

		#endregion

		#region TestOrgAddressChange

		public void TestOrgAddressChange()
		{
			DocAddress.OrgAddressBeforeChange += new EventHandler(OrgAddressBeforeChange);
			OrgAddress orgA = GetNewOrgAddress();
			SetDefaultContactAndCompanyForAddress(orgA);

			AssertEquals("Original OrgAddress should be null.", ZGuid.Empty, DocAddress.E2_OA_Address);

			DocAddress.E2_OA_Address = orgA.PK;
			AssertEquals("Original OrgAddressPK should be empty.", ZGuid.Empty, CurrentPK);

			OrgAddress orgA2 = GetNewOrgAddress();
			SetDefaultContactAndCompanyForAddress(orgA2);
			DocAddress.E2_OA_Address = orgA2.PK;

			AssertEquals("Original OrgAddressPK should be last one.", orgA.PK, CurrentPK);
			AssertEquals("Current OrgAddressPK should be last one.", orgA2.PK, DocAddress.E2_OA_Address);
		}

		void OrgAddressBeforeChange(object sender, EventArgs e)
		{
			CurrentPK = DocAddress.E2_OA_Address;
		}

		ZGuid CurrentPK;

		#endregion

		#region TestChangeEvents

		public void TestBeforeChangeEvents()
		{
			DocAddress.AnyAddressFieldBeforeChange += new EventHandler(DocAddress_AnyAddressFieldBeforeChange);

			DocAddress.E2_AddressOverride = true;
			DocAddress.E2_Contact = "Contact";
			DocAddress.E2_CompanyName = "Company";
			DocAddress.E2_AdditionalAddressInformation = "Additional";
			DocAddress.E2_Address1 = "11";
			DocAddress.E2_Address2 = "21";
			DocAddress.E2_City = "31";
			DocAddress.E2_State = "41";
			DocAddress.E2_Postcode = "51";
			DocAddress.E2_RN_NKCountryCode = "XY";
			DocAddress.E2_Phone = "61";
			DocAddress.E2_Fax = "71";
			DocAddress.E2_Email = "81";
			DocAddress.E2_Mobile = "91";

			AssertEquals("Precondition", 9, FieldCount);

			DocAddress.E2_CompanyName = "Company1";
			AssertEquals("Previous value incorrectly set.", PreviousValue + "1", DocAddress.E2_CompanyName);
			DocAddress.E2_AdditionalAddressInformation = "Additional1";
			AssertEquals("Previous value incorrectly set.", PreviousValue + "1", DocAddress.E2_AdditionalAddressInformation);
			DocAddress.E2_Address1 = "111";
			AssertEquals("Previous value incorrectly set.", PreviousValue + "1", DocAddress.E2_Address1);
			DocAddress.E2_Address2 = "211";
			AssertEquals("Previous value incorrectly set.", PreviousValue + "1", DocAddress.E2_Address2);
			DocAddress.E2_City = "311";
			AssertEquals("Previous value incorrectly set.", PreviousValue + "1", DocAddress.E2_City);
			DocAddress.E2_State = "411";
			AssertEquals("Previous value incorrectly set.", PreviousValue + "1", DocAddress.E2_State);
			DocAddress.E2_Postcode = "511";
			AssertEquals("Previous value incorrectly set.", PreviousValue + "1", DocAddress.E2_Postcode);
			DocAddress.E2_RN_NKCountryCode = "AB";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "XY");

			PreviousValue = "";
			DocAddress.E2_Contact = "Contact1";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_Phone = "611";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_Fax = "711";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_Email = "811";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_Mobile = "911";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
		}

		public void TestAfterChangeEvents()
		{
			DocAddress.DocAddressChanged += new EventHandler(DocAddress_AnyAddressFieldAfterChange);

			DocAddress.E2_AddressOverride = true;
			DocAddress.E2_Contact = "Contact";
			DocAddress.E2_CompanyName = "Company";
			DocAddress.E2_AdditionalAddressInformation = "00";
			DocAddress.E2_Address1 = "11";
			DocAddress.E2_Address2 = "21";
			DocAddress.E2_City = "31";
			DocAddress.E2_State = "41";
			DocAddress.E2_Postcode = "51";
			DocAddress.E2_RN_NKCountryCode = "XY";

			AssertEquals("Precondition", 10, FieldCount);

			// these should not cause DocAddressChanged to fire
			DocAddress.E2_Phone = "61";
			DocAddress.E2_Fax = "71";
			DocAddress.E2_Email = "81";
			DocAddress.E2_Mobile = "91";

			AssertEquals("The field change count hasn't updated.", 10, FieldCount);

			DocAddress.E2_CompanyName = "Company1";
			DocAddress.E2_AdditionalAddressInformation = "000";
			DocAddress.E2_Address1 = "111";
			DocAddress.E2_Address2 = "211";
			DocAddress.E2_City = "311";
			DocAddress.E2_State = "411";
			DocAddress.E2_Postcode = "511";
			DocAddress.E2_RN_NKCountryCode = "AB";

			AssertEquals("The field change count has updated.", 18, FieldCount);

			DocAddress.E2_Contact = "Contact1";
			DocAddress.E2_Phone = "611";
			DocAddress.E2_Fax = "711";
			DocAddress.E2_Email = "811";
			DocAddress.E2_Mobile = "911";

			AssertEquals("The field change count hasn't updated.", 18, FieldCount);
		}

		public void TestBypassChangeEvents()
		{
			DocAddress.AnyAddressFieldBeforeChange += new EventHandler(DocAddress_AnyAddressFieldBeforeChange);

			DocAddress.E2_AddressOverride = true;
			DocAddress.E2_Contact = "Contact";
			DocAddress.E2_AdditionalAddressInformation = "00";
			DocAddress.E2_CompanyName = "Company";
			DocAddress.E2_Address1 = "11";
			DocAddress.E2_Address2 = "21";
			DocAddress.E2_City = "31";
			DocAddress.E2_State = "41";
			DocAddress.E2_Postcode = "51";
			DocAddress.E2_RN_NKCountryCode = "XY";
			DocAddress.E2_Phone = "61";
			DocAddress.E2_Fax = "71";
			DocAddress.E2_Email = "81";
			DocAddress.E2_Mobile = "91";

			AssertEquals("Precondition", 9, FieldCount);

			PreviousValue = "";

			DocAddress.BypassFireEventBeforeChange = true;
			DocAddress.E2_CompanyName = "Company1";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_AdditionalAddressInformation = "111";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_Address1 = "111";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_Address2 = "211";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_City = "311";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_State = "411";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_Postcode = "511";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");
			DocAddress.E2_RN_NKCountryCode = "AB";
			AssertEquals("Previous value incorrectly set.", PreviousValue, "");

			AssertEquals("The field change count hasn't updated.", 9, FieldCount);
		}

		void DocAddress_AnyAddressFieldBeforeChange(object sender, EventArgs e)
		{
			FieldCount++;
			ZPropertyInfo propertyInfo = (ZPropertyInfo)sender;
			PreviousValue = (ZString)propertyInfo.Value;
		}

		void DocAddress_AnyAddressFieldAfterChange(object sender, EventArgs e)
		{
			FieldCount++;
		}

		int FieldCount;
		ZString PreviousValue;

		#endregion

		#region TestContact

		public void TestContact()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Di";

			OrgAddress orgAddy = GetNewOrgAddress();
			org.Addresses.Add(orgAddy);

			DocAddress.E2_OA_Address = orgAddy.PK;
			AssertEquals("Precondition - DocAddress has no contact.", null, DocAddress.Contact);

			DocAddress.ContactPK = contact.PK;
			AssertEquals("DocAddress contact should be set.", contact.PK, DocAddress.Contact.PK);
			AssertEquals("DocAddress contact should be set.", "Di", DocAddress.E2_Contact);
		}

		public void TestContact_DoesntHitDbForNonPersistentSystemDefault()
		{
			var docAddress = GetNewDocAddressWithParent();
			var address = GetNewOrgAddress();
			address.FillWithValidTestData();
			docAddress.E2_OA_Address = address.PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var docAddressInOtherFactory = factory2.Load<JobDocAddress>(docAddress.PK);
			docAddressInOtherFactory.DefaultContactType = ContactType.Consignee;

			var poke1 = docAddressInOtherFactory.Contact;
			AssertTableHitCount("Should hit database only once while attempting to find a non persistent contact.", 1, OrgContactSchema.Constants.TableName, factory2);

			var contact = docAddressInOtherFactory.Organisation.Contacts.AddNew();
			contact.OC_ContactName = "Raphael";
			contact.OC_Phone = "123";
			contact.OC_Fax = "456";
			contact.OC_Email = "moo@moo.com";
			contact.OC_Mobile = "000";

			docAddressInOtherFactory.ContactPK = contact.PK;
			AssertEquals(contact, docAddressInOtherFactory.Contact);
		}

		public void TestContactDetailsRetainedWhenSwappingBetweenOverride()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Raphael";
			contact.OC_Phone = "123";
			contact.OC_Fax = "456";
			contact.OC_Email = "moo@moo.com";
			contact.OC_Mobile = "000";

			OrgAddress orgAddy = GetNewOrgAddress();
			org.Addresses.Add(orgAddy);

			DocAddress.E2_OA_Address = orgAddy.PK;
			DocAddress.ContactPK = contact.PK;

			DocAddress.E2_AddressOverride = true;
			AssertEquals("Contact name should have been copied to E2_Contact when overriding.", "Raphael", DocAddress.E2_Contact);
			AssertEquals("Contact phone should have been copied to E2_Phone when overriding.", "123", DocAddress.E2_Phone);
			AssertEquals("Contact fax should have been copied to E2_Fax when overriding.", "456", DocAddress.E2_Fax);
			AssertEquals("Contact e-mail should have been copied to E2_Email when overriding.", "moo@moo.com", DocAddress.E2_Email);
			AssertEquals("Contact mobile should have been copied to E2_Mobile when overriding.", "000", DocAddress.E2_Mobile);

			DocAddress.E2_AddressOverride = false;
			AssertEquals("Contact should have been retained when going from override back to non-override.", "Raphael", DocAddress.E2_Contact);
			AssertEquals("Contact should have been retained when going from override back to non-override.", contact.PK, DocAddress.Contact.PK);
			AssertEquals("Contact phone should have been retained when going from override back to non-override.", "123", DocAddress.E2_Phone);
			AssertEquals("Contact fax should have been retained when going from override back to non-override.", "456", DocAddress.E2_Fax);
			AssertEquals("Contact e-mail should have been retained when going from override back to non-override.", "moo@moo.com", DocAddress.E2_Email);
			AssertEquals("Contact mobile should have been retained when going from override back to non-override.", "000", DocAddress.E2_Mobile);
		}

		#endregion

		#region TestHasRealAddress

		public void TestHasRealAddress()
		{
			DocAddress.E2_AddressOverride = true;
			AssertEquals(false, DocAddress.HasRealAddress);

			DocAddress.E2_AddressOverride = false;
			DocAddress.E2_OA_Address = GetNewOrgAddress().PK;
			AssertEquals(true, DocAddress.HasRealAddress);

			DocAddress.E2_OA_Address = ZGuid.NewZGuid();
			AssertEquals(false, DocAddress.HasRealAddress);
		}

		public void TestOnLoadedStoresOrgPK()
		{
			OrgAddress address = GetNewOrgAddress();
			address.FillWithValidTestData();
			DocAddress.FillWithValidTestData();
			DocAddress.E2_AddressOverride = false;
			DocAddress.E2_OA_Address = address.PK;
			Factory.Save();

			JobDocAddress reloadedDocAddress = new BusinessObjectFactory().Load<JobDocAddress>(DocAddress.PK);
			reloadedDocAddress.SetDefaultAddressFromOrg();
			AssertNotEquals(ZGuid.Empty, reloadedDocAddress.E2_OA_Address);
		}

		#endregion

		#region TestHasRealOrganisation

		public void TestHasRealOrganizationFactoryHits()
		{
			var factory = new BusinessObjectFactory();

			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "MOHS";

			var address = factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "MOHS";

			var docAddress = factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = address.PK;
			docAddress.OrganisationPK = orgHeader.PK;

			factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadedDocAddress = factory2.Load<JobDocAddress>(docAddress.PK);

			try
			{
				factory2.ResetDatabaseLoadCount();
				BusinessObjectFactory.StopLogging();
				BusinessObjectFactory.StartLogging();

				var result = loadedDocAddress.HasRealOrganisation;
				var loadCounter = GetLoadCountForEachTable(BusinessObjectFactory.DebugLog);
				AssertEquals(4, loadCounter["Enterprise.MasterFiles.Business.OrgAddress"]);
				AssertEquals(2, loadCounter["Enterprise.MasterFiles.Business.OrgHeader"]);
				AssertEquals(6, BusinessObjectFactory.DebugLogCount);
			}
			finally
			{
				BusinessObjectFactory.StopLogging();
			}
		}

		public void TestHasRealOrganizationFactoryHits_WhenOrganizationIsNotReal()
		{
			var factory = new BusinessObjectFactory();

			var docAddress = factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = true;

			factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadedDocAddress = factory2.Load<JobDocAddress>(docAddress.PK);

			try
			{
				factory2.ResetDatabaseLoadCount();
				BusinessObjectFactory.StopLogging();
				BusinessObjectFactory.StartLogging();

				var result = loadedDocAddress.HasRealOrganisation;
				var loadCounter = GetLoadCountForEachTable(BusinessObjectFactory.DebugLog);
				AssertEquals(1, loadCounter["Enterprise.MasterFiles.Business.OrgHeader"]);
				AssertEquals(1, BusinessObjectFactory.DebugLogCount);
			}
			finally
			{
				BusinessObjectFactory.StopLogging();
			}
		}

		public static Dictionary<string, int> GetLoadCountForEachTable(string debugLog)
		{
			var logs = debugLog.Split('\r', '\n');
			var result = new Dictionary<string, int>();

			foreach (var log in logs)
			{
				if (log.Contains("LOAD"))
				{
					var splittedlog = log.Split(' ');
					var tableName = (splittedlog[3] == "FromUniqueKey") ? splittedlog[4] : splittedlog[3];

					if (!result.ContainsKey(tableName))
					{
						result.Add(tableName, 1);
					}
					else
					{
						result[tableName] = result[tableName] + 1;
					}
				}
			}

			return result;
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			AssertEquals("Initially IsEmpty should be true", true, docAddress.IsEmpty);
			docAddress.E2_AddressType = DocAddressTypes.Codes.BookingPartyDocumentaryAddress;
			AssertEquals("Still IsEmpty should be true even a DocAddressType is specified", true, docAddress.IsEmpty);
			docAddress.E2_AddressOverride = true;
			AssertEquals("IsEmpty should be false because the Address is overridden", false, docAddress.IsEmpty);
			docAddress.E2_AddressOverride = false;
			AssertEquals("IsEmpty should be now True because the Address is not overridden", true, docAddress.IsEmpty);
			docAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
			AssertEquals("IsEmpty should be false because there is an OrgAddress attached to it", false, docAddress.IsEmpty);
			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("IsEmpty should be true because the OrgAddress is now gone", true, docAddress.IsEmpty);
		}

		public void TestIsOverridenButEmpty()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			AssertEquals("Initially IsOverridenButEmpty should be false", false, docAddress.IsOverridenButEmpty);
			docAddress.E2_AddressType = DocAddressTypes.Codes.BookingPartyDocumentaryAddress;
			AssertEquals("Still IsOverridenButEmpty should be false even a DocAddressType is specified", false, docAddress.IsOverridenButEmpty);
			docAddress.E2_AddressOverride = true;
			AssertEquals("IsOverridenButEmpty should be true because the Address is overridden", true, docAddress.IsOverridenButEmpty);
			docAddress.E2_Address1 = "Add1";
			AssertEquals("IsOverridenButEmpty should be false because there is some info filled in", false, docAddress.IsOverridenButEmpty);
		}
		#endregion

		#region PopulateAddressThroughRequirement

		public void TestPopulateAddressThroughRequirement()
		{
			JobDocAddress testAddress = GetNewDocAddressWithParent();
			testAddress.ShouldAlwaysUpdateSecondary = true;
			testAddress.OverrideRequirement = new JobDocAddressRequirement();
			testAddress.Requirement.AddLinkedRequirement(DocAddressType.BuyingParty);
			JobDocAddress resultAddress = testAddress.Parent.DocAddresses.AddNew(DocAddressType.BuyingParty);

			OrgAddress address = Factory.New<OrgAddress>();
			testAddress.E2_OA_Address = address.PK;
			AssertEquals("Secondary Address should be the same because flag is true", resultAddress.E2_OA_Address, testAddress.E2_OA_Address);

			testAddress.ShouldAlwaysUpdateSecondary = false;
			OrgAddress address2 = Factory.New<OrgAddress>();
			testAddress.E2_OA_Address = address2.PK;
			AssertEquals(resultAddress.E2_OA_Address, address.PK);
			AssertEquals(testAddress.E2_OA_Address, address2.PK);
		}

		public void TestSecondaryPropertiesPopulated()
		{
			var testAddress = GetNewDocAddressWithParent();
			testAddress.ShouldAlwaysUpdateSecondary = true;
			testAddress.OverrideRequirement = new JobDocAddressRequirement();
			testAddress.Requirement.AddLinkedRequirement(DocAddressType.ConsignorPickupDeliveryAddress);
			var secondaryAddress = testAddress.Parent.DocAddresses.AddNew(DocAddressType.ConsignorPickupDeliveryAddress);

			AssertEquals(ZString.Empty, secondaryAddress.E2_AddressMap);
			AssertEquals(ZGeography.Empty, secondaryAddress.E2_GeoLocation);
			Assert(!secondaryAddress.E2_SuppressAddressValidationError);

			testAddress.E2_AddressOverride = true;
			testAddress.AddressMap = "SNA1[0-0]SA1[2-9]";
			testAddress.E2_GeoLocation = ZGeography.CreatePoint(-4.1, 55.7);
			testAddress.E2_SuppressAddressValidationError = true;

			AssertEquals("Secondary Address property synced: E2_AddressMap", testAddress.AddressMap, secondaryAddress.E2_AddressMap);
			AssertEquals("Secondary Address property synced: E2_GeoLocation", testAddress.E2_GeoLocation, secondaryAddress.E2_GeoLocation);
			Assert("Secondary Address property synced: E2_SuppressAddressValidationError", secondaryAddress.E2_SuppressAddressValidationError);
		}

		#endregion

		#region TestJobDocAddressIsDeletedOnSavingIfEmpty

		public void TestJobDocAddressIsDeletedOnSavingIfEmpty()
		{
			JobDocAddress docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			ZGuid pk = docAddress.PK;
			docAddress.E2_AddressOverride = true;
			AssertEquals("IsEmpty", false, docAddress.IsEmpty);
			Factory.Save();

			AssertEquals("IsDeleted", false, docAddress.IsDeleted);
			JobDocAddress reloadedDocAddress = Factory.Load<JobDocAddress>(pk);
			AssertNotNull(reloadedDocAddress);

			docAddress.E2_AddressOverride = false;
			AssertEquals("IsEmpty", true, docAddress.IsEmpty);
			Factory.Save();
			AssertEquals("IsDeleted", true, docAddress.IsDeleted);
			reloadedDocAddress = Factory.Load<JobDocAddress>(pk);
			AssertNull(reloadedDocAddress);
		}

		#endregion

		#region TestSetDefaultAddressFromOrg

		public void TestSetDefaultAddressFromOrg()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";

			OrgAddress mainAddress = header.MainAddress;
			mainAddress.FillWithValidTestData();

			OrgAddress deliveryAddress = header.Addresses.AddNew();
			deliveryAddress.FillWithValidTestData();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);

			OrgAddress deliveryAddress2 = header.Addresses.AddNew();
			deliveryAddress2.FillWithValidTestData();
			deliveryAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress2.OA_RL_NKRelatedPortCode = "AUBNE";

			JobDocAddress consigneeDocumentaryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			consigneeDocumentaryAddress.DefaultAddressType = AddressType.DLV;
			consigneeDocumentaryAddress.OrganisationPK = header.PK;

			consigneeDocumentaryAddress.UpdateDefaultAddress(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE"));
			consigneeDocumentaryAddress.SetDefaultAddressFromOrg();
			AssertEquals(consigneeDocumentaryAddress.E2_OA_Address, deliveryAddress2.PK);

			consigneeDocumentaryAddress.UpdateDefaultAddress(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			consigneeDocumentaryAddress.SetDefaultAddressFromOrg();
			AssertEquals(consigneeDocumentaryAddress.E2_OA_Address, deliveryAddress.PK);

			consigneeDocumentaryAddress.UpdateDefaultAddress(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX"));
			consigneeDocumentaryAddress.SetDefaultAddressFromOrg();
			AssertEquals(consigneeDocumentaryAddress.E2_OA_Address, deliveryAddress.PK);

			consigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			consigneeDocumentaryAddress.UpdateDefaultAddress(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEHAM"));
			consigneeDocumentaryAddress.SetDefaultAddressFromOrg(false, false);
			AssertEquals(consigneeDocumentaryAddress.E2_OA_Address, ZGuid.Empty);

			consigneeDocumentaryAddress.SetDefaultAddressFromOrg();
			AssertEquals(consigneeDocumentaryAddress.E2_OA_Address, deliveryAddress.PK);

			Factory.Save();
			consigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, consigneeDocumentaryAddress.OrganisationPK);

			consigneeDocumentaryAddress.OrganisationPK = header.PK;
			consigneeDocumentaryAddress.UpdateDefaultAddress(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			AssertEquals(consigneeDocumentaryAddress.E2_OA_Address, deliveryAddress.PK);
			consigneeDocumentaryAddress.UpdateDefaultAddress(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE"));
			AssertEquals(consigneeDocumentaryAddress.E2_OA_Address, deliveryAddress2.PK);
		}

		#endregion

		#region TestSetDefaultAddressFromOrg_ActiveOnly

		public void TestSetDefaultAddressFromOrg_ActiveOnly()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress mainAddress = org.MainAddress;
			mainAddress.FillWithValidTestData();
			mainAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			OrgAddress deliveryAddress = org.Addresses.AddNew();
			deliveryAddress.FillWithValidTestData();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			JobDocAddress consigneeDocumentaryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			consigneeDocumentaryAddress.DefaultAddressType = AddressType.DLV;
			consigneeDocumentaryAddress.OrganisationPK = org.PK;

			consigneeDocumentaryAddress.UpdateDefaultAddress(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			consigneeDocumentaryAddress.SetDefaultAddressFromOrg();
			AssertEquals(consigneeDocumentaryAddress.E2_OA_Address, deliveryAddress.PK);

			deliveryAddress.OA_IsActive = ZBool.False;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader assertOrg = newFactory.Load<OrgHeader>(org.PK);

			JobDocAddress assertDocAdr = newFactory.New<JobDocAddress>();
			assertDocAdr.DefaultAddressType = AddressType.DLV;
			assertDocAdr.OrganisationPK = assertOrg.PK;

			assertDocAdr.UpdateDefaultAddress(newFactory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			assertDocAdr.SetDefaultAddressFromOrg();
			AssertEquals(assertDocAdr.E2_OA_Address, mainAddress.PK);
		}

		#endregion

		#region TestUpdateDefaultAddress

		public void TestUpdateDefaultAddress_WhenMainAddressIsMissing_ErrorReporterShouldNotSendMesage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>(); //Will create MainAddress automatically with OA_RL_NKRelatedPortCode = "AUSYD"

			var deliveryAddress1 = org.Addresses.AddNew();
			deliveryAddress1.FillWithValidTestData();
			deliveryAddress1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress1.OA_RL_NKRelatedPortCode = "AUBNE";

			var deliveryAddress2 = org.Addresses.AddNew();
			deliveryAddress2.FillWithValidTestData();
			deliveryAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);

			var consigneeDocumentaryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			consigneeDocumentaryAddress.DefaultAddressType = AddressType.DLV;
			consigneeDocumentaryAddress.E2_OA_Address = deliveryAddress1.PK;

			Factory.Save();

			org.MainAddress.Delete();
			Factory.Save();

			ErrorReporter.Clear();
			consigneeDocumentaryAddress.UpdateDefaultAddress(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL"));
			AssertEquals("Should not have ActiveBusinessObjectCollection`1..during enumeration message", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		#endregion

		/// <summary>
		/// WI00005345 and Issue 00037422
		/// </summary>
		/// 

		#region TestSettingAddressToMiscAddressOrEmptyWillClearOverride

		public void TestSettingAddressToMiscAddressOrEmptyWillClearOverride()
		{
			ZGuid miscAddressPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress;
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "~ZZZ~";
			org.OH_FullName = "Real Company OK";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Nibuz Ooppa";
			contact.OC_Email = "nibuz@hednahshkar.com";
			contact.OC_Fax = "939393939";
			contact.OC_Phone = "11122";
			contact.OC_NotifyMode = "FAX";

			OrgAddress address = org.MainAddress;
			address.OA_Address1 = "First Street";
			address.OA_Address2 = "2nd Street";
			address.OA_City = "Somewhere";
			address.OA_State = "QLD";
			address.OA_PostCode = "325235";

			DocAddress.FillWithValidTestData();
			DocAddress.E2_AddressOverride = true;
			AssertEquals("E2_OA_Address", miscAddressPK, DocAddress.E2_OA_Address);
			AssertEquals("OrganisationPK", ZGuid.Empty, DocAddress.OrganisationPK);
			Factory.Save();

			DocAddress.E2_OA_Address = address.PK;
			AssertEquals("E2_AddressOverride", false, DocAddress.E2_AddressOverride);
			AssertEquals("OrganisationPK", org.PK, DocAddress.OrganisationPK);
			Factory.Save();

			DocAddress.E2_AddressOverride = true;
			AssertEquals("E2_OA_Address", miscAddressPK, DocAddress.E2_OA_Address);
			AssertEquals("OrganisationPK", ZGuid.Empty, DocAddress.OrganisationPK);
			Factory.Save();

			DocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("E2_AddressOverride", false, DocAddress.E2_AddressOverride);
			AssertEquals("OrganisationPK", ZGuid.Empty, DocAddress.OrganisationPK);
			Factory.Save();
			AssertEquals(true, DocAddress.IsDeleted);
		}

		#endregion

		#region TestTickingOverrideFiresValueChanged

		public void TestTickingOverrideFiresValueChanged()
		{
			BusinessObject parent = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			JobDocAddress docAddress = JobDocAddress.GetOrCreateNonPersistantDocAddress(parent, DocAddressType.ArrivalCFSAddress, ZGuid.Empty);
			bool changed = false;
			docAddress.OrganisationPKIncludesMiscOrgInfo.ValueChanged += delegate
			{ changed = true; };
			docAddress.E2_AddressOverride = true;
			Assert(changed);
		}

		#endregion

		#region TestDontValidateNonPersistentDocAddress

		public void TestDontValidateNonPersistentDocAddress()
		{
			BusinessObject parent = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			JobDocAddress docAddress = JobDocAddress.GetOrCreateNonPersistantDocAddress(parent, DocAddressType.ArrivalCFSAddress, ZGuid.Empty);
			using (docAddress.GetValidationSuspender())
			{
				docAddress.E2_OA_Address = ZGuid.Invalid;
			}

			docAddress.RunPreSaveValidation();
			AssertEquals("No validation run for a non-persistent JobDocAddress", false, docAddress.HasErrors);
		}

		#endregion

		#region TestGovernmentNumberCopiedFromAddress

		public void TestGovernmentNumberCopiedFromAddress()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgaddress = organisation.Addresses.AddNew();
			orgaddress.OA_Address1 = "Add1-1";

			var cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC1");
			var address = JobDocAddress.New((BusinessObject)Factory.New<Forwarding.IForwardingShipment>(), DocAddressType.SupplierPickupDeliveryAddress);
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.GetRegistrationNumberResult =
				(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = cusCode.OK_CustomsRegNo, NumberType = cusCode.OK_CodeType }; });

			address.E2_OA_Address = orgaddress.PK;
			AssertEquals("ABC1", address.E2_GovRegNum);
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, address.E2_GovRegNumType);

			address.E2_AddressOverride = true;
			AssertEquals("ABC1", address.E2_GovRegNum);
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, address.E2_GovRegNumType);

			address.E2_GovRegNum = "ABC2";
			AssertEquals("ABC2", address.E2_GovRegNum);
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, address.E2_GovRegNumType);

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.TaxFileCode;
			AssertEquals("ABC2", address.E2_GovRegNum);
			AssertEquals(OrgCusCode.CodeTypes.TaxFileCode, address.E2_GovRegNumType);

			address.E2_AddressOverride = false;
			AssertEquals("ABC1", address.E2_GovRegNum);
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, address.E2_GovRegNumType);

			address.Requirement.GetRegistrationNumberResult =
				(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = "ABC3", NumberType = OrgCusCode.CodeTypes.BrokerageRegistration }; });

			AssertEquals("Should not cache RegistrationNumber.", "ABC3", address.E2_GovRegNum);
			AssertEquals("Should not cache RegistrationNumber.", OrgCusCode.CodeTypes.BrokerageRegistration, address.E2_GovRegNumType);
		}

		#endregion

		#region TestCacheRegistrationNumberFromRequirement

		public void TestCacheRegistrationNumberFromRequirement()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgaddress = organisation.Addresses.AddNew();
			orgaddress.OA_Address1 = "Add1-1";

			var cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC1");

			var address = JobDocAddress.New((BusinessObject)Factory.New<Forwarding.IForwardingShipment>(), DocAddressType.SupplierPickupDeliveryAddress);
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.GetRegistrationNumberResult =
				(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = cusCode.OK_CustomsRegNo, NumberType = cusCode.OK_CodeType }; });

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.TaxFileCode;
			address.E2_GovRegNum = "ABC2";
			address.E2_AddressOverride = true;
			address.E2_OA_Address = orgaddress.PK;

			using (address.CacheRegistrationNumberFromRequirement())
			{
				AssertEquals("Precondition.", OrgCusCode.CodeTypes.CarrierCode, address.E2_GovRegNumType);
				AssertEquals("Precondition.", "ABC1", address.E2_GovRegNum);

				address.Requirement.GetRegistrationNumberResult =
					(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = "ABC3", NumberType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode }; });

				AssertEquals("Should have cached RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.CarrierCode, address.E2_GovRegNumType);
				AssertEquals("Should have cached RegistrationNumberFromRequirement.", "ABC1", address.E2_GovRegNum);

				address.E2_AddressOverride = true;
				address.E2_GovRegNumType = OrgCusCode.CodeTypes.TaxFileCode;
				address.E2_GovRegNum = "ABC2";
				AssertEquals("Should *not* cache properties, just the RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.TaxFileCode, address.E2_GovRegNumType);
				AssertEquals("Should *not* cache properties, just the RegistrationNumberFromRequirement.", "ABC2", address.E2_GovRegNum);
			}

			address.E2_AddressOverride = false;
			AssertEquals("Should no longer cache RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, address.E2_GovRegNumType);
			AssertEquals("Should no longer cache RegistrationNumberFromRequirement.", "ABC3", address.E2_GovRegNum);

			address.Requirement.GetRegistrationNumberResult =
				(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = "ABC4", NumberType = OrgCusCode.CodeTypes.CorporationCode }; });

			// Test cached result only lives within the scope of the using
			using (address.CacheRegistrationNumberFromRequirement())
			{
				AssertEquals("Should return the new RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.CorporationCode, address.E2_GovRegNumType);
				AssertEquals("Should return the new RegistrationNumberFromRequirement.", "ABC4", address.E2_GovRegNum);

				address.Requirement.GetRegistrationNumberResult =
					(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = "ABC3", NumberType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode }; });

				AssertEquals("Should have cached RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.CorporationCode, address.E2_GovRegNumType);
				AssertEquals("Should have cached RegistrationNumberFromRequirement.", "ABC4", address.E2_GovRegNum);
			}
		}

		#endregion

		#region TestCacheRegistrationNumberFromRequirement_NestedUse

		public void TestCacheRegistrationNumberFromRequirement_NestedUse()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgaddress = organisation.Addresses.AddNew();
			orgaddress.OA_Address1 = "Add1-1";

			var cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC1");

			var address = JobDocAddress.New((BusinessObject)Factory.New<Forwarding.IForwardingShipment>(), DocAddressType.SupplierPickupDeliveryAddress);
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.GetRegistrationNumberResult =
				(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = cusCode.OK_CustomsRegNo, NumberType = cusCode.OK_CodeType }; });

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.TaxFileCode;
			address.E2_GovRegNum = "ABC2";
			address.E2_AddressOverride = true;
			address.E2_OA_Address = orgaddress.PK;

			using (address.CacheRegistrationNumberFromRequirement())
			{
				using (address.CacheRegistrationNumberFromRequirement())
				{
					AssertEquals("Precondition.", OrgCusCode.CodeTypes.CarrierCode, address.E2_GovRegNumType);
					AssertEquals("Precondition.", "ABC1", address.E2_GovRegNum);
				}

				address.Requirement.GetRegistrationNumberResult =
					(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = "ABC3", NumberType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode }; });

				AssertEquals("Should have cached RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.CarrierCode, address.E2_GovRegNumType);
				AssertEquals("Should have cached RegistrationNumberFromRequirement.", "ABC1", address.E2_GovRegNum);
			}

			AssertEquals("Should no longer cache RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, address.E2_GovRegNumType);
			AssertEquals("Should no longer cache RegistrationNumberFromRequirement.", "ABC3", address.E2_GovRegNum);
		}

		#endregion

		#region TestCheckE2_AddressOverride

		public void TestCheckE2_AddressOverride_NO_E2_City()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_ParentID = dummy.PK;
			address.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			address.E2_AddressOverride = false;
			address.RunPreSaveValidation();
			Factory.Save();
			Assert(((ILightValidationInternals)address).IsValid);
			address.E2_AddressOverride = true;
			address.E2_Address1 = "TestAddress";
			address.E2_City = "";
			address.E2_CompanyName = "TestCompanyName";
			address.RunPreSaveValidation();
			Factory.Save();
			AssertNoError(address.E2_CityInfo, "Please enter a Consignor Documentary Address: City.");

			Env.Registry.EnableAddressValidationWebService = false;
			address.E2_City = "Test";
			AssertNoError(address.E2_CityInfo, "Please enter a Consignor Documentary Address: City.");
			address.E2_City = "";
			AssertHasError(address.E2_CityInfo, "Please enter a Consignor Documentary Address: City.");
		}

		public void TestCheckE2_AddressOverride_NO_E2_CompanyName()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_ParentID = dummy.PK;
			address.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			address.E2_AddressOverride = false;
			address.RunPreSaveValidation();
			Factory.Save();
			Assert(((ILightValidationInternals)address).IsValid);
			address.E2_AddressOverride = true;
			address.E2_Address1 = "TestAddress";
			address.E2_City = "TestCity";
			address.E2_CompanyName = "";
			address.RunPreSaveValidation();
			Factory.Save();
			AssertHasError(address.E2_CompanyNameInfo, "Please enter a Company Name, or remove the override for this Address.");
		}

		#endregion

		#region TestOnConcurrencyExceptionAfterMergeCoreChangeTheValidationStatus

		public void TestOnConcurrencyExceptionAfterMergeCoreChangeTheValidationStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "ABC AT D";
			org.MainAddress.Address2 = "ABC AT D";
			org.MainAddress.ValidationStatus = AddressValidationStatus.Verified;
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = org.MainAddress.PK;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			// Mock BAV (Background Address Validation Service) change the validation status
			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var jobDocAddressInNewFactory = newFactory.Load<JobDocAddress>(jobDocAddress.PK);
			jobDocAddressInNewFactory.E2_ValidationStatus = AddressValidationStatus.Invalid;
			newFactory.Save();

			jobDocAddress.E2_AddressOverride = false;

			try
			{
				Factory.Save();
				Assert("Should not be here", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertEquals(false, jobDocAddress.E2_AddressOverride);
			AssertEquals("Validation status should be the same as OrgAddress", AddressValidationStatus.Verified, jobDocAddress.E2_ValidationStatus);
			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());
		}

		#endregion

		#region TestHasChanges

		public void TestGetOrCreateNonPersistentDocAddress_HasChanges()
		{
			var parent = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var docAddress = JobDocAddress.GetOrCreateNonPersistantDocAddress(parent, DocAddressType.ArrivalCFSAddress, ZGuid.NewZGuid());
			docAddress.MakePersistentIfNotEmpty(); // so it uses base logic for HasChanges
			Assert(!docAddress.HasChanges);
		}

		#endregion

		#region TestRowDeletedErrorSuppressedWhenGettingHumanReadableName

		public void TestRowDeletedErrorSuppressedWhenGettingHumanReadableName()
		{
			var parent = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var docAddress = JobDocAddress.GetOrCreateNonPersistantDocAddress(parent, DocAddressType.ArrivalCFSAddress, ZGuid.NewZGuid());
			docAddress.Delete();
			Factory.Save();
			AssertNoExceptionThrown(() => { string test = docAddress.E2_AddressTypeInfo.HumanReadableName; });
		}

		#endregion

		public void TestDefaultResidentialCommercialAddressType()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			AssertEquals("ResidentialCommercialAddressType should be Commercial on default", ResidentialCommercialAddressTypeList.Codes.Commercial, jobDocAddress.ResidentialCommercialAddressType);
		}

		public void TestAddressTypeResidential()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.ResidentialCommercialAddressType = ResidentialCommercialAddressTypeList.Codes.Residential;
			AssertEquals("When Address Type code is Residential isResidential must be true.", true, jobDocAddress.E2_IsResidential);
		}

		public void TestAddressTypeResidentialAfterSave()
		{
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "42 FOOBAR STREET";
			docAddress.E2_Address2 = "FUNPLACE";
			docAddress.E2_Postcode = "0000";
			docAddress.E2_City = "WHITERUN";
			docAddress.E2_State = "TAMRIEL";
			docAddress.E2_RN_NKCountryCode = "ID";
			docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			docAddress.ResidentialCommercialAddressType = ResidentialCommercialAddressTypeList.Codes.Residential;
			Factory.Save();
			AssertEquals("After saving, Residential should be true.", true, docAddress.E2_IsResidential);
			var reloadedDocAddress = new BusinessObjectFactory().Load<JobDocAddress>(docAddress.PK);
			AssertEquals("After loading from DB, Residential should be true.", true, reloadedDocAddress.E2_IsResidential);
		}

		public void TestAddressTypeCommercial()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.ResidentialCommercialAddressType = ResidentialCommercialAddressTypeList.Codes.Commercial;
			AssertEquals("When Address Type code is Commercial isResidential must be false.", false, jobDocAddress.E2_IsResidential);
		}

		#region IDocAddress Members

		public void TestIDocAddressMembers()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "INBOM";
			OrgAddress orgaddress = organisation.Addresses.AddNew();

			IDocAddress iAddress = DocAddress;
			AssertNotNull("Precondition - IAddress.Address is empty.", iAddress);
			AssertEquals("IDocAddress.E2_AddressOverride.", false, iAddress.E2_AddressOverride);
			AssertEquals("IDocAddress.E2_PortCode", "", iAddress.E2_PortCode);

			DocAddress.E2_OA_Address = orgaddress.PK;
			AssertNotNull("IDocAddress.E2_PortCode", iAddress.E2_PortCode);

			DocAddress.E2_AddressOverride = true;
			AssertEquals("IDocAddress.E2_AddressOverride.", true, iAddress.E2_AddressOverride);

			DocAddress.E2_CompanyName = "Nintendo";
			AssertEquals("IDocAddress.E2_CompanyName", "Nintendo", iAddress.E2_CompanyName);

			DocAddress.E2_AdditionalAddressInformation = "Unit 123";
			AssertEquals("IDocAddress.E2_AdditionalAddressInformation", "Unit 123", iAddress.E2_AdditionalAddressInformation);

			DocAddress.E2_Address1 = "1 Epping Road";
			AssertEquals("IDocAddress.E2_Address1", "1 Epping Road", iAddress.E2_Address1);

			DocAddress.E2_Address2 = "2 Epping Road";
			AssertEquals("IDocAddress.E2_Address2", "2 Epping Road", iAddress.E2_Address2);

			DocAddress.E2_City = "Tokyo";
			AssertEquals("IDocAddress.E2_City", "Tokyo", iAddress.E2_City);

			DocAddress.E2_State = "NSW";
			AssertEquals("IDocAddress.E2_State", "NSW", iAddress.E2_State);

			DocAddress.E2_GovRegNum = "GST123232";
			AssertEquals("IDocAddress.E2_GovRegNum", "GST123232", iAddress.E2_GovRegNum);

			DocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.TaxFileCode;
			AssertEquals("IDocAddress.E2_GovRegNumType", OrgCusCode.CodeTypes.TaxFileCode, iAddress.E2_GovRegNumType);

			DocAddress.E2_OA_Address = ZGuid.NewZGuid();
			AssertEquals("IDocAddress.E2_OA_Address", DocAddress.E2_OA_Address, iAddress.E2_OA_Address);

			DocAddress.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;
			AssertEquals("IDocAddress.E2_AddressType", AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, iAddress.E2_AddressType);

			AssertEquals("IDocAddress.E2_PortCode", "", iAddress.E2_PortCode);

			DocAddress.E2_RN_NKCountryCode = "US";
			AssertEquals("IDocAddress.CountryCode", "US", iAddress.CountryCode);

			AssertEquals("IDocAddress.E2_RN_NKCountryCode", "US", iAddress.E2_RN_NKCountryCode);
			DocAddress.E2_AddressOverride = false;
			orgaddress.OA_RN_NKCountryCode = ZString.Empty;
			DocAddress.E2_OA_Address = orgaddress.PK;
			AssertEquals("IDocAddress.E2_RN_NKCountryCode", ZString.Empty, iAddress.E2_RN_NKCountryCode);
			orgaddress.OA_RN_NKCountryCode = "CA";
			AssertEquals("IDocAddress.E2_RN_NKCountryCode", "CA", iAddress.E2_RN_NKCountryCode);
			AssertEquals("IDocAddress.CountryDescription", "Canada", iAddress.CountryDescription);
		}

		public void TestParentDescriptionWhenParentIsNull()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			Factory.Save();

			AssertNull(jobDocAddress.Parent);

			var docAddress = (IDocAddress)jobDocAddress;

			AssertNotNull(docAddress.ParentDescription);
			AssertEquals("When Parent of JobDocAddress is null, ParentDescription is an empty string", ZString.Empty, docAddress.ParentDescription);
		}

		public void TestParentDescription()
		{
			var parent = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_ParentID = parent.PK;
			jobDocAddress.E2_ParentTableCode = "JS";
			Factory.Save();

			AssertEquals("ParentDescription of jobDocAddress is correct", parent.HumanReadableName, ((IDocAddress)jobDocAddress).ParentDescription);
		}

		#endregion

		#region Denied Party Screening

		public void TestInvalidateByLocalDataChanges_WhenAddressOverrideIsTrue_ShouldInvalidateScreeningStatus()
		{
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			Factory.Save();

			CombineAssertions("Should invalidate screening", () =>
			{
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_CompanyName = "Tesla"; });
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_Address1 = "York"; });
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_Address2 = "Corner Fred Street"; });
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_City = "Syndey"; });
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_State = "NSW"; });
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_Postcode = "2000"; });
			});

			void AssertHasInvalidatedScreeningStatuses(Action dataChange)
			{
				docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Clear, docAddress.E2_ScreeningStatus);

				dataChange.Invoke();
				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Unknown, docAddress.E2_ScreeningStatus);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges, docAddress.ScreeningLogCollection[docAddress.ScreeningLogCollection.Count - 1].PJ_Status);
			}
		}

		public void TestInvalidateByLocalDataChanges_WhenAddressOverrideIsTrue_ShouldNotInvalidateScreeningStatus()
		{
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			Factory.Save();

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			CombineAssertions("Should invalidate screening", () =>
			{
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_Contact = "Fred Johson"; });
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_Phone = "03 6256 9094"; });
				AssertHasInvalidatedScreeningStatuses(() => { docAddress.E2_Email = "Fred.Johson@mail.com"; });
			});

			void AssertHasInvalidatedScreeningStatuses(Action dataChange)
			{
				dataChange.Invoke();
				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Clear, docAddress.E2_ScreeningStatus);
				AssertEquals(0, docAddress.ScreeningLogCollection.Count);
			}
		}

		public void TestScreeningStatusesFromOrgHeaderIfNotOverriden()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.E2_OA_Address = org.MainAddress.PK;

			IStmEntityScreeningLogCollection orgStatuses = org.ScreeningLogCollection;
			address.E2_AddressOverride = false;
			IStmEntityScreeningLogCollection addressStatusesWhenNotOverride = address.ScreeningLogCollection;
			address.E2_AddressOverride = true;
			IStmEntityScreeningLogCollection addressStatusesWhenOverride = address.ScreeningLogCollection;

			AssertEquals("When address is not overriden, screening statuses getting from dbo.orgheader", orgStatuses.GetHashCode(), addressStatusesWhenNotOverride.GetHashCode());
			AssertNotEquals("Different collections", addressStatusesWhenOverride.GetHashCode(), addressStatusesWhenNotOverride.GetHashCode());
		}

		public void TestGetWorstScreeningStatus()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			JobDocAddress address = Factory.New<JobDocAddress>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			address.E2_OA_Address = org.MainAddress.PK;
			address.E2_AddressOverride = true;
			address.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			AssertEquals("When overriden, getWorstSceeningStatus should get status from overriden", ScreeningStatusesList.Codes.Clear, (address as IScreeningPartyProvider).GetWorstScreeningStatus());
			address.E2_AddressOverride = false;
			AssertEquals("When not overriden, getWorstSceeningStatus should get status from dbo.OrgHeader", ScreeningStatusesList.Codes.Unknown, (address as IScreeningPartyProvider).GetWorstScreeningStatus());
		}

		public void TestDeletedJobDocAddress()
		{
			var address = Factory.New<JobDocAddress>();
			address.Delete();
			AssertNoExceptionThrown("No exception should be thrown here", () => ((IScreeningPartyProvider)address).GetWorstScreeningStatus());
		}

		public void TestDelete()
		{
			var docAddress1 = Factory.New<JobDocAddress>();
			docAddress1.E2_Phone_IsManuallyVerified = true;
			docAddress1.E2_Fax_IsManuallyVerified = true;

			var docAddress2 = Factory.New<JobDocAddress>();
			docAddress2.E2_Mobile_IsManuallyVerified = true;

			Factory.Save();

			var acks1 = new GenCustomAddOnRuleAckCollection(docAddress1);
			var acks2 = new GenCustomAddOnRuleAckCollection(docAddress2);
			AssertEquals("Precondition", 2, acks1.Count);
			AssertEquals("Precondition", 1, acks2.Count);

			docAddress1.Delete();
			AssertEquals(0, acks1.Count);
			AssertEquals(1, acks2.Count);
		}

		public void TestDelete_RemovesRelatedJobDocumentExclusionRecords()
		{
			var docAddressA = Factory.NewWithValidTestData<JobDocAddress>();
			var docAddressB = Factory.NewWithValidTestData<JobDocAddress>();

			var exclusionA1 = Factory.New<JobDocumentExclusion>();
			exclusionA1.JDE_E2_Address = docAddressA.PK;
			exclusionA1.JDE_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exclusionA1.JDE_ParentID = ZGuid.NewZGuid();

			var exclusionA2 = Factory.New<JobDocumentExclusion>();
			exclusionA2.JDE_E2_Address = docAddressA.PK;
			exclusionA2.JDE_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exclusionA2.JDE_ParentID = ZGuid.NewZGuid();

			var exclusionB = Factory.New<JobDocumentExclusion>();
			exclusionB.JDE_E2_Address = docAddressB.PK;
			exclusionB.JDE_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exclusionB.JDE_ParentID = ZGuid.NewZGuid();

			AssertEquals("Precondition", 2, GetRelatedJobDocumentExclusionCount(docAddressA.PK));
			AssertEquals("Precondition", 1, GetRelatedJobDocumentExclusionCount(docAddressB.PK));

			docAddressA.Delete();

			AssertEquals("Related JobDocumentExclusion records should be deleted alongside their JobDocAddress", 0, GetRelatedJobDocumentExclusionCount(docAddressA.PK));
			AssertEquals("Unrelated JobDocumentExclusion records should not be deleted", 1, GetRelatedJobDocumentExclusionCount(docAddressB.PK));

			int GetRelatedJobDocumentExclusionCount(ZGuid docAddressPK)
			{
				return Factory.Load<JobDocumentExclusion>(new ZQuery(JobDocumentExclusionSchema.JDE_E2_Address, docAddressPK)).Length;
			}
		}

		public void TestInvalidateByLocalDataChanges_WhenAddressOverrideIsFalse_ShouldNotInvalidateScreeningStatus()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = header.MainAddress.PK;
			docAddress.E2_AddressOverride = false;
			docAddress.E2_CompanyName = "Tesla Automotive";
			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals("Should not invalidate Org screening status", ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);
			AssertEquals("Job Doc Address screening status from Org", ScreeningStatusesList.Codes.Clear, docAddress.E2_ScreeningStatus);
			AssertEquals(header.ScreeningLogCollection, docAddress.ScreeningLogCollection);
			AssertEquals(0, docAddress.ScreeningLogCollection.Count);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "Tesla Automotive New";
			Factory.Save();

			AssertEquals("Should not invalidate Org screening status", ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);
			AssertEquals("Job Doc Address screening status from itself", ScreeningStatusesList.Codes.Unknown, docAddress.E2_ScreeningStatus);
			AssertNotEquals(header.ScreeningLogCollection, docAddress.ScreeningLogCollection);
			AssertEquals(1, docAddress.ScreeningLogCollection.Count);
		}

		public void TestInvalidateByLocalDataChanges_WhenAddressOverrideIsFalseAndOrgIsNull_ShouldInvalidateScreeningStatus()
		{
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = ZGuid.NewZGuid();

			CombineAssertions("Precondition:", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, docAddress.E2_ScreeningStatus);
				AssertEquals(false, docAddress.E2_AddressOverride);
				AssertNull(docAddress.Organisation);
			});

			try
			{
				Factory.Save();
				Assert("Should not be here", false);
			}
			catch (ZSaveException)
			{
			}

			AssertEquals("Should invalidate job DOC address screening status", ScreeningStatusesList.Codes.Unknown, docAddress.E2_ScreeningStatus);
		}

		public void TestInvalidateByLocalDataChanges_WhenAddressOverrideIsTrueAndStatusIsCLP_ShouldNoInvalidateScreeningStatus()
		{
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();

			AssertEquals("Precondition", ScreeningStatusesList.Codes.PermanentClear, docAddress.E2_ScreeningStatus);

			docAddress.Address1 = "New Address 1 Data";
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, docAddress.E2_ScreeningStatus);
			AssertEquals(0, docAddress.ScreeningLogCollection.Count);
		}

		public void TestDeleteInDatabaseAddressWillSetParentShouldUpdateScreeningStatusToTrue()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var jobDocAddressCollection = new JobDocAddressDependentCollection(shipment as IDocAddresses);

			var jobDocAddress = jobDocAddressCollection.AddNew();
			jobDocAddress.E2_Address1 = "Dummy";
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_ParentID = shipment.PK;
			jobDocAddress.E2_ParentTableCode = "JS";
			jobDocAddress.E2_AddressType = "CRD";
			jobDocAddress.ValidationStatus = AddressValidationStatus.Invalid;
			Factory.Save();

			AssertEquals(false, (shipment as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus);

			jobDocAddress.Delete();
			AssertEquals(true, (shipment as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus);

			Factory.Save();
			AssertEquals(false, (shipment as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus);

			var jobDocAddress2 = jobDocAddressCollection.AddNew();
			jobDocAddress2.Delete();
			AssertEquals(false, (shipment as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus);
		}

		#endregion

		#region IZAddress

		public void TestInactiveSelectedAddressIsInList()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Code = "Test Address";
			orgAddress.OA_IsActive = false;

			var docAddress = Factory.New<JobDocAddress>();
			var addressList = ((IZAddress)docAddress).AddressList;
			var addressItem = addressList.Cast<ZAddressItem>().FirstOrDefault(item => item.PK == orgAddress.PK);
			AssertNull("Not contains inactive address", addressItem);

			docAddress.E2_OA_Address = orgAddress.PK;
			addressList = ((IZAddress)docAddress).AddressList;
			addressItem = addressList.Cast<ZAddressItem>().FirstOrDefault(item => item.PK == orgAddress.PK);
			AssertNotNull("Contains inactive selected address", addressItem);
			AssertEquals("Inactive: Test Address", addressItem.UsageComment);
			AssertEquals("Inactive", addressItem.Capabilities[0].Capability);
			AssertEquals(false, addressItem.Capabilities[0].IsDefault);
		}

		[ExpectNoExceptions]
		public void TestInactiveSelectedAddressWhenAddressOverride()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.Address.OA_IsActive = false;

			AssertEquals("AddressList should has only one address", 1, ((IZAddress)docAddress).AddressList.Count);
		}

		#endregion

		#region Address Validation

		public void TestValidationStatus_WhenSettingToNonNrqFromNrq_ShouldSetAddressOverrideToTrue()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = false;
			address.E2_ValidationStatus = AddressValidationStatus.NotRequired;

			// Act.

			address.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;

			// Assert.

			Assert("Expecting E2_AddressOverride = TRUE", address.E2_AddressOverride);
			AssertEquals(AddressValidationStatus.ToBeVerified, address.E2_ValidationStatus);
		}

		public void TestValidationStatus_WhenSettingToNrqFromNonNrq_ShouldSetAddressOverrideToFalse()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;

			// Act.

			address.E2_ValidationStatus = AddressValidationStatus.NotRequired;

			// Assert.

			Assert("Expecting E2_AddressOverride = FALSE", !address.E2_AddressOverride);
			AssertEquals(AddressValidationStatus.NotRequired, address.E2_ValidationStatus);
		}

		public void TestValidationStatus_WhenSettingToNonNrqFromNonNrq_ShouldKeepAddressOverride()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;

			// Act.

			address.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			// Assert.

			Assert("Expecting E2_AddressOverride = TRUE", address.E2_AddressOverride);
			AssertEquals(AddressValidationStatus.ManuallyVerified, address.E2_ValidationStatus);
		}

		public void TestValidationStatus_WhenChangingAddressFieldWhileValueIsCna_ShouldKeepItAsCna()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			// Act & Assert.

			address.E2_Address1 = "[_MOCK_ADDRESS_1_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.E2_ValidationStatus);

			address.E2_Address2 = "[_MOCK_ADDRESS_2_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.E2_ValidationStatus);

			address.E2_City = "[_MOCK_CITY_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.E2_ValidationStatus);

			address.E2_Postcode = "0000";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.E2_ValidationStatus);

			address.E2_State = "[_MOCK_STATE_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.E2_ValidationStatus);

			address.E2_RN_NKCountryCode = "XY";
			AssertEquals(AddressValidationStatus.ToBeVerified, address.E2_ValidationStatus);
		}

		public void TestChangingAddressResetsValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_RN_NKCountryCode = "AU";
			address.ValidationStatus = AddressValidationStatus.Verified;

			AssertValidationStatusIsReset(address, () => address.Address1 += "A");
			AssertValidationStatusIsReset(address, () => address.Address2 += "A");
			AssertValidationStatusIsReset(address, () => address.City += "A");
			AssertValidationStatusIsReset(address, () => address.Postcode += "A");
			AssertValidationStatusIsReset(address, () => address.State += "A");
		}

		void AssertValidationStatusIsReset(ISupportWebAddressValidation address, Action action)
		{
			address.ValidationStatus = AddressValidationStatus.Verified;
			action.Invoke();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestRaiseAddressValidationStatusChanged()
		{
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.Address2 = "";

			address.AddressValidationStatusChanged += address_AddressValidationStatusChanged;
			address.E2_ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("It happened", address.Address2);
		}

		void address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			((JobDocAddress)sender).Address2 = "It happened";
		}

		public void TestValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_RN_NKCountryCode = country.Code;
			address.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestState_CountryHasRefData_StateMatchesRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_RN_NKCountryCode = country.Code;
			address.E2_State = "NSW";
			AssertEquals("New South Wales", address.State);

			address.State = "Victoria";
			AssertEquals("VIC", address.E2_State);
		}

		public void TestState_CountryHasRefData_StateDoesNotMatchRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_RN_NKCountryCode = country.Code;
			address.E2_State = "XYZ";
			AssertEquals("XYZ", address.State);
		}

		public void TestState_CountryHasNoRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "NL"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_RN_NKCountryCode = country.Code;
			address.E2_State = "XYZ";
			AssertEquals("XYZ", address.State);
		}

		public void TestUnrestrictedAdditionalAddressInformation_WhenJobDocAddressOverriden_ShouldReturnUnrestrictedAdditionalAddressInformation()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_AdditionalAddressInformation = "AdditionalInfo";
			jobDocAddress.OrganisationPK = org.PK;

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.UnrestrictedAdditionalAddressInformation = "UnrestrictedAdditionalInfo";

			AssertEquals("UnrestrictedAdditionalAddressInformation retrieves its own UnrestrictedAdditionalAddressInformation when JobDocAddress is overriden", "UnrestrictedAdditionalInfo", jobDocAddress.UnrestrictedAdditionalAddressInformation);
		}

		public void TestUnrestrictedAdditionalAddressInformation_WhenJobDocAddressNotOverriden()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "AdditionalInfo";
			jobDocAddress.OrganisationPK = org.PK;

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				jobDocAddress.E2_AddressOverride = false;
				jobDocAddress.UnrestrictedAdditionalAddressInformation = "UnrestrictedAdditionalInfo";
				AssertEquals("UnrestrictedAdditionalAddressInformation retrieves E2_AdditionalAddressInformation info from dbo.JobDocAddress when is not overriden and E2_AdditionalAddressInformation is not empty", "UnrestrictedAdditionalInfo", jobDocAddress.UnrestrictedAdditionalAddressInformation);

				jobDocAddress.UnrestrictedAdditionalAddressInformation = string.Empty;
				AssertEquals("UnrestrictedAdditionalAddressInformation retrieves additional addresss info from dbo.orgAddress when JobDocAddress is not overriden and E2_AdditionalAddressInformation is not empty", "AdditionalInfo", jobDocAddress.UnrestrictedAdditionalAddressInformation);
			}
		}

		public void TestUnrestrictedAdditionalAddressInformation_WhenJobDocAddressOverridenAndUnrestrictedIsEmpty_ShouldReturnOrgAddressAdditionalInfo()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "AdditionalInfo";
			jobDocAddress.OrganisationPK = org.PK;

			jobDocAddress.E2_AddressOverride = true;

			AssertEquals("UnrestrictedAdditionalAddressInformation retrieves additional addresss info from dbo.orgAddress if not empty when UnrestrictedAdditionalAddressInformation is empty",
				"AdditionalInfo", jobDocAddress.UnrestrictedAdditionalAddressInformation);
		}

		public void TestUnrestrictedAdditionalAddressInformation_ShouldBeSetCorrectlyAndUpdateE2_AdditionalAddressInformation()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AdditionalAddressInformation = "E2_AdditionalAddressInformation";
			jobDocAddress.HasChanges = false;

			jobDocAddress.UnrestrictedAdditionalAddressInformation = "E2_AdditionalAddressInformation";

			AssertEquals("HasChanges is false when UnrestrictedAdditionalAddressInformation is same as E2_AdditionalAddressInformation", false, jobDocAddress.HasChanges);

			jobDocAddress.UnrestrictedAdditionalAddressInformation = " UnrestrictedAdditionalInfo ";

			AssertEquals("UnrestrictedAdditionalAddressInformation has trimmed spaces from the end", " UnrestrictedAdditionalInfo", jobDocAddress.UnrestrictedAdditionalAddressInformation);
			AssertEquals("E2_AdditionalAddressInformation is updated from UnrestrictedAdditionalAddressInformation", jobDocAddress.UnrestrictedAdditionalAddressInformation, jobDocAddress.E2_AdditionalAddressInformation);
			AssertEquals("HasChanges is true when UnrestrictedAdditionalAddressInformation is different to E2_AdditionalAddressInformation", true, jobDocAddress.HasChanges);
		}

		public void TestE2_AdditionalAddressInformation_UpdatesUnrestrictedAdditionalAddressInformation()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AdditionalAddressInformation = "Entrance from backdoor";
			AssertEquals("E2_AdditionalAddressInformation updates UnrestrictedAdditionalAddressInformation", "Entrance from backdoor", jobDocAddress.UnrestrictedAdditionalAddressInformation);
		}

		[ExpectNoExceptions]
		public void TestNeedValidation()
		{
			var australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			australia.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var china = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			china.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_RN_NKCountryCode = "";
			address.AdditionalAddressInformation = "AdditionalInfo";
			address.Address1 = "A1";
			address.Address2 = "A2";
			address.Postcode = "1234";
			address.City = "Syd";
			address.State = "NSW";
			Assert(!address.NeedValidation);
			address.E2_RN_NKCountryCode = "AU";
			Assert(address.NeedValidation);
			Factory.Save();
			Assert(address.IsInDatabase);
			Assert(!address.NeedValidation);
			address.Address1 += "A";
			Assert(address.NeedValidation);
			address.Address2 = "";
			Assert(address.NeedValidation);
			address.Address1 = "";
			Assert(!address.NeedValidation);
			address.E2_RN_NKCountryCode = "";
			Assert(!address.NeedValidation);
		}

		public void TestResetAddressMap()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_RN_NKCountryCode = "AU";

			AssertAddressMap(address, address.E2_AdditionalAddressInformationInfo);
			AssertAddressMap(address, address.E2_Address1Info);
			AssertAddressMap(address, address.E2_Address2Info);
			AssertAddressMap(address, address.E2_CityInfo);
			AssertAddressMap(address, address.E2_PostcodeInfo);
			AssertAddressMap(address, address.E2_StateInfo);
			AssertAddressMap(address, address.E2_RN_NKCountryCodeInfo);

			address.E2_RN_NKCountryCode = "AU";
			var usPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")).PK.ToGuid();
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForOverrideAddress: true));

			AssertAddressMap(address, address.E2_AdditionalAddressInformationInfo);
			AssertAddressMap(address, address.E2_Address1Info);
			AssertAddressMap(address, address.E2_Address2Info);
			AssertAddressMap(address, address.E2_CityInfo);
			AssertAddressMap(address, address.E2_PostcodeInfo);
			AssertAddressMap(address, address.E2_StateInfo);
			AssertAddressMap(address, address.E2_RN_NKCountryCodeInfo);
		}

		public void TestResetParentScreeningStatus()
		{
			var testAddress = GetNewDocAddressWithParent();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			testAddress.E2_OA_Address = header.Addresses[0].PK;

			var parentProvider = testAddress.Parent as IShouldUpdateScreeningStatus;
			var screeningStatusProvider = testAddress.Parent as IScreeningStatusProvider;
			parentProvider.ShouldUpdateScreeningStatus = false;
			screeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			testAddress.HasChanges = true;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, false);

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			testAddress.HasChanges = true;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, false);

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			testAddress.HasChanges = true;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, false);

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			testAddress.HasChanges = true;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, true);

			parentProvider.ShouldUpdateScreeningStatus = false;
			screeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			testAddress.E2_AddressOverride = true;
			testAddress.HasChanges = true;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, true);

			parentProvider.ShouldUpdateScreeningStatus = false;
			screeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			testAddress.HasChanges = true;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, true);
		}

		public void TestResetParentScreeningStatus_AddressPKNotChangedOrChangedToInvalid()
		{
			var testAddress = GetNewDocAddressWithParent();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = header.Addresses.AddNew();
			address2.Address1 = "New Dummy Address 2";
			testAddress.E2_OA_Address = header.Addresses[0].PK;

			var parentProvider = testAddress.Parent as IShouldUpdateScreeningStatus;
			var screeningStatusProvider = testAddress.Parent as IScreeningStatusProvider;
			parentProvider.ShouldUpdateScreeningStatus = false;
			screeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			testAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, false);

			testAddress.E2_OA_Address = ZGuid.Invalid;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, false);

			testAddress.E2_OA_Address = header.Addresses[0].PK;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, true);

			Factory.Save();

			screeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			testAddress.HasChanges = true;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, false);

			testAddress.E2_OA_Address = address2.PK;
			AssertEquals(parentProvider.ShouldUpdateScreeningStatus, true);
		}

		public void TestResetParentScreeningStatus_OrganizationIsNull()
		{
			var testAddress = GetNewDocAddressWithParent();
			var parentProvider = testAddress.Parent as IShouldUpdateScreeningStatus;
			var screeningStatusProvider = testAddress.Parent as IScreeningStatusProvider;
			parentProvider.ShouldUpdateScreeningStatus = false;
			screeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			testAddress.E2_OA_Address = ZGuid.NewZGuid();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals(false, !testAddress.E2_OA_Address.IsValid || testAddress.IsInDatabase && !testAddress.E2_OA_AddressInfo.HasChanges);
				AssertNull(testAddress.Organisation);
				AssertEquals(false, testAddress.E2_AddressOverride);
				AssertEquals(false, parentProvider.ShouldUpdateScreeningStatus);
			});

			testAddress.HasChanges = true;
			AssertEquals("Not set to true when organization is null and is not override address", false, parentProvider.ShouldUpdateScreeningStatus);

			testAddress.E2_AddressOverride = true;
			AssertEquals(true, parentProvider.ShouldUpdateScreeningStatus);
		}

		void AssertAddressMap(JobDocAddress address, ZPropertyInfo propertyInfo)
		{
			address.AddressMap = "ABCDE";
			propertyInfo.Value = (ZString)(propertyInfo.Name == nameof(JobDocAddress.E2_RN_NKCountryCode) ? "US" : (ZString)propertyInfo.Value + "1");
			Assert(string.IsNullOrEmpty(address.AddressMap));
		}

		public void TestAddressValidationRuleForCountry()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.StreetNumber;

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_RN_NKCountryCode = country.Code;
			AssertEquals("NUM", address.AddressValidationRuleForCountry);
		}

		public void TestValidationSection()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			AssertEquals(AddressValidationSection.OverrideAddress, jobDocAddress.ValidationSection);

			jobDocAddress.E2_AddressOverride = false;
			AssertEquals(AddressValidationSection.OrganizationAddress, jobDocAddress.ValidationSection);

			jobDocAddress.IsInAdminPanel = true;
			AssertEquals(AddressValidationSection.AdminPanel, jobDocAddress.ValidationSection);
		}

		#endregion

		#region E2_ValidationStatus

		public void TestValidationStatus_WhenSetToManuallyVerifiedFromOtherValue_ShouldStayAsIsUntilJobDocAddressIsReloaded()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = true;

			docAddress.E2_Address1 = "42 FOOBAR STREET";
			docAddress.E2_Address2 = "FUNPLACE";
			docAddress.E2_Postcode = "0000";
			docAddress.E2_City = "WHITERUN";
			docAddress.E2_State = "TAMRIEL";
			docAddress.E2_RN_NKCountryCode = "ID";

			// Act.

			docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			docAddress.E2_Address1 = "72 O'RIORDAN STREET";
			docAddress.E2_Address2 = "WISETECH GLOBAL";
			docAddress.E2_Postcode = "2015";
			docAddress.E2_City = "ALEXANDRIA";
			docAddress.E2_State = "NSW";
			docAddress.E2_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ManuallyVerified, docAddress.E2_ValidationStatus);
		}

		public void TestValidationStatus_WhenLoadedAsManuallyVerifiedFromDatabase_ShouldResetValueAfterChangingAddressField()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = true;

			docAddress.E2_Address1 = "42 FOOBAR STREET";
			docAddress.E2_Address2 = "FUNPLACE";
			docAddress.E2_Postcode = "0000";
			docAddress.E2_City = "WHITERUN";
			docAddress.E2_State = "TAMRIEL";
			docAddress.E2_RN_NKCountryCode = "ID";

			docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();

			var reloadedDocAddress = new BusinessObjectFactory().Load<JobDocAddress>(docAddress.PK);

			// Act.

			reloadedDocAddress.E2_Address1 = "72 O'RIORDAN STREET";
			reloadedDocAddress.E2_Address2 = "WISETECH GLOBAL";
			reloadedDocAddress.E2_Postcode = "2015";
			reloadedDocAddress.E2_City = "ALEXANDRIA";
			reloadedDocAddress.E2_State = "NSW";
			reloadedDocAddress.E2_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ToBeVerified, reloadedDocAddress.E2_ValidationStatus);
		}

		public void TestJobDocAddressValidationStatus()
		{
			TestCase(false, AddressValidationStatus.Verified, AddressValidationStatus.NotRequired, AddressValidationStatus.Verified);
			TestCase(false, AddressValidationStatus.Invalid, AddressValidationStatus.NotRequired, AddressValidationStatus.Invalid);
			TestCase(true, AddressValidationStatus.Verified, AddressValidationStatus.Unverifiable, AddressValidationStatus.Unverifiable);
			TestCase(true, AddressValidationStatus.Invalid, AddressValidationStatus.Verified, AddressValidationStatus.Verified);

			void TestCase(bool addressOverride, string orgAddressValidationStatus, string jobDocAddressValidationStatus, string expectedValidationStatus)
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.MainAddress.Address1 = "ABC AT D";
				org.MainAddress.Address2 = "ABC AT D";
				org.MainAddress.ValidationStatus = orgAddressValidationStatus;
				var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_OA_Address = org.MainAddress.PK;
				jobDocAddress.E2_AddressOverride = addressOverride;
				jobDocAddress.E2_ValidationStatus = jobDocAddressValidationStatus;
				Factory.Save();

				AssertEquals(expectedValidationStatus, jobDocAddress.E2_ValidationStatus);
			}
		}

		#endregion

		#region ILocation Members

		public void TestILocationMembers()
		{
			#region Location Set up

			var testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_Code = "YR";
			testCountry.RN_Desc = "Skyrim";

			var state1 = Factory.NewWithValidTestData<RefCountryStates>();
			state1.RW_Code = "WR";
			state1.RW_Description = "Whiterun";
			state1.RW_RN_NKCountryCode = testCountry.RN_Code;

			var state2 = Factory.NewWithValidTestData<RefCountryStates>();
			state2.RW_Code = "TR";
			state2.RW_Description = "The Rift";
			state2.RW_RN_NKCountryCode = testCountry.RN_Code;

			var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco1.RL_Code = "YRWHR";
			unloco1.RL_PortName = "Whiterun";
			unloco1.RL_RW = state1.PK;

			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco2.RL_Code = "YRRIF";
			unloco2.RL_PortName = "Riften";
			unloco2.RL_RW = state2.PK;

			var cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_RN_NKCountry = testCountry.RN_Code;
			cityTown1.R9_InternationalName = "Whiterun";
			cityTown1.R9_RW_NKState = state1.RW_Code;

			var cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown2.R9_RN_NKCountry = testCountry.RN_Code;
			cityTown2.R9_InternationalName = "Riften";
			cityTown2.R9_RW_NKState = state2.RW_Code;

			var cityTown3 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown3.R9_RN_NKCountry = testCountry.RN_Code;
			cityTown3.R9_InternationalName = "Shor's Stone";
			cityTown3.R9_RW_NKState = state2.RW_Code;

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.OA_RN_NKCountryCode = testCountry.RN_Code;
			mainAddress.OA_Address1 = "1";
			mainAddress.OA_City = cityTown1.R9_InternationalName;
			mainAddress.OA_State = state1.RW_Code;
			mainAddress.OA_RL_NKRelatedPortCode = unloco1.RL_Code;

			var deliveryAddress = org.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_RN_NKCountryCode = testCountry.RN_Code;
			deliveryAddress.OA_Address1 = "2";
			deliveryAddress.OA_City = cityTown2.R9_InternationalName;
			deliveryAddress.OA_State = state2.RW_Code;
			deliveryAddress.OA_RL_NKRelatedPortCode = unloco2.RL_Code;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultAddressType = AddressType.DLV;
			docAddress.OrganisationPK = org.PK;

			AssertEquals(deliveryAddress.PK, docAddress.E2_OA_Address);
			AssertEquals("Should load UNLOCO from the address", unloco2, ((ILocation)docAddress).UNLOCO);
			AssertEquals(testCountry, ((ILocation)docAddress).Country);
			AssertEquals(state2, ((ILocation)docAddress).State);
			AssertEquals("Should match city name and state to city town", cityTown2, ((ILocation)docAddress).CityTown);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_City = cityTown3.R9_InternationalName;

			AssertEquals("Should match new city town", cityTown3, ((ILocation)docAddress).CityTown);
			AssertEquals(testCountry, ((ILocation)docAddress).Country);
			AssertEquals(state2, ((ILocation)docAddress).State);
			AssertNull("Address has been overriden, cannot guess UNLOCO", ((ILocation)docAddress).UNLOCO);

			docAddress = Factory.New<JobDocAddress>();
			docAddress.OrganisationPK = org.PK;

			AssertEquals(mainAddress.PK, docAddress.E2_OA_Address);
			AssertEquals("ILocation.Code should be empty. if it is implemented as non-empty then the tax defaulting for org can be impacted, so please consult accounting team", ZString.Empty, ((ILocation)docAddress).Code);
			AssertEquals(unloco1, ((ILocation)docAddress).UNLOCO);
			AssertEquals(testCountry, ((ILocation)docAddress).Country);
			AssertEquals(state1, ((ILocation)docAddress).State);
			AssertEquals(cityTown1, ((ILocation)docAddress).CityTown);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_State = state1.RW_Code;
			docAddress.E2_City = cityTown3.R9_InternationalName;
			AssertEquals(state1, ((ILocation)docAddress).State);
			AssertNull("Should not match as this city does not belong to this state", ((ILocation)docAddress).CityTown);

			docAddress.E2_State = ZString.Empty;
			AssertNull("No fall back for state", ((ILocation)docAddress).State);
			AssertEquals("Should match when state has been removed", cityTown3, ((ILocation)docAddress).CityTown);
		}

		public void TestILocationCompletelyCovers()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "GBBYS";
			var mainOrgAddress = org.MainAddress;
			mainOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			mainOrgAddress.OA_RL_NKRelatedPortCode = "GBBYS";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultAddressType = AddressType.DLV;
			docAddress.OrganisationPK = org.PK;

			var britain = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			var gbUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBABB"));
			var gbZone = Factory.NewWithValidTestData<RefZoneHeader>();
			gbZone.UNLOCOs.Add(gbUnloco);

			AssertEquals("Same jobDoc covers each other.", true, docAddress.CompletelyCovers(docAddress));
			AssertEquals("jobDoc can't cover UNLOCO", false, docAddress.CompletelyCovers(gbUnloco));
			AssertEquals("jobDoc doesn't cover country", false, docAddress.CompletelyCovers(britain));
			AssertEquals("jobDoc doesn't cover Zone with unloco.", false, docAddress.CompletelyCovers(gbZone));
			AssertEquals("jobdoc doesn't cover different docAddress", false, docAddress.CompletelyCovers(mainOrgAddress));
		}

		#endregion

		#region Logging

		public void TestLogMessage()
		{
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			Factory.Save();
			AssertEquals("No Add Log was generated", 0, address.Logs.GetAllLogs().Count);

			address.Address1 = "TESTADDRESS";
			Factory.Save();
			AssertEquals("No EDT Log was generated", 0, address.Logs.GetAllLogs().Count);

			address.Delete();
			AssertEquals("No DEL Log was generated", 0, address.Logs.GetAllLogs().Count);
		}

		#endregion

		#region DuplicatedRecords

		public void TestDuplicatedRecords()
		{
			var pk = ZGuid.Empty;
			for (int i = 0; i < 10; i++)
			{
				var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_AddressOverride = true;
				jobDocAddress.E2_Address1 = "JobDocAddress1";
				jobDocAddress.E2_Address2 = "JobDocAddress2";
				jobDocAddress.E2_Postcode = "0001";
				jobDocAddress.E2_City = "ALEXANDRIA";
				jobDocAddress.E2_State = "NSW";
				jobDocAddress.E2_RN_NKCountryCode = "AU";
				jobDocAddress.GeoLocation = ZGeography.CreatePoint(1.0, 1.0);
				jobDocAddress.AddressMap = "AddressMap";
				jobDocAddress.E2_ValidationStatus = "INV";
			}

			Factory.Save();

			var jobAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "JobDocAddress1")).FirstOrDefault();
			AssertEquals(10, jobAddress.AllDuplicatedRecords.Count);
		}

		#endregion

		#region IsRowDeletedOrDetachedOrNull

		public void TestGetOrganisationPKWhenJobDocAddressIsDetached()
		{
			var detachedAddress = Factory.NewWithValidTestData<JobDocAddress>();
			detachedAddress.Delete();
			AssertEquals(DataRowState.Detached, ((INeedRow)detachedAddress).Row.RowState);

			ErrorReporter.Clear();
			var pk = detachedAddress.OrganisationPK;
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestGetOrganisationPKWhenJobDocAddressIsDeleted()
		{
			var deletedAddress = Factory.NewWithValidTestData<JobDocAddress>();
			Factory.Save();
			deletedAddress.Delete();
			AssertEquals(DataRowState.Deleted, ((INeedRow)deletedAddress).Row.RowState);

			ErrorReporter.Clear();
			var pk = deletedAddress.OrganisationPK;
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestSetOrganisationPKWhenJobDocAddressIsDetached()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "NO!!";
			Factory.Save();

			ErrorReporter.Clear();
			var addressCollection = new JobDocAddressCollection(Factory);
			var address = ((IBindingList)addressCollection).AddNew();
			var detachedAddress = (JobDocAddress)address;
			AssertEquals(DataRowState.Detached, ((INeedRow)detachedAddress).Row.RowState);
			detachedAddress.OrganisationPK = org.PK;
			AssertEquals("Should have a OrgCode as OrganisationPK has entered.", org.OH_Code, detachedAddress.Organisation.OH_Code);
			AssertEquals("E2_OA_Address is populated", org.MainAddress.PK, detachedAddress.E2_OA_Address);
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestSetOrganisationPKWhenJobDocAddressIsDeleted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "NO!!";
			Factory.Save();

			var deletedAddress = Factory.NewWithValidTestData<JobDocAddress>();
			Factory.Save();
			deletedAddress.Delete();
			AssertEquals(DataRowState.Deleted, ((INeedRow)deletedAddress).Row.RowState);

			ErrorReporter.Clear();
			deletedAddress.OrganisationPK = org.PK;
			AssertNull("Should not have organization", deletedAddress.Organisation);
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		#endregion

		#region CountryCodeChangedEvent

		public void TestCountryCodeChangedEvent_WhenSettingCountryCodeWithDifferentValue_ShouldRaiseEvent()
		{
			// Arrange.

			var isEventRaised = false;
			var oldCode = string.Empty;
			var propertyName = string.Empty;

			DocAddress.E2_RN_NKCountryCode = "AU";

			DocAddress.CountryCodeChanged += (_, args) =>
			{
				isEventRaised = true;

				if (args != null)
				{
					oldCode = args.OldValue?.ToString();
					propertyName = args.Property.Name;
				}
			};

			// Act.

			DocAddress.E2_RN_NKCountryCode = "US";

			// Assert.

			AssertEquals(true, isEventRaised);
			AssertEquals("AU", oldCode);
			AssertEquals("E2_RN_NKCountryCode", propertyName);
		}

		public void TestCountryCodeChangedEvent_WhenSettingCountryCodeWithSameValue_ShouldNotRaiseEvent()
		{
			// Arrange.

			var isEventRaised = false;
			var oldCode = string.Empty;

			DocAddress.E2_RN_NKCountryCode = "AU";

			DocAddress.CountryCodeChanged += (_, args) =>
			{
				isEventRaised = true;
			};

			// Act.

			DocAddress.E2_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(false, isEventRaised);
		}

		public void TestCountryCodeChangedEvent_WhenFindingNoEventHandler_ShouldNotThrowException()
		{
			// Arrange.

			DocAddress.E2_RN_NKCountryCode = "AU";

			// Act & Assert.

			AssertNoExceptionThrown(() => DocAddress.E2_RN_NKCountryCode = "US");
		}

		public void TestCountryCodeChangedEvent_WhenSettingOtherAddressFieldsBesidesCountryCode_ShouldNotRaiseEvent()
		{
			// Arrange.

			var isEventRaised = false;

			DocAddress.E2_AdditionalAddressInformation = "Leave at reception";
			DocAddress.E2_Address1 = "42 RIORDAN ST";
			DocAddress.E2_Address2 = "WTG";
			DocAddress.E2_City = "XANDRIA";
			DocAddress.E2_Postcode = "3000";
			DocAddress.E2_State = "VIC";
			DocAddress.E2_Latitude = 0.0;
			DocAddress.E2_Longitude = 0.0;
			DocAddress.E2_ValidationStatus = "NYV";

			DocAddress.CountryCodeChanged += (_, args) =>
			{
				isEventRaised = true;
			};

			// Act.

			DocAddress.E2_AdditionalAddressInformation = "Ground Level";
			DocAddress.E2_Address1 = "UNIT 3, 72 O'RIORDAN ST";
			DocAddress.E2_Address2 = "WISETECH GLOBAL";
			DocAddress.E2_City = "ALEXANDRIA";
			DocAddress.E2_Postcode = "2015";
			DocAddress.E2_State = "NSW";
			DocAddress.E2_Latitude = 42.0;
			DocAddress.E2_Longitude = 42.0;
			DocAddress.E2_ValidationStatus = "VAD";

			// Assert.

			AssertEquals(false, isEventRaised);
		}

		#endregion

		#region AddressSourceTable 

		public void TestAddressSourceTable_WithNonEmptyParentTable_EqualsParentTableCode()
		{
			var address = Factory.New<JobDocAddress>();
			address.E2_ParentTableCode = "JK";
			AssertEquals("JK", address.AddressSourceTable);
		}

		public void TestAddressSourceTable_WithEmptyParentTable_DefaultsToSchemaPrefix()
		{
			var address = Factory.New<JobDocAddress>();
			AssertEquals(JobDocAddressSchema.Constants.Prefix, address.AddressSourceTable);
		}

		#endregion

		#region Address Validation Event Log

		public void TestAddAddressValidationEventLog()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_ParentID = shipment.PK;
			jobDocAddress.E2_ParentTableCode = "JS";
			jobDocAddress.E2_AddressType = "GBA";
			jobDocAddress.ValidationStatus = AddressValidationStatus.Invalid;
			Factory.Save();

			var shipmentLogs = (shipment as BusinessObject).GetLogs();
			AssertMostRecentLog(shipmentLogs, 1, "ADD", "Added a record to the system", "", "");

			jobDocAddress.ValidationStatus = AddressValidationStatus.Verified;
			Factory.Save();
			shipmentLogs = (shipment as BusinessObject).GetLogs();
			AssertMostRecentLog(shipmentLogs, 3, "AVS", "Address Validation Status", "|DEP=GBA|STA=VAD|TYP=AVS", "Address Validation Status: Valid, Type: Address Validation Service, Party: Goods Billed To Address");

			jobDocAddress.ValidationStatus = AddressValidationStatus.Invalid;
			Factory.Save();
			shipmentLogs = (shipment as BusinessObject).GetLogs();
			AssertMostRecentLog(shipmentLogs, 3, "AVS", "Address Validation Status", "|DEP=GBA|STA=VAD|TYP=AVS", "Address Validation Status: Valid, Type: Address Validation Service, Party: Goods Billed To Address");

			jobDocAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();
			shipmentLogs = (shipment as BusinessObject).GetLogs();
			AssertMostRecentLog(shipmentLogs, 4, "AVS", "Address Validation Status", "|DEP=GBA|STA=VAD|TYP=MAN", "Address Validation Status: Valid, Type: Manual Verification, Party: Goods Billed To Address");

			void AssertMostRecentLog(Logs logs, int expectedCount, string expectedType, string expectedDescription, string expectedReference, string expectedEventDetail)
			{
				var log = logs.GetAllLogs().Cast<StmALog>().Where(x => x.Event.SE_Code == AutoEvents.AddressValidationStatusCode || x.Event.SE_Code == AutoEvents.AddedARecordToTheSystemCode).OrderByDescending(x => x.SL_EventTime).FirstOrDefault();
				CombineAssertions(() =>
				{
					AssertEquals(expectedCount, logs.GetAllLogs().Count);
					AssertEquals(expectedType, log.Event.SE_Code);
					AssertEquals(expectedDescription, log.Event.SE_Desc);
					AssertEquals(expectedReference, log.SL_Reference);
					AssertEquals(expectedEventDetail, log.DisplayEventReference);
				});
			}
		}

		#endregion

		#region PreSaveValidation

		public void TestRunPreSaveValidationCoreNotThrowExceptionForDeletedBizo()
		{
			AssertNoExceptionThrown(() =>
			{
				var docAddress = Factory.NewWithValidTestData<JobDocAddressForTest>();
				docAddress.Delete();
				docAddress.RunPreSaveValidationCore_Exposed();
			});
		}

		#endregion

		#region TestConcurrencyExceptionAfterMerge

		public void TestConcurrencyExceptionAfterMergeCore_WhenRowHasBeendeDeleted_ShouldNotThrowException()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = org.MainAddress.PK;
			jobDocAddress.E2_AddressOverride = false;
			Factory.Save();

			var jobDocAddressInNewFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<JobDocAddress>(jobDocAddress.PK);
			jobDocAddressInNewFactory.E2_AddressOverride = true;
			jobDocAddressInNewFactory.Factory.Save();

			jobDocAddress.Delete();

			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true));
		}

		#endregion

		public void TestDocAddressNumbers()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddressForTest>();
			jobDocAddress.E2_AddressOverride = true;
			var jobDocNumber1 = Factory.NewWithValidTestData<JobDocAddressNumber>();
			jobDocNumber1.E2N_E2 = jobDocAddress.PK;
			jobDocNumber1.E2N_NumberType = "VAT";
			jobDocNumber1.E2N_Number = "123456";
			var emptyNumber = jobDocAddress.DocAddressNumbers.AddNew();
			Factory.Save();
			AssertEquals("Empty JobDocAddressNumber should be deleted after saving.", true, emptyNumber.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedJobDocAddress = newFactory.Load<JobDocAddressForTest>(jobDocAddress.PK);
			AssertEquals("DocAddressNumbers should have been correctly loaded.", 1, reloadedJobDocAddress.DocAddressNumbers.Count);
			AssertEquals("Item of DocAddressNumbers should have been correctly loaded.", jobDocNumber1.PK, reloadedJobDocAddress.DocAddressNumbers[0].PK);

			jobDocAddress.E2_AddressOverride = false;
			AssertEquals("Should not yet delete dbo.JobDocAddressNumber when E2_AddressOverride set to false.", false, jobDocNumber1.IsDeleted);
			Factory.Save();
			AssertEquals("Should delete dbo.JobDocAddressNumber after saving.", true, jobDocNumber1.IsDeleted);

			var jobDocAddress2 = Factory.NewWithValidTestData<JobDocAddressForTest>();
			jobDocAddress2.E2_AddressOverride = true;
			var jobDocNumber2 = Factory.NewWithValidTestData<JobDocAddressNumber>();
			jobDocNumber2.E2N_E2 = jobDocAddress2.PK;
			jobDocNumber2.E2N_NumberType = "VAT";
			jobDocNumber2.E2N_Number = "123456";
			Factory.Save();
			jobDocAddress.Delete();
			Factory.Save();
			AssertEquals("JobDocAddressNumber should be deleted after JobDocAddress delete.", true, jobDocNumber1.IsDeleted);
		}

		public void TestICusInBondHeaderSupportedAsParent()
		{
			var jobDocAddress = Factory.New<JobDocAddressForTest>();
			AssertEquals(true, typeof(Enterprise.Integration.Customs.ICusInBondMoveHeader).IsAssignableFrom(jobDocAddress.GetParentTypeExposed("BM")));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory) as JobDocAddress;
			result.E2_AddressOverride = true;

			return result;
		}

		JobDocAddress GetNewDocAddressWithParent()
		{
			BusinessObject parentForTest = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			return JobDocAddress.New(parentForTest);
		}

		ZPropertyInfo[] AddressOverrideProperties
		{
			get
			{
				return new ZPropertyInfo[]
				{
					DocAddress.E2_Address1Info, DocAddress.E2_Address2Info, DocAddress.E2_CityInfo,
					DocAddress.E2_CompanyNameInfo, DocAddress.E2_EmailInfo, DocAddress.E2_MobileInfo,
					DocAddress.E2_FaxInfo, DocAddress.E2_PhoneInfo, DocAddress.E2_PostcodeInfo,
					DocAddress.E2_RN_NKCountryCodeInfo, DocAddress.E2_StateInfo,
					DocAddress.E2_Address1AndE2_Address2Info
				};
			}
		}

		ZPropertyInfo[] AddressNotOverrideProperties
		{
			get { return new ZPropertyInfo[] { DocAddress.E2_OA_AddressInfo, DocAddress.OrganisationPKInfo }; }
		}

		OrgAddress GetNewOrgAddress()
		{
			var address = Factory.New<OrgAddress>();
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = DefaultUNLOCOPortCode;
			uNLOCO.RL_RN_NKCountryCode = Country.RN_Code;

			return address;
		}

		RefCountry Country
		{
			get
			{
				if (country == null)
				{
					country = Factory.New<RefCountry>();
					country.RN_Code = DefaultCountryCode;
				}
				return country;
			}
		}
		RefCountry country;

		OrgContact SetDefaultContactAndCompanyForAddress(OrgAddress address)
		{
			OrgContact contact = null;
			if (address != null)
			{
				OrgHeader org = Factory.New<OrgHeader>();
				address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
				org.Addresses.Add(address);
				org.OH_FullName = DefaultCompanyName;
				org.OH_Code = DefaultCompanyCode;
				contact = org.Contacts.AddNew();
				contact.OC_ContactName = DefaultContactName;
				contact.OC_Phone = DefaultContactPhone;
			}

			return contact;
		}

		JobDocAddress DocAddress => docAddress ?? (docAddress = Factory.New<JobDocAddress>());
		JobDocAddress docAddress;

		const string DefaultUNLOCOPortCode = "XXABC";
		const string DefaultCountryCode = "XX";
		const string DefaultContactName = "John Smith";
		const string DefaultContactPhone = "18181818";
		const string DefaultCompanyCode = "PingPongX";
		const string DefaultCompanyName = "PingPong Ball Restorers";

		#endregion

		#region IDataVersionLoggingSupported

		public void TestJobDocAddress_IsDataVersionLoggingSupported()
		{
			// Arrange
			var jobDocAddress = Factory.New<JobDocAddress>();

			// Act & Assert
			AssertEquals("JobDocAddress should implement IDataVersionLoggingSupported", true, jobDocAddress is IDataVersionLoggingSupported);
			AssertEquals("JobDocAddress should enable IsDataVersionsAutoLogged", true, ((IDataVersionLoggingSupported)jobDocAddress).IsDataVersionsAutoLogged);
		}

		#endregion
	}
}
