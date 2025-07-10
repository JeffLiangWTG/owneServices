using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CustomLabelInfoList : CollectionBase
	{
		/// <summary>
		/// A human-readable string explaining where the custom labels configuration organisation can be found. Used
		/// in the customize columns window on the grid.
		/// </summary>
		public readonly MultilingualString ConfigOrgLocatedAt;

		public CustomLabelInfoList(Type bizType, OrgHeader org, MultilingualString configOrgLocatedAt, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			this.bizType = bizType;
			this.org = org;
			this.Factory = factory;
			ConfigOrgLocatedAt = configOrgLocatedAt;
		}

		public CustomLabelInfoBase this[int index]
		{
			get { return (CustomLabelInfoBase)List[index]; }
		}

		#region Implementation

		readonly Type bizType;
		readonly OrgHeader org;
		readonly BusinessObjectFactory Factory;

		Type GetpropertyTypeFromName(string propertyName)
		{
			Argument.NotNullOrEmpty(propertyName, "propertyName");
			PropertyDescriptor property = TypeDescriptor.GetProperties(bizType)[propertyName]
				?? throw new ArgumentException(string.Format("Property {0} could not be found on {1}", propertyName, bizType.FullName));
			return property.PropertyType;
		}

		#endregion

		public void Add(CustomLabelInfoBase field)
		{
			List.Add(field);
		}

		public void Add(string labelName, string propertyName, MultilingualString defaultCaption)
		{
			Add(labelName, propertyName, defaultCaption, null);
		}

		public void Add(string labelName, string propertyName, MultilingualString defaultCaption, CustomLabelStyles styles)
		{
			Type propertyType = GetpropertyTypeFromName(propertyName);
			Add(labelName, propertyName, propertyType, defaultCaption, styles);
		}

		public void Add(string labelName, string propertyName, MultilingualString defaultCaption, MultilingualString defaultHint)
		{
			Type propertyType = GetpropertyTypeFromName(propertyName);
			Add(labelName, propertyName, propertyType, defaultCaption, defaultHint);
		}

		public void Add(string labelName, string propertyName, MultilingualString defaultCaption, MultilingualString defaultHint, CustomLabelStyles styles)
		{
			Type propertyType = GetpropertyTypeFromName(propertyName);
			Add(labelName, propertyName, propertyType, defaultCaption, defaultHint, styles);
		}

		public void Add(string labelName, string propertyName, Type propertyType, MultilingualString defaultCaption, CustomLabelStyles styles)
		{
			Add(labelName, propertyName, propertyType, defaultCaption, null, styles);
		}

		public void Add(string labelName, string propertyName, Type propertyType, MultilingualString defaultCaption, MultilingualString defaultHint)
		{
			Add(labelName, propertyName, propertyType, defaultCaption, defaultHint, CustomLabelStyles.None);
		}

		public void Add(string labelName, string propertyName, Type propertyType, MultilingualString defaultCaption, MultilingualString defaultHint, CustomLabelStyles styles)
		{
			List.Add(new CustomLabelInfo(labelName, propertyName, propertyType, defaultCaption, defaultHint, styles, org, Factory));
		}

		public void Remove(string propertyName)
		{
			foreach (CustomLabelInfoBase info in List)
			{
				if (info.PropertyName == propertyName)
				{
					List.Remove(info);
					break;
				}
			}
		}

		public bool Contains(string propertyName)
		{
			foreach (CustomLabelInfoBase field in this)
			{
				if (propertyName == field.PropertyName)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasEnabledFields()
		{
			foreach (CustomLabelInfoBase field in this)
			{
				if (field.IsEnabled)
				{
					return true;
				}
			}
			return false;
		}

		public CustomLabelInfoBase GetFieldByPropertyName(string propertyName)
		{
			foreach (CustomLabelInfoBase field in this)
			{
				if (field.PropertyName == propertyName)
				{
					return field;
				}
			}
			return null;
		}

		#region Sort

		public void SortByPosition()
		{
			InnerList.Sort(new CustomLabelInfoPositionComparer());
		}

		class CustomLabelInfoPositionComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return ((CustomLabelInfoBase)x).Position - ((CustomLabelInfoBase)y).Position;
			}
		}

		#endregion
	}
}
