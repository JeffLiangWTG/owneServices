using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public class PGAPG04 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.PGAPG04, IBIRDPGAConstituentRecord
	{
		#region IBIRDPGAConstituentRecord Members

		void IBIRDPGAConstituentRecord.Update(ConstituentElement constituentElement, INotifications notifications)
		{
			constituentElement.US_PGANameOfTheConstituentElement = NameOfTheConstituentElement;
			constituentElement.US_PGAPercentOfConstituentElement = PercentOfConstituentElement;
			constituentElement.US_PGAQuantityOfConstituentElement = QuantityOfConstituentElement;
			constituentElement.US_PGAUnitOfMeasure = UnitOfMeasure;
		}

		#endregion
	}
}
