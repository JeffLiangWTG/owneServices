using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public partial class ServicesControl : ZUserControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded binding constant")]
		public const string ServicesCollectionDefaultBinding = "Services";

		public ServicesControl()
		{
			InitializeComponent();
			BindToServices = ServicesCollectionDefaultBinding;
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToServices
		{
			get { return ServicesUserControl.BindToServices; }
			set { ServicesUserControl.BindToServices = value; }
		}

		public bool IsContextVisibleInGrid
		{
			get { return this.ServicesUserControl.ContextColumnVisibleInGrid; }
			set { this.ServicesUserControl.ContextColumnVisibleInGrid = value; }
		}
	}
}
