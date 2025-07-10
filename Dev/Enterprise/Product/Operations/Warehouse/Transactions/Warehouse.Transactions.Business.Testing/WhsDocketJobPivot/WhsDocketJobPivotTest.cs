using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketJobPivot))]
	public class WhsDocketJobPivotTest : EnterpriseBusinessObjectTestCase
	{
		#region TestParent

		public void TestParent()
		{
			var pivot = Factory.New<WhsDocketJobPivot>();
			AssertNull(pivot.Parent);

			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			AssertEquals(shipment, pivot.Parent);
		}

		#endregion

		#region TestParentTableCodeConstraint

		public void TestParentTableCodeConstraint_Allow_BH() => AssertTestParentTableCodeConstraint("BH", true);
		public void TestParentTableCodeConstraint_Allow_CH() => AssertTestParentTableCodeConstraint("CH", true);
		public void TestParentTableCodeConstraint_Allow_JD() => AssertTestParentTableCodeConstraint("JD", true);
		public void TestParentTableCodeConstraint_Allow_JE() => AssertTestParentTableCodeConstraint("JE", true);
		public void TestParentTableCodeConstraint_Allow_JS() => AssertTestParentTableCodeConstraint("JS", true);
		public void TestParentTableCodeConstraint_OtherNotAllowed_VV() => AssertTestParentTableCodeConstraint("VV", false);
		public void TestParentTableCodeConstraint_OtherNotAllowed_ZZZ() => AssertTestParentTableCodeConstraint("ZZZ", false);

		public void AssertTestParentTableCodeConstraint(string parentTableCode, bool expectedAllowed)
		{
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentTableCode = parentTableCode;
			pivot.WV_ParentId = ZGuid.NewZGuid();
			pivot.WV_DocketType = DocketType.Codes.Receive;

			if (expectedAllowed)
			{
				AssertNoExceptionThrown(Factory.Save);
			}
			else
			{
				var expectedErrorMsg = "The INSERT statement conflicted with the CHECK constraint \"Constraint_WV_ParentTableCode\".";
				AssertInnermostException("SqlException should throw", typeof(SqlException), expectedErrorMsg, Factory.Save, true);
			}
		}

		#endregion
	}
}
