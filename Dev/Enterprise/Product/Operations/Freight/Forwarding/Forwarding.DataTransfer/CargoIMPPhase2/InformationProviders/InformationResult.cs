using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	struct InformationResult<T> where T : struct
	{
		public static InformationResult<T> Empty => new InformationResult<T>();

		public InformationResult(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo != null)
			{
				Value = (T)propertyInfo.Value;
				Name = propertyInfo.HumanReadableName;
				BusinessEntity = propertyInfo.BizObj;
			}
			else
			{
				Value = default(T);
				BusinessEntity = null;
			}
		}

		public InformationResult(ZString name)
		{
			Value = default(T);
			Name = name;
			BusinessEntity = null;
		}

		public InformationResult(T value, ZString name)
		{
			Value = value;
			Name = name;
			BusinessEntity = null;
		}

		public InformationResult(T value, ZPropertyInfo propertyInfo)
		{
			Value = value;
			if (propertyInfo != null)
			{
				Name = propertyInfo.HumanReadableName;
				BusinessEntity = propertyInfo.BizObj;
			}
			else
			{
				BusinessEntity = null;
			}
		}

		public InformationResult(T value, IBusiness businessEntity, ZString name)
		{
			Value = value;
			Name = name;
			BusinessEntity = businessEntity;
		}

		public InformationResult(T value, IBusiness businessEntity, ZPropertyInfo propertyInfo)
		{
			Value = value;
			BusinessEntity = businessEntity;
			if (propertyInfo != null)
			{
				Name = propertyInfo.HumanReadableName;
			}
		}

		public T Value;
		public IBusiness BusinessEntity;
		public ZString Name;
	}
}
