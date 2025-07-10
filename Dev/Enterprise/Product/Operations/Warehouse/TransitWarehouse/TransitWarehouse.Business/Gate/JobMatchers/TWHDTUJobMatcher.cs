using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHDTUJobMatcher : TWHJobMatcher
	{
		public TWHDTUJobMatcher(BusinessObjectFactory factory, WhsWarehouse trw, string referenceNumber, ReferenceNumberTypes referenceNumberType)
		{
			Factory = factory;
			TransitWarehouse = trw;
			ReferenceNumber = referenceNumber;
			ReferenceNumberType = referenceNumberType;
		}

		protected override TWHJobMatcherResult InternalProcess()
		{
			var dtu = FindDTU(TransitWarehouse, ReferenceNumberType, ReferenceNumber);
			if (dtu != null)
			{
				var packageStates = TWHJobValidationHelper.GetValidPackageStates(dtu.ValidPackageStatesInPendingDLLs);
				var packageStateTotals = TWHJobValidationHelper.GetTotalsOfPackageStates(packageStates);

				if (dtu.WDH_UnitType == TransportUnitTypes.Vehicle)
				{
					ReferenceNumberType = ReferenceNumberTypes.VehicleReference;
				}
				else if (dtu.WDH_UnitType == TransportUnitTypes.Container || dtu.WDH_UnitType == TransportUnitTypes.ULD)
				{
					ReferenceNumberType = ReferenceNumberTypes.ContainerNumber;
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

		WhsItemDispatchTransportationUnit FindDTU(WhsWarehouse trw, ReferenceNumberTypes referenceNumberType, string reference)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
			query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference, reference);
			query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse, trw.PK);
			query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_GateInTime, null);
			if (referenceNumberType == ReferenceNumberTypes.ContainerNumber)
			{
				query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_UnitType, new string[] { TransportUnitTypes.Container, TransportUnitTypes.ULD });
			}
			else if (referenceNumberType == ReferenceNumberTypes.VehicleReference)
			{
				query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_UnitType, TransportUnitTypes.Vehicle);
			}
			query.OrderBy = WhsItemDispatchTransportationUnitSchema.Constants.WDH_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<WhsItemDispatchTransportationUnit>(query);
		}
	}
}
