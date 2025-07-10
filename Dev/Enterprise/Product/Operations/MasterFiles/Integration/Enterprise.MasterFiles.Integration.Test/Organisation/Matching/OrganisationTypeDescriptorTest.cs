using System;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Integration.Testing
{
	public class OrganisationTypeDescriptorTest : TestCase
	{
		public void TestEnumMatchTranslation()
		{
			foreach (OrganisationTypes currentType in Enum.GetValues(typeof(OrganisationTypes)))
			{
				if (currentType != OrganisationTypes.None)
				{
					string typeName = currentType.ToString();
					Assert(typeName + " translation is missing", OrganisationTypeDescriptor.Translations.ContainsKey(currentType));
				}
			}
		}

		public void TestGetSigleType()
		{
			AssertEquals(OrganisationTypeDescriptor.GetString(OrganisationTypes.None), "");
			AssertEquals(OrganisationTypeDescriptor.GetString(OrganisationTypes.Broker), OrganisationTypeDescriptor.Descriptions.Broker);
			AssertEquals(OrganisationTypeDescriptor.GetString(OrganisationTypes.ControllingCustomer), OrganisationTypeDescriptor.Descriptions.ControllingCustomer);
			AssertEquals(OrganisationTypeDescriptor.GetString(OrganisationTypes.Debtor), OrganisationTypeDescriptor.Descriptions.Debtor);
			AssertEquals(OrganisationTypeDescriptor.GetString(OrganisationTypes.DistributionCentre), OrganisationTypeDescriptor.Descriptions.DistributionCentre);
			AssertEquals(OrganisationTypeDescriptor.GetString(OrganisationTypes.Sales), OrganisationTypeDescriptor.Descriptions.Sales);
		}

		public void TestGetMultipleTypes()
		{
			var types = OrganisationTypes.Broker | OrganisationTypes.ControllingAgent | OrganisationTypes.Carrier;
			var typesDesc = OrganisationTypeDescriptor.GetString(types);
			AssertEquals(typesDesc, "Carrier, Broker, Controlling Agent");

			types = OrganisationTypes.DistributionCentre | OrganisationTypes.ControllingCustomer | OrganisationTypes.Consignee;
			typesDesc = OrganisationTypeDescriptor.GetString(types);
			AssertEquals(typesDesc, "Consignee, Distribution Center, Controlling Customer");
		}
	}
}
