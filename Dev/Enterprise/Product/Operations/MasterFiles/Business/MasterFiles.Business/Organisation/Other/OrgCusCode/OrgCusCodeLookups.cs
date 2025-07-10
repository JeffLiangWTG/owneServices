using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.NZ;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCusCodeLookups : AutoOrgCusCodeLookups
	{
		public OrgCusCodeLookups(AutoOrgCusCode parent)
			: base(parent)
		{
			this.CusCode = (OrgCusCode)parent;
		}

		readonly OrgCusCode CusCode;

		#region Code Types

		public CodeDescriptionPairList OK_CodeType_List
		{
			get
			{
				return Factory.GetCachedValue($"OrgCusCodeLookups.OK_CodeType_List.{CusCode.OK_RN_NKCodeCountry}.{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}", () => GetOK_CodeType_List());
			}
		}

		protected virtual CodeDescriptionPairList GetOK_CodeType_List()
		{
			return new OrgCodeLists().CustomsCodes_List(CusCode.CodeCountry);
		}

		#endregion

		#region Organisations

		public new OrganisationsFindBoxCollection Headers
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region NZCSupplier_List
		public BusinessObjectCollection NZCSupplier_List
		{
			get
			{
				if (fNZCSupplier_List == null)
				{
					fNZCSupplier_List = ObjectFactory.Get<INZSupplierProvider>().GetSupplierList(Factory, CusCode);
				}
				return fNZCSupplier_List;
			}
		}
		BusinessObjectCollection fNZCSupplier_List;

		#endregion

		#region SGPartyStatusTypeList

		public ICodeDescriptionPairList SGPartyStatusTypeList
		{
			get
			{
				return ObjectFactory.Get<Enterprise.Integration.Customs.ASYCUDA.SGAccess.IAsycudaPartyStatusProvider>().GetPartyStatusCodeList(Factory);
			}
		}

		#endregion

		#region DIRList

		public CodeDescriptionPairList SGDirectDeliveryList
		{
			get
			{
				return Factory.GetCachedValue("OrgCusCodeDIRList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ZString.Empty, ZString.Empty);
					result.AddPair("Y", ResString.GetMultilingualString("99a95eec-b1cf-4872-b212-f87aa8dae165", "Yes"));
					return result;
				});
			}
		}

		#endregion

		#region NMFCParticipantList

		public CodeDescriptionPairList NMFCParticipantList
		{
			get
			{
				return Factory.GetCachedValue("OrgCusCodeNMFCParticipantList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(OrgConstants.NMFCParticipantCodes.Code.Yes, OrgConstants.NMFCParticipantCodes.Description.Yes);
					result.AddPair(OrgConstants.NMFCParticipantCodes.Code.No, OrgConstants.NMFCParticipantCodes.Description.No);
					return result;
				});
			}
		}

		#endregion

		#region OrgPremiseGateCode_List

		public RefPremisesGateCodeCollection OrgPremiseGateCode_List
		{
			get
			{
				if (fOrgPremiseGateCode_List == null)
				{
					fOrgPremiseGateCode_List = new RefPremisesGateCodeCollection(Factory);
					fOrgPremiseGateCode_List.AdditionalFilter = ZQuery.NoResultQuery;
				}
				return fOrgPremiseGateCode_List;
			}
		}
		RefPremisesGateCodeCollection fOrgPremiseGateCode_List;

		#endregion

		#region CustomsSupervisingOffice_List

		// This lookup is tested in Enterpise.Customs.Universal
		public CodeDescriptionPairList CustomsSupervisingOfficeList
		{
			get
			{
				var zzCusCodeListGetter = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>();
				return zzCusCodeListGetter.GetList(Factory, Parent.OK_RN_NKCodeCountry, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupervisingOffice, ZDateTime.Today, null) as CodeDescriptionPairList;
			}
		}

		#endregion

		// This lookup is tested in Enterpise.Customs.Universal
		#region CustomsOfficeOfExit_List
		public IBusinessObjectCollection CustomsOfficeOfExitList
		{
			get
			{
				var zzCusCodeListGetter = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>();
				return zzCusCodeListGetter.GetCollection(Factory, new ZString[] { Parent.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes }, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEClearanceLocation, ZDateTime.Today);
			}
		}
		#endregion

		#region KR

		// This lookup is tested in Enterpise.Customs.Universal
		public IBusinessObjectCollection KRIndustrialParkCodeList
		{
			get
			{
				var zzCusCodeListGetter = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>();
				return zzCusCodeListGetter.GetCollection(Factory, Parent.OK_RN_NKCodeCountry, KoreaSouthComplianceInfo.IndustrialParkCusCodeType, ZDateTime.Today);
			}
		}

		// This lookup is tested in Enterpise.Customs.Universal
		public IBusinessObjectCollection KRBondedAreaCodeList
		{
			get
			{
				var zzCusCodeListGetter = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>();
				return zzCusCodeListGetter.GetCollection(Factory, Parent.OK_RN_NKCodeCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, ZDateTime.Today);
			}
		}

		public IBusinessObjectCollection KRCustomsCarrierCodeList
		{
			get
			{
				var zzCusCodeListGetter = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>();
				return zzCusCodeListGetter.GetCollection(Factory, Parent.OK_RN_NKCodeCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.KRForwarderIDs, ZDateTime.Today);
			}
		}

		#endregion

		#region JP

		public IBusinessObjectCollection JPCustomsControlledPremisesCodeList
		{
			get
			{
				var zzCusCodeListGetter = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>();
				return zzCusCodeListGetter.GetCollection(Factory, Parent.OK_RN_NKCodeCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, ZDateTime.Today);
			}
		}

		#endregion

		public new OrgCusCode Parent => (OrgCusCode)base.Parent;

		#region Addresses

		public new OrgAddressDependentCollection PremisesAddresses
		{
			get
			{
				OrgAddressDependentCollection premisesAddresses;

				if (!CusCode.IsDeleted && CusCode.Header != null)
				{
					ZQuery filter = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
					filter.AddToFilter(OrgAddressSchema.OA_IsActive, ZBool.True);
					if (!CusCode.OK_OA_PremisesAddress.IsEmpty)
					{
						filter.AddToFilter(OrgAddressSchema.PK, CusCode.OK_OA_PremisesAddress);
					}
					premisesAddresses = new OrgAddressDependentCollection(CusCode.Header, filter);
					premisesAddresses.Load();
				}
				else
				{
					premisesAddresses = new OrgAddressDependentCollection(Factory);
				}

				return premisesAddresses;
			}
		}

		#endregion
	}
}
