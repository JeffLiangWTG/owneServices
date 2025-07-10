namespace Enterprise.ProcessManagement.Business
{
	#region SuppressResourceStringsCheckRegion

	public static class JiraConstants
	{
		public static class Project
		{
			public const string ID = "id";
			public const string Code = "key";
			public const string Description = "description";
			public const string Lead = "lead";
			public const string Components = "components";
			public const string IssueTypes = "issueTypes";
			public const string Name = "name";
			public const string ProjectCategory = "projectCategory";
			public const string Type = "projectTypeKey";
		}

		public static class ProjectCategory
		{
			public const string ID = "id";
			public const string Name = "name";
			public const string Description = "description";
		}

		public static class User
		{
			public const string Key = "key";
			public const string ID = "accountId";
			public const string Name = "name";
			public const string Email = "emailAddress";
			public const string DisplayName = "displayName";
			public const string IsActive = "active";
		}

		public static class Component
		{
			public const string ID = "id";
			public const string Name = "name";
			public const string Description = "description";
			public const string IsAssigneeTypeValid = "isAssigneeTypeValid";
		}

		public static class Issue
		{
			public const string ID = "id";
			public const string Code = "key";
			public const string MiscFields = "fields";
			public const string MiscRenderedFields = "renderedFields";
			public const string IssueType = "issuetype";
			public const string TimeSpentInSeconds = "timespent";
			public const string Project = "project";
			public const string IsDone = "resolution";
			public const string DoneDate = "resolutiondate";
			public const string Priority = "priority";
			public const string SoftTags = "labels";
			public const string TimeEstimateInSeconds = "timeestimate";
			public const string IssueLinks = "issuelinks";
			public const string Assignee = "assignee";
			public const string IssueStatus = "status";
			public const string Components = "components";
			public const string IssueDescription = "description";
			public const string IssueDescriptionContent = "content";
			public const string Summary = "summary";
			public const string Creator = "creator";
			public const string Subtasks = "subtasks";
			public const string Reporter = "reporter";
			public const string DueDate = "duedate";
			public const string CommentList = "comment";
			public const string IssueComments = "comments";
			public const string Worklog = "worklog";
			public const string Attachment = "attachment";
		}

		public static class IssueType
		{
			public const string ID = "id";
			public const string Description = "description";
			public const string Name = "name";
			public const string IsSubtask = "subtask";
		}

		public static class IssueLink
		{
			public const string ID = "id";
			public const string Type = "type";
			public const string InwardIssue = "inwardIssue";
			public const string OutwardIssue = "outwardIssue";
		}

		public static class IssueLinkType
		{
			public const string ID = "id";
			public const string Name = "name";
			public const string Inward = "inward";
			public const string Outward = "outward";
		}

		public static class IssueStatus
		{
			public const string Description = "description";
			public const string Name = "name";
			public const string ID = "id";
			public const string IssueStatusCategory = "category";
		}

		public static class IssueStatusCategory
		{
			public const string ID = "id";
			public const string Key = "key";
			public const string Colour = "colorName";
			public const string Name = "name";
		}

		public static class IssueDescriptionContent
		{
			public const string Content = "content";
			public const string Text = "text";
		}

		public static class IssueComment
		{
			public const string ID = "id";
			public const string Author = "author";
			public const string Body = "body";
			public const string CreatedTime = "created";
			public const string BodyContent = "content";
			public const string BodyText = "text";
		}

		public static class IssueAttachment
		{
			public const string ID = "id";
			public const string FileName = "filename";
			public const string ContentURL = "content";
			public const string Created = "created";
		}

		public static class Worklog
		{
			public const string Author = "author";
			public const string LastUpdatedAuthor = "updateAuthor";
			public const string Comment = "comment";
			public const string CreatedTime = "created";
			public const string UpdatedTime = "updated";
			public const string StartedTime = "started";
			public const string DurationInSeconds = "timeSpentSeconds";
			public const string ID = "id";
			public const string IssueID = "issueId";
		}

		public static class JiraAPIVersions
		{
			public const string JiraRequestAPI_2 = "rest/api/2/";
			public const string JiraRequestAPI_3 = "rest/api/3/";
		}
	}

	#endregion
}
