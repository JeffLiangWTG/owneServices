using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting;
using Enterprise.ZArchitecture.GUI;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting
{
	public partial class EditManifestForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public EditManifestForm(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			this.entryHeader = entryHeader;
		}
		readonly CusEntryHeader entryHeader;

		#region FormCaption
		public override string FormCaption
		{
			get
			{
				var bgmReference = entryHeader.CH_BGMReference;
				if (bgmReference.IsEmpty)
				{
					return Enterprise.Customs.NZ.GUI.Res.GetString("Enterprise.Customs.NZ.GUI.EditManifestForm|Caption", "ECI Write-Off Manifest");
				}
				else
				{
					return Enterprise.Customs.NZ.GUI.Res.GetString("Enterprise.Customs.NZ.GUI.EditManifestForm|CaptionWithBGMReference", "ECI Write-Off Manifest - {0}", bgmReference);
				}
			}
		}
		#endregion
		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void SubmitManifestButton_Click(object sender, System.EventArgs e)
		{
			ShowSubmitToCustomsForm(MessageManager.OperationType.SubmitMessage);
			entryHeader.ManifestWrapper.MessageModeInfo.RefreshBinding();
		}

		void CancelManifestButton_Click(object sender, System.EventArgs e)
		{
			ShowSubmitToCustomsForm(MessageManager.OperationType.CancelMessage);
		}

		void ShowSubmitToCustomsForm(Business.MessageBuilders.MessageManager.OperationType operationType)
		{
			var declaration = GetManifestDeclaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsGUI();
			var messagingManager = new MessagingFunctionalityManager(declaration);
			entryHeader.Declarations.Cast<JobDeclaration>().ForEach(x => x.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW);  // Manifest messaging mode is to override the declaration creation value if different
			entryHeader.Factory.Save();

			messagingManager.ShowSubmitToCustomsForm(operationType, this, false, false, true);
		}

		JobDeclaration GetManifestDeclaration
		{
			get
			{
				var result = entryHeader.Declaration;
				if (result != null && result.JE_MessageSubType != JobMessageSubTypeList.Codes.WriteOff)
				{
					if (entryHeader.Declarations.Count > 0)
					{
						result = entryHeader.Declarations[0];
					}
				}

				return result;
			}
		}

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return control == SubmitManifestButton && previousControl == CloseButton;
		}
	}
}
