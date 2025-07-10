using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GenericConsolHelperTest : TestCaseWithFactory
	{
		public void TestGetDataContextTypeByParentTableCode()
		{
			AssertEquals("VX_ParentTableCode is 'JK'", DataContextType.ForwardingConsol, GenericConsolHelper.GetDataContextTypeByParentTableCode(JobConsolSchema.Constants.Prefix));
			AssertEquals("VX_ParentTableCode is 'KB'", DataContextType.TransportBookingConsolidation, GenericConsolHelper.GetDataContextTypeByParentTableCode(DtbBookingConsolidationSchema.Constants.Prefix));
			AssertEquals("VX_ParentTableCode is 'KM'", DataContextType.TransportBooking, GenericConsolHelper.GetDataContextTypeByParentTableCode(DtbBookingSchema.Constants.Prefix));
			AssertEquals("VX_ParentTableCode is 'KG'", DataContextType.TransportConsignmentRunSheet, GenericConsolHelper.GetDataContextTypeByParentTableCode(DtbConsignmentRunSheetSchema.Constants.Prefix));
			AssertEquals("VX_ParentTableCode is 'EY'", DataContextType.LocalTransportRunSheet, GenericConsolHelper.GetDataContextTypeByParentTableCode(JobCartageRunSheetSchema.Constants.Prefix));
			AssertEquals("VX_ParentTableCode is 'WRH'", DataContextType.TransitReceiveHeader, GenericConsolHelper.GetDataContextTypeByParentTableCode(WhsItemReceiveTransportationUnitSchema.Constants.Prefix));
			AssertEquals("VX_ParentTableCode is 'WDH'", DataContextType.TransitDispatchHeader, GenericConsolHelper.GetDataContextTypeByParentTableCode(WhsItemDispatchTransportationUnitSchema.Constants.Prefix));
			AssertEquals("VX_ParentTableCode is 'WDL'", DataContextType.TransitDispatchLoadList, GenericConsolHelper.GetDataContextTypeByParentTableCode(WhsItemDispatchLoadListSchema.Constants.Prefix));

			var exceptions = ErrorReporter.LastExceptionsReported();
			AssertEquals("No existing exception", 0, exceptions.Count);

			AssertNull("VX_ParentTableCode is unknown", GenericConsolHelper.GetDataContextTypeByParentTableCode("Boo"));

			exceptions = ErrorReporter.LastExceptionsReported();
			Assert(exceptions.Exists(ex => ex.Contains("Cannot describe 'DataContextType' because VX_ParentTableCode 'Boo' is undefined. Refer to 'GenericConsolHelper.cs' class for details.")));

			ErrorReporter.Clear();
		}
	}
}
