using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDUnitLineItem))]
	public class CYDUnitLineItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDUnitLineItem>();
		}

		#region TestContainerType

		public void TestContainerType()
		{
			var container = Factory.NewWithValidTestData<RefContainer>();
			var unitLineItem = (CYDUnitLineItem)GetNewBusinessObject();
			unitLineItem.YLI_RC_ContainerType = container.PK;
			AssertEquals(container, unitLineItem.ContainerType);
		}

		#endregion

		#region TestMachineryLineItem

		public void TestMachineryLineItem()
		{
			var machineryLineItem = Factory.NewWithValidTestData<CYDMachineryLineItem>();
			var unitLineItem = (CYDUnitLineItem)GetNewBusinessObject();
			unitLineItem.YLI_YMI_MachineryLineItem = machineryLineItem.PK;
			AssertEquals(machineryLineItem, unitLineItem.MachineryLineItem);
		}

		#endregion

		#region Clone

		public void TestSupportsClone()
		{
			Assert(GetNewBusinessObject().SupportsClone());
		}

		#endregion

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues() => new List<ZString>() { CYDUnitLineItem.Schema.YLI_RX_NKDPPCurrency };
	}
}
