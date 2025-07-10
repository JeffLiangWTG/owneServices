using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusPackableItemTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForLoadWhenCountryIsEmpty()
		{
			var packableItem = (CusPackableItem)GetNewBusinessObjectForLoadTest();
			packableItem.PackingList.Declaration.Branch.Company.GC_RN_NKCountryCode = string.Empty;
			Factory.Save();
			var newBusinessObjectFactory = new BusinessObjectFactory();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var loadedPackableItem = newBusinessObjectFactory.Load(BaseTypeDecidedType, packableItem.PK);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.TW.ICusPackableItem>(), loadedPackableItem.GetType());

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			loadedPackableItem = newBusinessObjectFactory.Load(BaseTypeDecidedType, packableItem.PK);
			AssertType<CusPackableItem>(loadedPackableItem);
		}

		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			if (bizO is CusPackableItem packableItem)
			{
				packableItem.PackingList.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackableItem = packingList.PackableItems.AddNew();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_ClusterKey = 1;
			return cusPackableItem;
		}

		protected override Type BaseTypeDecidedType => typeof(CusPackableItem);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusPackableItem>() },
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusPackableItem>() }
			};
		}

		#endregion
	}
}
