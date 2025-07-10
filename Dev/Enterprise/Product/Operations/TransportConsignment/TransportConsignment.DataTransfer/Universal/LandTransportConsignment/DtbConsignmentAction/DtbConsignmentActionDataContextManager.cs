using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbConsignmentActionDataContextManager : EventDataContextManager<DtbConsignmentAction>
	{
		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.LandTransportConsignmentAction; }
		}

		#endregion

		#region DataContextKey

		public override ZString DataContextKey
		{
			get { return ""; }
		}

		#endregion

		#region DefaultOutputDirectory

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region GetDataContextKeyMatchingQuery

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery { IsNoResultQuery = true };
		}

		#endregion

		#region GetEventContextValues

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		#endregion

		#region GetEventParentFinder

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DtbConsignmentActionParentEventFinder(this, factory, logger);
		}

		#endregion
	}
}
