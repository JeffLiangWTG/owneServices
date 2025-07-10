using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CanadaForwardingShipmentSupportTest : TestCaseWithFactory
	{
		public static void DeleteAnyCarrierCode(BusinessObjectFactory factory, string countryCode = "")
		{
			var currentCompany = GlbCompany.GetCurrentCompany(factory);
			var orgCusCode = currentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, string.IsNullOrEmpty(countryCode) ? Constants.CountryCodes.Canada : countryCode);
			if (orgCusCode != null)
			{
				currentCompany.OrgProxy.CustomsCodes.RemoveAndDelete(orgCusCode);
				factory.Save();
			}
		}

		public void TestCanadaCargoControlNumber()
		{
			CreateNewShipmentWithValidData();
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			AssertEquals("Empty number for new shipment", "", GetCargoControlNumber(shipment));

			CreateNewShipmentWithValidData();
			DeleteAnyCarrierCode(Factory);
			DeleteAnyCarrierCode(Factory, Core.Constants.CountryCodes.Australia);
			shipment.HasChanges = true;
			Factory.Save();
			AssertEquals("Empty number for new shipment, if no carrier code", "", GetCargoControlNumber(shipment));

			CreateNewShipmentWithValidData();
			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			shipment.HasChanges = true;
			Factory.Save();
			AssertEquals("Empty number for new shipment, if no carrier code for Canada", "", GetCargoControlNumber(shipment));

			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;

			FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain);
			CreateNewShipmentWithValidData();
			shipment.HasChanges = true;
			Factory.Save();
			ZString createdCustomNumber = GetCargoControlNumber(shipment);
			AssertEquals("New cargo control number", "123400000001", createdCustomNumber);

			CFSShipment loadedShipment = NewFactory().Load<CFSShipment>(shipment.PK);
			AssertEquals("Loaded custom number equals created number", createdCustomNumber, GetCargoControlNumber(loadedShipment));

			shipment.HasChanges = true;
			Factory.Save();
			AssertEquals("No overwriting of current CCN", createdCustomNumber, GetCargoControlNumber(shipment));

			shipment.Numbers.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("New cargo control number", "123400000002", GetCargoControlNumber(shipment));

			FreightDataRegistry.Instance.CanadaCargoControlNumberBranchPrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 32);
			shipment.Numbers.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("New cargo control number with branch prefix", "123432000003", GetCargoControlNumber(shipment));

			FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.HouseBill);
			CreateNewShipmentWithValidData();
			shipment.HasChanges = true;

			Factory.Save();
			createdCustomNumber = GetCargoControlNumber(shipment);
			AssertEquals("New cargo control number", "123448163264", createdCustomNumber);

			FreightDataRegistry.Instance.CanadaCargoControlNumberHBLDigits.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			shipment.Numbers.RemoveAndDeleteAll();
			Factory.Save();
			createdCustomNumber = GetCargoControlNumber(shipment);
			AssertEquals("New cargo control number", "12341248163264", createdCustomNumber);

			shipment.Numbers.RemoveAndDeleteAll();
			shipment.JS_RL_NKOrigin = "CATOR";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.HasChanges = true;
			Factory.Save();
			AssertEquals("Blank for exports", "", GetCargoControlNumber(shipment));
			DeleteAnyCarrierCode(Factory);
			DeleteAnyCarrierCode(Factory, Core.Constants.CountryCodes.Australia);
		}

		[ExpectNoExceptions]
		public void TestSetCanadaCargoControlNumberIfNotExistDoesNotThrow()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			DeleteAnyCarrierCode(Factory);
			CreateNewShipmentWithValidData();
			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			shipment.Delete();
			Factory.Save();
		}

		public void TestGetCarrierCodePerBranch()
		{
			FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain);
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			var oldOrgProxy = currentBranch.GB_OH_OrgProxy;
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			currentBranch.GB_OH_OrgProxy = orgProxy.PK;
			var orgCusCode = currentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "2345", Core.Constants.CountryCodes.Canada);
			Factory.Save();

			CreateNewShipmentWithValidData();
			shipment.HasChanges = true;
			Factory.Save();
			var createdCustomNumber = GetCargoControlNumber(shipment);
			Assert("Get Carrier Code from current branch OrgProxy", createdCustomNumber.StartsWith("2345"));

			orgCusCode.Delete();

			CreateNewShipmentWithValidData();
			shipment.HasChanges = true;
			Factory.Save();
			createdCustomNumber = GetCargoControlNumber(shipment);
			Assert("Fall back to get Carrier Code from current company OrgProxy", createdCustomNumber.StartsWith("1234"));

			DeleteAnyCarrierCode(Factory);
			currentBranch.GB_OH_OrgProxy = oldOrgProxy;
		}

		public void TestRollbackSetCanadaCargoControlNumber()
		{
			FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain);
			var anotherShipment = Factory.NewWithValidTestData<CFSShipment>();
			anotherShipment.JS_UniqueConsignRef = "S1000001";
			Factory.Save();

			CreateNewShipmentWithValidData();
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			shipment.JS_UniqueConsignRef = "S1000001";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals("Setting CargoControlNumber should rollback if the save failed", "", GetCargoControlNumber(shipment));

			shipment.JS_UniqueConsignRef = "S1000002";
			Factory.Save();
			ZString createdCustomNumber = GetCargoControlNumber(shipment);
			AssertEquals("New cargo control number", "123400000002", createdCustomNumber);

			shipment.JS_UniqueConsignRef = "S1000001";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals("CargoControlNumber in database should NOT be deleted", "123400000002", GetCargoControlNumber(shipment));
		}

		#region Implementation

		ZString GetCargoControlNumber(CFSShipment shipment)
		{
			CusEntryNumber result = GetCargoControlNumberEntryNum(shipment);
			return result == null ? ZString.Empty : result.CE_EntryNum;
		}

		CusEntryNumber GetCargoControlNumberEntryNum(CFSShipment shipment)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			CusEntryNumber[] results = (CusEntryNumber[])shipment.Numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		void CreateNewShipmentWithValidData()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.SetCountry(Core.Constants.CountryCodes.Canada);

			shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CATOR";
			shipment.JS_HouseBill = "1248163264";
		}

		CFSShipment shipment;

		#endregion
	}
}
