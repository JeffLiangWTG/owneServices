using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DocumentEngineCore;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeader))]
	public class OrgHeaderTest : EnterpriseBusinessObjectTestCase
	{
		#region TestGetRefreshOptions

		public void TestGetASNRefreshOptions()
		{
			var header = Factory.New<OrgHeader>();
			var companyData = header.CompanyData;
			companyData.ImporterOverride = true;
			var currentCompanyGuid = GlbCompany.CurrentCompany.PK.ToGuid();

			var refreshOptions = header.GetASNRefreshOptions(currentCompanyGuid);
			AssertEquals(0, refreshOptions.Count());

			var customsDataRegistryMock = new Mock<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>();
			customsDataRegistryMock.Setup(x => x.ASNRefreshOptionsFieldTypes(currentCompanyGuid)).Returns(ImmutableList.Create<ZString>(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Preference));

			using (ObjectFactory.Substitute(customsDataRegistryMock.Object))
			{
				refreshOptions = header.GetASNRefreshOptions(currentCompanyGuid);
				AssertEquals(0, refreshOptions.Count());

				companyData.ImporterOverride = false;

				refreshOptions = header.GetASNRefreshOptions(currentCompanyGuid);
				AssertEquals(1, refreshOptions.Count());
				AssertEquals(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Preference, refreshOptions.First());

				companyData.ImporterOverride = true;
				companyData.DefaultOptions.Add(new IMProductValueDefaultOption(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification, Factory));
				refreshOptions = header.GetASNRefreshOptions(currentCompanyGuid);
				AssertEquals(1, refreshOptions.Count());
				AssertEquals(Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification, refreshOptions.First());
			}
		}

		#endregion

		#region Doc Data
		public void TestAdditionalInformation()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.AdditionalInformation.Description, "Test Additional Information");
			AssertEquals("Test Additional Information", header.AdditionalInformation);
		}

		public void TestExportersBankName()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExportersBankName.Description, "Test Export Bank Name");
			AssertEquals("Test Export Bank Name", header.ExportersBankName);
		}
		public void TestExportersBankAccountNo()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExportersBankAccountNo.Description, "Test Export Bank Number");
			AssertEquals("Test Export Bank Number", header.ExportersBankAccount);
		}
		public void TestExportersBankSWIFTCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExportersBankSWIFTCode.Description, "Test Export Bank Swift Code");
			AssertEquals("Test Export Bank Swift Code", header.ExportersSwiftCode);
		}
		public void TestMethodOfPayment()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.MethodOfPayment.Description, "Test Method Of Payment");
			AssertEquals("Test Method Of Payment", header.MethodOfPayment);
		}
		#endregion

		#region IConversationProvider

		public void TestInterfaceReturnsCorrectValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = "My Org";
			org.OH_IsActive = true;
			org.OH_Code = "MYORGSYD";

			CombineAssertions(() =>
			{
				var participant = (IConversationParticipant)org;
				AssertEquals("MYORGSYD", participant.Code);
				AssertEquals("My Org", participant.Name);
				AssertEquals("My Org", participant.OrganisationName);
				AssertEquals("AUSYD", participant.Location);
				Assert("Should return true when org is active", participant.IsActive);
				Assert("Organisation is always external - IsInternal should return false", !participant.IsInternal);

				org.OH_IsActive = false;
				Assert("Should return false when org is inactive", !participant.IsActive);
			});
		}

		#endregion

		#region ARAddress

		public void TestAddressForSendingAPOrARDocuments_DbHits()
		{
			var header = Factory.New<OrgHeader>();
			var mainOfficeAddress = header.Addresses.AddNew();

			_ = header.AddressForSendingAPDocuments;
			_ = header.AddressForSendingARDocuments;
			AssertEquals("Precondition: GlbCompany in uber factory has 1 OrgHeader DB table hit.", 1, GlbCompany.CurrentCompany.Factory.GetTableHitCount(OrgHeaderSchema.Constants.TableName));

			// expect no additional DB hits
			var expectedNoDbHits = new Dictionary<string, int>();

			using (AssertDbHitsForAllFactories(expectedNoDbHits))
			{
				for (int index = 0; index < 10; index++)
				{
					_ = header.AddressForSendingAPDocuments;
					_ = header.AddressForSendingARDocuments;
				}
			}
		}

		public void TestAddressForSendingAPDocuments()
		{
			AssertAddressForSendingDocuments(LedgerTypes.AccountsPayable);
		}

		public void TestAddressForSendingARDocuments()
		{
			AssertAddressForSendingDocuments(LedgerTypes.AccountsReceivable);
		}

		public void TestHasMoreThanOneDefaultForDocumentMenuItem_NullMenuItem()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			bool hasMoreThanOneDefault = header.HasMoreThanOneDefaultForDocumentMenuItem(ZGuid.Empty);
			AssertEquals(false, hasMoreThanOneDefault);
		}

		void AssertAddressForSendingDocuments(string documentType)
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainOfficeAddress = header.Addresses.AddNew();
			OrgAddress officeAddress = header.Addresses.AddNew();
			SetupAddress(mainOfficeAddress, "Main Office Address", OrgConstants.AddressType.Office, true);
			SetupAddress(officeAddress, "Office Address", OrgConstants.AddressType.Office, false);
			AssertEquals("Main Office address", mainOfficeAddress,
				documentType == LedgerTypes.AccountsReceivable ? header.AddressForSendingARDocuments : header.AddressForSendingAPDocuments);

			OrgAddress mainPostalAddress = header.Addresses.AddNew();
			OrgAddress postalAddress = header.Addresses.AddNew();
			SetupAddress(mainPostalAddress, "Main Postal Address", OrgConstants.AddressType.Postal, true);
			SetupAddress(postalAddress, "Postal Address", OrgConstants.AddressType.Postal, false);
			AssertEquals("Main Postal address should be used over Office address", mainPostalAddress,
				documentType == LedgerTypes.AccountsReceivable ? header.AddressForSendingARDocuments : header.AddressForSendingAPDocuments);

			OrgAddress mainReceivablesAddress = header.Addresses.AddNew();
			OrgAddress receivablesAddress = header.Addresses.AddNew();
			SetupAddress(mainReceivablesAddress, "Main Receivables Address", OrgConstants.AddressType.Receivables, true);
			SetupAddress(receivablesAddress, "Receivables Address", OrgConstants.AddressType.Receivables, false);

			OrgAddress mainPayablesAddress = header.Addresses.AddNew();
			OrgAddress payablesAddress = header.Addresses.AddNew();
			SetupAddress(mainPayablesAddress, "Main Payabales Address", OrgConstants.AddressType.Payables, true);
			SetupAddress(payablesAddress, "Payables Address", OrgConstants.AddressType.Payables, false);

			if (documentType == LedgerTypes.AccountsReceivable)
			{
				AssertEquals("Main Receivables address should be used over postal and office address", mainReceivablesAddress, header.AddressForSendingARDocuments);
			}
			else
			{
				AssertEquals("Main Payables address should be used over postal and office address", mainPayablesAddress, header.AddressForSendingAPDocuments);
			}
		}

		public void TestAddressForSendingAPDocumentsLanguage()
		{
			AssertAddressForSendingDocuments_LanguageSpecific(LedgerTypes.AccountsPayable);
		}

		public void TestAddressForSendingARDocumentsLanguage()
		{
			AssertAddressForSendingDocuments_LanguageSpecific(LedgerTypes.AccountsReceivable);
		}

		void AssertAddressForSendingDocuments_LanguageSpecific(string documentType)
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Language = Constants.Languages.English;
			SetupAddress(header.MainAddress, "Main Office Address", OrgConstants.AddressType.Office, true);

			OrgAddress receivableAddressChinese = header.Addresses.AddNew();
			SetupAddress(receivableAddressChinese, "Chinese Receivable Address", OrgConstants.AddressType.Receivables, true, Constants.Languages.ChineseSimplified);
			OrgAddress receivableAddressEnglish = header.Addresses.AddNew();
			SetupAddress(receivableAddressEnglish, "English Receivable Address", OrgConstants.AddressType.Receivables, true, Constants.Languages.English);

			OrgAddress payablesAddressChinese = header.Addresses.AddNew();
			SetupAddress(payablesAddressChinese, "Chinese Payables Address", OrgConstants.AddressType.Payables, true, Constants.Languages.ChineseSimplified);
			OrgAddress payablesAddressEnglish = header.Addresses.AddNew();
			SetupAddress(payablesAddressEnglish, "English Payables Address", OrgConstants.AddressType.Payables, true, Constants.Languages.English);

			OrgAddress postalAddressEnglish = header.Addresses.AddNew();
			SetupAddress(postalAddressEnglish, "Postal Address English", OrgConstants.AddressType.Postal, true, Constants.Languages.English);
			OrgAddress postalAddressChinese = header.Addresses.AddNew();
			SetupAddress(postalAddressChinese, "Postal Address Chinese", OrgConstants.AddressType.Postal, true, Constants.Languages.ChineseSimplified);
			OrgAddress officeAddressChinese = header.Addresses.AddNew();
			SetupAddress(officeAddressChinese, "Office Address Chinese", OrgConstants.AddressType.Office, true, Constants.Languages.ChineseSimplified);

			OrgHeader loginOrgProxy = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			AssertEquals("Current login company language is ENG", Constants.Languages.English, loginOrgProxy.OH_Language);
			if (documentType == LedgerTypes.AccountsReceivable)
			{
				AssertEquals("ARAddress should use the English receivable address", "English Receivable Address", header.AddressForSendingARDocuments.OA_Address1);
			}
			else
			{
				AssertEquals("ARAddress should use the English payables address", "English Payables Address", header.AddressForSendingAPDocuments.OA_Address1);
			}

			header.OH_Language = Constants.Languages.ChineseSimplified;
			loginOrgProxy.OH_Language = Constants.Languages.ChineseSimplified;
			Factory.Save();
			if (documentType == LedgerTypes.AccountsReceivable)
			{
				AssertEquals("ARAddress should use the Chinese receivable address", "Chinese Receivable Address", header.AddressForSendingARDocuments.OA_Address1);
			}
			else
			{
				AssertEquals("ARAddress should use the Chinese payables address", "Chinese Payables Address", header.AddressForSendingAPDocuments.OA_Address1);
			}

			header.OH_Language = Constants.Languages.French;
			loginOrgProxy.OH_Language = Constants.Languages.French;
			Factory.Save();
			if (documentType == LedgerTypes.AccountsReceivable)
			{
				AssertEquals("ARAddress should use the English receivable address when no ARM address with org and login language is found", "English Receivable Address", header.AddressForSendingARDocuments.OA_Address1);
			}
			else
			{
				AssertEquals("ARAddress should use the English receivable address when no ARM address with org and login language is found", "English Payables Address", header.AddressForSendingAPDocuments.OA_Address1);
			}

			header.Addresses.Remove(receivableAddressEnglish);
			header.Addresses.Remove(payablesAddressEnglish);
			receivableAddressEnglish.Delete();
			payablesAddressEnglish.Delete();
			header.OH_Language = Constants.Languages.ChineseSimplified;
			loginOrgProxy.OH_Language = Constants.Languages.English;
			Factory.Save();
			if (documentType == LedgerTypes.AccountsReceivable)
			{
				AssertEquals("ARAddress should use the Chinese receivable address even proxy language is English since it's the only ARM address", "Chinese Receivable Address", header.AddressForSendingARDocuments.OA_Address1);
			}
			else
			{
				AssertEquals("ARAddress should use the Chinese payables address even proxy language is English since it's the only ARM address", "Chinese Payables Address", header.AddressForSendingAPDocuments.OA_Address1);
			}

			header.OH_Language = Constants.Languages.English;
			if (documentType == LedgerTypes.AccountsReceivable)
			{
				AssertEquals("ARAddress should use the Chinese receivable address even both org language and proxy language are English", "Chinese Receivable Address", header.AddressForSendingARDocuments.OA_Address1);
			}
			else
			{
				AssertEquals("ARAddress should use the Chinese payables address even both org language and proxy language are English", "Chinese Payables Address", header.AddressForSendingAPDocuments.OA_Address1);
			}

			header.OH_Language = Constants.Languages.ChineseSimplified;
			loginOrgProxy.OH_Language = Constants.Languages.ChineseSimplified;
			Factory.Save();
			header.Addresses.Remove(receivableAddressChinese);
			header.Addresses.Remove(payablesAddressChinese);
			AssertEquals("ARAddress should use the Chinese postal address when no receivable address is found", "Postal Address Chinese", (documentType == LedgerTypes.AccountsReceivable ? header.AddressForSendingARDocuments : header.AddressForSendingAPDocuments).OA_Address1);

			header.Addresses.Remove(postalAddressChinese);
			AssertEquals("ARAddress should use the English postal address when no Chinese postal address is found ", "Postal Address English", (documentType == LedgerTypes.AccountsReceivable ? header.AddressForSendingARDocuments : header.AddressForSendingAPDocuments).OA_Address1);
			header.Addresses.Remove(postalAddressEnglish);
			AssertEquals("ARAddress should use the Chinese office address when no receivable or postal address is found ", "Office Address Chinese", (documentType == LedgerTypes.AccountsReceivable ? header.AddressForSendingARDocuments : header.AddressForSendingAPDocuments).OA_Address1);

			header.Addresses.Remove(officeAddressChinese);
			AssertEquals("ARAddress should use the main address when no receivable or postal address is found ", "Main Office Address", (documentType == LedgerTypes.AccountsReceivable ? header.AddressForSendingARDocuments : header.AddressForSendingAPDocuments).OA_Address1);
		}

		void SetupAddress(OrgAddress address, string description, string addressCapability, bool isMain, string language = Constants.Languages.English)
		{
			address.OA_Address1 = description;
			address.OA_Address2 = description + " 2";
			address.OA_City = "Org City";
			address.OA_Language = language;
			address.AddressCapability.SetCapabilityEnabled(addressCapability);

			if (isMain)
			{
				address.AddressCapability.SetIsMainAddress(addressCapability);
			}
			else
			{
				address.AddressCapability.SetIsNotMainAddress(addressCapability);
			}
		}

		#endregion

		#region OrgCodeGeneration
		public void TestCodeDuplicationHandlingWithoutUniqueNumber()
		{
			Env.Registry.CanUserEditOrganisationCode = false;

			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;

			algorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header1.OH_RL_NKClosestPort = "AUSYD";
			header1.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header1.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org1 Code", "CARAUSSYD", header1.OH_Code);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgHeader header2 = factory2.New<OrgHeader>();
			header2.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header2.OH_RL_NKClosestPort = "AUSYD";
			header2.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header2.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org2 Code", "CARAUSSYD", header2.OH_Code);
			factory2.Save();
			AssertEquals("Org2 Code", "CARAUSSYD", header2.OH_Code);

			AssertEquals("Org1 Code", "CARAUSSYD", header1.OH_Code);
			Factory.Save();
			AssertEquals("Org1 Code", "CARAUSSYD1", header1.OH_Code);
		}

		public void TestClosedPortGetterUsesProxiedMainAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsGlobalAccount = true;
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			AssertEquals("USLAX", orgHeader.OH_RL_NKClosestPort);
			((INeedRow)orgHeader).Row[OrgHeaderSchema.OH_RL_NKClosestPort.Name] = "AUSYD";
			AssertEquals("USLAX", orgHeader.OH_RL_NKClosestPort);

			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "";
			((INeedRow)orgHeader).Row[OrgHeaderSchema.OH_RL_NKClosestPort.Name] = "AUSYD";
			AssertEquals("AUSYD", orgHeader.OH_RL_NKClosestPort);
		}

		public void TestCodeDuplicationHandlingWithoutUniqueNumberButUserCanEdit()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;

			algorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header1.OH_RL_NKClosestPort = "AUSYD";
			header1.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header1.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org1 Code", "CARAUSSYD", header1.OH_Code);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgHeader header2 = factory2.New<OrgHeader>();
			header2.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header2.OH_RL_NKClosestPort = "AUSYD";
			header2.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header2.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org2 Code", "CARAUSSYD", header2.OH_Code);
			factory2.Save();
			AssertEquals("Org2 Code", "CARAUSSYD", header2.OH_Code);

			AssertEquals("Org1 Code", "CARAUSSYD", header1.OH_Code);
			Factory.Save();
			AssertEquals("Org1 Code", "CARAUSSYD1", header1.OH_Code);
		}

		public void TestDeduplicationStartEventPermissions()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			var reg = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			var security = Env.Security.OrganisationModify.IsAllowed;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Env.Security.OrganisationModify.IsAllowed = false;
				bool wasCalled = false;
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.DeduplicationStarted += (o, e) => wasCalled = true;

				SetDeduplicationConditions(org, true, true, true, true);
				org.FindDuplicates();
				Assert(wasCalled);
				wasCalled = false;

				SetDeduplicationConditions(org, true, true, true, false);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, true, true, false, true);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, true, false, true, true);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, false, true, true, true);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, true, true, false, false);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, true, false, true, false);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, false, true, false, true);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, false, false, true, true);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, true, false, false, true);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, false, true, true, false);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, false, false, false, true);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, false, false, true, false);
				org.FindDuplicates();
				Assert(!wasCalled);
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.OrganisationModify.IsAllowed = false;

				SetDeduplicationConditions(org, false, true, false, false);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, true, false, false, false);
				org.FindDuplicates();
				Assert(!wasCalled);

				SetDeduplicationConditions(org, false, false, false, false);
				org.FindDuplicates();
				Assert(!wasCalled);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
				Env.Security.OrganisationModify.IsAllowed = security;
			}

			void SetDeduplicationConditions(OrgHeader org, bool readOnlyValue, bool enbableDedeuplicationFinderValue, bool organisationModifyAllowedValue, bool shouldRunDeduplicationValue)
			{
				org.ReadOnly = !readOnlyValue;
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enbableDedeuplicationFinderValue);
				Env.Security.OrganisationModify.IsAllowed = organisationModifyAllowedValue;
				((IDeduplicatable)org).ShouldRunDeduplication = shouldRunDeduplicationValue;
			}
		}

		public void TestFindDuplicatesWhenOrgExcluded()
		{
			var originalOrgModify = Env.Security.OrganisationModify.IsAllowed;
			try
			{
				Env.Security.OrganisationModify.IsAllowed = true;
				using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (OrganisationsDataRegistry.Instance.MaximumAddressCountForUserDrivenDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
				using (OrganisationsDataRegistry.Instance.MaximumContactCountForUserDrivenDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
				{
					var org = Factory.NewWithValidTestData<OrgHeaderForTest>();
					((IDeduplicatable)org).ShouldRunDeduplication = true;
					org.ReadOnly = false;
					org.OH_Code = "TestOrg";

					while (org.Addresses.Count < 20)
					{
						org.Addresses.AddNew();
					}

					while (org.Contacts.Count < 20)
					{
						org.Contacts.AddNew();
					}

					var exceedContact = org.Contacts.AddNew();
					org.FindDuplicates();
					AssertEquals("TestOrg: This Organization has 20 addresses and 21 contacts, The system excludes record with more than 20 addresses/20 contacts. Consider if this organization needs to be split using management groups.", org.CurrentExclusionManager.DisplayInfo.Trim());

					org.Addresses.AddNew();
					org.FindDuplicates();
					AssertEquals("TestOrg: This Organization has 21 addresses and 21 contacts, The system excludes record with more than 20 addresses/20 contacts. Consider if this organization needs to be split using management groups.", org.CurrentExclusionManager.DisplayInfo.Trim());

					org.Contacts.Remove(exceedContact);
					org.FindDuplicates();
					AssertEquals("TestOrg: This Organization has 21 addresses and 20 contacts, The system excludes record with more than 20 addresses/20 contacts. Consider if this organization needs to be split using management groups.", org.CurrentExclusionManager.DisplayInfo.Trim());
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = originalOrgModify;
			}
		}

		public void TestDeduplicationActionOccurredEvent()
		{
			var reg = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			try
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				DeduplicationAction actionInvoked = DeduplicationAction.None;
				IDuplicationEventArgs eventArgs = null;
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.DeduplicationActionOccurred += (o, e) =>
				{
					actionInvoked = e.InvokedAction;
					eventArgs = e;
				};

				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null);
				AssertEquals(DeduplicationAction.None, actionInvoked);
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Ignore, null, null, null);
				AssertEquals(DeduplicationAction.Ignore, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Link, null, null, null);
				AssertEquals(DeduplicationAction.Link, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.Merge, null, null, null);
				AssertEquals(DeduplicationAction.Merge, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.OpenMaster, null, null, null);
				AssertEquals(DeduplicationAction.OpenMaster, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.OpenTarget, null, null, null);
				AssertEquals(DeduplicationAction.OpenTarget, actionInvoked);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.ExcludeCountriesFilterChanged, null, null, null, true, false);
				AssertEquals(DeduplicationAction.ExcludeCountriesFilterChanged, actionInvoked);
				Assert(!eventArgs.IsExcludingOtherCountriesFromResults);
				Assert(eventArgs.IsExcludingInactiveFromResults);
				org.PropagateDeduplicationActionOccurred(DeduplicationAction.ExcludeInactiveFilterChanged, null, null, null, false, true);
				AssertEquals(DeduplicationAction.ExcludeInactiveFilterChanged, actionInvoked);
				Assert(eventArgs.IsExcludingOtherCountriesFromResults);
				Assert(!eventArgs.IsExcludingInactiveFromResults);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reg);
			}
		}

		public void TestInvalidOperationExceptionForIncludeAndExclude()
		{
			var deDupOrg = Factory.New<DeduplicationOrganisationForTest>();
			deDupOrg.DOH_Status = DeduplicationHelper.StatusConstants.ToBeProcessed;
			((INeedRow)deDupOrg).Row.AcceptChanges();
			var org = Factory.NewWithPrimaryKey<OrgHeader>(deDupOrg.PK.ToGuid());
			org.OH_Code = "TESTORG";

			var excResult = Factory.New<PatternMatchingResult>();
			excResult.PMT_MasterPK = deDupOrg.PK;
			excResult.PMT_MasterTableCode = deDupOrg.MasterOrgHeader.TableCode;
			excResult.PMT_Status = PatternMatchingResult.StatusCodes.Excluded;
			excResult.PMT_FoundTimeUtc = ZDateTime.UtcToday;
			excResult.PMT_ScorePercent = 0;
			excResult.PMT_GS_NKExcludeBy = "E";

			Factory.Save();
			deDupOrg.Reload();

			AssertExceptionThrown(typeof(InvalidOperationException), DuplicationResponseMessages.Exclusion, () => ((IDeduplicatable)org).IsExcludedFromDeduplication = true);
			((IDeduplicatable)org).IsExcludedFromDeduplication = false;
			AssertExceptionThrown(typeof(InvalidOperationException), DuplicationResponseMessages.NoExclusionExisted, () => ((IDeduplicatable)org).IsExcludedFromDeduplication = false);
		}

		public void TestCodeDuplicationHandlingWithoutUniqueNumberAndAlreadyInDatabaseButRegened()
		{
			Env.Registry.CanUserEditOrganisationCode = false;

			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;

			algorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header1.OH_RL_NKClosestPort = "AUSYD";
			header1.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header1.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org1 Code", "CARAUSSYD", header1.OH_Code);
			Factory.Save();
			AssertEquals("Org1 Code", "CARAUSSYD", header1.OH_Code);

			header1.OH_FullName = "Newly Changed Cargowise Code";
			AssertEquals("Org1 Code", "NEWCODSYD", header1.OH_Code);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgHeader header2 = factory2.New<OrgHeader>();
			header2.OH_FullName = "Newly Changed Cargowise Other Code";
			header2.OH_RL_NKClosestPort = "AUSYD";
			header2.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header2.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org2 Code", "NEWCODSYD", header2.OH_Code);
			factory2.Save();
			AssertEquals("Org2 Code", "NEWCODSYD", header2.OH_Code);

			AssertEquals("Org1 Code", "NEWCODSYD", header1.OH_Code);
			Factory.Save();
			AssertEquals("Org1 Code", "NEWCODSYD1", header1.OH_Code);
		}

		public void TestCodeDuplicationHandlingWithCodeSpecificUniqueNumber()
		{
			Env.Registry.CanUserEditOrganisationCode = false;

			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 4;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;

			algorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header1.OH_RL_NKClosestPort = "AUSYD";
			header1.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header1.MainAddress.OA_City = "Alexandria";
			header1.OH_Code = "CAREDISYD001";

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgHeader header2 = factory2.New<OrgHeader>();
			header2.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header2.OH_RL_NKClosestPort = "AUSYD";
			header2.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header2.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org2 Code", "", header2.OH_Code);
			factory2.Save();
			AssertEquals("Org2 Code", "CARAUSSYD001", header2.OH_Code);

			AssertEquals("Org1 Code", "CAREDISYD001", header1.OH_Code);
			Factory.Save();
			AssertEquals("Org1 Code", "CAREDISYD001", header1.OH_Code);
		}

		public void TestCodeDuplicationHandlingWithCodeSpecificUniqueNumberAlreadyInDatabaseButRegened()
		{
			Env.Registry.CanUserEditOrganisationCode = false;

			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 4;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;

			algorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header1.OH_RL_NKClosestPort = "AUSYD";
			header1.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header1.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org1 Code", "", header1.OH_Code);
			Factory.Save();
			AssertEquals("Org1 Code", "CARAUSSYD001", header1.OH_Code);
			header1.OH_FullName = "Newly Changed Cargowise Code";

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgHeader header2 = factory2.New<OrgHeader>();
			header2.OH_FullName = "Newly Changed Cargowise Other Code";
			header2.OH_RL_NKClosestPort = "AUSYD";
			header2.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header2.MainAddress.OA_City = "Alexandria";

			AssertEquals("Org2 Code", "", header2.OH_Code);
			factory2.Save();
			AssertEquals("Org2 Code", "NEWCODSYD001", header2.OH_Code);

			Factory.Save();
			AssertEquals("Org1 Code", "NEWCODSYD002", header1.OH_Code);
		}

		public void TestCodeDuplicationHandlingWithCodeSpecificUniqueNumberWhenUserCanEditOrgCode()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 4;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;

			algorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header1.OH_RL_NKClosestPort = "AUSYD";
			header1.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header1.MainAddress.OA_City = "Alexandria";

			header1.OH_Code = "USEREDITED";

			AssertEquals("Org1 Code", "USEREDITED", header1.OH_Code);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgHeader header2 = factory2.New<OrgHeader>();
			header2.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header2.OH_RL_NKClosestPort = "AUSYD";
			header2.MainAddress.OA_Address1 = "Unit 3a, 72 O'Riordan Street";
			header2.MainAddress.OA_City = "Alexandria";

			OrgHeader header3 = factory2.New<OrgHeader>();
			header3.OH_FullName = "Duplicated Org for Test";
			header3.OH_RL_NKClosestPort = "AUSYD";
			header3.MainAddress.OA_City = "Alexandria";
			header3.MainAddress.OA_Address1 = "Test address";
			header3.OH_Code = "USEREDITED";
			AssertEquals("Org2 Code", "CARAUSSYD001", header2.OH_Code);
			AssertEquals("Org3 Code", "USEREDITED", header3.OH_Code);
			factory2.Save();
			AssertEquals("Org2 Code", "CARAUSSYD001", header2.OH_Code);
			AssertEquals("Org3 Code", "USEREDITED", header3.OH_Code);

			AssertEquals("Org1 Code", "USEREDITED", header1.OH_Code);
			Factory.Save();
			AssertEquals("Org1 Code", "CARAUSSYD002", header1.OH_Code);
		}

		public void TestPortName()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			AssertNoExceptionThrown(delegate
			{
				var dummy = org.PortName;
			});

			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Sydney", org.PortName);

			org.OH_RL_NKClosestPort = "CNSHA";
			AssertEquals("Shanghai Hongqiao International Apt", org.PortName);
		}

		public void TestCountryName()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			AssertNoExceptionThrown(delegate
			{
				var dummy = org.CountryName;
			});

			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Australia", org.CountryName);

			org.OH_RL_NKClosestPort = "CNSHA";
			AssertEquals("China", org.CountryName);
		}

		public void TestCityName()
		{
			var org = Factory.New<OrgHeader>();
			Assert(org.CityName.IsEmpty);

			org.MainAddress.City = "Town";
			AssertEquals("Town", org.CityName);
		}

		public void TestEditNonCodeSourceFields()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Stormcloaks";
			org1.OH_RL_NKClosestPort = "JPMI3";
			Factory.Save();

			Env.Registry.CanUserEditOrganisationCode = true;

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Stormcloaks";
			org2.OH_RL_NKClosestPort = "JPMI3";
			org2.OH_Code = "STORMCMI32";
			Factory.Save();
			org2 = Factory.Load<OrgHeader>(org2.PK); // Load() is called here because Save() can re-generate the code

			Env.Registry.CanUserEditOrganisationCode = false;

			AssertEquals("STORMCMI3", org1.OH_Code);
			AssertEquals("STORMCMI32", org2.OH_Code);

			org2.OH_IsConsignee = true;
			Factory.Save();
			org2 = Factory.Load<OrgHeader>(org2.PK); // Load() is called here because Save() can re-generate the code
			AssertEquals("STORMCMI32", org2.OH_Code);
		}

		public void TestEditNonCodeSourceFields2()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgCodeAlgorithm oca = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			oca.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 4;
			oca.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oca);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Stormcloaks";
			org1.OH_RL_NKClosestPort = "JPMI3";
			org1.OH_Code = "STORMCMI3000";
			Factory.Save();
			org1 = Factory.Load<OrgHeader>(org1.PK); // Load() is called here because Save() can re-generate the code

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Stormcloaks";
			org2.OH_RL_NKClosestPort = "JPMI3";
			org2.OH_Code = "STORMCMI3002";
			Factory.Save();
			org2 = Factory.Load<OrgHeader>(org2.PK);

			Env.Registry.CanUserEditOrganisationCode = false;

			AssertEquals("STORMCMI3000", org1.OH_Code);
			AssertEquals("STORMCMI3002", org2.OH_Code);

			org2.OH_IsConsignee = true;
			Factory.Save();
			org2 = Factory.Load<OrgHeader>(org2.PK); // Load() is called here because Save() can re-generate the code
			AssertEquals("STORMCMI3002", org2.OH_Code);
		}

		public void TestRegenCodeWhenIsNationalAccountChange()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "ABC DEF XYZ";
			org.OH_IsNationalAccount = false;
			org.OH_RL_NKClosestPort = "AUSYD";

			Factory.Save();

			Assert(!org.OH_Code.EndsWith("AU"));

			org.OH_IsNationalAccount = true;
			Assert(org.OH_Code.EndsWith("AU"));
		}

		public void TestRegenCodeWhenNameIsChangedToOriginalValue()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "ABC DEF XYZ";
			org.OH_IsNationalAccount = false;
			org.OH_RL_NKClosestPort = "AUSYD";

			Factory.Save();

			string originalCode = org.OH_Code;

			org.OH_FullName = "CBA DEF XYZ";
			Assert("Code is regened", org.OH_Code != originalCode);

			org.OH_FullName = "ABC DEF XYZ";
			AssertEquals("Code is regened and back to original value", originalCode, org.OH_Code);
		}

		public void TestOrgCodeRegenOH_IsGlobalAccountForNewOrg()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "ZX";

			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ZXZZZ";
			unloco.RL_RN_NKCountryCode = country.Code;

			organisation.OH_FullName = "Test Org";
			organisation.OH_RL_NKClosestPort = "ZXZZZ";
			organisation.OH_IsGlobalAccount = true;

			Env.Registry.CanUserEditOrganisationCode = false;
			Assert(organisation.OH_IsGlobalAccount);
			AssertEquals("Global account should use WW code instead of UNLOCO", "TESORG_WW", organisation.OH_Code);

			organisation.OH_IsGlobalAccount = false;
			Assert(!organisation.OH_IsGlobalAccount);
			AssertEquals("not Global account should use UNLOCO code.", "TESORGZZZ", organisation.OH_Code);

			Env.Registry.CanUserEditOrganisationCode = true;
			organisation.OH_IsGlobalAccount = true;
			Assert(organisation.OH_IsGlobalAccount);
			AssertEquals("Global account should use WW code instead of UNLOCO", "TESORG_WW", organisation.OH_Code);

			organisation.OH_IsGlobalAccount = false;
			Assert(!organisation.OH_IsGlobalAccount);
			AssertEquals("not Global account should use UNLOCO code.", "TESORGZZZ", organisation.OH_Code);
		}

		public void TestOrgCodeRegenOH_IsGlobalAccountForExistingOrg()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "ZX";

			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ZXZZZ";
			unloco.RL_RN_NKCountryCode = country.Code;

			organisation.OH_FullName = "Test Org";
			organisation.OH_RL_NKClosestPort = "ZXZZZ";
			Factory.Save();

			Env.Registry.CanUserEditOrganisationCode = false;
			organisation.OH_IsGlobalAccount = true;
			Assert(organisation.OH_IsGlobalAccount);
			AssertEquals("Global account should use WW code instead of UNLOCO", "TESORG_WW", organisation.OH_Code);
			Factory.Save();
			organisation = Factory.Load<OrgHeader>(organisation.PK); // Load() is called here because Save() can re-generate the code
			AssertEquals("Global account should use WW code instead of UNLOCO", "TESORG_WW", organisation.OH_Code);

			organisation.OH_IsGlobalAccount = false;
			Assert(!organisation.OH_IsGlobalAccount);
			AssertEquals("not Global account should use UNLOCO code.", "TESORGZZZ", organisation.OH_Code);
			Factory.Save();
			organisation = Factory.Load<OrgHeader>(organisation.PK); // Load() is called here because Save() can re-generate the code
			AssertEquals("not Global account should use UNLOCO code.", "TESORGZZZ", organisation.OH_Code);

			Env.Registry.CanUserEditOrganisationCode = true;
			organisation.OH_IsGlobalAccount = true;
			Assert(organisation.OH_IsGlobalAccount);
			AssertEquals("Global account should use WW code instead of UNLOCO", "TESORG_WW", organisation.OH_Code);
			Factory.Save();
			organisation = Factory.Load<OrgHeader>(organisation.PK); // Load() is called here because Save() can re-generate the code
			AssertEquals("Global account should use WW code instead of UNLOCO", "TESORG_WW", organisation.OH_Code);

			organisation.OH_IsGlobalAccount = false;
			Assert(!organisation.OH_IsGlobalAccount);
			AssertEquals("not Global account should use UNLOCO code.", "TESORGZZZ", organisation.OH_Code);
			Factory.Save();
			organisation = Factory.Load<OrgHeader>(organisation.PK); // Load() is called here because Save() can re-generate the code
			AssertEquals("not Global account should use UNLOCO code.", "TESORGZZZ", organisation.OH_Code);
		}

		public void TestOrgDetailsTypesReadOnly_SecurityDenied()
		{
			var org = Factory.NewWithValidTestData<OrgHeaderForTest>();
			Factory.Save();

			Env.Security.OrgDetailsModifyOrganisationType.IsAllowed = false;
			var isOH_IsCreditorReadOnly = org.GetReadOnlySecurity_Exposed(org.OH_IsCreditorInfo.PropertyDescriptor);
			var isOH_IsDebtorReadOnly = org.GetReadOnlySecurity_Exposed(org.OH_IsDebtorInfo.PropertyDescriptor);
			AssertEquals(true, isOH_IsCreditorReadOnly);
			AssertEquals(true, isOH_IsDebtorReadOnly);

			Env.Security.OrgDetailsModifyOrganisationType.IsAllowed = true;
			isOH_IsCreditorReadOnly = org.GetReadOnlySecurity_Exposed(org.OH_IsCreditorInfo.PropertyDescriptor);
			isOH_IsDebtorReadOnly = org.GetReadOnlySecurity_Exposed(org.OH_IsDebtorInfo.PropertyDescriptor);
			AssertEquals(false, isOH_IsCreditorReadOnly);
			AssertEquals(false, isOH_IsDebtorReadOnly);
		}

		#endregion

		#region TestOrgServiceLevels

		public void TestOrgServiceLevels()
		{
			ActiveServiceLevelCollection refLevels = new ActiveServiceLevelCollection(Factory);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Assert("Precondidtion: should be at least 1 active level to run this test", refLevels.Count > 0);
			AssertEquals("org should contain default levels", refLevels.Count, org.OrgServiceLevels.Count);

			RefServiceLevel changingLevel = refLevels[0];

			RegistryServiceLevelCollection registryValue = WebDataRegistry.Instance.ServiceLevelVisibility.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			foreach (RegistryServiceLevel level in registryValue)
			{
				if (level.RefServiceLevelPK == changingLevel.PK)
				{
					level.Bool = false;
					break;
				}
			}

			WebDataRegistry.Instance.ServiceLevelVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			foreach (RegistryServiceLevel level in WebDataRegistry.Instance.ServiceLevelVisibility.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				if (level.RefServiceLevelPK == changingLevel.PK)
				{
					Assert(!level.Bool);
					break;
				}
			}

			bool found = false;
			foreach (OrgServiceLevel level in org.OrgServiceLevels)
			{
				if (level.PM_RS_NKSrvLvl == changingLevel.RS_Code)
				{
					found = true;
					Assert("This level was unpublished in registry", !level.PM_IsPublished);
					org.IsServiceLevelOverridden = true;
					level.PM_IsPublished = true;
					Factory.Save();
					break;
				}
			}

			Assert(found);

			OrgServiceLevelCollection testCollection = new OrgServiceLevelCollection(Factory, org.PK);
			Assert(testCollection.FindByRefServiceLevelCode(changingLevel.RS_Code).PM_IsPublished);

			Assert(refLevels.Count > 0);
			AssertEquals(refLevels.Count, testCollection.Count);
			AssertEquals(
				"Service levels are overridden and should be stored agains the Org",
				refLevels.Count,
				new BusinessObjectFactory().Load<OrgServiceLevel>(new ZQuery(OrgServiceLevelSchema.PM_OH, org.PK)).Length);

			org.IsServiceLevelOverridden = false;
			Factory.Save();

			AssertEquals("Service levels are not overridden -- all records should be deleted from the DB", 0, new BusinessObjectFactory().Load<OrgServiceLevel>(new ZQuery(OrgServiceLevelSchema.PM_OH, org.PK)).Length);
		}

		#endregion

		#region TestOrgCarrierAccounts

		public void TestOrgCarrierAccounts()
		{
			var org = Factory.New<OrgHeader>();
			var can1 = Factory.New<OrgCarrierAccount>();
			can1.OAN_OH_Carrier = org.PK;
			can1.OAN_AccountNumber = "ACCNO1";
			can1.OAN_DepotID = "DEPID1";
			can1.OAN_MerchantNumber = "MERCNO1";

			AssertEquals(1, org.CarrierAccounts.Count);
			AssertEquals("ACCNO1", org.CarrierAccounts[0].OAN_AccountNumber);
			AssertEquals("DEPID1", org.CarrierAccounts[0].OAN_DepotID);
			AssertEquals("MERCNO1", org.CarrierAccounts[0].OAN_MerchantNumber);
		}

		#endregion

		#region TestOrgCarrierNamedAccounts

		public void TestOrgCarrierNamedAccounts()
		{
			var org = Factory.New<OrgHeader>();
			var can1 = Factory.New<OrgCarrierNamedAccount>();
			can1.ONA_OH_Carrier = org.PK;
			can1.ONA_ForeignName = "Test";

			AssertEquals(1, org.CarrierNamedAccounts.Count);
			AssertEquals("Test", org.CarrierNamedAccounts[0].ONA_ForeignName);
		}

		#endregion

		#region TestOrgMappedNamedAccounts

		public void TestOrgMappedNamedAccounts()
		{
			var org = Factory.New<OrgHeader>();
			var can1 = Factory.New<OrgCarrierNamedAccount>();
			can1.ONA_OH_Organization = org.PK;
			can1.ONA_ForeignName = "Test";

			AssertEquals(1, org.MappedNamedAccounts.Count);
			AssertEquals("Test", org.MappedNamedAccounts[0].ONA_ForeignName);
		}

		#endregion

		#region TestOrgWhsClientAccountAssociations

		public void TestOrgWhsClientAccountAssociations()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierAccount = carrier.CarrierAccounts.AddNew();
			var clientAccountAssociation = Factory.New<OrgWhsClientAccountAssociation>();
			clientAccountAssociation.OWC_OAN_CarrierAccount = carrierAccount.PK;
			clientAccountAssociation.OWC_OH_Client = client.PK;
			AssertEquals(1, client.OrgWhsClientAccountAssociations.Count);
			AssertEquals(carrierAccount.PK, client.OrgWhsClientAccountAssociations[0].OWC_OAN_CarrierAccount);
		}

		#endregion

		#region TestOrgAirlineMAWBStockManagementCollection()

		public void TestOrgAirlineMAWBStockManagementCollection()
		{
			var org = Factory.New<OrgHeader>();
			Assert("Precondition: OH_IsAirLine", !org.OH_IsAirLine);
			Assert("Precondition: OH_IsAirWholesaler", !org.OH_IsAirWholesaler);
			Assert("Collection should be read-only", org.OrgAirlineMAWBStockManagementCollection.ReadOnly);

			org.OH_IsAirLine = true;
			Assert("Collection is read/write for Airline", !org.OrgAirlineMAWBStockManagementCollection.ReadOnly);

			var item = org.OrgAirlineMAWBStockManagementCollection.AddNew();
			item.OHM_GB_Branch = GlbBranch.CurrentBranch.PK;
			item.OHM_MAWBStockThreshold = 100;

			org.OH_IsAirWholesaler = true;
			org.OH_IsAirLine = false;
			Assert("Collection is read/write for Air Wholesaler", !org.OrgAirlineMAWBStockManagementCollection.ReadOnly);
			AssertCollectionContains(item, org.OrgAirlineMAWBStockManagementCollection);

			org.OH_IsAirWholesaler = false;
			Assert("Collection is read-only", org.OrgAirlineMAWBStockManagementCollection.ReadOnly);
			AssertEquals("Collection is empty", 0, org.OrgAirlineMAWBStockManagementCollection.Count);
			Assert("Item is deleted", item.IsDeleted);
		}

		#endregion

		#region Denied Party Screening

		public void TestEventLogAddedWhenModified()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var result = org.GetLogs().Find(query);
			AssertEquals(false, result.Any());

			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Code = "DLV: LOMANDRA DRIVE";
			orgAddress.OA_Address1 = "LOMANDRA DRIVE";
			Factory.Save();
			result = org.GetLogs().Find(query);
			var eventLog = result.Single();
			AssertEquals("New Status: Unknown, Old Status: Clear, Type: Manual Screen", eventLog.DisplayEventReference);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			orgAddress.OA_Code = "94-95 LOMANDRA DRIVE";
			orgAddress.OA_Address1 = "94-95 LOMANDRA DRIVE";
			Factory.Save();
			result = org.GetLogs().Find(query).Except(eventLog).ToArray();
			AssertEquals("New Status: Unknown, Old Status: Clear, Type: Manual Screen", result.Single().DisplayEventReference);
		}

		public void TestInvalidateByLocalDataChanges_ShouldInvalidateScreeningStatus()
		{
			var logCount = 0;
			var header = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			CombineAssertions("Should invalidate screening status and create screening log", () =>
			{
				AssertHasInvalidatedScreeningStatuses(() =>
				{
					header.OH_FullName = "Updated ORG Fullname";
					header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				});
				AssertHasInvalidatedScreeningStatuses(() =>
				{
					header.OH_RL_NKClosestPort = "MDKIV";
					header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				});
			});

			void AssertHasInvalidatedScreeningStatuses(Action dataChange)
			{
				logCount++;
				dataChange.Invoke();
				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges, header.ScreeningLogCollection[logCount - 1].PJ_Status);
				AssertEquals(logCount, header.ScreeningLogCollection.Count);
			}
		}

		public void TestShouldUpdateRelatedJobs()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.Matched, orgHeader.OH_ScreeningStatus);

			orgHeader.ShouldUpdateRelatedJobs = false;
			orgHeader.OH_FullName = "TEST";
			Factory.Save();
			AssertEquals("org.ShouldUpdateRelatedJobs:", true, orgHeader.ShouldUpdateRelatedJobs);

			orgHeader.ShouldUpdateRelatedJobs = false;
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			OrgAddress orgAddress = orgHeader.Addresses[0];
			orgAddress.OA_Address1 = "TEST ADDRESS 123";
			Factory.Save();
			AssertEquals("org.ShouldUpdateRelatedJobs:", true, orgHeader.ShouldUpdateRelatedJobs);
		}

		public void TestShouldUpdateRelatedJobsIsFalseWhenCurrentStatusIsCLP()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.PermanentClear, orgHeader.OH_ScreeningStatus);

			orgHeader.ShouldUpdateRelatedJobs = false;
			orgHeader.OH_FullName = "TEST";
			Factory.Save();
			AssertEquals("org.ShouldUpdateRelatedJobs:", false, orgHeader.ShouldUpdateRelatedJobs);
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.PermanentClear, orgHeader.OH_ScreeningStatus);

			var orgAddress = orgHeader.Addresses[0];
			orgAddress.OA_Address1 = "TEST ADDRESS 123";
			Factory.Save();
			AssertEquals("org.ShouldUpdateRelatedJobs:", false, orgHeader.ShouldUpdateRelatedJobs);
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.PermanentClear, orgHeader.OH_ScreeningStatus);
		}

		public void TestDeactivateOrgChangeScreeningStatusToUNK()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.Matched, orgHeader.OH_ScreeningStatus);

			orgHeader.OH_IsActive = false;
			Factory.Save();
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.Unknown, orgHeader.OH_ScreeningStatus);
		}

		public void TestDeactivateOrgNotChangeScreeningStatusToUNKWhenCurrentStatusIsCLP()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.PermanentClear, orgHeader.OH_ScreeningStatus);

			orgHeader.OH_IsActive = false;
			Factory.Save();
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.PermanentClear, orgHeader.OH_ScreeningStatus);
		}

		public void TestInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus()
		{
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.Matched, false, true);
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.Matched, true, false);
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.PermanentClear, false, false);
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.NotScreened, false, false);
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.Unknown, false, false);
		}

		void AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(string screeningStatus, bool registryValue, bool expectedValue)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_ScreeningStatus = screeningStatus;

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertEquals("Precondition: ", false, orgHeader.ShouldUpdateRelatedJobs);
				orgHeader.InvalidateScreeningStatuses();
				AssertEquals(expectedValue, orgHeader.ShouldUpdateRelatedJobs);
			}
		}

		public void TestInvalidateScreeningStatusesNotChangeStatusWhenCurrentStatusIsCLP()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			orgHeader.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, orgHeader.OH_ScreeningStatus);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			orgHeader.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, orgHeader.OH_ScreeningStatus);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			orgHeader.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, orgHeader.OH_ScreeningStatus);
		}

		public void TestScreeningStatusCangRecognizeThePortChangeForGlobalAccount()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsGlobalAccount = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "NZAKL";

			AssertEquals("PRE: orgHeader.OH_ScreeningStatus = NOT:", ScreeningStatusesList.Codes.NotScreened, orgHeader.OH_ScreeningStatus);
			Factory.Save();
			AssertEquals("OH_ScreeningStatus", ScreeningStatusesList.Codes.NotScreened, orgHeader.OH_ScreeningStatus);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			AssertEquals("orgHeader.OH_ScreeningStatus:", ScreeningStatusesList.Codes.Clear, orgHeader.OH_ScreeningStatus);
		}

		#endregion

		#region OrgRelatedParties

		public void TestAllRelatedPartiesDoesntLoadForDifferentCompany()
		{
			OrgHeader relatedParty = Factory.New<OrgHeader>();
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);

			GlbCompany newCompany = Factory.New<GlbCompany>();

			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
			partyRecord.PR_OH_Parent = company.PK;
			partyRecord.PR_OH_RelatedParty = relatedParty.PK;
			partyRecord.PR_GC = newCompany.PK;

			AssertEquals("Only one related Party in the collection", 1, company.AllRelatedParties.Count);
		}

		public void TestRelatedParties()
		{
			OrgHeader relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "AAA";
			AssertEquals("Default DeliveryCustomsBillTo", company, company.DeliveryCustomsBillTo);
			AssertEquals("Default DeliveryFreightBillTo", company, company.DeliveryFreightBillTo);
			AssertEquals("Default DeliveryAttributeRevenueTo", company, company.DeliveryAttributeRevenueTo);
			AssertEquals("Default PickupCustomsBillTo", company, company.PickupCustomsBillTo);
			AssertEquals("Default PickupFreightBillTo", company, company.PickupFreightBillTo);
			AssertEquals("Default PickupAttributeRevenueTo", company, company.PickupAttributeRevenueTo);
			AssertEquals("Default ManagementGrouping", company, company.ManagementGrouping);
			AssertEquals("Default ARGrouping", company, company.ARGrouping);
			AssertEquals("Default APGrouping", company, company.APGrouping);
			AssertEquals("Default ForwarderGrouping", company, company.ForwarderGrouping);

			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, "AUMEL");
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Delivery);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Pickup);

			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ForwarderGroup, RelatedPartyDirectionList.Codes.Forwarder);

			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.SourceOfSalesLead, RelatedPartyDirectionList.Codes.Sales);
			company.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);

			company.SetRelatedParty(company.MainAddress.PK, relatedParty, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Sales, Constants.TransportModes.Air, ZString.Empty);

			company.ARSettlementGroupPK = relatedParty.PK;
			company.APSettlementGroupPK = relatedParty.PK;

			AssertEquals(relatedParty.PK, company.DeliveryAirCustomsBroker.PK);
			AssertEquals(relatedParty.PK, company.DeliverySeaCustomsBroker.PK);
			AssertEquals(relatedParty.PK, company.PickupAirCustomsBroker.PK);
			AssertEquals(relatedParty.PK, company.PickupSeaCustomsBroker.PK);
			AssertEquals(relatedParty.PK, company.DeliveryCustomsBillTo.PK);
			AssertEquals(relatedParty.PK, company.DeliveryFreightBillTo.PK);
			AssertEquals(relatedParty.PK, company.DeliveryAttributeRevenueTo.PK);
			AssertEquals(relatedParty.PK, company.PickupCustomsBillTo.PK);
			AssertEquals(relatedParty.PK, company.PickupFreightBillTo.PK);
			AssertEquals(relatedParty.PK, company.PickupAttributeRevenueTo.PK);
			AssertEquals(relatedParty.PK, company.ManagementGrouping.PK);
			AssertEquals(relatedParty.PK, company.ARGrouping.PK);
			AssertEquals(relatedParty.PK, company.APGrouping.PK);

			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.SourceOfSalesLead, RelatedPartyDirectionList.Codes.Sales).PK);
			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales).PK);

			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder).PK);
			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder).PK);
			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder).PK);
			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty).PK);
			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty).PK);
			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderGroup, RelatedPartyDirectionList.Codes.Forwarder).PK);
			AssertEquals(relatedParty.PK, company.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, "AUMEL").PK);

			AssertEquals(relatedParty.PK, company.GetRelatedParty(company.MainAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Sales, Constants.TransportModes.Air, ZString.Empty).PK);

			Factory.Save();

			AssertEquals(relatedParty, company.APSettlementGroup);
			AssertEquals(relatedParty, company.ARSettlementGroup);

			company.ARSettlementGroupPK = ZGuid.Empty;
			company.APSettlementGroupPK = ZGuid.Empty;

			AssertNull(company.APSettlementGroup);
			AssertNull(company.ARSettlementGroup);

			var party = company.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty);
			party.OH_IsActive = false;
			AssertEquals(party, company.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty));

			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.CustomsAgentBroker).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.InvoiceFreightJobsTo).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.ReportRevenueTo).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.ManagementGrouping).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.ARNettingGroup).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.APNettingGroup).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.ForwarderCFS).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.ForwarderGroup).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.SourceOfSalesLead).First().PR_OH_Parent);
			AssertEquals(company.PK, relatedParty.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.ControllingAgent).First().PR_OH_Parent);
		}

		public void TestServiceRelatedParties() => CombineAssertions(() =>
		{
			var org = Factory.New<OrgHeader>();
			var relatedParties = org.ServiceRelatedParties;
			AssertType<ServiceOrgRelatedPartySubsetCollection>("Type", relatedParties);
			AssertEquals("ChildEditable", true, org.IsRegisteredEditableChildObject(relatedParties));
		});

		public void TestGetRelatedBillToParty()
		{
			var org = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var pickupIFT = Factory.New<OrgHeader>();
			var deliveryIFT = Factory.New<OrgHeader>();
			var pickupICT = Factory.New<OrgHeader>();
			var deliveryICT = Factory.New<OrgHeader>();
			var warehouseBillTo = Factory.New<OrgHeader>();

			org.SetRelatedParty(pickupIFT, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			org.SetRelatedParty(deliveryIFT, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			org.SetRelatedParty(pickupICT, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			org.SetRelatedParty(deliveryICT, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			org.SetRelatedParty(warehouseBillTo, RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, string.Empty, Constants.TransportModes.All, ZString.Empty);

			AssertEquals(org2.PK, org2.GetRelatedBillToParty(null, false).PK);
			AssertEquals(org2.PK, org2.GetRelatedBillToParty(null, true).PK);

			AssertEquals(pickupIFT.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Shipment, false).PK);
			AssertEquals(deliveryIFT.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Shipment, true).PK);
			AssertEquals(pickupIFT.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.QuotedBooking, false).PK);
			AssertEquals(deliveryIFT.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.QuotedBooking, true).PK);
			AssertEquals(pickupICT.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Brokerage, false).PK);
			AssertEquals(deliveryICT.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Brokerage, true).PK);
			AssertEquals(warehouseBillTo.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob, null).PK);
			AssertEquals(warehouseBillTo.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseInwards, null).PK);
			AssertEquals(warehouseBillTo.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseOutwards, null).PK);
			AssertEquals(warehouseBillTo.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseStocktake, null).PK);
			AssertEquals(warehouseBillTo.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseStorage, null).PK);

			var depot = Factory.New<OrgHeader>();
			using (CFSDataRegistry.Instance.DepotCashSalesOrg.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, depot.PK.ToGuid()))
			{
				AssertEquals(depot.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.CFSShipment, true).PK);
				AssertEquals(org.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.CFSShipment, false).PK);
			}
		}

		public void TestGetRelatedBillToPartyByTransportAndContainerMode()
		{
			var org = Factory.New<OrgHeader>();

			OrgHeader MakeRelatedParty(ZString partyType, ZString transportMode = default, ZString containerMode = default)
			{
				OrgHeader rp = Factory.New<OrgHeader>();
				org.SetRelatedParty(rp, partyType, partyType == RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo ? ZString.Empty : (ZString)RelatedPartyDirectionList.Codes.Pickup, transportMode, containerMode);
				return rp;
			}

			var invoiceFreightAll = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, Constants.TransportModes.All);
			var invoiceFreightSea = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, Constants.TransportModes.Sea);
			var invoiceFreightSeaFcl = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			var invoiceFreightAir = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, Constants.TransportModes.Air);
			var invoiceFreightAirUld = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, Constants.TransportModes.Air, Constants.ContainerModes.ULD);

			var invoiceCustomsAll = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, Constants.TransportModes.All);
			var invoiceCustomsSea = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, Constants.TransportModes.Sea);
			var invoiceCustomsSeaFcl = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			var invoiceCustomsAir = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, Constants.TransportModes.Air);
			var invoiceCustomsAirUld = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, Constants.TransportModes.Air, Constants.ContainerModes.ULD);

			var invoiceWarehouseAll = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, Constants.TransportModes.All);
			var invoiceWarehouseSea = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, Constants.TransportModes.Sea);
			var invoiceWarehouseSeaFcl = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			var invoiceWarehouseAir = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, Constants.TransportModes.Air);
			var invoiceWarehouseAirUld = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, Constants.TransportModes.Air, Constants.ContainerModes.ULD);

			var invoiceFreightOther = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, Constants.TransportModes.Other, ZString.Empty);
			var invoiceCustomsOther = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, Constants.TransportModes.Other, ZString.Empty);
			var invoiceWarehouseOther = MakeRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, Constants.TransportModes.Other, ZString.Empty);

			AssertEquals(invoiceFreightAll.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Shipment, false, Constants.TransportModes.All, ZString.Empty).PK);
			AssertEquals(invoiceFreightSea.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Shipment, false, Constants.TransportModes.Sea).PK);
			AssertEquals(invoiceFreightSeaFcl.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Shipment, false, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK);
			AssertEquals(invoiceFreightAir.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Shipment, false, Constants.TransportModes.Air).PK);
			AssertEquals(invoiceFreightAirUld.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Shipment, false, Constants.TransportModes.Air, Constants.ContainerModes.ULD).PK);

			AssertEquals(invoiceCustomsAll.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Brokerage, false, Constants.TransportModes.All, ZString.Empty).PK);
			AssertEquals(invoiceCustomsSea.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Brokerage, false, Constants.TransportModes.Sea).PK);
			AssertEquals(invoiceCustomsSeaFcl.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Brokerage, false, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK);
			AssertEquals(invoiceCustomsAir.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Brokerage, false, Constants.TransportModes.Air).PK);
			AssertEquals(invoiceCustomsAirUld.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Brokerage, false, Constants.TransportModes.Air, Constants.ContainerModes.ULD).PK);

			AssertEquals(invoiceWarehouseAll.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseStorage, null, Constants.TransportModes.All, ZString.Empty).PK);
			AssertEquals(invoiceWarehouseSea.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseStorage, null, Constants.TransportModes.Sea).PK);
			AssertEquals(invoiceWarehouseSeaFcl.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseStorage, null, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK);
			AssertEquals(invoiceWarehouseAir.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseStorage, null, Constants.TransportModes.Air).PK);
			AssertEquals(invoiceWarehouseAirUld.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseStorage, null, Constants.TransportModes.Air, Constants.ContainerModes.ULD).PK);

			AssertEquals(invoiceFreightOther.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Shipment, false, Constants.TransportModes.PassengerHandCarried, ZString.Empty).PK);
			AssertEquals(invoiceCustomsOther.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.Brokerage, false, Constants.TransportModes.SeaAir, ZString.Empty).PK);
			AssertEquals(invoiceWarehouseOther.PK, org.GetRelatedBillToParty(JobInvoicingConsumerTypes.WarehouseStorage, null, Constants.TransportModes.AirSea, ZString.Empty).PK);
		}

		public void TestSetDefaultCartageProviders()
		{
			OrgHeader relatedParty1 = Factory.New<OrgHeader>();
			OrgHeader relatedParty2 = Factory.New<OrgHeader>();
			OrgHeader relatedParty3 = Factory.New<OrgHeader>();
			OrgHeader relatedParty4 = Factory.New<OrgHeader>();

			FreightDataRegistry.Instance.AIRCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, relatedParty1.PK.ToGuid());
			FreightDataRegistry.Instance.LCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, relatedParty2.PK.ToGuid());
			FreightDataRegistry.Instance.FCLCartageCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, relatedParty3.PK.ToGuid());
			FreightDataRegistry.Instance.FCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, relatedParty4.PK.ToGuid());

			OrgHeader org = Factory.New<OrgHeader>();

			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty));
			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty));
			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty));

			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL));
			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL));
			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.LCL));

			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL));
			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL));
			AssertNull(org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL));

			org.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			AssertEquals(relatedParty1.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty).PK);
			AssertEquals(relatedParty1.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty).PK);
			AssertEquals(relatedParty1.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty).PK);

			AssertEquals(relatedParty2.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL).PK);
			AssertEquals(relatedParty2.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL).PK);
			AssertEquals(relatedParty2.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.LCL).PK);

			AssertEquals(relatedParty4.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK);
			AssertEquals(relatedParty4.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK);
			AssertEquals(relatedParty4.PK, org.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK);
		}

		public void TestGetRelatedPartyWithAddressFallback()
		{
			AssertEquals(0, company.AllRelatedParties.Count);

			var orgWithAddress = Factory.New<OrgHeader>();
			var orgWithoutAddress = Factory.New<OrgHeader>();

			var addressPK = ZGuid.NewZGuid();
			company.SetRelatedParty(addressPK, orgWithAddress, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			company.SetRelatedParty(orgWithoutAddress, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);

			AssertEquals(2, company.AllRelatedParties.Count);

			var relatedParty = company.GetRelatedPartyWithAddressFallback(addressPK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty, ZString.Empty);
			AssertEquals("Related parties with addresses should be prioritised, even when the no-address fallback is a better match.", orgWithAddress.PK, relatedParty.PK);

			relatedParty = company.GetRelatedPartyWithAddressFallback(ZGuid.NewZGuid(), RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("When no related party matching the address criteria is found, those without addresses should be considered as fallbacks.", orgWithoutAddress.PK, relatedParty.PK);
		}

		#endregion

		#region ReadOnly Security

		public void TestStaffAssignmentsCollectionIsReadOnly()
		{
			bool oldStaffValue = Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed;

			Action<bool> setStaffAssignments = (isAllowed) =>
			{
				foreach (var pair in Env.Security.OrgDetailsModifyStaffAssignmentsLookup)
				{
					pair.Value.IsAllowed = isAllowed;
				}
			};

			setStaffAssignments(true);

			try
			{
				Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = true;
				setStaffAssignments(true);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.StaffAssignments.ReadOnly);

				Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = false;
				setStaffAssignments(true);
				ResetOrgInDB();
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.StaffAssignments.ReadOnly);

				Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = true;
				setStaffAssignments(false);
				ResetOrgInDB();
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.StaffAssignments.ReadOnly);

				Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = false;
				setStaffAssignments(false);
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.StaffAssignments.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = oldStaffValue;
				setStaffAssignments(true);
			}
		}

		public void TestSecurityRightsCollectionIsReadOnly()
		{
			bool oldSecurityRightsValue = Env.Security.OrgDetailsModifyWebSecurity.IsAllowed;
			bool oldNewSecurityRightsValue = Env.Security.OrgDetailsNewWebSecurity.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.SecurityRights.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.SecurityRights.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.SecurityRights.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.SecurityRights.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !Factory.NewWithValidTestData<OrgHeader>().SecurityRights.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", Factory.NewWithValidTestData<OrgHeader>().SecurityRights.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !Factory.NewWithValidTestData<OrgHeader>().SecurityRights.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", Factory.NewWithValidTestData<OrgHeader>().SecurityRights.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = oldSecurityRightsValue;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = oldNewSecurityRightsValue;
			}
		}

		public void TestOpportunityCollectionIsReadOnly()
		{
			bool oldOpportunityValue = Env.Security.OpportunityManagementEdit.IsAllowed;

			try
			{
				Env.Security.OpportunityManagementEdit.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.SalesOpportunities.ReadOnly);

				Env.Security.OpportunityManagementEdit.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.SalesOpportunities.ReadOnly);
			}
			finally
			{
				Env.Security.OpportunityManagementEdit.IsAllowed = oldOpportunityValue;
			}
		}

		public void TestBuyerLinksCollectionIsReadOnly()
		{
			bool oldCNRRelationshipsValue = Env.Security.OrgConsignorModifyRelationships.IsAllowed;

			try
			{
				Env.Security.OrgConsignorModifyRelationships.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.BuyerLinks.ReadOnly);

				Env.Security.OrgConsignorModifyRelationships.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.BuyerLinks.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConsignorModifyRelationships.IsAllowed = oldCNRRelationshipsValue;
			}
		}

		public void TestSupplierLinksCollectionIsReadOnly()
		{
			bool oldCNERelationshipsValue = Env.Security.OrgConsigneeModifyRelationships.IsAllowed;

			try
			{
				Env.Security.OrgConsigneeModifyRelationships.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.SupplierLinks.ReadOnly);

				Env.Security.OrgConsigneeModifyRelationships.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.SupplierLinks.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConsigneeModifyRelationships.IsAllowed = oldCNERelationshipsValue;
			}
		}

		public void TestLandedCostingCollectionIsReadOnly()
		{
			bool oldLandedCostingValue = Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed;

			try
			{
				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.LandedCostingPreferences.ReadOnly);

				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.LandedCostingPreferences.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = oldLandedCostingValue;
			}
		}

		public void TestAppointedAgentAndGatewayAgentPortsCollectionIsReadOnly()
		{
			bool oldAppointedAgentPortsValue = Env.Security.OrgForwarderModifyDetails.IsAllowed;

			try
			{
				Env.Security.OrgForwarderModifyDetails.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.AppointedAgentPorts.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.AppointedGatewayAgentPorts.ReadOnly);

				Env.Security.OrgForwarderModifyDetails.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.AppointedAgentPorts.ReadOnly);
				Assert("Access Disallowed - ReadOnly", OrgInDB.AppointedGatewayAgentPorts.ReadOnly);
			}
			finally
			{
				Env.Security.OrgForwarderModifyDetails.IsAllowed = oldAppointedAgentPortsValue;
			}
		}

		public void TestCarrierAppointedAgentPortsCollectionIsReadOnly()
		{
			bool oldCarrierAppointedAgentPortsValue = Env.Security.OrgCarrierModify.IsAllowed;

			try
			{
				Env.Security.OrgCarrierModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CarrierAppointedAgentPorts_AirCTO.ReadOnly);

				Env.Security.OrgCarrierModify.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.CarrierAppointedAgentPorts_AirCTO.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierAppointedAgentPortsValue;
			}
		}

		public void TestAgentProfitShareCollectionIsReadOnly()
		{
			bool oldForwarderValue = Env.Security.OrgForwarderModifyProfitShare.IsAllowed;

			try
			{
				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.AgentRelationships.ReadOnly);

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.AgentRelationships.ReadOnly);
			}
			finally
			{
				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = oldForwarderValue;
			}
		}

		public void TestCustomsCodesCollectionIsReadOnly()
		{
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
				var cusCodeCollection = CreateOrgCusCodeCollection();
				AssertEquals(true, cusCodeCollection.ReadOnly);

				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = true;
				cusCodeCollection = CreateOrgCusCodeCollection();
				AssertEquals(false, cusCodeCollection.ReadOnly);

				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = false;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				cusCodeCollection = CreateOrgCusCodeCollection();
				AssertEquals(false, cusCodeCollection.ReadOnly);

				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;
				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = true;
				cusCodeCollection = CreateOrgCusCodeCollection();
				AssertEquals(false, cusCodeCollection.ReadOnly);

				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = false;
				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				cusCodeCollection = CreateOrgCusCodeCollection();
				AssertEquals(false, cusCodeCollection.ReadOnly);

				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = false;
				cusCodeCollection = CreateOrgCusCodeCollection();
				AssertEquals(true, cusCodeCollection.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = oldSecurityModifyFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyFinancialNonARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialNonARAPRegistrationNumbers;
			}

			OrgCusCodeCollection CreateOrgCusCodeCollection()
			{
				var org = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
				return org.CustomsCodes;
			}
		}

		public void TestEDICodeMappingCollectionIsReadOnly()
		{
			bool oldCodeMappingValue = Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed;

			try
			{
				Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.PatternMatchOverrides_ForBinding.ReadOnly);

				Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.PatternMatchOverrides_ForBinding.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = oldCodeMappingValue;
			}
		}

		public void TestBrandsAndCompanyNamesCollectionIsReadOnly()
		{
			bool oldBrandsValue = Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed;

			try
			{
				Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.BrandsOrRelatedNames.ReadOnly);

				Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.BrandsOrRelatedNames.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed = oldBrandsValue;
			}
		}

		public void TestCusomLabelsCollectionsAreReadOnly()
		{
			bool oldCustomValue = Env.Security.OrgCustomModify.IsAllowed;

			try
			{
				Env.Security.OrgCustomModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CustomDocumentLabels.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CustomFormLabels.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CustomLabels.ReadOnly);

				Env.Security.OrgCustomModify.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.CustomDocumentLabels.ReadOnly);
				Assert("Access Disallowed - ReadOnly", OrgInDB.CustomFormLabels.ReadOnly);
				Assert("Access Disallowed - ReadOnly", OrgInDB.CustomLabels.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCustomModify.IsAllowed = oldCustomValue;
			}
		}

		public void TestChangingARFlagForNewOrg()
		{
			bool oldTempFlagValue = Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed;
			bool oldARFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed;
			bool oldARTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed;

			try
			{
				// From Temp to Non-Temp
				company.OH_IsTempAccount = ZBool.True;
				company.CompanyData.OB_IsDebtor = ZBool.True;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = true;
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertEquals("AR flag is true", ZBool.True, company.CompanyData.OB_IsDebtor);

				company.OH_IsTempAccount = ZBool.True;
				company.CompanyData.OB_IsDebtor = ZBool.True;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = false;
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertEquals("AR flag is false", ZBool.False, company.CompanyData.OB_IsDebtor);

				// From Non-Temp to Temp
				company.OH_IsTempAccount = ZBool.False;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = true;
				company.CompanyData.OB_IsDebtor = ZBool.True;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = true;
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertEquals("AR flag is true", ZBool.True, company.CompanyData.OB_IsDebtor);

				company.OH_IsTempAccount = ZBool.False;
				company.CompanyData.OB_IsDebtor = ZBool.True;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = false;
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertEquals("AR flag is false", ZBool.False, company.CompanyData.OB_IsDebtor);
			}
			finally
			{
				Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed = oldTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = oldARFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = oldARTempFlagValue;
			}
		}

		public void TestChangingAPFlagForNewOrg()
		{
			bool oldTempFlagValue = Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed;
			bool oldAPFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed;
			bool oldAPTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed;

			try
			{
				// From Temp to Non-Temp
				company.OH_IsTempAccount = ZBool.True;
				company.CompanyData.OB_IsCreditor = ZBool.True;
				Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = true;
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertEquals("AP flag is true", ZBool.True, company.CompanyData.OB_IsCreditor);

				company.OH_IsTempAccount = ZBool.True;
				company.CompanyData.OB_IsCreditor = ZBool.True;
				Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = false;
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertEquals("AP flag is false", ZBool.False, company.CompanyData.OB_IsCreditor);

				// From Non-Temp to Temp
				company.OH_IsTempAccount = ZBool.False;
				Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = true;
				company.CompanyData.OB_IsCreditor = ZBool.True;
				Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = true;
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertEquals("AP flag is true", ZBool.True, company.CompanyData.OB_IsCreditor);

				company.OH_IsTempAccount = ZBool.False;
				company.CompanyData.OB_IsCreditor = ZBool.True;
				Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = false;
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertEquals("AP flag is false", ZBool.False, company.CompanyData.OB_IsCreditor);
			}
			finally
			{
				Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed = oldTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = oldAPFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = oldAPTempFlagValue;
			}
		}

		public void TestChangingAirCarrierFlag()
		{
			bool oldFlagValue = Env.Security.OrgCarrierModifyAir.IsAllowed;
			OrgInDB.OH_IsAirLine = false;
			OrgInDB.OH_IsAirWholesaler = false;

			try
			{
				Env.Security.OrgCarrierModifyAir.IsAllowed = false;
				OrgInDB.OH_IsAirLine = true;
				CombineAssertions(() =>
				{
					AssertEquals("OH_IsAirLine should be read only", ZBool.True, OrgInDB.OH_IsAirLineInfo.ReadOnly);
					AssertEquals("OH_IsAirWholesaler should be read only", ZBool.True, OrgInDB.OH_IsAirWholesalerInfo.ReadOnly);
					AssertEquals("OrgAirlineMAWBStockManagementCollection should be read only", ZBool.True, OrgInDB.OrgAirlineMAWBStockManagementCollection.ReadOnly);
					AssertEquals("OrgAirlineBranchAccounts should be read only", ZBool.True, OrgInDB.OrgAirlineBranchAccounts.ReadOnly);
				});

				Env.Security.OrgCarrierModifyAir.IsAllowed = true;
				OrgInDB.OH_IsAirWholesaler = true;
				CombineAssertions(() =>
				{
					AssertEquals("OH_IsAirLine should not be read only", ZBool.False, OrgInDB.OH_IsAirLineInfo.ReadOnly);
					AssertEquals("OH_IsAirWholesaler should not be read only", ZBool.False, OrgInDB.OH_IsAirWholesalerInfo.ReadOnly);
					AssertEquals("OrgAirlineMAWBStockManagementCollection should not be read only", ZBool.False, OrgInDB.OrgAirlineMAWBStockManagementCollection.ReadOnly);
					AssertEquals("OrgAirlineBranchAccounts should not be read only", ZBool.False, OrgInDB.OrgAirlineBranchAccounts.ReadOnly);
				});
			}
			finally
			{
				Env.Security.OrgCarrierModifyAir.IsAllowed = oldFlagValue;
			}
		}

		public void TestChangingSeaCarrierFlag()
		{
			bool oldFlagValue = Env.Security.OrgCarrierModifySea.IsAllowed;
			OrgInDB.OH_IsShippingLine = false;
			OrgInDB.OH_IsSeaWholesaler = false;

			try
			{
				Env.Security.OrgCarrierModifySea.IsAllowed = false;
				OrgInDB.OH_IsShippingLine = true;
				CombineAssertions(() =>
				{
					AssertEquals("OH_IsShippingLine should be read only", ZBool.True, OrgInDB.OH_IsShippingLineInfo.ReadOnly);
					AssertEquals("OH_IsSeaWholesaler should be read only", ZBool.True, OrgInDB.OH_IsSeaWholesalerInfo.ReadOnly);
					AssertEquals("OH_IsShippingConsortium should be read only", ZBool.True, OrgInDB.OH_IsShippingConsortiumInfo.ReadOnly);
					AssertEquals("OH_RSL_ShippingLine should be read only", ZBool.True, OrgInDB.OH_RSL_ShippingLineInfo.ReadOnly);
				});

				Env.Security.OrgCarrierModifySea.IsAllowed = true;
				OrgInDB.OH_IsSeaWholesaler = true;
				CombineAssertions(() =>
				{
					AssertEquals("OH_IsShippingLine should not be read only", ZBool.False, OrgInDB.OH_IsShippingLineInfo.ReadOnly);
					AssertEquals("OH_IsSeaWholesaler should not be read only", ZBool.False, OrgInDB.OH_IsSeaWholesalerInfo.ReadOnly);
					AssertEquals("OH_IsShippingConsortium should not be read only", ZBool.False, OrgInDB.OH_IsShippingConsortiumInfo.ReadOnly);
					AssertEquals("OH_RSL_ShippingLine should not be read only", ZBool.False, OrgInDB.OH_RSL_ShippingLineInfo.ReadOnly);
				});
			}
			finally
			{
				Env.Security.OrgCarrierModifySea.IsAllowed = oldFlagValue;
			}
		}

		public void TestChangingLandCarrierFlag()
		{
			bool oldFlagValue = Env.Security.OrgCarrierModifyLand.IsAllowed;

			try
			{
				Env.Security.OrgCarrierModifyLand.IsAllowed = false;
				CombineAssertions(() =>
				{
					AssertEquals("OH_IsLocalTransport should be read only", ZBool.True, OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
					AssertEquals("OH_IsLineHaulProvider should be read only", ZBool.True, OrgInDB.OH_IsLineHaulProviderInfo.ReadOnly);
					AssertEquals("OH_IsRailProvider should be read only", ZBool.True, OrgInDB.OH_IsRailProviderInfo.ReadOnly);
					AssertEquals("OH_IsInlandWaterwayProvider should be read only", ZBool.True, OrgInDB.OH_IsInlandWaterwayProviderInfo.ReadOnly);
				});

				Env.Security.OrgCarrierModifyLand.IsAllowed = true;
				CombineAssertions(() =>
				{
					AssertEquals("OH_IsLocalTransport should not be read only", ZBool.False, OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
					AssertEquals("OH_IsLineHaulProvider should not be read only", ZBool.False, OrgInDB.OH_IsLineHaulProviderInfo.ReadOnly);
					AssertEquals("OH_IsRailProvider should not be read only", ZBool.False, OrgInDB.OH_IsRailProviderInfo.ReadOnly);
					AssertEquals("OH_IsInlandWaterwayProvider should not be read only", ZBool.False, OrgInDB.OH_IsInlandWaterwayProviderInfo.ReadOnly);
				});
			}
			finally
			{
				Env.Security.OrgCarrierModifyLand.IsAllowed = oldFlagValue;
			}
		}

		public void TestChangingFlagsForNewOrg()
		{
			bool oldTempFlagValue = Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed;
			bool oldSPFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagSP.IsAllowed;
			bool oldSPTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempSPFlag.IsAllowed;
			bool oldConFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed;
			bool oldConTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempConFlag.IsAllowed;
			bool oldTCFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagTC.IsAllowed;
			bool oldTCTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempTCFlag.IsAllowed;
			bool oldWHFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagWH.IsAllowed;
			bool oldWHTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempWHFlag.IsAllowed;
			bool oldCrrFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed;
			bool oldFAFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagFA.IsAllowed;
			bool oldFATempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempFAFlag.IsAllowed;
			bool oldBRFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed;
			bool oldBRTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempBRFlag.IsAllowed;
			bool oldSVFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed;
			bool oldSVTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempSVFlag.IsAllowed;
			bool oldCMFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagCM.IsAllowed;
			bool oldCMTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempCMFlag.IsAllowed;
			bool oldSalFlagValue = Env.Security.OrgDetailsNewOrgTypeFlagSal.IsAllowed;
			bool oldSalTempFlagValue = Env.Security.OrgDetailsNewOrgTypeTempSalFlag.IsAllowed;

			try
			{
				// From Temp to Non-Temp
				company.OH_IsTempAccount = ZBool.True;
				SetSecurityFlagsForNewOrg(true);
				SetOrgFlags(ZBool.True);
				company.OH_IsTempAccount = ZBool.False;
				AssertOrgFlags(ZBool.True);

				company.OH_IsTempAccount = ZBool.True;
				SetSecurityFlagsForNewOrg(false);
				SetOrgFlags(ZBool.True);
				company.OH_IsTempAccount = ZBool.False;
				AssertOrgFlags(ZBool.False);

				// From Non-Temp to Temp
				company.OH_IsTempAccount = ZBool.False;
				SetSecurityFlagsForNewOrg(true);
				SetSecurityFlagsForTempNewOrg(true);
				SetOrgFlags(ZBool.True);
				company.OH_IsTempAccount = ZBool.True;
				AssertOrgFlags(ZBool.True);

				company.OH_IsTempAccount = ZBool.False;
				SetSecurityFlagsForTempNewOrg(false);
				SetOrgFlags(ZBool.True);
				company.OH_IsTempAccount = ZBool.True;
				AssertOrgFlags(ZBool.False);
			}
			finally
			{
				Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed = oldTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagSP.IsAllowed = oldSPFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempSPFlag.IsAllowed = oldSPTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed = oldConFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempConFlag.IsAllowed = oldConTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagTC.IsAllowed = oldTCFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempTCFlag.IsAllowed = oldTCTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagWH.IsAllowed = oldWHFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempWHFlag.IsAllowed = oldWHTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed = oldCrrFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagFA.IsAllowed = oldFAFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempFAFlag.IsAllowed = oldFATempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed = oldBRFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempBRFlag.IsAllowed = oldBRTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = oldSVFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempSVFlag.IsAllowed = oldSVTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagCM.IsAllowed = oldCMFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempCMFlag.IsAllowed = oldCMTempFlagValue;
				Env.Security.OrgDetailsNewOrgTypeFlagSal.IsAllowed = oldSalFlagValue;
				Env.Security.OrgDetailsNewOrgTypeTempSalFlag.IsAllowed = oldSalTempFlagValue;
			}
		}

		void SetSecurityFlagsForNewOrg(ZBool flag)
		{
			Env.Security.OrgDetailsNewOrgTypeFlagSP.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagTC.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagWH.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagFA.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagCM.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeFlagSal.IsAllowed = flag;
		}

		void SetSecurityFlagsForTempNewOrg(ZBool flag)
		{
			Env.Security.OrgDetailsNewOrgTypeTempSPFlag.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeTempConFlag.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeTempTCFlag.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeTempWHFlag.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeTempFAFlag.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeTempBRFlag.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeTempSVFlag.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeTempCMFlag.IsAllowed = flag;
			Env.Security.OrgDetailsNewOrgTypeTempSalFlag.IsAllowed = flag;
		}

		void SetOrgFlags(ZBool flag)
		{
			company.OH_IsConsignor = flag;
			company.OH_IsConsignee = flag;
			company.OH_IsTransportClient = flag;
			company.OH_IsWarehouseClient = flag;
			company.OH_IsShippingProvider = flag;
			company.OH_IsForwarder = flag;
			company.OH_IsBroker = flag;
			company.OH_IsMiscFreightServices = flag;
			company.OH_IsCompetitor = flag;
			company.OH_IsSalesLead = flag;
		}

		void AssertOrgFlags(ZBool flag)
		{
			AssertEquals("Shipper flag", flag, company.OH_IsConsignor);
			AssertEquals("Consignee flag", flag, company.OH_IsConsignee);
			AssertEquals("Transport Client flag", flag, company.OH_IsTransportClient);
			AssertEquals("Warehouse Client flag", flag, company.OH_IsWarehouseClient);
			AssertEquals("Forwarder flag", flag, company.OH_IsForwarder);
			AssertEquals("Broker flag", flag, company.OH_IsBroker);
			AssertEquals("Services flag", flag, company.OH_IsMiscFreightServices);
			AssertEquals("Competitor flag", flag, company.OH_IsCompetitor);
			AssertEquals("Sales flag", flag, company.OH_IsSalesLead);
		}

		public void TestChangingARFlagForExistingOrg()
		{
			bool oldTempFlagValue = Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed;
			bool oldARFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed;
			bool oldARTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed;

			try
			{
				// From Temp to Non-Temp
				company.OH_IsTempAccount = ZBool.True;
				company.CompanyData.OB_IsDebtor = ZBool.True;
				company.Factory.Save();
				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = true;
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertEquals("AR flag is true", ZBool.True, company.CompanyData.OB_IsDebtor);

				company.OH_IsTempAccount = ZBool.True;
				company.CompanyData.OB_IsDebtor = ZBool.True;
				company.Factory.Save();
				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = false;
				company.CompanyData.OB_IsDebtor = ZBool.False;
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertEquals("AR flag is false", ZBool.True, company.CompanyData.OB_IsDebtor);

				// From Non-Temp to Temp
				company.OH_IsTempAccount = ZBool.False;
				company.CompanyData.OB_IsDebtor = ZBool.True;
				company.Factory.Save();
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = true;
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertEquals("AR flag is true", ZBool.True, company.CompanyData.OB_IsDebtor);

				company.OH_IsTempAccount = ZBool.False;
				company.CompanyData.OB_IsDebtor = ZBool.True;
				company.Factory.Save();
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = false;
				company.CompanyData.OB_IsDebtor = ZBool.False;
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertEquals("AR flag is false", ZBool.True, company.CompanyData.OB_IsDebtor);
			}
			finally
			{
				Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed = oldTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = oldARFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = oldARTempFlagValue;
			}
		}

		public void TestChangingAPFlagForExistingOrg()
		{
			bool oldTempFlagValue = Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed;
			bool oldAPFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed;
			bool oldAPTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed;

			try
			{
				// From Temp to Non-Temp
				company.OH_IsTempAccount = ZBool.True;
				company.CompanyData.OB_IsCreditor = ZBool.True;
				company.Factory.Save();
				Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = true;
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertEquals("AP flag is true", ZBool.True, company.CompanyData.OB_IsCreditor);

				company.OH_IsTempAccount = ZBool.True;
				company.CompanyData.OB_IsCreditor = ZBool.True;
				company.Factory.Save();
				company.CompanyData.OB_IsCreditor = ZBool.False;
				Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = false;
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertEquals("AP flag is false", ZBool.True, company.CompanyData.OB_IsCreditor);

				// From Non-Temp to Temp
				company.OH_IsTempAccount = ZBool.False;
				company.CompanyData.OB_IsCreditor = ZBool.True;
				company.Factory.Save();
				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = true;
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertEquals("AP flag is true", ZBool.True, company.CompanyData.OB_IsCreditor);

				company.OH_IsTempAccount = ZBool.False;
				company.CompanyData.OB_IsCreditor = ZBool.True;
				company.Factory.Save();
				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = false;
				company.CompanyData.OB_IsCreditor = ZBool.False;
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertEquals("AP flag is false", ZBool.True, company.CompanyData.OB_IsCreditor);
			}
			finally
			{
				Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed = oldTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = oldAPFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = oldAPTempFlagValue;
			}
		}

		public void TestChangingFlagsForExistingOrg()
		{
			bool oldTempFlagValue = Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed;
			bool oldSPFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagSP.IsAllowed;
			bool oldSPTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempSPFlag.IsAllowed;
			bool oldConFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagCon.IsAllowed;
			bool oldConTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempConFlag.IsAllowed;
			bool oldTCFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagTC.IsAllowed;
			bool oldTCTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTemlTCFlag.IsAllowed;
			bool oldWHFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagWH.IsAllowed;
			bool oldWHTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempWHFlag.IsAllowed;
			bool oldCrrFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagCrr.IsAllowed;
			bool oldFAFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagFA.IsAllowed;
			bool oldFATempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempFAFlag.IsAllowed;
			bool oldBRFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagBR.IsAllowed;
			bool oldBRTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempBRFlag.IsAllowed;
			bool oldSVFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagSV.IsAllowed;
			bool oldSVTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempSVFlag.IsAllowed;
			bool oldCMFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagCM.IsAllowed;
			bool oldCMTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempCMFlag.IsAllowed;
			bool oldSalFlagValue = Env.Security.OrgDetailsModifyOrgTypeFlagSal.IsAllowed;
			bool oldSalTempFlagValue = Env.Security.OrgDetailsModifyOrgTypeTempSalFlag.IsAllowed;
			try
			{
				// From Temp to Non-Temp
				company.OH_IsTempAccount = ZBool.True;
				SetSecurityFlagsForExistingOrg(ZBool.True);
				SetOrgFlags(ZBool.True);
				company.Factory.Save();
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertOrgFlags(ZBool.True);

				company.OH_IsTempAccount = ZBool.True;
				SetOrgFlags(ZBool.False);
				company.Factory.Save();
				SetSecurityFlagsForExistingOrg(ZBool.False);
				company.OH_IsTempAccount = ZBool.False; // Trigger
				AssertOrgFlags(ZBool.False);

				// From Non-Temp to Temp
				company.OH_IsTempAccount = ZBool.False;
				SetOrgFlags(ZBool.True);
				company.Factory.Save();
				SetSecurityFlagsForTempExistingOrg(ZBool.True);
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertOrgFlags(ZBool.True);

				company.OH_IsTempAccount = ZBool.True;
				SetOrgFlags(ZBool.False);
				company.Factory.Save();
				SetSecurityFlagsForTempExistingOrg(ZBool.False);
				company.OH_IsTempAccount = ZBool.True; // Trigger
				AssertOrgFlags(ZBool.False);
			}
			finally
			{
				Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed = oldTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagSP.IsAllowed = oldSPFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempSPFlag.IsAllowed = oldSPTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagCon.IsAllowed = oldConFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempConFlag.IsAllowed = oldConTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagTC.IsAllowed = oldTCFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTemlTCFlag.IsAllowed = oldTCTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagWH.IsAllowed = oldWHFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempWHFlag.IsAllowed = oldWHTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagCrr.IsAllowed = oldCrrFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagFA.IsAllowed = oldFAFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempFAFlag.IsAllowed = oldFATempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagBR.IsAllowed = oldBRFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempBRFlag.IsAllowed = oldBRTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagSV.IsAllowed = oldSVFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempSVFlag.IsAllowed = oldSVTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagCM.IsAllowed = oldCMFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempCMFlag.IsAllowed = oldCMTempFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagSal.IsAllowed = oldSalFlagValue;
				Env.Security.OrgDetailsModifyOrgTypeTempSalFlag.IsAllowed = oldSalTempFlagValue;
			}
		}

		void SetSecurityFlagsForExistingOrg(ZBool flag)
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagSP.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagCon.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagTC.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagWH.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagCrr.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagFA.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagBR.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagSV.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagCM.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeFlagSal.IsAllowed = flag;
		}

		void SetSecurityFlagsForTempExistingOrg(ZBool flag)
		{
			Env.Security.OrgDetailsModifyOrgTypeTempSPFlag.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeTempConFlag.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeTemlTCFlag.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeTempWHFlag.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeTempFAFlag.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeTempBRFlag.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeTempSVFlag.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeTempCMFlag.IsAllowed = flag;
			Env.Security.OrgDetailsModifyOrgTypeTempSalFlag.IsAllowed = flag;
		}

		public void TestReadOnlySecurity()
		{
			bool oldModifyCodeValue = Env.Security.OrgDetailsModifyCode.IsAllowed;
			bool oldModifyNameAndAddressValue = Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed;
			bool oldModifyPhoneFaxWebValue = Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed;
			bool oldModifyOrgTypeValue = Env.Security.OrgDetailsNewOrganisationType.IsAllowed;
			bool oldModifyTempOrgValue = Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed;
			bool oldNewTempOrgValue = Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed;
			bool oldModifyNationalOrgValue = Env.Security.OrgDetailsModifyIsNationalOrg.IsAllowed;
			bool oldModifyOrgDetailValue = Env.Security.OrgDetailsModify.IsAllowed;
			bool oldCarrierValue = Env.Security.OrgCarrierModify.IsAllowed;
			bool oldServicesValue = Env.Security.OrgServicesModify.IsAllowed;
			bool oldSalesValue = Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed;
			bool oldModifyGlobalValue = Env.Security.OrgDetailsModifyIsGlobalOrg.IsAllowed;
			bool oldReceivablesModifyInvoicingValue = Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed;
			bool oldPayablesModifyValue = Env.Security.OrgPayablesModify.IsAllowed;
			bool oldPayablesDefaultsModifyValue = Env.Security.OrgPayablesDefaultsModify.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyCode.IsAllowed = true;
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
				Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = true;
				Env.Security.OrgDetailsNewOrganisationType.IsAllowed = true;
				Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed = true;
				Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed = true;
				Env.Security.OrgDetailsModifyIsNationalOrg.IsAllowed = true;
				Env.Security.OrgDetailsModify.IsAllowed = true;
				Env.Registry.CanUserEditOrganisationCode = true;
				Env.Security.OrgCarrierModify.IsAllowed = true;
				Env.Security.OrgServicesModify.IsAllowed = true;
				Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed = true;
				Env.Security.OrgDetailsModifyIsGlobalOrg.IsAllowed = true;

				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_CodeInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_FullNameInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_RL_NKClosestPortInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_LanguageInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsigneeInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsignorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTransportClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsWarehouseClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsForwarderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsBrokerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsMiscFreightServicesInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsCompetitorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSalesLeadInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTempAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsNationalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_SystemCreateUserInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsInlandWaterwayProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLineHaulProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingConsortiumInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsPackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUnpackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRoadFreightDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsContainerYardInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsFumigationContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsVGMContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailHeadInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag1Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag2Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag3Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag4Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag5Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag6Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag7Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag8Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag9Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag10Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag11Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag12Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag13Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag14Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag15Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag16Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag17Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag18Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag19Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag20Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag21Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag22Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag23Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag24Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag25Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag26Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag27Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag28Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag29Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag30Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag31Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUserFlag32Info.ReadOnly);
				Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed = false;
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag1Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag2Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag3Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag4Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag5Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag6Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag7Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag8Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag9Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag10Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag11Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag12Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag13Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag14Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag15Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag16Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag17Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag18Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag19Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag20Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag21Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag22Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag23Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag24Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag25Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag26Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag27Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag28Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag29Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag30Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag31Info.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_IsUserFlag32Info.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_CodeInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_FullNameInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_RL_NKClosestPortInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_LanguageInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsigneeInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsignorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTransportClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsWarehouseClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsForwarderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsBrokerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsMiscFreightServicesInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsCompetitorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSalesLeadInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTempAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsNationalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_SystemCreateUserInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsInlandWaterwayProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLineHaulProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingConsortiumInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsPackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUnpackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRoadFreightDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsContainerYardInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsFumigationContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsVGMContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailHeadInfo.ReadOnly);

				Env.Security.OrgDetailsModifyCode.IsAllowed = false;
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_CodeInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_FullNameInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_RL_NKClosestPortInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_LanguageInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsigneeInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsignorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTransportClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsWarehouseClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsForwarderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsBrokerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsMiscFreightServicesInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsCompetitorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSalesLeadInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTempAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsNationalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_SystemCreateUserInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsInlandWaterwayProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLineHaulProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingConsortiumInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsPackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUnpackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRoadFreightDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsContainerYardInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsFumigationContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsVGMContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailHeadInfo.ReadOnly);

				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_FullNameInfo.ReadOnly);
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_RL_NKClosestPortInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_LanguageInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsigneeInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsignorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTransportClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsWarehouseClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsForwarderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsBrokerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsMiscFreightServicesInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsCompetitorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSalesLeadInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTempAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsNationalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_SystemCreateUserInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsInlandWaterwayProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLineHaulProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingConsortiumInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsPackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUnpackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRoadFreightDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsContainerYardInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsFumigationContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsVGMContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailHeadInfo.ReadOnly);

				Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = false;
				Assert("Access NOT Allowed - Reaonly", OrgInDB.OH_LanguageInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsigneeInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsConsignorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTransportClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsWarehouseClientInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsForwarderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsBrokerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsMiscFreightServicesInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsCompetitorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSalesLeadInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsTempAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsNationalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_SystemCreateUserInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsInlandWaterwayProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLineHaulProviderInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsShippingConsortiumInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsPackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsUnpackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsAirCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsSeaCTOInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRoadFreightDepotInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsContainerYardInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsFumigationContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsVGMContractorInfo.ReadOnly);
				Assert("Access Allowed - Not Reaonly", !OrgInDB.OH_IsRailHeadInfo.ReadOnly);

				Env.Security.OrgDetailsModifyOrganisationType.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsConsigneeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsConsignorInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsTransportClientInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsWarehouseClientInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsShippingProviderInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsForwarderInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsBrokerInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsMiscFreightServicesInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsCompetitorInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsSalesLeadInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsShippingLineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsRailProviderInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsInlandWaterwayProviderInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsAirWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsSeaWholesalerInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsLineHaulProviderInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsShippingConsortiumInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsLocalTransportInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsPackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsUnpackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsAirLineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsAirCTOInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsSeaCTOInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsRoadFreightDepotInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsContainerYardInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsFumigationContractorInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsVGMContractorInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsRailHeadInfo.ReadOnly);

				Env.Security.OrgCarrierModify.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsPackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsUnpackDepotInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsAirCTOInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsSeaCTOInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsRoadFreightDepotInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsContainerYardInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsFumigationContractorInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsVGMContractorInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsRailHeadInfo.ReadOnly);

				Env.Security.OrgServicesModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsPackDepotInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsUnpackDepotInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsAirCTOInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsSeaCTOInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsRoadFreightDepotInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsContainerYardInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsFumigationContractorInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsVGMContractorInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsRailHeadInfo.ReadOnly);

				Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsTempAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsNationalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_SystemCreateUserInfo.ReadOnly);

				Assert("Access Allowed - NOT ReadOnly", !company.OH_IsTempAccountInfo.ReadOnly); //Not in DB
				Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", company.OH_IsTempAccountInfo.ReadOnly);

				Env.Security.OrgDetailsModifyIsNationalOrg.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsNationalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_SystemCreateUserInfo.ReadOnly);

				Env.Security.OrgDetailsModifyIsGlobalOrg.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_IsGlobalAccountInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OH_SystemCreateUserInfo.ReadOnly);

				Env.Security.OrgDetailsModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.OH_SystemCreateUserInfo.ReadOnly);

				OrgHeader newOrg = Factory.New<OrgHeader>();
				newOrg.CompanyData.OB_ARCreditApproved = false;
				OrgInDB.CompanyData.OB_ARCreditApproved = false;
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = false;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = false;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);

				newOrg.CompanyData.OB_ARCreditApproved = true;
				OrgInDB.CompanyData.OB_ARCreditApproved = true;
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = false;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = false;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !newOrg.ARSettlementGroupPKInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !OrgInDB.ARSettlementGroupPKInfo.ReadOnly);

				Env.Security.OrgPayablesDefaultsModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.APSettlementGroupPKInfo.ReadOnly);
				Env.Security.OrgPayablesDefaultsModify.IsAllowed = true;
				Assert("Access Allowed - ReadOnly", !OrgInDB.APSettlementGroupPKInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyCode.IsAllowed = oldModifyCodeValue;
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = oldModifyNameAndAddressValue;
				Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = oldModifyPhoneFaxWebValue;
				Env.Security.OrgDetailsNewOrganisationType.IsAllowed = oldModifyOrgTypeValue;
				Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed = oldModifyTempOrgValue;
				Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed = oldNewTempOrgValue;
				Env.Security.OrgDetailsModifyIsNationalOrg.IsAllowed = oldModifyNationalOrgValue;
				Env.Security.OrgDetailsModifyIsGlobalOrg.IsAllowed = oldModifyGlobalValue;
				Env.Security.OrgDetailsModify.IsAllowed = oldModifyOrgDetailValue;
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierValue;
				Env.Security.OrgServicesModify.IsAllowed = oldServicesValue;
				Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed = oldSalesValue;
				Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = oldReceivablesModifyInvoicingValue;
				Env.Security.OrgPayablesModify.IsAllowed = oldPayablesModifyValue;
				Env.Security.OrgPayablesDefaultsModify.IsAllowed = oldPayablesDefaultsModifyValue;
			}
		}

		public void TestReadOnlySecurityForCategory()
		{
			bool originalOrgDetailsNewModifyCategory = Env.Security.OrgDetailsNewModifyCategory.IsAllowed;
			bool originalOrgConfigModifyCategory = Env.Security.OrgDetailsModifyCategory.IsAllowed;

			try
			{
				OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
				Env.Security.OrgDetailsNewModifyCategory.IsAllowed = true;
				Assert("New Org Access Allowed - ReadOnly", !organisation.OH_CategoryInfo.ReadOnly);
				Env.Security.OrgDetailsNewModifyCategory.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", organisation.OH_CategoryInfo.ReadOnly);
				Factory.Save();
				Env.Security.OrgDetailsModifyCategory.IsAllowed = true;
				Assert("Org Access Allowed - ReadOnly", !organisation.OH_CategoryInfo.ReadOnly);
				Env.Security.OrgDetailsModifyCategory.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", organisation.OH_CategoryInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsNewModifyCategory.IsAllowed = originalOrgDetailsNewModifyCategory;
				Env.Security.OrgDetailsModifyCategory.IsAllowed = originalOrgConfigModifyCategory;
			}
		}

		public void TestGetAdditionalCompanyName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;

				AssertGetAdditionalCompanyName(OrgAddressType.Payables, LedgerTypes.AccountsPayable);
				AssertGetAdditionalCompanyName(OrgAddressType.Receivables, LedgerTypes.AccountsReceivable);
			}
		}

		public void TestGetAdditionalCompanyNameWhenCompanyOrgProxyIsNull()
		{
			var companyWithoutOrgProxy = Factory.NewWithValidTestData<GlbCompany>();
			companyWithoutOrgProxy.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			companyWithoutOrgProxy.GC_OH_OrgProxy = ZGuid.Empty;

			var branchWithoutOrgProxy = Factory.NewWithValidTestData<GlbBranch>();
			branchWithoutOrgProxy.GB_GC = companyWithoutOrgProxy.PK;
			branchWithoutOrgProxy.GB_OH_OrgProxy = ZGuid.Empty;

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
			var branchWithOrgProxy = Factory.NewWithValidTestData<GlbBranch>();
			branchWithOrgProxy.GB_GC = companyWithoutOrgProxy.PK;
			branchWithOrgProxy.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();

			AssertEquals("Precondition", ZGuid.Empty, companyWithoutOrgProxy.GC_OH_OrgProxy);
			AssertEquals("Precondition", ZGuid.Empty, branchWithoutOrgProxy.GB_OH_OrgProxy);
			AssertEquals("Precondition", branchOrgProxy.PK, branchWithOrgProxy.GB_OH_OrgProxy);

			var debtorOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			debtorOrgHeader.OH_IsDebtor = true;
			debtorOrgHeader.Addresses.RemoveAndDeleteAll();
			var orgAddress = debtorOrgHeader.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress.Language = SharedConstants.Languages.ChineseSimplified;
			orgAddress.Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
			orgAddress.CompanyName = OrgHeaderUnicodeTestConstants.ChineseCompanyName1;
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
			orgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				ZString additionalCompanyName = default;
				AssertNoExceptionThrown(() => additionalCompanyName = debtorOrgHeader.GetAdditionalCompanyName(OrgAddressType.Receivables));
				AssertEquals(OrgHeaderUnicodeTestConstants.ChineseCompanyName1, additionalCompanyName);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithoutOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				ZString additionalCompanyName = default;
				AssertNoExceptionThrown(() => additionalCompanyName = debtorOrgHeader.GetAdditionalCompanyName(OrgAddressType.Receivables));
				AssertEquals(ZString.Empty, additionalCompanyName);
			}
		}

		public void TestOHLanguage()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Language = "CHS";
			AssertEquals("Old language code should be corrected after setting", Core.SharedConstants.Languages.ChineseSimplified, orgHeader.OH_Language);
		}

		void AssertGetAdditionalCompanyName(OrgAddressType addressType, string ledgerType)
		{
			var expectedAdditionalCompanyName = OrgHeaderUnicodeTestConstants.ChineseCompanyName1;

			// Main Address
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.Addresses.RemoveAndDeleteAll();

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress.Language = SharedConstants.Languages.ChineseSimplified;
			orgAddress.Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
			orgAddress.CompanyName = expectedAdditionalCompanyName;
			orgAddress.AddressCapability.SetCapabilityEnabled(addressType);
			orgAddress.AddressCapability.SetIsMainAddress(addressType);

			Factory.Save();

			AssertEquals("The addtional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, orgHeader.GetAdditionalCompanyName(addressType));

			// Main Translated Address
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_IsDebtor = true;
			orgHeader1.Addresses.RemoveAndDeleteAll();

			var orgAddress1 = orgHeader1.Addresses.AddNew();
			orgAddress1.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress1.Language = SharedConstants.Languages.English;
			orgAddress1.Address1 = "Test Address";
			orgAddress1.AddressCapability.SetCapabilityEnabled(addressType);
			orgAddress1.AddressCapability.SetIsMainAddress(addressType);

			var translatedAddress1 = orgAddress1.TranslatedAddresses.AddNew();
			translatedAddress1.Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
			translatedAddress1.CompanyName = expectedAdditionalCompanyName;
			translatedAddress1.Language = SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			AssertEquals("The addtional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, orgHeader1.GetAdditionalCompanyName(addressType));

			// Non-Main Address
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_IsDebtor = true;
			orgHeader2.Addresses.RemoveAndDeleteAll();

			var orgAddress2 = orgHeader2.Addresses.AddNew();
			orgAddress2.AddressCapability.SetCapabilityEnabled(addressType);
			orgAddress2.Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
			orgAddress2.CompanyName = expectedAdditionalCompanyName;
			orgAddress2.Language = SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			AssertEquals("The addtional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, orgHeader2.GetAdditionalCompanyName(addressType));

			// Non-Main Translated Address
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_IsDebtor = true;
			orgHeader3.Addresses.RemoveAndDeleteAll();

			var orgAddress3 = orgHeader3.Addresses.AddNew();
			orgAddress3.AddressCapability.SetCapabilityEnabled(addressType);
			orgAddress3.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			orgAddress3.Language = SharedConstants.Languages.English;
			orgAddress3.Address1 = "Test Address";

			var translatedAddress3 = orgAddress3.TranslatedAddresses.AddNew();
			translatedAddress3.Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
			translatedAddress3.CompanyName = expectedAdditionalCompanyName;
			translatedAddress3.Language = SharedConstants.Languages.ChineseSimplified;

			Factory.Save();

			AssertEquals("The addtional company name should be expectedAdditionalCompanyName.", expectedAdditionalCompanyName, orgHeader3.GetAdditionalCompanyName(addressType));

			// Empty
			var orgHeader4 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_IsDebtor = true;

			Factory.Save();

			AssertEquals("The addtional company name should be empty.", string.Empty, orgHeader4.GetAdditionalCompanyName(addressType));
		}

		public void TestDefermentAccountNumberCollectionHasRecordAfterAddNew()
		{
			// add an OrgCusAccount for Germany using a stand-alone OrgCusAccountCollection
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var collectionGermany = new OrgCusAccountCollection(orgHeader, Core.Constants.CountryCodes.Germany);
			collectionGermany.AddNew().FillWithValidTestData();
			Factory.Save();

			// get the DefermentAccountNumberCollection from the OrgHeader and assert that it contains the record that was added
			var orgCusAccountCollection = orgHeader.DefermentAccountNumberCollection;
			AssertEquals("DefermentAccountNumberCollection.Count", 1, orgCusAccountCollection.Count);
			AssertEquals("PK matched", collectionGermany[0].PK, orgCusAccountCollection[0].PK);
		}

		public void TestDefermentAccountNumberCollectionIsRegisteredEditableChildObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Register editable child", true, orgHeader.IsRegisteredEditableChildObject(orgHeader.DefermentAccountNumberCollection));
		}

		public void TestDefermentAccountNumberCollectionOnlyContainsCountryCodeGermany()
		{
			// add an OrgCusAccount for Eritrea using a stand-alone OrgCusAccountCollection
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var directCollection = new OrgCusAccountCollection(orgHeader, Core.Constants.CountryCodes.Eritrea);
			directCollection.AddNew().FillWithValidTestData();
			Factory.Save();

			// get the DefermentAccountNumberCollection from the OrgHeader and assert that it does NOT contains the record that was added
			AssertEquals("DefermentAccountNumberCollection.Count", 0, orgHeader.DefermentAccountNumberCollection.Count);
		}

		public void TestDeltaAgreementNumberCollectionHasRecordAfterAddNew()
		{
			// add an OrgCusAccount for France using a stand-alone OrgCusAccountCollection
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var collectionFrance = new OrgCusAccountCollection(orgHeader, Core.Constants.CountryCodes.France);
			var deltaAgreement = collectionFrance.AddNew();
			deltaAgreement.FillWithValidTestData();
			deltaAgreement.CZ_Code = "DXI";
			deltaAgreement.CZ_Type = "G1";
			Factory.Save();

			// get the DeltaAgreementNumberCollection from the OrgHeader and assert that it contains the record that was added
			var orgCusAccountCollection = orgHeader.DeltaAgreementNumberCollection;
			AssertEquals("DeltaAgreementNumberCollection.Count", 1, orgCusAccountCollection.Count);
			AssertEquals("PK matched", collectionFrance[0].PK, orgCusAccountCollection[0].PK);
		}

		public void TestDeltaAgreementNumberCollectionIsRegisteredEditableChildObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Register editable child", true, orgHeader.IsRegisteredEditableChildObject(orgHeader.DeltaAgreementNumberCollection));
		}

		public void TestDeltaAgreementNumberCollectionOnlyContainsCountryCodeFrance()
		{
			// add an OrgCusAccount for Eritrea using a stand-alone OrgCusAccountCollection
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var directCollection = new OrgCusAccountCollection(orgHeader, Core.Constants.CountryCodes.Eritrea);
			directCollection.AddNew().FillWithValidTestData();
			Factory.Save();

			// get the DeltaAgreementNumberCollection from the OrgHeader and assert that it does NOT contains the record that was added
			AssertEquals("DeltaAgreementNumberCollection.Count", 0, orgHeader.DeltaAgreementNumberCollection.Count);
		}

		public void TestReadOnlySecurityForCollectionCalls()
		{
			bool originalOrgDetailsModify = Env.Security.OrgDetailsModify.IsAllowed;
			bool originalReceivablesCollectionCallsEdit = Env.Security.ReceivablesCollectionCallsEdit.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModify.IsAllowed = false;
				Env.Security.ReceivablesCollectionCallsEdit.IsAllowed = false;

				OrgHeader organisation = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
				organisation.OH_Code = "TESTING_NN";
				organisation.Factory.Save();
				AssertCollectionCallPropertiesAreReadOnly("No Security Rights - [{0}] should be readonly.", true, organisation);

				Env.Security.OrgDetailsModify.IsAllowed = true;
				Env.Security.ReceivablesCollectionCallsEdit.IsAllowed = false;
				organisation = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
				organisation.OH_Code = "TESTING_YN";
				organisation.Factory.Save();
				AssertCollectionCallPropertiesAreReadOnly("Modify Org Details Security is granted - [{0}] should be editable.", false, organisation);

				Env.Security.OrgDetailsModify.IsAllowed = false;
				Env.Security.ReceivablesCollectionCallsEdit.IsAllowed = true;
				organisation = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
				organisation.OH_Code = "TESTING_NY";
				organisation.Factory.Save();
				AssertCollectionCallPropertiesAreReadOnly("Receivables Collection Calls Edit Security is granted - [{0}] should be editable).", false, organisation);

				Env.Security.OrgDetailsModify.IsAllowed = true;
				Env.Security.ReceivablesCollectionCallsEdit.IsAllowed = true;
				organisation = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
				organisation.OH_Code = "TESTING_YY";
				organisation.Factory.Save();
				AssertCollectionCallPropertiesAreReadOnly("Both Security Rights are granted - [{0}] should be editable).", false, organisation);
			}
			finally
			{
				Env.Security.OrgDetailsModify.IsAllowed = originalOrgDetailsModify;
				Env.Security.ReceivablesCollectionCallsEdit.IsAllowed = originalReceivablesCollectionCallsEdit;
			}
		}

		void AssertCollectionCallPropertiesAreReadOnly(string message, bool shouldBeReadOnly, OrgHeader organisation)
		{
			AssertEquals(string.Format(message, "CollectionNotes"), shouldBeReadOnly, organisation.CollectionNotes.ReadOnly);
			AssertEquals(string.Format(message, "DateOfCallNoteFrom"), shouldBeReadOnly, organisation.DateOfCallNoteFromInfo.ReadOnly);
			AssertEquals(string.Format(message, "DateOfCallNoteTo"), shouldBeReadOnly, organisation.DateOfCallNoteToInfo.ReadOnly);
			AssertEquals(string.Format(message, "DateFollowUpFrom"), shouldBeReadOnly, organisation.DateFollowUpFromInfo.ReadOnly);
			AssertEquals(string.Format(message, "DateFollowUpTo"), shouldBeReadOnly, organisation.DateFollowUpToInfo.ReadOnly);
			AssertEquals(string.Format(message, "CallNoteContact"), shouldBeReadOnly, organisation.CallNoteContactInfo.ReadOnly);
			AssertEquals(string.Format(message, "CallingStaff"), shouldBeReadOnly, organisation.CallingStaffInfo.ReadOnly);
			AssertEquals(string.Format(message, "CollectionCallStatus"), shouldBeReadOnly, organisation.CollectionCallStatusInfo.ReadOnly);
			AssertEquals(string.Format(message, "CollectionCallDisposition"), shouldBeReadOnly, organisation.CollectionCallDispositionInfo.ReadOnly);
		}

		public void TestAllOrgRelatedBOsImplementReadOnlySecurity()
		{
			Type[] listOfOrgTypesInMasterFiles = Array.FindAll(Assembly.GetExecutingAssembly().GetTypes(), (objectType) => { return objectType.Name.StartsWith("Org"); });
			Assert("precondition: MasterFiles Org types should have been reflected out", listOfOrgTypesInMasterFiles.Length > 0);

			List<string> failedBOs = new List<string>();
			foreach (Type orgType in listOfOrgTypesInMasterFiles)
			{
				object[] attributes = orgType.GetCustomAttributes(typeof(ProvideMetaDataPropertyAttribute), true);
				bool isReadOnly = false;
				foreach (object attribute in attributes)
				{
					ProvideMetaDataPropertyAttribute metadataAttr = (ProvideMetaDataPropertyAttribute)(attribute);
					if (metadataAttr.PropertyName == "ReadOnlySecurity")
					{
						isReadOnly = true;
						break;
					}
					if (metadataAttr.PropertyName == "ShouldPropertiesBeReadOnly")
					{
						var shouldPropertiesBeReadOnly = orgType.GetMethod("GetShouldPropertiesBeReadOnly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
						var readonlySecurity = orgType.GetMethod("GetReadOnlySecurity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (shouldPropertiesBeReadOnly != null && readonlySecurity != null)
						{
							isReadOnly = true;
							break;
						}
					}
				}
				if (typeof(BusinessObject).IsAssignableFrom(orgType) &&
					!isReadOnly &&
					!Array.Exists(orgTypesNOTExpectedToImplementIReadOnlySecurity, (registeredType) => { return orgType == registeredType; }))
				{
					failedBOs.Add(orgType.Name);
				}
			}

			if (failedBOs.Count > 0)
			{
				failedBOs.Sort();

				string message = "The following organisation-related business objects either do not implement IReadOnlySecurity OR they were not listed as an exception to the Org Security functionality. " +
					"If this business object is a new object to the organisation screen, then it MUST:\r\n" +
					"\ta) Implement the IReadOnlySecurity interface, and be locked down by an Org Related security checkpoint OR\r\n" +
					"\tb) The type of the business object must be added to the field OrgTypesNOTExpectedToImplementIReadOnlySecurity in the OrgHeaderTest class. This will exclude the BO from the list." +
					"\r\nPlease do not just exlude your BO to make the test pass!\r\n\r\nClasses That Failed Are:\r\n" + string.Join("\r\n", failedBOs.ToArray());
				Fail(message);
			}
		}

		public void TestOrgWebURLsIsReadOnly()
		{
			bool oldPhFaxWebDetailsValue = Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.OrgWebURLs.ReadOnly);

				Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.OrgWebURLs.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = oldPhFaxWebDetailsValue;
			}
		}

		readonly Type[] orgTypesNOTExpectedToImplementIReadOnlySecurity = new Type[]
		{
			typeof(OrgColdCallRegister),
			typeof(EnquiryOrgFinder),
			typeof(OrgAirlineMAWBStockManagement),
			typeof(OrgCommissionAgreementItem),
			typeof(OrgCommissionAgreementRecipient),
			typeof(OrgCommissionAgreementRecipientRate),
			typeof(OrgCountryData),
			typeof(OrgCreditorGroup),
			typeof(OrgDebtorGroup),
			typeof(OrgDebtorGroupBankDefault),
			typeof(OrgDebtorGroupBankCurrentOverride),
			typeof(OrgMatchApproval),
			typeof(OrgOpportunity),
			typeof(OrgRefFacility),
			typeof(OrgCompetitor),
			typeof(OrgCommissionAgreement),
			typeof(OrgCommissionCalculationQueue),
			typeof(OrgOpportunityValue),
			typeof(OrgOpportunityStageProgress),
			typeof(OrgPartLocation),
			typeof(OrgPartRelation),
			typeof(OrgPartUnit),
			typeof(OrgPatternMatch),
			typeof(OrgPatternMatchAddress),
			typeof(OrgSalesValueAssociationPivot),
			typeof(OrgSalesCallAdditionalAttendee),
			typeof(OrgSupplierBuyerLink),
			typeof(OrgSupplierBuyerLinkTolerance),
			typeof(OrgSupplierPart),
			typeof(OrgPartBOM),
			typeof(OrgSecondaryPartBOM),
			typeof(OrgSecondaryPartBOMPivot),
			typeof(OrgSupplierBulkRelationshipChanger),
			typeof(OrgSupplierBulkDeactivator),
			typeof(OrgContainerDetention),
			typeof(OrgCollectionCall),
			typeof(OrgCollectionNote),
			typeof(OrgRelatedParty),
			typeof(OrgSupplierPartBarcode),
			typeof(OrgPartBomView),
			typeof(OrgPartCategory),
			typeof(OrgAddressHelper),
			typeof(OrgInvTypeDeferredCharges),
			typeof(OrgHeaderWorkflowDescriptor),
			typeof(OrgStaffAssignmentsLookupsImplementer),
			typeof(OrgSalesCallProcessTask),
			typeof(OrgSalesCallWorkflowDescriptor),
			typeof(OrgSupplierPartProcessTask),
			typeof(OrgSupplierPartWorkflowDescriptor),
			typeof(OrgPartRelationProcessTask),
			typeof(OrgManagementGroupingModel),
			typeof(OrgPartRelationWorkflowDescriptor),
			typeof(OrgsEvaluatedForCreditControl),
			typeof(OrgContactWebWarehouseEligibility),
			typeof(OrgCommissionAgreementItemCondition),
			typeof(OrgSecurityProfile),
			typeof(OrgSecurityProfileUpdater),
			typeof(OrgSecurityProfileSetting),
			typeof(OrgCusAccount),
			typeof(CountryCompliance.CountryComplianceInfoDisplay.OrgCusCodeTypeDisplay),
			typeof(OrganisationTaxRateFileImport),
			typeof(OrganisationTaxRateFileImportLine),
		};

		OrgHeader OrgInDB
		{
			get
			{
				if (orgInDB == null)
				{
					ResetOrgInDB();
				}

				return orgInDB;
			}
		}

		OrgHeader orgInDB;

		void ResetOrgInDB()
		{
			orgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region MainAddressCollection

		public void TestCountryCode()
		{
			OrgHeader header = OrgHeader.New(Factory);
			AssertEquals(ZString.Empty, header.CountryCode);
			header.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("AU", header.CountryCode);
		}

		public void TestMainAddressCollection()
		{
			AssertEquals("Main address collection should have only 1 entry", 1, company.MainAddressCollection.Count);
			AssertEquals("Main address collection should contain main address", company.MainAddress, company.MainAddressCollection[0]);
		}

		public void TestMainAddressCountryCodes()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_IsGlobalAccount = true;

			var mainAddress1 = header.Addresses.AddNew();
			mainAddress1.OA_RN_NKCountryCode = "AU";
			mainAddress1.OA_Language = "EN";
			mainAddress1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			mainAddress1.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			AssertEquals("AU", header.MainAddressCountryCodes);

			var mainAddress2 = header.Addresses.AddNew();
			mainAddress2.OA_RN_NKCountryCode = "US";
			mainAddress2.OA_Language = "EN-US";
			mainAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			mainAddress2.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			AssertEquals("AU, US", header.MainAddressCountryCodes);
		}

		public void TestAddressesNoAutoCreate()
		{
			OrgHeader header = OrgHeader.New(Factory);
			AssertEquals("There should be no addresses in the collection", 0, header.AddressesNoAutoCreate.Count);
			AssertEquals("There should be one address in the collection", 1, header.Addresses.Count);
		}

		public void TestAddressesNoAutoCreateEditSecurity()
		{
			OrgHeader header = OrgHeader.New(Factory);
			header.OH_Code = "XXXYYYZZZ";
			Factory.Save();

			bool oldValue = Env.Security.OrgAddressModify.IsAllowed;
			try
			{
				Env.Security.OrgAddressModify.IsAllowed = false;
				AssertEquals("Should be readonly", true, new BusinessObjectFactory().Load<OrgHeader>(header.PK).AddressesNoAutoCreate.ReadOnly);

				Env.Security.OrgAddressModify.IsAllowed = true;
				AssertEquals("Should be writable", false, new BusinessObjectFactory().Load<OrgHeader>(header.PK).AddressesNoAutoCreate.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressModify.IsAllowed = oldValue;
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteOrgNoAddressesDoesNotAddNewAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Org 1 2 3";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "26 Myrtle Street";
			org.MainAddress.OA_City = "Prospect";
			Factory.Save();

			OrgAddress address = org.Addresses[0];
			address.Delete();   // Mimicking a merged org - which has no addresses
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader reloadedOrg = factory2.Load<OrgHeader>(org.PK);
			reloadedOrg.SetReadOnlyIncludingChildren(true); // Emulate what happens when a delete form is shown

			reloadedOrg.Delete();
			Assert("Org was deleted successfully", reloadedOrg.IsDeleted);
		}

		public void TestAddressesNoAutoCreateIsNullIfThrowExceptionWhenLoad()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Org 1 2 3";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "26 Myrtle Street";
			org.MainAddress.OA_City = "Prospect";
			Factory.Save();

			AssertEquals("Should have one address", 1, org.AddressesNoAutoCreate.Count);
			var addressesInfo = typeof(OrgHeader).GetField("addresses", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull("The addresses of Org are not null", addressesInfo.GetValue(org));

			addressesInfo.SetValue(org, null);
			Factory.RowsLoaded += (sender, args) =>
			{
				throw CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(1222, "Lock request time out period exceeded.");
			};

			AssertExceptionThrown<Exception>(() =>
			{
				var addresses = org.AddressesNoAutoCreate;
			});
			AssertNull("The addresses of Org are still null", addressesInfo.GetValue(org));
		}

		#endregion

		#region IEnrichmentDataProvider

		public void TestIEnrichmentDataProvider()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.OH_RL_NKClosestPort = "AUSYD";

			var mainAddress = org.MainAddress;
			mainAddress.Address1 = "TEST ADDRESS 1";
			((INeedRow)mainAddress).Row[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode] = "USORD";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "TEST CONTACT";

			var brand = org.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "A NEW NAME";

			var webURL = org.OrgWebURLs.AddNew();
			webURL.PU_URL = "www.testtest.com";

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "ABC";
			cusCode.OK_CustomsRegNo = "ABCDEFG";

			Factory.Save();

			var enrichmentDataProvider = (IEnrichmentDataProvider)Factory.Load<OrgHeader>(org.PK);

			using (Env.SetTemporaryUserContext(null))
			{
				AssertNoExceptionThrown(() =>
				{
					CombineAssertions(() =>
					{
						AssertEquals("AUSYD", enrichmentDataProvider.OH_RL_NKClosestPort);
						AssertEquals("TEST ADDRESS 1", (enrichmentDataProvider.Addresses.Single() as OrgAddress).Address1);
						AssertEquals("TEST CONTACT", (enrichmentDataProvider.Contacts.Single() as OrgContact).OC_ContactName);
						AssertEquals("A NEW NAME", (enrichmentDataProvider.BrandsOrRelatedNames.Single() as OrgBrandOrRelatedName).P1_RelatedName);
						AssertEquals("www.testtest.com", (enrichmentDataProvider.OrgWebURLs.Single() as OrgWebURL).PU_URL);
						AssertEquals("ABCDEFG", (enrichmentDataProvider.CustomsCodes.Single() as OrgCusCode).OK_CustomsRegNo);
					});

					org.OH_IsGlobalAccount = true;
					AssertEquals("USORD", enrichmentDataProvider.OH_RL_NKClosestPort);

					((INeedRow)mainAddress).Row[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode] = "";
					AssertEquals("AUSYD", enrichmentDataProvider.OH_RL_NKClosestPort);
				});
			}
		}

		public void TestIEnrichmentDataProvider_OH_RL_NKClosestPort()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.OH_RL_NKClosestPort = "AUSYD";

			var mainAddress1 = org.MainAddress;
			mainAddress1.OA_Language = Core.Constants.Languages.English;
			mainAddress1.Address1 = "TEST ADDRESS 1";
			((INeedRow)mainAddress1).Row[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode] = "USORD";

			var mainAddress2 = org.Addresses.AddNewMainAddress();
			mainAddress2.OA_Language = Core.Constants.Languages.Arabic;
			mainAddress2.Address1 = "TEST ADDRESS 2";
			((INeedRow)mainAddress2).Row[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode] = "CNAHK";

			var address3 = org.Addresses.AddNew();
			address3.OA_Language = Core.Constants.Languages.English;
			address3.Address1 = "TEST ADDRESS 3";
			((INeedRow)address3).Row[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode] = "CH3DV";

			Factory.Save();

			var enrichmentDataProvider = (IEnrichmentDataProvider)Factory.Load<OrgHeader>(org.PK);

			using (Env.SetTemporaryUserContext(null))
			{
				AssertEquals("AUSYD", enrichmentDataProvider.OH_RL_NKClosestPort);

				org.OH_IsGlobalAccount = true;
				AssertEquals("USORD", enrichmentDataProvider.OH_RL_NKClosestPort);
			}

			mainAddress1.Delete();

			using (Env.SetTemporaryUserContext(null))
			{
				AssertEquals("CNAHK", enrichmentDataProvider.OH_RL_NKClosestPort);

				((INeedRow)mainAddress2).Row[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode] = "";
				AssertEquals("AUSYD", enrichmentDataProvider.OH_RL_NKClosestPort);
			}
		}

		public void TestIEnrichmentDataProvider_OrganisationTypes()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;
			org.OH_IsConsignor = true;
			org.OH_IsConsignee = true;
			org.OH_IsTransportClient = true;
			org.OH_IsShippingProvider = true;
			org.OH_IsForwarder = true;
			org.OH_IsBroker = true;
			org.OH_IsMiscFreightServices = true;
			org.OH_IsCompetitor = true;
			org.OH_IsSalesLead = true;
			org.OH_IsWarehouseClient = true;
			org.OH_IsDistributionCentre = true;
			org.OH_IsControllingAgent = true;
			org.OH_IsControllingCustomer = true;

			var orgTyes = org.OrganisationTypes;
			orgTyes &= ~OrganisationTypes.Debtor;
			orgTyes &= ~OrganisationTypes.Creditor; // Not care company data related organization type

			using (Env.SetTemporaryUserContext(null))
			{
				AssertNoExceptionThrown(() =>
				{
					AssertEquals((int)orgTyes, (org as IEnrichmentDataProvider).OrganisationTypes);
					AssertEquals(orgTyes, (OrganisationTypes)(org as IEnrichmentDataProvider).OrganisationTypes);
				});
			}

			org.OH_IsConsignor = false;
			org.OH_IsConsignee = true;
			org.OH_IsTransportClient = false;
			org.OH_IsShippingProvider = true;
			org.OH_IsForwarder = false;
			org.OH_IsBroker = true;
			org.OH_IsMiscFreightServices = false;
			org.OH_IsCompetitor = true;
			org.OH_IsSalesLead = false;
			org.OH_IsWarehouseClient = true;
			org.OH_IsDistributionCentre = false;
			org.OH_IsControllingAgent = true;
			org.OH_IsControllingCustomer = false;

			orgTyes = org.OrganisationTypes;
			orgTyes &= ~OrganisationTypes.Debtor;
			orgTyes &= ~OrganisationTypes.Creditor;

			using (Env.SetTemporaryUserContext(null))
			{
				AssertNoExceptionThrown(() =>
				{
					AssertEquals((int)orgTyes, (org as IEnrichmentDataProvider).OrganisationTypes);
					AssertEquals(orgTyes, (OrganisationTypes)(org as IEnrichmentDataProvider).OrganisationTypes);
				});
			}
		}

		#endregion

		#region OrgHeaderForTest

		public class OrgHeaderForTest : OrgHeader, IDeduplicatable
		{
			public OrgHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public GlbBranch BranchForCurrentUNLOCOExposed
			{
				get { return base.BranchForCurrentUNLOCO; }
			}

			void IDeduplicatable.PropagateDeduplicationEnded<TBizo>(IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModel, object targetObjects, DuplicationStatus lastRunStatus, DeduplicationExclusionManager<TBizo> exclusionManager)
			{
				CurrentExclusionManager = exclusionManager as DeduplicationExclusionManager<OrgHeader>;
				CurrentExclusionManager.BuildDisplay();
			}

			public bool GetReadOnlySecurity_Exposed(PropertyDescriptor property) => GetReadOnlySecurity(property);

			#region New Properties

			public DeduplicationExclusionManager<OrgHeader> CurrentExclusionManager { get; set; }

			public static int MaximumResultsToSearchExposed
			{
				get
				{
					return OrganisationsDataRegistry.Instance.OrgPatternMatchLimit.Value;
				}
			}

			#endregion
		}

		#endregion

		#region OrgHeaderAddressCount

		public void TestNullOrgHeaderAddressCount()
		{
			OrgHeader org = Factory.GetNull<OrgHeader>();
			AssertEquals(0, org.Addresses.Count);
		}

		#endregion

		#region LookupsOverrideTesting

		public void TestLookupTypeisOrgHeadeLookupsWithUserFilters()
		{
			var lookups = new OrgHeaderLookupsWithUserFilters(OrgHeader.New(Factory));
			AssertEquals(lookups.GetType().Equals(typeof(OrgHeaderLookupsWithUserFilters)), true);
		}

		#endregion

		#region OrgHeader Main Address Not Null

		public void TestMainAddressOnNullOrgHeader()
		{
			OrgHeader org = Factory.GetNull<OrgHeader>();
			AssertEquals(true, org.MainAddress.IsNull);
		}

		#endregion

		#region Tests DeleteUnusedStoredDefaults

		public void TestMakeOrganisationInactiveDeletesUnusedStoredDefaults()
		{
			AssertUnusedStoredDefaultsAreDeleted((OrgHeader organisation) => { organisation.OH_IsActive = false; Factory.Save(); });
		}

		public void TestDeleteOrganisationRemovesStoredDefaults()
		{
			AssertUnusedStoredDefaultsAreDeleted((OrgHeader organisation) => organisation.Delete());
		}

		void AssertUnusedStoredDefaultsAreDeleted(Action<OrgHeader> action)
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var data1ForOrganisation = Factory.New<StmData>();
			data1ForOrganisation.SD_DepartmentGuid = organisation.PK;
			var data2ForOrganisation = Factory.New<StmData>();
			data2ForOrganisation.SD_DepartmentGuid = organisation.PK;
			data2ForOrganisation.SD_Owner = ZGuid.NewZGuid();

			var otherOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var data1ForOtherOrganisation = Factory.New<StmData>();
			data1ForOtherOrganisation.SD_DepartmentGuid = otherOrganisation.PK;
			var data2ForOtherOrganisation = Factory.New<StmData>();
			data2ForOtherOrganisation.SD_DepartmentGuid = otherOrganisation.PK;
			data2ForOtherOrganisation.SD_Owner = ZGuid.NewZGuid();

			Factory.Save();

			action(organisation);

			AssertNull(Factory.Load<StmData>(data1ForOrganisation.PK));
			AssertNull(Factory.Load<StmData>(data2ForOrganisation.PK));
			AssertNotNull(Factory.Load<StmData>(data1ForOtherOrganisation.PK));
			AssertNotNull(Factory.Load<StmData>(data2ForOtherOrganisation.PK));
		}

		#endregion

		#region Test LoadFromCode

		public void TestLoadFromCode()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "TESTCODE";

			AssertEquals("TESTCODE", OrgHeader.LoadFromCode(Factory, "TESTCODE").OH_Code);
			AssertNull(OrgHeader.LoadFromCode(Factory, "XXXYY"));
		}

		#endregion

		#region Test Loading Sales Calls Via Filter

		#region Helpers

		struct FieldNameValuePair
		{
			public FieldNameValuePair(string inputField, object inputValue)
			{
				FieldName = inputField;
				Value = inputValue;
			}

			public readonly string FieldName;
			public readonly object Value;
		}

		#endregion

		#endregion

		#region Related Collection Notes Tests

		#region Helpers

		void AddTestCollectionNoteToOrg(OrgHeader orgToAddTo, int numberOfItems, params FieldNameValuePair[] inputFieldNamesAndValues)
		{
			for (int i = 0; i < numberOfItems; i++)
			{
				OrgCollectionNote newNote = orgToAddTo.CollectionNotes.AddNew();
				newNote.FillWithValidTestData();
				foreach (FieldNameValuePair nameValuePair in inputFieldNamesAndValues)
				{
					newNote[nameValuePair.FieldName] = nameValuePair.Value;
				}
			}
		}

		#endregion

		#region Test Loading Collection Notes Via Filter

		#region Date Filter Tests

		bool CollectionNotesContainDate(OrgHeader org, string fieldToCompare, ZDateTime inputDate)
		{
			foreach (OrgCollectionNote currentNote in org.CollectionNotes)
			{
				if ((ZDateTime)currentNote[fieldToCompare] == inputDate)
				{
					return true;
				}
			}

			return false;
		}

		void RunCollectionNotesFilterDateFilteringTests(string dateFromPropertyName, string dateToPropertyName)
		{
			/*						DateFrom				DateTo
				 *		14/12/00		7/9/01		8/8/02		6/10/03	(*)		4/5/04
				 *   |-------------------------------------------------------------------|
				 * 2000																	2005
				 * 
				 */

			string fieldToCompare = dateFromPropertyName == "DateOfCallNoteFrom" ? OrgCollectionNoteSchema.PN_SystemCreateTimeUtc.Name : OrgCollectionNoteSchema.PN_CallBackDate.Name;

			company.OH_Code = "XXXXXX";
			ZDateTime testDate1 = new ZDateTime(2000, 12, 14);
			ZDateTime testDate2 = new ZDateTime(2001, 9, 7);
			ZDateTime testDate3 = new ZDateTime(2002, 8, 8);
			ZDateTime testDate4 = new ZDateTime(2003, 10, 6);
			ZDateTime testDate5 = new ZDateTime(2004, 5, 4);
			AddTestCollectionNoteToOrg(company, 1, new FieldNameValuePair(fieldToCompare, testDate1));
			AddTestCollectionNoteToOrg(company, 1, new FieldNameValuePair(fieldToCompare, testDate2));
			AddTestCollectionNoteToOrg(company, 1, new FieldNameValuePair(fieldToCompare, testDate3));
			AddTestCollectionNoteToOrg(company, 1, new FieldNameValuePair(fieldToCompare, testDate4));
			AddTestCollectionNoteToOrg(company, 1, new FieldNameValuePair(fieldToCompare, testDate4.AddDays(1)));
			AddTestCollectionNoteToOrg(company, 1, new FieldNameValuePair(fieldToCompare, testDate5));
			Factory.Save();

			company[dateFromPropertyName] = testDate2;
			company[dateToPropertyName] = testDate4;

			// Test Boundaries
			company.LoadCollectionNotesWithFiltering();
			Assert("TestDate 1 should not be in range", !CollectionNotesContainDate(company, fieldToCompare, testDate1));
			Assert("TestDate 2 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate2));
			Assert("TestDate 3 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate3));
			Assert("TestDate 4 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate4));
			Assert("TestDate 5 should not be in range", !CollectionNotesContainDate(company, fieldToCompare, testDate5));

			// Test Single Date
			company[dateFromPropertyName] = testDate1;
			company[dateToPropertyName] = testDate1;
			company.LoadCollectionNotesWithFiltering();
			Assert("TestDate 1 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate1));

			// Test Bad Date
			company[dateFromPropertyName] = testDate5;
			company[dateToPropertyName] = testDate1;
			company.LoadCollectionNotesWithFiltering();
			AssertEquals("Sales Calls should be empty", 0, company.CollectionNotes.Count);

			// Test Open Upper Bound
			company[dateFromPropertyName] = testDate2;
			company[dateToPropertyName] = ZDateTime.Empty;
			company.LoadCollectionNotesWithFiltering();
			Assert("TestDate 1 should not be in range", !CollectionNotesContainDate(company, fieldToCompare, testDate1));
			Assert("TestDate 2 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate2));
			Assert("TestDate 3 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate3));
			Assert("TestDate 4 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate4));
			Assert("TestDate 5 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate5));

			// Test Open Lower Bound
			company[dateFromPropertyName] = ZDateTime.Empty;
			company[dateToPropertyName] = testDate4;
			company.LoadCollectionNotesWithFiltering();
			Assert("TestDate 1 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate1));
			Assert("TestDate 2 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate2));
			Assert("TestDate 3 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate3));
			Assert("TestDate 4 should be in range", CollectionNotesContainDate(company, fieldToCompare, testDate4));
			Assert("TestDate 5 should not be in range", !CollectionNotesContainDate(company, fieldToCompare, testDate5));
		}

		public void TestCollectionNotesFilterDateOfCallNote()
		{
			RunCollectionNotesFilterDateFilteringTests("DateOfCallNoteFrom", "DateOfCallNoteTo");
		}

		public void TestCollectionNotesFilterDateFollowUp()
		{
			RunCollectionNotesFilterDateFilteringTests("DateFollowUpFrom", "DateFollowUpTo");
		}

		#endregion

		#region Contact Test

		public void TestNoteContactFilter()
		{
			company.OH_Code = "XXXXXX";

			// Contact 1 assigned to collection notes 0, 1
			OrgContact newContact1 = company.Contacts.AddNew();
			newContact1.OC_ContactName = "Test Contact 1";
			AddTestCollectionNoteToOrg(company, 2, new FieldNameValuePair(OrgCollectionNoteSchema.PN_OC.Name, newContact1.PK));

			// Contact 1 assigned to sales calls 2
			OrgContact newContact2 = company.Contacts.AddNew();
			newContact2.OC_ContactName = "Test Contact 2";
			AddTestCollectionNoteToOrg(company, 1, new FieldNameValuePair(OrgCollectionNoteSchema.PN_OC.Name, newContact2.PK));

			Factory.Save();

			company.CallNoteContact = newContact1.PK;
			company.LoadCollectionNotesWithFiltering();
			AssertEquals("There are 2 results returned", 2, company.CollectionNotes.Count);

			company.CallNoteContact = newContact2.PK;
			company.LoadCollectionNotesWithFiltering();
			AssertEquals("There is 1 result returned", 1, company.CollectionNotes.Count);

			company.CallNoteContact = ZGuid.Empty;
			company.LoadCollectionNotesWithFiltering();
			AssertEquals("There are 3 results returned", 3, company.CollectionNotes.Count);

			OrgContact newContact3 = company.Contacts.AddNew();
			newContact3.OC_ContactName = "Test Contact 3";
			company.CallNoteContact = newContact3.PK;
			company.LoadCollectionNotesWithFiltering();
			AssertEquals("Result collection should be empty", 0, company.CollectionNotes.Count);
		}

		public void TestGetLatestActiveContacts()
		{
			var contact1 = company.Contacts.AddNew();
			contact1.OC_IsActive = true;
			var contact2 = company.Contacts.AddNew();
			contact2.OC_IsActive = true;

			var activeContactCollection = company.GetActiveContacts();

			AssertEquals(2, activeContactCollection.Count);
			AssertEquals(true, activeContactCollection.Contains(contact1));
			AssertEquals(true, activeContactCollection.Contains(contact2));

			contact2.OC_IsActive = false;

			activeContactCollection = company.GetActiveContacts();

			AssertEquals(1, activeContactCollection.Count);
			AssertEquals(true, activeContactCollection.Contains(contact1));
			AssertEquals(false, activeContactCollection.Contains(contact2));
		}

		#endregion

		#endregion

		#region TestClearCollectionNotesFilterValues

		public void TestClearCollectionNotesFilterValues()
		{
			OrgContact newContact1 = company.Contacts.AddNew();
			newContact1.OC_ContactName = "Test Contact 1";
			AddTestCollectionNoteToOrg(company, 2, new FieldNameValuePair(OrgCollectionNoteSchema.PN_OC.Name, newContact1.PK));

			company.DateOfCallNoteFrom = ZDateTime.Now;
			company.DateOfCallNoteTo = ZDateTime.Now;
			company.DateFollowUpFrom = ZDateTime.Now;
			company.DateFollowUpTo = ZDateTime.Now;

			OrgContact newContact2 = company.Contacts.AddNew();
			newContact2.OC_ContactName = "Test Contact 2";
			company.CallNoteContact = newContact2.PK;
			company.CallingStaff = GlbStaff.CurrentUser.GS_Code;

			company.LoadCollectionNotesWithFiltering();
			AssertEquals("The result collection should be empty", 0, company.CollectionNotes.Count);

			company.ClearCollectionNotesFilterValues();
			AssertEquals("There are 2 results returned", 2, company.CollectionNotes.Count);
			AssertEquals("DateOfCallNoteFrom should be defaulted", ZDateTime.Empty, company.DateOfCallNoteFrom);
			AssertEquals("DateOfCallNoteTo should be defaulted", ZDateTime.Empty, company.DateOfCallNoteTo);
			AssertEquals("DateFollowUpFrom should be defaulted", ZDateTime.Empty, company.DateFollowUpFrom);
			AssertEquals("DateFollowUpTo should be defaulted", ZDateTime.Empty, company.DateFollowUpTo);
			AssertEquals("CallNoteContact should be defaulted", ZGuid.Empty, company.CallNoteContact);
			AssertEquals("CallingStaff should be defaulted", ZString.Empty, company.CallingStaff);
		}

		#endregion

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = string.Empty;
			AssertEquals("HumanReadableName", (NoResString)"Organization", org.HumanReadableName);
			org.OH_Code = "code";
			AssertEquals("HumanReadableName", (NoResString)"Organization (code)", org.HumanReadableName);
		}

		#endregion

		#region TestHumanReadableShortcutName

		public void TestHumanReadableShortcutName()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "code";
			org.OH_FullName = "name";
			AssertEquals("HumanReadableShortcutName", "code - name", org.HumanReadableShortcutName);

			org.OH_RL_NKClosestPort = "USORD";
			org.OH_Code = "code";
			AssertEquals("HumanReadableShortcutName with port", "code - name (USORD)", org.HumanReadableShortcutName);
		}

		#endregion

		#region Helper methods tests

		public void TestIsProxyOrg()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			Assert("Not expecting org proxy to match org", !org.IsProxyOrg(company));

			company.GC_OH_OrgProxy = org.PK;

			Assert("Expecting org proxy to match org", org.IsProxyOrg(company));

			branch.GB_OH_OrgProxy = org2.PK;

			Assert("Expecting org proxy to match org", org.IsProxyOrg(company));
			Assert("Expecting org proxy to match org2", org2.IsProxyOrg(company));

			company.GC_OH_OrgProxy = org2.PK;

			Assert("Not expecting org proxy to match org", !org.IsProxyOrg(company));
			Assert("Expecting org proxy to match org2", org2.IsProxyOrg(company));
		}

		public void TestIsProxyOrgOfAnyOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DXZENTBNE";
			org.OH_FullName = "DXZ Enterprises";
			org.OH_RL_NKClosestPort = "AUBNE";
			org.OH_IsForwarder = true;
			org.MainAddress.OA_Address1 = "1 Street St";

			Factory.Save();

			Assert("Not expecting org to be proxy org.", !org.IsProxyOrgOfAnyCompany());

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "DXZ";
			newCompany.GC_Name = "DXZ Enterprises";
			newCompany.GC_RN_NKCountryCode = "AU";
			newCompany.GC_RX_NKLocalCurrency = "AUD";
			newCompany.GC_OH_OrgProxy = org.PK;

			Factory.Save();

			Assert("Expecting org to be proxy org.", org.IsProxyOrgOfAnyCompany());

			OrgHeader org1 = Factory.New<OrgHeader>();
			GlbBranch branch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK); // this is changed
			branch.GB_OH_OrgProxy = org1.PK;
			OrgHeader org2 = Factory.New<OrgHeader>();

			Assert("Expecting org1 to be proxy org", org1.IsProxyOrgOfAnyCompany());

			Assert("Not expecting org2 to be proxy org", !org2.IsProxyOrgOfAnyCompany());

			GlbCompany company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = org2.PK;
			Assert("Not expecting org2 to be proxy org (excluding current)", !org2.IsProxyOrgOfAnyCompany(true));
			Assert("Expecting org2 to be proxy org (including current)", org2.IsProxyOrgOfAnyCompany());
			Assert("Expecting org2 to be proxy org (including current) explicit", org2.IsProxyOrgOfAnyCompany(false));
		}

		public void TestIsProxyOrgOfAnyOrgExcludingCurrentBranch()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DXZENTBNE";
			org.OH_FullName = "DXZ Enterprises";
			org.OH_RL_NKClosestPort = "AUBNE";
			org.OH_IsForwarder = true;
			org.MainAddress.OA_Address1 = "1 Street St";

			Factory.Save();

			var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var currentBranch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);

			Assert("org is not any company's org proxy branch", !org.IsProxyOrgOfAnyCompany(false));
			Assert("org is not any company's org proxy branch", !org.IsProxyOrgOfAnyCompany(true));

			otherBranch.GB_OH_OrgProxy = org.PK;

			Assert("org is org proxy of a sister company branch", org.IsProxyOrgOfAnyCompany(false));
			Assert("org is org proxy of a sister company branch", org.IsProxyOrgOfAnyCompany(true));

			currentBranch.GB_OH_OrgProxy = org.PK;

			Assert("org is org proxy of a sister and current company branch", org.IsProxyOrgOfAnyCompany(false));
			Assert("org is org proxy of a sister and current company branch", org.IsProxyOrgOfAnyCompany(true));

			otherBranch.GB_OH_OrgProxy = ZGuid.Empty;

			Assert("org is org proxy of a current company branch only", org.IsProxyOrgOfAnyCompany(false));
			Assert("org is org proxy of a current company branch only", !org.IsProxyOrgOfAnyCompany(true));
		}

		public void TestIsProxyOrgOfAnyCompanyActive()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DXZENTBNE";
			org.OH_FullName = "DXZ Enterprises";
			org.OH_RL_NKClosestPort = "AUBNE";
			org.OH_IsForwarder = true;
			org.MainAddress.OA_Address1 = "1 Street St";
			Factory.Save();

			Assert("Not expecting org to be proxy org.", !org.IsProxyOrgOfAnyCompany());

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "DXZ";
			newCompany.GC_Name = "DXZ Enterprises";
			newCompany.GC_RN_NKCountryCode = "AU";
			newCompany.GC_RX_NKLocalCurrency = "AUD";
			newCompany.GC_OH_OrgProxy = org.PK;
			newCompany.GC_IsActive = false;
			Factory.Save();

			Assert("Expecting org to be proxy org.", !org.IsProxyOrgOfAnyCompany(false));

			OrgHeader org1 = Factory.New<OrgHeader>();
			GlbBranch branch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK); // this is changed
			branch.GB_OH_OrgProxy = org1.PK;
			Assert("Expecting org1 to be proxy org", org1.IsProxyOrgOfAnyCompany(false));

			branch.GB_IsActive = false;
			Assert("Expecting org1 to be proxy org", !org1.IsProxyOrgOfAnyCompany(false));
		}

		public void TestCompanyProxies()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "DXZENTBNE";
			org.OH_FullName = "DXZ Enterprises";
			org.OH_RL_NKClosestPort = "AUBNE";
			org.OH_IsForwarder = true;
			org.MainAddress.OA_Address1 = "1 Street St";

			Factory.Save();

			AssertEquals("Not expecting org to be proxy org of any company.", 0, org.CompanyProxies(false).Count);

			GlbCompany newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "DXZ";
			newCompany.GC_Name = "DXZ Enterprises";
			newCompany.GC_RN_NKCountryCode = "AU";
			newCompany.GC_RX_NKLocalCurrency = "AUD";
			newCompany.GC_OH_OrgProxy = org.PK;

			Factory.Save();

			AssertEquals("Expecting org to be proxy org of 1 company.", 1, org.CompanyProxies(false).Count);
			AssertEquals("Expecting org to be proxy org of newCompany.", newCompany.PK, org.CompanyProxies(false)[0].PK);

			OrgHeader org1 = Factory.New<OrgHeader>();
			GlbBranch branch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK); // this is changed
			branch.GB_OH_OrgProxy = org1.PK;
			OrgHeader org2 = Factory.New<OrgHeader>();

			AssertEquals("Expecting org to be proxy org of 1 company.", 1, org1.CompanyProxies(false).Count);
			AssertEquals("Expecting org to be proxy org of newCompany.", GlbCompany.CurrentCompany.PK, org1.CompanyProxies(false)[0].PK);
			AssertEquals("Not expecting org to be proxy org of any company.", 0, org2.CompanyProxies(false).Count);

			AssertEquals("Exclude CurrentCompany, expect 0.", 0, org1.CompanyProxies(true).Count);

			newCompany.GC_OH_OrgProxy = org1.PK;
			AssertEquals("Expecting org1 to be proxy org of 2 companies.", 2, org1.CompanyProxies(false).Count);
			Assert("Expecting org1 to be proxy org of newCompany.", org1.CompanyProxies(false).Contains(newCompany));
			Assert("Expecting org1 to be proxy org of CurrentCompany.", org1.CompanyProxies(false).ConvertAll(c => c.PK).Contains(GlbCompany.CurrentCompany.PK));
			Assert("Expecting org1 to be proxy org of CurrentCompany.", org1.CompanyProxies(false).ConvertAll(c => c.PK).Contains(newCompany.PK));

			AssertEquals("Exclude CurrentCompany. Expecting org1 to be proxy org of 1 companies.", 1, org1.CompanyProxies(true).Count);
			AssertEquals("Expecting org to be proxy org of newCompany.", newCompany.PK, org1.CompanyProxies(true)[0].PK);
		}

		public void TestIsProxyOrgOfAnyCompanyOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			AssertEquals("Should not be a proxy", false, org.IsProxyOrgOfAnyCompanyOnly(false));

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_OH_OrgProxy = org.PK;
			Factory.Save();
			AssertEquals("Should not be a proxy for current company", false, org.IsProxyOrgOfAnyCompanyOnly(true));
			AssertEquals("Should be a proxy for current company", true, org.IsProxyOrgOfAnyCompanyOnly(false));
			currentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			Factory.Save();

			var inactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
			inactiveCompany.GC_OH_OrgProxy = org.PK;
			inactiveCompany.GC_IsActive = false;
			Factory.Save();
			AssertEquals("Should not be a proxy for inactive company", false, org.IsProxyOrgOfAnyCompanyOnly(false));

			var activeCompany = Factory.NewWithValidTestData<GlbCompany>();
			activeCompany.GC_OH_OrgProxy = org.PK;
			activeCompany.GC_IsActive = true;
			Factory.Save();
			AssertEquals("Should be a proxy for active company", true, org.IsProxyOrgOfAnyCompanyOnly(false));
		}

		public void TestIsProxyOrgOfAnyBranchOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			AssertEquals("Should not be a proxy", false, org.IsProxyOrgOfAnyBranchOnly(false));

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;
			currentBranch.GB_OH_OrgProxy = org.PK;
			currentBranch.GB_IsActive = true;
			Factory.Save();
			AssertEquals("Should not be a proxy for current branch", false, org.IsProxyOrgOfAnyBranchOnly(true));
			AssertEquals("Should be a proxy for current branch", true, org.IsProxyOrgOfAnyBranchOnly(false));
			currentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			Factory.Save();

			var inactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
			inactiveCompany.GC_IsActive = false;
			var inactiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			inactiveBranch.GB_GC = inactiveCompany.PK;
			inactiveBranch.GB_OH_OrgProxy = org.PK;
			inactiveBranch.GB_IsActive = false;
			Factory.Save();
			AssertEquals("Should not be a proxy for inactive branch", false, org.IsProxyOrgOfAnyBranchOnly(false));

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_IsActive = true;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newCompany.PK;
			newBranch.GB_OH_OrgProxy = org.PK;
			newBranch.GB_IsActive = true;
			Factory.Save();
			AssertEquals("Should be a proxy for active branch", true, org.IsProxyOrgOfAnyBranchOnly(false));
		}

		public void TestIsProxyOrgOfAnyBranchOnly_EmptyCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var currentBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = Guid.NewGuid();
			currentBranch.GB_OH_OrgProxy = org.PK;
			currentBranch.GB_IsActive = true;
			AssertNull(currentBranch.Company);

			AssertNoExceptionThrown(() => org.IsProxyOrgOfAnyBranchOnly(true));
			AssertEquals("Should not be a proxy", false, org.IsProxyOrgOfAnyBranchOnly(false));
			ErrorReporter.Clear();
		}

		public void TestIsSameRelatedParty()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.CustomsCodes.AddNew(Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "XYZ 123");
			Assert("Not expecting same related party to match org", !org.IsInterOfficeBillingOrgNotReportableForTax);

			var relation2 = org.AllRelatedParties.AddNew();
			relation2.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			relation2.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;
			Factory.Save();
			Assert("Expecting same related party to match org", org.IsInterOfficeBillingOrgNotReportableForTax);

			org.AllRelatedParties.RemoveAndDeleteAll();
			Factory.Save();
			Assert("Not expecting same related party to match org", !org.IsInterOfficeBillingOrgNotReportableForTax);

			var relation3 = org.AllRelatedParties.AddNew();
			relation3.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			relation3.PR_OH_RelatedParty = GlbBranch.CurrentBranch.OrgProxy.PK;
			Factory.Save();
			Assert("Expecting same related party to match org", org.IsInterOfficeBillingOrgNotReportableForTax);
		}

		public void TestIsInterOfficeBillingOrgNotReportableForTax()
		{
			OrgHeader orgWithSameTaxRegDetailsAsCurrentCompany = Factory.NewWithValidTestData<OrgHeader>();
			orgWithSameTaxRegDetailsAsCurrentCompany.CustomsCodes.AddNew(ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "XYZ 123");
			OrgHeader branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();

			Assert("Doesn't have same tax reg details and isn't org proxy", !orgWithSameTaxRegDetailsAsCurrentCompany.IsInterOfficeBillingOrgNotReportableForTax);
			Assert("Doesn't have same tax reg details and isn't org proxy", !branchOrgProxy.IsInterOfficeBillingOrgNotReportableForTax);

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "X Y Z 123";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();

			Assert("Has same tax reg details", orgWithSameTaxRegDetailsAsCurrentCompany.IsInterOfficeBillingOrgNotReportableForTax);
			Assert("Is Org Proxy", branchOrgProxy.IsInterOfficeBillingOrgNotReportableForTax);
			Assert("Is Company Org Proxy", GlbCompany.CurrentCompany.OrgProxy.IsInterOfficeBillingOrgNotReportableForTax);
		}

		public void TestGetLCMarginPercentagesForFallBack()
		{
			company.MiscServ.OM_LandedCostMarginPercent1 = 0.2m;
			company.MiscServ.OM_LandedCostMarginPercent2 = 0.3m;
			company.MiscServ.OM_LandedCostMarginPercent3 = 0.4m;

			LCMarginPercentages result = company.GetLCMarginPercentagesForFallBack();
			AssertEquals("OM_LandedCostMarginPercent1", 0.2m, result.LCMarginPercentage1);
			AssertEquals("OM_LandedCostMarginPercent2", 0.3m, result.LCMarginPercentage2);
			AssertEquals("OM_LandedCostMarginPercent3", 0.4m, result.LCMarginPercentage3);
		}

		#endregion

		#region Test Logs

		public void TestBusinessObjectsWithRelatedEvents()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertCollectionContains("Business objects with related logs should contain", org.MiscServ, org.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Business objects with related logs should contain", org.MainAddress, org.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Business objects with related logs should contain", org.CompanyData, org.BusinessObjectsWithRelatedEvents);

			OrgAddress address = org.Addresses.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", address, org.BusinessObjectsWithRelatedEvents);

			OrgContact contact = org.Contacts.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", contact, org.BusinessObjectsWithRelatedEvents);

			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", link, org.BusinessObjectsWithRelatedEvents);

			OrgSupplierBuyerLink link2 = org.BuyerLinks.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", link2, org.BusinessObjectsWithRelatedEvents);

			OrgSales sales = org.SalesCollection.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", sales, org.BusinessObjectsWithRelatedEvents);

			OrgPatternMatchOverride matchOverride = org.CreatePatternMatchOverrideForTest();
			AssertCollectionContains("Business objects with related logs should contain", matchOverride, org.BusinessObjectsWithRelatedEvents);

			OrgCusCode code = org.CustomsCodes.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", code, org.BusinessObjectsWithRelatedEvents);

			OrgCustomLabels labels = org.CustomLabels.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", labels, org.BusinessObjectsWithRelatedEvents);

			OrgTradeProspect client = org.Clients.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", client, org.BusinessObjectsWithRelatedEvents);

			OrgStaffAssignments assignment = org.StaffAssignments.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", assignment, org.BusinessObjectsWithRelatedEvents);

			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", opp, org.BusinessObjectsWithRelatedEvents);

			OpportunityProcessTasks task = (OpportunityProcessTasks)opp.WorkflowItems.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", task, org.BusinessObjectsWithRelatedEvents);

			OrgAppointedAgentPorts carrAppAgPorts = org.CarrierAppointedAgentPorts_AirCTO.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", carrAppAgPorts, org.BusinessObjectsWithRelatedEvents);

			OrgAppointedAgentPorts appAgPorts = org.AppointedAgentPorts.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", appAgPorts, org.BusinessObjectsWithRelatedEvents);

			OrgAppointedAgentPorts appGatewayAgPorts = org.AppointedGatewayAgentPorts.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", appGatewayAgPorts, org.BusinessObjectsWithRelatedEvents);

			OrgAgentRelationship relation = org.AgentRelationships.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", relation, org.BusinessObjectsWithRelatedEvents);

			OrgLandedCostingPrefs prefs = org.LandedCostingPreferences.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", prefs, org.BusinessObjectsWithRelatedEvents);

			OrgLandedCostingPrefCharges prefCharges = prefs.Charges.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", prefCharges, org.BusinessObjectsWithRelatedEvents);

			OrgBrandOrRelatedName name = org.BrandsOrRelatedNames.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", name, org.BusinessObjectsWithRelatedEvents);

			OrgCustomLabels formLabel = org.CustomFormLabels.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", formLabel, org.BusinessObjectsWithRelatedEvents);

			OrgCustomLabels documentLabels = org.CustomDocumentLabels.AddNew();
			AssertCollectionContains("Business objects with related logs should contain", documentLabels, org.BusinessObjectsWithRelatedEvents);
		}

		public void TestCFXConfigurationAndExRateConfigurationIsIncludedInBusinessObjectsWithRelatedEvents()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, GlbBranch.CurrentBranch.PK.ToGuid()));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = org.PK;
			orgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			orgCompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			orgCompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			company.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.CurrencyCodes.Afghanistan, 5m, 0.1m);
			branch.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.CurrencyCodes.Afghanistan, 5m, 0.1m);
			orgCompanyData.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, Constants.CurrencyCodes.Afghanistan, 5m, 0.1m);

			var companyLevelCFX = orgCompanyData.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(x => x.Level == AccCFXConfigurationLevelEnum.Company);
			AssertNotNull("Company level CFX uplift", companyLevelCFX);
			var branchLevelCFX = orgCompanyData.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(x => x.Level == AccCFXConfigurationLevelEnum.Branch);
			AssertNotNull("Branch level CFX uplift", branchLevelCFX);
			var orgLevelCFX = orgCompanyData.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(x => x.Level == AccCFXConfigurationLevelEnum.Organisation);
			AssertNotNull("Organisation level CFX uplift", orgLevelCFX);

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.Load();
			systemLevelExchangeRateConfigs.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			company.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			creditorGroup.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			debtorGroup.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Domestic, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			orgCompanyData.AccAPExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			orgCompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Sea, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var systemLevelAPExRateConfig = orgCompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);
			AssertNotNull("System level AP Ex Rate Config", systemLevelAPExRateConfig);
			var systemLevelARExRateConfig = orgCompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);
			AssertNotNull("System level AR Ex Rate Config", systemLevelARExRateConfig);

			var companyLevelAPExRateConfig = orgCompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);
			AssertNotNull("Company level AP Ex Rate Config", companyLevelAPExRateConfig);
			var companyLevelARExRateConfig = orgCompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);
			AssertNotNull("Company level AR Ex Rate Config", companyLevelARExRateConfig);

			var creditorGroupLevelExRateConfig = orgCompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup);
			AssertNotNull("Creditor Group level Ex Rate Config", creditorGroupLevelExRateConfig);
			var debtorGroupLevelExRateConfig = orgCompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup);
			AssertNotNull("Debtor group level Ex Rate Config", debtorGroupLevelExRateConfig);

			var orgLevelAPExRateConfig = orgCompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Creditor);
			AssertNotNull("Organisation level AP Ex Rate Config", orgLevelAPExRateConfig);
			var orgLevelARExRateConfig = orgCompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Debtor);
			AssertNotNull("Organisation level AR Ex Rate Config", orgLevelARExRateConfig);

			var bizObjsWithRelatedEvents = org.BusinessObjectsWithRelatedEvents;

			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain company level CFX.", companyLevelCFX, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain branch level CFX.", branchLevelCFX, bizObjsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents must contain organisation level CFX.", orgLevelCFX, bizObjsWithRelatedEvents);

			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain system level Ex Rate config.", systemLevelAPExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain system level Ex Rate config.", systemLevelARExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain company level Ex Rate config.", companyLevelAPExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain company level Ex Rate config.", companyLevelARExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain creditor group level Ex Rate config.", creditorGroupLevelExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain debtor group level Ex Rate config.", debtorGroupLevelExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents must contain organisation level AP Ex Rate config.", orgLevelAPExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents must contain organisation level AR Ex Rate config.", orgLevelARExRateConfig, bizObjsWithRelatedEvents);
		}

		public void TestLogsAddedForSwitchingOH_Category()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Category = OrgConstants.Category.Business;
			Factory.Save();
			AssertEquals("No logs about at the start", 0, org.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Organization category was changed from")).Length);

			org.OH_Category = OrgConstants.Category.Government;
			Factory.Save();
			AssertEquals("there should be 1 log", 1, org.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Organization category was changed from: BUS to: GOV")).Length);
		}

		public void TestLogsAddedForSwitchingOH_IsActive()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			AssertEquals("No logs about at the start", 0, org.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, (NoResString)"Organization was marked as" + " " + (NoResString)"inactive")).Length);
			AssertEquals("Org should be marked as active", 0, org.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, (NoResString)"Organization was marked as" + " " + (NoResString)"active")).Length);

			org.OH_IsActive = false;
			Factory.Save();
			AssertEquals("Org should be marked as inactive", 1, org.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, (NoResString)"Organization was marked as" + " " + (NoResString)"inactive")).Length);
			AssertEquals("No logs should be added because this is a new org", 0, org.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, (NoResString)"Organization was marked as" + " " + (NoResString)"active")).Length);

			org.OH_IsActive = true;
			Factory.Save();

			OrgHeader org1 = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			org1.OH_IsActive = false;
			org1.Factory.Save();
			AssertEquals("Log should be added", 2, org1.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, (NoResString)"Organization was marked as" + " " + (NoResString)"inactive")).Length);
			AssertEquals("Log should be added at previous save", 1, org1.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, (NoResString)"Organization was marked as" + " " + (NoResString)"active")).Length);

			OrgHeader org2 = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			org2.Factory.Save();
			AssertEquals("No Logs should be added", 2, org2.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, (NoResString)"Organization was marked as" + " " + (NoResString)"inactive")).Length);

			org2.OH_IsActive = true;
			org2.Factory.Save();
			AssertEquals("Log should be added", 2, org2.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, (NoResString)"Organization was marked as" + " " + (NoResString)"active")).Length);
		}

		#endregion

		#region Test Defaults

		public void TestDefaultRelatedParties()
		{
			OrgHeader relatedOrg = Factory.New<OrgHeader>();
			OrganisationsDataRegistry.Instance.ImportSeaBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, relatedOrg.PK.ToGuid());
			OrganisationsDataRegistry.Instance.ImportAirBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, relatedOrg.PK.ToGuid());
			OrganisationsDataRegistry.Instance.ExportSeaBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, relatedOrg.PK.ToGuid());
			OrganisationsDataRegistry.Instance.ExportAirBroker.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, relatedOrg.PK.ToGuid());

			OrgHeader org = Factory.New<OrgHeader>();

			AssertRelatedParty(relatedOrg, GlbCompany.CurrentCompany, org.AllRelatedParties, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			AssertRelatedParty(relatedOrg, GlbCompany.CurrentCompany, org.AllRelatedParties, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			AssertRelatedParty(relatedOrg, GlbCompany.CurrentCompany, org.AllRelatedParties, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			AssertRelatedParty(relatedOrg, GlbCompany.CurrentCompany, org.AllRelatedParties, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
		}

		void AssertRelatedParty(OrgHeader expectedOrg, GlbCompany expectedCompany, OrgRelatedPartyCompanySpecificCollection parties, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			var party = parties.GetRelatedParty(partyType, direction, transportMode, containerMode);
			AssertEquals("PR_OH_RelatedParty", expectedOrg.PK, party.PR_OH_RelatedParty);
			AssertEquals("PR_GC", expectedCompany.PK, party.PR_GC);
		}

		public void TestDefaultLanguage()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			Assert("Default language is English", org.OH_Language.Equals(Core.Constants.Languages.English));
		}

		#endregion

		#region Related Business Objects

		#region OrgCompanyData

		public void TestOrgDataReturnsValidObject()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader org = factory.New<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "Test Org Address";

			AssertNotNull("OrgData not null", org.CompanyData);
			AssertEquals("OrgData.OB_GC", GlbCompany.CurrentCompany.PK, org.CompanyData.OB_GC);

			GlbCompany newCompany = factory.New<GlbCompany>();
			OrgCompanyData data1 = factory.New<OrgCompanyData>();
			data1.OB_GC = newCompany.PK;
			data1.OB_OH = org.PK;

			factory.Save();

			OrgHeaderForTest testCompany = factory.Load<OrgHeaderForTest>(org.PK);
			AssertNotNull("OrgDataCollection.Count", testCompany.CompanyData);
			ZBool currentCompanyDataExists = testCompany.CompanyData.OB_GC == GlbCompany.CurrentCompany.PK;
			Assert("CompanyData for CurrentCompany exists", currentCompanyDataExists);
		}

		public void TestCompanyDataRegistered()
		{
			var header = Factory.New<OrgHeader>();
			Assert(!header.HasChanges);
			var companyData = header.CompanyData;
			Assert(!header.HasChanges);
			companyData.OB_APPaymentTermDays = new ZByte(1);
			Assert(header.HasChanges);
		}

		public void TestCompanyDataIsAlwaysForCurrentCompany()
		{
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch newCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			newCompanyBranch.GB_GC = newCompany.PK;

			Factory.Save();

			var header = Factory.New<OrgHeader>();
			AssertNotNull("CompanyData", header.CompanyData);
			AssertEquals("CompanyData for CurrentCompany", GlbCompany.CurrentCompany.PK, header.CompanyData.OB_GC);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, newCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				AssertEquals("Set as CurrentCompany", newCompany.PK, GlbCompany.CurrentCompany.PK);
				AssertNotNull("CompanyData", header.CompanyData);
				AssertEquals("CompanyData for CurrentCompany", GlbCompany.CurrentCompany.PK, header.CompanyData.OB_GC);
			}

			AssertNotNull("CompanyData", header.CompanyData);
			AssertEquals("CompanyData for CurrentCompany", GlbCompany.CurrentCompany.PK, header.CompanyData.OB_GC);
		}

		public void TestCompanyDataWhenDeleted()
		{
			var header = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				AssertNotNull("before delete, CompanyData is not null", header.CompanyData);
				header.Delete();
				AssertNull("after delete, CompanyData is null", header.CompanyData);
			});
		}

		public void TestNoErrorsWhenReloadCompanyData_OrgHeaderIsInactive()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			newCompanyBranch.GB_GC = newCompany.PK;
			Factory.Save();

			var org = new ConsigneeCollection(Factory).AddNew();
			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "Test Org Address";
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, newCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				org.OH_IsActive = false;
				AssertNoErrors(org.CompanyData.OB_OHInfo);
				AssertHasWarning(org.CompanyData.OB_OHInfo, "This Organisation is inactive.");
			}
		}

		public void TestCompanyDataCollectionGetCompanyDataForGlbCompany()
		{
			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
			Assert("should be more than one company", companies.Count > 0);

			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals("Org data collection count should be 0 to begin with", 0, org.CompanyDataCollection.Count);

			foreach (GlbCompany company in companies)
			{
				AssertNotNull("Should not be null", org.GetCompanyDataForGlbCompany(company));
				AssertCollectionContains("should be added to collection", org.GetCompanyDataForGlbCompany(company), org.CompanyDataCollection);
			}
		}

		public void TestCompanyDataLoadOnly()
		{
			var orgHeader = Factory.New<OrgHeader>();
			AssertNull(orgHeader.CompanyDataLoadOnly);
			AssertEquals(0, orgHeader.CompanyDataCollection.Count);

			var newCompanyData = orgHeader.CompanyData;
			AssertEquals(newCompanyData, orgHeader.CompanyDataLoadOnly);
			AssertEquals(1, orgHeader.CompanyDataCollection.Count);
		}

		#endregion

		public void TestAddress_ListWithNoMainAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address1 = Factory.New<OrgAddress>();
			address1.OA_OH = org.PK;
			address1.OA_IsActive = true;
			address1.OA_City = "city";

			var address2 = Factory.New<OrgAddress>();
			address2.OA_OH = org.PK;
			address2.OA_IsActive = true;
			address2.OA_City = "city";

			AssertEquals("Pre-condition: Just 2 addresses before generating item during enumeration", 2, org.AddressesActive.Count);

			AssertNoExceptionThrown(() =>
			{
				var addresses = org.Address_List.List;
			});

			CombineAssertions("Main address generated and added in list during enumeration", () =>
			{
				AssertEquals(3, org.AddressesActive.Count);
				AssertEquals("***Address Not On File***",
						org.AddressesActive.Except(new OrgAddress[] { address1, address2 }).Single().AddressDescription);
				AssertEquals("The new generated address is main Address", "***Address Not On File***", org.MainAddress.AddressDescription);
			});
		}

		public void TestDeletingOrgDeletesCompanyData()
		{
			BusinessObjectFactory companyFactory = new BusinessObjectFactory();
			BusinessObjectFactory testFactory = new BusinessObjectFactory();

			GlbCompany currentCompany = companyFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbCompany singCompany = companyFactory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "SIN");
			GlbCompany demCompany = companyFactory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "XXXXXX";
			org.MainAddress.OA_Address1 = "Some text";
			org.MainAddress.OA_City = "Some City";
			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";
			AccAPAccountDetails accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_AccountName = "1234";

			Factory.Save();
			AssertEquals("Header Saved To DB", 1, testFactory.GetDatabaseCount(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, org.PK)));
			AssertEquals("1 Company Data for Org", 1, testFactory.GetDatabaseCount(typeof(OrgCompanyData), new ZQuery(OrgCompanyDataSchema.OB_OH, org.PK)));

			OrgCompanyData companyData = testFactory.New<OrgCompanyData>();
			companyData.OB_GC = currentCompany.PK == singCompany.PK ? demCompany.PK : singCompany.PK;
			companyData.OB_OH = org.PK;

			testFactory.Save();
			AssertEquals("Same Header in DB", 1, testFactory.GetDatabaseCount(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, org.PK)));
			AssertEquals("2 Company Data for Org", 2, testFactory.GetDatabaseCount(typeof(OrgCompanyData), new ZQuery(OrgCompanyDataSchema.OB_OH, org.PK)));

			ZGuid orgPK = org.PK;
			org.Delete();
			Factory.Save();

			AssertEquals("No Header in DB", 0, testFactory.GetDatabaseCount(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, orgPK)));
			AssertEquals("0 Company Data for Org", 0, testFactory.GetDatabaseCount(typeof(OrgCompanyData), new ZQuery(OrgCompanyDataSchema.OB_OH, orgPK)));
		}

		public void TestDeleteOrgDeletesAgentRelationships()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgAgentRelationship relationship = org.AgentRelationships.AddNew();
			Factory.Save();

			AssertNotNull("Agent relationships should exist", new BusinessObjectFactory().Load<OrgAgentRelationship>(relationship.PK));

			org.Delete();
			Factory.Save();

			AssertNull("Agent relationships should be deleted", new BusinessObjectFactory().Load<OrgAgentRelationship>(relationship.PK));
		}

		public void TestDeletingOrgDeletesOrgSales()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAABBBCCC";
			org1.MainAddress.OA_Address1 = "Some text";
			org1.MainAddress.OA_City = "Some City";
			org1.OH_FullName = "Test Org";
			org1.OH_RL_NKClosestPort = "AUSYD";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "DDDEEEFFF";
			org2.MainAddress.OA_Address1 = "Some text";
			org2.MainAddress.OA_City = "Some City";
			org2.OH_FullName = "Test Org 2";
			org2.OH_RL_NKClosestPort = "NZAKL";

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var tradeLane1 = Factory.NewWithValidTestData<OrgSales>();
			tradeLane1.OW_OriginID = ausyd.PK;
			tradeLane1.OW_OriginTableCode = RefUNLOCOSchema.Constants.Prefix;
			tradeLane1.OW_DestinationID = nzakl.PK;
			tradeLane1.OW_DestinationTableCode = RefUNLOCOSchema.Constants.Prefix;
			tradeLane1.OW_OH_Buyer = org1.PK;

			var tradeLane2 = Factory.NewWithValidTestData<OrgSales>();
			tradeLane2.OW_OriginID = ausyd.PK;
			tradeLane2.OW_OriginTableCode = RefUNLOCOSchema.Constants.Prefix;
			tradeLane2.OW_DestinationID = nzakl.PK;
			tradeLane2.OW_DestinationTableCode = RefUNLOCOSchema.Constants.Prefix;
			tradeLane2.OW_OH_Supplier = org1.PK;

			var tradeLane3 = Factory.NewWithValidTestData<OrgSales>();
			tradeLane3.OW_IsTraded = true;
			tradeLane3.OW_OriginID = ausyd.PK;
			tradeLane3.OW_OriginTableCode = RefUNLOCOSchema.Constants.Prefix;
			tradeLane3.OW_DestinationID = nzakl.PK;
			tradeLane3.OW_DestinationTableCode = RefUNLOCOSchema.Constants.Prefix;
			tradeLane3.OW_OH_Buyer = org2.PK;

			var tradeDetail3 = tradeLane3.TradeDetails.AddNew();
			tradeDetail3.PA_TradeMode = "AIR";

			var tradePeriod3a = tradeDetail3.TradedPeriods.AddNew();
			tradePeriod3a.PAS_OH_Client = org1.PK;
			tradePeriod3a.PAS_Period = new ZDate(2017, 11, 1);

			var tradePeriod3b = tradeDetail3.TradedPeriods.AddNew();
			tradePeriod3b.PAS_OH_Client = org2.PK;
			tradePeriod3b.PAS_Period = new ZDate(2017, 11, 1);

			Factory.Save();

			org1.Delete();

			Assert(tradeLane1.IsDeleted);
			Assert(tradeLane2.IsDeleted);
			Assert(!tradeLane3.IsDeleted);
			Assert(tradePeriod3a.IsDeleted);
			Assert(!tradePeriod3b.IsDeleted);
		}

		public void TestMiscServReturnsValidObject()
		{
			AssertNotNull(company.MiscServ);
			AssertEquals("OM_OH", company.PK, company.MiscServ.OM_OH);
		}

		public void TestReloadOrgMiscServSaveMiscServToDatabase()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Will not save MiscServ to database if OrgHeader is not in database", org1.MiscServ.IsInDatabase, false);

			Factory.Save();
			AssertEquals("Will save MiscServ to database", org1.MiscServ.IsInDatabase, true);

			var newFactory = Factory.CreateNewFactory();
			var org1FromNewFactory = newFactory.Load<OrgHeader>(org1.PK);
			AssertEquals("Will not save new MiscServ to database if OrgHeader and MiscServ is already in database", org1.MiscServ.PK, org1FromNewFactory.MiscServ.PK);
		}

		public void TestReloadOrgMiscServWhenOrgIsDeletedFromAnotherFactory()
		{
			// Arrange
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			var factory1 = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};
			var factory2 = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};
			org = factory1.Load<OrgHeader>(org.PK);
			var orgFromAnotherFactory = factory2.Load<OrgHeader>(org.PK);
			orgFromAnotherFactory.Delete();
			factory2.Save();
			Assert(org.IsInDatabase);
			// Act & Assert
			OrgMiscServ miscServ = null;
			AssertNoExceptionThrown(() => miscServ = org.MiscServ);
			AssertNotNull(miscServ);
		}

		public void TestDeleteOrgWithRequiredDocumentsDeleted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var document = org.RequiredDocuments.AddNew();
			Factory.Save();

			AssertEquals("Required documents should contain 1 records", 1, org.RequiredDocuments.Count);
			AssertNotNull("Required document should exist", new BusinessObjectFactory().Load<JobRequiredDocument>(document.PK));

			org.Delete();
			Factory.Save();

			AssertNull("Required documents should be deleted", new BusinessObjectFactory().Load<JobRequiredDocument>(document.PK));
		}

		public void TestDeletingOrgDeletesCompetitors()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgCompetitor = Factory.NewWithValidTestData<OrgHeader>();
			var competitor1 = Factory.NewWithValidTestData<OrgCompetitor>();
			competitor1.OCP_OH_Parent = org.PK;
			competitor1.OCP_OH_Competitor = orgCompetitor.PK;
			competitor1.OCP_Type = "CMB";

			var competitor2 = Factory.NewWithValidTestData<OrgCompetitor>();
			competitor2.OCP_OH_Parent = orgCompetitor.PK;
			competitor2.OCP_OH_Competitor = org.PK;
			competitor2.OCP_Type = "CMB";

			Factory.Save();

			org.Delete();

			Assert(!orgCompetitor.IsDeleted);
			Assert(competitor1.IsDeleted);
			Assert(competitor2.IsDeleted);
		}

		#region CusBondDetailCollection

		public void TestCusBondDetailCollection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusBondInfo = org.CusBondDetails.AddNew();
			Factory.Save();

			AssertEquals("CusBondDetails should contain 1 records", 1, org.CusBondDetails.Count);
			AssertNotNull("CusBondInfo should exist", new BusinessObjectFactory().Load<CusBondDetail>(cusBondInfo.PK));

			org.CusBondDetails.DeleteAll();
			Factory.Save();

			AssertNull("CusBondDetails should be deleted", new BusinessObjectFactory().Load<CusBondDetail>(cusBondInfo.PK));
		}

		#endregion

		#region CustomsCodes

		public void TestLocalSupplierCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgCusCode supplierCode = Factory.New<OrgCusCode>();
			supplierCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			supplierCode.OK_RN_NKCodeCountry = "AU";
			supplierCode.OK_CustomsRegNo = "0059781A";
			supplierCode.OK_OH = header.PK;
			AssertEquals("Able to get the Supplier Code", "0059781A", header.LocalCustomsSupplierCode);
		}

		public void TestCustomsClientID()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgCusCode importerID = Factory.New<OrgCusCode>();
			importerID.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			importerID.OK_RN_NKCodeCountry = "AU";
			importerID.OK_CustomsRegNo = "12345678901";
			importerID.OK_OH = header.PK;
			AssertEquals("Able to get the Importer ID for CMR", "12345678901", header.CustomsClientID);
		}

		public void TestLocalCustomsClientCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "12345");
			AssertEquals("12345", header.LocalCustomsClientCode);
		}

		public void TestLocalCustomsCarrierCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "12345");
			AssertEquals("12345", header.LocalCustomsCarrierCode);
		}

		public void TestCarrierCode_CountryIsUS()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "USXX");
				AssertEquals("USXX", header.SCACCode);
			}
		}

		public void TestCarrierCode_CountryIsNotUS()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AUXX");
				AssertEquals("", header.SCACCode);
			}
		}

		public void TestCarrierC1CCode_CountryIsUS()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "USXX");
				AssertEquals("USXX", header.C1CCode);
			}
		}

		public void TestCarrierC1CCode_CountryIsNotUS()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "AUXX");
				AssertEquals("", header.C1CCode);
			}
		}

		public void TestBondHolderLocalCustomsCarrierCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				header.SetLocalCustomsCode(OrgCusCode.CodeTypes.BondHolderCode, "12345");
				AssertEquals("12345", header.BondHolderLocalCustomsCarrierCode);
			}
		}

		public void TestLocalReleaseAgentCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				header.SetLocalCustomsCode(OrgCusCode.CodeTypes.ReleaseAgentCode, "12345");
				AssertEquals("12345", header.LocalReleaseAgentCode);
			}
		}

		public void TestLocalRebateUserCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				header.SetLocalCustomsCode(OrgCusCode.CodeTypes.RebateUserCode, "12345");
				AssertEquals("12345", header.LocalRebateUserCode);
			}
		}

		public void TestLocalVATCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				header.SetLocalCustomsCode(OrgCusCode.CodeTypes.VATCode, "12345");
				AssertEquals("12345", header.LocalVATCode);
			}
		}

		public void TestLocalLocalManifestID()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.ManifestProviderID, "12345");
			AssertEquals("12345", header.LocalManifestID);
		}

		public void TestLocalPrincipalID()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "12345");
			AssertEquals("12345", header.LocalPrincipalID);
		}

		public void TestLocalBusinessRegNo()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.LocalBusinessRegNo = "12345";
			AssertEquals("12345", header.LocalBusinessRegNo);
		}

		public void TestLegacyCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.LegacySystemCode, "ABCDE");
			AssertEquals("ABCDE", header.LegacyCode);
		}

		public void TestRoadCarrierRegistration()
		{
			var header = Factory.New<OrgHeader>();
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.RoadCarrierRegistration, "RCR12345");
			AssertEquals("RCR12345", header.RoadCarrierRegistrationNumber);
		}

		#endregion

		#region Contacts

		public void TestHasUniqueContactName()
		{
			var org = Factory.New<OrgHeader>();

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Apple";
			AssertEquals(true, org.HasUniqueContactName(contact1));

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Apple";
			AssertEquals(false, org.HasUniqueContactName(contact1));
			AssertEquals(false, org.HasUniqueContactName(contact2));

			contact2.OC_ContactName = "Banana";
			AssertEquals(true, org.HasUniqueContactName(contact1));
			AssertEquals(true, org.HasUniqueContactName(contact2));

			var contactAddedFromCollectionView = org.FilteredContacts.AddNew();
			contactAddedFromCollectionView.OC_ContactName = "Carrot";
			AssertEquals(true, org.HasUniqueContactName(contactAddedFromCollectionView));

			var uncommittedContactAddedFromCollectionView = (OrgContact)((IBindingList)org.FilteredContacts).AddNew();
			uncommittedContactAddedFromCollectionView.OC_ContactName = "Egg";
			AssertEquals(true, org.HasUniqueContactName(uncommittedContactAddedFromCollectionView));

			uncommittedContactAddedFromCollectionView.OC_ContactName = "Apple";
			((ICancelAddNew)org.FilteredContacts).EndNew(((IList)org.FilteredContacts).IndexOf(uncommittedContactAddedFromCollectionView));
			AssertEquals(false, org.HasUniqueContactName(uncommittedContactAddedFromCollectionView));
			AssertEquals(false, org.HasUniqueContactName(contact1));

			org.FilteredContacts.RemoveAndDelete(uncommittedContactAddedFromCollectionView);
			AssertEquals(true, org.HasUniqueContactName(contact1));
		}

		public void TestContactsReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader Contacts count", 0, company.Contacts.Count);
		}

		public void TestContactsActiveReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader Contacts count", 0, company.ContactsActive.Count);
		}

		public void TestContactsActive()
		{
			company.OH_FullName = "Some Test Org";
			company.OH_RL_NKClosestPort = "USBTV";
			company.MainAddress.OA_Address1 = "Address";

			var orgContact1 = company.Contacts.AddNew();
			orgContact1.OC_ContactName = "John Doe";
			var orgContact2 = company.Contacts.AddNew();
			orgContact2.OC_ContactName = "John Smith";

			orgContact1.OC_IsActive = ZBool.True;
			orgContact2.OC_IsActive = ZBool.False;

			Factory.Save();

			AssertEquals("ContactsActive", 1, company.ContactsActive.Count);
			AssertCollectionContains("ContactsActive", orgContact1, company.ContactsActive);
			AssertCollectionNotContains("ContactsActive", orgContact2, company.ContactsActive);
		}

		public void TestContactsFilter()
		{
			company.OH_FullName = "Some Test Org";
			company.OH_RL_NKClosestPort = "USBTV";
			company.MainAddress.OA_Address1 = "Address";

			var orgContact1 = company.Contacts.AddNew();
			orgContact1.OC_ContactName = "Mary Johnston";

			var orgContact2 = company.Contacts.AddNew();
			orgContact2.OC_ContactName = "John Smith";

			var orgContact3 = company.Contacts.AddNew();
			orgContact3.OC_ContactName = "Bob JOHN";

			var orgContact4 = company.Contacts.AddNew();
			orgContact4.OC_ContactName = "Jonny.Wiggles";

			var orgContact5 = company.Contacts.AddNew();
			orgContact5.OC_ContactName = "Sam";
			orgContact5.OC_Title = "John";

			var orgContact6 = company.Contacts.AddNew();
			orgContact6.OC_ContactName = "Fred";
			orgContact6.OC_Email = "mrjohn@whateversomething.moc";

			var orgContact7 = company.Contacts.AddNew();
			orgContact7.OC_ContactName = "Jane";
			orgContact7.OC_Phone = "0123JOHN456";

			var orgContact8 = company.Contacts.AddNew();
			orgContact8.OC_ContactName = "Jack";
			orgContact8.OC_Mobile = "0123jOhN456";

			var orgContact9 = company.Contacts.AddNew();
			orgContact9.OC_ContactName = "Janice";
			orgContact9.OC_HomePhone = "JOHN.JOHN.JOHN";

			Factory.Save();

			company.FilteredContacts.FilterString = "";
			AssertEquals("All contacts should be included", 9, company.FilteredContacts.Count);

			company.FilteredContacts.FilterString = "John";
			AssertEquals("Only matching contacts should be included", 8, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact1, company.FilteredContacts);
			AssertCollectionContains(orgContact2, company.FilteredContacts);
			AssertCollectionContains(orgContact3, company.FilteredContacts);
			AssertCollectionNotContains(orgContact4, company.FilteredContacts);
			AssertCollectionContains(orgContact5, company.FilteredContacts);
			AssertCollectionContains(orgContact6, company.FilteredContacts);
			AssertCollectionContains(orgContact7, company.FilteredContacts);
			AssertCollectionContains(orgContact8, company.FilteredContacts);
			AssertCollectionContains(orgContact9, company.FilteredContacts);

			company.FilteredContacts.FilterString = ".";
			AssertEquals("Only matching contacts should be included", 3, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact4, company.FilteredContacts);
			AssertCollectionContains(orgContact6, company.FilteredContacts);
			AssertCollectionContains(orgContact9, company.FilteredContacts);
		}

		public void TestAllocatedContactFilterOption()
		{
			var orgContactNZC = company.Contacts.AddNew();
			orgContactNZC.OC_ContactName = "TestContact1";
			var personalAlloc1 = orgContactNZC.Allocations.AddNew();
			personalAlloc1.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;

			var orgContactCAP = company.Contacts.AddNew();
			orgContactCAP.OC_ContactName = "TestContact2";
			var personalAlloc2 = orgContactCAP.Allocations.AddNew();
			personalAlloc2.PC_Type = OrgConstants.ContactAllocationType.CAPGA;

			var orgContactCNC = company.Contacts.AddNew();
			orgContactCNC.OC_ContactName = "TestContact3";
			var personalAlloc3 = orgContactCNC.Allocations.AddNew();
			personalAlloc3.PC_Type = OrgConstants.ContactAllocationType.CNCUS;

			var orgContactNZB = company.Contacts.AddNew();
			orgContactNZB.OC_ContactName = "TestContact4";
			var personalAlloc4 = orgContactNZB.Allocations.AddNew();
			personalAlloc4.PC_Type = OrgConstants.ContactAllocationType.NZBiosecurity;

			var orgContactUSF = company.Contacts.AddNew();
			orgContactUSF.OC_ContactName = "TestContact5";
			var personalAlloc5 = orgContactUSF.Allocations.AddNew();
			personalAlloc5.PC_Type = OrgConstants.ContactAllocationType.USFSV;

			var orgContactUSP = company.Contacts.AddNew();
			orgContactUSP.OC_ContactName = "TestContact6";
			var personalAlloc6 = orgContactUSP.Allocations.AddNew();
			personalAlloc6.PC_Type = OrgConstants.ContactAllocationType.USPGA;

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableControllingContactFilters.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				company.FilteredContacts.FilterOption = OrgHeaderLookups.LookupConstants.Contacts;
				company.FilteredContacts.FilterString = "";
				AssertEquals(6, company.FilteredContacts.Count);

				company.FilteredContacts.FilterString = "Contact";
				AssertEquals(6, company.FilteredContacts.Count);

				company.FilteredContacts.FilterString = "1";
				AssertEquals(1, company.FilteredContacts.Count);
				AssertCollectionContains(orgContactNZC, company.FilteredContacts);

				company.FilteredContacts.FilterOption = OrgHeaderLookups.LookupConstants.AllocatedContact;
				company.FilteredContacts.FilterString = "";
				AssertEquals(6, company.FilteredContacts.Count);

				company.FilteredContacts.FilterString = "Contact";
				AssertEquals(2, company.FilteredContacts.Count);
				AssertCollectionContains(orgContactUSF, company.FilteredContacts);

				company.FilteredContacts.FilterString = "1";
				AssertEquals(0, company.FilteredContacts.Count);

				company.FilteredContacts.FilterString = "NZC";
				AssertEquals(1, company.FilteredContacts.Count);
				AssertCollectionContains(orgContactNZC, company.FilteredContacts);

				company.FilteredContacts.FilterString = "Biosecurity";
				AssertEquals(1, company.FilteredContacts.Count);
				AssertCollectionContains(orgContactNZB, company.FilteredContacts);

				company.FilteredContacts.FilterString = "US";
				AssertEquals(4, company.FilteredContacts.Count);
				AssertCollectionContains(orgContactUSF, company.FilteredContacts);
				AssertCollectionContains(orgContactUSP, company.FilteredContacts);

				company.FilteredContacts.FilterString = "Customs";
				AssertEquals(2, company.FilteredContacts.Count);
				AssertCollectionContains(orgContactCNC, company.FilteredContacts);
				AssertCollectionContains(orgContactNZC, company.FilteredContacts);
			}
		}

		public void TestContactsFilterCanFilterFormattedPhoneNumber()
		{
			company.OH_FullName = "Some Test Org";
			company.OH_RL_NKClosestPort = "USBTV";
			company.MainAddress.OA_Address1 = "Address";

			var orgContact1 = company.Contacts.AddNew();
			orgContact1.OC_ContactName = "Mary Johnston";
			orgContact1.OC_Phone = "+12015555555";
			orgContact1.OC_Mobile = "+12013004000";
			orgContact1.OC_HomePhone = "+61212345678";
			AssertEquals("+1 201-555-5555", orgContact1.OC_Phone_Formatted);
			AssertEquals("+1 201-300-4000", orgContact1.OC_Mobile_Formatted);
			AssertEquals("+61 2 1234 5678", orgContact1.OC_HomePhone_Formatted);

			var orgContact2 = company.Contacts.AddNew();
			orgContact2.OC_ContactName = "John Smith";
			orgContact2.OC_Phone = "+13032229876";
			orgContact2.OC_Mobile = "TestInvalidData";
			orgContact2.OC_HomePhone = "";
			AssertEquals("+1 303-222-9876", orgContact2.OC_Phone_Formatted);
			AssertEquals("TestInvalidData", orgContact2.OC_Mobile_Formatted);
			AssertEquals("", orgContact2.OC_HomePhone_Formatted);

			var orgContact3 = company.Contacts.AddNew();
			orgContact3.OC_ContactName = "Owen Qiu";
			orgContact3.OC_Phone = "+8615912345239";
			orgContact3.OC_Mobile = "87112abc67890";
			orgContact3.OC_HomePhone = "a(b)/c223456";
			AssertEquals("+86 159 1234 5239", orgContact3.OC_Phone_Formatted);
			AssertEquals("87112abc67890", orgContact3.OC_Mobile_Formatted);
			AssertEquals("a(b)/c223456", orgContact3.OC_HomePhone_Formatted);

			company.FilteredContacts.FilterString = "";
			AssertEquals("All contacts should be included", 3, company.FilteredContacts.Count);

			company.FilteredContacts.FilterString = "+12015555555";
			AssertEquals(1, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact1, company.FilteredContacts);

			company.FilteredContacts.FilterString = "+1 201-300-4000";
			AssertEquals(1, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact1, company.FilteredContacts);

			company.FilteredContacts.FilterString = "+61 2 1234";
			AssertEquals(1, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact1, company.FilteredContacts);

			company.FilteredContacts.FilterString = "222-9876";
			AssertEquals(1, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact2, company.FilteredContacts);

			company.FilteredContacts.FilterString = "TESTINVALIDDATA";
			AssertEquals(1, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact2, company.FilteredContacts);

			company.FilteredContacts.FilterString = "+86 159";
			AssertEquals(1, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact3, company.FilteredContacts);

			company.FilteredContacts.FilterString = " 12-ab/c67 ";
			AssertEquals(1, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact3, company.FilteredContacts);

			company.FilteredContacts.FilterString = "223 456";
			AssertEquals(1, company.FilteredContacts.Count);
			AssertCollectionContains(orgContact3, company.FilteredContacts);

			company.FilteredContacts.FilterString = "~!#$%^&&*(";
			AssertEquals(0, company.FilteredContacts.Count);

			company.FilteredContacts.FilterString = "+1 201-300-4001";
			AssertEquals(0, company.FilteredContacts.Count);
		}

		public void TestShowSystemGenerateContacts()
		{
			company.OH_FullName = "Some Test Org";
			company.OH_RL_NKClosestPort = "USBTV";
			company.MainAddress.OA_Address1 = "Address";

			var orgContact1 = company.Contacts.AddNew();
			orgContact1.OC_ContactName = "John Doe";
			orgContact1.OC_SystemCreateUser = User.ServiceUserCode;
			var orgContact2 = company.Contacts.AddNew();
			orgContact2.OC_ContactName = "John Smith";

			Factory.Save();

			Assert("By default show all contacts", company.ShowSystemGeneratedContacts);
			company.ShowSystemGeneratedContacts = false;

			AssertEquals("Non-system contacts", 1, company.Contacts.Count);
			AssertCollectionNotContains(orgContact1, company.Contacts);
			AssertCollectionContains(orgContact2, company.Contacts);

			company.ShowSystemGeneratedContacts = true;

			AssertEquals("All contacts", 2, company.Contacts.Count);
			AssertCollectionContains(orgContact1, company.Contacts);
			AssertCollectionContains(orgContact2, company.Contacts);
		}

		public void TestAllocatedContactCollection()
		{
			company.OH_FullName = "Sydney Test Company Pty. Ltd.";
			company.OH_RL_NKClosestPort = "AUSYD";
			company.MainAddress.OA_Address1 = "575 George St.";
			company.MainAddress.OA_City = "Sydney";
			company.MainAddress.OA_State = "NSW";
			company.MainAddress.OA_PostCode = "2000";
			company.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			company.MainAddress.OA_Phone = "+61 2 80012201";
			company.MainAddress.OA_Fax = "+61 2 99999999";
			company.MainAddress.OA_Email = "admin@testcompany.com.au";

			var contact1 = company.Contacts.AddNew();
			contact1.OC_ContactName = "John Doe";
			var contact2 = company.Contacts.AddNew();
			contact2.OC_ContactName = "John Smith";

			var contact3 = company.Contacts.AddNew();
			contact3.OC_ContactName = "James Smith";
			var contact4 = company.Contacts.AddNew();
			contact4.OC_ContactName = "Henry Johnson";
			Factory.Save();
			var savedCompanyPK = company.PK;
			AssertEquals("AllocatedContacts - no contacts allocated", 0, company.AllocatedContacts.Count);

			var personalAtt = contact1.Attributes.AddNew();
			personalAtt.PC_Type = "ARL";
			var personalAlloc1 = contact2.Allocations.AddNew();
			personalAlloc1.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			Factory.Save();

			var tempFactory = new BusinessObjectFactory();
			company = tempFactory.Load<OrgHeader>(savedCompanyPK);

			AssertEquals("AllocatedContacts - 1 contact has allocation", 1, company.AllocatedContacts.Count);
			AssertEquals("Contact allocated", contact2.PK, company.AllocatedContacts[0].PK);
			AssertCollectionNotContains("This Contact should not be in collection", contact3, company.AllocatedContacts);

			var personalAlloc2 = contact3.Allocations.AddNew();
			personalAlloc2.PC_Type = OrgConstants.ContactAllocationType.NZBiosecurity;
			Factory.Save();

			tempFactory = new BusinessObjectFactory();
			company = tempFactory.Load<OrgHeader>(savedCompanyPK);

			AssertEquals("AllocatedContacts - 2 contacts now have allocations", 2, company.AllocatedContacts.Count);
			AssertEquals("Contact allocated", contact2.PK, company.AllocatedContacts[0].PK);
			AssertEquals("This Contact now has been allocated as a designated contact", contact3.PK, company.AllocatedContacts[1].PK);
		}

		public void TestAddressesActive()
		{
			company.OH_FullName = "Some Test Org";
			company.OH_RL_NKClosestPort = "USBTV";
			company.MainAddress.OA_Address1 = "Address";

			OrgAddress adr1 = Factory.New<OrgAddress>();
			OrgAddress adr2 = Factory.New<OrgAddress>();
			adr1.OA_Address1 = "address 1";
			adr2.OA_Address1 = "address 2";

			company.AddressesNoAutoCreate.Add(adr1);
			company.AddressesNoAutoCreate.Add(adr2);

			adr1.OA_IsActive = ZBool.True;
			adr2.OA_IsActive = ZBool.False;

			Factory.Save();

			AssertEquals("AddressesActive (main and adr1)", 2, company.AddressesActive.Count);
			AssertCollectionContains("AddressesActive", adr1, company.AddressesActive);
			AssertCollectionNotContains("AddressesActive", adr2, company.AddressesActive);
		}

		public void TestExchangeRateCollection()
		{
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var rate1 = RefExchangeRate.New(newFactory);
			rate1.RE_ExRateType = "SEL";
			rate1.RE_StartDate = ZDateTime.Today;
			rate1.RE_ExpiryDate = ZDateTime.Today;
			rate1.RE_SellRate = 10m;
			rate1.RE_GC = GlbCompany.CurrentCompany.PK;
			rate1.RE_RX_NKExCurrency = "AUD";
			rate1.RE_OH_Client = company.PK;

			var rate2 = RefExchangeRate.New(newFactory);
			rate2.RE_ExRateType = "SEL";
			rate2.RE_StartDate = ZDateTime.Today;
			rate2.RE_ExpiryDate = ZDateTime.Today;
			rate2.RE_SellRate = 10m;
			rate2.RE_GC = GlbCompany.CurrentCompany.PK;
			rate2.RE_RX_NKExCurrency = "USD";
			rate2.RE_OH_Client = company.PK;

			var rate3 = RefExchangeRate.New(newFactory);
			rate3.RE_ExRateType = "SEL";
			rate3.RE_StartDate = ZDateTime.Today;
			rate3.RE_ExpiryDate = ZDateTime.Today;
			rate3.RE_SellRate = 10m;
			rate3.RE_GC = GlbCompany.CurrentCompany.PK;
			rate3.RE_RX_NKExCurrency = "CNY";
			rate3.RE_OH_Client = ZGuid.Empty;

			newFactory.Save();

			AssertEquals("Should have 2 records.", 2, company.ExchangeRateCollection.Count);
			AssertNotNull("Should contain the related rate.", company.ExchangeRateCollection.FindByPK(rate1.PK));
			AssertNotNull("Should contain the related rate.", company.ExchangeRateCollection.FindByPK(rate2.PK));
			AssertNull("Shouldn't contain the unrelated rate.", company.ExchangeRateCollection.FindByPK(rate3.PK));
		}

		#endregion

		#region Addresses

		public void TestNewOrgHeaderAlwaysHasOFCAddress()
		{
			AssertEquals("Newly added OrgHeader Addresses count", 1, company.Addresses.Count);
			AssertEquals("Address[0].AddressType", ZBool.True, company.Addresses[0].AddressCapability.GetCapabilityEnabled(OrgAddressType.Office.Code));
			AssertNotNull("Always returns MainAddress", company.MainAddress);
		}

		public void TestGetAddressWithFallback()
		{
			AssertEquals("An Organisation with no additional address should return the postal address - DLV", company.MainAddress.PK, company.GetAddressWithFallback(AddressType.DLV).PK);
			AssertEquals("An Organisation with no additional address should return the postal address - PIC", company.MainAddress.PK, company.GetAddressWithFallback(AddressType.PIC).PK);
			AssertEquals("An Organisation with no additional address should return the postal address - SQM", company.MainAddress.PK, company.GetAddressWithFallback(AddressType.SQM).PK);
			AssertEquals("An Organisation with no additional address should return the postal address - APM", company.MainAddress.PK, company.GetAddressWithFallback(AddressType.APM).PK);
			AssertEquals("An Organisation with no additional address should return the postal address - ARM", company.MainAddress.PK, company.GetAddressWithFallback(AddressType.ARM).PK);
			AssertEquals("An Organisation with no additional address should return the postal address - No Default", company.MainAddress.PK, company.GetAddressWithFallback(AddressType.NoDefault).PK);

			OrgAddress padAddress = company.Addresses.AddNew();
			padAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			padAddress.OA_Address1 = "PAD ADDRESS 1";
			padAddress.OA_City = "PAD City";
			padAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			padAddress.OA_IsActive = true;

			AssertEquals("An Organisation with PAD address should return that address for DLV", padAddress.PK, company.GetAddressWithFallback(AddressType.DLV).PK);
			AssertEquals("An Organisation with PAD address should return that address for PIC", padAddress.PK, company.GetAddressWithFallback(AddressType.PIC).PK);

			OrgAddress dlvAddress = company.Addresses.AddNew();
			dlvAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			dlvAddress.OA_Address1 = "DLV Address 1";
			dlvAddress.OA_City = "DLV City";
			dlvAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);
			dlvAddress.OA_IsActive = true;

			AssertEquals("An Organisation with DLV address should return that address for DLV", dlvAddress.PK, company.GetAddressWithFallback(AddressType.DLV).PK);

			OrgAddress picAddress = company.Addresses.AddNew();
			picAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			picAddress.OA_Address1 = "PIC Address 1";
			picAddress.OA_City = "PIC City";
			picAddress.OA_IsActive = true;

			AssertEquals("An Organisation with PAD address should return that address for PIC", picAddress.PK, company.GetAddressWithFallback(AddressType.PIC).PK);

			var ecaAddress = company.Addresses.AddNew();
			ecaAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.EUCustomsAddress);
			ecaAddress.OA_Address1 = "ECA Address 1";
			ecaAddress.OA_City = "ECA City";
			ecaAddress.OA_IsActive = true;

			AssertEquals("An Organisation with PAD address should return that address for ECA", ecaAddress.PK, company.GetAddressWithFallback(AddressType.ECA).PK);
		}

		#endregion

		#region EDICommunicationModes

		public void TestEDICommunicationsModesNotNull()
		{
			AssertNotNull(company.EDICommunicationsModes);
		}

		public void TestDeleteOrgHeaderDeletesEDICommunicationsModesCollection()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsMode mode = Factory.New<EDICommunicationsMode>();
			header.Delete();
			AssertNotNull(mode);
		}

		#endregion

		public void TestAddOrgPatternMatch()
		{
			company.MainAddress.OA_Phone_Formatted = "+61426829924";
			company.MainAddress.OA_Address1 = "Address 1";
			company.MainAddress.OA_City = "City";
			company.OH_Code = "TST";
			Factory.Save();

			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(OrgPatternMatchSchema.OS_OH, company.PK);

			BusinessObject[] orgMatches = tempFactory.Load(typeof(OrgPatternMatch), filter);
			bool matchWasFound = false;
			foreach (OrgPatternMatch match in orgMatches)
			{
				if (match.OS_Phone == "6829924")
				{
					matchWasFound = true;
				}
			}

			Assert("A Pattern Match record should exist with the number encoded", matchWasFound);

			company.MainAddress.OA_Phone_Formatted = "+8615601131981";
			Factory.Save();

			tempFactory = new BusinessObjectFactory();
			orgMatches = tempFactory.Load(typeof(OrgPatternMatch), filter);
			matchWasFound = false;
			foreach (OrgPatternMatch match in orgMatches)
			{
				if (match.OS_Phone == "1131981")
				{
					matchWasFound = true;
				}
			}

			Assert("A Pattern Match record should exist with the number encoded", matchWasFound);

			company.Delete();
			Factory.Save();
			tempFactory = new BusinessObjectFactory();
			orgMatches = tempFactory.Load(typeof(OrgPatternMatch), filter);
			AssertEquals("Existing OrgPatternMatch row should be deleted", 0, orgMatches.Length);
		}

		public void TestAddDuplicateOrg()
		{
			company.OH_FullName = "Existing organisation";
			company.MainAddress.OA_Address1 = "Existing org address 1";
			company.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			OrgHeader newCompany = Factory.New<OrgHeader>();
			newCompany.OH_FullName = company.OH_FullName;
			newCompany.OH_RL_NKClosestPort = company.OH_RL_NKClosestPort;
			newCompany.MainAddress.OA_Address1 = company.MainAddress.OA_Address1;
			newCompany.MainAddress.OA_Address2 = company.MainAddress.OA_Address2;
			newCompany.MainAddress.OA_City = company.MainAddress.OA_City;
			newCompany.MainAddress.OA_Phone = company.MainAddress.OA_Phone;
			newCompany.MainAddress.OA_Fax = company.MainAddress.OA_Fax;

			newCompany.MustPerformCheckForDuplicateOrganisations = false;
			Assert("Ignore duplicate checking", !newCompany.IsLikelyDuplicate());
			newCompany.MustPerformCheckForDuplicateOrganisations = true;

			Assert("Saving a possible duplicate org", newCompany.IsLikelyDuplicate());
			Factory.Save();
			Assert("Org is already saved, should not show as a duplicate", !newCompany.IsLikelyDuplicate());

			OrgHeader nonDuplicate = Factory.New<OrgHeader>();
			nonDuplicate.OH_FullName = "NotDuplicate";
			nonDuplicate.OH_RL_NKClosestPort = "XXZZZ";
			nonDuplicate.MainAddress.OA_Address1 = "NonDuplicateAddress1";
			Assert("Organisation should not be a duplicate", !nonDuplicate.IsLikelyDuplicate());
		}

		public void TestRequiresSecurityOverrideToUpdateCountry()
		{
			var companyFR = Factory.NewWithValidTestData<GlbCompany>();
			companyFR.GC_Code = "AAA";
			companyFR.GC_Name = "AAA Company";
			companyFR.GC_RN_NKCountryCode = "FR";
			GlbBranch branchFR = companyFR.Branches.AddNew();
			branchFR.GB_Code = "ABC";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TTT";
			staff.GS_LoginName = "testuser";

			Factory.Save();

			using (Env.SetTemporaryUserContext("testuser", branchFR.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = true;

				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_Code = "ORG1";
				org1.OH_RL_NKClosestPort = "AUSYD";
				Assert("Has right > should not require security override", !org1.RequiresSecurityOverrideToUpdateCountry);

				org1.OH_RL_NKClosestPort = "FRPAR";
				Assert("Has right > should not require security override", !org1.RequiresSecurityOverrideToUpdateCountry);

				Factory.Save();
				Assert("Has right > should not require security override", !org1.RequiresSecurityOverrideToUpdateCountry);

				org1.OH_RL_NKClosestPort = "AUSYD";
				Factory.Save();
				Assert("Has right > should not require security override", !org1.RequiresSecurityOverrideToUpdateCountry);

				Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = false;

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.OH_Code = "ORG2";
				org2.OH_RL_NKClosestPort = "AUSYD";
				Assert("New org, has no right, different countries > should require security override", org2.RequiresSecurityOverrideToUpdateCountry);

				org2.OH_RL_NKClosestPort = "FRPAR";
				Assert("New org, has no right, same country > should not require security override", !org2.RequiresSecurityOverrideToUpdateCountry);

				Factory.Save();
				Assert("Existing org, has no right, same country > should not require security override", !org2.RequiresSecurityOverrideToUpdateCountry);

				org2.OH_RL_NKClosestPort = "AUSYD";
				Assert("Existing org, has no right, different countries, UNLOCO has changes > require security override", org2.RequiresSecurityOverrideToUpdateCountry);
				Factory.Save();
				Assert("Existing org, has no right, different countries, UNLOCO has no changes > should not require security override", !org2.RequiresSecurityOverrideToUpdateCountry);
			}
		}

		public void TestCheckForInvoiceOrdersDuplicatesValidation()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			AccClientInvoiceOrder line1 = company.InvoiceOrders.AddNew();
			line1.AI_AC = chargeCode1.PK;
			line1.AI_PrintOrder = 1;
			line1.AI_InvoiceType = "IT1";
			AccClientInvoiceOrder line2 = company.InvoiceOrders.AddNew();
			line2.AI_AC = chargeCode1.PK;
			line2.AI_PrintOrder = 2;
			line2.AI_InvoiceType = "IT2";
			AccClientInvoiceOrder line3 = company.InvoiceOrders.AddNew();
			line3.AI_AC = chargeCode3.PK;
			line3.AI_PrintOrder = 3;
			line3.AI_InvoiceType = "IT3";
			AccClientInvoiceOrder line4 = company.InvoiceOrders.AddNew();
			line4.AI_AC = chargeCode2.PK;
			line4.AI_PrintOrder = 1;
			line4.AI_InvoiceType = "IT1";
			AccClientInvoiceOrder line5 = company.InvoiceOrders.AddNew();
			line5.AI_AC = chargeCode3.PK;
			line5.AI_PrintOrder = 5;
			line5.AI_InvoiceType = "IT3";

			Assert("Line 1 is not valid because it has duplicate Print Order", line1.HasRowErrors);
			Assert(string.Format("Line 1 error message should be {0}", "Duplicate values of Print Order are not allowed."), line1.Notifications.GetErrors().ToUniqueMessageListString().Contains("Duplicate values of Print Order are not allowed."));
			Assert("Line 2 shouldn't have any errors", !line2.HasRowErrors);
			Assert("Line 3 is not valid because both charge code and invoice type have duplicates", line3.HasRowErrors);
			Assert(string.Format("Line 3 error message should be {0}", "Duplicate entries are not allowed."), line3.Notifications.GetErrors().ToUniqueMessageListString().Contains("Duplicate entries are not allowed."));
			Assert("Line 4 is not valid because it has duplicate Print Order", line4.HasRowErrors);
			Assert(string.Format("Line 4 error message should be {0}", "Duplicate values of Print Order are not allowed."), line4.Notifications.GetErrors().ToUniqueMessageListString().Contains("Duplicate values of Print Order are not allowed."));
			Assert("Line 5 is not valid because both charge code and invoice type have duplicates", line5.HasRowErrors);
			Assert(string.Format("Line 5 error message should be {0}", "Duplicate entries are not allowed."), line5.Notifications.GetErrors().ToUniqueMessageListString().Contains("Duplicate entries are not allowed."));
		}

		public void TestPatternMatchOverridesReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader PatternMatchOverrides count", 0, company.PatternMatchOverrides_ForBinding.Count);
		}

		public void TestSalesCollectionReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader SalesCollection count", 0, company.SalesCollection.Count);
		}

		public void TestSalesCallsReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader SalesCalls count", 0, company.SalesCalls.Count);
		}

		public void TestSupplierLinksReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader SupplierLinks count", 0, company.SupplierLinks.Count);
		}

		public void TestBuyerLinksReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader BuyerLinks count", 0, company.BuyerLinks.Count);
		}

		public void TestCustomLabelsReturnsEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader CustomLabels count", 0, company.CustomLabels.Count);
		}

		public void TestCustomFormLabelsReturnEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader CustomFormLabels count", 0, company.CustomFormLabels.Count);
		}

		public void TestCustomDocumentLabelsReturnEmptyCollection()
		{
			AssertEquals("Newly added OrgHeader CustomDocumentLabels count", 0, company.CustomDocumentLabels.Count);
		}

		public void TestUNLOCO()
		{
			company.OH_RL_NKClosestPort = "GBLON";
			AssertEquals("Valid UNLOCO code", "GBLON", company.UNLOCO.RL_Code);
			company.OH_RL_NKClosestPort = "splat";
			AssertNull("Invalid UNLOCO code", company.UNLOCO);
			company.OH_RL_NKClosestPort = string.Empty;
			AssertNull("Empty UNLOCO code", company.UNLOCO);
		}

		public void TestSalesOpportunities()
		{
			OrgHeader org = OrgHeader.New(Factory);
			AssertNotNull("Sales Opportunities not null", org.SalesOpportunities);
			AssertEquals("Precondition: No sales opportunities", 0, org.SalesOpportunities.Count);

			org.SalesOpportunities.AddNew();
			AssertEquals("1 sales opportunity", 1, org.SalesOpportunities.Count);
		}

		public void TestMostRecentSalesOpportunity()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			AssertEquals("Precondition: org.SalesOpportunities.Count", 0, org.SalesOpportunities.Count);
			AssertEquals(null, org.MostRecentSalesOpportunity);

			var salesOpportunity1 = org.SalesOpportunities.AddNew();
			Factory.Save();
			AssertEquals(salesOpportunity1, org.MostRecentSalesOpportunity);

			var salesOpportunity2 = org.SalesOpportunities.AddNew();
			Factory.Save();
			AssertEquals(salesOpportunity2, org.MostRecentSalesOpportunity);
		}

		public void TestGetStaffAssignmentsForGlbCompany()
		{
			OrgHeader org = OrgHeader.New(Factory);

			GlbCompany differentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			OrgStaffAssignments assignment1 = Factory.New<OrgStaffAssignments>();
			assignment1.O8_OH = org.PK;
			assignment1.O8_GC = GlbCompany.CurrentCompany.PK;

			OrgStaffAssignments assignment2 = Factory.New<OrgStaffAssignments>();
			assignment2.O8_OH = org.PK;
			assignment2.O8_GC = differentCompany.PK;

			OrgStaffAssignmentsCollection collection = org.StaffAssignments;
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(assignment1.PK));

			collection = org.GetStaffAssignmentsForGlbCompany(differentCompany);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(assignment2.PK));
		}

		public void TestConsigneeContainerPenalties()
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();

			AssertEquals("ConsigneeContainerPenalties.Count", 0, consignee.ConsigneeContainerPenalties.Count);

			OrgContainerDetention conDet = consignee.ConsigneeContainerPenalties.AddNew();
			AssertEquals("ConsigneeContainerPenalties.Count", 1, consignee.ConsigneeContainerPenalties.Count);

			Factory.Save();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			OrgHeader org = testFactory.Load<OrgHeader>(consignee.PK);

			AssertEquals("Org.ConsigneeContainerPenalties.Count", 1, org.ConsigneeContainerPenalties.Count);
			AssertEquals("Correct ConsigneeContainerPenalties loaded", consignee.PK, org.ConsigneeContainerPenalties[0].PD_OH_Client);
		}

		public void TestConsignorContainerPenalties()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			AssertEquals("ConsignorContainerPenalties.Count", 0, consignor.ConsignorContainerPenalties.Count);

			var conDet = consignor.ConsignorContainerPenalties.AddNew();
			AssertEquals("ConsignorContainerPenalties.Count", 1, consignor.ConsignorContainerPenalties.Count);

			Factory.Save();

			var testFactory = new BusinessObjectFactory();
			var org = testFactory.Load<OrgHeader>(consignor.PK);

			AssertEquals("Org.ConsignorContainerPenalties.Count", 1, org.ConsignorContainerPenalties.Count);
			AssertEquals("Correct ConsignorContainerPenalties loaded", consignor.PK, org.ConsignorContainerPenalties[0].PD_OH_Client);
		}

		public void TestCTOStorages()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var storage = org.ConsigneeCTOStorages.AddNew();
			AssertEquals(Constants.ContainerDetentionPenaltyType.STO, storage.PD_PenaltyType);
			AssertEquals(org.PK, storage.PD_OH_Client);
			AssertEquals(Constants.ContainerDetentionDirection.Import, storage.PD_Direction);
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.CTO, storage.PD_CreditorType);
			AssertEquals(1, org.ConsigneeCTOStorages.Count);

			storage = org.CarrierContainerPenalties.AddNew();
			storage.PD_Direction = Constants.ContainerDetentionDirection.Import;
			storage.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.STO;
			AssertEquals(org.PK, storage.PD_OH_Carrier);
			AssertEquals(Constants.ContainerDetentionDirection.Import, storage.PD_Direction);
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.Carrier, storage.PD_CreditorType);
			AssertEquals(1, org.CarrierContainerPenalties.Count(p => p.PD_Direction == Constants.ContainerDetentionDirection.Import && p.PD_PenaltyType == Constants.ContainerDetentionPenaltyType.STO));

			storage = org.CarrierContainerPenalties.AddNew();
			storage.PD_Direction = Constants.ContainerDetentionDirection.Export;
			storage.PD_PenaltyType = Constants.ContainerDetentionPenaltyType.STO;
			AssertEquals(org.PK, storage.PD_OH_Carrier);
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.Carrier, storage.PD_CreditorType);
			AssertEquals(1, org.CarrierContainerPenalties.Count(p => p.PD_Direction == Constants.ContainerDetentionDirection.Export && p.PD_PenaltyType == Constants.ContainerDetentionPenaltyType.STO));

			storage = org.ServiceImportCTOStorages.AddNew();
			AssertEquals(Constants.ContainerDetentionPenaltyType.STO, storage.PD_PenaltyType);
			AssertEquals(org.PK, storage.PD_OH_CTO);
			AssertEquals(Constants.ContainerDetentionDirection.Import, storage.PD_Direction);
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.CTO, storage.PD_CreditorType);
			AssertEquals(1, org.ServiceImportCTOStorages.Count);

			storage = org.ServiceExportCTOStorages.AddNew();
			AssertEquals(Constants.ContainerDetentionPenaltyType.STO, storage.PD_PenaltyType);
			AssertEquals(org.PK, storage.PD_OH_CTO);
			AssertEquals(Constants.ContainerDetentionDirection.Export, storage.PD_Direction);
			AssertEquals(Constants.ContainerPenaltyCreditorType.Codes.CTO, storage.PD_CreditorType);
			AssertEquals(1, org.ServiceExportCTOStorages.Count);
		}

		public void TestCarrierContainerDetentions()
		{
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();

			AssertEquals("CarrierContainerPenalties.Count", 0, carrier.CarrierContainerPenalties.Count);

			OrgContainerDetention conDet = carrier.CarrierContainerPenalties.AddNew();
			conDet.PD_Direction = Constants.ContainerDetentionDirection.Import;
			AssertEquals("CarrierContainerPenalties.Count", 1, carrier.CarrierContainerPenalties.Count);

			Factory.Save();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			OrgHeader org = testFactory.Load<OrgHeader>(carrier.PK);

			AssertEquals("Org.CarrierContainerPenalties.Count", 1, org.CarrierContainerPenalties.Count);
			AssertEquals("Correct CarrierContainerPenalties loaded", carrier.PK, org.CarrierContainerPenalties[0].PD_OH_Carrier);
			AssertEquals("CreditorType is defaulted ", Constants.ContainerPenaltyCreditorType.Codes.Carrier, org.CarrierContainerPenalties[0].PD_CreditorType);
		}

		public void TestDependentCollectionsAreClearedWhenOrgTypeIsChanged()
		{
			company.OH_IsDebtor = true;
			company.OH_IsCreditor = true;
			company.OH_IsConsignor = true;
			company.OH_IsConsignee = true;
			company.OH_IsTransportClient = true;
			company.OH_IsWarehouseClient = true;
			company.OH_IsShippingProvider = true;
			company.OH_IsForwarder = true;
			company.OH_IsBroker = true;
			company.OH_IsMiscFreightServices = true;
			company.OH_IsCompetitor = true;
			company.OH_IsSalesLead = true;

			company.Contacts.AddNew();
			AssertEquals("Contacts count", 1, company.Contacts.Count);

			company.SalesCallContacts.AddNew();
			AssertEquals("Sales Call contacts count", 1, company.SalesCallContacts.Count);
			AssertEquals("Addresses count", 1, company.Addresses.Count); // Address automatically created for a new org

			company.SalesCollection.AddNew();
			AssertEquals("SalesCollection count", 1, company.SalesCollection.Count);

			company.Clients.AddNew();
			AssertEquals("Sales ClientsCollection count", 1, company.Clients.Count);

			company.SalesCalls.AddNew();
			AssertEquals("SalesCalls collection count", 1, company.SalesCalls.Count);

			company.SupplierLinks.AddNew();
			AssertEquals("SupplierLinks collectin count", 1, company.SupplierLinks.Count);

			company.BuyerLinks.AddNew();
			AssertEquals("BuyerLinks collection count", 1, company.BuyerLinks.Count);

			company.AppointedAgentPorts.AddNew();
			AssertEquals("AppointedAgentPorts collection count", 1, company.AppointedAgentPorts.Count);

			company.AppointedGatewayAgentPorts.AddNew();
			AssertEquals("AppointedGatewayAgentPorts collection count", 1, company.AppointedGatewayAgentPorts.Count);

			company.CarrierAppointedAgentPorts_AirCTO.AddNew();
			AssertEquals("CarrierAppointedAgentPorts collection count", 1, company.CarrierAppointedAgentPorts_AirCTO.Count);

			company.MiscServ.CarrierServiceLevels.AddNew();
			AssertEquals("MiscServ.CarrierServiceLevels collection count", 2, company.MiscServ.CarrierServiceLevels.Count);

			company.AgentRelationships.AddNew();
			AssertEquals("AgentRelationships collection count", 1, company.AgentRelationships.Count);

			company.LandedCostingPreferences.AddNew();
			AssertEquals("LandedCostingPreferences collection count", 1, company.LandedCostingPreferences.Count);

			company.StaffAssignments.AddNew();
			AssertEquals("StaffAssignments collection count", 1, company.StaffAssignments.Count);

			company.SalesOpportunities.AddNew();
			AssertEquals("SalesOpportunities collection count", 1, company.SalesOpportunities.Count);

			company.BrandsOrRelatedNames.AddNew();
			AssertEquals("BrandsOrRelatedNames collection count", 1, company.BrandsOrRelatedNames.Count);

			company.OH_IsDebtor = false;
			company.OH_IsCreditor = false;
			company.OH_IsConsignor = false;
			company.OH_IsConsignee = false;
			company.OH_IsTransportClient = false;
			company.OH_IsWarehouseClient = false;
			company.OH_IsShippingProvider = false;
			company.OH_IsForwarder = false;
			company.OH_IsBroker = false;
			company.OH_IsMiscFreightServices = false;
			company.OH_IsCompetitor = false;
			company.OH_IsSalesLead = false;

			AssertEquals("Contacts collection count", 1, company.Contacts.Count);
			AssertEquals("SalesCallContacts collection count", 1, company.SalesCallContacts.Count);
			AssertEquals("Addresses collection count", 1, company.Addresses.Count);
			AssertEquals("SalesCollection count", 1, company.SalesCollection.Count);
			AssertEquals("Sales Clients collection count", 0, company.Clients.Count);
			AssertEquals("SalesCalls collection count", 1, company.SalesCalls.Count);
			AssertEquals("SupplierLinks collection count", 0, company.SupplierLinks.Count);
			AssertEquals("BuyerLinks collection count", 0, company.BuyerLinks.Count);
			AssertEquals("AppointedAgentPorts collection count", 0, company.AppointedAgentPorts.Count);
			AssertEquals("AppointedGatewayAgentPorts collection count", 0, company.AppointedGatewayAgentPorts.Count);
			AssertEquals("CarrierAppointedAgentPorts collection count", 0, company.CarrierAppointedAgentPorts_Stevedore.Count);
			AssertEquals("CarrierAppointedAgentPorts collection count", 0, company.CarrierAppointedAgentPorts_AirCTO.Count);
			AssertEquals("CarrierAppointedAgentPorts collection count", 0, company.CarrierAppointedAgentPorts_RailHeadDepot.Count);
			AssertEquals("CarrierAppointedAgentPorts collection count", 0, company.CarrierAppointedAgentPorts_RoadDepotShed.Count);
			AssertEquals("CarrierAppointedAgentPorts collection count", 0, company.CarrierAppointedAgentPorts_ContainerYardPark.Count);
			AssertEquals("CarrierAppointedAgentPorts collection count", 0, company.CarrierAppointedAgentPorts_Agency.Count);
			AssertEquals("AgentRelationships collection count", 0, company.AgentRelationships.Count);
			AssertEquals("LandedCostingPreferences collection count", 0, company.LandedCostingPreferences.Count);
			AssertEquals("StaffAssignments collection count", 1, company.StaffAssignments.Count);
			AssertEquals("SalesOpportunities collection count", 1, company.SalesOpportunities.Count);
			AssertEquals("BrandsOrRelatedNames collection count", 1, company.BrandsOrRelatedNames.Count);
		}

		public void TestCountry()
		{
			OrgHeader organization = Factory.New<OrgHeader>();
			AssertNull("Country", organization.Country);

			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = "RRR";
			organization.OH_RL_NKClosestPort = port.RL_Code;
			AssertNull("Country", organization.Country);

			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			port.RL_RN_NKCountryCode = country.RN_Code;
			AssertEquals("Country", country, organization.Country);
		}

		public void TestPrimaryRegistrationNumber()
		{
			OrgHeader organization = Factory.New<OrgHeader>();
			AssertEquals("PrimaryRegistrationNumber should be registered.", true, organization.IsRegisteredEditableChildObject(organization.PrimaryRegistrationNumber));
		}

		public void TestCountryData()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			OrgCountryData countryData1 = organisation.CountryDataCollectionForThisCompany.AddNew();
			countryData1.OV_OA_ApprovedLocation = organisation.MainAddress.PK;
			countryData1.OV_RN_NKClientCountryRelation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			OrgCountryData countryData2 = Factory.New<OrgCountryData>();
			countryData2.OV_OH_OrgHeader = organisation.PK;
			countryData2.OV_RN_NKClientCountryRelation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			CombineAssertions(() =>
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				OrgHeader organisationLoaded = factory2.Load<OrgHeader>(organisation.PK);
				AssertEquals("CountryData for Organisation", countryData2.PK, organisationLoaded.CountryData.PK);

				organisation = Factory.New<OrgHeader>();
				AssertNotNull("CountryData for Organisation", organisation.CountryData);
				AssertEquals(ZGuid.Empty, organisation.CountryData.OV_OA_ApprovedLocation);

				organisation.Delete();
				AssertNull("after delete, CountryData is null", organisation.CountryData);
			});
		}

		public void TestGetCountryData()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			var uae = organisation.GetCountryData(Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedArabEmirates));
			var uk = organisation.GetCountryData(Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedKingdom));
			AssertNotEquals(uae.PK, uk.PK);
			AssertEquals("AE", uae.OV_RN_NKClientCountryRelation);
			AssertEquals("GB", uk.OV_RN_NKClientCountryRelation);
		}

		public void TestCountryDataUpdateOrgAdderssKnownShipperDetails_WhenAdded()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var countryData = organisation.CountryDataCollectionForThisCompany.AddNew();
			countryData.OV_OA_ApprovedLocation = organisation.MainAddress.PK;
			countryData.OV_RN_NKClientCountryRelation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			countryData.OV_EXApprovalNumber = "123456";

			var knownShipperDetails = organisation.MainAddress.KnownShipperDetails;

			AssertEquals(1, knownShipperDetails.Count);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, knownShipperDetails[0].OV_RN_NKClientCountryRelation);
			AssertEquals(AviationSecuritySchemeMembershipEx.Codes.Yes, knownShipperDetails[0].OV_EXApprovedOrMajorExporter);
			AssertEquals("123456", knownShipperDetails[0].OV_EXApprovalNumber);
		}

		[ExpectNoExceptions]
		public void TestCAAccountSecurityNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var countryData = Factory.New<OrgCountryData>();
				countryData.OV_OH_OrgHeader = organisation.PK;
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Canada;
				AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IOrgImpAddInfo>(), countryData.ImpAddInfo.GetType());
				AssertEquals(string.Empty, countryData.ImpAddInfo["ZO_AccountSecurityNumber"]);
				countryData.ImpAddInfo["ZO_AccountSecurityNumber"] = "12345";
				AssertEquals("CA AccountSecurity Number", "12345", organisation.CAAccountSecurityNumber);
			}
		}

		[ExpectNoExceptions]
		public void TestAUIsDutyDeferred()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var countryData = Factory.New<OrgCountryData>();
				countryData.OV_OH_OrgHeader = organisation.PK;
				countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Australia;
				AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgImpAddInfo>(), countryData.ImpAddInfo.GetType());
				AssertEquals(false, countryData.ImpAddInfo["ZO_IsDutyDeferred"]);
				countryData.ImpAddInfo["ZO_IsDutyDeferred"] = true;
				AssertEquals("AU IsDutyDeferred", true, organisation.AUIsDutyDeferred);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				organisation.AUIsDutyDeferred = true;
				Assert("AUIsDutyDeferred is not valid when current country is not AU.", !organisation.AUIsDutyDeferred);
			}
		}

		#endregion

		#region TestRatingDocumentsChargeOrderCollection

		public void TestRatingDocumentsChargeOrderCollection()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var info = org.RatingDocumentsChargeOrders.AddNew();

			info.RCO_DocumentType = "DNA";
			info.RCO_PrintOrder = 1;
			info.RCO_AC_ChargeCode = chargeCode.PK;

			Factory.Save();

			AssertEquals("RatingDocumentsChargeOrders should contain 1 records", 1, org.RatingDocumentsChargeOrders.Count);
			AssertNotNull("RatingDocumentChargeOrder info should exist", new BusinessObjectFactory().Load<RatingDocumentsChargeOrder>(info.PK));

			org.RatingDocumentsChargeOrders.DeleteAll();
			Factory.Save();

			AssertNull("RatingDocumentsChargeOrders should be deleted", new BusinessObjectFactory().Load<RatingDocumentsChargeOrder>(info.PK));
		}

		public void TestCheckForRatingDocumentsOrdersDuplicatesValidation()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			RatingDocumentsChargeOrder line1 = company.RatingDocumentsChargeOrders.AddNew();
			line1.RCO_AC_ChargeCode = chargeCode1.PK;
			line1.RCO_PrintOrder = 1;
			line1.RCO_DocumentType = "DC1";
			RatingDocumentsChargeOrder line2 = company.RatingDocumentsChargeOrders.AddNew();
			line2.RCO_AC_ChargeCode = chargeCode1.PK;
			line2.RCO_PrintOrder = 2;
			line2.RCO_DocumentType = "DC2";
			RatingDocumentsChargeOrder line3 = company.RatingDocumentsChargeOrders.AddNew();
			line3.RCO_AC_ChargeCode = chargeCode3.PK;
			line3.RCO_PrintOrder = 3;
			line3.RCO_DocumentType = "DC3";
			RatingDocumentsChargeOrder line4 = company.RatingDocumentsChargeOrders.AddNew();
			line4.RCO_AC_ChargeCode = chargeCode2.PK;
			line4.RCO_PrintOrder = 1;
			line4.RCO_DocumentType = "DC1";
			RatingDocumentsChargeOrder line5 = company.RatingDocumentsChargeOrders.AddNew();
			line5.RCO_AC_ChargeCode = chargeCode3.PK;
			line5.RCO_PrintOrder = 5;
			line5.RCO_DocumentType = "DC3";

			Assert("Line 1 is not valid because it has duplicate Print Order", line1.HasRowErrors);
			Assert(string.Format("Line 1 error message should be {0}", "Duplicate values of Print Order are not allowed."), line1.Notifications.GetErrors().ToUniqueMessageListString().Contains("Duplicate values of Print Order are not allowed."));
			Assert("Line 2 shouldn't have any errors", !line2.HasRowErrors);
			Assert("Line 3 is not valid because both charge code and rating documents type have duplicates", line3.HasRowErrors);
			Assert(string.Format("Line 3 error message should be {0}", "Duplicate entries are not allowed."), line3.Notifications.GetErrors().ToUniqueMessageListString().Contains("Duplicate entries are not allowed."));
			Assert("Line 4 is not valid because it has duplicate Print Order", line4.HasRowErrors);
			Assert(string.Format("Line 4 error message should be {0}", "Duplicate values of Print Order are not allowed."), line4.Notifications.GetErrors().ToUniqueMessageListString().Contains("Duplicate values of Print Order are not allowed."));
			Assert("Line 5 is not valid because both charge code and rating documents type have duplicates", line5.HasRowErrors);
			Assert(string.Format("Line 5 error message should be {0}", "Duplicate entries are not allowed."), line5.Notifications.GetErrors().ToUniqueMessageListString().Contains("Duplicate entries are not allowed."));
		}

		#endregion

		#region Property Overrides

		#region TestOH_RL_NKClosestPort

		public void TestOH_RL_NKClosestPort()
		{
			company = BasicCompanyForTest;
			AssertEquals("OH_Code", "DUUUUH", company.OH_Code);
			AssertEquals("MainAddress.OA_State", string.Empty, company.MainAddress.OA_State);

			company.OH_FullName = string.Empty;
			company.OH_Code = string.Empty;
			company.OH_RL_NKClosestPort = "USBTM";
			AssertEquals("OH_Code", string.Empty, company.OH_Code);
			AssertEquals("MainAddress.OA_State", "MT", company.MainAddress.OA_State);
			AssertEquals("MainAddress.OA_RL_NKRelatedPortCode", "USBTM", company.MainAddress.OA_RL_NKRelatedPortCode);

			company.OH_FullName = "Test Org";
			AssertEquals("OH_Code", "TESORGBTM", company.OH_Code);

			company.MainAddress.OA_State = "STATE";
			company.OH_RL_NKClosestPort = "SGSIN";
			AssertEquals("MainAddress.OA_State should have been left as is.", "STATE", company.MainAddress.OA_State);
			company.OH_RL_NKClosestPort = "XXX"; // Invalid UNLOCO
			AssertEquals("MainAddress.OA_State should have been left as is.", "STATE", company.MainAddress.OA_State);
		}

		public void TestOH_RL_NKClosestPortOnWeb()
		{
			Globals.IsWeb = true;
			company = BasicCompanyForTest;
			company.OH_FullName = string.Empty;
			company.OH_Code = string.Empty;
			company.MainAddress.OA_State = "STATE";
			company.OH_RL_NKClosestPort = "USBTM";

			AssertEquals("MainAddress.OA_State should have been left as is on Web.", "STATE", company.MainAddress.OA_State);
			AssertEquals("MainAddress.OA_RL_NKRelatedPortCode", "USBTM", company.MainAddress.OA_RL_NKRelatedPortCode);
		}

		public void TestOH_RLSetDefaultCurrencies()
		{
			company = BasicCompanyForTest;
			Assert("CompanyData.OB_APDefaultCurreny is empty", company.CompanyData.OB_RX_NKAPDefltCurrency.IsEmpty);

			GetUNLOCO("BRRIO").Country.LocalCurrency.RX_IsActive = false;
			company.OH_RL_NKClosestPort = "BRRIO";
			Assert("CompanyData.OB_APDefaultCurreny is empty", company.CompanyData.OB_RX_NKAPDefltCurrency.IsEmpty);

			company.OH_RL_NKClosestPort = "USLAX";
			AssertEquals("CompanyData.OB_APDefaultCurrency", GetUNLOCO("USLAX").Country.RN_RX_NKLocalCurrency, company.CompanyData.OB_RX_NKAPDefltCurrency);

			company.CompanyData.OB_RX_NKAPDefltCurrency = GetUNLOCO("GBLON").Country.RN_RX_NKLocalCurrency;
			company.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("CompanyData.OB_APDefaultCurrency", GetUNLOCO("GBLON").Country.RN_RX_NKLocalCurrency, company.CompanyData.OB_RX_NKAPDefltCurrency);
		}

		public void TestSetControllingBranch()
		{
			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "001";
			branch1.GB_GC = currentCompany.PK;
			branch1.GB_RL_NKHomePort = "Port3";

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "002";
			branch2.GB_GC = currentCompany.PK;
			branch2.GB_RL_NKHomePort = "Port2";

			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_Code = "003";
			branch3.GB_GC = currentCompany.PK;
			branch3.GB_RL_NKHomePort = "Port3";
			Factory.Save();

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK));

			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "Port3";
			AssertEquals("Will set branch1 as the default controlling branch because it has the highest priority order by name in the current company.", branch1.PK, header.CompanyData.ControllingBranch.PK);

			header.CompanyData.OB_GB_ControllingBranch = Guid.Empty;
			header.OH_RL_NKClosestPort = "Port2";
			AssertEquals(branch2.PK, Env.CurrentBranch.PK);
			AssertEquals("Will set branch2 as the default controlling branch because branch2 is the login branch and its port is the same as header's port.", branch2.PK, header.CompanyData.ControllingBranch.PK);

			header.CompanyData.OB_GB_ControllingBranch = Guid.Empty;
			header.OH_RL_NKClosestPort = "Port1";
			AssertNull("Will not set controlling brach because the port is not existed in the current company.", header.CompanyData.ControllingBranch);
		}

		public void TestSetTaxApplicable()
		{
			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "YY";
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = country.Code;
			unloco.RL_Code = "YYLOC";

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
			company.CompanyData.OB_APVATConfig = "NON";
			company.OH_RL_NKClosestPort = unloco.RL_Code;
			Assert("OM_ARTaxApplicable should be true, Org GST registered, though countries are different", company.CompanyData.IsARTaxApplicable);
			Assert("OM_ARWHTApplicable should be false, Org GST registered, though countries are different", !company.CompanyData.OB_ARWHTApplicable);
			Assert("OM_APTaxApplicable should be false, Org GST registered, as countries are different", !company.CompanyData.IsAPTaxApplicable);
			Assert("OM_APWHTApplicable should be false, Org GST registered, as countries are different", !company.CompanyData.OB_APWHTApplicable);

			company.OH_RL_NKClosestPort = string.Empty;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			company.CompanyData.OB_APVATConfig = "NON";
			company.OH_RL_NKClosestPort = unloco.RL_Code;
			Assert("OM_ARTaxApplicable should be false, Org GST not registered, though countries are different", !company.CompanyData.IsARTaxApplicable);
			Assert("OM_ARWHTApplicable should be false, Org GST not registered, though countries are different", !company.CompanyData.OB_ARWHTApplicable);
			Assert("OM_APTaxApplicable should be false, Org GST not registered, as countries are different", !company.CompanyData.IsAPTaxApplicable);
			Assert("OM_APWHTApplicable should be false, Org GST not registered, as countries are different", !company.CompanyData.OB_APWHTApplicable);

			company.OH_RL_NKClosestPort = string.Empty;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
			unloco.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			company.OH_RL_NKClosestPort = unloco.RL_Code;
			Assert("OM_ARTaxApplicable is false, Org not GST registered", !company.CompanyData.IsARTaxApplicable);
			Assert("OM_APTaxApplicable is false, same countries, Org not GST registered", !company.CompanyData.IsAPTaxApplicable);
			Assert("OM_ARWHTApplicable is false, Org not WHT registered", !company.CompanyData.OB_ARWHTApplicable);
			Assert("OM_APWHTApplicable is false, same countries, Org not WHT registered", !company.CompanyData.OB_APWHTApplicable);

			company.OH_RL_NKClosestPort = string.Empty;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
			company.OH_RL_NKClosestPort = unloco.RL_Code;
			Assert("OM_ARTaxApplicable is true, same countries, Org GST registered", company.CompanyData.IsARTaxApplicable);
			Assert("OM_APTaxApplicable is true, same countries, Org GST registered", company.CompanyData.IsAPTaxApplicable);
			Assert("OM_ARWHTApplicable is false, same countries, Org WHT registered", !company.CompanyData.OB_ARWHTApplicable);
			Assert("OM_APWHTApplicable is false, same countries, Org WHT registered", !company.CompanyData.OB_APWHTApplicable);
		}

		public void TestMarkMiscServNeedValidationIfRequired()
		{
			var airLine = Factory.NewWithValidTestData<RefAirline>();

			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG AAA";
			org1.OH_IsAirLine = true;
			org1.OH_IsShippingProvider = true;

			org1.MainAddress.OA_City = "Sydney";
			org1.MainAddress.OA_PostCode = "0000";
			org1.MainAddress.OA_Address1 = "TEST ADR 001";
			org1.MainAddress.State = "NSW";

			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.MiscServ.OM_RM_Airline = airLine.PK;

			org1.RunPreSaveValidation();

			AssertNoErrors(org1);
			AssertNoErrors(org1.MiscServ);

			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "ORG BBB";
			org2.OH_IsAirLine = true;
			org2.OH_IsShippingProvider = true;

			org2.MainAddress.OA_City = "Auckland";
			org2.MainAddress.OA_PostCode = "0001";
			org2.MainAddress.OA_Address1 = "TEST ADR 002";
			org2.MainAddress.State = "TEST";

			org2.OH_RL_NKClosestPort = "NZAKL";
			org2.MiscServ.OM_RM_Airline = airLine.PK;

			org2.RunPreSaveValidation();

			AssertNoErrors(org2);
			AssertNoErrors(org2.MiscServ);

			Factory.Save();

			org2.OH_RL_NKClosestPort = "AUBNE";
			org2.RunPreSaveValidation();

			Assert("Should has errors as these two organisationsntry have same country's port and use same airline code.", org2.HasErrors);
			AssertHasError("Should has expected error as these two organisations have same country's port and use same airline code.",
				org2.MiscServ.OM_RM_AirlineInfo,
				"This master bill prefix has already been used on organization ORGAAASYD.");
		}

		#endregion

		#region TestOH_Code

		// Tests the case when an invalid UNLOCO is entered and then the UNLOCO with that code is created.
		// The org code should still get generated when the org is saved.
		public void TestOH_CodeWhenInvalidUNLOCOCreated()
		{
			Env.Registry.CanUserEditOrganisationCode = false;

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			OrgHeader newOrg = orgFactory.New<OrgHeader>();
			newOrg.OH_FullName = "Mary Poppins";
			newOrg.OH_RL_NKClosestPort = "HKZZZ";       // invalid UNLOCO
			newOrg.MainAddress.OA_Address1 = "Some Street somewhere";
			AssertEquals("Precondition: Org Code calculated with wrong unloco", "MARPOP", newOrg.OH_Code);
			AssertHasErrors("Precondition: UNLOCO has errors", newOrg.OH_RL_NKClosestPortInfo);

			BusinessObjectFactory unlocoFactory = new BusinessObjectFactory();
			RefUNLOCO unloco = unlocoFactory.New<RefUNLOCO>();
			unloco.RL_Code = "HKZZZ";
			unloco.RL_PortName = "Some new port i made";
			unlocoFactory.Save();

			newOrg.Validation.ValidateOH_RL_NKClosestPort();    // emulate what happens after the findbox creates the new UNLOCO
			orgFactory.Save();
			AssertEquals("Org Code calculated on saving", "MARPOPZZZ", newOrg.OH_Code);
		}

		public void TestOH_Code()
		{
			company.OH_RL_NKClosestPort = "AUSYD";
			company.OH_FullName = "test or ganisation";

			OrgCodeGenerator codeGen = new OrgCodeGenerator();
			ZString expectedCode = codeGen.GenerateCode(company).GetProposedCode();

			AssertEquals("Calculated Code", expectedCode, company.OH_Code);
		}

		public void TestCodeCanBeSetManually()
		{
			OrgHeader organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_FullName = "KABOOM";
			organization.OH_Code = "XXX";
			Factory.Save();
			AssertEquals("OH_Code", "XXX", organization.OH_Code);
		}

		public void TestCodeIsNotUpdatedIfRegenerateOnFieldChangeOptionIsOff()
		{
			OrgCodeAlgorithm algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			algorithm.RegenerateOrgCodeOnChanges = false;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_FullName = "Ice Cream";
			organization.OH_RL_NKClosestPort = "AUMEL";
			AssertEquals("OH_Code", "ICECREMEL", organization.OH_Code);

			organization.OH_IsNationalAccount = true;
			AssertEquals("OH_Code", "ICECRE_AU", organization.OH_Code);

			Factory.Save();
			AssertEquals("OH_Code", "ICECRE_AU", organization.OH_Code);

			organization.OH_FullName = "Chocolate Chip";
			organization.OH_IsNationalAccount = false;
			AssertEquals("OH_Code", "ICECRE_AU", organization.OH_Code);
		}

		public void TestGenerateProposedCode_NotAllowRecalculatedOrgCodeByUser()
		{
			var algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			algorithm.AllowRecalculatedOrgCodeByUser = false;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_FullName = "DEMO TEST COMPANY PTY LTD";
			organization.OH_RL_NKClosestPort = "CO8SG";
			organization.OH_Code = "DEMTESSYD";
			organization.GenerateProposedCode(false);
			AssertEquals("OH_Code", "DEMTESSYD", organization.OH_Code);
		}

		public void TestGenerateProposedCode_AllowRecalculatedOrgCodeByUser()
		{
			var algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			algorithm.AllowRecalculatedOrgCodeByUser = true;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_FullName = "DEMO TEST COMPANY PTY LTD";
			organization.OH_RL_NKClosestPort = "CO8SG";
			organization.OH_Code = "DEMTESSYD";
			organization.GenerateProposedCode(false);
			AssertEquals("OH_Code", "DEMTES8SG", organization.OH_Code);
		}

		public void TestProposedCodeIsNotGeneratedIfRegenerateOnFieldChangeOptionIsOff()
		{
			OrgCodeAlgorithm algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			algorithm.RegenerateOrgCodeOnChanges = false;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader organization = Factory.NewWithValidTestData<OrgHeader>();

			organization.OH_FullName = "Ice Cream";
			organization.OH_RL_NKClosestPort = "AUMEL";
			organization.OH_Code = "012345";
			AssertEquals("OH_Code", "012345", organization.OH_Code);

			Factory.Save();
			AssertEquals("OH_Code", "012345", organization.OH_Code);

			organization.GenerateProposedCode();
			AssertEquals("OH_Code", "012345", organization.OH_Code);
		}

		public void TestCodeIsUpdatedWhenOrgTypeChangesIfRegenerateCodeWhenOrgTypesChangeIsTrue()
		{
			OrgCodeAlgorithm defaultAlgorithm = new OrgCodeAlgorithm();
			OrgCodeAlgorithm overrideAlgorithm = new OrgCodeAlgorithm();

			defaultAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			defaultAlgorithm.Elements[OrgCodeElementDescription.SecondName].Order = 1;
			defaultAlgorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;

			overrideAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			overrideAlgorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			overrideAlgorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;

			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Broker].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Carrier].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Competitor].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Consignee].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Consignor].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Forwarder].Selected = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultAlgorithm);
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overrideAlgorithm);

			OrgHeader organization = Factory.New<OrgHeader>();
			organization.RegenerateCodeWhenOrgTypesChange = true;
			organization.OH_FullName = "APPLE PIE";
			organization.OH_RL_NKClosestPort = "AUSYD";

			organization.OH_IsBroker = true;
			AssertEquals("OH_Code", "APP", organization.OH_Code);

			organization.OH_IsBroker = false;
			AssertEquals("OH_Code", "PIE", organization.OH_Code);

			organization.OH_IsShippingProvider = true;
			AssertEquals("OH_Code", "APP", organization.OH_Code);

			organization.OH_IsShippingProvider = false;
			AssertEquals("OH_Code", "PIE", organization.OH_Code);

			organization.OH_IsCompetitor = true;
			AssertEquals("OH_Code", "APP", organization.OH_Code);

			organization.OH_IsCompetitor = false;
			AssertEquals("OH_Code", "PIE", organization.OH_Code);

			organization.OH_IsConsignee = true;
			AssertEquals("OH_Code", "APP", organization.OH_Code);

			organization.OH_IsConsignee = false;
			AssertEquals("OH_Code", "PIE", organization.OH_Code);

			organization.OH_IsConsignor = true;
			AssertEquals("OH_Code", "APP", organization.OH_Code);

			organization.OH_IsConsignor = false;
			AssertEquals("OH_Code", "PIE", organization.OH_Code);

			organization.OH_IsForwarder = true;
			AssertEquals("OH_Code", "APP", organization.OH_Code);

			organization.OH_IsForwarder = false;
			AssertEquals("OH_Code", "PIE", organization.OH_Code);

			overrideAlgorithm.SelectableOrgTypes.Load();
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Payables].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Receivables].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Sales].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Services].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.TransportClient].Selected = true;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Warehouse].Selected = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overrideAlgorithm);

			organization = Factory.New<OrgHeader>();
			organization.OH_FullName = "ORANGE JUICE";
			organization.OH_RL_NKClosestPort = "AUSYD";
			organization.RegenerateCodeWhenOrgTypesChange = true;

			organization.OH_IsCreditor = true;
			AssertEquals("OH_Code", "ORA", organization.OH_Code);

			organization.OH_IsCreditor = false;
			AssertEquals("OH_Code", "JUI", organization.OH_Code);

			organization.OH_IsDebtor = true;
			AssertEquals("OH_Code", "ORA", organization.OH_Code);

			organization.OH_IsDebtor = false;
			AssertEquals("OH_Code", "JUI", organization.OH_Code);

			organization.OH_IsSalesLead = true;
			AssertEquals("OH_Code", "ORA", organization.OH_Code);

			organization.OH_IsSalesLead = false;
			AssertEquals("OH_Code", "JUI", organization.OH_Code);

			organization.OH_IsMiscFreightServices = true;
			AssertEquals("OH_Code", "ORA", organization.OH_Code);

			organization.OH_IsMiscFreightServices = false;
			AssertEquals("OH_Code", "JUI", organization.OH_Code);

			organization.OH_IsTransportClient = true;
			AssertEquals("OH_Code", "ORA", organization.OH_Code);

			organization.OH_IsTransportClient = false;
			AssertEquals("OH_Code", "JUI", organization.OH_Code);

			organization.OH_IsWarehouseClient = true;
			AssertEquals("OH_Code", "ORA", organization.OH_Code);

			organization.OH_IsWarehouseClient = false;
			AssertEquals("OH_Code", "JUI", organization.OH_Code);
		}

		public void TestPayablesFlagIsEnabled_When_OrgDetailsNewOrgTypeTempAPFlag_IsAllowed()
		{
			bool originalNewOrgTypeTempAPFlag = Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed;
			bool originalModifyOrgTypeTempAPFlag = Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed;

			OrgCodeAlgorithm defaultAlgorithm = new OrgCodeAlgorithm();

			defaultAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			defaultAlgorithm.Elements[OrgCodeElementDescription.SecondName].Order = 1;
			defaultAlgorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultAlgorithm);

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountNum = GetRandomString(10);
			bankAccount.AB_BSB = GetRandomString(6);
			bankAccount.AB_Code = GetRandomString(3);
			bankAccount.AB_IsDefaultReceiptBankAccount = new ZBool(Core.Constants.BooleanTrueString);
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			try
			{
				OrgHeader organization = Factory.New<OrgHeader>();
				organization.OH_FullName = "ORANGE JUICE";
				organization.OH_RL_NKClosestPort = "AUSYD";
				organization.RegenerateCodeWhenOrgTypesChange = true;

				organization.OH_IsTempAccount = true;
				organization.CompanyData.OB_RX_NKAPDefltCurrency = "AUD";
				organization.CompanyData.OB_GB_ControllingBranch = bankAccount.AB_GB;

				Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = true;
				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = false;

				organization.OH_IsCreditor = true;
				AssertEquals(true, organization.CompanyData.OB_IsCreditor);
				Assert(organization.CompanyData.OB_AB_APDefaultBankAccount != ZGuid.Empty);

				organization.CompanyData.OB_AB_APDefaultBankAccount = ZGuid.Empty;
				Factory.Save();
				organization.OH_IsCreditor = true;
				AssertEquals(ZGuid.Empty, organization.CompanyData.OB_AB_APDefaultBankAccount);

				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = true;
				organization.CompanyData.OB_AB_APDefaultBankAccount = ZGuid.Empty;
				organization.OH_IsCreditor = true;
				Assert(organization.CompanyData.OB_AB_APDefaultBankAccount != ZGuid.Empty);
			}
			finally
			{
				Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = originalNewOrgTypeTempAPFlag;
				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = originalModifyOrgTypeTempAPFlag;
			}
		}

		public void TestReceivablesFlagIsEnabled_When_OrgDetailsNewOrgTypeTempARFlag_IsAllowed()
		{
			var originalNewOrgTypeTempARFlag = Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed;
			var originalModifyOrgTypeTempARFlag = Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed;

			var defaultAlgorithm = new OrgCodeAlgorithm();

			defaultAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			defaultAlgorithm.Elements[OrgCodeElementDescription.SecondName].Order = 1;
			defaultAlgorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultAlgorithm);

			try
			{
				var organization = Factory.New<OrgHeader>();
				organization.OH_FullName = "ORANGE JUICE";
				organization.OH_RL_NKClosestPort = "AUSYD";
				organization.RegenerateCodeWhenOrgTypesChange = true;

				organization.OH_IsTempAccount = true;
				organization.CompanyData.OB_RX_NKAPDefltCurrency = "AUD";
				organization.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;

				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = true;
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = false;

				organization.OH_IsDebtor = true;
				AssertEquals(true, organization.CompanyData.OB_IsDebtor);

				Factory.Save();

				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = true;
				organization.OH_IsDebtor = true;
				AssertEquals(true, organization.CompanyData.OB_IsDebtor);
			}
			finally
			{
				Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = originalNewOrgTypeTempARFlag;
				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = originalModifyOrgTypeTempARFlag;
			}
		}

		#region Random String Generator

		string GetRandomString(int stringLength)
		{
			string result = "";
			for (int i = 0; i < stringLength; i++)
			{
				int index = Generator.Next(65, 90);
				result += (char)index;
			}
			return result;
		}

		Random Generator
		{
			get { return generator ?? (generator = new Random()); }
		}
		Random generator;

		#endregion

		public void TestCodeIsNotUpdatedWhenOrgTypeChangesIfRegenerateCodeWhenOrgTypesChangeIsFalse()
		{
			OrgHeader organization = Factory.New<OrgHeader>();
			AssertEquals("RegenerateCodeWhenOrgTypesChange", false, organization.RegenerateCodeWhenOrgTypesChange);
			organization.OH_FullName = "APPLE PIE";
			organization.OH_RL_NKClosestPort = "AUSYD";
			organization.OH_Code = "XXX";

			organization.OH_IsBroker = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsBroker = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsShippingProvider = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsShippingProvider = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsCompetitor = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsCompetitor = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsConsignee = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsConsignee = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsConsignor = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsConsignor = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsForwarder = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsForwarder = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsCreditor = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsCreditor = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsDebtor = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsDebtor = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsSalesLead = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsSalesLead = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsMiscFreightServices = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsMiscFreightServices = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsTransportClient = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsTransportClient = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsWarehouseClient = true;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);

			organization.OH_IsWarehouseClient = false;
			AssertEquals("OH_Code", "XXX", organization.OH_Code);
		}

		public void TestCodeRegenerationOnSave()
		{
			OrgCodeAlgorithm defaultAlgorithm = new OrgCodeAlgorithm();
			OrgCodeAlgorithm overrideAlgorithm = new OrgCodeAlgorithm();

			overrideAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			overrideAlgorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			overrideAlgorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			overrideAlgorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 2;
			overrideAlgorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 5;
			overrideAlgorithm.SelectableOrgTypes[OrgCodeOrgTypeDescription.Broker].Selected = true;
			overrideAlgorithm.RegenerateOrgCodeOnChanges = true;

			defaultAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			defaultAlgorithm.Elements[OrgCodeElementDescription.LastName].Order = 1;
			defaultAlgorithm.Elements[OrgCodeElementDescription.LastName].Length = 5;
			defaultAlgorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 2;
			defaultAlgorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;
			defaultAlgorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultAlgorithm);
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overrideAlgorithm);

			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgHeader org3 = Factory.New<OrgHeader>();
			OrgHeader org4 = Factory.New<OrgHeader>();

			org1.OH_IsBroker = true;
			org2.OH_IsBroker = true;

			org1.OH_FullName = "APPLE PIE";
			org2.OH_FullName = "CHEESE CAKE";
			org3.OH_FullName = "ORANGE JUICE";
			org4.OH_FullName = "FROZEN COKE";

			org1.OH_RL_NKClosestPort = "AUSYD";
			org2.OH_RL_NKClosestPort = "USORD";
			org3.OH_RL_NKClosestPort = "AUBNE";
			org4.OH_RL_NKClosestPort = "USCHI";

			Factory.Save();

			AssertEquals("org1.OH_Code", "APP00001", org1.OH_Code);
			AssertEquals("org2.OH_Code", "CHE00002", org2.OH_Code);
			AssertEquals("org3.OH_Code", "JUICE001", org3.OH_Code);
			AssertEquals("org4.OH_Code", "FCOKE001", org4.OH_Code);

			org1.OH_FullName = "APPLOOSE PIEZA";
			org2.OH_IsGlobalAccount = true;
			org3.OH_FullName = "FREE COKE";

			Factory.Save();

			AssertEquals("org1.OH_Code", "APP00001", org1.OH_Code);
			AssertEquals("org2.OH_Code", "CHE00003_WW", org2.OH_Code);
			AssertEquals("org3.OH_Code", "FCOKE002", org3.OH_Code);
			AssertEquals("org4.OH_Code", "FCOKE001", org4.OH_Code);

			org1.OH_Code = "XXX";
			org4.OH_FullName = "BANANA SPLIT";

			Factory.Save();

			AssertEquals("org1.OH_Code", "XXX", org1.OH_Code);
			AssertEquals("org2.OH_Code", "CHE00003_WW", org2.OH_Code);
			AssertEquals("org3.OH_Code", "FCOKE002", org3.OH_Code);
			AssertEquals("org4.OH_Code", "SPLIT001", org4.OH_Code);

			var orgFountain = Env.NumberFountains.OrgCodeNumberFountain;
			AssertEquals("Next number for the global fountain", 4L, orgFountain.PeekPreliminary(Factory));
		}

		public void TestOverridingCodes()
		{
			bool currentCanUserEditSetting = Env.Registry.CanUserEditOrganisationCode;

			try
			{
				Env.Registry.CanUserEditOrganisationCode = true;
				company.OH_Code = string.Empty;
				company.OH_FullName = "TEST ORG";
				company.OH_RL_NKClosestPort = "AUSYD";
				AssertEquals("Code should be generated", "TESORGSYD", company.OH_Code);

				company.OH_IsNationalAccount = true;
				AssertEquals("Code should be overridden, as the old code was a generated code", "TESORG_AU", company.OH_Code);

				company.OH_Code = "TESTCODE";
				company.OH_IsNationalAccount = false;
				AssertEquals("Code should not be overridden, as the old code was not a generated code", "TESTCODE", company.OH_Code);

				Env.Registry.CanUserEditOrganisationCode = false;
				company.OH_IsNationalAccount = true;
				AssertEquals("Code should be overridden, because users cannot edit the code manually.", "TESORG_AU", company.OH_Code);
			}
			finally
			{
				Env.Registry.CanUserEditOrganisationCode = currentCanUserEditSetting;
			}
		}

		public void TestInvalidUNLOCOForCode_ShouldGenerateCode()
		{
			company.OH_FullName = "TEST ORG";
			company.OH_RL_NKClosestPort = "WRONG";
			AssertEquals("Code should not have changed since last successful set", "TESORGSYD", company.OH_Code);
		}

		public void TestUniqueCodeGenerationWithFountain()
		{
			OrgCodeAlgorithm overrideAlgorithm = new OrgCodeAlgorithm();

			overrideAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			overrideAlgorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			overrideAlgorithm.Elements[OrgCodeElementDescription.FirstName].Length = 6;
			overrideAlgorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 2;
			overrideAlgorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 3;
			overrideAlgorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overrideAlgorithm);

			OrgHeader org1 = Factory.New<OrgHeader>();

			org1.OH_FullName = "ABCTST";
			org1.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			AssertEquals("ABCTST001", org1.OH_Code);

			org1.OH_Code = "ABCTST002";
			Factory.Save();

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "ABCTST";
			org2.OH_RL_NKClosestPort = "AUSYD";

			Factory.Save();
			AssertEquals("ABCTST003", org2.OH_Code);
		}

		public void TestRegenerateCodeWithChangedNumber()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "ABCTST";
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.OH_Code = "ABCTSTSYD2";
			Assert("!HasWarnings", !org1.OH_CodeInfo.HasWarnings());
			Factory.Save();
			org1.OH_RL_NKClosestPort = "USSAN";
			AssertEquals("ABCTSTSAN", org1.OH_Code);
			Assert("!HasWarnings", !org1.OH_CodeInfo.HasWarnings());
		}

		public void TestRegenerateCodeWhenUpdatingUNLOCOAndUsingUniqueNumber()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.LastName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.LastName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.CountryCode].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.CountryCode].Length = 2;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 4;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 2;

			algorithm.RegenerateOrgCodeOnChanges = true;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "";
			org1.OH_FullName = "ABC 123 XYZ";

			AssertEquals("Can not generate a code that requires a country code without a country.", string.Empty, org1.OH_Code);

			org1.OH_RL_NKClosestPort = "USSAN";
			AssertEquals("Org code has been regened.", "ABCXYZUS01", org1.OH_Code);
		}

		public void TestRegenerateCodeForChangedUnolocoEvenIfCodePossiblyChangedByUser()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "ABC";
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.OH_Code = "ABCTSTSYD2";
			Factory.Save();
			org1.OH_RL_NKClosestPort = "USSAN";
			AssertEquals("ABCSAN", org1.OH_Code);
			Assert("HasWarnings", org1.OH_CodeInfo.HasWarnings());
			org1.OH_Code = "ABCTSTSAN";
			Assert("!HasWarnings", !org1.OH_CodeInfo.HasWarnings());
		}

		public void TestRegenerateOrgCodeOnChangesRule()
		{
			Env.Registry.CanUserEditOrganisationCode = true;
			OrgCodeAlgorithm overrideAlgorithm = new OrgCodeAlgorithm();
			OrgCodeAlgorithm algorithm = OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.Value;
			algorithm.RegenerateOrgCodeOnChanges = false;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			// don't regenerate codes for saved org
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "ABCTST";
			org.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			string oldCode = org.OH_Code;
			org.OH_RL_NKClosestPort = "USSAN";
			AssertEquals(oldCode, org.OH_Code);
			Factory.Save();
			AssertEquals(oldCode, org.OH_Code);

			// don't regnerate user entered code
			org = Factory.New<OrgHeader>();
			org.OH_Code = "55555";
			org.OH_FullName = "ABCTST";
			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("55555", org.OH_Code);
			Factory.Save();
			AssertEquals("55555", org.OH_Code);

			// do regenerate generated code for unsaved org
			org = Factory.New<OrgHeader>();
			org.OH_FullName = "XYZTST";
			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("XYZTSTSYD", org.OH_Code);
			org.OH_RL_NKClosestPort = "USSAN";
			AssertEquals("XYZTSTSAN", org.OH_Code);
		}

		public void TestRegenerateDoesNotUseInvalidAlgorithm()
		{
			OrgCodeAlgorithm algorithm = new AllowEmptyOrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			algorithm.RegenerateOrgCodeOnChanges = true;
			foreach (OrgCodeElement element in algorithm.Elements)
			{
				element.Order = 0;
			}
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "ABCTST";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_Code = "ABCTSTSYD";
			Factory.Save();
			AssertEquals("ABCTSTSYD", org.OH_Code);

			org.OH_RL_NKClosestPort = "USSAN";
			AssertEquals("ABCTSTSYD", org.OH_Code);
			Assert("HasWarnings", org.OH_CodeInfo.HasWarnings());
			Factory.Save();
			AssertEquals("ABCTSTSYD", org.OH_Code);
		}

		[System.Xml.Serialization.XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
		class AllowEmptyOrgCodeAlgorithm : OrgCodeAlgorithm
		{
			protected override void RunPreSaveValidationCore()
			{
			}
		}

		public void TestOH_CodeReadOnly()
		{
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();

			Env.Registry.CanUserEditOrganisationCode = true;
			OrgHeader header1 = OrgHeader.New(Factory);
			header1.FillWithValidTestData();
			Assert("Should be editable", !header1.OH_CodeInfo.ReadOnly);

			Factory.Save();
			OrgHeader header2 = secondFactory.Load<OrgHeader>(header1.PK);
			Assert("Should be editable", !header2.OH_CodeInfo.ReadOnly);

			Env.Registry.CanUserEditOrganisationCode = false;
			header1 = OrgHeader.New(Factory);
			header1.FillWithValidTestData();
			Assert("Should be editable", header1.OH_CodeInfo.ReadOnly);

			Factory.Save();

			Env.Security.OrgDetailsModifyCode.IsAllowed = true;
			Env.Registry.CanUserEditOrganisationCode = true;
			Assert("Should be editable", !header1.OH_CodeInfo.ReadOnly);

			Env.Registry.CanUserEditOrganisationCode = false;
			header2 = secondFactory.Load<OrgHeader>(header1.PK);
			Assert("Should be editable", header2.OH_CodeInfo.ReadOnly);

			Env.Security.OrgDetailsModifyCode.IsAllowed = false;
			Env.Registry.CanUserEditOrganisationCode = false;
			Assert("Should be readonly", header1.OH_CodeInfo.ReadOnly);

			Env.Security.OrgDetailsModifyCode.IsAllowed = true;
			Env.Registry.CanUserEditOrganisationCode = false;
			Assert("Should be readonly", header1.OH_CodeInfo.ReadOnly);

			Env.Security.OrgDetailsModifyCode.IsAllowed = false;
			Env.Registry.CanUserEditOrganisationCode = true;
			Assert("Should be readonly", header1.OH_CodeInfo.ReadOnly);
		}

		#endregion

		#region TestOH_FullName

		public void TestOH_FullName()
		{
			company = BasicCompanyForTest;
			company.OH_FullName = "Test Org";
			AssertEquals("OH_Code", "DUUUUH", company.OH_Code);

			company.OH_RL_NKClosestPort = "USBTM";
			AssertEquals("OH_Code", "TESORGBTM", company.OH_Code);

			company.OH_FullName = " Test Org";
			AssertEquals("Leading space should be removed", "Test Org", company.OH_FullName);

			company.OH_FullName = "   Test Org";
			AssertEquals("Leading space should be removed", "Test Org", company.OH_FullName);
		}

		public void TestOH_FullName_WithInvalidUnicode()
		{
			AssertNoExceptionThrown(() => company.OH_FullName = "Test" + char.ConvertFromUtf32(0x10ffff) + "Org");
			Assert("Error - Full Name contains invalid character(s)", company.OH_FullNameInfo.HasError("Full Name contains invalid character(s)"));
		}

		#endregion

		#region TestOH_IsNationalAccount

		public void TestIsNationalAccount()
		{
			company.OH_FullName = "Test Org";
			company.OH_RL_NKClosestPort = "AUSYD";
			company.OH_IsNationalAccount = false;

			AssertEquals("Code should use UNLOCO", "TESORGSYD", company.OH_Code);

			company.OH_IsNationalAccount = true;
			AssertEquals("Code should use UNLOCO", "TESORG_AU", company.OH_Code);

			company.OH_IsNationalAccount = false;
			AssertEquals("Code should use UNLOCO", "TESORGSYD", company.OH_Code);
		}

		#endregion

		#region TestOH_IsAirline

		public void TestOH_IsAirline()
		{
			var airLine = Factory.NewWithValidTestData<RefAirline>();

			company.OH_IsAirLine = true;
			company.MiscServ.OM_RM_Airline = airLine.PK;
			company.CompanyData.OB_APAirlineAccountNumber = "321";
			AssertEquals("MiscServ.OM_RM_Airline", airLine.PK, company.MiscServ.OM_RM_Airline);
			AssertEquals("MiscServ.Airline", airLine, company.MiscServ.Airline);
			AssertEquals("CompanyData.OB_APAirlineAccountNumber", "321", company.CompanyData.OB_APAirlineAccountNumber);

			company.OH_IsAirLine = false;
			AssertEquals("MiscServ.OM_RM_Airline", ZGuid.Empty, company.MiscServ.OM_RM_Airline);
			AssertEquals("CompanyData.OB_APAirlineAccountNumber", string.Empty, company.CompanyData.OB_APAirlineAccountNumber);
			AssertEquals("Airline details should also be cleared", null, company.MiscServ.Airline);

			company.MiscServ.MarkLightValidationAsValidForTesting();
			AssertEquals("Precondition: Should be marked as NOT Needing Validation", true, company.MiscServ.LightValidationIsValid);
			company.OH_IsAirLine = true;
			AssertEquals("Should be marked as Needing Validation", false, company.MiscServ.LightValidationIsValid);
		}

		#endregion

		#region TestOH_IsConsignee

		public void TestOH_IsConsignee()
		{
			company.OH_IsConsignee = true;
			company.SupplierLinks.AddNew();
			company.SupplierLinks.AddNew();
			company.CustomLabels.AddNew();
			company.ConsigneeContainerPenalties.AddNew();
			AssertEquals("SupplierLinks.Count", 2, company.SupplierLinks.Count);
			AssertEquals("CustomLabels.Count", 1, company.CustomLabels.Count);
			AssertEquals("ConsigneeContainerPenalties.Count", 1, company.ConsigneeContainerPenalties.Count);

			company.OH_IsConsignee = false;
			AssertEquals("SupplierLinks.Count", 0, company.SupplierLinks.Count);
			AssertEquals("CustomLabels.Count", 0, company.CustomLabels.Count);
			AssertEquals("ConsigneeContainerPenalties.Count", 0, company.ConsigneeContainerPenalties.Count);
		}

		public void TestRegisteredEditableChildrenForConsignee()
		{
			company.OH_IsConsignee = true;
			Assert("RateTariffLevels is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));

			company.OH_IsConsignee = false;
			Assert("RateTariffLevels is not registered editable child", !company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));

			company.OH_IsConsignee = true;
			Assert("RateTariffLevels is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));
		}

		#endregion

		#region TestOH_IsConsignor

		public void TestOH_IsConsignor()
		{
			company.OH_IsConsignor = true;
			company.BuyerLinks.AddNew();
			company.BuyerLinks.AddNew();
			AssertEquals("BuyerLinks.Count", 2, company.BuyerLinks.Count);

			company.OH_IsConsignor = false;
			AssertEquals("BuyerLinks.Count", 0, company.BuyerLinks.Count);
		}

		public void TestRegisteredEditableChildrenForConsigor()
		{
			company.OH_IsConsignor = true;
			Assert("RateTariffLevels is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));

			company.OH_IsConsignor = false;
			Assert("RateTariffLevels is not registered editable child", !company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));

			company.OH_IsConsignor = true;
			Assert("RateTariffLevels is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));
		}

		#endregion

		#region TestOH_IsForwarder

		public void TestForwarderDependentCollectionsAreEmptied()
		{
			company.OH_IsForwarder = true;
			company.AppointedAgentPorts.AddNew();
			company.AppointedAgentPorts.AddNew();
			company.AppointedGatewayAgentPorts.AddNew();
			AssertEquals("AppointedAgentPorts.Count", 2, company.AppointedAgentPorts.Count);
			AssertEquals("AppointedGatewayAgentPorts.Count", 1, company.AppointedGatewayAgentPorts.Count);

			company.OH_IsForwarder = false;
			AssertEquals("AppointedAgentPorts.Count - collection should be cleared after org type is changed", 0, company.AppointedAgentPorts.Count);
			AssertEquals("AppointedGatewayAgentPorts.Count - collection should be cleared after org type is changed", 0, company.AppointedGatewayAgentPorts.Count);
		}

		#endregion

		#region TestOH_IsSalesLead

		public void TestOH_IsSalesLead()
		{
			var periodOfActivities = new CodeDescriptionWithEnabledAndDefaultCollection(5);
			periodOfActivities.AddNew("AAA", (NoResString)"AAA Desc", true, true);
			periodOfActivities.AddNew("BBB", (NoResString)"BBB Desc", false, true);
			OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodOfActivities);

			company.OH_IsSalesLead = true;
			company.SalesCalls.AddNew();
			AssertEquals("SalesCalls.Count", 1, company.SalesCalls.Count);
			AssertEquals("Period of Activity should have been defaulted", "AAA", company.MiscServ.OM_CMPeriodOfActivity);

			company.MiscServ.OM_CMPeriodOfActivity = "BBB";
			company.OH_IsSalesLead = false;
			AssertEquals("SalesCalls.Count", 1, company.SalesCalls.Count);
			AssertEquals("BBB", company.MiscServ.OM_CMPeriodOfActivity);

			company.OH_IsSalesLead = true;
			AssertEquals("Period of Activity should not have been defaulted - value already exists", "BBB", company.MiscServ.OM_CMPeriodOfActivity);
		}

		public void TestRegisteredEditableChildrenForSalesLead()
		{
			company.OH_IsSalesLead = true;
			Assert("SalesCalls should not be registered editable child", !company.IsRegisteredEditableChildObject(company.SalesCalls));
			Assert("SalesOpportunities is registered editable child", company.IsRegisteredEditableChildObject(company.SalesOpportunities));
			Assert("RateTariffLevels is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));

			company.OH_IsSalesLead = false;
			Assert("SalesCalls should not be registered editable child", !company.IsRegisteredEditableChildObject(company.SalesCalls));
			Assert("SalesOpportunities is not registered editable child", !company.IsRegisteredEditableChildObject(company.SalesOpportunities));
			Assert("RateTariffLevels is not registered editable child", !company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));

			company.OH_IsSalesLead = true;
			Assert("SalesCalls should not be registered editable child", !company.IsRegisteredEditableChildObject(company.SalesCalls));
			Assert("SalesOpportunities is registered editable child", company.IsRegisteredEditableChildObject(company.SalesOpportunities));
			Assert("RateTariffLevels is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));
		}

		public void TestSalesLeadCollectionsHasChangesIsCleared()
		{
			company.OH_IsSalesLead = true;

			OrgOpportunity opportunity = company.SalesOpportunities.AddNew();
			opportunity.P8_DiscountAmount = 12;

			OrgSalesCall call = company.SalesCalls.AddNew();

			Assert("SalesOpportunities.HasChanges should be true", company.SalesOpportunities.HasChanges);
			Assert("SalesCalls.HasChanges should be true", ((IBusinessObjectState)company.SalesCalls).HasChanges);

			company.OH_IsSalesLead = false;
			Assert("SalesOpportunities.HasChanges should be false", !company.SalesOpportunities.HasChanges);
			Assert("SalesCalls.HasChanges should be false", !((IBusinessObjectState)company.SalesCalls).HasChanges);
		}

		#endregion

		#region TestOH_IsCompetitor

		public void TestOH_IsCompetitor()
		{
			company.OH_IsCompetitor = true;
			company.Clients.AddNew();
			AssertEquals("Clients.Count", 1, company.Clients.Count);

			company.OH_IsCompetitor = false;
			AssertEquals("Clients.Count", 0, company.Clients.Count);
		}

		#endregion

		#region TestOH_IsShippingProvider

		public void TestOH_IsShippingProvider()
		{
			var airLine = Factory.NewWithValidTestData<RefAirline>();

			company.OH_IsShippingProvider = true;
			company.OH_IsAirLine = true;
			company.MiscServ.OM_RM_Airline = airLine.PK;
			company.OH_IsShippingProvider = false;
			AssertEquals("MiscServ.OM_RM_Airline", ZGuid.Empty, company.MiscServ.OM_RM_Airline);
			AssertEquals("Airline flag should also be cleared", false, company.OH_IsAirLine);
			AssertEquals("Airline details should also be cleared", null, company.MiscServ.Airline);
		}

		public void TestShippingProviderCollectionsAreEmptied()
		{
			company.OH_IsShippingProvider = true;
			company.CarrierContainerPenalties.AddNew();
			company.CarrierContainerPenalties.AddNew();
			AssertEquals("CarrierContainerPenalties.Count", 2, company.CarrierContainerPenalties.Count);

			company.OH_IsShippingProvider = false;
			AssertEquals("CarrierContainerPenalties.Count - collection should be cleared after org type is changed", 0, company.CarrierContainerPenalties.Count);
		}

		public void TestOH_IsShippingProvider_BailsOutWhenNoGUIAttached()
		{
			company.OH_IsShippingProvider = true;
			company.CarrierContainerPenalties.AddNew();
			company.CarrierContainerPenalties.AddNew();
			AssertEquals("OH_IsShippingProvider", true, company.OH_IsShippingProvider);

			company.OH_IsShippingProvider = false;
			AssertEquals("OH_IsShippingProvider", false, company.OH_IsShippingProvider);
		}

		public void TestOH_IsShippingProvider_CallsGUIWhenAttached()
		{
			company.OH_IsShippingProvider = true;
			company.CarrierContainerPenalties.AddNew();
			company.CarrierContainerPenalties.AddNew();
			AssertEquals("OH_IsShippingProvider", true, company.OH_IsShippingProvider);

			company.OH_IsShippingProvider = false;
			AssertEquals("OH_IsShippingProvider", false, company.OH_IsShippingProvider);
		}

		#endregion

		#region Controlling Agent and Customer tests

		public void TestControllingAgentCollectionsAreEmptied_OH_IsControllingAgent_EnableControllingAgentFunctionalityAndValidationsIsTrue()
		{
			AssertControllingAgentCollectionsAreEmptied_EnableControllingAgentFunctionalityAndValidationsIsTrue(company.OH_IsControllingAgentInfo);
		}

		public void TestControllingAgentCollectionsAreEmptied_OH_IsForwarder_EnableControllingAgentFunctionalityAndValidationsIsTrue()
		{
			AssertControllingAgentCollectionsAreEmptied_EnableControllingAgentFunctionalityAndValidationsIsTrue(company.OH_IsForwarderInfo);
		}

		void AssertControllingAgentCollectionsAreEmptied_EnableControllingAgentFunctionalityAndValidationsIsTrue(ZPropertyInfo info)
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			info.Value = ZBool.True;

			var party1 = company.AllParentParties.AddNew();
			party1.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;

			var party2 = company.AllParentParties.AddNew();
			party2.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;

			var party3 = company.AllParentParties.AddNew();
			party3.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;

			AssertEquals("AllParentParties.Count", 3, company.AllParentParties.Count);

			info.Value = ZBool.False;
			AssertEquals("AllParentParties.Count - Controlling Agent related parties should be removed after org type is changed", 1, company.AllParentParties.Count);
			Assert("AllParentParties should contain only non-Controlling Agent related parties", company.AllParentParties.Contains(party2));
		}

		public void TestControllingAgentCollectionsAreEmptied_OH_IsControllingAgent_EnableControllingAgentFunctionalityAndValidationsIsFalse()
		{
			AssertControllingAgentCollectionsAreEmptied_EnableControllingAgentFunctionalityAndValidationsIsFalse(company.OH_IsControllingAgentInfo);
		}

		public void TestControllingAgentCollectionsAreEmptied_OH_IsForwarder_EnableControllingAgentFunctionalityAndValidationsIsFalse()
		{
			AssertControllingAgentCollectionsAreEmptied_EnableControllingAgentFunctionalityAndValidationsIsFalse(company.OH_IsForwarderInfo);
		}

		void AssertControllingAgentCollectionsAreEmptied_EnableControllingAgentFunctionalityAndValidationsIsFalse(ZPropertyInfo info)
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			info.Value = ZBool.True;

			var party1 = company.AllParentParties.AddNew();
			party1.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;

			var party2 = company.AllParentParties.AddNew();
			party2.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;

			var party3 = company.AllParentParties.AddNew();
			party3.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;

			AssertEquals("AllParentParties.Count", 3, company.AllParentParties.Count);

			info.Value = ZBool.False;
			AssertEquals("AllParentParties.Count", 3, company.AllParentParties.Count);
		}

		public void TestControllingCustomerCollectionsAreEmptied_EnableControllingCustomerFunctionalityAndValidationsIsTrue()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			company.OH_IsControllingCustomer = true;

			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var org3SupplierLink1 = org3.SupplierLinks.AddNew();
			org3SupplierLink1.OL_OH_ControllingCustomer = company.PK;
			org3SupplierLink1.OL_OH_Supplier = org1.PK;

			var org3BuyerLink1 = org3.BuyerLinks.AddNew();
			org3BuyerLink1.OL_OH_ControllingCustomer = company.PK;
			org3BuyerLink1.OL_OH_Buyer = org1.PK;

			var org3BuyerLink2 = org3.BuyerLinks.AddNew();
			org3BuyerLink2.OL_OH_ControllingCustomer = org2.PK;
			org3BuyerLink2.OL_OH_Buyer = org1.PK;

			var mode1 = org3BuyerLink2.OrgSupBuyLinkTrnModes[0];
			mode1.PF_OH_ControllingCustomer = company.PK;

			var mode2 = org3BuyerLink2.OrgSupBuyLinkTrnModes.AddNew();
			mode2.PF_OH_ControllingCustomer = org2.PK;

			var party1 = company.AllParentParties.AddNew();
			party1.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;

			var party2 = company.AllParentParties.AddNew();
			party2.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;

			var party3 = company.AllParentParties.AddNew();
			party3.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;

			AssertEquals("AllParentParties.Count", 3, company.AllParentParties.Count);
			AssertEquals("org3.SupplierLinks.Count", 1, org3.SupplierLinks.Count);
			AssertEquals(company.PK, org3SupplierLink1.OL_OH_ControllingCustomer);
			AssertEquals("org3.BuyerLinks.Count", 2, org3.BuyerLinks.Count);
			AssertEquals(company.PK, org3BuyerLink1.OL_OH_ControllingCustomer);
			AssertEquals(org2.PK, org3BuyerLink2.OL_OH_ControllingCustomer);
			AssertEquals("org3BuyerLink2.OrgSupBuyLinkTrnModes.Count", 2, org3BuyerLink2.OrgSupBuyLinkTrnModes.Count);
			AssertEquals(company.PK, mode1.PF_OH_ControllingCustomer);
			AssertEquals(org2.PK, mode2.PF_OH_ControllingCustomer);

			company.OH_IsControllingCustomer = false;
			AssertEquals("AllParentParties.Count - Controlling Customer related parties should be removed after org type is changed", 1, company.AllParentParties.Count);
			Assert("AllParentParties should contain only non-Controlling Customer related parties", company.AllParentParties.Contains(party2));
			AssertEquals("org3.SupplierLinks.Count", 1, org3.SupplierLinks.Count);
			AssertEquals(ZGuid.Empty, org3SupplierLink1.OL_OH_ControllingCustomer);
			AssertEquals("org3.BuyerLinks.Count", 2, org3.BuyerLinks.Count);
			AssertEquals(ZGuid.Empty, org3BuyerLink1.OL_OH_ControllingCustomer);
			AssertEquals(org2.PK, org3BuyerLink2.OL_OH_ControllingCustomer);
			AssertEquals("org3BuyerLink2.OrgSupBuyLinkTrnModes.Count", 2, org3BuyerLink2.OrgSupBuyLinkTrnModes.Count);
			AssertEquals(ZGuid.Empty, mode1.PF_OH_ControllingCustomer);
			AssertEquals(org2.PK, mode2.PF_OH_ControllingCustomer);
		}

		public void TestControllingCustomerCollectionsAreEmptied_EnableControllingCustomerFunctionalityAndValidationsIsFalse()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			company.OH_IsControllingCustomer = true;

			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var org3SupplierLink1 = org3.SupplierLinks.AddNew();
			org3SupplierLink1.OL_OH_ControllingCustomer = company.PK;
			org3SupplierLink1.OL_OH_Supplier = org1.PK;

			var org3BuyerLink1 = org3.BuyerLinks.AddNew();
			org3BuyerLink1.OL_OH_ControllingCustomer = company.PK;
			org3BuyerLink1.OL_OH_Buyer = org1.PK;

			var org3BuyerLink2 = org3.BuyerLinks.AddNew();
			org3BuyerLink2.OL_OH_ControllingCustomer = org2.PK;
			org3BuyerLink2.OL_OH_Buyer = org1.PK;

			var mode1 = org3BuyerLink2.OrgSupBuyLinkTrnModes[0];
			mode1.PF_OH_ControllingCustomer = company.PK;

			var mode2 = org3BuyerLink2.OrgSupBuyLinkTrnModes.AddNew();
			mode2.PF_OH_ControllingCustomer = org2.PK;

			var party1 = company.AllParentParties.AddNew();
			party1.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;

			var party2 = company.AllParentParties.AddNew();
			party2.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;

			var party3 = company.AllParentParties.AddNew();
			party3.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			AssertEquals("AllParentParties.Count", 3, company.AllParentParties.Count);
			AssertEquals("org3.SupplierLinks.Count", 1, org3.SupplierLinks.Count);
			AssertEquals(company.PK, org3SupplierLink1.OL_OH_ControllingCustomer);
			AssertEquals("org3.BuyerLinks.Count", 2, org3.BuyerLinks.Count);
			AssertEquals(company.PK, org3BuyerLink1.OL_OH_ControllingCustomer);
			AssertEquals(org2.PK, org3BuyerLink2.OL_OH_ControllingCustomer);
			AssertEquals("org3BuyerLink2.OrgSupBuyLinkTrnModes.Count", 2, org3BuyerLink2.OrgSupBuyLinkTrnModes.Count);
			AssertEquals(company.PK, mode1.PF_OH_ControllingCustomer);
			AssertEquals(org2.PK, mode2.PF_OH_ControllingCustomer);

			company.OH_IsControllingCustomer = false;
			AssertEquals("AllParentParties.Count", 3, company.AllParentParties.Count);
			AssertEquals("org3.SupplierLinks.Count", 1, org3.SupplierLinks.Count);
			AssertEquals(company.PK, org3SupplierLink1.OL_OH_ControllingCustomer);
			AssertEquals("org3.BuyerLinks.Count", 2, org3.BuyerLinks.Count);
			AssertEquals(company.PK, org3BuyerLink1.OL_OH_ControllingCustomer);
			AssertEquals(org2.PK, org3BuyerLink2.OL_OH_ControllingCustomer);
			AssertEquals("org3BuyerLink2.OrgSupBuyLinkTrnModes.Count", 2, org3BuyerLink2.OrgSupBuyLinkTrnModes.Count);
			AssertEquals(company.PK, mode1.PF_OH_ControllingCustomer);
			AssertEquals(org2.PK, mode2.PF_OH_ControllingCustomer);
		}

		#endregion

		public void TestRegisteredEditableChildrenForDebtor()
		{
			company.OH_IsDebtor = true;
			Assert("RateTariffLevels is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));
			Assert("InvoiceTypes is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.InvoiceTypes));
			Assert("InvoiceRollupOrGroups is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.InvoiceRollupOrGroups));

			company.OH_IsDebtor = false;
			Assert("RateTariffLevels is not registered editable child", !company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));
			Assert("InvoiceTypes is not registered editable child", !company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.InvoiceTypes));
			Assert("InvoiceRollupOrGroups is not registered editable child", !company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.InvoiceRollupOrGroups));

			company.OH_IsDebtor = true;
			Assert("RateTariffLevels is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.RateTariffLevels));
			Assert("InvoiceTypes is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.InvoiceTypes));
			Assert("InvoiceRollupOrGroups is registered editable child", company.CompanyData.IsRegisteredEditableChildObject(company.CompanyData.InvoiceRollupOrGroups));
		}

		#region TestOH_ScreeningStatus

		public void TestOH_ScreeningStatus()
		{
			AssertEquals("OH_ScreeningStatus must be readonly", true, company.OH_ScreeningStatusInfo.ReadOnly);
			var referenceList = OrgHeaderLookups.GetScreeningStatusesList(Factory);
			var screeningStatusesList = CargoWise.ComponentModel.MetaData.GetListDataSource(company, company.OH_ScreeningStatusInfo.PropertyDescriptor);

			AssertType<ScreeningStatusesList>(referenceList);
			AssertNotNull(referenceList);
			Assert("Same instance", object.ReferenceEquals(screeningStatusesList, referenceList));

			referenceList = OrgHeaderLookups.GetScreeningStatusesList(null);
			AssertType<ScreeningStatusesList>(referenceList);
			AssertNotNull(referenceList);
			Assert("Not same instance", !object.ReferenceEquals(screeningStatusesList, referenceList));
		}

		#endregion

		#endregion

		#region Organisation Types

		public void TestGetSetOrganisationType()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			foreach (OrganisationTypes type in Enum.GetValues(typeof(OrganisationTypes)))
			{
				organisation.OrganisationTypes = type;
				AssertEquals("Organisation type set correctly", true, organisation.OrganisationTypes == type);
			}
		}

		public void TestGetSetOrganisationTypeCombination()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrganisationTypes types = OrganisationTypes.Consignee & OrganisationTypes.Debtor;

			organisation.OrganisationTypes = types;
			AssertEquals("Organisation type set correctly", true, (organisation.OrganisationTypes & types) == 0);
		}

		public void TestForNewOrgTypeFlagColumnAddedToDB()
		{
			ArrayList orgHeaderAndOrgCompanyDataColumns = new ArrayList();
			orgHeaderAndOrgCompanyDataColumns.AddRange(OrgHeaderSchema.All);
			orgHeaderAndOrgCompanyDataColumns.AddRange(OrgCompanyDataSchema.All);

			string newOrgTypeBooleanColumns = string.Empty;
			foreach (SchemaColumn column in orgHeaderAndOrgCompanyDataColumns)
			{
				bool isBooleanIsColumn =
					column is SchemaBoolColumn &&
					(column.Name.StartsWith("OH_Is") || column.Name.StartsWith("OB_Is"));

				if (isBooleanIsColumn &&
					!OrgTypeBooleanColumns.Contains(column.Name) &&
					!NonOrgTypeBooleanColumnsToExclude.Contains(column.Name))
				{
					newOrgTypeBooleanColumns += column.Name + "\r\n";
				}
			}

			string message =
				"The following fields may be new organisation types.\r\n" +
				"Either add them to the OrganisationTypes enum and the OrgTypeBooleanColumns property below, or add it to NonOrgTypeBooleanColumnsToExclude below";
			AssertEquals(message, string.Empty, newOrgTypeBooleanColumns);

			AssertEquals(
				"The OrgTypeBooleanColumns below doesn't have the correct number of items in it. It should have the same number of items as the OrganisationTypes enum (excluding 'None')",
				Enum.GetValues(typeof(OrganisationTypes)).Length - 1, OrgTypeBooleanColumns.Count);
		}

		static readonly IList OrgTypeBooleanColumns = new string[]
		{
			OrgCompanyDataSchema.OB_IsDebtor.Name,
			OrgCompanyDataSchema.OB_IsCreditor.Name,
			OrgHeaderSchema.OH_IsConsignor.Name,
			OrgHeaderSchema.OH_IsConsignee.Name,
			OrgHeaderSchema.OH_IsTransportClient.Name,
			OrgHeaderSchema.OH_IsShippingProvider.Name,
			OrgHeaderSchema.OH_IsForwarder.Name,
			OrgHeaderSchema.OH_IsBroker.Name,
			OrgHeaderSchema.OH_IsMiscFreightServices.Name,
			OrgHeaderSchema.OH_IsCompetitor.Name,
			OrgHeaderSchema.OH_IsSalesLead.Name,
			OrgHeaderSchema.OH_IsWarehouseClient.Name,
			OrgHeaderSchema.OH_IsDistributionCentre.Name,
			OrgHeaderSchema.OH_IsControllingAgent.Name,
			OrgHeaderSchema.OH_IsControllingCustomer.Name
		};

		static readonly IList NonOrgTypeBooleanColumnsToExclude = new string[]
		{
			OrgHeaderSchema.OH_IsActive.Name,
			OrgHeaderSchema.OH_IsRailProvider.Name,
			OrgHeaderSchema.OH_IsInlandWaterwayProvider.Name,
			OrgHeaderSchema.OH_IsContainerYard.Name,
			OrgHeaderSchema.OH_IsPackDepot.Name,
			OrgHeaderSchema.OH_IsNationalAccount.Name,
			OrgHeaderSchema.OH_IsTempAccount.Name,
			OrgHeaderSchema.OH_IsAirLine.Name,
			OrgHeaderSchema.OH_IsSeaCTO.Name,
			OrgHeaderSchema.OH_IsLineHaulProvider.Name,
			OrgHeaderSchema.OH_IsUserFlag1.Name,
			OrgHeaderSchema.OH_IsUserFlag2.Name,
			OrgHeaderSchema.OH_IsUserFlag3.Name,
			OrgHeaderSchema.OH_IsUserFlag4.Name,
			OrgHeaderSchema.OH_IsUserFlag5.Name,
			OrgHeaderSchema.OH_IsUserFlag6.Name,
			OrgHeaderSchema.OH_IsUserFlag7.Name,
			OrgHeaderSchema.OH_IsUserFlag8.Name,
			OrgHeaderSchema.OH_IsUserFlag9.Name,
			OrgHeaderSchema.OH_IsUserFlag10.Name,
			OrgHeaderSchema.OH_IsUserFlag11.Name,
			OrgHeaderSchema.OH_IsUserFlag12.Name,
			OrgHeaderSchema.OH_IsUserFlag13.Name,
			OrgHeaderSchema.OH_IsUserFlag14.Name,
			OrgHeaderSchema.OH_IsUserFlag15.Name,
			OrgHeaderSchema.OH_IsUserFlag16.Name,
			OrgHeaderSchema.OH_IsUserFlag17.Name,
			OrgHeaderSchema.OH_IsUserFlag18.Name,
			OrgHeaderSchema.OH_IsUserFlag19.Name,
			OrgHeaderSchema.OH_IsUserFlag20.Name,
			OrgHeaderSchema.OH_IsUserFlag21.Name,
			OrgHeaderSchema.OH_IsUserFlag22.Name,
			OrgHeaderSchema.OH_IsUserFlag23.Name,
			OrgHeaderSchema.OH_IsUserFlag24.Name,
			OrgHeaderSchema.OH_IsUserFlag25.Name,
			OrgHeaderSchema.OH_IsUserFlag26.Name,
			OrgHeaderSchema.OH_IsUserFlag27.Name,
			OrgHeaderSchema.OH_IsUserFlag28.Name,
			OrgHeaderSchema.OH_IsUserFlag29.Name,
			OrgHeaderSchema.OH_IsUserFlag30.Name,
			OrgHeaderSchema.OH_IsUserFlag31.Name,
			OrgHeaderSchema.OH_IsUserFlag32.Name,
			OrgHeaderSchema.OH_IsUnpackDepot.Name,
			OrgHeaderSchema.OH_IsFerryWaterTerminal.Name,
			OrgHeaderSchema.OH_IsAirCTO.Name,
			OrgHeaderSchema.OH_IsShippingLine.Name,
			OrgHeaderSchema.OH_IsSeaWholesaler.Name,
			OrgHeaderSchema.OH_IsWarehouseClient.Name,
			OrgHeaderSchema.OH_IsLocalTransport.Name,
			OrgHeaderSchema.OH_IsFumigationContractor.Name,
			OrgHeaderSchema.OH_IsVGMContractor.Name,
			OrgHeaderSchema.OH_IsPersonalEffectsAccount.Name,
			OrgHeaderSchema.OH_IsAirWholesaler.Name,
			OrgHeaderSchema.OH_IsRailHead.Name,
			OrgHeaderSchema.OH_IsShippingConsortium.Name,
			OrgHeaderSchema.OH_IsRoadFreightDepot.Name,
			OrgHeaderSchema.OH_IsContainerLeasingCompany.Name,
			OrgHeaderSchema.OH_IsGlobalAccount.Name,
			OrgHeaderSchema.OH_IsValid.Name,
			OrgCompanyDataSchema.OB_IsValid.Name
		};

		public void TestOrganisationTypesAsString()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			AssertEquals("When there are no organisation types, a blank string should be shown", string.Empty, organisation.OrganisationTypesAsString);
			organisation.OH_IsConsignee = true;
			organisation.OH_IsDebtor = true;
			AssertEquals("When there are organisation types they should be comma separated", "Debtor, Consignee", organisation.OrganisationTypesAsString);
		}

		public void TestDebtorAndCreditorCompany()
		{
			var organisation = Factory.New<OrgHeader>();
			var company1 = Factory.New<GlbCompany>();
			company1.CompanyName = "Dummy Company1";
			var company2 = Factory.New<GlbCompany>();
			company2.CompanyName = "Dummy Company2";
			var company3 = Factory.New<GlbCompany>();
			company3.CompanyName = "Dummy Company3";

			var companyData1 = Factory.New<OrgCompanyData>();
			companyData1.OB_OH = organisation.PK;
			companyData1.OB_GC = company1.PK;
			companyData1.Company.CompanyName = "Dummy Company1";

			var companyData2 = Factory.New<OrgCompanyData>();
			companyData2.OB_OH = organisation.PK;
			companyData2.OB_GC = company2.PK;
			companyData2.Company.CompanyName = "Dummy Company2";

			var companyData3 = Factory.New<OrgCompanyData>();
			companyData3.OB_OH = organisation.PK;
			companyData3.OB_GC = company3.PK;
			companyData3.Company.CompanyName = "Dummy Company3";

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("companyData1 is not a debtor company", false, companyData1.OB_IsDebtor);
				AssertEquals("companyData2 is not a debtor company", false, companyData2.OB_IsDebtor);
				AssertEquals("companyData3 is not a debtor company", false, companyData3.OB_IsDebtor);
				AssertEquals("companyData1 is not a creditor company", false, companyData1.OB_IsCreditor);
				AssertEquals("companyData2 is not a creditor company", false, companyData2.OB_IsCreditor);
				AssertEquals("companyData3 is not a creditor company", false, companyData3.OB_IsCreditor);
			});

			AssertEquals(string.Empty, organisation.DebtorCompany);
			AssertEquals(string.Empty, organisation.CreditorCompany);

			companyData1.OB_IsDebtor = true;
			companyData2.OB_IsCreditor = true;

			AssertEquals(true, companyData1.OB_IsDebtor);
			AssertEquals(true, companyData2.OB_IsCreditor);
			AssertEquals("Dummy Company1", organisation.DebtorCompany);
			AssertEquals("Dummy Company2", organisation.CreditorCompany);

			companyData2.OB_IsDebtor = true;
			companyData3.OB_IsCreditor = true;

			AssertEquals(true, companyData2.OB_IsDebtor);
			AssertEquals(true, companyData3.OB_IsCreditor);
			AssertEquals("Dummy Company1, Dummy Company2", organisation.DebtorCompany);
			AssertEquals("Dummy Company2, Dummy Company3", organisation.CreditorCompany);
		}

		public void TestIsOrgTypeInvolvedInOverseasTransactions()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals("New org is not involved in overseas transactions", false, org.IsOrgTypeInvolvedInOverseasTransactions);

			org.OH_IsCreditor = true;
			AssertEquals("Creditor is not involved in overseas transactions", false, org.IsOrgTypeInvolvedInOverseasTransactions);

			org.OH_IsForwarder = true;
			AssertEquals("Forwarder IS involved in overseas transactions", true, org.IsOrgTypeInvolvedInOverseasTransactions);

			org.OH_IsCreditor = false;
			AssertEquals("Forwarder IS involved in overseas transactions", true, org.IsOrgTypeInvolvedInOverseasTransactions);

			org.OH_IsForwarder = false;
			org.OH_IsDebtor = true;
			AssertEquals("Debtor is not involved in overseas transactions", false, org.IsOrgTypeInvolvedInOverseasTransactions);
			org.OH_IsDebtor = false;

			org.OH_IsConsignee = true;
			AssertEquals("Consignee is involved in overseas transactions", true, org.IsOrgTypeInvolvedInOverseasTransactions);
			org.OH_IsConsignee = false;

			org.OH_IsConsignor = true;
			AssertEquals("Consignor is involved in overseas transactions", true, org.IsOrgTypeInvolvedInOverseasTransactions);
			org.OH_IsConsignor = false;

			org.OH_IsShippingProvider = true;
			AssertEquals("Carrier is involved in overseas transactions", true, org.IsOrgTypeInvolvedInOverseasTransactions);
			org.OH_IsShippingProvider = false;

			org.OH_IsCompetitor = true;
			AssertEquals("Competitor is not involved in overseas transactions", false, org.IsOrgTypeInvolvedInOverseasTransactions);
			org.OH_IsCompetitor = false;
		}

		#endregion

		#region New Bound Properties

		#region TestValidateMainWebURL.PU_URL

		#endregion

		public void TestStaffAssignmentBoundProperties()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Wendy";
			staff.GS_Code = "WND";
			OrgHeader org = Factory.New<OrgHeader>();

			org.StaffAssignments.OverallAccountManager = staff.GS_Code;
			AssertEquals("Should be Overall Account Manager name", staff.GS_FullName, org.OverallAccountManagerStaff);

			org.StaffAssignments.OverallAccountManager = ZString.Empty;
			AssertEquals("Overall Account Manager Staff should be empty", ZString.Empty, org.OverallAccountManagerStaff);

			org.StaffAssignments.OverallController = staff.GS_Code;
			AssertEquals("Should be Overall Controller name", staff.GS_FullName, org.OverallControllerStaff);

			org.StaffAssignments.OverallController = ZString.Empty;
			AssertEquals("Overall Controller Staff should be empty", ZString.Empty, org.OverallControllerStaff);

			org.StaffAssignments.OverallCustomerServiceRep = staff.GS_Code;
			AssertEquals("Should be Overall Customer Service Rep name", staff.GS_FullName, org.OverallCustomerServiceRepStaff);

			org.StaffAssignments.OverallCustomerServiceRep = ZString.Empty;
			AssertEquals("Overall Customer Service Rep should be empty", ZString.Empty, org.OverallCustomerServiceRepStaff);

			org.StaffAssignments.OverallSalesRep = staff.GS_Code;
			AssertEquals("Should be Overall Sales Rep name", staff.GS_FullName, org.OverallSalesRepStaff);

			org.StaffAssignments.OverallSalesRep = ZString.Empty;
			AssertEquals("Overall Sales Rep should be empty", ZString.Empty, org.OverallSalesRepStaff);
		}

		#endregion

		#region New Properties

		public void TestCreatedUnderProperties()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "TMC";
			company.Branches.AddNew().GB_Code = "TMB";
			Factory.Save();
			using (DisposableEnvironment.ForBranch("TMB"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				AssertEquals("TMB", org.OH_CreatedUnderBranch);
				AssertEquals("TMC", org.OH_CreatedUnderCompany);
			}
		}

		public void TestOH_CreatedUnderCompany_DoesNotThrowException_WithInvalidCreatedUnderBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = company.Branches.AddNew();
			testBranch.GB_Code = "XYZ";
			Factory.Save();

			OrgHeader org;
			using (DisposableEnvironment.ForBranch("XYZ"))
			{
				org = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
			}

			testBranch.Delete();
			Factory.Save();
			AssertNoExceptionThrown(() => { var x = org.OH_CreatedUnderCompany; });
		}

		public void TestGlobalCreditGroupChilds()
		{
			var currencyCode = Env.CurrentCompany.LocalCurrency.Code;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";
			org1.OH_FullName = "TestOrgFullName1";
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.OB_ARCreditLimit = 50m;
			org1.CompanyData.IncreaseOutstandingBalance(500);
			org1.MiscServ.OM_ARGlobalCreditApproved = true;
			org1.MiscServ.OM_RX_NKARGlobalCreditCurrency = currencyCode;
			org1.MiscServ.OM_ARGlobalCreditLimit = 500m;

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, currencyCode));
			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			exchangeRate.RE_SellRate = 1.1m;
			exchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
			exchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_FullName = "TestOrgFullName2";
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.OB_ARCreditLimit = 75m;
			org2.CompanyData.IncreaseOutstandingBalance(1000);
			org2.MiscServ.OM_OH_ARGlobalCreditGroup = org1.PK;

			Factory.Save();

			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var tempFactory = new BusinessObjectFactory();
				var tempOrg = tempFactory.Load<OrgHeader>(org1.PK);
				tempOrg.CompanyData.OB_IsDebtor = true;
				tempOrg.CompanyData.OB_ARCreditLimit = 200m;

				var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, currencyCode));
				var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
				tempExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
				tempExchangeRate.RE_SellRate = 2m;
				tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
				tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

				tempFactory.Save();
			}

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TestOrg3";
			org3.OH_FullName = "TestOrgFullName3";
			org3.CompanyData.OB_IsDebtor = true;
			org3.CompanyData.OB_ARCreditLimit = 100m;

			Factory.Save();

			//AssertEquals("Org in Group should display all members in group", 3, org1.GlobalCreditGroupChilds.Count);		// Manuel please uncomment after your check in
			AssertEquals("Org in Group should display all members in group", 3, org2.GlobalCreditGroupChilds.Count);
			AssertEquals("Org not in Group should only display its own companies", 1, org3.GlobalCreditGroupChilds.Count);

			AssertEquals("property should be filled with outstanding balance of company", 500m, ((OrgCompanyData)org2.GlobalCreditGroupChilds.First(x => ((OrgCompanyData)x).PK == org1.CompanyData.PK)).CreditOutStandingBalance);
			AssertEquals("property should be filled with outstanding balance of company", 1000m, ((OrgCompanyData)org2.GlobalCreditGroupChilds.First(x => ((OrgCompanyData)x).PK == org2.CompanyData.PK)).CreditOutStandingBalance);
		}

		public void TestGlobalCreditGroupChildsForClaim()
		{
			var currencyCode = Env.CurrentCompany.LocalCurrency.Code;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";
			org1.OH_FullName = "TestOrgFullName1";
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.OB_ARCreditLimit = 50m;
			var arInvForOrg1 = CreateARInvoice(500m, org1, 0, GlbCompany.CurrentCompany.PK, "111");
			org1.MiscServ.OM_ARGlobalCreditApproved = true;
			org1.MiscServ.OM_RX_NKARGlobalCreditCurrency = currencyCode;
			org1.MiscServ.OM_ARGlobalCreditLimit = 500m;

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, currencyCode));
			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			exchangeRate.RE_SellRate = 1.1m;
			exchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
			exchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

			Factory.Save();

			CreateClaim(arInvForOrg1, "OPN", 100m);
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_FullName = "TestOrgFullName2";
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.OB_ARCreditLimit = 75m;
			var arInvForOrg2 = CreateARInvoice(1000M, org2, 0, GlbCompany.CurrentCompany.PK, "222");
			org2.MiscServ.OM_OH_ARGlobalCreditGroup = org1.PK;

			Factory.Save();

			CreateClaim(arInvForOrg2, "OPN", 200m);
			Factory.Save();

			using ((ObjectFactory.Get<IAccounting>().Registry.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation as BooleanRegistryItem).SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("property should be filled with outstanding balance of company", 400m, ((OrgCompanyData)org2.GlobalCreditGroupChilds.First(x => ((OrgCompanyData)x).PK == org1.CompanyData.PK)).CreditOutStandingBalance);
				AssertEquals("property should be filled with outstanding balance of company", 800m, ((OrgCompanyData)org2.GlobalCreditGroupChilds.First(x => ((OrgCompanyData)x).PK == org2.CompanyData.PK)).CreditOutStandingBalance);
			}
		}

		public void TestCountryHasStateList()
		{
			company.OH_RL_NKClosestPort = "AUSYD";
			Assert("AUSYD should have list", company.CountryHasStateList);

			company.OH_RL_NKClosestPort = "SGSIN";
			Assert("SGSIN should not have list", !company.CountryHasStateList);
		}

		public void TestIsValidABN()
		{
			company.PrimaryRegistrationNumber.Number = "21 003 980 130";
			AssertEquals("Valid ABN", true, company.IsValidABN);
			company.PrimaryRegistrationNumber.Number = "21 003 980 131";
			AssertEquals("Invalid ABN", false, company.IsValidABN);
		}

		public void TestCityFallbackHasCity()
		{
			company.MainAddress.OA_City = "City";
			AssertEquals("CityFallback", "City", company.CityFallback);
		}

		public void TestCityFallbackHasPort()
		{
			company.MainAddress.OA_City = string.Empty;
			company.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("CityFallback", "Sydney", company.CityFallback);
		}

		public void TestCityFallbackHasCityAndPort()
		{
			company.MainAddress.OA_City = "City";
			company.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("CityFallback", "City", company.CityFallback);
		}

		public void TestCityFallbackNoCityOrPort()
		{
			company = BasicCompanyForTest;
			company.MainAddress.OA_City = ZString.Empty;
			company.OH_RL_NKClosestPort = string.Empty;
			AssertEquals("CityFallback", ZString.Empty, company.CityFallback);
		}

		public void TestCanGenerateCode()
		{
			company = BasicCompanyForTest;
			company.OH_FullName = string.Empty;
			Assert("Cannot generate code yet", !company.CanGenerateCode);
			company.OH_FullName = "zz";
			Assert("Can't generate code without unloco", !company.CanGenerateCode);
			company.OH_RL_NKClosestPort = "AUSYD";
			Assert("Now has enough information to generate the code", company.CanGenerateCode);
			company.OH_FullName = string.Empty;
			Assert("Cannot generate with name missing", !company.CanGenerateCode);
		}

		public void TestOrganisationTypeIsSelected()
		{
			Assert("No organisation type selected", !company.OrganisationTypeIsSelected);

			company.OH_IsDebtor = true;
			Assert("OH_IsDebtor selected", company.OrganisationTypeIsSelected);

			company.OH_IsDebtor = false;
			company.OH_IsCreditor = true;
			Assert("OH_IsCreditor selected", company.OrganisationTypeIsSelected);

			company.OH_IsCreditor = false;
			company.OH_IsConsignee = true;
			Assert("OH_IsConsignee selected", company.OrganisationTypeIsSelected);

			company.OH_IsConsignee = false;
			company.OH_IsConsignor = true;
			Assert("OH_IsConsignor selected", company.OrganisationTypeIsSelected);

			company.OH_IsConsignor = false;
			company.OH_IsTransportClient = true;
			Assert("OH_IsTransportClient selected", company.OrganisationTypeIsSelected);

			company.OH_IsTransportClient = false;
			company.OH_IsWarehouseClient = true;
			Assert("OH_IsWarehouseClient selected", company.OrganisationTypeIsSelected);

			company.OH_IsWarehouseClient = false;
			company.OH_IsShippingProvider = true;
			Assert("OH_IsShippingProvider selected", company.OrganisationTypeIsSelected);

			company.OH_IsShippingProvider = false;
			company.OH_IsForwarder = true;
			Assert("OH_IsForwarder selected", company.OrganisationTypeIsSelected);

			company.OH_IsForwarder = false;
			company.OH_IsBroker = true;
			Assert("OH_IsBroker selected", company.OrganisationTypeIsSelected);

			company.OH_IsBroker = false;
			company.OH_IsMiscFreightServices = true;
			Assert("OH_IsMiscFreightServices selected", company.OrganisationTypeIsSelected);

			company.OH_IsMiscFreightServices = false;
			company.OH_IsCompetitor = true;
			Assert("OH_IsCompetitor selected", company.OrganisationTypeIsSelected);

			company.OH_IsCompetitor = false;
			company.OH_IsSalesLead = true;
			Assert("OH_IsSalesLead selected", company.OrganisationTypeIsSelected);

			company.OH_IsSalesLead = false;
			company.OH_IsControllingAgent = true;
			Assert("OH_IsControllingAgent selected", company.OrganisationTypeIsSelected);

			company.OH_IsControllingAgent = false;
			company.OH_IsControllingCustomer = true;
			Assert("OH_IsControllingCustomer selected", company.OrganisationTypeIsSelected);

			company.OH_IsConsignee = true;
			company.OH_IsConsignor = true;
			company.OH_IsSalesLead = true;
			Assert("More than one org type selected", company.OrganisationTypeIsSelected);
		}

		public void TestCompetitorCollectionsAreReadOnly()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			Assert("Clients must be a readonly collection", org.Clients.ReadOnly);
			Assert("Clients must not allow new records", !org.Clients.AllowNew);
		}

		public void TestPartAttributeManager()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertNotNull("Should be lazy loaded and never null", org.PartAttributeManager);
			AssertEquals("Constructed with incorrect Organisation", org.PK, org.PartAttributeManager.Organisation.PK);
		}

		public void TestIsDepot()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals("Precondition", false, org.IsDepot);

			TestIsDepotCore(org, (v) => org.OH_IsUnpackDepot = v);
			TestIsDepotCore(org, (v) => org.OH_IsPackDepot = v);
			TestIsDepotCore(org, (v) => org.OH_IsRoadFreightDepot = v);
			TestIsDepotCore(org, (v) => org.OH_IsRailHead = v);
		}

		void TestIsDepotCore(OrgHeader org, Action<bool> setProperty)
		{
			org.OH_IsMiscFreightServices = true;
			setProperty(true);
			AssertEquals(true, org.IsDepot);

			org.OH_IsMiscFreightServices = false;
			AssertEquals(false, org.IsDepot);
			setProperty(false); // clean up
		}

		public void TestIsSystemDefinedOrganisation()
		{
			ZGuid unmatchedOrgPK = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			ZGuid miscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			OrgHeader orgUnmatched = Factory.Load<OrgHeader>(unmatchedOrgPK);
			OrgHeader orgMisc = Factory.Load<OrgHeader>(miscOrgPK);
			OrgHeader orgABIGAS = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS"));

			Assert("UNMATCHED is system Defined", orgUnmatched.IsSystemDefinedOrganisation);
			Assert("MISC is system Defined", orgMisc.IsSystemDefinedOrganisation);
			Assert("ABIGAS is NOT system Defined", !orgABIGAS.IsSystemDefinedOrganisation);
			Assert("UNMATCHED is system Defined", ((ICanBeExcludedFromOperationalActions)orgUnmatched).ShouldExclude);
			Assert("MISC is system Defined", ((ICanBeExcludedFromOperationalActions)orgMisc).ShouldExclude);
			Assert("ABIGAS is NOT system Defined", !((ICanBeExcludedFromOperationalActions)orgABIGAS).ShouldExclude);
		}

		public void TestIsNMFCParticipant()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals(false, org.IsNMFCParticipant);

			org.CustomsCodes.AddNew();
			org.CustomsCodes[0].OK_CodeType = "NMD";
			org.CustomsCodes[0].OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.Yes;
			AssertEquals(false, org.IsNMFCParticipant);

			org.CustomsCodes[0].OK_CodeType = OrgCusCode.USACodeTypes.NMFCParticipant;
			AssertEquals(true, org.IsNMFCParticipant);

			org.CustomsCodes[0].OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.No;
			AssertEquals(false, org.IsNMFCParticipant);
		}

		public void TestENettRegistrationNumber()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals(string.Empty, org.ENettRegistrationNumber);

			org.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");
			AssertEquals("12345", org.ENettRegistrationNumber);
		}

		public void TestCartageTransportWebSite()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals(string.Empty, org.CartageTransportWebSite);

			org.OrgWebURLs.AddNew(OrgWebUrlList.Codes.MainWebsite).PU_URL = "www.MainWebSite.com";
			AssertEquals(string.Empty, org.CartageTransportWebSite);

			org.OrgWebURLs.AddNew(OrgWebUrlList.Codes.CartageTracking).PU_URL = "www.CartageWebSite.com";
			AssertEquals("www.CartageWebSite.com", org.CartageTransportWebSite);
		}

		public void TestARSettlementGroupPK()
		{
			company.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;

			company.ARSettlementGroupPK = ZGuid.Empty;
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", false, company.CompanyData.OB_ARUseSettlementGroupCreditLimit);

			company.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			company.ARSettlementGroupPK = orgHeader.PK;
			AssertEquals("ARSettlementGroupPK", orgHeader.PK, company.ARSettlementGroupPK);
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", true, company.CompanyData.OB_ARUseSettlementGroupCreditLimit);

			company.ARSettlementGroupPK = company.PK;
			AssertEquals("ARSettlementGroupPK", company.PK, company.ARSettlementGroupPK);
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", false, company.CompanyData.OB_ARUseSettlementGroupCreditLimit);
		}

		public void TestIsSettlementGroup()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeaderParent = Factory.NewWithValidTestData<OrgHeader>();
			OrgRelatedParty orgRelatedParty = Factory.New<OrgRelatedParty>();
			orgRelatedParty.PR_OH_Parent = orgHeaderParent.PK;
			orgRelatedParty.PR_OH_RelatedParty = orgHeader.PK;
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			orgRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			OrgHeader orgHeaderParent2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgRelatedParty orgRelatedParty2 = Factory.New<OrgRelatedParty>();
			orgRelatedParty2.PR_OH_Parent = orgHeaderParent2.PK;
			orgRelatedParty2.PR_OH_RelatedParty = orgHeader.PK;
			orgRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			orgRelatedParty2.PR_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			int dbLoadsBefore = Factory.DatabaseLoadCount;
			AssertEquals("IsSettlementGroup", false, orgHeader2.IsSettlementGroup(""));
			AssertEquals("IsSettlementGroup should load something before cache value.", 1, Factory.DatabaseLoadCount - dbLoadsBefore);

			dbLoadsBefore = Factory.DatabaseLoadCount;
			AssertEquals("IsSettlementGroup", false, orgHeader2.IsSettlementGroup(""));
			AssertEquals("IsSettlementGroup shouldn't load anything when cache is enabled", 0, Factory.DatabaseLoadCount - dbLoadsBefore);

			dbLoadsBefore = Factory.DatabaseLoadCount;
			AssertEquals("IsSettlementGroup", true, orgHeader.IsSettlementGroup(""));
			AssertEquals("IsSettlementGroup should load something before cache value for another ogranisation.", 1, Factory.DatabaseLoadCount - dbLoadsBefore);

			dbLoadsBefore = Factory.DatabaseLoadCount;
			AssertEquals("IsSettlementGroup", true, orgHeader.IsSettlementGroup(""));
			AssertEquals("IsSettlementGroup shouldn't load anything when cache is enabled.", 0, Factory.DatabaseLoadCount - dbLoadsBefore);

			dbLoadsBefore = Factory.DatabaseLoadCount;
			AssertEquals("IsSettlementGroup", true, orgHeader.IsSettlementGroup("AR"));
			AssertEquals("IsSettlementGroup should load something before cache value for another ledger.", 1, Factory.DatabaseLoadCount - dbLoadsBefore);

			dbLoadsBefore = Factory.DatabaseLoadCount;
			AssertEquals("IsSettlementGroup", true, orgHeader.IsSettlementGroup("AR"));
			AssertEquals("IsSettlementGroup shouldn't load anything when cache is enabled.", 0, Factory.DatabaseLoadCount - dbLoadsBefore);

			dbLoadsBefore = Factory.DatabaseLoadCount;
			AssertEquals("IsSettlementGroup", true, orgHeader.IsSettlementGroup("AP"));
			AssertEquals("IsSettlementGroup should load something before cache value for another ledger.", 1, Factory.DatabaseLoadCount - dbLoadsBefore);

			dbLoadsBefore = Factory.DatabaseLoadCount;
			AssertEquals("IsSettlementGroup", true, orgHeader.IsSettlementGroup("AP"));
			AssertEquals("IsSettlementGroup shouldn't load anything when cache is enabled.", 0, Factory.DatabaseLoadCount - dbLoadsBefore);
		}

		public void TestDeletedStack()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Delete stack NOT exists on deleted object", $"Delete Stack: {System.Environment.NewLine}Delete stack never collected", orgHeader.DeletedStack);

			orgHeader.Delete();
			AssertStartsWith("Delete stack exists on deleted object", $"Delete Stack: {System.Environment.NewLine}   at Enterprise.MasterFiles.Business.OrgHeader.Delete()", orgHeader.DeletedStack);
		}

		#region Invoices

		public void TestDisbursementTerms()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();

			InvoiceTermsList termList = new ARInvoiceTermsList();
			ZString expected = termList.GetDescriptionFromCode("COD").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, header.DisbursmentTerms);

			OrgARTerms disbursementTerms = header.CompanyData.CreateOrLoadDisbursementARTerm();
			disbursementTerms.PY_InvoiceTerm = "COD";
			AssertEquals("Invoice terms", expected, header.DisbursmentTerms);

			disbursementTerms.PY_InvoiceTerm = "INV";
			disbursementTerms.PY_InvoiceDays = 12;

			expected = "12 DAYS " + termList.GetDescriptionFromCode("INV").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, header.DisbursmentTerms);

			disbursementTerms.PY_InvoiceTerm = "MIC";
			disbursementTerms.PY_InvoiceDays = 2;

			expected = "2 MONTHS " + InvoiceTermsList.MonthsFromInvoiceCycleDate.Description.Trim().ToUpper();
			AssertEquals("Invoice terms", expected, header.DisbursmentTerms);

			disbursementTerms.PY_InvoiceTerm = "DPC";
			disbursementTerms.PY_InvoiceDays = 5;

			expected = "5 " + InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Description.Trim().ToUpper();
			AssertEquals("Invoice terms", expected, header.DisbursmentTerms);

			var term = header.CompanyData.ARTerms.AddNew();
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = "SHP";
				term.PY_GB_Branch = Guid.Empty;
				term.PY_GE_Department = Guid.Empty;
				term.PY_Direction = "ALL";
				term.PY_TransportMode = "AIR";
				term.PY_InvoiceClass = "DSB";
				term.PY_InvoiceTerm = "INV";
				term.PY_InvoiceDays = 25;

				expected = "COD, (SHP-AIR-DSB)->25/INV, (DSB)->5/DPC";
				AssertEquals(expected, header.DisbursmentTerms);
			}
		}

		public void TestShortDisbursementTerms()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.Factory.Save();

			CodeDescriptionPairList termList = new ARInvoiceTermsList();
			ZString expected = termList.GetDescriptionFromCode("COD").Trim().ToUpper();
			AssertEquals(expected, header.ShortDisbursementTerms);

			OrgARTerms disbursementTerms = header.CompanyData.CreateOrLoadDisbursementARTerm();
			disbursementTerms.PY_InvoiceTerm = "COD";
			AssertEquals(expected, header.ShortDisbursementTerms);

			disbursementTerms.PY_InvoiceTerm = "INV";
			disbursementTerms.PY_InvoiceDays = 12;

			expected = "12 DAYS";
			AssertEquals(expected, header.ShortDisbursementTerms);

			disbursementTerms.PY_InvoiceTerm = "MIC";
			disbursementTerms.PY_InvoiceDays = 2;

			expected = "2 MONTHS";
			AssertEquals(expected, header.ShortDisbursementTerms);

			disbursementTerms.PY_InvoiceTerm = "PIA";
			disbursementTerms.PY_InvoiceDays = 0;

			expected = "PAYMENT IN ADVANCE";
			AssertEquals(expected, header.ShortDisbursementTerms);

			var term = header.CompanyData.ARTerms.AddNew();
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = "SHP";
				term.PY_GB_Branch = Guid.Empty;
				term.PY_GE_Department = Guid.Empty;
				term.PY_Direction = "ALL";
				term.PY_TransportMode = "AIR";
				term.PY_InvoiceClass = "DSB";
				term.PY_InvoiceTerm = "INV";
				term.PY_InvoiceDays = 25;

				expected = "COD, (SHP-AIR-DSB)->25/INV, (DSB)->PIA";
				AssertEquals(expected, header.ShortDisbursementTerms);
			}
		}

		public void TestShortInvoiceTerms()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.Factory.Save();

			var termList = new ARInvoiceTermsList();
			var expected = termList.GetDescriptionFromCode("COD").Trim().ToUpper();
			AssertEquals(expected, header.ShortInvoiceTerms);

			var invoiceTerms = header.CompanyData.LoadARTermForAllInvoiceTypes();
			invoiceTerms.PY_InvoiceTerm = "COD";
			AssertEquals(expected, header.ShortInvoiceTerms);

			invoiceTerms.PY_InvoiceTerm = "INV";
			invoiceTerms.PY_InvoiceDays = 12;

			expected = "12 DAYS";
			AssertEquals("Invoice terms", expected, header.ShortInvoiceTerms);

			invoiceTerms.PY_InvoiceTerm = "MIC";
			invoiceTerms.PY_InvoiceDays = 2;

			expected = "2 MONTHS";
			AssertEquals("Invoice terms", expected, header.ShortInvoiceTerms);

			invoiceTerms.PY_InvoiceTerm = "PIA";
			invoiceTerms.PY_InvoiceDays = 0;

			expected = "PAYMENT IN ADVANCE";
			AssertEquals("Invoice terms", expected, header.ShortInvoiceTerms);

			var term = header.CompanyData.ARTerms.AddNew();
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = "SHP";
				term.PY_GB_Branch = Guid.Empty;
				term.PY_GE_Department = Guid.Empty;
				term.PY_Direction = "ALL";
				term.PY_TransportMode = "AIR";
				term.PY_InvoiceClass = "FIN";
				term.PY_InvoiceTerm = "INV";
				term.PY_InvoiceDays = 25;

				expected = "PIA, (SHP-AIR-FIN)->25/INV";
				AssertEquals(expected, header.ShortInvoiceTerms);
			}
		}

		public void TestShortInvoiceTermsTranslated()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("abda7af3-b44c-4a04-9d1b-8ddb4b8cc8bf", new ResourceStringData("abda7af3-b44c-4a04-9d1b-8ddb4b8cc8bf", "月"));
				mockChs.Put("34a5b61a-ccd5-4332-aeea-3d8209e4e85d", new ResourceStringData("34a5b61a-ccd5-4332-aeea-3d8209e4e85d", "日"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var header = Factory.NewWithValidTestData<OrgHeader>();
					header.CompanyData.Factory.Save();

					var termList = new ARInvoiceTermsList();
					var invoiceTerms = header.CompanyData.LoadARTermForAllInvoiceTypes();

					invoiceTerms.PY_InvoiceTerm = "INV";
					invoiceTerms.PY_InvoiceDays = 12;
					var expected = "12 日";
					AssertEquals("Invoice terms", expected, header.ShortInvoiceTerms);

					invoiceTerms.PY_InvoiceTerm = "MIC";
					invoiceTerms.PY_InvoiceDays = 2;

					expected = "2 月";
					AssertEquals("Invoice terms", expected, header.ShortInvoiceTerms);
				}
			}
		}

		#endregion

		#region External Validation Status

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestExternalValidationStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			AssertEquals(OrgConstants.FilterControl.ExternalValidationStatus.Description.NotRun, org.ExternalValidationStatus);

			org.Logs.AddNew(AutoEvents.ExternalValidationPassed);
			Factory.Save();
			AssertEquals(OrgConstants.FilterControl.ExternalValidationStatus.Description.Passed, org.ExternalValidationStatus);

			org.Logs.AddNew(AutoEvents.ExternalValidationFailed);
			Factory.Save();
			AssertEquals(OrgConstants.FilterControl.ExternalValidationStatus.Description.Failed, org.ExternalValidationStatus);

			org.Logs.AddNew(AutoEvents.ExternalValidationNotCompleted);
			Factory.Save();
			AssertEquals(OrgConstants.FilterControl.ExternalValidationStatus.Description.NotCompleted, org.ExternalValidationStatus);

			org.Logs.AddNew(AutoEvents.ExternalValidationPassed);
			Factory.Save();
			AssertEquals(OrgConstants.FilterControl.ExternalValidationStatus.Description.Passed, org.ExternalValidationStatus);
		}

		#endregion

		#endregion

		#region TestOH_IsTempAccount

		public void TestOH_IsTempAccount()
		{
			ZGuid currentBranch = GlbBranch.CurrentBranch.PK;
			ZGuid currentDept = GlbDepartment.CurrentDepartment.PK;

			GlbStaff staffWithoutTempAccountRights = Factory.NewWithValidTestData<GlbStaff>();
			GlbSecurity securityRecordNoRights = Factory.New<GlbSecurity>();
			securityRecordNoRights.GU_SecurityRight = Env.Security.OrgDetailsModifyIsTemporaryOrg.Code;
			securityRecordNoRights.GU_SecurityItemIsAllowed = false;
			securityRecordNoRights.GU_GS = staffWithoutTempAccountRights.PK;
			staffWithoutTempAccountRights.GroupSecurityPermissionsCollectionForBinding.Add(securityRecordNoRights);

			GlbStaff staffWithTempAccountRights = Factory.NewWithValidTestData<GlbStaff>();
			GlbSecurity securityRecordWithRights = Factory.New<GlbSecurity>();
			securityRecordWithRights.GU_SecurityRight = Env.Security.OrgDetailsModifyIsTemporaryOrg.Code;
			securityRecordWithRights.GU_SecurityItemIsAllowed = true;
			securityRecordWithRights.GU_GS = staffWithTempAccountRights.PK;
			staffWithTempAccountRights.GroupSecurityPermissionsCollectionForBinding.Add(securityRecordWithRights);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staffWithoutTempAccountRights.GS_LoginName, currentBranch.ToGuid(), currentDept.ToGuid()))
			{
				Assert("User logged in without Temp Account rights, Temp Account should be readonly", OrgInDB.OH_IsTempAccountInfo.ReadOnly);
			}
			using (Env.SetTemporaryUserContext(staffWithTempAccountRights.GS_LoginName, currentBranch.ToGuid(), currentDept.ToGuid()))
			{
				ResetOrgInDB();
				Assert("User logged in with Temp Account rights, Temp Account should NOT be readonly", !OrgInDB.OH_IsTempAccountInfo.ReadOnly);
			}
		}

		#endregion

		#region Organisation Names

		public void TestOrganisationNames()
		{
			Assert("OrganisationNames should contain the main org name", company.AllOrganisationNames().Contains(new StringWithLanguage(company.OH_FullName.ToString(), company.OH_Language.ToString())));

			Assert("MainAddress has a CompanyNameOverride set", !company.MainAddress.OA_CompanyNameOverride.IsEmpty);
			Assert("OrganisationNames should contain the MainAddress name", company.AllOrganisationNames().Contains(new StringWithLanguage(company.MainAddress.OA_CompanyNameOverride, company.OH_Language)));

			company.MainAddress.OA_CompanyNameOverride = string.Empty;
			Assert("OrganisationNames should NOT contain the MainAddress name", !company.AllOrganisationNames().Contains(new StringWithLanguage(company.MainAddress.OA_CompanyNameOverride, company.OH_Language)));

			OrgAddress testAddress1 = company.Addresses.AddNew();
			testAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
			testAddress1.OA_CompanyNameOverride = "Some name";
			Assert("OrganisationNames should contain TestAddress1", company.AllOrganisationNames().Contains(new StringWithLanguage(testAddress1.OA_CompanyNameOverride, testAddress1.OA_Language)));

			OrgAddress testAddress2 = company.Addresses.AddNew();
			testAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
			testAddress2.OA_CompanyNameOverride = string.Empty;
			Assert("OrganisationNames should NOT contain TestAddress2", !company.AllOrganisationNames().Contains(new StringWithLanguage(testAddress2.OA_CompanyNameOverride, testAddress2.OA_Language)));

			AssertEquals("OrganisationNames should only contain 2 addresses", 2, company.AllOrganisationNames().Count());
		}

		public void TestOH_FullNameTruncated()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the";
			AssertEquals("OH_FullNameTruncatedLength is 50 characters", 50, OrgHeader.Schema.OH_FullNameTruncatedLength);
			Assert("Precondition: OH_FullName can be set for more than 50 characters upto 100 characters", org.OH_FullName.Length > OrgHeader.Schema.OH_FullNameTruncatedLength);
			AssertEquals("OH_FullNameTruncated should not exceed 50 characters though", OrgHeader.Schema.OH_FullNameTruncatedLength, org.OH_FullNameTruncated.Length);
			AssertEquals(org.OH_FullName.Substring(0, OrgHeader.Schema.OH_FullNameTruncatedLength), org.OH_FullNameTruncated);
		}

		#endregion

		#region Branch/CompanyData AutoCreate

		public void TestBranchAndCompanyDataAutoCreation()
		{
			OrgHeaderForTest orgWithCompanyData = Factory.New<OrgHeaderForTest>();
			orgWithCompanyData.CompanyData.OB_IsDebtor = ZBool.True;
			OrgHeaderForTest orgWithoutCompanyData = Factory.New<OrgHeaderForTest>();

			AssertEquals(orgWithCompanyData.CompanyData.ControllingBranch, orgWithCompanyData.Branch);
			AssertNull(orgWithoutCompanyData.Branch);
		}

		#endregion

		#region PreValidateMainAddressForRegistry

		public void TestPreValidateMainAddressForRegistry()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Mr K. RULEZ!";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_IsConsignee = true;

			org.MainAddress.OA_Email = string.Empty;
			org.MainAddress.OA_Address2 = string.Empty;
			org.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			org.MainAddress.OA_City = string.Empty;
			org.MainAddress.OA_Phone = string.Empty;
			org.PrimaryRegistrationNumber.Number = string.Empty;
			org.MainAddress.OA_Fax = string.Empty;
			org.MainWebURL.PU_URL = string.Empty;

			var orgRequiredFieldsAll = new OrgRequiredFields(true, true, true, true, true, false, true, true, true, false, false);
			Env.Registry.SetOrgConsigneeRequiredFields(orgRequiredFieldsAll);
			var memberInfo = typeof(OrgHeader).GetField("requiredFieldsForOrgCache", BindingFlags.NonPublic | BindingFlags.Instance);
			memberInfo?.SetValue(org, null);

			org.PreValidateMainAddressForRegistry();

			AssertHasErrors("Email is required", org.MainAddress.OA_EmailInfo);
			AssertHasErrors("Address2 is required", org.MainAddress.OA_Address2Info);
			AssertHasErrors("Branch is required", org.CompanyData.OB_GB_ControllingBranchInfo);
			AssertHasErrors("City is required", org.MainAddress.OA_CityInfo);
			AssertHasErrors("Phone is required", org.MainAddress.OA_Phone_FormattedInfo);
			AssertHasErrors("BusinessNumber is required", org.PrimaryRegistrationNumber.NumberInfo);
			AssertHasErrors("Fax is required", org.MainAddress.OA_Fax_FormattedInfo);
			AssertHasErrors("WebUrl is required", org.MainWebURL.PU_URLInfo);

			DataRegistry.Instance.EnableAddressValidationWebService = true;
			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			org.MainAddress.OA_RN_NKCountryCode = "AU";
			org.MainAddress.OA_State = "";
			org.MainAddress.OA_PostCode = "";

			org.MainAddress.ValidationStatus = AddressValidationStatus.Verified;
			org.PreValidateMainAddressForRegistry();

			AssertEquals("There should be no errors or warnings.", false, org.MainAddress.OA_StateInfo.HasErrors() || org.MainAddress.OA_StateInfo.HasWarnings() || org.MainAddress.OA_PostCodeInfo.HasErrors() || org.MainAddress.OA_PostCodeInfo.HasWarnings());
			AssertNoError(org.MainAddress.OA_StateInfo, "You must enter a state. The state validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");
			AssertNoError(org.MainAddress.OA_PostCodeInfo, "You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");

			org.MainAddress.ValidationStatus = AddressValidationStatus.ToBeVerified;
			org.PreValidateMainAddressForRegistry();

			AssertHasError(org.MainAddress.OA_StateInfo, "You must enter a state. The state validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");
			AssertHasError(org.MainAddress.OA_PostCodeInfo, "You must enter a postcode. The postcode validation rule for the country/region Australia is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.");
		}

		#endregion

		#region Brands Or Related Names

		public void TestBrandsOrRelatedNames()
		{
			OrgBrandOrRelatedName testBrand1 = company.BrandsOrRelatedNames.AddNew();
			testBrand1.P1_RelatedName = "Brand1";
			OrgBrandOrRelatedName testBrand2 = company.BrandsOrRelatedNames.AddNew();
			testBrand2.P1_RelatedName = "Brand2";
			OrgBrandOrRelatedName testBrand3 = company.BrandsOrRelatedNames.AddNew();
			testBrand3.P1_RelatedName = "Brand3";
			Factory.Save();

			AssertEquals("BrandsOrRelatedNamesCollection should have 3 brands listed", 3, company.BrandsOrRelatedNames.Count);
			Assert("BrandsOrRelatedNamesCollection should contain Brand1", company.BrandsOrRelatedNames.Contains(testBrand1.PK));
			Assert("BrandsOrRelatedNamesCollection should contain Brand2", company.BrandsOrRelatedNames.Contains(testBrand2.PK));
			Assert("BrandsOrRelatedNamesCollection should contain Brand2", company.BrandsOrRelatedNames.Contains(testBrand3.PK));
		}

		#endregion

		#region Pattern Matching

		public void TestMultiplePatternMatchesForThisOrg()
		{
			company.OH_IsSalesLead = true;
			Factory.Save();

			int patternMatchCount = Factory.GetDatabaseCount(typeof(OrgPatternMatch), new ZQuery(OrgPatternMatchSchema.OS_OH, company.PK));
			AssertEquals("Pattern Match Count should be 2", 2, patternMatchCount);
			AssertEquals("PatternMatchesForThisOrg is same as loaded", patternMatchCount, company.PatternMatchesForThisOrg.Count);

			OrgAddress address2 = company.Addresses.AddNew();
			address2.OA_Address1 = "Address";
			address2.OA_City = "hoolahoop";
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			OrgBrandOrRelatedName brand = company.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "Company Name 2";

			company.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(company);
			Factory.Save();

			patternMatchCount = Factory.GetDatabaseCount(typeof(OrgPatternMatch), new ZQuery(OrgPatternMatchSchema.OS_OH, company.PK));
			AssertEquals("Pattern Match Count", 5, patternMatchCount);
			AssertEquals("PatternMatchesForThisOrg is same as loaded", patternMatchCount, company.PatternMatchesForThisOrg.Count);
		}

		#endregion

		#region Security

		public void TestSecurityProvider()
		{
			OrganisationSecurityProvider provider = company.SecurityProvider;
			AssertNotNull(provider);
			AssertEquals("The security provider should be cached", provider, company.SecurityProvider);
		}

		#endregion

		#region IJobNumber

		public void TestJobNumber()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "Code";
			AssertEquals("Code", ((IJobNumber)organisation).JobNumber);
		}

		#endregion

		#region IContactable

		public void TestIContactable()
		{
			OrgHeader company = Factory.New<OrgHeader>();
			company.OH_FullName = "name";
			company.MainAddress.OA_Email = "email@example.com";
			company.MainAddress.OA_Mobile = "1234567";
			company.MainAddress.OA_IsActive = false;
			AssertEquals("name", ((IContactable)company).Name);
			AssertEquals("email@example.com", ((IContactable)company).Email);
			AssertEquals("1234567", ((IContactable)company).Mobile);
			AssertEquals(true, ((IContactable)company).IsActive); //activeness of the main address doesn't influence this

			OrgContact newContact1 = company.Contacts.AddNew();
			newContact1.OC_ContactName = "nested_name1";
			OrgContact newContact2 = company.Contacts.AddNew();
			newContact2.OC_ContactName = "nested_name2";
			AssertEquals(company.MainAddress.OA_IsActive, false);
			AssertEquals(newContact1.OC_IsActive, true);
			AssertEquals(newContact2.OC_IsActive, true);
			AssertEquals("Main Contact", ((IContactable)company).GetNestedContacts(string.Empty)[0].Name);
			AssertEquals("email@example.com", ((IContactable)company).GetNestedContacts(string.Empty)[0].Email);
			AssertEquals(false, ((IContactable)company).GetNestedContacts(string.Empty)[0].IsActive);
			AssertEquals("nested_name1", ((IContactable)company).GetNestedContacts(string.Empty)[1].Name);
			AssertEquals(true, ((IContactable)company).GetNestedContacts(string.Empty)[1].IsActive);
			AssertEquals("nested_name2", ((IContactable)company).GetNestedContacts(string.Empty)[2].Name);
			AssertEquals(true, ((IContactable)company).GetNestedContacts(string.Empty)[2].IsActive);
		}

		#endregion

		#region ISendEmailSource

		public void TestGetAddressBookSelection()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "staff1";
			staff1.GS_EmailAddress = "staff1@mail.ru";
			staff1.GS_MobilePhone = "1234567";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "staff2";
			staff2.GS_EmailAddress = "staff2@mail.ru";
			staff2.GS_MobilePhone = "7654321";
			OrgHeader header = Factory.New<OrgHeader>();

			OrgContact newContact1 = header.Contacts.AddNew();
			newContact1.OC_ContactName = "nested_name1";
			OrgContact newContact2 = header.Contacts.AddNew();
			newContact2.OC_ContactName = "nested_name2";
			OrgStaffAssignments assignment1 = header.StaffAssignments.AddNew();
			assignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			OrgStaffAssignments assignment2 = header.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			AssertEquals("There are 2 StaffAssignments in Assignments List", 2, header.StaffAssignments.Count);
			AssertEquals("There are 2 Contacts in Contacts List", 2, header.Contacts.Count);

			AddressBookSelection selection = ((ISendEmailSource)header).GetAddressBookSelection();
			AssertEquals(4, selection.Recipients.Count);
			Assert(selection.Recipients.Contains(staff1));
			Assert(selection.Recipients.Contains(staff2));
			Assert(selection.Recipients.Contains(newContact1));
			Assert(selection.Recipients.Contains(newContact2));
		}

		#endregion ISendEmailSource

		#region IDocManagerSupport

		public void TestDocManagerCode()
		{
			AssertEquals("Code should be Company. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "ORG", ((IDocManagerSupport)company).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Load

		public void TestLoadFromForeignCode()
		{
			// Add Organisation 
			OrgHeader company = OrgHeader.New(Factory);
			company.OH_Code = "TESTORG";

			// Add Organisation for mapping
			OrgHeader localCompany = OrgHeader.New(Factory);
			localCompany.OH_Code = "EYLKNP";

			OrgPatternMatchOverride orgPatternMatch = company.CreatePatternMatchOverrideForTest();
			orgPatternMatch.OO_ForeignCode = "FOREIGNCODE";
			orgPatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPatternMatch.OO_LocalGuid = localCompany.PK;

			AssertEquals("EYLKNP", OrgHeader.LoadFromForeignCode(Factory, "FOREIGNCODE", company).OH_Code);
		}

		#endregion

		#region FindByOrgCusCode

		public void TestFindByOrgCusCode()
		{
			CombineAssertions("Test Using CurrentCompany Country", () =>
			{
				OrgHeader testOrg1 = Factory.New<OrgHeader>();
				OrgCusCode testCusCode1 = Factory.New<OrgCusCode>();
				testCusCode1.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
				testCusCode1.OK_CustomsRegNo = "101";
				testCusCode1.OK_OH = testOrg1.PK;

				OrgHeader testOrg2 = Factory.New<OrgHeader>();
				OrgCusCode testCusCode2 = Factory.New<OrgCusCode>();
				testCusCode2.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
				testCusCode2.OK_CustomsRegNo = "102";
				testCusCode2.OK_OH = testOrg2.PK;

				AssertNull("Should not find OrgHeader, wrong ExternalCodeType", OrgHeader.FindByOrgCusCode(Factory, "BLAH", "101"));
				AssertEquals(testOrg1.PK, OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.GSTCode, "101").PK);
				AssertEquals(testOrg2.PK, OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "102").PK);
				AssertNull("Should not find OrgHeader, wrong ExternalCode", OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "101"));
			});
		}

		public void TestFindByOrgCusCode_WithGivenCountry()
		{
			CombineAssertions("Test Using Given Country", () =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					OrgHeader testOrg1 = Factory.New<OrgHeader>();
					OrgCusCode testCusCode1 = Factory.New<OrgCusCode>();
					testCusCode1.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
					testCusCode1.OK_CustomsRegNo = "101";
					testCusCode1.OK_OH = testOrg1.PK;

					OrgCusCode testCusCode12 = Factory.New<OrgCusCode>();
					testCusCode12.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
					testCusCode12.OK_CustomsRegNo = "102";
					testCusCode12.OK_OH = testOrg1.PK;
					testCusCode12.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;

					OrgHeader testOrg2 = Factory.New<OrgHeader>();
					OrgCusCode testCusCode2 = Factory.New<OrgCusCode>();
					testCusCode2.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
					testCusCode2.OK_CustomsRegNo = "102";
					testCusCode2.OK_OH = testOrg2.PK;

					OrgCusCode testCusCode22 = Factory.New<OrgCusCode>();
					testCusCode22.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
					testCusCode22.OK_CustomsRegNo = "101";
					testCusCode22.OK_OH = testOrg2.PK;
					testCusCode22.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;

					AssertNull("Should not find OrgHeader, wrong ExternalCodeType_1", OrgHeader.FindByOrgCusCode(Factory, "BLAH", "101"));
					AssertNull("Should not find OrgHeader, wrong ExternalCodeType_2", OrgHeader.FindByOrgCusCode(Factory, "BLAH", "101", "AU"));
					AssertNull("Should not find OrgHeader, wrong ExternalCodeType_3", OrgHeader.FindByOrgCusCode(Factory, "BLAH", "101", "ZA"));
					AssertEquals("GST_1", testOrg1.PK, OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.GSTCode, "101").PK);
					AssertEquals("GST_2", testOrg1.PK, OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.GSTCode, "101", "AU").PK);
					AssertEquals("GST_3", testOrg2.PK, OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.GSTCode, "101", "ZA").PK);
					AssertNull("Should not find OrgHeader, wrong Country_1", OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.GSTCode, "101", "CA"));
					AssertEquals("CID_1", testOrg2.PK, OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "102").PK);
					AssertEquals("CID_2", testOrg2.PK, OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "102", "AU").PK);
					AssertEquals("CID_3", testOrg1.PK, OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "102", "ZA").PK);
					AssertNull("Should not find OrgHeader, wrong Country_2", OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "102", "CA"));
					AssertNull("Should not find OrgHeader, wrong ExternalCode_1", OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "101"));
					AssertNull("Should not find OrgHeader, wrong ExternalCode_2", OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "101", "AU"));
					AssertNull("Should not find OrgHeader, wrong ExternalCode_3", OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.CustomsClientID, "101", "ZA"));
				}
			});
		}

		#endregion

		#region FindBy3CharAirlineCode

		public void TestFindBy3CharAirlineCode()
		{
			AssertNull("Pre-condition", OrgHeader.FindBy3CharAirlineCode(Factory, "000"));

			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "000").PK;

			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "999").PK;

			Factory.Save();

			AssertEquals(orgHeader1.PK, OrgHeader.FindBy3CharAirlineCode(Factory, "000").PK);
			AssertEquals(orgHeader2.PK, OrgHeader.FindBy3CharAirlineCode(Factory, "999").PK);
		}

		#endregion

		#region Clone

		public void TestClone()
		{
			OrgHeader company = Factory.New<OrgHeader>();
			company.OH_FullName = "Test Org";
			company.OH_RL_NKClosestPort = "AUSYD";
			company.OH_IsConsignee = true;

			OrgHeader clonedOrg = (OrgHeader)company.Clone();
			foreach (ZPropertyInfo property in company.ZPropertyInfoHash)
			{
				AssertEquals("Property " + property.Name, company[property.Name], clonedOrg[property.Name]);
			}
		}

		#endregion

		#region Save and Delete

		public override void TestSaveAndDeleteBusinessObject()
		{
			base.TestSaveAndDeleteBusinessObject();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			OrgHeader org = CreateOrgToTestDelete(testFactory);
			org.MainAddress.OA_Address1 = "Test Address";
			AddCusCodeForDifferentBranchInOrg(testFactory, org.PK);

			org.OH_IsConsignor = true;

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Sam";
			contact.OC_Email = "sam@test.com";

			SalesEnquiry enquiry = testFactory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			enquiry.O1_OC_LinkedContact = contact.PK;
			AssertEquals("enquiry linked to org", enquiry.O1_OH_ConvertedToQualifiedLead, org.PK);

			testFactory.Save();

			BusinessObjectFactory savedFactory = new BusinessObjectFactory();
			OrgHeader savedOrg = savedFactory.Load<OrgHeader>(org.PK);
			AssertNotNull("Org is in DB", savedOrg);

			ZQuery filter = new ZQuery(OrgCusCodeSchema.OK_OH, org.PK);
			BusinessObject[] cusCodeCollection = savedFactory.Load(typeof(OrgCusCode), filter);
			AssertEquals("CusCodeCollection.Count", 1, cusCodeCollection.Length);

			savedOrg.Delete();
			savedFactory.Save();

			BusinessObjectFactory deleteFactory = new BusinessObjectFactory();
			OrgHeader deleteOrg = deleteFactory.Load<OrgHeader>(org.PK);
			AssertNull("Org not in DB", deleteOrg);

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			var loadedEnquiry = anotherFactory.Load<SalesEnquiry>(enquiry.PK);
			AssertNull("enquiry should be deleted", loadedEnquiry);
		}

		[ExpectNoExceptions]
		public void TestDeleteVsSecurityCreation()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			OrgHeader org = CreateOrgToTestDelete(testFactory);
			org.MainAddress.OA_Address1 = "Test Address";
			AddCusCodeForDifferentBranchInOrg(testFactory, org.PK);

			org.OH_IsConsignor = true;

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Sam";
			contact.OC_Email = "sam@test.com";

			SalesEnquiry enquiry = testFactory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			enquiry.O1_OC_LinkedContact = contact.PK;
			AssertEquals("enquiry linked to org", enquiry.O1_OH_ConvertedToQualifiedLead, org.PK);
			testFactory.Save();

			BusinessObjectFactory savedFactory = new BusinessObjectFactory();
			savedFactory.SuspendValidation();
			RefDocType malformedDocType = savedFactory.NewWithValidTestData<RefDocType>();
			malformedDocType.RT_ReferenceType = "SCL";
			malformedDocType.RT_DocType = "AAA";

			OrgHeader savedOrg = savedFactory.Load<OrgHeader>(org.PK);
			AssertNotNull("Org is in DB", savedOrg);

			ZQuery filter = new ZQuery(OrgCusCodeSchema.OK_OH, org.PK);
			BusinessObject[] cusCodeCollection = savedFactory.Load(typeof(OrgCusCode), filter);
			AssertEquals("CusCodeCollection.Count", 1, cusCodeCollection.Length);

			savedOrg.Delete();
			savedFactory.Save();
		}

		public void TestRegenCodeForExistingInNonUserInteractiveModeWithBadClosestPort()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Stormcloaks";
			org1.OH_RL_NKClosestPort = "JPMI3";
			org1.OH_Code = "STORMCMI3";
			Factory.Save();

			org1 = Factory.Load<OrgHeader>(org1.PK); // Load() is called here because Save() can re-generate the code
			AssertEquals("STORMCMI3", org1.OH_Code);

			Env.Registry.CanUserEditOrganisationCode = false;
			Globals.IsUserInteractive = false;

			org1.OH_RL_NKClosestPort = "JP";
			Factory.Save();

			org1 = Factory.Load<OrgHeader>(org1.PK); // Load() is called here because Save() can re-generate the code
			AssertEquals("Code should be unchanged", "STORMCMI3", org1.OH_Code);
		}

		void AddCusCodeForDifferentBranchInOrg(BusinessObjectFactory factory, ZGuid orgPK)
		{
			OrgCusCode cusCode = factory.New<OrgCusCode>();
			cusCode.OK_OH = orgPK;
			cusCode.OK_CodeType = "MID";
			cusCode.OK_CustomsRegNo = "Reg No";
			RefUNLOCO unloco = GetUNLOCO("GBLON");
			cusCode.OK_RN_NKCodeCountry = unloco.RL_RN_NKCountryCode;
		}

		#endregion

		#region Read Only Factory

		public void TestReadOnlyFactory()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			BusinessObjectFactory readOnlyFactory = header.ReadOnlyFactory;
			AssertEquals("OrgHeader ReadOnly Factory should not change", readOnlyFactory, header.ReadOnlyFactory);
		}

		#endregion

		#region Address Management

		public void TestAddress_AddressNotOnFile()
		{
			company.MainAddress.OA_City = string.Empty;
			company.MainAddress.OA_State = string.Empty;
			company.MainAddress.OA_CompanyNameOverride = string.Empty;
			company.MainAddress.OA_PostCode = string.Empty;

			ZAddressList addressList = company.Address_List;

			AssertEquals(1, addressList.Count);
			AssertEquals("AddressDescription should be set to OA_Address1", "This should have been updated (OFC)", addressList.List[0].Description);

			company.MainAddress.OA_Address1 = string.Empty;
			addressList = company.Address_List;

			AssertEquals(1, addressList.Count);
			AssertEquals("AddressDescription should be set to OA_Address1", OrgAddress.AddressNotOnFile + " (" + OrgAddressType.Office.Code + ")", addressList.List[0].Description);
		}

		public void TestAddress_ActiveOnly()
		{
			company.MainAddress.OA_City = "abcd";
			company.MainAddress.OA_State = "nsw";
			company.MainAddress.OA_PostCode = "1234";
			OrgAddress adr1 = company.Addresses.AddNew();
			adr1.FillWithValidTestData();
			OrgAddressCapability cap = Factory.New<OrgAddressCapability>();
			cap.PZ_AddressType = OrgAddressType.Delivery.Code;
			cap.PZ_OA = adr1.PK;
			OrgAddress adr2 = company.Addresses.AddNew();
			adr2.FillWithValidTestData();
			adr2.OA_IsActive = ZBool.False;

			ZAddressList addressList = company.Address_List;
			AssertEquals("should conatin only active", 2, addressList.Count);

			bool containsInactive = false;
			foreach (ZAddressItem address in addressList)
			{
				if (address.PK == adr2.PK)
				{
					containsInactive = true;
					break;
				}
			}

			Assert("should not contain inactive", !containsInactive);
		}

		public void TestAddress_AddressCapabilities()
		{
			company.MainAddress.OA_City = "abcd";
			company.MainAddress.OA_State = "nsw";
			company.MainAddress.OA_PostCode = "1234";
			company.MainAddress.AddAddressType(OrgAddressType.Delivery);

			AssertEquals("Single address only - even though 2 capabilities", 1, company.Address_List.Count);
			AssertEquals(2, ((ZAddressItem)company.Address_List.List[0]).Capabilities.Length);
			AssertEquals("Test Organisation Different Name abcd nsw 1234 AUSYD (DLV, OFC)", ((ZAddressItem)company.Address_List.List[0]).AddressDescription);
		}

		#endregion

		#region AddressChanged event

		public void TestAddressChangedUnsubscribeFromSameElement()
		{
			var mainWebURL = company.MainWebURL;
			AssertNotNull("Precondition", company.MainWebURL);

			var nonMainWebURL = company.OrgWebURLs.AddNew();
			AssertNotEquals("Should be different URL", nonMainWebURL, mainWebURL);

			((IOrgHeader)company).AddressChanged += AddressChangedHandler;

			addressChangedFired = false;
			nonMainWebURL.PU_URL = "xyz";
			Assert("Should not fire on non-subcribed element", !addressChangedFired);

			addressChangedFired = false;
			mainWebURL.PU_URL = "xyz";
			Assert("Should subscribe to main URL", addressChangedFired);

			mainWebURL.PU_IsPrimary = false;
			nonMainWebURL.PU_IsPrimary = true;
			Factory.InvalidateCachedProperties();

			AssertNotEquals(mainWebURL, company.MainWebURL);
			AssertEquals("Should refresh reference to MainWebURL", nonMainWebURL, company.MainWebURL);

			addressChangedFired = false;
			nonMainWebURL.PU_URL = "abc";
			company.MainWebURL.PU_URL = "abc";
			Assert("Should not fire on new main URL as it was not subscribed", !addressChangedFired);

			addressChangedFired = false;
			mainWebURL.PU_URL = "abc";
			Assert("Still should fire on old main URL", addressChangedFired);

			((IOrgHeader)company).AddressChanged -= AddressChangedHandler;

			addressChangedFired = false;
			mainWebURL.PU_URL = "123";
			Assert("Unsubscribe from initiali subscribed element", !addressChangedFired);
		}

		void AddressChangedHandler(object sender, EventArgs e)
		{
			addressChangedFired = true;
		}

		bool addressChangedFired;

		#endregion

		#region Tax Registration

		public void TestTaxRegistrationNumberIncludeCountryCodePrefixOnce()
		{
			ZString originalCurrentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			RefCountry nl = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Netherlands);
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = nl.Code;

				OrgHeader org1 = Factory.New<OrgHeader>();
				AddCustomsCodeForTest(org1, nl.Code, "123456789");
				AssertEquals("Tax Registration #", "NL123456789", org1.TaxRegistrationNumber);

				OrgHeader org2 = Factory.New<OrgHeader>();
				AddCustomsCodeForTest(org2, nl.Code, "NL333444");
				AssertEquals("Tax Registration #", "NL333444", org2.TaxRegistrationNumber);

				OrgHeader org3 = Factory.New<OrgHeader>();
				AddCustomsCodeForTest(org3, nl.Code, "nl333444");
				AssertEquals("Tax Registration #", "nl333444", org3.TaxRegistrationNumber);

				OrgHeader org4 = Factory.New<OrgHeader>();
				AddCustomsCodeForTest(org4, nl.Code, "nL333444");
				AssertEquals("Tax Registration #", "nL333444", org4.TaxRegistrationNumber);

				OrgHeader org5 = Factory.New<OrgHeader>();
				AddCustomsCodeForTest(org5, nl.Code, "Nkl333444");
				AssertEquals("Tax Registration #", "NLNkl333444", org5.TaxRegistrationNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCurrentCountry;
			}
		}

		public void TestCountryOfTaxRegistration_AU()
		{
			ZString originalCurrentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			RefCountry au = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			RefCountry fr = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry uk = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);

			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = uk.Code;

				AccChargeCode chargeCode = Factory.New<AccChargeCode>();
				OrgHeader organisation = Factory.New<OrgHeader>();
				organisation.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, fr.Code)).Code;

				AddCustomsCodeForTest(organisation, au.Code, "123456789");
				AssertEquals("Country Code", uk.Code, organisation.CountryOfTaxRegistration.Code);
				AssertEquals("Tax Registration #", "GB", organisation.TaxRegistrationNumber);
				AssertEquals("Raw Tax Registration #", string.Empty, organisation.RawTaxRegistrationNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCurrentCountry;
			}
		}

		public void TestCountryOfTaxRegistration_FR()
		{
			ZString originalCurrentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			RefCountry au = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			RefCountry fr = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry uk = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);

			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = uk.Code;

				AccChargeCode chargeCode = Factory.New<AccChargeCode>();
				OrgHeader organisation = Factory.New<OrgHeader>();
				organisation.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, fr.Code)).Code;

				AddCustomsCodeForTest(organisation, au.Code, "123456789");
				AddCustomsCodeForTest(organisation, fr.Code, "123456789");
				AssertEquals("Country Code", fr.Code, organisation.CountryOfTaxRegistration.Code);
				AssertEquals("Tax Registration #", "FR123456789", organisation.TaxRegistrationNumber);
				AssertEquals("Raw Tax Registration #", "123456789", organisation.RawTaxRegistrationNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCurrentCountry;
			}
		}

		public void TestCountryOfTaxRegistration_UK()
		{
			ZString originalCurrentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			RefCountry au = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			RefCountry fr = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry uk = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);

			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = uk.Code;

				AccChargeCode chargeCode = Factory.New<AccChargeCode>();
				OrgHeader organisation = Factory.New<OrgHeader>();
				organisation.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, fr.Code)).Code;

				AddCustomsCodeForTest(organisation, au.Code, "123456789");
				AddCustomsCodeForTest(organisation, fr.Code, "123456789");
				AddCustomsCodeForTest(organisation, uk.Code, "123456789");
				AssertEquals("Country Code", uk.Code, organisation.CountryOfTaxRegistration.Code);
				AssertEquals("Tax Registration #", "GB123456789", organisation.TaxRegistrationNumber);
				AssertEquals("Raw Tax Registration #", "123456789", organisation.RawTaxRegistrationNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCurrentCountry;
			}
		}

		public void TestCountryOfTaxRegistration_IncludeOrgCusCode()
		{
			AssertCountryOfTaxRegistration_IncludeOrgCusCode(true, true);
		}

		public void TestCountryOfTaxRegistration_ExcludeOrgCusCode()
		{
			AssertCountryOfTaxRegistration_IncludeOrgCusCode(false, false);
		}

		void AssertCountryOfTaxRegistration_IncludeOrgCusCode(bool includeOrgCusCodeForTaxRegsistration, bool expectedOrgCusCodeIsPresent)
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockOrgCusCodePredicateProvider = new Mock<IOrgCusCodePredicateProvider>();

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			{
				mockICountryComplianceFactory.Setup(x => x.GetIOrgCusCodePredicateProvider(It.IsAny<ZString>())).Returns(mockOrgCusCodePredicateProvider.Object);
				mockOrgCusCodePredicateProvider.Setup(x => x.IncludeForOrgHeaderTaxRegsistration(It.IsAny<OrgCusCode>())).Returns(includeOrgCusCodeForTaxRegsistration);

				var organisation = Factory.New<OrgHeader>();
				organisation.OH_RL_NKClosestPort = "INPLS";
				var address = organisation.Addresses.AddNewMainAddress();
				address.OA_RN_NKCountryCode = "IN";
				address.State = "AP";
				address.OA_RL_NKRelatedPortCode = "INPLS";
				var vatCode = organisation.CustomsCodes.AddNew();
				vatCode.OK_RN_NKCodeCountry = "AU";
				vatCode.OK_CodeType = "ABN";
				vatCode.OK_CustomsRegNo = "123456";
				organisation.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Country Code", "AU", organisation.CountryOfTaxRegistration.Code);
				AssertEquals("Raw Tax Registration #", expectedOrgCusCodeIsPresent ? "123456" : string.Empty, organisation.RawTaxRegistrationNumber);
			}
		}

		public void TestGetCodeForTaxRegistrationInOrgCountryForBrexit()
		{
			var transaction = Factory.New<AccTransactionHeader>();
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "GBLON";
			AssertEquals("Preconditions: organisation.Country", Constants.CountryCodes.UnitedKingdom, organisation.Country.RN_Code);

			AddCustomsCodeForTest(organisation, Constants.CountryCodes.UnitedKingdom, "1234");

			var gbCountry = RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			gbCountry.Factory.Save();
			AssertEquals("Before Brexit", "GB1234", organisation.GetCodeForTaxRegistrationInOrgCountry(address: null));

			gbCountry.RN_EconomicGrouping = "";
			gbCountry.Factory.Save();
			AssertEquals("After Brexit", "GB1234", organisation.GetCodeForTaxRegistrationInOrgCountry(address: null));
		}

		public void TestGetCodeForTaxRegistrationInOrgCountry()
		{
			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("CNAAT", true, "VAT", "GBR", "GCR");

			#region test SpecificCountry

			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("BR6MO", true, "CMT", "CJN", "GBR", "GCR");

			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("ARABA", true, "IVA", "CUI", "GBR", "GCR");

			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("IN5PA", true, "SER", "PAN", "GBR", "GCR");

			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("CLA2T", true, "IVA", "RUT", "GBR", "GCR");

			#endregion
		}

		public void TestGetCountryCodeAndTaxRegistrationWithoutPrefix()
		{
			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("CNAAT", false, "VAT", "GBR", "GCR");

			#region test SpecificCountry

			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("BR6MO", false, "CMT", "CJN", "GBR", "GCR");

			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("ARABA", false, "IVA", "CUI", "GBR", "GCR");

			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("IN5PA", false, "SER", "PAN", "GBR", "GCR");

			GetCodeForTaxRegistrationInOrgCountryForFallBackTest("CLA2T", false, "IVA", "RUT", "GBR", "GCR");

			#endregion
		}

		void GetCodeForTaxRegistrationInOrgCountryForFallBackTest(ZString portInCountry, bool usePrefix = true, params ZString[] cusCodes)
		{
			var transaction = Factory.New<AccTransactionHeader>();
			var organisation = Factory.New<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			transaction.AH_OA_InvoiceAddressOverride = address.PK;
			var countryCode = portInCountry.Left(2);

			for (var i = cusCodes.Length - 1; i >= 0; i--)
			{
				var expectedRegNumber = $"111_{countryCode}_{cusCodes[i]}";
				var code = cusCodes[i];

				AddCustomsCodeForTest(organisation, "GB", expectedRegNumber, code);
				AddCustomsCodeForTest(organisation, countryCode, expectedRegNumber, code);

				transaction.InvoiceAddressOverride.OA_RN_NKCountryCode = countryCode;
				organisation.OH_RL_NKClosestPort = "GBLON";
				organisation.OH_IsGlobalAccount = true;
				if (usePrefix)
				{
					AssertGetCodeForTaxRegistrationInOrgCountry($"Global Org should take Country from Address. Country Code:{countryCode}, Code:{code}.", organisation, transaction.InvoiceAddressOverride, expectedRegNumber);
				}
				else
				{
					AssertGetCountryCodeAndTaxRegistrationWithoutPrefix($"Global Org should take Country from Address. Country Code:{countryCode}, Code:{code}.", organisation, transaction.InvoiceAddressOverride, countryCode, expectedRegNumber);
				}

				organisation.OH_RL_NKClosestPort = portInCountry;
				organisation.OH_IsGlobalAccount = true;
				if (usePrefix)
				{
					AssertGetCodeForTaxRegistrationInOrgCountry($"Global Org without Address should fall back to own Country. Country Code:{countryCode}, Code:{code}.", organisation, null, expectedRegNumber);
				}
				else
				{
					AssertGetCountryCodeAndTaxRegistrationWithoutPrefix($"Global Org without Address should fall back to own Country. Country Code:{countryCode}, Code:{code}.", organisation, null, countryCode, expectedRegNumber);
				}

				transaction.InvoiceAddressOverride.OA_RN_NKCountryCode = "GB";
				organisation.OH_IsGlobalAccount = false;
				if (usePrefix)
				{
					AssertGetCodeForTaxRegistrationInOrgCountry($"Non Global Org should always use own Country. Country Code:{countryCode}, Code:{code}.", organisation, transaction.InvoiceAddressOverride, expectedRegNumber);
				}
				else
				{
					AssertGetCountryCodeAndTaxRegistrationWithoutPrefix($"Non Global Org should always use own Country. Country Code:{countryCode}, Code:{code}.", organisation, transaction.InvoiceAddressOverride, countryCode, expectedRegNumber);
				}
			}
		}

		void AssertGetCodeForTaxRegistrationInOrgCountry(string descStr, OrgHeader organisation, OrgAddress address, string expectedValue)
		{
			var taxRegistrationNumberInOrgCountryFromRefAddress = organisation.GetCodeForTaxRegistrationInOrgCountry(address);
			var prefixCode = string.Empty;

			if (organisation.OH_IsGlobalAccount && address != null && address.Country.IsPartOfEuropeanUnion)
			{
				prefixCode = RefCountry.GetPrefixForTaxRegistrationCode(address.Country.Code);
			}

			if ((!organisation.OH_IsGlobalAccount || address == null) && organisation.Country.IsPartOfEuropeanUnion)
			{
				prefixCode = RefCountry.GetPrefixForTaxRegistrationCode(organisation.Country.Code);
			}

			if (!expectedValue.StartsWith(prefixCode, StringComparison.OrdinalIgnoreCase))
			{
				expectedValue = prefixCode + expectedValue;
			}

			AssertEquals(descStr, expectedValue, taxRegistrationNumberInOrgCountryFromRefAddress);
		}

		void AssertGetCountryCodeAndTaxRegistrationWithoutPrefix(string descStr, OrgHeader organisation, OrgAddress address, string expectedCountryCode, string expectedTaxNumber)
		{
			var taxRegistrationNumberInOrgCountryFromRefAddress = organisation.GetCountryCodeAndTaxRegistrationWithoutPrefix(address);
			var taxRegistrationNumberInOrgCountryFromOverload = organisation.GetCountryCodeAndTaxRegistrationWithoutPrefix(address?.Country?.RN_Code);
			AssertEquals("RefAddress should return same as ZString overload; " + descStr, taxRegistrationNumberInOrgCountryFromRefAddress, taxRegistrationNumberInOrgCountryFromOverload);
			CombineAssertions(descStr, () =>
			{
				AssertEquals(expectedCountryCode, taxRegistrationNumberInOrgCountryFromRefAddress.countryCode);
				AssertEquals(expectedTaxNumber, taxRegistrationNumberInOrgCountryFromRefAddress.registrationNumber);
			});
		}

		OrgCusCode AddCustomsCodeForTest(OrgHeader organisation, ZString country, ZString customsRegNo, string codeType = null)
		{
			OrgCusCode taxCode = organisation.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = country;
			taxCode.OK_CodeType = codeType ?? Country.GetConsumptionTaxDescription(country);
			taxCode.OK_CustomsRegNo = customsRegNo;
			return taxCode;
		}

		#endregion

		#region Global Account

		public void TestGlobalAndNationalAccountValidation()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "ZX";

			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ZXZZZ";
			unloco.RL_RN_NKCountryCode = country.Code;

			organisation.OH_FullName = "Test Org";
			organisation.OH_RL_NKClosestPort = "ZXZZZ";
			organisation.OH_IsShippingProvider = true;
			organisation.OH_IsGlobalAccount = true;
			organisation.RunPreSaveValidation();

			string bothNationalAndGlobalAccountError = (NoResString)@"You cannot mark an Organization as both a National and a Global account. Please only choose one of these options.

A National account is one that has offices in many locations within the same country, whilst a Global account is one that has offices around the world.";

			Assert(organisation.OH_IsGlobalAccount);
			AssertEquals("Global account should use WW code instead of UNLOCO.", "TESORG_WW", organisation.OH_Code);
			AssertNoErrors(organisation.OH_IsGlobalAccountInfo);
			AssertNoErrors(organisation.OH_IsNationalAccountInfo);

			organisation.OH_IsNationalAccount = true;
			AssertHasError(organisation.OH_IsGlobalAccountInfo, bothNationalAndGlobalAccountError);
			AssertHasError(organisation.OH_IsNationalAccountInfo, bothNationalAndGlobalAccountError);

			organisation.RunPreSaveValidation();
			AssertHasError(organisation.OH_IsGlobalAccountInfo, bothNationalAndGlobalAccountError);
			AssertHasError(organisation.OH_IsNationalAccountInfo, bothNationalAndGlobalAccountError);

			organisation.OH_IsGlobalAccount = false;
			organisation.RunPreSaveValidation();
			AssertNoErrors(organisation.OH_IsGlobalAccountInfo);
			AssertNoErrors(organisation.OH_IsNationalAccountInfo);

			organisation.OH_IsGlobalAccount = true;
			AssertHasError(organisation.OH_IsGlobalAccountInfo, bothNationalAndGlobalAccountError);
			AssertHasError(organisation.OH_IsNationalAccountInfo, bothNationalAndGlobalAccountError);

			organisation.RunPreSaveValidation();
			AssertHasError(organisation.OH_IsGlobalAccountInfo, bothNationalAndGlobalAccountError);
			AssertHasError(organisation.OH_IsNationalAccountInfo, bothNationalAndGlobalAccountError);

			organisation.OH_IsNationalAccount = false;
			organisation.OH_IsShippingProvider = false;
			organisation.RunPreSaveValidation();

			string message = (NoResString)@"Global organizations are used to define global carrier relationships.
They are not suitable for clients, shippers or other relationships as these must have specific local entities (the corporations in each location) that you deal with.

This validation prevents the use of a global organization on anything but a carrier. If you do bypass this and use a non-carrier as a global organization, the reporting system and other systems will not function correctly and errors will result.

Please revert this organization to being a non-global organization, or if it is a carrier please set the carrier flag on.";

			AssertHasError(organisation.OH_IsGlobalAccountInfo, message);
			AssertNoErrors(organisation.OH_IsNationalAccountInfo);

			organisation.OH_IsShippingProvider = true;
			organisation.RunPreSaveValidation();
			AssertNoErrors(organisation.OH_IsGlobalAccountInfo);
			AssertNoErrors(organisation.OH_IsNationalAccountInfo);

			organisation.OH_IsShippingProvider = false;
			organisation.OH_IsGlobalAccount = false;
			organisation.RunPreSaveValidation();
			AssertNoErrors(organisation.OH_IsGlobalAccountInfo);
			AssertNoErrors(organisation.OH_IsNationalAccountInfo);
		}

		public void TestMainAddressForGlobalAccount()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address1 = organisation.MainAddress;
			address1.OA_RL_NKRelatedPortCode = "NZAKL";
			address1.OA_Address1 = "Address1";
			organisation.OH_IsGlobalAccount = ZBool.True;

			OrgAddress address2 = Factory.New<OrgAddress>();
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			address2.OA_Address1 = "Address2";
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			organisation.Addresses.Add(address2);
			Factory.Save();

			OrgHeader testOrganisation = Factory.Load<OrgHeader>(organisation.PK);

			AssertEquals("Main Address based on logged organisation should be loaded as main", "Address2", testOrganisation.MainAddress.OA_Address1);
		}

		#endregion

		#region Opportunities Filter Tests

		public void TestOpportunitiesFilter_ClearValues()
		{
			company.OpportunitiesDateTypeToFilter = "DFSL";
			AssertHasErrors("Invalid input should give error", company.OpportunitiesDateTypeToFilterInfo);
			company.OpportunitiesDateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.ClosedDate;
			AssertNoErrors("Valid Input should remove all errors on DateTypeToFilter", company.OpportunitiesDateTypeToFilterInfo);

			company.OpportunitiesDateTypeToFilter = "DFSL"; // Set back to invalid
			company.ClearOpportunitiesFilterValues();
			Assert("Clicking Clear should remove all errors and set everything back to default", !company.OpportunitiesDateTypeToFilterInfo.HasErrors());
			AssertEquals("Clicking clear should set drop-list to None", OrgHeaderLookups.LookupConstants.DateFilterListConstants.None, company.OpportunitiesDateTypeToFilter);
			AssertEquals("Clicking clear should set From date to Empty", ZDateTime.Empty, company.OpportunityDateFrom);
			AssertEquals("Clicking clear should set To date to Empty", ZDateTime.Empty, company.OpportunityDateTo);
			AssertNoErrors("Clicking clear should remove all errors on DateTypeToFilter", company.OpportunitiesDateTypeToFilterInfo);
			AssertNoErrors("Clicking clear should remove all errors on the From Date", company.OpportunityDateFromInfo);
			AssertNoErrors("Clicking clear should remove all errors on the To Date", company.OpportunityDateToInfo);
		}

		public void TestOpportunitiesFilter_DefaultValues()
		{
			AssertEquals("DateTypeToFilterList should be set to None as default", OrgHeaderLookups.LookupConstants.DateFilterListConstants.None, company.OpportunitiesDateTypeToFilter);
		}

		public void TestOpportunitiesFilter_Validation()
		{
			// Check invalid
			company.OpportunitiesDateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.RecallDate;
			company.OpportunityDateFrom = ZDateTime.Invalid;
			AssertHasErrors(company.OpportunityDateFromInfo);

			company.OpportunityDateTo = ZDateTime.Invalid;
			AssertHasErrors(company.OpportunityDateToInfo);

			// Check From > To
			company.OpportunityDateFrom = new ZDateTime(2006, 05, 03);
			company.OpportunityDateTo = new ZDateTime(2006, 04, 03);
			AssertHasErrors("FROM date is greater than TO date - ERROR on FROM date", company.OpportunityDateFromInfo);
			AssertHasErrors("FROM date is greater than TO date - NO ERROR on the TO date", company.OpportunityDateToInfo);

			company.OpportunityDateFrom = new ZDateTime(2006, 03, 03);
			company.OpportunityDateTo = new ZDateTime(2006, 04, 03);
			AssertNoErrors("Valid From and To dates shouldn't return errors", company.OpportunityDateFromInfo);
			AssertNoErrors("Valid From and To dates shouldn't return errors", company.OpportunityDateToInfo);

			// Readonly validation
			company.OpportunitiesDateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.None;
			Assert(company.OpportunityDateFromInfo.ReadOnly);
			Assert(company.OpportunityDateToInfo.ReadOnly);
			Assert(company.OpportunityDateFrom.IsEmpty);
			Assert(company.OpportunityDateTo.IsEmpty);

			company.OpportunitiesDateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.ClosedDate;
			Assert(!company.OpportunityDateFromInfo.ReadOnly);
			Assert(!company.OpportunityDateToInfo.ReadOnly);

			company.OpportunityDateFrom = new ZDateTime(2006, 03, 03);
			company.OpportunityDateTo = new ZDateTime(2006, 03, 03);
			ZDateTime expected = new ZDateTime(2006, 03, 03);

			company.OpportunitiesDateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.RecallDate;
			Assert(!company.OpportunityDateFromInfo.ReadOnly);
			Assert(!company.OpportunityDateToInfo.ReadOnly);
			AssertEquals("Date values shouldn't be cleared", expected, company.OpportunityDateFrom);
			AssertEquals("Date values shouldn't be cleared", expected, company.OpportunityDateTo);

			company.FilterValidationError += new EventHandler(Company_FilterValidationError);

			// From Date
			company.OpportunityDateFrom = ZDateTime.Invalid;
			Assert("OnFilterValidationError has NOT been called", !filterValidationHit);
			company.LoadOpportunitiesWithFiltering();
			Assert("OnFilterValidationError should have been called", filterValidationHit);

			filterValidationHit = false;

			// To Date
			company.OpportunityDateTo = ZDateTime.Invalid;
			Assert("OnFilterValidationError has NOT been called", !filterValidationHit);
			company.LoadOpportunitiesWithFiltering();
			Assert("OnFilterValidationError should have been called", filterValidationHit);

			filterValidationHit = false;

			// DateTypeToFilter
			company.OpportunitiesDateTypeToFilter = "DFDF";
			Assert("OnFilterValidationError has NOT been called", !filterValidationHit);
			company.LoadOpportunitiesWithFiltering();
			Assert("OnFilterValidationError should have been called", filterValidationHit);

			filterValidationHit = false;

			company.OpportunitiesDateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.ClosedDate;
			company.OpportunityDateFrom = ZDateTime.Today;
			company.OpportunityDateTo = ZDateTime.Today;
			Assert("OnFilterValidationError has NOT been called", !filterValidationHit);
			company.LoadOpportunitiesWithFiltering();
			Assert("OnFilterValidationError has NOT been called", !filterValidationHit);

			company.FilterValidationError -= new EventHandler(Company_FilterValidationError);
		}

		public void TestOpportunitiesFilter()
		{
			company.SalesOpportunities.AddNew();
			company.SalesOpportunities.AddNew();
			company.SalesOpportunities.AddNew();
			company.SalesOpportunities.AddNew();

			company.SalesOpportunities[0].P8_ClosedDateLocal = new ZDateTime(2006, 05, 03);
			company.SalesOpportunities[0].P8_RecallDateLocal = new ZDateTime(2007, 05, 03);

			company.SalesOpportunities[1].P8_ClosedDateLocal = new ZDateTime(2006, 06, 03);
			company.SalesOpportunities[1].P8_RecallDateLocal = new ZDateTime(2007, 06, 03);

			company.SalesOpportunities[2].P8_ClosedDateLocal = new ZDateTime(2006, 07, 03);
			company.SalesOpportunities[2].P8_RecallDateLocal = new ZDateTime(2007, 07, 03);

			company.SalesOpportunities[3].P8_ClosedDateLocal = new ZDateTime(2006, 09, 12);
			company.SalesOpportunities[3].P8_RecallDateLocal = new ZDateTime(2007, 09, 12);

			RunDateTypeFilterTests(OrgHeaderLookups.LookupConstants.DateFilterListConstants.ClosedDate, 2006, company.SalesOpportunities[3]);
			RunDateTypeFilterTests(OrgHeaderLookups.LookupConstants.DateFilterListConstants.RecallDate, 2007, company.SalesOpportunities[3]);
		}

		void RunDateTypeFilterTests(string dateType, int year, OrgOpportunity opportunityForTesting)
		{
			company.OpportunitiesDateTypeToFilter = dateType;

			// Check for From & To dates empty
			company.OpportunityDateFrom = ZDateTime.Empty;
			company.OpportunityDateTo = ZDateTime.Empty;
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("All Sales Opportunities should be loaded", 4, company.SalesOpportunities.Count);

			// Check for only To date empty
			company.OpportunityDateFrom = new ZDateTime(year, 05, 01);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("All Sales Opportunities should be loaded", 4, company.SalesOpportunities.Count);

			// Check for valid From and To dates 
			company.OpportunityDateTo = new ZDateTime(year, 12, 20);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("All Sales Opportunities should be loaded", 4, company.SalesOpportunities.Count);

			company.OpportunityDateFrom = new ZDateTime(year, 8, 03);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("Only 1 Sales Opportunity should have loaded", 1, company.SalesOpportunities.Count);
			AssertCollectionContains(opportunityForTesting, company.SalesOpportunities);

			// Check for only From date empty
			company.OpportunityDateFrom = ZDateTime.Empty;
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("All Sales Opportunities should be loaded", 4, company.SalesOpportunities.Count);

			// Check none in range
			company.OpportunityDateFrom = ZDateTime.Empty;
			company.OpportunityDateTo = ZDateTime.Empty;
			company.OpportunityDateFrom = new ZDateTime(year + 10, 05, 04);
			company.OpportunityDateTo = new ZDateTime(year + 10, 09, 04);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("No sales opportunities should have been loaded - " + dateType + "only for " + year + " From: " + company.OpportunityDateFrom.ToLongTimeString(), 0, company.SalesOpportunities.Count);

			// Check From boundary condition
			company.OpportunityDateFrom = new ZDateTime(year, 05, 02);
			company.OpportunityDateTo = new ZDateTime(year, 05, 05);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("Only one sales opportunity should be loaded", 1, company.SalesOpportunities.Count);
			AssertCollectionContains("The sales opportunity with date 03/05/" + year + " should be loaded", company.SalesOpportunities[0], company.SalesOpportunities);

			company.OpportunityDateFrom = new ZDateTime(year, 05, 03);
			company.OpportunityDateTo = new ZDateTime(year, 05, 05);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("Only one sales opportunity should be loaded", 1, company.SalesOpportunities.Count);
			AssertCollectionContains("The sales opportunity with date 03/05/" + year + " should be loaded", company.SalesOpportunities[0], company.SalesOpportunities);

			company.OpportunityDateFrom = new ZDateTime(year, 05, 04);
			company.OpportunityDateTo = new ZDateTime(year, 05, 05);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("No sales opportunities should have be loaded", 0, company.SalesOpportunities.Count);

			// Check To boundary condition
			company.OpportunityDateFrom = new ZDateTime(year, 04, 12);
			company.OpportunityDateTo = new ZDateTime(year, 05, 05);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("Only one sales opportunity should be loaded", 1, company.SalesOpportunities.Count);
			AssertCollectionContains("The sales opportunity with date 03/05/" + year + " should be loaded", company.SalesOpportunities[0], company.SalesOpportunities);

			company.OpportunityDateFrom = new ZDateTime(year, 04, 12);
			company.OpportunityDateTo = new ZDateTime(year, 05, 03);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("Only one sales opportunity should be loaded", 1, company.SalesOpportunities.Count);
			AssertCollectionContains("The sales opportunity with date 03/05/" + year + " should be loaded", company.SalesOpportunities[0], company.SalesOpportunities);

			company.OpportunityDateFrom = new ZDateTime(year, 04, 12);
			company.OpportunityDateTo = new ZDateTime(year, 05, 02);
			company.LoadOpportunitiesWithFiltering();
			AssertEquals("No sales opportunities should have be loaded", 0, company.SalesOpportunities.Count);

			company.ClearOpportunitiesFilterValues();
		}

		void Company_FilterValidationError(object sender, EventArgs e)
		{
			filterValidationHit = true;
		}

		bool filterValidationHit;

		public void TestOpportunitiesFilter_DateTypeMaxLength()
		{
			Assert("The Max Code Length should be the length of the longest code in the list. if this test fails you may have added a new code to the list which exceeds the limit of this test. Please update this test AND also the maxlength of the OpportunitiesDateTypeToFilterInfo.", company.Lookups.DateFilterList.MaxCodeLength <= 11);
		}

		#endregion

		#region TestIAddressDetails

		public void TestIAddressDetails()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address = org.MainAddress;

			IAddressDetails addressDetails = org;
			address.OA_Address1 = "address1";
			AssertEquals("address1", "address1", addressDetails.AddressLine1);

			address.OA_Address2 = "address2";
			AssertEquals("address2", "address2", addressDetails.AddressLine2);

			address.OA_City = "city";
			AssertEquals("city", "city", addressDetails.City);

			org.OH_FullName = "CargoWise edi";
			AssertEquals("CompanyName", "CargoWise edi", addressDetails.CompanyName);

			address.OA_Email = "support@cargowise.com";
			AssertEquals("Email", "support@cargowise.com", addressDetails.Email);

			address.OA_Fax = "02 8520 2200";
			AssertEquals("Fax", "02 8520 2200", addressDetails.Fax);

			address.OA_Phone = "02 8520 2201";
			AssertEquals("Phone", "02 8520 2201", addressDetails.Phone);

			address.OA_State = "NSW";
			AssertEquals("State", "NSW", addressDetails.State);

			address.OA_PostCode = "2015";
			AssertEquals("PostCode", "2015", addressDetails.PostCode);
		}

		#endregion

		#region ValidateSalesRep

		public void TestValidateSalesRep()
		{
			bool value = OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.Value;
			try
			{
				RawDataRegistry.Instance.MakeSalesRepMandatoryForTempOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertValidateSalesRep(false, false, false, false);
				AssertValidateSalesRep(false, false, true, false);
				AssertValidateSalesRep(false, true, false, false);
				AssertValidateSalesRep(false, true, true, false);
				AssertValidateSalesRep(false, false, false, true);
				AssertValidateSalesRep(false, false, true, true);
				AssertValidateSalesRep(false, true, false, true);
				AssertValidateSalesRep(false, true, true, true);
				OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertValidateSalesRep(false, false, false, false);
				AssertValidateSalesRep(true, false, true, false);
				AssertValidateSalesRep(true, true, false, false);
				AssertValidateSalesRep(true, true, true, false);
				AssertValidateSalesRep(false, false, false, true);
				AssertValidateSalesRep(false, false, true, true);
				AssertValidateSalesRep(false, true, false, true);
				AssertValidateSalesRep(false, true, true, true);

				RawDataRegistry.Instance.MakeSalesRepMandatoryForTempOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertValidateSalesRep(false, false, false, false);
				AssertValidateSalesRep(false, false, true, false);
				AssertValidateSalesRep(false, true, false, false);
				AssertValidateSalesRep(false, true, true, false);
				AssertValidateSalesRep(false, false, false, true);
				AssertValidateSalesRep(true, false, true, true);
				AssertValidateSalesRep(true, true, false, true);
				AssertValidateSalesRep(true, true, true, true);
				OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertValidateSalesRep(false, false, false, false);
				AssertValidateSalesRep(true, false, true, false);
				AssertValidateSalesRep(true, true, false, false);
				AssertValidateSalesRep(true, true, true, false);
				AssertValidateSalesRep(false, false, false, true);
				AssertValidateSalesRep(true, false, true, true);
				AssertValidateSalesRep(true, true, false, true);
				AssertValidateSalesRep(true, true, true, true);

				AssertGlobalSalesRep();
			}
			finally
			{
				OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
		}

		void AssertValidateSalesRep(bool expected, bool debtor, bool sales, bool temp)
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>(); // new
			org1.OH_IsDebtor = debtor;
			org1.OH_IsSalesLead = sales;
			org1.OH_IsTempAccount = temp;
			OrgHeader org2 = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery()); // for edits
			org2.OH_IsDebtor = debtor;
			org2.OH_IsSalesLead = sales;
			org2.OH_IsTempAccount = temp;
			org2.StaffAssignments.RemoveAndDeleteAll();
			org1.RunPreSaveValidation();
			org2.RunPreSaveValidation();

			AssertEquals(expected, org1.StaffAssignments.HasErrors());
			AssertEquals(expected, org2.StaffAssignments.HasErrors());
			if (OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.Value)
			{
				Assert(!org2.LightValidationEnabled);
			}
			if (RawDataRegistry.Instance.MakeSalesRepMandatoryForTempOrganization.Value)
			{
				Assert(!org2.LightValidationEnabled);
			}
		}

		void AssertGlobalSalesRep()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			GlbStaff staff = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();
			OrgStaffAssignments assign = org1.StaffAssignments.AddNew();
			assign.O8_GS_NKPersonResponsible = staff.GS_Code;
			assign.O8_GC = ZGuid.Empty;
			assign.O8_Role = "SAL";
			factory.Save();
			org1.RunPreSaveValidation();
			Assert(!org1.StaffAssignments.HasErrors());
		}

		#endregion

		#region Temporary Organisation Tests

		[ExpectNoExceptions()]
		public void TestSaveTempOrg()
		{
			OrgHeader temp = Factory.New<OrgHeader>();
			temp.SetDefaultValuesForTemporaryOrganisation();
			temp.OH_FullName = "Testing Temp Org2";
			temp.OH_RL_NKClosestPort = "AUBNE";
			temp.MainAddress.OA_Address1 = "Test Address";
			temp.OH_Code = "TestCode";

			Factory.Save();
		}

		public void TestSetDefaultValuesForTemporaryOrganisation()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals(org.OH_IsTempAccount, false);
			Assert(org.OH_IsActive);

			org.SetDefaultValuesForTemporaryOrganisation();
			AssertEquals("orgheader is a temporary acct", true, org.OH_IsTempAccount);
			AssertEquals("similar organisations should be read only", true, org.SimilarOrgMatches.ReadOnly);
		}

		public void TestMatchesFilteringForTemporaryOrganisation()
		{
			TempOrg.OH_FullName = "12345";
			TempOrg.OH_RL_NKClosestPort = "AUSYD";
			TempOrg.MainAddress.OA_Address1 = "Address1";
			TempOrg.MainAddress.OA_City = "City";
			TempOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TempOrg);
			Assert("Pattern Matches Exist", TempOrg.PatternMatchesForThisOrg.Count > 0);

			foreach (OrgPatternMatch match in TempOrg.PatternMatchesForThisOrg)
			{
				Assert("Similar organisations should not contain the new organisation being added", !TempOrg.SimilarOrgMatches.Contains(match));
			}
		}

		public void TestMandatoryValidation()
		{
			TempOrg.OH_FullName = string.Empty;
			TempOrg.MainAddress.OA_Address1 = string.Empty;
			TempOrg.OH_RL_NKClosestPort = string.Empty;
			TempOrg.MainAddress.OA_RN_NKCountryCode = string.Empty;

			TempOrg.MainAddress.OA_Address2 = string.Empty;
			TempOrg.MainAddress.OA_PostCode = string.Empty;
			TempOrg.MainAddress.OA_Phone = string.Empty;
			TempOrg.MainAddress.OA_Fax = string.Empty;
			TempOrg.MainAddress.OA_Email = string.Empty;
			TempOrg.PrimaryRegistrationNumber.Number = string.Empty;

			TempOrg.Validation.ValidateOH_FullName();
			TempOrg.MainAddress.Validation.ValidateOA_Address1();
			TempOrg.MainAddress.Validation.ValidateOA_Address2();
			TempOrg.MainAddress.Validation.ValidateOA_PostCode();
			TempOrg.Validation.ValidateOH_RL_NKClosestPort();
			TempOrg.MainAddress.Validation.ValidateOA_Phone();
			TempOrg.MainAddress.Validation.ValidateOA_Fax();
			TempOrg.MainAddress.Validation.ValidateOA_Email();
			TempOrg.PrimaryRegistrationNumber.Validation.ValidateNumber();

			AssertEquals("Full name should be in error", 1, TempOrg.OH_FullNameInfo.GetErrors().Count());

			// Note: this field is no longer part of OrgHeader (it needs to be removed from the DB) so will not cause error						
			AssertEquals("Port should be in error", 1, TempOrg.OH_RL_NKClosestPortInfo.GetErrors().Count());

			AssertEquals("Address 2 should not be in error", 0, TempOrg.MainAddress.OA_Address2Info.GetErrors().Count());
			AssertEquals("PostCode should not be in error", 0, TempOrg.MainAddress.OA_PostCodeInfo.GetErrors().Count());
			AssertEquals("Phone should not be in error", 0, TempOrg.MainAddress.OA_PhoneInfo.GetErrors().Count());
			AssertEquals("Fax should not be in error", 0, TempOrg.MainAddress.OA_FaxInfo.GetErrors().Count());
			AssertEquals("Email should not be in error", 0, TempOrg.MainAddress.OA_EmailInfo.GetErrors().Count());
			AssertEquals("BusinessRegNo should not be in error", 0, TempOrg.PrimaryRegistrationNumber.NumberInfo.GetErrors().Count());
		}

		public void TestTempOrgTypeValidation()
		{
			var expectedErrorMessage = string.Format("{0} organizations must be of at least one these types: {1}, {2}, {3} {4} or {5}.",
						TempOrg.OH_IsTempAccountInfo.HumanReadableName,
						TempOrg.OH_IsConsigneeInfo.HumanReadableName,
						TempOrg.OH_IsConsignorInfo.HumanReadableName,
						TempOrg.OH_IsDebtorInfo.HumanReadableName,
						TempOrg.OH_IsCreditorInfo.HumanReadableName,
						TempOrg.OH_IsSalesLeadInfo.HumanReadableName);
			TempOrg.OH_IsConsignee = false;
			TempOrg.OH_IsConsignor = false;
			TempOrg.OH_IsDebtor = false;
			TempOrg.OH_IsCreditor = false;
			TempOrg.OH_IsSalesLead = false;
			Assert("Temporary Acct should be in error", TempOrg.OH_IsTempAccountInfo.HasError(expectedErrorMessage));

			TempOrg.OH_IsConsignee = true;
			Assert("Temporary Acct should not be in error", !TempOrg.OH_IsTempAccountInfo.HasErrors());

			TempOrg.OH_IsConsignee = false;
			TempOrg.OH_IsConsignor = true;
			Assert("Temporary Acct should not be in error", !TempOrg.OH_IsTempAccountInfo.HasErrors());

			TempOrg.OH_IsConsignor = false;
			TempOrg.OH_IsDebtor = true;
			Assert("Temporary Acct should not be in error", !TempOrg.OH_IsTempAccountInfo.HasErrors());

			TempOrg.OH_IsDebtor = false;
			TempOrg.OH_IsCreditor = true;
			Assert("Temporary Acct should not be in error", !TempOrg.OH_IsTempAccountInfo.HasErrors());

			TempOrg.OH_IsCreditor = false;
			TempOrg.OH_IsSalesLead = true;
			Assert("Temporary Acct should not be in error", !TempOrg.OH_IsTempAccountInfo.HasErrors());

			TempOrg.OH_IsSalesLead = false;
			Assert("Temporary Acct should be in error", TempOrg.OH_IsTempAccountInfo.HasError(expectedErrorMessage));

			TempOrg.OH_IsTempAccount = false;
			Assert("Temporary Acct should not be in error", !TempOrg.OH_IsTempAccountInfo.HasErrors());

			TempOrg.OH_IsTempAccount = true;
			Assert("Temporary Acct should be in error", TempOrg.OH_IsTempAccountInfo.HasError(expectedErrorMessage));
		}

		public void TestControllingAgentAndControllingCustomerAreExclusive()
		{
			using (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedErrorMessage = "You cannot mark an Organization as both Controlling Agent and Controlling Customer. Please only choose one of these options.";
				TempOrg.OH_IsControllingAgent = false;
				TempOrg.OH_IsControllingCustomer = false;
				AssertNoErrors(TempOrg.OH_IsControllingAgentInfo);
				AssertNoErrors(TempOrg.OH_IsControllingCustomerInfo);

				TempOrg.OH_IsControllingAgent = true;
				AssertNoErrors(TempOrg.OH_IsControllingAgentInfo);
				AssertNoErrors(TempOrg.OH_IsControllingCustomerInfo);

				TempOrg.OH_IsControllingCustomer = true;
				AssertHasError(TempOrg.OH_IsControllingAgentInfo, expectedErrorMessage);
				AssertHasError(TempOrg.OH_IsControllingCustomerInfo, expectedErrorMessage);

				TempOrg.OH_IsControllingAgent = false;
				AssertNoErrors(TempOrg.OH_IsControllingAgentInfo);
				AssertNoErrors(TempOrg.OH_IsControllingCustomerInfo);
			}
		}

		public void TestControllingCustomerRequiresCAGRelatedParty_NewOrg()
		{
			AssertControllingCustomerRequiresCAGRelatedParty(Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomerWithoutAgt, false);
		}

		public void TestControllingCustomerRequiresCAGRelatedParty_ExistingOrg()
		{
			AssertControllingCustomerRequiresCAGRelatedParty(Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomerWithoutAgt, true);
		}

		void AssertControllingCustomerRequiresCAGRelatedParty(ISecurityCheckpoint securityCheckpoint, bool saveToDB)
		{
			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedErrorMessage = @"You have insufficient security rights to flag an organization as a Controlling Customer without specifying a Controlling Agent first.
Add related party CAG – Controlling Agent first, or remove ""Controlling Customer"" organization type flag.";

				securityCheckpoint.IsAllowed = true;

				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				if (saveToDB)
				{
					Factory.Save();
				}

				AssertNoErrors(org1.OH_IsControllingCustomerInfo);

				org1.OH_IsControllingCustomer = true;
				AssertNoErrors(org1.OH_IsControllingCustomerInfo);

				securityCheckpoint.IsAllowed = false;
				org1.OH_IsControllingCustomer = false;
				AssertNoErrors(org1.OH_IsControllingCustomerInfo);

				org1.OH_IsControllingCustomer = true;
				AssertHasError(org1.OH_IsControllingCustomerInfo, expectedErrorMessage);

				var relation = org1.AllRelatedParties.AddNew();
				relation.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
				relation.PR_OH_Parent = org1.PK;
				relation.PR_OH_RelatedParty = org2.PK;

				AssertHasError(org1.OH_IsControllingCustomerInfo, expectedErrorMessage);

				relation.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
				AssertNoErrors(org1.OH_IsControllingCustomerInfo);

				org1.AllRelatedParties.RemoveAndDelete(relation);
				AssertHasError(org1.OH_IsControllingCustomerInfo, expectedErrorMessage);
			}
		}
		public void TestOH_IsControllingCustomer_ReadOnly()
		{
			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testHeader = Factory.NewWithValidTestData<OrgHeader>();
				testHeader.OH_IsControllingCustomer = true;
				AssertEquals(false, testHeader.IsInDatabase);

				Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomer.IsAllowed = false;
				AssertEquals(true, testHeader.OH_IsControllingCustomerInfo.ReadOnly);

				Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomer.IsAllowed = true;
				AssertEquals(false, testHeader.OH_IsControllingCustomerInfo.ReadOnly);

				Factory.Save();
				AssertEquals(true, testHeader.IsInDatabase);

				Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomer.IsAllowed = false;
				AssertEquals(true, testHeader.OH_IsControllingCustomerInfo.ReadOnly);

				Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomer.IsAllowed = true;
				AssertEquals(false, testHeader.OH_IsControllingCustomerInfo.ReadOnly);
			}
		}

		public void TestControllingCustomersAsString()
		{
			var mainOrg = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerDelivery = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerDelivery.OH_Code = "TESTORGDLV";
			var controllingCustomerPickup = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerPickup.OH_Code = "TESTORGPIC";
			var controllingCustomerPickupAndDelivery = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerPickupAndDelivery.OH_Code = "TESTORGPAD";

			AssertEquals(string.Empty, mainOrg.ControllingCustomersAsString);

			var relationDelivery = mainOrg.AllRelatedParties.AddNew();
			relationDelivery.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relationDelivery.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relationDelivery.PR_OH_Parent = mainOrg.PK;
			relationDelivery.PR_OH_RelatedParty = controllingCustomerDelivery.PK;

			AssertEquals(string.Empty, mainOrg.ControllingCustomersAsString);

			relationDelivery.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			AssertEquals("DLV - TESTORGDLV", mainOrg.ControllingCustomersAsString);

			var relationPickup = mainOrg.AllRelatedParties.AddNew();
			relationPickup.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			relationPickup.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			relationPickup.PR_OH_Parent = mainOrg.PK;
			relationPickup.PR_OH_RelatedParty = controllingCustomerPickup.PK;

			AssertEquals("DLV - TESTORGDLV, PIC - TESTORGPIC", mainOrg.ControllingCustomersAsString);

			var relationPickupAndDelivery = mainOrg.AllRelatedParties.AddNew();
			relationPickupAndDelivery.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			relationPickupAndDelivery.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			relationPickupAndDelivery.PR_OH_Parent = mainOrg.PK;
			relationPickupAndDelivery.PR_OH_RelatedParty = controllingCustomerPickupAndDelivery.PK;

			AssertEquals("DLV - TESTORGDLV, PAD - TESTORGPAD, PIC - TESTORGPIC", mainOrg.ControllingCustomersAsString);

			relationPickup.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertEquals("DLV - TESTORGDLV, PAD - TESTORGPAD", mainOrg.ControllingCustomersAsString);
		}

		public void TestDebtorGroupReadOnlyClearsNotifications()
		{
			TempOrg.MiscServ.OM_OJ_ARDebtorGroup = ZGuid.Missing;
			TempOrg.OH_IsDebtor = false;
			Assert("Debtor group should be ReadOnly", TempOrg.MiscServ.OM_OJ_ARDebtorGroupInfo.ReadOnly);
			Assert("Debtor group is ReadOnly and should have no notifications", !TempOrg.MiscServ.OM_OJ_ARDebtorGroupInfo.HasNotifications());
			TempOrg.OH_IsDebtor = true;
			Assert("Debtor group should not be ReadOnly", !TempOrg.MiscServ.OM_OJ_ARDebtorGroupInfo.ReadOnly);
			Assert("Debtor group is no longer ReadOnly and should have notifications", TempOrg.MiscServ.OM_OJ_ARDebtorGroupInfo.HasNotifications());
		}

		public void TestCreditorGroupReadOnlyClearsNotifications()
		{
			TempOrg.MiscServ.OM_OG_APCreditorGroup = ZGuid.Missing;
			TempOrg.OH_IsCreditor = false;
			Assert("Creditor group should be ReadOnly", TempOrg.MiscServ.OM_OG_APCreditorGroupInfo.ReadOnly);
			Assert("Creditor group is ReadOnly and should have no notifications", !TempOrg.MiscServ.OM_OG_APCreditorGroupInfo.HasNotifications());
			TempOrg.OH_IsCreditor = true;
			Assert("Creditor group should not be ReadOnly", !TempOrg.MiscServ.OM_OG_APCreditorGroupInfo.ReadOnly);
			Assert("Creditor group is no longer ReadOnly and should have notifications", TempOrg.MiscServ.OM_OG_APCreditorGroupInfo.HasNotifications());
		}

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(OrgHeader)));
		}

		#endregion

		#region Org Type Defaulting

		public void TestDefaultValuesOnTypeChange()
		{
			company.OH_IsCreditor = false;

			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "AUD";
			GlbBranch branch1 = Factory.New<GlbBranch>();
			AccBankAccount account1 = new MasterFilesTestHelper(Factory).GetNewBankAccountWithBranch(currency1, branch1, true);

			company.CompanyData.OB_RX_NKAPDefltCurrency = currency1.RX_Code;
			company.CompanyData.OB_GB_ControllingBranch = branch1.PK;

			AssertEquals("The default bank account should not be set when the org isn't a creditor", ZGuid.Empty, company.CompanyData.OB_AB_APDefaultBankAccount);

			company.OH_IsCreditor = true;

			AssertEquals("Default bank account should be set when the payables flag is set", account1.PK, company.CompanyData.OB_AB_APDefaultBankAccount);
		}

		#endregion

		#region Grouping

		public void TestShouldAskToCreateEnglishMainAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org.MainAddress;
			address1.OA_Language = Constants.Languages.English;
			address1.OA_RL_NKRelatedPortCode = "Solar";
			address1.OA_State = "Solar";
			address1.OA_Address1 = "Hydrogen";
			address1.OA_Address2 = "Oxygen";
			address1.OA_City = "Mercury";
			Assert(org.HasEnglishMainAddress);

			Assert(!org.ShouldAskToCreateEnglishMainAddress);
			address1.OA_Language = Constants.Languages.French;
			address1.OA_City = "Nîmes";
			Assert(!org.HasEnglishMainAddress);
			Assert(org.ShouldAskToCreateEnglishMainAddress);
			address1.OA_Language = Constants.Languages.English;
			address1.OA_City = "Mercury";

			Factory.Save();
			address1.OA_Language = Constants.Languages.French;
			address1.OA_City = "Nîmes";
			Assert(org.ShouldAskToCreateEnglishMainAddress);

			var address2 = org.CreateEnglishEquivalentAddress(address1);
			Assert(!org.ShouldAskToCreateEnglishMainAddress);
		}

		public void TestMainAddressLocalizedForDocument()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Language = Constants.Languages.English;
			address.OA_RL_NKRelatedPortCode = "Solar";
			address.OA_State = "Solar";
			address.OA_Address1 = "Hydrogen";
			address.OA_Address2 = "Oxygen";
			address.OA_City = "Mercury";

			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Language = Constants.Languages.French;
			translatedAddress.OTA_Address1 = "Nîmes";
			translatedAddress.OTA_Address2 = "Nîmes";
			translatedAddress.OTA_City = "Nîmes";

			AssertEquals("HYDROGEN OXYGEN MERCURY SOLAR", org.MainAddress.AddressAsASingleLine);

			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			{
				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("HYDROGEN OXYGEN MERCURY SOLAR", org.MainAddress.AddressAsASingleLine);
				}
				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.French))
				{
					AssertEquals("NÎMES NÎMES NÎMES SOLAR", org.MainAddress.AddressAsASingleLine);

					translatedAddress.OTA_Address1 = "Hyères";
					translatedAddress.OTA_Address2 = "Hyères";
					translatedAddress.OTA_City = "Hyères";
					AssertEquals("HYÈRES HYÈRES HYÈRES SOLAR", org.MainAddress.AddressAsASingleLine);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestMainAddressLocalizedForDocumentNotStackOverflow()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Language = Constants.Languages.Japanese;
			address.SetBaseOA_RL_NKRelatedPortCode("");
			address.OA_Address1 = "広島県福山市霞町2-4-1";

			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Language = Constants.Languages.English;
			translatedAddress.OTA_Address1 = "2 Chome-4-1, Kasumichō";

			AssertEquals("広島県福山市霞町2-4-1", org.MainAddress.Address1);

			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.English))
			{
				//It will throw StackOverflowException here before the fix
				AssertEquals("2 Chome-4-1, Kasumichō", org.MainAddress.Address1);
			}
		}

		public void TestCreateEnglishEquivalentAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Language = Constants.Languages.French;
			address.OA_RL_NKRelatedPortCode = "Solar";
			address.OA_State = "Solar";
			address.OA_Address1 = "Nîmes";
			address.OA_Address2 = "Hyères";
			address.OA_City = "Paris";
			address.OA_PostCode = "Angoulême";

			var englishEquivilent = org.CreateEnglishEquivalentAddress(address);
			AssertEquals(Constants.Languages.English, englishEquivilent.OTA_Language);
			AssertEquals("Nimes", englishEquivilent.OTA_Address1);
			AssertEquals("Hyeres", englishEquivilent.OTA_Address2);
			AssertEquals("Paris", englishEquivilent.OTA_City);
			AssertEquals("Angouleme", englishEquivilent.OTA_PostCode);
		}

		#endregion

		public void TestRelatedIWorkflowProvidersDoesNotAddOpportunitiesWhenOrgIsNotSales()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.OH_IsSalesLead = true;
			org.SalesOpportunities.AddNew();

			Factory.Save();

			_ = org.RelatedIWorkflowProviders;

			Assert("When OH_IsSalesLead is true, RelatedIWorkflowProviders should register SalesOpportunities as child", org.IsRegisteredEditableChildObject(org.SalesOpportunities));

			org.OH_IsSalesLead = false;
			org.RegisterEditableChildObject(org.SalesOpportunities); // Setting OH_IsSalesLead unregisters SalesOpportunities so we must re-register it.

			_ = org.RelatedIWorkflowProviders;

			Assert("When OH_IsSalesLead is false, RelatedIWorkflowProviders should unregister SalesOpportunities as child", !org.IsRegisteredEditableChildObject(org.SalesOpportunities));
		}

		public void TestInvalidGlobalCreditCurrencyOrMissingExRateMessage()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC123";

			var expectedMessage = @"Global outstanding transactions balance cannot be calculated for the global credit group.
It requires valid exchange rates for today to be entered (using ‘GCB’ or ‘PER’ exchange rate type) in all system companies where the Global Credit Group has transactions and/or local credit limits.
For a list of system companies and currency codes, please refer to Organization (ABC123) > A/R > Credit Control and Settlement > Global > Companies Local Credit Control and Settlement Details.";

			AssertEquals(expectedMessage, org.InvalidGlobalCreditCurrencyOrMissingExRateMessage);
		}

		public void TestGetCompanyOrgProxyFromCompanyCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC123";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CO1";
			company.GC_OH_OrgProxy = orgHeader.PK;

			Factory.Save();

			var actualOrgProxy = OrgHeader.GetCompanyOrgProxyFromCompanyCode(Factory, "CO1");

			AssertEquals(orgHeader.PK, actualOrgProxy.PK);
		}

		public void TestSecurityRightsViewWithWarehouse()
		{
			int allSecuritCount = company.SecurityRights.Count;
			company.OH_IsWarehouseClient = true;
			AssertEquals("Should show all securitis for Warehouse client", allSecuritCount, company.SecurityRightsView.Count);

			company.OH_IsWarehouseClient = false;
			AssertNotEquals("Should exclude some securitis for Warehouse client", allSecuritCount, company.SecurityRightsView.Count);

			foreach (OrgSecurity warehouseRight in company.SecurityRightsView.Where(r => r.IsWebWarehouseSecurity))
			{
				AssertCollectionNotContains("Should not contain any warehouse security: " + warehouseRight.SecurityKey, warehouseRight, company.SecurityRightsView);
			}
		}

		public void TestContactSecurityRightsViewWithWarehouse()
		{
			OrgContact newContact = company.Contacts.AddNew();
			newContact.OC_IsActive = true;

			int allSecuritCount = company.SecurityRights.Count;
			company.OH_IsWarehouseClient = true;
			AssertEquals("Should show all securitis for contact for Warehouse client", allSecuritCount, newContact.SecurityRightsView.Count);

			company.OH_IsWarehouseClient = false;
			AssertNotEquals("Should exclude some securitis for Warehouse client", allSecuritCount, newContact.SecurityRightsView.Count);

			foreach (OrgSecurity warehouseRight in newContact.SecurityRightsView.Where(r => r.Security.IsWebWarehouseSecurity).Select(r => r.Security))
			{
				AssertCollectionNotContains("Should not contain any warehouse security: " + warehouseRight.SecurityKey, warehouseRight, company.SecurityRightsView);
			}
		}

		public void TestReDefaultARAPTaxConfigurationsBasedOnRegistry()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;

			var collection1 = new ARAPDefaultTaxRecognitionRuleCollection();
			AddDefaultTaxRecognitionRule(collection1, "IEU", "IEU", "NON");
			AddDefaultTaxRecognitionRule(collection1, "IEU", "OEU", "NON");
			AddDefaultTaxRecognitionRule(collection1, "OEU", "SAL", "DEF");
			AddDefaultTaxRecognitionRule(collection1, "OEU", "DTL", "DEF");
			AccountingMasterFilesRegistry.Instance.ARDefaultTaxRecognitionRule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection1);

			var collection2 = new ARAPDefaultTaxRecognitionRuleCollection();
			AddDefaultTaxRecognitionRule(collection2, "IEU", "IEU", "NON");
			AddDefaultTaxRecognitionRule(collection2, "IEU", "OEU", "DEF");
			AddDefaultTaxRecognitionRule(collection2, "OEU", "SAL", "NON");
			AddDefaultTaxRecognitionRule(collection2, "OEU", "DTL", "DEF");
			AccountingMasterFilesRegistry.Instance.APDefaultTaxRecognitionRule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection2);

			// login company in EU
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				// org in EU
				org.OH_RL_NKClosestPort = "ITROM";
				org.RedefaultTaxRecognition();
				AssertEquals("NON", org.CompanyData.OB_ARVATConfig);
				AssertEquals("NON", org.CompanyData.OB_APVATConfig);

				// org outside EU
				org.OH_RL_NKClosestPort = "AUSYD";
				org.RedefaultTaxRecognition();
				AssertEquals("NON", org.CompanyData.OB_ARVATConfig);
				AssertEquals("DEF", org.CompanyData.OB_APVATConfig);
			}

			// login company outside EU
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				// org in the same country as the login company
				org.OH_RL_NKClosestPort = "USNYC";
				org.RedefaultTaxRecognition();
				AssertEquals("DEF", org.CompanyData.OB_ARVATConfig);
				AssertEquals("NON", org.CompanyData.OB_APVATConfig);

				// org's country different to the login company
				org.OH_RL_NKClosestPort = "AUSYD";
				org.RedefaultTaxRecognition();
				AssertEquals("DEF", org.CompanyData.OB_ARVATConfig);
				AssertEquals("DEF", org.CompanyData.OB_APVATConfig);
			}
		}

		void AddDefaultTaxRecognitionRule(ARAPDefaultTaxRecognitionRuleCollection ruleCollection, string loginCompanyCountryRuleCode, string organizationCountryRuleCode, string taxRecognitionCode)
		{
			var item = ruleCollection.AddNew();
			item.LoginCompanyCountryRuleCode = loginCompanyCountryRuleCode;
			item.OrganizationCountryRuleCode = organizationCountryRuleCode;
			item.TaxRecognitionCode = taxRecognitionCode;
		}

		public void TestCacheMainAddress()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress address = org.MainAddress;
			OrgAddress newMainAddress;
			using (((IOrgHeaderForMatching)org).CacheMainAddress())
			{
				AssertEquals("Precondition", address, org.MainAddress);

				newMainAddress = org.Addresses.AddNew();
				newMainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
				newMainAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

				Assert(!address.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
				Assert(newMainAddress.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
				AssertEquals("Should return cached value", address, org.MainAddress);
			}

			AssertNotEquals("Should return new value", address, org.MainAddress);
			AssertEquals("Should return new value", newMainAddress, org.MainAddress);
		}

		[ExpectNoExceptions]
		public void TestCountryDataDoesNotThrowShouldNotBeAccessingAPropertyOnADeletedBizO()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ZGuid countryDataPk = org.CountryData.PK;

			OrgCountryData countryData = Factory.Load<OrgCountryData>(countryDataPk);
			countryData.Delete();

			ZBool dummyBool = org.CountryData.OV_MakePartsBothImportAndExport;
		}

		[ExpectNoExceptions]
		public void TestRequiredFieldsForOrgDoesNotThrowShouldNotBeAccessingAPropertyOnADeletedBizO()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.Delete();

			ZBool dummyBool = org.RequiredFieldsForOrg.RequireBusinessNumber;
		}

		public void TestCustomsAddress()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "ORGCODE";
			var customsAddress = header.Addresses.AddNew();
			customsAddress.OA_Address1 = "KNZ Test";
			customsAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			var mainAddress = header.Addresses.AddNew();
			mainAddress.OA_Address1 = "main address";
			mainAddress.AddAddressType(OrgAddressType.Office);
			AssertNotNull(header.CustomsAddress);
			AssertEquals("KNZ Test", header.CustomsAddress.OA_Address1);
		}

		public void TestRelatedPartiesViews()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "ORGCODE";
			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_Code = "TESTPARTY";
			Factory.Save();

			var relatedParty1 = Factory.NewWithValidTestData<OrgRelatedParty>();
			relatedParty1.PR_OH_Parent = header.PK;
			relatedParty1.PR_OH_RelatedParty = header2.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			var relatedParty2 = Factory.NewWithValidTestData<OrgRelatedParty>();
			relatedParty2.PR_OH_Parent = header.PK;
			relatedParty2.PR_OH_RelatedParty = header2.PK;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			Factory.Save();

			AssertEquals(2, header.AllRelatedPartiesView.Count);
			AssertEquals(2, header2.AllParentPartiesView.Count);
		}

		public void TestCompanyDataInDifferentCompanyExistsInCollection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tempCompany = Factory.NewWithValidTestData<GlbCompany>();
			var tempBranch = Factory.NewWithValidTestData<GlbBranch>();
			tempBranch.GB_GC = tempCompany.PK;
			Factory.Save();

			org.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, tempBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var x = org.CompanyData; // invoke CompanyData getter
			}

			AssertNoExceptionThrown(() => org.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new[] { tempCompany.PK, GlbCompany.CurrentCompany.PK }, org.CompanyDataCollection.Cast<OrgCompanyData>().Select(x => x.OB_GC));
		}

		public void TestOrgFountainsAreReadonly()
		{
			var header = Factory.New<OrgHeader>();
			Assert(header.OrgFountains.ReadOnly);
		}

		public void TestDeleteOrgWithNullDefaultOrg()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = ZGuid.Empty;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertNull("PRE: The default org is actually null", OrgHeader.DefaultOrg);
				org.Delete();
			}
		}

		public void TestDeleteOrgWithOrgPatternMatch()
		{
			OrgHeader company1 = Factory.New<OrgHeader>();
			company1.OH_IsActive = false;
			company1.OH_FullName = "Company1";
			company1.OH_Code = "COM";

			OrgPatternMatchOverride orgPattern = Factory.New<OrgPatternMatchOverride>();
			orgPattern.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPattern.OO_OH = OrgHeader.DefaultOrg.PK;
			orgPattern.OO_LocalGuid = company1.PK;
			orgPattern.OO_LocalCode = "TestCom";
			orgPattern.OO_ForeignCode = "TestCom";

			OrgPatternMatchOverride orgPattern1 = Factory.New<OrgPatternMatchOverride>();
			orgPattern1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgPattern1.OO_OH = OrgHeader.DefaultOrg.PK;
			orgPattern1.OO_LocalGuid = ZGuid.NewZGuid();
			orgPattern1.OO_LocalCode = "TestCom";
			orgPattern1.OO_ForeignCode = "TestCom1";
			Factory.Save();

			var pk = orgPattern.PK;
			var pk1 = orgPattern1.PK;

			AssertNotNull("Related OrgPatternMatchOverride should have been generated", Factory.Load<OrgPatternMatchOverride>(pk));
			AssertNotNull("OrgPatternMatchOverride with same localcode should have been generated", Factory.Load<OrgPatternMatchOverride>(pk1));

			company1.Delete();
			Factory.Save();

			AssertNull("Related OrgPatternMatchOverride should have been deleted", Factory.Load<OrgPatternMatchOverride>(pk));
			AssertNotNull("OrgPatternMatchOverride with same localcode should not be deleted", Factory.Load<OrgPatternMatchOverride>(pk1));
		}

		#region TestGetMatchingNumberRange

		public void TestGetMatchingNumberRange()
		{
			var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("C1");
			var warehouse = helper.CreateWarehouse("whs", "A").PK;

			var stmNums1 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<OrganisationViewStmNums>(org, "1111111");
			var stmNums2 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<OrganisationViewStmNums>(org, "2222222");
			var stmNums3 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<OrganisationViewStmNums>(org, "3333333");
			StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(org, stmNums1.SN_Type, stmNums1.SN_Prefix, client: null, warehouse: null);
			StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(org, stmNums2.SN_Type, stmNums2.SN_Prefix, client: null, warehouse: warehouse);
			StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(org, stmNums3.SN_Type, stmNums3.SN_Prefix, client: client, warehouse: null);
			Factory.Save();

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				AssertEquals("Should find with no client and warehouse when no ranking condition",
					"111111100000001", org.GetMatchingNumberRange(type).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var rankerMatchWarehouse = new List<ColumnValueRanker.ColumnValuesPair>();
				rankerMatchWarehouse.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_WW_Whs, warehouse));
				AssertEquals("222222200000001", org.GetMatchingNumberRange(type, rankerMatchWarehouse).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var rankerMatchClient = new List<ColumnValueRanker.ColumnValuesPair>();
				rankerMatchClient.Add(StmNumberRangeMatchingDetail.MatchWithValueOrNull(StmNumberRangeMatchingDetailSchema.NRM_OH_Client, client));
				AssertEquals("333333300000001", org.GetMatchingNumberRange(type, rankerMatchClient).LinkedFountain.TryGetNumberFountain().GetNextFormatted(Factory));

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				AssertNull("Should not return value for different organisation", org2.GetMatchingNumberRange(type));
			}
		}

		#endregion

		#region TestNumberRangeMatchingDetails

		public void TestNumberRangeMatchingDetails()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("C1");

			var stmNums1 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<OrganisationViewStmNums>(org, "1111111");
			var stmNums2 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<OrganisationViewStmNums>(org2, "2222222");
			var stmNums3 = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<OrganisationViewStmNums>(org, "3333333");
			var matchingDetails1 = StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(org, type, stmNums1.SN_Prefix, client: null, warehouse: null);
			var matchingDetails2 = StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(org2, type, stmNums2.SN_Prefix, client: null, warehouse: null);
			var matchingDetails3 = StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(org, type, stmNums3.SN_Prefix, client: client, warehouse: null);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new StmNumberRangeMatchingDetail[] { matchingDetails1, matchingDetails3 }, org.NumberRangeMatchingDetails);
		}

		#endregion

		#region TestDeleteOrgWillDeleteNumberRangeMatchingDetails

		public void TestDeleteOrgWillDeleteNumberRangeMatchingDetails()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("C1");

			var stmNums = StmNumberRangeMatchingDetailsTest.CreateViewStmNums<OrganisationViewStmNums>(org, "1111111");
			var matchingDetails = StmNumberRangeMatchingDetailsTest.CreateStmNumberRangeMatchingDetails(org, type, stmNums.SN_Prefix, client: null, warehouse: null);
			Factory.Save();

			AssertEquals("Precondition", false, matchingDetails.IsDeleted);
			org.Delete();

			AssertEquals("Delete organisation should delete matching detail.", true, matchingDetails.IsDeleted);
		}

		#endregion

		#region TestHasModifyConfigSecurity

		public void TestHasModifyConfigSecurity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			Env.Security.OrgConfigModify.IsAllowed = false;
			AssertEquals("Should NOT be allowed", true, org.NumberRangeMatchingDetails.ReadOnly);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Env.Security.OrgConfigModify.IsAllowed = true;
			AssertEquals("Should be allowed", false, org2.NumberRangeMatchingDetails.ReadOnly);
		}

		#endregion

		[ExpectExceptionMessage(typeof(RegistryValidationException), "You cannot enter an item with no Description.")]
		public void TestOrgStaffMemberAssignmentRoles_ShouldNotAllowRoleWithEmptyDescription()
		{
			Env.Registry.OrgStaffMemberAssignmentRoles = new CodeDescriptionPairList
				{
					new CodeDescriptionPair("ABC", "This is ABC role"),
					new CodeDescriptionPair("DEF", string.Empty)
				};
		}

		#region UNLOCO

		public void TestStateIsSetOnlyForMBERule()
		{
			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			OrgHeader org = Factory.New<OrgHeader>();

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			org.OH_RL_NKClosestPort = "AUMEL";
			AssertEquals(ZString.Empty, org.MainAddress.OA_State);

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("NSW", org.MainAddress.OA_State);
		}

		[ExpectNoExceptions]
		public void TestEmptyUNLOCOonMainAddressDoesntLeadToNRE()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "CNSHA";
			orgHeader.OH_IsGlobalAccount = true;
			orgHeader.OH_IsShippingProvider = true;
			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "";
			mainAddress.OA_RN_NKCountryCode = "CN";
			mainAddress.OA_Address1 = Guid.NewGuid().ToString();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var headerFreshLoaded = anotherFactory.Load<OrgHeader>(orgHeader.PK);
			//preload objects to factory to make the cachedProperty behave the same as when issue happens
			var capabilities = anotherFactory.Load<OrgAddressCapability>(new ZQuery(OrgAddressCapabilitySchema.PZ_OA, mainAddress.PK));
			var country = anotherFactory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			{
				var address = headerFreshLoaded.MainAddress;
				AssertEquals(mainAddress.OA_Address1, address.OA_Address1);
			}
		}

		#endregion

		#region Last Quoted Date

		public void TestLastQuotedDate()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			BusinessObject quote1 = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			quote1[RatingHeaderSchema.TH_QuoteNumber] = "00221000";
			quote1[RatingHeaderSchema.TH_OH] = org.PK;
			quote1[RatingHeaderSchema.TH_QuoteDate] = new ZDateTime(2011, 5, 1);

			BusinessObject quote2 = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			quote2[RatingHeaderSchema.TH_QuoteNumber] = "00221001";
			quote2[RatingHeaderSchema.TH_OH] = org.PK;
			quote2[RatingHeaderSchema.TH_QuoteDate] = new ZDateTime(2011, 6, 16);

			Factory.Save();
			AssertEquals(new ZDateTime(2011, 6, 16), org.LastQuotedDate);

			BusinessObject quote3 = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			quote3[RatingHeaderSchema.TH_QuoteNumber] = "00221002";
			quote3[RatingHeaderSchema.TH_OH] = org.PK;
			quote3[RatingHeaderSchema.TH_QuoteDate] = new ZDateTime(2011, 8, 23);

			Factory.Save();
			AssertEquals(new ZDateTime(2011, 8, 23), org.LastQuotedDate);
		}

		#endregion

		#region IWorkflowProvider

		public void TestIWorkflowProvider()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			IWorkflowProvider workflowProvider = orgHeader;

			AssertNotNull(workflowProvider);
			AssertEquals(orgHeader.PK, workflowProvider.PK);
			AssertEquals(new OrgHeaderWorkflowDescriptor().Code, workflowProvider.WorkflowType);
			AssertEquals(typeof(OrgHeaderProcessTasksCollection), workflowProvider.WorkflowItems.GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestCreateWorkflowTasksOnSave()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			ProcessTaskTemplate workflowTemplate = new BusinessObjectFactory().NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = orgHeader.WorkflowItems.WorkflowType;
			ProcessTask task = workflowTemplate.WorkflowItems.AddNew();
			ProcessTask milestone = workflowTemplate.WorkflowItems.AddNew();
			milestone.IsMilestone = true;
			workflowTemplate.Factory.Save();

			AssertEquals("No milestones initially", 0, orgHeader.WorkflowItems.Milestones.Count);
			AssertEquals("No tasks initially", 0, orgHeader.WorkflowItems.Tasks.Count);

			Factory.Save();

			AssertEquals("1 milestone added", 1, orgHeader.WorkflowItems.Milestones.Count);
			AssertEquals("1 task added", 1, orgHeader.WorkflowItems.Tasks.Count);
		}

		public void TestCustomPropertiesContainStaffAssignmentRoles()
		{
			ICustomPropertyContainer orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var staffAssignmentRoles = Env.Registry.OrgStaffMemberAssignmentRoles;
			var roleNames = staffAssignmentRoles.ToList<CodeDescriptionPair>().ToArray().Select(cd => cd.Description).ToArray();
			var propertyNames = orgHeader.CustomProperties.Select(cp => cp.Identifier).ToArray();

			foreach (var roleName in roleNames)
			{
				AssertCollectionContains("Each role name should become a custom property of OrgHeader.", $"Staff - {roleName}", propertyNames);
			}
		}

		public void TestCustomPropertiesContainCompetitorTypes()
		{
			ICustomPropertyContainer orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var competitorTypes = OrganisationsDataRegistry.Instance.CompetitorType.Value.GetActiveCodeDescriptionPairList();
			var types = competitorTypes.ToList<CodeDescriptionPair>().ToArray().Select(cd => cd.Description).ToArray();
			var propertyNames = orgHeader.CustomProperties.Select(cp => cp.Identifier).ToArray();

			foreach (var type in types)
			{
				AssertCollectionContains("Each competitor type should become a custom property of OrgHeader.", $"Competitor on {type}", propertyNames);
			}
		}

		public void TestCustomRolePropertiesHaveTheCorrectValues()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			Factory.Save();

			var staffAssignment = orgHeader.StaffAssignments.AddNew();
			var staffAssignmentRoles = new StaffAssignmentRoles();
			staffAssignment.O8_Role = staffAssignmentRoles[0].Code;
			staffAssignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			staffAssignment.O8_Department = "ALL";
			Factory.Save();

			var roleProperty1 = ((ICustomPropertyContainer)orgHeader).CustomProperties.First(cp => cp.Identifier == $"Staff - {staffAssignmentRoles[0].Description}");
			var roleProperty2 = ((ICustomPropertyContainer)orgHeader).CustomProperties.First(cp => cp.Identifier == $"Staff - {staffAssignmentRoles[1].Description}");
			var role1 = roleProperty1.GetValue(orgHeader).ToString();
			var role2 = roleProperty2.GetValue(orgHeader).ToString();

			AssertEquals(staff.GS_Code, role1);
			AssertEquals(string.Empty, role2);
		}

		public void TestCustomCompetitorTypePropertiesHaveTheCorrectValues()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var competitorOH1 = Factory.NewWithValidTestData<OrgHeader>();
			competitorOH1.OH_Code = "oh001";
			var competitorOH2 = Factory.NewWithValidTestData<OrgHeader>();
			competitorOH2.OH_Code = "oh002";

			var competitorTypes = OrganisationsDataRegistry.Instance.CompetitorType.Value.GetActiveCodeDescriptionPairList();
			var competitor = orgHeader.Competitors.AddNew();
			competitor.OCP_Type = competitorTypes[0].Code;
			competitor.OCP_OH_Competitor = competitorOH1.PK;

			var competitor2 = orgHeader.Competitors.AddNew();
			competitor2.OCP_Type = competitorTypes[1].Code;
			competitor2.OCP_OH_Competitor = competitorOH1.PK;
			var competitor3 = orgHeader.Competitors.AddNew();
			competitor3.OCP_Type = competitorTypes[1].Code;
			competitor3.OCP_OH_Competitor = competitorOH2.PK;
			Factory.Save();

			var competitorProperty1 = ((ICustomPropertyContainer)orgHeader).CustomProperties.First(cp => cp.Identifier == $"Competitor on {competitorTypes[0].Description}");
			var competitorProperty2 = ((ICustomPropertyContainer)orgHeader).CustomProperties.First(cp => cp.Identifier == $"Competitor on {competitorTypes[1].Description}");
			var competitorProperty3 = ((ICustomPropertyContainer)orgHeader).CustomProperties.First(cp => cp.Identifier == $"Competitor on {competitorTypes[2].Description}");
			var type1 = competitorProperty1.GetValue(orgHeader).ToString();
			var type2 = competitorProperty2.GetValue(orgHeader).ToString();
			var type3 = competitorProperty3.GetValue(orgHeader).ToString();

			AssertEquals(competitorOH1.OH_Code, type1);
			AssertEquals(string.Join(", ", new string[] { competitorOH1.OH_Code, competitorOH2.OH_Code }), type2);
			AssertEquals(string.Empty, type3);
		}

		public void TestNullPersonResponsibleOfStaffAssignmentShouldNotBeReported()
		{
			var invalidStaffCode = "ABC";
			AssertEquals("Precondition: ", false, Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, invalidStaffCode)).Any());

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var staffAssignment = orgHeader.StaffAssignments.AddNew();
			staffAssignment.O8_Role = new StaffAssignmentRoles()[0].Code;
			staffAssignment.O8_Department = "ALL";
			staffAssignment.O8_GS_NKPersonResponsible = invalidStaffCode;
			Factory.Save();

			var roleProperty = ((ICustomPropertyContainer)orgHeader).CustomProperties.First(cp => cp.Identifier == $"Staff - {new StaffAssignmentRoles()[0].Description}");
			AssertNoExceptionThrown(() => roleProperty.GetValue(orgHeader));
		}

		#endregion

		#region Accessing Deleted Business Object Tests

		public void TestBranchForCurrentUNLOCODoNotReturnDeletedGlbBranch()
		{
			var org = Factory.NewWithValidTestData<OrgHeaderForTest>();
			org.OH_RL_NKClosestPort = "AUMEL";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = "Melbourne Branch";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_RL_NKHomePort = "AUMEL";
			Factory.Save();

			AssertEquals(branch1.GB_BranchName, org.BranchForCurrentUNLOCOExposed.GB_BranchName);

			branch1.Delete();
			Assert(branch1.IsDeleted);

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_BranchName = "Geelong Branch";
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_RL_NKHomePort = "AUMEL";
			Factory.Save();

			ErrorReporter.Clear();
			var resultBranchName = org.BranchForCurrentUNLOCOExposed.GB_BranchName;
			AssertNull(ErrorReporter.LastExceptionReported);
			AssertEquals(branch2.GB_BranchName, resultBranchName);

			branch2.Delete();
			Assert(branch2.IsDeleted);

			AssertNull(org.BranchForCurrentUNLOCOExposed);
		}

		public void TestCodeForTaxRegistrationDoNotReturnDeletedOrgCusCode()
		{
			var originalCurrentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var aU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = aU.Code;
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var customCode1 = AddCustomsCodeForTest(organisation, aU.Code, "12004044937", "ABN");
				Factory.Save();

				AssertEquals(aU.Code, organisation.CountryOfTaxRegistration.Code);
				AssertEquals("AU12004044937", organisation.TaxRegistrationNumber);
				AssertEquals("12004044937", organisation.RawTaxRegistrationNumber);

				customCode1.Delete();
				Assert(customCode1.IsDeleted);

				ErrorReporter.Clear();
				var country = organisation.CountryOfTaxRegistration;
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(aU.Code, country.Code);
				AssertEquals("AU", organisation.TaxRegistrationNumber);
				AssertEquals(ZString.Empty, organisation.RawTaxRegistrationNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCurrentCountry;
			}
		}

		public void TestCustomsAddressDoNotReturnDeletedOrgAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsAddress1 = org.Addresses.AddNew();
			customsAddress1.OA_Address1 = "Sydney";
			customsAddress1.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			AssertEquals(customsAddress1.OA_Address1, org.CustomsAddress.OA_Address1);

			customsAddress1.Delete();
			Assert(customsAddress1.IsDeleted);

			var customsAddress2 = org.Addresses.AddNew();
			customsAddress2.OA_Address1 = "Perth";
			customsAddress2.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			ErrorReporter.Clear();
			var resultAddress = org.CustomsAddress.OA_Address1;
			AssertNull(ErrorReporter.LastExceptionReported);
			AssertEquals(customsAddress2.OA_Address1, resultAddress);

			customsAddress2.Delete();
			Assert(customsAddress2.IsDeleted);

			AssertNull(org.CustomsAddress);
		}

		public void TestMainAddressDoNotReturnDeletedOrgAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = org.Addresses.MainAddress;
			mainAddress.OA_Address1 = "Sydney";
			Factory.Save();

			using (((IOrgHeaderForMatching)org).CacheMainAddress())
			{
				AssertEquals(mainAddress.OA_Address1, org.MainAddress.OA_Address1);

				mainAddress.Delete();
				Assert(mainAddress.IsDeleted);

				ErrorReporter.Clear();
				var resultAddress = org.MainAddress.OA_Address1;
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(ZString.Empty, resultAddress);
			}
		}

		#endregion

		public void TestCreateTempOrgAddressFromOrgTranslatedAddressDoNotThrowRowNotInTableException()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Language = Constants.Languages.English;
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_Address1 = "Bourke Street";

			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Language = Constants.Languages.French;
			translatedAddress.OTA_Address1 = "Bourke Rue";

			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			{
				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.French))
				{
					var temporaryAddress = org.MainAddress;
					AssertEquals(translatedAddress.OTA_Address1, temporaryAddress.OA_Address1);

					temporaryAddress.Delete();
					Assert(temporaryAddress.IsDeleted);

					AssertNoExceptionThrown(() => temporaryAddress = org.MainAddress);
					AssertEquals(translatedAddress.OTA_Address1, temporaryAddress.OA_Address1);
				}
			}
		}

		public void TestNoInfiniteLoopWhenIsGeneratingDocumentAndTranslateAddressExist()
		{
			var factory = new FactoryForLoopTest();
			var orgHeader = factory.NewWithValidTestData<OrgHeaderForLoopTest>();
			orgHeader.OH_IsGlobalAccount = true;
			var address = orgHeader.MainAddress;
			var transAddress = address.TranslatedAddresses.AddNew();
			transAddress.OTA_Address1 = "AAAA";
			transAddress.OTA_Language = Constants.Languages.ChineseSimplified;
			factory.Save();

			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			using (Res.TemporarilySwitchLanguage(Constants.Languages.ChineseSimplified))
			{
				try
				{
					OrgHeaderForLoopTest.throwExceptionWhenTranslatedAddressLoopOccurForTest = true;
					OrgHeaderForLoopTest.translatedAddressLoopTimesForTest = 0;
					AssertNoExceptionThrown(() => _ = address.CountryName);
				}
				finally
				{
					OrgHeaderForLoopTest.throwExceptionWhenTranslatedAddressLoopOccurForTest = false;
					OrgHeaderForLoopTest.translatedAddressLoopTimesForTest = 0;
				}
			}
		}

		#region ValidateDuplicateResult Tests

		public void TestValidateDuplicateResult()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCD";

			org.ValidateDuplicationResult(true);
			AssertHasWarning(org.OH_FullNameInfo, "The name you have entered resulted in potential duplicates. Please confirm that they are actual duplicates.");

			org.ValidateDuplicationResult(false);
			AssertNoWarnings(org.OH_FullNameInfo);
		}

		[ExpectNoExceptions]
		public void TestValidateDuplicateResult_WithMultiThreads()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCD";

			string expectedException = string.Empty;
			var task = Task.WhenAll(Task.Run(() =>
			{
				try
				{
					org.ValidateDuplicationResult(false);
				}
				catch (Exception ex)
				{
					expectedException = ex.Message;
				}
			}), Task.Run(() =>
			{
				org.ValidateDuplicationResult(true);
			}));

			task.Wait();
		}

		#endregion

		public void TestDeleteOrgWillDeletePatternMatching()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var orgPatterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, org);
			var orgResults = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingResults(Factory, org);

			Factory.Save();
			org.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Org should be deleted", true, org.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Org patterns should all be deleted", Array.Empty<BusinessObject>(), orgPatterns.Where(p => !p.IsDeleted));
				AssertContainsExactElementsInAnyOrder("Org pattern results should all be deleted", Array.Empty<BusinessObject>(), orgResults.Where(p => !p.IsDeleted));
			});
		}

		public void TestContainerYardRelatedCarrierAppointedAgentPorts()
		{
			var orgContainerYard = Factory.NewWithValidTestData<OrgHeader>();
			orgContainerYard.OH_IsContainerYard = true;
			orgContainerYard.OH_Code = "CY";
			orgContainerYard.OH_FullName = "Container Yard";

			AssertEquals(0, orgContainerYard.ContainerYardRelatedCarrierAppointedAgentPorts.Count);

			Factory.Save();

			var orgCarrier = Factory.NewWithValidTestData<OrgHeader>();
			orgCarrier.OH_IsShippingProvider = true;
			orgCarrier.OH_FullName = "Carrier 1";
			orgCarrier.OH_Code = "CR1";
			var carrierAppointedPort = orgCarrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			carrierAppointedPort.O5_PortOrCountry = "AUMEL";
			carrierAppointedPort.OrganisationPK = orgContainerYard.PK;
			carrierAppointedPort.O5_OA_AgentOfficeAddress = orgContainerYard.MainAddress.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			orgContainerYard = newFactory.Load<OrgHeader>(orgContainerYard.PK);

			AssertEquals(1, orgContainerYard.ContainerYardRelatedCarrierAppointedAgentPorts.Count);
			AssertEquals(true, orgContainerYard.ContainerYardRelatedCarrierAppointedAgentPorts[0].ReadOnly);
		}

		public void TestGetFailingCheckpointModifyRegistrationNumber()
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
				org.OH_IsDebtor = true;
				org.OH_IsCreditor = true;
				AssertEquals(Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, org.GetFailingCheckpointWhenModifyRegistrationNumber(true));

				org.OH_IsDebtor = false;
				org.OH_IsCreditor = false;
				AssertEquals(Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers, org.GetFailingCheckpointWhenModifyRegistrationNumber(true));

				org.OH_IsDebtor = true;
				org.OH_IsCreditor = true;
				AssertEquals(Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers, org.GetFailingCheckpointWhenModifyRegistrationNumber(false));

				org.OH_IsDebtor = false;
				org.OH_IsCreditor = false;
				AssertEquals(Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers, org.GetFailingCheckpointWhenModifyRegistrationNumber(false));

				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = true;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = true;
				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = true;

				org.OH_IsDebtor = true;
				org.OH_IsCreditor = true;
				AssertNull(org.GetFailingCheckpointWhenModifyRegistrationNumber(true));

				org.OH_IsDebtor = false;
				org.OH_IsCreditor = false;
				AssertNull(org.GetFailingCheckpointWhenModifyRegistrationNumber(true));

				org.OH_IsDebtor = true;
				org.OH_IsCreditor = true;
				AssertNull(org.GetFailingCheckpointWhenModifyRegistrationNumber(false));

				org.OH_IsDebtor = false;
				org.OH_IsCreditor = false;
				AssertNull(org.GetFailingCheckpointWhenModifyRegistrationNumber(false));
			}
			finally
			{
				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = oldSecurityModifyFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyFinancialNonARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialNonARAPRegistrationNumbers;
			}
		}

		#region TestGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumber

		public void TestGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumber_CurrentCompany()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var currentCompanyData = orgHeader.CompanyData;
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, true, Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers, string.Empty, string.Empty);
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, false, Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers, string.Empty, string.Empty);

			currentCompanyData.OB_IsDebtor = true;
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, "A/R", currentCompanyData.Company.GC_Code);

			currentCompanyData.OB_IsCreditor = true;
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, false, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers, "A/R and A/P", currentCompanyData.Company.GC_Code);

			currentCompanyData.OB_IsDebtor = false;
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, false, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers, "A/P", currentCompanyData.Company.GC_Code);
		}

		public void TestGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumber_OtherCompany()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "MDM";
			var otherCompanyData = Factory.New<OrgCompanyData>();
			otherCompanyData.OB_GC = otherCompany.PK;
			otherCompanyData.OB_OH = orgHeader.PK;

			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, true, Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers, string.Empty, string.Empty);
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, false, Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers, string.Empty, string.Empty);

			otherCompanyData.OB_IsDebtor = true;
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, "A/R", "MDM");

			otherCompanyData.OB_IsCreditor = true;
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, false, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers, "A/R and A/P", "MDM");

			otherCompanyData.OB_IsDebtor = false;
			AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(orgHeader, false, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers, "A/P", "MDM");
		}

		void AssertGetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumberResult(OrgHeader orgHeader, bool isPrimaryCode, ISecurityCheckpoint securityCheckpoint, string arAPInfo, string code)
		{
			securityCheckpoint.IsAllowed = false;
			var result = orgHeader.GetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumber(isPrimaryCode);
			AssertEquals(securityCheckpoint, result.FailingCheckpoint);

			if (!string.IsNullOrEmpty(arAPInfo))
			{
				AssertEquals($"This organization is marked as {arAPInfo} under Company {code}.", result.CompanyARAPInfo);
			}
			else
			{
				AssertNullOrEmpty(result.CompanyARAPInfo);
			}

			securityCheckpoint.IsAllowed = true;
			result = orgHeader.GetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumber(isPrimaryCode);
			AssertNull(result.FailingCheckpoint);
		}

		#endregion

		#region TestGetFailingCheckpointWhenModifyRegistrationNumber

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompanyNotARAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompany_IsAROrAP(
				false, false, Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompanyIsAR()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompany_IsAROrAP(
				false, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompanyIsAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompany_IsAROrAP(
				true, false, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompanyIsARAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompany_IsAROrAP(
				true, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		void TestGetFailingCheckpointWhenModifyRegistrationNumber_CurrentCompany_IsAROrAP(
			bool isAP, bool isAR, SecurityCheckpoint checkpointForFinancial, SecurityCheckpoint checkpointForNonFinancial)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var originalModifyFinancialSecurity = checkpointForFinancial.IsAllowed;
			var originalModifyNonFinancialSecurity = checkpointForNonFinancial.IsAllowed;
			try
			{
				checkpointForNonFinancial.IsAllowed = false;
				checkpointForFinancial.IsAllowed = false;
				var orgDataForCurrentCompany = Factory.NewWithValidTestData<OrgCompanyData>();
				orgDataForCurrentCompany.OB_OH = orgHeader.PK;
				orgDataForCurrentCompany.OB_GC = GlbCompany.CurrentCompany.PK;
				orgDataForCurrentCompany.OB_IsDebtor = isAR;
				orgDataForCurrentCompany.OB_IsCreditor = isAP;

				var result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber(true);
				AssertEquals(checkpointForFinancial, result);

				result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber();
				AssertEquals(checkpointForNonFinancial, result);
			}
			finally
			{
				checkpointForFinancial.IsAllowed = originalModifyFinancialSecurity;
				checkpointForNonFinancial.IsAllowed = originalModifyNonFinancialSecurity;
			}
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompanyNotARAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompany_IsAROrAP(
				false, false, Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompanyIsAR()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompany_IsAROrAP(
				false, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompanyIsAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompany_IsAROrAP(
				true, false, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompanyIsARAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompany_IsAROrAP(
				true, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		void TestGetFailingCheckpointWhenModifyRegistrationNumber_OtherCompany_IsAROrAP(
			bool isAP, bool isAR, SecurityCheckpoint checkpointForFinancial, SecurityCheckpoint checkpointForNonFinancial)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var originalModifyFinancialSecurity = checkpointForFinancial.IsAllowed;
			var originalModifyNonFinancialSecurity = checkpointForNonFinancial.IsAllowed;
			try
			{
				var otherCompany = Factory.New<GlbCompany>();
				checkpointForFinancial.IsAllowed = false;
				checkpointForNonFinancial.IsAllowed = false;
				var orgDataForOtherCompany = Factory.New<OrgCompanyData>();
				orgDataForOtherCompany.OB_OH = orgHeader.PK;
				orgDataForOtherCompany.OB_IsDebtor = isAR;
				orgDataForOtherCompany.OB_IsCreditor = isAP;
				orgDataForOtherCompany.OB_GC = otherCompany.PK;

				var result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber(true);
				AssertEquals(checkpointForFinancial, result);

				result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber();
				AssertEquals(checkpointForNonFinancial, result);
			}
			finally
			{
				checkpointForFinancial.IsAllowed = originalModifyFinancialSecurity;
				checkpointForNonFinancial.IsAllowed = originalModifyNonFinancialSecurity;
			}
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_NeitherCompanyIsARAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_BothCompany_IsAROrAP(
				false, false, Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_EitherCompanyIsAR()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_EitherCompany_IsAROrAP(
				false, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_EitherCompanyIsAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_EitherCompany_IsAROrAP(
				true, false, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_EitherCompanyIsARAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_EitherCompany_IsAROrAP(
				true, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		void TestGetFailingCheckpointWhenModifyRegistrationNumber_EitherCompany_IsAROrAP(
			bool isAP, bool isAR, SecurityCheckpoint checkpointForFinancial, SecurityCheckpoint checkpointForNonFinancial)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var originalModifyFinancialSecurity = checkpointForFinancial.IsAllowed;
			var originalModifyNonFinancialSecurity = checkpointForNonFinancial.IsAllowed;
			try
			{
				checkpointForFinancial.IsAllowed = false;
				checkpointForNonFinancial.IsAllowed = false;
				var orgDataForCurrentCompany = Factory.NewWithValidTestData<OrgCompanyData>();
				orgDataForCurrentCompany.OB_GC = GlbCompany.CurrentCompany.PK;
				orgDataForCurrentCompany.OB_OH = orgHeader.PK;
				orgDataForCurrentCompany.OB_IsDebtor = isAR;
				orgDataForCurrentCompany.OB_IsCreditor = isAP;
				var orgDataForOtherCompany = Factory.NewWithValidTestData<OrgCompanyData>();
				orgDataForOtherCompany.OB_OH = orgHeader.PK;
				orgDataForOtherCompany.OB_IsDebtor = false;
				orgDataForOtherCompany.OB_IsCreditor = false;

				var result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber(true);
				AssertEquals(checkpointForFinancial, result);

				result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber();
				AssertEquals(checkpointForNonFinancial, result);

				orgDataForCurrentCompany.OB_IsDebtor = false;
				orgDataForCurrentCompany.OB_IsCreditor = false;
				orgDataForOtherCompany.OB_IsDebtor = isAR;
				orgDataForOtherCompany.OB_IsCreditor = isAP;

				result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber(true);
				AssertEquals(checkpointForFinancial, result);

				result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber();
				AssertEquals(checkpointForNonFinancial, result);
			}
			finally
			{
				checkpointForFinancial.IsAllowed = originalModifyFinancialSecurity;
				checkpointForNonFinancial.IsAllowed = originalModifyNonFinancialSecurity;
			}
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_BothCompanyIsAR()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_BothCompany_IsAROrAP(
				false, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_BothCompanyIsAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_BothCompany_IsAROrAP(
				true, false, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		public void TestGetFailingCheckpointWhenModifyRegistrationNumber_BothCompanyIsARAP()
		{
			TestGetFailingCheckpointWhenModifyRegistrationNumber_BothCompany_IsAROrAP(
				true, true, Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers);
		}

		void TestGetFailingCheckpointWhenModifyRegistrationNumber_BothCompany_IsAROrAP(
			bool isAP, bool isAR, SecurityCheckpoint checkpointForFinancial, SecurityCheckpoint checkpointForNonFinancial)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var originalModifyFinancialSecurity = checkpointForFinancial.IsAllowed;
			var originalModifyNonFinancialSecurity = checkpointForNonFinancial.IsAllowed;
			try
			{
				checkpointForFinancial.IsAllowed = false;
				checkpointForNonFinancial.IsAllowed = false;
				var orgDataForCurrentCompany = Factory.NewWithValidTestData<OrgCompanyData>();
				orgDataForCurrentCompany.OB_GC = GlbCompany.CurrentCompany.PK;
				orgDataForCurrentCompany.OB_OH = orgHeader.PK;
				orgDataForCurrentCompany.OB_IsDebtor = isAR;
				orgDataForCurrentCompany.OB_IsCreditor = isAP;
				var orgDataForOtherCompany = Factory.NewWithValidTestData<OrgCompanyData>();
				orgDataForOtherCompany.OB_OH = orgHeader.PK;
				orgDataForOtherCompany.OB_IsDebtor = isAR;
				orgDataForOtherCompany.OB_IsCreditor = isAP;

				var result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber(true);
				AssertEquals(checkpointForFinancial, result);

				result = orgHeader.GetFailingCheckpointWhenModifyRegistrationNumber();
				AssertEquals(checkpointForNonFinancial, result);
			}
			finally
			{
				checkpointForFinancial.IsAllowed = originalModifyFinancialSecurity;
				checkpointForNonFinancial.IsAllowed = originalModifyNonFinancialSecurity;
			}
		}

		#endregion

		public void TestPowerOfAttorneyValidToDate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var document = org.RequiredDocuments.AddNew();
			document.EQ_DocType = Constants.RefDocTypes.PowerOfAttorney;
			document.EQ_ValidToDate = ZDateTime.BrettsBirthday;

			AssertEquals(ZDateTime.BrettsBirthday, org.PowerOfAttorneyValidToDate);
		}

		public void TestPowerOfAttorneyValidToDate_EmptyValidDate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var document = org.RequiredDocuments.AddNew();
			document.EQ_DocType = Constants.RefDocTypes.PowerOfAttorney;

			AssertEquals(ZDateTime.Empty, org.PowerOfAttorneyValidToDate);
		}

		public void TestPowerOfAttorneyValidToDate_MissingPowerOfAttorneyDocument()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var document = org.RequiredDocuments.AddNew();
			document.EQ_DocType = Constants.RefDocTypes.AgentsInstruction;

			AssertEquals(ZDateTime.Empty, org.PowerOfAttorneyValidToDate);
		}

		public void TestPowerOfAttorneyValidToDate_MultiplePowerOfAttorneyDocuments()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var document = org.RequiredDocuments.AddNew();
			document.EQ_DocType = Constants.RefDocTypes.PowerOfAttorney;
			document.EQ_ValidToDate = ZDateTime.BrettsBirthday;

			var document2 = org.RequiredDocuments.AddNew();
			document2.EQ_DocType = Constants.RefDocTypes.PowerOfAttorney;
			document2.EQ_ValidToDate = ZDateTime.BrettsBirthday.AddDays(-1);

			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-1), org.PowerOfAttorneyValidToDate);
		}

		public void TestEmployerIdentificationNumber()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CustomsRegNo = "11-1234567";
			cusCode.OK_OH = header.PK;

			AssertEquals("11-1234567", header.EmployerIdentificationNumber);
		}

		public void TestEmployerIdentificationNumber_NoEINCusCode()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			AssertEquals(string.Empty, header.EmployerIdentificationNumber);
		}

		public void TestSetDefaultOrgSecurities()
		{
			var profiles = new OrgSecurityProfileCollection();
			var profile = profiles.AddNew();
			profile.Default = true;
			profile.Name = "Profile1";
			profile.OrgSecuritySettings.PopulateDefaultSettings();
			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = true; x.CustomerManaged = true; });
			AssertEquals(true, profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().All(x => x.CustomerManaged && x.Granted));
			AssertEquals(true, profile.OrgSecuritySettings.Any<OrgSecurityProfileSetting>());
			var securities = new HashSet<ZString>(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().Select(x => x.SecurityKey));

			OrganisationRegistry.Instance.WebSecurityDefaultValues.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, profiles);

			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "a@cw1.com";
			contact.OC_WebAccessEnabled = true;

			AssertEquals(true, org.SecurityRightsView.OfType<OrgSecurity>().Where(x => securities.Contains(x.SecurityKey)).All(x => x.OX_Granted && x.OX_IsCustomerManaged));
			AssertEquals(true, contact.SecurityRightsView.OfType<OrgSecurityContacts>().Where(x => securities.Contains(x.Security.SecurityKey)).All(x => x.OZ_Granted));

			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = false; x.CustomerManaged = false; });
			AssertEquals(true, profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().All(x => !x.CustomerManaged && !x.Granted));

			OrganisationRegistry.Instance.WebSecurityDefaultValues.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, profiles);

			org = Factory.New<OrgHeader>();
			contact = org.Contacts.AddNew();
			contact.OC_Email = "b@cw1.com";
			contact.OC_WebAccessEnabled = true;

			AssertEquals(true, org.SecurityRightsView.OfType<OrgSecurity>().Where(x => securities.Contains(x.SecurityKey)).All(x => !x.OX_Granted && !x.OX_IsCustomerManaged));
			AssertEquals(true, contact.SecurityRightsView.OfType<OrgSecurityContacts>().Where(x => securities.Contains(x.Security.SecurityKey)).All(x => !x.OZ_Granted));
		}

		public void TestSyncWebSecurityToNeoGroup()
		{
			var creator = new MasterFilesTestHelper(Factory);
			var orgs = Enumerable.Range(1, 6)
				.Select(x =>
				{
					var orgCode = $"OH_{x}";
					var org = creator.CreateOrganisation(orgCode, creator.AUSYD.Code);
					org.OH_Code = orgCode;

					foreach (var idx in Enumerable.Range(1, 2))
					{
						var contactName = $"OC_{x}_{idx}";
						var contact = creator.GetNewContact(org, contactName, "");
						contact.OC_Email = $"{contactName}@cw.com";
						contact.OC_WebAccessEnabled = contact.OC_IsActive = true;
					}
					return org;
				}).ToArray();

			Factory.Save();

			void setOrgSecurity(OrgHeader org, string securityItemName, bool granted, bool isReport = false)
			{
				if (isReport)
				{
					var stmMenuItemPK = Db.Connection.ExecuteScalar<Guid>($"SELECT GU_ItemGUID FROM dbo.GlbSecurity WHERE GU_GG = (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = '{securityItemName}')");
					org.SecurityRights.OfType<OrgSecurity>().Single(x => x.OX_SU == stmMenuItemPK).OX_Granted = granted;
				}
				else
				{
					org.SecurityRights.OfType<OrgSecurity>().Single(x => x.OX_SecurityItemName == securityItemName).OX_Granted = granted;
				}
			}

			void setOrgSecurityContacts(OrgContact contact, string securityItemName, bool granted, bool isReport = false)
			{
				if (isReport)
				{
					var stmMenuItemPK = Db.Connection.ExecuteScalar<Guid>($"SELECT GU_ItemGUID FROM dbo.GlbSecurity WHERE GU_GG = (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = '{securityItemName}')");
					contact.SecurityRightsView.OfType<OrgSecurityContacts>().Single(x => x.Security.OX_SU == stmMenuItemPK).OZ_Granted = granted;
				}
				else
				{
					contact.SecurityRightsView.OfType<OrgSecurityContacts>().Single(x => x.Security.OX_SecurityItemName == securityItemName).OZ_Granted = granted;
				}
			}

			//Security Name						Group				GrantedByDefault
			//Web ISF (View)					WEBISFVIEW			N
			//Web Booking (View)				WEBBKGVIEW			Y
			//RefDocType:ACC:BRC				DOC_ACC_BRC			Y
			//RefDocType:SCL:POA				DOC_SCL_POA			Y
			//Report							REPCUSTOMS00001		N
			//Actual Milestones (Update)		WEBACTMILUPD		N
			//Estimated Milestones (Update)		WEBESTMILUPD		N
			//Web Reports						WEBREPORTSVIEW		Y
			//Web Warehouse Orders (View)		WEBWHSORDVIEW		Y

			//org level override only
			var org1 = orgs[0];
			setOrgSecurity(org1, "Web ISF (View)", true);
			setOrgSecurity(org1, "RefDocType:ACC:BRC", false);
			setOrgSecurity(org1, "REPCUSTOMS00001", true, isReport: true);
			setOrgSecurity(org1, "Actual Milestones (Update)", true);
			setOrgSecurity(org1, "Estimated Milestones (Update)", true);

			//contact level override only (1 contact with override, 1 contact without override)
			var org2 = orgs[1];
			setOrgSecurityContacts(org2.Contacts[0], "Web ISF (View)", true);
			setOrgSecurityContacts(org2.Contacts[0], "RefDocType:ACC:BRC", false);
			setOrgSecurityContacts(org2.Contacts[0], "REPCUSTOMS00001", true, isReport: true);
			setOrgSecurityContacts(org2.Contacts[0], "Actual Milestones (Update)", true);
			setOrgSecurityContacts(org2.Contacts[0], "Estimated Milestones (Update)", true);

			//contact level override only (all contacts with override)
			var org3 = orgs[2];
			setOrgSecurityContacts(org3.Contacts[0], "Web ISF (View)", true);
			setOrgSecurityContacts(org3.Contacts[0], "RefDocType:ACC:BRC", false);
			setOrgSecurityContacts(org3.Contacts[0], "REPCUSTOMS00001", true, isReport: true);
			setOrgSecurityContacts(org3.Contacts[0], "Actual Milestones (Update)", true);
			setOrgSecurityContacts(org3.Contacts[0], "Estimated Milestones (Update)", true);
			setOrgSecurityContacts(org3.Contacts[1], "Web ISF (View)", true);
			setOrgSecurityContacts(org3.Contacts[1], "RefDocType:ACC:BRC", false);
			setOrgSecurityContacts(org3.Contacts[1], "REPCUSTOMS00001", true, isReport: true);
			setOrgSecurityContacts(org3.Contacts[1], "Actual Milestones (Update)", true);
			setOrgSecurityContacts(org3.Contacts[1], "Estimated Milestones (Update)", true);

			//org level override + contact level default
			var org4 = orgs[3];
			setOrgSecurity(org4, "Web ISF (View)", true);
			setOrgSecurity(org4, "RefDocType:ACC:BRC", false);
			setOrgSecurity(org4, "REPCUSTOMS00001", true, isReport: true);
			setOrgSecurity(org4, "Actual Milestones (Update)", true);
			setOrgSecurity(org4, "Estimated Milestones (Update)", true);
			setOrgSecurityContacts(org4.Contacts[0], "Web ISF (View)", false);
			setOrgSecurityContacts(org4.Contacts[0], "RefDocType:ACC:BRC", true);
			setOrgSecurityContacts(org4.Contacts[0], "REPCUSTOMS00001", false, isReport: true);
			setOrgSecurityContacts(org4.Contacts[0], "Actual Milestones (Update)", false);
			setOrgSecurityContacts(org4.Contacts[0], "Estimated Milestones (Update)", false);

			//org level default + contact level override
			var org5 = orgs[4];
			setOrgSecurity(org5, "Web ISF (View)", false);
			setOrgSecurity(org5, "RefDocType:ACC:BRC", true);
			setOrgSecurity(org5, "REPCUSTOMS00001", false, isReport: true);
			setOrgSecurity(org5, "Actual Milestones (Update)", false);
			setOrgSecurity(org5, "Estimated Milestones (Update)", false);
			setOrgSecurityContacts(org5.Contacts[0], "Web ISF (View)", true);
			setOrgSecurityContacts(org5.Contacts[0], "RefDocType:ACC:BRC", false);
			setOrgSecurityContacts(org5.Contacts[0], "REPCUSTOMS00001", true, isReport: true);
			setOrgSecurityContacts(org5.Contacts[0], "Actual Milestones (Update)", true);
			setOrgSecurityContacts(org5.Contacts[0], "Estimated Milestones (Update)", true);

			//no override at all
			var org6 = orgs[5];

			Factory.Save();
			orgs.ForEach(x => x.SyncWebSecurityToNeoGroup());

			AssertNEOGroups(orgs.Select(x => x.OH_Code.ToString()),
@"//org level override only
OH_1
OH_CODE         OC_CONTACTNAME  LS_TYPE         LS_GG_CODE      GRANTED         OVERRIDE
OH_1            OC_1_1          DOC             DOC_ACC_BRC     FALSE           Y
OH_1            OC_1_1          DOC             DOC_SCL_POA     TRUE            N
OH_1            OC_1_1          RPT             REPCUSTOMS00001 TRUE            Y
OH_1            OC_1_1          WEB             WEBACTMILUPD    TRUE            Y
OH_1            OC_1_1          WEB             WEBBKGVIEW      TRUE            N
OH_1            OC_1_1          WEB             WEBESTMILUPD    TRUE            Y
OH_1            OC_1_1          WEB             WEBISFVIEW      TRUE            Y
OH_1            OC_1_1          WEB             WEBREPORTSVIEW  TRUE            N
OH_1            OC_1_1          WEB             WEBWHSORDVIEW   TRUE            N
OH_1            OC_1_2          DOC             DOC_ACC_BRC     FALSE           Y
OH_1            OC_1_2          DOC             DOC_SCL_POA     TRUE            N
OH_1            OC_1_2          RPT             REPCUSTOMS00001 TRUE            Y
OH_1            OC_1_2          WEB             WEBACTMILUPD    TRUE            Y
OH_1            OC_1_2          WEB             WEBBKGVIEW      TRUE            N
OH_1            OC_1_2          WEB             WEBESTMILUPD    TRUE            Y
OH_1            OC_1_2          WEB             WEBISFVIEW      TRUE            Y
OH_1            OC_1_2          WEB             WEBREPORTSVIEW  TRUE            N
OH_1            OC_1_2          WEB             WEBWHSORDVIEW   TRUE            N

OH_CODE         GG_CODE
OH_1            DOC_SCL_POA
OH_1            REPCUSTOMS00001
OH_1            WEBACTMILUPD
OH_1            WEBBKGVIEW
OH_1            WEBESTMILUPD
OH_1            WEBISFVIEW
OH_1            WEBREPORTSVIEW
OH_1            WEBWHSORDVIEW

OH_CODE         OC_CONTACTNAME  GG_CODE
OH_1            OC_1_1          DOC_SCL_POA
OH_1            OC_1_1          REPCUSTOMS00001
OH_1            OC_1_1          WEBACTMILUPD
OH_1            OC_1_1          WEBBKGVIEW
OH_1            OC_1_1          WEBESTMILUPD
OH_1            OC_1_1          WEBISFVIEW
OH_1            OC_1_1          WEBREPORTSVIEW
OH_1            OC_1_1          WEBWHSORDVIEW
OH_1            OC_1_2          DOC_SCL_POA
OH_1            OC_1_2          REPCUSTOMS00001
OH_1            OC_1_2          WEBACTMILUPD
OH_1            OC_1_2          WEBBKGVIEW
OH_1            OC_1_2          WEBESTMILUPD
OH_1            OC_1_2          WEBISFVIEW
OH_1            OC_1_2          WEBREPORTSVIEW
OH_1            OC_1_2          WEBWHSORDVIEW


//contact level override only (1 contact with override, 1 contact without override)
OH_2
OH_CODE         OC_CONTACTNAME  LS_TYPE         LS_GG_CODE      GRANTED         OVERRIDE
OH_2            OC_2_1          DOC             DOC_ACC_BRC     FALSE           Y
OH_2            OC_2_1          DOC             DOC_SCL_POA     TRUE            N
OH_2            OC_2_1          RPT             REPCUSTOMS00001 TRUE            Y
OH_2            OC_2_1          WEB             WEBACTMILUPD    TRUE            Y
OH_2            OC_2_1          WEB             WEBBKGVIEW      TRUE            N
OH_2            OC_2_1          WEB             WEBESTMILUPD    TRUE            Y
OH_2            OC_2_1          WEB             WEBISFVIEW      TRUE            Y
OH_2            OC_2_1          WEB             WEBREPORTSVIEW  TRUE            N
OH_2            OC_2_1          WEB             WEBWHSORDVIEW   TRUE            N
OH_2            OC_2_2          DOC             DOC_ACC_BRC     TRUE            N
OH_2            OC_2_2          DOC             DOC_SCL_POA     TRUE            N
OH_2            OC_2_2          RPT             REPCUSTOMS00001 FALSE           N
OH_2            OC_2_2          WEB             WEBACTMILUPD    FALSE           N
OH_2            OC_2_2          WEB             WEBBKGVIEW      TRUE            N
OH_2            OC_2_2          WEB             WEBESTMILUPD    FALSE           N
OH_2            OC_2_2          WEB             WEBISFVIEW      FALSE           N
OH_2            OC_2_2          WEB             WEBREPORTSVIEW  TRUE            N
OH_2            OC_2_2          WEB             WEBWHSORDVIEW   TRUE            N

OH_CODE         GG_CODE
OH_2            DOC_ACC_BRC
OH_2            DOC_SCL_POA
OH_2            NEODOCACCESS
OH_2            NEOROLES
OH_2            REPCUSTOMS00001
OH_2            WEBACTMILUPD
OH_2            WEBBKGVIEW
OH_2            WEBESTMILUPD
OH_2            WEBISFVIEW
OH_2            WEBREPORTSVIEW
OH_2            WEBWHSORDVIEW

OH_CODE         OC_CONTACTNAME  GG_CODE
OH_2            OC_2_1          DOC_SCL_POA
OH_2            OC_2_1          REPCUSTOMS00001
OH_2            OC_2_1          WEBACTMILUPD
OH_2            OC_2_1          WEBBKGVIEW
OH_2            OC_2_1          WEBESTMILUPD
OH_2            OC_2_1          WEBISFVIEW
OH_2            OC_2_1          WEBREPORTSVIEW
OH_2            OC_2_1          WEBWHSORDVIEW
OH_2            OC_2_2          NEODOCACCESS
OH_2            OC_2_2          NEOROLES


//contact level override only (all contacts with override)
OH_3
OH_CODE         OC_CONTACTNAME  LS_TYPE         LS_GG_CODE      GRANTED         OVERRIDE
OH_3            OC_3_1          DOC             DOC_ACC_BRC     FALSE           Y
OH_3            OC_3_1          DOC             DOC_SCL_POA     TRUE            N
OH_3            OC_3_1          RPT             REPCUSTOMS00001 TRUE            Y
OH_3            OC_3_1          WEB             WEBACTMILUPD    TRUE            Y
OH_3            OC_3_1          WEB             WEBBKGVIEW      TRUE            N
OH_3            OC_3_1          WEB             WEBESTMILUPD    TRUE            Y
OH_3            OC_3_1          WEB             WEBISFVIEW      TRUE            Y
OH_3            OC_3_1          WEB             WEBREPORTSVIEW  TRUE            N
OH_3            OC_3_1          WEB             WEBWHSORDVIEW   TRUE            N
OH_3            OC_3_2          DOC             DOC_ACC_BRC     FALSE           Y
OH_3            OC_3_2          DOC             DOC_SCL_POA     TRUE            N
OH_3            OC_3_2          RPT             REPCUSTOMS00001 TRUE            Y
OH_3            OC_3_2          WEB             WEBACTMILUPD    TRUE            Y
OH_3            OC_3_2          WEB             WEBBKGVIEW      TRUE            N
OH_3            OC_3_2          WEB             WEBESTMILUPD    TRUE            Y
OH_3            OC_3_2          WEB             WEBISFVIEW      TRUE            Y
OH_3            OC_3_2          WEB             WEBREPORTSVIEW  TRUE            N
OH_3            OC_3_2          WEB             WEBWHSORDVIEW   TRUE            N

OH_CODE         GG_CODE
OH_3            DOC_SCL_POA
OH_3            REPCUSTOMS00001
OH_3            WEBACTMILUPD
OH_3            WEBBKGVIEW
OH_3            WEBESTMILUPD
OH_3            WEBISFVIEW
OH_3            WEBREPORTSVIEW
OH_3            WEBWHSORDVIEW

OH_CODE         OC_CONTACTNAME  GG_CODE
OH_3            OC_3_1          DOC_SCL_POA
OH_3            OC_3_1          REPCUSTOMS00001
OH_3            OC_3_1          WEBACTMILUPD
OH_3            OC_3_1          WEBBKGVIEW
OH_3            OC_3_1          WEBESTMILUPD
OH_3            OC_3_1          WEBISFVIEW
OH_3            OC_3_1          WEBREPORTSVIEW
OH_3            OC_3_1          WEBWHSORDVIEW
OH_3            OC_3_2          DOC_SCL_POA
OH_3            OC_3_2          REPCUSTOMS00001
OH_3            OC_3_2          WEBACTMILUPD
OH_3            OC_3_2          WEBBKGVIEW
OH_3            OC_3_2          WEBESTMILUPD
OH_3            OC_3_2          WEBISFVIEW
OH_3            OC_3_2          WEBREPORTSVIEW
OH_3            OC_3_2          WEBWHSORDVIEW


//org level override + contact level default
OH_4
OH_CODE         OC_CONTACTNAME  LS_TYPE         LS_GG_CODE      GRANTED         OVERRIDE
OH_4            OC_4_1          DOC             DOC_ACC_BRC     TRUE            N
OH_4            OC_4_1          DOC             DOC_SCL_POA     TRUE            N
OH_4            OC_4_1          RPT             REPCUSTOMS00001 FALSE           N
OH_4            OC_4_1          WEB             WEBACTMILUPD    FALSE           N
OH_4            OC_4_1          WEB             WEBBKGVIEW      TRUE            N
OH_4            OC_4_1          WEB             WEBESTMILUPD    FALSE           N
OH_4            OC_4_1          WEB             WEBISFVIEW      FALSE           N
OH_4            OC_4_1          WEB             WEBREPORTSVIEW  TRUE            N
OH_4            OC_4_1          WEB             WEBWHSORDVIEW   TRUE            N
OH_4            OC_4_2          DOC             DOC_ACC_BRC     FALSE           Y
OH_4            OC_4_2          DOC             DOC_SCL_POA     TRUE            N
OH_4            OC_4_2          RPT             REPCUSTOMS00001 TRUE            Y
OH_4            OC_4_2          WEB             WEBACTMILUPD    TRUE            Y
OH_4            OC_4_2          WEB             WEBBKGVIEW      TRUE            N
OH_4            OC_4_2          WEB             WEBESTMILUPD    TRUE            Y
OH_4            OC_4_2          WEB             WEBISFVIEW      TRUE            Y
OH_4            OC_4_2          WEB             WEBREPORTSVIEW  TRUE            N
OH_4            OC_4_2          WEB             WEBWHSORDVIEW   TRUE            N

OH_CODE         GG_CODE
OH_4            DOC_ACC_BRC
OH_4            DOC_SCL_POA
OH_4            NEODOCACCESS
OH_4            NEOROLES
OH_4            REPCUSTOMS00001
OH_4            WEBACTMILUPD
OH_4            WEBBKGVIEW
OH_4            WEBESTMILUPD
OH_4            WEBISFVIEW
OH_4            WEBREPORTSVIEW
OH_4            WEBWHSORDVIEW

OH_CODE         OC_CONTACTNAME  GG_CODE
OH_4            OC_4_1          NEODOCACCESS
OH_4            OC_4_1          NEOROLES
OH_4            OC_4_2          DOC_SCL_POA
OH_4            OC_4_2          REPCUSTOMS00001
OH_4            OC_4_2          WEBACTMILUPD
OH_4            OC_4_2          WEBBKGVIEW
OH_4            OC_4_2          WEBESTMILUPD
OH_4            OC_4_2          WEBISFVIEW
OH_4            OC_4_2          WEBREPORTSVIEW
OH_4            OC_4_2          WEBWHSORDVIEW


//org level default + contact level override
OH_5
OH_CODE         OC_CONTACTNAME  LS_TYPE         LS_GG_CODE      GRANTED         OVERRIDE
OH_5            OC_5_1          DOC             DOC_ACC_BRC     FALSE           Y
OH_5            OC_5_1          DOC             DOC_SCL_POA     TRUE            N
OH_5            OC_5_1          RPT             REPCUSTOMS00001 TRUE            Y
OH_5            OC_5_1          WEB             WEBACTMILUPD    TRUE            Y
OH_5            OC_5_1          WEB             WEBBKGVIEW      TRUE            N
OH_5            OC_5_1          WEB             WEBESTMILUPD    TRUE            Y
OH_5            OC_5_1          WEB             WEBISFVIEW      TRUE            Y
OH_5            OC_5_1          WEB             WEBREPORTSVIEW  TRUE            N
OH_5            OC_5_1          WEB             WEBWHSORDVIEW   TRUE            N
OH_5            OC_5_2          DOC             DOC_ACC_BRC     TRUE            N
OH_5            OC_5_2          DOC             DOC_SCL_POA     TRUE            N
OH_5            OC_5_2          RPT             REPCUSTOMS00001 FALSE           N
OH_5            OC_5_2          WEB             WEBACTMILUPD    FALSE           N
OH_5            OC_5_2          WEB             WEBBKGVIEW      TRUE            N
OH_5            OC_5_2          WEB             WEBESTMILUPD    FALSE           N
OH_5            OC_5_2          WEB             WEBISFVIEW      FALSE           N
OH_5            OC_5_2          WEB             WEBREPORTSVIEW  TRUE            N
OH_5            OC_5_2          WEB             WEBWHSORDVIEW   TRUE            N

OH_CODE         GG_CODE
OH_5            DOC_ACC_BRC
OH_5            DOC_SCL_POA
OH_5            NEODOCACCESS
OH_5            NEOROLES
OH_5            REPCUSTOMS00001
OH_5            WEBACTMILUPD
OH_5            WEBBKGVIEW
OH_5            WEBESTMILUPD
OH_5            WEBISFVIEW
OH_5            WEBREPORTSVIEW
OH_5            WEBWHSORDVIEW

OH_CODE         OC_CONTACTNAME  GG_CODE
OH_5            OC_5_1          DOC_SCL_POA
OH_5            OC_5_1          REPCUSTOMS00001
OH_5            OC_5_1          WEBACTMILUPD
OH_5            OC_5_1          WEBBKGVIEW
OH_5            OC_5_1          WEBESTMILUPD
OH_5            OC_5_1          WEBISFVIEW
OH_5            OC_5_1          WEBREPORTSVIEW
OH_5            OC_5_1          WEBWHSORDVIEW
OH_5            OC_5_2          NEODOCACCESS
OH_5            OC_5_2          NEOROLES


//no override at all
OH_6
OH_CODE         OC_CONTACTNAME  LS_TYPE         LS_GG_CODE      GRANTED         OVERRIDE
OH_6            OC_6_1          DOC             DOC_ACC_BRC     TRUE            N
OH_6            OC_6_1          DOC             DOC_SCL_POA     TRUE            N
OH_6            OC_6_1          RPT             REPCUSTOMS00001 FALSE           N
OH_6            OC_6_1          WEB             WEBACTMILUPD    FALSE           N
OH_6            OC_6_1          WEB             WEBBKGVIEW      TRUE            N
OH_6            OC_6_1          WEB             WEBESTMILUPD    FALSE           N
OH_6            OC_6_1          WEB             WEBISFVIEW      FALSE           N
OH_6            OC_6_1          WEB             WEBREPORTSVIEW  TRUE            N
OH_6            OC_6_1          WEB             WEBWHSORDVIEW   TRUE            N
OH_6            OC_6_2          DOC             DOC_ACC_BRC     TRUE            N
OH_6            OC_6_2          DOC             DOC_SCL_POA     TRUE            N
OH_6            OC_6_2          RPT             REPCUSTOMS00001 FALSE           N
OH_6            OC_6_2          WEB             WEBACTMILUPD    FALSE           N
OH_6            OC_6_2          WEB             WEBBKGVIEW      TRUE            N
OH_6            OC_6_2          WEB             WEBESTMILUPD    FALSE           N
OH_6            OC_6_2          WEB             WEBISFVIEW      FALSE           N
OH_6            OC_6_2          WEB             WEBREPORTSVIEW  TRUE            N
OH_6            OC_6_2          WEB             WEBWHSORDVIEW   TRUE            N

OH_CODE         GG_CODE
OH_6            NEODOCACCESS
OH_6            NEOROLES

OH_CODE         OC_CONTACTNAME  GG_CODE
OH_6            OC_6_1          NEODOCACCESS
OH_6            OC_6_1          NEOROLES
OH_6            OC_6_2          NEODOCACCESS
OH_6            OC_6_2          NEOROLES



");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Unit test")]
		void AssertNEOGroups(IEnumerable<string> orgCodes, string expectedData)
		{
			var sql1 = @"
DECLARE @OrgPK UNIQUEIDENTIFIER = (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = '{orgCode}');
 
DECLARE @Report_SU UNIQUEIDENTIFIER = 
(SELECT GU_ItemGUID FROM dbo.GlbSecurity WHERE GU_GG = (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'RepCustoms00001'));

DECLARE @LegacySecurity TABLE 
(
	LS_OX_SecurityItemName VARCHAR(35) NULL
	,LS_OX_SU UNIQUEIDENTIFIER NULL
	,LS_IsGrantedByDefault BIT NOT NULL
	,LS_GG_Code VARCHAR(15) NULL
	,LS_Type VARCHAR(3) NOT NULL CHECK (LS_Type IN ('WEB', 'DOC', 'RPT'))
);

INSERT INTO @LegacySecurity 
(
	LS_OX_SecurityItemName
	,LS_IsGrantedByDefault
	,LS_GG_Code
	,LS_Type
	,LS_OX_SU
)
VALUES
('Web ISF (View)', 0, 'WEBISFVIEW', 'WEB', null),
('Web Booking (View)', 1, 'WEBBKGVIEW', 'WEB', null),
('RefDocType:ACC:BRC', 1, 'DOC_ACC_BRC', 'DOC', null),
('RefDocType:SCL:POA', 1, 'DOC_SCL_POA', 'DOC', null),
('Report', 0, 'RepCustoms00001', 'RPT', @Report_SU),
('Actual Milestones (Update)', 0, 'WEBACTMILUPD', 'WEB', null),
('Estimated Milestones (Update)', 0, 'WEBESTMILUPD', 'WEB', null),
('Web Reports', 1, 'WEBREPORTSVIEW', 'WEB', null),
('Web Warehouse Orders (View)', 1, 'WEBWHSORDVIEW', 'WEB', null);

SELECT
	OH_Code,
	OC_ContactName,
	LS_Type,
	LS_GG_Code
	,Granted = COALESCE(OZ_Granted, OX_Granted, LS_IsGrantedByDefault)
	,Override = CASE WHEN LS_IsGrantedByDefault <> COALESCE(OZ_Granted, OX_Granted, LS_IsGrantedByDefault) THEN 'Y' ELSE 'N' END
FROM dbo.OrgContact
JOIN dbo.OrgHeader ON OH_PK = OC_OH
JOIN @LegacySecurity ON 1 = 1
LEFT JOIN dbo.OrgSecurity ON
(
	(
		( OX_SecurityItemName <> '' AND OX_SecurityItemName = LS_OX_SecurityItemName ) -- static / doc
		OR ( OX_SU IS NOT NULL AND OX_SU = LS_OX_SU ) -- report
	)
	AND OX_OH = OC_OH
)
LEFT JOIN dbo.OrgSecurityContacts ON OZ_OC = OC_PK AND OZ_OX = OX_PK
WHERE OC_IsActive = 1 AND OC_WebAccessEnabled = 1 AND OC_OH	= @OrgPK
ORDER by 1,2,3,4,5,6;";

			var sql2 = @"
SELECT OH_Code, GG_Code
FROM dbo.GlbGroupOrgLink  GOK 
JOIN dbo.GlbGroup ON GOK_GG_Group = GG_PK
JOIN dbo.OrgHeader ON GOK_OH_Org = OH_PK
WHERE GG_Code IN ('WEBISFVIEW', 'WEBBKGVIEW', 'NEOROLES', 'DOC_ACC_BRC', 'DOC_SCL_POA', 'NEODOCACCESS', 'RepCustoms00001', 'WEBACTMILUPD', 'WEBESTMILUPD', 'WEBREPORTSVIEW', 'WEBWHSORDVIEW')
AND OH_Code = '{orgCode}'
ORDER BY 1,2;";

			var sql3 = @"
SELECT OH_Code, OC_ContactName, GG_Code 
FROM dbo.GlbGroupOrgContactLink 
JOIN dbo.GlbGroup ON GCK_GG_Group = GG_PK
JOIN dbo.OrgContact ON GCK_OC_Contact = OC_PK
JOIN dbo.OrgHeader ON OH_PK = OC_OH
WHERE GG_Code IN ('WEBISFVIEW', 'WEBBKGVIEW', 'NEOROLES', 'DOC_ACC_BRC', 'DOC_SCL_POA', 'NEODOCACCESS', 'RepCustoms00001', 'WEBACTMILUPD', 'WEBESTMILUPD', 'WEBREPORTSVIEW', 'WEBWHSORDVIEW')
AND OH_Code = '{orgCode}'
ORDER BY 1,2,3;";

			var rows = new StringBuilder();
			foreach (var orgCode in orgCodes)
			{
				rows.AppendLine(orgCode);

				foreach (var table in new[] { sql1, sql2, sql3 }
										.Select(x => x.Replace("{orgCode}", orgCode))
										.Select(x => DataUtils.GetDataTableFromQuery(Db.Connection, x)))
				{
					rows.AppendLine(string.Join("", table.Columns.OfType<DataColumn>().Select(x => x.ColumnName.ToUpper().PadRight(16))).Trim());

					foreach (DataRow row in table.Rows)
					{
						rows.AppendLine(string.Join("", row.ItemArray.Select(x => x.ToString().ToUpper().PadRight(16))).Trim());
					}

					rows.AppendLine();
				}

				rows.AppendLine();
			}

			AssertEquals(string.Join("\r\n", expectedData.SplitByLine().Where(x => !x.StartsWith("//"))), rows.ToString());
		}

		#region  Add Or Update DDR Log

		public void TestDDRCreatedWhenDuplicateDetected()
		{
			InitDedupOrgInfo(out var orgHeaderInDb, out var orgTarget1InDb, out var orgTarget2InDb, out var targetListsInDb, out var scoreResultsInDb);
			Factory.Save();
			Assert("Precondition", orgHeaderInDb.IsInDatabase);
			orgHeaderInDb.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", orgHeaderInDb, targetListsInDb, scoreResultsInDb, null));
			var referenceStr1 = $"|NAM={orgTarget2InDb.OH_Code}|RES=HIGH|SCR=91|TYP=EDT";
			var referenceStr2 = $"|NAM={orgTarget1InDb.OH_Code}|RES=MED|SCR=76|TYP=EDT";
			orgHeaderInDb.OH_FullName = "New Name To Invoke Save";

			Factory.Save();
			var stmAlogsForDDR = LoadExistingDDRLogs(orgHeaderInDb.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Two logs added", 2, stmAlogsForDDR.Length);
				AssertEquals(referenceStr1, stmAlogsForDDR[0].SL_Reference);
				AssertEquals(referenceStr2, stmAlogsForDDR[1].SL_Reference);
				AssertEquals("Should have time of event", orgHeaderInDb.dateTimeForDDR.ToZDateTime(), stmAlogsForDDR[0].SL_EventTime);
				AssertEquals("Should have time of event", orgHeaderInDb.dateTimeForDDR.ToZDateTime(), stmAlogsForDDR[1].SL_EventTime);
			});

			InitDedupOrgInfo(out var orgHeader, out var orgTarget1, out var orgTarget2, out var targetLists, out var scoreResults);

			orgHeader.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", orgHeader, targetLists, scoreResults, null));
			referenceStr1 = $"|NAM={orgTarget2.OH_Code}|RES=HIGH|SCR=91|TYP=ADD";
			referenceStr2 = $"|NAM={orgTarget1.OH_Code}|RES=MED|SCR=76|TYP=ADD";

			stmAlogsForDDR = LoadExistingDDRLogs(orgHeader.PK);
			AssertEquals("No logs added", 0, stmAlogsForDDR.Length);

			Factory.Save();
			stmAlogsForDDR = LoadExistingDDRLogs(orgHeader.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Two new logs added", 2, stmAlogsForDDR.Length);
				Assert(stmAlogsForDDR[0].IsInDatabase);
				Assert(stmAlogsForDDR[1].IsInDatabase);
				AssertEquals(referenceStr1, stmAlogsForDDR[0].SL_Reference);
				AssertEquals(referenceStr2, stmAlogsForDDR[1].SL_Reference);
				AssertEquals("Should have time of event", orgHeader.dateTimeForDDR.ToZDateTime(), stmAlogsForDDR[0].SL_EventTime);
				AssertEquals("Should have time of event", orgHeader.dateTimeForDDR.ToZDateTime(), stmAlogsForDDR[1].SL_EventTime);
			});

			orgHeader.OH_FullName = "New Name 1";
			Factory.Save();
			stmAlogsForDDR = LoadExistingDDRLogs(orgHeader.PK);
			AssertEquals("No new logs added", 2, stmAlogsForDDR.Length);

			orgHeader.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", orgHeader, targetLists, scoreResults, null));
			Factory.Save();
			stmAlogsForDDR = LoadExistingDDRLogs(orgHeader.PK);
			AssertEquals("No new logs added", 2, stmAlogsForDDR.Length);

			orgHeader.OH_FullName = "New Name 2";
			Factory.Save();
			stmAlogsForDDR = LoadExistingDDRLogs(orgHeader.PK);
			AssertEquals("New log added", 4, stmAlogsForDDR.Length);
		}

		public void TestDDREventDetails()
		{
			var logParameters = new Dictionary<string, string>
			{
				["TYP"] = "EDT",
				["RES"] = "HIGH",
				["SCR"] = "100",
				["NAM"] = "TEST"
			};
			GenericTestDDREventDetails("Type: Existing Record, Threshold: High, Score: 100%, Code: TEST", logParameters);

			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "ADD",
				["RES"] = "MED",
				["SCR"] = "0",
				["NAM"] = "ABC"
			};
			GenericTestDDREventDetails("Type: New Record, Threshold: Medium, Score: 0%, Code: ABC", logParameters);

			logParameters = new Dictionary<string, string>
			{
				["TYP"] = "ADD",
				["RES"] = "LOW",
				["SCR"] = "50",
				["NAM"] = "DEF"
			};
			GenericTestDDREventDetails("Type: New Record, Threshold: Low, Score: 50%, Code: DEF", logParameters);
		}

		void GenericTestDDREventDetails(string expectedEventDetails, Dictionary<string, string> logParameters)
		{
			var debo = Factory.New<DummyEnterpriseBusinessObject>();
			var filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DuplicateDetectedForReview.Code);
			debo.GetLogs().AddNew(Events.DuplicateDetectedForReview, string.Empty, logParameters.ToArray());
			var log = debo.GetLogs().Find(filter).Single();
			AssertNotNull("Should have a StmALog", log);
			AssertEquals("Reference Display Event Details", expectedEventDetails, log.DisplayEventReference);
		}

		public void TestNewDDRsOverrideTheFormerOnes()
		{
			InitDedupOrgInfo(out var orgHeader, out var orgTarget1, out var orgTarget2, out var targetLists, out var scoreResults);
			AssertEquals("PreCondition", 2, scoreResults.Count);
			AssertEquals("PreCondition", 0.76, scoreResults[0].Score);
			AssertEquals("PreCondition", 0.91, scoreResults[1].Score);

			orgHeader.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", orgHeader, targetLists, scoreResults, null));

			scoreResults[0].Score = 0.88;
			scoreResults[1].Score = 0.99;
			orgHeader.AddOrUpdateDDRLogInfo(ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", orgHeader, targetLists, scoreResults, null));

			Factory.Save();

			var stmAlogsForDDR = LoadExistingDDRLogs(orgHeader.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Should only be two logs", 2, stmAlogsForDDR.Length);
				AssertEquals("Should have time of event", orgHeader.dateTimeForDDR.ToZDateTime(), stmAlogsForDDR[0].SL_EventTime);
				AssertEquals("Should have time of event", orgHeader.dateTimeForDDR.ToZDateTime(), stmAlogsForDDR[1].SL_EventTime);
				AssertEquals("Should take newest reference", $"|NAM={orgTarget2.OH_Code}|RES=HIGH|SCR=99|TYP=ADD", stmAlogsForDDR[0].SL_Reference);
				AssertEquals("Should take newest reference", $"|NAM={orgTarget1.OH_Code}|RES=HIGH|SCR=88|TYP=ADD", stmAlogsForDDR[1].SL_Reference);
			});
		}
		#endregion

		#region Associated Fields

		public void TestAssociatedDeclarations()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			AssertEquals("PreCondition", false, orgAddress.Header.AssociatedDeclarations.Any());

			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			CreateDeclaration(orgAddress2);
			AssertEquals("PreCondition", false, orgAddress.Header.AssociatedDeclarations.Any());
			AssertEquals("OrgAddress2's header should have associated shipments.", 1, orgAddress2.Header.AssociatedDeclarations.Count);

			var jobs = new List<ZGuid>();
			for (int i = 0; i < 3; i++)
			{
				jobs.Add(CreateDeclaration(orgAddress));
			}

			AssertEquals(3, orgAddress.Header.AssociatedDeclarations.Count);
			AssertEquals(false, orgAddress.Header.AssociatedDeclarations.Except(jobs).Any());
		}

		public void TestGetAssociatedDeclarations()
		{
			AssertEquals(false, OrgHeader.GetAssociatedDeclarations(ZGuid.Empty).Any());
			AssertEquals(false, OrgHeader.GetAssociatedDeclarations(ZGuid.Invalid).Any());

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var pk = CreateDeclaration(orgAddress);
			var declarations = OrgHeader.GetAssociatedDeclarations(orgAddress.Header.PK);
			AssertEquals(pk, declarations.Single());
		}

		public void TestAssociatedShipments()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			AssertEquals("PreCondition", false, orgAddress.Header.AssociatedShipments.Any());

			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			CreateShipment(orgAddress2);
			AssertEquals("PreCondition", false, orgAddress.Header.AssociatedShipments.Any());
			AssertEquals("OrgAddress2's header should have associated shipments.", 1, orgAddress2.Header.AssociatedShipments.Count);

			var jobs = new List<ZGuid>();
			for (int i = 0; i < 3; i++)
			{
				jobs.Add(CreateShipment(orgAddress));
			}

			AssertEquals(3, orgAddress.Header.AssociatedShipments.Count);
			AssertEquals(false, orgAddress.Header.AssociatedShipments.Except(jobs).Any());
		}

		public void TestGetAssociatedShipments()
		{
			AssertEquals(false, OrgHeader.GetAssociatedShipments(ZGuid.Empty).Any());
			AssertEquals(false, OrgHeader.GetAssociatedShipments(ZGuid.Invalid).Any());

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var pk = CreateShipment(orgAddress);
			var shipments = OrgHeader.GetAssociatedShipments(orgAddress.Header.PK);
			AssertEquals(pk, shipments.Single());
		}

		public void TestAssociatedConsols()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			AssertEquals("PreCondition", false, orgAddress.Header.AssociatedConsols.Any());

			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			CreateConsol(orgAddress2);
			AssertEquals("PreCondition", false, orgAddress.Header.AssociatedConsols.Any());
			AssertEquals("OrgAddress2's header should have associated consols.", 1, orgAddress2.Header.AssociatedConsols.Count);

			var jobs = new List<ZGuid>();
			for (int i = 0; i < 3; i++)
			{
				jobs.Add(CreateConsol(orgAddress));
			}

			AssertEquals(3, orgAddress.Header.AssociatedConsols.Count);
			AssertEquals(false, orgAddress.Header.AssociatedConsols.Except(jobs).Any());
		}

		public void TestGetAssociatedConsols()
		{
			AssertEquals(false, OrgHeader.GetAssociatedConsols(ZGuid.Empty).Any());
			AssertEquals(false, OrgHeader.GetAssociatedConsols(ZGuid.Invalid).Any());

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var pk = CreateConsol(orgAddress);
			var consols = OrgHeader.GetAssociatedConsols(orgAddress.Header.PK);
			AssertEquals(pk, consols.Single());
		}

		#endregion

		#region RSL_ShippingLine

		public void TestSetShippingLine_ShouldUpdateCusCodeAddNewScacWithUsCountry_WhenCustomsCodesContainScacAndC1c_ShippingLineSCACIsNotBlank()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var existingSCAC = org.CustomsCodes.AddNew();
			existingSCAC.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			existingSCAC.OK_CustomsRegNo = "1111";
			var existingC1C = org.CustomsCodes.AddNew();
			existingC1C.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			existingC1C.OK_CustomsRegNo = "c1ab";

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";

			var scacCode = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.CarrierCode).First();
			var c1cCode = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode).First();

			AssertEquals("1111", scacCode.OK_CustomsRegNo);
			AssertEquals("c1ab", c1cCode.OK_CustomsRegNo);

			org.OH_RSL_ShippingLine = shippingLine.PK;

			var auCustomCarrierCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Australia);
			var auC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.Australia);
			var usScacCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var usC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.UnitedStates);
			AssertNull(auC1cCode);
			AssertEquals(existingSCAC.OK_CustomsRegNo, auCustomCarrierCode.OK_CustomsRegNo);
			AssertEquals(shippingLine.RSL_StandardCarrierAlphaCode, usScacCode.OK_CustomsRegNo);
			AssertEquals(shippingLine.RSL_CargoWiseOneCode, usC1cCode.OK_CustomsRegNo);
		}

		public void TestSetShippingLine_ShouldAddCusCodeAddNewScacWithUsCountry_WhenCustomsCodesContainScacAndDoesNotContainC1c_ShippingLineSCACIsNotBlank()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var existingSCAC = org.CustomsCodes.AddNew();
			existingSCAC.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			existingSCAC.OK_CustomsRegNo = "1111";

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1ab";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";

			var scacCode = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.CarrierCode).First();
			AssertEquals("1111", scacCode.OK_CustomsRegNo);

			org.OH_RSL_ShippingLine = shippingLine.PK;

			var auCustomCarrierCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Australia);
			var auC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.Australia);
			var usScacCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var usC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.UnitedStates);
			AssertNull(auC1cCode);
			AssertEquals(existingSCAC.OK_CustomsRegNo, auCustomCarrierCode.OK_CustomsRegNo);
			AssertEquals(shippingLine.RSL_StandardCarrierAlphaCode, usScacCode.OK_CustomsRegNo);
			AssertEquals(shippingLine.RSL_CargoWiseOneCode, usC1cCode.OK_CustomsRegNo);
		}

		public void TestSetShippingLine_ShouldUpdateCusCodeAddNewScacWithUsCountry_WhenCustomsCodesContainC1cAndDoesNotContainScac_ShippingLineSCACIsNotBlank()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var existingC1C = org.CustomsCodes.AddNew();
			existingC1C.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			existingC1C.OK_CustomsRegNo = "c1ab";

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1ab";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";

			var c1cCode = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode).First();
			AssertEquals("c1ab", c1cCode.OK_CustomsRegNo);

			org.OH_RSL_ShippingLine = shippingLine.PK;

			var auCustomCarrierCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Australia);
			var auC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.Australia);
			var usScacCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var usC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.UnitedStates);
			AssertNull(auC1cCode);
			AssertNull(auCustomCarrierCode);
			AssertEquals(shippingLine.RSL_StandardCarrierAlphaCode, usScacCode.OK_CustomsRegNo);
			AssertEquals(shippingLine.RSL_CargoWiseOneCode, usC1cCode.OK_CustomsRegNo);
		}

		public void TestSetShippingLine_ShouldUpdateCusCodeUpdateScacIfExisting_WhenCustomsCodesContainScacAndC1c_ShippingLineSCACIsBlank()
		{
			//ShippingLine without SCAC code
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1ba";
			shippingLine.RSL_CarrierName = "testship";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var existingSCAC = org.CustomsCodes.AddNew();
			existingSCAC.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			existingSCAC.OK_CustomsRegNo = "1111";
			var existingC1C = org.CustomsCodes.AddNew();
			existingC1C.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			existingC1C.OK_CustomsRegNo = "c1ab";

			var scacCode = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.CarrierCode).First();
			var c1cCode = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode).First();

			AssertEquals("1111", scacCode.OK_CustomsRegNo);
			AssertEquals("c1ab", c1cCode.OK_CustomsRegNo);

			org.OH_RSL_ShippingLine = shippingLine.PK;

			var auCustomCarrierCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Australia);
			var auC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.Australia);
			var usScacCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var usC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.UnitedStates);
			AssertNull(auC1cCode);
			AssertNotNull(auCustomCarrierCode);
			AssertEquals("c1ba", usC1cCode.OK_CustomsRegNo);
			AssertNull(usScacCode);
		}

		public void TestSetShippingLine_ShouldAddCusCodeAddScac_WhenCustomsCodesDoesNotContainScacAndC1c_ShippingLineSCACIsNotBlank()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1ab";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";

			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_RSL_ShippingLine = shippingLine.PK;

			var usScacCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var usC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var auScacCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Australia);
			var auC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.Australia);

			AssertEquals("1234", usScacCode.OK_CustomsRegNo);
			AssertEquals("c1ab", usC1cCode.OK_CustomsRegNo);
			AssertNull(auScacCode);
			AssertNull(auC1cCode);
		}

		public void TestSetShippingLine_ShouldAddCusCode_ShouldNotAddScac_WhenCustomsCodesDoesNotContainScacAndC1c_ShippingLineSCACIsBlank()
		{
			//ShippingLine without SCAC code
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1ab";
			shippingLine.RSL_CarrierName = "testship";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var existingSCAC = org.CustomsCodes.AddNew();
			existingSCAC.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			existingSCAC.OK_CustomsRegNo = "1234";

			org.OH_RSL_ShippingLine = shippingLine.PK;

			var scacCodes = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.CarrierCode);
			var c1cCodes = org.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);

			AssertEquals("The existing au CCC should not be deleted", 1, scacCodes.Length); //au ccc should not be deleted
			AssertEquals("There should be a new US C1C", 1, c1cCodes.Length);

			var usC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var auC1cCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Core.Constants.CountryCodes.Australia);
			var usScacCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			var auScacCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Australia);
			AssertEquals("c1ab", usC1cCode.OK_CustomsRegNo);
			AssertNull(auC1cCode);
			AssertNotNull(auScacCode);
			AssertNull(usScacCode);
		}

		public void TestSetShippingLine_NameAndSCACLinked()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			org.OH_IsShippingLine = true;
			shippingLine.RSL_CarrierName = "test";
			shippingLine.RSL_StandardCarrierAlphaCode = "ABCD";

			Factory.Save();

			AssertEquals(ZString.Empty, org.ShippingLineCarrierName);
			AssertEquals(ZString.Empty, org.ShippingLineSCAC);

			org.OH_RSL_ShippingLine = shippingLine.PK;

			AssertEquals(shippingLine.RSL_CarrierName, org.ShippingLineCarrierName);
			AssertEquals(shippingLine.RSL_StandardCarrierAlphaCode, org.ShippingLineSCAC);

			org.OH_RSL_ShippingLine = Guid.NewGuid();

			AssertEquals(ZString.Empty, org.ShippingLineCarrierName);
			AssertEquals(ZString.Empty, org.ShippingLineSCAC);
		}

		public void TestRefShippingLineToOrgHeader_WhenLinkedShippingLine()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = true;

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_RSL_ShippingLine = shippingLine.PK;

			Assert(org.OH_IsSeaWholesalerInfo.ReadOnly);
			Assert(org.OH_IsSeaWholesaler);
			Assert(org.OH_IsShippingLineInfo.ReadOnly);
			Assert(org.OH_IsShippingLine);
			Assert(!org.OH_RSL_ShippingLineInfo.ReadOnly);

			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = false;
			org.OH_RSL_ShippingLine = ZGuid.Empty;
			org.OH_RSL_ShippingLine = shippingLine.PK;

			Assert(org.OH_IsSeaWholesalerInfo.ReadOnly);
			Assert(org.OH_IsSeaWholesaler);
			Assert(org.OH_IsShippingLineInfo.ReadOnly);
			Assert(!org.OH_IsShippingLine);
			Assert(!org.OH_RSL_ShippingLineInfo.ReadOnly);

			shippingLine.RSL_IsNVO = false;
			shippingLine.RSL_IsShippingLine = false;
			org.OH_RSL_ShippingLine = ZGuid.Empty;
			org.OH_RSL_ShippingLine = shippingLine.PK;

			Assert(org.OH_IsSeaWholesalerInfo.ReadOnly);
			Assert(!org.OH_IsSeaWholesaler);
			Assert(org.OH_IsShippingLineInfo.ReadOnly);
			Assert(org.OH_IsShippingLine);
			Assert(!org.OH_RSL_ShippingLineInfo.ReadOnly);
		}

		public void TestLinkedShippingLine_ReadOnly_WhenNotIsShippingLine()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsShippingLine = false;

			AssertEquals(true, org.OH_RSL_ShippingLineInfo.ReadOnly);

			org.OH_IsShippingLine = true;
			AssertEquals(false, org.OH_RSL_ShippingLineInfo.ReadOnly);

			org.OH_IsShippingLine = false;
			AssertEquals(true, org.OH_RSL_ShippingLineInfo.ReadOnly);
		}

		public void TestVoyageRecyclingPeriod_ReadOnly_WhenNotIsShippingLine()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_IsShippingLine = false;
			AssertEquals(true, org.MiscServ.VoyageRecyclingPeriodCodeInfo.ReadOnly);

			org.OH_IsShippingLine = true;
			AssertEquals(false, org.MiscServ.VoyageRecyclingPeriodCodeInfo.ReadOnly);

			org.OH_IsShippingLine = false;
			AssertEquals(true, org.MiscServ.VoyageRecyclingPeriodCodeInfo.ReadOnly);
		}

		public void TestLinkedShippingLine_ShippingLineOrg_DoesNotRaiseException()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsShippingLine = true;
			org.OH_RSL_ShippingLine = shippingLine.PK;

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestLinkedShippingLine_ReadOnly_WhenNotIsSeaWholesaler()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_IsSeaWholesaler = false;
			AssertEquals(true, org.OH_RSL_ShippingLineInfo.ReadOnly);

			org.OH_IsSeaWholesaler = true;
			AssertEquals(false, org.OH_RSL_ShippingLineInfo.ReadOnly);

			org.OH_IsSeaWholesaler = false;
			AssertEquals(true, org.OH_RSL_ShippingLineInfo.ReadOnly);
		}

		public void TestVoyageRecyclingPeriod_ReadOnly_WhenNotIsSeaWholesaler()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_IsSeaWholesaler = false;
			AssertEquals(true, org.MiscServ.VoyageRecyclingPeriodCodeInfo.ReadOnly);

			org.OH_IsSeaWholesaler = true;
			AssertEquals(false, org.MiscServ.VoyageRecyclingPeriodCodeInfo.ReadOnly);

			org.OH_IsSeaWholesaler = false;
			AssertEquals(true, org.MiscServ.VoyageRecyclingPeriodCodeInfo.ReadOnly);
		}

		public void TestLinkedShippingLine_SeaWholesalerOrg_DoesNotRaiseException()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsSeaWholesaler = true;
			org.OH_RSL_ShippingLine = shippingLine.PK;

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestLinkedShippingLine_NonSeaCarrierOrg_RaisesException()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;
			var orgRow = (INeedRow)org;
			orgRow.Row[OrgHeaderSchema.Constants.OH_IsSeaWholesaler] = false;
			orgRow.Row[OrgHeaderSchema.Constants.OH_IsShippingLine] = false;

			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		#endregion

		#region ShippingLineIntegrations

		public void TestShippingLineIntegrations()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals(string.Empty, org.ShippingLineIntegrations);

			var shippingLine = Factory.New<RefShippingLine>();
			org.OH_IsShippingLine = true;
			org.OH_RSL_ShippingLine = shippingLine.PK;
			AssertEquals("No Integrations Specified", org.ShippingLineIntegrations);

			var shippinglineWithAllItems = Factory.New<RefShippingLine>();
			shippinglineWithAllItems.RSL_OceanCarrierMessagingAvailable = true;
			shippinglineWithAllItems.RSL_GlobalSailingScheduleAvailable = true;
			shippinglineWithAllItems.RSL_ContainerAutomationAvailable = true;
			shippinglineWithAllItems.RSL_InvoiceAvailable = true;
			org.OH_RSL_ShippingLine = shippinglineWithAllItems.PK;
			AssertEquals(@"Ocean Carrier Messaging
Global Sailing Schedule
Container Automation
Invoice
", org.ShippingLineIntegrations);
		}

		#endregion

		#region UniqueIndexFailureHandler

		public void TestUniqueIndexFailureHandler()
		{
			var headerA = Factory.NewWithValidTestData<OrgHeader>();
			headerA.OH_Code = "TESTESSYD";

			var headerB = Factory.NewWithValidTestData<OrgHeader>();
			headerB.OH_FullName = "TEST TESTOrganization";
			headerB.OH_RL_NKClosestPort = "AUSYD";
			headerB.MainAddress.OA_Address1 = "111 Demo St";
			headerB.MainAddress.OA_City = "Demoville";
			headerB.MainAddress.OA_PostCode = "12345";
			headerB.OH_IsSalesLead = true;
			Factory.Save();

			try
			{
				headerB.IsCodeDuplicatedCheckDisabledForOnce = true;
				headerB.OH_Code = headerA.OH_Code;
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				CombineAssertions(() =>
				{
					AssertEquals("Organization code is duplicated and will be regenerated, please attempt to save again.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNoExceptionThrown(() => { Factory.Save(); });
					AssertEquals("OH_Code has been regenerated and is not duplicated.", "TESTESSYD1", headerB.OH_Code);
				});
			}
		}

		#endregion

		#region OrgAirlineBranchAccounts

		public void TestOrgAirlineBranchAccounts()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsAirLine = true;

			var branchAccount1 = org.OrgAirlineBranchAccounts.AddNew();
			branchAccount1.OAA_GB_Branch = GlbBranch.CurrentBranch.PK;
			branchAccount1.OAA_APAirlineAccountNumber = "11111";

			var branchAccount2 = org.OrgAirlineBranchAccounts.AddNew();
			branchAccount2.OAA_GB_Branch = GlbBranch.CurrentBranch.PK;
			branchAccount2.OAA_APAirlineAccountNumber = "22222";

			AssertEquals(2, org.OrgAirlineBranchAccounts.Count);
			AssertContainsExactElementsInAnyOrder(new[] { branchAccount1, branchAccount2 }, org.OrgAirlineBranchAccounts.Cast<OrgAirlineBranchAccount>().ToArray());
		}

		#endregion

		#region ModifyCompanyTypeFlags

		public void TestLogsAddedForCompanyTypeFlagChange()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			CombineAssertions(() =>
			{
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsNationalAccount = false; }, () => { header.OH_IsNationalAccount = true; }, "National Account");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsGlobalAccount = false; }, () => { header.OH_IsGlobalAccount = true; }, "Global Supplier");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsTempAccount = false; }, () => { header.OH_IsTempAccount = true; }, "Temporary Account");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsConsignor = false; }, () => { header.OH_IsConsignor = true; }, "Consignor");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsConsignee = false; }, () => { header.OH_IsConsignee = true; }, "Consignee");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsTransportClient = false; }, () => { header.OH_IsTransportClient = true; }, "Transport Client");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsWarehouseClient = false; }, () => { header.OH_IsWarehouseClient = true; }, "Warehouse");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsShippingProvider = false; }, () => { header.OH_IsShippingProvider = true; }, "Carrier");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsForwarder = false; }, () => { header.OH_IsForwarder = true; }, "Forwarder/Agent");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsBroker = false; }, () => { header.OH_IsBroker = true; }, "Broker");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsMiscFreightServices = false; }, () => { header.OH_IsMiscFreightServices = true; }, "Services");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsCompetitor = false; }, () => { header.OH_IsCompetitor = true; }, "Competitor");
				GenericTestLogsAddedForCompanyTypeFlagChange(header, () => { header.OH_IsSalesLead = false; }, () => { header.OH_IsSalesLead = true; }, "Sales");
			});
		}

		public void GenericTestLogsAddedForCompanyTypeFlagChange(OrgHeader header, Action setIsOrgTypeFalse, Action setIsOrgTypeTrue, string description)
		{
			AssertEquals(message: $"'{description} Flag Un-Ticked' log not expected", 0, header.Logs.Find(x => x.SL_Reference == $"{description} Flag Un-Ticked").Count());
			AssertEquals(message: $"'{description} Flag Ticked' log not expected", 0, header.Logs.Find(x => x.SL_Reference == $"{description} Flag").Count());

			setIsOrgTypeTrue();
			Factory.Save();
			AssertEquals(message: $"'{description} Flag Ticked' log expected", 1, header.Logs.Find(x => x.SL_Reference == $"{description} Flag Ticked").Count());

			setIsOrgTypeFalse();
			Factory.Save();
			AssertEquals(message: $"'{description} Flag Un-Ticked' log expected", 1, header.Logs.Find(x => x.SL_Reference == $"{description} Flag Un-Ticked").Count());
		}

		#endregion ModifyCompanyTypeFlags

		#region IEInvoicingEligibilityLiteOrgHeader

		public void TestIEInvoicingEligibilityLiteOrgHeaderMembers()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var asEligibility = (IEInvoicingEligibilityLiteOrgHeader)orgHeader;
			AssertEquals("CustomsCodes / RegistrationCodes", orgHeader.CustomsCodes.Count, asEligibility.RegistrationCodes.Count);
			AssertEquals("CustomsCodes / RegistrationCodes", 0, asEligibility.RegistrationCodes.Count);

			orgHeader.CustomsCodes.AddNew("999", "SomeRegistrationNumber", "ZZ");
			AssertEquals("CustomsCodes / RegistrationCodes", 1, asEligibility.RegistrationCodes.Count);

			orgHeader.CustomsCodes.AddNew("GST", "SomeGSTNumber", "AU");
			AssertEquals("CustomsCodes / RegistrationCodes", 2, asEligibility.RegistrationCodes.Count);
		}

		public void TestIEInvoicingEligibilityLiteOrgHeader_Category()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var asEligibility = (IEInvoicingEligibilityLiteOrgHeader)orgHeader;

			AssertEquals("OH_Category / Category", orgHeader.OH_Category, asEligibility.Category);

			orgHeader.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Category should be GOV", OrgConstants.Category.Government, asEligibility.Category);

			orgHeader.OH_Category = "___";
			AssertEquals("Category should be ___", "___", asEligibility.Category);
		}

		public void TestIEInvoicingEligibilityLiteOrgHeader_HasAnyRegistrationCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var asEligibility = (IEInvoicingEligibilityLiteOrgHeader)orgHeader;
			orgHeader.CustomsCodes.AddNew("999", "SomeRegistrationNumber", "ZZ");
			orgHeader.CustomsCodes.AddNew("GST", "SomeGSTNumber", "AU");
			orgHeader.CustomsCodes.AddNew("999", "", "AU");

			Assert("Contains GST code for AU country", asEligibility.HasAnyRegistrationCode("GST", "AU"));
			Assert("Contains 999 code for ZZ country", asEligibility.HasAnyRegistrationCode("999", "ZZ"));
			Assert("Does not contain GST code for ZZ country", !asEligibility.HasAnyRegistrationCode("GST", "ZZ"));
			Assert("Does not contain VAT code for ZZ country", !asEligibility.HasAnyRegistrationCode("VAT", "ZZ"));
			Assert("Does not contain VAT code for NZ country", !asEligibility.HasAnyRegistrationCode("VAT", "NZ"));
			Assert("Does not contain 999 code for AU country", !asEligibility.HasAnyRegistrationCode("999", "AU"));
		}

		#endregion

		#region IDocAddresses

		public void TestSupportedAddressType()
		{
			var orgHeader = Factory.New<OrgHeader>();
			IDocAddresses addresses = orgHeader;

			AssertEquals(orgHeader.NotifyPartyDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.NotifyParty).DefaultDocAddressType);
			AssertEquals(orgHeader.MasterBillShipperOverrideDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.MasterBillShipperOverride).DefaultDocAddressType);
			AssertEquals(orgHeader.MasterBillConsigneeOverrideDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.MasterBillConsigneeOverride).DefaultDocAddressType);

			var supportedAddressTypes = addresses.SupportedAddressTypes;

			AssertCollectionContains("should support notify party", DocAddressType.NotifyParty, supportedAddressTypes);
			AssertCollectionContains("should support master bill shipper override", DocAddressType.MasterBillShipperOverride, supportedAddressTypes);
			AssertCollectionContains("should support master bill consignee override", DocAddressType.MasterBillConsigneeOverride, supportedAddressTypes);
		}

		public void TestNotifyPartyAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			AssertEquals(false, orgHeader.NotifyPartyDocumentaryAddress.IsValidAddress);
			AssertEquals(null, orgHeader.NotifyParty);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			orgHeader.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, orgHeader.NotifyPartyDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, orgHeader.NotifyParty);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, orgHeader.NotifyPartyDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, orgHeader.NotifyParty);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			orgHeader.NotifyPartyDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, orgHeader.NotifyPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, orgHeader.NotifyPartyDocumentaryAddress.ContactPK);
		}

		public void TestMasterBillShipperOverrideAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			AssertEquals(false, orgHeader.MasterBillShipperOverrideDocumentaryAddress.IsValidAddress);
			AssertEquals(null, orgHeader.MasterBillShipperOverride);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			orgHeader.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, orgHeader.MasterBillShipperOverrideDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, orgHeader.MasterBillShipperOverride);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, orgHeader.MasterBillShipperOverrideDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, orgHeader.MasterBillShipperOverride);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			orgHeader.MasterBillShipperOverrideDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, orgHeader.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, orgHeader.MasterBillShipperOverrideDocumentaryAddress.ContactPK);
		}

		public void TestMasterBillConsigneeOverrideAdress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			AssertEquals(false, orgHeader.MasterBillConsigneeOverrideDocumentaryAddress.IsValidAddress);
			AssertEquals(null, orgHeader.MasterBillConsigneeOverride);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			orgHeader.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, orgHeader.MasterBillConsigneeOverrideDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, orgHeader.MasterBillConsigneeOverride);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, orgHeader.MasterBillConsigneeOverrideDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, orgHeader.MasterBillConsigneeOverride);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			orgHeader.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, orgHeader.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, orgHeader.MasterBillConsigneeOverrideDocumentaryAddress.ContactPK);
		}

		#endregion

		#region OrgRefFacility

		public void TestOrgRefFacility()
		{
			var org = Factory.New<OrgHeader>();
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			var orgRefFacility = Factory.New<OrgRefFacility>();

			org.OH_Code = "TSTTSTTST";
			refFacility.RFT_Code = "00000000001";
			refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility.OFC_OH_Organization = org.PK;
			orgRefFacility.OFC_RFT_Facility = refFacility.PK;
			var orgRefFacilityResults = org.OrgRefFacilities;
			AssertEquals("Facility should be the same", orgRefFacility, orgRefFacilityResults[0]);
		}

		#endregion

		#region AccExchangeRateConfigurations

		public void TestCascadeDeleteExchangeRateConfigurations()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var company = Factory.New<GlbCompany>();
			company.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			debtorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			creditorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var orgHeader = testObjectCreator.CreateOrgHeader("DMO", true, true);
			orgHeader.CompanyData.AccAPExchangeRateConfigurations.SetExRate("AP", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			orgHeader.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var anotherOrgHeader = testObjectCreator.CreateOrgHeader("DMY", true, true);
			anotherOrgHeader.CompanyData.AccAPExchangeRateConfigurations.SetExRate("AP", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.SellRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			anotherOrgHeader.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.SellRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			Factory.Save();

			var systemLevelPK = systemLevelExchangeRateConfigs.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.System).PK;
			var companyLevelPK = company.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Company).PK;
			var debtorGroupPK = debtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup).PK;
			var creditorGroupPK = creditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup).PK;
			var orgHeaderARPK = orgHeader.CompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Debtor).PK;
			var orgHeaderAPPK = orgHeader.CompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Creditor).PK;
			var anotherOrgHeaderARPK = anotherOrgHeader.CompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Debtor).PK;
			var anotherOrgHeaderAPPK = anotherOrgHeader.CompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Creditor).PK;

			var queryForExRateConfig = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
										.AddToFilter(new ZQuery(AccExchangeRateConfigurationViewSchema.PK,
														new List<ZGuid>() { systemLevelPK, companyLevelPK, debtorGroupPK, creditorGroupPK, orgHeaderARPK, orgHeaderAPPK, anotherOrgHeaderARPK, anotherOrgHeaderAPPK }));
			var exRateConfigCount = Factory.Load<AccExchangeRateConfiguration>(queryForExRateConfig);
			AssertEquals(8, exRateConfigCount.Length);

			orgHeader.CompanyData.AccARExchangeRateConfigurations.Reload(true);
			orgHeader.CompanyData.AccAPExchangeRateConfigurations.Reload(true);
			orgHeader.Delete();
			Factory.Save();

			var queryForExRateConfigExceptOrganizationLevel = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
										.AddToFilter(new ZQuery(AccExchangeRateConfigurationViewSchema.PK,
														new List<ZGuid>() { systemLevelPK, companyLevelPK, debtorGroupPK, creditorGroupPK, anotherOrgHeaderARPK, anotherOrgHeaderAPPK }));
			exRateConfigCount = Factory.Load<AccExchangeRateConfiguration>(queryForExRateConfigExceptOrganizationLevel);
			AssertEquals(false, Factory.Exists(typeof(AccExchangeRateConfiguration),
							new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
								.AddToFilter(AccExchangeRateConfigurationViewSchema.PK, new List<ZGuid>() { orgHeaderARPK, orgHeaderAPPK }), true));
			AssertEquals(6, exRateConfigCount.Length);
		}

		#endregion

		#region Implementation

		ZGuid CreateDeclaration(OrgAddress address)
		{
			var job = (BusinessObject)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			job[JobDeclarationSchema.JE_GB] = branch.PK;

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress[JobDocAddressSchema.E2_OA_Address] = address.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentID] = job.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentTableCode] = "JE";
			Factory.Save();

			return job.PK;
		}

		ZGuid CreateShipment(OrgAddress address)
		{
			var job = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_OA_LocalChargesAddr] = address.PK;
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			Factory.Save();

			return job.PK;
		}

		ZGuid CreateConsol(OrgAddress address)
		{
			var job = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress[JobDocAddressSchema.E2_OA_Address] = address.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentID] = job.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentTableCode] = "JK";
			Factory.Save();

			return job.PK;
		}

		OrgHeader company;

		protected override void SetUp()
		{
			base.SetUp();
			company = Factory.New<OrgHeader>();
			company.OH_FullName = "Test Organisation Pty Limited";
			company.OH_RL_NKClosestPort = "AUSYD";
			company.OH_Code = "DUUUUH";
			company.MainAddress.OA_Address1 = "This should have been updated";
			company.MainAddress.OA_City = "A city";
			company.MainAddress.OA_CompanyNameOverride = "Test Organisation Different Name";
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = "OrgForDelete";
			return org;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return BasicCompanyForTest;
		}

		RefUNLOCO GetUNLOCO(string code)
		{
			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, code);
			RefUNLOCO unloco = Factory.LoadTop1<RefUNLOCO>(filter);
			return unloco;
		}

		OrgHeader CreateOrgToTestDelete(BusinessObjectFactory factory)
		{
			OrgHeader company = factory.New<OrgHeader>();
			company.OH_RL_NKClosestPort = "AUSYD";
			company.OH_FullName = "Test Org";
			company.MainAddress.OA_Address1 = "Test Org Address";
			company.OH_Code = "DDDOOOHHH";

			var airline = factory.NewWithValidTestData<RefAirline>();
			company.MiscServ.OM_RM_Airline = airline.PK;

			OrgAddress address2 = company.Addresses.AddNew();
			address2.OA_Address1 = "Test Address";

			company.Contacts.AddNew();

			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			company.SalesCollection.AddNew();
			company.SalesCollection[0].OW_OH_Buyer = company.PK;
			company.SalesCollection[0].OW_OriginID = uslax.PK;
			company.SalesCollection[0].TradeDetails.AddNew();

			company.SalesCalls.AddNew();

			OrgHeader supplier = factory.New<OrgHeader>();
			supplier.OH_FullName = "Test Supplier";
			supplier.MainAddress.OA_Address1 = "Test address 1";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			company.SupplierLinks.AddNew();
			company.SupplierLinks[0].OL_OH_Supplier = supplier.PK;
			company.BuyerLinks.AddNew();
			company.BuyerLinks[0].OL_OH_Buyer = supplier.PK;
			company.CompanyData.InvoiceTypes.AddNew();

			return company;
		}

		OrgHeader BasicCompanyForTest
		{
			get
			{
				company = Factory.New<OrgHeader>();
				company.OH_FullName = "Test Organisation Pty Limited";
				company.OH_Code = "DUUUUH";
				company.MainAddress.OA_Address1 = "This should have been updated";
				company.MainAddress.OA_City = "A city";

				return company;
			}
		}

		OrgHeader TempOrg
		{
			get
			{
				if (tempOrg == null)
				{
					tempOrg = Factory.New<OrgHeader>();
					tempOrg.SetDefaultValuesForTemporaryOrganisation();
				}

				return tempOrg;
			}
		}

		OrgHeader tempOrg;

		StmALog[] LoadExistingDDRLogs(ZGuid pk)
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, pk);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DuplicateDetectedForReviewCode);
			return Factory.Load<StmALog>(filter);
		}

		void InitDedupOrgInfo(out OrgHeader orgHeader, out OrgHeader orgTarget1, out OrgHeader orgTarget2, out List<CargoWise.Glow.Model.Interfaces.IOrgHeader> targetLists, out List<ScoringResult> scoreResults)
		{
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgTarget1 = Factory.NewWithValidTestData<OrgHeader>();
			orgTarget2 = Factory.NewWithValidTestData<OrgHeader>();
			targetLists = ObjectFactory.Get<IMasterDataProvider>().GetTargetLists(orgTarget1, orgTarget2);
			scoreResults = new List<ScoringResult>()
			{
				new ScoringResult()
				{
					MasterPK = orgHeader.PK.ToGuid(),
					TargetPK = orgTarget1.PK.ToGuid(),
					Score = 0.76
				},
				new ScoringResult()
				{
					MasterPK = orgHeader.PK.ToGuid(),
					TargetPK = orgTarget2.PK.ToGuid(),
					Score = 0.91
				}
			};
		}

		AccTransactionHeader CreateARInvoice(ZDecimal amount, OrgHeader org, ZInt? overdueDays, ZGuid companyPK, ZString transactionNumber)
		{
			var branch = Factory.Load<GlbCompany>(companyPK).FirstActiveBranch;
			var invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_Desc = "Test Invoice";
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_ExchangeRate = 1;
			invoice.AH_OH = org.PK;
			invoice.AH_TransactionNum = transactionNumber;
			invoice.AH_Ledger = "AR";
			invoice.AH_InvoiceDate = DateTime.Now;
			invoice.AH_GB = branch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_OutstandingAmount = amount;
			invoice.AH_InvoiceAmount = amount;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_TransactionCategory = "";
			invoice.AH_DueDate = overdueDays.HasValue ? ZDateTime.Now.AddDays(-overdueDays.Value) : ZDateTime.Empty;

			var line = Factory.New<AccTransactionLines>();
			line.AL_AH = invoice.PK;
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_LineAmount = amount;
			line.AL_OSAmount = amount;
			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_ExchangeRate = 1;
			line.AL_OSAmount = amount;
			line.AL_Desc = "tee he he";
			line.AL_GB = branch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			return invoice;
		}

		AccQueryClaim CreateClaim(AccTransactionHeader invoice, string claimStatus, decimal claimAmount)
		{
			var claim = (AccQueryClaim)Factory.New<IARAccQueryClaim>();
			claim.FillWithValidTestData();
			claim.AY_AH = invoice.PK;
			claim.AY_GB = invoice.AH_GB;
			claim.AY_OH_Debtor = invoice.AH_OH;
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_OH = invoice.AH_OH;
			claim.AY_OC = orgContact.PK;
			claim.AY_QueryClaimAmount = claimAmount;
			claim.AY_QueryClaimStatus = claimStatus;
			return claim;
		}

		#endregion

		public void TestGetCusCodeDataTypes()
		{
			var types = (company as Enterprise.Integration.Customs.ICusCodeDataTypeSupporter).GetCusCodeDataTypes();
			CombineAssertions(() =>
			{
				AssertEquals(3, types.Count);
				AssertEquals("Enterprise.Customs.CA.Business.FreightPercentage", types[OrgHeader.CusCodeDataTypeList.Codes.FreightPercentage].ToString());
				AssertEquals("Enterprise.Customs.CA.Business.SafeFoodLicense", types[OrgHeader.CusCodeDataTypeList.Codes.SafeFoodLicense].ToString());
				AssertEquals("Enterprise.Customs.BR.Business.AdditionalIdentification", types[OrgHeader.CusCodeDataTypeList.Codes.AdditionalIdentification].ToString());
			});
		}

		public void TestThrowZCannotSaveExceptionWhenOH_CodeIsEmpty()
		{
			var newFactory = new BusinessObjectFactory();
			var org = newFactory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = ZString.Empty;
			AssertExceptionThrown<ZCannotSaveException>("Empty OH_Code", "Organization code is empty, and could not be calculated using the code generation algorithm setup in the registry under Master Data -> Organizations -> Codes -> Organization Code - Default Set. The algorithm may be setup incorrectly, or insufficient information was entered for a code to be generated.", () => newFactory.Save());
		}

		public void TestSuppressedDocumentsShouldBeDeletedWhenOrgHeaderIsDeleted()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var document1 = orgHeader.SuppressedDocuments.AddNew();
			var document2 = orgHeader.SuppressedDocuments.AddNew();
			document1.OD_DocumentGroup = ContactType.Consignee.Code;
			document2.OD_DocumentGroup = ContactType.Consignor.Code;
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var orgHeader1 = factory1.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgHeader.PK));
			AssertEquals("OrgHeader.SuppressedDocumentCollection count should be 2", 2, factory1.GetDatabaseCount(typeof(OrgDocument), new ZQuery(OrgDocumentSchema.OD_OH_Suppressed, orgHeader.PK)));
			orgHeader1.Delete();
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			AssertEquals("OrgHeader has been deleted", 0, factory2.GetDatabaseCount(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, orgHeader.PK)));
			AssertEquals("OrgHeader.SuppressedDocumentCollection count should be 0", 0, factory2.GetDatabaseCount(typeof(OrgDocument), new ZQuery(OrgDocumentSchema.OD_OH_Suppressed, orgHeader.PK)));
		}

		public void TestSuppressedDocumentsWithDummyContact()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var doc = orgHeader.SuppressedDocuments.AddNew();

			var dummyContact = Factory.NewWithValidTestData<OrgContact>();
			dummyContact.OC_ContactName = "DUMMY CONTACT TO SUPPRESS DOCS";
			dummyContact.OC_NotifyMode = "DND";
			dummyContact.OC_OH = orgHeader.PK;

			var dummyDocument = Factory.NewWithValidTestData<OrgDocument>();
			dummyContact.Documents.Add(dummyDocument);
			Factory.Save();

			AssertEquals(true, orgHeader.SuppressedDocumentsIncludeDummyContact.Contains(dummyDocument));
			AssertEquals(true, orgHeader.SuppressedDocumentsIncludeDummyContact.Contains(doc));

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#region IRegisterStatusChangeContext

		public void TestTemporarilySetStatusChangedByTriggerEvent()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			Assert(!orgHeader.IsChangingByWorkflowTrigger);

			using ((orgHeader as IRegisterStatusChangeContext).TemporarilySetStatusChangedByTriggerEvent(null))
			{
				Assert(orgHeader.IsChangingByWorkflowTrigger);
			}

			Assert(!orgHeader.IsChangingByWorkflowTrigger);
		}

		#endregion
	}
}
