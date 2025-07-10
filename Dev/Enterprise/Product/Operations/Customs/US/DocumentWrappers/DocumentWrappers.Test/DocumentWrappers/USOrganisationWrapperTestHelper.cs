using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	sealed class USOrganisationWrapperTestHelper
	{
		public USOrganisationWrapperTestHelper(BusinessObjectFactory testFactory)
		{
			factory = testFactory;
		}

		BusinessObjectFactory factory { get; }
		internal OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = factory.New<OrgHeader>();
					fSupplier.FillWithValidTestData();
					fSupplier.OH_FullName = "DUMMY SUPPLIER COMPANY";
					fSupplier.OH_IsConsignor = true;
				}
				return fSupplier;
			}
		}
		OrgHeader fSupplier;

		internal OrgAddress SupplierAddress
		{
			get
			{
				if (fSupplierAddress == null)
				{
					fSupplierAddress = Supplier.Addresses.AddNew();
					fSupplierAddress.FillWithValidTestData();
					fSupplierAddress.OA_Address1 = "DUMMY SUPPLIER ADDRESS 1";
					fSupplierAddress.OA_Address2 = "DUMMY SUPPLIER ADDRESS 2";
				}
				return fSupplierAddress;
			}
		}
		OrgAddress fSupplierAddress;

		internal OrgContact SupplierContact
		{
			get
			{
				if (fSupplierContact == null)
				{
					fSupplierContact = Supplier.Contacts.AddNew();
					fSupplierContact.FillWithValidTestData();
					fSupplierContact.OC_Phone = "+61 2 9332 2342";
				}
				return fSupplierContact;
			}
		}
		OrgContact fSupplierContact;

		internal USOrganisation USOrganisation
		{
			get
			{
				if (fUSOrganisation == null)
				{
					fUSOrganisation = CreateNewUSOrganisation();
					fUSOrganisation.ZO_OA_Address = SupplierAddress.PK;
					fUSOrganisation.ZO_Phone = SupplierContact.OC_Phone;
					fUSOrganisation.ZO_Contact = SupplierContact.OC_ContactName;
				}
				return fUSOrganisation;
			}
		}
		USOrganisation fUSOrganisation;

		internal USOrganisation CreateNewUSOrganisation()
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = Supplier.PK;
			return invoice.US_USPPI;
		}
	}
}
