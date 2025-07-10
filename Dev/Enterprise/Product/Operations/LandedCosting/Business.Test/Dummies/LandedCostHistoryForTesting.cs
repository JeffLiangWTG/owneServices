using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandedCostHistoryForTesting : LandedCostHistory
	{
		public LandedCostHistoryForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Overrides of LandedCostHistory

		protected override TypeLoaderCollection GetUltimateDistributeeLoaders()
		{
			var loaders = new TypeLoaderCollection();
			loaders.Add(new TypeLoader(typeof(DummyIUltimateDistributee)));
			loaders.Add(new TypeLoader(typeof(DummyIUltimateDistributee), JobComInvoiceLineSchema.Constants.Prefix));
			loaders.Add(new TypeLoader(typeof(DummyIUltimateDistributee), JobOrderLineSchema.Constants.Prefix));
			return loaders;
		}

		#endregion
	}
}
