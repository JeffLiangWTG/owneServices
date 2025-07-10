using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgColdCallRegister))]
	sealed class OrgColdCallRegisterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			var anotherFactory = new BusinessObjectFactory();
			var newSalesEnquiry = Factory.New<OrgColdCallRegister>();
			Factory.Save();
			AssertType(typeof(SalesEnquiry), anotherFactory.Load<OrgColdCallRegister>(newSalesEnquiry.PK));
		}

		#region GeoLocation

		public void TestConstructor_WhenCreatingWithDataRow_ShouldInitializeGeoLocationWithNonNullValue()
		{
			// Arrange.

			// Act.

			var register = Factory.New<OrgColdCallRegister>();

			// Assert.

			var row = ((INeedRow)register).Row;

			AssertEquals(ZGeography.Empty, row[OrgColdCallRegisterSchema.Constants.O1_GeoLocation]);
		}

		public void TestGeoLocation_WhenGettingEmptyValue_ShouldSetItToPointZero()
		{
			// Arrange.

			var register = Factory.New<OrgColdCallRegister>();

			// Act.

			register.O1_GeoLocation = ZGeography.Empty;

			// Assert.

			AssertEquals(ZGeography.Empty, register.O1_GeoLocation);
		}

		#endregion
	}
}
