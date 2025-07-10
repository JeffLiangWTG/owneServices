using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	public class ContainerStockDataContextManager : EventDataContextManager<RefContainerStock>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.ContainerStock; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.R6_ContainerNum; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var containerStockEventContextReader = new ContainerStockEventContextReader(ParentBO);
				containerStockEventContextReader.AddContainerContextValues(result);
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ContainerStockEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(RefContainerStockSchema.R6_ContainerNum, matchingValues.Key);
		}
	}
}
