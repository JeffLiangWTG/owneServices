using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	public class MAWBUserControlTest : TestCaseWithFactory
	{
		public void TestSecurityStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_SG.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var userControl = new MAWBUserControlForTest())
			{
				AssertEquals("EH_KnownConsignorCodeTextBox should be Invisible", false, userControl.EH_KnownConsignorCodeTextBoxForTesting.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_SG.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var userControl = new MAWBUserControlForTest())
			{
				AssertEquals("EH_KnownConsignorCodeTextBox should be Invisible", false, userControl.EH_KnownConsignorCodeTextBoxForTesting.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_SG.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var userControl = new MAWBUserControlForTest())
			{
				AssertEquals("EH_KnownConsignorCodeTextBox should be Visible", true, userControl.EH_KnownConsignorCodeTextBoxForTesting.Visible);
			}
		}

		#region MAWBUserControlForTest

		class MAWBUserControlForTest : MAWBUserControl
		{
			public ZTextBox EH_KnownConsignorCodeTextBoxForTesting
			{
				get { return base.EH_KnownConsignorCodeTextBox; }
			}
		}

		#endregion
	}
}
