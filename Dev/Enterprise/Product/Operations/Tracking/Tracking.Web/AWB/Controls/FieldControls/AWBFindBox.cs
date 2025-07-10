using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Web.GUI.Internal;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBFindBox runat=server></{0}:AWBFindBox>")]
	public class AWBFindBox : AWBFieldControl, ISelfBindingWebControl
	{
		#region Constructors

		public AWBFindBox()
			: base() { }

		#endregion

		#region Properties

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToList))]
		public string BindToList
		{
			get { return FindBox.BindToList; }
			set { FindBox.BindToList = value; }
		}

		[Browsable(true)]
		[Category("Behavior")]
		[DefaultValue(false)]
		public bool AutoPostBack
		{
			get { return FindBox.AutoPostBack; }
			set { FindBox.AutoPostBack = value; }
		}

		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(WebModuleIDEditor), typeof(UITypeEditor))]
		public virtual WebModuleID ModuleID
		{
			get { return FindBox.ModuleID; }
			set { FindBox.ModuleID = value; }
		}

		protected ZFindBox FindBox
		{
			get { return FieldBox as ZFindBox; }
		}

		#endregion

		#region Overrides

		protected override WebControl GetNewFieldBox()
		{
			return new ZFindBox();
		}

		#endregion

	}
}
