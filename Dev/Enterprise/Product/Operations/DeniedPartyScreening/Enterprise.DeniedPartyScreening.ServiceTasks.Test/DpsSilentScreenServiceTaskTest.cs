using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Business.Test;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

namespace Enterprise.DeniedPartyScreening.ServiceTasks.Test
{
	[TestedType(typeof(DpsSilentScreenServiceTask))]
	public class DpsSilentScreenServiceTaskTest : ServiceTaskTestCase<DpsSilentScreenServiceTask>
	{
		readonly string localhost = string.Format(CultureInfo.InvariantCulture, "http://localhost:{0}/", HttpServiceForTest.GetFreeTcpPort().ToString());

		public void TestHostedServiceAttributes()
		{
			var serviceAttribute = GetHostedServiceAttributes().SingleOrDefault();
			AssertEquals("AllowsMultipleInstances", false, serviceAttribute.AllowsMultipleInstances);
			AssertEquals("IsMandatory", false, serviceAttribute.IsMandatory);
			AssertEquals("MinimumPeriod", "1minute", serviceAttribute.MinimumPeriod);
			AssertEquals("ServiceTaskCode", "DSS", serviceAttribute.Code);
			AssertEquals("Description", "Denied Party Screening Silent Screen Service", serviceAttribute.Description);
			AssertEquals("Category", "DPS", serviceAttribute.Category);
			AssertEquals("Type", typeof(DpsSilentScreenServiceTask), serviceAttribute.Type);
			AssertEquals("CanRunInAnyBranch", true, serviceAttribute.CanRunInAnyBranch);
			AssertEquals("DefaultScheduleRunEvery", "5minutes", serviceAttribute.DefaultScheduleRunEvery);
			Assert("NOT ActiveByDefault", !serviceAttribute.ActiveByDefault);
		}

		public void TestScreenOrganizationWhenGradeIsLow_ExpectedStatusIsCLR()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "SYDORGH";
			organization.OH_FullName = "Test Organization";

			Factory.Save();

			var profileId = Guid.NewGuid();
			var profileId1 = Guid.NewGuid();
			var profileId2 = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var addressId = Guid.NewGuid();
			var profileNameInfo = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Test Organization", Language = "English", IsPrimaryName = true, SourceProfileID =  profileId1 },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 63, SourceProfileID = profileId }
			};
			var profileAddressInfo = new List<ProfileAddressInfo>()
			{
				new ProfileAddressInfo { ID = addressId, Street = "322 OLD HARRISBURG RD", City = "GETTYSBURG", StateProvince = "PA", Country = "US", PostCode = "17323", Language = "English", SourceProfileID = profileId2 },
			};
			var addressMatchInfo = new List<AddressMatchInfo>()
			{
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "3225 OLD HARRISBURG RD", City = "GETTYSBURG", State = "PA", PostCode = "17325", Country = "US" }, MatchingAddressID = addressId, MatchingAddressScore = 63, SourceProfileID = profileId }
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfo, ProfileAddresses = profileAddressInfo, SourceListCodes = new[] { "REFCOM1", "REFCOM2" }, TypeOfEntity = organization.TablePrefix },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, AddressMatches = addressMatchInfo, Profiles = profiles }, 200, true);

			AssertEquals(ScreeningStatusesList.Codes.Clear, organization.OH_ScreeningStatus);
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var result = organization.GetLogs().Find(query).First();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Clear, Old Status: Not Screened, Type: Silent Screen"), result.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=CLR|OLD=NOT|TYP=SIL"), result.SL_Reference);
		}

		public void TestScreenOrganizationWhenScoreGradeIsMedium_ExpectedStatusIsREQ()
		{
			var complist1 = CreateComplianceList("REFCOM1", true);
			var complist2 = CreateComplianceList("REFCOM2", false);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "SYDORGH";
			organization.OH_FullName = "Test Organization";

			Factory.Save();

			var profileId = Guid.NewGuid();
			var profileId1 = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNameInfo = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Test Organization", Language = "English", IsPrimaryName = true, SourceProfileID =  profileId1 },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 70, SourceProfileID = profileId }
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfo, SourceListCodes = new[] { "REFCOM1", "REFCOM2" }, TypeOfEntity = organization.TablePrefix },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			AssertEquals(ScreeningStatusesList.Codes.RequiresReview, organization.OH_ScreeningStatus);
			AssertEdiMessageCreated(organization.PK.ToGuid(), organization.TablePrefix, organization.OH_ScreeningStatus);

			var collection = new StmEntityScreeningLogCollection(organization);
			AssertEquals(1, collection.Count);
			AssertEquals(User.ServiceUserCode, collection[0].PJ_SystemCreateUser.ToString());
			AssertEquals(LogsScreeningStatus.PotentialMatchesFound, collection[0].PJ_Status);
			Assert(collection[0].PJ_ExcludedLists.Contains(complist1.RCL_ListCode));
			Assert(collection[0].PJ_IncludedLists.Contains(complist2.RCL_ListCode));
			AssertEquals(organization.PK, collection[0].PJ_ParentID);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var result = organization.GetLogs().Find(query).First();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Requires Review, Old Status: Not Screened, Score: Medium, Type: Silent Screen"), result.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=REQ|OLD=NOT|SCR=MED|TYP=SIL"), result.SL_Reference);
		}

		public void TestScreenOrganizationWhenScoreGradeIsHigh_ExpectedStatusIsREQ()
		{
			var complist1 = CreateComplianceList("REFCOM1", true);
			var complist2 = CreateComplianceList("REFCOM2", false);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "SYDORGH";
			organization.OH_FullName = "Test Organization";

			Factory.Save();

			var profileId = Guid.NewGuid();
			var profileId1 = Guid.NewGuid();
			var profileId2 = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var addressId = Guid.NewGuid();
			var profileNameInfo = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Test Organization", Language = "English", IsPrimaryName = true, SourceProfileID =  profileId1 },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 90, SourceProfileID = profileId }
			};
			var profileAddressInfo = new List<ProfileAddressInfo>()
			{
				new ProfileAddressInfo { ID = addressId, Street = "322 OLD HARRISBURG RD", City = "GETTYSBURG", StateProvince = "PA", Country = "US", PostCode = "17323", Language = "English", SourceProfileID = profileId2 },
			};
			var addressMatchInfo = new List<AddressMatchInfo>()
			{
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "3225 OLD HARRISBURG RD", City = "GETTYSBURG", State = "PA", PostCode = "17325", Country = "US" }, MatchingAddressID = addressId, MatchingAddressScore = 81, SourceProfileID = profileId }
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfo, ProfileAddresses = profileAddressInfo, SourceListCodes = new[] { "REFCOM1", "REFCOM2" }, TypeOfEntity = organization.TablePrefix },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, AddressMatches = addressMatchInfo, Profiles = profiles }, 200, true);

			AssertEquals(ScreeningStatusesList.Codes.RequiresReview, organization.OH_ScreeningStatus);
			AssertEdiMessageCreated(organization.PK.ToGuid(), organization.TablePrefix, organization.OH_ScreeningStatus);

			var collection = new StmEntityScreeningLogCollection(organization);
			AssertEquals(1, collection.Count);
			AssertEquals(User.ServiceUserCode, collection[0].PJ_SystemCreateUser.ToString());
			AssertEquals(LogsScreeningStatus.PotentialMatchesFound, collection[0].PJ_Status);
			Assert(collection[0].PJ_ExcludedLists.Contains(complist1.RCL_ListCode));
			Assert(collection[0].PJ_IncludedLists.Contains(complist2.RCL_ListCode));
			AssertEquals(organization.PK, collection[0].PJ_ParentID);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var result = organization.GetLogs().Find(query).First();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Requires Review, Old Status: Not Screened, Score: High, Type: Silent Screen"), result.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=REQ|OLD=NOT|SCR=HIG|TYP=SIL"), result.SL_Reference);
		}

		public void TestScreenOrganizationWhenMatchIsExcluded_ExpectedStatusIsCLR()
		{
			var excludedList = CreateComplianceList("REFCOM1", true);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "SYDORGH";
			organization.OH_FullName = "Test Organization";

			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var addressId = Guid.NewGuid();
			var profileNameInfo = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Test Organization", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 90, SourceProfileID = profileId }
			};
			var profileAddressInfo = new List<ProfileAddressInfo>()
			{
				new ProfileAddressInfo { ID = addressId, Street = "322 OLD HARRISBURG RD", City = "GETTYSBURG", StateProvince = "PA", Country = "US", PostCode = "17323", Language = "English", SourceProfileID = Guid.NewGuid() },
			};
			var addressMatchInfo = new List<AddressMatchInfo>()
			{
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "3225 OLD HARRISBURG RD", City = "GETTYSBURG", State = "PA", PostCode = "17325", Country = "US" }, MatchingAddressID = addressId, MatchingAddressScore = 81, SourceProfileID = profileId }
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfo, ProfileAddresses = profileAddressInfo, SourceListCodes = new[] { "REFCOM1" }, TypeOfEntity = organization.TablePrefix },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, AddressMatches = addressMatchInfo, Profiles = profiles }, 200, true);

			AssertEquals(ScreeningStatusesList.Codes.Clear, organization.OH_ScreeningStatus);
			AssertEdiMessageCreated(organization.PK.ToGuid(), organization.TablePrefix, organization.OH_ScreeningStatus);

			var collection = new StmEntityScreeningLogCollection(organization);
			AssertEquals(1, collection.Count);
			AssertEquals(User.ServiceUserCode, collection[0].PJ_SystemCreateUser.ToString());
			AssertEquals(LogsScreeningStatus.ScreenedClear, collection[0].PJ_Status);
			Assert(collection[0].PJ_ExcludedLists.Contains(excludedList.RCL_ListCode));
			AssertEquals(organization.PK, collection[0].PJ_ParentID);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var results = organization.GetLogs().Find(query);
			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Clear, Old Status: Not Screened, Type: Silent Screen"), results.FirstOrDefault().DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=CLR|OLD=NOT|TYP=SIL"), results.FirstOrDefault().SL_Reference);
		}

		public void TestScreenOrganizationExcludeUnmatchedOrg()
		{
			CreateComplianceList("REFCOM1", true);
			var organization = OrgHeader.UnmatchOrg(Factory);
			organization.OH_ScreeningStatus = "NOT";
			organization.OH_IsActive = true;

			Factory.Save();

			var profileID = Guid.NewGuid();
			var profileID1 = Guid.NewGuid();
			var profileID2 = Guid.NewGuid();
			var nameID = Guid.NewGuid();
			var addressID = Guid.NewGuid();
			var profileNameInfo = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameID, FullName = "Test Organization", Language = "English", IsPrimaryName = true, SourceProfileID = profileID1 },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameID, MatchingNameScore = 90, SourceProfileID = profileID }
			};
			var profileAddressInfo = new List<ProfileAddressInfo>()
			{
				new ProfileAddressInfo { ID = addressID, Street = "322 OLD HARRISBURG RD", City = "GETTYSBURG", StateProvince = "PA", Country = "US", PostCode = "17323", Language = "English", SourceProfileID = profileID2 },
			};
			var addressMatchInfo = new List<AddressMatchInfo>()
			{
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "3225 OLD HARRISBURG RD", City = "GETTYSBURG", State = "PA", PostCode = "17325", Country = "US" }, MatchingAddressID = addressID, MatchingAddressScore = 81, SourceProfileID = profileID }
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileID, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfo, ProfileAddresses = profileAddressInfo, SourceListCodes = new[] { "REFCOM1" }, TypeOfEntity = organization.TablePrefix },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, AddressMatches = addressMatchInfo, Profiles = profiles }, 200, true, false);

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, organization.OH_ScreeningStatus);
			AssertEquals(0, new StmEntityScreeningLogCollection(organization).Count);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			AssertEquals(false, organization.GetLogs().Find(query).Any());
		}

		public void TestScreenAddressWhenScoreGradeIsHigh_ExpectedScoreIsHighAndStatusIsREQ()
		{
			var complist1 = CreateComplianceList("REFCOM1", true);
			var complist2 = CreateComplianceList("REFCOM2", false);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "SYDORGH";
			organization.OH_FullName = "Test Organization";

			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var addressId = Guid.NewGuid();
			var profileNameInfo = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Test Full Name 1", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name 3" }, MatchingNameID = nameId, MatchingNameScore = 10, SourceProfileID = profileId },
			};
			var profileAddressInfo = new List<ProfileAddressInfo>()
			{
				new ProfileAddressInfo { ID = addressId, Street = "322 OLD HARRISBURG RD", City = "GETTYSBURG", StateProvince = "PA", Country = "US", PostCode = "17323", Language = "English", SourceProfileID = Guid.NewGuid() },
			};
			var addressMatchInfo = new List<AddressMatchInfo>()
			{
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "3225 OLD HARRISBURG RD", City = "GETTYSBURG", State = "PA", PostCode = "17325", Country = "US" }, MatchingAddressID = addressId, MatchingAddressScore = 88, SourceProfileID = profileId }
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfo, ProfileAddresses = profileAddressInfo, SourceListCodes = new[] { "REFCOM1", "REFCOM2" }, TypeOfEntity = organization.TablePrefix },
			};

			var log = RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, AddressMatches = addressMatchInfo, Profiles = profiles }, 200, true);

			AssertEquals(ScreeningStatusesList.Codes.RequiresReview, organization.OH_ScreeningStatus);
			AssertEdiMessageCreated(organization.PK.ToGuid(), organization.TablePrefix, organization.OH_ScreeningStatus);

			var collection = new StmEntityScreeningLogCollection(organization);
			AssertEquals(1, collection.Count);
			AssertEquals(User.ServiceUserCode, collection[0].PJ_SystemCreateUser.ToString());
			AssertEquals(LogsScreeningStatus.PotentialMatchesFound, collection[0].PJ_Status);
			Assert(collection[0].PJ_ExcludedLists.Contains(complist1.RCL_ListCode));
			Assert(collection[0].PJ_IncludedLists.Contains(complist2.RCL_ListCode));
			AssertEquals(organization.PK, collection[0].PJ_ParentID);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var result = organization.GetLogs().Find(query).First();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Requires Review, Old Status: Not Screened, Score: High, Type: Silent Screen"), result.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=REQ|OLD=NOT|SCR=HIG|TYP=SIL"), result.SL_Reference);
			AssertEquals($@"Information|Denied Party Silent Screening batch has started, found 1 record(s).
Debug|OrgHeader: {organization.OH_Code} silently screened|New Status is: REQ.|Response Code: Successful|Response Message: 
Debug|OrgHeader: {organization.OH_Code} performed update related jobs has finished.
Information|Denied Party Silent Screening batch has finished.
", log);
		}

		public void TestScreenVesselExpectedStatusIsCLR()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();

			var log = RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
			//AssertEdiMessageCreated(vessel.PK.ToGuid(), vessel.TablePrefix, vessel.RV_ScreeningStatus);

			var collection = new StmEntityScreeningLogCollection(vessel);
			AssertEquals(1, collection.Count);
			AssertEquals(User.ServiceUserCode, collection[0].PJ_SystemCreateUser.ToString());
			AssertEquals(LogsScreeningStatus.ScreenedClear, collection[0].PJ_Status);
			AssertEquals(vessel.PK, collection[0].PJ_ParentID);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var result = vessel.GetLogs().Find(query).First();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Clear, Old Status: Not Screened, Type: Silent Screen"), result.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=CLR|OLD=NOT|TYP=SIL"), result.SL_Reference);
			AssertEquals($@"Information|Denied Party Silent Screening batch has started, found 1 record(s).
Debug|RefVessel: {vessel.RV_Code} silently screened|New Status is: CLR.|Response Code: Successful|Response Message: 
Debug|RefVessel: {vessel.RV_Code} performed update related jobs has finished.
Information|Denied Party Silent Screening batch has finished.
", log);
		}

		public void TestProcessEntity_WithLastLogStatusDPI_ShouldNotSkipOrganizationAndStatusIsCLR()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCEDW";
			org.OH_FullName = "Intel Corp";
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var dateUTC = ZDateTime.UtcNow;
			var orgPartyStatus1 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgPartyStatus1.PJ_SystemCreateUser = "~BP";
			orgPartyStatus1.PJ_Status = "DPP";
			orgPartyStatus1.PJ_ParentTableCode = "OH";
			orgPartyStatus1.PJ_ParentID = org.PK;
			orgPartyStatus1.PJ_SystemCreateTimeUtc = dateUTC;

			var orgPartyStatus2 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgPartyStatus2.PJ_SystemCreateUser = "CW1";
			orgPartyStatus2.PJ_Status = "DPI";
			orgPartyStatus2.PJ_ParentTableCode = "OH";
			orgPartyStatus2.PJ_ParentID = org.PK;
			orgPartyStatus2.PJ_SystemCreateTimeUtc = dateUTC.AddMilliseconds(20);

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, org.PK));
			AssertEquals("CLR", org.OH_ScreeningStatus);
			AssertEquals("DPC", logStatus.LastOrDefault()?.PJ_Status);
		}

		public void TestProcessEntity_WithLastLogStatusDPI_ShouldNotSkipVesselAndStatusIsCLR()
		{
			var ves = Factory.NewWithValidTestData<RefVessel>();
			ves.RV_Code = "MAERSK";
			ves.RV_Name = "Maersk Line";
			ves.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var screenDateUtc = ZDateTime.UtcNow;
			var vesLogStatus = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			vesLogStatus.PJ_SystemCreateUser = "~BP";
			vesLogStatus.PJ_Status = "DPP";
			vesLogStatus.PJ_ParentTableCode = "RV";
			vesLogStatus.PJ_ParentID = ves.PK;
			vesLogStatus.PJ_SystemCreateTimeUtc = screenDateUtc;

			var vesLogStatus1 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			vesLogStatus1.PJ_SystemCreateUser = "CW1";
			vesLogStatus1.PJ_Status = "DPI";
			vesLogStatus1.PJ_ParentTableCode = "RV";
			vesLogStatus1.PJ_ParentID = ves.PK;
			vesLogStatus1.PJ_SystemCreateTimeUtc = screenDateUtc.AddMilliseconds(20);

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, ves.PK));
			AssertEquals("CLR", ves.RV_ScreeningStatus);
			AssertEquals("DPC", logStatus.LastOrDefault()?.PJ_Status);
		}

		public void TestProcessEntity_WithStatusUnknownIsInactive_ShouldSkipOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			org.OH_IsActive = false;

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true, false);

			AssertEquals("UNK", org.OH_ScreeningStatus);
		}

		public void TestProcessEntity_WithStatusUnknownAndInactive_ShouldSkipVessel()
		{
			var ves = Factory.NewWithValidTestData<RefVessel>();
			ves.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			ves.RV_IsActive = false;

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true, false);

			AssertEquals("UNK", ves.RV_ScreeningStatus);
		}

		public void TestProcessEntity_WithStatusNotScreenedIsInactive_ShouldSkipOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			org.OH_IsActive = false;

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true, false);

			AssertEquals("NOT", org.OH_ScreeningStatus);
		}

		public void TestProcessEntity_WithStatusUnknownIsActive_ShouldScreenOrganizationAndStatusIsCLR()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var orgPartyStatus2 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgPartyStatus2.PJ_SystemCreateUser = "CW1";
			orgPartyStatus2.PJ_Status = "DPI";
			orgPartyStatus2.PJ_ParentTableCode = "OH";
			orgPartyStatus2.PJ_ParentID = org.PK;

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, org.PK));
			AssertEquals("CLR", org.OH_ScreeningStatus);
			AssertEquals("DPC", logStatus.LastOrDefault().PJ_Status);
		}

		public void TestProcessEntity_WithStatusNotScreenedIsActive_ShouldScreenOrganizationAndStatusIsCLR()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, org.PK));
			AssertEquals("CLR", org.OH_ScreeningStatus);
			AssertEquals("DPC", logStatus.FirstOrDefault().PJ_Status);
		}

		public void TestProcessEntity_WithStatusUnknownAndIsActive_ShouldScreenVesselAndStatusIsCLR()
		{
			var ves = Factory.NewWithValidTestData<RefVessel>();
			ves.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var vesLogStatus = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			vesLogStatus.PJ_SystemCreateUser = "CW1";
			vesLogStatus.PJ_Status = "DPI";
			vesLogStatus.PJ_ParentTableCode = "RV";
			vesLogStatus.PJ_ParentID = ves.PK;

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, ves.PK));
			AssertEquals("CLR", ves.RV_ScreeningStatus);
			AssertEquals("DPC", logStatus.LastOrDefault().PJ_Status);
		}

		public void TestProcessEntity_WithStatusNotScreenedAndIsActive_ShouldScreenVesselAndStatusIsCLR()
		{
			var ves = Factory.NewWithValidTestData<RefVessel>();
			ves.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			Factory.Save();

			RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, ves.PK));
			AssertEquals("CLR", ves.RV_ScreeningStatus);
			AssertEquals("DPC", logStatus.FirstOrDefault().PJ_Status);
		}

		public void TestProcessEntity_WithStatusNotScreenedAndRiskMatched_ShouldScreenVesselAndStatusIsREQ()
		{
			var ves = Factory.NewWithValidTestData<RefVessel>();
			ves.RV_Code = "MAERSK";
			ves.RV_Name = "Maersk Line";
			ves.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNames = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Maersk Line Ltd", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = ves.TablePrefix, FullName = ves.RV_Name }, MatchingNameID = nameId, MatchingNameScore = 89, SourceProfileID = profileId },
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNames, SourceListCodes = new[] { "Source List" }, TypeOfEntity = "VES" },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, ves.PK));
			AssertEquals("REQ", ves.RV_ScreeningStatus);
			AssertEquals("DPP", logStatus.FirstOrDefault().PJ_Status);
		}

		public void TestProcessEntity_WithStatusUnknownAndRiskMatched_ShouldScreenVesselAndStatusIsREQ()
		{
			var ves = Factory.NewWithValidTestData<RefVessel>();
			ves.RV_Code = "MAERSK";
			ves.RV_Name = "Maersk Line";
			ves.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var vesLogStatus = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			vesLogStatus.PJ_SystemCreateUser = "CW1";
			vesLogStatus.PJ_Status = "DPI";
			vesLogStatus.PJ_ParentTableCode = "RV";
			vesLogStatus.PJ_ParentID = ves.PK;

			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNames = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Maersk Line Ltd", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = ves.TablePrefix, FullName = ves.RV_Name }, MatchingNameID = nameId, MatchingNameScore = 89, SourceProfileID = profileId },
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNames, SourceListCodes = new[] { "Source List" }, TypeOfEntity = "VES" },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, ves.PK));
			AssertEquals("REQ", ves.RV_ScreeningStatus);
			AssertEquals("DPP", logStatus.Last().PJ_Status);
		}

		public void TestProcessEntity_WithStatusNotScreenedAndRiskMatched_ShouldScreenOrganizationAndStatusIsREQ()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "INTCORP";
			org.OH_FullName = "Intel Corp";
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNames = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Intel Corp Ltd", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = org.TablePrefix, FullName = org.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 89, SourceProfileID = profileId },
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNames, SourceListCodes = new[] { "Source List" }, TypeOfEntity = "ORG" },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, org.PK));
			AssertEquals("REQ", org.OH_ScreeningStatus);
			AssertEquals("DPP", logStatus.First().PJ_Status);
		}

		public void TestProcessEntity_WithStatusUnknownAndRiskMatched_ShouldScreenOrganizationAndStatusIsREQ()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "INTCORP";
			org.OH_FullName = "Intel Corp";
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var orgPartyStatus2 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgPartyStatus2.PJ_SystemCreateUser = "CW1";
			orgPartyStatus2.PJ_Status = "DPI";
			orgPartyStatus2.PJ_ParentTableCode = "OH";
			orgPartyStatus2.PJ_ParentID = org.PK;

			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNames = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Intel Corp Ltd", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = org.TablePrefix, FullName = org.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 89, SourceProfileID = profileId },
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNames, SourceListCodes = new[] { "Source List" }, TypeOfEntity = "ORG" },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, org.PK));
			AssertEquals("REQ", org.OH_ScreeningStatus);
			AssertEquals("DPP", logStatus.LastOrDefault()?.PJ_Status);
		}

		public void TestProcessEntity_WithComplianceIsNotExcludedAndHighRiskMatched_ShouldCreateEventScoreIsHighAndStatusIsREQ()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "INTCORP";
			organization.OH_FullName = "Intel Corp";

			var includeList = CreateComplianceList("INCCOMPLST", false);
			var excludeList = CreateComplianceList("EXLCOMPLST", true);

			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNames = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Intel Corp Ltd", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 95, SourceProfileID = profileId },
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNames, SourceListCodes = new[] { "INCCOMPLST", "EXLCOMPLST" }, TypeOfEntity = "ORG" },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, organization.PK)).First();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var result = organization.GetLogs().Find(query).First();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Requires Review, Old Status: Not Screened, Score: High, Type: Silent Screen"), result.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=REQ|OLD=NOT|SCR=HIG|TYP=SIL"), result.SL_Reference);
			Assert(logStatus.PJ_IncludedLists.Contains(includeList.RCL_ListCode));
			Assert(logStatus.PJ_ExcludedLists.Contains(excludeList.RCL_ListCode));
			AssertEquals("DPP", logStatus.PJ_Status);
			AssertEdiMessageCreated(organization.PK.ToGuid(), organization.TablePrefix, organization.OH_ScreeningStatus);
			AssertEquals("REQ", organization.OH_ScreeningStatus);
		}

		public void TestProcessEntity_WithComplianceIsNotExcludedAndHighRiskMatched_ShouldCreateEventScoreIsMediumAndStatusIsREQ()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "INTCORP";
			organization.OH_FullName = "Intel Corp";
			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var includeList = CreateComplianceList("INCCOMPLST", false);
			var excludeList = CreateComplianceList("EXLCOMPLST", true);

			var orgPartyStatus2 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgPartyStatus2.PJ_SystemCreateUser = "CW1";
			orgPartyStatus2.PJ_Status = "DPI";
			orgPartyStatus2.PJ_ParentTableCode = "OH";
			orgPartyStatus2.PJ_ParentID = organization.PK;

			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNames = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Intel Corp Ltd", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 70, SourceProfileID = profileId },
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNames, SourceListCodes = new[] { "INCCOMPLST", "EXLCOMPLST" }, TypeOfEntity = "ORG" },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, organization.PK)).Last();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var result = organization.GetLogs().Find(query).First();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Requires Review, Old Status: Unknown, Score: Medium, Type: Silent Screen"), result.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=REQ|OLD=UNK|SCR=MED|TYP=SIL"), result.SL_Reference);
			Assert(logStatus.PJ_IncludedLists.Contains(includeList.RCL_ListCode));
			Assert(logStatus.PJ_ExcludedLists.Contains(excludeList.RCL_ListCode));
			AssertEquals("DPP", logStatus.PJ_Status);
			AssertEdiMessageCreated(organization.PK.ToGuid(), organization.TablePrefix, organization.OH_ScreeningStatus);
			AssertEquals("REQ", organization.OH_ScreeningStatus);
		}

		public void TestProcessEntity_WithComplianceIsExcluded_ShouldNotCreateEventScoreAndStatusIsCLR()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "INTCORP";
			organization.OH_FullName = "Intel Corp";
			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var excludedList = CreateComplianceList("EXLCOMPLST", true);
			var orgPartyStatus2 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgPartyStatus2.PJ_SystemCreateUser = "CW1";
			orgPartyStatus2.PJ_Status = "DPI";
			orgPartyStatus2.PJ_ParentTableCode = "OH";
			orgPartyStatus2.PJ_ParentID = organization.PK;
			Factory.Save();

			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNames = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Intel Corp Ltd", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 90, SourceProfileID = profileId },
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNames, SourceListCodes = new[] { "EXLCOMPLST" }, TypeOfEntity = "ORG" },
			};

			RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, organization.PK)).Last();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var result = organization.GetLogs().Find(query).First();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Clear, Old Status: Unknown, Type: Silent Screen"), result.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=CLR|OLD=UNK|TYP=SIL"), result.SL_Reference);
			Assert(logStatus.PJ_ExcludedLists.Contains(excludedList.RCL_ListCode));
			AssertEquals("DPC", logStatus.PJ_Status);
			AssertEdiMessageCreated(organization.PK.ToGuid(), organization.TablePrefix, "CLR");
			AssertEquals("CLR", organization.OH_ScreeningStatus);
		}

		public void TestProcessEntity_OrganizationWithSanctionedCountry_WithStatusNotScreenedAndIsActive_ExpectedStatusIsREQ()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			orgAddress.OA_RN_NKCountryCode = "IR";
			orgAddress.OA_Code = "Test Sanctioned Country 2";

			Factory.Save();

			RunSilentScreenServiceTask(GetDpsResponseWithCountryMatched(), status: 200, shouldSetServiceLogger: true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, org.PK)).Last();

			Assert(org.OH_ScreeningStatus.Equals("REQ"));
			AssertEquals("DPP", logStatus.PJ_Status);
		}

		public void TestProcessEntity_OrganizationWithSanctionedCountry_WithLastLogStatusDPI_ExpectedStatusIsREQ()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			orgAddress.OA_RN_NKCountryCode = "IR";
			orgAddress.OA_Code = "Test Sanctioned Country 1";

			SetupLogStatus(org.PK, parentTableCode: "OH", status: "DPI");

			Factory.Save();

			RunSilentScreenServiceTask(GetDpsResponseWithCountryMatched(), status: 200, shouldSetServiceLogger: true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, org.PK)).Last();

			Assert(org.OH_ScreeningStatus.Equals("REQ"));
			AssertEquals("DPP", logStatus.PJ_Status);
		}

		public void TestProcessEntity_VesselWithSanctionedCountry_WithStatusNotScreenedAndIsActive_ExpectedStatusIsREQ()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			vessel.RV_RN_NKCountryOfReg = "IR";

			Factory.Save();

			RunSilentScreenServiceTask(GetDpsResponseWithCountryMatched(), status: 200, shouldSetServiceLogger: true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, vessel.PK)).Last();

			Assert(vessel.RV_ScreeningStatus.Equals("REQ"));
			AssertEquals("DPP", logStatus.PJ_Status);
		}

		public void TestProcessEntity_VesselWithSanctionedCountry_WithLastLogStatusDPI_ExpectedStatusIsREQ()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			vessel.RV_RN_NKCountryOfReg = "IR";

			SetupLogStatus(vessel.PK, parentTableCode: "RV", status: "DPI");

			Factory.Save();

			RunSilentScreenServiceTask(GetDpsResponseWithCountryMatched(), status: 200, shouldSetServiceLogger: true);

			var logStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, vessel.PK)).Last();

			Assert(vessel.RV_ScreeningStatus.Equals("REQ"));
			AssertEquals("DPP", logStatus.PJ_Status);
		}

		public void TestHandleSavingException()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "INTCORP";
			org.OH_FullName = "Intel Corp";
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			Factory.Save();

			var sql = "SELECT COUNT(1) FROM dbo.OrgHeader";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			var dataRow = dataTable.Rows[0];

			var zSaveConcurrencyException = new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("ZSaveConcurrencyException"), dataRow, Db.Connection), Factory);
			AssertHandleSavingException(org, zSaveConcurrencyException, "Error|Error occurred when processing silent screening|CargoWise.EntityFramework.ZSaveConcurrencyException");

			var zCannotSaveException = new ZCannotSaveException("ZCannotSaveException", "");
			AssertHandleSavingException(org, zCannotSaveException, "Error|Error occurred when processing silent screening|CargoWise.EntityFramework.ZCannotSaveException");

			var zSaveException = new ZSaveException(new ZDataException(new Exception(""), dataRow, Db.Connection), Factory);
			AssertHandleSavingException(org, zSaveException, "Error|Error occurred when processing silent screening|CargoWise.EntityFramework.ZSaveException");
		}

		void AssertHandleSavingException(OrgHeader org, Exception exception, string expectedMessage)
		{
			var profileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();
			var profileNames = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId, FullName = "Intel Corp Ltd", Language = "English", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
			};
			var nameMatchInfo = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = org.TablePrefix, FullName = org.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 89, SourceProfileID = profileId },
			};
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNames, SourceListCodes = new[] { "Source List" }, TypeOfEntity = "ORG" },
			};

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory => throw exception);

			var log = RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

			AssertContains(expectedMessage, log);
		}

		public void TestHandleWebException()
		{
			Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertHandleWebException(400, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (400) Bad Request.", false);
			AssertHandleWebException(408, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (408) Request Timeout.", false);
			AssertHandleWebException(500, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (500) Internal Server Error.", true);
			AssertHandleWebException(501, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (501) Not Implemented.", false);
#if NET
			AssertHandleWebException(503, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (503) Service Unavailable.", false);
#else
			AssertHandleWebException(503, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (503) Server Unavailable.", false);
#endif

			AssertHandleWebException(504, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (504) Gateway Timeout.", false);
#if NET
			AssertHandleWebException(305, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (305) Use Proxy.", false);
#else
			AssertHandleWebException(305, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (305) Use Proxy Redirect.", false);
#endif
		}

		void AssertHandleWebException(int statusCode, string expectedMessage, bool shouldReportOnce)
		{
			numberOfCalls = 0;
			var log = RunSilentScreenServiceTask(new DpsResponse(), statusCode, true);

			CombineAssertions(() =>
			{
				AssertEquals(3, numberOfCalls);

				var messages = expectedMessage.Split('|');

				AssertEquals(3, messages.Length);
				AssertContains(messages[0] + "|" + messages[1], log);
				AssertContains(messages[2], log);

				if (shouldReportOnce)
				{
					AssertEquals("Web error occurred in DPS Service Task.", ErrorReporter.LastKeyReported);
					AssertEquals(typeof(WebException), ErrorReporter.LastExceptionReported.GetType());
					ErrorReporter.Clear();
				}
				else
				{
					AssertNull(ErrorReporter.LastExceptionReported);
				}
			});
		}

		public void TestRunTask_When_AuthCertNotFoundException_Should_OnlyLogMessageAndNotReport()
		{
			ErrorReporter.Clear();

			Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var serviceLogs = RunSilentScreenServiceTask(new DpsResponse(), 200, true, false);

				AssertContains(new AuthCertNotFoundException().Message, serviceLogs);
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		public void TestScreenVesselAndOrganizationAtTheSameTimeAndStatusIsCLR()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var log = RunSilentScreenServiceTask(new DpsResponse(), 200, true);
			AssertContains("Information|Denied Party Silent Screening batch has started, found 2 record(s).", log);
			AssertContains($"Debug|RefVessel: {vessel.RV_Code} silently screened|New Status is: CLR.", log);
			AssertContains($"Debug|OrgHeader: {header.OH_Code} silently screened|New Status is: CLR.", log);
			AssertContains("Information|Denied Party Silent Screening batch has finished.", log);
		}

		public void TestScreenWithoutSettingServiceLogger()
		{
			Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();

			AssertNoExceptionThrown(() => RunSilentScreenServiceTask(new DpsResponse(), 200, false));
		}

		public void TestSilentScreenWillUpdateRelatedJob()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();
				organization.OH_Code = "TestOrgSSU";

				var job = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				job[JobShipmentSchema.JS_OH_ExportBroker] = organization.PK;
				Factory.Save();

				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.NotScreened, organization.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.NotScreened, job[JobShipmentSchema.JS_ScreeningStatus].ToString());

				var profileId = Guid.NewGuid();
				var nameId = Guid.NewGuid();
				var profileNameInfo = new List<ProfileNameInfo>()
				{
					new ProfileNameInfo { ID = nameId, FullName = "Test Full Name 1", Language = "Chinese", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
				};
				var nameMatchInfo = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = organization.TablePrefix, FullName = organization.OH_FullName }, MatchingNameID = nameId, MatchingNameScore = 70, SourceProfileID = profileId }
				};
				var profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = profileId, ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfo, SourceListCodes = new[] { "REFCOM1" }, TypeOfEntity = organization.TablePrefix },
				};

				var log = RunSilentScreenServiceTask(new DpsResponse { NameMatches = nameMatchInfo, Profiles = profiles }, 200, true);

				var loadFactory = new BusinessObjectFactory();
				var jobLoaded = (BusinessObject)loadFactory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(job.PK);
				AssertEquals(ScreeningStatusesList.Codes.RequiresReview, organization.OH_ScreeningStatus);
				AssertEquals("Should update related job.", ScreeningStatusesList.Codes.RequiresReview, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
				AssertContains($"Debug|OrgHeader: {organization.OH_Code} performed update related jobs has finished.", log);
			}
		}

		public void TestScreenOrganization_CorrectLogs()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var log = RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			AssertContains($"Debug|OrgHeader: {orgHeader.OH_Code} performed update related jobs has finished.", log);
		}

		public void TestScreenVessel_CorrectLogs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();

			var log = RunSilentScreenServiceTask(new DpsResponse(), 200, true);

			AssertContains($"Debug|RefVessel: {vessel.RV_Code} performed update related jobs has finished.", log);
		}

		public void TestRequestHeaderContainsLicenceCode()
		{
			Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();

			var httpService = GetHttpResponseService(new Uri(localhost), JsonConvert.SerializeObject(new DpsResponse()), 200);
			var serviceTask = new DpsSilentScreenServiceTask();

			try
			{
				httpService.Start();
				using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DpsWebServiceUrlTestHelper.SetRegistryUrl(localhost)))
				{
					serviceTask.RunTask(CancellationToken.None);
				}
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			AssertEquals(HMACSHA256Helper.GetComputedLicenceCode(GlbBranch.GetOneActiveBranchPerCompany().First().Company.GetLicenceKeyIdentifier("-")), httpService.Headers["LicenceCode"]);
		}

		public void TestWebServiceWithFailover_ShouldThrowAggregateException()
		{
			var webServices = new DpsWebServiceItemCollection
			{
				new DpsWebServiceItem() { Code = "TST1", WebServiceUrl = "https://dpsv4.wisegrid.net1" , Role = RoleHelper.Code.Production },
				new DpsWebServiceItem() { Code = "TST2", WebServiceUrl = "https://dpsv4-usord.wisegrid.net2", Role = RoleHelper.Code.ProductionFailover },
				new DpsWebServiceItem() { Code = "TST3", WebServiceUrl = "https://dpsv4-test.wisegrid.net2", Role = RoleHelper.Code.Staging },
			};
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, webServices))
			{
				var serviceTask = new DpsSilentScreenServiceTask()
				{
					ServiceLogger = new TestServiceLogger()
				};
				serviceTask.RunTask(CancellationToken.None);

#if NET
				AssertContains("Warning|There was no Http response.|System.Net.WebException: No such host is known. (dpsv4-test.wisegrid.net2:443)", serviceTask.ServiceLogger.ToString());
#else

				AssertContains("Warning|There was no Http response.|System.Net.WebException: The remote name could not be resolved: 'dpsv4-test.wisegrid.net2'", serviceTask.ServiceLogger.ToString());
#endif
				AssertEquals("NOT", organization.OH_ScreeningStatus);
			}
		}

		public void TestWebServiceWithFailover_ShouldBeSuccefulAndStatusIsCLR()
		{
			var webServices = new DpsWebServiceItemCollection
			{
				new DpsWebServiceItem() { Code = "TST1", WebServiceUrl = "https://dpsv4.wisegrid.net1" , Role = RoleHelper.Code.Production },
				new DpsWebServiceItem() { Code = "TST2", WebServiceUrl = "https://dpsv4.wisegrid.net2", Role = RoleHelper.Code.ProductionFailover },
				new DpsWebServiceItem() { Code = "TST3", WebServiceUrl = localhost, Role = RoleHelper.Code.Staging },
			};
			var httpService = GetHttpResponseService(new Uri(localhost), JsonConvert.SerializeObject(new DpsResponse()), 200);
			var serviceTask = new DpsSilentScreenServiceTask();
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			try
			{
				httpService.Start();
				using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, webServices))
				{
					serviceTask.RunTask(CancellationToken.None);

					AssertEquals("CLR", organization.OH_ScreeningStatus);
				}
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}
		}

		public void TestScreenOrganization_WhenScreeningRequestTooLarge_ShouldUpdateScreeningStatusREQ_CreateLogScreeningStatusSPE_CreateTrackingEvent()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Large Name & Address";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			Factory.Save();

			var requestTooLargeMessage = "This request does not appear to represent a single entity. There are a large number of distinct names and addresses, please consider separating these names into different records";
			var logServiceTask = RunSilentScreenServiceTask(new DpsResponse { ResponseCode = DpsResponseCode.Failed, ExtraMessage = requestTooLargeMessage }, 200, true);

			AssertContains("Response Code: Failed", logServiceTask);
			AssertContains("Response Message: " + requestTooLargeMessage, logServiceTask);
			AssertEquals(ScreeningStatusesList.Codes.RequiresReview, header.OH_ScreeningStatus);

			var logEvent = header.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode)).Last();
			AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Requires Review, Old Status: Unknown, Type: Silent Screen"), logEvent.DisplayEventReference);
			AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=REQ|OLD=UNK|TYP=SIL"), logEvent.SL_Reference);

			var logScreeningStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).Last();
			AssertEquals(LogsScreeningStatus.ScreeningProcessError, logScreeningStatus.PJ_Status);
			AssertEquals(requestTooLargeMessage, logScreeningStatus.PJ_ClearedReason);
		}

		public void TestScreenOrganization_WhenReturnDpsResponseCodeException_ShouldNotUpdateScreeningStatus_NotCreateLogScreeningStatusd_NotCreateTrackingEvent()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Large Name & Address";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			Factory.Save();

			var extraMessage = "Dummy Error Occur in the server side.";
			var logServiceTask = RunSilentScreenServiceTask(new DpsResponse { ResponseCode = DpsResponseCode.Exception, ExtraMessage = extraMessage }, 200, true);
			AssertContains("Error|Error occurred when processing silent screening|System.Exception", logServiceTask);
			AssertContains("DPS.Response Code: Exception", logServiceTask);
			AssertContains("DPS.Response Message: " + extraMessage, logServiceTask);
			AssertEquals("Should not update entity screening status", ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);

			var logEvent = header.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode));
			AssertEquals("Should not create tracking event", 0, logEvent.Length);

			var logScreeningStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK));
			AssertEquals("Should not create DPS log screening status", 0, logScreeningStatus.Length);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestScreenOrganizationNameSeparators()
		{ 
			var httpService = GetHttpResponseService(new Uri(localhost), JsonConvert.SerializeObject(new DpsResponse()), 200);
			try
			{
				httpService.Start();
				using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DpsWebServiceUrlTestHelper.SetRegistryUrl(localhost)))
				using (OrganisationsDataRegistry.Instance.NameSeparators.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "C/O" }))
				{
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					orgHeader.OH_FullName = "CHINA AIRLINES C/O CASS";
					Factory.Save();

					var serviceTaskResult = new DpsSilentScreenServiceTaskForTest().GetRequestHeaderAndResultModel(orgHeader);

					AssertEquals(2, serviceTaskResult.RequestHeader.DpsNameCandidates.Count());
					AssertContainsExactElementsInAnyOrder(new[] { "CHINA AIRLINES", "CASS" }, serviceTaskResult.RequestHeader.DpsNameCandidates.Select(u => u.FullName).ToArray());
				}
			}
			finally
			{
				httpService.Stop();
			}
		}

		public void TestRunsInAnyBranch()
		{
			ErrorReporter.Clear();
			Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var serviceTask = new DpsSilentScreenServiceTask();
			var httpService = GetHttpResponseService(new Uri(localhost), null);

			try
			{
				httpService.Start();
				using (Env.Instance.TemporaryServiceTaskContext(DpsSilentScreenServiceTask.Code, canRunInAnyBranch: true))
				{
					serviceTask.RunTask(CancellationToken.None);
					AssertEquals("No Errors", string.Empty, ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		string RunSilentScreenServiceTask(DpsResponse responseTest, int status, bool shouldSetServiceLogger, bool validateAuthorizationHeader = true)
		{
			var httpService = GetHttpResponseService(new Uri(localhost), JsonConvert.SerializeObject(responseTest), status);
			var serviceTask = new DpsSilentScreenServiceTask();

			try
			{
				httpService.Start();
				using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DpsWebServiceUrlTestHelper.SetRegistryUrl(localhost)))
				{
					if (shouldSetServiceLogger)
					{
						serviceTask.ServiceLogger = new TestServiceLogger();
					}

					serviceTask.RunTask(CancellationToken.None);
				}
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			Assert(!httpService.IsStarted);

			if (validateAuthorizationHeader)
			{
				var authorizationHeader = httpService.Headers["Authorization"].Split(' ');
				var authorizationtoken = new AuthenticationHeaderValue(authorizationHeader[0], authorizationHeader[1]);

				AssertEquals("Bearer", authorizationtoken.Scheme);
				AssertNotNullOrEmpty(authorizationtoken.Parameter);
			}

			if (shouldSetServiceLogger)
			{
				return serviceTask.ServiceLogger.ToString();
			}

			return string.Empty;
		}

		int numberOfCalls;

		HttpServiceForTest GetHttpResponseService(Uri serviceUrl, string response, int status = 200)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (uri, request) =>
				{
					numberOfCalls++;
					return new Tuple<int, string>(status, response);
				},
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}

		IDisposable tokenProvider;
		IDisposable mdmSupportCertificateForDPS;

		protected override void SetUpCore()
		{
			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();

			tokenProvider = ObjectFactory.Substitute(nameof(IAuthorizationTokenProvider), new AuthorizationTokenProviderForTest());
			mdmSupportCertificateForDPS = OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo);

			base.SetUpCore();

			Db.Connection.ExecuteNonQuery("Update dbo.OrgHeader set OH_IsActive = 0");
			Db.Connection.ExecuteNonQuery("Update dbo.RefVessel set RV_IsActive = 0");
			Db.Connection.ExecuteNonQuery("Delete dbo.RefComplianceList");
		}

		protected override void TearDownCore()
		{
			tokenProvider?.Dispose();
			mdmSupportCertificateForDPS?.Dispose();

			base.TearDownCore();
		}

		RefComplianceList CreateComplianceList(string code, bool isExcluded)
		{
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList.RCL_ListCode = code;
			complianceList.RCL_ListPublisher = code + "Publisher";
			complianceList.RCL_ListDescription = code + "Description";
			complianceList.RCL_ListType = code + "Type";
			complianceList.RCL_ListName = code;
			complianceList.RCL_IsActive = true;
			complianceList.RCL_IsExcluded = isExcluded;

			return complianceList;
		}

		void AssertEdiMessageCreated(Guid expectedId, string expectedType, string expectedStatus)
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DPSRequestMessage);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.JDC);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ScreeningRequest);

			var ediMessages = Factory.Load<DpsEDIMessage>(filter);
			AssertEquals("One DPS EDI message should have been created", 1, ediMessages.Length);

			var content = JsonConvert.DeserializeObject<DpsEDIMessageCandidatesContent>(ediMessages[0].EM_MessageData.ToUTF8());
			AssertNotNull(content);
			AssertEquals("ID is incorrect for the created EDI message", expectedId, content.ClientSpecifiedIdentifier);
			AssertEquals("Type is incorrect for the created EDI message", expectedType, content.EntityType);
			AssertEquals("Status is incorrect for the created EDI message", expectedStatus, content.PersistentStatus);
		}

		DpsResponse GetDpsResponseWithCountryMatched()
		{
			var matchingCountryId = Guid.NewGuid();
			var sourceProfileId = Guid.NewGuid();

			var countryMatchInfo = new CountryMatchInfo()
			{
				MatchingCountryId = matchingCountryId,
				SourceProfileID = sourceProfileId,
				MatchingCountryCode = "IR",
				MatchingCountryName = "Iran",
				MatchingStandardizedValue = "IR",
				MatchingCountryScore = 100,
				RequestCountry = new DpsCountryCandidate()
				{
					HeaderPk = Guid.Empty,
					CountryCode = "IR",
					StandardizedValue = "IR"
				}
			};

			var profileHeaderInfo = new ProfileHeaderInfo()
			{
				SourceProfileID = sourceProfileId,
				ProfileRisk = "High",
				ProfileNames = new List<ProfileNameInfo>()
				{
					new ProfileNameInfo()
					{
						ID = Guid.NewGuid(),
						FullName = "Iran",
						IsPrimaryName = true,
						Language = string.Empty,
						SourceProfileID = sourceProfileId
					}
				},
				ProfileCountries = new List<ProfileCountryInfo>()
				{
					new ProfileCountryInfo()
					{
						ID = matchingCountryId,
						SourceProfileID = sourceProfileId,
						Code = "IR",
						CountryName = "Iran"
					}
				},
				TypeOfEntity = "COY",
				ProfileNotes = Array.Empty<byte>(),
				SourceListCodes = new[] { "Source List Code" }
			};

			return new DpsResponse()
			{
				ResponseCode = DpsResponseCode.Successful,
				CountryMatches = new List<CountryMatchInfo>()
				{
					countryMatchInfo
				},
				Profiles = new List<ProfileHeaderInfo>()
				{
					profileHeaderInfo
				}
			};
		}

		void SetupLogStatus(ZGuid id, string parentTableCode, string status)
		{
			var dateUtc = ZDateTime.UtcNow;

			var orgPartyStatus1 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgPartyStatus1.PJ_SystemCreateUser = "~BP";
			orgPartyStatus1.PJ_Status = "DPP";
			orgPartyStatus1.PJ_ParentTableCode = parentTableCode;
			orgPartyStatus1.PJ_ParentID = id;
			orgPartyStatus1.PJ_SystemCreateTimeUtc = dateUtc;

			var orgPartyStatus2 = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgPartyStatus2.PJ_SystemCreateUser = "CW1";
			orgPartyStatus2.PJ_Status = status;
			orgPartyStatus2.PJ_ParentTableCode = parentTableCode;
			orgPartyStatus2.PJ_ParentID = id;
			orgPartyStatus2.PJ_SystemCreateTimeUtc = dateUtc.AddMilliseconds(20);

			Factory.Save();
		}

		class DpsSilentScreenServiceTaskForTest : DpsSilentScreenServiceTask
		{
			public DpsSilentScreenServiceTaskForTest() : base()
			{
			}

			public new (DpsRequestHeaderWithAddressMatching RequestHeader, ScreenedPartyModel ResultModel, string EntityCode) GetRequestHeaderAndResultModel<TBizo>(TBizo entity) where TBizo : BusinessObject
			{
				candidateCreator = new DpsCandidateCreator();
				return base.GetRequestHeaderAndResultModel(entity);
			}
		}
	}
}
