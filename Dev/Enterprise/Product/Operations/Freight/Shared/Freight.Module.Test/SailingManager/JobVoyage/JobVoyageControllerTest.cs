using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	public abstract class JobVoyageControllerTest : ZControllerBasherTest
	{
		public void TestUseSameCheckPointsAsJobSailing()
		{
			ZController voyageController = ZControllerFactory.Create(GetControllerID());
			ZController sailingController = ZControllerFactory.Create(SailingEquivalentControllerID);

			string[] propertyNames = new string[]
			{
				"CheckPointForView",
				"CheckPointForNew",
				"CheckPointForEdit",
				"CheckPointForDelete",
			};

			foreach (string propertyName in propertyNames)
			{
				SecurityCheckpoint expected = (SecurityCheckpoint)typeof(ZController).InvokeMember(propertyName, BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, sailingController, System.Array.Empty<object>());
				SecurityCheckpoint actual = (SecurityCheckpoint)typeof(ZController).InvokeMember(propertyName, BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, voyageController, System.Array.Empty<object>());
				AssertEquals(propertyName, expected, actual);
			}
		}

		[GuiTest]
		public void TestNewVoyagesType()
		{
			ZController controller = ZControllerFactory.Create(GetControllerID());

			using (ZForm form = (ZForm)controller.ShowNewForm())
			{
				JobVoyage voyage = (JobVoyage)form.BusinessEntity;
				AssertEquals(TransportType, voyage.JV_AirSeaRoad);
			}
		}

		#region Implementation

		protected abstract ZString TransportType { get; }

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportType;
			Factory.Save();
			return voyage;
		}

		protected abstract ControllerID SailingEquivalentControllerID { get; }

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var voyage = base.GetBusinessObjectWithoutValidationErrors() as JobVoyage;
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-10);
			voyage.Origins.Add(origin);
			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations.Add(destination);
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel1";
			var line = Factory.New<OrgHeader>();
			line.OH_Code = "RAILLINE";
			line.OH_IsShippingProvider = true;
			line.OH_IsShippingLine = true;
			vessel.RV_OH = line.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";
			return voyage;
		}

		#endregion
	}
}
