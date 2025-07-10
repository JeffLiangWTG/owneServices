using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public class AWBTabPage : ZTabPage
	{
		public AWBTabPage()
		{
			Text = Res.GetString("Forwarding|AWBTabPage|Header", "AWB");
		}

		#region AWB Interface

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal IAWBParent AWBInterface
		{
			get { return awbInterface; }
			set
			{
				if (awbInterface != value)
				{
					awbInterface = value;
					awbInterface.IsAWBHeaderAccessibleChanged += OnIsAWBHeaderAccessibleChanged;
					RefreshAWBVisibility();
				}
			}
		}

		IAWBParent awbInterface;

		#endregion

		#region Visibility of AWB Tab Page

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZTemplateTabControl TopLevelTabControl { get; set; }

		bool isAWBVisible;
		protected int tabPageIndex;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected internal bool IsAWBVisible
		{
			get { return isAWBVisible; }
			set
			{
				if (IsUserControlConstructed)
				{
					UserControl.MainPanel.Visible = true;
				}
				isAWBVisible = value;

				if (awbTabPageVisible != value && TopLevelTabControl != null)
				{
					SwitchAWBTabPageVisibility(value);
					awbTabPageVisible = value;
				}
			}
		}
		bool awbTabPageVisible = true;

		protected virtual void SwitchAWBTabPageVisibility(ZBool value)
		{
			TabVisible = value;
			TabRelevant = value;
		}

		void RefreshAWBVisibility()
		{
			IsAWBVisible = AWBInterface != null && AWBInterface.IsAWBHeaderAccessible;
		}

		void OnIsAWBHeaderAccessibleChanged(object sender, EventArgs e)
		{
			RefreshAWBVisibility();
		}

		#endregion

		#region UserControl

		protected AWBUserControl UserControl
		{
			get
			{
				if (userControl == null)
				{
					ConstructUserControl();
				}
				return userControl;
			}
		}
		AWBUserControl userControl;

		protected bool IsUserControlConstructed
		{
			get { return userControl != null; }
		}

		public void ConstructUserControl()
		{
			if (!IsUserControlConstructed)
			{
				userControl = NewUserControl();
				userControl.Dock = DockStyle.Fill;
				userControl.OverrideValuesCheckBox.BindTo = AWBInterface.IsAWBValuesOverriddenPropertyInfo.Name;
				Controls.Add(userControl);
			}
		}

		protected virtual AWBUserControl NewUserControl()
		{
			return new AWBUserControl();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (AWBInterface != null)
			{
				AWBInterface.IsAWBHeaderAccessibleChanged -= OnIsAWBHeaderAccessibleChanged;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
