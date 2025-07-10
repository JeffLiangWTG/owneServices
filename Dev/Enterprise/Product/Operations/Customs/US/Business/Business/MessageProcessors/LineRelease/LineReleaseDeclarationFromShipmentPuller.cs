using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	public class LineReleaseDeclarationFromShipmentPuller : Customs.Business.DeclarationFromShipmentPuller
	{
		public LineReleaseDeclarationFromShipmentPuller(LineReleaseDeclarationCreator lineReleaseCreator)
			: base(lineReleaseCreator.message.Factory)
		{
			this.lineReleaseCreator = lineReleaseCreator;
		}

		readonly LineReleaseDeclarationCreator lineReleaseCreator;

		protected override Customs.Business.BaseJobDeclaration CreateOrLoadDeclarationForShipmentCore(BusinessObjectFactory factoryToCreateFor)
		{
			return lineReleaseCreator.AddToShipment(ShipmentPK, factoryToCreateFor);
		}

		protected override ForwardingShipmentCollection GetNewForwardingShipmentCollection()
		{
			var declarationQuery = JobDeclarationFilter.ForCountry(false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, lineReleaseCreator.message.Factory);
			var subQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.JE_JS, true);
			subQuery.AddToFilter(declarationQuery);
			subQuery.AddToFilter(JobDeclarationSchema.JE_JS, SQLComparisonOperator.NotEqual, null);
			var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
			query.AddSubQuery(JobShipmentSchema.PK, subQuery, JoinCondition.And);
			var result = new ForwardingShipmentCollection(Factory, query);
			result.SetOverrideNotificationWhenAdditionalFilterNotMet("This Shipment has already a Declaration linked to it; please select another Shipment.");
			return result;
		}
	}
}
