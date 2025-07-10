using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(ProofOfPaymentModule))]
	sealed class ProofOfPaymentModuleTest : ZModuleBasherTest
	{
		public void TestSendProofOfPaymentsMenu()
		{
			using (ProofOfPaymentModule module = new ProofOfPaymentModule())
			{
				var actionMenuItem = ((ZDisplayGrid)module.DisplayGrid).ContextMenu.MenuItems.FindByText("Actions");
				actionMenuItem.OnPopup(EventArgs.Empty);
				MenuItem menuItem = actionMenuItem.MenuItems.FindByText("Send Proof Of Payments Document");
				AssertNotNull(menuItem);
			}
		}

		public void TestGridCollection()
		{
			using (var module = new ProofOfPaymentModule())
			{
				var collection = module.GridCollection as ModuleCusEntryPayInfoCollection;
				AssertNotNull(collection);
				var filterDefaults = collection.FilterBusinessObjectDefaults;
				AssertEquals(1, filterDefaults.Count);
				AssertEquals(ZDecimal.Zero, filterDefaults[ProofOfPaymentFilterBusinessObject.FilterConstants.VATAmount + ":GreaterThanDefaultProperty"].Value);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ZAModuleIDs.ZA404ProofOfPayment;

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;
	}
}
