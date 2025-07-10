using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.TR;

namespace Enterprise.Customs.TR.Business
{
	public class EDIMessageTypeDecider : TypeDecider, IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			switch (messageType)
			{
				case TRMessageTypes.Codes.TRO:
				case TRMessageTypes.Codes.T1O:
				case TRMessageTypes.Codes.T2O:
				case TRMessageTypes.Codes.T3O:
				case TRMessageTypes.Codes.TRM:
					return typeof(TRManifestMessage);
				case TRMessageTypes.Codes.TRE:
				case TRMessageTypes.Codes.TRQ:
				case TRMessageTypes.Codes.TRL:
				case TRMessageTypes.Codes.TRI:
				case TRMessageTypes.Codes.TRS:
				case TRMessageTypes.Codes.TRB:
				case TRMessageTypes.Codes.TRD:
				case TRMessageTypes.Codes.TCD:
				case TRMessageTypes.Codes.T1D:
				case TRMessageTypes.Codes.T2D:
				case TRMessageTypes.Codes.T1S:
				case TRMessageTypes.Codes.T1E:
					return typeof(ETradeEDIMessage);
				case TRMessageTypes.Codes.TSP:
				case TRMessageTypes.Codes.T1P:
					return typeof(SPTSMessage);
				case TRMessageTypes.Codes.TRN:
				case TRMessageTypes.Codes.T1N:
				case TRMessageTypes.Codes.T2N:
				case TRMessageTypes.Codes.TR5:
				case TRMessageTypes.Codes.T15:
					return typeof(NCTSMessage);
				case TRMessageTypes.Codes.DKO:
				case TRMessageTypes.Codes.DK1:
				case TRMessageTypes.Codes.DT1:
				case TRMessageTypes.Codes.DT2:
				case TRMessageTypes.Codes.DT3:
				case TRMessageTypes.Codes.DTE:
					return typeof(TRImportExportMessage);
				case TRMessageTypes.Codes.EUT:
				case TRMessageTypes.Codes.EUR:
					return typeof(ExportUnionMessage);
				default:
					return typeof(TRBaseMessage);
			}
		}

		public override Type GetTypeForNew() => null;
	}
}
