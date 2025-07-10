using System.ComponentModel;
using System.IO;
using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RichTextEmailDisplayZForm : ZChildForm
	{
		public static RichTextEmailDisplayZForm FromFile(string filePath, string titleText)
		{
			var result = new RichTextEmailDisplayZForm(titleText);
			result.FilePath = filePath;
			return result;
		}

		public static RichTextEmailDisplayZForm FromFile(ZString displayText, string filePath, ZString titleText)
		{
			return new RichTextEmailDisplayZForm(displayText, filePath, titleText);
		}

		public RichTextEmailDisplayZForm(ZString displayText, ZString titleText) : this(titleText)
		{
			CreateRichTextFile(displayText);
			Display(displayText);
		}

		public RichTextEmailDisplayZForm(ZString displayText, string filePath, ZString titleText) : this(titleText)
		{
			FilePath = filePath;
			CreateRichTextFile(displayText);
			Display(displayText);
		}

		RichTextEmailDisplayZForm(string titleText)
		{
			this.TitleText = titleText;
#if DEBUG
			foreach (var childControl in Controls)
			{
				if (childControl is KRichTextBox)
				{
					TypeDescriptor.AddAttributes(childControl, new[] { new SuppressDpiAwareBasherAttribute() });
				}
			}
#endif
		}

		void Display(ZString displayText)
		{
			#if !WINZOR
			EmailViewerTextBox.Rtf = displayText;
			#else
			EmailViewerTextBox.Html = displayText;
			#endif
		}

		readonly string TitleText = "";
		string FilePath;

		void CreateRichTextFile(string fileContent)
		{
			if (FilePath == null)
			{
				FilePath = Temp.GetTempFileNameWithExtension(".rtf");
			}
			using (StreamWriter writer = new StreamWriter(FilePath, true, Encoding.UTF8))
			{
				writer.WriteLine(fileContent);
			}
		}

		public override string FormCaption
		{
			get
			{
				return TitleText;
			}
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (File.Exists(FilePath))
			{
				File.Delete(FilePath);
			}
		}

		#endregion

		internal ZRichTextBox EmailViewerTextBox;
	}
}
