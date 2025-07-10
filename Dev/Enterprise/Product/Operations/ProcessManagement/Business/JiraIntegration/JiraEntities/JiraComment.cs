using CargoWise.Types;
using Newtonsoft.Json.Linq;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraComment : JiraEntity
	{
		public JiraComment(JToken commentToken)
		{
			if (commentToken != null)
			{
				Id = ParseStringField(commentToken, JiraConstants.IssueComment.ID);
				CreatedTime = ParseDateField(commentToken, JiraConstants.IssueComment.CreatedTime);
				Body = ParseBody(commentToken);
				Author = ParseAuthor(commentToken);
			}
		}

		public string Id { get; }
		public JiraUser Author { get; set; }
		public string Body { get; }
		public ZDateTime CreatedTime { get; }

		public bool IsValid => CreatedTime.IsValid && !string.IsNullOrEmpty(Body);

		static string ParseBody(JToken commentToken)
		{
			if (TryParseToken(commentToken, JiraConstants.IssueComment.Body, out var bodyToken) &&
				TryParseToken(bodyToken, JiraConstants.IssueComment.BodyContent, out var contentToken) &&
				TryParseToken(contentToken.First, JiraConstants.IssueComment.BodyContent, out var innerContentToken))
			{
				return ParseStringField(innerContentToken.First, JiraConstants.IssueComment.BodyText);
			}

			return null;
		}

		static JiraUser ParseAuthor(JToken commentToken)
		{
			return TryParseToken(commentToken, JiraConstants.IssueComment.Author, out var authorToken)
				? new JiraUser(authorToken)
				: null;
		}
	}
}
