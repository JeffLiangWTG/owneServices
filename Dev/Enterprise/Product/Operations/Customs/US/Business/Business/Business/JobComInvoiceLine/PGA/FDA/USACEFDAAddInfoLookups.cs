using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USACEFDAAddInfoLookups : AutoUSACEFDAAddInfoLookups
	{
		public USACEFDAAddInfoLookups(AutoUSACEFDAAddInfo parent)
			: base(parent)
		{
		}

		new USACEFDAAddInfo Parent
		{
			get { return (USACEFDAAddInfo)base.Parent; }
		}

		public CodeDescriptionPairList FDABaseUQs
		{
			get
			{
				var fda = Parent.Parent;
				var isPriorNotice = fda != null ? fda.IsPriorNotice : ZBool.False;
				return Factory.GetCachedValue("FDABaseUQs" + Parent.US_ProgramCode + isPriorNotice, delegate
				{
					return ACE_FDABaseUQList.GetFDABaseUQsCodeList(Parent.US_ProgramCode, isPriorNotice);
				});
			}
		}

		public CodeDescriptionPairList FDAUQs
		{
			get
			{
				var fda = Parent.Parent;
				var isPriorNotice = fda != null ? fda.IsPriorNotice : ZBool.False;
				return Factory.GetCachedValue("ACE_FDAUQs" + Parent.US_ProgramCode + isPriorNotice, delegate
				{
					return ACE_FDABaseUQList.GetFDAUQsCodeList(Parent.US_ProgramCode, isPriorNotice);
				});
			}
		}

		public USCCountryCollection USCCountryList
		{
			get { return new USCCountryCollection(Factory); }
		}

		public FDAProgramCodeList ProgramCodeList
		{
			get { return Factory.GetCachedValue<FDAProgramCodeList>(); }
		}

		public CodeDescriptionPairList ProcessingCodeList
		{
			get
			{
				var programCode = Parent.US_ProgramCode;
				return Factory.GetCachedValue("ProcessingCodeList" + programCode, delegate
				{
					return GetProgramCodeRelatedProcessingCodeList(programCode);
				});
			}
		}

		public static CodeDescriptionPairList GetProgramCodeRelatedProcessingCodeList(string programCode)
		{
			var list = new CodeDescriptionPairList();
			switch (programCode)
			{
				case FDAProgramCodeList.Codes.BIO:
					list.AddPair(FDAProcessingCodeList.Codes.BIO_ALG, FDAProcessingCodeList.Descriptions.BIO_ALG);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_BLO, FDAProcessingCodeList.Descriptions.BIO_BLO);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_BBA, FDAProcessingCodeList.Descriptions.BIO_BBA);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_BDP, FDAProcessingCodeList.Descriptions.BIO_BDP);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_CGT, FDAProcessingCodeList.Descriptions.BIO_CGT);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_HCT, FDAProcessingCodeList.Descriptions.BIO_HCT);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_BLD, FDAProcessingCodeList.Descriptions.BIO_BLD);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_PVE, FDAProcessingCodeList.Descriptions.BIO_PVE);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_VAC, FDAProcessingCodeList.Descriptions.BIO_VAC);
					list.AddPair(FDAProcessingCodeList.Codes.BIO_XEN, FDAProcessingCodeList.Descriptions.BIO_XEN);
					return list;
				case FDAProgramCodeList.Codes.COS:
					return list;
				case FDAProgramCodeList.Codes.DEV:
					list.AddPair(FDAProcessingCodeList.Codes.DEV_NED, FDAProcessingCodeList.Descriptions.DEV_NED);
					list.AddPair(FDAProcessingCodeList.Codes.DEV_RED, FDAProcessingCodeList.Descriptions.DEV_RED);
					return list;
				case FDAProgramCodeList.Codes.DRU:
					list.AddPair(FDAProcessingCodeList.Codes.DRU_INV, FDAProcessingCodeList.Descriptions.DRU_INV);
					list.AddPair(FDAProcessingCodeList.Codes.DRU_OTC, FDAProcessingCodeList.Descriptions.DRU_OTC);
					list.AddPair(FDAProcessingCodeList.Codes.DRU_PHN, FDAProcessingCodeList.Descriptions.DRU_PHN);
					list.AddPair(FDAProcessingCodeList.Codes.DRU_PRE, FDAProcessingCodeList.Descriptions.DRU_PRE);
					list.AddPair(FDAProcessingCodeList.Codes.DRU_RND, FDAProcessingCodeList.Descriptions.DRU_RND);
					list.AddPair(FDAProcessingCodeList.Codes.DRU_804, FDAProcessingCodeList.Descriptions.DRU_804);
					return list;
				case FDAProgramCodeList.Codes.FOO:
					list.AddPair(FDAProcessingCodeList.Codes.FOO_ADD, FDAProcessingCodeList.Descriptions.FOO_ADD);
					list.AddPair(FDAProcessingCodeList.Codes.FOO_FEE, FDAProcessingCodeList.Descriptions.FOO_FEE);
					list.AddPair(FDAProcessingCodeList.Codes.FOO_CCW, FDAProcessingCodeList.Descriptions.FOO_CCW);
					list.AddPair(FDAProcessingCodeList.Codes.FOO_DSU, FDAProcessingCodeList.Descriptions.FOO_DSU);
					list.AddPair(FDAProcessingCodeList.Codes.FOO_NSF, FDAProcessingCodeList.Descriptions.FOO_NSF);
					list.AddPair(FDAProcessingCodeList.Codes.FOO_PRO, FDAProcessingCodeList.Descriptions.FOO_PRO);
					return list;
				case FDAProgramCodeList.Codes.RAD:
					list.AddPair(FDAProcessingCodeList.Codes.RAD_REP, FDAProcessingCodeList.Descriptions.RAD_REP);
					return list;
				case FDAProgramCodeList.Codes.TOB:
					list.AddPair(FDAProcessingCodeList.Codes.TOB_CSU, FDAProcessingCodeList.Descriptions.TOB_CSU);
					list.AddPair(FDAProcessingCodeList.Codes.TOB_INV, FDAProcessingCodeList.Descriptions.TOB_INV);
					list.AddPair(FDAProcessingCodeList.Codes.TOB_FFM, FDAProcessingCodeList.Descriptions.TOB_FFM);
					return list;
				case FDAProgramCodeList.Codes.VME:
					list.AddPair(FDAProcessingCodeList.Codes.VME_ADE, FDAProcessingCodeList.Descriptions.VME_ADE);
					list.AddPair(FDAProcessingCodeList.Codes.VME_ADR, FDAProcessingCodeList.Descriptions.VME_ADR);
					return list;
				default:
					return new FDAProcessingCodeList();
			}
		}

		public CodeDescriptionPairList IntendedUseCodeList
		{
			get
			{
				var programCode = Parent.US_ProgramCode;
				var processingCode = Parent.US_ProcessingCode;

				var isProcessingCodeRequired = ProcessingCodeList.Count > 0;
				var isProcessingCodeValid = ProcessingCodeList.ContainsCode(processingCode);

				if (programCode.IsEmpty || (isProcessingCodeRequired && !isProcessingCodeValid))
				{
					return new CodeDescriptionPairList();
				}

				var list = GetIntendedUseCodeListByProgramAndProcessingCode(programCode, processingCode);
				if (list.Count == 0)
				{
					list = GetIntendedUseCodeListByProgramAndProcessingCode(programCode, ZString.Empty);
				}

				return list;
			}
		}

		public CodeDescriptionPairList GetIntendedUseCodeListByProgramAndProcessingCode(ZString programCode, ZString processingCode)
		{
			var filter = new List<KeyValuePair<ZString, ZString>>
			{
				new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.FDA),
				new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeProgram, programCode)
			};

			if (!processingCode.IsEmpty)
			{
				filter.Add(new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeProcess, processingCode));
			}

			return RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory, Core.Constants.CountryCodes.UnitedStates,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, ZDateTime.Today, filter.ToArray());
		}

		public ZZRefCusCodeListCombinedCollection FDAProductsList
		{
			get
			{
				var fda = Parent.Parent;
				var effectiveDate = fda?.InvoiceLine?.EffectiveDateForDutyRate ?? ZDateTime.Today;
				var countryCode = Core.Constants.CountryCodes.UnitedStates;
				var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;

				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, countryCode, listType, effectiveDate);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(countryCode), false));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(listType), false));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", effectiveDate));
				return collection;
			}
		}

		public ConsignorCollection Consignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		public CodeDescriptionPairList DimensionUQs
		{
			get
			{
				return Factory.GetCachedValue("FDA DimesionUQs for " + Parent.US_ProgramCode, delegate
				{
					var result = new FDAMeasurementUnitList();
					if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.FOO)
					{
						result.RemoveCode(FDAMeasurementUnitList.Codes.Centimeters);
						result.RemoveCode(FDAMeasurementUnitList.Codes.InchesWithOneTenthDecimals);
					}
					return result;
				});
			}
		}

		public CylindricalRectangularList CylindricalRectangularList
		{
			get { return Factory.GetCachedValue<CylindricalRectangularList>(); }
		}

		public ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public CodeDescriptionPairList IdentityNumberQualifierList
		{
			get
			{
				return Factory.GetCachedValue("ItemIdentityNumberQualifierList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(ItemIdentityNumberQualifierList.Codes.ModelNumber, ItemIdentityNumberQualifierList.Descriptions.ModelNumber);
					list.AddPair(ItemIdentityNumberQualifierList.Codes.SerialNumber, ItemIdentityNumberQualifierList.Descriptions.SerialNumber);
					list.AddPair(ItemIdentityNumberQualifierList.Codes.RegisteredNumber, ItemIdentityNumberQualifierList.Descriptions.RegisteredNumber);
					return list;
				}
				);
			}
		}

		public CodeDescriptionPairList ProducerFirmTypes
		{
			get
			{
				return Factory.GetCachedValue("FirmTypes" + Parent.US_ProgramCode, delegate
				{
					var list = new CodeDescriptionPairList();
					switch (Parent.US_ProgramCode)
					{
						case FDAProgramCodeList.Codes.TOB:
							list.AddPair("I", EntityRoleCodeList.Descriptions.IndependentThirdPartyLaboratory);
							list.AddPair("L", EntityRoleCodeList.Descriptions.Laboratory);
							return list;
						case FDAProgramCodeList.Codes.FOO:
							list = new ProducerFirmTypeList();
							list.RemoveCode(ProducerFirmTypeList.Codes.M);
							list.AddPair(ProducerFirmTypeList.Codes.M, "Manufacturer");
							return list;
						default:
							list.AddPair(ProducerFirmTypeList.Codes.M, "Manufacturer");
							return list;
					}
				});
			}
		}

		public FDAPriorNoticeExemptCodeList FoodFacilityRegistrationExemptionCodes
		{
			get
			{
				return Factory.GetCachedValue("FoodFacilityRegistrationExemptionCodes", delegate
				{
					var list = new FDAPriorNoticeExemptCodeList();
					list.RemoveCode(FDAPriorNoticeExemptCodeList.Codes.G);
					list.RemoveCode(FDAPriorNoticeExemptCodeList.Codes.H);
					list.RemoveCode(FDAPriorNoticeExemptCodeList.Codes.I);
					list.RemoveCode(FDAPriorNoticeExemptCodeList.Codes.J);
					list.RemoveCode(FDAPriorNoticeExemptCodeList.Codes.L);
					list.RemoveCode(FDAPriorNoticeExemptCodeList.Codes.M);
					list.RemoveCode(FDAPriorNoticeExemptCodeList.Codes.O);
					list.RemoveCode(FDAPriorNoticeExemptCodeList.Codes.Y);
					return list;
				});
			}
		}

		public ZAddressList ShipperAddressList
		{
			get
			{
				var fda = Parent.Parent;
				return fda.GetDataFromZAddressOrJobDocAddress(
					() => fda.US_OA_ShipperAddress_ZAddress.OrgAddress_List,
					() => fda.ShipperDocAddress,
					(x) => x.Lookups.Address_List,
					() => new ZAddressList());
			}
		}

		public ZAddressList DeliverToPartyAddressList
		{
			get
			{
				var fda = Parent.Parent;
				return fda.GetDataFromZAddressOrJobDocAddress(
					() => fda.US_DeliverToPartyAddress_ZAddress.OrgAddress_List,
					() => fda.DeliverToPartyDocAddress,
					(x) => x.Lookups.Address_List,
					() => new ZAddressList());
			}
		}

		public ZAddressList FDAImporterAddressList
		{
			get
			{
				var fda = Parent.Parent;
				return fda.GetDataFromZAddressOrJobDocAddress(
					() => fda.US_FDAImporterAddress_ZAddress.OrgAddress_List,
					() => fda.FDAImporterDocAddress,
					(x) => x.Lookups.Address_List,
					() => new ZAddressList());
			}
		}

		public ZAddressList FSVPImporterAddressList
		{
			get
			{
				var fda = Parent.Parent;
				return fda.GetDataFromZAddressOrJobDocAddress(
					() => fda.US_FSVPImporterAddress_ZAddress.OrgAddress_List,
					() => fda.FSVPImporterDocAddress,
					(x) => x.Lookups.Address_List,
					() => new ZAddressList());
			}
		}
	}
}
