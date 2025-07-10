using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaTaxLookups : ASYCUDA.Business.AsycudaTaxLookups
	{
		public AsycudaTaxLookups(ASYCUDA.Business.AsycudaTax parent) : base(parent)
		{
		}

		protected new AsycudaTax Parent => (AsycudaTax)base.Parent;

		AsycudaManifestHeader ManifestHeader => (AsycudaManifestHeader)Parent.Bill.Header;

		public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<MethodOfPaymentList>();

		public CodeDescriptionPairList MethodOfCalculationList => TWRefCusCodeListTypes.GetMethodOfCalculationList(Factory);

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				CodeDescriptionPairList result;
				switch (Parent.AET_MethodOfPayment)
				{
					case AsycudaTaxPairList.MethodOfPaymentList.Codes.DutyLevied:
						result = ChargeTypeCASList;
						break;
					case AsycudaTaxPairList.MethodOfPaymentList.Codes.DutyNotLevied:
						result = ChargeTypeDEFList;
						break;
					default:
						result = ChargeTypeAllList;
						break;
				}

				return result;
			}
		}

		public CodeDescriptionPairList ChargeTypeAllList
		{
			get
			{
				return Factory.GetCachedValue("TW.AsycudaTaxLookups_ChargeTypeAllList", () =>
				{
					var result = new ChargeTypeCASList();
					result.AddRange(new ChargeTypeDEFList());
					return result;
				});
			}
		}

		CodeDescriptionPairList ChargeTypeCASList
		{
			get
			{
				var asycudaManifestHeader = ManifestHeader;
				var key = "TWChargeTypeList_CAS_" + asycudaManifestHeader?.AMA_Nature ?? ZString.Empty;
				return Factory.GetCachedValue(key, () =>
				{
					var result = new ChargeTypeCASList();
					if (asycudaManifestHeader is AsycudaManifestHeader header)
					{
						if (header.IsExport)
						{
							result.RemoveCode(AsycudaTaxPairList.ChargeTypeCASList.Codes.ImportTradePromotionFee);
						}
						else if (header.IsImport)
						{
							result.RemoveCode(AsycudaTaxPairList.ChargeTypeCASList.Codes.ExportTradePromotionFee);
						}
					}
					return result;
				});
			}
		}

		CodeDescriptionPairList ChargeTypeDEFList
		{
			get
			{
				var asycudaManifestHeader = ManifestHeader;
				var key = "TWChargeTypeList_DEF_" + asycudaManifestHeader?.AMA_Nature ?? ZString.Empty;
				return Factory.GetCachedValue(key, () =>
				{
					var result = new ChargeTypeDEFList();
					if (asycudaManifestHeader is AsycudaManifestHeader header && header.IsExport)
					{
						result.RemoveCode(AsycudaTaxPairList.ChargeTypeDEFList.Codes.ImportTradePromotionFee);
					}
					return result;
				});
			}
		}
	}
}
