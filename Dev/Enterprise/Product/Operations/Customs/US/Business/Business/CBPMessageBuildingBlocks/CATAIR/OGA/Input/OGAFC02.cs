using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGAFC02 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGAFC02, IBIRDOGALineRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			FCC fccLine = (FCC)ogaLine;

			fccLine.US_FCCQty = FCCQuantity;
			fccLine.US_FCCWithhold = WithholdFromPublicInspectionRequested == "W";
		}

		#endregion
	}
}
