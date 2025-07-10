using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGADT02 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGADT02, IBIRDOGALineRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			DOT dotLine = (DOT)ogaLine;

			DOTVIN vin = dotLine.DOTVINs.AddNew();
			vin.US_DOTMake = MakeOfVehicle;
			vin.US_DOTModel = Model;
			vin.US_DOTYear = Year;
			vin.US_DOTVIN = VehicleIdentificationNumber;
			vin.US_DOTRINo = NHTSARegisteredImporterRINumber;
			vin.US_DOTVEN = VehicleEligibilityNumber;
		}

		#endregion
	}
}
