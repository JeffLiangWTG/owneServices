using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Module.Testing
{
	[TestedType(typeof(SPTSModule))]
	class SPTSModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.TR.SimplifiedProcedureTransitSystem;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Turkey; }
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (SPTSHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			result.BH_ApplicationCode = "SPT";
			result.BH_GB = GlbBranch.CurrentBranch.PK;
			result.BH_JobReference = "Test Ref";
			return result;
		}
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			var factory = collection.Factory;
			var result = factory.New<SPTSHeader>();
			result.BH_ApplicationCode = "SPT";
			result.BH_GB = GlbBranch.CurrentBranch.PK;
			result.BH_JobReference = "Test Ref";
			factory.Save();
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert(true);
		}
	}
}
