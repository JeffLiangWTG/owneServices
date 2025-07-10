using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	sealed class DeclarationOrderTrackingDatesMap : OrderTrackingDatesMap
	{
		public DeclarationOrderTrackingDatesMap(BusinessObject declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}

		readonly BusinessObject declaration;

		const string IsImportPropertyName = "IsImport";

		ZBool IsImportDeclaration
		{
			get { return new ZBool(declaration[IsImportPropertyName]); }
		}

		public override ZDateTime DepartureActualDate
		{
			get
			{
				var transport = ((IRoutingSupport)declaration).TransportsIncludingRelated.DepartureTransport;
				return transport != null ? transport.JW_ATD : ZDateTime.Empty;
			}
		}

		public override ZDateTime ArrivalActualDate
		{
			get
			{
				var transport = ((IRoutingSupport)declaration).TransportsIncludingRelated.ArrivalTransport;
				return transport != null ? transport.JW_ATA : ZDateTime.Empty;
			}
		}

		public override ZDateTime CargoAvailableActualDate
		{
			get { return ZDateTime.Empty; }
		}

		public override ZDateTime DeliveryCartageAdvisedActualDate
		{
			get
			{
				return IsImportDeclaration
				  ? ((IShipmentWithDocsAndCartage)declaration).DocsAndCartage.JP_DeliveryCartageAdvised
				  : ZDateTime.Empty;
			}
		}

		public override ZDateTime DeliveryCartageCompleteFinalizedActualDate
		{
			get
			{
				return IsImportDeclaration
				  ? ((IShipmentWithDocsAndCartage)declaration).DocsAndCartage.JP_DeliveryCartageCompleted
				  : ZDateTime.Empty;
			}
		}

		public override ZDateTime DepartureScheduledDate
		{
			get { return (ZDateTime)declaration[JobDeclarationSchema.Constants.JE_DateAtOrigin]; }
		}

		public override ZDateTime ArrivalScheduledDate
		{
			get { return (ZDateTime)declaration[JobDeclarationSchema.Constants.JE_DateAtFinalDestination]; }
		}

		public override ZDateTime DeliveryCartageCompleteFinalizedScheduledDate
		{
			get
			{
				return IsImportDeclaration
				  ? ((IShipmentWithDocsAndCartage)declaration).DocsAndCartage.JP_EstimatedDelivery
				  : ZDateTime.Empty;
			}
		}
	}
}
