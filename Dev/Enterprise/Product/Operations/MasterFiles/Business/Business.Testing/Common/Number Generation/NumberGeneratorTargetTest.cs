using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class NumberGeneratorTargetTest : TestCaseWithFactory
	{
		protected static void Set(BillCustomisationByServiceLevelRegistryItem item, string serviceLevel, string code)
		{
			Set(item, serviceLevel, code, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected static void Set(BillCustomisationByServiceLevelRegistryItem item, string serviceLevel, string code, GlbBranch branch)
		{
			Set(item, serviceLevel, code, branch.GB_GC, branch.PK, ZGuid.Empty);
		}

		protected static void Set(BillCustomisationByServiceLevelRegistryItem item, string serviceLevel, string code, ZGuid company, ZGuid branch, ZGuid department)
		{
			Set(item, serviceLevel, code,
				company.IsValid ? company.ToGuid() : Guid.Empty,
				branch.IsValid ? branch.ToGuid() : Guid.Empty,
				department.IsValid ? department.ToGuid() : Guid.Empty
				);
		}

		protected static void Set(BillCustomisationByServiceLevelRegistryItem item, string serviceLevel, string code, Guid company, Guid branch, Guid department)
		{
			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisation customisation = null;
			if (string.IsNullOrEmpty(serviceLevel))
			{
				customisation = customisations.BillOfLadingNumberCustomisations["ALL"];
			}
			else
			{
				customisation = customisations.BillOfLadingNumberCustomisations.AddNew();
				customisation.ServiceLevel = serviceLevel;
			}

			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1];
			element.Include = true;
			element.Order = 1;
			element.Detail = code;
			element.Fountain = false;

			if (branch == Guid.Empty)
			{
				item.SetValue(company, Guid.Empty, department, customisations);
			}
			else
			{
				item.SetValue(Guid.Empty, branch, department, customisations);
			}
		}

		protected static void AssertCustomisation(string message, string serviceLevel, string expectedCode, BillOfLadingNumberCustomisationsByServiceLevel customisations)
		{
			BillOfLadingNumberCustomisation customisation = customisations.BillOfLadingNumberCustomisations[serviceLevel];
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1];
			AssertEquals(message, expectedCode, element.Detail);
		}

		protected static void Set(BillCustomisationRegistryItem item, string code)
		{
			Set(item, code, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected static void Set(BillCustomisationRegistryItem item, string code, GlbBranch branch)
		{
			Set(item, code, branch.GB_GC, branch.PK, ZGuid.Empty);
		}

		protected static void Set(BillCustomisationRegistryItem item, string code, ZGuid company, ZGuid branch, ZGuid department)
		{
			Set(item, code,
				company.IsValid ? company.ToGuid() : Guid.Empty,
				branch.IsValid ? branch.ToGuid() : Guid.Empty,
				department.IsValid ? department.ToGuid() : Guid.Empty
				);
		}

		protected static void Set(BillCustomisationRegistryItem item, string code, Guid company, Guid branch, Guid department)
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();

			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1];
			element.Include = true;
			element.Order = 1;
			element.Detail = code;
			element.Fountain = false;

			if (branch == Guid.Empty)
			{
				item.SetValue(company, Guid.Empty, department, customisation);
			}
			else
			{
				item.SetValue(Guid.Empty, branch, department, customisation);
			}
		}

		protected static void AssertCustomisation(string message, string expectedCode, BillOfLadingNumberCustomisation customisation, string unfilteredElement = BillOfLadingNumberCustomisationElement.Keys.ClientCoded1)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[unfilteredElement];
			AssertEquals(message, expectedCode, element.Detail);
		}

		protected static void AssertLocation(IRegistryItemInternals registryItem, string location)
		{
			AssertEquals(registryItem.Location, location);
		}
	}
}
