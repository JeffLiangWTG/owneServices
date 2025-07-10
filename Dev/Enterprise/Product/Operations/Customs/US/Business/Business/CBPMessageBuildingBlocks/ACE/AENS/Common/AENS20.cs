using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS20 : Abstract.AENS20, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_UI_NKCarrierSCAC = CarrierCode;
			declaration.US_SchDArrival = DistrictPortOfUnlading;
			declaration.US_EntryDate = EstimatedDateOfArrival;
			declaration.US_US_NKLocationOfGoods = LocationOfGoodsCode;
			declaration.JE_VesselName = ConveyanceName;
			declaration.US_SchDExam = DesignatedExamPortCode;
			declaration.US_ITDate = InBondInTransitDate;
		}

		#endregion
	}
}
