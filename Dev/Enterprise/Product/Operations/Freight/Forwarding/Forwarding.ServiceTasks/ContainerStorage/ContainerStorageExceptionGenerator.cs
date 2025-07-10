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
	public class ContainerStorageExceptionGenerator : ContainerWorkflowExceptionGenerator
	{
		protected override string ContainerExceptionEvent
		{
			get { return ProcessWorkflowExceptionType.ExceptionContainerStorage; }
		}

		protected override DateTimeRegistryItem HighWaterMarkRegistryItem
		{
			get { return ForwardingConfigurationRegistry.Instance.ContainerStorageExceptionGeneratorHighWaterMark; }
		}

		protected override IEnumerable<ZDBOnlyQuery> ConsolQueries
		{
			get
			{
				yield return ConsolContainerOverriddenChargesStartedQuery;
				yield return ConsolContainerNotOverriddenChargesStartedQuery;
			}
		}

		protected override IEnumerable<ZDBOnlyQuery> DeclarationQueries
		{
			get
			{
				yield return DeclarationContainerOverriddenChargesStartedQuery;
				yield return DeclarationContainerNotOverriddenChargesStartedQuery;
			}
		}

		#region Queries

		ZDBOnlySubQuery CreateContainerSubQuery(SchemaColumn foreignKey, bool isOverriddenCharges)
		{
			ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), foreignKey);
			containerSubQuery.AddToFilter(JobContainerSchema.JC_FCLWharfGateOut, ZDateTime.Empty);

			if (isOverriddenCharges)
			{
				containerSubQuery.AddToFilter(JobContainerSchema.JC_OverrideFCLAvailableStorage, ZBool.True);
				containerSubQuery.AddToFilter(JobContainerSchema.JC_ArrivalCTOStorageStartDate, SQLComparisonOperator.LessThan, ZDateTime.Now);

				if (HighWaterMark.IsValidSmallDateTime)
				{
					ZQuery searchNarrowingQuery = new ZQuery(JobContainerSchema.JC_ArrivalCTOStorageStartDate, SQLComparisonOperator.GreaterThan, HighWaterMark.AddDays(-3));
					searchNarrowingQuery.AddToFilter(JoinCondition.Or, JobContainerSchema.JC_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThan, HighWaterMark.AddDays(-3));
					containerSubQuery.AddToFilter(searchNarrowingQuery);
				}
				else
				{
					containerSubQuery.AddToFilter(JobContainerSchema.JC_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddDays(-20));
				}
			}
			else
			{
				containerSubQuery.AddToFilter(JobContainerSchema.JC_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddDays(-20));
				containerSubQuery.AddToFilter(JobContainerSchema.JC_OverrideFCLAvailableStorage, ZBool.False);
			}

			return containerSubQuery;
		}

		ZDBOnlyQuery ConsolContainerOverriddenChargesStartedQuery
		{
			get
			{
				ZDBOnlyQuery consolQuery = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
				consolQuery.AddToFilter(JobConsolSchema.JK_IsForwarding, ZBool.True);

				ZDBOnlySubQuery containerSubQuery = CreateContainerSubQuery(JobContainerSchema.JC_JK, true);
				consolQuery.AddSubQuery(containerSubQuery, JoinCondition.And);

				return consolQuery;
			}
		}

		ZDBOnlyQuery ConsolContainerNotOverriddenChargesStartedQuery
		{
			get
			{
				ZDBOnlyQuery consolQuery = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
				consolQuery.AddToFilter(JobConsolSchema.JK_IsForwarding, ZBool.True);

				ZDBOnlySubQuery containerSubQuery = CreateContainerSubQuery(JobContainerSchema.JC_JK, false);
				consolQuery.AddSubQuery(containerSubQuery, JoinCondition.And);

				ZDBOnlySubQuery destinationSubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_StorageDate, SQLComparisonOperator.LessThan, ZDateTime.Now);
				destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_StorageDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddDays(-20));

				ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
				sailingSubQuery.AddSubQuery(destinationSubQuery, JoinCondition.And);

				ZDBOnlySubQuery transportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transportSubQuery.AddToFilter(JobConsolTransportSchema.JW_IsLinked, ZBool.True);
				transportSubQuery.AddSubQuery(sailingSubQuery, JoinCondition.And);

				consolQuery.AddSubQuery(transportSubQuery, JoinCondition.And);

				return consolQuery;
			}
		}

		ZDBOnlyQuery DeclarationContainerOverriddenChargesStartedQuery
		{
			get
			{
				ZDBOnlyQuery declarationQuery = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());

				ZDBOnlySubQuery containerSubQuery = CreateContainerSubQuery(CusContainerSchema.CO_JC, true);

				ZDBOnlySubQuery cusContainerSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(), CusContainerSchema.CO_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				cusContainerSubQuery.AddSubQuery(containerSubQuery, JoinCondition.And);

				declarationQuery.AddSubQuery(cusContainerSubQuery, JoinCondition.And);

				return declarationQuery;
			}
		}

		ZDBOnlyQuery DeclarationContainerNotOverriddenChargesStartedQuery
		{
			get
			{
				var declarationQuery = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());

				var containerSubQuery = CreateContainerSubQuery(CusContainerSchema.CO_JC, false);
				var cusContainerSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(), CusContainerSchema.CO_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				cusContainerSubQuery.AddSubQuery(containerSubQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(cusContainerSubQuery, JoinCondition.And);

				var destinationSubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_StorageDate, SQLComparisonOperator.NotEqual, null);
				destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_StorageDate, SQLComparisonOperator.LessThan, ZDateTime.Now);
				destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_StorageDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddDays(-20));

				var sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
				sailingSubQuery.AddSubQuery(destinationSubQuery, JoinCondition.And);

				var transportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transportSubQuery.AddToFilter(JobConsolTransportSchema.JW_IsLinked, ZBool.True);
				transportSubQuery.AddToFilter(JobConsolTransportSchema.JW_IsCharter, ZBool.False);
				transportSubQuery.AddSubQuery(sailingSubQuery, JoinCondition.And);

				declarationQuery.AddSubQuery(transportSubQuery, JoinCondition.And);

				return declarationQuery;
			}
		}

		#endregion
	}
}
