
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SalesHeaderForTest : SalesHeader
	{
		#region Constructor

		public SalesHeaderForTest(OrgHeader org, OrgSalesProduct salesProduct)
			: this(org, org, salesProduct)
		{
			Argument.NotNull(org, "org");
			Argument.NotNull(salesProduct, "salesProduct");
		}

		public SalesHeaderForTest(OrgHeader org, ISalesValueAssociatedEntity entity, OrgSalesProduct salesProduct)
			: base(GetAllEntitySalesCollectionSorted(entity), new TradedSalesCollection(org), salesProduct, entity)
		{
			Argument.NotNull(org, "org");
			Argument.NotNull(entity, "entity");
			Argument.NotNull(salesProduct, "salesProduct");
		}

		static EntitySalesWrapperCollection GetAllEntitySalesCollectionSorted(ISalesValueAssociatedEntity entity)
		{
			var unSortedResult = new EntitySalesWrapperCollection(entity);
			unSortedResult.Load();
			var result = new EntitySalesWrapperCollection(entity);
			result.AddRange(unSortedResult.ToList().OfType<EntitySalesWrapper>().OrderBy(s => s.OW_SystemCreateTimeUtc));
			return result;
		}

		#endregion
	}
}
