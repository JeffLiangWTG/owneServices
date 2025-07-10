using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ProcessManagement.Business
{
	/// <summary>
	/// Unpersistent bizo for the project's client contact
	/// just to implement the ISendEmailSource interface
	/// </summary>
	public class ProjectContactForEmail : NonPersistentBusinessObject
		, ISendEmailSource
		, IObsoleteValidation
	{
		public ProjectContactForEmail(Project project)
		{
			Project = project;
		}
		readonly Project Project;

		public override bool HasChanges
		{
			get { return Project.HasChanges; }
			set { Project.HasChanges = value; }
		}

		#region ISendEmailSource

		string ISendEmailSource.EmailSubject
		{
			get { return Project.EmailSubject; }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return Project.TemplateCategory; }
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return Project.DefaultFromDisplayName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return Project.OverridingDefaultFromEmailAddress; }
		}

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = ((ISendEmailSource)Project).GetAddressBookSelection();
			result.AddRecipient(Project.Contact, true);
			return result;
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return Project.DocWrapperType; }
		}

		Logs ISendEmailSource.Logs
		{
			get { return Project.GetLogs(); }
		}

		#endregion

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(Project, Core.Constants.DocManagerCodes.Project)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
