using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1;
using CargoWise.eHub.Products.NZCustoms.Common;

namespace CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1
{
	public partial class RequestLodgeResponseResponse
	{

		public bool IsEmpty()
		{
			return
				string.IsNullOrWhiteSpace(this.RequestResponseResponse.MailboxMsgId) &&
				string.IsNullOrWhiteSpace(GetClientID(this.RequestResponseResponse.MessageName, this.RequestResponseResponse.Partner)) &&
				string.IsNullOrWhiteSpace(this.RequestResponseResponse.CustomerReference) &&
				(this.attachments == null || this.attachments.Length == 0 || IsMessageNotFound(this.attachments))
				;
		}

		public bool IsValid()
		{
			return this.RequestResponseResponse != null && !IsMissingProperties();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (this.RequestResponseResponse == null)
			{
				sb.Append("RequestResponseResponse is null");
			}
			else
			{
				sb.Append(string.Format("MailboxMsgId={0}, ", IsPropertyEmpty(this.RequestResponseResponse.MailboxMsgId)));
				sb.Append(string.Format("RequestResponseResponse={0}, ", this.RequestResponseResponse));
				sb.Append(string.Format("MessageName={0}, ", IsPropertyEmpty(this.RequestResponseResponse.MessageName)));
				sb.Append(string.Format("Partner={0}, ", IsPropertyEmpty(this.RequestResponseResponse.Partner)));
				sb.Append(string.Format("CustomerReference={0}, ", IsPropertyEmpty(this.RequestResponseResponse.CustomerReference)));
				sb.Append("Attachments: ");

				if (this.attachments != null)
				{
					for (int i = 0; i < this.attachments.Length; i++)
					{
						sb.Append(string.Format("Attachment [{0}]", i));
						if (this.attachments[i] == null)
						{
							sb.Append("empty.");
						}
						else
						{
							sb.Append(string.Format("FileName={0}", IsPropertyEmpty(this.attachments[i].Filename)));
							sb.Append(string.Format("ContentType={0}", IsPropertyEmpty(this.attachments[i].ContentType)));
							sb.Append(string.Format("Content={0}", IsPropertyEmpty(Encoding.UTF8.GetString(Convert.FromBase64String(this.attachments[i].Content)))));
						}
					}
				}
			}

			return sb.ToString();
		}

		string GetClientID(string messageName, string partner)
		{
			string clientId = partner;
			if (!string.IsNullOrWhiteSpace(messageName))
			{
				var parts = messageName.Split('_');

				if (parts.Length > 0)
				{
					clientId = parts[0];
				}
			}

			return clientId;
		}

		bool IsMissingProperties()
		{
			if (!string.IsNullOrEmpty(this.RequestResponseResponse.MessageName) &&
				(string.IsNullOrEmpty(this.RequestResponseResponse.Partner) || string.IsNullOrEmpty(this.RequestResponseResponse.CustomerReference)))
			{
				var splitMessageName = this.RequestResponseResponse.MessageName.Split('_');
				if (splitMessageName.Length > 1)
				{
					this.RequestResponseResponse.Partner = splitMessageName[0];
					var reference = splitMessageName[1].Split('.');
					if (reference.Length > 0)
					{
						this.RequestResponseResponse.CustomerReference = reference[0];
					}
					else
					{
						this.RequestResponseResponse.CustomerReference = splitMessageName[1];
					}
				}
			}

			return
				!IsEmpty() &&
				(string.IsNullOrWhiteSpace(this.RequestResponseResponse.MailboxMsgId) ||
				string.IsNullOrWhiteSpace(GetClientID(this.RequestResponseResponse.MessageName, this.RequestResponseResponse.Partner)) ||
				string.IsNullOrWhiteSpace(this.RequestResponseResponse.CustomerReference) ||
				(this.attachments == null || this.attachments.Length == 0));
		}

		string IsPropertyEmpty(string property)
		{
			if (String.IsNullOrWhiteSpace(property))
			{
				return "is empty.";
			}
			return property;
		}

		bool IsMessageNotFound(Attachment[] attachments)
		{
			if (attachments.Length == 1)
			{
				var decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(attachments[0].Content));
				return decodedContent.Contains("<ReqStatus>NOT FOUND</ReqStatus>");
			}
			return false;
		}
	}
}
