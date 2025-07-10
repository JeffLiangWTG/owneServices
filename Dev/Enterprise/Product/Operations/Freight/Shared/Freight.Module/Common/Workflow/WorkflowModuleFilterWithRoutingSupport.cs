using System;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class WorkflowModuleFilterWithRoutingSupport : WorkflowModuleFilter
	{
		public WorkflowModuleFilterWithRoutingSupport(ZString description, Type bizObjType, WorkflowModuleFilterTypes filterType)
			: this(description, bizObjType, filterType, ZString.Empty, null, ZString.Empty)
		{
		}

		public WorkflowModuleFilterWithRoutingSupport(string description, Type businessObjectType, WorkflowModuleFilterTypes filterType, ZString parentTableCodeOverride, SchemaColumn schemaColumnOverride, ZString templateCode)
			: base(description, businessObjectType, filterType, templateCode)
		{
			this.parentTableCodeOverride = parentTableCodeOverride;
			this.schemaColumnOverride = schemaColumnOverride;
		}

		readonly ZString parentTableCodeOverride;
		readonly SchemaColumn schemaColumnOverride;

		#region Lookups

		public RefUNLOCOCollection UNLOCOs
		{
			get { return fUNLOCOs ?? (fUNLOCOs = new RefUNLOCOCollection(new BusinessObjectFactory())); }
		}

		RefUNLOCOCollection fUNLOCOs;

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			Origin = ZString.Empty;
			Destination = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && Origin.IsEmpty && Destination.IsEmpty;

		#endregion

		#region Properties

		[List("UNLOCOs")]
		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public ZString Origin
		{
			get { return fOrigin; }
			set
			{
				if (fOrigin != value)
				{
					CheckMaximumLength(OriginInfo, value);
					fOrigin = value;
					OriginInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZString fOrigin;

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(nameof(Origin)); }
		}

		protected bool Origin_ReadOnly
		{
			get { return !WorkflowRoutingSupportHelper.IsTransportLinkedEvent(MilestoneEvent); }
		}

		[List("UNLOCOs")]
		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public ZString Destination
		{
			get { return fDestination; }
			set
			{
				if (fDestination != value)
				{
					CheckMaximumLength(DestinationInfo, value);
					fDestination = value;
					DestinationInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZString fDestination;

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(nameof(Destination)); }
		}

		protected bool Destination_ReadOnly
		{
			get { return !WorkflowRoutingSupportHelper.IsTransportLinkedEvent(MilestoneEvent); }
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			return !IsEmpty
				? WorkflowModuleFilterQueryBuilder.BuildQuery(businessObjectType, GetMilestoneQuery(), RelatedParentSubQueries, schemaColumnOverride)
				: new ZQuery();
		}

		protected override ZDBOnlySubQuery GetMilestoneQuery()
		{
			ZDBOnlySubQuery resultSubQuery = base.GetMilestoneQuery();
			WorkflowRoutingSupportHelper.AddTransportSubQuery(resultSubQuery, Origin, Destination);

			return resultSubQuery;
		}

		protected override void AddParentTableCodeQuery(ZDBOnlySubQuery query)
		{
			if (!parentTableCodeOverride.IsEmpty && parentTableCodeOverride.Length <= ProcessTask.Schema.P9_ParentTableCodeMaxLength)
			{
				query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, parentTableCodeOverride);
			}
			else
			{
				base.AddParentTableCodeQuery(query);
			}
		}

		#endregion

		#region XML Serialization

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == "WorkflowModuleFilterWithRoutingSupportOrigin")
			{
				Origin = reader.ReadElementString("WorkflowModuleFilterWithRoutingSupportOrigin");
			}
			if (reader.Name == "WorkflowModuleFilterWithRoutingSupportDestination")
			{
				Destination = reader.ReadElementString("WorkflowModuleFilterWithRoutingSupportDestination");
			}
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("WorkflowModuleFilterWithRoutingSupportOrigin", Origin);
			writer.WriteElementString("WorkflowModuleFilterWithRoutingSupportDestination", Destination);
		}

		#endregion

	}
}
