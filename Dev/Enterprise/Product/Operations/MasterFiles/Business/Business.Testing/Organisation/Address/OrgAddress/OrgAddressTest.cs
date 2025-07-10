using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddress))]
	sealed class OrgAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNull()
		{
			using (OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TRL"))
			using (OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PSL"))
			using (OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "HWL"))
			{
				var address = Factory.GetNull<OrgAddress>();
				AssertNullOrEmpty(address.OA_RN_NKCountryCode);
				AssertNullOrEmpty(address.OA_FCLEquipmentNeeded);
				AssertNullOrEmpty(address.OA_LCLEquipmentNeeded);
				AssertNullOrEmpty(address.OA_AIREquipmentNeeded);
			}
		}

		public void TestOaAdditionalAddressInfoMaxLengthShouldEqualToOAI_Address()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Test";

			AssertEquals(orgAddress.OA_AdditionalAddressInformationInfo.MaxLength, orgAddress.PrimaryOrgAddressAdditionalInfo.OAI_AdditionalInfoInfo.MaxLength);
		}

		public void TestCountryCode_MacroIgnoreAndObsolete()
		{
			CombineAssertions(() =>
			{
				var propertyInfo = typeof(OrgAddress).GetProperty("CountryCode");
				AssertNotNull("MacroIgnoreAttribute", propertyInfo.GetCustomAttribute<MacroIgnoreAttribute>());
				AssertNotNull("ObsoleteAttribute", propertyInfo.GetCustomAttribute<ObsoleteAttribute>());
			});
		}

		public void TestIsOverrideJobLoadingDuration()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			AssertEquals(false, address.IsOverridenJobLoadingDuration);
			address.OA_JobLoadingDuration = 60;
			AssertEquals(true, address.IsOverridenJobLoadingDuration);
			address.IsOverridenJobLoadingDuration = false;
			var defaultJobLoadingFixedDurationValue = ObjectFactory.Get<ILandTransportRegistry>().DefaultJobLoadingFixedDuration.Value;
			AssertEquals(defaultJobLoadingFixedDurationValue, address.OA_JobLoadingDuration);
		}

		public void TestOverrideJobLoadingDuration()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var defaultJobLoadingFixedDurationValue = ObjectFactory.Get<ILandTransportRegistry>().DefaultJobLoadingFixedDuration.Value;
			AssertEquals(defaultJobLoadingFixedDurationValue, address.OA_JobLoadingDuration);
			address.OA_JobLoadingDuration = 16;
			CombineAssertions("Job Loading Durations value is what we set, status of Override also should be ture", () =>
			{
				AssertEquals(16, address.OA_JobLoadingDuration);
				AssertEquals(true, address.IsOverridenJobLoadingDuration);
			});
		}

		public void TestOA_JobLoadingDuration_ReadOnly()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			AssertEquals("Pre-condition:IsOverridenJobLoadingDuration is false", false, address.IsOverridenJobLoadingDuration);
			AssertEquals(true, address.OA_JobLoadingDuration_ReadOnly);
			address.IsOverridenJobLoadingDuration = true;
			AssertEquals(false, address.OA_JobLoadingDuration_ReadOnly);
		}

		public void TestDedupIsCalledAddressStatusIsCNA()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = OrgInDB;
			org.DeduplicationStarted += delegate
			{
				Assert("Should get in here because dedup will be called, otherwise this is an empty test.", true);
			};
			((IDeduplicatable)org).ShouldRunDeduplication = true;
			Env.Security.OrganisationModify.IsAllowed = true;
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var cnaAddress = org.MainAddress;
				cnaAddress.IsSuspendingDeduplication = false;
				cnaAddress.OA_ValidationStatus = AddressValidationStatus.CountryNotAvailable;
			}
		}

		public void TestGetRefCountryStatesFromCodeOrDesc()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "COL";
			refCountryStates1.RW_RN_NKCountryCode = "CN";
			refCountryStates1.RW_Description = "COLIMA";
			Factory.Save();

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = "CN";
			address.OA_State = "COLIMA";
			AssertEquals("COL", ((IAddressDetails)address).State);
			address.OA_State = "COL";
			AssertEquals("COL", ((IAddressDetails)address).State);
			address.OA_State = "XXX";
			AssertEquals("XXX", ((IAddressDetails)address).State);
		}

		public void TestValidateState()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "COL";
			refCountryStates1.RW_RN_NKCountryCode = "CN";
			refCountryStates1.RW_Description = "COLIMA";
			Factory.Save();

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = "CN";
			address.OA_State = "COLIMA";
			Assert(address.ValidateState().IsEmpty);
			address.OA_State = "COL";
			Assert(address.ValidateState().IsEmpty);
			address.OA_State = "XXX";
			Assert(!address.ValidateState().IsEmpty);
		}

		#region TestConcurrencyExceptionSuspendValidation

		public void TestGetSystemLastEditUserForConcurrencyWhenDeleted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			Factory.Save();

			var newFactory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};

			var orgAddressInNewFactory = newFactory.Load<OrgAddress>(orgAddress.PK);
			orgAddressInNewFactory.OA_Address1 = "address 1";
			newFactory.Save();

			try
			{
				orgAddress.Delete();
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());
		}

		public void TestConcurrencyExceptionSuspendValidation()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};
			var orgAddressInNewFactory = newFactory.Load<OrgAddress>(orgAddress.PK);

			orgAddress.OA_Address1 = "New Address For Test";
			Factory.Save();

			orgAddressInNewFactory.OA_AdditionalAddressInformation = "New Additional Address Info";
			Assert(!orgAddressInNewFactory.IsValidationSuspended);

			try
			{
				newFactory.Save();
				Assert("Should not be here", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert(orgAddressInNewFactory.IsValidationSuspended);

			orgAddressInNewFactory.OA_Address1 = "New Address In New Factory";
			Assert(!orgAddressInNewFactory.IsValidationSuspended);

			ErrorReporter.Clear();
		}

		public void TestConcurrencyExceptionNotResumeValidation()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};
			var orgAddressInNewFactory = newFactory.Load<OrgAddress>(orgAddress.PK);

			orgAddress.OA_Address1 = "New Address For Test";
			Factory.Save();

			orgAddressInNewFactory.OA_AdditionalAddressInformation = "New Additional Address Info";
			orgAddressInNewFactory.SuspendValidation();
			Assert(orgAddressInNewFactory.IsValidationSuspended);

			try
			{
				newFactory.Save();
				Assert("Should not be here", false);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert(orgAddressInNewFactory.IsValidationSuspended);

			orgAddressInNewFactory.OA_Address1 = "New Address In New Factory";
			Assert(orgAddressInNewFactory.IsValidationSuspended);

			ErrorReporter.Clear();
		}

		#endregion

		public void TestOALanguage()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Language = "CHS";
			AssertEquals("Old language code should be corrected after setting", Core.SharedConstants.Languages.ChineseSimplified, address.OA_Language);
		}

		public void TestSetApprovedTSAKnownShipperToNotApprovedWhenAddressInfoChanged()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var countryData1 = Factory.New<OrgCountryData>();
			countryData1.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
			countryData1.OV_OA_ApprovedLocation = org1.MainAddress.PK;
			countryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			countryData1.OV_OH_OrgHeader = org1.PK;

			var countryData2 = Factory.New<OrgCountryData>();
			countryData2.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
			countryData2.OV_OA_ApprovedLocation = org2.MainAddress.PK;
			countryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			countryData2.OV_OH_OrgHeader = org2.PK;

			Factory.Save();
			AssertEquals("Precondition", AviationSecuritySchemeMembershipEx.Codes.Yes, countryData1.OV_EXApprovedOrMajorExporter);
			AssertEquals("Precondition", AviationSecuritySchemeMembershipEx.Codes.Yes, countryData2.OV_EXApprovedOrMajorExporter);

			org1.MainAddress.OA_Address1 = "NEW DUMMY ADDRESS";
			Factory.Save();
			AssertEquals(AviationSecuritySchemeMembershipEx.Codes.No, countryData1.OV_EXApprovedOrMajorExporter);
			AssertEquals(AviationSecuritySchemeMembershipEx.Codes.Yes, countryData2.OV_EXApprovedOrMajorExporter);

			countryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;
			Factory.Save();

			countryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			org2.MainAddress.OA_Address1 = "NEW DUMMY ADDRESS";
			Factory.Save();
			AssertEquals("Can change all connected country data", AviationSecuritySchemeMembershipEx.Codes.No, countryData2.OV_EXApprovedOrMajorExporter);
		}

		public void TestAddEventWhenTSAKnownShipperApprovedStatusChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "ABC DUMMY ADDRESS";
			Factory.Save();

			var countryData1 = Factory.New<OrgCountryData>();
			countryData1.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
			countryData1.OV_OA_ApprovedLocation = org.MainAddress.PK;
			countryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			countryData1.OV_OH_OrgHeader = org.PK;

			var countryData2 = Factory.New<OrgCountryData>();
			countryData2.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
			countryData2.OV_OA_ApprovedLocation = address2.PK;
			countryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			countryData2.OV_OH_OrgHeader = org.PK;
			Factory.Save();
			AssertTKSEventLog(0);

			org.MainAddress.OA_Address1 = "NEW DUMMY ADDRESS";
			Factory.Save();
			AssertTKSEventLog(1);

			org.MainAddress.OA_Address1 = "NEW DUMMY ADDRESS 1";
			Factory.Save();
			AssertTKSEventLog(1); // No new logs added

			countryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			Factory.Save();

			org.MainAddress.OA_Address1 = "NEW DUMMY ADDRESS 2";
			address2.OA_Address1 = "NEW DUMMY ADDRESS 2";
			Factory.Save();
			AssertTKSEventLog(2); // Not add logs twice

			void AssertTKSEventLog(int count)
			{
				var logs = org.GetLogs().Find(u => u.SL_SE_NKEvent == AutoEvents.TSAKnownShipperApprovedStatusChanged.Code).ToList();
				AssertEquals(count, logs.Count);

				foreach (var log in logs)
				{
					AssertEquals("TSA Known Shipper Approved Status Changed To No", log.SL_Reference);
				}
			}
		}

		#region Test_OA_AuthorityToLeave_Log

		public void Test_OA_AuthorityToLeave_Log()
		{
			var excludeLogs = new List<StmALog>();
			var factory = new BusinessObjectFactory();
			var org = factory.New<OrgHeader>();
			org.OH_Code = "TST";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_Code = "TST";
			AssertEquals("ATL default value is 'DEF'", "DEF", org.MainAddress.OA_AuthorityToLeave);

			var logs = org.MainAddress.Logs;
			factory.Save();
			AssertEquals("Should be no logs, as DEF is default.", false, logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.AuthorisedCode));

			org.MainAddress.OA_AuthorityToLeave = "YES";
			factory.Save();
			var latestLog = FindLatestWithType(logs, Events.AuthorisedCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("YES", org.MainAddress.OA_AuthorityToLeave);
			AssertEquals(Events.AuthorisedCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			org.MainAddress.OA_AuthorityToLeave = "DEF";
			factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("DEF", org.MainAddress.OA_AuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawn.Code, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			org.MainAddress.OA_AuthorityToLeave = "NO";
			factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			AssertEquals("NO", org.MainAddress.OA_AuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawn.Code, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "NO", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);
		}

		StmALog FindLatestWithType(Logs logs, ZString eventType, params StmALog[] excludeLogs)
		{
			return logs.GetAllLogs().Cast<StmALog>().Where(l => !excludeLogs.Contains(l)).First(l => l.SL_SE_NKEvent == eventType);
		}

		#endregion

		#region TestPortAndCountryNames

		public void TestPortAndCountryNames()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Language = Constants.Languages.English;
			org.OH_RL_NKClosestPort = "AUSYD";
			var adr = org.Addresses.AddNew();

			AssertEquals("Sydney", adr.PortName);
			AssertEquals("Australia", adr.CountryName);

			adr.OA_RL_NKRelatedPortCode = "GBLON";
			AssertEquals("GB", adr.OA_RN_NKCountryCode);
			AssertEquals("London", adr.PortName);
			AssertEquals("United Kingdom", adr.CountryName);

			var adr2 = Factory.New<OrgAddress>();
			AssertEquals("", adr2.PortName);
			AssertEquals("", adr2.CountryName);
		}

		#endregion

		#region TestOrgAddressLanguageWhenChangingOrgLanguage

		public void TestOrgAddressLanguageWhenChangingOrgLanguage()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Language = Constants.Languages.English;

			org.MainAddress.OA_Address1 = "Address 1";
			OrgAddress chineseAddress = org.Addresses.AddNew();
			chineseAddress.OA_Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;

			AssertEquals("Main Address Language", Constants.Languages.English, org.MainAddress.OA_Language);
			AssertEquals("Chinese Address Language", Constants.Languages.English, chineseAddress.OA_Language);

			org.OH_Language = Constants.Languages.ChineseSimplified;
			AssertEquals("Main Address Language", Constants.Languages.English, org.MainAddress.OA_Language);
			AssertEquals("Chinese Address Language", Constants.Languages.ChineseSimplified, chineseAddress.OA_Language);
		}

		#endregion

		#region TestOrgAddressLanguageDefaultWhenNonEnglish

		public void TestOrgAddressLanguageDefaultWhenNonEnglish()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Language = Constants.Languages.ChineseSimplified;
			org.OH_FullName = OrgHeaderUnicodeTestConstants.ChineseCompanyName1;

			org.MainAddress.OA_Address1 = "Address 1";
			OrgAddress chineseAddress = org.Addresses.AddNew();
			chineseAddress.OA_Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;

			AssertEquals("Main Address Language", Constants.Languages.English, org.MainAddress.OA_Language);
			AssertEquals("Chinese Address Language", Constants.Languages.ChineseSimplified, chineseAddress.OA_Language);
		}

		#endregion

		#region TestScreeningStatuses

		#region TestScreeningStatuses_WhenWebServiceReturnPointExact

		public void TestScreeningStatuses_ShouldNotChangeWhenWebServiceReturnPointExactToInDatabaseAddress()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "QAZ";
			org.OH_FullName = "ABC";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.Address1 = "72 O'Riordan St";
			address.Address2 = "";
			address.City = "Alexandria";
			address.State = "NSW";
			address.Postcode = "2015";
			address.OA_RN_NKCountryCode = "AU";
			Factory.Save();
			org.OH_ScreeningStatus = "CLR";

			address.IsExactPointFound = true;
			address.Address1 = "72 O'riordan St";
			Factory.Save();
			AssertEquals("CLR", org.OH_ScreeningStatus);

			address.IsExactPointFound = false;
			address.Address1 = "Hello World";
			Factory.Save();
			AssertEquals("UNK", org.OH_ScreeningStatus);
		}

		public void TestScreeningStatuses_ShouldChangeWhenWebServiceReturnPointExactToNotInDatabaseAddress()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "QAZ";
			org.OH_FullName = "ABC";
			Factory.Save();

			org.OH_ScreeningStatus = "CLR";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.Address1 = "1 BARASSIE";
			address.Address2 = "";
			address.City = "GLASGOW";
			address.State = "";
			address.Postcode = "G74 4SD";
			address.OA_RN_NKCountryCode = "GB";
			address.IsExactPointFound = true;
			Factory.Save();
			AssertEquals("UNK", org.OH_ScreeningStatus);

			org.OH_ScreeningStatus = "CLR";
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_OH = org.PK;
			address2.Address1 = "Hello World";
			address2.Address2 = "";
			address2.City = "Alexandria";
			address2.State = "NSW";
			address2.Postcode = "2015";
			address2.OA_RN_NKCountryCode = "AU";
			address2.IsExactPointFound = false;
			Factory.Save();
			AssertEquals("UNK", org.OH_ScreeningStatus);
		}

		#endregion

		#region TestScreeningStatuses_ShouldChangeWhenGUIMakesChanges

		public void TestScreeningStatuses_ShouldChangeWhenGUIMakesChanges()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "QAZ";
			org.OH_FullName = "ABC";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_OH = org.PK;
			address.Address1 = "72 O'Riordan St";
			address.Address2 = "";
			address.City = "Alexandria";
			address.State = "NSW";
			address.Postcode = "2015";
			address.OA_RN_NKCountryCode = "AU";
			Factory.Save();
			org.OH_ScreeningStatus = "CLR";

			address.HasBeenChangedByUser = false;
			address.IsExactPointFound = true;
			address.Address1 = "72 O'riordan St";
			Factory.Save();
			AssertEquals("CLR", org.OH_ScreeningStatus);

			address.HasBeenChangedByUser = true;
			address.IsExactPointFound = true;
			address.Address1 = "72 o'riordan St";
			Factory.Save();
			AssertEquals("UNK", org.OH_ScreeningStatus);
		}

		#endregion

		#region TestScreeningStatuses_InvalidatedByLocalDataChanges

		public void TestInvalidateByLocalDataChanges_ShouldInvalidateScreeningStatus()
		{
			Address.OA_Address1 = "Need some text here to not get exception on save";

			var logCount = 0;
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "York Street";

			Factory.Save();

			CombineAssertions("Should invalidate screening status and create screening log", () =>
			{
				AssertHasInvalidatedScreeningStatuses(address.OA_Address1Info, ScreeningStatusesList.Codes.Clear);
				AssertHasInvalidatedScreeningStatuses(address.OA_Address2Info, ScreeningStatusesList.Codes.Clear);
				AssertHasInvalidatedScreeningStatuses(address.OA_CityInfo, ScreeningStatusesList.Codes.Matched);
				AssertHasInvalidatedScreeningStatuses(address.OA_PostCodeInfo, ScreeningStatusesList.Codes.Matched);
				AssertHasInvalidatedScreeningStatuses(address.OA_StateInfo, ScreeningStatusesList.Codes.Clear);
				AssertHasInvalidatedScreeningStatuses(address.OA_RL_NKRelatedPortCodeInfo, ScreeningStatusesList.Codes.Matched);
				AssertHasInvalidatedScreeningStatuses(address.OA_CompanyNameOverrideInfo, ScreeningStatusesList.Codes.RequiresReview);
			});

			void AssertHasInvalidatedScreeningStatuses(ZPropertyInfo propertyInfo, string screeningStatus)
			{
				logCount++;
				header.OH_ScreeningStatus = screeningStatus;
				propertyInfo.SetValueFromString("hello");

				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges, header.ScreeningLogCollection[logCount - 1].PJ_Status);
				AssertEquals(logCount, header.ScreeningLogCollection.Count);
			}
		}

		public void TestInvalidateByLocalDataChanges_ShouldNotInvalidateScreeningStatus()
		{
			Address.OA_Address1 = "Need some text here to not get exception on save";

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "York Street";

			Factory.Save();

			CombineAssertions("Should not invalidate screening status", () =>
			{
				AssertHasInvalidatedScreeningStatuses(address.OA_PhoneInfo, ScreeningStatusesList.Codes.Unknown);
				AssertHasInvalidatedScreeningStatuses(address.OA_PhoneInfo, ScreeningStatusesList.Codes.Clear);
				AssertHasInvalidatedScreeningStatuses(address.OA_MobileInfo, ScreeningStatusesList.Codes.Matched);
				AssertHasInvalidatedScreeningStatuses(address.OA_EmailInfo, ScreeningStatusesList.Codes.RequiresReview);
				AssertHasInvalidatedScreeningStatuses(address.OA_PhoneInfo, ScreeningStatusesList.Codes.PermanentClear);
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

		#region TestScreeningStatus_ShouldChangeAfterDeleteAddress

		public void TestScreeningStatus_NotClearOrg_DeleteAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "hello world";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			address.Delete();
			Factory.Save();

			AssertEquals("Screening status reset after delete", ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);
		}

		public void TestScreeningStatus_ClearOrg_DeleteAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "hello world";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals("PreCondition", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);

			address.Delete();
			Factory.Save();

			AssertEquals("Screening status remains clear after delete", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);
		}

		#endregion

		#region TestScreeningStatus_AddressActiveStatusChange

		public void TestScreeningStatus_ClearOrg_AddressActiveStatusChange()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals("Pre-condition", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);

			address.OA_IsActive = false;
			Factory.Save();
			AssertEquals("Screening status remains clear after inactive address", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);

			address.OA_IsActive = true;
			Factory.Save();
			AssertEquals("Screening status resets after activating address", ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);
		}

		public void TestScreeningStatus_MatchedOrg_AddressActiveStatusChange()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			AssertEquals("Pre-condition", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			address.OA_IsActive = false;
			Factory.Save();
			AssertEquals("Screening status resets after inactive address", ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("Pre-condition", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			address.OA_IsActive = true;
			Factory.Save();
			AssertEquals("Screening status remains Matched after activating address", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
		}

		#endregion

		#region TestScreeningStatus_AddAddress

		public void TestScreeningStatus_MatchedOrg_AddAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("Pre-condition", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			Factory.Save();

			AssertEquals("Screening status still Matched after adding address", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
		}

		public void TestScreeningStatus_NotMatchedOrg_AddAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			Factory.Save();

			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			AssertEquals("Pre-condition", ScreeningStatusesList.Codes.Clear, org.OH_ScreeningStatus);

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			Factory.Save();

			AssertEquals("Not Matched screening status resets after adding address", ScreeningStatusesList.Codes.Unknown, org.OH_ScreeningStatus);
		}

		#endregion

		#endregion

		#region TestCityStateCountryDescription

		public void TestCityStateCountryDescription()
		{
			RefUNLOCO.Loader uNLOCOLoader = new RefUNLOCO.Loader(Factory);
			RefUNLOCO aUSYD = uNLOCOLoader.Load("AUSYD");

			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals("expected TestStateCityCountryDescription", string.Empty, org.MainAddress.CityStateCountryDescription);

			org.OH_RL_NKClosestPort = aUSYD.RL_Code;
			AssertEquals("expected TestStateCityCountryDescription", "NSW AUSYD", org.MainAddress.CityStateCountryDescription);

			org.MainAddress.OA_City = "Sydney";
			AssertEquals("expected TestStateCityCountryDescription", "Sydney NSW AUSYD", org.MainAddress.CityStateCountryDescription);

			org.MainAddress.OA_Address1 = "O'Riodan ST";
			AssertEquals("expected TestStateCityCountryDescription", "Sydney NSW AUSYD O'Riodan ST", org.MainAddress.CityStateCountryDescription);
		}

		#endregion

		#region TestEffectiveRelatedPortCode

		public void TestEffectiveRelatedPortCode()
		{
			RefUNLOCO.Loader uNLOCOLoader = new RefUNLOCO.Loader(Factory);
			RefUNLOCO aUSYD = uNLOCOLoader.Load("AUSYD");
			RefUNLOCO nZAKL = uNLOCOLoader.Load("NZAKL");

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = aUSYD.RL_Code;

			OrgAddress address = Factory.New<OrgAddress>();
			AssertEquals("address.EffecitiveRelatedPortCode", null, address.EffectiveRelatedPortCode);

			address.OA_OH = org.PK;
			AssertEquals("address.EffecitiveRelatedPortCode", aUSYD, address.EffectiveRelatedPortCode);

			address.OA_RL_NKRelatedPortCode = nZAKL.RL_Code;
			AssertEquals("address.EffecitiveRelatedPortCode", nZAKL, address.EffectiveRelatedPortCode);
		}

		#endregion

		#region TestCachePortAndCountryNames

		public void TestCachePortAndCountryNames()
		{
			RefUNLOCO.Loader uNLOCOLoader = new RefUNLOCO.Loader(Factory);
			RefUNLOCO aUSYD = uNLOCOLoader.Load("AUSYD");
			RefUNLOCO nZAKL = uNLOCOLoader.Load("NZAKL");

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = aUSYD.RL_Code;

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;

			AssertEquals("address.EffecitiveRelatedPortCode", aUSYD, address.EffectiveRelatedPortCode);

			ZString portName = address.PortName;
			ZString countryName = address.CountryName;

			using (((IMatchingAddress)address).CachePortAndCountryNames())
			{
				AssertEquals("Precondition", portName, address.PortName);
				AssertEquals("Precondition", countryName, address.CountryName);

				address.OA_RL_NKRelatedPortCode = nZAKL.RL_Code;
				AssertEquals("address.EffecitiveRelatedPortCode", nZAKL, address.EffectiveRelatedPortCode);

				AssertEquals("Should return cached value", portName, address.PortName);
				AssertEquals("Should return cached value", countryName, address.CountryName);
			}

			AssertNotEquals("Should return new value", portName, address.PortName);
			AssertNotEquals("Should return new value", countryName, address.CountryName);
		}

		#endregion

		#region TestOA_CompanyNameOverrideTruncated

		public void TestOA_CompanyNameOverrideTruncated()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_CompanyNameOverride = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the";
			AssertEquals("OA_CompanyNameOverrideTruncatedLength is 50 characters", 50, OrgAddress.Schema.OA_CompanyNameOverrideTruncatedLength);
			Assert("Precondition: OA_CompanyNameOverride can be set for more than 50 characters up to 100 characters", address.OA_CompanyNameOverride.Length > OrgAddress.Schema.OA_CompanyNameOverrideTruncatedLength);
			AssertEquals("OA_CompanyNameOverrideTruncated will not exceed 50 characters though", OrgAddress.Schema.OA_CompanyNameOverrideTruncatedLength, address.OA_CompanyNameOverrideTruncated.Length);
			AssertEquals(address.OA_CompanyNameOverride.Substring(0, OrgAddress.Schema.OA_CompanyNameOverrideTruncatedLength), address.OA_CompanyNameOverrideTruncated);
		}

		#endregion

		#region TestEffectiveCompanyName

		public void TestEffectiveCompanyName()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "EAT MORETIEAT MORETIEAT MORETIEAT MORETIEAT MORETIEAT MORETI";

			AssertEquals("address.EffectiveCompanyName", ZString.Empty, address.EffectiveCompanyName);

			address.OA_OH = org.PK;
			AssertEquals("address.EffectiveCompanyName", "EAT MORETIEAT MORETIEAT MORETIEAT MORETIEAT MORETIEAT MORETI", address.EffectiveCompanyName);

			address.OA_CompanyNameOverride = "NONONO, EAT THE CUCKOO SQUEAKER INSTEAD NONONO, EAT THE CUCKOO SQUEAKER INSTEAD";
			AssertEquals("address.EffectiveCompanyName", "NONONO, EAT THE CUCKOO SQUEAKER INSTEAD NONONO, EAT THE CUCKOO SQUEAKER INSTEAD", address.EffectiveCompanyName);
		}

		public void TestEffectiveCompanyNameTruncated()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "EAT MORE YETI";

			AssertEquals("address.EffectiveCompanyName", ZString.Empty, address.EffectiveCompanyNameTruncated);

			address.OA_OH = org.PK;
			AssertEquals("address.EffectiveCompanyName", "EAT MORE YETI", address.EffectiveCompanyNameTruncated);

			address.OA_CompanyNameOverride = "NONONO, EAT THE CUCKOO SQUEAKER INSTEAD";
			AssertEquals("address.EffectiveCompanyName", "NONONO, EAT THE CUCKOO SQUEAKER INSTEAD", address.EffectiveCompanyNameTruncated);
		}

		#endregion

		#region Test Data Referesh Bus

		public void TestDataRefreshBus()
		{
			OrgHeader org1 = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			org1.MainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			org1.MainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			org1.Factory.Save();

			OrgHeader org2 = new BusinessObjectFactory().Load<OrgHeader>(org1.PK);
			org2.MainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Sales);
			org2.MainAddress.AddressCapability.SetCapabilityDisabled(OrgConstants.AddressType.Delivery);

			Assert(org1.MainAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office));
			Assert(org1.MainAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Delivery));
			Assert(!org1.MainAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Sales));

			org2.Factory.Save();

			Assert("Office capability should remain", org1.MainAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office));
			Assert("Delivery capability should be disabled", !org1.MainAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Delivery));
			Assert("Sales capability should be enabled", org1.MainAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Sales));
		}

		#endregion

		#region TestIContactable

		public void TestIContactable()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Colombia";

			OrgAddress address = org.Addresses.AddNewMainAddress();
			address.OA_Email = "moo@colombia.com";
			address.OA_Mobile = "1234567";
			address.OA_IsActive = false;

			IContactable addressIContactable = address;
			AssertEquals(address.PK, addressIContactable.PK);
			AssertEquals(org.OH_FullName, addressIContactable.Name);
			AssertEquals(address.OA_Mobile, addressIContactable.Mobile);
			AssertEquals(address.OA_Email, addressIContactable.Email);
			AssertEquals(false, addressIContactable.IsActive);

			AssertEquals("If this is changed you will need to subclass OrgAddress in ZClientSEV.sln and ensure it returns 0 nested contacts.",
				0, addressIContactable.GetNestedContacts("").Length);
		}

		#endregion

		#region Global organisations

		public void TestIsUniqueMainAddressForGlobalOrgOfType()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";
			header.OH_IsGlobalAccount = ZBool.True;

			OrgAddress firstMain = header.MainAddress;
			firstMain.OA_RN_NKCountryCode = "";
			firstMain.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgAddress secondMain = Factory.New<OrgAddress>();
			secondMain.OA_RN_NKCountryCode = "NZ";
			secondMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			header.Addresses.Add(secondMain);

			AssertEquals("FirstMain - Addresses in different countries so are unique", true, firstMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
			AssertEquals("SecondMain - Addresses in different countries so are unique", true, secondMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));

			secondMain.OA_RN_NKCountryCode = "AU";

			AssertEquals("FirstMain - Addresses are in the same country so not unique", false, firstMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
			AssertEquals("SecondMain - Addresses are in the same country so not unique", false, secondMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
		}

		public void TestIsUniqueMainAddressForGlobalOrgOfType_AddressesInCountriesOtherThanHeaderUnique()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";
			header.OH_IsGlobalAccount = ZBool.True;

			OrgAddress firstMain = header.MainAddress;
			firstMain.OA_RN_NKCountryCode = "";
			firstMain.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgAddress secondMain = Factory.New<OrgAddress>();
			secondMain.OA_RN_NKCountryCode = "NZ";
			secondMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			header.Addresses.Add(secondMain);

			OrgAddress thirdMain = Factory.New<OrgAddress>();
			thirdMain.OA_RN_NKCountryCode = "CH";
			thirdMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			header.Addresses.Add(thirdMain);

			AssertEquals("Addresses in different countries so are unique", true, firstMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
			AssertEquals("Addresses in different countries so are unique", true, secondMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
			AssertEquals("Addresses in different countries so are unique", true, thirdMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));

			secondMain.OA_RN_NKCountryCode = "CH";

			AssertEquals("FirstMain - Addresses in different country so are unique", true, firstMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
			AssertEquals("SecondMain - Addresses are in the same country so not unique", false, secondMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
			AssertEquals("ThirdMain - Addresses are in the same country so not unique", false, thirdMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
		}

		public void TestRelatedCountry()
		{
			RefUNLOCO.Loader uNLOCOLoader = new RefUNLOCO.Loader(Factory);
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;

			var aUCountry = uNLOCOLoader.Load("AUSYD").Country;
			header.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("If OA_RL_NKRelatedPortCode is empty, related country based on OH_RL_NKClosestPort", aUCountry, mainAddress.RelatedCountry);
			AssertEquals("Country is AU", "AU", mainAddress.RelatedCountry.Code);
			header.OH_RL_NKClosestPort = "AU";
			AssertEquals("Country code should be deemed to be the UNLOCO if it is just a country code", aUCountry, mainAddress.RelatedCountry);
			AssertEquals("Country is AU", "AU", mainAddress.RelatedCountry.Code);

			var nZCountry = uNLOCOLoader.Load("NZAKL").Country;
			mainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			AssertEquals("If OA_RL_NKRelatedPortCode is empty, related country based on OH_RL_NKClosestPort", nZCountry, mainAddress.RelatedCountry);
			AssertEquals("Country is NZ", "NZ", mainAddress.RelatedCountry.Code);
			mainAddress.OA_RL_NKRelatedPortCode = "NZ";
			AssertEquals("Country code should be deemed to be the UNLOCO if it is just a country code", nZCountry, mainAddress.RelatedCountry);
			AssertEquals("Country is NZ", "NZ", mainAddress.RelatedCountry.Code);

			mainAddress.OA_RN_NKCountryCode = "";
			mainAddress.OA_RL_NKRelatedPortCode = "AUD";
			header.OH_RL_NKClosestPort = "AUD";
			AssertNull("MainAddress.RelatedCountry is null if invalid port code lengths", mainAddress.RelatedCountry);

			OrgAddress newAddress = Factory.New<OrgAddress>();
			AssertNull(newAddress.Header);
			AssertNotNull(newAddress.RelatedCountry);
		}

		public void TestRelatedState()
		{
			RefUNLOCO.Loader uNLOCOLoader = new RefUNLOCO.Loader(Factory);
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("New South Wales", org.MainAddress.RelatedState.RW_Description);

			org.OH_RL_NKClosestPort = "AUMEL";
			org.MainAddress.OA_State = "VIC";
			AssertEquals("Victoria", org.MainAddress.RelatedState.RW_Description);
		}

		public void TestOA_RL_NKRelatedPortCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address = header.MainAddress;
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("Main Address not global - OA_RL_NKRelatedPortCode = OH_RL_NKClosestPort", "AUSYD", address.OA_RL_NKRelatedPortCode);
			AssertEquals("Main Address not global - OA_RL_NKRelatedPortCode = OH_RL_NKClosestPort", "AUSYD", header.OH_RL_NKClosestPort);

			OrgAddress secondAddress = header.Addresses.AddNew();
			AssertEquals("not main and not global - should be empty", "", secondAddress.OA_RL_NKRelatedPortCode);
			secondAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			AssertEquals("not main and not global - should be NZAKL", "NZAKL", secondAddress.OA_RL_NKRelatedPortCode);
			AssertEquals("not main and not global - OH_RL_NKClosestPort is still same", "AUSYD", header.OH_RL_NKClosestPort);

			header.OH_IsGlobalAccount = ZBool.True;
			AssertEquals("Main Address is global - OA_RL_NKRelatedPortCode = OH_RL_NKClosestPort as OA_RL_NKRelatedPortCode is empty", "AUSYD", address.OA_RL_NKRelatedPortCode);
			address.OA_RL_NKRelatedPortCode = "AUBNE";
			AssertEquals("main and global - should be AUBNE", "AUBNE", address.OA_RL_NKRelatedPortCode);
			string tableValue = ((INeedRow)header).Row[OrgHeaderSchema.OH_RL_NKClosestPort.Name].ToString();
			AssertEquals("main and global - OH_RL_NKClosestPort table value", "AUSYD", tableValue);
			AssertEquals("main and global - OH_RL_NKClosestPort business object value", "AUBNE", header.OH_RL_NKClosestPort);
		}

		public void TestOA_RL_NKRelatedPortCodeHasChanges()
		{
			var factory = new BusinessObjectFactory();

			var header = factory.New<OrgHeader>();
			header.OH_FullName = "CargoWise edi Australia Pty Ltd";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.OH_IsGlobalAccount = true;

			var mainAddr = header.MainAddress;
			mainAddr.OA_Address1 = "Unit 3a, 72 O'Riordan Street";

			factory.Save();

			mainAddr.OA_RL_NKRelatedPortCode = "";

			factory.Save();

			Assert("address.OA_RL_NKRelatedPortCodeInfo.HasChanges", mainAddr.OA_RL_NKRelatedPortCodeInfo.HasChanges);
			Assert("!address.OA_RL_NKRelatedPortCodeHasChangesComparingWithTableValue", !mainAddr.OA_RL_NKRelatedPortCodeHasChanges);
		}

		public void TestMergeMainAddresses()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "NZAKL";
			header.OH_IsGlobalAccount = ZBool.True;

			OrgAddress firstMain = header.MainAddress;
			firstMain.OA_RL_NKRelatedPortCode = "AUSYD";
			OrgAddress secondMain = Factory.New<OrgAddress>();
			secondMain.OA_RN_NKCountryCode = "NZ";
			secondMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			header.Addresses.Add(secondMain);

			OrgAddressCapabilityWrapper oACfirst = firstMain.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			OrgAddressCapabilityWrapper oACsecond = secondMain.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			Assert("both of address should be enabled as main as they have different country relation", oACfirst.Enabled);
			Assert("both of address should be enabled as main as they have different country relation", oACfirst.Main);
			Assert("both of address should be enabled as main as they have different country relation", oACsecond.Enabled);
			Assert("both of address should be enabled as main as they have different country relation", oACsecond.Main);
			firstMain.OA_RL_NKRelatedPortCode = "NZAKL";
			Assert("only second is main now", oACfirst.Enabled);
			Assert("only second is main now", !oACfirst.Main);
			Assert("only second is main now", oACsecond.Enabled);
			Assert("only second is main now", oACsecond.Main);

			firstMain.OA_Language = Core.Constants.Languages.English;
			secondMain.OA_Language = Core.Constants.Languages.Albanian;
			firstMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			Assert("both of address should be enabled as main as they have different language relation", oACfirst.Enabled);
			Assert("both of address should be enabled as main as they have different language relation", oACfirst.Main);
			Assert("both of address should be enabled as main as they have different language relation", oACsecond.Enabled);
			Assert("both of address should be enabled as main as they have different language relation", oACsecond.Main);
			firstMain.OA_Language = Core.Constants.Languages.Albanian;
			Assert("only second is main now", oACfirst.Enabled);
			Assert("only second is main now", !oACfirst.Main);
			Assert("only second is main now", oACsecond.Enabled);
			Assert("only second is main now", oACsecond.Main);
		}

		public void TestIAddressDetails()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address = org.Addresses.AddNew();
			IAddressDetails addressDetails = address;

			address.OA_Address1 = "Address1";
			AssertEquals("Address line1", "Address1", addressDetails.AddressLine1);

			address.OA_Address2 = "Address2";
			AssertEquals("Address line2", "Address2", addressDetails.AddressLine2);

			address.OA_City = "Chicago";
			AssertEquals("City", "Chicago", addressDetails.City);

			address.OA_CompanyNameOverride = "";
			org.OH_FullName = "Eagle Datamation International";
			AssertEquals("CompanyName", org.OH_FullName, addressDetails.CompanyName);

			address.OA_CompanyNameOverride = "CargoWise";
			AssertEquals("CompanyName", address.OA_CompanyNameOverride, addressDetails.CompanyName);

			AssertEquals("ContactName", "", addressDetails.ContactName);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Henry Ye";
			AssertEquals("ContactName", "Henry Ye", addressDetails.ContactName);

			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Country", "AU", addressDetails.Country);

			address.OA_RL_NKRelatedPortCode = "USCHI";
			AssertEquals("Country", "US", addressDetails.Country);

			address.OA_Email = "support@cargowise.com";
			AssertEquals("Email", "support@cargowise.com", addressDetails.Email);

			address.OA_Fax = "02 8820 2250";
			AssertEquals("Fax", "02 8820 2250", addressDetails.Fax);

			address.OA_Phone = "02 8820 2251";
			AssertEquals("Phone", "02 8820 2251", addressDetails.Phone);

			address.OA_PostCode = "2015";
			AssertEquals("PostCode", "2015", addressDetails.PostCode);

			address.OA_State = "NSW";
			AssertEquals("State", "NSW", addressDetails.State);
		}
		#endregion

		#region IReadOnlySecurity

		public void TestAddressDetailsReadOnlyAsPerAddressCapabilities()
		{
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Only non AR/AP address capability, allowed.", true, true, false, new[] { OrgConstants.AddressType.CustomsAddressOfRecord });
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Only non AR/AP address capability, not allowed.", true, false, true, new[] { OrgConstants.AddressType.CustomsAddressOfRecord });
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Only AP address capability, allowed.", true, true, false, new[] { OrgConstants.AddressType.Payables });
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Only AP address capability, not allowed.", false, true, true, new[] { OrgConstants.AddressType.Payables });
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Only AR address capability, allowed.", true, true, false, new[] { OrgConstants.AddressType.Receivables });
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Only AR address capability, not allowed.", false, true, true, new[] { OrgConstants.AddressType.Receivables });
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Both AR/AP and non AR/AP addresses capability, both allowed.", true, true, false, new[] { OrgConstants.AddressType.CustomsAddressOfRecord, OrgConstants.AddressType.Payables });
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Both AR/AP and non AR/AP addresses capability, AR/AP allowed, non AR/AP not allowed.", true, false, true, new[] { OrgConstants.AddressType.CustomsAddressOfRecord, OrgConstants.AddressType.Payables });
			AssertAddressDetailsReadOnlyAsPerAddressCapabilities("Both AR/AP and non AR/AP addresses capability, AR/AP not allowed, non AR/AP allowed.", false, true, true, new[] { OrgConstants.AddressType.CustomsAddressOfRecord, OrgConstants.AddressType.Payables });
		}

		void AssertAddressDetailsReadOnlyAsPerAddressCapabilities(string message, bool modifyDetailsARAPAllowed, bool modifyDetailsNonARAPAllowed, bool expectedReadOnly, IEnumerable<string> enabledCapabilities)
		{
			var address = OrgInDB.Addresses.AddNew();
			address.AddressCapability.DisableAllCapabilities();
			AssertEquals("Precondition 1: ", false, address.AddressCapability.EnabledCapabilities.Any());

			Env.Security.OrgAddressDetailsARAP.IsAllowed = modifyDetailsARAPAllowed;
			Env.Security.OrgAddressDetailsNonARAP.IsAllowed = modifyDetailsNonARAPAllowed;
			enabledCapabilities.ForEach(c => address.AddressCapability.SetCapabilityEnabled(c));

			CombineAssertions(message, () =>
			{
				AssertEquals("1", expectedReadOnly, address.OA_IsActiveInfo.ReadOnly);
				AssertEquals("2", expectedReadOnly, address.OA_CodeInfo.ReadOnly);
				AssertEquals("3", expectedReadOnly, address.OA_Address1Info.ReadOnly);
				AssertEquals("4", expectedReadOnly, address.OA_Address2Info.ReadOnly);
				AssertEquals("5", expectedReadOnly, address.OA_CityInfo.ReadOnly);
				AssertEquals("6", expectedReadOnly, address.OA_PostCodeInfo.ReadOnly);
				AssertEquals("7", expectedReadOnly, address.OA_StateInfo.ReadOnly);
				AssertEquals("8", expectedReadOnly, address.OA_RN_NKCountryCodeInfo.ReadOnly);
				AssertEquals("9", expectedReadOnly, address.OA_CompanyNameOverrideInfo.ReadOnly);
				AssertEquals("10", expectedReadOnly, address.OA_LanguageInfo.ReadOnly);
				AssertEquals("11", expectedReadOnly, address.OA_PhoneInfo.ReadOnly);
				AssertEquals("12", expectedReadOnly, address.OA_MobileInfo.ReadOnly);
				AssertEquals("13", expectedReadOnly, address.OA_FaxInfo.ReadOnly);
				AssertEquals("14", expectedReadOnly, address.OA_Phone_FormattedInfo.ReadOnly);
				AssertEquals("15", expectedReadOnly, address.OA_Mobile_FormattedInfo.ReadOnly);
				AssertEquals("16", expectedReadOnly, address.OA_Fax_FormattedInfo.ReadOnly);
				AssertEquals("17", expectedReadOnly, address.OA_EmailInfo.ReadOnly);
				AssertEquals("18", expectedReadOnly, address.OA_RL_NKRelatedPortCodeInfo.ReadOnly);
				AssertEquals("19", expectedReadOnly, address.OA_AdditionalAddressInformationInfo.ReadOnly);
			});
		}

		public void TestMainAddressReadOnlySecurityMembers()
		{
			OrgAddress mainAddress = OrgInDB.MainAddress;

			Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed = true;
			Env.Security.OrgDetailsModifyAddressShortCode.IsAllowed = true;
			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
			Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_IsActiveInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_CodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Address1Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Address2Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_CityInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PostCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_StateInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_LanguageInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PhoneInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_MobileInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_FaxInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_EmailInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_RL_NKRelatedPortCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PalletJackInfo.ReadOnly); // Other field on OA

			Env.Security.OrgDetailsModifyAddressShortCode.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_IsActiveInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_CodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Address1Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Address2Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_CityInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PostCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_StateInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_LanguageInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PhoneInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_MobileInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_FaxInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_EmailInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_RL_NKRelatedPortCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PalletJackInfo.ReadOnly); // Other field on OA

			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_IsActiveInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_CodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_Address1Info.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_Address2Info.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_CityInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_PostCodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_StateInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_LanguageInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_RL_NKRelatedPortCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PhoneInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_MobileInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_FaxInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_EmailInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PalletJackInfo.ReadOnly); // Other field on OA

			Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = false;
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_PhoneInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_MobileInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_FaxInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_EmailInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_RL_NKRelatedPortCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !mainAddress.OA_PalletJackInfo.ReadOnly); // Other field on OA

			Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed = false;
			Assert("Access NOT Allowed - ReadOnly", mainAddress.OA_PalletJackInfo.ReadOnly); // Other field on OA
		}

		public void TestNonMainAddressDetailsReadOnlySecurityMembers()
		{
			OrgAddress address = OrgInDB.Addresses.AddNew();

			Env.Security.OrgAddressDetailsModify.IsAllowed = true;
			Env.Security.OrgAddressShortCodeModify.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !address.OA_IsActiveInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Address1Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Address2Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CityInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_PostCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_StateInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_LanguageInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_PhoneInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_MobileInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_FaxInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_EmailInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_RL_NKRelatedPortCodeInfo.ReadOnly);

			Env.Security.OrgAddressShortCodeModify.IsAllowed = false;
			Assert("Access Allowed - Not ReadOnly", !address.OA_IsActiveInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Address1Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Address2Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CityInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_PostCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_StateInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_LanguageInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_PhoneInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_MobileInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_FaxInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_EmailInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_RL_NKRelatedPortCodeInfo.ReadOnly);

			Env.Security.OrgAddressDetailsModify.IsAllowed = false;
			Assert("Access NOT Allowed - ReadOnly", address.OA_IsActiveInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Address1Info.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Address2Info.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CityInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_PostCodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_StateInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_LanguageInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_PhoneInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_MobileInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_FaxInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_EmailInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_RL_NKRelatedPortCodeInfo.ReadOnly);
		}

		public void TestCustomsAddressDetailsReadOnlySecurityMembers()
		{
			var address = OrgInDB.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			AssertEquals("Is Customs Address", true, address.IsCustomsAddress);

			Env.Security.OrgAddressCustomsAddressModify.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !address.OA_IsActiveInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Address1Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Address2Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CityInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_PostCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_StateInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_LanguageInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_PhoneInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_MobileInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_FaxInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_EmailInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_RL_NKRelatedPortCodeInfo.ReadOnly);

			Env.Security.OrgAddressCustomsAddressModify.IsAllowed = false;
			Assert("Access NOT Allowed - ReadOnly", address.OA_IsActiveInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Address1Info.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Address2Info.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CityInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_PostCodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_StateInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_LanguageInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_PhoneInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_MobileInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_FaxInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_EmailInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_RL_NKRelatedPortCodeInfo.ReadOnly);
		}

		public void TestEUCustomsAddressDetailsReadOnlySecurityMembers()
		{
			var address = OrgInDB.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.EUCustomsAddress);
			AssertEquals("Is EU Customs Address", true, address.IsEUCustomsAddress);

			Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !address.OA_IsActiveInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Address1Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Address2Info.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CityInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_PostCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_StateInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_LanguageInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_PhoneInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_MobileInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_FaxInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_EmailInfo.ReadOnly);
			Assert("Access Allowed - Not ReadOnly", !address.OA_RL_NKRelatedPortCodeInfo.ReadOnly);

			Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed = false;
			Assert("Access NOT Allowed - ReadOnly", address.OA_IsActiveInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Address1Info.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Address2Info.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CityInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_PostCodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_StateInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_CompanyNameOverrideInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_LanguageInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_PhoneInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_MobileInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_FaxInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Phone_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Mobile_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_Fax_FormattedInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_EmailInfo.ReadOnly);
			Assert("Access NOT Allowed - ReadOnly", address.OA_RL_NKRelatedPortCodeInfo.ReadOnly);
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					fOrgInDB = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		#endregion

		#region Main Address

		public void TestCannotUnDefaultMainOfficeAddress()
		{
			var testHeader = Factory.New<OrgHeader>();
			testHeader.ShowMessage += new OrgHeader.ShowMessageEventHandler(TestHeader_ShowMessage);

			AssertNull(CaptionShownToUser);
			AssertNull(MessageShownToUser);
			testHeader.MainAddress.AddressCapability.SetIsNotMainAddress(nameof(AddressType.OFC));
			Assert("MainAddress is still Default in BO", testHeader.MainAddress.AddressCapability.GetIsMainAddress(nameof(AddressType.OFC)));
		}

		void TestHeader_ShowMessage(string caption, string message)
		{
			CaptionShownToUser = caption;
			MessageShownToUser = message;
		}
		string CaptionShownToUser;
		string MessageShownToUser;

		#region AddressCapabilityHelpers

		public void TestAddressCapabilityHelpers()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address = header.MainAddress;
			Assert("MainAddress is OFC", address.IsAddressOfType(OrgAddressType.Office));
			Assert("MainAddress is main", address.IsMainAddressOfType(OrgAddressType.Office));
			Assert("MainAddress is not DLV", !address.IsAddressOfType(OrgAddressType.Delivery));
			Assert("MainAddress is not DLV main", !address.IsMainAddressOfType(OrgAddressType.Delivery));
			address.AddAddressType(OrgAddressType.Delivery);
			Assert("MainAddress is DLV", address.IsAddressOfType(OrgAddressType.Delivery));
			Assert("MainAddress is main DLV automatically", address.IsMainAddressOfType(OrgAddressType.Delivery));
			address.DeleteAddressType(OrgAddressType.Delivery);
			Assert("MainAddress is not DLV", !address.IsAddressOfType(OrgAddressType.Delivery));
			Assert("MainAddress is not DLV main", !address.IsMainAddressOfType(OrgAddressType.Delivery));
		}

		#endregion

		public void TestDefaultTheFirstAddressOfEachTypeIfPossible()
		{
			Address = Company.Addresses.AddNew();
			foreach (CodeDescriptionPair addressTypePair in OrgCodeLists.AddressType_List(Factory))
			{
				Address.AddressCapability.SetCapabilityEnabled(addressTypePair.Code);
				if (!Address.AddressCapability.GetCapabilityEnabled(OrgAddressType.Office.Code))
				{
					if (Address.AddressCapability.GetCapabilityEnabled(OrgAddressType.Miscellaneous.Code))
					{
						Assert("The first Address of type '" + OrgConstants.AddressType.Miscellaneous + "' should not be set as default.", !Address.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Miscellaneous));
					}
					else
					{
						Assert("The first Address of type '" + addressTypePair.Code + "' should automatically be set as default.", Address.AddressCapability.GetIsMainAddress(addressTypePair.Code));
					}
				}
			}

			Address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			Assert("Should automatically be set as default.", Address.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Pickup));

			OrgAddress address2 = Collection.AddNew();
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			Assert("Should not be set as default.", !address2.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.PickupAndDelivery));
		}

		public void TestMainAddressAsASingleString()
		{
			Company.OH_FullName = "Test Company";
			Address.OA_Address1 = "Test Address 1";
			Address.OA_Address2 = "Test Address 2";
			Address.OA_City = "TestCity";
			Address.OA_State = "TestState";
			Address.OA_PostCode = "TsPostcode";

			AssertEquals("Should have formatted address as single line", "TEST COMPANY TEST ADDRESS 1 TEST ADDRESS 2 TESTCITY TESTSTATE TSPOSTCODE", Address.AddressAsASingleLine);

			Address.OA_Address2 = "";
			Address.OA_State = "";
			Address.OA_PostCode = "";

			AssertEquals("Should have formatted address as single line with missing fields", "TEST COMPANY TEST ADDRESS 1 TESTCITY", Address.AddressAsASingleLine);
		}

		public void TestAddressAsASingleLineWithoutCompanyName()
		{
			Company.OH_FullName = "Test Company";
			MainAddress.OA_Address1 = "Test Address 1";
			MainAddress.OA_Address2 = "Test Address 2";
			MainAddress.OA_AdditionalAddressInformation = "Test Additional Address Info";
			MainAddress.OA_City = "TestCity";
			MainAddress.OA_State = "TestState";
			MainAddress.OA_PostCode = "TsPostcode";

			AssertEquals("Should have formatted address as single line, excluding Company Name", "TEST ADDITIONAL ADDRESS INFO TEST ADDRESS 1 TEST ADDRESS 2 TESTCITY TESTSTATE TSPOSTCODE", MainAddress.AddressAsASingleLineWithoutCompanyName);
		}

		public void TestAddressAsASingleLineWithoutCompanyNameAndAdditionalAddressInfo()
		{
			Company.OH_FullName = "Test Company";
			MainAddress.OA_Address1 = "Test Address 1";
			MainAddress.OA_Address2 = "Test Address 2";
			MainAddress.OA_AdditionalAddressInformation = "Test Additional Address Info";
			MainAddress.OA_City = "TestCity";
			MainAddress.OA_State = "TestState";
			MainAddress.OA_PostCode = "TsPostcode";

			AssertEquals("Should have formatted address as single line but excluded Additional Address info along with Company Name", "TEST ADDRESS 1 TEST ADDRESS 2 TESTCITY TESTSTATE TSPOSTCODE", MainAddress.AddressAsASingleLineWithoutCompanyNameAndAdditionalAddressInfo);

			Factory.Save();
			AssertEquals("Ensure AdditionalAddressInformation is unaffected", "Test Additional Address Info", MainAddress.OA_AdditionalAddressInformation);
		}

		#endregion

		#region Additional Address Information

		public void TestAdditionalAddressInformation_WhenSettingNewValue_ShouldUpdateUnrestrictedOne()
		{
			// Arrange

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_AdditionalAddressInformation = string.Empty;
			Factory.Save();
			AssertEquals("Pre-condition: HasChanges is false before setting OA_AdditionalAddressInformation", false, address.HasChanges);

			// Act

			address.OA_AdditionalAddressInformation = "[_MOCK_ADDITIONAL_INFO_]";

			// Assert
			AssertEquals(true, address.HasChanges);
			AssertEquals("[_MOCK_ADDITIONAL_INFO_]", address.UnrestrictedAdditionalAddressInformation);
		}

		public void TestUnrestrictedAddressInformation_WhenValueLessThanMaxLength_ShouldUpdateRestrictedOne()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_AdditionalAddressInformation = string.Empty;

			// Act.

			address.UnrestrictedAdditionalAddressInformation = "[_MOCK_ADDITIONAL_INFO_]";

			// Assert.

			AssertEquals("[_MOCK_ADDITIONAL_INFO_]", address.UnrestrictedAdditionalAddressInformation);
			AssertEquals("[_MOCK_ADDITIONAL_INFO_]", address.OA_AdditionalAddressInformation);
			AssertEquals(true, address.HasChanges);
			AssertEquals(false, address.UnrestrictedAdditionalAddressInformationInfo.HasErrors());
		}

		public void TestUnrestrictedAddressInformation_WhenValueMoreThanMaxLength_ShouldNotUpdateRestrictedOne()
		{
			// Arrange.
			var address = Factory.NewWithValidTestData<OrgAddress>();

			var addressInfo = Factory.New<OrgAddressAdditionalInfo>();
			addressInfo.OAI_OA_Address = address.PK;
			addressInfo.OAI_IsPrimary = true;
			var longInfo = new string('0', address.AdditionalInfos[0].OAI_AdditionalInfoInfo.MaxLength + 1);
			// Act.

			address.UnrestrictedAdditionalAddressInformation = longInfo;

			// Assert.

			AssertEquals(longInfo, address.UnrestrictedAdditionalAddressInformation);
			AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
			AssertEquals(true, address.HasChanges);
		}

		public void TestUnrestrictedAddressInformation_WhenSettingValueSameAsRestrictedOne_ShouldNotSetHasChangesFlag()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_AdditionalAddressInformation = "[_MOCK_ADDITIONAL_ADDRESS_INFORMATION_]";
			Factory.Save();
			AssertEquals("Pre-condition: HasChanges is false before setting UnrestrictedAdditionalAddressInformation", false, address.HasChanges);

			// Act.

			address.UnrestrictedAdditionalAddressInformation = "[_MOCK_ADDITIONAL_ADDRESS_INFORMATION_]";

			// Assert.

			AssertEquals(false, address.HasChanges);
		}

		#endregion

		#region IOrgAddress Test

		public void TestIOrgAddressAddress()
		{
			var expectedAddress = new StringBuilder();
			var expectedAddressFull = new StringBuilder();

			var iAddress = (IOrgAddress)Address;

			AssertEquals("Precondition - IAddress.Address is empty.", expectedAddress.ToString(), iAddress.Address.ToString());

			Company.OH_FullName = "Nintendo";
			expectedAddress.Append("Nintendo");
			AssertEquals("IAddress.Address", expectedAddress.ToString(), iAddress.Address.ToString());

			Address.OA_CompanyNameOverride = "Nintendo USA";
			expectedAddress.Append(" USA");
			AssertEquals("IAddress.Address", expectedAddress.ToString(), iAddress.Address.ToString());

			Address.OA_Address1 = "Unit 3a";
			expectedAddressFull.AppendLine(expectedAddress.ToString());
			expectedAddressFull.Append("Unit 3a");
			AssertEquals("IAddress.Address", expectedAddress.ToString(), iAddress.Address.ToString());
			AssertEquals("IAddress.AddressFull", expectedAddressFull.ToString(), iAddress.AddressFull.ToString());

			Address.OA_Address2 = "1 Epping Road";
			expectedAddress.AppendLine();
			expectedAddress.Append("1 Epping Road");
			expectedAddressFull.Append(", 1 Epping Road");
			AssertEquals("IAddress.Address", expectedAddress.ToString(), iAddress.Address.ToString());
			AssertEquals("IAddress.AddressFull", expectedAddressFull.ToString(), iAddress.AddressFull.ToString());

			Address.OA_RL_NKRelatedPortCode = "AUSYD";
			expectedAddressFull.Append(" AUSYD");
			AssertEquals("IAddress.Address", expectedAddress.ToString(), iAddress.Address.ToString());
			AssertEquals("IAddress.AddressFull", expectedAddressFull.ToString(), iAddress.AddressFull.ToString());

			Address.OA_City = "Tokyo";
			expectedAddress.Append(" Tokyo");
			AssertEquals("IAddress.Address", expectedAddress.ToString(), iAddress.Address.ToString());

			Address.OA_State = "Tokyo";
			expectedAddress.Append(" Tokyo");
			AssertEquals("IAddress.Address", expectedAddress.ToString(), iAddress.Address.ToString());

			Address.OA_PostCode = "	120-0003";
			expectedAddress.Append(" 	120-0003");
			AssertEquals("IAddress.Address", expectedAddress.ToString(), iAddress.Address.ToString());

			ZString expectedDetails = "Nintendo USA\r\nUnit 3a\r\n1 Epping Road\r\nTokyo Tokyo \t120-0003\r\n";
			AssertEquals("Address details should be ", ((IOrgAddress)Address).AddressDetailed, expectedDetails);

			AssertEquals("IOrgAddress PK returns PK", Address.PK, ((IOrgAddress)Address).PK);
		}

		#endregion

		#region IDocAddress Test

		public void TestIDocAddressMembers()
		{
			IDocAddress iAddress = Address;

			AssertNotNull("Precondition - IAddress.Address is empty.", iAddress);
			AssertEquals("IDocAddress.Override should always be false.", false, iAddress.E2_AddressOverride);

			Company.OH_FullName = "Nintendo";
			AssertEquals("IDocAddress.Company should come from OrgHeader.", Company.OH_FullName, iAddress.E2_CompanyName);

			Address.OA_CompanyNameOverride = "Nintendo USA";
			AssertEquals("IDocAddress.Company should come from OA_CompanyNameOverride.", Address.OA_CompanyNameOverride, iAddress.E2_CompanyName);

			Address.OA_AdditionalAddressInformation = "Unit 34";
			AssertEquals("IDocAddress.E2_AdditionalAddressInformation should come from OA_AdditionalAddressInformation.", Address.OA_AdditionalAddressInformation, iAddress.E2_AdditionalAddressInformation);

			Address.OA_Address1 = "1 Epping Road";
			AssertEquals("IDocAddress.E2_Address1 should come from OA_Address1.", Address.OA_Address1, iAddress.E2_Address1);

			Address.OA_Address2 = "2 Epping Road";
			AssertEquals("IDocAddress.E2_Address2 should come from OA_Address2.", Address.OA_Address2, iAddress.E2_Address2);

			Address.OA_City = "Tokyo";
			AssertEquals("IDocAddress.E2_City should come from OA_City.", Address.OA_City, iAddress.E2_City);

			Address.OA_State = "Tokyo";
			AssertEquals("IDocAddress.E2_State should come from OA_State.", Address.OA_State, iAddress.E2_State);

			AssertEquals("IDocAddress.E2_GovRegNum", "", iAddress.E2_GovRegNum);

			AssertEquals("IDocAddress.E2_GovRegNumType", "", iAddress.E2_GovRegNumType);

			AssertEquals("IDocAddress.E2_OA_Address should return PK", Address.PK, iAddress.E2_OA_Address);

			AssertEquals("IDocAddress.E2_AddressType", "", iAddress.E2_AddressType);

			RefUNLOCO ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			RefUNLOCO nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
			Company.OH_RL_NKClosestPort = ausyd.Code;
			Address.OA_RL_NKRelatedPortCode = ZString.Empty;
			AssertEquals("IDocAddress.E2_PortCode", "AUSYD", iAddress.E2_PortCode);
			AssertEquals("IDocAddress.CountryCode", "AU", iAddress.CountryCode);

			Address.OA_RL_NKRelatedPortCode = nzakl.Code;
			AssertEquals("IDocAddress.E2_PortCode", "NZAKL", iAddress.E2_PortCode);
			AssertEquals("IDocAddress.CountryCode", "NZ", iAddress.CountryCode);

			Address.OA_RN_NKCountryCode = "HK";
			AssertEquals("IDocAddress.E2_RN_NKCountryCode", "HK", iAddress.E2_RN_NKCountryCode);
			AssertEquals("IDocAddress.CountryDescription", "Hong Kong", iAddress.CountryDescription);
		}

		#endregion

		#region CodeLists

		public void TestOA_AccessPoint_List()
		{
			Assert("OA_AccessPoint_List.Count > 0", Address.OA_AccessPoint_List.Count > 0);
		}

		public void TestOA_CommunicationRequired_List()
		{
			Assert("OA_CommunicationRequired_List.Count > 0", Address.OA_CommunicationRequired_List.Count > 0);
		}

		public void TestOA_ContainerHandling_List()
		{
			Assert("OA_ContainerHandling_List.Count > 0", Address.OA_ContainerHandling_List.Count > 0);
		}

		public void TestOA_LabourRequired_List()
		{
			Assert("OA_LabourRequired_List.Count > 0", Address.OA_LabourRequired_List.Count > 0);
		}

		public void TestLanguageList()
		{
			Assert("LanguageList.Count > 0", Address.LanguageList.Count > 0);
		}

		public void TestOA_DockHeight_List()
		{
			Assert("OA_DockHeight_List", Address.OA_Dock_Height_List.Count > 0);
		}

		public void TestOM_CMSalesCategory_List()
		{
			Assert("OM_CMSalesCategory_List", Address.OM_CMSalesCategory_List.Count > 0);
		}

		public void TestAdditionalAddressInfoList()
		{
			Assert("Precondition : AdditionalAddressInfoList should be zero", Address.AdditionalAddressInfoList.Count == 0);
			AssertEquals("Precondition : AdditionalAddressInfoList should be CodeDescriptionPairList", typeof(CodeDescriptionPairList), Address.AdditionalAddressInfoList.GetType());

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var additionalInfo = address.AdditionalInfos.AddNew();
			additionalInfo.OAI_AdditionalInfo = "Additional Info";
			additionalInfo.OAI_IsPrimary = true;

			Factory.Save();

			AssertEquals(address.PK, additionalInfo.OAI_OA_Address);
			AssertEquals(1, address.AdditionalAddressInfoList.Count);
		}

		#endregion

		#region KnownShipper

		public void TestKnownShipper()
		{
			AssertEquals("Pre-condition", null, Address.KnownShipper);

			var knownShipperDetails1 = Address.KnownShipperDetails.AddNew();
			knownShipperDetails1.OV_EXApprovalNumber = "111";
			AssertEquals("111", Address.KnownShipper.OV_EXApprovalNumber);
		}

		#endregion

		#region Language

		public void TestIsEnglish()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Address is English by default", true, org.MainAddress.IsEnglish);

			org.MainAddress.OA_Language = Core.Constants.Languages.Hindi;
			AssertEquals("Address is not English", false, org.MainAddress.IsEnglish);

			org.MainAddress.OA_Language = Core.Constants.Languages.English;
			AssertEquals("Address is English", true, org.MainAddress.IsEnglish);

			org.MainAddress.OA_Language = Core.Constants.Languages.EnglishAmerican;
			AssertEquals("Address is English", true, org.MainAddress.IsEnglish);

			org.MainAddress.OA_Language = Core.Constants.Languages.French;
			AssertEquals("Address is not English", false, org.MainAddress.IsEnglish);

			org.MainAddress.OA_Language = Core.Constants.Languages.EnglishBritish;
			AssertEquals("Address is English", true, org.MainAddress.IsEnglish);
		}

		public void TestDefaults()
		{
			var org = Factory.New<OrgHeader>();
			OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TRL");
			OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PSL");
			OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HWL");

			var address1 = org.Addresses.AddNew();
			AssertEquals("Default language", Core.Constants.Languages.English, address1.OA_Language);
			AssertEquals("FCL Equipment Needed", "TRL", address1.OA_FCLEquipmentNeeded);
			AssertEquals("LCL Equipment Needed", "PSL", address1.OA_LCLEquipmentNeeded);
			AssertEquals("Air Equipment Needed", "HWL", address1.OA_AIREquipmentNeeded);
		}

		#endregion

		#region Phone Numbers

		public void TestPhoneNumbers()
		{
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "AUSYD";

			Address.OA_Mobile_Formatted = "0426 829 924";
			Address.OA_Phone_Formatted = "2 9025 2222";
			Address.OA_Fax_Formatted = "2 9025 2223";

			AssertEquals("+61 426 829 924", Address.MobilePhoneNumber.FormattedForBinding);
			AssertEquals("+61 2 9025 2222", Address.PhoneNumber.FormattedForBinding);
			AssertEquals("+61 2 9025 2223", Address.FaxNumber.FormattedForBinding);

			AssertEquals("0426 829 924", Address.MobilePhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("(02) 9025 2222", Address.PhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("(02) 9025 2223", Address.FaxNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
		}

		public void TestOA_Mobile_IsManuallyVerified()
		{
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgAddress.Schema.OA_Mobile_IsManuallyVerified, OrgAddressSchema.Constants.Prefix, OrgAddressSchema.Constants.OA_Mobile, orgAddress1, orgAddress2);
		}

		public void TestOA_Phone_IsManuallyVerified()
		{
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgAddress.Schema.OA_Phone_IsManuallyVerified, OrgAddressSchema.Constants.Prefix, OrgAddressSchema.Constants.OA_Phone, orgAddress1, orgAddress2);
		}

		public void TestOA_Fax_IsManuallyVerified()
		{
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgAddress.Schema.OA_Fax_IsManuallyVerified, OrgAddressSchema.Constants.Prefix, OrgAddressSchema.Constants.OA_Fax, orgAddress1, orgAddress2);
		}

		public void TestSettingPhoneNumbersResetsIsManuallyVerifiedFlag()
		{
			var contact = Factory.NewWithValidTestData<OrgAddress>();

			contact.OA_Mobile_IsManuallyVerified = true;
			Assert("Precondition", contact.OA_Mobile_IsManuallyVerified);
			contact.OA_Mobile_Formatted = "+61 425 465 800";
			Assert(!contact.OA_Mobile_IsManuallyVerified);

			contact.OA_Phone_IsManuallyVerified = true;
			Assert("Precondition", contact.OA_Phone_IsManuallyVerified);
			contact.OA_Phone_Formatted = "+61 425 465 800";
			Assert(!contact.OA_Phone_IsManuallyVerified);

			contact.OA_Fax_IsManuallyVerified = true;
			Assert("Precondition", contact.OA_Fax_IsManuallyVerified);
			contact.OA_Fax_Formatted = "+61 425 465 800";
			Assert(!contact.OA_Fax_IsManuallyVerified);
		}

		public void TestSettingPhoneNumberCorrectlyTriggersRefreshBinding()
		{
			// Arrange
			var orgAddress = Factory.NewWithValidTestData<OrgAddressForPhoneNumberTests>();
			orgAddress.OA_Address1 = "Setup Address";
			// Act
			orgAddress.OA_Phone_Formatted = "+61 426 829 924";
			bool isRefreshBindingCalledWhenPhoneNumberIsChanged = orgAddress.IsRefreshBindingCalled;
			orgAddress.IsRefreshBindingCalled = false;
			orgAddress.OA_Phone_Formatted = "+61 426 829 924";
			bool isRefreshBindingCalledWhenPhoneNumberIsNotChanged = orgAddress.IsRefreshBindingCalled;
			// Assert
			Assert(isRefreshBindingCalledWhenPhoneNumberIsChanged);
			Assert(!isRefreshBindingCalledWhenPhoneNumberIsNotChanged);
		}

		#endregion

		#region Pattern Matching

		public void TestSettingDifferentEmailSetsPatternMatchRequiresRegen()
		{
			var org = Factory.New<OrgHeader>();

			org.MainAddress.OA_Email = "";
			org.PatternMatchRequiresRegen = false;
			org.MainAddress.OA_Email = "test@test.com";
			Assert("PatternMatchRequiresRegen should be true", org.PatternMatchRequiresRegen);

			org.PatternMatchRequiresRegen = false;
			org.MainAddress.OA_Email = "test@test.com";
			Assert("PatternMatchRequiresRegen should be false", !org.PatternMatchRequiresRegen);
		}

		#endregion

		#region Logging

		public void TestLogMessage()
		{
			Address.OA_Address1 = "Test Address";
			Address.OA_Code = "Test Address Code";
			Factory.Save();
			AssertEquals("Autolog event reference description", "Address Test Address Code", Address.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestLogMessageWithNonWesternEuropeanCharacters()
		{
			Address.OA_Address1 = "Test \u0444 Address";
			Address.OA_Code = "Test \u0444 Address Code";
			Factory.Save();
			AssertEquals("Autolog reference description stripped of Non-WE Chars", "Address (Non Western-European Characters Removed) Test  Address Code", Address.Logs.AutoCreatedLog.SL_Reference);
		}

		#endregion

		#region Property overrides

		#region TestAuthorisedToLeave

		public void TestAuthorisedToLeave()
		{
			var factory = new BusinessObjectFactory();
			var pickupOrganisation = factory.New<OrgHeader>();
			pickupOrganisation.OH_Code = "PICSYD";
			pickupOrganisation.OrganisationTypes = OrganisationTypes.Consignor;
			var pickupAddress = AddAddressToOrganisation(pickupOrganisation, "Pickup Addy", OrgAddressType.Pickup, " ");

			var deliveryOrganisation = factory.New<OrgHeader>();
			deliveryOrganisation.OH_Code = "DELSYD";
			deliveryOrganisation.OrganisationTypes = OrganisationTypes.Consignee;
			var deliveryAddress = AddAddressToOrganisation(deliveryOrganisation, "Delivery Addy", OrgAddressType.Delivery, " ");

			var buyerSuppilerLink = pickupOrganisation.BuyerLinks.AddNew(deliveryOrganisation);
			deliveryOrganisation.SupplierLinks.Add(buyerSuppilerLink);

			factory.Save();
			AssertEquals("ATL should be false by default", false, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(deliveryAddress, pickupAddress));

			pickupAddress.OA_AuthorityToLeave = "YES";
			deliveryAddress.OA_AuthorityToLeave = "YES";

			buyerSuppilerLink.OL_AuthorityToLeave = "NO";

			pickupOrganisation.MiscServ.OM_ConsigneeAuthorityToLeave = "NO";
			deliveryOrganisation.MiscServ.OM_ConsigneeAuthorityToLeave = "NO";

			factory.Save();
			AssertEquals("ATL should be false by default", true, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(deliveryAddress, pickupAddress));

			pickupAddress.OA_AuthorityToLeave = "DEF";
			deliveryAddress.OA_AuthorityToLeave = "DEF";

			buyerSuppilerLink.OL_AuthorityToLeave = "NO";

			pickupOrganisation.MiscServ.OM_ConsigneeAuthorityToLeave = "YES";
			deliveryOrganisation.MiscServ.OM_ConsigneeAuthorityToLeave = "YES";

			factory.Save();
			AssertEquals("ATL should be changed to false", false, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(deliveryAddress, pickupAddress));

			pickupAddress.OA_AuthorityToLeave = "DEF";
			deliveryAddress.OA_AuthorityToLeave = "DEF";

			buyerSuppilerLink.OL_AuthorityToLeave = "DEF";

			pickupOrganisation.MiscServ.OM_ConsignorAuthorityToLeave = "DEF";
			deliveryOrganisation.MiscServ.OM_ConsigneeAuthorityToLeave = "YES";

			factory.Save();
			AssertEquals("ATL should be changed to true", true, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(deliveryAddress, pickupAddress));

			pickupAddress.OA_AuthorityToLeave = "DEF";
			deliveryAddress.OA_AuthorityToLeave = "DEF";

			buyerSuppilerLink.OL_AuthorityToLeave = "DEF";

			pickupOrganisation.MiscServ.OM_ConsignorAuthorityToLeave = "DEF";
			deliveryOrganisation.MiscServ.OM_ConsigneeAuthorityToLeave = "DEF";

			var defaultFromRegistry = ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.Value;
			factory.Save();
			AssertEquals("ATL should update to the value of the Registry Item", defaultFromRegistry, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(deliveryAddress, pickupAddress));
		}

		OrgAddress AddAddressToOrganisation(OrgHeader organisation, ZString address1, OrgAddressType addressType, ZString dropMode)
		{
			var address = organisation.Addresses.AddNew();

			address.AddressCapability.SetCapabilityEnabled(addressType);
			address.AddressCapability.SetIsMainAddress(addressType);
			address.OA_Address1 = address1;

			return address;
		}

		#endregion

		public void TestIsDefaultCannotBeChangedWithoutSecurity()
		{
			bool messageShownToUser = false;

			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
			OrgHeader testHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			testHeader.ShowMessage += new OrgHeader.ShowMessageEventHandler(delegate
			{ messageShownToUser = true; });
			OrgAddress mainAddress = testHeader.MainAddress;
			OrgAddress address = testHeader.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			AssertEquals("No message shown to user", false, messageShownToUser);
			AssertEquals("Address is not default", false, address.IsMainAddress);

			mainAddress.AddressCapability.SetIsNotMainAddress(OrgAddressType.Office.Code);
			AssertEquals("Message shown to user", true, messageShownToUser);
			AssertEquals("Address is not default", false, address.IsMainAddress);
		}

		public void TestRelatedPortOnMainAddressIsClosestPort()
		{
			Company.OH_RL_NKClosestPort = "AUBNE";
			AssertEquals("Main address related port should be updated", Company.OH_RL_NKClosestPort, MainAddress.OA_RL_NKRelatedPortCode);

			Company.OH_RL_NKClosestPort = "///";
			AssertHasErrors("Main address related port should have errors", MainAddress.OA_RL_NKRelatedPortCodeInfo);

			Company.OH_RL_NKClosestPort = "AUSYD";
			AssertNoErrors("Main address related port should not have errors", MainAddress.OA_RL_NKRelatedPortCodeInfo);

			MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			AssertEquals("Company closest port should be updated", MainAddress.OA_RL_NKRelatedPortCode, Company.OH_RL_NKClosestPort);

			MainAddress.OA_RL_NKRelatedPortCode = "///";
			AssertHasErrors("Company closest port should have errors", Company.OH_RL_NKClosestPortInfo);

			MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertNoErrors("Company closest port should not have errors", Company.OH_RL_NKClosestPortInfo);
		}

		public void TestSetBaseOA_RL_NKRelatedPortCode()
		{
			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RL_NKRelatedPortCode = "AAAAA";
			AssertNotEquals("Precondition", "PCODE", address.OA_RL_NKRelatedPortCode);
			address.SetBaseOA_RL_NKRelatedPortCode("PCODE");
			AssertEquals("PCODE", address.OA_RL_NKRelatedPortCode);
		}

		public void TestOA_Phone_Formatted()
		{
			Address.OA_RN_NKCountryCode = ZString.Empty;
			Address.OA_RL_NKRelatedPortCode = "AUSYD";
			Company.OH_RL_NKClosestPort = null;
			Address.OA_Phone_Formatted = "61 2 9025 1101";
			var expectedFormattedPhone = "+61 2 9025 1101";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			var expectedNormalizedPhone = "+61290251101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_RN_NKCountryCode = ZString.Empty;
			Address.OA_RL_NKRelatedPortCode = "????";
			Company.OH_RL_NKClosestPort = "AUSYD";
			Address.OA_Phone_Formatted = "2 9025 2222";
			expectedFormattedPhone = "+61 2 9025 2222";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+61290252222";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);
			var expectedLocalNumberIfLoggedInSameCountry = "(02) 9025 2222";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, Address.OA_Phone_FormattedLocalNumberIfLoggedInSameCountry);

			Address.OA_RN_NKCountryCode = "AU";
			Address.OA_RL_NKRelatedPortCode = "????";
			Company.OH_RL_NKClosestPort = ZString.Empty;
			Address.OA_Phone_Formatted = "2 8001 2200";
			expectedFormattedPhone = "+61 2 8001 2200";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+61280012200";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);
			expectedLocalNumberIfLoggedInSameCountry = "(02) 8001 2200";
			AssertEquals("The tooltip should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, Address.OA_Phone_FormattedLocalNumberIfLoggedInSameCountry);

			Address.OA_Phone_Formatted = "+86 156-0113-198 1";
			expectedFormattedPhone = "+86 156 0113 1981";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+8615601131981";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be blank", string.Empty, Address.OA_Phone_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestOA_Phone_FormattedForItaly()
		{
			Address.OA_RL_NKRelatedPortCode = null;
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "ITATO";

			Address.OA_Phone_Formatted = "39 06 698 83913";
			var expectedFormattedPhone = "+39 06 6988 3913";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			var expectedNormalizedPhone = "+390669883913";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "39 032 19 5724";
			expectedFormattedPhone = "+39 0321 95724";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+39032195724";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			OrgHeader testHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			OrgAddress mainAddress = testHeader.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "ITATO";
			mainAddress.OA_Phone_Formatted = "39 032 19 5724";
			mainAddress.RunPreSaveValidation();
			Assert(!mainAddress.OA_Phone_FormattedInfo.Notifications.Any());
		}

		public void TestOA_Phone_FormattedForMonacoPhones()
		{
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "MCMON";

			Address.OA_Phone_Formatted = "37744149061";
			var expectedFormattedPhone = "+377 44 149 061";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			var expectedNormalizedPhone = "+37744149061";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);
		}

		public void TestOA_Phone_FormattedForSloveniaPhones()
		{
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "SIPRA";

			Address.OA_Phone_Formatted = "38643149061";
			var expectedFormattedPhone = "+386 43 149 061";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			var expectedNormalizedPhone = "+38643149061";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "38649149061";
			expectedFormattedPhone = "+386 49 149 061";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+38649149061";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);
		}

		public void TestOA_PhoneForSweden()
		{
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "SEABS";

			Address.OA_Phone_Formatted = "40 355381";
			var expectedFormattedPhone = "+46 40 35 53 81";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			var expectedNormalizedPhone = "+4640355381";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "040 355381";
			expectedFormattedPhone = "+46 40 35 53 81";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+4640355381";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "037134000";
			expectedFormattedPhone = "+46 371 340 00";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+4637134000";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "0854570140";
			expectedFormattedPhone = "+46 8 545 701 40";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+46854570140";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "+46854570140";
			expectedFormattedPhone = "+46 8 545 701 40";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+46854570140";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "+46 8 54570140";
			expectedFormattedPhone = "+46 8 545 701 40";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+46854570140";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "090 355381";
			expectedFormattedPhone = "+46 90 35 53 81";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+4690355381";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "465457014";
			expectedFormattedPhone = "+46 46 545 70 14";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+46465457014";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "+46 46 46701";
			expectedFormattedPhone = "+46 46 467 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+464646701";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "46 46701";
			expectedFormattedPhone = "+46 46 467 01";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+464646701";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);
		}

		public void TestOA_PhoneForSingapore()
		{
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "SGSIN";

			Address.OA_Phone_Formatted = "6526 5214";
			var expectedFormattedPhone = "+65 6526 5214";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			var expectedNormalizedPhone = "+6565265214";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "65 6526 5214";
			expectedFormattedPhone = "+65 6526 5214";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+6565265214";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);

			Address.OA_Phone_Formatted = "+65 6526 5214";
			expectedFormattedPhone = "+65 6526 5214";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Phone_Formatted);
			expectedNormalizedPhone = "+6565265214";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Phone);
		}

		public void TestOA_Phone_FormattedIsUpdatedWhenUnlocoIsUpdated()
		{
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "SGSIN";
			Address.OA_Phone_Formatted = "0426829924";
			Assert("Predcondition - There should be validation errors.", Address.OA_Phone_FormattedInfo.Notifications.Any());
			AssertEquals("Precondition - The phone number should stay unchanged.", "0426829924", Address.OA_Phone_Formatted);

			Address.OA_RL_NKRelatedPortCode = "AUSYD";

			Assert("There should not be validation errors after the UNLOCO is set correctly.", !Address.OA_Phone_FormattedInfo.Notifications.Any());
			AssertEquals("The phone number should also be formatted after the UNLOCO is set correctly.", "+61 426 829 924", Address.OA_Phone_Formatted);
		}

		public void TestOA_Fax_Formatted()
		{
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "AUSYD";
			Address.OA_Fax_Formatted = "61 2 9025 1101";
			var expectedFormattedPhone = "+61 2 9025 1101";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Fax_Formatted);
			var expectedNormalizedPhone = "+61290251101";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Fax);

			Address.OA_RL_NKRelatedPortCode = "????";
			Company.OH_RL_NKClosestPort = "AUSYD";
			Address.OA_Fax_Formatted = "2 9025 2222";
			expectedFormattedPhone = "+61 2 9025 2222";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Fax_Formatted);
			expectedNormalizedPhone = "+61290252222";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Fax);
			var expectedLocalNumberIfLoggedInSameCountry = "(02) 9025 2222";
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, Address.OA_Fax_FormattedLocalNumberIfLoggedInSameCountry);

			Address.OA_Fax_Formatted = "+86 156-0113-198 1";
			expectedFormattedPhone = "+86 156 0113 1981";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Fax_Formatted);
			expectedNormalizedPhone = "+8615601131981";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Fax);
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be blank", string.Empty, Address.OA_Fax_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestOA_Fax_Formatted_MustNotBeEmptyIfContactHasDocumetsToReciveByFax()
		{
			OrgContact orgContact = Company.Contacts.AddNew();
			orgContact.OC_ContactName = "Alex";
			MainAddress.OA_Fax_Formatted = "654321";
			orgContact.OC_Fax_Formatted = "123456";
			Company.RunPreSaveValidation();
			AssertNoRowErrors(orgContact);

			OrgDocument orgDocument = orgContact.Documents.AddNew();
			orgDocument.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			orgDocument.OD_DocumentGroup = ContactType.NotifyParty.ToString();
			MainAddress.OA_Fax_Formatted = "";
			orgContact.OC_Fax_Formatted = "";
			Company.RunPreSaveValidation();
			AssertHasRowError(orgContact, (NoResString)"Please enter a fax number to be used for delivering documents to this Organization.");

			MainAddress.OA_Fax_Formatted = "+61 2 9025 2222";
			orgContact.OC_Fax_Formatted = "";
			Company.RunPreSaveValidation();
			AssertNoRowErrors(orgContact);
			AssertHasRowWarning(orgContact, (NoResString)"The fax number from the main details page of this organization will be used for delivering documents to this Organization.");

			MainAddress.OA_Fax_Formatted = "";
			orgContact.OC_Fax_Formatted = "1233456";
			Company.RunPreSaveValidation();

			AssertNoRowErrors(orgContact);
			AssertNoRowWarnings(orgContact);
		}

		public void TestOA_Mobile_Formatted()
		{
			Company.OH_RL_NKClosestPort = null;
			Address.OA_RL_NKRelatedPortCode = "AUSYD";
			Address.OA_Mobile_Formatted = "0426 829 924";
			var expectedFormattedPhone = "+61 426 829 924";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Mobile_Formatted);
			var expectedNormalizedPhone = "+61426829924";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Mobile);

			Address.OA_RL_NKRelatedPortCode = "????";
			Company.OH_RL_NKClosestPort = "AUSYD";
			var expectedLocalNumberIfLoggedInSameCountry = "0426 829 924";
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be shown correctly", expectedLocalNumberIfLoggedInSameCountry, Address.OA_Mobile_FormattedLocalNumberIfLoggedInSameCountry);

			Address.OA_Mobile_Formatted = "+86 156-0113-198 1";
			expectedFormattedPhone = "+86 156 0113 1981";
			AssertEquals("Number should be formatted correctly", expectedFormattedPhone, Address.OA_Mobile_Formatted);
			expectedNormalizedPhone = "+8615601131981";
			AssertEquals("Number should be normalized correctly", expectedNormalizedPhone, Address.OA_Mobile);
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be blank", string.Empty, Address.OA_Mobile_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestOA_CompanyNameOverride()
		{
			Address.Header.PatternMatchRequiresRegen = false;
			Assert(!Address.Header.PatternMatchRequiresRegen);
			Address.OA_CompanyNameOverride = "muhaha";
			Assert(Address.Header.PatternMatchRequiresRegen);
		}

		public void TestOA_IsActiveForcesPatternRegen()
		{
			Address.Header.PatternMatchRequiresRegen = false;
			Assert(!Address.Header.PatternMatchRequiresRegen);
			Address.OA_IsActive = !Address.OA_IsActive;
			Assert(Address.Header.PatternMatchRequiresRegen);
		}

		#endregion

		#region ICanDelete

		public void TestCanDelete()
		{
			OrgHeader testHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			OrgAddress address1 = testHeader.MainAddress;
			OrgAddress address2 = testHeader.Addresses.AddNew();

			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);

			AssertEquals("Cannot delete main address.", false, address1.CanDelete);
			AssertEquals("Cannot delete main address.", address1.ReasonForNotAbleToDelete);
			AssertEquals("Can delete other address", true, address2.CanDelete);
			AssertEquals(ZString.Empty, address2.ReasonForNotAbleToDelete);

			address1.AddressCapability.SetCapabilityDisabled(OrgAddressType.Office.Code);

			AssertEquals("Can delete non-main address", true, address1.CanDelete);
			AssertEquals(ZString.Empty, address1.ReasonForNotAbleToDelete);
			AssertEquals("Can delete other address", true, address2.CanDelete);
			AssertEquals(ZString.Empty, address2.ReasonForNotAbleToDelete);
		}

		public void TestCanDeleteIncludingIsReferencedByContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Name1";
			contact1.OC_IsActive = true;
			contact1.OC_OA_OrgAddress = address.PK;
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Name2";
			contact2.OC_IsActive = false;
			contact2.OC_OA_OrgAddress = address.PK;

			Assert(!address.IsMainAddress);
			Assert(address.IsReferencedByContact);
			Assert(!address.CanDelete);
			AssertEquals("This address cannot be deleted as it is referenced by one or more contacts: \r\n'Name1(Active)' and 'Name2(Inactive)'", address.ReasonForNotAbleToDelete);

			contact1.OC_OA_OrgAddress = ZGuid.Empty;
			contact2.OC_OA_OrgAddress = ZGuid.Empty;
			Assert(!address.IsReferencedByContact);
			Assert(address.CanDelete);
			AssertEquals(string.Empty, address.ReasonForNotAbleToDelete);
		}

		#endregion

		#region New Properties

		public void TestLocalControlledPremisesIDGetter()
		{
			AssertEquals("Precondition", ZString.Empty, Address.LocalControlledPremisesID);
			OrgCusCode customsCode = Address.Header.CustomsCodes.AddNew();
			customsCode.OK_CustomsRegNo = "123";
			customsCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			customsCode.OK_OA_PremisesAddress = Address.PK;
			AssertEquals("Premise code", "123", Address.LocalControlledPremisesID);
		}

		public void TestLocalControlledPremisesIDSetter()
		{
			AssertEquals("Precondition", ZString.Empty, Address.LocalControlledPremisesID);
			Address.LocalControlledPremisesID = "XYZ";
			AssertEquals("XYZ", Address.LocalControlledPremisesID);
		}

		public void TestDepotLocalControlledPremisesIDGetter()
		{
			AssertEquals("Precondition", ZString.Empty, Address.DepotLocalControlledPremisesID);
			OrgCusCode customsCode = Address.Header.CustomsCodes.AddNew();
			customsCode.OK_CustomsRegNo = "123";
			customsCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.DepotControlledPremisesID;
			customsCode.OK_OA_PremisesAddress = Address.PK;
			AssertEquals("Premise code", "123", Address.DepotLocalControlledPremisesID);
		}

		public void TestDepotLocalControlledPremisesIDSetter()
		{
			AssertEquals("Precondition", ZString.Empty, Address.DepotLocalControlledPremisesID);
			Address.DepotLocalControlledPremisesID = "XYZ";
			AssertEquals("XYZ", Address.DepotLocalControlledPremisesID);
			Address.DepotLocalControlledPremisesID = "YYY";
			AssertEquals("YYY", Address.DepotLocalControlledPremisesID);
		}

		public void TestWarehouseLocalControlledPremisesIDGetter()
		{
			AssertEquals("Precondition", ZString.Empty, Address.WarehouseLocalControlledPremisesID);
			OrgCusCode customsCode = Address.Header.CustomsCodes.AddNew();
			customsCode.OK_CustomsRegNo = "123";
			customsCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			customsCode.OK_OA_PremisesAddress = Address.PK;
			AssertEquals("Premise code", "123", Address.WarehouseLocalControlledPremisesID);
		}

		public void TestWarehouseLocalControlledPremisesIDSetter()
		{
			AssertEquals("Precondition", ZString.Empty, Address.WarehouseLocalControlledPremisesID);
			Address.WarehouseLocalControlledPremisesID = "XYZ";
			AssertEquals("XYZ", Address.WarehouseLocalControlledPremisesID);
			Address.WarehouseLocalControlledPremisesID = "YYY";
			AssertEquals("YYY", Address.WarehouseLocalControlledPremisesID);
		}

		public void TestPatternMatches()
		{
			OrgHeader testHeader = OrgHeader.New(Factory);
			testHeader.OH_FullName = "Name";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "Address1";
			testHeader.MainAddress.OA_City = "City";
			testHeader.MainAddress.OA_CompanyNameOverride = "Name2";

			AssertEquals("PatternMatchesForThisOrg.Count - 1 Address, 1 Name Override", 2, testHeader.PatternMatchesForThisOrg.Count);
			OrgAddress address2 = testHeader.Addresses.AddNew();
			address2.OA_Address1 = "DifferentAddress";
			address2.OA_City = "DifferentCity";

			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);
			AssertEquals("PatternMatchesForThisOrg.Count - 2 Addresses, 1 Name Override", 3, testHeader.PatternMatchesForThisOrg.Count);

			address2.OA_CompanyNameOverride = "name3";
			testHeader.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(testHeader);
			AssertEquals("PatternMatchesForThisOrg.Count - 2 Addresses, 2 Name Overrides", 4, testHeader.PatternMatchesForThisOrg.Count);

			address2.Delete();

			AssertEquals("PatternMatchesForThisOrg.Count - 1 Address, 1 Name Override", 2, testHeader.PatternMatchesForThisOrg.Count);
			Assert("PatternMatch should NOT require regen", !testHeader.PatternMatchRequiresRegen);
		}

		public void TestIsMainAddress()
		{
			TestIsMainAddress_Core(pokeAddressCapabilitiesCollection: true);
		}

		public void TestIsMainAddress_WithUnloadedCapabilitiesCollection()
		{
			TestIsMainAddress_Core(pokeAddressCapabilitiesCollection: false);
		}

		void TestIsMainAddress_Core(bool pokeAddressCapabilitiesCollection)
		{
			Company.Delete();
			Address.Delete();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = header.MainAddress;
			mainAddress.FillWithValidTestData();
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			mainAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Payables.Code);
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			mainAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables.Code);
			mainAddress.AddressCapability.SetIsNotMainAddress(OrgAddressType.Receivables.Code);

			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var headerInFactory2 = factory2.Load<OrgHeader>(header.PK);
			var mainAddressInFactory2 = factory2.Load<OrgAddress>(mainAddress.PK);

			if (pokeAddressCapabilitiesCollection)
			{
				var poke = mainAddressInFactory2.AddressCapability; // Loads capabilities in collection
			}

			AssertEquals(true, mainAddressInFactory2.IsMainAddressOfType(OrgAddressType.Payables));
			AssertEquals("Precondition", 1, factory2.GetTableHitCount(OrgAddressCapabilitySchema.Constants.TableName));

			AssertEquals(true, mainAddressInFactory2.IsMainAddressOfType(OrgAddressType.Office));
			AssertEquals(false, mainAddressInFactory2.IsMainAddressOfType(OrgAddressType.Receivables));
			AssertEquals(false, mainAddressInFactory2.IsMainAddressOfType(OrgAddressType.Sales));
			AssertEquals("Should not have hit the database further", 1, factory2.GetTableHitCount(OrgAddressCapabilitySchema.Constants.TableName));
		}

		#endregion

		#region New Bound Properties

		public void TestStateListHasMembers()
		{
			Address.Header.OH_RL_NKClosestPort = "AUSYD";
			Assert("OA_State_List.Count > 0", Address.OA_State_List.Count > 0);
			Assert("StateListHasMembers is true", Address.StateListHasMembers);

			Address.Header.OH_RL_NKClosestPort = "SGSIN";
			Assert("OA_State_List.Count = 0", Address.OA_State_List.Count == 0);
			Assert("StateListHasMembers is false", !Address.StateListHasMembers);
		}

		public void TestStateListDoeNotHaveMembers()
		{
			Address.Header.OH_RL_NKClosestPort = "AUSYD";
			Assert("OA_State_List.Count > 0", Address.OA_State_List.Count > 0);
			Assert("StateListDoesNotHaveMembers is false", !Address.StateListDoesNotHaveMembers);

			Address.Header.OH_RL_NKClosestPort = "SGSIN";
			Assert("OA_State_List.Count = 0", Address.OA_State_List.Count == 0);
			Assert("StateListDoesNotHaveMembers is true", Address.StateListDoesNotHaveMembers);
		}

		public void TestOA_State_List_For_Current_Address()
		{
			Address.Header.OH_RL_NKClosestPort = "AUSYD";
			object pk = Address.OA_State_List[0].PK;

			AssertNotNull(pk);

			OrgAddress address1 = Company.Addresses.AddNew();
			object pk1 = address1.OA_State_List_For_Current_Address[0].PK;

			AssertNotNull(pk1);
			AssertEquals(pk, pk1);

			address1.OA_RL_NKRelatedPortCode = "GBLON";
			AssertEquals("GB", address1.OA_RN_NKCountryCode);
			pk1 = address1.OA_State_List_For_Current_Address[0].PK;

			AssertNotNull(pk1);
			AssertNotEquals(pk, pk1);

			Address.Header.OH_RL_NKClosestPort = "GBLON";
			pk = Address.OA_State_List[0].PK;

			AssertNotNull(pk);
			AssertEquals(pk, pk1);

			address1.OA_RL_NKRelatedPortCode = "";
			Address.Header.OH_RL_NKClosestPort = "USLAX";
			pk = Address.OA_State_List[0].PK;
			pk1 = address1.OA_State_List_For_Current_Address[0].PK;
			AssertNotEquals(pk, pk1);

			address1.OA_RN_NKCountryCode = "";
			pk1 = address1.OA_State_List_For_Current_Address[0].PK;
			AssertEquals(pk, pk1);
		}

		public void TestCityFallback()
		{
			Address.OA_City = ZString.Empty;
			Address.Header.OH_RL_NKClosestPort = "AUMEL";
			AssertEquals("Fall back to Header UNLOCO", "Melbourne", Address.CityFallback);
			Address.OA_City = "Alexandria";
			AssertEquals("Use City", "Alexandria", Address.CityFallback);
		}

		public void TestShouldShowErrorsInAddressesOfContactsAndAppointedAgentWhenDeactivating()
		{
			Address.OA_IsActive = true;

			var orgContact = Address.Header.Contacts.AddNew();
			Address.OA_Address1 = "Kiev";
			orgContact.OC_ContactName = "Alex";
			orgContact.OC_OA_OrgAddress = Address.PK;
			var appointedAgentPort = Address.Header.AppointedAgentPorts.AddNew();
			appointedAgentPort.O5_SeaAirCarrierOrForwarderType = OrgAppointedAgentPorts.Forwarder;
			appointedAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPort.O5_PortOrCountry = "AU";
			appointedAgentPort.O5_OA_AgentOfficeAddress = Address.PK;
			var appointedGatewayAgentPort = Address.Header.AppointedGatewayAgentPorts.AddNew();
			appointedGatewayAgentPort.O5_SeaAirCarrierOrForwarderType = OrgAppointedAgentPorts.Forwarder;
			appointedGatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedGatewayAgentPort.O5_PortOrCountry = "AU";
			appointedGatewayAgentPort.O5_OA_AgentOfficeAddress = Address.PK;

			Address.OA_IsActive = false;
			AssertHasError(orgContact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
			AssertHasError(appointedAgentPort.O5_OA_AgentOfficeAddressInfo, "Only active address can be set as Agent Office Address");
			AssertHasError(appointedGatewayAgentPort.O5_OA_AgentOfficeAddressInfo, "Only active address can be set as Agent Office Address");

			Address.OA_IsActive = true;
			AssertNoError(orgContact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
			AssertNoError(appointedAgentPort.O5_OA_AgentOfficeAddressInfo, "Only active address can be set as Agent Office Address");
			AssertNoError(appointedGatewayAgentPort.O5_OA_AgentOfficeAddressInfo, "Only active address can be set as Agent Office Address");
		}

		#endregion

		#region UsageComment

		public void TestSetDefaultUsageComment()
		{
			Address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			Address.OA_Address1 = "Test Address";
			ZString expectedUsageComment = Address.OA_Address1.Left(25);
			AssertEquals("Default Usage comment", expectedUsageComment, Address.OA_Code);

			Address.OA_Address1 = "Test Really Long Address";
			ZString addressForUsageComment = Address.OA_Address1.Length > Address.MaxLengthOfAddressForUsageComment ? Address.OA_Address1.Substring(0, Address.MaxLengthOfAddressForUsageComment) : Address.OA_Address1;
			expectedUsageComment = addressForUsageComment;
			AssertEquals("Default Usage comment", expectedUsageComment, Address.OA_Code);
		}

		public void TestSetDefaultUsageComment2()
		{
			ZString usageComment = "ZZZ";
			Address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			Address.OA_Address1 = usageComment;
			AssertEquals("Default Usage comment", usageComment, Address.OA_Code);

			Address.SetDefaultUsageComment(new string[] { "ZZZ" });
			AssertEquals("Shouldn't be ZZZ", usageComment + "1", Address.OA_Code);

			Address.SetDefaultUsageComment(new string[] { "zzz" });
			AssertEquals("Shouldn't be ZZZ", usageComment + "1", Address.OA_Code);
		}

		public void TestSetDefaultUsageComment3()
		{
			Address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			Address.OA_Address1 = "Test Address";
			ZString expectedUsageComment = Address.OA_Address1.Left(25);
			AssertEquals("Default Usage comment", expectedUsageComment, Address.OA_Code);

			(Address as ISupportDataImporting).IsImportingData = true;
			Address.OA_Address1 = "80kg bench";
			AssertEquals("Same comment as IsImportingData = true", expectedUsageComment, Address.OA_Code);
		}

		public void TestSetDefaultUsageComment_DoesOverrideIfNotInDatabase()
		{
			Address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			Address.OA_Address1 = "Test Address";
			var firstExpectedUsageComment = Address.OA_Address1.Left(25);
			Assert("Address is not in database", !Address.IsInDatabase);
			AssertEquals("Default Usage comment", firstExpectedUsageComment, Address.OA_Code);

			Address.OA_Address1 = "Test Address Updated";
			var secondExpectedUsageComment = Address.OA_Address1.Left(25);
			Assert("Address is not in database", !Address.IsInDatabase);
			AssertEquals("Usage comment should be updated", secondExpectedUsageComment, Address.OA_Code);
		}

		public void TestSetDefaultUsageComment_DoesNotOverrideIfInDatabase()
		{
			Address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			Address.OA_Address1 = "Test Address";
			var expectedUsageComment = Address.OA_Address1.Left(25);
			Assert("Address is not in database", !Address.IsInDatabase);
			AssertEquals("Default Usage comment", expectedUsageComment, Address.OA_Code);

			Factory.Save();

			Address.OA_Address1 = "Test Address Updated";
			Assert("Address is in database", Address.IsInDatabase);
			AssertEquals("Usage comment should not be updated", expectedUsageComment, Address.OA_Code);
		}

		public void TestMakeUsageCommentUnique()
		{
			OrgAddress address1 = Collection.AddNew();
			OrgAddress address2 = Collection.AddNew();
			OrgAddress address3 = Collection.AddNew();

			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Sales);
			address3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			address1.OA_Address1 = "Test Long Name For Uniqueness";
			address2.OA_Address1 = "Test Long Name For Uniqueness";
			address3.OA_Address1 = "Test Long Name For Uniqueness";
			ZString address1ExpectedUsage = "Test Long Name For Unique";
			ZString address2ExpectedUsage = "Test Long Name For Uniqu1";
			ZString address3ExpectedUsage = "Test Long Name For Uniqu2";

			AssertEquals("Address1.UsageComment", address1ExpectedUsage, address1.OA_Code);
			AssertEquals("Address2.UsageComment", address2ExpectedUsage, address2.OA_Code);
			AssertEquals("Address3.UsageComment", address3ExpectedUsage, address3.OA_Code);
		}

		#endregion

		#region Timetable

		public void TestSetDefaultTimetable()
		{
			var temporaryDefaultOrgTimetable = new DefaultOrgTimetableSettingsCollection();
			var defaultSettings = temporaryDefaultOrgTimetable.AddNew();
			var item1 = defaultSettings.Timetables.AddNew();
			item1.Type = OrgTimetableType.Codes.Pickup;
			item1.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0);
			item1.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0);
			item1.Day = "MON";

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryDefaultOrgTimetable))
			{
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				var timetables = orgAddress.Timetables;
				AssertEquals(timetables.Count, 1);
				AssertEquals(timetables[0].OTT_Type, OrgTimetableType.Codes.Pickup);
				AssertEquals(timetables[0].DayOfWeek, "MON");
				AssertEquals(timetables[0].OTT_TimeFrom, new ZDateTime(1900, 1, 1, 9, 0, 0));
				AssertEquals(timetables[0].OTT_TimeTo, new ZDateTime(1900, 1, 1, 17, 0, 0));
			}
		}

		#endregion

		#region Set PatternMatchRequiresRegen

		public void TestPatternMatchRequiresRegen()
		{
			OrgAddress mainAddress = Company.MainAddress;
			OrgAddress otherAddress = Company.Addresses.AddNew();

			CheckForProperty(mainAddress, OrgAddressSchema.OA_Address1, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_Address1, true);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_Address2, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_Address2, true);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_City, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_City, true);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_PostCode, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_PostCode, true);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_State, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_State, true);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_Phone, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_Phone, true);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_Fax, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_Fax, true);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_Mobile, false);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_Mobile, false);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_Email, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_Email, true);

			CheckForProperty(mainAddress, OrgAddressSchema.OA_RL_NKRelatedPortCode, true);
			CheckForProperty(otherAddress, OrgAddressSchema.OA_RL_NKRelatedPortCode, true);
		}

		void CheckForProperty(OrgAddress address, SchemaColumn column, ZBool expectedResult)
		{
			Company.PatternMatchRequiresRegen = false;
			address[column.Name] = "blah";
			AssertEquals("PatternMatchRequiresRegen for property " + column.Name, expectedResult, Company.PatternMatchRequiresRegen);
		}

		#endregion

		public void TestOnLoaded_ShouldUpdateUnrestrictedditionalAddressInformation()
		{
			// Arrange.

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_AdditionalAddressInformation = "[_MOCK_ADDITIONAL_ADDRESS_INFORMATION_]";

			Factory.Save();

			// Act.

			var address2 = new BusinessObjectFactory().Load<OrgAddress>(address1.PK);

			// Assert.

			AssertNotNull(address2);
			AssertEquals("[_MOCK_ADDITIONAL_ADDRESS_INFORMATION_]", address2.UnrestrictedAdditionalAddressInformation);
		}

		#region Deduplication

		public void TestSettingEmailInvokesDeduplication()
		{
			//Arrange
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var dedupeStarted = false;
			address.Header.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)address.Header).ShouldRunDeduplication = true;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//Act
				address.OA_Email = "a@b.com";

				//Assert
				AssertEquals(true, dedupeStarted);
			}
		}

		public void TestSettingEmailDoesNotInvokeDeduplication()
		{
			//Arrange
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var dedupeStarted = false;
			EventHandler eventStarted = (o, e) => { dedupeStarted = true; };
			address.Header.DeduplicationStarted += eventStarted;
			((IDeduplicatable)address.Header).ShouldRunDeduplication = false;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//Act
				address.OA_Email = "a@b.com";

				//Assert
				AssertEquals(false, dedupeStarted);
			}

			address.Header.DeduplicationStarted -= eventStarted;
		}

		public void TestSettingValidationStatusInvokesDeduplication()
		{
			//Arrange
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var dedupeStarted = false;
			EventHandler eventStarted = (o, e) => { dedupeStarted = true; };
			address.Header.DeduplicationStarted += eventStarted;
			((IDeduplicatable)address.Header).ShouldRunDeduplication = true;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//Act
				address.ValidationStatus = "VAD";

				//Assert
				AssertEquals(true, dedupeStarted);
			}

			address.Header.DeduplicationStarted -= eventStarted;
		}

		public void TestSettingValidationStatusDoesNotInvokeDeduplication()
		{
			//Arrange
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var dedupeStarted = false;
			EventHandler eventStarted = (o, e) => { dedupeStarted = true; };
			address.Header.DeduplicationStarted += eventStarted;
			((IDeduplicatable)address.Header).ShouldRunDeduplication = false;

			//Act
			address.ValidationStatus = "VAD";

			//Assert
			AssertEquals(false, dedupeStarted);

			address.Header.DeduplicationStarted -= eventStarted;
		}

		public void TestTriggerDeduplicationWhenAddressValidationWebServiceDisabled()
		{
			AssertTriggerDeduplicationResult(false, true, true);
		}

		public void TestTriggerDeduplicationWhenCountryAddressValidationDisabled()
		{
			AssertTriggerDeduplicationResult(true, false, true);
		}

		public void TestTriggerDeduplicationWhenCountryIsNull()
		{
			AssertTriggerDeduplicationResult(true, false, true, false);
		}

		public void TestNotTriggerDeduplication()
		{
			AssertTriggerDeduplicationResult(true, true, false);
		}

		void AssertTriggerDeduplicationResult(bool enableAddressValidationWebService, bool enableCountryAddressValidation, bool triggerDeduplication, bool hasCountryCode = true)
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var rawRegistry = Env.Instance.Registry.EnableAddressValidationWebService;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = hasCountryCode ? Constants.CountryCodes.Australia : string.Empty;
			address.OA_Address2 = "Address 2";
			address.OA_PostCode = "00001";
			address.OA_City = "SYDNEY";
			address.OA_State = "ACT";

			var deduplicationRunTimes = 0;
			address.Header.DeduplicationStarted += (sender, e) => { deduplicationRunTimes++; };
			((IDeduplicatable)address.Header).ShouldRunDeduplication = true;

			AssertEquals("Precondition", 0, deduplicationRunTimes);

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = enableAddressValidationWebService;

				var countryCode = Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, Constants.CountryCodes.Australia);
				AssertNotNull("Precondition", countryCode);
				var disabledAddressValidationCountries = enableCountryAddressValidation ? new AddressValidationDisabledCountryItemCollection() : DisabledCountryItemCollectionTestHelper.GetCollection(countryCode.PK, disabledForOrgAddress: true);

				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, disabledAddressValidationCountries))
				{
					CombineAssertions(() =>
					{
						AssertDuplicationRunTimes(0, address.OA_Address2Info, "1");

						AssertDuplicationRunTimes(0, address.OA_PostCodeInfo, ZString.Empty);
						AssertDuplicationRunTimes(0, address.OA_PostCodeInfo, " ");
						AssertDuplicationRunTimes(triggerDeduplication ? 1 : 0, address.OA_PostCodeInfo, "00002");
						AssertDuplicationRunTimes(triggerDeduplication ? 1 : 0, address.OA_PostCodeInfo, "00002");

						AssertDuplicationRunTimes(triggerDeduplication ? 2 : 0, address.OA_CityInfo, "HOUSTON");

						AssertDuplicationRunTimes(triggerDeduplication ? 3 : 0, address.OA_StateInfo, "QLD");
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistry;
			}

			void AssertDuplicationRunTimes(int runTimes, ZPropertyInfo zPropertyInfo, ZString value)
			{
				zPropertyInfo.SetValueFromString(value);
				AssertEquals(runTimes, deduplicationRunTimes);
			}
		}

		#endregion

		#region OnSaving

		OrgHeader HeaderInNewFactory
		{
			get
			{
				OrgHeader result = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
				result.OH_FullName = "Test COMPANY";
				result.OH_RL_NKClosestPort = "AUSYD";
				return result;
			}
		}

		public void TestOnSaving()
		{
			Company = HeaderInNewFactory;
			Company.MainAddress.OA_Address1 = "";
			Company.MainAddress.OA_City = "Sydney";
			Company.MainAddress.OA_PostCode = "2000";
			Company.MainAddress.OA_State = "NSW";
			Company.Factory.Save();

			AssertNoErrors(Company);
			AssertEquals("Address set to Address Not On File", OrgAddress.AddressNotOnFile, Company.MainAddress.OA_Address1);

			OrgAddress address2 = Company.Addresses.AddNew();
			address2.OA_Address1 = "Some Place";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup.Code);
			Company.Factory.Save();
			AssertEquals("OA_Address1 should be set to 'Some Place'", "Some Place", address2.OA_Address1);

			address2.OA_Address1 = "Some Place";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			Company.Factory.Save();
			AssertEquals("OA_Address1 should be set to 'Some Place'", "Some Place", address2.OA_Address1);

			address2.OA_Address1 = "Some Place";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup.Code);
			Company.Factory.Save();
			AssertEquals("OA_Address1 should be set to 'Some Place'", "Some Place", address2.OA_Address1);

			Company = HeaderInNewFactory;
			address2 = Company.Addresses.AddNew();
			try
			{
				address2.OA_Address1 = "";
				address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
				address2.AddressCapability.SetIsNotMainAddress(OrgAddressType.Pickup.Code);
				Company.Factory.Save();
			}
			catch (ZSaveException e)
			{
				if (e.InnerException != null && e.InnerException.InnerException != null && e.InnerException.InnerException is SqlException)
				{
					Assert(e.InnerException.InnerException.Message.Contains(OrgAddressSchema.OA_Address1.Name));
					AssertEquals("OA_Address1 should be set to Empty", "", address2.OA_Address1);
				}
				else
				{
					throw;
				}
			}

			Company = HeaderInNewFactory;
			address2 = Company.Addresses.AddNew();
			try
			{
				address2.OA_Address1 = "";
				address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
				address2.AddressCapability.SetIsNotMainAddress(OrgAddressType.Office.Code);
				Company.Factory.Save();
			}
			catch (ZSaveException e)
			{
				if (e.InnerException != null && e.InnerException.InnerException != null && e.InnerException.InnerException is SqlException)
				{
					Assert(e.InnerException.InnerException.Message.Contains(OrgAddressSchema.OA_Address1.Name));
					AssertEquals("OA_Address1 should be set to Empty", "", address2.OA_Address1);
				}
				else
				{
					throw;
				}
			}

			Company = HeaderInNewFactory;
			address2 = Company.Addresses.AddNew();
			try
			{
				address2.OA_Address1 = "";
				address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
				address2.AddressCapability.SetIsNotMainAddress(OrgAddressType.Pickup.Code);
				Company.Factory.Save();
			}
			catch (ZSaveException e)
			{
				if (e.InnerException != null && e.InnerException.InnerException != null && e.InnerException.InnerException is SqlException)
				{
					Assert(e.InnerException.InnerException.Message.Contains(OrgAddressSchema.OA_Address1.Name));
					AssertEquals("OA_Address1 should be set to Empty", "", address2.OA_Address1);
				}
				else
				{
					throw;
				}
			}
		}

		#endregion

		public void TestDelete()
		{
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Phone_IsManuallyVerified = true;
			address1.OA_Fax_IsManuallyVerified = true;
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Mobile_IsManuallyVerified = true;

			var acks1 = new GenCustomAddOnRuleAckCollection(address1);
			var acks2 = new GenCustomAddOnRuleAckCollection(address2);
			AssertEquals("Precondition", 2, acks1.Count);
			AssertEquals("Precondition", 1, acks2.Count);

			address1.Delete();

			AssertEquals(0, acks1.Count);
			AssertEquals(1, acks2.Count);
		}

		public void TestDeleteAdditionalAddressInfos()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test Address";
			address.OA_OH = header.PK;

			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Code = "Test Address 2";
			address2.OA_OH = header.PK;

			var addressInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo1.OAI_AdditionalInfo = "Test Additional Info 1";
			addressInfo1.OAI_OA_Address = address.PK;

			var addressInfo2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo2.OAI_AdditionalInfo = "Test Additional Info 2";
			addressInfo2.OAI_OA_Address = address.PK;

			var addressInfo3 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo3.OAI_AdditionalInfo = "Test Additional Info 3";
			addressInfo3.OAI_OA_Address = address2.PK;

			Factory.Save();

			address.Delete();

			CombineAssertions(() =>
			{
				Assert(addressInfo1.IsDeleted);
				Assert(addressInfo2.IsDeleted);
				Assert(!addressInfo3.IsDeleted);
			});
		}

		public void TestCustomsCodes()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address = org.MainAddress;
			AssertEquals(typeof(OrgAddressCusCodeCollection), address.CustomsCodes.GetType());
		}

		public void TestHasGlowInterfaceReference()
		{
			AssertNotNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(OrgAddress), true));
		}

		public void TestConvertAddressToAnalysisText()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "main address1";
			address.OA_Address2 = "main address2";
			address.OA_City = "Sydney";
			address.OA_PostCode = "botany";
			address.OA_State = "NSW";
			address.OA_RN_NKCountryCode = "AU";

			var expectContent = @"InputAddress1: main address1
InputAddress2: main address2
InputCity: Sydney
InputPostcode: botany
InputState: NSW
InputCountryCode: AU";

			AssertMultilineASCIIEquals(expectContent, OrgAddress.ConvertAddressToAnalysisText(address));
			AssertMultilineASCIIEquals(ZString.Empty, OrgAddress.ConvertAddressToAnalysisText(null));
		}

		public void TestAllowModify()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = org.MainAddress;
			var address = org.Addresses.AddNew();

			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
			Assert(mainAddress.AllowModify);

			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
			Assert(!mainAddress.AllowModify);

			Env.Security.OrgAddressDetailsModify.IsAllowed = true;
			Assert(address.AllowModify);

			Env.Security.OrgAddressDetailsModify.IsAllowed = false;
			Assert(!address.AllowModify);
		}

		public void TestSetDefaultLanguage_MainAddress()
		{
			var country1 = Env.CurrentCompany.Country;
			var country1Language = Constants.Languages.Afrikaans;
			var country2 = Factory.NewWithValidTestData<RefCountry>();
			country2.Code = "88";
			var country3 = Factory.NewWithValidTestData<RefCountry>();
			country3.Code = "77";
			var country3Language = Constants.Languages.Bangla;
			Factory.Save();

			var defaultLanguages = new CountryDefaultLanguageBusinessObjectCollection()
			{
				new CountryDefaultLanguageBusinessObject
				{
					CountryPk = country1.PK,
					DefaultLanguage = country1Language,
				},
				new CountryDefaultLanguageBusinessObject
				{
					CountryPk = country3.PK,
					DefaultLanguage = country3Language,
				}
			};

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = header.MainAddress;
			AssertEquals(Constants.Languages.English, mainAddress.OA_Language);
			AssertEquals(Constants.Languages.English, header.OH_Language);

			using (OrganisationsDataRegistry.Instance.CountryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguages))
			{
				header = Factory.NewWithValidTestData<OrgHeader>();
				mainAddress = header.MainAddress;
				AssertEquals(country1Language, mainAddress.OA_Language);
				AssertEquals(Constants.Languages.English, header.OH_Language);

				mainAddress.OA_RN_NKCountryCode = country2.Code;
				AssertEquals(country1Language, mainAddress.OA_Language);
				AssertEquals(Constants.Languages.English, header.OH_Language);

				mainAddress.OA_RN_NKCountryCode = country3.Code;
				AssertEquals(country3Language, mainAddress.OA_Language);
				AssertEquals(country3Language, header.OH_Language);

				Factory.Save();

				mainAddress.OA_RN_NKCountryCode = country2.Code;
				AssertEquals(country3Language, mainAddress.OA_Language);
				AssertEquals(country3Language, header.OH_Language);
			}
		}

		public void TestSetDefaultLanguage_NotMainAddress()
		{
			var country1 = Env.CurrentCompany.Country;
			var country1Language = Constants.Languages.Afrikaans;
			var country2 = Factory.NewWithValidTestData<RefCountry>();
			country2.Code = "88";
			var country3 = Factory.NewWithValidTestData<RefCountry>();
			country3.Code = "77";
			var country3Language = Constants.Languages.Bangla;
			Factory.Save();

			var defaultLanguages = new CountryDefaultLanguageBusinessObjectCollection()
			{
				new CountryDefaultLanguageBusinessObject
				{
					CountryPk = country1.PK,
					DefaultLanguage = country1Language,
				},
				new CountryDefaultLanguageBusinessObject
				{
					CountryPk = country3.PK,
					DefaultLanguage = country3Language,
				}
			};

			using (OrganisationsDataRegistry.Instance.CountryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguages))
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_Code = "Test";
				address.OA_OH = header.PK;
				AssertEquals(false, address.IsMainAddress);
				AssertEquals(country1Language, address.OA_Language);
				AssertEquals(Constants.Languages.English, header.OH_Language);

				address.OA_RN_NKCountryCode = country2.Code;
				AssertEquals(country1Language, address.OA_Language);
				AssertEquals(Constants.Languages.English, header.OH_Language);

				address.OA_RN_NKCountryCode = country3.Code;
				AssertEquals(country3Language, address.OA_Language);
				AssertEquals(Constants.Languages.English, header.OH_Language);

				Factory.Save();

				address.OA_RN_NKCountryCode = country2.Code;
				AssertEquals(country3Language, address.OA_Language);
				AssertEquals(Constants.Languages.English, header.OH_Language);
			}
		}

		public void TestUnrestrictedAdditionalAddressInformationHasOriginalValue()
		{
			var orgAddress1 = Factory.New<OrgAddress>();
			AssertEquals(string.Empty, orgAddress1.UnrestrictedAdditionalAddressInformation);

			orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "A";
			AssertEquals("A", orgAddress1.UnrestrictedAdditionalAddressInformation);

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_AdditionalAddressInformation = "B";
			AssertEquals("B", orgAddress2.UnrestrictedAdditionalAddressInformation);

			var orgAddress3 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddressAdditionalInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			orgAddressAdditionalInfo1.OAI_OA_Address = orgAddress3.PK;
			orgAddressAdditionalInfo1.OAI_AdditionalInfo = "C";
			orgAddressAdditionalInfo1.OAI_IsPrimary = true;

			AssertEquals("C", orgAddress3.UnrestrictedAdditionalAddressInformation);
		}

		public void TestOnlySetOA_AdditionalAddressInformation_ShouldSynchronize()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "B";
			orgAddress.OA_AdditionalAddressInformation = "A";

			CombineAssertions(() =>
			{
				AssertEquals("A", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("A", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("A", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(2, orgAddress.AdditionalInfos.Count);
			});

			orgAddress.OA_AdditionalAddressInformation = "B";
			CombineAssertions(() =>
			{
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("B", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(2, orgAddress.AdditionalInfos.Count);
			});
		}

		public void TestSetPrimaryOrgAddressAdditionalInfoDetail_ShouldSynchronize()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_AdditionalAddressInformation = "B";
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "A";

			CombineAssertions(() =>
			{
				AssertEquals("A", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("A", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("A", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(2, orgAddress.AdditionalInfos.Count);
			});

			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "B";

			CombineAssertions(() =>
			{
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("B", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(2, orgAddress.AdditionalInfos.Count);
			});
		}

		public void TestSetUnrestrictedAdditionalAddressInformationInfo_ShouldSynchronize()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "B";
			orgAddress.UnrestrictedAdditionalAddressInformation = "A";

			CombineAssertions(() =>
			{
				AssertEquals("A", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("A", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("A", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(2, orgAddress.AdditionalInfos.Count);
			});

			orgAddress.UnrestrictedAdditionalAddressInformation = "B";

			CombineAssertions(() =>
			{
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("B", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(2, orgAddress.AdditionalInfos.Count);
			});
		}

		public void TestSetOA_AdditionalAddressInformation_WhenSetWithSameValue_ShouldNotChange()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddressAdditionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			orgAddressAdditionalInfo.OAI_OA_Address = orgAddress.PK;
			orgAddressAdditionalInfo.OAI_AdditionalInfo = "B";
			orgAddressAdditionalInfo.OAI_IsPrimary = true;
			Factory.Save();
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("B", orgAddressAdditionalInfo.OAI_AdditionalInfo);
				AssertEquals(true, orgAddressAdditionalInfo.OAI_IsPrimary);
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
			});

			orgAddress.OA_AdditionalAddressInformation = "B";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals(1, orgAddress.AdditionalInfos.Count);
				AssertEquals("B", orgAddressAdditionalInfo.OAI_AdditionalInfo);
				AssertEquals(true, orgAddressAdditionalInfo.OAI_IsPrimary);
			});
		}

		public void TestSetOA_AdditionalAddressInformation_WhenSetWithExistingNonPrimaryValue_ShouldReserveNotEmptyOld()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var orgAddressAdditionalInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			orgAddressAdditionalInfo1.OAI_AdditionalInfo = "A";
			orgAddressAdditionalInfo1.OAI_IsPrimary = true;
			orgAddressAdditionalInfo1.OAI_OA_Address = orgAddress.PK;

			var orgAddressAdditionalInfo2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			orgAddressAdditionalInfo2.OAI_OA_Address = orgAddress.PK;
			orgAddressAdditionalInfo2.OAI_AdditionalInfo = "B";
			orgAddressAdditionalInfo2.OAI_IsPrimary = false;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("A", orgAddressAdditionalInfo1.OAI_AdditionalInfo);
				AssertEquals(true, orgAddressAdditionalInfo1.OAI_IsPrimary);
				AssertEquals("B", orgAddressAdditionalInfo2.OAI_AdditionalInfo);
				AssertEquals(false, orgAddressAdditionalInfo2.OAI_IsPrimary);
			});

			orgAddress.OA_AdditionalAddressInformation = "B";
			CombineAssertions(() =>
			{
				AssertEquals(1, orgAddress.PrimaryOrgAddressAdditionalInfoCount);
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("A", orgAddress.AdditionalInfos[0].OAI_AdditionalInfo);
				AssertEquals(false, orgAddress.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals("B", orgAddress.AdditionalInfos[1].OAI_AdditionalInfo);
				AssertEquals(true, orgAddress.AdditionalInfos[1].OAI_IsPrimary);
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
			});

			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "A";
			CombineAssertions(() =>
			{
				AssertEquals(1, orgAddress.PrimaryOrgAddressAdditionalInfoCount);
				AssertEquals("A", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("A", orgAddress.AdditionalInfos[0].OAI_AdditionalInfo);
				AssertEquals(true, orgAddress.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals("B", orgAddress.AdditionalInfos[1].OAI_AdditionalInfo);
				AssertEquals(false, orgAddress.AdditionalInfos[1].OAI_IsPrimary);
				AssertEquals("A", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
			});

			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "C";
			CombineAssertions(() =>
			{
				AssertEquals(1, orgAddress.PrimaryOrgAddressAdditionalInfoCount);
				AssertEquals("C", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("A", orgAddress.AdditionalInfos[0].OAI_AdditionalInfo);
				AssertEquals(false, orgAddress.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals("B", orgAddress.AdditionalInfos[1].OAI_AdditionalInfo);
				AssertEquals(false, orgAddress.AdditionalInfos[1].OAI_IsPrimary);
				AssertEquals(3, orgAddress.AdditionalInfos.Count);
				AssertEquals("C", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
			});

			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = string.Empty;
			orgAddressAdditionalInfo1.OAI_IsPrimary = true;
			orgAddressAdditionalInfo1.OAI_AdditionalInfo = string.Empty;
			orgAddressAdditionalInfo2.OAI_IsPrimary = false;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("", orgAddressAdditionalInfo1.OAI_AdditionalInfo);
				AssertEquals(true, orgAddressAdditionalInfo1.OAI_IsPrimary);
				AssertEquals("B", orgAddressAdditionalInfo2.OAI_AdditionalInfo);
				AssertEquals(false, orgAddressAdditionalInfo2.OAI_IsPrimary);
			});
			orgAddress.OA_AdditionalAddressInformation = "B";
			CombineAssertions(() =>
			{
				AssertEquals(1, orgAddress.PrimaryOrgAddressAdditionalInfoCount);
				AssertEquals("", orgAddress.AdditionalInfos[0].OAI_AdditionalInfo);
				AssertEquals(false, orgAddress.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("B", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("B", orgAddress.AdditionalInfos[1].OAI_AdditionalInfo);
				AssertEquals(true, orgAddress.AdditionalInfos[1].OAI_IsPrimary);
				AssertEquals(2, orgAddress.AdditionalInfos.Count);
			});
		}

		public void TestSetOA_AdditionalAddressInformation_WhenSetWithNonExisting()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var orgAddressAdditionalInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			orgAddressAdditionalInfo1.OAI_AdditionalInfo = "A";
			orgAddressAdditionalInfo1.OAI_IsPrimary = true;
			orgAddressAdditionalInfo1.OAI_OA_Address = orgAddress.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("A", orgAddressAdditionalInfo1.OAI_AdditionalInfo);
				AssertEquals(true, orgAddressAdditionalInfo1.OAI_IsPrimary);
			});

			orgAddress.OA_AdditionalAddressInformation = "B";

			CombineAssertions(() =>
			{
				AssertEquals("A", orgAddress.AdditionalInfos[0].OAI_AdditionalInfo);
				AssertEquals(false, orgAddress.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals("B", orgAddress.AdditionalInfos[1].OAI_AdditionalInfo);
				AssertEquals(true, orgAddress.AdditionalInfos[1].OAI_IsPrimary);
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals(1, orgAddress.PrimaryOrgAddressAdditionalInfoCount);
			});
		}

		public void TestSetOA_AdditionalAddressInformation_WhenOriginalEmpty()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var orgAddressAdditionalInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			orgAddressAdditionalInfo1.OAI_OA_Address = orgAddress.PK;
			orgAddressAdditionalInfo1.OAI_IsPrimary = true;
			orgAddressAdditionalInfo1.OAI_AdditionalInfo = string.Empty;

			orgAddress.OA_AdditionalAddressInformation = "A";
			CombineAssertions(() =>
			{
				AssertEquals("A", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("A", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("A", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(1, orgAddress.PrimaryOrgAddressAdditionalInfoCount);
				AssertEquals(1, orgAddress.AdditionalInfos.Count);
			});

			orgAddress.PrimaryOrgAddressAdditionalInfo.OAI_AdditionalInfo = string.Empty;
			orgAddress.UnrestrictedAdditionalAddressInformation = "B";
			CombineAssertions(() =>
			{
				AssertEquals("B", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("B", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("B", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(1, orgAddress.PrimaryOrgAddressAdditionalInfoCount);
				AssertEquals(1, orgAddress.AdditionalInfos.Count);
			});

			orgAddress.PrimaryOrgAddressAdditionalInfo.OAI_AdditionalInfo = string.Empty;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "C";
			CombineAssertions(() =>
			{
				AssertEquals("C", orgAddress.OA_AdditionalAddressInformation);
				AssertEquals("C", orgAddress.PrimaryOrgAddressAdditionalInfoDetail);
				AssertEquals("C", orgAddress.UnrestrictedAdditionalAddressInformation);
				AssertEquals(1, orgAddress.PrimaryOrgAddressAdditionalInfoCount);
				AssertEquals(1, orgAddress.AdditionalInfos.Count);
			});
		}

		public void TestSetOA_AdditionalAddressInformation_Case4()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_AdditionalAddressInformation = "A";

			var orgAddressAdditionalInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			orgAddressAdditionalInfo1.OAI_AdditionalInfo = "A";
			orgAddressAdditionalInfo1.OAI_IsPrimary = true;
			orgAddressAdditionalInfo1.OAI_OA_Address = orgAddress.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("A", orgAddressAdditionalInfo1.OAI_AdditionalInfo);
				AssertEquals(true, orgAddressAdditionalInfo1.OAI_IsPrimary);
			});

			orgAddress.OA_AdditionalAddressInformation = string.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("A", orgAddressAdditionalInfo1.OAI_AdditionalInfo);
				AssertEquals(false, orgAddressAdditionalInfo1.OAI_IsPrimary);
				AssertEquals(false, orgAddress.AdditionalInfos.Any(a => a.OAI_IsPrimary));
			});
		}

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(OrgAddress)));
		}

		public void TestIsCancelled()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address = org.MainAddress;

			Assert("Precondition", !address.IsCancelled);

			address.IsCancelled = true;
			Assert(address.IsCancelled);

			address.IsCancelled = false;
			Assert(!address.IsCancelled);

			org.IsCancelled = true;
			Assert(address.IsCancelled);
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

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_Code = "YRWHR";
			unloco.RL_PortName = "Whiterun";
			unloco.RL_RW = state1.PK;

			var cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_RN_NKCountry = testCountry.RN_Code;
			cityTown1.R9_InternationalName = "Whiterun";
			cityTown1.R9_RW_NKState = state1.RW_Code;

			var cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown2.R9_RN_NKCountry = testCountry.RN_Code;
			cityTown2.R9_InternationalName = "Riften";
			cityTown2.R9_RW_NKState = state2.RW_Code;

			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_ZoneType = "WRS";
			zone.UNLOCOs.Add(unloco);
			zone.Countries.Add(testCountry);

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = testCountry.RN_Code;
			address.OA_Address1 = "1";
			address.OA_City = cityTown1.R9_InternationalName;
			address.OA_State = state1.RW_Code;
			address.OA_RL_NKRelatedPortCode = unloco.RL_Code;

			AssertEquals("ILocation.Code should be empty. if it is implemented as non-empty then the tax defaulting for org can be impacted, so please consult accounting team", ZString.Empty, ((ILocation)address).Code);
			AssertEquals(unloco, ((ILocation)address).UNLOCO);
			AssertEquals(testCountry, ((ILocation)address).Country);
			AssertEquals(state1, ((ILocation)address).State);
			AssertEquals(cityTown1, ((ILocation)address).CityTown);
			AssertContainsExactElementsInAnyOrder(new RefZoneHeader[] { zone }, ((ILocation)address).Zones);

			address.OA_City = cityTown2.R9_InternationalName;
			AssertNull("Should not match as there is no city for this state", ((ILocation)address).CityTown);

			address.OA_State = ZString.Empty;
			AssertEquals("Should match when state has been removed", cityTown2, ((ILocation)address).CityTown);
		}

		#endregion

		#region Address Validation

		public void TestValidationStatusIsNYVAfterReActivate()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			AssertNotEquals(AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);

			address.OA_IsActive = false;
			address.OA_IsActive = true;

			AssertEquals(AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);
		}

		public void TestValidationStatus_WhenChangingAddressFieldWhileValueIsCna_ShouldKeepItAsCna()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			// Act & Assert.

			address.OA_Address1 = "[_MOCK_ADDRESS_1_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OA_ValidationStatus);

			address.OA_Address2 = "[_MOCK_ADDRESS_2_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OA_ValidationStatus);

			address.OA_City = "[_MOCK_CITY_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OA_ValidationStatus);

			address.OA_PostCode = "0000";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OA_ValidationStatus);

			address.OA_State = "[_MOCK_STATE_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, address.OA_ValidationStatus);

			address.OA_RN_NKCountryCode = "XY";
			AssertEquals(AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);
		}

		public void TestChangingAddressResetsValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
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
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.Address2 = "";

			address.AddressValidationStatusChanged += address_AddressValidationStatusChanged;
			address.OA_ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("It happened", address.OA_Code);
		}

		void address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			((OrgAddress)sender).OA_Code = "It happened";
		}

		public void TestValidationStatus()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = country.Code;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestState_CountryHasRefData_StateMatchesRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = country.Code;
			address.OA_State = "NSW";
			AssertEquals("New South Wales", address.State);

			address.State = "Victoria";
			AssertEquals("VIC", address.OA_State);
		}

		public void TestState_CountryHasRefData_StateDoesNotMatchRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = country.Code;
			address.OA_State = "XYZ";
			AssertEquals("XYZ", address.State);
		}

		public void TestState_CountryHasNoRefData()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "NL"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = country.Code;
			address.OA_State = "XYZ";
			AssertEquals("XYZ", address.State);
		}

		public void TestNeedValidation()
		{
			var factory = new BusinessObjectFactory();

			var australia = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			australia.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var china = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			china.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;

			var address = factory.NewWithValidTestData<OrgAddress>();
			address.Address1 = "A1";
			address.Address2 = "A2";
			address.Postcode = "1234";
			address.City = "Syd";
			address.State = "NSW";
			address.OA_RN_NKCountryCode = "CN";
			Assert(address.NeedValidation);

			address.OA_RN_NKCountryCode = "AU";
			Assert(address.NeedValidation);

			factory.Save();
			Assert(address.IsInDatabase);
			Assert(!address.NeedValidation);

			address.Address1 += "A";
			Assert(address.NeedValidation);

			address.Address2 = "";
			Assert(address.NeedValidation);

			address.Address1 = "";
			Assert(!address.NeedValidation);
		}

		public void TestResetAddressMap()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			AssertAddressMap(address, address.OA_Address1Info);
			AssertAddressMap(address, address.OA_Address2Info);
			AssertAddressMap(address, address.OA_CityInfo);
			AssertAddressMap(address, address.OA_PostCodeInfo);
			AssertAddressMap(address, address.OA_StateInfo);
			AssertAddressMap(address, address.OA_RN_NKCountryCodeInfo);

			address.OA_RN_NKCountryCode = "AU";
			var usPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")).PK.ToGuid();
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForOrgAddress: true));
			AssertAddressMap(address, address.OA_Address1Info);
			AssertAddressMap(address, address.OA_Address2Info);
			AssertAddressMap(address, address.OA_CityInfo);
			AssertAddressMap(address, address.OA_PostCodeInfo);
			AssertAddressMap(address, address.OA_StateInfo);
			AssertAddressMap(address, address.OA_RN_NKCountryCodeInfo);
		}

		void AssertAddressMap(OrgAddress address, ZPropertyInfo propertyInfo)
		{
			address.AddressMap = "ABCDE";
			propertyInfo.Value = (ZString)(propertyInfo.Name == nameof(OrgAddress.OA_RN_NKCountryCode) ? "US" : (ZString)propertyInfo.Value + "1");
			Assert(string.IsNullOrEmpty(address.AddressMap));
		}

		public void TestValidationSection()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			AssertEquals(AddressValidationSection.OrganizationAddress, orgAddress.ValidationSection);

			orgAddress.IsInAdminPanel = true;
			AssertEquals(AddressValidationSection.AdminPanel, orgAddress.ValidationSection);
		}

		public void TestOA_StateMaxLength()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.State = "ABCDEFGHIJKLMNOPQRSTUVWXY";
			AssertEquals("ABCDEFGHIJKLMNOPQRSTUVWXY", address.OA_State);

			address.State = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			AssertEquals(string.Empty, address.OA_State);
		}

		public void TestILocationCompletelyCovers()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "GBBYS";
			var mainOrgAddress = org.MainAddress;
			mainOrgAddress.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			mainOrgAddress.OA_RL_NKRelatedPortCode = "GBBYS";

			var deliveryAddress = org.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			deliveryAddress.OA_RL_NKRelatedPortCode = "GBABB";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultAddressType = AddressType.DLV;
			docAddress.OrganisationPK = org.PK;

			var abbots = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBBYS"));
			var britain = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedKingdom);
			var gbZone = Factory.NewWithValidTestData<RefZoneHeader>();
			gbZone.UNLOCOs.Add(abbots);

			AssertEquals("Same address covers each other.", true, mainOrgAddress.CompletelyCovers(mainOrgAddress));
			AssertEquals("Different addresses don't cover each other.", false, mainOrgAddress.CompletelyCovers(deliveryAddress));
			AssertEquals("Address can't cover UNLOCO it is in.", false, mainOrgAddress.CompletelyCovers(abbots));
			AssertEquals("Address can't cover Country.", false, mainOrgAddress.CompletelyCovers(britain));
			AssertEquals("Address can't cover zone it is in.", false, mainOrgAddress.CompletelyCovers(gbZone));
			AssertEquals("Address can't cover jobDoc.", false, mainOrgAddress.CompletelyCovers(docAddress));
		}

		public void TestAddresseeFallbackLogic()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TESTORG";
			var mainOrgAddress = org.MainAddress;
			mainOrgAddress.OA_CompanyNameOverride = "Override";
			AssertEquals("Override", mainOrgAddress.Addressee);

			mainOrgAddress.OA_CompanyNameOverride = "";
			AssertEquals("TESTORG", mainOrgAddress.Addressee);

			mainOrgAddress.OA_OH = ZGuid.Empty;
			AssertNoExceptionThrown(() => AssertEquals("", mainOrgAddress.Addressee));

			mainOrgAddress.OA_CompanyNameOverride = "Override";
			AssertNoExceptionThrown(() => AssertEquals("Override", mainOrgAddress.Addressee));
		}

		#endregion

		#region OA_ValidationStatus

		public void TestSaveValidationStatusByUser()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var orgAddressInNewFactory = newFactory.Load<OrgAddress>(orgAddress.PK);
			orgAddressInNewFactory.OA_GeoLocation = new ZGeography("POINT(-112.3 40.6)");
			orgAddressInNewFactory.OA_ValidationStatus = AddressValidationStatus.Invalid;

			newFactory.Save();

			try
			{
				orgAddress.OA_GeoLocation = new ZGeography("POINT(-122.3 47.6)");
				orgAddress.OA_ValidationStatus = AddressValidationStatus.ManuallyVerified;
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());
			AssertEquals("Validation status should not be changed", AddressValidationStatus.Invalid, orgAddress.OA_ValidationStatus);
			AssertEquals("GeoLocation should not be changed", new ZGeography("POINT(-112.3 40.6)"), orgAddress.OA_GeoLocation);
		}

		public void TestSaveValidationStatusByBPWhenRunBAV()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			Factory.Save();

			// Mock BAV (Background Address Validation Service) change the validation status
			using (EnvProxy.Instance.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var newFactory = Factory.CreateNewFactory();
				newFactory.RefreshEnabled = false;
				var orgAddressInNewFactory = newFactory.Load<OrgAddress>(orgAddress.PK);
				orgAddressInNewFactory.OA_GeoLocation = new ZGeography("POINT(-112.3 40.6)");
				orgAddressInNewFactory.OA_ValidationStatus = AddressValidationStatus.Invalid;

				newFactory.Save();
			}

			try
			{
				orgAddress.OA_GeoLocation = new ZGeography("POINT(-122.3 47.6)");
				orgAddress.OA_ValidationStatus = AddressValidationStatus.ManuallyVerified;
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());
			AssertEquals("Validation status should be changed", AddressValidationStatus.ManuallyVerified, orgAddress.OA_ValidationStatus);
			AssertEquals("GeoLocation should be changed", new ZGeography("POINT(-122.3 47.6)"), orgAddress.OA_GeoLocation);
		}

		public void TestValidationStatus_WhenSetToManuallyVerifiedFromOtherValue_ShouldStayAsIsUntilAddressIsReloaded()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			Address.OA_Address1 = "42 FOOBAR STREET";
			Address.OA_Address2 = "FUNPLACE";
			Address.OA_PostCode = "0000";
			Address.OA_City = "WHITERUN";
			Address.OA_State = "TAMRIEL";
			Address.OA_RN_NKCountryCode = "ID";

			// Act.

			Address.OA_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			Address.OA_Address1 = "72 O'RIORDAN STREET";
			Address.OA_Address2 = "WISETECH GLOBAL";
			Address.OA_PostCode = "2015";
			Address.OA_City = "ALEXANDRIA";
			Address.OA_State = "NSW";
			Address.OA_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ManuallyVerified, Address.OA_ValidationStatus);
		}

		public void TestValidationStatus_WhenLoadedAsManuallyVerifiedFromDatabase_ShouldResetValueAfterChangingAddressField()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			Address.OA_Address1 = "42 FOOBAR STREET";
			Address.OA_Address2 = "FUNPLACE";
			Address.OA_PostCode = "0000";
			Address.OA_City = "WHITERUN";
			Address.OA_State = "TAMRIEL";
			Address.OA_RN_NKCountryCode = "ID";

			Address.OA_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();

			var reloadedAddress = new BusinessObjectFactory().Load<OrgAddress>(Address.PK);

			// Act.

			reloadedAddress.OA_Address1 = "72 O'RIORDAN STREET";
			reloadedAddress.OA_Address2 = "WISETECH GLOBAL";
			reloadedAddress.OA_PostCode = "2015";
			reloadedAddress.OA_City = "ALEXANDRIA";
			reloadedAddress.OA_State = "NSW";
			reloadedAddress.OA_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ToBeVerified, reloadedAddress.OA_ValidationStatus);
		}

		#endregion

		#region multiple language

		public void TestIsEnglishAddress()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var address1 = org.MainAddress;
			address1.OA_Language = Constants.Languages.English;
			address1.OA_RL_NKRelatedPortCode = "Solar";
			address1.OA_State = "Solar";
			address1.OA_Address1 = "Hydrogen";
			address1.OA_Address2 = "Oxygen";
			address1.OA_City = "Mercury";

			Assert(address1.IsEnglishOnlyOrEmpty);
			address1.OA_Language = Constants.Languages.ChineseSimplified;
			address1.OA_City = "æ°´æ˜Ÿ";
			Assert(!address1.IsEnglishOnlyOrEmpty);
			address1.OA_Language = Constants.Languages.English;
			address1.OA_City = "Mercury";
			factory.Save();

			Assert(address1.IsEnglishOnlyOrEmpty);
			address1.OA_Language = Constants.Languages.ChineseSimplified;
			address1.OA_City = "æ°´æ˜Ÿ";
			Assert(!address1.IsEnglishOnlyOrEmpty);
			Assert(address1.IsOriginalValueEnglishOnly);

			factory.Save();
			address1.OA_Language = Constants.Languages.English;
			address1.OA_City = "Mercury";
			Assert(address1.IsEnglishOnlyOrEmpty);
			Assert(!address1.IsOriginalValueEnglishOnly);
		}

		public void TestAddressLanguagePackAndTranslatedAddress()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			AssertEquals(1, orgAddress.AddressLanguagePack.Count);
			AssertEquals(0, orgAddress.TranslatedAddresses.Count);

			var translatedLanguage1 = orgAddress.AddNewTranslatedAddress();
			var translatedLanguage2 = orgAddress.AddNewTranslatedAddress();

			AssertEquals(3, orgAddress.AddressLanguagePack.Count);
			AssertEquals(2, orgAddress.TranslatedAddresses.Count);
			Assert(orgAddress.AddressLanguagePack.Contains(orgAddress));
			Assert(orgAddress.AddressLanguagePack.Contains(translatedLanguage1));
			Assert(orgAddress.AddressLanguagePack.Contains(translatedLanguage2));
			Assert(orgAddress.TranslatedAddresses.Contains(translatedLanguage1));
			Assert(orgAddress.TranslatedAddresses.Contains(translatedLanguage2));

			orgAddress.DeleteTranslatedAddress(translatedLanguage1);
			AssertEquals(2, orgAddress.AddressLanguagePack.Count);
			AssertEquals(1, orgAddress.TranslatedAddresses.Count);
			Assert(!orgAddress.AddressLanguagePack.Contains(translatedLanguage1));
			Assert(!orgAddress.TranslatedAddresses.Contains(translatedLanguage1));
		}

		public void TestCheckNoDuplicateLocalAddress()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var translatedLanguage1 = orgAddress.AddNewTranslatedAddress();
			var translatedLanguage2 = orgAddress.AddNewTranslatedAddress();

			orgAddress.OA_Language = Constants.Languages.English;
			translatedLanguage1.OTA_Language = Constants.Languages.English;
			AssertHasError(translatedLanguage1.OTA_LanguageInfo, "Address already has another local translation in this language.");
			translatedLanguage1.OTA_Language = Constants.Languages.ChineseSimplified;
			AssertNoError(translatedLanguage1.OTA_LanguageInfo, "Address already has another local translation in this language.");

			translatedLanguage2.OTA_Language = Constants.Languages.ChineseSimplified;
			AssertHasError(translatedLanguage2.OTA_LanguageInfo, "Address already has another local translation in this language.");
			translatedLanguage2.OTA_Language = Constants.Languages.English;
			AssertHasError(translatedLanguage2.OTA_LanguageInfo, "Address already has another local translation in this language.");
			translatedLanguage2.OTA_Language = Constants.Languages.ChineseTraditional;
			AssertNoError(translatedLanguage2.OTA_LanguageInfo, "Address already has another local translation in this language.");

			orgAddress.OA_Language = Constants.Languages.ChineseSimplified;
			AssertHasError(orgAddress.OA_LanguageInfo, "Address already has another local translation in this language.");
			orgAddress.OA_Language = Constants.Languages.English;
			AssertNoError(orgAddress.OA_LanguageInfo, "Address already has another local translation in this language.");
		}

		public void TestLocalAddressDisplayText()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = "MAIN ADDRESS";
			orgAddress.OA_Language = Constants.Languages.ChineseTraditional;
			AssertEquals("Set SelectedTranslatedAddress to self by default", orgAddress, orgAddress.SelectedTranslatedAddress);
			AssertEquals(orgAddress.DisplayText, orgAddress.LocalAddressDisplayText);

			var translatedLanguage1 = orgAddress.AddNewTranslatedAddress();
			var translatedLanguage2 = orgAddress.AddNewTranslatedAddress();
			translatedLanguage1.OTA_Language = Constants.Languages.English;
			translatedLanguage2.OTA_Language = Constants.Languages.French;

			orgAddress.SelectedTranslatedAddress = translatedLanguage1;
			AssertEquals(translatedLanguage1.DisplayText, orgAddress.LocalAddressDisplayText);

			orgAddress.LocalAddressDisplayText = translatedLanguage2.DisplayText;
			AssertEquals(translatedLanguage2, orgAddress.SelectedTranslatedAddress);

			orgAddress.LocalAddressDisplayText = "AAA";
			AssertEquals(translatedLanguage2, orgAddress.SelectedTranslatedAddress);
		}

		public void TestSelectTranlatedAddress()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = "Main Address";
			orgAddress.OA_Language = Constants.Languages.ChineseTraditional;
			AssertEquals("Set SelectedTranslatedAddress to self by default", orgAddress, orgAddress.SelectedTranslatedAddress);
			AssertEquals(orgAddress.DisplayText, orgAddress.LocalAddressDisplayText);

			var translatedLanguage1 = orgAddress.AddNewTranslatedAddress();
			translatedLanguage1.OTA_Language = Constants.Languages.English;

			orgAddress.LocalAddressDisplayText = translatedLanguage1.DisplayText;
			AssertEquals(translatedLanguage1, orgAddress.SelectedTranslatedAddress);

			orgAddress.LocalAddressDisplayText = "MAIN ADDRESS";
			AssertEquals("Should select main address", orgAddress, orgAddress.SelectedTranslatedAddress);
		}

		public void TestGetAddressInSpecificLanguage()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var translatedLanguage1 = orgAddress.AddNewTranslatedAddress();
			var translatedLanguage2 = orgAddress.AddNewTranslatedAddress();
			translatedLanguage1.OTA_Language = Constants.Languages.English;
			translatedLanguage2.OTA_Language = Constants.Languages.French;

			AssertEquals(translatedLanguage1, orgAddress.GetTranslatedAddressInSpecificLanguage(Constants.Languages.English));
			AssertEquals(translatedLanguage2, orgAddress.GetTranslatedAddressInSpecificLanguage(Constants.Languages.French));
		}

		public void TestNotAddDuplicateTranslatedAddresses()
		{
			var orgAddress = Factory.New<OrgAddress>();

			orgAddress.AddNewTranslatedAddress();

			AssertEquals("Should have 2 Addresses", 2, orgAddress.AddressLanguagePack.Count);
		}

		public void TestLanguageForNewTranslatedAddress()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			for (int i = 0; i < 10; i++)
			{
				orgAddress.AddNewTranslatedAddress();
			}
			AssertEquals("Should all have different languages", orgAddress.TranslatedAddresses.Count, orgAddress.TranslatedAddresses.Select(a => a.OTA_Language).Distinct().Count());
		}

		#endregion

		#region Address Validation Event Log

		public void TestAddAddressValidationEventLog()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.ValidationStatus = AddressValidationStatus.Invalid;

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			jobDocAddress.E2_ParentID = shipment.PK;
			jobDocAddress.E2_ParentTableCode = "JS";
			jobDocAddress.E2_AddressType = "CRD";
			Factory.Save();

			var orgHeaderLogs = orgAddress.Header.GetLogs();
			var shipmentLogs = (shipment as BusinessObject).GetLogs();

			AssertMostRecentLog(orgHeaderLogs, 1, "ADD", "Added a record to the system", "", "");
			AssertMostRecentLog(shipmentLogs, 1, "ADD", "Added a record to the system", "", "");

			orgAddress.ValidationStatus = AddressValidationStatus.Verified;
			Factory.Save();
			orgHeaderLogs = orgAddress.Header.GetLogs();
			shipmentLogs = (shipment as BusinessObject).GetLogs();
			AssertMostRecentLog(orgHeaderLogs, 2, "AVS", "Address Validation Status", "|DEP=OrgAddress|STA=VAD|TYP=AVS", "Address Validation Status: Valid, Type: Address Validation Service, Party: Organization Address");
			AssertMostRecentLog(shipmentLogs, 1, "ADD", "Added a record to the system", "", "");

			orgAddress.ValidationStatus = AddressValidationStatus.Invalid;
			Factory.Save();
			orgHeaderLogs = orgAddress.Header.GetLogs();
			shipmentLogs = (shipment as BusinessObject).GetLogs();
			AssertMostRecentLog(orgHeaderLogs, 2, "AVS", "Address Validation Status", "|DEP=OrgAddress|STA=VAD|TYP=AVS", "Address Validation Status: Valid, Type: Address Validation Service, Party: Organization Address");
			AssertMostRecentLog(shipmentLogs, 1, "ADD", "Added a record to the system", "", "");

			orgAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();
			orgHeaderLogs = orgAddress.Header.GetLogs();
			shipmentLogs = (shipment as BusinessObject).GetLogs();
			AssertMostRecentLog(orgHeaderLogs, 3, "AVS", "Address Validation Status", "|DEP=OrgAddress|STA=VAD|TYP=MAN", "Address Validation Status: Valid, Type: Manual Verification, Party: Organization Address");
			AssertMostRecentLog(shipmentLogs, 1, "ADD", "Added a record to the system", "", "");

			void AssertMostRecentLog(Logs logs, int expectedCount, string expectedType, string expectedDescription, string expectedReference, string expectedEventDetail)
			{
				var log = logs.MostRecentLog;
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

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			Address.FillWithValidTestData();

			return Address;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Address;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return MainAddress;
		}

		protected override IEnumerable<string> GetAdditionalIgnoreTablesForFetchHintsCheck()
		{
			var result = base.GetAdditionalIgnoreTablesForFetchHintsCheck().ToList();
			result.Add(OrgHeaderSchema.Constants.TableName);
			return result;
		}

		OrgAddress Address;
		OrgHeader Company;
		OrgAddressDependentCollection Collection;
		OrgAddress MainAddress;

		protected override void SetUp()
		{
			base.SetUp();
			Company = Factory.NewWithValidTestData<OrgHeader>();
			MainAddress = Company.MainAddress;
			Address = Company.Addresses.AddNew();
			Address.OA_Address1 = "Setup Address";
			Collection = Company.Addresses;
		}

		class OrgAddressForPhoneNumberTests : OrgAddress
		{
			public OrgAddressForPhoneNumberTests(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnElementChanged()
			{
				base.OnElementChanged();
				IsRefreshBindingCalled = true;
			}

			public bool IsRefreshBindingCalled { get; set; }
		}

		#endregion

		public void TestAddress1AndAddress2()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			CombineAssertions(() =>
			{
				orgAddress.OA_Address1 = "";
				orgAddress.OA_Address2 = "";
				AssertEquals("When Address 1 and 2 are empty", ZString.Empty, orgAddress.Address1AndAddress2);

				orgAddress.OA_Address1 = "Street";
				AssertEquals("When Address 1 is filled and Address 2 is empty", "Street", orgAddress.Address1AndAddress2);

				orgAddress.OA_Address2 = "123";
				AssertEquals("When Address 1 and 2 are filled", "Street 123", orgAddress.Address1AndAddress2);

				orgAddress.OA_Address1 = "";
				AssertEquals("When Address 1 is empty and Address 2 is filled", "123", orgAddress.Address1AndAddress2);
			});
		}
	}
}
