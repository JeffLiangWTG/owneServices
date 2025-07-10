using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public sealed class ExportNotificationMessageTestHelper
	{
		public ExportNotificationMessageTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		#region CreateShipment

		public ForwardingShipment CreateShipment()
		{
			var number1 = factory.New<CusEntryNumber>();
			number1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number1.CE_EntryType = "MRN";
			number1.CE_EntryNum = "MRN01";

			var number2 = factory.New<CusEntryNumber>();
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number2.CE_EntryType = "COC";
			number2.CE_EntryNum = "N01";

			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "FRPAR";
			shipment.JS_RL_NKDestination = "FRSXB";
			shipment.JS_HouseBill = "HWB";
			shipment.JS_OuterPacks = 1;
			shipment.JS_UniqueConsignRef = "S00006000";
			shipment.JS_ActualWeight = 20m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.Numbers.Add(number1);
			shipment.Numbers.Add(number2);

			var cfs = factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRPAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			var orgCusCode = cfs.CustomsCodes.AddNew();
			orgCusCode.OK_OH = cfs.PK;
			orgCusCode.OK_CodeType = "CTR";
			orgCusCode.SecuredCustomsRegNo = "N01";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;

			var orgCINCode = cfs.CustomsCodes.AddNew();
			orgCINCode.OK_OH = cfs.PK;
			orgCINCode.OK_CodeType = "CIN";
			orgCINCode.SecuredCustomsRegNo = "112233";
			orgCINCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_MasterBillNum = "11122222222";
			consol.JK_RL_NKLoadPort = "FRPAR";
			consol.JK_RL_NKDischargePort = "FRSXB";
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;

			var carrier = factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.OH_IsAirLine = true;
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			var carrierCINCode = carrier.CustomsCodes.AddNew();
			carrierCINCode.OK_OH = carrier.PK;
			carrierCINCode.OK_CodeType = "CIN";
			carrierCINCode.SecuredCustomsRegNo = "445566";
			carrierCINCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			var sendingParty = proxyMainAddress?.Header;

			if (sendingParty != null)
			{
				var sendingForwarderCINCode = sendingParty.CustomsCodes.AddNew();
				sendingForwarderCINCode.OK_OH = sendingParty.PK;
				sendingForwarderCINCode.OK_CodeType = "CIN";
				sendingForwarderCINCode.SecuredCustomsRegNo = "778899";
				sendingForwarderCINCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			}

			return shipment;
		}

		#endregion

		#region CreateConsol

		public ForwardingConsol CreateConsol()
		{
			var consol = factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NLAMS";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "123-12345678";

			var number1 = factory.New<CusEntryNumber>();
			number1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
			number1.CE_EntryType = "MRN";
			number1.CE_EntryNum = "MRN01";

			var shipment = consol.Shipments.AddNew();
			shipment.Numbers.Add(number1);
			shipment.JS_UniqueConsignRef = consol.JK_UniqueConsignRef;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NLAMS";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_HouseBill = "H001";
			shipment.JS_ActualWeight = 20;
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = "PKG";
			//shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			//shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = "";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_UnitOfVolume = "D3";

			var branchProxy = factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = "NL";
			branchProxy.OH_FullName = "TestCompany";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			factory.Save();

			var cgnCode1 = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			cgnCode1.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode;
			cgnCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
			cgnCode1.OK_CustomsRegNo = "C001";

			var carrier = factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.OH_IsAirLine = true;
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "NL";

			var firstTransportLeg = consol.Transports.Cast<Freight.Business.Transport>().First();
			firstTransportLeg.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			var cgnCode2 = carrier.CustomsCodes.AddNew();
			cgnCode2.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode;
			cgnCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
			cgnCode2.OK_CustomsRegNo = "C002";
			factory.Save();

			return consol;
		}

		#endregion
	}
}
