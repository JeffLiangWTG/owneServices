using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;

namespace Enterprise.ProcessManagement.Business
{
	[DebuggerDisplay("Jira Issue - {Code}: {Description}")]
	public class JiraIssue : JiraEntity, IExternalEntityLinkable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		internal JiraIssue(JToken issueToken)
		{
			if (issueToken != null)
			{
				ID = ParseStringField(issueToken, JiraConstants.Issue.ID);
				Code = ParseStringField(issueToken, JiraConstants.Issue.Code);

				if (TryParseToken(issueToken, JiraConstants.Issue.MiscRenderedFields, out var renderedFieldsToken))
				{
					Description = ParseStringField(renderedFieldsToken, JiraConstants.Issue.IssueDescription);
				}

				if (TryParseToken(issueToken, JiraConstants.Issue.MiscFields, out var fieldsToken))
				{
					if (TryParseToken(fieldsToken, JiraConstants.Issue.Project, out var projectToken))
					{
						ProjectCode = ParseStringField(projectToken, JiraConstants.Project.Code);
					}

					Summary = ParseStringField(fieldsToken, JiraConstants.Issue.Summary);

					if (TryParseToken(fieldsToken, JiraConstants.Issue.Creator, out var creatorToken))
					{
						Creator = new JiraUser(creatorToken);
					}

					if (Description.IsNullOrEmpty())
					{
						DescriptionIsPlaintext = true;
						Description = ParseUnrenderedDescription(fieldsToken);
					}

					ParseComments(fieldsToken);

					var issueType = fieldsToken[JiraConstants.Issue.IssueType];

					if (IsJTokenUseable(issueType))
					{
						IssueType = ParseStringField(issueType, JiraConstants.IssueType.Name);
					}

					if (fieldsToken[JiraConstants.Issue.Attachment] is JArray attachmentArray)
					{
						foreach (var attachment in attachmentArray)
						{
							Attachments.Add(new JiraAttachment(attachment));
						}
					}

					var customFields = fieldsToken
						.Children<JProperty>()
						.Cast<JProperty>()
						.Where(_ => _.Name.StartsWith("customfield", StringComparison.InvariantCultureIgnoreCase) && _.Value.HasValues)
						.ToArray();

					CustomFields = customFields.Select(ExtractCustomFieldValue).SelectMany(_ => _).ToArray();
				}
			}
		}

		public string ID { get; }

		public string Code { get; }

		public string ProjectCode { get; }

		public string Description { get; }

		public bool DescriptionIsPlaintext { get; }

		public string Summary { get; }

		public JiraUser Creator { get; }

		public string IssueType { get; }

		public JiraCustomField[] CustomFields { get; set; } = Array.Empty<JiraCustomField>();

		public WorkItem WorkItem { get; set; }

		public List<JiraComment> Comments { get; } = new List<JiraComment>();

		public List<JiraAttachment> Attachments { get; } = new List<JiraAttachment>();

		public static JiraCustomField[] ExtractCustomFieldValue(string id, JToken token)
		{
			var result = new List<JiraCustomField>();

			switch (token.Type)
			{
				case JTokenType.Property:
					var property = (JProperty)token;
					var values = ExtractCustomFieldValue(id, property.Value);
					foreach (var value in values.Where(_ => string.IsNullOrEmpty(_.Key)))
					{
						value.Key = property.Name;
					}

					result.AddRange(values);
					break;

				case JTokenType.String:
				case JTokenType.Float:
				case JTokenType.Boolean:
				case JTokenType.Date:
				case JTokenType.Integer:
				case JTokenType.TimeSpan:
					result.Add(new JiraCustomField
					{
						Id = id,
						Value = token.ToString()
					});
					break;

				case JTokenType.Array:
					foreach (var item in token)
					{
						result.AddRange(ExtractCustomFieldValue(id, item));
					}
					break;

				case JTokenType.Object:
					foreach (var child in token.Children())
					{
						result.AddRange(ExtractCustomFieldValue(id, child));
					}
					break;
			}

			return result.ToArray();
		}

		public static JiraCustomField[] ExtractCustomFieldValue(JProperty prop)
		{
			if (!prop.HasValues)
			{
				return Array.Empty<JiraCustomField>();
			}

			var id = ExtractCustomFieldIdFromName(prop.Name);
			return ExtractCustomFieldValue(id, prop);
		}

		public static string ExtractCustomFieldIdFromName(string name)
		{
			var index = (name?.IndexOf('_') ?? -1);
			if (index == -1)
			{
				return name;
			}

			return name.Substring(index + 1, name.Length - index - 1);
		}

		public override bool Equals(object obj)
		{
			var otherObject = (JiraIssue)obj;

			return otherObject != null
				&& ID == otherObject.ID
				&& Code == otherObject.Code
				&& ProjectCode == otherObject.ProjectCode
				&& Description == otherObject.Description
				&& Summary == otherObject.Summary
				&& Creator == otherObject.Creator;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 143477404;
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ID);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProjectCode);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Summary);
				hashCode = hashCode * -1521134295 + EqualityComparer<JiraUser>.Default.GetHashCode(Creator);
				return hashCode;
			}
		}

		static string ParseUnrenderedDescription(JToken fieldsToken)
		{
			if (TryParseToken(fieldsToken, JiraConstants.Issue.IssueDescription, out var descriptionToken) &&
				TryParseToken(descriptionToken, JiraConstants.Issue.IssueDescriptionContent, out var contentToken) &&
				TryParseToken(contentToken[0], JiraConstants.IssueDescriptionContent.Content, out var innerContentToken))
			{
				return ParseStringField(innerContentToken[0], JiraConstants.IssueDescriptionContent.Text);
			}

			return null;
		}

		void ParseComments(JToken fieldsToken)
		{
			if (TryParseToken(fieldsToken, JiraConstants.Issue.CommentList, out var commentListToken) &&
				TryParseToken(commentListToken, JiraConstants.Issue.IssueComments, out var commentsToken))
			{
				foreach (var commentToken in (JArray)commentsToken)
				{
					var comment = new JiraComment(commentToken);
					Comments.Add(comment);
				}
			}
		}

		#region IExternalEntityLinkable Members

		string IExternalEntityLinkable.ParentTableCode => WorkItemSchema.Constants.Prefix;

		#endregion IExternalEntityLinkable Members
	}
}
