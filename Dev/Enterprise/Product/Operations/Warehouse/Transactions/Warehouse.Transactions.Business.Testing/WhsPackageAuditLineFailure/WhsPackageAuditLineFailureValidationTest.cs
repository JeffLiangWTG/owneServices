using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsPackageAuditLineFailureValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestExpectedQuantityIsDifferentOfAuditedQuantity

		public void TestExpectedQuantityIsDifferentOfAuditedQuantity()
		{
			var auditFailure = Factory.New<WhsPackageAuditLineFailure>();

			auditFailure.WPF_AuditedQty = 1;
			auditFailure.WPF_ExpectedQty = 1;

			AssertHasError(auditFailure.WPF_AuditedQtyInfo, "Audited Quantity must be different than the Expected Quantity.");
			AssertHasError(auditFailure.WPF_ExpectedQtyInfo, "Audited Quantity must be different than the Expected Quantity.");

			auditFailure.WPF_AuditedQty = 2;
			AssertNoErrors(auditFailure.WPF_AuditedQtyInfo);
			AssertNoErrors(auditFailure.WPF_ExpectedQtyInfo);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var log = Factory.New<WhsPackageAuditLineFailure>();
			var validation = new TestWhsPackageAuditLineFailureValidation(log);

			foreach (var propertyInfo in log.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (propertyInfo.Name == WhsPackageAuditLineFailureSchema.Constants.WPF_WPA_WhsPackageAudit)
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsPackageAuditLineFailureValidation

		class TestWhsPackageAuditLineFailureValidation : WhsPackageAuditLineFailureValidation
		{
			public TestWhsPackageAuditLineFailureValidation(WhsPackageAuditLineFailure parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
