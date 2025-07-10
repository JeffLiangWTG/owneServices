using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	class DpsOrgOrDocAddressUpdateRelatedJobsTest : TestCaseWithFactory
	{
		public void TestWhenOrgChangedToCLR_RelatedConsolWithBLKorNOTStatusSetToBLKWhenRelatedShipmentHasNotClearStatuses()
		{
			foreach (var jobStatus in new[] { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened })
			{
				foreach (var relatedStatus in new[] { ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Matched })
				{
					try
					{
						var org = Factory.NewWithValidTestData<OrgHeader>();
						org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

						var jobConsol = new OrgRelatedJobConsolTestObject(Factory, org, jobStatus, ScreeningStatusesList.Codes.Block);
						jobConsol.AddRelatedShipment(relatedStatus);
						Factory.Save();

						GenericTestWhenOrgChangedUpdateJobs(org, true, jobConsol);
					}
					catch (AssertionFailedError ex)
					{
						throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
					}
				}
			}
		}

		public void TestWhenOrgChangedToCLR_RelatedConsolWithBLKorNOTStatusChangedToRELWhenRelatedShipmentHasClearStatuses()
		{
			foreach (var jobStatus in new[] { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened })
			{
				foreach (var relatedStatus in new[] { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.PermanentClear })
				{
					try
					{
						var org = Factory.NewWithValidTestData<OrgHeader>();
						org.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

						var jobConsol = new OrgRelatedJobConsolTestObject(Factory, org, jobStatus, ScreeningStatusesList.Codes.Release);
						jobConsol.AddRelatedShipment(relatedStatus);
						Factory.Save();

						GenericTestWhenOrgChangedUpdateJobs(org, true, jobConsol);
					}
					catch (AssertionFailedError ex)
					{
						throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
					}
				}
			}
		}

		public void TestWhenOrgChangedToNotCLR_RelatedConsolWithNonJCLStatusChangedToBLK()
		{
			foreach (string jobStatus in new[] { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Block })
			{
				try
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

					var jobCon = new OrgRelatedJobConsolTestObject(Factory, org, jobStatus, ScreeningStatusesList.Codes.Block);
					Factory.Save();

					GenericTestWhenOrgChangedUpdateJobs(org, false, jobCon);
				}
				catch (Exception ex)
				{
					throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
				}
			}
		}

		public void TestWhenOrgChangedToNotCLR_JobConsolWithJCLStatusNotChangedToBLK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var jobCon = new OrgRelatedJobConsolTestObject(Factory, org, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.JobCleared);
			Factory.Save();

			GenericTestWhenOrgChangedUpdateJobs(org, false, jobCon);
		}

		public void TestUpdateRelatedJobsForOrg_ShipmentWithDeclarationRegardlessOfOverrideOrNot()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG1";
			orgHeader1.OH_FullName = "Organization For Test 1";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTORG2";
			orgHeader2.OH_FullName = "Organization For Test 2";

			Factory.Save();

			var shipment = Factory.New<IForwardingShipment>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			shipment.JS_OH_ImportBroker = orgHeader2.PK;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			orgHeader1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			orgHeader2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			UpdateRelatedJobs(orgHeader1, GlbCompany.CurrentCompany);

			((BusinessObject)shipment).Reload();
			((BusinessObject)declaration).Reload();
			AssertEquals(ScreeningStatusesList.Codes.Block, shipment.JS_ScreeningStatus);
			AssertEquals("'Override Freight Defaults' declaration has same screening status with shipment", ScreeningStatusesList.Codes.Block, declaration.JE_ScreeningStatus);

			declaration.JE_OverrideFreightDefaults = false;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			Factory.Save();

			UpdateRelatedJobs(orgHeader1, GlbCompany.CurrentCompany);

			((BusinessObject)shipment).Reload();
			((BusinessObject)declaration).Reload();
			AssertEquals(ScreeningStatusesList.Codes.Block, shipment.JS_ScreeningStatus);
			AssertEquals("Not 'Override Freight Defaults' declaration has same screening status with shipment", ScreeningStatusesList.Codes.Block, declaration.JE_ScreeningStatus);
		}

		void GenericTestWhenOrgChangedUpdateJobs(OrgHeader org, bool orgToClear, OrgRelatedJobConsolTestObject orgRelatedConsol)
		{
			var endOrgStatus = orgToClear ? ScreeningStatusesList.Codes.Clear : ScreeningStatusesList.Codes.NotScreened;
			AssertNotEquals($"org must not yet be set to {endOrgStatus}", endOrgStatus, org.OH_ScreeningStatus);

			orgRelatedConsol.SetScreeningStatus(orgRelatedConsol.StartingStatus);
			Factory.Save();

			org.OH_ScreeningStatus = endOrgStatus;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals($"Job Consol's starting status is as expected", orgRelatedConsol.StartingStatus, orgRelatedConsol.GetScreeningStatus());
				AssertEquals($"org set to {endOrgStatus}", endOrgStatus, org.OH_ScreeningStatus);
			});

			UpdateRelatedJobs(org, GlbCompany.CurrentCompany);

			CombineAssertions("Job updated", () =>
			{
				((BusinessObject)orgRelatedConsol.JobConsol).Reload();
				AssertEquals($"Job Consol's end status is {orgRelatedConsol.ExpectedEndStatus}", orgRelatedConsol.ExpectedEndStatus, orgRelatedConsol.GetScreeningStatus());
			});
		}

		int UpdateRelatedJobs(OrgHeader org, GlbCompany glbCompany)
		{
			DbCommand cmd = Db.Connection.Command("UpdateRelatedJobsForOrgOrDocAddressNew");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
			cmd.AddParameter("@earliestDT", SqlDbType.DateTime, ZDateTime.Today.AddDays(-7).ToDateTime());
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, glbCompany.PK.ToGuid());
			cmd.AddParameter("@updateShipments", SqlDbType.Bit, true);
			cmd.AddParameter("@updateConsols", SqlDbType.Bit, true);
			cmd.AddParameter("@jobUpdatePeriod", SqlDbType.DateTime, new DateTime(1900, 01, 01, 00, 00, 00));
			cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());

			return cmd.ExecuteProcedureWithReturnValue();
		}

		public class OrgRelatedJobConsolTestObject
		{
			readonly BusinessObjectFactory factory;
			public string StartingStatus { get; }
			public string ExpectedEndStatus { get; }
			public IForwardingConsol JobConsol { get; set; }

			public OrgRelatedJobConsolTestObject(BusinessObjectFactory factory, OrgHeader org, string startingStatus, string expectedEndStatus)
			{
				this.factory = factory;
				StartingStatus = startingStatus;
				ExpectedEndStatus = expectedEndStatus;

				if (JobConsol == null)
				{
					JobConsol = this.factory.New<IForwardingConsol>();
					JobConsol.JK_ScreeningStatus = StartingStatus;
					JobConsol.JK_OA_CreditorAddress = org.MainAddress.PK;
				}
			}

			public ForwardingShipment AddRelatedShipment(string status)
			{
				var shipment = factory.NewWithValidTestData<ForwardingShipment>();
				var jobConLink = factory.NewWithValidTestData<JobConShipLink>();
				factory.Save();
				jobConLink.JN_JS = shipment.PK;
				jobConLink.JN_JK = JobConsol.PK;
				shipment.JS_ScreeningStatus = status;
				factory.Save();

				return shipment;
			}

			public string GetScreeningStatus()
			{
				return JobConsol.JK_ScreeningStatus;
			}

			public void SetScreeningStatus(string status)
			{
				JobConsol.JK_ScreeningStatus = status;
			}
		}
	}
}
