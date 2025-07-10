#if DEBUG
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	public class DummyLandedCostDistributeTo : DummyBusinessObject, ILandedCostDistributeTo
	{
		public DummyLandedCostDistributeTo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ILandedCostDistributeTo Members

		public ZString UniqueCodeExposed;
		public ZString UniqueCode
		{
			get
			{
				return UniqueCodeExposed;
			}
		}

		public ZString TableCodeExposed;
		public ZString TableCode
		{
			get
			{
				return TableCodeExposed;
			}
		}

		public IUltimateDistributee[] UltimateDistributeesExposed;
		public IEnumerable<IUltimateDistributee> UltimateDistributees
		{
			get
			{
				return UltimateDistributeesExposed;
			}
		}

		public ZString DescriptionExposed;
		public ZString Description
		{
			get
			{
				return DescriptionExposed;
			}
		}

		ZGuid ILandedCostDistributeTo.PK
		{
			get { return PK; }
		}

		#endregion
	}
}
#endif
