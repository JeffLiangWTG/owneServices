using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Module.Tests
{
	[TestedType(typeof(GenShapeGeographyModule))]
	public class GenShapeGeographyModuleTest : ZModuleBasherTest
	{
		#region CheckPoints

		public void TestLicenseCheckPoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckPoint()
		{
			AssertEquals(Env.Security.Geography, Module.SecurityCheckpoint);
		}

		#endregion

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (GenShapeGeographyModuleForTest module = new GenShapeGeographyModuleForTest())
			using (IFilterControl filterControl = module.GetNewFilterControl_Exposed())
			{
				AssertEquals("Filter control should be a GenShapeGeographyFilterControl", true, filterControl is GenShapeGeographyFilterControl);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (GenShapeGeographyModuleForTest module = new GenShapeGeographyModuleForTest())
			{
				IBusinessObjectCollection geographiesCollection = module.GetNewGridCollection_Exposed();
				AssertEquals("Filter grid collection should be a GenShapeGeographyCollection", true, geographiesCollection is GenShapeGeographyCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (GenShapeGeographyModuleForTest module = new GenShapeGeographyModuleForTest())
			{
				FilterBusinessObject filterBizo = module.GetNewFilterBusinessObject_Exposed();
				AssertEquals("Filter bizo should be a GenShapeGeographyFilterBusinessObject", true, filterBizo is GenShapeGeographyFilterBusinessObject);
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.GenShapeGeography;

		protected virtual GenShapeGeographyModule Module
		{
			get
			{
				if (module == null)
				{
					module = (GenShapeGeographyModule)ZModuleFactory.Instance.Create(ModuleIDs.GenShapeGeography);
				}
				return module;
			}
		}
		protected GenShapeGeographyModule module;

		protected override void TearDown()
		{
			base.TearDown();
			if (module != null)
			{
				module.Dispose();
			}
		}

		#endregion
	}

	#region GenShapeGeographyModuleForTest

	public class GenShapeGeographyModuleForTest : GenShapeGeographyModule
	{
		public GenShapeGeographyModuleForTest()
		{
		}

		public IFilterControl GetNewFilterControl_Exposed() => GetNewFilterControl();

		public IBusinessObjectCollection GetNewGridCollection_Exposed() => GetNewGridCollection();

		public FilterBusinessObject GetNewFilterBusinessObject_Exposed() => GetNewFilterBusinessObject();
	}

	#endregion
}
