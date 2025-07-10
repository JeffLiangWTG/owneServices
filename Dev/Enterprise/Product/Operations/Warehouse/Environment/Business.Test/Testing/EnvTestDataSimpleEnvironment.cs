using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class EnvTestDataSimpleEnvironment : Assertion
	{
		public EnvTestDataSimpleEnvironment(BusinessObjectFactory factory, bool saveFactory_doNotUseForNewTests = true)
			: this(factory, 1, 1, saveFactory_doNotUseForNewTests)
		{
		}

		public EnvTestDataSimpleEnvironment(BusinessObjectFactory factory, short cols, short levels, bool saveFactory_doNotUseForNewTests = true)
		{
			this.Factory = factory;
			this.cols = cols;
			this.levels = levels;
			CreateEnvironment();
			if (saveFactory_doNotUseForNewTests)
			{
				factory.Save();
			}
		}

		protected virtual void CreateEnvironment()
		{
			Whs1 = Helper.CreateWarehouse("1", "A", cols, levels);
			Org1 = Helper.CreateClient("111", "111");
			Part1 = Helper.CreateProduct(Org1, "P1");
			Part2 = Helper.CreateProduct(Org1, "P2");
		}

		public WhsWarehouse Whs1;
		public OrgHeader Org1;
		public OrgSupplierPart Part1;
		public OrgSupplierPart Part2;
		readonly short cols;
		readonly short levels;

		WhsTestHelperFunctionsEnv helper;

		protected WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}
		protected BusinessObjectFactory Factory;
	}
}
