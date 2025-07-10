using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Declaration
{
	public abstract class BaseStatusControl : BaseUserControl, ISelfBindingWebControl
	{
		public BusinessObject DataSource { get; set; }

		protected override void OnInit(System.EventArgs e)
		{
			base.OnInit(e);

			var zgrids = Controls.OfType<ZGrid>().ToArray();
			for (int i = 0; i < zgrids.Length; i++)
			{
				zgrids[i].InitCompleteHook();
			}
		}

		protected override void OnPreRender(System.EventArgs e)
		{
			base.OnPreRender(e);
			base.Visible = base.Visible && IsBound;
		}

		#region ISelfBindingWebControl Members

		public abstract void Bind(object dataSource);

		protected virtual void BindControl<T>(object dataSource)
		{
			EnsureChildControls();

			IsBound = false;
			if (BindTo != ZString.Empty)
			{
				this.DataSource = ((BindTo == ".") ? dataSource : ZPropertyAccessor.Get(dataSource, BindTo)) as BusinessObject;
				if (IsBindable<T>(this.DataSource) && !SkipDataBind())
				{
					PreDataBind();
					DataBind();
					IsBound = true;
				}
			}
		}

		public virtual bool SkipDataBind() { return false; }

		public virtual void PreDataBind() { }

		public override void DataBind()
		{
			base.DataBind();
			BindChildControls(this);
		}

		public bool IsBindable(object dataSource)
		{
			return dataSource != null;
		}

		bool IsBindable<T>(object dataSource)
		{
			if (IsBindable(dataSource))
			{
				if (dataSource is TrackingDeclaration)
				{
					return ((TrackingDeclaration)dataSource).Declaration is T;
				}
				return dataSource is T;
			}
			return false;
		}

		public void UnBind()
		{
			DataSource = null;
		}

		void BindChildControls(Control parentControl)
		{
			foreach (Control control in parentControl.Controls)
			{
				var bindControl = control as ISelfBindingWebControl;
				if (bindControl != null)
				{
					bindControl.Bind(DataSource);
				}
				else
				{
					BindChildControls(control);
				}
			}
		}

		#endregion

		#region IBindTo Members

		[DefaultValue(".")]
		public string BindTo { get; set; }

		#endregion

		#region Properties

		public bool IsBound { get; private set; }

		#endregion
	}
}