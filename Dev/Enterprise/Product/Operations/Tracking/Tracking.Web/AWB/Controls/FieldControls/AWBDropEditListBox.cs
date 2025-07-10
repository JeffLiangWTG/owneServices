using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBDropEditListBox runat=server></{0}:AWBDropEditListBox>")]
	public class AWBDropEditListBox : AWBFieldControl, ISelfBindingWebControl
	{
		#region Constructors

		public AWBDropEditListBox()
			: base() { }

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue(0), Browsable(true)]
		public int MaxLength { get; set; }

		#endregion

		#region Overrides

		ZDropEditList DropEditBox
		{
			get { return FieldBox as ZDropEditList; }
		}

		protected override WebControl GetNewFieldBox()
		{
			return new ZDropEditList();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			if (DropEditBox != null)
			{
				DropEditBox.Width = Width;
				if (MaxLength != 0)
				{
					DropEditBox.MaxLength = MaxLength;
				}
				DropEditBox.Style.Add("white-space", (NoResString)"nowrap");
			}
		}

		#endregion

	}
}
