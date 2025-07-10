using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupporter.DocumentSupporterHelper;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingModuleConsol : ForwardingConsol, ITopLevelBusinessEntityForDocSup
	{
		#region Schema

		public new abstract class Schema : CommonConsol.Schema
		{
			public const string JK_RoutingComplete = "JK_RoutingComplete";
		}

		#endregion

		public ForwardingModuleConsol(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region JK_EntryStatus

		[MaxLength(50)]
		public ZString JK_EntryStatus
		{
			get
			{
				if (fJK_EntryStatus == null)
				{
					fJK_EntryStatus = GetEntryNumStatus();
				}
				return fJK_EntryStatus;
			}
		}
		string fJK_EntryStatus;

		public ZPropertyInfo JK_EntryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.JK_EntryStatus); }
		}

		#endregion

		#region JK_RoutingComplete

		public ZBool JK_RoutingComplete
		{
			get
			{
				if (fJK_RoutingComplete == null)
				{
					fJK_RoutingComplete = new CachedProperty<ZBool>(Factory, new GetValueDelegate<ZBool>(CalcRoutingComplete));
				}

				return fJK_RoutingComplete.Value;
			}
		}

		CachedProperty<ZBool> fJK_RoutingComplete;

		public ZPropertyInfo JK_RoutingCompleteInfo
		{
			get { return GetZPropertyInfo(Schema.JK_RoutingComplete); }
		}

		ZBool CalcRoutingComplete()
		{
			bool matchLoad = false;
			bool matchDischarge = false;

			foreach (Transport transport in Transports)
			{
				if (transport.JW_RL_NKLoadPort == JK_RL_NKLoadPort)
				{
					matchLoad = true;
				}

				if (transport.JW_RL_NKDiscPort == JK_RL_NKDischargePort)
				{
					matchDischarge = true;
				}
			}

			return matchLoad && matchDischarge;
		}

		#endregion

		#region JK_SecurityStatus

		public ZString JK_SecurityStatus
		{
			get
			{
				if (!jk_SecurityStatus.HasValue)
				{
					jk_SecurityStatus = SecurityStatusCode;
				}

				return jk_SecurityStatus.Value;
			}
		}
		ZString? jk_SecurityStatus;

		#endregion

		public ZDecimal JK_TotalLoadingMeters
		{
			get { return Shipments.Cast<ForwardingShipment>().Sum(shipment => shipment.JS_LoadingMeters); }
		}

		public ZBool JK_Calc_PossibleOversize
		{
			get { return Shipments.Cast<ForwardingShipment>().Any(shipment => shipment.FindPossibleOversizePacklineDisplayMessage(this).Length > 0); }
		}

		#region Implementation

		string GetEntryNumStatus()
		{
			return ConsolDomainService.GetInstance(Factory).ModuleConsolCollection.GetEntryStatus(this);
		}

		public Type TopLevelBusinessEntity => typeof(ForwardingConsol);

		#endregion

		#region Validation

		protected override bool EnableLightValidationIfAvailable => false;

		#endregion
	}
}
