using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WarningAcknowledgementModule))]
	sealed class WarningAcknowledgementModuleTest : ZModuleBasherTest
	{
		public void TestCheckpoints()
		{
			using (var module = new WarningAcknowledgementModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.WarningAcknowledgement, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		[StressTest]
		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			var warning = Factory.NewWithValidTestData<GenCustomAddOnRuleAck>();
			warning.XK_ParentID = Guid.NewGuid();
			warning.XK_ParentTableCode = "GS";
			warning.XK_SystemCreateTimeUtc = DateTime.UtcNow;
			warning.XK_SystemCreateUser = "E";
			warning.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation;

			Factory.Save();

			base.TestModuleShowsAndCanSearch();
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new WarningAcknowledgementModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is WarningAcknowledgementControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new WarningAcknowledgementModuleForTest())
			{
				IBusinessObjectCollection warningCollection = module.NewGridCollection;
				Assert("Invalid type", warningCollection is GenCustomAddOnRuleAckCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new WarningAcknowledgementModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is WarningAcknowledgementFilterBusinessObject);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WarningAcknowledgement;
		}

		sealed class WarningAcknowledgementModuleForTest : WarningAcknowledgementModule
		{
			public WarningAcknowledgementModuleForTest() { }

			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return GetNewGridCollection(); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}
		}
	}
}
