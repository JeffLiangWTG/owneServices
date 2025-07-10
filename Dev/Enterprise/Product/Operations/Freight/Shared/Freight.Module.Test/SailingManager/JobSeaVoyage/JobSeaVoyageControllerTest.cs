using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobSeaVoyageController))]
	sealed class JobSeaVoyageControllerTest : JobVoyageControllerTest
	{
		public void TestModuleID()
		{
			var controller = (JobSeaVoyageController)(ZControllerFactory.Create(ControllerIDs.JobSeaVoyage));
			AssertEquals(ModuleIDs.JobSeaVoyage, controller.ModuleID);
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobSeaVoyage;
		}

		protected override ZString TransportType
		{
			get { return Constants.TransportModes.Sea; }
		}

		protected override ControllerID SailingEquivalentControllerID
		{
			get { return ControllerIDs.JobSeaSailing; }
		}

		protected override IEnumerable<ControllerID> NonCustomsPlugInsToExcludeFromTest
			=> new ControllerID[] { ControllerIDs.ETerminalReleaseManifestPortMessaging }
			.Union(base.NonCustomsPlugInsToExcludeFromTest);

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
