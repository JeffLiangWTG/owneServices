using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.ProcessManagement.Business
{
	// turns a response from the JiraServiceProvider into a set of Cargowise-specific BusinessObjects

	public static class JiraToCargoWiseDecoder
	{
		#region JSON Deserialisation

		/// <summary>
		/// Expects JSON in array format
		/// </summary>
		public static JiraProject[] DecodeProjectJSONArray(string arrayOfProjectsInJSONFormat, JiraResult result)
		{
			var projectList = new List<JiraProject>();
			try
			{
				var deserialisedArray = JsonConvert.DeserializeObject(arrayOfProjectsInJSONFormat);

				foreach (var projectJSON in ((JArray)deserialisedArray).WhereNotNull())
				{
					projectList.Add(GetProjectFromJSON(projectJSON));
				}
			}
			catch (JsonReaderException)
			{
				var jiraInvalidInputPrompt = new ZStringBuilder("");
				jiraInvalidInputPrompt.Append(Res.GetString("fe00ceb0-c90c-449b-ace1-5f4aa4ec2d8f", "The information returned wasn't in the expected format and can't be converted into Projects and Work Items. Please ensure the Jira URL is correct. Please contact Atlassian support for help determining the correct URL to use."));
				jiraInvalidInputPrompt.Append(Res.GetString("d42c5da7-55d0-411b-9953-a5afabf6f97a", "The following message was returned: "));
				jiraInvalidInputPrompt.AppendLine(arrayOfProjectsInJSONFormat.Length >= 150 ? arrayOfProjectsInJSONFormat.Substring(0, 150) : arrayOfProjectsInJSONFormat);
				result.Status = JiraResponseStatus.GenericFailure;
				result.Response = jiraInvalidInputPrompt.ToStringWithNewLineBetweenAppends();
			}
			return projectList.ToArray();
		}

		/// <summary>
		/// Expects JSON in array format
		/// </summary>
		public static JiraIssue[] DecodeIssueJSONArray(string arrayOfIssuesInJSONFormat)
		{
			var paginatedQuery = SafelyParseJSONString(arrayOfIssuesInJSONFormat);

			if (IsJTokenUseable(paginatedQuery))
			{
				var issuesArray = paginatedQuery["issues"];

				if (IsJTokenUseable(issuesArray))
				{
					var issueList = new List<JiraIssue>();

					foreach (var jsonToken in ((JArray)issuesArray).WhereNotNull())
					{
						issueList.Add(GetIssueFromJSON(jsonToken));
					}

					return issueList.ToArray();
				}
			}

			return System.Array.Empty<JiraIssue>();
		}

		public static JiraUser GetUserFromJSON(string userInJSONFormat)
		{
			if (userInJSONFormat != null)
			{
				var jUser = SafelyParseJSONString(userInJSONFormat);

				if (IsJTokenUseable(jUser))
				{
					return new JiraUser(jUser);
				}
			}

			return null;
		}

		public static JiraComment[] DecodeCommentJSONArray(string arrayOfCommentsInJSONFormat)
		{
			var paginatedQuery = SafelyParseJSONString(arrayOfCommentsInJSONFormat);

			if (IsJTokenUseable(paginatedQuery))
			{
				var commentsArray = paginatedQuery["comments"];

				if (IsJTokenUseable(commentsArray))
				{
					return commentsArray.Cast<JObject>().WhereNotNull().Select(x => new JiraComment(x)).ToArray();
				}
			}

			return System.Array.Empty<JiraComment>();
		}

		static JiraProject GetProjectFromJSON(JToken projectJSON)
		{
			return new JiraProject(projectJSON);
		}

		static JiraIssue GetIssueFromJSON(JToken issueToken)
		{
			return new JiraIssue(issueToken);
		}

		#endregion JSON Deserialisation

		#region JiraEntity To CW1Bizo

		public static Project GetProjectFromJiraProject(JiraProject jiraProject, BusinessObjectFactory factory)
		{
			var project = factory.New<ProjectConvertedFromJiraProject>();
			project.WKP_Summary = GetTruncatedString(jiraProject.Code + " - " + jiraProject.Name, AutoWorkProject.Schema.WKP_SummaryMaxLength);
			project.WKP_Details = ZBlob.FromUTF8(jiraProject.Description);
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Working;

			var mapItems = ProcessManagementRegistry.Instance.JiraProjectCategoriesMapping.Value.JiraClassificationMap.Cast<JiraEntityClassificationMapItem>();

			if (mapItems != null)
			{
				var relevantMapItems = mapItems.Where(mapItem => mapItem.JiraEntityName.Equals(jiraProject.CategoryName));

				foreach (var mapItem in relevantMapItems)
				{
					var projectPropertyName = mapItem.SelectionCriterionFieldName;

					if (!jiraProject.CategoryName.IsNullOrEmpty())
					{
						project[projectPropertyName] = mapItem.SelectionCriterionFieldValue;
					}
				}
			}

			return project;
		}

		#region JiraIssue to WorkItem conversion

		public static WorkItem GetWorkItemFromJiraIssue(JiraIssue jiraIssue, BusinessObjectFactory factory)
			=> GetWorkItemFromJiraIssue(
				jiraIssue,
				factory,
				ProcessManagementRegistry.Instance.JiraCustomFieldsMapping.Value.JiraClassificationMap.Cast<JiraCustomFieldMapItem>());

		public static WorkItem GetWorkItemFromJiraIssue(JiraIssue jiraIssue, BusinessObjectFactory factory, IEnumerable<JiraCustomFieldMapItem> customFieldMapItems)
		{
			var workItem = factory.New<WorkItemConvertedFromJiraIssue>();
			workItem.WKI_Summary = GetTruncatedString(GetFormattedJiraIssueSummary(jiraIssue), AutoWorkItem.Schema.WKI_SummaryMaxLength);

			MapCustomFieldData(workItem, jiraIssue, customFieldMapItems);
			MapClassificationData(workItem, jiraIssue.IssueType);

			return workItem;
		}

		public static void SetWorkItemDescription(WorkItem workItem, string description)
		{
			workItem.WKI_Details = ZBlob.FromUTF8(description);
		}

		static void MapClassificationData(WorkItem workItem, string dataToMap)
		{
			var mapItems = ProcessManagementRegistry.Instance.JiraIssueTypesMapping.Value.JiraClassificationMap.Cast<JiraEntityClassificationMapItem>();
			if (dataToMap.IsNullOrEmpty() || mapItems == null)
			{
				return;
			}

			var relevantMapItems = mapItems.Where(mapItem => mapItem.JiraEntityName.Equals(dataToMap));
			var orderedMapItems = relevantMapItems.OrderBy(item => item.SelectionCriterionFields.ToList().IndexOf(item.SelectionCriterionFieldName));

			foreach (var mapItem in orderedMapItems)
			{
				var workItemPropertyName = mapItem.SelectionCriterionFieldName;

				workItem[workItemPropertyName] = mapItem.SelectionCriterionFieldValue;
			}
		}

		static void MapCustomFieldData(WorkItem workItem, JiraIssue jiraIssue, IEnumerable<JiraCustomFieldMapItem> customFieldMapping)
		{
			foreach (var customField in jiraIssue.CustomFields)
			{
				var relevantMapItems = customFieldMapping.Where(mapItem => mapItem.CustomFieldId.Equals(customField.Id) && mapItem.JiraEntityName.Equals(customField.Value));
				var orderedMapItems = relevantMapItems.OrderBy(item => item.SelectionCriterionFields.ToList().IndexOf(item.SelectionCriterionFieldName));

				foreach (var mapItem in orderedMapItems)
				{
					var workItemPropertyName = mapItem.SelectionCriterionFieldName;

					workItem[workItemPropertyName] = mapItem.SelectionCriterionFieldValue;
				}
			}
		}

		static ZString GetFormattedJiraIssueSummary(JiraIssue jiraIssue)
		{
			return jiraIssue.Code + ": " + jiraIssue.Summary;
		}

		#endregion JiraIssue to WorkItem conversion

		static ZString GetTruncatedString(string stringToTruncate, int maxLength)
		{
			return new ZString(stringToTruncate).SubstringSafe(0, maxLength);
		}

		#endregion JiraEntity To CW1Bizo

		#region Helper Methods

		public static bool IsJTokenUseable(object jToken)
		{
			return jToken != null && jToken is JContainer jContainer && jContainer.HasValues;
		}

		static JObject SafelyParseJSONString(string jsonToParse)
		{
			JObject deserialisedJSON;

			try
			{
				deserialisedJSON = JObject.Parse(jsonToParse);
			}
			catch (JsonReaderException)
			{
				deserialisedJSON = null;
			}

			return deserialisedJSON;
		}

		#endregion Helper Methods
	}
}
