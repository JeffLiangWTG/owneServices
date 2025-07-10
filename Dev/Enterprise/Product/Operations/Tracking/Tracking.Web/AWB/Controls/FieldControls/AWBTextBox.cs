using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBTextBox runat=server></{0}:AWBTextBox>")]
	public class AWBTextBox : AWBFieldControl, ISelfBindingWebControl
	{
		#region Constructors

		public AWBTextBox()
			: base() { }

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue(TextBoxMode.SingleLine), Browsable(true)]
		public TextBoxMode TextMode { get; set; }

		#endregion

		#region Overrides

		protected override void CreateChildControls()
		{
			TextFieldBox.TextMode = TextMode;
			base.CreateChildControls();
		}

		protected override WebControl GetNewFieldBox()
		{
			return new ZTextBox();
		}

		#endregion

		#region Implementation

		ZTextBox TextFieldBox
		{
			get { return FieldBox as ZTextBox; }
		}

		#endregion
	}
}
