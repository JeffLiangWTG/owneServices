using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public partial class CustomsEntryAndDiscardedMessagesUserControl : BaseCustomsEntryUserControl
	{
		public CustomsEntryAndDiscardedMessagesUserControl()
			: this(null)
		{
		}

		public CustomsEntryAndDiscardedMessagesUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
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

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			CreateAndBindImportMessageUserControl(dataSource, dataMember);
		}

		#region Debug Control Properties
#if DEBUG

		[Browsable(false)]
		public BaseCustomsEntryUserControl NewMessageUserControl
		{
			get { return fMessageUserControl ?? (fMessageUserControl = GetMessageUserControl()); }
		}

#endif
		#endregion

		protected void CreateAndBindImportMessageUserControl(object dataSource, string dataMember)
		{
			if (fMessageUserControl == null)
			{
				fMessageUserControl = GetMessageUserControl();
				fMessageUserControl.JobDeclaration = JobDeclaration;
				fMessageUserControl.Dock = DockStyle.Fill;
				fMessageUserControl.Visible = false;
				MessagesTabPage.Controls.Add(fMessageUserControl);
				fMessageUserControl.Visible = true;
				var customsEntryUserControl = fMessageUserControl;
				customsEntryUserControl?.InitializeGridLayout();
			}
			fMessageUserControl.SetDataBinding(dataSource, dataMember);
		}

		protected virtual BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new ImportMessageUserControl();
		}

		protected BaseCustomsEntryUserControl fMessageUserControl;
	}
}
