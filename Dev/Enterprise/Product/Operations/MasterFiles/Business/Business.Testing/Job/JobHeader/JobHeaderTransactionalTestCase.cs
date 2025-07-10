using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobHeaderTransactionalTestCase : TestCaseWithFactory
	{
		public void TestIsReadyForFinancialClosure()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Factory.Save();

			Assert("The job should not be ready for financial closure", !job.IsReadyForFinancialClosure);

			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Assert("The job should be ready for financial closure", job.IsReadyForFinancialClosure);
		}

		public void TestIsReadyForFinancialClosureWithoutModifySecurity()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			var cacheValue = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsReadyForFinancialClosureWithoutModifySecurity should be true", job.IsReadyForFinancialClosureWithoutModifySecurity);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				using (Env.Instance.TemporaryServiceTaskContext("Test", false))
				{
					Assert("IsReadyForFinancialClosureWithoutModifySecurity should be true", job.IsReadyForFinancialClosureWithoutModifySecurity);
				}
				Assert("IsReadyForFinancialClosureWithoutModifySecurity should be false", !job.IsReadyForFinancialClosureWithoutModifySecurity);
			}
		}

		public void TestIsReadyForFinancialClosureWithoutPostSecurity()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			var cacheValue = Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed;

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsReadyForFinancialClosureWithoutPostSecurity should be true", job.IsReadyForFinancialClosureWithoutPostSecurity);
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				using (Env.Instance.TemporaryServiceTaskContext("Test", false))
				{
					Assert("IsReadyForFinancialClosureWithoutPostSecurity should be true", job.IsReadyForFinancialClosureWithoutPostSecurity);
				}
				Assert("IsReadyForFinancialClosureWithoutPostSecurity should be false", !job.IsReadyForFinancialClosureWithoutPostSecurity);
			}
		}

		public void TestConcurrencyPolicyForJH_Status()
		{
			JobHeader jobHeaderOriginal = NewJobHeaderWithSpecificStatusLocalChargesAddrAndUniqueJobInoiceNumber();
			OrgAddress newOrgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress newOrgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			var companyDataToSave = newOrgAddress1.Header.CompanyData;
			companyDataToSave = newOrgAddress2.Header.CompanyData;
			Factory.Save();

			BusinessObjectFactory factoryInSession1 = GetNonUpdatingFactory();
			BusinessObjectFactory factoryInSession2 = GetNonUpdatingFactory();

			JobHeader jobHeaderInSession1 = factoryInSession1.Load<JobHeader>(jobHeaderOriginal.PK);
			JobHeader jobHeaderInSession2 = factoryInSession2.Load<JobHeader>(jobHeaderOriginal.PK);

			AssertConcurrencyHandled((jobHeader => jobHeader.JH_Status = JobHeaderStatus.Closed.Code), (jobHeader => jobHeader.JH_Status = JobHeaderStatus.InvoiceOnHold.Code), jobHeaderInSession1, jobHeaderInSession2);
		}

		public void TestConcurrencyPolicyForJH_OA_LocalChargesAddr()
		{
			JobHeader jobHeaderOriginal = NewJobHeaderWithSpecificStatusLocalChargesAddrAndUniqueJobInoiceNumber();
			OrgAddress newOrgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress newOrgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			var companyDataToSave = newOrgAddress1.Header.CompanyData;
			companyDataToSave = newOrgAddress2.Header.CompanyData;
			Factory.Save();

			BusinessObjectFactory factoryInSession1 = GetNonUpdatingFactory();
			BusinessObjectFactory factoryInSession2 = GetNonUpdatingFactory();

			JobHeader jobHeaderInSession1 = factoryInSession1.Load<JobHeader>(jobHeaderOriginal.PK);
			JobHeader jobHeaderInSession2 = factoryInSession2.Load<JobHeader>(jobHeaderOriginal.PK);

			AssertConcurrencyHandled((jobHeader => jobHeader.JH_OA_LocalChargesAddr = newOrgAddress1.PK), (jobHeader => jobHeader.JH_OA_LocalChargesAddr = newOrgAddress2.PK), jobHeaderInSession1, jobHeaderInSession2);
		}

		public void TestConcurrencyPolicyForJH_UniqueJobInvoiceNumber()
		{
			JobHeader jobHeaderOriginal = NewJobHeaderWithSpecificStatusLocalChargesAddrAndUniqueJobInoiceNumber();
			OrgAddress newOrgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress newOrgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			var companyDataToSave = newOrgAddress1.Header.CompanyData;
			companyDataToSave = newOrgAddress2.Header.CompanyData;
			Factory.Save();

			BusinessObjectFactory factoryInSession1 = GetNonUpdatingFactory();
			BusinessObjectFactory factoryInSession2 = GetNonUpdatingFactory();

			JobHeader jobHeaderInSession1 = factoryInSession1.Load<JobHeader>(jobHeaderOriginal.PK);
			JobHeader jobHeaderInSession2 = factoryInSession2.Load<JobHeader>(jobHeaderOriginal.PK);

			AssertConcurrencyHandled((jobHeader => jobHeader.JH_UniqueJobInvoiceNumber = 2), (jobHeader => jobHeader.JH_UniqueJobInvoiceNumber = 3), jobHeaderInSession1, jobHeaderInSession2);
		}

		public void TestConcurrencyPolicyForLastEditedUserTime()
		{
			JobHeader jobHeaderOriginal = NewJobHeaderWithSpecificStatusLocalChargesAddrAndUniqueJobInoiceNumber();
			GlbStaff newUser1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff newUser2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			BusinessObjectFactory factoryInSession1 = GetNonUpdatingFactory();
			BusinessObjectFactory factoryInSession2 = GetNonUpdatingFactory();

			JobHeader jobHeaderInSession1 = factoryInSession1.Load<JobHeader>(jobHeaderOriginal.PK);
			JobHeader jobHeaderInSession2 = factoryInSession2.Load<JobHeader>(jobHeaderOriginal.PK);

			AssertNoConcurrencyOnLastEditedUser(newUser1, newUser2, jobHeaderInSession1, jobHeaderInSession2);

			jobHeaderInSession1.Reload();
			jobHeaderInSession2.Reload();
			AssertNoConcurrency((jobHeader => jobHeader.JH_SystemLastEditTimeUtc = ZDateTime.Now.AddMinutes(-1)), (jobHeader => jobHeader.JH_SystemLastEditTimeUtc = ZDateTime.Now.AddMinutes(1)), jobHeaderInSession1, jobHeaderInSession2);
		}

		void AssertConcurrencyHandled(UpdateValue updateValueInSession1, UpdateValue updateValueInSession2, JobHeader jobHeaderInSession1, JobHeader jobHeaderInSession2)
		{
			updateValueInSession1(jobHeaderInSession1);
			jobHeaderInSession1.Factory.Save();
			ErrorReporter.Clear();
			try
			{
				updateValueInSession2(jobHeaderInSession2);
				jobHeaderInSession2.Factory.Save();
				Fail("Should not come here because a ZSaveException is expected");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			catch (Exception e)
			{
				Fail("Should be a ZSaveException but was a " + e.GetType().Name + " with message: " + e.Message);
			}
			AssertContains("Must not be mergeable.", "The system cannot automatically merge your changes because there are conflicts with critical fields.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void AssertNoConcurrencyOnLastEditedUser(GlbStaff userInSession1, GlbStaff userInSession2, JobHeader jobHeaderInSession1, JobHeader jobHeaderInSession2)
		{
			jobHeaderInSession1.JH_SystemLastEditUser = userInSession1.GS_Code;
			using (Env.SetTemporaryUserContext(userInSession1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				jobHeaderInSession1.Factory.Save();
			}
			ErrorReporter.Clear();
			try
			{
				jobHeaderInSession2.JH_SystemLastEditUser = userInSession2.GS_Code;
				using (Env.SetTemporaryUserContext(userInSession2.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					jobHeaderInSession2.Factory.Save();
				}
				Assert("Expect no Excception", true);
			}
			catch (ZSaveException ex)
			{
				Fail("Expected no Exception but was a ZSaveException with message : " + ex.Message);
			}
			catch (Exception e)
			{
				Fail(string.Format("Expected no Exception but was a {0} with message : {1}", e.GetType(), e.Message));
			}
		}

		void AssertNoConcurrency(UpdateValue updateValueInSession1, UpdateValue updateValueInSession2, JobHeader jobHeaderInSession1, JobHeader jobHeaderInSession2)
		{
			updateValueInSession1(jobHeaderInSession1);
			jobHeaderInSession1.Factory.Save();
			ErrorReporter.Clear();
			try
			{
				updateValueInSession2(jobHeaderInSession2);
				jobHeaderInSession2.Factory.Save();
				Assert("Expect no Excception", true);
			}
			catch (ZSaveException ex)
			{
				Fail("Expected no Exception but was a ZSaveException with message : " + ex.Message);
			}
			catch (Exception e)
			{
				Fail(string.Format("Expected no Exception but was a {0} with message : {1}", e.GetType(), e.Message));
			}
		}

		#region Implementation
		delegate void UpdateValue(JobHeader jobHeader);

		BusinessObjectFactory GetNonUpdatingFactory()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			return factory;
		}

		JobHeader NewJobHeaderWithSpecificStatusLocalChargesAddrAndUniqueJobInoiceNumber()
		{
			JobHeader result = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			result.JH_Status = JobHeaderStatus.Working.Code;
			result.JH_UniqueJobInvoiceNumber = 1;
			result.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgAddress>().PK;
			return result;
		}
		#endregion
	}
}
