using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MultipleItemExtensions;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public class MultipleItemManagerGUID<T> : NonPersistentBusinessObject, IObsoleteValidation
		where T : BusinessObject
	{
		public MultipleItemManagerGUID(IBusinessObjectCollection collection, SchemaColumn column, BusinessObjectFactory factory) : base(factory)
			=> (Collection, Column) = (collection, column);

		public IBusinessObjectCollection Collection { get; }

		public T RelatedBusinessObject => Factory.Load<T>(Value);

		[RelatedBusinessObject(nameof(RelatedBusinessObject))]
		public ZGuid Value
		{
			get
			{
				switch (Collection.Count)
				{
					case 0:
						return (ZGuid)ZDataType.ZTypeToEmptyValue(typeof(ZGuid));

					case 1:
						var businessObject = (BusinessObject)Collection[0];
						return (ZGuid)businessObject[Column];

					default:
						return UNDGConstants.UNDGDataItem.GridGuidForMany;
				}
			}
			set
			{
				switch (Collection.Count)
				{
					case 0:
						AddNewItemAndSetValue(value);
						break;

					case 1:
						SetItemToExistingValueAndDeleteIfEmpty(value);
						break;

					default:
						throw new InvalidOperationException("Should not be attempting to set the value when the collection contains > 1 item.");
				}

				ValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));

		public ZString FieldColumnType
		{
			get
			{
				FieldType result;
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

		#region Implementation

		SchemaColumn Column { get; }

		void AddNewItemAndSetValue(ZGuid value)
		{
			var item = Collection.AddNew();
			item[Column] = value;
		}

		void SetItemToExistingValueAndDeleteIfEmpty(ZGuid value)
		{
			var item = (BusinessObject)Collection[0];
			item[Column] = value;

			if (item.ItemIsEmpty())
			{
				item.Delete();
			}
		}

		#endregion
	}
}
