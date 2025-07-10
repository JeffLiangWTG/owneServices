namespace Enterprise.MasterFiles.GUI
{
	using System.ComponentModel;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.GUI;

	public partial class CertificatesUserControl : ZUserControl
	{
		public CertificatesUserControl()
		{
			InitializeComponent();
		}

		[DefaultValue("Certificates.XZ_Comment")]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToForDescription
		{
			get { return CommentsTextBox.BindTo; }
			set
			{
				CommentsTextBox.BindTo = value;
				zTextBoxColumnStyleInfo1.ColumnName = PropertyUtilities.GetPropertyName(value);
			}
		}
	}
}
