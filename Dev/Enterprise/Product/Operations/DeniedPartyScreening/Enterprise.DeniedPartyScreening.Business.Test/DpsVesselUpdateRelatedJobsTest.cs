using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.Shared;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	class DpsVesselUpdateRelatedJobsTest : TestCaseWithFactory
	{
		public void GenericTestWhenVesselChangedUpdateJobs(RefVessel vessel, bool vesselToClear, List<RelatedPartyJobTestObject> vesselJobs)
		{
			string endVesselStatus = vesselToClear ? ScreeningStatusesList.Codes.Clear : ScreeningStatusesList.Codes.NotScreened;
			AssertNotEquals(message: $"vessel must not yet be set to {endVesselStatus}", endVesselStatus, vessel.RV_ScreeningStatus);

			vesselJobs = vesselJobs.OrderBy(x => x.ProcessingOrder).ToList();
			foreach (var job in vesselJobs)
			{
				job.SetScreeningStatus(job.StartingStatus);
				Factory.Save();
			}

			vessel.RV_ScreeningStatus = endVesselStatus;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				foreach (var job in vesselJobs)
				{
					AssertEquals(message: $"job ({job.Party.GetType().Name}) starting status is as expected", job.StartingStatus, job.GetScreeningStatus());
				}
				AssertEquals(message: $"vessel set to {endVesselStatus}", endVesselStatus, vessel.RV_ScreeningStatus);
			});

			UpdateRelatedJobs(vessel, GlbCompany.CurrentCompany);

			CombineAssertions("Job updated", () =>
			{
				foreach (var job in vesselJobs)
				{
					job.Party.Reload();
					AssertEquals(message: $"Job ({job.Party.GetType().Name}) end status is {job.ExpectedEndStatus}", job.ExpectedEndStatus, job.GetScreeningStatus());
				}
			});
		}

		int vesselCount = 1;
		protected override void SetUp()
		{
			base.SetUp();
			Factory.Save();
		}

		#region JobShipment
		public void TestCasesWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenOrgHeaderHasClearStatuses()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.PermanentClear })
				{
					BaseTestWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenOrgHeaderHasClearStatuses(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenOrgHeaderHasClearStatuses(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

				var shipment = new JobShipmentTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Release);
				shipment.AddRelatedOrgHeader_OrgAddress_JobDocsAndCartage(relatedStatus);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment };

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenOrgHeader_JobDecHasClearStatus_AndJobDecChangedToREL()
		{
			foreach (string status in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Release })
				{
					BaseTestWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenOrgHeader_JobDecHasAnyClearStatus_AndJobDecChangedToREL(status, relatedStatus);
				}
			}
		}

		void BaseTestWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenOrgHeader_JobDecHasAnyClearStatus_AndJobDecChangedToREL(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

				var shipment = new JobShipmentTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Release);
				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Release);
				jobDec.AddRelatedOrgHeader_JobComInv_JobDec(relatedStatus);
				shipment.AddLinkedJobDec(jobDec.Party as IBaseJobDeclaration);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment, jobDec };

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus}, {relatedStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenRelatedPartiesHaveClearStatusOfAll4Types()
		{
			foreach (string status in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened })
			{
				BaseTestWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenRelatedPartiesHaveClearStatusOfAll4Types(status);
			}
		}

		void BaseTestWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenRelatedPartiesHaveClearStatusOfAll4Types(string jobStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var shipment = new JobShipmentTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Release);
				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Release);
				shipment.AddLinkedJobDec(jobDec.Party as IBaseJobDeclaration);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment };

				shipment.AddRelatedOrgHeader_OrgAddress_JobDocsAndCartage(ScreeningStatusesList.Codes.Clear);
				jobDec.AddRelatedOrgHeader_JobComInv_JobDec(ScreeningStatusesList.Codes.JobCleared);
				jobDec.AddRelatedOrgHeader_JobDec(ScreeningStatusesList.Codes.PermanentClear, false);
				jobDec.AddRelatedOrgHeader_JobDec(ScreeningStatusesList.Codes.Release, true);

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedOrgHeader_JobDecHasUnclearStatus()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Release })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown }) /*ScreeningStatusesList.Codes.Block, , ScreeningStatusesList.Codes.Canceled*/
				{
					BaseTestWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedOrgHeader_JobDecHasUnclearStatus(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedOrgHeader_JobDecHasUnclearStatus(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var shipment = new JobShipmentTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment };

				shipment.AddLinkedJobDec(jobDec.Party as IBaseJobDeclaration);
				jobDec.AddRelatedOrgHeader_JobDec(relatedStatus);

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus}, {relatedStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedOrgHeader_OrgAddress_DocsAndCart_HasUnclearStatus()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Release })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown }) /*ScreeningStatusesList.Codes.Block, , ScreeningStatusesList.Codes.Canceled*/
				{
					BaseTestCasesWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedOrgHeader_OrgAddress_DocsAndCart_HasUnclearStatus(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestCasesWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedOrgHeader_OrgAddress_DocsAndCart_HasUnclearStatus(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var shipment = new JobShipmentTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				shipment.AddRelatedOrgHeader_OrgAddress_JobDocsAndCartage(ScreeningStatusesList.Codes.NotScreened);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment };

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus}, {relatedStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedPartiesHaveClearAndUnclearStatus()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Release })
			{
				BaseWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedPartiesHaveClearAndUnclearStatus(jobStatus, ScreeningStatusesList.Codes.NotScreened);
				BaseWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedPartiesHaveClearAndUnclearStatus(jobStatus, ScreeningStatusesList.Codes.Matched);
			}
		}

		void BaseWhenVesselChangedToCLR_JobShipmentWithNOTorBLKorRELStatusSetToBLKWhenRelatedPartiesHaveClearAndUnclearStatus(string jobStatus, string relatedOrgStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var shipment = new JobShipmentTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				shipment.AddLinkedJobDec(jobDec.Party as IBaseJobDeclaration);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment };

				jobDec.AddRelatedOrgHeader_JobDec(relatedOrgStatus);
				jobDec.AddRelatedOrgHeader_JobComInv_JobDec(ScreeningStatusesList.Codes.PermanentClear);

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({ScreeningStatusesList.Codes.NotScreened}, ({ScreeningStatusesList.Codes.NotScreened}, {ScreeningStatusesList.Codes.PermanentClear}))" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobShipmentWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.JobCleared })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Block })
				{
					BaseTestCaseWhenVesselChangedToCLR_JobShipmentWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestCaseWhenVesselChangedToCLR_JobShipmentWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var shipment = new JobShipmentTestObject(Factory, vessel, jobStatus, jobStatus);
				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, jobStatus);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment };

				shipment.AddLinkedJobDec(jobDec.Party as IBaseJobDeclaration);
				jobDec.AddRelatedOrgHeader_JobDec(relatedStatus);

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (Exception ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus}, {relatedStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToNotCLR_JobShipmentWithNonJCLStatusChangedToBLK()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Block })
			{
				BaseTestWhenVesselChangedToNotCLR_JobShipmentWithNonJCLStatusChangedToBLK(ScreeningStatusesList.Codes.Clear, jobStatus);
				BaseTestWhenVesselChangedToNotCLR_JobShipmentWithNonJCLStatusChangedToBLK(ScreeningStatusesList.Codes.PermanentClear, jobStatus);
			}
		}

		public void BaseTestWhenVesselChangedToNotCLR_JobShipmentWithNonJCLStatusChangedToBLK(string vesselStatus, string jobStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = vesselStatus;

				var shipment = new JobShipmentTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment };

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: false, vesselJobs);
			}
			catch (Exception ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestWhenVesselChangedToNotCLR_JobShipmentWithJCLStatusNotChangedToBLK()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "VESSEL1";
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var shipment = new JobShipmentTestObject(Factory, vessel, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.JobCleared);
			var vesselJobs = new List<RelatedPartyJobTestObject>() { shipment };

			Factory.Save();

			GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: false, vesselJobs);
		}

		#endregion JobShipment

		#region JobDec
		public void TestUpdateRelatedJobsForVessel_ShipmentWithDeclaration()
		{
			var job = (BusinessObject)Factory.New<IForwardingShipment>();
			var declarationConnectedToJob = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			((IBaseJobDeclaration)declarationConnectedToJob).JE_JS = job.PK;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var transport = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ITransport>();
			((Enterprise.Integration.Freight.ITransport)transport).ParentType = job.GetType();
			transport[JobConsolTransportSchema.JW_ParentGUID] = job.PK;
			transport[JobConsolTransportSchema.JW_ParentType] = "SHP";
			transport[JobConsolTransportSchema.JW_Vessel] = vessel.RV_Code;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader[JobHeaderSchema.JH_ParentID] = job.PK;
			jobHeader[JobHeaderSchema.JH_Status] = "WRK";

			Factory.Save();

			var forPartyFactory = new BusinessObjectFactory();
			var partyLoaded = forPartyFactory.Load<RefVessel>(vessel.PK);
			partyLoaded.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			forPartyFactory.Save();

			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, job[JobShipmentSchema.JS_ScreeningStatus].ToString());
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.NotScreened, declarationConnectedToJob[JobDeclarationSchema.JE_ScreeningStatus].ToString());

			AssertEquals(0, UpdateRelatedJobs(vessel, GlbCompany.CurrentCompany));

			var loadFactory = new BusinessObjectFactory();
			var jobLoaded = (BusinessObject)loadFactory.Load<IForwardingShipment>(job.PK);
			var declarationLoaded = (BusinessObject)loadFactory.Load<IBaseJobDeclaration>(declarationConnectedToJob.PK);
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Block, jobLoaded[JobShipmentSchema.JS_ScreeningStatus].ToString());
			AssertEquals("Screening Status", ScreeningStatusesList.Codes.Block, declarationLoaded[JobDeclarationSchema.JE_ScreeningStatus].ToString());
		}

		public void TestCasesWhenVesselChangedToCLR_JobDeclarationWithBLKorNOTStatusChangedToRELWhenOrgHeaderHasClearStatuses()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.PermanentClear })
				{
					BaseTestWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenRelatedPartyHasClearStatuses(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestWhenVesselChangedToCLR_JobShipmentWithBLKorNOTStatusChangedToRELWhenRelatedPartyHasClearStatuses(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Release);
				jobDec.AddRelatedOrgHeader_JobDec(relatedStatus);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { jobDec };

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobDeclarationWithNOTorBLKorRELStatusSetToBLKWhenOrgHeaderHasUnclearStatus()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Release })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown }) /*ScreeningStatusesList.Codes.Block, , ScreeningStatusesList.Codes.Canceled*/
				{
					BaseTestWhenVesselChangedToCLR_JobDeclarationWithNOTorBLKorRELStatusSetToBLKWhenOrgHeaderHasUnclearStatus(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestWhenVesselChangedToCLR_JobDeclarationWithNOTorBLKorRELStatusSetToBLKWhenOrgHeaderHasUnclearStatus(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { jobDec };

				jobDec.AddRelatedOrgHeader_JobDec(relatedStatus);

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus}, {relatedStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobDecWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.JobCleared })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Block })
				{
					BaseTestCaseWhenVesselChangedToCLR_JobDecWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestCaseWhenVesselChangedToCLR_JobDecWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, jobStatus);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { jobDec };

				jobDec.AddRelatedOrgHeader_JobDec(relatedStatus);

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (Exception ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus}, {relatedStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToNotCLR_JobDecWithNonJCLStatusChangedToBLK()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Block })
			{
				BaseTestWhenVesselChangedToNotCLR_JobDecWithNonJCLStatusChangedToBLK(jobStatus);
			}
		}

		public void BaseTestWhenVesselChangedToNotCLR_JobDecWithNonJCLStatusChangedToBLK(string jobStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				var jobDec = new JobDeclarationTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { jobDec };

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: false, vesselJobs);
			}
			catch (Exception ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestWhenVesselChangedToNotCLR_JobDecWithJCLStatusNotChangedToBLK()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "VESSEL1";
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var jobDec = new JobDeclarationTestObject(Factory, vessel, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.JobCleared);
			var vesselJobs = new List<RelatedPartyJobTestObject>() { jobDec };

			Factory.Save();

			GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: false, vesselJobs);
		}
		#endregion JobDec

		#region JobConsol
		public void TestCasesWhenVesselChangedToCLR_JobConsolWithBLKorNOTStatusChangedToRELWhenRelatedShipmentHasClearStatuses()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.PermanentClear })
				{
					BaseTestWhenVesselChangedToCLR_JobConsolWithBLKorNOTStatusChangedToRELWhenRelatedShipmentHasClearStatuses(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestWhenVesselChangedToCLR_JobConsolWithBLKorNOTStatusChangedToRELWhenRelatedShipmentHasClearStatuses(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

				var jobConsol = new JobConsolTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Release);
				jobConsol.AddRelatedShipment(relatedStatus);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { jobConsol };

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobConsolWithNOTorBLKorRELStatusSetToBLKWhenRelatedShipmentHasUnclearStatus()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.Block, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Release })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Block })
				{
					BaseTestWhenVesselChangedToCLR_JobDeclarationWithNOTorBLKorRELStatusSetToBLKWhenRelatedShipmentHasUnclearStatus(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestWhenVesselChangedToCLR_JobDeclarationWithNOTorBLKorRELStatusSetToBLKWhenRelatedShipmentHasUnclearStatus(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var jobCon = new JobConsolTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { jobCon };

				jobCon.AddRelatedShipment(relatedStatus);

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (AssertionFailedError ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus}, {relatedStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToCLR_JobConsolWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.JobCleared })
			{
				foreach (string relatedStatus in new List<string>() { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Block })
				{
					BaseTestCaseWhenVesselChangedToCLR_JobConsolWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus(jobStatus, relatedStatus);
				}
			}
		}

		void BaseTestCaseWhenVesselChangedToCLR_JobConsolWithJCLStatusUnchanged_RegardlessOfRelatedPartyStatus(string jobStatus, string relatedStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;

				var jobCon = new JobConsolTestObject(Factory, vessel, jobStatus, jobStatus);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { jobCon };

				jobCon.AddRelatedShipment(relatedStatus);

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: true, vesselJobs);
			}
			catch (Exception ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus}, {relatedStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestCasesWhenVesselChangedToNotCLR_JobConsolWithNonJCLStatusChangedToBLK()
		{
			foreach (string jobStatus in new List<string>() { ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Release, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Block })
			{
				BaseTestWhenVesselChangedToNotCLR_JobConsolWithNonJCLStatusChangedToBLK(jobStatus);
			}
		}

		public void BaseTestWhenVesselChangedToNotCLR_JobConsolWithNonJCLStatusChangedToBLK(string jobStatus)
		{
			try
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Code = $"VESSEL{vesselCount++}";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				var jobCon = new JobConsolTestObject(Factory, vessel, jobStatus, ScreeningStatusesList.Codes.Block);
				var vesselJobs = new List<RelatedPartyJobTestObject>() { jobCon };

				Factory.Save();

				GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: false, vesselJobs);
			}
			catch (Exception ex)
			{
				throw new AssertionFailedError($"Test Case ({jobStatus})" + System.Environment.NewLine + System.Environment.NewLine + ex.Message, ex);
			}
		}

		public void TestWhenVesselChangedToNotCLR_JobConsolWithJCLStatusNotChangedToBLK()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "VESSEL1";
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var jobCon = new JobConsolTestObject(Factory, vessel, ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.JobCleared);
			var vesselJobs = new List<RelatedPartyJobTestObject>() { jobCon };

			Factory.Save();

			GenericTestWhenVesselChangedUpdateJobs(vessel, vesselToClear: false, vesselJobs);
		}

		#endregion JobConsol

		public IEnumerable<BusinessObject> RelatedPartiesDataSets()
		{
			return null;
		}

		public static int UpdateRelatedJobs(RefVessel vessel, GlbCompany glbCompany)
		{
			DbCommand cmd = Db.Connection.Command("UpdateRelatedJobsForVesselNew");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = OrganisationsDataRegistry.Instance.DeniedPartyScreeningTimeoutForSQLCommandQuery.Value;
			cmd.AddParameter("@vesselPK", SqlDbType.UniqueIdentifier, vessel.PK.ToGuid());
			cmd.AddParameter("@earliestDT", SqlDbType.DateTime, ZDateTime.Today.AddDays(-7).ToDateTime());
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, glbCompany.PK.ToGuid());
			cmd.AddParameter("@jobUpdatePeriod", SqlDbType.DateTime, new DateTime(1900, 01, 01, 00, 00, 00));
			cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());

			return cmd.ExecuteProcedureWithReturnValue();
		}
	}

	public abstract class RelatedPartyTestObject
	{
		protected BusinessObjectFactory Factory;
		public RelatedPartyTestObject(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public abstract string GetScreeningStatus();

		public abstract BusinessObject Party { get; set; }
	}

	public abstract class RelatedPartyJobTestObject : RelatedPartyTestObject
	{
		public abstract void SetScreeningStatus(string status);
		public abstract int ProcessingOrder { get; }

		public Transport LinkingTransport { get; protected set; }
		protected RefVessel Vessel;
		public string StartingStatus;
		public string ExpectedEndStatus;
		public RelatedPartyJobTestObject(BusinessObjectFactory factory, RefVessel vessel, string startingStatus, string expectedEndStatus)
			: base(factory)
		{
			Vessel = vessel;
			StartingStatus = startingStatus;
			ExpectedEndStatus = expectedEndStatus;
		}
	}

	public class JobConsolTestObject : RelatedPartyJobTestObject
	{
		IForwardingConsol jobConsol;
		public JobConsolTestObject(BusinessObjectFactory factory, RefVessel vessel, string startingStatus, string expectedEndStatus)
			: base(factory, vessel, startingStatus, expectedEndStatus)
		{
			LinkingTransport = Factory.New<Transport>();
			ConstructorBase();
		}

		public JobConsolTestObject(BusinessObjectFactory factory, RefVessel vessel, string startingStatus, string expectedEndStatus, Transport linkingTransport)
			 : base(factory, vessel, startingStatus, expectedEndStatus)
		{
			LinkingTransport = linkingTransport;
			ConstructorBase();
		}

		void ConstructorBase()
		{
			if (jobConsol == null)
			{
				jobConsol = Factory.New<IForwardingConsol>();
				jobConsol.JK_ScreeningStatus = StartingStatus;
				LinkingTransport.ParentType = jobConsol.GetType();
				LinkingTransport.JW_ParentType = ((ITransportParent)jobConsol).TypeCode;
				LinkingTransport.JW_ParentGUID = jobConsol.PK;
				LinkingTransport.JW_Vessel = Vessel.RV_Code;
			}
		}

		public ForwardingShipment AddRelatedShipment(string status)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobConLink = Factory.NewWithValidTestData<JobConShipLink>();
			Factory.Save();
			jobConLink.JN_JS = shipment.PK;
			jobConLink.JN_JK = jobConsol.PK;
			shipment.JS_ScreeningStatus = status;
			Factory.Save();
			return shipment;
		}

		public override string GetScreeningStatus()
		{
			return jobConsol.JK_ScreeningStatus;
		}

		public override void SetScreeningStatus(string status)
		{
			jobConsol.JK_ScreeningStatus = status;
		}

		public override BusinessObject Party { get { return (BusinessObject)jobConsol; } set { jobConsol = (IForwardingConsol)value; } }

		public override int ProcessingOrder => 1;
	}

	public class JobShipmentTestObject : RelatedPartyJobTestObject
	{
		ForwardingShipment JobShipment;

		public JobShipmentTestObject(BusinessObjectFactory factory, RefVessel vessel, string startingStatus, string expectedEndStatus)
			: base(factory, vessel, startingStatus, expectedEndStatus)
		{
			LinkingTransport = factory.New<Transport>();
			ConstructorBase();
		}

		public JobShipmentTestObject(BusinessObjectFactory factory, RefVessel vessel, string startingStatus, string expectedEndStatus, IForwardingShipment shipment)
			: base(factory, vessel, startingStatus, expectedEndStatus)
		{
			if (JobShipment == null)
			{
				JobShipment = factory.New<ForwardingShipment>();
				JobShipment.JS_ScreeningStatus = StartingStatus;
				JobShipment.JS_JS_ColoadMasterShipment = shipment.PK;
			}
		}

		public JobShipmentTestObject(BusinessObjectFactory factory, RefVessel vessel, string startingStatus, string expectedEndStatus, Transport linkingTransport)
			: base(factory, vessel, startingStatus, expectedEndStatus)
		{
			LinkingTransport = linkingTransport;
			ConstructorBase();
		}

		void ConstructorBase()
		{
			if (JobShipment == null)
			{
				JobShipment = Factory.New<ForwardingShipment>();
				JobShipment.JS_ScreeningStatus = StartingStatus;
				LinkingTransport.ParentType = JobShipment.GetType();
				LinkingTransport.JW_ParentType = ((ITransportParent)JobShipment).TypeCode;
				LinkingTransport.JW_ParentGUID = JobShipment.PK;
				LinkingTransport.JW_Vessel = Vessel.RV_Code;
			}
		}

		public override string GetScreeningStatus()
		{
			return JobShipment.JS_ScreeningStatus;
		}

		public override void SetScreeningStatus(string status)
		{
			JobShipment.JS_ScreeningStatus = status;
		}

		public override BusinessObject Party { get { return JobShipment; } set { JobShipment = (ForwardingShipment)value; } }
		public override int ProcessingOrder => 2;

		public void AddRelatedOrgHeader_OrgAddress_JobDocsAndCartage(string status, bool pickupOrDelivery = false)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			orgHeader.OH_ScreeningStatus = status;

			var orgAddress = orgHeader.Addresses.First();

			var docAndCartage = JobDocsAndCartage.Load(JobShipment)
				?? JobDocsAndCartage.New(JobShipment);

			if (pickupOrDelivery)
			{
				docAndCartage.JP_OA_DeliveryCartageCoAddr = orgAddress.PK;
			}
			else
			{
				docAndCartage.JP_OA_PickupCartageCoAddr = orgAddress.PK;
			}
			Factory.Save();
		}

		public void AddLinkedJobDec(IBaseJobDeclaration jobDec)
		{
			jobDec.JE_JS = JobShipment.PK;
			Factory.Save();
		}
	}

	public class JobDeclarationTestObject : RelatedPartyJobTestObject
	{
		IBaseJobDeclaration JobDec;

		public JobDeclarationTestObject(BusinessObjectFactory factory, RefVessel vessel, string startingStatus, string expectedEndStatus)
			: base(factory, vessel, startingStatus, expectedEndStatus)
		{
			LinkingTransport = Factory.New<Transport>();
			ConstructorBase();
		}

		public JobDeclarationTestObject(BusinessObjectFactory factory, RefVessel vessel, string startingStatus, string expectedEndStatus, Transport linkingTransport)
			 : base(factory, vessel, startingStatus, expectedEndStatus)
		{
			LinkingTransport = linkingTransport;
			ConstructorBase();
		}

		void ConstructorBase()
		{
			if (JobDec == null)
			{
				JobDec = Factory.New<IBaseJobDeclaration>();
				JobDec.JE_ScreeningStatus = StartingStatus;

				LinkingTransport.ParentType = JobDec.GetType();
				LinkingTransport.JW_ParentType = ((ITransportParent)JobDec).TypeCode;
				LinkingTransport.JW_ParentGUID = JobDec.PK;
				LinkingTransport.JW_Vessel = Vessel.RV_Code;
			}
		}

		public void AddRelatedOrgHeader_JobComInv_JobDec(string status, bool buyerOrSupplier = false)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceHeader = Factory.New<IBaseJobComInvoiceHeader>();
			Factory.Save();

			if (!buyerOrSupplier)
			{
				invoiceHeader.JZ_OH_Buyer = orgHeader.PK;
			}
			else
			{
				invoiceHeader.JZ_OH_Supplier = orgHeader.PK;
			}

			invoiceHeader.JZ_JE = JobDec.PK;
			orgHeader.OH_ScreeningStatus = status;
			Factory.Save();
		}

		public void AddRelatedOrgHeader_JobDec(string status, bool importerOrSupplier = false)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			if (!importerOrSupplier)
			{
				JobDec.JE_OH_Importer = orgHeader.PK;
			}
			else
			{
				JobDec.JE_OH_Supplier = orgHeader.PK;
			}

			orgHeader.OH_ScreeningStatus = status;
			Factory.Save();
		}

		public override string GetScreeningStatus()
		{
			return JobDec.JE_ScreeningStatus;
		}

		public override void SetScreeningStatus(string status)
		{
			JobDec.JE_ScreeningStatus = status;
		}

		public override BusinessObject Party { get { return (BusinessObject)JobDec; } set { JobDec = (IBaseJobDeclaration)value; } }
		public override int ProcessingOrder => 1;
	}
}
