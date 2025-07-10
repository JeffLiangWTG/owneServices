using System;
using System.Diagnostics;
using CargoWise.Types;
using Newtonsoft.Json.Linq;

namespace Enterprise.ProcessManagement.Business
{
	[DebuggerDisplay("Jira Issue Attachment {ID} - {FileName}: {ContentURL}")]
	public class JiraAttachment : JiraEntity
	{
		internal JiraAttachment(string customFileName, string descriptionDocContent)
		{
			FileName = customFileName;
			DescriptionDocContent = descriptionDocContent;
		}

		internal JiraAttachment(JToken toker)
		{
			ID = ParseStringField(toker, JiraConstants.IssueAttachment.ID);
			FileName = ParseStringField(toker, JiraConstants.IssueAttachment.FileName);
			CreateTime = ParseDateField(toker, JiraConstants.IssueAttachment.Created);

			var uriAsString = ParseStringField(toker, JiraConstants.IssueAttachment.ContentURL);

			if (!string.IsNullOrEmpty(uriAsString))
			{
				ContentURL = new Uri(uriAsString);
			}
		}

		public string ID { get; }

		public string FileName { get; }

		public ZDateTime CreateTime { get; }

		public Uri ContentURL { get; }

		public string DescriptionDocContent { get; }
	}
}
