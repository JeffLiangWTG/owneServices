using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusSupportingInfoCollection<out T> : IBusinessObjectCollection<T>
		where T : CusSupportingInfo
	{
		BusinessObject Master { get; }
		new T this[int index] { get; }
		new T AddNew();

		void AddCloneFrom(IEnumerable<CusSupportingInfo> collectionToCloneFrom, BusinessObjectCloneArgs args);
		void SetReadOnlyIncludingChildren(bool readOnly);
		bool Any();
		T First();
		T FirstOrDefault();
		T Last();
		int MaxCount { get; }
	}

	public class CusSupportingInfoCollection<T> : DependentBusinessObjectCollection<T, BusinessObject>, ICusSupportingInfoCollection<T>
		where T : CusSupportingInfo
	{
		public CusSupportingInfoCollection(BusinessObject parent, ZString cSI_Type)
			: base(parent)
		{
			this.CSI_Type = cSI_Type;
		}

		public CusSupportingInfoCollection(BusinessObject parent, ZString cSI_Type, ZString cSI_SubType)
			: this(parent, cSI_Type)
		{
			this.CSI_SubType = Argument.NotNullOrEmpty(cSI_SubType, nameof(cSI_SubType));
		}

		public CusSupportingInfoCollection(BusinessObject parent, ZString cSI_Type, ZGuid cSI_CSI_SupportingInfo)
		: this(parent, cSI_Type)
		{
			this.CSI_CSI_SupportingInfo = Argument.NotNull(cSI_CSI_SupportingInfo, nameof(cSI_CSI_SupportingInfo));
		}

		public readonly ZString CSI_Type;
		public readonly ZString CSI_SubType;
		public readonly ZGuid CSI_CSI_SupportingInfo;

		/// <summary>
		/// Copy this collection to the cloneResult collection
		/// </summary>
		public virtual void CloneElementsTo(ICusSupportingInfoCollection<T> cloneResult)
		{
			var args = new BusinessObjectCloneArgs(Enumerable.Empty<string>(), typeof(T));
			args.AddValueOverride(TypeOfElements, FKSchemaColumnInDependent.Name, cloneResult.Master.PK);
			foreach (T data in this)
			{
				cloneResult.Add(data.Clone());
			}
		}

		public void AddCloneFrom(IEnumerable<T> collectionToCloneFrom, BusinessObjectCloneArgs args)
		{
			args.AddValueOverride(TypeOfElements, FKSchemaColumnInDependent.Name, Master.PK);
			foreach (T dataToClone in collectionToCloneFrom.ToArray())
			{
				var clonedData = (T)dataToClone.Clone(args);
				using (clonedData.SuspendSettingHasChanges())
				{
					Add(clonedData);
				}
			}
		}

		public bool Any() => Count > 0;
		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();
		public T First() => this[0];
		public T FirstOrDefault() => this.Cast<T>().FirstOrDefault();
		public T Last() => this[Count - 1];

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusSupportingInfoSchema.CSI_ParentID; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusSupportingInfoSchema.CSI_Type, SQLComparisonOperator.Equal, CSI_Type);

			if (!CSI_SubType.IsEmpty)
			{
				result.AddToFilter(JoinCondition.And, CusSupportingInfoSchema.CSI_SubType, SQLComparisonOperator.Equal, CSI_SubType);
			}

			if (!CSI_CSI_SupportingInfo.IsEmpty)
			{
				result.AddToFilter(JoinCondition.And, CusSupportingInfoSchema.CSI_CSI_SupportingInfo, SQLComparisonOperator.Equal, CSI_CSI_SupportingInfo);
			}

			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (T)dependent;

			child.CSI_Type = CSI_Type;

			if (!CSI_SubType.IsEmpty)
			{
				child.CSI_SubType = CSI_SubType;
			}

			if (!CSI_CSI_SupportingInfo.IsEmpty)
			{
				child.CSI_CSI_SupportingInfo = CSI_CSI_SupportingInfo;
			}

			child.Parent = Master;
		}

		void ICusSupportingInfoCollection<T>.AddCloneFrom(IEnumerable<CusSupportingInfo> collectionToCloneFrom, BusinessObjectCloneArgs args) => AddCloneFrom(collectionToCloneFrom.Cast<T>(), args);
	}
}
