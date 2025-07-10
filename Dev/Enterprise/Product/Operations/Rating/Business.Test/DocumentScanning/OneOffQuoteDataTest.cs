using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Business.Testing
{
	public class OneOffQuoteDataTest : TestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			var assemblyData = new OneOffQuoteData();
			AssertEquals(ObjectFactory.GetType<IViewQuotedBooking>(), assemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			var assemblyData = new OneOffQuoteData();
			AssertNotNull(assemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(ObjectFactory.GetType<IViewOneOffQuoteCollection>(), assemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var assemblyData = new OneOffQuoteData();
			AssertEquals(ModuleIDs.OneOffQuotes, assemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			var assemblyData = new OneOffQuoteData();
			AssertEquals(Core.Constants.ReferenceTypes.ClientSupplierRelationship, assemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			var assemblyData = new OneOffQuoteData();
			AssertEquals(true, assemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

	}
}
