using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	[ToolboxItem(true)]
	public class ZStmNotePopupEditWithBindableText : ZTextBoxWithDetailsOnNote
	{
		public ZStmNotePopupEditWithBindableText()
		{
			InitializeComponent();
			ButtonText = Res.GetString("E6D1D99C-B299-42A6-8794-2316D86CD09E", "More...");
		}

		#region Button Text

		[Category(ZGUIConstants.DesignerCategory)]
		public new string ButtonText
		{
			get { return base.ButtonText; }
			set { base.ButtonText = value; }
		}

		#endregion

		protected override void HookPopup(ZStmNotePopupForm notePopupForm)
		{
			base.HookPopup(notePopupForm);
			notePopupForm.NoteHasChangesChanged += new ZStmNotePopupForm.NoteHasChangesEventHandler(NotePopupForm_NoteHasChangesChanged);
		}

		void NotePopupForm_NoteHasChangesChanged()
		{
			Binding binding = TextBox.DataBindings["Text"];
			if (binding != null)
			{
				binding.ReadValue();
			}
		}

		void InitializeComponent()
		{
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// popupButton
			// 
			this.popupButton.Text = "Detail";
			// 
			// ZStmNotePopupEditWithBindableText
			// 
			this.ButtonText = "Detail";
			this.Name = "ZStmNotePopupEditWithBindableText";
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
