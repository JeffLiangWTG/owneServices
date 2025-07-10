using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;

namespace Enterprise.Customs.Business
{
	partial class BaseJobComInvoiceHeader : IRoutingSupport, ITransportParent
	{
		#region IRoutingSupport Implementation

		class BaseJobComInvoiceHeaderRoutingSupport : IRoutingSupport
		{
			public BaseJobComInvoiceHeaderRoutingSupport(BaseJobComInvoiceHeader invoiceHeader)
			{
				this.invoiceHeader = invoiceHeader;
			}
			readonly BaseJobComInvoiceHeader invoiceHeader;

			BusinessObjectFactory IRoutingSupport.Factory
			{
				get { return invoiceHeader.Factory; }
			}

			TransportCollection IRoutingSupport.Transports
			{
				get { return invoiceHeader.Transports; }
			}

			RoutingCollection IRoutingSupport.TransportsIncludingRelated
			{
				get { return invoiceHeader.TransportsIncludingRelated; }
			}

			ZString IRoutingSupport.TransportMode
			{
				get { return invoiceHeader.JobDeclaration?.JE_TransportMode ?? ZString.Empty; }
			}

			string IRoutingSupport.AdditionalETAUpdateMsg
			{
				get { return null; }
			}

			string IRoutingSupport.AdditionalETDUpdateMsg
			{
				get { return null; }
			}
		}

		TransportCollection IRoutingSupport.Transports
		{
			get { return RoutingSupportProvider.Transports; }
		}

		RoutingCollection IRoutingSupport.TransportsIncludingRelated
		{
			get { return RoutingSupportProvider.TransportsIncludingRelated; }
		}

		ZString IRoutingSupport.TransportMode
		{
			get { return RoutingSupportProvider.TransportMode; }
		}

		string IRoutingSupport.AdditionalETAUpdateMsg
		{
			get { return RoutingSupportProvider.AdditionalETAUpdateMsg; }
		}

		string IRoutingSupport.AdditionalETDUpdateMsg
		{
			get { return RoutingSupportProvider.AdditionalETDUpdateMsg; }
		}

		IRoutingSupport RoutingSupportProvider
		{
			get { return routingSupportProvider ?? (routingSupportProvider = GetRoutingSupportProvider()); }
		}
		IRoutingSupport routingSupportProvider;

		IRoutingSupport GetRoutingSupportProvider()
		{
			return new BaseJobComInvoiceHeaderRoutingSupport(this);
		}

		[ChildEditable]
		public TransportCollection Transports
		{
			get
			{
				if (transports == null)
				{
					transports = new TransportCollection(this);
					transports.Load();
					RegisterEditableChildObject(transports);
				}

				return transports;
			}
		}
		TransportCollection transports;

		public RoutingCollection TransportsIncludingRelated
		{
			get { return transportsIncludingRelated ?? (transportsIncludingRelated = new RoutingCollection(this)); }
		}
		RoutingCollection transportsIncludingRelated;

		#endregion

		#region ITransportParent Basic Properties

		ZString ITransportParentCommon.TypeCode
		{
			get { return Constants.TransportParentTypes.CommercialInvoice; }
		}

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new BaseJobComInvoiceHeaderTransportSupporter(this); }
		}

		#endregion

		#region ITransportChangeNotifier Members

		void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
		}

		#endregion
	}
}
