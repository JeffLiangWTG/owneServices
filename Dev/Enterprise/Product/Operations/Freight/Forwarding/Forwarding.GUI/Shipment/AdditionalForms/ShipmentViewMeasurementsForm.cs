using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentViewMeasurementsForm : ZChildForm
	{
		internal ShipmentViewMeasurementsForm()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(TextBox, new SuppressDpiAwareBasherAttribute());
#endif
		}

		public static void ShowDialog(string title, string text)
		{
			var textDialogWithCloseButtonForm = new ShipmentViewMeasurementsForm();
			textDialogWithCloseButtonForm.Text = title;
			textDialogWithCloseButtonForm.TextBox.Text = text;
			FormatCommentsInRichTextBox(textDialogWithCloseButtonForm.TextBox);
			ZFormModaliser.ShowDialogAndDispose(textDialogWithCloseButtonForm);
		}

		static void FormatCommentsInRichTextBox(RichTextBox richTextBox)
		{
			var text = richTextBox.Text;
			var regex = new Regex(@"\(.*?\)");
			foreach (var match in regex.Matches(text).Cast<Match>())
			{
				richTextBox.Select(match.Index, match.Length);
				richTextBox.SelectionColor = Color.Red;
			}
		}
	}
}
