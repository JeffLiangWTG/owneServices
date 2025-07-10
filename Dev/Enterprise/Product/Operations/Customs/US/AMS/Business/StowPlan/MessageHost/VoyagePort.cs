using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class VoyagePort : AutoVoyagePort
	{
		public VoyagePort(JobVoyage voyage, ZString port)
			: base(voyage.Factory)
		{
			this.voyage = voyage;
			this.port = port;
		}
		readonly JobVoyage voyage;
		readonly ZString port;

		public override ZString Port
		{
			get { return port; }
		}

		public override ZString MessageStatusDescription
		{
			get { return Factory.GetCachedValue<MessageStatusListSTW>().GetDescriptionFromCode(MessageStatusCode); }
		}

		public ZDateTime ArrivalTime
		{
			get
			{
				var result = ZDateTime.Empty;
				if (Destination != null)
				{
					result = Destination.JB_E_ARV;
				}
				else if (Origin != null)
				{
					result = Origin.JA_E_DEP;
				}
				return result;
			}
		}

		public ZDateTime DepartureTime
		{
			get
			{
				var result = ZDateTime.Empty;
				if (Origin != null)
				{
					return result = Origin.JA_E_DEP;
				}
				else if (Destination != null)
				{
					return result = Destination.JB_E_ARV;
				}
				return result;
			}
		}

		IStowPlanMessageAttachee MessageHost
		{
			get
			{
				IStowPlanMessageAttachee result = null;
				if (Origin != null && Origin.Messages.Cast<EDIMessage>().Any(x => x.EM_ApplicationCode == EDIMessage.ApplicationCodes.StowPlan))
				{
					result = Origin;
				}
				else if (Destination != null && Destination.Messages.Cast<EDIMessage>().Any(x => x.EM_ApplicationCode == EDIMessage.ApplicationCodes.StowPlan))
				{
					result = Destination;
				}
				else if (Origin != null)
				{
					result = Origin;
				}
				else
				{
					result = Destination;
				}
				return result;
			}
		}

		public ZString MessageStatusCode
		{
			get
			{
				var host = MessageHost;
				return host != null ? host.StowPlanMessageStatus : ZString.Empty;
			}
			set
			{
				var host = MessageHost;
				if (host != null)
				{
					host.StowPlanMessageStatus = value;
				}
			}
		}

		public EDIMessageCollection Messages
		{
			get
			{
				EDIMessageCollection result = null;
				var host = MessageHost;
				if (host != null)
				{
					result = new STWMessageCollection((BusinessObject)host, Factory);
					result.Load();
					result.IsManagedForDataRefresh = true;
				}
				return result;
			}
		}

		internal VoyageOrigin Origin
		{
			get { return fOrigin ?? (fOrigin = voyage.Origins.Cast<VoyageOrigin>().FirstOrDefault(x => x.JA_RL_NKPortOfLoading == Port)); }
		}
		VoyageOrigin fOrigin;

		internal VoyageDestination Destination
		{
			get { return fDestination ?? (fDestination = voyage.Destinations.Cast<VoyageDestination>().FirstOrDefault(x => x.JB_RL_NKPortOfDischarge == Port)); }
		}
		VoyageDestination fDestination;
	}
}
