using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(LoadPlanRequestSupportedAddressTypesProvider))]
	internal class LoadPlanRequestSupportedAddressTypesProviderTest : TestCaseWithFactory
	{
		public void TestSupportedAssigneeAddressTypes()
		{
			var provider = new LoadPlanRequestSupportedAddressTypesProvider();

			AssertEquals(4, provider.SupportedAssigneeAddressTypes.Count);
			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Codes.BuyerDocumentaryAddress,
				DocAddressTypes.Codes.SupplierDocumentaryAddress,
				DocAddressTypes.Codes.ControllingCustomer,
				DocAddressTypes.Codes.Manufacturer,
			}, provider.SupportedAssigneeAddressTypes.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Descriptions.BuyerDocumentaryAddress,
				DocAddressTypes.Descriptions.SupplierDocumentaryAddress,
				DocAddressTypes.Descriptions.ControllingCustomer,
				DocAddressTypes.Descriptions.Manufacturer,
			}, provider.SupportedAssigneeAddressTypes.OfType<ICodeDescription>().Select(el => el.Description).ToArray());
		}

		public void TestSupportedReviewerAddressTypes()
		{
			var provider = new LoadPlanRequestSupportedAddressTypesProvider();

			AssertEquals(4, provider.SupportedReviewerAddressTypes.Count);
			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Codes.BuyerDocumentaryAddress,
				DocAddressTypes.Codes.SupplierDocumentaryAddress,
				DocAddressTypes.Codes.ControllingCustomer,
				DocAddressTypes.Codes.Manufacturer,
			}, provider.SupportedReviewerAddressTypes.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Descriptions.BuyerDocumentaryAddress,
				DocAddressTypes.Descriptions.SupplierDocumentaryAddress,
				DocAddressTypes.Descriptions.ControllingCustomer,
				DocAddressTypes.Descriptions.Manufacturer,
			}, provider.SupportedReviewerAddressTypes.OfType<ICodeDescription>().Select(el => el.Description).ToArray());
		}
	}
}
