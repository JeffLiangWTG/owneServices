using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public abstract class AWBBaseControl : CompositeControl, ISelfBindingWebControl
	{
		#region Constructors

		public AWBBaseControl()
			: base()
		{
		}

		#endregion

		#region ISelfBindingWebControl Members

		public virtual bool IsBindable(object dataSource)
		{
			return ((!string.IsNullOrEmpty(BindTo) && dataSource != null && dataSource is BusinessObject) || (dataSource != null && dataSource is ExportAWBHeader));
		}

		public void Bind(object dataSource)
		{
			UnBind();
			ExportAWBHeader source = dataSource as ExportAWBHeader;

			if (source != null)
			{
				BindToAWBHeader(source);
			}
		}

		protected virtual void BindToAWBHeader(ExportAWBHeader source)
		{
			BusinessEntity = source;
			new ZWebControlBinder(source).Bind(Controls);
			DataBind();
			SetupInternalControlsVisibility();
		}

		protected virtual void SetupInternalControlsVisibility()
		{
		}

		public void UnBind()
		{
			BusinessEntity = null;
			DataBind();
		}

		protected ExportAWBHeader BusinessEntity
		{
			get { return businessEntity; }
			set { businessEntity = value; }
		}

		ExportAWBHeader businessEntity;

		#endregion

		#region IBindTo Members

		public string BindTo { get; set; }

		#endregion

#if DEBUG
		public void InitializeForTesting()
		{
			OnInit(new EventArgs());
			InitializeChildControlsForTesting(Controls);
			CreateChildControls();
		}

		void InitializeChildControlsForTesting(ControlCollection controls)
		{
			foreach (Control childControl in controls)
			{
				if (childControl is AWBBaseControl)
				{
					((AWBBaseControl)childControl).InitializeForTesting();
				}
				if (childControl is AWBFieldControl)
				{
					((AWBFieldControl)childControl).InitializeForTesting();
				}
				InitializeChildControlsForTesting(childControl.Controls);
			}
		}
#endif
	}
}
