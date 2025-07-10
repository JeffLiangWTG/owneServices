using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressControlRequiresTextBasher]
	public partial class ZButtonTransportCoHotlink : ZButton, IDataBoundControl
	{
		#region MetaData

		public static new PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZButtonTransportCoHotlink>()
				.Property("Text", "")
				.Property("ReadOnly", true, false)
				.Property("IsVisibleForBinding", ZBool.True)
				.Property("IsEnabledForBinding", ZBool.True)
				.Property("TransportCo", (OrgHeader)null)
				.Property("TransportReferenceNo", "")
				.Result;
		}

		#endregion

		public ZButtonTransportCoHotlink()
		{
			Image = Icons.GetIcon(IconTypes.GlobePretty).ToBitmap();
			TabStop = false;

			BindToTransportCo = "";
			BindToTransportRef = "";
		}

		#region Click event

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			OpenTransportCoURL();
		}

		void OpenTransportCoURL()
		{
			ZString errorMsg = TransportCoHotlinkOpener.Instance.OpenWebSite(TransportReferenceNo, TransportCo);
			if (!errorMsg.IsEmpty)
			{
				Globals.Message.ShowError(errorMsg);
			}
		}

		#endregion

		#region TransportCo

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public OrgHeader TransportCo
		{
			get { return fTransportCo; }
			set
			{
				fTransportCo = value;
				if (TransportCoChanged != null)
				{
					TransportCoChanged(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler TransportCoChanged;
		OrgHeader fTransportCo;

		#endregion

		#region TransportReferenceNo

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string TransportReferenceNo
		{
			get { return fTransportReferenceNo; }
			set
			{
				fTransportReferenceNo = value;
				if (TransportReferenceNoChanged != null)
				{
					TransportReferenceNoChanged(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler TransportReferenceNoChanged;
		string fTransportReferenceNo;

		#endregion

		#region Prevent Text/Size change

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return base.Text; }
			set { }
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);

			SuspendLayout();
			try
			{
				var desiredWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(24);
				if (Width != desiredWidth)
				{
					ControlDpiScalingHelper.SetWidth(this, desiredWidth, false);
				}

				var desiredHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(22);
				if (Height != desiredHeight)
				{
					ControlDpiScalingHelper.SetHeight(this, desiredHeight, false);
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (Image != null)
				{
					Image.Dispose();
				}
			}
		}

		#endregion

		#region BindTos

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToTransportCo { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToTransportRef { get; set; }

		#endregion

		#region IDataBoundControl Members

		public void SetDataBinding(object dataSource, string dataMember)
		{
			DataBindings.RemoveBinding(nameof(TransportCo));
			DataBindings.RemoveBinding(nameof(TransportReferenceNo));

			this.DataSource = dataSource;
			this.DataMember = dataMember;

			if (dataSource != null)
			{
				DataBindings.Add(new KBinding(nameof(TransportCo), dataSource, new KBindingMemberInfo(dataMember, BindToTransportCo).BindingMember));
				DataBindings.Add(new KBinding(nameof(TransportReferenceNo), dataSource, new KBindingMemberInfo(dataMember, BindToTransportRef).BindingMember));
			}
		}

		object IDataBoundControl.DataSource
		{
			get { return DataSource; }
		}
		protected object DataSource { get; private set; }

		string IDataBoundControl.DataMember
		{
			get { return DataMember; }
		}
		protected string DataMember { get; private set; }

		Type IDataBoundControl.DataSourceType
		{
			get { return DataSourceType; }
		}

		protected Type DataSourceType
		{
			get { return typeof(object); }
		}

		#endregion
	}
}
