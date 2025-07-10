using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public abstract class TestObjectCreator
	{
		protected TestObjectCreator(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		protected readonly BusinessObjectFactory Factory;

		protected string uNLOCOSydney = "AUSYD";
		protected string uNLOCOAuckland = "NZAKL";

		OrgHeader fLocalParty;
		public OrgHeader LocalParty
		{
			get { return fLocalParty ?? (fLocalParty = GetNewOrgHeader("ZZIMPE", "ADULT BOOKS LTD", uNLOCOAuckland, "TEST CODE FOR NZ CUSTOMS", "LOCATED IN NZAKL", "", "", isConsignor: true, isConsignee: true, isCarrier: false, OrgCusCode.CodeTypes.CustomsClientCode, "00782903F")); }
		}

		OrgHeader fOSParty;
		public OrgHeader OSParty
		{
			get { return fOSParty ?? (fOSParty = GetNewOrgHeader("ZZSUPA", "TEST SUPPLIER AU", uNLOCOSydney, "TEST CODE FOR NZ CUSTOMS", "LOCATED IN AUSYD", "", "", isConsignor: true, isConsignee: true, isCarrier: false, OrgCusCode.CodeTypes.SupplierCode, "00710841Y")); }
		}

		OrgHeader fShippingLine;
		protected OrgHeader ShippingLine
		{
			get { return fShippingLine ?? (fShippingLine = DHL); }
		}

		OrgHeader fForwarder;
		protected OrgHeader Forwarder
		{
			get { return fForwarder ?? (fForwarder = DHL); }
		}

		OrgHeader fDHL;
		protected OrgHeader DHL
		{
			get { return fDHL ?? (fDHL = GetNewOrgHeader("DHL", "DHL INTERNATIONAL LTD", uNLOCOAuckland, "CNR LAURENCE STEVENS & HAPE DRIVE", "AUCKLAND INTERNATIONAL AIRPORT", "", "", isConsignor: true, isConsignee: true, isCarrier: true)); }
		}

		OrgHeader fCarrier;
		public OrgHeader Carrier
		{
			get
			{
				if (fCarrier == null)
				{
					fCarrier = GetNewOrgHeader("TESCAR", "TEST CARRIER", "NZAKL", "1 CARRIER WAY", "CARRIERVILLE", "AUCKLAND", "1000", isConsignor: false, isConsignee: false, isCarrier: true);
					fCarrier.OH_IsAirLine = true;
				}

				return fCarrier;
			}
		}

		RefCurrency fRefCurrencyNZD;
		protected RefCurrency RefCurrencyNZD
		{
			get { return fRefCurrencyNZD ?? (fRefCurrencyNZD = GetRefCurrencyNZD()); }
		}
		RefCurrency GetRefCurrencyNZD()
		{
			return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.NewZealand);
		}

		protected OrgHeader GetNewOrgHeader(ZString code, ZString name, ZString unLOCO, ZString address1, ZString address2, ZString city, ZString postCode, ZBool isConsignor, ZBool isConsignee, ZBool isCarrier, ZString customsCodeType, ZString customsCode)
		{
			OrgHeader result = GetNewOrgHeader(code, name, unLOCO, address1, address2, city, postCode, isConsignor, isConsignee, isCarrier);
			result.SetLocalCustomsCode(customsCodeType, customsCode);
			return result;
		}

		protected OrgHeader GetNewOrgHeader(ZString code, ZString name, ZString unLOCO, ZString address1, ZString address2, ZString city, ZString postCode, ZBool isConsignor, ZBool isConsignee, ZBool isCarrier)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = name;
			result.OH_RL_NKClosestPort = unLOCO;
			result.OH_Code = code;
			result.MainAddress.OA_Address1 = address1;
			result.MainAddress.OA_Address2 = address2;
			result.MainAddress.OA_City = city;
			result.MainAddress.OA_PostCode = postCode;
			result.OH_IsConsignee = isConsignee;
			result.OH_IsConsignor = isConsignor;
			result.OH_IsShippingLine = isCarrier;
			result.OH_IsShippingProvider = isCarrier;
			result.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			return result;
		}

		OrgHeader fSupplier1;
		public OrgHeader Supplier1
		{
			get { return fSupplier1 ?? (fSupplier1 = GetNewOrgHeader("TESSUP1", "TEST SUPPLIER 1", "USLAX", "1 SUPPLIER ROAD", "SUPPLIERVILLE", "LOS ANGELES", "90120", isConsignor: true, isConsignee: false, isCarrier: false)); }
		}

		OrgHeader fSupplier2;
		public OrgHeader Supplier2
		{
			get { return fSupplier2 ?? (fSupplier2 = GetNewOrgHeader("TESSUP2", "TEST SUPPLIER 2", "USDNQ", "2 SUPPLIER ROAD", "SUPPLIERVILLE", "DENVER", "90210", isConsignor: true, isConsignee: false, isCarrier: false)); }
		}

		OrgHeader fImporter1;
		public OrgHeader Importer1
		{
			get { return fImporter1 ?? (fImporter1 = GetNewOrgHeader("TESIMP1", "TEST IMPORTER 1", "NZAKL", "1 IMPORTER ROAD", "IMPORTERVILLE", "AUCKLAND", "1012", isConsignor: false, isConsignee: true, isCarrier: false)); }
		}

		OrgHeader fImporter2;
		public OrgHeader Importer2
		{
			get { return fImporter2 ?? (fImporter2 = GetNewOrgHeader("TESIMP2", "TEST IMPORTER 2", "NZCHC", "2 IMPORTER ROAD", "IMPORTERVILLE", "CHRISTCHURCH", "2012", isConsignor: false, isConsignee: true, isCarrier: false)); }
		}

		OrgHeader fNotify;
		public OrgHeader Notify
		{
			get { return fNotify ?? (fNotify = GetNewOrgHeader("TESNOTIFY", "TEST NOTIFY 2", "AUSYD", "NOTIFY ROAD", "NOTIFYVILLE", "SYDANY", "3012", isConsignor: false, isConsignee: false, isCarrier: false)); }
		}
	}
}
