using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.BatchProcessor
{
	public class ZACInterchange : EDIInterchange
	{
		public const string SARSeHubID = "ZACustoms";

		public ZACInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = EDIInterchange.ApplicationCodes.SouthAfricanCustoms;
		}

		protected override bool ShouldSendViaEHubCore
		{
			get { return true; }
		}

		protected override Type GetMessageTypeToCreate(ZString messageText)
		{
			Type messageType;
			switch (UNB?.ApplicationReference)
			{
				case SARSEDIMessage.MessageTypeNames.CONTRL:
					messageType = typeof(CONTRLEDIMessage);
					break;
				case SARSEDIMessage.MessageTypeNames.CUSCAR:
					messageType = typeof(CUSCAREDIMessage);
					break;
				case SARSEDIMessage.MessageTypeNames.CUSRES:
				case SARSEDIMessage.MessageTypeNames.CUSRES_CALINF:
				case SARSEDIMessage.MessageTypeNames.CUSRES_COSTCO:
				case SARSEDIMessage.MessageTypeNames.CUSRES_CUSCAR:
				case SARSEDIMessage.MessageTypeNames.CUSRES_EXP_RA:
				case SARSEDIMessage.MessageTypeNames.CUSRES_GOVGIO:
				case SARSEDIMessage.MessageTypeNames.CUSRES_GIO:
					messageType = typeof(CUSRESEDIMessage);
					break;
				case SARSEDIMessage.MessageTypeNames.CUSRES_REQDOC:
					messageType = typeof(CUSRES_REQDOCEDIMessage);
					break;
				case SARSEDIMessage.MessageTypeNames.GENRAL:
					messageType = typeof(GENRALMessage);
					break;
				case SARSEDIMessage.MessageTypeNames.EXPORT:
					messageType = typeof(CUSRESEDIMessage);
					break;
				case SARSEDIMessage.MessageTypeNames.STATAC_DAILY:
				case SARSEDIMessage.MessageTypeNames.STATAC_DETAIL:
					messageType = typeof(STATACEDIMessage);
					break;
				default:
					messageType = typeof(EDIMessage);
					break;
			}
			return messageType;
		}

		public ZDateTime DateTimeOfPreparation
		{
			get
			{
				var preparationDateTime = ZDateTime.Empty;
				if (UNB != null)
				{
					var dateAndTime = UNB.DateTimeOfPreparation.Date + UNB.DateTimeOfPreparation.Time;
					ZDateTime.TryParseExact(dateAndTime, out preparationDateTime, "yyyyMMddHHmm");
				}
				return preparationDateTime;
			}
		}

		public override UNCharacterSet CharacterSet => new ZACharacterSet();
	}
}
