using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	public abstract class PhaseDependantsProviderTestCase : TransactionedTestCase
	{
		public void TestAllSpecifiedPropertiesExist()
		{
			PropertyInfo[] allPublicProperties = GetParentType().GetProperties();
			PhaseDependantsProvider provider = GetDependantsProvider();

			foreach (IPhaseDependant dependant in provider.GetIZTypeProperties())
			{
				AssertEquals(dependant.Name + " property exist on parent type", true, allPublicProperties.Any(x => x.Name == dependant.Name));
			}

			foreach (IPhaseDependant dependant in provider.GetChildDependants())
			{
				if (!ChildDependantsThatHaveDifferentExposedNames.Contains(dependant.Name))
				{
					AssertEquals(dependant.Name + " property exist on parent type", true, allPublicProperties.Any(x => x.Name == dependant.Name));
				}
			}

			foreach (var childExpandableDependant in provider.GetChildExpandableDependants())
			{
				PropertyInfo childPropertyInfo = allPublicProperties.FirstOrDefault(x => x.Name == childExpandableDependant.Key.Name);
				AssertNotNull(childExpandableDependant.Key.Name + " property exist on parent type", childPropertyInfo);

				PropertyInfo[] allChildPublicProperties = childPropertyInfo.PropertyType.GetProperties();
				foreach (IPhaseDependant dependant in childExpandableDependant.Value)
				{
					ZString childNamePrefix = childExpandableDependant.Key.Name + ".";
					AssertEquals(dependant.Name + " property is prefixed with child name", true, dependant.Name.StartsWith(childNamePrefix));

					ZString propertyNameWithoutPrefix = dependant.Name.Substring(childNamePrefix.Length);
					AssertEquals(dependant.Name + " property exist on child type", true, allChildPublicProperties.Any(x => x.Name == propertyNameWithoutPrefix));
				}
			}
		}

		protected virtual ZString[] ChildDependantsThatHaveDifferentExposedNames
		{
			get { return Array.Empty<ZString>(); }
		}

		public void TestGetIZTypeProperties_ReturnOnlyWriteableIZTypes()
		{
			PropertyInfo[] allPublicProperties = GetParentType().GetProperties();
			PhaseDependantsProvider provider = GetDependantsProvider();

			foreach (IPhaseDependant dependant in provider.GetIZTypeProperties())
			{
				PropertyInfo propertyInfo = allPublicProperties.FirstOrDefault(property => property.Name == dependant.Name);

				string message = string.Format("Expecting only IZType, but {0} is {1}", propertyInfo.Name, propertyInfo.PropertyType);
				AssertEquals(message, true, typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType));

				message = string.Format("Expecting only writeable properties, but {0} is readonly", propertyInfo.Name);
				AssertEquals(message, true, propertyInfo.CanWrite);
			}
		}

		public void TestCustomWorkflowFields()
		{
			var factory = new BusinessObjectFactory();

			var parent = factory.New(GetParentType());
			var workflowProvider = parent as IWorkflowProvider;
			var provider = GetDependantsProvider();

			if (workflowProvider != null)
			{
				var existingTemplatesQuery = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, workflowProvider.WorkflowType);
				var existingTemplates = factory.Load<ProcessTaskTemplate>(existingTemplatesQuery);

				foreach (var template in existingTemplates)
				{
					template.P0_IsSystem = false;
					template.Delete();
				}

				var template1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
				template1.P0_ProcessType = workflowProvider.WorkflowType;

				var def11 = template1.GenCustomColumnDefinitions.AddNew();
				def11.XC_Name = "custom field 1";
				def11.XC_Type = AddOnColumnDataType.Codes.String;

				var def12 = template1.GenCustomColumnDefinitions.AddNew();
				def12.XC_Name = "custom field 2";
				def12.XC_Type = AddOnColumnDataType.Codes.Integer;

				var template2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
				template2.P0_ProcessType = workflowProvider.WorkflowType;

				var def21 = template2.GenCustomColumnDefinitions.AddNew();
				def21.XC_Name = "custom field 3";
				def21.XC_Type = AddOnColumnDataType.Codes.Datetime;

				var def22 = template2.GenCustomColumnDefinitions.AddNew();
				def22.XC_Name = "custom field 4";
				def22.XC_Type = AddOnColumnDataType.Codes.Boolean;

				var defDuplicate = template2.GenCustomColumnDefinitions.AddNew();
				defDuplicate.XC_Name = "custom field 2";
				defDuplicate.XC_Type = AddOnColumnDataType.Codes.String;

				var template3 = factory.NewWithValidTestData<ProcessTaskTemplate>();
				template3.P0_ProcessType = workflowProvider.WorkflowType;

				var def31 = template3.GenCustomColumnDefinitions.AddNew();
				def31.XC_Name = "custom field 5";
				def31.XC_Type = AddOnColumnDataType.Codes.Short;

				var def32 = template3.GenCustomColumnDefinitions.AddNew();
				def32.XC_Name = "custom field 6";
				def32.XC_Type = AddOnColumnDataType.Codes.Decimal;

				var def33 = template3.GenCustomColumnDefinitions.AddNew();
				def33.XC_Name = "custom field 7";
				def33.XC_Type = AddOnColumnDataType.Codes.Guid;

				var def34 = template3.GenCustomColumnDefinitions.AddNew();
				def34.XC_Name = "custom field 8";
				def34.XC_Type = AddOnColumnDataType.Codes.Byte;

				factory.Save();

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"__CUSTOM FIELD 1__prop__ZString|custom field 1 (STR)",
					"__CUSTOM FIELD 2__prop__ZInt|custom field 2 (INT)",
					"__CUSTOM FIELD 3__prop__ZDateTime|custom field 3 (DAT)",
					"__CUSTOM FIELD 4__prop__ZBool|custom field 4 (BOO)",
					"__CUSTOM FIELD 5__prop__ZShort|custom field 5 (SHO)",
					"__CUSTOM FIELD 6__prop__ZDecimal|custom field 6 (DEC)",
					"__CUSTOM FIELD 7__prop__ZGuid|custom field 7 (GUI)",
					"__CUSTOM FIELD 8__prop__ZByte|custom field 8 (BYT)",
					"__CUSTOM FIELD 2__prop__ZString|custom field 2 (STR)"
				},
				provider.GetWorkflowFields().Select(dep => string.Format("{0}|{1}", dep.Name, dep.Description)));
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IPhaseDependant>(), provider.GetWorkflowFields());
			}
		}

		protected abstract PhaseDependantsProvider GetDependantsProvider();
		protected abstract Type GetParentType();
	}
}
