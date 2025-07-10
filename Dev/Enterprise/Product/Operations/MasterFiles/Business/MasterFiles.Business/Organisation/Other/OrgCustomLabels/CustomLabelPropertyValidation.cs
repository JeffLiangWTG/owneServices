using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class CustomLabelPropertyValidation
	{
		public CustomLabelPropertyValidation(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		readonly BusinessObjectFactory Factory;

		public void Validate(ICustomLabelsProvider customLabelsProvider, ZPropertyInfo propertyInfo)
		{
			if (customLabelsProvider == null)
			{
				throw new ArgumentNullException(nameof(customLabelsProvider), "CustomLabelsProvider");
			}

			if (propertyInfo == null)
			{
				throw new ArgumentNullException(nameof(propertyInfo), "PropertyInfo");
			}

			ICustomLabelsConfigOrgProvider configOrgProvider = customLabelsProvider.ConfigOrgProvider;
			if (configOrgProvider != null && configOrgProvider.ConfigOrg != null)
			{
				CustomLabelInfoList customLabels = customLabelsProvider.GetCustomFields(configOrgProvider.ConfigOrg, Factory);
				CustomLabelInfoBase labelInfo = customLabels.GetFieldByPropertyName(propertyInfo.Name);
				if (labelInfo.IsMandatory)
				{
					MandatoryValidation.CheckEntered(propertyInfo);
				}
			}
		}

		#region Implementation

		protected MandatoryValidation MandatoryValidation = new MandatoryValidation();

		#endregion
	}
}
