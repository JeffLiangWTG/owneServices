using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefLanguageTextTranslationForm : ZChildForm
	{
		static string TranslationColumn => (NoResString)"Translation";

		public RefLanguageTextTranslationForm(RefLanguageTextPage refLanguageTextPage)
			: base(refLanguageTextPage)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl);

			Text = Res.GetString("E8361708-6226-45F5-9F30-90EA5FE53184", "Translations: {0}", refLanguageTextPage.Description);
			Attach();

			ZFormMenuStrategy.AddAdornments(this);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("5B1456E1-DDF5-43A8-9CDB-2AF00B015957", "Bulk Export Text"), OnExport);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("8A553ED3-321C-41AC-9607-F66D5153D8D1", "Bulk Import Text"), OnImport);
		}

		public void Attach()
		{
			translationsOfCurrentValueGrid.AfterBind += new EventHandler(translationsOfCurrentValueGrid_AfterBind);
			allValuesGrid.AfterBind += new EventHandler(allValuesGrid_AfterBind);
		}

		public void Detach()
		{
			translationsOfCurrentValueGrid.AfterBind -= new EventHandler(translationsOfCurrentValueGrid_AfterBind);
			allValuesGrid.AfterBind -= new EventHandler(allValuesGrid_AfterBind);
		}

		protected void OnExport(object sender, EventArgs e)
		{
			var dir = GetDirectory(Res.GetString("5E930D87-EE17-4FD7-B9F1-DA02F71C35BA", "Please select the folder into which the exported files should be saved. Existing files will be overridden."));
			if (!string.IsNullOrEmpty(dir))
			{
				var export = Page.ExportEntries();
				var errors = new List<string>();
				var invalid = Path.GetInvalidFileNameChars();
				var prefix = Page.Description;

				foreach (char c in invalid)
				{
					prefix = prefix.Replace(c.ToString(), "");
				}

				foreach (var entry in export)
				{
					string filename = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}-{1}.csv", prefix, entry.Key);

					string path = Path.Combine(dir, filename);

					if (!SaveFile(path, entry.Value.ToString()))
					{
						errors.Add(filename);
					}
				}

				string message = Res.GetString("3723411C-E2E8-4B2C-832D-182205548611", "Export has been completed. The files can be found at {0}.", dir);

				if (errors.Count > 0)
				{
					message += " ";
					message += Res.GetString("63F47430-5E8B-45F8-9624-F7B51BA82D8E", "The files for the following languages were not saved: {0}.", string.Join(", ", errors.ToArray()));
				}

				Globals.Message.Show(message);
			}
		}

		protected virtual bool SaveFile(string path, string content)
		{
			try
			{
				File.WriteAllText(path, content, Encoding.UTF8);
			}
			catch (IOException)
			{
				return false;
			}

			return true;
		}

		protected virtual string GetDirectory(string description)
		{
			string directory = string.Empty;

			using (var browser = new ZFolderBrowserDialog())
			{
				browser.ShowNewFolderButton = true;
				browser.Description = description;

				browser.RequireMappablePath = true;
				if (browser.ShowDialog() == DialogResult.OK)
				{
					directory = browser.MappedSelectedPath;
				}
			}

			return directory;
		}

		protected void OnImport(object sender, EventArgs e)
		{
			var dir = GetDirectory(Res.GetString("45033652-71D4-49F8-85B2-8BFC78A5665F", "Please select the folder with the files to import."));
			if (!string.IsNullOrEmpty(dir))
			{
				var errors = new List<string>();
				foreach (var entry in Directory.EnumerateFileSystemEntries(dir, "*.csv", SearchOption.TopDirectoryOnly))
				{
					var error = Page.ImportEntries(entry);
					if (!string.IsNullOrEmpty(error))
					{
						errors.Add(error);
					}
				}

				if (errors.Count > 0)
				{
					Globals.Message.ShowError(Res.GetString("2B43414B-8D35-4040-A135-A4352CA33623", "Import completed with errors: {0}", string.Join(", ", errors.ToArray())));
				}
				else
				{
					Globals.Message.Show(Res.GetString("C46020BE-02F1-4AE5-A8E9-E07A92D12BF0", "Import has been successful"));
				}
			}
		}

		void translationsOfCurrentValueGrid_AfterBind(object sender, EventArgs e)
		{
			translationsOfCurrentValueGrid.ListManager.CurrentChanged += new EventHandler(translationsOfCurrentValueGrid_CurrentChanged);
		}

		void allValuesGrid_AfterBind(object sender, EventArgs e)
		{
			allValuesGrid.ListManager.CurrentChanged += new EventHandler(allValuesGrid_CurrentChanged);

			changingCurrent = true;
			UpdateTranslationsOfCurrentGrid();
			UpdateAllValuesGrid();
			changingCurrent = false;
		}

		RefLanguageTextPage Page
		{
			get { return (RefLanguageTextPage)BusinessEntity; }
		}

		void translationsOfCurrentValueGrid_CurrentChanged(object sender, EventArgs e)
		{
			if (!changingCurrent)
			{
				var current = translationsOfCurrentValueGrid.ListManager.GetCurrent() as RefLanguageTextPageEntry;
				if (current != null)
				{
					changingCurrent = true;
					Page.CurrentLanguage = current.Language;
					UpdateAllValuesGrid();
					changingCurrent = false;
				}
			}
		}

		void allValuesGrid_CurrentChanged(object sender, EventArgs e)
		{
			if (!changingCurrent)
			{
				var current = allValuesGrid.ListManager.GetCurrent() as RefLanguageTextPageEntry;
				if (current != null)
				{
					changingCurrent = true;
					Page.CurrentCaption = current.Caption;
					UpdateTranslationsOfCurrentGrid();
					changingCurrent = false;
				}
			}
		}

		bool changingCurrent;

		void UpdateAllValuesGrid()
		{
			string currentLanguageDescription = new CodeDescriptionPairList(OLookUpEditType.Language).GetDescriptionFromCode(Page.CurrentLanguage);
			allValuesLabel.Text = Res.GetString("D177A48B-E471-4B20-9836-42323C95AD55", "All values in {0}", currentLanguageDescription);
			allValuesGrid.Columns[TranslationColumn].ColumnStyle.HeaderText = currentLanguageDescription;
			for (int i = 0; i < allValuesGrid.ListManager.List.Count; i++)
			{
				if (((RefLanguageTextPageEntry)allValuesGrid.ListManager.List[i]).Caption.ResourceKey == Page.CurrentCaption.ResourceKey)
				{
					allValuesGrid.ListManager.Position = i;
					break;
				}
			}
		}

		void UpdateTranslationsOfCurrentGrid()
		{
			for (int i = 0; i < translationsOfCurrentValueGrid.ListManager.List.Count; i++)
			{
				if (((RefLanguageTextPageEntry)translationsOfCurrentValueGrid.ListManager.List[i]).Language == Page.CurrentLanguage)
				{
					translationsOfCurrentValueGrid.ListManager.Position = i;
					break;
				}
			}
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void SaveInternal()
		{
			Page.Save();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Detach();
			}
			base.Dispose(disposing);
		}
	}
}
