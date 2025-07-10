using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.MultiLineAddInfos
{
	public class DependentCusAddInfoCollection<T, MasterT> : DependentBusinessObjectCollection<T, MasterT>
		where T : CusAddInfo
		where MasterT : BusinessObject, ILinkable
	{
		public DependentCusAddInfoCollection(MasterT master, ZString filterType)
			: base(master)
		{
			this.filterType = filterType;
		}

		public DependentCusAddInfoCollection(MasterT master, ZString filterType, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
			this.filterType = filterType;
		}

		public void AddCloneFrom(DependentCusAddInfoCollection<T, MasterT> collectionToCloneFrom, BusinessObjectCloneArgs args, Dictionary<ZGuid, ZGuid> jobDocAddressPKPairs = null)
		{
			args.AddValueOverride(TypeOfElements, FKSchemaColumnInDependent.Name, Master.PK);
			foreach (T dataToClone in collectionToCloneFrom.ToArray())
			{
				var clonedData = (T)dataToClone.Clone(args, jobDocAddressPKPairs);
				using (clonedData.SuspendSettingHasChanges())
				{
					Add(clonedData);
				}
			}
		}

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		readonly protected ZString filterType;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = new ZQuery(CusAddInfoSchema.B7_Type, filterType);
			result.AddToFilter(base.CreateRelationshipFilter());
			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (T)dependent;
			child.B7_Type = filterType;
			child.Parent = Master;
		}

		protected override bool AllowNewCore
		{
			get { return Master != null && !Master.IsDeleted; }
		}
	}

	public class CusAddInfoCollection<T, MasterT> : CusAddInfoCollection<T>
		where T : BaseAddInfo
		where MasterT : BusinessObject, ILinkable
	{
		public CusAddInfoCollection(MasterT master)
			: base(master)
		{
		}

		public CusAddInfoCollection(BusinessObject master, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
		}

		public new MasterT Master
		{
			get { return (MasterT)base.Master; }
		}
	}

	public class CusAddInfoCollection<T> : DependentCusAddInfoCollection<CusAddInfo<T>, BusinessObject>
		where T : BaseAddInfo
	{
		public CusAddInfoCollection(BusinessObject master)
			: this(master, new ZQuery())
		{
		}

		public CusAddInfoCollection(BusinessObject master, ZQuery additionalFilter)
			: base(master, CusAddInfoTypeAttribute.Get(typeof(T)).TypeCode, additionalFilter)
		{
			ConstructorInfo addInfoConstructor = typeof(T).GetConstructor(new Type[] { typeof(ZPropertyInfo) })
				?? throw new InvalidOperationException("The Generic Type specified for this object must have a constructor taking a ZPropertyInfo.");

			if (TypeAttribute == null)
			{
				throw new InvalidOperationException("The Generic Type specified for this object must have a valid CusAddInfoTypeAttribute applied against the class.");
			}
		}

		public List<BusinessObject> GetInners()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			foreach (CusAddInfo<T> element in this)
			{
				result.Add(element.Data);
			}
			return result;
		}

		CusAddInfoTypeAttribute TypeAttribute
		{
			get { return typeAttribute ?? (typeAttribute = CusAddInfoTypeAttribute.Get(typeof(T))); }
		}
		CusAddInfoTypeAttribute typeAttribute;
	}
}
