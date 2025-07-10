using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.Module
{
	public partial class EDIMessagePurposeFilterControl : ZFilterStripControl
	{
		readonly System.ComponentModel.Container components;

		public EDIMessagePurposeFilterControl(IBusinessObjectCollection gridCollection, EDIMessagePurposeFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			this.CaptionRenderingEnabled = true;
		}

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}


