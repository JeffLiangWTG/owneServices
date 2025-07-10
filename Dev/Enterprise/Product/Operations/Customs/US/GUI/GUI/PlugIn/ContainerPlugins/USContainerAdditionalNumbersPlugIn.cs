using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.GUI
{
	public class USContainerAdditionalNumbersPlugIn : ZPlugIn, IAddColumnsToGrid
	{
		public USContainerAdditionalNumbersPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		#region GUI

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return null;
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		#endregion

		#region License

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Forwarder; }
		}

		#endregion

		public override string Name => "Numbers";

		#region IAddColumnsToGrid

		void IAddColumnsToGrid.AddColumnsToContainerGrid(ZGrid grid)
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
#if DEBUG
			TypeDescriptor.AddAttributes(zTextBoxColumnStyleInfo1, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(zTextBoxColumnStyleInfo2, new SuppressFormsLocalizedTestAttribute());
#endif
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("D3B42ED4-8C8F-4CB6-8B97-D45E240F8BE8", "", "AMS Number", "");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "AMSNumber";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref zTextBoxColumnStyleInfo1, 110, true);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("98394D69-F30C-4AA9-8E29-E81414958EA7", "IT Ref.", "InBond (IT) Reference Number", "");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ITReferenceNumber";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		}

		#endregion
	}
}
