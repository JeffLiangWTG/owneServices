using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public static class JiraIntegrationTestHelper
	{
		public class DummyProgressTracker : JiraImporterProgressTracker
		{
			public DummyProgressTracker()
			{
				ProgressUpdated += (sender, e) => statusHistory.Add(Tuple.Create(e.Status, e.Percentage));
			}

			public IEnumerable<Tuple<string, int>> StatusHistory => statusHistory;

			readonly List<Tuple<string, int>> statusHistory = new List<Tuple<string, int>>();
		}

		public static void AddDefaultOrgProxyClient()
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			var contact = orgProxy.Contacts.AddNew();
			contact.OC_ContactName = "Jan Michael Vincent";
			contact.OC_Email = "boberly.lastingtonnamersen@sampleweb.com";
			orgProxy.Factory.Save();
		}

		internal static JiraResult AssertImportWasSuccessful(JiraEntityImporter importer)
		{
			var genericCredentials = new JiraCredentials("WhoAmI?", "NoneOfYourBusiness!");
			JiraResult result = null;

			Assertion.AssertNoExceptionThrown("A generic import works without exception", () => result = importer.Import(genericCredentials));
			Assertion.AssertEquals("The import was expected to succeed but sadly it failed. VERY UNFAIR! " + result.Response, JiraResponseStatus.Success, result.Status);

			return result;
		}

		public static void SetJiraUrlsRegistryItem(params Tuple<string, string>[] codesAndUrls)
		{
			var list = new CodeDescriptionPairList();

			if (!codesAndUrls.Any())
			{
				list.AddPair("SYS", "https://www.creedthoughts.gov.www/creedthoughts");
			}
			else
			{
				codesAndUrls.ForEach(x => list.AddPair(x.Item1, x.Item2));
			}

			ProcessManagementRegistry.Instance.JiraSiteUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
		}

		public static void AssertDescriptionEDocAndHyperlinkAreCorrect(BusinessObjectFactory factory, WorkItem importedIssue, string issueCode, string issueDescription)
		{
			var descriptionDoc = AssertAndGetDescriptionEDoc(factory, importedIssue.PK, issueCode, issueDescription);
			AssertDescriptionIsEDocHyperlink(factory, importedIssue, descriptionDoc);
		}

		public static void AssertDescriptionIsEDocHyperlink(BusinessObjectFactory factory, WorkItem workItem, StorageDocsBase actualEDoc)
		{
			var expectedHyperlinkText = GenerateRichTextHyperlink(actualEDoc);
			var actualHyperlinkText = workItem.WKI_Details.ToUTF8().ToString();

			Assertion.AssertEquals(expectedHyperlinkText, actualHyperlinkText);
		}

		public static void AssertDescriptionDetails(string expectedDetails, WorkItem workItem)
		{
			var actualHyperlinkText = workItem.WKI_Details.ToUTF8().ToString();

			Assertion.AssertEquals(expectedDetails, actualHyperlinkText);
		}

		public static StorageDocsBase AssertAndGetDescriptionEDoc(BusinessObjectFactory factory, ZGuid workItemPK, string issueCode, string issueDescription)
		{
			return AssertAndGetEDocInDatabase(factory, workItemPK, issueCode + " Description", "html", ZDateTime.Now, Encoding.ASCII.GetBytes(issueDescription));
		}

		public static StorageDocsBase AssertAndGetEDocInDatabase(BusinessObjectFactory factory, ZGuid workItemPK, string expectedFileName, string expectedFileExtension, ZDateTime expectedCreateTime, byte[] expectedContent)
		{
			var documentFactory = new DbBackendDocumentFactory(factory);
			var storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, workItemPK));
			Assertion.AssertNotNull(storageMain);

			var doc = storageMain.eDocs.Cast<StorageDocsBase>().SingleOrDefault(edoc => edoc.SC_FileName == expectedFileName);

			Assertion.AssertNotNull(doc);
			Assertion.AssertEquals(expectedCreateTime, doc.SC_Date);
			Assertion.AssertEquals(expectedContent, doc.SC_ImageData);
			Assertion.AssertEquals(expectedFileName + "." + expectedFileExtension, doc.SC_FileNameWithExtension);
			Assertion.AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, doc.SC_DocType);

			return doc;
		}

		public static string GenerateRichTextHyperlink(StorageDocsBase storageDoc)
		{
			var storageCommand = ShowStorageDocUrlHandler.Instance.Create(storageDoc);

			return $@"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033
{{\colortbl ;\red0\green0\blue255;}}
\viewkind4\uc1\pard\sb100\sa100\lang3081\f0\fs24{{\field{{\*\fldinst{{HYPERLINK ""{storageCommand}""}}}}{{\fldrslt{{\cf1\ul Open Description In Browser}}}}}}
}}";
		}

		public static JiraResult EmptyJiraResult()
		{
			return new JiraResult(JiraResponseStatus.Success, "");
		}

		public static void AssertJiraResultIsSuccessfulAndNoResponseIsSet(JiraResult result)
		{
			Assertion.AssertEquals(JiraResponseStatus.Success, result.Status);
			Assertion.Assert(result.Response.IsNullOrEmpty());
		}

		public static class DummyJiraLinks
		{
			public const string DummyServerBaseUri = "https://testingforserver.com/";
			public const string DummyCloudBaseUri = "https://testingforcloud.com/";
			public const string DummyServerQuery = "https://testingforserver.com/rest/api/2/serverInfo";
			public const string DummyCloudQuery = "https://testingforcloud.com/rest/api/3/serverInfo";
		}

		#region Jira JSON strings

		#region JSON for One project

		public const string JSONForOneProject = @"
[
{
	""expand"": ""description,lead,issueTypes,url,projectKeys"",
	""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008"",
	""id"": ""10008"",
	""key"": ""BP"",
	""description"": ""Bibbity Bobb"",
	""lead"": {
		""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
		""key"": ""boberly.lastingtonnamersen"",
		""accountId"": ""11235"",
		""name"": ""Boberly.LastingtonNamersen"",
		""avatarUrls"": {
			""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
			""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
			""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue"",
			""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue""
		},
		""displayName"": ""Boberly Namersen"",
		""active"": true
	},
	""components"": [],
	""issueTypes"": [
		{
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
			""id"": ""10004"",
			""description"": ""A task that needs to be done."",
			""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
			""name"": ""Task"",
			""subtask"": false,
			""avatarId"": 10318
		},
		{
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10005"",
			""id"": ""10005"",
			""description"": ""The sub-task of the issue"",
			""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10513&avatarType=issuetype"",
			""name"": ""Sub-task"",
			""subtask"": true,
			""avatarId"": 10513
		}
	],
	""assigneeType"": ""UNASSIGNED"",
	""versions"": [],
	""name"": ""BusinessProject"",
	""roles"": {
		""atlassian-addons-project-access"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008/role/10003"",
		""Service Desk Team"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008/role/10101"",
		""Developers"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008/role/10106"",
		""Service Desk Customers"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008/role/10100"",
		""Administrators"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008/role/10002"",
		""Review"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008/role/10107""
	},
	""avatarUrls"": {
		""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
		""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
		""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
		""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
	},
	""projectCategory"": {
		""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
		""id"": ""10000"",
		""name"": ""Prooject"",
		""description"": ""Project that is custom""
	},
	""projectTypeKey"": ""business"",
	""simplified"": false,
	""style"": ""classic"",
	""isPrivate"": false
}
]";

		#endregion

		#region JSON for Two projects

		public const string JSONForTwoProjects = @"
[
	{
		""expand"": ""description,lead,issueTypes,url,projectKeys"",
		""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
		""id"": ""10001"",
		""key"": ""AVINA"",
		""description"": ""hello I am descriptive"",
		""lead"": {
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
			""key"": ""boberly.lastingtonnamersen"",
			""accountId"": ""11235"",
			""name"": ""Boberly.LastingtonNamersen"",
			""avatarUrls"": {
				""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
				""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
				""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue"",
				""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue""
			},
			""displayName"": ""Boberly Namersen"",
			""active"": true
		},
		""issueTypes"": [
			{
				""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
				""id"": ""10004"",
				""description"": ""A task that needs to be done."",
				""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
				""name"": ""Task"",
				""subtask"": false,
				""avatarId"": 10318
			},
			{
				""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10005"",
				""id"": ""10005"",
				""description"": ""The sub-task of the issue"",
				""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10513&avatarType=issuetype"",
				""name"": ""Sub-task"",
				""subtask"": true,
				""avatarId"": 10513
			},
			{
				""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10003"",
				""id"": ""10003"",
				""description"": ""Stories track functionality or features expressed as user goals."",
				""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10315&avatarType=issuetype"",
				""name"": ""Story"",
				""subtask"": false,
				""avatarId"": 10315
			},
			{
				""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10006"",
				""id"": ""10006"",
				""description"": ""jira.translation.issuetype.bug.name.desc"",
				""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10303&avatarType=issuetype"",
				""name"": ""Bug"",
				""subtask"": false,
				""avatarId"": 10303
			},
			{
				""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10000"",
				""id"": ""10000"",
				""description"": ""A big user story that needs to be broken down. Created by Jira Software - do not edit or delete."",
				""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/issuetypes/epic.svg"",
				""name"": ""Epic"",
				""subtask"": false
			}
		],
		""name"": ""Avengers Initiative"",
		""avatarUrls"": {
			""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
			""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
			""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
			""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
		},
		""projectKeys"": [
			""AVIN"",
			""AVINA""
		],
		""projectCategory"": {
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
			""id"": ""10000"",
			""name"": ""Prooject"",
			""description"": ""Project that is custom""
		},
		""projectTypeKey"": ""software"",
		""simplified"": false,
		""style"": ""classic"",
		""isPrivate"": false
	},
	{
		""expand"": ""description,lead,issueTypes,url,projectKeys"",
		""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008"",
		""id"": ""10008"",
		""key"": ""BP"",
		""description"": ""Business-handling project of the business-most ordinance"",
		""lead"": {
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
			""key"": ""boberly.lastingtonnamersen"",
			""accountId"": ""11235"",
			""name"": ""Boberly.LastingtonNamersen"",
			""avatarUrls"": {
				""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
				""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
				""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue"",
				""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue""
			},
			""displayName"": ""Boberly Namersen"",
			""active"": true
		},
		""issueTypes"": [
			{
				""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
				""id"": ""10004"",
				""description"": ""A task that needs to be done."",
				""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
				""name"": ""Task"",
				""subtask"": false,
				""avatarId"": 10318
			},
			{
				""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10005"",
				""id"": ""10005"",
				""description"": ""The sub-task of the issue"",
				""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10513&avatarType=issuetype"",
				""name"": ""Sub-task"",
				""subtask"": true,
				""avatarId"": 10513
			}
		],
		""name"": ""Business Project"",
		""avatarUrls"": {
			""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
			""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
			""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
			""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
		},
		""projectKeys"": [
			""BP""
		],
		""projectTypeKey"": ""business"",
		""simplified"": false,
		""style"": ""classic"",
		""isPrivate"": false
	}
]";

		#endregion

		#region JSON for nothing

		public const string JSONForNothing = "[]";

		#endregion

		#region JSON for One issue

		public const string JSONForOneIssue = @"
{
	""expand"": ""names,schema"",
	""startAt"": 0,
	""maxResults"": 1,
	""total"": 4,
	""issues"": [
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10054"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10054"",
			""key"": ""AVINA-4"",
			""renderedFields"": {
				""description"": ""<p>Description field but I changed it</p>"",
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10006"",
					""id"": ""10006"",
					""description"": ""jira.translation.issuetype.bug.name.desc"",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10303&avatarType=issuetype"",
					""name"": ""Bug"",
					""subtask"": false,
					""avatarId"": 10303
				},
				""timespent"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""customfield_10031"": null,
				""customfield_10032"": null,
				""fixVersions"": [],
				""customfield_10033"": null,
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""resolution"": null,
				""customfield_10035"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": -1,
				""lastViewed"": ""2019-02-04T11:53:42.059+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-4/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-02-04T11:26:19.923+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000jz:"",
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
					""name"": ""Medium"",
					""id"": ""3""
				},
				""customfield_10023"": null,
				""customfield_10024"": [],
				""customfield_10025"": null,
				""labels"": [
					""ProjectAdmin""
				],
				""customfield_10026"": null,
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},				
				""customfield_10019"": null,
				""aggregatetimeoriginalestimate"": null,
				""timeestimate"": null,
				""versions"": [],
				""issuelinks"": [],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-04T11:53:46.468+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Backlog"",
					""id"": ""10005"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [
					{
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/component/10021"",
						""id"": ""10021"",
						""name"": ""Phase 1"",
						""description"": ""The first phase""
					}
				],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""heading"",
							""attrs"": {
								""level"": 1
							},
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Description field but I changed it""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""customfield_10007"": null,
				""security"": null,
				""customfield_10008"": null,
				""aggregatetimeestimate"": null,
				""customfield_10009"": null,
				""summary"": ""Buge"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""20000"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""customfield_10043"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10000"": ""{}"",
				""customfield_10044"": null,
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 0
				},
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Environment Field""
								}
							]
						}
					]
				},
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-4/votes"",
					""votes"": 0,
					""hasVoted"": false
				}
			}
		}
	]
}";

		#endregion

		#region JSON for One issue with long key

		public const string JSONForOneIssue_LongKey = @"
{
	""expand"": ""names,schema"",
	""startAt"": 0,
	""maxResults"": 1,
	""total"": 4,
	""issues"": [
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10055"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10054"",
			""key"": ""AVINA-4"",
			""renderedFields"": {
				""description"": ""Description field but I changed it"",
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10006"",
					""id"": ""10006"",
					""description"": ""jira.translation.issuetype.bug.name.desc"",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10303&avatarType=issuetype"",
					""name"": ""Bug"",
					""subtask"": false,
					""avatarId"": 10303
				},
				""timespent"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""customfield_10031"": null,
				""customfield_10032"": null,
				""fixVersions"": [],
				""customfield_10033"": null,
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""resolution"": null,
				""customfield_10035"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": -1,
				""lastViewed"": ""2019-02-04T11:53:42.059+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-4/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-02-04T11:26:19.923+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000jz:"",
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
					""name"": ""Medium"",
					""id"": ""3""
				},
				""customfield_10023"": null,
				""customfield_10024"": [],
				""customfield_10025"": null,
				""labels"": [
					""ProjectAdmin""
				],
				""customfield_10026"": null,
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""aggregatetimeoriginalestimate"": null,
				""timeestimate"": null,
				""versions"": [],
				""issuelinks"": [],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-04T11:53:46.468+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Backlog"",
					""id"": ""10005"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [
					{
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/component/10021"",
						""id"": ""10021"",
						""name"": ""Phase 1"",
						""description"": ""The first phase""
					}
				],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""heading"",
							""attrs"": {
								""level"": 1
							},
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Description field but I changed it""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""customfield_10007"": null,
				""security"": null,
				""customfield_10008"": null,
				""aggregatetimeestimate"": null,
				""customfield_10009"": null,
				""summary"": ""VeryLong-VeryVery-Long-SuperDuperLong-WhyIsThisSoLong?-ThisCanNeverHappenSurely-LikeTypingSomethigThisLongIsActuallyDifficult"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""customfield_10043"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10000"": ""{}"",
				""customfield_10044"": null,
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 0
				},
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Environment Field""
								}
							]
						}
					]
				},
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-4/votes"",
					""votes"": 0,
					""hasVoted"": false
				}
			}
		}
	]
}";

		#endregion

		#region JSON for One issue with renderedFields

		public const string JSONForOneIssueWithRenderedFields = @"
{
	""expand"": ""names,schema"",
	""startAt"": 0,
	""maxResults"": 100,
	""total"": 1,
	""issues"": [
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10055"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10055"",
			""key"": ""AVINA-5"",
			""renderedFields"": {
				""issuetype"": null,
				""timespent"": null,
				""project"": null,
				""customfield_10031"": null,
				""customfield_10032"": null,
				""customfield_10033"": null,
				""fixVersions"": null,
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""resolution"": null,
				""customfield_10035"": null,
				""customfield_10036"": null,
				""customfield_10037"": """",
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": null,
				""lastViewed"": ""Today 3:31 PM"",
				""watches"": null,
				""created"": ""Today 3:01 PM"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": null,
				""customfield_10023"": null,
				""priority"": null,
				""customfield_10024"": null,
				""customfield_10025"": null,
				""customfield_10026"": null,
				""labels"": null,
				""customfield_10016"": null,
				""customfield_10017"": """",
				""customfield_10018"": null,
				""customfield_10019"": null,
				""aggregatetimeoriginalestimate"": null,
				""timeestimate"": null,
				""versions"": null,
				""issuelinks"": null,
				""assignee"": null,
				""updated"": ""Today 3:36 PM"",
				""status"": null,
				""components"": null,
				""timeoriginalestimate"": null,
				""description"": ""<p>This is my description. It is very descriptive. It has some <font color=\""#ff0000\"">red text</font> too&#33;<br/>\nAnd even a \\n second line&#33;</p>\n\n<p>A third line with gibberish;:'“,&lt;.&gt;/?[{]}&#124;`&#126;1&#33;2@3#4$5%6^7&amp;8*9(0)&#45;&#95;=&#43;</p>"",
				""customfield_10010"": null,
				""customfield_10014"": null,
				""timetracking"": {},
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""security"": null,
				""customfield_10007"": null,
				""customfield_10008"": null,
				""customfield_10009"": null,
				""aggregatetimeestimate"": null,
				""attachment"": [],
				""summary"": null,
				""creator"": null,
				""subtasks"": null,
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""reporter"": null,
				""customfield_10043"": null,
				""customfield_10000"": null,
				""customfield_10044"": null,
				""aggregateprogress"": null,
				""customfield_10045"": null,
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": """",
				""customfield_10039"": null,
				""environment"": """",
				""duedate"": null,
				""progress"": null,
				""votes"": null,
				""comment"": {
					""comments"": [
						{
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10055/comment/10045"",
							""id"": ""10045"",
							""author"": {
								""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
								""name"": ""Boberly.LastingtonNamersen"",
								""key"": ""boberly.lastingtonnamersen"",
								""accountId"": ""11235"",
								""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
								""avatarUrls"": {
									""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
									""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
									""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
									""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
								},
								""displayName"": ""Boberly Namersen"",
								""active"": true,
								""timeZone"": ""Australia/Sydney""
							},
							""body"": {
								""version"": 1,
								""type"": ""doc"",
								""content"": [
									{
										""type"": ""heading"",
										""attrs"": {
											""level"": 5
										},
										""content"": [
											{
												""type"": ""text"",
												""text"": ""hello"",
												""marks"": [
													{
														""type"": ""em""
													}
												]
											}
										]
									},
									{
										""type"": ""paragraph"",
										""content"": [
											{
												""type"": ""text"",
												""text"": ""this is"",
												""marks"": [
													{
														""type"": ""strong""
													},
													{
														""type"": ""em""
													}
												]
											},
											{
												""type"": ""hardBreak""
											},
											{
												""type"": ""text"",
												""text"": ""Text"",
												""marks"": [
													{
														""type"": ""strong""
													}
												]
											},
											{
												""type"": ""hardBreak""
											},
											{
												""type"": ""text"",
												""text"": ""\\:)"",
												""marks"": [
													{
														""type"": ""textColor"",
														""attrs"": {
															""color"": ""#ff5630""
														}
													}
												]
											}
										]
									}
								]
							},
							""updateAuthor"": {
								""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
								""name"": ""Boberly.LastingtonNamersen"",
								""key"": ""boberly.lastingtonnamersen"",
								""accountId"": ""11235"",
								""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
								""avatarUrls"": {
									""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
									""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
									""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
									""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
								},
								""displayName"": ""Boberly Namersen"",
								""active"": true,
								""timeZone"": ""Australia/Sydney""
							},
							""created"": ""Today 3:33 PM"",
							""updated"": ""Today 3:36 PM"",
							""jsdPublic"": true
						}
					],
					""maxResults"": 1,
					""total"": 1,
					""startAt"": 0
				},
				""worklog"": {
					""startAt"": 0,
					""maxResults"": 20,
					""total"": 0,
					""worklogs"": []
				}
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10006"",
					""id"": ""10006"",
					""description"": ""jira.translation.issuetype.bug.name.desc"",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10303&avatarType=issuetype"",
					""name"": ""Bug2"",
					""subtask"": false,
					""avatarId"": 10303
				},
				""timespent"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					}
				},
				""customfield_10031"": null,
				""customfield_10032"": null,
				""customfield_10033"": null,
				""fixVersions"": [],
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""resolution"": null,
				""customfield_10035"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": -1,
				""lastViewed"": ""2019-02-22T15:31:58.839+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-5/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-02-22T15:01:40.205+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000k7:"",
				""customfield_10023"": null,
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
					""name"": ""Medium"",
					""id"": ""3""
				},
				""customfield_10024"": [],
				""customfield_10025"": null,
				""customfield_10026"": null,
				""labels"": [],
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""aggregatetimeoriginalestimate"": null,
				""timeestimate"": null,
				""versions"": [],
				""issuelinks"": [],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-22T15:36:29.740+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Backlog"",
					""id"": ""10005"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Description field but I changed it""
								}
							]
						},
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""With a second line even!""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""timetracking"": {},
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""security"": null,
				""customfield_10007"": null,
				""customfield_10008"": null,
				""customfield_10009"": null,
				""aggregatetimeestimate"": null,
				""attachment"": [],
				""summary"": ""This is my summaroonie"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10043"": null,
				""customfield_10000"": ""{}"",
				""customfield_10044"": null,
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 0
				},
				""customfield_10045"": null,
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": null,
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-5/votes"",
					""votes"": 0,
					""hasVoted"": false
				},
				""comment"": {
					""comments"": [
						{
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10055/comment/10045"",
							""id"": ""10045"",
							""author"": {
								""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
								""name"": ""Boberly.LastingtonNamersen"",
								""key"": ""boberly.lastingtonnamersen"",
								""accountId"": ""11235"",
								""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
								""avatarUrls"": {
									""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
									""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
									""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
									""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
								},
								""displayName"": ""Boberly Namersen"",
								""active"": true,
								""timeZone"": ""Australia/Sydney""
							},
							""body"": {
								""version"": 1,
								""type"": ""doc"",
								""content"": [
									{
										""type"": ""heading"",
										""attrs"": {
											""level"": 5
										},
										""content"": [
											{
												""type"": ""text"",
												""text"": ""hello"",
												""marks"": [
													{
														""type"": ""em""
													}
												]
											}
										]
									},
									{
										""type"": ""paragraph"",
										""content"": [
											{
												""type"": ""text"",
												""text"": ""this is"",
												""marks"": [
													{
														""type"": ""strong""
													},
													{
														""type"": ""em""
													}
												]
											},
											{
												""type"": ""hardBreak""
											},
											{
												""type"": ""text"",
												""text"": ""Text"",
												""marks"": [
													{
														""type"": ""strong""
													}
												]
											},
											{
												""type"": ""hardBreak""
											},
											{
												""type"": ""text"",
												""text"": ""\\:)"",
												""marks"": [
													{
														""type"": ""textColor"",
														""attrs"": {
															""color"": ""#ff5630""
														}
													}
												]
											}
										]
									}
								]
							},
							""updateAuthor"": {
								""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
								""name"": ""Boberly.LastingtonNamersen"",
								""key"": ""boberly.lastingtonnamersen"",
								""accountId"": ""11235"",
								""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
								""avatarUrls"": {
									""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
									""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
									""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
									""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
								},
								""displayName"": ""Boberly Namersen"",
								""active"": true,
								""timeZone"": ""Australia/Sydney""
							},
							""created"": ""2019-02-22T15:33:56.141+1100"",
							""updated"": ""2019-02-22T15:36:29.725+1100"",
							""jsdPublic"": true
						}
					],
					""maxResults"": 1,
					""total"": 1,
					""startAt"": 0
				},
				""worklog"": {
					""startAt"": 0,
					""maxResults"": 20,
					""total"": 0,
					""worklogs"": []
				}
			}
		}
	]
}";

		#endregion

		#region JSON for One issue with renderedFields

		public const string JSONForOneIssueWithoutRenderedFields = @"
{
	""expand"": ""names,schema"",
	""startAt"": 0,
	""maxResults"": 100,
	""total"": 1,
	""issues"": [
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10055"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10055"",
			""key"": ""AVINA-5"",
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10006"",
					""id"": ""10006"",
					""description"": ""jira.translation.issuetype.bug.name.desc"",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10303&avatarType=issuetype"",
					""name"": ""Bug2"",
					""subtask"": false,
					""avatarId"": 10303
				},
				""timespent"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					}
				},
				""customfield_10031"": null,
				""customfield_10032"": null,
				""customfield_10033"": null,
				""fixVersions"": [],
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""resolution"": null,
				""customfield_10035"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": -1,
				""lastViewed"": ""2019-02-22T15:31:58.839+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-5/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-02-22T15:01:40.205+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000k7:"",
				""customfield_10023"": null,
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
					""name"": ""Medium"",
					""id"": ""3""
				},
				""customfield_10024"": [],
				""customfield_10025"": null,
				""customfield_10026"": null,
				""labels"": [],
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""aggregatetimeoriginalestimate"": null,
				""timeestimate"": null,
				""versions"": [],
				""issuelinks"": [],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-22T15:36:29.740+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Backlog"",
					""id"": ""10005"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""h1. Description field but I changed it""
								}
							]
						},
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""With a second line even! That is never seen...""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""timetracking"": {},
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""security"": null,
				""customfield_10007"": null,
				""customfield_10008"": null,
				""customfield_10009"": null,
				""aggregatetimeestimate"": null,
				""attachment"": [],
				""summary"": ""This is my summaroonie"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10043"": null,
				""customfield_10000"": ""{}"",
				""customfield_10044"": null,
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 0
				},
				""customfield_10045"": null,
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": null,
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-5/votes"",
					""votes"": 0,
					""hasVoted"": false
				},
				""comment"": {
					""comments"": [
						{
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10055/comment/10045"",
							""id"": ""10045"",
							""author"": {
								""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
								""name"": ""Boberly.LastingtonNamersen"",
								""key"": ""boberly.lastingtonnamersen"",
								""accountId"": ""11235"",
								""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
								""avatarUrls"": {
									""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
									""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
									""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
									""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
								},
								""displayName"": ""Boberly Namersen"",
								""active"": true,
								""timeZone"": ""Australia/Sydney""
							},
							""body"": {
								""version"": 1,
								""type"": ""doc"",
								""content"": [
									{
										""type"": ""heading"",
										""attrs"": {
											""level"": 5
										},
										""content"": [
											{
												""type"": ""text"",
												""text"": ""hello"",
												""marks"": [
													{
														""type"": ""em""
													}
												]
											}
										]
									},
									{
										""type"": ""paragraph"",
										""content"": [
											{
												""type"": ""text"",
												""text"": ""this is"",
												""marks"": [
													{
														""type"": ""strong""
													},
													{
														""type"": ""em""
													}
												]
											},
											{
												""type"": ""hardBreak""
											},
											{
												""type"": ""text"",
												""text"": ""Text"",
												""marks"": [
													{
														""type"": ""strong""
													}
												]
											},
											{
												""type"": ""hardBreak""
											},
											{
												""type"": ""text"",
												""text"": ""\\:)"",
												""marks"": [
													{
														""type"": ""textColor"",
														""attrs"": {
															""color"": ""#ff5630""
														}
													}
												]
											}
										]
									}
								]
							},
							""updateAuthor"": {
								""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
								""name"": ""Boberly.LastingtonNamersen"",
								""key"": ""boberly.lastingtonnamersen"",
								""accountId"": ""11235"",
								""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
								""avatarUrls"": {
									""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
									""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
									""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
									""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
								},
								""displayName"": ""Boberly Namersen"",
								""active"": true,
								""timeZone"": ""Australia/Sydney""
							},
							""created"": ""2019-02-22T15:33:56.141+1100"",
							""updated"": ""2019-02-22T15:36:29.725+1100"",
							""jsdPublic"": true
						}
					],
					""maxResults"": 1,
					""total"": 1,
					""startAt"": 0
				},
				""worklog"": {
					""startAt"": 0,
					""maxResults"": 20,
					""total"": 0,
					""worklogs"": []
				}
			}
		}
	]
}";

		#endregion

		#region JSON for Two issues

		public const string JSONForTwoIssues = @"
{
	""expand"": ""schema,names"",
	""startAt"": 0,
	""maxResults"": 50,
	""total"": 4,
	""issues"": [
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10054"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10054"",
			""key"": ""AVINA-4"",
			""renderedFields"": {
				""description"": ""Description field but I changed it"",
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10006"",
					""id"": ""10006"",
					""description"": ""jira.translation.issuetype.bug.name.desc"",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10303&avatarType=issuetype"",
					""name"": ""Bug"",
					""subtask"": false,
					""avatarId"": 10303
				},
				""timespent"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""customfield_10031"": null,
				""customfield_10032"": null,
				""customfield_10033"": null,
				""fixVersions"": [],
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""resolution"": null,
				""customfield_10035"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": -1,
				""lastViewed"": ""2019-02-05T17:21:24.470+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-4/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-02-04T11:26:19.923+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000jz:"",
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
					""name"": ""Medium"",
					""id"": ""3""
				},
				""customfield_10023"": null,
				""customfield_10024"": [],
				""customfield_10025"": null,
				""labels"": [
					""ProjectAdmin""
				],
				""customfield_10026"": null,
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""aggregatetimeoriginalestimate"": null,
				""timeestimate"": null,
				""versions"": [],
				""issuelinks"": [],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-04T11:53:46.468+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Backlog"",
					""id"": ""10005"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [
					{
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/component/10021"",
						""id"": ""10021"",
						""name"": ""Phase 1"",
						""description"": ""The first phase""
					}
				],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""heading"",
							""attrs"": {
								""level"": 1
							},
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Description field but I changed it""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""security"": null,
				""customfield_10007"": null,
				""customfield_10008"": null,
				""customfield_10009"": null,
				""aggregatetimeestimate"": null,
				""summary"": ""Buge"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10043"": null,
				""customfield_10044"": null,
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 0
				},
				""customfield_10000"": ""{}"",
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Environment Field""
								}
							]
						}
					]
				},
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-4/votes"",
					""votes"": 0,
					""hasVoted"": false
				}
			}
		},
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10044"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10044"",
			""key"": ""AVINA-3"",
			""renderedFields"": {
				""description"": ""They're really neat, make em shiny"",
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
					""id"": ""10004"",
					""description"": ""A task that needs to be done."",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
					""name"": ""Hugg"",
					""subtask"": false,
					""avatarId"": 10318
				},
				""timespent"": null,
				""customfield_10031"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""customfield_10032"": null,
				""fixVersions"": [],
				""customfield_10033"": null,
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""customfield_10035"": null,
				""resolution"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10027"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": -1,
				""lastViewed"": ""2019-02-04T11:58:44.118+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-3/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-01-22T09:51:16.172+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000hr:"",
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/2"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/high.svg"",
					""name"": ""High"",
					""id"": ""2""
				},
				""customfield_10023"": null,
				""customfield_10024"": [],
				""customfield_10025"": null,
				""labels"": [],
				""customfield_10026"": null,
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""timeestimate"": null,
				""aggregatetimeoriginalestimate"": null,
				""versions"": [],
				""issuelinks"": [
					{
						""id"": ""10008"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10008"",
						""type"": {
							""id"": ""10000"",
							""name"": ""Blocks"",
							""inward"": ""is blocked by"",
							""outward"": ""blocks"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10000""
						},
						""inwardIssue"": {
							""id"": ""10038"",
							""key"": ""SSP-19"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10038"",
							""fields"": {
								""summary"": ""As a user, I'd like a historical story to show in reports"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10007"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Done"",
									""id"": ""10007"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/3"",
										""id"": 3,
										""key"": ""done"",
										""colorName"": ""green"",
										""name"": ""Done""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
									""name"": ""Medium"",
									""id"": ""3""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10003"",
									""id"": ""10003"",
									""description"": ""Stories track functionality or features expressed as user goals."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10315&avatarType=issuetype"",
									""name"": ""Story"",
									""subtask"": false,
									""avatarId"": 10315
								}
							}
						}
					},
					{
						""id"": ""10006"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10006"",
						""type"": {
							""id"": ""10003"",
							""name"": ""Relates"",
							""inward"": ""relates to"",
							""outward"": ""relates to"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10003""
						},
						""inwardIssue"": {
							""id"": ""10043"",
							""key"": ""AVINA-2"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10043"",
							""fields"": {
								""summary"": ""Recruit Thor"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10006"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Selected for Development"",
									""id"": ""10006"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
										""id"": 2,
										""key"": ""new"",
										""colorName"": ""blue-gray"",
										""name"": ""To Do""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/5"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/lowest.svg"",
									""name"": ""Lowest"",
									""id"": ""5""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
									""id"": ""10004"",
									""description"": ""A task that needs to be done."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
									""name"": ""Task"",
									""subtask"": false,
									""avatarId"": 10318
								}
							}
						}
					},
					{
						""id"": ""10007"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10007"",
						""type"": {
							""id"": ""10003"",
							""name"": ""Relates"",
							""inward"": ""relates to"",
							""outward"": ""relates to"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10003""
						},
						""inwardIssue"": {
							""id"": ""10003"",
							""key"": ""AVINA-1"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10003"",
							""fields"": {
								""summary"": ""Get Iron Man Dude"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10007"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Done"",
									""id"": ""10007"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/3"",
										""id"": 3,
										""key"": ""done"",
										""colorName"": ""green"",
										""name"": ""Done""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
									""name"": ""Medium"",
									""id"": ""3""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
									""id"": ""10004"",
									""description"": ""A task that needs to be done."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
									""name"": ""Task"",
									""subtask"": false,
									""avatarId"": 10318
								}
							}
						}
					}
				],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=adminID"",
					""name"": ""admin"",
					""key"": ""admin"",
					""accountId"": ""adminID"",
					""emailAddress"": ""informationservicesaspacsyd@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/4c818ad9b612f10a9575ad4e43adc83b?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F4c818ad9b612f10a9575ad4e43adc83b%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/4c818ad9b612f10a9575ad4e43adc83b?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F4c818ad9b612f10a9575ad4e43adc83b%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/4c818ad9b612f10a9575ad4e43adc83b?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F4c818ad9b612f10a9575ad4e43adc83b%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/4c818ad9b612f10a9575ad4e43adc83b?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F4c818ad9b612f10a9575ad4e43adc83b%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Information Services"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-04T12:00:21.321+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Backlog"",
					""id"": ""10005"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""They're really neat, make em shiny""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""customfield_10007"": null,
				""security"": null,
				""customfield_10008"": null,
				""customfield_10009"": null,
				""aggregatetimeestimate"": null,
				""summary"": ""Make Helicarrier"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""customfield_10043"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10000"": ""{}"",
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 0
				},
				""customfield_10044"": null,
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": null,
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-3/votes"",
					""votes"": 0,
					""hasVoted"": false
				}
			}
		}
	]
}";

		#endregion

		#region JSON for Four issues (expand region at own risk)

		public const string JSONFourForIssues = @"
{
	""expand"": ""schema,names"",
	""startAt"": 0,
	""maxResults"": 50,
	""total"": 4,
	""issues"": [
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10054"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10054"",
			""key"": ""AVINA-4"",
			""renderedFields"": {
				""description"": ""Description field but I changed it"",
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10006"",
					""id"": ""10006"",
					""description"": ""jira.translation.issuetype.bug.name.desc"",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10303&avatarType=issuetype"",
					""name"": ""Bug"",
					""subtask"": false,
					""avatarId"": 10303
				},
				""timespent"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""customfield_10031"": null,
				""customfield_10032"": null,
				""customfield_10033"": null,
				""fixVersions"": [],
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""resolution"": null,
				""customfield_10035"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": -1,
				""lastViewed"": ""2019-02-05T17:21:24.470+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-4/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-02-04T11:26:19.923+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000jz:"",
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
					""name"": ""Medium"",
					""id"": ""3""
				},
				""customfield_10023"": null,
				""customfield_10024"": [],
				""customfield_10025"": null,
				""labels"": [
					""ProjectAdmin""
				],
				""customfield_10026"": null,
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""aggregatetimeoriginalestimate"": null,
				""timeestimate"": null,
				""versions"": [],
				""issuelinks"": [],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-04T11:53:46.468+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Backlog"",
					""id"": ""10005"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [
					{
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/component/10021"",
						""id"": ""10021"",
						""name"": ""Phase 1"",
						""description"": ""The first phase""
					}
				],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""heading"",
							""attrs"": {
								""level"": 1
							},
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Description field but I changed it""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""security"": null,
				""customfield_10007"": null,
				""customfield_10008"": null,
				""customfield_10009"": null,
				""aggregatetimeestimate"": null,
				""summary"": ""Buge"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10043"": null,
				""customfield_10044"": null,
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 0
				},
				""customfield_10000"": ""{}"",
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""customfield_10907"": {
					""self"": ""https://support.softship.com/jira/rest/api/2/customFieldOption/14804"",
					""value"": ""EDI & Services"",
					""id"": ""14804""
					},
				""environment"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Environment Field""
								}
							]
						}
					]
				},
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-4/votes"",
					""votes"": 0,
					""hasVoted"": false
				}
			}
		},
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10044"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10044"",
			""key"": ""AVINA-3"",
			""renderedFields"": {
				""description"": ""They're really neat, make em shiny"",
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
					""id"": ""10004"",
					""description"": ""A task that needs to be done."",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
					""name"": ""Task"",
					""subtask"": false,
					""avatarId"": 10318
				},
				""timespent"": null,
				""customfield_10031"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""customfield_10032"": null,
				""fixVersions"": [],
				""customfield_10033"": null,
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""customfield_10035"": null,
				""resolution"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10027"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": -1,
				""lastViewed"": ""2019-02-04T11:58:44.118+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-3/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-01-22T09:51:16.172+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000hr:"",
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/2"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/high.svg"",
					""name"": ""High"",
					""id"": ""2""
				},
				""customfield_10023"": null,
				""customfield_10024"": [],
				""customfield_10025"": null,
				""labels"": [],
				""customfield_10026"": null,
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""timeestimate"": null,
				""aggregatetimeoriginalestimate"": null,
				""versions"": [],
				""issuelinks"": [
					{
						""id"": ""10008"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10008"",
						""type"": {
							""id"": ""10000"",
							""name"": ""Blocks"",
							""inward"": ""is blocked by"",
							""outward"": ""blocks"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10000""
						},
						""inwardIssue"": {
							""id"": ""10038"",
							""key"": ""SSP-19"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10038"",
							""fields"": {
								""summary"": ""As a user, I'd like a historical story to show in reports"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10007"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Done"",
									""id"": ""10007"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/3"",
										""id"": 3,
										""key"": ""done"",
										""colorName"": ""green"",
										""name"": ""Done""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
									""name"": ""Medium"",
									""id"": ""3""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10003"",
									""id"": ""10003"",
									""description"": ""Stories track functionality or features expressed as user goals."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10315&avatarType=issuetype"",
									""name"": ""Story"",
									""subtask"": false,
									""avatarId"": 10315
								}
							}
						}
					},
					{
						""id"": ""10006"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10006"",
						""type"": {
							""id"": ""10003"",
							""name"": ""Relates"",
							""inward"": ""relates to"",
							""outward"": ""relates to"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10003""
						},
						""inwardIssue"": {
							""id"": ""10043"",
							""key"": ""AVINA-2"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10043"",
							""fields"": {
								""summary"": ""Recruit Thor"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10006"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Selected for Development"",
									""id"": ""10006"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
										""id"": 2,
										""key"": ""new"",
										""colorName"": ""blue-gray"",
										""name"": ""To Do""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/5"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/lowest.svg"",
									""name"": ""Lowest"",
									""id"": ""5""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
									""id"": ""10004"",
									""description"": ""A task that needs to be done."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
									""name"": ""Task"",
									""subtask"": false,
									""avatarId"": 10318
								}
							}
						}
					},
					{
						""id"": ""10007"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10007"",
						""type"": {
							""id"": ""10003"",
							""name"": ""Relates"",
							""inward"": ""relates to"",
							""outward"": ""relates to"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10003""
						},
						""inwardIssue"": {
							""id"": ""10003"",
							""key"": ""AVINA-1"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10003"",
							""fields"": {
								""summary"": ""Get Iron Man Dude"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10007"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Done"",
									""id"": ""10007"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/3"",
										""id"": 3,
										""key"": ""done"",
										""colorName"": ""green"",
										""name"": ""Done""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
									""name"": ""Medium"",
									""id"": ""3""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
									""id"": ""10004"",
									""description"": ""A task that needs to be done."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
									""name"": ""Task"",
									""subtask"": false,
									""avatarId"": 10318
								}
							}
						}
					}
				],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=adminID"",
					""name"": ""admin"",
					""key"": ""admin"",
					""accountId"": ""adminID"",
					""emailAddress"": ""informationservicesaspacsyd@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/4c818ad9b612f10a9575ad4e43adc83b?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F4c818ad9b612f10a9575ad4e43adc83b%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/4c818ad9b612f10a9575ad4e43adc83b?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F4c818ad9b612f10a9575ad4e43adc83b%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/4c818ad9b612f10a9575ad4e43adc83b?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F4c818ad9b612f10a9575ad4e43adc83b%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/4c818ad9b612f10a9575ad4e43adc83b?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F4c818ad9b612f10a9575ad4e43adc83b%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Information Services"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-04T12:00:21.321+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Backlog"",
					""id"": ""10005"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""They're really neat, make em shiny""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""customfield_10007"": null,
				""security"": null,
				""customfield_10008"": null,
				""customfield_10009"": null,
				""aggregatetimeestimate"": null,
				""summary"": ""Make Helicarrier"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""customfield_10043"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10000"": ""{}"",
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 0
				},
				""customfield_10044"": null,
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": null,
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-3/votes"",
					""votes"": 0,
					""hasVoted"": false
				}
			}
		},
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10043"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10043"",
			""key"": ""AVINA-2"",
			""renderedFields"": {
				""description"": ""Do the thing"",
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
					""id"": ""10004"",
					""description"": ""A task that needs to be done."",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
					""name"": ""Task"",
					""subtask"": false,
					""avatarId"": 10318
				},
				""timespent"": null,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""customfield_10031"": null,
				""customfield_10032"": null,
				""customfield_10033"": null,
				""fixVersions"": [],
				""aggregatetimespent"": null,
				""customfield_10034"": null,
				""customfield_10035"": null,
				""resolution"": null,
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10027"": null,
				""customfield_10028"": null,
				""resolutiondate"": null,
				""workratio"": 0,
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-2/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""lastViewed"": ""2019-02-04T11:21:18.115+1100"",
				""created"": ""2019-01-22T09:46:39.786+1100"",
				""customfield_10020"": null,
				""customfield_10021"": null,
				""customfield_10022"": ""0|i000hj:"",
				""customfield_10023"": null,
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/5"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/lowest.svg"",
					""name"": ""Lowest"",
					""id"": ""5""
				},
				""customfield_10024"": [],
				""customfield_10025"": null,
				""labels"": [
					""Cheeseburger"",
					""EnrolAvenger"",
					""TalkToPeople""
				],
				""customfield_10026"": null,
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""timeestimate"": 36000,
				""aggregatetimeoriginalestimate"": 36000,
				""versions"": [],
				""issuelinks"": [
					{
						""id"": ""10005"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10005"",
						""type"": {
							""id"": ""10000"",
							""name"": ""Blocks"",
							""inward"": ""is blocked by"",
							""outward"": ""blocks"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10000""
						},
						""inwardIssue"": {
							""id"": ""10003"",
							""key"": ""AVINA-1"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10003"",
							""fields"": {
								""summary"": ""Get Iron Man Dude"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10007"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Done"",
									""id"": ""10007"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/3"",
										""id"": 3,
										""key"": ""done"",
										""colorName"": ""green"",
										""name"": ""Done""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
									""name"": ""Medium"",
									""id"": ""3""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
									""id"": ""10004"",
									""description"": ""A task that needs to be done."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
									""name"": ""Task"",
									""subtask"": false,
									""avatarId"": 10318
								}
							}
						}
					},
					{
						""id"": ""10006"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10006"",
						""type"": {
							""id"": ""10003"",
							""name"": ""Relates"",
							""inward"": ""relates to"",
							""outward"": ""relates to"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10003""
						},
						""outwardIssue"": {
							""id"": ""10044"",
							""key"": ""AVINA-3"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10044"",
							""fields"": {
								""summary"": ""Make Helicarrier"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Backlog"",
									""id"": ""10005"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
										""id"": 2,
										""key"": ""new"",
										""colorName"": ""blue-gray"",
										""name"": ""To Do""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/2"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/high.svg"",
									""name"": ""High"",
									""id"": ""2""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
									""id"": ""10004"",
									""description"": ""A task that needs to be done."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
									""name"": ""Task"",
									""subtask"": false,
									""avatarId"": 10318
								}
							}
						}
					}
				],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=definitelyNotDaveID"",
					""name"": ""notDave.west"",
					""key"": ""definitelyNotDaveID"",
					""accountId"": ""definitelyNotDaveID"",
					""emailAddress"": ""notDave.west@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Dave West"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-04T11:18:05.937+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10006"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Selected for Development"",
					""id"": ""10006"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
						""id"": 2,
						""key"": ""new"",
						""colorName"": ""blue-gray"",
						""name"": ""To Do""
					}
				},
				""components"": [],
				""timeoriginalestimate"": 36000,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Do the thing""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""customfield_10007"": null,
				""security"": null,
				""customfield_10008"": null,
				""aggregatetimeestimate"": 36000,
				""customfield_10009"": null,
				""summary"": ""Recruit Thor"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""customfield_10043"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10044"": null,
				""customfield_10000"": ""{}"",
				""aggregateprogress"": {
					""progress"": 0,
					""total"": 36000,
					""percent"": 0
				},
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": null,
				""duedate"": null,
				""progress"": {
					""progress"": 0,
					""total"": 36000,
					""percent"": 0
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-2/votes"",
					""votes"": 0,
					""hasVoted"": false
				}
			}
		},
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""10003"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10003"",
			""key"": ""AVINA-1"",
			""renderedFields"": {
				""description"": ""Gimmie cheeseburger Obadiah"",
			},
			""fields"": {
				""issuetype"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
					""id"": ""10004"",
					""description"": ""A task that needs to be done."",
					""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
					""name"": ""Task"",
					""subtask"": false,
					""avatarId"": 10318
				},
				""timespent"": 115200,
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""customfield_10031"": null,
				""customfield_10032"": null,
				""customfield_10033"": null,
				""fixVersions"": [],
				""customfield_10034"": null,
				""aggregatetimespent"": 115200,
				""customfield_10035"": null,
				""resolution"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/resolution/10000"",
					""id"": ""10000"",
					""description"": ""Work has been completed on this issue."",
					""name"": ""Done""
				},
				""customfield_10036"": null,
				""customfield_10037"": null,
				""customfield_10027"": null,
				""customfield_10028"": null,
				""resolutiondate"": ""2019-02-04T11:03:18.270+1100"",
				""workratio"": -1,
				""lastViewed"": ""2019-02-04T11:16:21.901+1100"",
				""watches"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-1/watchers"",
					""watchCount"": 1,
					""isWatching"": true
				},
				""created"": ""2019-01-11T08:50:45.203+1100"",
				""customfield_10020"": ""10007_*:*_1_*:*_0_*|*_3_*:*_2_*:*_5335_*|*_10006_*:*_2_*:*_1649_*|*_10005_*:*_4_*:*_2081546093"",
				""customfield_10021"": null,
				""customfield_10022"": ""0|i0000n:"",
				""customfield_10023"": null,
				""priority"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/3"",
					""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
					""name"": ""Medium"",
					""id"": ""3""
				},
				""customfield_10024"": [],
				""customfield_10025"": null,
				""labels"": [
					""Cheeseburger"",
					""EnrolAvenger"",
					""TalkToPeople""
				],
				""customfield_10026"": null,
				""customfield_10016"": null,
				""customfield_10017"": null,
				""customfield_10018"": {
					""hasEpicLinkFieldDependency"": false,
					""showField"": false,
					""nonEditableReason"": {
						""reason"": ""PLUGIN_LICENSE_ERROR"",
						""message"": ""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"": null,
				""aggregatetimeoriginalestimate"": null,
				""timeestimate"": 0,
				""versions"": [],
				""issuelinks"": [
					{
						""id"": ""10005"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10005"",
						""type"": {
							""id"": ""10000"",
							""name"": ""Blocks"",
							""inward"": ""is blocked by"",
							""outward"": ""blocks"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10000""
						},
						""outwardIssue"": {
							""id"": ""10043"",
							""key"": ""AVINA-2"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10043"",
							""fields"": {
								""summary"": ""Recruit Thor"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10006"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Selected for Development"",
									""id"": ""10006"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
										""id"": 2,
										""key"": ""new"",
										""colorName"": ""blue-gray"",
										""name"": ""To Do""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/5"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/lowest.svg"",
									""name"": ""Lowest"",
									""id"": ""5""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
									""id"": ""10004"",
									""description"": ""A task that needs to be done."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
									""name"": ""Task"",
									""subtask"": false,
									""avatarId"": 10318
								}
							}
						}
					},
					{
						""id"": ""10007"",
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLink/10007"",
						""type"": {
							""id"": ""10003"",
							""name"": ""Relates"",
							""inward"": ""relates to"",
							""outward"": ""relates to"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issueLinkType/10003""
						},
						""outwardIssue"": {
							""id"": ""10044"",
							""key"": ""AVINA-3"",
							""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/10044"",
							""fields"": {
								""summary"": ""Make Helicarrier"",
								""status"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10005"",
									""description"": """",
									""iconUrl"": ""https://sampleweb.atlassian.net/"",
									""name"": ""Backlog"",
									""id"": ""10005"",
									""statusCategory"": {
										""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/2"",
										""id"": 2,
										""key"": ""new"",
										""colorName"": ""blue-gray"",
										""name"": ""To Do""
									}
								},
								""priority"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/priority/2"",
									""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/priorities/high.svg"",
									""name"": ""High"",
									""id"": ""2""
								},
								""issuetype"": {
									""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
									""id"": ""10004"",
									""description"": ""A task that needs to be done."",
									""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
									""name"": ""Task"",
									""subtask"": false,
									""avatarId"": 10318
								}
							}
						}
					}
				],
				""assignee"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""updated"": ""2019-02-04T11:18:01.541+1100"",
				""status"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/status/10007"",
					""description"": """",
					""iconUrl"": ""https://sampleweb.atlassian.net/"",
					""name"": ""Done"",
					""id"": ""10007"",
					""statusCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/statuscategory/3"",
						""id"": 3,
						""key"": ""done"",
						""colorName"": ""green"",
						""name"": ""Done""
					}
				},
				""components"": [],
				""timeoriginalestimate"": null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""Gimmie cheeseburger Obadiah""
								}
							]
						}
					]
				},
				""customfield_10010"": null,
				""customfield_10014"": null,
				""customfield_10015"": null,
				""customfield_10005"": null,
				""customfield_10006"": null,
				""customfield_10007"": null,
				""security"": null,
				""customfield_10008"": null,
				""customfield_10009"": null,
				""aggregatetimeestimate"": 0,
				""summary"": ""Get Iron Man Dude"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""subtasks"": [],
				""customfield_10040"": null,
				""customfield_10041"": null,
				""customfield_10042"": null,
				""customfield_10043"": null,
				""reporter"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""customfield_10044"": null,
				""customfield_10000"": ""{}"",
				""aggregateprogress"": {
					""progress"": 115200,
					""total"": 115200,
					""percent"": 100
				},
				""customfield_10001"": null,
				""customfield_10002"": null,
				""customfield_10003"": null,
				""customfield_10004"": null,
				""customfield_10038"": null,
				""customfield_10039"": null,
				""environment"": null,
				""duedate"": null,
				""progress"": {
					""progress"": 115200,
					""total"": 115200,
					""percent"": 100
				},
				""votes"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/AVINA-1/votes"",
					""votes"": 0,
					""hasVoted"": false
				}
			}
		}
	]
}";

		#endregion

		#region JSON for One user

		public const string JSONForOneUser = @"
{
	""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
	""key"": ""boberly.lastingtonnamersen"",
	""accountId"": ""11235"",
	""name"": ""Boberly.LastingtonNamersen"",
	""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
	""avatarUrls"": {
		""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
		""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
		""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue"",
		""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue""
	},
	""displayName"": ""Boberly Namersen"",
	""active"": true,
	""timeZone"": ""Australia/Sydney"",
	""locale"": ""en_US"",
	""groups"": {
		""size"": 6,
		""items"": []
	},
	""applicationRoles"": {
		""size"": 3,
		""items"": []
	},
	""expand"": ""groups,applicationRoles""
}";

		#endregion

		#region JSON for arbitrary number of issues

		public static string GetJsonWithSpecifiedNumberOfIssues(int numberOfIssues, bool includeAttachments, Tuple<string, string>[] extraProjects = null)
		{
			const string baseJson =
@"{
	""expand"": ""schema,names"",
	""startAt"": 0,
	""maxResults"": 50,
	""total"": 4,
	""issues"": [
		ISSUESGOHERE
	]
}";

			string getIssueJson(string project, string id)
			{
				return @"
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""ISSUENUMBER"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/ISSUENUMBER"",
			""key"": """ + project + @"-ISSUENUMBER"",
			""renderedFields"": {
				""description"": ""Description field but I changed it"",
			},
			""fields"": {
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": """ + id + @""",
					""key"": """ + project + @""",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""summary"": ""Buge"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
				""attachment"": [ATTACHMENTSGOHERE],
			""comment"":{  
				""comments"":[
				{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/issue/10054/comment/10035"",
					""id"":""10035"",
					""author"":{  
						""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
						""name"":""Boberly.LastingtonNamersen"",
						""key"":""boberly.lastingtonnamersen"",
						""accountId"":""11235"",
						""emailAddress"":""janmichaelvincent@xyz.com"",
						""avatarUrls"":{  
							""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						},
						""displayName"":""Boberly Namersen"",
						""active"":true,
						""timeZone"":""Australia/Sydney""
					},
					""body"": {
							""version"": 1,
							""type"": ""doc"",
							""content"": [
							  {
								""type"": ""heading"",
								""attrs"": {
								  ""level"": 5
								},
								""content"": [
								  {
									""type"": ""text"",
									""text"": ""another thingie""
								  }
								]
							  }
							]
						  },
					""updateAuthor"":{  
						""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
						""name"":""Boberly.LastingtonNamersen"",
						""key"":""boberly.lastingtonnamersen"",
						""accountId"":""11235"",
						""emailAddress"":""janmichaelvincent@xyz.com"",
						""avatarUrls"":{  
							""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						},
						""displayName"":""Boberly Namersen"",
						""active"":true,
						""timeZone"":""Australia/Sydney""
					},
					""created"":""2019-02-04T11:46:20.965+1100"",
					""updated"":""2019-02-04T11:46:20.965+1100"",
					""jsdPublic"":true
				},
				{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/issue/10054/comment/10037"",
					""id"":""10037"",
					""author"":{  
						""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
						""name"":""Boberly.LastingtonNamersen"",
						""key"":""boberly.lastingtonnamersen"",
						""accountId"":""11235"",
						""emailAddress"":""mrssullivan@xyz.com"",
						""avatarUrls"":{  
							""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						},
						""displayName"":""Boberly Namersen"",
						""active"":true,
						""timeZone"":""Australia/Sydney""
					},
					""body"": {
							""version"": 1,
							""type"": ""doc"",
							""content"": [
							  {
								""type"": ""heading"",
								""attrs"": {
								  ""level"": 5
								},
								""content"": [
								  {
									""type"": ""text"",
									""text"": ""yet another thingie""
								  }
								]
							  }
							]
						  },
					""updateAuthor"":{  
						""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
						""name"":""Boberly.LastingtonNamersen"",
						""key"":""boberly.lastingtonnamersen"",
						""accountId"":""11235"",
						""emailAddress"":""mrssullivan@xyz.com"",
						""avatarUrls"":{  
							""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						},
						""displayName"":""Boberly Namersen"",
						""active"":true,
						""timeZone"":""Australia/Sydney""
					},
					""created"":""2019-02-04T11:53:46.468+1100"",
					""updated"":""2019-02-04T11:53:46.468+1100"",
					""jsdPublic"":true
				}
				],
				""maxResults"":2,
				""total"":2,
				""startAt"":0
			},
			}
		}";
			}

			var issuesJson = Enumerable.Range(0, numberOfIssues).Select(i => getIssueJson("AVINA", "10001").Replace("ISSUENUMBER", (10000 + i).ToString())).ToList();

			if (extraProjects != null)
			{
				int issuesTotal = numberOfIssues + 1; // start at id after previous issue

				foreach (var projectPair in extraProjects)
				{
					var additionalIssuesJson = Enumerable.Range(0, numberOfIssues).Select(i => getIssueJson(projectPair.Item1, projectPair.Item2).Replace("ISSUENUMBER", (10000 + i + issuesTotal).ToString())).ToList();
					issuesJson.AddRange(additionalIssuesJson);
					issuesTotal += numberOfIssues;
				}
			}

			var newBaseJson = baseJson.Replace("ISSUESGOHERE", string.Join(",\r\n", issuesJson));

			return includeAttachments ? newBaseJson.Replace("ATTACHMENTSGOHERE", AttachmentArray) : newBaseJson.Replace("ATTACHMENTSGOHERE", "");
		}

		const string AttachmentArray = @"
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10009"",
						""id"":""10009"",
						""filename"":""harrharrharr.txt"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-02-11T14:07:56.614+1100"",
						""size"":11,
						""mimeType"":""text/plain"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10009/harrharrharr.txt""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10008"",
						""id"":""10008"",
						""filename"":""Microsoft Edge.lnk"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-02-11T14:07:53.272+1100"",
						""size"":1417,
						""mimeType"":""application/x-ms-shortcut"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10008/Microsoft+Edge.lnk""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10001"",
						""id"":""10001"",
						""filename"":""pOOOOOOrg.tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
						   ""name"":""Boberly.LastingtonNamersen"",
						   ""key"":""boberly.lastingtonnamersen"",
						   ""accountId"":""11235"",
						   ""emailAddress"":""boberly.lastingtonnamersen@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Boberly Namersen"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-01-22T09:46:00.144+1100"",
						""size"":161822,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10001/pOOOOOOrg.tga"",
						""thumbnail"":""https://sampleweb.atlassian.net/secure/thumbnail/10001/pOOOOOOrg.tga""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10006"",
						""id"":""10006"",
						""filename"":""Wireframe - Workflow Details.tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""size"":48274,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10006/Wireframe+-+Workflow+Details.tga""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10007"",
						""id"":""10007"",
						""filename"":""Wireframe - Workflow Details (ba7209e3-1232-49da-abd2-cada0846c7d4).tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""size"":48274,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10007/Wireframe+-+Workflow+Details+%28ba7209e3-1232-49da-abd2-cada0846c7d4%29.tga""
					 }";

		#endregion

		#region JSON for issues with attachments

		public const string JSONWithIssueAndAttachments =
@"{
	""expand"": ""names,schema"",
	""startAt"": 0,
	""maxResults"": 1,
	""total"": 4,
	""issues"": [
		{
			""expand"": ""operations,versionedRepresentations,editmeta,changelog,renderedFields"",
			""id"": ""ISSUENUMBER"",
			""self"": ""https://sampleweb.atlassian.net/rest/api/3/issue/ISSUENUMBER"",
			""key"": ""AVINA-4"",
			""renderedFields"": {
				""description"": ""Description field but I changed it"",
			},
			""fields"": {
				""project"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
					""id"": ""10001"",
					""key"": ""AVINA"",
					""name"": ""Avengers Initiative"",
					""projectTypeKey"": ""software"",
					""avatarUrls"": {
						""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"": {
						""self"": ""https://sampleweb.atlassian.net/rest/api/3/projectCategory/10000"",
						""id"": ""10000"",
						""description"": ""Project that is custom"",
						""name"": ""Prooject""
					}
				},
				""attachment"":[  
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10009"",
						""id"":""10009"",
						""filename"":""harrharrharr.txt"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-02-11T14:07:56.614+1100"",
						""size"":11,
						""mimeType"":""text/plain"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10009/harrharrharr.txt""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10008"",
						""id"":""10008"",
						""filename"":""Microsoft Edge.lnk"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-02-11T14:07:53.272+1100"",
						""size"":1417,
						""mimeType"":""application/x-ms-shortcut"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10008/Microsoft+Edge.lnk""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10001"",
						""id"":""10001"",
						""filename"":""pOOOOOOrg.tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
						   ""name"":""Boberly.LastingtonNamersen"",
						   ""key"":""boberly.lastingtonnamersen"",
						   ""accountId"":""11235"",
						   ""emailAddress"":""boberly.lastingtonnamersen@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Boberly Namersen"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-01-22T09:46:00.144+1100"",
						""size"":161822,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10001/pOOOOOOrg.tga"",
						""thumbnail"":""https://sampleweb.atlassian.net/secure/thumbnail/10001/pOOOOOOrg.tga""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10006"",
						""id"":""10006"",
						""filename"":""Wireframe - Workflow Details.tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""size"":48274,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10006/Wireframe+-+Workflow+Details.tga""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10007"",
						""id"":""10007"",
						""filename"":""Wireframe - Workflow Details (ba7209e3-1232-49da-abd2-cada0846c7d4).tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""size"":48274,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10007/Wireframe+-+Workflow+Details+%28ba7209e3-1232-49da-abd2-cada0846c7d4%29.tga""
					 }
				],
				""summary"": ""Buge"",
				""creator"": {
					""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
					""name"": ""Boberly.LastingtonNamersen"",
					""key"": ""boberly.lastingtonnamersen"",
					""accountId"": ""11235"",
					""emailAddress"": ""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"": {
						""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"": ""Boberly Namersen"",
					""active"": true,
					""timeZone"": ""Australia/Sydney""
				},
			}
		}
	]
}
";

		#endregion

		#region JSON For Issue With Comments

		public const string JSONForIssueWithComments = @"
{
	""expand"": ""names,schema"",
	""startAt"": 0,
	""maxResults"": 1,
	""total"": 4,
	""issues"": [
		{
		""expand"":""renderedFields,names,schema,operations,editmeta,changelog,versionedRepresentations"",
			""id"":""10054"",
			""self"":""https://sampleweb.atlassian.net/rest/api/2/issue/10054"",
			""key"":""AVINA-4"",
			""renderedFields"": {
				""description"": ""<p>Description field but I changed it</p>"",
			},
			""fields"":{  
				""issuetype"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/issuetype/10006"",
					""id"":""10006"",
					""description"":""jira.translation.issuetype.bug.name.desc"",
					""iconUrl"":""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10303&avatarType=issuetype"",
					""name"":""Bug2"",
					""subtask"":false,
					""avatarId"":10303
				},
				""timespent"":null,
				""project"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/project/10001"",
					""id"":""10008"",
					""key"":""BP"",
					""name"":""Avengers Initiative"",
					""projectTypeKey"":""software"",
					""avatarUrls"":{  
						""48x48"":""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
						""24x24"":""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
						""16x16"":""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
						""32x32"":""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
					},
					""projectCategory"":{  
						""self"":""https://sampleweb.atlassian.net/rest/api/2/projectCategory/10000"",
						""id"":""10000"",
						""description"":""Project that is custom"",
						""name"":""Prooject""
					}
				},
				""customfield_10031"":null,
				""customfield_10032"":null,
				""fixVersions"":[


					],
				""customfield_10033"":null,
				""aggregatetimespent"":null,
				""customfield_10034"":null,
				""customfield_10035"":null,
				""resolution"":null,
				""customfield_10036"":null,
				""customfield_10037"":null,
				""customfield_10028"":null,
				""resolutiondate"":null,
				""workratio"":-1,
				""lastViewed"":""2019-02-11T14:59:05.695+1100"",
				""watches"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/issue/AVINA-4/watchers"",
					""watchCount"":1,
					""isWatching"":true
				},
				""created"":""2019-02-04T11:26:19.923+1100"",
				""customfield_10020"":null,
				""customfield_10021"":null,
				""customfield_10022"":""0|i000jz:"",
				""customfield_10023"":null,
				""priority"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/priority/3"",
					""iconUrl"":""https://sampleweb.atlassian.net/images/icons/priorities/medium.svg"",
					""name"":""Medium"",
					""id"":""3""
				},
				""customfield_10024"":[


					],
				""customfield_10025"":null,
				""labels"":[
				""ProjectAdmin""
					],
				""customfield_10026"":null,
				""customfield_10016"":null,
				""customfield_10017"":null,
				""customfield_10018"":{  
					""hasEpicLinkFieldDependency"":false,
					""showField"":false,
					""nonEditableReason"":{  
						""reason"":""PLUGIN_LICENSE_ERROR"",
						""message"":""Portfolio for Jira must be licensed for the Parent Link to be available.""
					}
				},
				""customfield_10019"":null,
				""timeestimate"":null,
				""aggregatetimeoriginalestimate"":null,
				""versions"":[


					],
				""issuelinks"":[


					],
				""assignee"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
					""name"":""Boberly.LastingtonNamersen"",
					""key"":""boberly.lastingtonnamersen"",
					""accountId"":""11235"",
					""emailAddress"":""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"":{  
						""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"":""Boberly Namersen"",
					""active"":true,
					""timeZone"":""Australia/Sydney""
				},
				""updated"":""2019-02-04T11:53:46.468+1100"",
				""status"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/status/10005"",
					""description"":"""",
					""iconUrl"":""https://sampleweb.atlassian.net/"",
					""name"":""Backlog"",
					""id"":""10005"",
					""statusCategory"":{  
						""self"":""https://sampleweb.atlassian.net/rest/api/2/statuscategory/2"",
						""id"":2,
						""key"":""new"",
						""colorName"":""blue-gray"",
						""name"":""To Do""
					}
				},
				""components"":[
				{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/component/10021"",
					""id"":""10021"",
					""name"":""Phase 1"",
					""description"":""The first phase""

				}
				],
				""timeoriginalestimate"":null,
				""description"": {
					""version"": 1,
					""type"": ""doc"",
					""content"": [
						{
							""type"": ""paragraph"",
							""content"": [
								{
									""type"": ""text"",
									""text"": ""h1. Description field but I changed it""
								}
							]
						}
					]
				},
				""customfield_10010"":null,
				""customfield_10014"":null,
				""timetracking"":{  

				},
				""customfield_10015"":null,
				""customfield_10005"":null,
				""customfield_10006"":null,
				""customfield_10007"":null,
				""security"":null,
				""customfield_10008"":null,
				""attachment"":[  
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10009"",
						""id"":""10009"",
						""filename"":""harrharrharr.txt"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-02-11T14:07:56.614+1100"",
						""size"":11,
						""mimeType"":""text/plain"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10009/harrharrharr.txt""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10008"",
						""id"":""10008"",
						""filename"":""Microsoft Edge.lnk"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-02-11T14:07:53.272+1100"",
						""size"":1417,
						""mimeType"":""application/x-ms-shortcut"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10008/Microsoft+Edge.lnk""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10001"",
						""id"":""10001"",
						""filename"":""pOOOOOOrg.tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=11235"",
						   ""name"":""Boberly.LastingtonNamersen"",
						   ""key"":""boberly.lastingtonnamersen"",
						   ""accountId"":""11235"",
						   ""emailAddress"":""boberly.lastingtonnamersen@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Boberly Namersen"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-01-22T09:46:00.144+1100"",
						""size"":161822,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10001/pOOOOOOrg.tga"",
						""thumbnail"":""https://sampleweb.atlassian.net/secure/thumbnail/10001/pOOOOOOrg.tga""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10006"",
						""id"":""10006"",
						""filename"":""Wireframe - Workflow Details.tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""size"":48274,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10006/Wireframe+-+Workflow+Details.tga""
					 },
					 {  
						""self"":""https://sampleweb.atlassian.net/rest/api/3/attachment/10007"",
						""id"":""10007"",
						""filename"":""Wireframe - Workflow Details (ba7209e3-1232-49da-abd2-cada0846c7d4).tga"",
						""author"":{  
						   ""self"":""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c05a6763acad8551c3364f8"",
						   ""name"":""dave.east"",
						   ""key"":""5c05a6763acad8551c3364f8"",
						   ""accountId"":""5c05a6763acad8551c3364f8"",
						   ""emailAddress"":""dave.east@sampleweb.com"",
						   ""avatarUrls"":{  
							  ""48x48"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
							  ""24x24"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
							  ""16x16"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
							  ""32x32"":""https://avatar-cdn.atlassian.com/84558e32992ecb4822fc6355568f7cf2?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F84558e32992ecb4822fc6355568f7cf2%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
						   },
						   ""displayName"":""Dave East"",
						   ""active"":true,
						   ""timeZone"":""Australia/Sydney""
						},
						""size"":48274,
						""mimeType"":""image/tga"",
						""content"":""https://sampleweb.atlassian.net/secure/attachment/10007/Wireframe+-+Workflow+Details+%28ba7209e3-1232-49da-abd2-cada0846c7d4%29.tga""
					 }
					],
				""aggregatetimeestimate"":null,
				""customfield_10009"":null,
				""summary"":""Buge"",
				""creator"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
					""name"":""Boberly.LastingtonNamersen"",
					""key"":""boberly.lastingtonnamersen"",
					""accountId"":""11235"",
					""emailAddress"":""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"":{  
						""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"":""Boberly Namersen"",
					""active"":true,
					""timeZone"":""Australia/Sydney""
				},
				""subtasks"":[


					],
				""customfield_10040"":null,
				""customfield_10041"":null,
				""customfield_10042"":null,
				""reporter"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
					""name"":""Boberly.LastingtonNamersen"",
					""key"":""boberly.lastingtonnamersen"",
					""accountId"":""11235"",
					""emailAddress"":""boberly.lastingtonnamersen@sampleweb.com"",
					""avatarUrls"":{  
						""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
						""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
						""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
						""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
					},
					""displayName"":""Boberly Namersen"",
					""active"":true,
					""timeZone"":""Australia/Sydney""
				},
				""customfield_10043"":null,
				""customfield_10000"":""{}"",
				""customfield_10044"":null,
				""aggregateprogress"":{  
					""progress"":0,
					""total"":0
				},
				""customfield_10045"":null,
				""customfield_10001"":null,
				""customfield_10002"":null,
				""customfield_10003"":null,
				""customfield_10004"":null,
				""customfield_10038"":null,
				""customfield_10039"":null,
				""environment"":""Environment Field"",
				""duedate"":null,
				""progress"":{  
					""progress"":0,
					""total"":0
				},
				""votes"":{  
					""self"":""https://sampleweb.atlassian.net/rest/api/2/issue/AVINA-4/votes"",
					""votes"":0,
					""hasVoted"":false
				},
				""comment"":{  
					""comments"":[
					{  
						""self"":""https://sampleweb.atlassian.net/rest/api/2/issue/10054/comment/10035"",
						""id"":""10035"",
						""author"":{  
							""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
							""name"":""Boberly.LastingtonNamersen"",
							""key"":""boberly.lastingtonnamersen"",
							""accountId"":""11235"",
							""emailAddress"":""janmichaelvincent@xyz.com"",
							""avatarUrls"":{  
								""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
								""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
								""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
								""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
							},
							""displayName"":""Boberly Namersen"",
							""active"":true,
							""timeZone"":""Australia/Sydney""
						},
						""body"": {
							""version"": 1,
							""type"": ""doc"",
							""content"": [
							  {
								""type"": ""heading"",
								""attrs"": {
								  ""level"": 5
								},
								""content"": [
								  {
									""type"": ""text"",
									""text"": ""another thingie""
								  }
								]
							  }
							]
						  },
						""updateAuthor"":{  
							""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
							""name"":""Boberly.LastingtonNamersen"",
							""key"":""boberly.lastingtonnamersen"",
							""accountId"":""11235"",
							""emailAddress"":""janmichaelvincent@xyz.com"",
							""avatarUrls"":{  
								""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
								""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
								""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
								""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
							},
							""displayName"":""Boberly Namersen"",
							""active"":true,
							""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-02-04T11:46:20.965+1100"",
						""updated"":""2019-02-04T11:46:20.965+1100"",
						""jsdPublic"":true
					},
					{  
						""self"":""https://sampleweb.atlassian.net/rest/api/2/issue/10054/comment/10037"",
						""id"":""10037"",
						""author"":{  
							""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
							""name"":""Boberly.LastingtonNamersen"",
							""key"":""boberly.lastingtonnamersen"",
							""accountId"":""11235"",
							""emailAddress"":""mrssullivan@xyz.com"",
							""avatarUrls"":{  
								""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
								""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
								""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
								""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
							},
							""displayName"":""Boberly Namersen"",
							""active"":true,
							""timeZone"":""Australia/Sydney""
						},
						""body"": {
							""version"": 1,
							""type"": ""doc"",
							""content"": [
							  {
								""type"": ""heading"",
								""attrs"": {
								  ""level"": 5
								},
								""content"": [
								  {
									""type"": ""text"",
									""text"": ""yet another thingie""
								  }
								]
							  }
							]
						  },
						""updateAuthor"":{  
							""self"":""https://sampleweb.atlassian.net/rest/api/2/user?accountId=11235"",
							""name"":""Boberly.LastingtonNamersen"",
							""key"":""boberly.lastingtonnamersen"",
							""accountId"":""11235"",
							""emailAddress"":""mrssullivan@xyz.com"",
							""avatarUrls"":{  
								""48x48"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue"",
								""24x24"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
								""16x16"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
								""32x32"":""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue""
							},
							""displayName"":""Boberly Namersen"",
							""active"":true,
							""timeZone"":""Australia/Sydney""
						},
						""created"":""2019-02-04T11:53:46.468+1100"",
						""updated"":""2019-02-04T11:53:46.468+1100"",
						""jsdPublic"":true
					}
					],
					""maxResults"":2,
					""total"":2,
					""startAt"":0
				},
				""worklog"":{  
					""startAt"":0,
					""maxResults"":20,
					""total"":0,
					""worklogs"":[


						]
				}
			}
		}
	]
}
";

		#endregion

		#region JSON For Multiple Projects

		public const string JSONForFourProjects = @"
[
  {
    ""expand"": ""description,lead,issueTypes,url,projectKeys"",
    ""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10001"",
    ""id"": ""10001"",
    ""key"": ""AVINA"",
    ""description"": ""hello I am descriptive"",
    ""lead"": {
      ""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c37b0c3d650366f5317227e"",
      ""key"": ""jan.michael.vincent"",
      ""accountId"": ""5c37b0c3d650366f5317227e"",
      ""name"": ""jan.michael.vincent"",
      ""avatarUrls"": {
        ""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
        ""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
        ""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue"",
        ""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue""
      },
      ""displayName"": ""Jan Michael Vincent"",
      ""active"": true
    },
    ""issueTypes"": [
      {
        ""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10000"",
        ""id"": ""10000"",
        ""description"": ""A big user story that needs to be broken down. Created by Jira Software - do not edit or delete."",
        ""iconUrl"": ""https://sampleweb.atlassian.net/images/icons/issuetypes/epic.svg"",
        ""name"": ""Epic"",
        ""subtask"": false
      }
    ],
    ""name"": ""Avengers Initiative"",
    ""avatarUrls"": {
      ""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
      ""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
      ""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
      ""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
    },
    ""projectKeys"": [
      ""AVIN"",
      ""AVINA""
    ],
    ""projectTypeKey"": ""software"",
    ""simplified"": false,
    ""style"": ""classic"",
    ""isPrivate"": false
  },
  {
    ""expand"": ""description,lead,issueTypes,url,projectKeys"",
    ""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008"",
    ""id"": ""10008"",
    ""key"": ""BP"",
    ""description"": """",
    ""lead"": {
      ""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c37b0c3d650366f5317227e"",
      ""key"": ""jan.michael.vincent"",
      ""accountId"": ""5c37b0c3d650366f5317227e"",
      ""name"": ""jan.michael.vincent"",
      ""avatarUrls"": {
        ""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
        ""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
        ""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue"",
        ""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue""
      },
      ""displayName"": ""Daniel Smerdely"",
      ""active"": true
    },
    ""issueTypes"": [
      {
        ""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
        ""id"": ""10004"",
        ""description"": ""A task that needs to be done."",
        ""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
        ""name"": ""Task"",
        ""subtask"": false,
        ""avatarId"": 10318
      },
      {
        ""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10005"",
        ""id"": ""10005"",
        ""description"": ""The sub-task of the issue"",
        ""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10513&avatarType=issuetype"",
        ""name"": ""Sub-task"",
        ""subtask"": true,
        ""avatarId"": 10513
      }
    ],
    ""name"": ""Business Project"",
    ""avatarUrls"": {
      ""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
      ""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
      ""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
      ""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
    },
    ""projectKeys"": [
      ""BP""
    ],
    ""projectTypeKey"": ""business"",
    ""simplified"": false,
    ""style"": ""classic"",
    ""isPrivate"": false
  },
{
    ""expand"": ""description,lead,issueTypes,url,projectKeys"",
    ""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008"",
    ""id"": ""10009"",
    ""key"": ""YEET"",
    ""description"": """",
    ""lead"": {
      ""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c37b0c3d650366f5317227e"",
      ""key"": ""jan.michael.vincent"",
      ""accountId"": ""5c37b0c3d650366f5317227e"",
      ""name"": ""jan.michael.vincent"",
      ""avatarUrls"": {
        ""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
        ""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
        ""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue"",
        ""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue""
      },
      ""displayName"": ""Daniel Smerdely"",
      ""active"": true
    },
    ""issueTypes"": [
      {
        ""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
        ""id"": ""10004"",
        ""description"": ""A task that needs to be done."",
        ""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
        ""name"": ""Task"",
        ""subtask"": false,
        ""avatarId"": 10318
      }
    ],
    ""name"": ""Business Project"",
    ""avatarUrls"": {
      ""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
      ""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
      ""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
      ""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
    },
    ""projectKeys"": [
      ""YEET""
    ],
    ""projectTypeKey"": ""business"",
    ""simplified"": false,
    ""style"": ""classic"",
    ""isPrivate"": false
  },
{
    ""expand"": ""description,lead,issueTypes,url,projectKeys"",
    ""self"": ""https://sampleweb.atlassian.net/rest/api/3/project/10008"",
    ""id"": ""10004"",
    ""key"": ""ByeDave:("",
    ""description"": """",
    ""lead"": {
      ""self"": ""https://sampleweb.atlassian.net/rest/api/3/user?accountId=5c37b0c3d650366f5317227e"",
      ""key"": ""jan.michael.vincent"",
      ""accountId"": ""5c37b0c3d650366f5317227e"",
      ""name"": ""jan.michael.vincent"",
      ""avatarUrls"": {
        ""16x16"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=16&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D16%26noRedirect%3Dtrue"",
        ""24x24"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=24&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D24%26noRedirect%3Dtrue"",
        ""32x32"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=32&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D32%26noRedirect%3Dtrue"",
        ""48x48"": ""https://avatar-cdn.atlassian.com/05fc8072104d5310c17da95174c289ca?s=48&d=https%3A%2F%2Fsecure.gravatar.com%2Favatar%2F05fc8072104d5310c17da95174c289ca%3Fd%3Dmm%26s%3D48%26noRedirect%3Dtrue""
      },
      ""displayName"": ""Daniel Smerdely"",
      ""active"": true
    },
    ""issueTypes"": [
      {
        ""self"": ""https://sampleweb.atlassian.net/rest/api/3/issuetype/10004"",
        ""id"": ""10004"",
        ""description"": ""A task that needs to be done."",
        ""iconUrl"": ""https://sampleweb.atlassian.net/secure/viewavatar?size=xsmall&avatarId=10318&avatarType=issuetype"",
        ""name"": ""Task"",
        ""subtask"": false,
        ""avatarId"": 10318
      }
    ],
    ""name"": ""Business Project"",
    ""avatarUrls"": {
      ""48x48"": ""https://sampleweb.atlassian.net/secure/projectavatar?avatarId=10324"",
      ""24x24"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=small&avatarId=10324"",
      ""16x16"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=xsmall&avatarId=10324"",
      ""32x32"": ""https://sampleweb.atlassian.net/secure/projectavatar?size=medium&avatarId=10324""
    },
    ""projectKeys"": [
      ""ByeDave:(""
    ],
    ""projectTypeKey"": ""business"",
    ""simplified"": false,
    ""style"": ""classic"",
    ""isPrivate"": false
  }
]
";

		#endregion

		#endregion
	}
}
