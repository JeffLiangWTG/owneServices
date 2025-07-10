using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class PartyTypeList
	{
		public static CodeDescriptionPairList GetListForPSTNotifyPartyList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PartyTypeListForPSTNotifyPartyList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.CustomsBroker, Descriptions.CustomsBroker);
				result.AddPair(Codes.Importer, Descriptions.Importer);
				return result;
			});
		}

		public static CodeDescriptionPairList GetListForPSTCertifyingIndividual(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PartyTypeListForPSTCertifyingIndividual", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.CustomsBroker, Descriptions.CustomsBroker);
				result.AddPair(Codes.Importer, Descriptions.Importer);
				result.AddPair(Codes.Shipper, Descriptions.Shipper);
				return result;
			});
		}

		public static CodeDescriptionPairList GetListForNHTSACertifyingIndividual(BusinessObjectFactory factory)
		{
			return GetList_CB_IM_DFP(factory);
		}

		public static CodeDescriptionPairList GetListForVNECertifyingIndividual(BusinessObjectFactory factory)
		{
			return GetList_CB_IM_DFP(factory);
		}

		public static CodeDescriptionPairList GetListForFSISCertifyingIndividual(BusinessObjectFactory factory)
		{
			return GetList_CB_IM(factory);
		}

		public static CodeDescriptionPairList GetListForFWSCertifyingIndividual(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PartyTypeListForFWSCertifyingIndividual", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.CustomsBroker, Descriptions.CustomsBroker);
				result.AddPair(Codes.FWSImporter, Descriptions.FWSImporter);
				result.AddPair(Codes.FWSForeignExporter, Descriptions.FWSForeignExporter);
				return result;
			});
		}

		static CodeDescriptionPairList GetList_CB_IM_DFP(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PartyTypeList_CB_IM_DFP", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.CustomsBroker, Descriptions.CustomsBroker);
				result.AddPair(Codes.Importer, Descriptions.Importer);
				result.AddPair(Codes.Owner, Descriptions.Owner);
				return result;
			});
		}

		static CodeDescriptionPairList GetList_CB_IM(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PartyTypeList_CB_IM", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.CustomsBroker, Descriptions.CustomsBroker);
				result.AddPair(Codes.Importer, Descriptions.Importer);
				return result;
			});
		}
	}
}
