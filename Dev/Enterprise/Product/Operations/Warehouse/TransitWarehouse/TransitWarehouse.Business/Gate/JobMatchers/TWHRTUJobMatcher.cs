using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHRTUJobMatcher : TWHJobMatcher
	{
		public TWHRTUJobMatcher(BusinessObjectFactory factory, WhsWarehouse trw, string referenceNumber, ReferenceNumberTypes referenceNumberType)
		{
			Factory = factory;
			TransitWarehouse = trw;
			ReferenceNumber = referenceNumber;
			ReferenceNumberType = referenceNumberType;
		}

		protected override TWHJobMatcherResult InternalProcess()
		{
			var rtu = FindRTU(TransitWarehouse, ReferenceNumberType, ReferenceNumber);
			if (rtu != null)
			{
				var packageStates = TWHJobValidationHelper.GetValidPackageStates(rtu.BookedPackagesInPendingASNs);
				var packageStateTotals = TWHJobValidationHelper.GetTotalsOfPackageStates(packageStates);

				if (rtu.WRH_UnitType == TransportUnitTypes.Vehicle)
				{
					ReferenceNumberType = ReferenceNumberTypes.VehicleReference;
				}
				else if (rtu.WRH_UnitType == TransportUnitTypes.Container || rtu.WRH_UnitType == TransportUnitTypes.ULD)
				{
					ReferenceNumberType = ReferenceNumberTypes.ContainerNumber;
				}

				var errorMessage = TWHJobValidationHelper.ValidateDGPackageNotExceedCore(packageStates, TransitWarehouse);
				if (errorMessage != null)
				{
					return new TWHJobMatcherResult()
					{
						ReferenceNumber = ReferenceNumber,
						ReferenceNumberType = ReferenceNumberType,
						ErrorCode = ValidationErrorCode.VDG,
						ErrorMessage = errorMessage
					};
				}

				return new TWHJobMatcherResult()
				{
					ReferenceNumber = ReferenceNumber,
					ReferenceNumberType = ReferenceNumberType,
					PackageStates = packageStates,
					GrossWeightValue = packageStateTotals.totalWeightInKG,
					GrossWeightUnit = Core.Constants.Weight.Kilograms,
					GrossVolumeValue = packageStateTotals.totalVolumeInM3,
					GrossVolumeUnit = Core.Constants.Volume.CubicMetres,
					QuantityValue = packageStateTotals.totalQuantity
				};
			}
			else
			{
				return new TWHJobMatcherResult()
				{
					ReferenceNumber = ReferenceNumber,
					ReferenceNumberType = ReferenceNumberType,
					ErrorCode = ValidationErrorCode.JNF
				};
			}
		}

		protected override bool IsReferenceTypeMatch() =>
			ReferenceNumberType == ReferenceNumberTypes.VehicleReference
			|| ReferenceNumberType == ReferenceNumberTypes.ContainerNumber
			|| ReferenceNumberType == ReferenceNumberTypes.Unknown;

		WhsItemReceiveTransportationUnit FindRTU(WhsWarehouse trw, ReferenceNumberTypes referenceNumberType, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference, reference);
			query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse, trw.PK);
			query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_GateInTime, null);
			if (referenceNumberType == ReferenceNumberTypes.ContainerNumber)
			{
				query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_UnitType, new string[] { TransportUnitTypes.Container, TransportUnitTypes.ULD });
			}
			else if (referenceNumberType == ReferenceNumberTypes.VehicleReference)
			{
				query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_UnitType, TransportUnitTypes.Vehicle);
			}
			query.OrderBy = WhsItemReceiveTransportationUnitSchema.Constants.WRH_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemReceiveTransportationUnit>(query);
		}
	}
}
