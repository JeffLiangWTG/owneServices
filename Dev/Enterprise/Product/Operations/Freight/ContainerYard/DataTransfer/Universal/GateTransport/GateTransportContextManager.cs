using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.ContainerYard.Business
{
	public sealed class GateTransportContextManager : EventDataContextManager<GateTransport>
	{
		public override DataContextType DataContextType => DataContextType.GateTransport;

		public override ZString DataContextKey => ParentBO.GTT_JobNumber;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(GateTransportSchema.GTT_JobNumber, matchingValues.Key);
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				if (ParentBO.GateTransportCFSDetails.Count > 0)
				{
					foreach (var cfsDetail in ParentBO.GateTransportCFSDetails)
					{
						var unitNumber = cfsDetail.GateBookingDetail?.YardUnit?.GTY_UnitNumber;
						if (unitNumber.HasValue)
						{
							result.AddIfNotEmpty(Event.ContextTypes.ContainerNumber, unitNumber);
						}
					}
				}
				result.AddIfNotEmpty(Event.ContextTypes.TransportReference, ParentBO.GTT_VehicleRegistration);
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new GateTransportEventParentFinder(factory, this, logger);
		}
	}
}
