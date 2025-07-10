using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class JobCO2eCollection : ActiveBusinessObjectCollection<JobCO2e>, IJobCO2eCollection
	{
		public JobCO2eCollection(ICO2eParent master)
			: base(((BusinessObject)master).Factory, new JobCO2eRelationship(master, typeof(JobCO2e)))
		{
			CountChanged += CO2eCountChanged;
		}

		internal ICO2eParent JobCO2eParent
		{
			get { return (ICO2eParent)Relationship.Master; }
		}

		public JobCO2e AddNew(ZString type)
		{
			var newElement = AddNew();
			newElement.JCO_Type = type;
			return newElement;
		}

		public IJobCO2e Get(ZString type)
		{
			JobCO2eParent.ValidateCO2eType(type);
			return Find(new ZQuery(JobCO2eSchema.JCO_Type, type)).FirstOrDefault();
		}

		void CO2eCountChanged(object sender, EventArgs e)
		{
			if (JobCO2eParent is BusinessObject { IsDeleted: false, IsDeleting: false })
			{
				JobCO2eParent.RefreshCO2e();
			}
		}

		public IJobCO2e GetOrCreate(ZString type) => Get(type) ?? AddNew(type);

		protected override void SetDefaultsForNewElementCore(JobCO2e newElement)
		{
			newElement.JCO_Status = CO2eStatusList.Codes.NotCalculated;
		}

		protected override void OnLoadedIntoCollectionCore(JobCO2e businessObject)
		{
			if (businessObject is IJobCO2e jobCO2e)
			{
				UnhookEvents(jobCO2e);
				HookEvents(jobCO2e);
			}
		}

		public override void Delete(JobCO2e businessObject)
		{
			UnhookEvents(businessObject);
			base.Delete(businessObject);
		}

		void HookEvents(IJobCO2e jobCO2e)
		{
			jobCO2e.StatusChanged += StatusChanged;
			jobCO2e.JobCO2eOnSaving += JobCO2eOnSaving;
			if (jobCO2e is BusinessObject bizo)
			{
				bizo.UpdatedByDataRefresh += UpdatedByDataRefresh;
			}
		}

		void UnhookEvents(IJobCO2e jobCO2e)
		{
			jobCO2e.StatusChanged -= StatusChanged;
			jobCO2e.JobCO2eOnSaving -= JobCO2eOnSaving;
			if (jobCO2e is BusinessObject bizo)
			{
				bizo.UpdatedByDataRefresh -= UpdatedByDataRefresh;
			}
		}

		void StatusChanged(object sender, EventArgs args)
		{
			JobCO2e_StatusChanged?.Invoke(sender, args);
		}

		void UpdatedByDataRefresh(object sender, EventArgs args)
		{
			JobCO2e_UpdatedByDataRefresh?.Invoke(sender, args);
		}

		void JobCO2eOnSaving(object sender, EventArgs args)
		{
			JobCO2e_OnSaving?.Invoke(sender, args);
		}

		IEnumerator<IJobCO2e> IEnumerable<IJobCO2e>.GetEnumerator() => GetEnumerator();

		public event EventHandler JobCO2e_StatusChanged;

		public event EventHandler JobCO2e_UpdatedByDataRefresh;

		public event EventHandler JobCO2e_OnSaving;
	}

	public class JobCO2eRelationship : CollectionRelationship
	{
		public JobCO2eRelationship(ICO2eParent master, Type elementType)
			: base(elementType)
		{
			this.master = master;
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var rhs = obj as JobCO2eRelationship;
			bool result = rhs != null;

			result = result && base.Equals(rhs);
			result = result && Master == rhs.Master;
			result = result && ElementType == rhs.ElementType;
			return result;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ Master.PK.GetHashCode();
		}

		#endregion

		#region Overrides

		protected override ZQuery RelationshipFilterCore
		{
			get
			{
				var query = new ZQuery(JobCO2eSchema.JCO_ParentID, master.JobCO2eParentID);
				query.AddToFilter(JobCO2eSchema.JCO_ParentTableCode, master.JobCO2eParentTableCode);
				return query;
			}
		}

		protected override bool SupportsAddToRelationshipCore()
		{
			return true;
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			businessObject[JobCO2eSchema.JCO_ParentID] = master.JobCO2eParentID;
			businessObject[JobCO2eSchema.JCO_ParentTableCode] = master.JobCO2eParentTableCode;
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			businessObject[JobCO2eSchema.JCO_ParentID] = ZGuid.Empty;
			businessObject[JobCO2eSchema.JCO_ParentTableCode] = ZString.Empty;
		}

		public override BusinessObject Master
		{
			get { return (BusinessObject)master; }
		}
		readonly ICO2eParent master;

		#endregion
	}
}
