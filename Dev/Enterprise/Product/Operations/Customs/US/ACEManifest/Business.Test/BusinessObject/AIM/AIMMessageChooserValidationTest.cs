using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.Testing;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	sealed class AIMMessageChooserValidationTest : TestCaseWithFactory
	{
		public void TestValidateReason()
		{
			var message = "Should only validate the input when the message type is FRC or FRX.";
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRC);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(chooser.ReasonInfo, "@#", AIMReasonCodes.Codes.R01);
			chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRX);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(chooser.ReasonInfo, "@#", AIMReasonCodes.Codes.R01);
			chooser = new AIMMessageChooser(header, items, string.Empty);
			chooser.Reason = string.Empty;
			chooser.Validation.ValidateAll();
			AssertNoMessageErrors(message, chooser.ReasonInfo);
			chooser.Reason = "@#";
			chooser.Validation.ValidateAll();
			AssertNoMessageErrors(message, chooser.ReasonInfo);
		}

		public void TestValidateRequestCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FSQ);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(chooser.RequestCodeInfo, "@#", AIMFreightStatusRequestCodes.Codes.RequestForRoutingInformation);
			chooser = new AIMMessageChooser(header, items, string.Empty);
			chooser.RequestCode = "@#";
			chooser.Validation.ValidateAll();
			AssertNoMessageErrors("Should only validate the input when the message type is FSQ.", chooser.RequestCodeInfo);
		}
	}
}
