//using CargoWise.Customs.NL.MessageContracts;
//using CargoWise.Customs.NL.MessageContracts.DMS.Import;
//using CargoWise.Types;
//using Enterprise.Customs.NL.Business.Message.Wrappers;

//namespace Enterprise.Customs.NL.Business.MessageSending
//{
//	public class AmendmentXMLBuilder : IMessageBuilder
//	{
//		readonly JobDeclarationMessageSendingObject sendingObject;

//		public AmendmentXMLBuilder(JobDeclarationMessageSendingObject sendingObject)
//		{
//			this.sendingObject = sendingObject;
//		}

//		public string CreateMessage(bool includeDeclaration)
//		{
//			var metaDataWrapper = new MetaDataWrapper(sendingObject);
//			var builder = new AmendmentMessageBuilder(metaDataWrapper);
//			ZString messageText = builder.CreateMessage(includeDeclaration);
//			var amendmentsXml = sendingObject.AmendmentDetails?.Amendments?.Xml;
//			if (amendmentsXml != null)
//			{
//				var reader = amendmentsXml.CreateReader();
//				reader.MoveToContent();
//				messageText = messageText.Insert(messageText.IndexOf("</MetaData>"), reader.ReadOuterXml());
//			}

//			return messageText;
//		}
//	}
//}
