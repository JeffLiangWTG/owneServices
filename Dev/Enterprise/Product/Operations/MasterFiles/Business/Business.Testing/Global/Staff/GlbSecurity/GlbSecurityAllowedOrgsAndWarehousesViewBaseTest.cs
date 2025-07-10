using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class GlbSecurityAllowedOrgsAndWarehousesViewBaseTest : BusinessObjectCollectionViewTestCase<GlbSecurityAllowedOrgsAndWarehousesView>
	{
		#region BusinessObject overrides

		protected override GlbSecurityAllowedOrgsAndWarehousesView GetCollectionToTest()
		{
			Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = SecurityRight;
			return Staff.SecurityAllowedOrgsAndWarehousesView;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_SecurityItemIsAllowed = true;
			security.GU_SecurityRight = SecurityRight;
			security.GU_GS = Staff.PK;
			return security;
		}

		#endregion

		#region Properties

		public void TestSecurityRight()
		{
			AssertEquals("Precondition: ", GlbSecurity.AllowedPrincipalsSecurityRightName, Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight);
			Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = "AAA";
			AssertEquals("AAA", Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight);
		}

		public void TestFindBoxCollectionType()
		{
			AssertEquals("Precondition: ", typeof(ShipsAgencyPrincipalCollection), Staff.SecurityAllowedOrgsAndWarehousesView.FindBoxCollectionType);

			Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			AssertEquals(typeof(WarehouseClientCollection), Staff.SecurityAllowedOrgsAndWarehousesView.FindBoxCollectionType);

			Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			AssertEquals(CargoWise.Application.ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouseCollection>(), Staff.SecurityAllowedOrgsAndWarehousesView.FindBoxCollectionType);

			Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			AssertEquals(typeof(ShipsAgencyPrincipalCollection), Staff.SecurityAllowedOrgsAndWarehousesView.FindBoxCollectionType);

			Staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = "AAA";
			AssertNull("Incorrect SecurityRight", Staff.SecurityAllowedOrgsAndWarehousesView.FindBoxCollectionType);
		}

		protected abstract ZString SecurityRight { get; }

		#endregion

		#region Staff

		protected GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.New<GlbStaff>();
				}
				return staff;
			}
		}

		GlbStaff staff;

		#endregion
	}
}
