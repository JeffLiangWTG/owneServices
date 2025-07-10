#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI;

public class DataMappingUrlHandler : UrlHandler
{
	public static DataMappingUrlHandler Instance => instance ??= new();
	[ThreadStatic]
	static DataMappingUrlHandler? instance;

	const string MappingType = "MappingType";

	readonly Dictionary<string, IDataMappingHandler> handlers = new()
	{
		{ SCACMappingHandler.MappingType, new SCACMappingHandler() },
		{ IATAMappingHandler.MappingType, new IATAMappingHandler() },
		{ CommodityGroupMappingHandler.MappingType, new CommodityGroupMappingHandler() },
		{ ViewCommodityGroupMappingsHandler.MappingType, new ViewCommodityGroupMappingsHandler() },
		{ UniversalChargeCodeMappingHandler.MappingType, new UniversalChargeCodeMappingHandler() },
		{ UniversalServiceLevelMappingHandler.MappingType, new UniversalServiceLevelMappingHandler() },
		{ ContainerCodeMappingHandler.MappingType, new ContainerCodeMappingHandler() }
	};

	protected override string ExpectedCommandText => "MapData";

	protected override bool HandleCore(QueryString queryString) => GetCheckedDataMappingHandler(queryString)?.Handle(queryString) ?? false;

	IDataMappingHandler? GetCheckedDataMappingHandler(QueryString queryString)
	{
		if (!handlers.TryGetValue(queryString[MappingType] ?? "", out var handler))
		{
			return null;
		}

		var paramsNotInQueryString = handler.RequiredParams.Where(param => string.IsNullOrEmpty(queryString[param])).ToList();
		if (paramsNotInQueryString.Count > 0)
		{
			throw new InvalidQueryStringException($"The following required parameters for mapping of '{queryString[MappingType]}' are missing: {string.Join(", ", paramsNotInQueryString)}");
		}

		return handler;
	}
}

#region Data Mapping Handlers

interface IDataMappingHandler
{
	bool Handle(QueryString queryString);
	List<string> RequiredParams { get; }
}

class SCACMappingHandler : IDataMappingHandler
{
	public static string MappingType => "SCAC";

	public List<string> RequiredParams { get; } = ["SCAC"];

	public bool Handle(QueryString queryString)
	{
		new AssignCarrierCodeCommand().Assign(queryString["SCAC"], string.Empty);
		return true;
	}
}

class IATAMappingHandler : IDataMappingHandler
{
	public static string MappingType => "IATA";

	public List<string> RequiredParams { get; } = ["IATA"];

	public bool Handle(QueryString queryString)
	{
		CreateAirlineAndAssignIATACodeCommand.AssignWithUserConfirmation(queryString["IATA"], null);
		return true;
	}
}

class UniversalChargeCodeMappingHandler : IDataMappingHandler
{
	public static string MappingType => "UniversalChargeCode";

	public List<string> RequiredParams { get; } = ["UniversalChargeCode", (NoResString)"Global"];

	public bool Handle(QueryString queryString)
	{
		if (!bool.TryParse(queryString["Global"], out var isGlobal))
		{
			throw new InvalidQueryStringException($"Expected parameter 'Global' to be bool but got {queryString["Global"]}.");
		}
		new AssignUniversalChargeCodeCommand(isGlobal).Assign(queryString["UniversalChargeCode"]);
		return true;
	}
}

class CommodityGroupMappingHandler : IDataMappingHandler
{
	public static string MappingType => "UniversalCommodityGroup";

	public List<string> RequiredParams { get; } = ["UniversalCommodityGroup"];

	public bool Handle(QueryString queryString)
	{
		new AssignUniversalCommodityGroupCommand() { ShowConfirmation = true }.Assign(queryString["UniversalCommodityGroup"]);
		return true;
	}
}

class ViewCommodityGroupMappingsHandler : IDataMappingHandler
{
	public static string MappingType => "ViewCommodities";

	public List<string> RequiredParams { get; } = ["UniversalCommodityGroup"];

	public bool Handle(QueryString queryString)
	{
		var commodityGroupViewModelCollection = GetCommodityGroupViewModelCollection(queryString["UniversalCommodityGroup"]!);
		ZFormModaliser.ShowDialogAndDispose(new CommodityGroupForm(commodityGroupViewModelCollection));
		return true;
	}

	public CommodityGroupViewModelCollection GetCommodityGroupViewModelCollection(string commodityGroup)
	{
		var commodityGroupViewModelCollection = new CommodityGroupViewModelCollection();
		var factory = new BusinessObjectFactory();
		var commodities = RefCommodityCode.GetCommodities(factory, commodityGroup);
		var universalCommodityGroups = RefCommodityCodeLookups.GetUniversalCommodityGroupMapUnfilteredList();
		foreach (var commodity in commodities)
		{
			commodityGroupViewModelCollection.Add(
				universalGroup: commodityGroup,
				universalGroupDescription: universalCommodityGroups.GetDescriptionFromCode(commodityGroup),
				commodityCode: commodity.RH_Code,
				commodityDescription: commodity.RH_DescriptionMultilingual);
		}
		return commodityGroupViewModelCollection;
	}
}

class UniversalServiceLevelMappingHandler : IDataMappingHandler
{
	public static string MappingType => "ServiceLevel";

	public List<string> RequiredParams { get; } = ["UniversalServiceLevel", "OrgId"];

	public bool Handle(QueryString queryString)
	{
		if (!ZGuid.TryParse(queryString["OrgId"], out var orgPk))
		{
			throw new InvalidQueryStringException($"Invalid argument 'OrgId'. Expected Guid, got ${queryString["OrgId"]}");
		}

		var factory = new BusinessObjectFactory();
		var org = factory.Load<OrgHeader>(orgPk);
		if (org is null)
		{
			return false;
		}

		new AssignCarrierServiceLevelsCommand(new ZForm()).Assign(org, queryString["UniversalServiceLevel"]);
		return true;
	}
}

class ContainerCodeMappingHandler : IDataMappingHandler
{
	public static string MappingType => "ContainerCode";

	public List<string> RequiredParams { get; } = new() { "ContainerCode", "IsSeaMode" };

	public bool Handle(QueryString queryString)
	{
		if (!bool.TryParse(queryString["IsSeaMode"], out var isSeaMode))
		{
			throw new InvalidQueryStringException($"Invalid argument 'IsSeaMode'. Expected bool, got {queryString["IsSeaMode"]}.");
		}

		var containerCode = queryString["ContainerCode"];

		if (isSeaMode)
		{
			AssignContainerCodeCommand.AssignContainerCodeForSea(containerCode);
		}
		else
		{
			AssignContainerCodeCommand.AssignContainerCodeForAir(containerCode);
		}

		return true;
	}
}

#endregion
