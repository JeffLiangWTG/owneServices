using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	public class ContainerDetentionExceptionGenerator : ContainerWorkflowExceptionGenerator
	{
		protected override string ContainerExceptionEvent
		{
			get { return ProcessWorkflowExceptionType.ExceptionContainerDetention; }
		}

		protected override DateTimeRegistryItem HighWaterMarkRegistryItem
		{
			get { return ForwardingConfigurationRegistry.Instance.ContainerDetentionExceptionGeneratorHighWaterMark; }
		}

		protected override IEnumerable<ZDBOnlyQuery> ConsolQueries
		{
			get
			{
				yield return ContainerConsolDetentionPassedQuery;
			}
		}

		protected override IEnumerable<ZDBOnlyQuery> DeclarationQueries
		{
			get
			{
				yield return ContainerBrokerageDetentionPassedQuery;
			}
		}

		#region Queries

		ZDBOnlyQuery ContainerConsolDetentionPassedQuery
		{
			get
			{
				ZDBOnlyQuery consolQuery = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
				consolQuery.AddToFilter(JobConsolSchema.JK_IsForwarding, ZBool.True);
				consolQuery.AddSubQuery(CreateContainerSubQuery(JobContainerSchema.JC_JK), JoinCondition.And);

				return consolQuery;
			}
		}

		ZDBOnlyQuery ContainerBrokerageDetentionPassedQuery
		{
			get
			{
				ZDBOnlyQuery declarationQuery = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_IsCancelled, ZBool.False);
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_JS, null);

				ZDBOnlySubQuery cusContainerSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(), CusContainerSchema.CO_JE);
				cusContainerSubQuery.AddSubQuery(CreateContainerSubQuery(CusContainerSchema.CO_JC), JoinCondition.And);

				declarationQuery.AddSubQuery(cusContainerSubQuery, JoinCondition.And);

				return declarationQuery;
			}
		}

		ZDBOnlySubQuery CreateContainerSubQuery(SchemaColumn foreignKey)
		{
			ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), foreignKey);
			containerSubQuery.AddToFilter(JobContainerSchema.JC_EmptyReturnedBy, SQLComparisonOperator.LessThan, ZDateTime.Now);
			containerSubQuery.AddToFilter(JobContainerSchema.JC_ContainerYardEmptyReturnGateIn, ZDateTime.Empty);

			if (HighWaterMark.IsValidSmallDateTime)
			{
				ZQuery searchNarrowingQuery = new ZQuery();
				searchNarrowingQuery.AddToFilter(JobContainerSchema.JC_EmptyReturnedBy, SQLComparisonOperator.GreaterThan, HighWaterMark.AddDays(-3));
				searchNarrowingQuery.AddToFilter(JoinCondition.Or, JobContainerSchema.JC_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThan, HighWaterMark.AddDays(-3));
				containerSubQuery.AddToFilter(searchNarrowingQuery);
			}

			return containerSubQuery;
		}

		#endregion
	}
}
