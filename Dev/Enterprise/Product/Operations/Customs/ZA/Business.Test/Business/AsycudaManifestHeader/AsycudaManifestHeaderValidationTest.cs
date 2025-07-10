using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using COSTCO = Enterprise.Customs.ZA.Business.ManifestTypeList.Codes;
using GOVGIO = Enterprise.Customs.ZA.Business.GateInOutMessageTypeCodeList.Codes;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderValidation))]
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		readonly string[] costcoCodes = new ManifestTypeList().GetAllCodes();
		readonly string[] govgioCodes = new GateInOutMessageTypeCodeList().GetAllCodes();

		public void TestCheckGateInOutMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.GateInOutMessageType = ZString.Empty;
			AssertNoMessageErrors(header.GateInOutMessageTypeInfo);

			header.GateInOutMessageType = "X";
			AssertHasMessageErrorContaining(header.GateInOutMessageTypeInfo, ListValidation.InvalidCodeMessageError);

			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.TerminalGateOut;
			AssertNoMessageErrors(header.GateInOutMessageTypeInfo);
		}

		public void TestCheckAMA_ManifestType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.AMA_ManifestType = ZString.Empty;
			AssertNoMessageErrors(header.AMA_ManifestTypeInfo);

			header.AMA_ManifestType = "X";
			AssertHasMessageErrorContaining(header.AMA_ManifestTypeInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			AssertNoMessageErrors(header.AMA_ManifestTypeInfo);
		}

		public void TestCheckUnpackedDate_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
				header.UnpackedDate = ZDate.Empty;
				AssertHasMessageErrorContaining(code, header.UnpackedDateInfo, MandatoryValidation.YouHaveNotEntered);

				header.UnpackedDate = ZDate.Today;
				AssertNoMessageErrors(code, header.UnpackedDateInfo);

				header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
				header.UnpackedDate = ZDate.Empty;
				AssertNoMessageErrors(code, header.UnpackedDateInfo);

				header.UnpackedDate = ZDate.Today;
				AssertHasMessageErrorContaining(code, header.UnpackedDateInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckUnpackedDate_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.UnpackedDate = ZDate.Empty;
				AssertNoMessageErrors(code, header.UnpackedDateInfo);

				header.UnpackedDate = ZDate.Today;
				AssertHasMessageErrorContaining(code, header.UnpackedDateInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckExcessIndicator_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					header.ExcessIndicator = ZString.Empty;
					AssertHasMessageErrorContaining(code, header.ExcessIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

					header.ExcessIndicator = ExcessIndicatorList.Codes.Excess;
					AssertNoMessageErrors(code, header.ExcessIndicatorInfo);

					header.ExcessIndicator = "X";
					AssertHasMessageErrorContaining(code, header.ExcessIndicatorInfo, ListValidation.InvalidCodeMessageError.ToString());
				}
				else
				{
					header.ExcessIndicator = ZString.Empty;
					AssertNoMessageErrors(code, header.ExcessIndicatorInfo);

					header.ExcessIndicator = ExcessIndicatorList.Codes.Excess;
					AssertHasMessageErrorContaining(code, header.ExcessIndicatorInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckExcessIndicator_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.ExcessIndicator = ZString.Empty;
				AssertNoMessageErrors(code, header.ExcessIndicatorInfo);

				header.ExcessIndicator = ExcessIndicatorList.Codes.Excess;
				AssertHasMessageErrorContaining(code, header.ExcessIndicatorInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAMA_Nature_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.AMA_Nature = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_NatureInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_Nature = NatureList.Codes.Export22;
				AssertNoMessageErrors(code, header.AMA_NatureInfo);

				header.AMA_Nature = "X";
				AssertHasMessageErrorContaining(code, header.AMA_NatureInfo, ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		public void TestCheckAMA_Nature_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_Nature = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_NatureInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_Nature = NatureList.Codes.Export22;
				AssertNoMessageErrors(code, header.AMA_NatureInfo);

				header.AMA_Nature = "X";
				AssertHasMessageErrorContaining(code, header.AMA_NatureInfo, ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		public void TestCheckAMA_Voyage_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.AMA_Voyage = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_VoyageInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_Voyage = "Voyage";
				AssertNoMessageErrors(code, header.AMA_VoyageInfo);
			}
		}

		public void TestCheckAMA_Voyage_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_Voyage = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_VoyageInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_Voyage = "Voyage";
				AssertNoMessageErrors(code, header.AMA_VoyageInfo);
			}
		}

		public void TestCheckAMA_TransportMode_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.AMA_TransportMode = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_TransportMode = "X";
				AssertHasMessageErrorContaining(code, header.AMA_TransportModeInfo, ListValidation.InvalidCodeMessageError.ToString());

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertNoMessageErrors(code, header.AMA_TransportModeInfo);
			}
		}

		public void TestCheckAMA_TransportMode_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_TransportMode = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_TransportMode = "X";
				AssertHasMessageErrorContaining(code, header.AMA_TransportModeInfo, ListValidation.InvalidCodeMessageError.ToString());

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertNoMessageErrors(code, header.AMA_TransportModeInfo);
			}
		}

		public void TestCheckAMA_OA_DischargeTerminalAddress_WhenCOSTCO()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "123";
			var address1 = org1.Addresses.AddNew();
			address1.Address1 = "12345";

			var org2 = Factory.New<OrgHeader>();
			org2.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "ABD", Core.Constants.CountryCodes.SouthAfrica);
			org2.OH_Code = "456";
			var address2 = org2.Addresses.AddNew();
			address2.Address1 = "45678";

			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.BulkBreakBulkOutturnReport, COSTCO.AirLoadDischarge))
				{
					header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
					AssertHasMessageErrorContaining(code, header.AMA_OA_DischargeTerminalAddressInfo, MandatoryValidation.YouHaveNotEntered);

					header.AMA_OA_DischargeTerminalAddress = ZGuid.Invalid;
					AssertHasErrorContaining(code, header.AMA_OA_DischargeTerminalAddressInfo, ListValidation.InvalidCodeError);

					header.AMA_OA_DischargeTerminalAddress = address1.PK;
					AssertHasMessageError(header.AMA_OA_DischargeTerminalAddressInfo, "Organization must have a code of type 'CPT' loaded.");

					header.AMA_OA_DischargeTerminalAddress = address2.PK;
					AssertNoMessageErrors(code, header.AMA_OA_DischargeTerminalAddressInfo);
				}
				else
				{
					header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
					AssertNoMessageErrors(code, header.AMA_OA_DischargeTerminalAddressInfo);

					header.AMA_OA_DischargeTerminalAddress = address1.PK;
					AssertHasMessageErrorContaining(code, header.AMA_OA_DischargeTerminalAddressInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckAMA_OA_DischargeTerminalAddress_WhenGOVGIO()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123";
			var carrierAddress = org.Addresses.AddNew();
			carrierAddress.Address1 = "12345";
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.TerminalGateIn, GOVGIO.AirTerminalGateIn, GOVGIO.BreakBulkGateIn))
				{
					header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
					AssertHasMessageErrorContaining(code, header.AMA_OA_DischargeTerminalAddressInfo, MandatoryValidation.YouHaveNotEntered);

					header.AMA_OA_DischargeTerminalAddress = ZGuid.Invalid;
					AssertHasErrorContaining(code, header.AMA_OA_DischargeTerminalAddressInfo, ListValidation.InvalidCodeError);

					header.AMA_OA_DischargeTerminalAddress = carrierAddress.PK;
					AssertNoMessageErrors(code, header.AMA_OA_DischargeTerminalAddressInfo);
				}
				else
				{
					header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
					AssertNoMessageErrors(code, header.AMA_OA_DischargeTerminalAddressInfo);

					header.AMA_OA_DischargeTerminalAddress = carrierAddress.PK;
					AssertHasMessageErrorContaining(code, header.AMA_OA_DischargeTerminalAddressInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckOutturnProvider_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Description");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities,
				"VW", "Description", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
			var outturnProviderList = header.Lookups.OutturnProviderList;
			outturnProviderList.Load();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.OutturnProvider = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.OutturnProviderInfo, MandatoryValidation.YouHaveNotEntered);

				header.OutturnProvider = "X";
				AssertHasMessageErrorContaining(code, header.OutturnProviderInfo, ListValidation.InvalidCodeMessageError.ToString());

				header.OutturnProvider = "VW";
				AssertNoMessageErrors(code, header.OutturnProviderInfo);
			}
		}

		public void TestCheckOutturnProvider_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Description");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities,
				"VW", "Description", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
			var outturnProviderList = header.Lookups.OutturnProviderList;
			outturnProviderList.Load();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.OutturnProvider = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.OutturnProviderInfo, MandatoryValidation.YouHaveNotEntered);

				header.OutturnProvider = "X";
				AssertHasMessageErrorContaining(code, header.OutturnProviderInfo, ListValidation.InvalidCodeMessageError.ToString());

				header.OutturnProvider = "VW";
				AssertNoMessageErrors(code, header.OutturnProviderInfo);
			}
		}

		public void TestCheckAMA_ContainerMode_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.AMA_ContainerMode = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_ContainerModeInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_ContainerMode = "X";
				AssertHasMessageErrorContaining(code, header.AMA_ContainerModeInfo, ListValidation.InvalidCodeMessageError.ToString());

				header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
				AssertNoMessageErrors(code, header.AMA_ContainerModeInfo);
			}
		}

		public void TestCheckAMA_ContainerMode_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_ContainerMode = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_ContainerModeInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_ContainerMode = "X";
				AssertHasMessageErrorContaining(code, header.AMA_ContainerModeInfo, ListValidation.InvalidCodeMessageError.ToString());

				header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
				AssertNoMessageErrors(code, header.AMA_ContainerModeInfo);

				header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
				AssertHasMessageErrorContaining(code, header.AMA_ContainerModeInfo, "Containerized cargo requires at least one container.");

				header.Containers.AddNew();
				header.Validation.ValidateAll();
				AssertNoMessageErrorContaining(code, header.AMA_ContainerModeInfo, "Containerized cargo requires at least one container.");
				header.Containers.RemoveAll();
			}
		}

		public void TestCheckFullyLoadedUnloadedDate_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code == COSTCO.VesselOutturnReport)
				{
					header.FullyLoadedUnloadedDate = ZDate.Empty;
					AssertNoMessageErrors(code, header.FullyLoadedUnloadedDateInfo);

					header.FullyLoadedUnloadedDate = ZDate.Today;
					AssertNoMessageErrors(code, header.FullyLoadedUnloadedDateInfo);
				}
				else
				{
					header.FullyLoadedUnloadedDate = ZDate.Empty;
					AssertNoMessageErrors(code, header.FullyLoadedUnloadedDateInfo);

					header.FullyLoadedUnloadedDate = ZDate.Today;
					AssertHasMessageErrorContaining(code, header.FullyLoadedUnloadedDateInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckFullyLoadedUnloadedDate_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.FullyLoadedUnloadedDate = ZDate.Empty;
				AssertNoMessageErrors(code, header.FullyLoadedUnloadedDateInfo);

				header.FullyLoadedUnloadedDate = ZDate.Today;
				AssertHasMessageErrorContaining(code, header.FullyLoadedUnloadedDateInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAMA_AgentType_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code == COSTCO.AirCargoOutturnReport)
				{
					header.AMA_AgentType = ZString.Empty;
					AssertHasMessageErrorContaining(code, header.AMA_AgentTypeInfo, MandatoryValidation.YouHaveNotEntered);

					header.AMA_AgentType = "X";
					AssertHasMessageErrorContaining(code, header.AMA_AgentTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

					header.AMA_AgentType = "DRT";
					AssertNoMessageErrors(code, header.AMA_AgentTypeInfo);
				}
				else
				{
					header.AMA_AgentType = ZString.Empty;
					AssertNoMessageErrors(code, header.AMA_AgentTypeInfo);

					header.AMA_AgentType = "DRT";
					AssertNoMessageErrors(code, header.AMA_AgentTypeInfo);
				}
			}
		}

		public void TestCheckAMA_AgentType_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_AgentType = ZString.Empty;
				AssertNoMessageErrors(code, header.AMA_AgentTypeInfo);

				header.AMA_AgentType = "DRT";
				AssertHasMessageErrorContaining(code, header.AMA_AgentTypeInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckParentBill_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.BulkBreakBulkOutturnReport, COSTCO.AirCargoOutturnReport))
				{
					header.ParentBill = ZString.Empty;
					AssertNoMessageErrors(code, header.ParentBillInfo);

					header.ParentBill = "12345";
					AssertNoMessageErrors(code, header.ParentBillInfo);
				}
				else
				{
					header.ParentBill = ZString.Empty;
					AssertNoMessageErrors(code, header.ParentBillInfo);

					header.ParentBill = "12345";
					AssertHasMessageErrorContaining(code, header.ParentBillInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckParentBill_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.ParentBill = ZString.Empty;
				AssertNoMessageErrors(code, header.ParentBillInfo);

				header.ParentBill = "12345";
				AssertHasMessageErrorContaining(code, header.ParentBillInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckBookingNumber_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.BookingNumber = ZString.Empty;
				AssertNoMessageErrors(code, header.BookingNumberInfo);

				header.BookingNumber = "12345";
				AssertHasMessageErrorContaining(code, header.BookingNumberInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckBookingNumber_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.TerminalGateIn))
				{
					header.BookingNumber = ZString.Empty;
					AssertHasMessageErrorContaining(code, header.BookingNumberInfo, MandatoryValidation.YouHaveNotEntered);

					header.BookingNumber = "12345";
					AssertNoMessageErrors(code, header.BookingNumberInfo);
				}
				else
				{
					header.BookingNumber = ZString.Empty;
					AssertNoMessageErrors(code, header.BookingNumberInfo);

					header.BookingNumber = "12345";
					AssertHasMessageErrorContaining(code, header.BookingNumberInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckAMA_OA_DeconsolidateAddress_WhenCOSTCO()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "123";
			var address1 = org1.Addresses.AddNew();
			address1.Address1 = "12345";

			var org2 = Factory.New<OrgHeader>();
			org2.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "ABD", Core.Constants.CountryCodes.SouthAfrica);
			org2.OH_Code = "456";
			var address2 = org2.Addresses.AddNew();
			address2.Address1 = "45678";

			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.BulkBreakBulkOutturnReport, COSTCO.AirLoadDischarge))
				{
					header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
					AssertHasMessageErrorContaining(code, header.AMA_OA_DeconsolidateAddressInfo, MandatoryValidation.YouHaveNotEntered);

					header.AMA_OA_DeconsolidateAddress = ZGuid.Invalid;
					AssertHasErrorContaining(code, header.AMA_OA_DeconsolidateAddressInfo, ListValidation.InvalidCodeError);

					header.AMA_OA_DeconsolidateAddress = address1.PK;
					AssertHasMessageError(code, header.AMA_OA_DeconsolidateAddressInfo, "Organization must have a code of type 'CPD' loaded.");

					header.AMA_OA_DeconsolidateAddress = address2.PK;
					AssertNoMessageErrors(code, header.AMA_OA_DeconsolidateAddressInfo);
				}
				else
				{
					header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
					AssertNoMessageErrors(code, header.AMA_OA_DeconsolidateAddressInfo);

					header.AMA_OA_DeconsolidateAddress = address1.PK;
					AssertHasMessageErrorContaining(code, header.AMA_OA_DeconsolidateAddressInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckAMA_OA_DeconsolidateAddress_WhenGOVGIO()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123";
			var address = org.Addresses.AddNew();
			address.Address1 = "12345";
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_OA_DeconsolidateAddressInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_OA_DeconsolidateAddress = ZGuid.Invalid;
				AssertHasErrorContaining(code, header.AMA_OA_DeconsolidateAddressInfo, ListValidation.InvalidCodeError);

				header.AMA_OA_DeconsolidateAddress = address.PK;
				AssertNoMessageErrors(code, header.AMA_OA_DeconsolidateAddressInfo);
			}
		}

		public void TestCheckGateInOutDate_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.GateInOutDate = ZDate.Empty;
				AssertNoMessageErrors(code, header.GateInOutDateInfo);

				header.GateInOutDate = ZDate.Today;
				AssertHasMessageErrorContaining(code, header.GateInOutDateInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckGateInOutDate_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.GateInOutDate = ZDateTime.Today;

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.Containers.RemoveAll();
				header.GateInOutDate = ZDate.Empty;
				AssertHasMessageErrorContaining(code, header.GateInOutDateInfo, MandatoryValidation.YouHaveNotEntered);

				header.GateInOutDate = ZDate.Today;
				AssertNoMessageErrors(code, header.GateInOutDateInfo);

				header.Containers.AddNew();
				header.GateInOutDate = ZDate.Empty;
				AssertNoMessageErrors(code, header.GateInOutDateInfo);

				header.GateInOutDate = ZDate.Today;
				AssertHasMessageErrorContaining(code, header.GateInOutDateInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAMA_OA_Carrier_WhenCOSTCO()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "123";
			var address1 = org1.Addresses.AddNew();
			address1.Address1 = "12345";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "456";
			org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "12", Core.Constants.CountryCodes.SouthAfrica);
			var address2 = org2.Addresses.AddNew();
			address2.Address1 = "45678";

			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.AMA_OA_Carrier = ZGuid.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_OA_Carrier = ZGuid.BrettsGuid;
				AssertHasErrorContaining(code, header.AMA_OA_CarrierInfo, ListValidation.InvalidCodeError);

				header.AMA_OA_Carrier = address1.PK;
				AssertHasMessageError(header.AMA_OA_CarrierInfo, "No carrier code can be determined. There is no related ZA Customs reference file for the vessel, and there is no record selected in the Carrier field where a 'CCC' code is present. Please supply a carrier.");

				header.AMA_OA_Carrier = address2.PK;
				AssertNoMessageErrors(code, header.AMA_OA_CarrierInfo);
			}
		}

		public void TestCheckAMA_OA_Carrier_WhenGOVGIO()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "123";
			var address1 = org1.Addresses.AddNew();
			address1.Address1 = "12345";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "456";
			org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "12", Core.Constants.CountryCodes.SouthAfrica);
			var address2 = org2.Addresses.AddNew();
			address2.Address1 = "45678";

			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_OA_Carrier = ZGuid.Empty;
				AssertHasMessageErrorContaining(code, header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_OA_Carrier = ZGuid.BrettsGuid;
				AssertHasErrorContaining(code, header.AMA_OA_CarrierInfo, ListValidation.InvalidCodeError);

				header.AMA_OA_Carrier = address1.PK;
				AssertHasMessageError(header.AMA_OA_CarrierInfo, "No carrier code can be determined. There is no related ZA Customs reference file for the vessel, and there is no record selected in the Carrier field where a 'CCC' code is present. Please supply a carrier.");

				header.AMA_OA_Carrier = address2.PK;
				AssertNoMessageErrors(code, header.AMA_OA_CarrierInfo);
			}
		}

		public void TestCheckAMA_VesselName_WhenCOSTCO()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "Vessel1";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "Vessel2";
			vessel2.RV_RadioCallSign = "RCS";

			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_VesselName = ZString.Empty;
				AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_VesselName = "X";
				AssertHasMessageError(header.AMA_VesselNameInfo, ListValidation.InvalidCodeMessageError);

				header.AMA_VesselName = "Vessel1";
				AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, "Vessel must have a radio call sign.");

				header.AMA_VesselName = "Vessel2";
				AssertNoMessageErrors(header.AMA_VesselNameInfo);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_VesselName = ZString.Empty;
				AssertNoMessageErrors(header.AMA_VesselNameInfo);

				header.AMA_VesselName = "Vessel1";
				AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAMA_VesselName_WhenGOVGIO()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel1";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "Vessel2";
			vessel2.RV_RadioCallSign = "RCS";

			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_VesselName = ZString.Empty;
				AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_VesselName = "X";
				AssertHasMessageError(header.AMA_VesselNameInfo, ListValidation.InvalidCodeMessageError);

				header.AMA_VesselName = "Vessel1";
				AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, "Vessel must have a radio call sign.");

				header.AMA_VesselName = "Vessel2";
				AssertNoMessageErrors(header.AMA_VesselNameInfo);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_VesselName = ZString.Empty;
				AssertNoMessageErrors(header.AMA_VesselNameInfo);

				header.AMA_VesselName = "Vessel1";
				AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAMA_MasterBill_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (!code.In(COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					header.AMA_MasterBill = ZString.Empty;
					AssertHasMessageErrorContaining(code, header.AMA_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

					header.AMA_MasterBill = "12345";
					AssertNoMessageErrors(code, header.AMA_MasterBillInfo);
				}
			}
		}

		public void TestCheckAMA_MasterBill_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.SeaDepotConsignmentGateIn, GOVGIO.BreakBulkGateIn))
				{
					header.AMA_MasterBill = ZString.Empty;
					AssertHasMessageErrorContaining(code, header.AMA_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

					header.AMA_MasterBill = "12345";
					AssertNoMessageErrors(code, header.AMA_MasterBillInfo);
				}
			}
		}

		public void TestParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestHeader>(header.Validation.Parent);
		}
	}
}
