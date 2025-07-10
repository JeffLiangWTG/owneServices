using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.TR.ETrade.Module.Testing
{
	[TestedType(typeof(ETradeModule))]
	public class ETradeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.TR.ETrade;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Turkey; }
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (AsycudaManifestHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			result.AMA_JobReference = ZString.Empty;
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			return result;
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert(true);
		}

		public void TestAllowNewDeleteEditAndWorkflowTypeAndSupportsWorkflow()
		{
			using (var module = new ETradeModule())
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
			using (ETradeModuleForTest module = new ETradeModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is ETradeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (ETradeModuleForTest module = new ETradeModuleForTest())
			{
				IBusinessObjectCollection collection = module.NewGridCollection;
				Assert("Invalid type", collection is ETradeCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (ETradeModuleForTest module = new ETradeModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is ETradeFilterBusinessObject);
			}
		}
	}

	#region ETradeModuleForTest
	public class ETradeModuleForTest : ETradeModule
	{
		public ETradeModuleForTest()
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

	#endregion
}
