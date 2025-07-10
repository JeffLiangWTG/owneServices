using System;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	public partial class SignatureForm : ZChildForm
	{
		#region Construction

		public SignatureForm(ZBlob signature)
			: base()
		{
			Signature = signature;
			InitializeComponent();
		}

		readonly ZBlob Signature;

		#endregion

		#region Overrides

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DrawSignature();
		}

		#endregion

		#region FormVerb

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}

		#endregion

		#endregion

		#region DrawSignature

		void DrawSignature()
		{
			var drawer = new SignatureDrawer(Signature);
			SignaturePictureBox.Image = drawer.Image;
		}

		#endregion

		#region Events

		void SignaturePictureBox_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Dispose

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

		#endregion
	}
}
