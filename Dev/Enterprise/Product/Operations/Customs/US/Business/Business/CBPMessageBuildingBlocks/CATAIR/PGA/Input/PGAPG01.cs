using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public class PGAPG01 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.PGAPG01, IBIRDOGALineIDRecord
	{
		#region IBIRDOGALineIDRecord Members

		OGAType IBIRDOGALineIDRecord.OGAType
		{
			get { return OGAType.PGA; }
		}

		#endregion

		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			PGA pga = (PGA)ogaLine;

			pga.US_PGALineItemNumber = PGALineItemNumber;
		}

		void IBIRDOGALineIDRecord.SetOGAIndicator(JobComInvoiceLine invoiceLine)
		{
			//no indicator for PGA
		}

		#endregion
	}
}
