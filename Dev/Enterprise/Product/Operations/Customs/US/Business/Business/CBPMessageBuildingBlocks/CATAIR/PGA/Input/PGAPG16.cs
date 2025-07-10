using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public class PGAPG16 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.PGAPG16, IBIRDPGAScientificRecord
	{
		#region IBIRDPGAScientificRecord Members

		void IBIRDPGAScientificRecord.Update(ConstituentElement constituentElement, INotifications notifications)
		{
			constituentElement.ScientificDataCollection.UpdateToCountryIfEmptyOrCreate(CountryCode);
		}

		#endregion
	}
}
