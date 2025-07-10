using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class GenApprovalRequestValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXP_ApprovalRequestDataIsValidZBlobSize()
		{
			var request = GetNewParentBusinessObject();
			request.XP_ApprovalRequestData = new ZBlob(new byte[1024 * 1024 * 10]);
			var validation = GetNewValidation(request);
			validation.ValidateXP_ApprovalRequestData();
			AssertNoErrors("XP_ApprovalRequestData should not be limited by this validation.", request.XP_ApprovalRequestDataInfo);
		}

		protected virtual GenApprovalRequestValidation GetNewValidation(GenApprovalRequest approvalRequest)
		{
			return new GenApprovalRequestValidation(approvalRequest);
		}

		protected virtual GenApprovalRequest GetNewParentBusinessObject()
		{
			return Factory.New<GenApprovalRequest>();
		}
	}
}
