using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.ZA;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAMessageTypeDecider : TypeDecider, IEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var eM_MessageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			switch (eM_MessageType)
			{
				case SARSEDIMessage.MessageTypes.CUSDEC:
					return typeof(CUSDECEDIMessage);
				case SARSEDIMessage.MessageTypes.CUSCAR:
					return typeof(CUSCAREDIMessage);
				case SARSEDIMessage.MessageTypes.GOVGIO:
					return typeof(GOVGIOEDIMessage);
				case SARSEDIMessage.MessageTypes.CONTRL:
					return typeof(CONTRLEDIMessage);
				case SARSEDIMessage.MessageTypes.CUSRES:
					return typeof(CUSRESEDIMessage);
				case SARSEDIMessage.MessageTypes.STATAC:
					return typeof(STATACEDIMessage);
				case SARSEDIMessage.MessageTypes.REQDOC:
					return typeof(REQDOCEDIMessage);
				case SARSEDIMessage.MessageTypes.CUSRES_REQDOC:
					return typeof(CUSRES_REQDOCEDIMessage);
				case SARSEDIMessage.MessageTypes.COSTCO:
					return typeof(COSTCOEDIMessage);
				case SARSEDIMessage.MessageTypes.CALINF:
					return typeof(CALINFEDIMessage);
				default:
					return typeof(ZAMessage);
			}
		}
	}
}
