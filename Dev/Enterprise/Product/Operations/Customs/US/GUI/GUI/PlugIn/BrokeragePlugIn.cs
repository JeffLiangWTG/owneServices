using System;
using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class BrokeragePlugIn : BrokeragePlugInOneToOne
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
			shipment.JS_RL_NKOriginInfo.ValueChanged += new EventHandler(JS_RL_NKOriginInfo_ValueChanged);
			shipment.JS_RL_NKDestinationInfo.ValueChanged += new EventHandler(JS_RL_NKDestinationInfo_ValueChanged);
			SetTabPageTextBaseOnDirection();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			SetTabPageTextBaseOnDirection();

			if (JobDeclaration != null)
			{
				JobDeclaration.RefreshExRateToLatestRateAvailableIfNeeded();
			}
		}

		protected override Customs.Business.CreateDeclarationHelper GetCreateDeclarationHelperCore() => new CreateDeclarationHelper();

		protected override BaseShipmentAndBrokerageCommon GetShipmentAndBrokergeCommon(Customs.Business.BaseJobDeclaration declaration)
		{
			return new ShipmentAndBrokerageCommon(declaration);
		}

		void JS_RL_NKOriginInfo_ValueChanged(object sender, EventArgs e)
		{
			SetTabPageTextBaseOnDirection();
		}

		void JS_RL_NKDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			SetTabPageTextBaseOnDirection();
		}

		void SetTabPageTextBaseOnDirection()
		{
			if (Enabled && TabPage != null)
			{
				string result = Name;
				Customs.Business.BaseJobDeclaration jobDeclaration = this.JobDeclaration;
				if (jobDeclaration == null)
				{
					result = GetBrokerageTabPageText(Shipment.IsExport(), Shipment.IsImport());
				}
				else
				{
					result = GetBrokerageTabPageText(jobDeclaration.IsExport, jobDeclaration.IsImport);
				}

				TabPage.Text = result;
			}
		}

		string GetBrokerageTabPageText(bool isExport, bool isImport)
		{
			string result = Name;
			if (isExport)
			{
				result = ExportBrokerageText;
			}
			else if (isImport)
			{
				result = ImportBrokerageText;
			}
			return result;
		}

		internal const string ExportBrokerageText = "Export Declaration";
		internal const string ImportBrokerageText = "Import Declaration";

		protected override void HookDeclarationEventsCore(Customs.Business.BaseJobDeclaration declaration)
		{
			base.HookDeclarationEventsCore(declaration);
			declaration.JE_MessageTypeInfo.ValueChanged += new EventHandler(JE_MessageTypeInfo_ValueChanged);
			SetTabPageTextBaseOnDirection();
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetTabPageTextBaseOnDirection();
		}

		protected override void UnHookDeclarationEventsCore(Customs.Business.BaseJobDeclaration declaration)
		{
			declaration.JE_MessageTypeInfo.ValueChanged -= new EventHandler(JE_MessageTypeInfo_ValueChanged);
			base.UnHookDeclarationEventsCore(declaration);
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl()
		{
			CustomsBrokerageUserControl result = new CustomsBrokerageUserControl();
			result.BindingContext = new ZBindingContext();
			return result;
		}

		MenuItem fTopLevelMenu;
		protected override MenuItem GetNewTopLevelMenuCore()
		{
			if (fTopLevelMenu == null)
			{
				fTopLevelMenu = new EDIMenu();
			}
			return fTopLevelMenu;
		}
	}
}
