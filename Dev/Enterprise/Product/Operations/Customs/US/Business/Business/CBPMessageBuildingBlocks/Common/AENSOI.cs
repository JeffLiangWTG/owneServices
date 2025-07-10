using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;

[assembly: Enterprise.Customs.US.Messaging.Business.ABIMessageBlockTypeProvider(typeof(Enterprise.Customs.US.Business.MessageBuildingBlocks.Common.AENSOI))]
namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Common
{
	public partial class AENSOI : Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract.AENSOI, IBIRDOGAStartRecord
	{
		#region IBIRDOGAStartRecord Members

		ZString IBIRDOGAStartRecord.CommercialDesc
		{
			get { return CommercialDescriptionText; }
		}

		void IBIRDOGAStartRecord.SetOGAIndicator(JobComInvoiceLine invoiceLine)
		{
			//nothing to set
		}

		#endregion
	}
}
