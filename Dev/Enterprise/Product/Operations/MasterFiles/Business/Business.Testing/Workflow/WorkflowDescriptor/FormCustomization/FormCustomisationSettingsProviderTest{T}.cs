using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(FormCustomisationSettingsProvider), ExcludePrivate = true)]
	public abstract class FormCustomisationSettingsProviderTest<T> : TestCaseWithFactory where T : FormCustomisationSettingsProvider
	{
		public abstract T GetNewProvider();
		public abstract void TestDisplayTabs();
		public abstract void TestPropertiesThatAffectWorkflow();
		public abstract void TestTabPlacementProhibitions();

		[ExpectNoExceptions]
		public void TestTabElementsElementGroupsHaveUniqueNames()
		{
			List<string> listElementNames = new List<string>();

			T settingsProvider = GetNewProvider();

			foreach (var element in settingsProvider.DisplayTabs.Cast<FormCustomisableElement>())
			{
				if (listElementNames.Contains(element.ElementName))
				{
					throw new Exception(String.Format("{0} element names must be unique - there's already an element {1}", "[tabs]", element.ElementName));
				}

				listElementNames.Add(element.ElementName);
			}

			foreach (var element in settingsProvider.DisplayFields.Cast<FormCustomisableElement>())
			{
				if (listElementNames.Contains(element.ElementName))
				{
					throw new Exception(String.Format("{0} element names must be unique - there's already an element {1}", "[elements]", element.ElementName));
				}

				listElementNames.Add(element.ElementName);
			}

			var groupNames = from elem in settingsProvider.DisplayFields.Cast<FormCustomisableElement>()
							 group elem by elem.ElementGroup into g
							 select g.Key;

			foreach (var groupName in groupNames)
			{
				if (listElementNames.Contains(groupName))
				{
					throw new Exception(String.Format("{0} element names must be unique - there's already an element {1}", "[element groups]", groupName));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestPropertiesThatAffectWorkflowTemplate()
		{
			ProcessTaskTemplate template = Factory.New<ProcessTaskTemplate>();

			T settingsProvider = GetNewProvider();

			foreach (string propertyName in settingsProvider.PropertiesThatAffectWorkflowTemplate)
			{
				if (template.ZPropertyInfoHash[propertyName] == null)
				{
					Fail(String.Format("invalid property name {0}", propertyName));
				}
			}
		}
	}
}
