using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RecipientSelectionAdvancedSearchForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public RecipientSelectionAdvancedSearchForm()
		{
			InitializeComponent();
		}

		public RecipientSelectionAdvancedSearchForm(RecipientSelection recipientSelection)
			: base(recipientSelection)
		{
			InitializeComponent();
		}

		new RecipientSelection BusinessEntity
		{
			get { return (RecipientSelection)base.BusinessEntity; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void NewCancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void ResetFiltersButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.ResetAdvancedSearchQueries();
		}
	}
}
