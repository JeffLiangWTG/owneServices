using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocumentDeliveryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJDC_DocumentGroup_List()
		{
			Assert("JDC_DocumentGroup_List.Count > 0", jobDocumentDelivery.Lookups.JDC_DocumentGroup_List.Count > 0);
		}

		public void TestJDC_DeliveryMethod_List()
		{
			Assert("JDC_DeliveryMethod_List.Count > 0", jobDocumentDelivery.Lookups.JDC_DeliveryMethod_List.Count > 0);
		}

		public void TestJDC_AttachmentType_List()
		{
			Assert("JDC_AttachmentType_List.Count > 0", jobDocumentDelivery.Lookups.JDC_AttachmentType_List.Count > 0);
		}

		#region Implementation

		JobDocumentDelivery jobDocumentDelivery;

		protected override void SetUp()
		{
			base.SetUp();
			jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
		}

		#endregion
	}
}
