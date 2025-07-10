using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.EPayment
{
	sealed class EPaymentPropertyHelperTest : TestCaseWithFactory
	{
		public void TestGetDefaultEPaymentReferenceForEPaymentMethod()
		{
			var collection = new DefaultEPaymentReferenceCollection();
			var reference = collection.AddNew();
			reference.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reference.ReferenceType = EPaymentReferenceTypes.FreeText;
			reference.Reference = "Test Reference";

			using (AccountingMasterFilesRegistry.Instance.DefaultPaymentReference.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var defaultEPaymentReference = EPaymentPropertyHelper.GetDefaultEPaymentReferenceForEPaymentMethod(EPaymentMethods.EPaymentViaOFX);
				AssertNotNull(defaultEPaymentReference);
				AssertEquals(EPaymentReferenceTypes.FreeText, defaultEPaymentReference.ReferenceType);
				AssertEquals("Test Reference", defaultEPaymentReference.Reference);

				defaultEPaymentReference = EPaymentPropertyHelper.GetDefaultEPaymentReferenceForEPaymentMethod("XXX");
				AssertNull(defaultEPaymentReference);

				AccountingMasterFilesRegistry.Instance.DefaultPaymentReference.Value.Cast<DefaultEPaymentReference>().FirstOrDefault().ProviderCode = "XXX";
				defaultEPaymentReference = EPaymentPropertyHelper.GetDefaultEPaymentReferenceForEPaymentMethod(EPaymentMethods.EPaymentViaOFX);
				AssertNull(defaultEPaymentReference);
			}
		}

		#region PaymentReasons

		public void TestGetPaymentReasonsForEPaymentMethod()
		{
			AssertEquals(7, EPaymentPropertyHelper.GetPaymentReasonsForEPaymentMethod(Guid.Empty, EPaymentMethods.EPaymentViaOFX).Count);
			AssertEquals(0, EPaymentPropertyHelper.GetPaymentReasonsForEPaymentMethod(Guid.Empty, "XXX").Count);
		}

		public void TestGetPaymentReasonsForProvider()
		{
			AssertEquals(7, EPaymentPropertyHelper.GetPaymentReasonsForProvider(Guid.Empty, EPaymentProviderCodes.Codes.OFX).Count);
			AssertEquals(0, EPaymentPropertyHelper.GetPaymentReasonsForProvider(Guid.Empty, "XXX").Count);
		}

		public void TestGetPaymentReasonDescriptionForProvider()
		{
			AssertEquals("Accounting services", EPaymentPropertyHelper.GetPaymentReasonDescriptionForProvider(Guid.Empty, EPaymentProviderCodes.Codes.OFX, EPaymentReasonCodes.OFXReasonCodes.AccountingServices));
			AssertNullOrEmpty(EPaymentPropertyHelper.GetPaymentReasonDescriptionForProvider(Guid.Empty, EPaymentMethods.EPaymentViaOFX, "XXX"));
			AssertNullOrEmpty(EPaymentPropertyHelper.GetPaymentReasonDescriptionForProvider(Guid.Empty, "XXX", "XXX"));
		}

		#endregion

		#region DefaultPaymentReason

		public void TestGetDefaultPaymentReasonForEPaymentMethod()
		{
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.ServicesTrade, EPaymentPropertyHelper.GetDefaultPaymentReasonForEPaymentMethod(Guid.Empty, Guid.Empty, EPaymentMethods.EPaymentViaOFX));
			AssertNullOrEmpty(EPaymentPropertyHelper.GetDefaultPaymentReasonForEPaymentMethod(Guid.Empty, Guid.Empty, "XXX"));
		}

		public void TestGetDefaultPaymentReasonDescriptionForProvider()
		{
			AssertEquals("Services trade", EPaymentPropertyHelper.GetDefaultPaymentReasonDescriptionForProvider(Guid.Empty, Guid.Empty, EPaymentProviderCodes.Codes.OFX));
			AssertNullOrEmpty(EPaymentPropertyHelper.GetDefaultPaymentReasonDescriptionForProvider(Guid.Empty, Guid.Empty, "XXX"));
		}

		#endregion
	}
}
