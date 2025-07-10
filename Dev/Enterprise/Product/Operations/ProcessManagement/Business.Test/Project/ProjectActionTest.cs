using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectAction))]
	public class ProjectActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClose()
		{
			var project = Factory.New<ProjectForTest>();
			var action = new ProjectAction(project);
			action.ActionType = ActionType.Close;

			action.Synchronise();
			AssertEquals("No method has been set", false, project.IsClosed);

			action.CloseType = ProcessTaskStatusCodeList.Codes.Open;
			action.Synchronise();
			AssertEquals("Invalid method has been chosen", false, project.IsClosed);

			action.CloseType = ProcessTaskStatusCodeList.Codes.Closed;
			action.Synchronise();
			AssertEquals("Valid method has been chosen", true, project.IsClosed);
		}

		public void TestCancel()
		{
			var project = Factory.New<ProjectForTest>();
			var action = new ProjectAction(project);
			action.ActionType = ActionType.Close;

			action.Synchronise();
			AssertEquals("No method has been set", false, project.IsCancelled);

			action.CloseType = ProcessTaskStatusCodeList.Codes.Open;
			action.Synchronise();
			AssertEquals("Invalid method has been chosen", false, project.IsCancelled);

			action.CloseType = ProcessTaskStatusCodeList.Codes.Cancelled;
			action.Synchronise();
			AssertEquals("Valid method has been chosen", true, project.IsCancelled);
		}

		public void TestCommentAddToProjectLogLength()
		{
			ProjectAction action = new ProjectAction(Factory.New<Project>());
			AssertEquals("Same MaxLength as Project Log", PredefinedNoteTypes.Instance.ProjectLog.TextOnlyMaxLength, action.CommentInfo.MaxLength);
		}

		public void TestValidateComment()
		{
			Project project = Factory.NewWithValidTestData<Project>();
			ProjectAction action = new ProjectAction(project);
			action.ActionType = ActionType.Log;

			action.Comment = new string('A', 10000);
			action.Synchronise();
			AssertNoErrors(action.CommentInfo);

			action.Comment = new string('B', 40001);
			action.Synchronise();
			AssertHasErrorContaining(action.CommentInfo, "The comment is too long to add to the Project Log.");

			action.Comment = new string('C', project.CommentMaxLength);
			action.Synchronise();
			AssertNoErrors(action.CommentInfo);

			action.Comment = new string('D', 10000);
			action.Synchronise();
			AssertHasError(action.CommentInfo, "The Project Log is too long to have any further comments to be added to it.");

			AssertContains("Postcondition", new string('A', 10000), project.LogText);
			AssertNotContains("Postcondition", new string('B', 10000), project.LogText);
			AssertContains("Postcondition", new string('C', 10000), project.LogText);
			AssertNotContains("Postcondition", new string('D', 10000), project.LogText);
		}

		public void TestValidateComment_ReOpen()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);
			action.ActionType = ActionType.ReOpen;
			AssertValidateComment_WithActionWord(action);
		}

		public void TestValidateComment_Close()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);
			action.ActionType = ActionType.Close;
			action.CloseType = ProcessTaskStatusCodeList.Codes.Closed;
			AssertValidateComment_WithActionWord(action);
		}
		public void TestValidateComment_Cancel()
		{
			Project project = Factory.New<Project>();
			ProjectAction action = new ProjectAction(project);
			action.ActionType = ActionType.Close;
			action.CloseType = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertValidateComment_WithActionWord(action);
		}

		void AssertValidateComment_WithActionWord(ProjectAction action)
		{
			// The added 20 at the end represents appended comments such as the following:
			// - 'Closed - '
			// - 'Cancelled - '
			// - 'Re-Opened - '
			// See Enterprise.ProcessManagement.Business.Project.CommentMaxLength
			action.Comment = new string('A', action.Project.CommentMaxLength + 20);
			action.Synchronise();
			AssertHasErrorContaining(action.CommentInfo, "The comment is too long to add to the Project Log.");
			AssertNotContains("Postcondition", new string('A', 10000), action.Project.LogText);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			Project project = Factory.New<Project>();
			return new ProjectAction(project);
		}
		#endregion
	}

	class ProjectForTest : Project
	{
		public ProjectForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsCancelled;
		public bool IsClosed;

		public override void Close(ZString closeType, ZString comment)
		{
			IsCancelled = true;
			IsClosed = true;
		}
	}
}
