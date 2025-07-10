using System.IO;
using System.Windows.Forms;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class GenShapeGeographyForm : ZTemplateForm
	{
		public GenShapeGeographyForm()
		{
		}

		public GenShapeGeographyForm(GenShapeGeography genShapeGeography) : base(genShapeGeography)
		{
			SHG_ShapeImportButton.ReadOnly = genShapeGeography.SHG_IsSystem;
		}

		protected override bool SupportsEDocs => false;

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return (BusinessEntity as GenShapeGeography)?.HumanReadableName ?? Res.GetString("GenShapeGeographyForm|FormCaption", "Geography"); }
		}

		#endregion

		void SHG_ShapeImportButton_Click(object sender, System.EventArgs e)
		{
			if (BusinessEntity is GenShapeGeography genShape && !genShape.SHG_IsSystem)
			{
				using (var dlg = new ZOpenFileDialog())
				{
					dlg.Filter = (NoResString)"Keyhole Markup Language (*.kml)|*.kml";
					if (dlg.ShowDialog() == DialogResult.OK)
					{
						string fileInfoStr;

						using (var fileSteam = dlg.OpenFile())
						{
							fileInfoStr = StreamToString(fileSteam);
						}

						using (var form = new GenShapeGeographyImportProgressForm(genShape, fileInfoStr))
						{
							form.ShowDialog();
						}
					}
				}
			}
		}

		static string StreamToString(Stream stream)
		{
			stream.Position = 0;
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
