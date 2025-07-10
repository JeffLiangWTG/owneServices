using System;
using System.Drawing;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingImageCollectionRegistryItem))]
	public class BillOfLadingImageCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<BillOfLadingImageCollection>
	{
		#region FindBillOfLadingImageForPrincipal

		public void TestFindBillOfLadingImageForPrincipal()
		{
			var principalPK1 = CreatePrincipal("Principal1");
			var principalPK2 = CreatePrincipal("Principal2");

			var collection = new BillOfLadingImageCollection();

			var bi1 = collection.AddNew();
			bi1.PrincipalPK = principalPK1;
			bi1.Description = "Test";
			bi1.Enabled = true;
			bi1.Image = new Bitmap(1, 1);

			var bi2 = collection.AddNew();
			bi2.PrincipalPK = principalPK2;
			bi2.Description = "Test2";
			bi2.Enabled = true;
			bi2.Image = new Bitmap(1, 1);

			Item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			AssertEquals("Find Bill Of Loading Image 1", bi1.PrincipalPK, Item.FindBillOfLadingImageForPrincipal(principalPK1).PrincipalPK);
			AssertEquals("Find Bill Of Loading Image 2", bi2.PrincipalPK, Item.FindBillOfLadingImageForPrincipal(principalPK2).PrincipalPK);
			AssertNull(Item.FindBillOfLadingImageForPrincipal(ZGuid.Empty));
		}

		#endregion

		protected override StronglyTypedRegistryItem<BillOfLadingImageCollection, BillOfLadingImageCollection> GetNewRegistryItem()
		{
			return new BillOfLadingImageCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		#region Implementation

		protected new BillOfLadingImageCollectionRegistryItem Item
		{
			get { return (BillOfLadingImageCollectionRegistryItem)base.Item; }
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
