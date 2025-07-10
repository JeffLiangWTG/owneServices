using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Summary description for PackLineNonDependentCollection.
	/// </summary>
	public class PackLineNonDependentCollection : BusinessObjectCollection<PackLine>, IPackLineCollection
	{
		public PackLineNonDependentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public PackLineNonDependentCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		#region IPackLineCollection Members

		protected PackLineCollectionCalculator fTotals;
		public PackLineCollectionCalculator Totals
		{
			get
			{
				if (fTotals == null)
				{
					fTotals = new PackLineCollectionCalculator(this, PackLineCollectionCalculator.FilterMode.NoFilter);
				}
				return fTotals;
			}
		}

		#endregion

		#region IPackLineCollection Members

		ZString IPackLineCollection.MasterPackagesUnit
		{
			get { return FreightPacksDataRegistry.Instance.OuterPackUnit.Value; }
		}

		ZString IPackLineCollection.MasterWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		ZString IPackLineCollection.MasterVolumeUnit
		{
			get { return Env.Registry.FreightVolumeUnit; }
		}

		#endregion
	}

	public class InternalPackLineNonDependentCollection : PackLineNonDependentCollection
	{
		public InternalPackLineNonDependentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public InternalPackLineNonDependentCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
