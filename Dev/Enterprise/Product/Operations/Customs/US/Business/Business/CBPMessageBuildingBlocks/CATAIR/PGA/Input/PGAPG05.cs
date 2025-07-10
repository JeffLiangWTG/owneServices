using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public class PGAPG05 : Messaging.Business.MessageBuildingBlocks.ACE.Input.AEPAPG05, IBIRDPGAScientificRecord
	{
		#region IBIRDPGAScientificRecord Members

		void IBIRDPGAScientificRecord.Update(ConstituentElement constituentElement, INotifications notifications)
		{
			ScientificData scientificData = constituentElement.ScientificDataCollection.AddNew();

			scientificData.US_PGAScientificGenusName = ScientificGenusName;
			scientificData.US_PGAScientificSpeciesName = ScientificSpeciesName;
		}

		#endregion
	}
}
