using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(USInBondMoveHeaderOperationalActionSupporter))]
	sealed class USInBondMoveHeaderOperationalActionSupporterTest : OperationalActionSupporterTest<USInBondMoveHeaderOperationalActionSupporter>
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CusInBondHeader, Supporter.BusinessContext);
		}

		public void TestRootType()
		{
			AssertEquals(typeof(USInBondMoveHeader), Supporter.RootType);
		}

		#region Implementation

		public override BusinessObject NewTarget()
		{
			var header = Factory.New<CusInBondHeader>();
			var movement = header.MovementHeaders.AddNew();
			Factory.Save();

			return Factory.Load<USInBondMoveHeader>(movement.PK);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.InBondMoveHeader;

		#endregion
	}
}
