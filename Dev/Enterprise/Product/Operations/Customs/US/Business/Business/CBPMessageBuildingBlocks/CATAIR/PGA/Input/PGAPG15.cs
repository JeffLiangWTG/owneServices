using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public class PGAPG15 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.PGAPG15, IBIRDPGAScientificRecord
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
