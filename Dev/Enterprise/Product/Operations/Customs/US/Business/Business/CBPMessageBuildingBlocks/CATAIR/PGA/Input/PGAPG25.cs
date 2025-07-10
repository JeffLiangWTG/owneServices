using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class PGAPG25 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.PGAPG25, IBIRDOGALineRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, CargoWise.ComponentModel.INotifications notifications)
		{
			PGA pga = (PGA)ogaLine;

			pga.US_InvCurrPGAValue = PGALineValue;
		}

		#endregion
	}
}
