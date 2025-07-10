using System.Collections.Generic;
using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.Business
{
	public class ABIInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public ABIInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }; }
		}

		protected override Enterprise.Messaging.Business.IInboundMessageCreator GetMessageCreator(Enterprise.Messaging.Business.EDIInterchange interchange) => new ABIInboundMessageCreator();

		protected override bool SupportEnvironmentSwitch => true;
		protected override bool IsNoBranchFilter => true;

		#region ABIInboundMessageCreator Class

		class ABIInboundMessageCreator : InboundMessageCreator<APLA, APLB, APLY, APLZ, MQEDIMessage>
		{
			protected override ZString GetApplicationIdentifier(APLA msgBlockA, APLB msgBlockB)
			{
				return (msgBlockB != null && !msgBlockB.ApplicationIdentifier.IsEmpty) ?
					msgBlockB.ApplicationIdentifier :
					(msgBlockA != null) ? msgBlockA.ApplicationIdentifier : ZString.Empty;
			}

			protected override ZString GetMessageNum(APLA msgBlockA, APLB msgBlockB, Stream messageTextStream, APLY msgBlockY, APLZ msgBlockZ)
			{
				return msgBlockB.UserData.Trim();
			}

			//NOTE: Currently ZipCode, ForeignPort and AffirmationOfCompliance processor assumes a message contains all the records and the other records that are not contained in a message is going to be deleted
			//If you are going to split into multi messages for above message types, you should be careful not to delete records that are not contained in one message
			protected override Dictionary<string, MultiMessageBreakInfo> GetMultiMessageInBY()
			{
				Dictionary<string, MultiMessageBreakInfo> multiMessageInBY = base.GetMultiMessageInBY();
				multiMessageInBY.Add(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults, new MultiMessageBreakInfo(new string[] { "R1" }, MultiMessageBreakInfo.NoByteLimit));
				multiMessageInBY.Add(ApplicationIdentifierCodeList.Codes.BillofLadingProcessingResults, new MultiMessageBreakInfo(new string[] { "P1" }, MultiMessageBreakInfo.NoByteLimit));
				multiMessageInBY.Add(ApplicationIdentifierCodeList.Codes.ProtestAutomaticNotificationandResponsetoFilerQuery, new MultiMessageBreakInfo(new string[] { "P10" }, MultiMessageBreakInfo.NoByteLimit));
				multiMessageInBY.Add(ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse, new MultiMessageBreakInfo(new string[] { "RA" }, 300000));
				multiMessageInBY.Add(ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse, new MultiMessageBreakInfo(new string[] { "C2", "C3" }, 300000));
				multiMessageInBY.Add(ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse, new MultiMessageBreakInfo(new string[] { "C2", "C3" }, 300000));
				multiMessageInBY.Add(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse, new MultiMessageBreakInfo(new string[] { "V1", "F211" }, 300000));
				multiMessageInBY.Add(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse, new MultiMessageBreakInfo(new string[] { "V1", "F211" }, 300000));
				multiMessageInBY.Add(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate, new MultiMessageBreakInfo(new string[] { "V1" }, 300000));
				multiMessageInBY.Add(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation, new MultiMessageBreakInfo(new string[] { "N1", "N3" }, MultiMessageBreakInfo.NoByteLimit));
				return multiMessageInBY;
			}
		}

		#endregion
	}
}
