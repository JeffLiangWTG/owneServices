using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class CustomLabelsTestCase
	{
		public CustomLabelsTestCase(BusinessObject bO, ICustomLabelsProvider customLabelsProvider)
		{
			this.BO = bO;
			CustomLabelsProvider = customLabelsProvider;
		}

		public void TestCustomLabelsMandatoryValidation(bool isValidationEnabled = true)
		{
			Assertion.AssertNotNull("You must have a CustomLabelsProvider.ConfigOrgProvider for this test to work", CustomLabelsProvider.ConfigOrgProvider);
			Assertion.AssertNotNull("You must have a CustomLabelsProvider.ConfigOrgProvider.ConfigOrg for this test to work", CustomLabelsProvider.ConfigOrgProvider.ConfigOrg);

			OrgHeader configOrg = CustomLabelsProvider.ConfigOrgProvider.ConfigOrg;
			CustomLabelInfoList customLabels = CustomLabelsProvider.GetCustomFields(configOrg, CustomLabelsProvider.ConfigOrgProvider.Factory);

			foreach (CustomLabelInfoBase customLabel in customLabels)
			{
				if (IsCustomLabelType(customLabel))
				{
					CustomLabelInfo cusLabel = (CustomLabelInfo)customLabel;
					OrgCustomLabels newLabel = configOrg.CustomLabels.AddNew();
					newLabel.OT_FieldName = cusLabel.LabelName;

					newLabel.OT_IsMandatory = true;
					TestMandatoryValidation(customLabel, true, isValidationEnabled);
					newLabel.OT_IsMandatory = false;
					TestMandatoryValidation(customLabel, false, false);

					newLabel.Delete();
				}
			}
		}

		protected void TestMandatoryValidation(CustomLabelInfoBase customLabel, bool expectedMandatoryValidationEnabled, bool shouldDoMandatoryValidtion)
		{
			if (customLabel.PropertyType == typeof(ZString) ||
				customLabel.PropertyType == typeof(ZDecimal) ||
				customLabel.PropertyType == typeof(ZDateTime))
			{
				if (IsCustomLabelType(customLabel))
				{
					Assertion.AssertEquals(((CustomLabelInfo)customLabel).LabelName + ": Expected the custom label setting to be the same for the test", expectedMandatoryValidationEnabled, customLabel.IsMandatory);
				}
				if (BO[customLabel.PropertyName] is ZString)
				{
					BO[customLabel.PropertyName] = "x";
					BO[customLabel.PropertyName] = ZString.Empty;
				}
				else if (BO[customLabel.PropertyName] is ZDecimal)
				{
					BO[customLabel.PropertyName] = 1m;
					BO[customLabel.PropertyName] = 0m;
				}
				else if (BO[customLabel.PropertyName] is ZDateTime)
				{
					BO[customLabel.PropertyName] = ZDateTime.Now;
					BO[customLabel.PropertyName] = ZDateTime.Empty;
				}
				else
				{
					Assertion.Fail("Don't know how to handle type '" + BO[customLabel.PropertyName].GetType().Name + "'");
				}

				bool actualMandatoryValidationEnabled = false;
				foreach (INotification error in BO.ZPropertyInfoHash[customLabel.PropertyName].GetErrors())
				{
					if (error.Message.ToLower().StartsWith("please enter"))
					{
						actualMandatoryValidationEnabled = true;
					}
				}
				if (IsCustomLabelType(customLabel))
				{
					Assertion.AssertEquals(((CustomLabelInfo)customLabel).LabelName + " [" + ((CustomLabelInfo)customLabel).PropertyName + "]: Should have mandatory error", shouldDoMandatoryValidtion, actualMandatoryValidationEnabled);
				}
				else
				{
					Assertion.AssertEquals("Should be false on PartCustomLabel", false, customLabel.IsMandatory);
				}
			}
		}

		bool IsCustomLabelType(CustomLabelInfoBase label)
		{
			return label.GetType() == typeof(CustomLabelInfo);
		}

		readonly BusinessObject BO;
		readonly ICustomLabelsProvider CustomLabelsProvider;
	}
}
