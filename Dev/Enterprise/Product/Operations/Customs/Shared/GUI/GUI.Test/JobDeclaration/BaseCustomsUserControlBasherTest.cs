using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	/// <summary>
	/// This Test class is for Developer Only test of specific usercontrols as using the default Declaration's Form Bashing is taking too long
	/// </summary>
	public abstract class BaseCustomsUserControlBasherTest : BaseCustomsFormBasherTest
	{
		[DeveloperOnlyTest]
		public override void TestBindingAllTabsOnIdle()
		{
			Assert(true);
		}

		[DeveloperOnlyTest]
		public override void TestHasChangesOnPreviouslySavedObject()
		{
			Assert(true);
		}

		[DeveloperOnlyTest]
		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}

		[DeveloperOnlyTest]
		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true);
		}

		[DeveloperOnlyTest]
		public override void TestBashingForm()
		{
			base.TestBashingForm();
		}

		[DeveloperOnlyTest]
		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			base.TestBoundListsAreNotLoadedOnAccess();
		}

		protected override void BashControl(Control controlToBash)
		{
			bool isExpectedControl = controlToBash.GetType() == UserControlToBashType;
			if (isExpectedControl)
			{
				shouldBash = true;
			}
			base.BashControl(controlToBash);
			if (isExpectedControl)
			{
				shouldBash = false;
			}
		}

		protected override BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = MessageTypeForFormBashing;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			return declaration;
		}

		protected abstract Type UserControlToBashType { get; }

		protected override IControlBasher[] GetControlBashers(Control control)
		{
			return shouldBash ? base.GetControlBashers(control) : Array.Empty<IControlBasher>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			shouldBash = false;
		}

		bool shouldBash;
	}
}
