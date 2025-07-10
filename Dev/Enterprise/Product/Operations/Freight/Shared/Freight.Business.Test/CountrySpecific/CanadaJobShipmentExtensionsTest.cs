using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CanadaJobShipmentExtensionsTest : TestCaseWithFactory
	{
		public void TestIsDestinationToCanada()
		{
			CreateNewShipmentWithValidData();
			shipment.JS_RL_NKDestination = "CATOR";
			shipment.JS_RL_NKOrigin = "AUSYD";
			Assert(shipment.IsDestinationToCanada());
			shipment.JS_RL_NKOrigin = "CABLO";
			Assert(!shipment.IsDestinationToCanada());
			shipment.JS_RL_NKDestination = "CNHSA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			Assert(!shipment.IsDestinationToCanada());
		}

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
		public void TestNoCanadaCCNWhenConsolHasDomestic()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.SetCountry(Constants.CountryCodes.Canada);
			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "CAHAL";
			shipment.JS_RL_NKDestination = "CATOR";
			shipment.JS_HouseBill = "HOSBIL1";

			var consolDomestic = shipment.Consols.AddNew();
			consolDomestic.JK_TransportMode = Constants.TransportModes.Sea;
			consolDomestic.JK_UniqueConsignRef = "CONSOL2";
			consolDomestic.JK_RL_NKDischargePort = "GBLON";
			consolDomestic.JK_RL_NKLoadPort = "CAHAL";
			consolDomestic.MostInterestingTransportForBinding[0].IsDomestic = true;
			var transportLeg1 = consolDomestic.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "CAHAL";
			transportLeg1.JW_RL_NKDiscPort = "CAYYZ";
			transportLeg1.IsDomestic = true;
			transportLeg1.JW_ETD = ZDateTime.Today;
			var transportLeg2 = consolDomestic.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "CAYYZ";
			transportLeg2.JW_RL_NKDiscPort = "GBLON";
			transportLeg2.JW_ETA = ZDateTime.Today;

			Factory.Save();

			AssertEquals("No cargo control number", false, shipment.SetCanadaCargoControlNumberIfNotExist());

			consolDomestic.JK_RL_NKLoadPort = "USCHI";
			consolDomestic.MostInterestingTransportForBinding[0].IsDomestic = false;
			transportLeg1.JW_RL_NKLoadPort = "USCHI";
			transportLeg1.IsDomestic = false;
			Factory.Save();

			AssertEquals("Copied CCN to Shipment ", true, shipment.SetCanadaCargoControlNumberIfNotExist());
		}

		public void TestCanadaCargoControlNumber()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				CreateNewShipmentWithValidData();
				var currentCompany = GlbCompany.GetCurrentCompany(Factory);
				AssertEquals("Empty number for new shipment", "", GetCargoControlNumber(shipment));

				CreateNewShipmentWithValidData();
				DeleteAnyCarrierCode(Factory);
				DeleteAnyCarrierCode(Factory, Constants.CountryCodes.Australia);
				Assert("Empty number for new shipment, if no carrier code", !shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				AssertEquals("Empty number for new shipment, if no carrier code", "", GetCargoControlNumber(shipment));

				CreateNewShipmentWithValidData();
				currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
				Assert("Empty number for new shipment, if no carrier code for Canada", !shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				AssertEquals("Empty number for new shipment, if no carrier code for Canada", "", GetCargoControlNumber(shipment));

				currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;

				FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain);
				CreateNewShipmentWithValidData();
				Assert("New cargo control number", shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				ZString createdCustomNumber = GetCargoControlNumber(shipment);
				AssertEquals("New cargo control number", "123400000001", createdCustomNumber);

				CommonShipment loadedShipment = NewFactory().Load<CommonShipment>(shipment.PK);
				AssertEquals("Loaded custom number equals created number", createdCustomNumber, GetCargoControlNumber(loadedShipment));

				Assert("No overwriting of current CCN", !shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				AssertEquals("No overwriting of current CCN", createdCustomNumber, GetCargoControlNumber(shipment));

				shipment.Numbers.RemoveAndDeleteAll();
				Assert("New cargo control number", shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				AssertEquals("New cargo control number", "123400000002", GetCargoControlNumber(shipment));

				FreightDataRegistry.Instance.CanadaCargoControlNumberBranchPrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 32);
				shipment.Numbers.RemoveAndDeleteAll();
				shipment.SetCanadaCargoControlNumberIfNotExist();
				Factory.Save();
				AssertEquals("New cargo control number with branch prefix", "123432000003", GetCargoControlNumber(shipment));

				FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.HouseBill);
				CreateNewShipmentWithValidData();
				Assert("New cargo control number", shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				createdCustomNumber = GetCargoControlNumber(shipment);
				AssertEquals("New cargo control number", "123448163264", createdCustomNumber);

				FreightDataRegistry.Instance.CanadaCargoControlNumberHBLDigits.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				shipment.Numbers.RemoveAndDeleteAll();
				Assert("New cargo control number", shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				createdCustomNumber = GetCargoControlNumber(shipment);
				AssertEquals("New cargo control number", "12341248163264", createdCustomNumber);

				shipment.Numbers.RemoveAndDeleteAll();
				shipment.JS_RL_NKOrigin = "CATOR";
				shipment.JS_RL_NKDestination = "AUSYD";
				Assert("Blank for exports", !shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				AssertEquals("Blank for exports", "", GetCargoControlNumber(shipment));
				DeleteAnyCarrierCode(Factory);
				DeleteAnyCarrierCode(Factory, Constants.CountryCodes.Australia);

				currentCompany.SetCountry(Constants.CountryCodes.Australia);
				currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "2345").OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;

				shipment = Factory.NewWithValidTestData<CommonShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "CATOR";
				shipment.JS_HouseBill = "1248163264";
				Assert("New cargo control number", shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				createdCustomNumber = GetCargoControlNumber(shipment);
				AssertEquals("CCN set for Non-CA country", "23451248163264", createdCustomNumber);

				currentCompany.SetCountry(Constants.CountryCodes.Iceland);
				CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
				entryTypeList.Add("AAA", (NoResString)"AAA Test Entry Type").IsUnique = true;
				FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

				shipment = Factory.NewWithValidTestData<CommonShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "CATOR";
				shipment.JS_HouseBill = "1248163264";
				Assert("Cargo control number not set when no CCN set in registry", !shipment.SetCanadaCargoControlNumberIfNotExist());

				currentCompany.SetCountry(Constants.CountryCodes.Jamaica);
				entryTypeList = new CustomsReferenceNumberTypeCollection();
				entryTypeList.Add("CCN", (NoResString)"TEST CCN").IsUnique = true;
				FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

				shipment = Factory.NewWithValidTestData<CommonShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "CATOR";
				shipment.JS_HouseBill = "1248163265";
				Assert("New cargo control number", shipment.SetCanadaCargoControlNumberIfNotExist());
				Factory.Save();
				createdCustomNumber = GetCargoControlNumber(shipment);
				AssertEquals("CCN set for Non-CA country when CCN set in registry", "23451248163265", createdCustomNumber);

				shipment = Factory.NewWithValidTestData<CommonShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "GBLON";
				shipment.JS_HouseBill = "1248163265";
				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "CONSOL1";
				consol.JK_RL_NKDischargePort = "GBLON";
				consol.JK_RL_NKLoadPort = "AUSYD";
				var transportLeg1 = consol.Transports.AddNew();
				transportLeg1.JW_RL_NKLoadPort = "USPHL";
				transportLeg1.JW_RL_NKDiscPort = "CAYYZ";
				transportLeg1.JW_ETD = ZDateTime.Today;
				var transportLeg2 = consol.Transports.AddNew();
				transportLeg2.JW_RL_NKLoadPort = "CAYYZ";
				transportLeg2.JW_RL_NKDiscPort = "GBLON";
				transportLeg2.JW_ETA = ZDateTime.Today;
				Factory.Save();

				Assert("New cargo control number", shipment.SetCanadaCargoControlNumberIfNotExist());
			}
		}

		public void TestGetCarrierCodePerBranch()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain);
				var currentCompany = GlbCompany.GetCurrentCompany(Factory);
				currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
				var currentBranch = GlbBranch.GetCurrentBranch(Factory);
				var oldOrgProxy = currentBranch.GB_OH_OrgProxy;
				var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
				currentBranch.GB_OH_OrgProxy = orgProxy.PK;
				var orgCusCode = currentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "2345", Constants.CountryCodes.Canada);
				Factory.Save();

				CreateNewShipmentWithValidData();
				shipment.SetCanadaCargoControlNumberIfNotExist();
				Factory.Save();
				var createdCustomNumber = GetCargoControlNumber(shipment);
				Assert("Get Carrier Code from current branch OrgProxy", createdCustomNumber.StartsWith("2345"));

				orgCusCode.Delete();

				CreateNewShipmentWithValidData();
				shipment.SetCanadaCargoControlNumberIfNotExist();
				Factory.Save();
				createdCustomNumber = GetCargoControlNumber(shipment);
				Assert("Fall back to get Carrier Code from current company OrgProxy", createdCustomNumber.StartsWith("1234"));

				DeleteAnyCarrierCode(Factory);
				currentBranch.GB_OH_OrgProxy = oldOrgProxy;

				CreateNewShipmentWithValidData();
				currentCompany.SetCountry(Constants.CountryCodes.Australia);
				CommonConsol consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "CONSOL1";
				consol.JK_RL_NKDischargePort = "CABLO";
				consol.JK_RL_NKLoadPort = "AUSYD";

				var recieveForwarder = Factory.NewWithValidTestData<OrgHeader>();
				var orgCusCodeRF = recieveForwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "7890", Constants.CountryCodes.Canada);
				consol.JK_OA_ReceivingForwarderAddress = recieveForwarder.MainAddress.PK;
				shipment.JS_RL_NKDestination = "CABLO";
				shipment.SetCanadaCargoControlNumberIfNotExist();
				Factory.Save();
				createdCustomNumber = GetCargoControlNumber(shipment);
				Assert("Fall back to get Carrier Code from current company OrgProxy", createdCustomNumber.StartsWith("7890"));
			}
		}

		public void TestRollbackSetCanadaCargoControlNumber()
		{
			CreateNewShipmentWithValidData();
			var customReferenceNumber = shipment.Numbers.AddNew();
			customReferenceNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			customReferenceNumber.CE_EntryNum = "1234 567890";

			AssertEquals("Precondition:", "1234 567890", GetCargoControlNumber(shipment));
			shipment.RollbackSetCanadaCargoControlNumber();
			AssertEquals("The CCN number should be deleted", "", GetCargoControlNumber(shipment));
			AssertEquals("The CCN number should be deleted", 0, shipment.Numbers.Find(l => l.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN).Count());

			customReferenceNumber = shipment.Numbers.AddNew();
			customReferenceNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			customReferenceNumber.CE_EntryNum = "1234 567890";
			Factory.Save();

			AssertEquals("Precondition:", "1234 567890", GetCargoControlNumber(shipment));
			shipment.RollbackSetCanadaCargoControlNumber();
			AssertEquals("The CCN number should NOT be deleted", "1234 567890", GetCargoControlNumber(shipment));
			AssertEquals("The CCN number should NOT be deleted", 1, shipment.Numbers.Find(l => l.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN).Count());
		}

		public void TestReferenceNumbersContainsCCN()
		{
			CreateNewShipmentWithValidData();
			Assert("Precondition:", !CanadaJobShipmentExtensions.ReferenceNumbersContainsCCN(shipment));
			Factory.Save();

			var customReferenceNumber = new BusinessObjectFactory().New<CusEntryNumber>();
			customReferenceNumber.CE_ParentTable = "JobShipment";
			customReferenceNumber.CE_ParentID = shipment.PK;
			customReferenceNumber.CE_Category = "OTH";
			customReferenceNumber.CE_RN_NKCountryCode = Constants.CountryCodes.Canada;
			customReferenceNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			customReferenceNumber.CE_EntryNum = "1234 567890";
			customReferenceNumber.Factory.Save();

			Assert("Numbers in another Factory should be load", CanadaJobShipmentExtensions.ReferenceNumbersContainsCCN(shipment));
		}

		public void TestGetCarrierCodeWhenShipmentIsNull()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.SetCountry(Constants.CountryCodes.HongKong);
			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234").OK_RN_NKCodeCountry = Constants.CountryCodes.HongKong;
			var shipment = (CommonShipment)null;
			shipment.GetCarrierCode();

			Assert(ErrorReporter.LastMessageReported.Contains("shipment is null. CurrentCompay ="));
			ErrorReporter.Clear();
		}

		public void TestGetCarrierCodeWhenCurrentCompanyIsNull()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var fakeUserContext = new FakeUserContext(Env.CurrentUserContext);
			fakeUserContext.CompanyIsNull = true;

			using (Env.SetTemporaryUserContext(fakeUserContext))
			{
				AssertNull(GlbCompany.CurrentCompany);
				AssertNoExceptionThrown(() => shipment.GetCarrierCode());
			}
		}

		public void TestGetCarrierCodeWhenCurrentBranchIsNull()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var fakeUserContext = new FakeUserContext(Env.CurrentUserContext);
			fakeUserContext.BranchIsNull = true;

			using (Env.SetTemporaryUserContext(fakeUserContext))
			{
				AssertNull(GlbBranch.CurrentBranch);
				AssertNoExceptionThrown(() => shipment.GetCarrierCode());
			}
		}

		class FakeUserContext : UserContext, IUserContext
		{
			public FakeUserContext(IUserContext currentUserContext)
			{
				this.currentUserContext = currentUserContext;
			}
			readonly IUserContext currentUserContext;

			public bool CompanyIsNull { get; set; }
			public bool BranchIsNull { get; set; }

			ICompany IUserContext.Company
			{
				get => CompanyIsNull ? null : currentUserContext.Company;
			}

			IBranch IUserContext.Branch
			{
				get => BranchIsNull ? null : currentUserContext.Branch;
			}

			IDepartment IUserContext.Department
			{
				get => currentUserContext.Department;
			}

			IUser IUserContext.User
			{
				get { return currentUserContext.User; }
			}
		}

		#region Implementation

		ZString GetCargoControlNumber(CommonShipment shipment)
		{
			CusEntryNumber result = GetCargoControlNumberEntryNum(shipment);
			return result == null ? ZString.Empty : result.CE_EntryNum;
		}

		CusEntryNumber GetCargoControlNumberEntryNum(CommonShipment shipment)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Canada);
			CusEntryNumber[] results = (CusEntryNumber[])shipment.Numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		void CreateNewShipmentWithValidData()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.SetCountry(Constants.CountryCodes.Canada);

			shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CATOR";
			shipment.JS_HouseBill = "1248163264";
		}

		CommonShipment shipment;

		#endregion
	}
}
