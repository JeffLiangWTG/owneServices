using System;
using System.Collections;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class WorkflowModuleTextFilterWithRoutingSupport : WorkflowModuleTextFilter
	{
		public WorkflowModuleTextFilterWithRoutingSupport(ZString description, GetTextQuery queryDelegate, IList list, Type bizObjType)
			: this(description, queryDelegate, list, bizObjType, ZString.Empty, null, ZString.Empty)
		{
		}

		public WorkflowModuleTextFilterWithRoutingSupport(ZString description, GetTextQuery queryDelegate, IList list, Type bizObjType, ZString parentTableCode, SchemaColumn schemaColumnOverride, ZString jobType)
			: base(description, queryDelegate, list, bizObjType, jobType)
		{
			this.schemaColumnOverride = schemaColumnOverride;
			this.parentTableCode = parentTableCode;
		}

		readonly SchemaColumn schemaColumnOverride;
		readonly ZString parentTableCode;

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
			if (!parentTableCode.IsEmpty && parentTableCode.Length <= ProcessTask.Schema.P9_ParentTableCodeMaxLength)
			{
				query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, parentTableCode);
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
			if (reader.Name == "WorkflowModuleTextFilterWithRoutingSupportOrigin")
			{
				Origin = reader.ReadElementString("WorkflowModuleTextFilterWithRoutingSupportOrigin");
			}
			if (reader.Name == "WorkflowModuleTextFilterWithRoutingSupportDestination")
			{
				Destination = reader.ReadElementString("WorkflowModuleTextFilterWithRoutingSupportDestination");
			}
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("WorkflowModuleTextFilterWithRoutingSupportOrigin", Origin);
			writer.WriteElementString("WorkflowModuleTextFilterWithRoutingSupportDestination", Destination);
		}

		#endregion
	}
}
