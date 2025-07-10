using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(JobStorage))]
	public abstract class JobStorageTestCase : EnterpriseBusinessObjectTestCase
	{
		public void TestPopulateET_StorageJobNumberIfRequired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var storage = GetStorageObject();
			storage.ET_OH_Client = ZGuid.Empty;
			var exceptionThrown = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				exceptionThrown = true;
			}
			Assert(exceptionThrown);
			exceptionThrown = false;
			AssertEquals(false, storage.IsInDatabase);
			AssertEquals(ZString.Empty, storage.ET_StorageJobNumber);

			storage.ET_OH_Client = org.PK;
			Factory.Save();
			Assert(!exceptionThrown);
			AssertEquals(true, storage.IsInDatabase);
			AssertNotEquals(ZString.Empty, storage.ET_StorageJobNumber);
			string number = storage.ET_StorageJobNumber;

			storage.ET_OH_Client = ZGuid.Empty;
			try
			{
				Factory.Save();
			}
			catch
			{
				exceptionThrown = true;
			}
			Assert(exceptionThrown);
			exceptionThrown = false;
			AssertEquals(true, storage.IsInDatabase);
			AssertEquals(number, storage.ET_StorageJobNumber);
		}

		#region TestTypeDecider

		public void TestTypeDecider()
		{
			var whsInvoice = Factory.New<IWhsInvoice>();
			var otherJobStorage = Factory.New<IWhsInvoice>();
			((JobStorage)otherJobStorage).ET_StorageType = "XYZ";

			AssertType(ObjectFactory.GetType<IWhsInvoice>(), Factory.Load<JobStorage>(((BusinessObject)whsInvoice).PK));
			AssertExceptionThrown<NoConcreteTypeException>(() => Factory.Load<JobStorage>(((BusinessObject)otherJobStorage).PK));
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent jobStorage = GetStorageObject();
			AssertEquals(ExpectedIJobHeaderParent_AllowInvoiceDeletion, jobStorage.AllowInvoiceDeletion);
		}

		protected virtual bool ExpectedIJobHeaderParent_AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		public abstract void TestIJobInvoicingPlugIn_DefaultDebtor();

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			var testJob = (IJobInvoicingPlugIn)GetStorageObject();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region Implementation

		protected JobStorage GetStorageObject()
		{
			return (JobStorage)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetStorageObject();
		}

		#endregion
	}
}
