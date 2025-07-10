using System;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Common.GUI
{
	public partial class CartageTypeForm : ZTemplateForm
	{
		public CartageTypeForm(CommonCartageType businessEntity)
			: base(businessEntity)
		{
		}

		protected override void InitialiseForm()
		{
			CaptionRenderingEnabled = true;
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Events

		void HookEvents()
		{
			CartageType.ContainerModeInfo.ValueChanged += new EventHandler(ContainerModeInfo_ValueChanged);
			CartageType.E3_JobTypeInfo.ValueChanged += new EventHandler(E3_JobTypeInfo_ValueChanged);
		}

		void UnHookEvents()
		{
			if (CartageType != null)
			{
				CartageType.ContainerModeInfo.ValueChanged -= new EventHandler(ContainerModeInfo_ValueChanged);
				CartageType.E3_JobTypeInfo.ValueChanged -= new EventHandler(E3_JobTypeInfo_ValueChanged);
			}
		}

		void ContainerModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupScreen();
		}

		void E3_JobTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupScreen();
		}

		void SetupScreen()
		{
			if (!CartageType.ContainerMode.IsEmpty)
			{
				ContainerLooseBookingSplitContainer.Panel1Collapsed = !CartageType.AllowContainerBookings;
				ContainerLooseBookingSplitContainer.Panel2Collapsed = !CartageType.AllowLooseBookings;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupScreen();
			HookEvents();
		}

		#endregion

		#region CartageType

		CommonCartageType CartageType
		{
			get { return (CommonCartageType)BusinessEntity; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookEvents();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Caption

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		#endregion
	}
}
