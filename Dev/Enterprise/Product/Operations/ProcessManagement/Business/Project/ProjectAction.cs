using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ProcessManagement.Business
{
	public enum ActionType
	{
		Close,
		ReOpen,
		Log
	}

	public class ProjectAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ProjectAction(Project project)
			: base(project.Factory)
		{
			this.Project = project;
		}

		#region Synchronisation

		public readonly Project Project;

		public void Synchronise()
		{
			RunPreSaveValidation();
			if (!HasErrors)
			{
				PerformAction();
			}
		}

		protected virtual void PerformAction()
		{
			if (ActionType == ActionType.Close && !CloseType.IsEmpty)
			{
				Project.Close(CloseType, Comment);
			}
			else if (ActionType == ActionType.ReOpen)
			{
				Project.ReOpen(Comment);
			}
			else if (ActionType == ActionType.Log && !Comment.IsEmpty)
			{
				Project.AddToLog(Comment);
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCloseType();
			ValidateComment();
		}

		void ValidateCloseType()
		{
			if (ActionType == ActionType.Close)
			{
				CloseTypeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(CloseTypeInfo);
				ListValidation.ErrorIfInvalidCode(CloseTypeInfo);
			}
		}

		void ValidateComment()
		{
			CommentInfo.ClearAllNotifications();

			if (!Comment.IsEmpty)
			{
				int commentMaxLength = Project.CommentMaxLength;
				if (commentMaxLength == 0)
				{
					CommentInfo.AddError(Res.GetString("881f5c43-855a-4067-8615-67151bd89bf1", "The Project Log is too long to have any further comments to be added to it."));
				}
				else if (Comment.Length > commentMaxLength)
				{
					CommentInfo.AddError(Res.GetString("d8081d7c-5752-4208-ab42-9d56e3d703da", "The comment is too long to add to the Project Log. Please make the comment below {0} characters.", commentMaxLength));
				}
			}
		}

		#endregion

		#region Properties

		public ActionType ActionType
		{
			get;
			set;
		}

		public ZString MessageCloseOrCancel
		{
			get { return Res.GetString("0ec02525-cada-400e-b3ac-4c74033dbfc5", "Please specify a close method, and optionally some comments."); }
		}

		public ZString MessageReOpen
		{
			get { return Res.GetString("8E445B45-4245-4046-8240-34765855DD2C", "Please specify an optional comment explaining why this project has been re-opened."); }
		}

		public ZString MessageLog
		{
			get { return Res.GetString("450FDDDF-4FC6-44F4-A0C7-3CD07661E1BC", "Please specify a descriptive comment to add to the log. All emails and file attachments should be added through the eDocs tab."); }
		}

		[List("Lookups.CloseList")]
		public ZString CloseType
		{
			get { return closeType; }
			set
			{
				if (closeType != value)
				{
					CheckMaximumLength(CloseTypeInfo, value);
					closeType = value;
					if (!IsValidationSuspended)
					{
						ValidateCloseType();
					}
					CloseTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CloseTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CloseType)); }
		}

		public int CloseType_MaxLength
		{
			get { return 3; }
		}

		ZString closeType;

		public ZString Comment
		{
			get { return comment; }
			set
			{
				if (comment != value)
				{
					CheckMaximumLength(CommentInfo, value);
					comment = value;
					if (!IsValidationSuspended)
					{
						ValidateComment();
					}
					CommentInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CommentInfo
		{
			get { return GetZPropertyInfo(nameof(Comment)); }
		}

		public int Comment_MaxLength
		{
			get { return PredefinedNoteTypes.Instance.ProjectLog.TextOnlyMaxLength; }
		}

		ZString comment;

		public WorkProjectLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new WorkProjectLookups(Factory);
				}
				return lookups;
			}
		}

		WorkProjectLookups lookups;

		#endregion
	}
}
