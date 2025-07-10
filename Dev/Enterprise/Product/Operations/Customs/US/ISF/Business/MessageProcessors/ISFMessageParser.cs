using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFMessageParser
	{
		public ISFMessageParser(IImporterSecurityFiling parent, MessageBlock[] messageBlocks)
		{
			ParseData(parent, messageBlocks);
		}

		public ABIResponseStatus Status
		{
			get { return status; }
		}
		ABIResponseStatus status;

		public ZString HtmlData
		{
			get { return htmlData; }
		}
		ZString htmlData;

		public ZString HtmlMessageDataOnly
		{
			get { return htmlMessageDataOnly; }
		}
		ZString htmlMessageDataOnly;

		public ZString JobReference
		{
			get { return jobReference; }
		}
		ZString jobReference;

		public ZString URI
		{
			get { return uri; }
		}
		ZString uri;

		void ParseData(IImporterSecurityFiling parent, MessageBlock[] messageBlocks)
		{
			HtmlTableCreator messageDetailCreator = null;
			HtmlTableCreator userDefinedReferenceDetailCreator = null;
			status = ABIResponseStatus.Undefined;
			StringBuilder html = new StringBuilder();
			List<ZString> errorList = new List<ZString>();

			html.Append("Importer Security Filing Message Result");
			if (parent != null)
			{
				jobReference = parent.HumanReadableName;
				HtmlTableCreator billCreator = new HtmlTableCreator(new string[] { "Bill Of Lading" });
				foreach (IShipmentReferenceID shipmentReferenceID in parent.ShipmentIDs)
				{
					billCreator.WriteRow(shipmentReferenceID.ShipmentReferenceIdentifier);
				}
				html.Append("<br /><br />");
				html.Append(billCreator.ToHtml());
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ImporterSecurityFiling, parent.PK.ToGuid());
			}
			else
			{
				jobReference = "Unknown";
				uri = "";
			}

			html.Append("<br /><br />");
			foreach (MessageBlock block in messageBlocks)
			{
				ISFSF90 sf90 = block as ISFSF90;
				if (sf90 != null)
				{
					messageDetailCreator = ParseISFSF90(messageDetailCreator, errorList, sf90);
				}
				else
				{
					ISFSF20 sf20 = block as ISFSF20;
					if (sf20 != null && sf20.ReferenceIdentifierQualifier == ReferenceDataCodeList.Codes.UserDefinedReferenceNumber)
					{
						userDefinedReferenceDetailCreator = ParseISFSF20(userDefinedReferenceDetailCreator, sf20);
					}
				}
			}
			ZStringBuilder htmlMessageDataBuilder = new ZStringBuilder();
			if (userDefinedReferenceDetailCreator != null)
			{
				string userDefinedDetail = userDefinedReferenceDetailCreator.ToHtml();
				htmlMessageDataBuilder.Append(userDefinedDetail);
			}
			if (messageDetailCreator != null)
			{
				string messageDetail = messageDetailCreator.ToHtml();
				htmlMessageDataBuilder.Append(messageDetail);
			}
			htmlMessageDataOnly = htmlMessageDataBuilder.ToStringWithDelimiterBetweenAppends("<br /><br />");
			html.Append(htmlMessageDataOnly);
			htmlData = html.ToString();
		}

		HtmlTableCreator ParseISFSF20(HtmlTableCreator creator, ISFSF20 sf20)
		{
			if (creator == null)
			{
				creator = new HtmlTableCreator(GetColumnTitlesSF20());
			}
			creator.WriteRow(sf20.ReferenceIdentifier);
			return creator;
		}

		HtmlTableCreator ParseISFSF90(HtmlTableCreator creator, List<ZString> errorList, ISFSF90 sf90)
		{
			if (!errorList.Contains(sf90.ErrorCode))
			{
				errorList.Add(sf90.ErrorCode);
			}

			switch (sf90.MessageTypeCode)
			{
				case ISFMessageStatus.Codes.Rejected:
					status = ABIResponseStatus.Rejected;
					break;
				case ISFMessageStatus.Codes.AcceptedWithWarning:
					if (status != ABIResponseStatus.Rejected)
					{
						status = ABIResponseStatus.PartialCleared;
					}
					break;
				case ISFMessageStatus.Codes.Accepted:
					if (status != ABIResponseStatus.PartialCleared || status != ABIResponseStatus.Rejected)
					{
						status = ABIResponseStatus.Cleared;
					}
					break;
			}

			if (creator == null)
			{
				creator = new HtmlTableCreator(GetColumnTitlesSF90());
			}
			AddColumnValuesSF90(creator, sf90);
			return creator;
		}

		void AddColumnValuesSF90(HtmlTableCreator creator, ISFSF90 block)
		{
			ZString narrativeMessage = ErrorList.GetDescriptionFromCode(block.ErrorCode) ?? ZString.Empty;
			if (narrativeMessage.IsEmpty)
			{
				narrativeMessage = block.NarrativeMessageText;
			}
			else
			{
				narrativeMessage = block.NarrativeMessageText + " (" + narrativeMessage + ")";
			}
			creator.WriteRow(block.ErrorCode, narrativeMessage);
		}

		ISFErrorList ErrorList
		{
			get { return errorList ?? (errorList = new ISFErrorList()); }
		}
		ISFErrorList errorList;

		IEnumerable<string> GetColumnTitlesSF90()
		{
			return new string[] { "Narrative Message Code Identifier", "Narrative Message Text" };
		}

		IEnumerable<string> GetColumnTitlesSF20()
		{
			return new string[] { "User-defined Reference Number" };
		}
	}
}
