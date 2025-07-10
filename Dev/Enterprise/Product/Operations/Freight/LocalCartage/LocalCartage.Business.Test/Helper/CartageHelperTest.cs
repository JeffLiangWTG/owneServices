using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageHelperTest : TestCaseWithFactory
	{
		// TODO: Change to reflect different parameters
		public void TestFindCartage()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			AssertNull(CartageHelper.FindCartage(cartageType));
			var cartageUnattached = Factory.New<CommonCartage>();
			cartageUnattached.JJ_ConsignmentID = "BBB";
			AssertNull(CartageHelper.FindCartage(cartageType));
			var cartageAttachedToParent_Inactive = Factory.New<CommonCartage>();
			cartageAttachedToParent_Inactive.JJ_ConsignmentID = "AAA";
			cartageAttachedToParent_Inactive.JJ_IsCancelled = true;
			cartageAttachedToParent_Inactive.SetParent(dummyCartageParent);
			AssertEquals(cartageAttachedToParent_Inactive, CartageHelper.FindCartage(cartageType));
			var cartageAttachedToParent_Inactive_MatchesBranch = Factory.New<CommonCartage>();
			cartageAttachedToParent_Inactive_MatchesBranch.JJ_ConsignmentID = "AAA2";
			cartageAttachedToParent_Inactive_MatchesBranch.JJ_IsCancelled = true;
			cartageAttachedToParent_Inactive_MatchesBranch.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartageAttachedToParent_Inactive_MatchesBranch.SetParent(dummyCartageParent);
			AssertEquals(cartageAttachedToParent_Inactive_MatchesBranch, CartageHelper.FindCartage(cartageType));
			var cartageAttachedToParent_Active = Factory.New<CommonCartage>();
			cartageAttachedToParent_Active.JJ_ConsignmentID = "AAA3";
			cartageAttachedToParent_Active.SetParent(dummyCartageParent);
			AssertEquals(cartageAttachedToParent_Active, CartageHelper.FindCartage(cartageType));
			var cartageAttachedToParent_Active_MatchesBranch = Factory.New<CommonCartage>();
			cartageAttachedToParent_Active_MatchesBranch.JJ_ConsignmentID = "AAA4";
			cartageAttachedToParent_Active_MatchesBranch.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartageAttachedToParent_Active_MatchesBranch.SetParent(dummyCartageParent);
			AssertEquals(cartageAttachedToParent_Active_MatchesBranch, CartageHelper.FindCartage(cartageType));
		}

		public void TestAttachCartageJobsToParentJob_AttachesIfParentIsNotInDatabase_NonStatic()
		{
			TestAttachCartageJobsToParentJob_AttachesIfParentIsNotInDatabaseCore(false);
		}

		public void TestAttachCartageJobsToParentJob_AttachesIfParentIsNotInDatabase_Static()
		{
			TestAttachCartageJobsToParentJob_AttachesIfParentIsNotInDatabaseCore(true);
		}

		void TestAttachCartageJobsToParentJob_AttachesIfParentIsNotInDatabaseCore(bool callStaticMethod)
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment.FillWithValidTestData();
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			var cartageJobHeader = Factory.NewJobForTesting<JobHeader>();
			cartageJobHeader.JH_ParentID = cartage.PK;
			cartageJobHeader.JH_ParentTableCode = cartage.TablePrefix;
			var shipmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			shipmentJobHeader.JH_ParentID = shipment.PK;
			shipmentJobHeader.JH_ParentTableCode = shipment.TablePrefix;

			var shipmentAsInvoicingSupporter = new JobInvoicingSupporter((IJobHeaderParent)shipment);

			AssertEquals("Precondition - shipment has job header", shipmentAsInvoicingSupporter.Job.PK, shipmentJobHeader.PK);
			AssertNotNull("Precondition - cartage has job header", cartage.Job);
			AssertEquals("Precondition - cartage job header parent is empty", ZGuid.Empty, cartage.Job.JH_JH_ParentJob);

			if (callStaticMethod)
			{
				CartageHelper.AttachCartageJobsToParentJob(shipmentJobHeader, shipment.PK);
			}
			else
			{
				helper.AttachCartageJobsToParentJob(shipmentJobHeader, shipment.PK);
			}

			AssertEquals("After call to CartageHelper.AttachCartageJobsToParentJob() cartage job header should be child of shipment job header", shipmentJobHeader.PK, cartage.Job.JH_JH_ParentJob);
		}

		public void TestAttachCartageJobsToParentJob_DoesNotAttachIfParentIsInDatabase_NonStatic()
		{
			TestAttachCartageJobsToParentJob_DoesNotAttachIfParentIsInDatabaseCore(false);
		}

		public void TestAttachCartageJobsToParentJob_DoesNotAttachIfParentIsInDatabase_Static()
		{
			TestAttachCartageJobsToParentJob_DoesNotAttachIfParentIsInDatabaseCore(true);
		}

		void TestAttachCartageJobsToParentJob_DoesNotAttachIfParentIsInDatabaseCore(bool callStaticMethod)
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment.FillWithValidTestData();
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			var cartageJobHeader = Factory.NewJobForTesting<JobHeader>();
			cartageJobHeader.JH_ParentID = cartage.PK;
			cartageJobHeader.JH_ParentTableCode = cartage.TablePrefix;
			var shipmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			shipmentJobHeader.JH_ParentID = shipment.PK;
			shipmentJobHeader.JH_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			cartage.Job.JH_JH_ParentJob = ZGuid.Empty;
			Factory.Save();

			var shipmentAsInvoicingSupporter = new JobInvoicingSupporter((IJobHeaderParent)shipment);

			AssertEquals("Precondition - shipment has job header", shipmentAsInvoicingSupporter.Job.PK, shipmentJobHeader.PK);
			AssertNotNull("Precondition - cartage has job header", cartage.Job);
			AssertEquals("Precondition - cartage job header parent is empty", ZGuid.Empty, cartage.Job.JH_JH_ParentJob);

			if (callStaticMethod)
			{
				CartageHelper.AttachCartageJobsToParentJob(shipmentJobHeader, shipment.PK);
			}
			else
			{
				helper.AttachCartageJobsToParentJob(shipmentJobHeader, shipment.PK);
			}

			AssertEquals("After call to CartageHelper.AttachCartageJobsToParentJob() cartage job header parent should still be empty", ZGuid.Empty, cartage.Job.JH_JH_ParentJob);
		}

		public void TestAttachCartageJobsToParentJob_AttachesIfParentIsInDatabaseButAttachFlagIsSet_NonStatic()
		{
			TestAttachCartageJobsToParentJob_AttachesIfParentIsInDatabaseButAttachFlagIsSetCore(false);
		}

		public void TestAttachCartageJobsToParentJob_AttachesIfParentIsInDatabaseButAttachFlagIsSet_Static()
		{
			TestAttachCartageJobsToParentJob_AttachesIfParentIsInDatabaseButAttachFlagIsSetCore(true);
		}

		void TestAttachCartageJobsToParentJob_AttachesIfParentIsInDatabaseButAttachFlagIsSetCore(bool callStaticMethod)
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment.FillWithValidTestData();
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			var cartageJobHeader = Factory.NewJobForTesting<JobHeader>();
			cartageJobHeader.JH_ParentID = cartage.PK;
			cartageJobHeader.JH_ParentTableCode = cartage.TablePrefix;
			var shipmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			shipmentJobHeader.JH_ParentID = shipment.PK;
			shipmentJobHeader.JH_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			cartage.Job.JH_JH_ParentJob = ZGuid.Empty;
			Factory.Save();

			var shipmentAsInvoicingSupporter = new JobInvoicingSupporter((IJobHeaderParent)shipment);

			AssertEquals("Precondition - shipment has job header", shipmentAsInvoicingSupporter.Job.PK, shipmentJobHeader.PK);
			AssertNotNull("Precondition - cartage has job header", cartage.Job);
			AssertEquals("Precondition - cartage job header parent is empty", ZGuid.Empty, cartage.Job.JH_JH_ParentJob);

			if (callStaticMethod)
			{
				CartageHelper.AttachCartageJobsToParentJob(shipmentJobHeader, shipment.PK, true);
			}
			else
			{
				helper.AttachCartageJobsToParentJob(shipmentJobHeader, shipment.PK, true);
			}

			AssertEquals("After call to CartageHelper.AttachCartageJobsToParentJob() cartage job header should be child of shipment job header", shipmentJobHeader.PK, cartage.Job.JH_JH_ParentJob);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new CartageHelper();
		}

		CartageHelper helper;
	}
}
