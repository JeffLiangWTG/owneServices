using System.Collections;
using System.Data;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentContainerPenalty : ContainerPenalty
	{
		public ShipmentContainerPenalty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region CPY_JC_Container
		[RelatedBusinessObject(nameof(Container))]
		[List("RelatedContainersList")]
		public override ZGuid CPY_JC_Container
		{
			get => base.CPY_JC_Container;
			set => base.CPY_JC_Container = value;
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList RelatedContainersList
		{
			get
			{
				var packLineSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.PK, JobContainerPackPivotSchema.J6_JL);
				packLineSubQuery.AddToFilter(JobPackLinesSchema.JL_JS, Shipment?.PK ?? ZGuid.Empty);
				var packPivotSubQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC, JobContainerSchema.PK);
				var containerQuery = new ZDBOnlyQuery(typeof(CommonContainer));
				packPivotSubQuery.AddSubQuery(packLineSubQuery, JoinCondition.And);
				containerQuery.AddSubQuery(packPivotSubQuery, JoinCondition.And);
				var containers = Factory.Load<CommonContainer>(containerQuery);
				return containers;
			}
		}
		#endregion

		public ZString JobServiceCode
		{
			get
			{
				if (CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage)
				{
					if (CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier)
					{
						return ChargeCodeSubGroupList.CarrierStorage;
					}
					else if (CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.CTO)
					{
						return ChargeCodeSubGroupList.Storage;
					}
				}
				else if (CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention && CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier)
				{
					return ChargeCodeSubGroupList.ContainerDetention;
				}
				else if (CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime && CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Transport)
				{
					return ChargeCodeSubGroupList.CartageDemurrageTotal;
				}
				else if (CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention && CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier)
				{
					return ChargeCodeSubGroupList.MergedDemurrageDetention;
				}

				return ZString.Empty; //return empty = no support; service will be disabled
			}
		}

		public ZString JobServiceTimeUnit
		{
			get
			{
				if (CPY_TimeUnit == Core.Constants.ContainerPenaltyTimeUnit.Codes.Hours)
				{
					return JobServiceInfo.Constants.Codes.Hour;
				}
				else if (CPY_TimeUnit == Core.Constants.ContainerPenaltyTimeUnit.Codes.Days)
				{
					return JobServiceInfo.Constants.Codes.Day;
				}
				return CPY_TimeUnit;
			}
		}
	}
}
