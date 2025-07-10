namespace Enterprise.MasterFiles.GUI
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Windows.Forms;
	using CargoWise.Common;
	using CargoWise.IO;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;

	public partial class UserRepositoryConsoleForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public UserRepositoryConsoleForm()
		{
			InitializeComponent();
		}

		public UserRepositoryConsoleForm(UserRepository bo)
			: base(bo)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("d136a3b8-a0ce-4e44-b0ee-23d81bcb23e4", "Customize User Repository Objects"); }
		}

		UserRepository Manager
		{
			get { return (UserRepository)BusinessEntity; }
		}

		void ReloadDbObjectListView(ListView dbObjectListView, Func<IEnumerable<UserRepository.IDatabaseObject>> getDbObjects)
		{
			dbObjectListView.BeginUpdate();
			dbObjectListView.Items.Clear();

			foreach (var dbObject in getDbObjects())
			{
				var item = new ListViewItem(dbObject.ObjectSchema);
				item.SubItems.Add(dbObject.ObjectName);
				item.SubItems.Add(dbObject.ObjectType);
				item.Tag = dbObject;
				item.ImageKey = dbObject.ObjectType;
				dbObjectListView.Items.Add(item);
			}

			dbObjectListView.EndUpdate();
		}

		internal static string ShowUserDefinedTypeDefinitionInformationMessage
		{
			get
			{
				return Res.GetString("3526F983-FE98-41B5-8210-E19D4C4B5F90", "In order to show the definition of this object you will need to use SQL Server Management Studio. If you do not already have read-only access configured, please contact your relationship manager. Pricing is available from our website for this feature.");
			}
		}

		void ShowSelectedObjectDefinition(ListView dbObjectListView)
		{
			if (dbObjectListView.SelectedItems.Count > 0)
			{
				if (ConfirmLossOfChanges())
				{
					var dbObject = (UserRepository.IDatabaseObject)dbObjectListView.SelectedItems[0].Tag;

					if (dbObject.ObjectType == "USER_TYPE")
					{
						Globals.Message.ShowInformation(ShowUserDefinedTypeDefinitionInformationMessage);
					}
					else
					{
						this.ObjectDefinitionTextBox.Text = dbObject.GetCreateScript();
						this.ObjectDefinitionGroupBox.Text = dbObject.ObjectName;
						savedState = ObjectDefinitionTextBox.Text;
					}
				}
			}
			else
			{
				ShowMessageOnStatusBar(Res.GetString("ed5b11d0-a044-4965-99ea-2100e0a12d34", "No object selected to show."));
			}
		}

		void Execute()
		{
			var sql = ObjectDefinitionTextBox.Text.Trim();
			if (!string.IsNullOrEmpty(sql))
			{
				RunActionAndReloadUserRepositoryObjects(() => Manager.Execute(sql));
			}
			else
			{
				ShowMessageOnStatusBar(Res.GetString("f5f518be-0c46-49dc-aeb1-4dc0aa1bd1bb", "No script to execute."));
			}
		}

		void DropSelectedUserRepositoryObject()
		{
			if (UserRepositoryObjectListView.SelectedItems.Count > 0)
			{
				RunActionAndReloadUserRepositoryObjects(() =>
				{
					var selectedItem = UserRepositoryObjectListView.SelectedItems[0];
					var dbObject = (UserRepository.IUserRepositoryDatabaseObject)selectedItem.Tag;
					dbObject.Drop();
				});
			}
			else
			{
				ShowMessageOnStatusBar(Res.GetString("ddd23740-e177-46e0-8bcb-868ff01883b3", "No object selected to delete."));
			}
		}

		void RunActionAndReloadUserRepositoryObjects(Action userRepositoryAction)
		{
			try
			{
				userRepositoryAction();
				ShowMessageOnStatusBar(Res.GetString("cf66907d-ebc6-4af3-9d73-1bcb2b7649a4", "Query executed successfully."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ShowMessageOnStatusBar(ex.Message);
			}

			ReloadDbObjectListView(UserRepositoryObjectListView, Manager.GetUserRepositoryDbObjects);
		}

		void ShowMessageOnStatusBar(string message)
		{
			MessageStatusBarPanel.Text = message;
		}

		bool ConfirmLossOfChanges()
		{
			var shouldContinue = true;
			if (!string.IsNullOrEmpty(ObjectDefinitionTextBox.Text.Trim()) && savedState != ObjectDefinitionTextBox.Text)
			{
				var result = Globals.Message.Show(
					Res.GetString("e3ebe921-1376-47a5-861d-0b6514aabae5", @"The script appears to have changes, and these may be lost with this action.
Do you want to first save your changes, or cancel this action?"),
					String.Empty, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, DialogResult.Cancel);
				if (result == DialogResult.Yes)
				{
					SaveScriptFileButton_Click(this, EventArgs.Empty);
				}
				else if (result == DialogResult.Cancel)
				{
					shouldContinue = false;
				}
			}
			return shouldContinue;
		}

		string savedState = String.Empty;

		#region Events

		void UserRepositoryConsoleForm_Load(object sender, EventArgs e)
		{
			ReloadDbObjectListView(this.EnterpriseObjectListView, Manager.GetMainDbObjects);
			ReloadDbObjectListView(this.UserRepositoryObjectListView, Manager.GetUserRepositoryDbObjects);
			this.ObjectDefinitionTextBox.MaxLength = 1024 * 1024;
			this.CargoWiseOneLabel.Font = this.UserRepositoryLabel.Font = new System.Drawing.Font(OFont.NormalFontName, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		}

		void ObjectDefinitionTextBox_TextChanged(object sender, EventArgs e)
		{
			ObjectDefinitionGroupBox.Text = "";
		}

		void MainStatusBar_MouseHover(object sender, EventArgs e)
		{
			MessageStatusBarPanel.ToolTipText = MessageStatusBarPanel.Text;
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			if (ConfirmLossOfChanges())
			{
				this.Close();
			}
		}

		void OpenScriptFileButton_Click(object sender, EventArgs e)
		{
			if (ConfirmLossOfChanges())
			{
				using (var dialog = new ZOpenFileDialog())
				{
					dialog.CheckPathExists = true;
					dialog.CheckFileExists = true;
					dialog.Multiselect = false;
					dialog.Filter = ScriptFileFilter;
					dialog.DefaultExt = ScriptFileExtension;

					if (dialog.ShowDialog(this) == DialogResult.OK)
					{
						this.ObjectDefinitionGroupBox.Text = Path.GetFileNameWithoutExtension(dialog.UnmappedFileName);
						using (var stream = dialog.OpenFile())
						{
							this.ObjectDefinitionTextBox.Text = stream.WriteToString();
						}
						savedState = ObjectDefinitionTextBox.Text;
					}
				}
			}
		}

		void SaveScriptFileButton_Click(object sender, EventArgs e)
		{
			var scriptToSave = ObjectDefinitionTextBox.Text.Trim();
			if (string.IsNullOrEmpty(scriptToSave))
			{
				ShowMessageOnStatusBar(Res.GetString("e5f0ae8d-dede-4143-872d-cafa80e0ee69", "No valid script to save."));
			}
			else
			{
				using (var dialog = new ZSaveFileDialog())
				{
					dialog.Filter = ScriptFileFilter;
					dialog.DefaultExt = ScriptFileExtension;

					if (dialog.ShowDialog(this) == DialogResult.OK)
					{
						using (var stream = new StreamWriter(dialog.OpenFile()))
						{
							stream.Write(scriptToSave);
							savedState = ObjectDefinitionTextBox.Text;
							ShowMessageOnStatusBar(Res.GetString("9412c9d3-1125-413d-8e05-4a3191487ac0", "Script saved to \"{0}\".", dialog.UnmappedFileName));
						}
					}
				}
			}
		}

		const string ScriptFileExtension = "sql";
		readonly string ScriptFileFilter = String.Format((NoResString)"SQL files (*.{0})|*.{0}", ScriptFileExtension);

		void DbObjectListView_DoubleClick(object sender, EventArgs e)
		{
			ShowSelectedObjectDefinition((ListView)sender);
		}

		void ShowEnterpriseObjectDefinitionButton_Click(object sender, EventArgs e)
		{
			ShowSelectedObjectDefinition(EnterpriseObjectListView);
		}

		void ShowUseRepositoryObjectDefinitionButton_Click(object sender, EventArgs e)
		{
			ShowSelectedObjectDefinition(UserRepositoryObjectListView);
		}

		void ExecuteUserRepositoryButton_Click(object sender, EventArgs e)
		{
			Execute();
		}

		void DropUserRepositoryObjectButton_Click(object sender, EventArgs e)
		{
			DropSelectedUserRepositoryObject();
		}

		#endregion
	}
}
