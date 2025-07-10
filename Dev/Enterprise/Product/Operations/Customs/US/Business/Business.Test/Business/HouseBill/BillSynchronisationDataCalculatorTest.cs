using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BillSynchronisationDataCalculatorTest : TestCaseWithFactory
	{
		public void TestSynchronisationProcessWithAMSNumber()
		{
			SetUpData();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			CusEntryNumber num1 = shipment.Numbers.AddNew();
			num1.CE_EntryNum = "GTRE50060023";
			num1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			shipment.JS_HouseBill = "HWB123";
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declaration.JE_HouseBill);
			shipment.Numbers.RemoveAndDeleteAll();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			SetTransportMode(Core.Constants.TransportModes.Road);
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declaration.JE_HouseBill);
			SetTransportMode(Core.Constants.TransportModes.Sea);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declaration.JE_HouseBill);
			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_HouseBill = "IY465230";
			AssertEquals("IY465230", declaration.JE_HouseBill);
			AssertEquals("", declaration.JE_HouseBillIssuerSCAC);
			num1 = shipment.Numbers.AddNew();
			num1.CE_EntryNum = "GTRE50060023";
			num1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("IY465230", declaration.JE_HouseBill);
			AssertEquals("", declaration.JE_HouseBillIssuerSCAC);
			SetTransportMode(Core.Constants.TransportModes.Rail);
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("GTRE", declaration.JE_HouseBillIssuerSCAC);
			AssertEquals("50060023", declaration.JE_HouseBill);
			declaration.JE_HouseBillIssuerSCAC = "";
			declaration.JE_HouseBill = "";
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("GTRE", declaration.JE_HouseBillIssuerSCAC);
			AssertEquals("50060023", declaration.JE_HouseBill);
			shipment.Numbers.RemoveAndDeleteAll();
			SetTransportMode(Core.Constants.TransportModes.Rail);
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("IY465230", declaration.JE_HouseBill);
		}

		public void TestSynchroniseBill()
		{
			SetUpData();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			SetTransportMode(Core.Constants.TransportModes.Sea);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			shipment.JS_HouseBill = "HWB123";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var declarationLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			declarationLoaded.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declarationLoaded.JE_HouseBill);
			AssertEquals("APLU", declarationLoaded.JE_HouseBillIssuerSCAC);
			AssertEquals(1, declarationLoaded.Bills.Count);
			declarationLoaded.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(1, declarationLoaded.Bills.Count);
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declaration.JE_HouseBill);
			declaration.JE_HouseBillIssuerSCAC = "AA";
			AssertEquals("AA", declaration.JE_HouseBillIssuerSCAC);
			shipment.JS_HouseBill = "HWB124";
			AssertEquals("HWB124", declaration.JE_HouseBill);
			AssertEquals("APLU", declaration.JE_HouseBillIssuerSCAC);
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SCAC";
			carrier.UI_ModeOfTransportation = "10";
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var orgProxyCarrierCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			shipment.JS_HouseBill = "SCACBN001";
			AssertEquals("BN001", declaration.JE_HouseBill);
			AssertEquals("SCAC", declaration.JE_HouseBillIssuerSCAC);
		}

		public void TestSyncWithContainerNotAddedExtraHouseBill()
		{
			SetUpData();
			SetTransportMode(Core.Constants.TransportModes.Sea);
			declaration.JE_JS = ZGuid.Empty;
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "RXHU123456";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "RXHU1111117";
			container.JC_ContainerMode = "LCL";
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			shipment.JS_HouseBill = "HWB123";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
			packLine.JL_Length = 30m;
			packLine.JL_Width = 20m;
			packLine.JL_Height = 18m;
			declaration2.JE_JS = shipment.PK;
			declaration2.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(2, declaration2.Bills.Count);
			AssertEquals("HWB123", declaration2.JE_HouseBill);
			AssertEquals("APLU", declaration2.JE_HouseBillIssuerSCAC);
		}

		public void TestProperties()
		{
			SetUpData();
			shipment.JS_HouseBill = "8C1N56230124";
			var calculator = new BillSynchronisationDataCalculator(declaration, shipment);
			AssertEquals("8C1N56230124", calculator.BillNumber);
			AssertEquals("", calculator.IssuerCode);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			SetTransportMode(Core.Constants.TransportModes.Rail);
			AssertEquals("8C1N56230124", calculator.BillNumber);
			AssertEquals("", calculator.IssuerCode);
			consol.JK_MasterBillNum = "YI894456";
			calculator = new BillSynchronisationDataCalculator(declaration, consol);
			AssertEquals("YI894456", calculator.BillNumber);
			AssertEquals("", calculator.IssuerCode);
			var num1 = consol.Numbers.AddNew();
			num1.CE_EntryNum = "GTRE50060023";
			num1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			AssertEquals("50060023", calculator.BillNumber);
			AssertEquals("GTRE", calculator.IssuerCode);
		}

		void SetUpData()
		{
			declaration = Factory.New<JobDeclaration>();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
		}

		void SetTransportMode(ZString transportMode)
		{
			consol.JK_TransportMode = transportMode;
			shipment.JS_TransportMode = transportMode;
			declaration.JE_TransportMode = transportMode;
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
		JobDeclaration declaration;
	}
}
