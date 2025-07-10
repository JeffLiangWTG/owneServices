using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business.Testing
{
	class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override BondedFactoryCollection BondedFactoriesCore
		{
			get
			{
				if (bondedFactoryJobDocAddresses == null)
				{
					bondedFactoryJobDocAddresses = new BondedFactoryCollectionForTest(this);
					RegisterEditableChildObject(bondedFactoryJobDocAddresses);
				}

				return bondedFactoryJobDocAddresses;
			}
		}
	}
}
