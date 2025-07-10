using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

#pragma warning disable IDE0001 // Simplify names. Designer requires fully qualified names to correctly deserialize properties

namespace Enterprise.Recruiter.GUI
{
	public partial class HRJobApplicationForm : Enterprise.ZArchitecture.GUI.ZTemplateForm, IEDocsUnattendedConfigProvider
	{
		readonly List<ZGuid> ignoreDragAndDropDocument; // we ignore documents added during drag and drop because these files were processed
		List<IeDoc> documentsAlreadyInDatabase;

		public HRJobApplicationForm(HRJobApplication application)
			: base(application)
		{
			if (ControllerID == null)
			{
				ControllerID = ControllerIDs.HRJobApplication;
			}

			WorkflowTabPage.Initialize(application);

			if (!Application.IsInDatabase)
			{
				Saved += (sender, e) =>
				{
					if (Application.IsInDatabase)
					{
						ApplicantGuidFindBox.ReadOnly = true;
					}
				};
			}
			else
			{
				ApplicantGuidFindBox.ReadOnly = true;
			}

			ignoreDragAndDropDocument = new List<ZGuid>();
			documentsAlreadyInDatabase = GetApplicationResumeDocuments();
			if (Globals.IsTest)
			{
				TypeDescriptor.AddAttributes(CurrentStatusDropEdit, new SuppressFormsLocalizedTestAttribute());
			}
		}

		public HRJobApplication Application
		{
			get { return BusinessEntity; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
			ResetControlsVisibility();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			ShowApplicantFormIfDataIsMissing();
		}

		void ShowApplicantFormIfDataIsMissing()
		{
			ResumeDragDropHelper.ShowApplicantFormToFixErrors(Application, true);

			if (Application?.Applicant == null)
			{
				ApplicantGuidFindBox.ReadOnly = false;
			}
			else
			{
				Application?.HP_HAInfo.RefreshBinding();
			}
		}

		public new HRJobApplication BusinessEntity
		{
			get { return (HRJobApplication)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get
			{
				string captionPrefix = Res.GetString("Recruiter|HRJobApplicationForm|FormCaption", "Application");
				return Application != null && Application.Applicant != null && !Application.Applicant.HA_FullName.IsEmpty ? captionPrefix + " : " + Application.Applicant.HA_FullName : captionPrefix;
			}
		}

		#region DragDrop

		protected override void OnDragDrop(DragEventArgs dragEvent)
		{
			if (MainTabControl.SelectedTab?.Name == "eDocsTabPage")
			{
				base.OnDragDrop(dragEvent);

				return;
			}

			if (!RecruiterDataRegistry.Instance.DaxtraEnable.Value)
			{
				ResumeDragDropHelper.ProcessResumeDragDropWithoutDaxtra(dragEvent?.Data, Application);
				base.OnDragDrop(dragEvent);
			}
			else
			{
				ProcessResumeWithDaxtra(dragEvent);
			}

			var converter = ObjectFactory.Get<IResumeConverter>(nameof(IResumeConverter), new object[] { null });
			converter.ConvertAvailableResumes(Application);
		}

		void ProcessResumeWithDaxtra(DragEventArgs dragEvent)
		{
			var preDragAndDropDocuments = GetApplicationResumeDocuments();
			var insertResult = ResumeDragDropHelper.ProcessResumeDragDrop(dragEvent?.Data, this);

			if (insertResult.ShouldAddToEdocs)
			{
				using (var suspender = insertResult.ShouldSuspendEDocPopup ? EDocPopupSuspender.GetSuspender() : null)
				{
					base.OnDragDrop(dragEvent);
				}
			}

			var currentDocuments = GetApplicationResumeDocuments();
			if (preDragAndDropDocuments.Count != currentDocuments.Count)
			{
				foreach (var item in currentDocuments)
				{
					if (!preDragAndDropDocuments.Any(d => d.UniqueKey == item.UniqueKey))
					{
						ignoreDragAndDropDocument.Add(item.UniqueKey);
					}
				}
			}
		}

		readonly FunctionalitySuspender EDocPopupSuspender = new FunctionalitySuspender();

		bool IEDocsUnattendedConfigProvider.ApplyDocumentConfig(IeDoc eDoc)
		{
			if (EDocPopupSuspender.IsSuspended)
			{
				eDoc.DocType = RefDocTypes.MiscellaneousDocument;
				eDoc.Description = RefDocTypeDescriptions.MiscellaneousDocument;
				return true;
			}
			else
			{
				return false;
			}
		}

		#endregion DragDrop

		List<string> DocumentsDocType
		{
			get
			{
				if (documentsDocType == null)
				{
					var factory = new BusinessObjectFactory();
					documentsDocType = new List<string>();

					var cv = RecruiterDataRegistry.Instance.GetDocTypeCV(factory);
					if (cv != null)
					{
						documentsDocType.Add(cv.RT_DocType);
					}

					var cover = RecruiterDataRegistry.Instance.GetDocTypeCoverLetter(factory);
					if (cover != null)
					{
						documentsDocType.Add(cover.RT_DocType);
					}
				}

				return documentsDocType;
			}
		}
		List<string> documentsDocType;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Don't want to block user with any exception regarding parsing queue, just report it.")]
		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			if (result == ContinueWithSave.Yes)
			{
				var addedDocument = GetApplicationResumeDocuments();
				if (addedDocument.Count != documentsAlreadyInDatabase.Count)
				{
					try
					{
						var factory = new BusinessObjectFactory();
						foreach (var item in addedDocument)
						{
							if (!ignoreDragAndDropDocument.Contains(item.UniqueKey) && !documentsAlreadyInDatabase.Any(d => d.UniqueKey == item.UniqueKey))
							{
								var queue = factory.New<HRJobApplicationParsingQueue>();
								queue.HPQ_HP = Application.PK;
								queue.HPQ_StorageDocReference = item.UniqueKey;
							}
						}
						factory.Save();
						documentsAlreadyInDatabase = GetApplicationResumeDocuments();
					}
					catch (Exception ex)
					{
						ErrorReporter.ReportOnce("Exception creating parsing queue", ex);
					}
				}
			}
			return result;
		}

		protected List<IeDoc> GetApplicationResumeDocuments()
		{
			var allEDocs = new List<IeDoc>();
			if (DocumentsDocType.Count > 0) // If no documents type were set we can't define what documents are resume/cover letter
			{
				allEDocs = Application.DocManagerInfo.AllEDocs.Cast<IeDoc>().Where(d => DocumentsDocType.Contains(d.DocType)).ToList();
			}
			return allEDocs;
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Controls Visibility

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.HP_SourceTypeInfo.ValueChanged -= HP_SourceTypeInfo_ValueChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (BusinessEntity != null)
			{
				BusinessEntity.HP_SourceTypeInfo.ValueChanged += HP_SourceTypeInfo_ValueChanged;
			}

			HP_SourceTypeInfo_ValueChanged(null, null);
		}

		void HP_SourceTypeInfo_ValueChanged(object sender, EventArgs e) => ResetControlsVisibility();

		void ResetControlsVisibility()
		{
			if (BusinessEntity != null)
			{
				ReferringOrgGuidFindBox.Visible = BusinessEntity.IsReferringOrganisationApplicable && !BusinessEntity.IsReferringOrganisationDropDown;
				ReferringOrgGuidZGuidDropEdit.Visible = BusinessEntity.IsReferringOrganisationApplicable && BusinessEntity.IsReferringOrganisationDropDown;
				ReferringOrgGuidZGuidDropEdit.BindToList = "Lookups.ReferringOrganisationsList";

				ReferringStaffFindBox.Visible = BusinessEntity.IsReferringStaffApplicable;
				ReferringPersonGuidFindBox.Visible = BusinessEntity.IsReferringPersonApplicable;
			}
		}

		#endregion

	}
}
