using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MultipleItemExtensions;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGMultipleItemManager<T> : NonPersistentBusinessObject, IObsoleteValidation
		where T : UNDGDataItem
	{
		public UNDGMultipleItemManager(UNDGDataItemCollection<T> collection, BusinessObjectFactory factory)
			: base(factory)
		{
			Collection = collection;
		}

		public UNDGDataItemCollection<T> Collection { get; }

		#region Value

		public UNDGSubstance UNDGSubstance => Factory.Load<UNDGSubstance>(Value);

		[RelatedBusinessObject(nameof(UNDGSubstance))]
		public ZGuid Value
		{
			get
			{
				if (Collection.Count == 0)
				{
					return (ZGuid)ZDataType.ZTypeToEmptyValue(typeof(ZGuid));
				}
				else if (Collection.Count == 1)
				{
					return Collection[0].SubstancePK;
				}
				else
				{
					return UNDGConstants.UNDGDataItem.GridGuidForMany;
				}
			}
			set
			{
				if (Collection.Count == 1)
				{
					var item = Collection[0];
					item.SubstancePK = value;
					if (item.ItemIsEmpty())
					{
						item.Delete();
					}
				}
				else if (Collection.Count == 0)
				{
					var item = Collection.AddNew();
					item.SubstancePK = value;
				}
				else
				{
					throw new InvalidOperationException("Should not be attempting to set the value when the collection contains > 1 item.");
				}

				ValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		public bool Value_ReadOnly
		{
			get { return (ReadOnlyWhenMany && Collection.Count > 1); }
		}

		public bool ReadOnlyWhenMany { get; set; }

		#endregion

		#region Field Type

		public ZString FieldColumnType
		{
			get
			{
				FieldType result = FieldType.Text;
				if (Collection.Count > 1)
				{
					result = FieldType.LinkLabel;
				}
				else
				{
					result = FieldType.Guid;
				}

				return result.ToString();
			}
		}

		#endregion
	}
}
