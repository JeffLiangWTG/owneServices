using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PickAndPackInfoWebServiceResponseTest : WebServiceResponseTestCase
	{
		#region TestDefaultPackType

		public void TestDefaultPackType()
		{
			var response = new PickAndPackInfoWebServiceResponse();
			AssertEquals("", response.DefaultPackType);

			response.DefaultPackType = "BOX";
			AssertEquals("BOX", response.DefaultPackType);
		}

		#endregion

		#region TestExistingPackages

		public void TestExistingPackages()
		{
			var response = new PickAndPackInfoWebServiceResponse();
			AssertNull(response.ExistingPackages);

			response.ExistingPackages = new[] { new PackageInfo() };
			AssertNotNull(response.ExistingPackages);
		}

		#endregion

		#region TestPackTypes

		public void TestPackTypes()
		{
			var response = new PickAndPackInfoWebServiceResponse();
			AssertNull(response.PackTypes);

			var pair = new CodeDescriptionPair("Code1", "Description1");
			response.PackTypes = new[] { new CodeDescriptionPairInfo(pair) };
			AssertNotNull(response.PackTypes);
		}

		#endregion

		#region TestExistingClosedPackages

		public void TestExistingClosedPackages()
		{
			var response = new PickAndPackInfoWebServiceResponse();
			AssertNull(response.ExistingClosedPackages);

			response.ExistingClosedPackages = new[] { new PackageInfo() };
			AssertNotNull(response.ExistingClosedPackages);
		}

		#endregion

		#region TestIsUsingOwnLabel

		public void TestIsUsingOwnLabel()
		{
			var response = new PickAndPackInfoWebServiceResponse();
			AssertEquals(false, response.IsUsingOwnLabel);

			response.IsUsingOwnLabel = true;
			AssertEquals(true, response.IsUsingOwnLabel);
		}

		#endregion

		#region TestPromptForWeightAndDims

		public void TestPromptForWeightAndDims()
		{
			var response = new PickAndPackInfoWebServiceResponse();
			AssertEquals(false, response.PromptForWeightAndDims);

			response.PromptForWeightAndDims = true;
			AssertEquals(true, response.PromptForWeightAndDims);
		}

		#endregion

		#region TestSupportsCarrierLabelIntegration

		public void TestSupportsCarrierLabelIntegration()
		{
			var response = new PickAndPackInfoWebServiceResponse();
			AssertEquals(false, response.SupportsCarrierLabelIntegration);

			response.SupportsCarrierLabelIntegration = true;
			AssertEquals(true, response.SupportsCarrierLabelIntegration);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new PickAndPackInfoWebServiceResponse();
		}

		#endregion
	}
}
