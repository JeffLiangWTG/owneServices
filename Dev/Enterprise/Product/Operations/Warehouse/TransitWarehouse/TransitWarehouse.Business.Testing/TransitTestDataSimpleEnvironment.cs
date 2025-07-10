using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TransitTestDataSimpleEnvironment : Assertion
	{
		#region Constructors

		public TransitTestDataSimpleEnvironment(BusinessObjectFactory factory)
			: this(factory, 1, 1)
		{
		}

		public TransitTestDataSimpleEnvironment(BusinessObjectFactory factory, short cols, short levels)
		{
			Factory = factory;
			Cols = cols;
			Levels = levels;
		}

		#endregion

		#region Whs1

		public WhsWarehouse Whs1
		{
			get
			{
				if (whs1 == null)
				{
					whs1 = Helper.CreateWarehouse("TW1", "A", Cols, Levels);
					whs1.WW_WarehouseType = WarehouseTypes.Codes.Transit;
					whs1.WW_GB_RelatedCompanyBranch = GetOrCreateBranch(Whs1.WW_WarehouseCode).PK;

					Factory.Save();
				}

				return whs1;
			}
		}
		GlbBranch GetOrCreateBranch(ZString code)
			=> Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, code)
				?? GetNewBranch(code);

		GlbBranch GetNewBranch(ZString code)
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = code;
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			return branch;
		}

		WhsWarehouse whs1;
		readonly short Cols;
		readonly short Levels;

		#endregion

		#region Org1

		public OrgHeader Org1
		{
			get { return org1 ?? (org1 = Helper.CreateClient("ORG1", "Org1")); }
		}

		OrgHeader org1;

		#endregion

		#region Org2

		public OrgHeader Org2
		{
			get { return org2 ?? (org2 = Helper.CreateClient("ORG2", "Org2")); }
		}

		OrgHeader org2;

		#endregion

		#region Org3

		public OrgHeader Org3
		{
			get { return org3 ?? (org3 = Helper.CreateClient("ORG3", "Org3")); }
		}

		OrgHeader org3;

		#endregion

		#region org4

		public OrgHeader Org4
		{
			get { return org4 ?? (org4 = Helper.CreateClient("ORG4", "Org4")); }
		}

		OrgHeader org4;

		#endregion

		#region org5

		public OrgHeader Org5
		{
			get { return org5 ?? (org5 = Helper.CreateClient("ORG5", "Org5")); }
		}

		OrgHeader org5;

		#endregion

		#region Part1

		public OrgSupplierPart Part1
		{
			get { return part1 ?? (part1 = Helper.CreateProduct("TP1", Org1)); }
		}

		OrgSupplierPart part1;

		#endregion

		#region Part2

		public OrgSupplierPart Part2
		{
			get { return part2 ?? (part2 = Helper.CreateProduct("TP2", Org1)); }
		}

		OrgSupplierPart part2;

		#endregion

		#region Implementation

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}

		protected WhsTransitTestHelper helper;

		protected BusinessObjectFactory Factory;

		#endregion
	}
}
