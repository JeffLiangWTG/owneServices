using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override CodeDescriptionPairList Natures => ShipmentTypeList.Export22AndImport23();

		public override CodeDescriptionPairList TransportModeList
		{
			get
			{
				return new CodeDescriptionPairList
				{
					new CodeDescriptionPair(TransportTypeList.Codes.Air, TransportTypeList.Descriptions.Air),
					new CodeDescriptionPair(TransportTypeList.Codes.Sea, TransportTypeList.Descriptions.Sea),
					new CodeDescriptionPair(TransportTypeList.Codes.Road, TransportTypeList.Descriptions.Road)
				};
			}
		}
		public ZZRefCusCodeListCombinedCollection GoodsLocationCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes }, ZDateTime.Now, null);

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		ZDateTime EffectiveDate => Parent.ApplicationBusinessProvider.GetEffectiveDateForDutyRate(Parent);

		ZString DataGrouping => Parent?.AMA_RN_NKCountry ?? ZString.Empty;

		#region Procedure List

		public ICodeDescriptionPairList Procedures => Factory.GetCachedValue("TRProcedures" + EffectiveDate + DataGrouping + Parent?.AMA_Nature,
			() => new RefCusProcedureCollection(Factory, DataGrouping, EffectiveDate, ZString.Empty, Parent?.AMA_Nature ?? ZString.Empty));

		#endregion

		public CodeDescriptionPairList CustomsOfficeList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Parent.AMA_RN_NKCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
		public CodeDescriptionPairList BondTypeList => Factory.GetCachedValue<GuaranteeTypeCodeList>();
		public RefCurrencyCollection CurrenciesList => new RefCurrencyCollection(Factory);
		public override CodeDescriptionPairList RegistrationStatusList => Factory.GetCachedValue<CustomsStatusList>();
		public CodeDescriptionPairList CustomsMessageStatusList => Factory.GetCachedValue<Messaging.TRMessageStatusCodeList>();

		public new CodeDescriptionPairList ContainerModes
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPairIfNotExist(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
				return list;
			}
		}

		public CodeDescriptionPairList MessageModeList
		{
			get
			{
				var isImport = Parent.IsImport;
				var cacheKey = "TR_ETrade_MessageModeList_" + (isImport ? (NoResString)"Import" : (NoResString)"Export");

				return Factory.GetCachedValue(cacheKey, () =>
				{
					var pairList = new CodeDescriptionPairList();
					pairList.AddPair(TRMessageTypes.Codes.TRE, TRMessageTypes.Descriptions.TRE);

					if (Parent.IsImport)
					{
						pairList.AddPair(TRMessageTypes.Codes.TRQ, TRMessageTypes.Descriptions.TRQ);
						pairList.AddPair(TRMessageTypes.Codes.TRI, TRMessageTypes.Descriptions.TRI);
						pairList.AddPair(TRMessageTypes.Codes.TRL, TRMessageTypes.Descriptions.TRL);
						pairList.AddPair(TRMessageTypes.Codes.TRB, TRMessageTypes.Descriptions.TRB);
						pairList.AddPair(TRMessageTypes.Codes.TRD, TRMessageTypes.Descriptions.TRD);
						pairList.AddPair(TRMessageTypes.Codes.TCD, TRMessageTypes.Descriptions.TCD);
					}
					else
					{
						pairList.AddPair(TRMessageTypes.Codes.TRS, TRMessageTypes.Descriptions.TRS);
						pairList.AddPair(TRMessageTypes.Codes.TRI, TRMessageTypes.Descriptions.TRI);
						pairList.AddPair(TRMessageTypes.Codes.TRL, TRMessageTypes.Descriptions.TRL);
					}

					return pairList;
				});
			}
		}
	}
}
