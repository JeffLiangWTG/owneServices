using System;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DistanceCalculationLists
	{
		DistanceCalculationLists() { }

		#region Instance

		public static DistanceCalculationLists Instance
		{
			get { return fInstance ?? (fInstance = new DistanceCalculationLists()); }
		}

		[ThreadStatic]
		static DistanceCalculationLists fInstance;

		#endregion

		public CodeDescriptionPairList Providers
		{
			get
			{
				if (fProviders == null)
				{
					fProviders = new CodeDescriptionPairList();
					fProviders.AddPair(DistanceCalculationConstants.Providers.CargoWise, ResString.GetMultilingualString("7824814f-8df5-4abb-aabd-1de60011940b", "CargoWise Service to choose most appropriate service(s)"));
					fProviders.AddPair(DistanceCalculationConstants.Providers.PCMiler, ResString.GetMultilingualString("b3dbf42e-3ce4-41b7-8cd8-e324f8c5fc31", "PC*Miler"));
					fProviders.DefaultCode = DistanceCalculationConstants.Providers.CargoWise;
				}
				return fProviders;
			}
		}
		CodeDescriptionPairList fProviders;

		public CodeDescriptionPairList Versions(string provider)
		{
			if (provider == DistanceCalculationConstants.Providers.PCMiler)
			{
				return DistanceCalculationLists.Instance.PCMilerVersions;
			}
			else
			{
				return EmptyList;
			}
		}

		public CodeDescriptionPairList CalculationMethods(string provider)
		{
			if (provider == DistanceCalculationConstants.Providers.PCMiler)
			{
				return DistanceCalculationLists.Instance.PCMilerCalculationMethods;
			}
			else
			{
				return EmptyList;
			}
		}

		CodeDescriptionPairList EmptyList
		{
			get { return fEmptyList ?? (fEmptyList = new CodeDescriptionPairList()); }
		}
		CodeDescriptionPairList fEmptyList;

		readonly MultilingualString versionString = ResString.GetMultilingualString("c1e0d220-1de9-44c9-96f0-5f3b8e99a258", "Version");

		public CodeDescriptionPairList PCMilerVersions
		{
			get
			{
				if (fPCMilerVersions == null)
				{
					fPCMilerVersions = new CodeDescriptionPairList();
					fPCMilerVersions.AddPair(DistanceCalculationConstants.ProviderVersions.PCMiler.Current, Res.GetString("02ec0362-5ab1-4987-90ea-ad50b6e2d175", "Current Version"));
					fPCMilerVersions.AddPair(DistanceCalculationConstants.ProviderVersions.PCMiler.v18, MultilingualString.Join(" ", versionString, (NoResString)DistanceCalculationConstants.ProviderVersions.PCMiler.v18));
					fPCMilerVersions.AddPair(DistanceCalculationConstants.ProviderVersions.PCMiler.v19, MultilingualString.Join(" ", versionString, (NoResString)DistanceCalculationConstants.ProviderVersions.PCMiler.v19));
					fPCMilerVersions.AddPair(DistanceCalculationConstants.ProviderVersions.PCMiler.v20, MultilingualString.Join(" ", versionString, (NoResString)DistanceCalculationConstants.ProviderVersions.PCMiler.v20));
					fPCMilerVersions.AddPair(DistanceCalculationConstants.ProviderVersions.PCMiler.v21, MultilingualString.Join(" ", versionString, (NoResString)DistanceCalculationConstants.ProviderVersions.PCMiler.v21));
					fPCMilerVersions.AddPair(DistanceCalculationConstants.ProviderVersions.PCMiler.v22, MultilingualString.Join(" ", versionString, (NoResString)DistanceCalculationConstants.ProviderVersions.PCMiler.v22));
					fPCMilerVersions.AddPair(DistanceCalculationConstants.ProviderVersions.PCMiler.v23, MultilingualString.Join(" ", versionString, (NoResString)DistanceCalculationConstants.ProviderVersions.PCMiler.v23));
					fPCMilerVersions.AddPair(DistanceCalculationConstants.ProviderVersions.PCMiler.v24, MultilingualString.Join(" ", versionString, (NoResString)DistanceCalculationConstants.ProviderVersions.PCMiler.v24));
					fPCMilerVersions.DefaultCode = DistanceCalculationConstants.ProviderVersions.PCMiler.Current;
				}
				return fPCMilerVersions;
			}
		}
		CodeDescriptionPairList fPCMilerVersions;

		public CodeDescriptionPairList PCMilerCalculationMethods
		{
			get
			{
				if (fPCMilerCalculationMethods == null)
				{
					fPCMilerCalculationMethods = new CodeDescriptionPairList();
					fPCMilerCalculationMethods.AddPair(DistanceCalculationConstants.CalculationMethods.PCMiler.Practical, Res.GetString("59920b58-d928-4ed7-a937-98cd1451f5dd", "Practical"));
					fPCMilerCalculationMethods.AddPair(DistanceCalculationConstants.CalculationMethods.PCMiler.Shortest, Res.GetString("2f9dd88a-c128-4e6f-8619-02dd28fac817", "Shortest"));
					fPCMilerCalculationMethods.DefaultCode = DistanceCalculationConstants.CalculationMethods.PCMiler.Practical;
				}
				return fPCMilerCalculationMethods;
			}
		}
		CodeDescriptionPairList fPCMilerCalculationMethods;
	}
}
