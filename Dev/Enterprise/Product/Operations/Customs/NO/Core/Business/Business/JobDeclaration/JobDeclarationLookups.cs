using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
{
	public JobDeclarationLookups(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public new CodeDescriptionPairList LocationOfGoodsCollection => Factory.GetCachedValue<ImportGoodsLocationCodeList>();

	public override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<ShipmentTypeImport>();

	protected override CodeDescriptionPairList GetEntryPhaseStatusListCore => Factory.GetCachedValue<CustomsEntryPhaseStatusList>();

	public new CodeDescriptionPairList MessageTypeList
	{
		get
		{
			return Factory.GetCachedValue("NO.JobDeclarationsLookups.MessageTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Enterprise.Core.Constants.FreightShipmentDirection.Code.Import, Enterprise.Core.Constants.FreightShipmentDirection.Description.Import);
				result.AddPair(Enterprise.Core.Constants.FreightShipmentDirection.Code.Export, Enterprise.Core.Constants.FreightShipmentDirection.Description.Export);
				return result;
			});
		}
	}

	protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
	{
		yield return JobMessageTypeList.Codes.Drawback;
	}

	public override CodeDescriptionPairList CargoIdTypeList
	{
		get
		{
			var transportMode = Parent.JE_TransportMode;
			return Factory.GetCachedValue("NO.JobDeclarationsLookups.CargoIdTypeList" + transportMode, () =>
			{
				var result = FreightCodePairLists.JS_PackingModeList(transportMode);
				result.AddPairIfNotExist(Enterprise.Core.Constants.ContainerModes.Containerised, Enterprise.Core.Constants.ContainerModeDescriptions.Containerised);
				result.AddPairIfNotExist(Enterprise.Core.Constants.ContainerModes.NonContainerised, Enterprise.Core.Constants.ContainerModeDescriptions.NonContainerised);
				return result;
			});
		}
	}

	public override CodeDescriptionPairList MergeByList => Factory.GetCachedValue("NO.JobDeclarationsLookups.MergeByList", () =>
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(MergeInvoiceLinesConstants.Maximum, ResString.GetMultilingualString("07c66434-3cc3-44bd-91f4-84b8dfe85741", "Maximum"));
		list.AddPair(OrgConstants.MergeInvoiceLines.NotMerge, Res.GetString("5B71B9BB-D934-4A0E-BF86-9F0BC9BC8EA0", "No Merge"));
		return list;
	});

	public override ConsigneeCollection ImportersList
	{
		get
		{
			var result = base.ImportersList;
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Name", "Property", ZString.Empty));
			return result;
		}
	}

	public CodeDescriptionPairList NOCustomsTransportModeList
	{
		get
		{
			var transportMode = Parent.JE_TransportMode;
			return Factory.GetCachedValue($"599772B1-2D32-35AC-4913-3B3262633325-{transportMode}", () =>
			{
				var codePairs = transportMode.ToString() switch
				{
					Core.Constants.TransportModes.Sea => new[]
					{
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.SEA, NOCustomsTransportTypeList.Descriptions.SEA),
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.RWS, NOCustomsTransportTypeList.Descriptions.RWS),
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.TRS, NOCustomsTransportTypeList.Descriptions.TRS),
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.HAS, NOCustomsTransportTypeList.Descriptions.HAS)
					},
					Core.Constants.TransportModes.Rail => new[]
					{
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.RAI, NOCustomsTransportTypeList.Descriptions.RAI),
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.THR, NOCustomsTransportTypeList.Descriptions.THR)
					},
					Core.Constants.TransportModes.Road => new[]
					{
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.ROA, NOCustomsTransportTypeList.Descriptions.ROA)
					},
					Core.Constants.TransportModes.Air => new[]
					{
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.AIR, NOCustomsTransportTypeList.Descriptions.AIR)
					},
					Core.Constants.TransportModes.Mail => new[]
					{
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.MAI, NOCustomsTransportTypeList.Descriptions.MAI)
					},
					Core.Constants.TransportModes.FixedTransportInstallations => new[]
					{
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.FIX, NOCustomsTransportTypeList.Descriptions.FIX)
					},
					Core.Constants.TransportModes.InlandWaterwayTransport => new[]
					{
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.IWT, NOCustomsTransportTypeList.Descriptions.IWT)
					},
					Core.Constants.TransportModes.OwnPropulsion => new[]
					{
						new CodeDescriptionPair(NOCustomsTransportTypeList.Codes.OWN, NOCustomsTransportTypeList.Descriptions.OWN)
					},
					_ => Array.Empty<CodeDescriptionPair>()
				};
				var result = new CodeDescriptionPairList();
				result.AddRange(codePairs);
				result.DefaultCode = result.Count > 0 ? result[0].Code : ZString.Empty;
				return result;
			});
		}
	}
}
