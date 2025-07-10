using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	abstract class AMSSynchroniserTestCase : SynchroniserTestCase
	{
		protected void AssertBill(CusInBondBill bill, ZGuid pk, ZString issuerCode, ZString billNumber)
		{
			AssertEquals(pk, bill.PK);
			AssertBill(bill, issuerCode, billNumber);
		}

		protected void AssertBill(CusInBondBill bill, ZString issuerCode, ZString billNumber)
		{
			AssertEquals(issuerCode, bill.B0_IssuerCode);
			AssertEquals(billNumber, bill.B0_MasterBillNumber);
		}

		protected OrgHeader orgProxy;
		protected OrgCusCode orgProxyCarrierCode;
		protected GlbCompany otherCompany;
		protected GlbBranch otherCompanyBranch;
		protected OrgHeader otherBranchOrgProxy;
		protected OrgCusCode otherBranchOrgProxyCarrierCode;
		protected OrgHeader org1;
		protected OrgCusCode org1CarrierCode;
		protected OrgHeader org2;
		protected OrgCusCode org2CarrierCode;
		protected OrgCusCode org2PrefixCarrierCode;
		protected ForwardingConsol consol;
		protected CusInBondHeader header;
		protected RefUNLOCO AUSYD
		{
			get
			{
				return ausyd ?? (ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			}
		}

		RefUNLOCO ausyd;
		protected RefUNLOCO AUMEL
		{
			get
			{
				return aumel ?? (aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL"));
			}
		}

		RefUNLOCO aumel;
		protected RefUNLOCO SGSIN
		{
			get
			{
				return sgsin ?? (sgsin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));
			}
		}

		RefUNLOCO sgsin;
		protected RefUNLOCO USLAX
		{
			get
			{
				return uslax ?? (uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX"));
			}
		}

		RefUNLOCO uslax;
		protected RefUNLOCO USNYC
		{
			get
			{
				return usnyc ?? (usnyc = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USNYC"));
			}
		}

		RefUNLOCO usnyc;
		protected RefUNLOCO USCHI
		{
			get
			{
				return uschi ?? (uschi = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USCHI"));
			}
		}

		RefUNLOCO uschi;
		protected override void SetUp()
		{
			base.SetUp();
			otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "Z!Z";
			otherBranchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			otherBranchOrgProxy.OH_Code = "ZZZ123WWW";
			otherBranchOrgProxyCarrierCode = otherBranchOrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.UnitedStates);
			otherCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherCompanyBranch.GB_Code = "Z!Z";
			otherCompanyBranch.GB_OH_OrgProxy = otherBranchOrgProxy.PK;
			otherCompany.Branches.Add(otherCompanyBranch);
			orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxyCarrierCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1Z";
			org1.MainAddress.OA_Address1 = "ORG1Z ADDRESS 1";
			org1CarrierCode = org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ORG1", Core.Constants.CountryCodes.UnitedStates);
			org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2Z";
			org2.MainAddress.OA_Address1 = "ORG2Z ADDRESS 1";
			org2PrefixCarrierCode = org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CarrierPrefixCode, "ORGP", Core.Constants.CountryCodes.UnitedStates);
			org2CarrierCode = org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ORG2", Core.Constants.CountryCodes.UnitedStates);
			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.Synchroniser.SetEnabled(false, false);
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselNonContainer;
			Factory.Save();
		}
	}
}
