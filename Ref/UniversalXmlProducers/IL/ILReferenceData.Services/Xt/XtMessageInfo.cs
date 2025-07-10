using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	public sealed class XtMessageInfo : IXtMessageInfo
	{
		public XtMessageInfo(string requestXml, string messageSubType)
		{
			this.requestXml = Argument.NotNullOrEmpty(requestXml, nameof(requestXml));
			this.messageSubType = Argument.NotNullOrEmpty(messageSubType, nameof(messageSubType));
		}

		string IXtMessageInfo.ApplicationCode => Constants.XtMessageInfo.ApplicationCode;

		string IXtMessageInfo.MessageType => Constants.XtMessageInfo.MessageType;

		string IXtMessageInfo.DestinationParty => Constants.XtMessageInfo.DestinationParty;

		string IXtMessageInfo.SourceParty => Constants.XtMessageInfo.SourceParty;

		string IXtMessageInfo.MessageTrackingID => Guid.NewGuid().ToString();

		Dictionary<string, string> IXtMessageInfo.XTMessageAttributes
			=> new Dictionary<string, string>()
		{
				{CargoWise.xTMessaging.Integration.Constants.CustomMsgAttributes.MessageType,Constants.XtMessageInfo.MessageType},
				{CargoWise.xTMessaging.Integration.Constants.CustomMsgAttributes.MessageSubType,messageSubType},
		};


		BinaryReader IXtMessageInfo.GetMessageData() => new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(requestXml)));

		readonly string requestXml;
		readonly string messageSubType;
	}
}
