using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobMawbUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestConflictResolution()
		{
			var mawb1 = Factory.New<JobMawbForTest>();
			mawb1.JM_Airline3DigitPrefix = "001";
			mawb1.JM_MAWB = "00000011";
			mawb1.JM_ServiceLevel = "STD";
			mawb1.JM_GB = GlbBranch.CurrentBranch.PK;

			var mawb2 = Factory.New<JobMawbForTest>();
			mawb2.JM_Airline3DigitPrefix = "001";
			mawb2.JM_MAWB = "00000022";
			mawb2.JM_ServiceLevel = "STD";
			mawb2.JM_GB = GlbBranch.CurrentBranch.PK;

			var mawbParent = Factory.New<MawbParentForTest>();
			mawbParent.MasterBillAirlinePrefix = "001";
			mawbParent.MasterBillMAWB = "NOT ALLOCATED";

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var mawbParentFactory1 = factory1.Load<MawbParentForTest>(mawbParent.PK);
			var mawbParentFactory2 = factory2.Load<MawbParentForTest>(mawbParent.PK);

			mawbParentFactory1.IsNeutralMaster = true;
			mawbParentFactory1.AWBServiceLevel = "STD";
			mawbParentFactory1.MawbPortOfLoading = "AUSYD";
			mawbParentFactory1.MasterBillMAWB = "00000011";

			var mawb1Factory1 = factory1.Load<JobMawbForTest>(mawb1.PK);
			mawb1Factory1.JM_ParentID = mawbParentFactory1.PK;
			mawb1Factory1.JM_ParentTableCode = mawbParentFactory1.TablePrefix;
			mawb1Factory1.parentOverride = mawbParentFactory1;

			mawbParentFactory2.IsNeutralMaster = true;
			mawbParentFactory2.AWBServiceLevel = "STD";
			mawbParentFactory2.MawbPortOfLoading = "AUSYD";
			mawbParentFactory2.MasterBillMAWB = "00000022";

			var mawb2Factory2 = factory2.Load<JobMawbForTest>(mawb2.PK);
			mawb2Factory2.JM_ParentID = mawbParentFactory2.PK;
			mawb2Factory2.JM_ParentTableCode = mawbParentFactory2.TablePrefix;
			mawb2Factory2.parentOverride = mawbParentFactory2;

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("should throw concurrency error");
			}
			catch (ZSaveConcurrencyException ex)
			{
				AssertContainsExactElementsInAnyOrder(new[] { mawbParentFactory2 }, ex.BusinessObjects);
				ZExceptionReporting.HandleSaveException(ex);
			}

			try
			{
				factory2.Save();
				Fail("should throw save exception as it violates unique index");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertMultilineASCIIEquals("user notification",
				@"The MAWB has been replaced due to changes made by another user. Master Bill 00100000022 has returned to unallocated MAWB stock.

Please review the changes to this MawbParentForTest.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNoExceptionThrown("should resolve the issue", factory2.Save);

			mawbParent.Reload();
			mawb1.Reload();
			mawb2.Reload();

			AssertEquals("00100000011", mawbParent.MasterBillAirlinePrefix + mawbParent.MasterBillMAWB);
			AssertEquals(mawbParent.PK, mawb1.JM_ParentID);
			AssertEquals(mawbParent.TablePrefix, mawb1.JM_ParentTableCode);
			AssertEquals(ZGuid.Empty, mawb2.JM_ParentID);
			AssertEquals(ZString.Empty, mawb2.JM_ParentTableCode);
		}

		sealed class JobMawbForTest : JobMawb
		{
			public JobMawbForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override IMAWBParent ParentCore
			{
				get
				{
					return parentOverride ?? base.ParentCore;
				}
			}

			internal IMAWBParent parentOverride;
		}
	}
}
