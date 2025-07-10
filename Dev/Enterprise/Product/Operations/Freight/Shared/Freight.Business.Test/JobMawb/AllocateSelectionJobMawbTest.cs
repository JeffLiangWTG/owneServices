using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(AllocateSelectionJobMawb))]
	sealed class AllocateSelectionJobMawbTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AllocateSelectionJobMawb(Factory, System.Array.Empty<BusinessObject>());
		}

		#region TestJM_OH_Allocated

		public void TestJM_OH_Allocated()
		{
			AllocateSelectionJobMawb testClass = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			AssertEquals(false, testClass.HasChanges);

			testClass.AllocatedTo = new ZGuid("B863236B-1490-4EC3-8C6B-1218EEBE21CD");

			AssertEquals(true, testClass.HasChanges);

			AssertEquals(true, testClass.AllocatedToInfo.Name == "AllocatedTo");
		}

		#endregion

		#region ReservedUntil

		public void TestReservedUntil()
		{
			AllocateSelectionJobMawb testClass = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			AssertEquals(false, testClass.HasChanges);

			testClass.ReservedUntil = new ZDateTime(2004, 3, 3);

			AssertEquals(true, testClass.HasChanges);

			AssertEquals(true, testClass.ReservedUntilInfo.Name == AllocateSelectionJobMawb.Schema.ReservedUntil);
		}

		#endregion

		#region IsCompletedAWBReturned

		public void TestIsCompletedAWBReturned()
		{
			AllocateSelectionJobMawb testClass = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			AssertEquals(false, testClass.HasChanges);

			testClass.IsCompletedAWBReturned = true;

			AssertEquals(true, testClass.HasChanges);

			AssertEquals(true, testClass.IsCompletedAWBReturnedInfo.Name == AllocateSelectionJobMawb.Schema.IsCompletedAWBReturned);
		}

		#endregion

		#region IsBorrowedAWBInvoiced

		public void TestIsBorrowedAWBInvoiced()
		{
			AllocateSelectionJobMawb testClass = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			AssertEquals(false, testClass.HasChanges);

			testClass.IsBorrowedAWBInvoiced = true;

			AssertEquals(true, testClass.HasChanges);

			AssertEquals(true, testClass.IsBorrowedAWBInvoicedInfo.Name == AllocateSelectionJobMawb.Schema.IsBorrowedAWBInvoiced);
		}

		#endregion

		#region TestFeildValidation

		public void TestFeildValidation()
		{
			AllocateSelectionJobMawb testClass = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			testClass.ReservedUntil = new ZDateTime(2005, 3, 3);
			AssertEquals(false, testClass.AllocatedToInfo.HasErrors());

			testClass = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			testClass.IsBorrowedAWBInvoiced = true;
			AssertEquals(true, testClass.AllocatedToInfo.HasErrors());

			testClass = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			testClass.IsCompletedAWBReturned = true;
			AssertEquals(true, testClass.AllocatedToInfo.HasErrors());

			testClass = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			testClass.AllocatedTo = new ZGuid("01F6F60F-244C-4B4D-9B6C-F95BC7AE6D98");
			AssertEquals(false, testClass.AllocatedToInfo.HasErrors());
		}

		#endregion

		#region TestProcess

		public void TestProcess()
		{
			JobMawb jobOne = Factory.New<JobMawb>();

			jobOne.JM_MAWB = "11111111";
			jobOne.JM_Airline3DigitPrefix = "153";

			JobMawb jobTwo = Factory.New<JobMawb>();

			jobTwo.JM_MAWB = "11111112";
			jobTwo.JM_Airline3DigitPrefix = "153";

			JobMawb jobNotSelected = Factory.New<JobMawb>();

			jobNotSelected.JM_MAWB = "11111111";
			jobNotSelected.JM_Airline3DigitPrefix = "154";

			jobNotSelected.Factory.Save();

			JobMawb[] jobMawbs = new JobMawb[2];

			jobMawbs[0] = jobOne;
			jobMawbs[1] = jobTwo;

			AllocateSelectionJobMawb testSelection = new AllocateSelectionJobMawb(Factory, jobMawbs);

			ZGuid forwarder = new ZGuid("52693619-6A83-467D-8400-D3EED5D5CDD3");

			testSelection.AllocatedTo = forwarder;
			testSelection.IsCompletedAWBReturned = true;
			testSelection.IsBorrowedAWBInvoiced = true;

			testSelection.Process();

			ZQuery filter = new ZQuery(JobMawbSchema.JM_OH_AllocatedTo, forwarder);

			JobMawb[] result = (JobMawb[])Factory.Load(typeof(JobMawb), filter);

			AssertEquals("Processed JobMawbs have not been Saved to the database", 2, result.Length);
		}

		#endregion
	}
}
