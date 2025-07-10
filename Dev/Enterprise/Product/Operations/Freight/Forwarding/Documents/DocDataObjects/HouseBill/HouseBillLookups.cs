using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.Freight.Forwarding.Documents.DataObjects.ResString;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class HouseBillLookups
	{
		public HouseBillLookups(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		#region SuppressResourceStringsCheckRegion 

		const string unlocosLookupKey = "DocDataObjects.Lookups.Unlocos";
		const string countriesLookupKey = "DocDataObjects.Lookups.Countries";
		const string currienciesLookupKey = "DocDataObjects.Lookups.Curriencies";

		public IRefUNLOCOCollection Unlocos => factory.GetCachedValue<IRefUNLOCOCollection>(unlocosLookupKey, () => new RefUNLOCOCollection(factory));

		public IRefCountryCollection Countries => factory.GetCachedValue<IRefCountryCollection>(countriesLookupKey, () => new RefCountryCollection(factory));

		public IFindBoxListProvider Currencies => factory.GetCachedValue<IFindBoxListProvider>(currienciesLookupKey, () => new RefCurrencyCollection(factory));

		public ICodeDescriptionPairList PaymentTerms
		{
			get
			{
				if (paymentTerms == null)
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.PaymentType.Prepaid, "Freight Prepaid");
					list.AddPair(Core.Constants.PaymentType.Collect, "Freight Collect");
					paymentTerms = list;
				}

				return paymentTerms;
			}
		}
		ICodeDescriptionPairList paymentTerms;

		public ICodeDescriptionPairList ReleaseTypes => releaseTypes ?? (releaseTypes = FreightDataRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList());
		ICodeDescriptionPairList releaseTypes;

		public CodeDescriptionPairList AsAgentOptions
		{
			get
			{
				if (asAgentOptions == null)
				{
					asAgentOptions = new CodeDescriptionPairList();
					asAgentOptions.AddPair(AsAgentOption.AsCarrier, ResString.GetMultilingualString("f8a73b79-eb4c-46cc-b3f6-b91fe9ee1f3b", "AS CARRIER"));
					asAgentOptions.AddPair(AsAgentOption.AsAgent, ResString.GetMultilingualString("c4eb13ff-8132-41d6-94fe-2b5e5a35daa6", "AS AGENT"));
					asAgentOptions.AddPair(AsAgentOption.AsAgentForCarrier, ResString.GetMultilingualString("d9d32bf6-a4f9-4d9e-9701-12f7c6f28aaa", "AS AGENT FOR CARRIER"));
					asAgentOptions.AddPair(AsAgentOption.AsAgentFor, ResString.GetMultilingualString("fe4e0e5a-46e7-4b33-9daf-c730fabc3041", "AS AGENT FOR"));
					asAgentOptions.DefaultCode = AsAgentOption.AsCarrier;
				}

				return asAgentOptions;
			}
		}
		CodeDescriptionPairList asAgentOptions;

		public static class AsAgentOption
		{
			public const string AsCarrier = "CRR";
			public const string AsAgent = "AGT";
			public const string AsAgentForCarrier = "AGC";
			public const string AsAgentFor = "AGF";
		}

		#endregion
	}
}
