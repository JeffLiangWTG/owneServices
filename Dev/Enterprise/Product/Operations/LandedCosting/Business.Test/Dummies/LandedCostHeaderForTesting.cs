using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandedCostHeaderForTesting : LandedCostHeader
	{
		public LandedCostHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Overrides of LandedCostHeader

		protected override TypeLoaderCollection GetParentLoaders()
		{
			var loaders = new TypeLoaderCollection();
			loaders.Add(new TypeLoader(typeof(DummyLandedCostHeader)));
			loaders.Add(new TypeLoader(typeof(DummyLandedCostHeader), JobDeclarationSchema.Constants.Prefix));
			loaders.Add(new TypeLoader(typeof(DummyLandedCostHeader), JobOrderHeaderSchema.Constants.Prefix));

			return loaders;
		}

		protected override TypeLoaderCollection GetParentLoaderForLandedCostInputDistributeTo()
		{
			var result = new TypeLoaderCollection();
			result.Add(new TypeLoader(typeof(DummyLandedCostDistributeTo)));
			result.Add(new TypeLoader(typeof(DummyLandedCostDistributeTo), JobOrderLineSchema.Constants.Prefix));

			return result;
		}

		#endregion
	}
}
