using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGAFC01 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGAFC01, IBIRDOGALineRecord, IBIRDOGALineIDRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			FCC fccLine = (FCC)ogaLine;

			fccLine.US_FCCLineNo = FCCLineNumber;
			fccLine.US_FCCImpCondNo = ImportConditionNumber;
			fccLine.US_FCCImpCondNoQtyAppr = this.ImportConditionNumberQuantityApproval == "Y";
			fccLine.US_FCCID = FCCIdentifier;
			fccLine.US_FCCTradeName = TradeName;
			fccLine.US_FCCModel = ModelTypeNumber;
		}

		#endregion

		#region IBIRDOGALineIDRecord Members

		OGAType IBIRDOGALineIDRecord.OGAType
		{
			get { return OGAType.FCC; }
		}

		void IBIRDOGALineIDRecord.SetOGAIndicator(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
		}

		#endregion
	}
}
