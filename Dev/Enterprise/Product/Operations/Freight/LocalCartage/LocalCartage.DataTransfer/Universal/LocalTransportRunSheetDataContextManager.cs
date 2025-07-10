using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.LocalCartage.DataTransfer
{
	public class LocalTransportRunSheetDataContextManager : DataContextManager<CommonWorkSheet>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.LocalTransportRunSheet; }
		}

		public override ZString DataContextKey
		{
			get { return ZString.Empty; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery { IsNoResultQuery = true };
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}
	}
}
