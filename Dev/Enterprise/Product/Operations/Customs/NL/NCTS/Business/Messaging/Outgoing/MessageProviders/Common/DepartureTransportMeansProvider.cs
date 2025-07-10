using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.NCTS.Business;

public abstract class DepartureTransportMeansProvider : INCTSDepartureTransportMeans
{
	readonly List<Func<int?>> typeOfIdentificationRules = new();

	public DepartureTransportMeansProvider(NctsCommonMovementHeader moveHeader, int sequenceNumeric)
	{
		this.moveHeader = Argument.NotNull(moveHeader, nameof(moveHeader));
		SequenceNumeric = sequenceNumeric;
	}
	protected readonly NctsCommonMovementHeader moveHeader;

	public int SequenceNumeric { get; }

	public int? TypeOfIdentification => GetTypeOfIdentification();

	public virtual string Id => moveHeader.BM_TransportAtDeparture;

	public abstract string Nationality { get; }

	protected virtual bool IsMainTransportID => true;

	protected void InitializeTypeOfIdentificationRules(params Func<int?>[] rules) => typeOfIdentificationRules.AddRange(rules);

	protected int? SeaTransportRule()
	{
		return moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._1_SeaTransport ? 11 : null;
	}

	protected int? SeaTransportWithVesselRule()
		=> moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._1_SeaTransport
		&& moveHeader.Factory.Exists(typeof(RefVessel), new ZQuery(RefVesselSchema.RV_LloydsNumber, moveHeader.BM_TransportAtDeparture))
		? 10
		: null;

	protected int? RailTransportRule()
	{
		if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport)
		{
			return IsMainTransportID ? 21 : 20;
		}

		return null;
	}

	protected int? RoadTransportRule()
	{
		if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport)
		{
			return IsMainTransportID ? 30 : 31;
		}

		return null;
	}

	protected int? AirTransportRule()
	{
		if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._4_AirTransport)
		{
			return IsMainTransportID ? 40 : 41;
		}

		return null;
	}

	protected int? InlandWaterwayTransportRule() => moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._8_InlandWaterwayTransport
			? 81
			: null;

	protected int? InlandWaterwayTransportWithVesselRule()
		=> moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._8_InlandWaterwayTransport
		&& moveHeader.Factory.Exists(typeof(RefVessel), new ZQuery(RefVesselSchema.RV_LloydsNumber, moveHeader.BM_TransportAtDeparture))
		? 80
		: null;

	protected int? OwnPropulsionRule()
	{
		if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._9_OwnPropulsion)
		{
			return int.TryParse(moveHeader.BM_TransportAtDepartureType, out var value) ? value : 0;
		}

		return null;
	}

	int? GetTypeOfIdentification()
	{
		foreach (var rule in typeOfIdentificationRules)
		{
			var result = rule();
			if (result.HasValue)
			{
				return result.Value;
			}
		}

		return null;
	}
}
