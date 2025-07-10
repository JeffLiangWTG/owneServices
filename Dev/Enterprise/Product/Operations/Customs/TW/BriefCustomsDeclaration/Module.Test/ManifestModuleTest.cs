using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Module.Testing
{
	[TestedType(typeof(ManifestModule))]
	sealed class ManifestModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckpoint()
		{
			using (var module = new ManifestModuleForTest())
			{
				AssertEquals(Env.Security.AsycudaManifestReporting, module.SecurityCheckpoint);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = new ManifestModuleForTest())
			{
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestAllowNewDeleteEditAndWorkflowTypeAndSupportsWorkflow()
		{
			using (var module = new ManifestModule())
			{
				AssertEquals(true, module.SupportsWorkflow);
				AssertEquals(true, module.AllowDelete);
				AssertEquals(true, module.AllowEdit);
				AssertEquals(true, module.AllowNew);
				AssertEquals(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestWorkflowDescriptor.Constants.Code, module.WorkflowType);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = new ManifestModuleForTest())
			{
				var filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is ASYCUDA.Module.AsycudaFilterStripControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new ManifestModuleForTest())
			{
				var collection = module.NewGridCollection;
				Assert("Invalid type", collection is AsycudaManifestModuleCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new ManifestModuleForTest())
			{
				var filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is ManifestBusinessObject);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.TW.BriefCustomsDeclarations;

		protected override string CountryCode => Core.Constants.CountryCodes.Taiwan;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (AsycudaManifestHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			result.AMA_JobReference = ZString.Empty;
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			return result;
		}
	}

	public class ManifestModuleForTest : ManifestModule
	{
		public ManifestModuleForTest()
		{
		}

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
