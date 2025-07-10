using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgAddressDefaultFinderTest : TestCaseWithFactory
	{
		public void TestNonMainAddressesAreIgnored()
		{
			using (TestHelper th = new TestHelper(Factory))
			{
				th.AddAddress(OrgAddressType.Receivables, false, Constants.Languages.English);
				th.OrgProxy.OH_Language = Constants.Languages.English;
				th.Organisation.OH_Language = Constants.Languages.English;
				AssertEquals("DefaultAddressOfType(OrgAddressType.Receivables)", "(no match found)", th.DefaultAddressOfType(OrgAddressType.Receivables));
			}
		}

		public void TestInactiveAddressesAreIgnored()
		{
			using (TestHelper th = new TestHelper(Factory))
			{
				th.OrgProxy.OH_Language = Constants.Languages.English;
				th.Organisation.OH_Language = Constants.Languages.English;

				var address = th.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.English);

				address.OA_IsActive = false;
				AssertEquals("Should ignore inactive address", "(no match found)", th.DefaultAddressOfType(OrgAddressType.Receivables));

				address.OA_IsActive = true;
				AssertEquals("Control - should find active address", "ARM Main EN", th.DefaultAddressOfType(OrgAddressType.Receivables));
			}
		}

		public void TestGetDefaultAddressOfTypeByOrder()
		{
			using (var testHelper = new TestHelper(Factory))
			{
				var address1 = testHelper.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.English);
				address1.OA_SystemCreateTimeUtc = DateTime.Now.AddDays(-1);
				address1.OA_Address1 = "Address 1";

				var address2 = testHelper.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.English);
				address2.OA_SystemCreateTimeUtc = DateTime.Now.AddDays(-1).AddHours(1);
				address2.OA_Address1 = "Address 2";

				AssertEquals("DefaultAddressOfType should return older address when there are multiple address met the condition.", "Address 1", testHelper.DefaultAddressOfType(OrgAddressType.Receivables));

				address1.OA_SystemCreateTimeUtc = DateTime.Now.AddDays(-1).AddHours(2);
				AssertEquals("DefaultAddressOfType should return older address when there are multiple address met the condition.", "Address 2", testHelper.DefaultAddressOfType(OrgAddressType.Receivables));
			}
		}

		public void TestCommonLanguageChangesLanguageSelectionButFallsBackToEnglish()
		{
			using (TestHelper th = new TestHelper(Factory))
			{
				th.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.English);
				th.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.Finnish);
				th.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.Dutch);
				th.AddAddress(OrgAddressType.Payables, true, Constants.Languages.English);
				th.AddAddress(OrgAddressType.Payables, true, Constants.Languages.Dutch);

				th.OrgProxy.OH_Language = Constants.Languages.Finnish;
				th.Organisation.OH_Language = Constants.Languages.Finnish;
				AssertEquals("DefaultAddressOfType(OrgAddressType.Payables)", "APM Main EN", th.DefaultAddressOfType(OrgAddressType.Payables));
				AssertEquals("DefaultAddressOfType(OrgAddressType.Receivables)", "ARM Main FI-FI", th.DefaultAddressOfType(OrgAddressType.Receivables));

				th.OrgProxy.OH_Language = Constants.Languages.English;
				th.Organisation.OH_Language = Constants.Languages.English;
				AssertEquals("DefaultAddressOfType(OrgAddressType.Payables)", "APM Main EN", th.DefaultAddressOfType(OrgAddressType.Payables));
				AssertEquals("DefaultAddressOfType(OrgAddressType.Receivables)", "ARM Main EN", th.DefaultAddressOfType(OrgAddressType.Receivables));
			}
		}

		public void TestDoesNotHitDatabaseIfCapabilityCollectionHasBeenLoaded()
		{
			using (var th = new TestHelper(Factory))
			{
				var address = th.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.English);
				th.Organisation.FillWithValidTestData();
				address.FillWithValidTestData();
				Factory.Save();

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var organisationInFactory2 = factory2.Load<OrgHeader>(th.Organisation.PK);
				var addressInFactory2 = factory2.Load<OrgAddress>(address.PK);

				foreach (var a in organisationInFactory2.AddressesActive)
				{
					var poke = a.AddressCapability; // Loads capabilities in constructor
				}

				AssertEquals("Precondition", 1, factory2.GetTableHitCount(OrgAddressCapabilitySchema.Constants.TableName));

				new OrgAddressDefaultFinder(organisationInFactory2.AddressesActive, true, null).DefaultAddressOfType(OrgAddressType.Payables);
				AssertEquals("Should not have hit db again", 1, factory2.GetTableHitCount(OrgAddressCapabilitySchema.Constants.TableName));
			}
		}

		public void TestFallsBackToEnglishWhereAvailableIfNoCommonLanguage()
		{
			using (TestHelper th = new TestHelper(Factory))
			{
				th.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.English);
				th.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.Finnish);
				th.AddAddress(OrgAddressType.Receivables, true, Constants.Languages.Dutch);
				th.AddAddress(OrgAddressType.Payables, true, Constants.Languages.English);
				th.AddAddress(OrgAddressType.Payables, true, Constants.Languages.Dutch);

				th.OrgProxy.OH_Language = Constants.Languages.English;
				th.Organisation.OH_Language = Constants.Languages.Finnish;
				AssertEquals("DefaultAddressOfType(OrgAddressType.Payables)", "APM Main EN", th.DefaultAddressOfType(OrgAddressType.Payables));
				AssertEquals("DefaultAddressOfType(OrgAddressType.Receivables)", "ARM Main EN", th.DefaultAddressOfType(OrgAddressType.Receivables));

				th.OrgProxy.OH_Language = Constants.Languages.Finnish;
				th.Organisation.OH_Language = Constants.Languages.English;
				AssertEquals("DefaultAddressOfType(OrgAddressType.Payables)", "APM Main EN", th.DefaultAddressOfType(OrgAddressType.Payables));
				AssertEquals("DefaultAddressOfType(OrgAddressType.Receivables)", "ARM Main EN", th.DefaultAddressOfType(OrgAddressType.Receivables));
			}
		}

		public void TestAddressInLanguageOfDestinationOrgWillMatchIfTheCommonLanguageIsEnglishAndThereIsNoOtherMatch()
		{
			using (TestHelper th = new TestHelper(Factory))
			{
				th.AddAddress(OrgAddressType.Pickup, true, Constants.Languages.Dutch);
				th.AddAddress(OrgAddressType.Pickup, true, Constants.Languages.Finnish);

				th.OrgProxy.OH_Language = Constants.Languages.English;
				th.Organisation.OH_Language = Constants.Languages.Dutch;
				AssertEquals("DefaultAddressOfType(OrgAddressType.Pickup)", "PIC Main NL-NL", th.DefaultAddressOfType(OrgAddressType.Pickup));

				th.OrgProxy.OH_Language = Constants.Languages.English;
				th.Organisation.OH_Language = Constants.Languages.English;
				AssertEquals("DefaultAddressOfType(OrgAddressType.Pickup)", "PIC Main FI-FI", th.DefaultAddressOfType(OrgAddressType.Pickup));
			}
		}

		public void TestGlobalAccountsTakePrecedenceOverEvenTheBestLanguageMatch()
		{
			string saveHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			try
			{
				GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort = "GBMNC";
				Factory.Save();

				GlbCompany.CurrentCompany.SetCountry("GB");
				using (TestHelper th = new TestHelper(Factory))
				{
					th.Organisation.OH_RL_NKClosestPort = "THBKK";
					var engAddress = th.AddAddress(OrgAddressType.Payables, true, Constants.Languages.English);
					engAddress.OA_RL_NKRelatedPortCode = "GBLON";
					engAddress.OA_Address1 = "APM Main EN GBLON";
					var finAddress = th.AddAddress(OrgAddressType.Payables, true, Constants.Languages.Finnish);
					finAddress.OA_RL_NKRelatedPortCode = "";
					finAddress.OA_RN_NKCountryCode = "";

					th.OrgProxy.OH_Language = Constants.Languages.Finnish;
					th.Organisation.OH_Language = Constants.Languages.Finnish;
					AssertEquals("DefaultAddressOfType(OrgAddressType.Pickup)", "APM Main FI-FI", th.DefaultAddressOfType(OrgAddressType.Payables));

					th.Organisation.OH_IsGlobalAccount = true;
					AssertEquals("DefaultAddressOfType(OrgAddressType.Pickup)", "APM Main FI-FI", th.DefaultAddressOfType(OrgAddressType.Payables));

					th.Organisation.OH_IsShippingProvider = true;
					AssertEquals("DefaultAddressOfType(OrgAddressType.Pickup)", "APM Main EN GBLON", th.DefaultAddressOfType(OrgAddressType.Payables));

					OrgAddress engAddressInPort = th.AddAddress(OrgAddressType.Payables, true, Constants.Languages.English);
					engAddressInPort.OA_RL_NKRelatedPortCode = "GBMNC";
					engAddressInPort.OA_Address1 = "APM Main EN GBMNC";

					AssertEquals("DefaultAddressOfType(OrgAddressType.Pickup)", "APM Main EN GBMNC", th.DefaultAddressOfType(OrgAddressType.Payables));
				}
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = saveHomePort;
			}
		}

		public void TestDefaultAddressOfType_DoesNotThrow_IfGlbBranchIsNull()
		{
			var userContextMock = new Mock<IUserContext>();
			var userContext = Env.CurrentUserContext;
			userContextMock
				.SetupGet(context => context.Company)
				.Returns(userContext.Company);

			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "NZAKL";
			header.OH_IsGlobalAccount = ZBool.True;
			header.OH_IsShippingProvider = ZBool.True;

			var address = Factory.New<OrgAddress>();
			address.OA_RL_NKRelatedPortCode = "AUBNE";
			address.OA_Language = Constants.Languages.Albanian;
			header.Addresses.Add(address);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Sales.Code);

			using (Env.SetTemporaryUserContext(userContextMock.Object))
			{
				AssertNull("Precondition: ", GlbBranch.CurrentBranch);
				AssertNoExceptionThrown(() => { header.Addresses.DefaultAddressOfType(OrgAddressType.Office); });
			}
		}

		public void TestDefaultAddressOfType_DoesNotThrow_IfGlbCompanyIsNull()
		{
			var userContextMock = new Mock<IUserContext>();
			var userContext = Env.CurrentUserContext;
			userContextMock
				.SetupGet(context => context.Branch)
				.Returns(userContext.Branch);

			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "NZAKL";
			header.OH_IsGlobalAccount = ZBool.True;
			header.OH_IsShippingProvider = ZBool.True;

			var address = Factory.New<OrgAddress>();
			address.OA_RL_NKRelatedPortCode = "AUBNE";
			address.OA_Language = Constants.Languages.Albanian;
			header.Addresses.Add(address);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Sales.Code);

			using (Env.SetTemporaryUserContext(userContextMock.Object))
			{
				AssertNull("Precondition: ", GlbCompany.CurrentCompany);
				AssertNoExceptionThrown(() => { header.Addresses.DefaultAddressOfType(OrgAddressType.Office); });
			}
		}

		#region Implementation

		class TestHelper : IDisposable
		{
			internal TestHelper(BusinessObjectFactory factory)
			{
				OrgProxy = GlbCompany.CurrentCompany.OrgProxy;
				AssertNotNull("Precondition: GlbCompany.CurrentCompany.OrgProxy must be setup.", OrgProxy);
				originalOrgProxyLanguage = OrgProxy.OH_Language;

				Organisation = factory.New<OrgHeader>();
				Organisation.MainAddress.OA_Address1 = "Main Address EN";
				Organisation.AddressesActive.ApplySort(OrgAddress.Schema.OA_Language, System.ComponentModel.ListSortDirection.Ascending);
			}

			internal readonly OrgHeader OrgProxy;
			internal readonly OrgHeader Organisation;
			readonly string originalOrgProxyLanguage;

			void IDisposable.Dispose()
			{
				if (originalOrgProxyLanguage != null)
				{
					OrgProxy.OH_Language = originalOrgProxyLanguage;
				}
			}

			internal string DefaultAddressOfType(OrgAddressType addressType)
			{
				OrgAddress result = new OrgAddressDefaultFinder(Organisation.AddressesActive, true, null).DefaultAddressOfType(addressType);
				return result != null ? result.OA_Address1.ToString() : "(no match found)";
			}

			internal OrgAddress AddAddress(OrgAddressType addressType, bool isMain, string languageCode)
			{
				OrgAddress address = Organisation.Addresses.AddNew();
				address.AddressCapability.SetCapabilityEnabled(addressType.Code);
				if (isMain)
				{
					address.AddressCapability.SetIsMainAddress(addressType.Code);
				}
				else
				{
					address.AddressCapability.SetIsNotMainAddress(addressType.Code);
				}
				address.OA_Language = languageCode;
				address.OA_Address1 = addressType.ToString() + (isMain ? " Main " : " Secondary ") + languageCode;
				return address;
			}
		}

		#endregion
	}
}
