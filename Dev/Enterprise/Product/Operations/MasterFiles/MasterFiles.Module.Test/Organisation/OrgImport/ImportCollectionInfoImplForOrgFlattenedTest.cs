using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class ImportCollectionInfoImplForOrgFlattenedTest : TestCaseWithFactory
	{
		public void TestMandatoryMappings()
		{
			var collection = new OrgFlattenedCollection(Factory);
			var impl = new ImportCollectionInfoImplForOrgFlattened(collection);
			AssertMappingIsMandatory(impl, OrgAddressSchema.Constants.OA_Address1, ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertMappingIsMandatory(impl, "Country", null);
		}

		void AssertMappingIsMandatory(ImportCollectionInfoImplForOrgFlattened impl, string fieldName, ImportCollectionInfoImplForOrgFlattened.ChildProperty? childProperty)
		{
			IImportPropertyInfo propertyInfo;
			if (childProperty.HasValue)
			{
				propertyInfo = impl.GetChildImportPropertyInfos(childProperty.Value).First(x => x.MappingName == fieldName);
			}
			else
			{
				propertyInfo = impl.HeaderProperties.First(x => x.MappingName == fieldName);
			}

			Assert($"Field {fieldName} should be mandatory", propertyInfo.IsMandatory);
		}

		public void TestAddPropertiesForImport()
		{
			var collection = new OrgFlattenedCollection(Factory);
			var impl = new ImportCollectionInfoImplForOrgFlattened(collection);
			var properties = impl.Cast<ImportPropertyInfoImpl<OrgFlattened>>().ToArray();

			AssertEquals(185, properties.Length);
			AssertProperty(impl, "Code", properties[0]);
			AssertProperty(impl, "Name", properties[1]);
			AssertProperty(impl, "Address 1", properties[2], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "Address 2", properties[3], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "Additional Address Info", properties[4], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "City", properties[5], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "State", properties[6], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "Postcode", properties[7], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "UNLOCO", properties[8]);
			AssertProperty(impl, "Country/Region", properties[9]);
			AssertProperty(impl, "Port City", properties[10]);
			AssertProperty(impl, "Phone", properties[11], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "Fax", properties[12], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "Email", properties[13], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "Web", properties[14]);
			AssertProperty(impl, "Business Registration Number", properties[15]);
			AssertProperty(impl, "Government Corporation Code", properties[16]);
			AssertProperty(impl, "Debtor", properties[17]);
			AssertProperty(impl, "Creditor", properties[18]);
			AssertProperty(impl, "Consignee", properties[19]);
			AssertProperty(impl, "Consignor", properties[20]);
			AssertProperty(impl, "Forwarder", properties[21]);
			AssertProperty(impl, "Broker", properties[22]);
			AssertProperty(impl, "Carrier", properties[23]);
			AssertProperty(impl, "Ship Line", properties[24]);
			AssertProperty(impl, "Airline", properties[25]);
			AssertProperty(impl, "Port Transport", properties[26]);
			AssertProperty(impl, "Sales Lead", properties[27]);
			AssertProperty(impl, "Services", properties[28]);
			AssertProperty(impl, "Competitor", properties[29]);
			AssertProperty(impl, "Contact Name", properties[30], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Contact Job Title", properties[31], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Contact Email", properties[32], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Contact Phone", properties[33], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Contact Mobile", properties[34], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Contact Fax", properties[35], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Debtor Code", properties[36]);
			AssertProperty(impl, "Debtor Group", properties[37]);
			AssertProperty(impl, "Debtor Settlement Group", properties[38]);
			AssertProperty(impl, "Currency", properties[39]);
			AssertProperty(impl, "Credit Limit", properties[40]);
			AssertProperty(impl, "Credit Rating", properties[41]);
			AssertProperty(impl, "GST", properties[42]);
			AssertProperty(impl, "Inv. Terms Standard", properties[43]);
			AssertProperty(impl, "Inv. Days Standard", properties[44]);
			AssertProperty(impl, "Inv. Terms Disbursement", properties[45]);
			AssertProperty(impl, "Inv. Days Disbursement", properties[46]);
			AssertProperty(impl, "Creditor Group", properties[47]);
			AssertProperty(impl, "Customs Agent", properties[48]);
			AssertProperty(impl, "Postal Address 1", properties[49], ImportCollectionInfoImplForOrgFlattened.ChildProperty.PostalAddress);
			AssertProperty(impl, "Postal Address 2", properties[50], ImportCollectionInfoImplForOrgFlattened.ChildProperty.PostalAddress);
			AssertProperty(impl, "Postal City", properties[51], ImportCollectionInfoImplForOrgFlattened.ChildProperty.PostalAddress);
			AssertProperty(impl, "Postal State", properties[52], ImportCollectionInfoImplForOrgFlattened.ChildProperty.PostalAddress);
			AssertProperty(impl, "Postal Postcode", properties[53], ImportCollectionInfoImplForOrgFlattened.ChildProperty.PostalAddress);
			AssertProperty(impl, "Delivery Address 1", properties[54], ImportCollectionInfoImplForOrgFlattened.ChildProperty.DeliveryAddress);
			AssertProperty(impl, "Delivery Address 2", properties[55], ImportCollectionInfoImplForOrgFlattened.ChildProperty.DeliveryAddress);
			AssertProperty(impl, "Delivery City", properties[56], ImportCollectionInfoImplForOrgFlattened.ChildProperty.DeliveryAddress);
			AssertProperty(impl, "Delivery State", properties[57], ImportCollectionInfoImplForOrgFlattened.ChildProperty.DeliveryAddress);
			AssertProperty(impl, "Delivery Postcode", properties[58], ImportCollectionInfoImplForOrgFlattened.ChildProperty.DeliveryAddress);
			AssertProperty(impl, "Bank", properties[59]);
			AssertProperty(impl, "Account Name", properties[60]);
			AssertProperty(impl, "Account Number", properties[61]);
			AssertProperty(impl, "BSB", properties[62]);
			AssertProperty(impl, "CCD", properties[63]);
			AssertProperty(impl, "CSC", properties[64]);
			AssertProperty(impl, "SCC", properties[65]);
			AssertProperty(impl, "CPP", properties[66]);
			AssertProperty(impl, "CCC", properties[67]);
			AssertProperty(impl, "CMP", properties[68]);
			AssertProperty(impl, "Work Notes", properties[69]);
			AssertProperty(impl, "Handling Notes", properties[70]);
			AssertProperty(impl, "Delivery Notes", properties[71]);
			AssertProperty(impl, "AR Notes", properties[72]);
			AssertProperty(impl, "AR Credit Notes", properties[73]);
			AssertProperty(impl, "AP Notes", properties[74]);
			AssertProperty(impl, "Contact Source Type", properties[75], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Contact Date Details Verified", properties[76], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Contact Salutation", properties[77], ImportCollectionInfoImplForOrgFlattened.ChildProperty.Contact);
			AssertProperty(impl, "Language", properties[78]);
			AssertProperty(impl, "Main Address Language", properties[79], ImportCollectionInfoImplForOrgFlattened.ChildProperty.MainAddress);
			AssertProperty(impl, "Postal Address Language", properties[80], ImportCollectionInfoImplForOrgFlattened.ChildProperty.PostalAddress);
			AssertProperty(impl, "Delivery Address Language", properties[81], ImportCollectionInfoImplForOrgFlattened.ChildProperty.DeliveryAddress);
			AssertProperty(impl, "Bank Currency", properties[82]);
			AssertProperty(impl, "Warehouse", properties[83]);
			AssertProperty(impl, "Controlling Agent", properties[84]);
			AssertProperty(impl, "Controlling Customer", properties[85]);
		}

		void AssertProperty(ImportCollectionInfoImplForOrgFlattened impl, string headerText, ImportPropertyInfoImpl<OrgFlattened> property)
		{
			AssertEquals(headerText, property.HeaderText);
			Assert(impl.HeaderProperties.Contains(property));
		}

		void AssertProperty(ImportCollectionInfoImplForOrgFlattened impl, string headerText, ImportPropertyInfoImpl<OrgFlattened> property, ImportCollectionInfoImplForOrgFlattened.ChildProperty childSchema)
		{
			AssertEquals(headerText, property.HeaderText);
			Assert(impl.GetChildImportPropertyInfos(childSchema).Contains(property));
		}
	}
}
