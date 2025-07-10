using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class BorderLineReleaseAllocationForm : ZChildForm
	{
		public BorderLineReleaseAllocationForm(BorderLineReleaseAllocator allocator)
			: base(allocator)
		{
		}

		public new BorderLineReleaseAllocator BusinessEntity => (BorderLineReleaseAllocator)base.BusinessEntity;

		#region Overrides

		public override string FormCaption => "Border Line Release Allocation";
		public override string FormVerb => "";

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void AllocateButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				try
				{
					BusinessEntity.RunPreSaveValidation();
					ZStringBuilder builder = new ZStringBuilder(BusinessEntity.NumberToAllocateInfo.Notifications.GetNotifications(NotificationType.Error).GetUniqueMessageList());
					ZString errorMessage = builder.ToStringWithNewLineBetweenAppends();

					if (errorMessage.IsEmpty)
					{
						string message = "You are about to allocate '" + BusinessEntity.NumberToAllocate.ToString() + "' Entry Number(s) for Border Line Release.\n\n" +
							"Are you sure you want to continue?";

						if (Globals.Message.ShowConfirmation(message, "Border Line Release Allocation", "yes", MessageBoxIcon.Warning) == DialogResult.OK)
						{
							using (var fileDialog = new ZSaveFileDialog())
							{
								fileDialog.FileName = GetDefaultFileName();
								fileDialog.DefaultExt = "txt";
								fileDialog.Filter = "Text Only (*.txt)|*.txt|All files (*.*)|*.*";

								if (ZFormModaliser.ShowCommonDialogWithoutDispose(fileDialog) == DialogResult.OK)
								{
									string[] entryNumbers = BusinessEntity.AllocateNumbers();
									using (var writer = new StreamWriter(fileDialog.OpenFile()))
									{
										writer.Write(new ZStringBuilder(entryNumbers).ToStringWithNewLineBetweenAppends());
									}
									string allocationInformation = string.Format(CultureInfo.InvariantCulture, "{0} Entry Number(s) have been allocated for Border Line Release.\r\nThe following file contains a list of these entry numbers.\r\n{1}\r\nPlease provide these entry numbers to Customs.", entryNumbers.Length, fileDialog.UnmappedFileName);
									Globals.Message.ShowInformation(allocationInformation, "Entry Number Allocation");
									Close();
								}
							}
						}
					}
					else
					{
						Globals.Message.ShowError(errorMessage);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleSaveException(ex);
				}
			}
		}

		string GetDefaultFileName()
		{
			var extraInfo = BusinessEntity.Wrapper.IsBranchLevel ? "(" + ((BusinessEntity.Wrapper.StmNums.Owner as GlbBranch)?.GB_Code ?? ZString.Empty) + ")" : "";
			return string.Format(CultureInfo.InvariantCulture, "Border Line Release Entry Number allocation for {0}{1}.txt", BusinessEntity.AppliesTo, extraInfo);
		}
	}
}
