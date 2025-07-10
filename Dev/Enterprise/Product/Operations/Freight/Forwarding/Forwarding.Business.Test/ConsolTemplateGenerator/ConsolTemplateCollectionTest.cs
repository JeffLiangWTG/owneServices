using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolTemplateCollection))]
	sealed class ConsolTemplateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ConsolTemplateCollection>
	{
		#region Implementation

		protected override ConsolTemplateCollection GetCollectionToTest()
		{
			return new ConsolTemplateCollection(MultiDaysSelectionForTest);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ConsolTemplate(MultiDaysSelectionForTest);
		}

		MultiDaysSelection MultiDaysSelectionForTest
		{
			get
			{
				var sailings = new JobSailingCollection(Factory);
				sailings.Add(CreateSailing());

				return new MultiDaysSelection(sailings, Factory);
			}
		}

		JobSailing CreateSailing()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "SQ22";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			destination.JB_E_ARV = ZDate.Today.AddDays(1);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		#endregion
	}
}
