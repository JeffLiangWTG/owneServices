using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class ConsolDataContextManager<TConsol, TShipment, TContainer> : ShipmentDataContextManager<TConsol>
		where TConsol : CommonConsol
		where TShipment : CommonShipment
		where TContainer : CommonContainer
	{
		public override ZString DataContextKey
		{
			get { return ParentBO.JK_UniqueConsignRef; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var result = new ZQuery(JobConsolSchema.JK_UniqueConsignRef, matchingValues.Key);
			result.AddToFilter(JobConsolSchema.JK_IsCancelled, false);

			return result;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var consolEventContextReader = new CommonConsolEventContextReader(ParentBO);
				consolEventContextReader.AddConsolContextValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ConsolEventParentFinder<TConsol, TShipment, TContainer>(factory, this, logger, Helper);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected IUniversalFreightHelper Helper
		{
			get { return helper ?? (helper = GetNewHelper()); }
		}

		IUniversalFreightHelper helper;

		protected abstract IUniversalFreightHelper GetNewHelper();
	}
}
