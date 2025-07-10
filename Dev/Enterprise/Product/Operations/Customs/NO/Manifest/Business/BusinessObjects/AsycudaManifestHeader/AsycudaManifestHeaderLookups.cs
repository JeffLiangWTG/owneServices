using System.Collections;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Manifest.Business;

public sealed class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
{
	public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
	{
	}

	public ICollection TransportMeansCodeList => GetTransportMeansCodeList();

	ICollection GetTransportMeansCodeList()
	{
		var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(
			Factory,
			Core.Constants.CountryCodes.Norway,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NOTransportationMeans,
			ZDateTime.Now);

		result.FilterBusinessObjectDefaults.Add(
			new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.Code, "Property", GetFirstCharInCodeValueFromTransportMode()));

		return result;
	}

	ZString GetFirstCharInCodeValueFromTransportMode()
	{
		var parent = Parent;
		var transportMeans = parent.AMA_TransportMeans;
		if (!transportMeans.IsEmpty)
		{
			return transportMeans;
		}

		return parent.AMA_TransportMode.ToString() switch
		{
			Core.Constants.TransportModes.Sea => "1",
			Core.Constants.TransportModes.Rail => "2",
			Core.Constants.TransportModes.Road => "3",
			Core.Constants.TransportModes.Air => "4",
			_ => ZString.Empty,
		};
	}

	public override CodeDescriptionPairList TransportModeList => Factory.GetValue(ref transportModesListCached, GetTransportModesCodeList);
	CachedProperty<CodeDescriptionPairList> transportModesListCached;

	CodeDescriptionPairList GetTransportModesCodeList()
	{
		var result = new CodeDescriptionPairList();
		var allPossibleModes = Factory.GetCachedValue<TransportTypeList>();
		foreach (var mode in ValidTransportModes)
		{
			result.AddPair(mode, allPossibleModes.GetDescriptionFromCode(mode) ?? mode);
		}
		return result;
	}

	static readonly ImmutableArray<string> ValidTransportModes = ImmutableArray.Create(
	TransportTypeList.Codes.Road,
	TransportTypeList.Codes.Sea,
	TransportTypeList.Codes.Air,
	TransportTypeList.Codes.Rail);
}
