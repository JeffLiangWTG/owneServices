using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	public class InBondHelperTest : DataObjectReaderTestHelper
	{
		OrgHeader importer;
		protected OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
					importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, CarrierCode2, Core.Constants.CountryCodes.UnitedStates);
				}

				return importer;
			}
		}

		OrgHeader supplier;
		protected OrgHeader Supplier => supplier ?? (supplier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory));

		OrgHeader supplier2;
		protected OrgHeader Supplier2 => supplier2 ?? (supplier2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory));

		OrgHeader warehouse;
		protected OrgHeader Warehouse => warehouse ?? (warehouse = GetOrganizationBO_INTHEMSYD(Factory.BOFactory));

		OrgSupplierPart part;
		protected OrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<OrgSupplierPart>();
					part.OP_PartNum = "~~1";
					part.OP_StockKeepingUnit = "NO";
					part.OP_Desc = "PART DESC";
					part.OP_Weight = 1.5m;
					part.OP_WeightUQ = Core.Constants.Weight.Pounds;
					part.RelatedOrganisations.AddOwner(Importer);
					part.RelatedOrganisations.AddSupplier(Supplier);
					_ = Pivot; // Force the creation of the pivot
				}
				return part;
			}
		}

		OrgSupplierPart part2;
		protected OrgSupplierPart Part2
		{
			get
			{
				if (part2 == null)
				{
					part2 = Factory.New<OrgSupplierPart>();
					part2.OP_PartNum = "~~2";
					part2.OP_StockKeepingUnit = "NO";
					part2.OP_Desc = "PART 2 DESC";
					part2.OP_Weight = 1.25m;
					part2.OP_WeightUQ = Core.Constants.Weight.Kilograms;
					part2.RelatedOrganisations.AddOwner(Importer);
					part2.RelatedOrganisations.AddSupplier(Supplier2);
					_ = Pivot2; // Force the creation of the pivot
				}

				return part2;
			}
		}

		CusClassification classification;
		protected CusClassification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<CusClassification>();
					classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
					classification.CC_LookupCode = "@#$34";
					classification.CC_TariffNum = "1010101010";
				}

				return classification;
			}
		}

		CusClassPartPivot pivot;
		protected CusClassPartPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					pivot = Part.PivotsForBinding.AddNew();
					pivot.CI_UsageComment = "U1";
					pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
					pivot.CI_OH = Part.RelatedOrganisations[0].OU_OH;
					pivot.CI_CC = Classification.PK;
					pivot.CD_LicenceNo = "13";
				}

				return pivot;
			}
		}

		CusClassPartPivot pivot2;
		protected CusClassPartPivot Pivot2
		{
			get
			{
				if (pivot2 == null)
				{
					pivot2 = Part2.PivotsForBinding.AddNew();
					pivot2.CI_UsageComment = "U1";
					pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
					pivot2.CI_OH = Part.RelatedOrganisations[0].OU_OH;
					pivot2.CI_TariffNum = "2010101010";
					pivot2.CD_LicenceNo = "13";
				}

				return pivot2;
			}
		}

		RefVessel aplVessel;
		protected RefVessel APLVessel
		{
			get
			{
				if (aplVessel == null)
				{
					aplVessel = Factory.New<RefVessel>();
					aplVessel.RV_Code = "APL VESSEL";
					aplVessel.RV_LloydsNumber = "9832343";
					aplVessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Jamaica;
				}

				return aplVessel;
			}
		}

		GlbCompany orgProxyCompany;
		protected GlbCompany OrgProxyCompany => orgProxyCompany ?? (orgProxyCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));

		GlbBranch inbondBranch;
		protected GlbBranch InBondBranch
		{
			get
			{
				if (inbondBranch == null)
				{
					inbondBranch = OrgProxyCompany.Branches.AddNew();
					inbondBranch.GB_Code = "IN@";
					inbondBranch.GB_BranchName = "InBond Branch Test";
					inbondBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				}

				return inbondBranch;
			}
		}

		ZZRefCusCodeListCombined seaLocalPort1ScheduleD;
		protected ZZRefCusCodeListCombined SeaLocalPort1ScheduleD
		{
			get
			{
				if (seaLocalPort1ScheduleD == null)
				{
					seaLocalPort1ScheduleD = Factory.New<ZZRefCusCodeListCombined>();
					seaLocalPort1ScheduleD.ZZD_Code = "BOB1";
					seaLocalPort1ScheduleD.ZZD_Description = "BOB SEA LOCAL PORT 1";
					seaLocalPort1ScheduleD.ZZD_CodeType = "CUSOF";
					seaLocalPort1ScheduleD.ZZD_CountryOrGrouping = "US";

					AssertNotNull(SeaLocalPort1);
				}

				return seaLocalPort1ScheduleD;
			}
		}

		RefUNLOCO seaLocalPort1;
		protected RefUNLOCO SeaLocalPort1
		{
			get
			{
				if (seaLocalPort1 == null)
				{
					seaLocalPort1 = Factory.New<RefUNLOCO>();
					seaLocalPort1.RL_Code = "LBOB1";
					seaLocalPort1.RL_PortName = "LOCAL SEA BOB PORT 1";
					seaLocalPort1.RL_HasSeaport = true;
					var locoMap = seaLocalPort1.RefLocoMaps.AddNew();
					locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
					locoMap.RY_LocalPortCode = SeaLocalPort1ScheduleD.ZZD_Code;
					locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
				}

				return seaLocalPort1;
			}
		}

		ZZRefCusCodeListCombined seaLocalPort2ScheduleD;
		protected ZZRefCusCodeListCombined SeaLocalPort2ScheduleD
		{
			get
			{
				if (seaLocalPort2ScheduleD == null)
				{
					seaLocalPort2ScheduleD = Factory.New<ZZRefCusCodeListCombined>();
					seaLocalPort2ScheduleD.ZZD_Code = "BOB2";
					seaLocalPort2ScheduleD.ZZD_Description = "BOB SEA LOCAL PORT 2";
					seaLocalPort2ScheduleD.ZZD_CodeType = "CUSOF";
					seaLocalPort2ScheduleD.ZZD_CountryOrGrouping = "US";
					AssertNotNull(SeaLocalPort2);
				}

				return seaLocalPort2ScheduleD;
			}
		}

		RefUNLOCO seaLocalPort2;
		protected RefUNLOCO SeaLocalPort2
		{
			get
			{
				if (seaLocalPort2 == null)
				{
					seaLocalPort2 = Factory.New<RefUNLOCO>();
					seaLocalPort2.RL_Code = "LBOB2";
					seaLocalPort2.RL_PortName = "LOCAL SEA BOB PORT 2";
					seaLocalPort2.RL_HasSeaport = true;
					var locoMap = seaLocalPort2.RefLocoMaps.AddNew();
					locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
					locoMap.RY_LocalPortCode = SeaLocalPort2ScheduleD.ZZD_Code;
					locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
				}

				return seaLocalPort2;
			}
		}

		ZZRefCusCodeListCombined seaLocalPort3ScheduleD;
		protected ZZRefCusCodeListCombined SeaLocalPort3ScheduleD
		{
			get
			{
				if (seaLocalPort3ScheduleD == null)
				{
					seaLocalPort3ScheduleD = Factory.New<ZZRefCusCodeListCombined>();
					seaLocalPort3ScheduleD.ZZD_Code = "BOB3";
					seaLocalPort3ScheduleD.ZZD_Description = "BOB SEA LOCAL PORT 3";
					seaLocalPort3ScheduleD.ZZD_CodeType = "CUSOF";
					seaLocalPort3ScheduleD.ZZD_CountryOrGrouping = "US";

					AssertNotNull(SeaLocalPort3);
				}

				return seaLocalPort3ScheduleD;
			}
		}

		RefUNLOCO seaLocalPort3;
		protected RefUNLOCO SeaLocalPort3
		{
			get
			{
				if (seaLocalPort3 == null)
				{
					seaLocalPort3 = Factory.New<RefUNLOCO>();
					seaLocalPort3.RL_Code = "LBOB3";
					seaLocalPort3.RL_PortName = "LOCAL SEA BOB PORT 3";
					seaLocalPort3.RL_HasSeaport = true;
					var locoMap = seaLocalPort3.RefLocoMaps.AddNew();
					locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
					locoMap.RY_LocalPortCode = SeaLocalPort3ScheduleD.ZZD_Code;
					locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
				}

				return seaLocalPort3;
			}
		}

		ZZRefCusCodeListCombined seaForeignPort1ScheduleK;
		protected ZZRefCusCodeListCombined SeaForeignPort1ScheduleK
		{
			get
			{
				if (seaForeignPort1ScheduleK == null)
				{
					seaForeignPort1ScheduleK = Factory.New<ZZRefCusCodeListCombined>();
					seaForeignPort1ScheduleK.ZZD_Code = "JACK1";
					seaForeignPort1ScheduleK.ZZD_Description = "JACK SEA FOREIGN PORT 1";
					seaForeignPort1ScheduleK.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port;
					seaForeignPort1ScheduleK.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.UnitedStates;
					AssertNotNull(SeaForeignPort1);
					var seaForeignPortAttribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
					seaForeignPortAttribute.ZZE_ZZD_CodeList = seaForeignPort1ScheduleK.PK;
					seaForeignPortAttribute.ZZE_Value = "";
					seaForeignPortAttribute.ZZE_ZXE_NKName = "PortValidType";
					AssertNotNull(seaForeignPortAttribute);
				}

				return seaForeignPort1ScheduleK;
			}
		}

		RefUNLOCO seaForeignPort1;
		protected RefUNLOCO SeaForeignPort1
		{
			get
			{
				if (seaForeignPort1 == null)
				{
					seaForeignPort1 = Factory.New<RefUNLOCO>();
					seaForeignPort1.RL_Code = "FJAK1";
					seaForeignPort1.RL_PortName = "FOREIGN SEA JACK PORT 1";
					seaForeignPort1.RL_HasSeaport = true;
					var locoMap = seaForeignPort1.RefLocoMaps.AddNew();
					locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
					locoMap.RY_LocalPortCode = SeaForeignPort1ScheduleK.ZZD_Code;
					locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
				}

				return seaForeignPort1;
			}
		}

		ZZRefCusCodeListCombined seaForeignPort2ScheduleK;
		protected ZZRefCusCodeListCombined SeaForeignPort2ScheduleK
		{
			get
			{
				if (seaForeignPort2ScheduleK == null)
				{
					seaForeignPort2ScheduleK = Factory.New<ZZRefCusCodeListCombined>();
					seaForeignPort2ScheduleK.ZZD_Code = "JACK2";
					seaForeignPort2ScheduleK.ZZD_Description = "JACK SEA FOREIGN PORT 2";
					seaForeignPort2ScheduleK.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port;
					seaForeignPort2ScheduleK.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.UnitedStates;
					AssertNotNull(SeaForeignPort2);
					var seaForeignPortAttribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
					seaForeignPortAttribute.ZZE_ZZD_CodeList = seaForeignPort2ScheduleK.PK;
					seaForeignPortAttribute.ZZE_Value = "";
					seaForeignPortAttribute.ZZE_ZXE_NKName = "PortValidType";
					AssertNotNull(seaForeignPortAttribute);
				}

				return seaForeignPort2ScheduleK;
			}
		}

		RefUNLOCO seaForeignPort2;
		protected RefUNLOCO SeaForeignPort2
		{
			get
			{
				if (seaForeignPort2 == null)
				{
					seaForeignPort2 = Factory.New<RefUNLOCO>();
					seaForeignPort2.RL_Code = "FJAK2";
					seaForeignPort2.RL_PortName = "FOREIGN SEA JACK PORT 2";
					seaForeignPort2.RL_HasSeaport = true;
					var locoMap = seaForeignPort2.RefLocoMaps.AddNew();
					locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
					locoMap.RY_LocalPortCode = SeaForeignPort2ScheduleK.ZZD_Code;
					locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
				}

				return seaForeignPort2;
			}
		}

		GlbStaff cusAgent;
		protected GlbStaff CusAgent
		{
			get
			{
				if (cusAgent == null)
				{
					cusAgent = Factory.New<GlbStaff>();
					cusAgent.GS_Code = "G@#";
					cusAgent.GS_LoginName = "CUSAGENT";
					cusAgent.GS_FullName = "BOB THE BUILDER";
					cusAgent.GS_EmailAddress = "BOB@WHERE.COM";
				}

				return cusAgent;
			}
		}

		OrgContact staffBob;
		protected OrgContact StaffBob
		{
			get
			{
				if (staffBob == null)
				{
					staffBob = Factory.New<OrgContact>();
					staffBob.OC_ContactName = "BOB THE BUILDER";
					staffBob.OC_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
					staffBob.OC_Phone = "+61 2 4040 9999";
				}

				return staffBob;
			}
		}

		OrgContact staffWendy;
		protected OrgContact StaffWendy
		{
			get
			{
				if (staffWendy == null)
				{
					staffWendy = Factory.New<OrgContact>();
					staffWendy.OC_ContactName = "WENDY THE DESTROYER";
					staffWendy.OC_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
					staffWendy.OC_Phone = "+61 3 5050 8435";
				}

				return staffWendy;
			}
		}

		OrgContact staffJack;
		protected OrgContact StaffJack
		{
			get
			{
				if (staffJack == null)
				{
					staffJack = Factory.New<OrgContact>();
					staffJack.OC_ContactName = "JACK THE PEACEMAKER";
					staffJack.OC_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
					staffJack.OC_Phone = "+61 8 9654 3624";
				}

				return staffJack;
			}
		}

		UNDGSubstance substance;
		protected UNDGSubstance Substance => substance ?? (substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery()));

		UNDGSubstance substance2;
		protected UNDGSubstance Substance2 => substance2 ?? (substance2 = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.PK, SQLComparisonOperator.NotEqual, Substance.PK)));

		ContainerType containerType1;
		protected ContainerType ContainerType1
		{
			get
			{
				if (containerType1 == null)
				{
					containerType1 = new ContainerType()
					{
						Code = ContainerTypeBO1.RC_Code
					};
				}

				return containerType1;
			}
		}

		ContainerType containerType2;
		protected ContainerType ContainerType2
		{
			get
			{
				if (containerType2 == null)
				{
					containerType2 = new ContainerType()
					{
						Code = ContainerTypeBO2.RC_Code
					};
				}

				return containerType2;
			}
		}

		RefContainer containerTypeBO1;
		protected RefContainer ContainerTypeBO1
		{
			get
			{
				if (containerTypeBO1 == null)
				{
					containerTypeBO1 = Factory.New<RefContainer>();
					containerTypeBO1.RC_Code = "Z12S";
					containerTypeBO1.RC_Description = "CONTAINER 1";
					containerTypeBO1.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
				}

				return containerTypeBO1;
			}
		}

		RefContainer containerTypeBO2;
		protected RefContainer ContainerTypeBO2
		{
			get
			{
				if (containerTypeBO2 == null)
				{
					containerTypeBO2 = Factory.New<RefContainer>();
					containerTypeBO2.RC_Code = "K77G";
					containerTypeBO2.RC_Description = "CONTAINER 2";
					containerTypeBO2.RC_ContainerType = Core.Constants.ContainerTypes.OpenTop;
				}

				return containerTypeBO2;
			}
		}

		protected const string CarrierCode1 = "SD23";
		protected const string CarrierCode2 = "KJ65";
	}
}
