using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class ContainerFilterFactory
	{
		#region Constructor & Static Instance

		ContainerFilterFactory()
		{
		}

		public static ContainerFilterFactory Instance
		{
			get { return fInstance ?? (fInstance = new ContainerFilterFactory()); }
		}

		[ThreadStatic]
		static ContainerFilterFactory fInstance;

		#endregion Constructor & Static Instance

		#region Container -> Shipments Relationship Query

		public ZDBOnlySubQuery GetContainerShipmentSubQuery(SchemaColumn schemaColumn, object value)
		{
			return GetContainerShipmentSubQuery(schemaColumn, SQLComparisonOperator.Equal, value);
		}

		public ZDBOnlySubQuery GetContainerShipmentSubQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(TrackingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter(schemaColumn, comparisonOperator, value);
			return GetContainerShipmentSubQueryCore(shipmentSubQuery);
		}

		public ZDBOnlySubQuery GetContainerShipmentSubQuery(ZDBOnlySubQuery shipmentSubQuery)
		{
			return GetContainerShipmentSubQueryCore(shipmentSubQuery);
		}

		ZDBOnlySubQuery GetContainerShipmentSubQueryCore(ZDBOnlySubQuery shipmentSubQuery)
		{
			ZDBOnlySubQuery packlinesQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.PK);
			packlinesQuery.AddSubQuery(JobPackLinesSchema.JL_JS, shipmentSubQuery, JoinCondition.And);

			ZDBOnlySubQuery containerPackPivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC);
			containerPackPivotQuery.AddSubQuery(JobContainerPackPivotSchema.J6_JL, packlinesQuery, JoinCondition.And);
			return containerPackPivotQuery;
		}

		#endregion

		#region Liner & Agency Container -> Shipments Relationship Query

		public ZDBOnlySubQuery GetLinerAndAgencyContainerShipmentSubQuery(SchemaColumn schemaColumn, object value)
		{
			return GetLinerAndAgencyContainerShipmentSubQuery(schemaColumn, SQLComparisonOperator.Equal, value);
		}

		public ZDBOnlySubQuery GetLinerAndAgencyContainerShipmentSubQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			var shipmentSubQuery = new ZDBOnlySubQuery(typeof(AgencyShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter(schemaColumn, comparisonOperator, value);
			return GetLinerAndAgencyContainerShipmentSubQueryCore(shipmentSubQuery);
		}

		public ZDBOnlySubQuery GetLinerAndAgencyContainerShipmentSubQuery(ZDBOnlySubQuery shipmentSubQuery)
		{
			return GetLinerAndAgencyContainerShipmentSubQueryCore(shipmentSubQuery);
		}

		ZDBOnlySubQuery GetLinerAndAgencyContainerShipmentSubQueryCore(ZDBOnlySubQuery shipmentSubQuery)
		{
			var packlinesQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.PK);
			packlinesQuery.AddSubQuery(JobPackLinesSchema.JL_JS, shipmentSubQuery, JoinCondition.And);

			var containerPackPivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC);
			containerPackPivotQuery.AddSubQuery(JobContainerPackPivotSchema.J6_JL, packlinesQuery, JoinCondition.And);
			return containerPackPivotQuery;
		}

		#endregion

		#region Container -> Declaration Relationship Query

		public ZDBOnlySubQuery GetContainerStandAloneDeclarationSubQuery(SchemaColumn schemaColumn, object value)
		{
			return GetContainerStandAloneDeclarationSubQuery(schemaColumn, SQLComparisonOperator.Equal, value);
		}

		public ZDBOnlySubQuery GetContainerStandAloneDeclarationSubQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			declarationSubQuery.AddToFilter(schemaColumn, comparisonOperator, value);
			return GetContainerStandAloneDeclarationSubQueryCore(declarationSubQuery);
		}

		public ZDBOnlySubQuery GetContainerStandAloneDeclarationSubQuery(ZDBOnlySubQuery declarationSubQuery)
		{
			return GetContainerStandAloneDeclarationSubQueryCore(declarationSubQuery);
		}

		ZDBOnlySubQuery GetContainerStandAloneDeclarationSubQueryCore(ZDBOnlySubQuery declarationSubQuery)
		{
			ZDBOnlySubQuery cusContainerSubQuery = new ZDBOnlySubQuery(typeof(BaseCusContainer), CusContainerSchema.CO_JC);
			cusContainerSubQuery.AddSubQuery(CusContainerSchema.CO_JE, declarationSubQuery, JoinCondition.And);
			return cusContainerSubQuery;
		}

		#endregion

		#region Container -> Orders Relationship Query

		public ZDBOnlySubQuery GetContainerOrdersSubQuery(SchemaColumn schemaColumn, object value)
		{
			return GetContainerOrdersSubQuery(schemaColumn, SQLComparisonOperator.Equal, value);
		}

		public ZDBOnlySubQuery GetContainerOrdersSubQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			ZDBOnlySubQuery ordersSubQuery = new ZDBOnlySubQuery(typeof(TrackingOrder), JobOrderHeaderSchema.JD_JS);
			ordersSubQuery.AddToFilter(schemaColumn, comparisonOperator, value);

			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(TrackingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddSubQuery(ordersSubQuery, JoinCondition.And);

			return GetContainerShipmentSubQueryCore(shipmentSubQuery);
		}

		#endregion
	}
}
