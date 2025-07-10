using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.GUI
{
	#region class SailingModulePopup

#if DEBUG
	[ZArchitecture.GUI.Testing.TestExcludeZWinFormHasTypedConstructor]
#endif
	class SailingModulePopup : EmbeddedModulePopup
	{
		public SailingModulePopup(BusinessObject businessEntity, ZFilterGridModule module)
			: base(module)
		{
			BizObj = businessEntity;
		}

		readonly BusinessObject BizObj;

		#region Implementation

		protected override void HandleSelection(BusinessObject[] selectedBizObjs)
		{
			if (selectedBizObjs.Length > 0)
			{
				if (BizObj is ISailingParentFindBox)
				{
					((ISailingParentFindBox)BizObj).SailingPK = selectedBizObjs[0].PK;
				}
				else if (BizObj is CommonShipment)
				{
					((CommonShipment)BizObj).JS_JX = selectedBizObjs[0].PK;
				}
				else if (BizObj is CommonContainer)
				{
					((CommonContainer)BizObj).JC_JX = selectedBizObjs[0].PK;
				}
				else if (BizObj is Transport)
				{
					((Transport)BizObj).JW_JX = selectedBizObjs[0].PK;
				}
				else if (BizObj is ICommonCartage)
				{
					BizObj[JobCartageSchema.JJ_JX_Sailing] = selectedBizObjs[0].PK;
				}
				else
				{
					var message = Res.GetString("cb2606df-4c11-46b0-afa2-bb9b2d344a7c", "Don't know how to set the sailing on a <{0}>, why was this not done with an interface?", BizObj.GetType());
					ErrorReporter.ReportOnce("{22E1F240-6872-41e1-9A61-26767031AF7E}", message);
				}
			}

			Dispose();
		}

		#endregion
	}

	#endregion

	public class SailingIFindBox : IFindBox
	{
		#region Constructors

		public SailingIFindBox(ISailingParentFindBox iSailingParent, ZForm parentForm)
			: this(parentForm, iSailingParent.Factory, false)
		{
			this.ISailingParent = iSailingParent;
			SetFilterDefaults(iSailingParent.LoadPort, iSailingParent.DischargePort);
		}

		public SailingIFindBox(CommonShipment shipment, ZForm parentForm)
			: this(parentForm, shipment.Factory, false)
		{
			this.Shipment = shipment;
			SetFilterDefaults(shipment.JS_RL_NKOrigin, shipment.JS_RL_NKDestination);
		}

		public SailingIFindBox(CommonContainer container, ZForm parentForm)
			: this(parentForm, container.Factory, false)
		{
			this.Container = container;
		}

		public SailingIFindBox(Transport transport, ZForm parentForm)
			: this(parentForm, transport.Factory, transport.JW_IsCharter)
		{
			this.Transport = transport;
			SetFilterDefaults(transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort);
		}

		SailingIFindBox(ZForm parentForm, BusinessObjectFactory factory, bool chartered)
		{
			this.ParentForm = parentForm;

			if (chartered)
			{
				Sailings = new CharterSailingCollection(factory);
			}
			else
			{
				Sailings = new NonCharterSailingCollection(factory);
			}
		}

		#endregion

		#region ShowModuleFromISailingParent

		public ZFilterGridModule ShowModuleFromISailingParent()
		{
			var moduleID = GetModuleFromTransportMode(ISailingParent.TransportMode);
			return ShowModule(moduleID, (BusinessObject)ISailingParent);
		}

		#endregion

		#region  ShowModuleFromShipment

		public ZFilterGridModule ShowModuleFromShipment()
		{
			var moduleID = GetModuleFromTransportMode(Shipment.JS_TransportMode);
			return ShowModule(moduleID, Shipment);
		}

		#endregion

		#region ShowModuleFromContainer

		public ZFilterGridModule ShowModuleFromContainer(ZString transportMode)
		{
			var moduleID = GetModuleFromTransportMode(transportMode);
			return ShowModule(moduleID, Container);
		}

		#endregion

		#region ShowModuleFromTransport

		public ZFilterGridModule ShowModuleFromTransport()
		{
			var moduleID = GetModuleFromTransportMode(Transport.JW_TransportMode);

			if (moduleID == ModuleIDs.NotAssigned)
			{
				Globals.Message.ShowError(Res.GetString("25e6d646-92fe-47ad-8505-b45af8ced810", "Please enter Transport Mode."));
			}

			return ShowModule(moduleID, Transport);
		}

		#endregion

		#region Implementation

		#region GetModuleFromTransportMode

		internal ModuleIdentifier GetModuleFromTransportMode(ZString transportMode)
		{
			switch (transportMode)
			{
				case Enterprise.Core.Constants.TransportModes.Sea:
					return ModuleIDs.JobSeaSailing;

				case Enterprise.Core.Constants.TransportModes.Air:
					return ModuleIDs.JobAirSailing;

				case Enterprise.Core.Constants.TransportModes.Road:
					return ModuleIDs.JobRoadSailing;

				case Enterprise.Core.Constants.TransportModes.Rail:
					return ModuleIDs.JobRailSailing;

				default:
					return ModuleIDs.NotAssigned;
			}
		}

		#endregion

		#region ShowModule

		ZFilterGridModule ShowModule(ModuleIdentifier iD, BusinessObject parent)
		{
			if (iD == ModuleIDs.NotAssigned)
			{
				return null;
			}

			var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(iD);
			if (module.SecurityCheckpoint.IsAllowed)
			{
				if (module is IJobSailingSchedule jobSailingSchedule)
				{
					jobSailingSchedule.ScheduleCreateFromJob = true;
				}

				module.OverrideModuleDecisionProvider(new PopupModuleDecisionProvider(this));
				Popup = new SailingModulePopup(parent, module);
				Popup.ShowModal(this, ParentForm);
#if DEBUG
				if (Globals.IsTest)
				{
					Popup.Dispose();
					module.Dispose();
				}
#endif

				return module;
			}
			else
			{
				module.SecurityCheckpoint.ShowError();
				module.Dispose();
			}

			return null;
		}

		#endregion

		#region SetFilterDefaults

		void SetFilterDefaults(ZString loadPort, ZString dischargePort)
		{
			SailingScheduleDefaultFilterProvider provider = new SailingScheduleDefaultFilterProvider();
			provider.LoadPort = loadPort;
			provider.DischargePort = dischargePort;
			provider.SetDefaultFilters(Sailings);
		}

		#endregion

		protected readonly JobSailingCollection Sailings;

		readonly ZForm ParentForm;
		readonly CommonShipment Shipment;
		readonly CommonContainer Container;
		readonly Transport Transport;
		readonly ISailingParentFindBox ISailingParent;

		#endregion

		#region IFindBox Members

		public string Code
		{
			get { return fCode; }
			set { fCode = value; }
		}

		public string Description
		{
			get { return fDescription; }
			set { fDescription = value; }
		}

		public IFindBoxPopup PopupForm
		{
			get { return Popup; }
		}

		public IFindBoxListProvider ListProvider
		{
			get { return Sailings; }
		}

		protected string fCode;
		protected string fDescription;
		protected EmbeddedModulePopup Popup;

		#endregion
	}
}
