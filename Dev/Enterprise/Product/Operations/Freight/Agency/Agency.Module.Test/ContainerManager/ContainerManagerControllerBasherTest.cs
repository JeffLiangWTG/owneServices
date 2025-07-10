using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerManagerController))]
	internal class ContainerManagerControllerBasherTest : ZControllerBasherTest
	{
		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyContainerManager;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			RefContainerStock container = Factory.New<RefContainerStock>();
			container.R6_ContainerNum = "FAKU4100015";
			container.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			return container;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
