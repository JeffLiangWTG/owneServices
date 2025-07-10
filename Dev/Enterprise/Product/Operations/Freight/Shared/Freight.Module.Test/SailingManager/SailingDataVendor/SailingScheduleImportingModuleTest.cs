using System;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Module.Testing
{
	[TestedType(typeof(SailingScheduleImportingModuleForTest))]
	sealed class SailingScheduleImportingModuleTest : ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.SailingDataVendorImporting, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow/*.OneStopVesselIntegration*/, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.SailingScheduleImporting, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(SailingScheduleImportingController), Module.GetNewController().GetType());
		}

		#region Test Classes

		class SailingScheduleImportingModuleForTest : SailingScheduleImportingModule
		{
			public new ZPopupController GetNewController()
			{
				return base.GetNewController();
			}
		}

		#endregion

		#region Implementation

		SailingScheduleImportingModuleForTest Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new SailingScheduleImportingModuleForTest();
				}
				return fModule;
			}
		}
		SailingScheduleImportingModuleForTest fModule;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SailingDataVendorImporting;
		}

		protected override void SetUp()
		{
			base.SetUp();
			countryChange = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia);
		}
		protected override void TearDown()
		{
			countryChange.Dispose();
			base.TearDown();
			if (fModule != null)
			{
				fModule.Dispose();
			}
		}

		IDisposable countryChange;

		#endregion
	}
}
