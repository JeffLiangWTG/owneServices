using CargoWise.Common;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class GenericConsolHelper
	{
		/// <summary>
		/// This method is for getting DataContextType enum object and to prevent multiple changing necessities.
		/// Update switch case block if ErrorReporter reports a missing context type.
		/// </summary>
		/// <param name="parentTableCode"></param>
		/// <returns>DataContextType? DataContextType</returns>
		public static DataContextType? GetDataContextTypeByParentTableCode(string parentTableCode)
		{
			switch (parentTableCode)
			{
				case JobConsolSchema.Constants.Prefix:
					return DataContextType.ForwardingConsol;

				case DtbBookingConsolidationSchema.Constants.Prefix:
					return DataContextType.TransportBookingConsolidation;

				case DtbBookingSchema.Constants.Prefix:
					return DataContextType.TransportBooking;

				case DtbConsignmentRunSheetSchema.Constants.Prefix:
					return DataContextType.TransportConsignmentRunSheet;

				case JobCartageRunSheetSchema.Constants.Prefix:
					return DataContextType.LocalTransportRunSheet;

				case WhsItemReceiveTransportationUnitSchema.Constants.Prefix:
					return DataContextType.TransitReceiveHeader;

				case WhsItemDispatchLoadListSchema.Constants.Prefix:
					return DataContextType.TransitDispatchLoadList;

				case WhsItemDispatchTransportationUnitSchema.Constants.Prefix:
					return DataContextType.TransitDispatchHeader;

				default:
					ErrorReporter.ReportOnce($"NotSupportedDataContextType-'{parentTableCode}'", $"Cannot describe 'DataContextType' because VX_ParentTableCode '{parentTableCode}' is undefined. Refer to 'GenericConsolHelper.cs' class for details.");
					return null;
			}
		}
	}
}
