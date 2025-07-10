using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(HVLVISFMessagesSendWrapper))]
	public class HVLVISFMessagesSendWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetRelatedJob()
		{
			var isfHeader = Factory.NewWithValidTestData<CusISFHeader>();
			var wrapper = new HVLVISFMessagesSendWrapper(isfHeader);

			AssertEquals(isfHeader.PK, wrapper.RelatedJob.BusinessObjectPK);
			AssertEquals(isfHeader.BF_JobReference, wrapper.JobNumber);
			AssertEquals(isfHeader.BF_CustomsStatus, wrapper.JobStatus);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => new HVLVISFMessagesSendWrapper(Factory.NewWithValidTestData<CusISFHeader>());

		#endregion
	}
}
