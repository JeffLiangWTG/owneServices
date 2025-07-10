using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface ICustomFieldsDescriptor
	{
		Type ParentType { get; }
		IEnumerable<CustomFieldInfo> ActiveCustomFieldsInfos { get; }
		IEnumerable<CustomFieldInfo> CustomFieldsInfos { get; }
	}

	public abstract class CustomFieldsDescriptor<T> : ICustomFieldsDescriptor
		where T : BusinessObject
	{
		public Type ParentType
		{
			get { return typeof(T); }
		}

		public ZString BindTo(CustomFieldInfo customFieldInfo)
		{
			return BindTo(string.Empty, customFieldInfo);
		}

		public ZString BindTo(string bindingPrefix, CustomFieldInfo customFieldInfo)
		{
			Argument.NotNull(customFieldInfo, "customFieldInfo");

			return !string.IsNullOrWhiteSpace(bindingPrefix)
				? string.Format("{0}+{1}", bindingPrefix, customFieldInfo.SchemaColumnName)
				: customFieldInfo.SchemaColumnName;
		}

		public IEnumerable<CustomFieldInfo> ActiveCustomFieldsInfos
		{
			get { return CustomFieldsInfos.Where(info => info.IsActive); }
		}

		public IEnumerable<CustomFieldInfo> CustomFieldsInfos
		{
			get { return GetCustomFieldsInfos(); }
		}

		protected abstract IEnumerable<CustomFieldInfo> GetCustomFieldsInfos();
	}
}
