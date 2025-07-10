using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ShipmentRequestSupportedAddressTypesProvider))]
	internal class ShipmentRequestSupportedAddressTypesProviderTest : TestCaseWithFactory
	{
		public void TestSupportedAssigneeAddressTypes()
		{
			var provider = new ShipmentRequestSupportedAddressTypesProvider();

			AssertEquals(4, provider.SupportedAssigneeAddressTypes.Count);
			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Codes.ConsigneeDocumentaryAddress,
				DocAddressTypes.Codes.ConsignorDocumentaryAddress,
				DocAddressTypes.Codes.ControllingCustomer,
				DocAddressTypes.Codes.LocalClient
			}, provider.SupportedAssigneeAddressTypes.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Descriptions.ConsigneeDocumentaryAddress,
				DocAddressTypes.Descriptions.ConsignorDocumentaryAddress,
				DocAddressTypes.Descriptions.ControllingCustomer,
				DocAddressTypes.Descriptions.LocalClient
			}, provider.SupportedAssigneeAddressTypes.OfType<ICodeDescription>().Select(el => el.Description).ToArray());
		}

		public void TestSupportedReviewerAddressTypes()
		{
			var provider = new ShipmentRequestSupportedAddressTypesProvider();

			AssertEquals(4, provider.SupportedReviewerAddressTypes.Count);
			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Codes.ConsigneeDocumentaryAddress,
				DocAddressTypes.Codes.ConsignorDocumentaryAddress,
				DocAddressTypes.Codes.ControllingCustomer,
				DocAddressTypes.Codes.LocalClient
			}, provider.SupportedReviewerAddressTypes.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Descriptions.ConsigneeDocumentaryAddress,
				DocAddressTypes.Descriptions.ConsignorDocumentaryAddress,
				DocAddressTypes.Descriptions.ControllingCustomer,
				DocAddressTypes.Descriptions.LocalClient
			}, provider.SupportedReviewerAddressTypes.OfType<ICodeDescription>().Select(el => el.Description).ToArray());
		}
	}
}
