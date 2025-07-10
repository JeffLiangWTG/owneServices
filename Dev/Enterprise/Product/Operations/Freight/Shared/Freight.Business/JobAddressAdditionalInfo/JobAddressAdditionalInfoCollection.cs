using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobAddressAdditionalInfoCollection : ActiveBusinessObjectCollection<JobAddressAdditionalInfo>, IJobAddressAdditionalInfoCollection
	{
		public JobAddressAdditionalInfoCollection(IJobAddressAdditionalInfoSupport master, BusinessObjectFactory factory)
			: base(((BusinessObject)master).Factory, new AddressAdditionalInfoRelationship(master, typeof(JobAddressAdditionalInfo)))
		{
		}

		internal IJobAddressAdditionalInfoSupport AddressAdditionalInfoParent
		{
			get { return (IJobAddressAdditionalInfoSupport)Relationship.Master; }
		}

		public JobAddressAdditionalInfo AddNew(ZString type)
		{
			var newElement = AddNew();
			newElement.JAI_AddressType = type;
			return newElement;
		}

		public IJobAddressAdditionalInfo Get(ZString type)
		{
			AddressAdditionalInfoParent.ValidateAdressType(type);
			return Find(new ZQuery(JobAddressAdditionalInfoSchema.JAI_AddressType, type)).FirstOrDefault();
		}

		IJobAddressAdditionalInfo IJobAddressAdditionalInfoCollection.Get(ZString type)
		{
			return Find(new ZQuery(JobAddressAdditionalInfoSchema.JAI_AddressType, type)).FirstOrDefault();
		}

		IEnumerator<IJobAddressAdditionalInfo> IEnumerable<IJobAddressAdditionalInfo>.GetEnumerator() => GetEnumerator();

		IJobAddressAdditionalInfo IJobAddressAdditionalInfoCollection.GetOrCreate(ZString type) => Get(type) ?? AddNew(type);
	}

	public class AddressAdditionalInfoRelationship : CollectionRelationship
	{
		public AddressAdditionalInfoRelationship(IJobAddressAdditionalInfoSupport master, Type elementType)
			: base(elementType)
		{
			this.master = master;
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var rhs = obj as AddressAdditionalInfoRelationship;
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
				var query = new ZQuery(JobAddressAdditionalInfoSchema.JAI_ParentID, master.JobAddressAdditionalInfoParentID);
				query.AddToFilter(JobAddressAdditionalInfoSchema.JAI_ParentTableCode, master.JobAddressAdditionalInfoTableCode);
				return query;
			}
		}

		protected override bool SupportsAddToRelationshipCore()
		{
			return true;
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			businessObject[JobAddressAdditionalInfoSchema.JAI_ParentID] = master.JobAddressAdditionalInfoParentID;
			businessObject[JobAddressAdditionalInfoSchema.JAI_ParentTableCode] = master.JobAddressAdditionalInfoTableCode;
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			businessObject[JobAddressAdditionalInfoSchema.JAI_ParentTableCode] = string.Empty;
			businessObject[JobAddressAdditionalInfoSchema.JAI_ParentID] = Guid.Empty;
		}

		public override BusinessObject Master
		{
			get { return (BusinessObject)master; }
		}
		readonly IJobAddressAdditionalInfoSupport master;

		#endregion
	}
}
