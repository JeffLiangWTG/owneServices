using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface ISalesValueAssociatedEntity : IBusiness, IAuditDetails
	{
		ZString ID { get; }

		ZString EntityType { get; }
		ControllerID ControllerID { get; }

		ZGuid? CompanyPk { get; }
		ZString CompanyCode { get; }
		ZPropertyInfo OrgPkInfo { get; }

		ZString Summary { get; }
		ZString ValueCurrency { get; }
		ZDateTime DateForExchangeRate { get; }
		string TablePrefix { get; }

		ISalesHeaderCollection ActualAndProspectiveSalesHeaderCollection { get; }
		ISalesHeaderCollection ProspectiveSalesHeaderCollection { get; }

		ZDateTime GetDateAssociatedToSalesValue(ISalesValue salesValue);
		ZString GetUserThatAssociatedToSalesValue(ISalesValue salesValue);
	}

	public static class SalesValueAssociatedEntity
	{
		public static void DeleteAllSalesValuePivots(ISalesValueAssociatedEntity entity)
		{
			var associationPivotsQuery = new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, entity.Identifier);
			associationPivotsQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, entity.TablePrefix);

			var factory = entity.Factory;
			var associationPivots = factory.Load<OrgSalesValueAssociationPivot>(associationPivotsQuery);
			foreach (var pivot in associationPivots)
			{
				pivot.Delete();
			}
		}
	}
}
