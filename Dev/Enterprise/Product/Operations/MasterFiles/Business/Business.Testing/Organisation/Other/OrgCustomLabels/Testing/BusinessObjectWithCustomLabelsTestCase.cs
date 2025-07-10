using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class BusinessObjectWithCustomLabelsTestCase : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestCustomLabelsMandatoryValidation__()
		{
			BusinessObject bO = GetNewBusinessObject();
			bO.FillWithValidTestData();
			ICustomLabelsProvider customLabelsProvider = GetNewCustomLabelsProvider(bO);

			AssertNotNull("You must have a CustomLabelsProvider.ConfigOrgProvider for this test to work",
				customLabelsProvider.ConfigOrgProvider);
			AssertNotNull("You must have a CustomLabelsProvider.ConfigOrgProvider.ConfigOrg for this test to work",
				customLabelsProvider.ConfigOrgProvider.ConfigOrg);
			OrgHeader configOrg = customLabelsProvider.ConfigOrgProvider.ConfigOrg;

			CustomLabelInfoList customLabels = customLabelsProvider.GetCustomFields(configOrg, bO.Factory);
			foreach (CustomLabelInfoBase customLabel in customLabels)
			{
				if (IsCustomLabelType(customLabel))
				{
					CustomLabelInfo cusLabel = (CustomLabelInfo)customLabel;
					OrgCustomLabels newLabel = configOrg.CustomLabels.AddNew();
					newLabel.OT_FieldName = cusLabel.LabelName;

					newLabel.OT_IsMandatory = true;
					TestMandatoryValidation(bO, customLabel, true);
					newLabel.OT_IsMandatory = false;
					TestMandatoryValidation(bO, customLabel, false);

					newLabel.Delete();
				}
			}
		}

		protected void TestMandatoryValidation(BusinessObject bO, CustomLabelInfoBase customLabel,
																					 bool expectedMandatoryValidationEnabled)
		{
			if (customLabel.PropertyType == typeof(ZString) ||
					customLabel.PropertyType == typeof(ZDecimal) ||
					customLabel.PropertyType == typeof(ZDateTime))
			{
				if (IsCustomLabelType(customLabel))
				{
					AssertEquals(
						((CustomLabelInfo)customLabel).LabelName + ": Expected the custom label setting to be the same for the test",
						expectedMandatoryValidationEnabled, customLabel.IsMandatory);
				}
				if (bO[customLabel.PropertyName] is ZString)
				{
					bO[customLabel.PropertyName] = "x";
					bO[customLabel.PropertyName] = ZString.Empty;
				}
				else if (bO[customLabel.PropertyName] is ZDecimal)
				{
					bO[customLabel.PropertyName] = 1m;
					bO[customLabel.PropertyName] = 0m;
				}
				else if (bO[customLabel.PropertyName] is ZDateTime)
				{
					bO[customLabel.PropertyName] = ZDateTime.Now;
					bO[customLabel.PropertyName] = ZDateTime.Empty;
				}
				else
				{
					Fail("Don't know how to handle type '" + bO[customLabel.PropertyName].GetType().Name + "'");
				}

				bool actualMandatoryValidationEnabled = false;
				foreach (INotification error in bO.ZPropertyInfoHash[customLabel.PropertyName].GetErrors())
				{
					if (error.Message.ToLower().StartsWith("please enter"))
					{
						actualMandatoryValidationEnabled = true;
					}
				}
				if (IsCustomLabelType(customLabel))
				{
					AssertEquals(((CustomLabelInfo)customLabel).LabelName + ": Should have mandatory error",
						expectedMandatoryValidationEnabled, actualMandatoryValidationEnabled);
				}
				else
				{
					AssertEquals("Should be false on PartCustomLabel", false, customLabel.IsMandatory);
				}
			}
		}

		bool IsCustomLabelType(CustomLabelInfoBase label)
		{
			return label.GetType() == typeof(CustomLabelInfo);
		}

		protected abstract ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO);
	}
}
