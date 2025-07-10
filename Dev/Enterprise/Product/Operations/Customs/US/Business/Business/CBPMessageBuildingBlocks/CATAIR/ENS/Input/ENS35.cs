using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS35 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS35, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_ADDCVDSuretyCode = ADDCVDSuretyCode;
		}

		#endregion
	}
}
