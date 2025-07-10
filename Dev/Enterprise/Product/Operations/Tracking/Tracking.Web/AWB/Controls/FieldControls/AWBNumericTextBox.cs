using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBNumericTextBox runat=server></{0}:AWBNumericTextBox>")]
	public class AWBNumericTextBox : AWBFieldControl, ISelfBindingWebControl
	{
		#region Constructors

		public AWBNumericTextBox()
			: base() { }

		#endregion

		#region Overrides

		protected override WebControl GetNewFieldBox()
		{
			return new ZNumericTextBox();
		}

		#endregion

	}
}
