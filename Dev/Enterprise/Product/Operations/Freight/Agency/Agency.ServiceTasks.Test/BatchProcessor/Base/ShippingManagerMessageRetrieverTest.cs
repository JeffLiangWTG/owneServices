using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class ShippingManagerMessageRetrieverTest : BaseAgencyTest
	{
		#region Implementation
		protected MailItem CreateIncomingMailItem(string emlText, string code)
		{
			int endOfHeader = emlText.IndexOf("\r\n\r\n");
			string header;
			string body;
			if (endOfHeader < 0)
			{
				header = emlText;
				body = "";
			}
			else
			{
				header = emlText.Substring(0, endOfHeader);
				body = emlText.Substring(endOfHeader + 4);
			}

			MailItem item = Factory.New<MailItem>();
			item.MI_Status = MailStatus.Queued;
			item.MI_Direction = MailDirection.Receive;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_From = Extract("^From: (?<value>.*)$", header);
			item.MI_Subject = Extract("^Subject: (?<value>.*)$", header);
			item.MI_Header = header;
			item.MI_Body = body;
			item.AddRecipientForSystemCommunication(Extract("^To: (?<value>.*)$", header));
			item.ExtractAttachments();
			MailFilterLocatorTestHelper.SetApplication(item, code);
			return item;
		}

		protected MailItem CreateIncomingMailItem(string from, string subject, string body, string code)
		{
			MailItem item = Factory.New<MailItem>();
			item.MI_Status = MailStatus.Queued;
			item.MI_Direction = MailDirection.Receive;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_From = from;
			item.MI_Subject = subject;
			item.MI_Body = body;
			MailFilterLocatorTestHelper.SetApplication(item, code);
			return item;
		}

		protected MailAttachment CreateAttachment(MailItem item, string fileName, ZBlob content)
		{
			MailAttachment attachment = item.MailAttachments.AddNew();
			attachment.MA_Data = content;
			attachment.MA_FileName = fileName;
			return attachment;
		}

		protected EDIMessage[] LoadMessages()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_Status, EIDOMessage.Status.Queued);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EIDOMessage.Direction.Receive);
			filter.TableIndexHints.Add(new TableIndexHint("PK_UX__EM_PK"));
			return Factory.Load<EDIMessage>(filter);
		}

		protected string Extract(string pattern, string source)
		{
			Regex regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			Match match = regex.Match(source);
			return match.Success ? match.Groups["value"].Value : "";
		}

		protected static void AssertMessages(string message, string[] expectedMessages, EDIMessage[] actualMessages)
		{
			const string format = "Application Code: {0}\r\n" + "Message Type: {1}\r\n" + "Message Sub Type: {2}\r\n" + "";
			AssertContainsExactElementsInAnyOrder(message, expectedMessages, Array.ConvertAll(actualMessages, (m) => string.Format(format, m.EM_ApplicationCode, m.EM_MessageType, m.EM_MessageSubType)));
		}

		protected void AssertEmailProcessed(MailItem item)
		{
			AssertEmailProcessed("Expecting email to be processed", item);
		}

		protected void AssertEmailProcessed(string message, MailItem item)
		{
			AssertEquals(message, item.MI_Status, MailStatus.Processed);
		}

		protected void AssertEmailFailed(MailItem item)
		{
			AssertEmailFailed("Expecting email to have failed processing", item);
		}

		protected void AssertEmailFailed(string message, MailItem item)
		{
			AssertEquals(message, item.MI_Status, MailStatus.Failed);
		}
		#endregion
	}
}
