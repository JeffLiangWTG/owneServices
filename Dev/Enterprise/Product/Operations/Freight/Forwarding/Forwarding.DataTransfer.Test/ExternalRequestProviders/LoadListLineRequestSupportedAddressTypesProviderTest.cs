using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(LoadListLineRequestSupportedAddressTypesProvider))]
	internal class LoadListLineRequestSupportedAddressTypesProviderTest : TestCaseWithFactory
	{
		public void TestSupportedAssigneeAddressTypes()
		{
			var provider = new LoadListLineRequestSupportedAddressTypesProvider();

			AssertEquals(5, provider.SupportedAssigneeAddressTypes.Count);
			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Codes.BuyerDocumentaryAddress,
				DocAddressTypes.Codes.SupplierDocumentaryAddress,
				DocAddressTypes.Codes.ControllingCustomer,
				DocAddressTypes.Codes.Manufacturer,
				DocAddressTypes.Codes.LoadListParty
			}, provider.SupportedAssigneeAddressTypes.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Descriptions.BuyerDocumentaryAddress,
				DocAddressTypes.Descriptions.SupplierDocumentaryAddress,
				DocAddressTypes.Descriptions.ControllingCustomer,
				DocAddressTypes.Descriptions.Manufacturer,
				DocAddressTypes.Descriptions.LoadListParty
			}, provider.SupportedAssigneeAddressTypes.OfType<ICodeDescription>().Select(el => el.Description).ToArray());
		}

		public void TestSupportedReviewerAddressTypes()
		{
			var provider = new LoadListLineRequestSupportedAddressTypesProvider();

			AssertEquals(5, provider.SupportedReviewerAddressTypes.Count);
			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Codes.BuyerDocumentaryAddress,
				DocAddressTypes.Codes.SupplierDocumentaryAddress,
				DocAddressTypes.Codes.ControllingCustomer,
				DocAddressTypes.Codes.Manufacturer,
				DocAddressTypes.Codes.LoadListParty
			}, provider.SupportedReviewerAddressTypes.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

			AssertArrayEqualsByElements(new string[] {
				DocAddressTypes.Descriptions.BuyerDocumentaryAddress,
				DocAddressTypes.Descriptions.SupplierDocumentaryAddress,
				DocAddressTypes.Descriptions.ControllingCustomer,
				DocAddressTypes.Descriptions.Manufacturer,
				DocAddressTypes.Descriptions.LoadListParty
			}, provider.SupportedReviewerAddressTypes.OfType<ICodeDescription>().Select(el => el.Description).ToArray());
		}
	}
}
