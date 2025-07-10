using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AllowSendingBookingConfirmationCollectionRegistryItem))]
	public class AllowSendingBookingConfirmationCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<AllowSendingBookingConfirmationCollection>
	{
		protected override StronglyTypedRegistryItem<AllowSendingBookingConfirmationCollection, AllowSendingBookingConfirmationCollection> GetNewRegistryItem()
		{
			return new AllowSendingBookingConfirmationCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, AllowSendingBookingConfirmationCollection.NewWithDefaultValues());
		}

		#region Implementation

		protected new AllowSendingBookingConfirmationCollectionRegistryItem Item
		{
			get { return (AllowSendingBookingConfirmationCollectionRegistryItem)base.Item; }
		}

		internal static ZGuid CreatePrincipal(ZString code)
		{
			BusinessObjectFactory dirtyFactory = new BusinessObjectFactory();
			BusinessObject principal = dirtyFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			principal[OrgHeaderSchema.Constants.OH_Code] = code;
			principal[OrgHeaderSchema.Constants.OH_IsShippingProvider] = true;

			BusinessObject companyData = (BusinessObject)principal["CompanyData"];
			companyData[OrgCompanyDataSchema.Constants.OB_CRIsShipsAgencyPrincipal] = true;
			dirtyFactory.Save();

			return principal.PK;
		}

		#endregion
	}
}
