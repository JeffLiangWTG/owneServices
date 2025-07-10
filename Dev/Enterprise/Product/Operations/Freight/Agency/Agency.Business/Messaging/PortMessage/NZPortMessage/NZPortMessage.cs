using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class NZPortMessage : PortMessage
	{
		public NZPortMessage(JobVoyage voyage)
			: base(voyage)
		{
		}

		#region Properties

		#region CTO

		[ReadOnly(true)]
		[List("Lookups.CTOs_List")]
		public override ZGuid CTO
		{
			get
			{
				if (cto.IsEmpty)
				{
					if (Direction == Core.Constants.PortDirection.Load)
					{
						var origin = Voyage.Origins.Cast<VoyageOrigin>().FirstOrDefault(o => o.JA_RL_NKPortOfLoading == Port);
						cto = (origin != null && origin.DepartureCTOAddress != null) ? origin.DepartureCTOAddress.OA_OH : ZGuid.Empty;
					}

					if (Direction == Core.Constants.PortDirection.Discharge)
					{
						var destination = Voyage.Destinations.Cast<VoyageDestination>().FirstOrDefault(d => d.JB_RL_NKPortOfDischarge == Port);
						cto = (destination != null && destination.ArrivalCTOAddress != null) ? destination.ArrivalCTOAddress.OA_OH : ZGuid.Empty;
					}
				}

				return cto;
			}
		}

		ZGuid cto;

		#endregion

		#region Port

		public override ZString Port
		{
			get
			{
				return base.Port;
			}
			set
			{
				base.Port = value;
				cto = ZGuid.Empty;

				UpdatePrincipal();
				UpdateMessageType();
				CTOInfo.RefreshBinding();
			}
		}

		protected override void UpdateDirectionOnPortChanged()
		{
			var list = Lookups.Direction_List;
			if (list.Count == 0)
			{
				Direction = ZString.Empty;
			}
			else if (list.Count == 1)
			{
				Direction = list[0].Code;
			}
		}

		#endregion

		#region Direction

		public override ZString Direction
		{
			get
			{
				return base.Direction;
			}
			set
			{
				base.Direction = value;
				cto = ZGuid.Empty;

				UpdatePrincipal();
				UpdateMessageType();
				CTOInfo.RefreshBinding();
			}
		}

		#endregion

		#region Principal

		void UpdatePrincipal()
		{
			var principalList = Lookups.Principal_List;
			if (principalList.Count == 1)
			{
				PrincipalPK = principalList[0].PK;
			}
			else if (PrincipalPK != ZGuid.Empty && !principalList.OfType<OrgHeader>().Any(x => x.PK == PrincipalPK))
			{
				PrincipalPK = ZGuid.Empty;
			}
		}

		#endregion

		public ZString LastMessageSent
		{
			get
			{
				if (lastMessageSent == null)
				{
					lastMessageSent = new CachedProperty<ZString>(Factory, GetLastMessageSent);
				}

				return lastMessageSent.Value;
			}
		}
		CachedProperty<ZString> lastMessageSent;

		#endregion

		#region PortMessage

		protected override PortMessageValidation GetNewValidation()
		{
			return new NZPortMessageValidation(this);
		}

		protected override PortMessageLookups GetNewLookups()
		{
			return new NZPortMessageLookups(this);
		}

		#endregion

		#region Implementation

		#region RelatedBillOfLadings

		public ZString GetSenderID()
		{
			var portConfig = PortManifestRegistryHelper.RetrievePortConfiguration(Port, PrincipalPK);
			return portConfig == null || !portConfig.Enabled ? ZString.Empty : portConfig.SenderID;
		}

		public override IEnumerable<BillOfLading> GetRelatedShipments(BusinessObjectFactory factory = null)
		{
			var filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, Voyage, Port, Direction);
			return Factory.Load<BillOfLading>(filter).Where
				(x => x.Principal != null && x.Principal.PK == PrincipalPK);
		}

		#endregion

		void UpdateMessageType()
		{
			lastMessageSent = null;

			if (Direction.IsEmpty || Port.IsEmpty)
			{
				MessageType = ZString.Empty;
			}
			else if (LastMessageSent == ZString.Empty || LastMessageSent == PortMessageTypeList.Codes.Cancellation)
			{
				MessageType = PortMessageTypeList.Codes.Original;
			}
			else if (LastMessageSent == PortMessageTypeList.Codes.Original || LastMessageSent == PortMessageTypeList.Codes.Replace)
			{
				MessageType = PortMessageTypeList.Codes.Replace;
			}

			MessageTypeInfo.RefreshBinding();
		}

		ZString GetLastMessageSent()
		{
			if (Direction.IsEmpty || Port.IsEmpty)
			{
				return ZString.Empty;
			}

			var original = Direction == Constants.PortDirection.Load ? Constants.EventReferenceMessageTypes.LoadManifest : Constants.EventReferenceMessageTypes.DischargeManifest;
			var replacement = Direction == Constants.PortDirection.Load ? Constants.EventReferenceMessageTypes.LoadManifestReplacement : Constants.EventReferenceMessageTypes.DischargeManifestReplacement;
			var cancellation = Direction == Constants.PortDirection.Load ? Constants.EventReferenceMessageTypes.LoadManifestCancellation : Constants.EventReferenceMessageTypes.DischargeManifestCancellation;

			var logs = new[]
			{
				Voyage.Logs.MostRecentLogByEventTime(Events.MessageSent,                  WhereParametersAre(type: original, location: Port)),
				Voyage.Logs.MostRecentLogByEventTime(Events.MessageSent,                  WhereParametersAre(type: replacement, location: Port)),
				Voyage.Logs.MostRecentLogByEventTime(Events.MessageWithdrawCancelRequest, WhereParametersAre(type: cancellation, location: Port))
			};

			var mostRecentLog = logs.Where(l => l != null).OrderByDescending(l => l.SL_EventTime).FirstOrDefault();

			if (mostRecentLog == null)
			{
				return ZString.Empty;
			}
			else if (mostRecentLog.SL_SE_NKEvent == Events.MessageSentCode)
			{
				return WhereParametersAre(original, Port)(mostRecentLog) ? PortMessageTypeList.Codes.Original : PortMessageTypeList.Codes.Replace;
			}
			else
			{
				return PortMessageTypeList.Codes.Cancellation;
			}
		}

		static Func<StmALog, bool> WhereParametersAre(string type, string location)
		{
			Func<StmALog, bool> result = log =>
			{
				string type1;
				string location1;

				return log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out type1)
					&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, out location1)
					&& StringComparer.InvariantCultureIgnoreCase.Compare(type, type1) == 0
					&& StringComparer.InvariantCultureIgnoreCase.Compare(location, location1) == 0;
			};

			return result;
		}

		#endregion
	}
}


