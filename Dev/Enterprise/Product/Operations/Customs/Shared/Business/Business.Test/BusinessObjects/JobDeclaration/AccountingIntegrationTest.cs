using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using ICommonCartage = Enterprise.Freight.LocalCartage.Integration.ICommonCartage;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AccountingIntegrationTest : TestCaseWithFactory
	{
		public void TestStatusInformation()
		{
			var testDec = Factory.New<TestDeclaration>();
			var statusInformation = testDec.RatingAdapter.StatusInformation;
			Assert("Test dec override sets cannot execute", !statusInformation.CanExecute);
			Assert("Test dec override sets a message", !statusInformation.Message.IsEmpty);

			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var branch = otherCompany.Branches.AddNew();
			branch.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			testDec.JE_GB = branch.PK;
			statusInformation = testDec.RatingAdapter.StatusInformation;
			Assert("Test dec status information should be default", statusInformation.CanExecute);
			Assert("Test dec status information should be default", statusInformation.Message.IsEmpty);
		}

		public void TestOnJobCreating()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BusinessObject cartage = (BusinessObject)Factory.New<ICommonCartage>();
			Factory.Save();

			cartage[JobCartageSchema.JJ_ParentID] = declaration.PK;
			cartage[JobCartageSchema.JJ_ParentTableCode] = declaration.TablePrefix;

			new JobHeader.Loader((IJobHeaderParent)cartage).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(((ICommonCartage)cartage).Job);
			Assert(((ICommonCartage)cartage).Job.JH_JH_ParentJob.IsEmpty);
			Assert(((ICommonCartage)cartage).Job.JH_OA_LocalChargesAddr.IsEmpty);
			Factory.Save();

			new JobHeader.Loader(declaration).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertEquals(declaration.Job.PK, ((ICommonCartage)cartage).Job.JH_JH_ParentJob);
		}

		public void TestLocalClient()
		{
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "hello";
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			job.JH_ParentID = declaration.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertEquals("Local client", declaration.Job.LocalChargesAddr.PK, declaration.LocalClientAddress.PK);
		}

		public void TestAutoPostingEmailRecipients()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				using (CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new AutoBillingGroupNotification()))
				{
					declaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
					declaration.CusAgent.GS_EmailAddress = "staff@cargowise.com";
					AssertEquals("Registry is not set, then staff is notified", GlbStaff.CurrentUser.PK, ((ICustomsJobInfo)declaration).AutoPostingNotification.EmailRecipients[0]);
				}

				var group = Factory.Load<GlbGroup>(Constants.Groups.PostMastersGroupPK);
				group.Staff[0].GS_EmailAddress = "test@cargowise.com";
				var groupNotification = new AutoBillingGroupNotification
				{
					SendGroupPK = group.PK
				};
				using (CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification))
				{
					AssertEquals("Only the group set in the registry is notified", 1, ((ICustomsJobInfo)declaration).AutoPostingNotification.EmailRecipients.Length);
					AssertEquals("Registry is set and is a priority", group.PK, ((ICustomsJobInfo)declaration).AutoPostingNotification.EmailRecipients[0]);
				}
			});
		}

		public void TestICustomsJobInfoMembers()
		{
			var declaration = Factory.New<TestDeclaration>();
			ICustomsJobInfo info = declaration;
			AssertEquals(creditor.PK, info.CreditorPK);
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, declaration.PK.ToGuid());
			AssertEquals(ZGuid.Empty, info.CreditorPK);

			declaration.JE_GB = ZGuid.Empty;
			AssertNoExceptionThrown(delegate
			{ info.GetValidAPInvoiceNumsToMatchAndValidateAgainst(); });
		}

		public void TestMAWBPrefixIsNotDefaultedForSpecialFlightTerm()
		{
			var testDec = Factory.New<TestDeclaration>();
			testDec.JE_TransportMode = "AIR";
			AssertEquals("Pre-condition", "", testDec.JE_MasterBill);

			testDec.JE_VoyageFlightNo = "VARIOUS";
			AssertEquals("Special flight term should not default a Mawb prefix for Airline code", "", testDec.JE_MasterBill);

			testDec.JE_VoyageFlightNo = "QF1";
			AssertEquals("Mawb prefix should default", "081", testDec.JE_MasterBill);

			testDec.JE_VoyageFlightNo = "VARIOUS";
			AssertEquals("Special flight term should clear out Mawb value", "", testDec.JE_MasterBill);
		}

		AccChargeCode chargeCode;
		OrgHeader creditor;
		protected override void SetUp()
		{
			base.SetUp();

			chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompany.PK));
			creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
		}

		public class TestDeclaration : BaseJobDeclaration
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override JobDeclarationIAccIntegrationDataProvider GetJobDeclarationIAccIntegrationDataProvider()
			{
				return new TestJobDeclarationIAccIntegrationDataProvider(this);
			}

			protected override RatingAdaptersProvider GetRatingAdaptersProviderCore()
			{
				return new TestBaseJobDeclarationRatingAdaptersProvider(this);
			}

			protected override IAutoRating GetRatingAdapterCore()
			{
				return new TestBaseJobDeclarationRatingAdapter(this);
			}

			public override bool HasSpecialFlightTerm => JE_VoyageFlightNo == "VARIOUS";
		}

		public class TestBaseJobDeclarationRatingAdaptersProvider : BaseJobDeclarationRatingAdaptersProvider
		{
			public TestBaseJobDeclarationRatingAdaptersProvider(BaseJobDeclaration parent) : base(parent) { }

			protected override List<IAutoRating> GetAdapters(BaseJobDeclaration parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
			{
				return new List<IAutoRating> { new TestBaseJobDeclarationRatingAdapter(parent) };
			}
		}

		public class TestBaseJobDeclarationRatingAdapter : BaseJobDeclarationRatingAdapter<BaseJobDeclaration>
		{
			public TestBaseJobDeclarationRatingAdapter(BaseJobDeclaration parent)
				: base(parent)
			{
			}

			protected override AutoRatingStatusInfo GetStatusInformationCore()
			{
				return new AutoRatingStatusInfo(false, "Some text");
			}
		}

		public class TestJobDeclarationIAccIntegrationDataProvider : JobDeclarationIAccIntegrationDataProvider
		{
			public TestJobDeclarationIAccIntegrationDataProvider(BaseJobDeclaration declaration)
				: base(declaration.GetActionsToTake(), declaration.GetEntryHeaderPKs(), declaration.PK, true, new BusinessObjectFactory())
			{
			}
		}
	}
}
