using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocAddressCreatorHostValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckAddressTypeCode

		public void TestCheckAddressTypeCode()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();

			host.AddressTypeCode = "";
			AssertHasErrors(host.AddressTypeCodeInfo);

			host.AddressTypeCode = "CFS";
			AssertNoErrors(host.AddressTypeCodeInfo);

			host.AddressTypeCode = DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageCFS);
			AssertHasErrors(host.AddressTypeCodeInfo);

			host.AddressTypeCode = "CTO";
			AssertNoErrors(host.AddressTypeCodeInfo);
		}

		#endregion

		#region DocAddressCreatorHelper

		DocAddressCreatorHelper DocAddressCreatorHelper
		{
			get { return docAddressCreatorHelper ?? (docAddressCreatorHelper = new DocAddressCreatorHelper(Factory)); }
		}
		DocAddressCreatorHelper docAddressCreatorHelper;

		#endregion
	}
}
