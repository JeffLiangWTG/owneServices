using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using ICustomsOffice = CargoWise.Customs.PL.MessageContracts.Interfaces.ICustomsOffice;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC015CProvider : CCProviderBase, ICC015C
{
	public CC015CProvider(NctsDepartureMovementHeader movementHeader, string messageType)
		: base(Argument.NotNull(movementHeader, nameof(movementHeader)), Argument.NotNull(messageType, nameof(messageType)))
	{
		this.movementHeader = movementHeader;
	}

	readonly NctsDepartureMovementHeader movementHeader;

	public ITransitOperation TransitOperation => transitOperation ??= new CC015CTransitOperationProvider(movementHeader);
	ITransitOperation transitOperation;

	public ICustomsOffice CustomsOfficeOfDeparture => CachedValueHelper.GetValue(ref customsOfficeOfDeparture, () => CustomsOfficeProvider.NewOrNull(GetCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)));
	CachedValue<ICustomsOffice> customsOfficeOfDeparture;

	public ICustomsOffice CustomsOfficeOfDestinationDeclared => CachedValueHelper.GetValue(ref customsOfficeOfDestinationDeclared, () => CustomsOfficeProvider.NewOrNull(GetCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination)));
	CachedValue<ICustomsOffice> customsOfficeOfDestinationDeclared;

	public IReadOnlyCollection<ICustomsOfficeOfTransit> CustomsOfficeOfTransitDeclared => customsOfficeOfTransitDeclared ?? (customsOfficeOfTransitDeclared = GetCustomsOfficeOfTransitDeclared());
	IReadOnlyCollection<ICustomsOfficeOfTransit> customsOfficeOfTransitDeclared;

	public IReadOnlyCollection<INumberedCustomsOffice> CustomsOfficeOfExitForTransitDeclared => customsOfficeOfExitForTransitDeclared ?? (customsOfficeOfExitForTransitDeclared = GetCustomsOfficeOfExitForTransitDeclared());
	IReadOnlyCollection<INumberedCustomsOffice> customsOfficeOfExitForTransitDeclared;

	public IHolderOfTheTransitProcedureWithMaxLength HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = new HolderOfTheTransitProcedureProvider(nctsHeader.Principal, movementHeader));
	IHolderOfTheTransitProcedureWithMaxLength holderOfTheTransitProcedure;

	public IRepresentative Representative => representative ?? (representative = new RepresentativeProvider(movementHeader.Representative, HolderOfTheTransitProcedure));
	IRepresentative representative;

	public IReadOnlyCollection<IGuarantee> Guarantee => guarantee ?? (guarantee = GetGuarantee());
	IReadOnlyCollection<IGuarantee> guarantee;

	public IConsignment Consignment => consignment ?? (consignment = new CC015CConsignmentProvider(movementHeader, this));
	IConsignment consignment;

	public PhaseID? PhaseID => CachedValueHelper.GetValue(ref phaseId, () => InPhase5TransitionPeriod
		? CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_0
		: CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_1);
	CachedValue<PhaseID?> phaseId;

	bool InPhase5TransitionPeriod => CachedValueHelper.GetValue(ref inPhase5TransitionPeriod, () => movementHeader.IsInPhase5TransitionPeriod);
	CachedValue<bool> inPhase5TransitionPeriod;

	NctsPLOfficeCode GetCustomsOfficeOfType(ZString officeCode) => nctsHeader.MovementHeader.CustomsOffices.Cast<NctsPLOfficeCode>().FirstOrDefault(x => x.CY_Code == officeCode);

	IReadOnlyCollection<ICustomsOfficeOfTransit> GetCustomsOfficeOfTransitDeclared() => !CheckRuleB1836() && !CheckRuleC0030()
		? nctsHeader.MovementHeader.CustomsOffices
			.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit)
			.Cast<NctsPLOfficeCode>()
			.Select((x, i) => new CustomsOfficeOfTransitProvider(i + 1, x))
			.ToArray()
		: Array.Empty<ICustomsOfficeOfTransit>();

	bool CheckRuleB1836() => InPhase5TransitionPeriod
							&& movementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR;

	bool CheckRuleC0030()
	{
		var bondEntryType = movementHeader.BM_InBondEntryType;
		return !InPhase5TransitionPeriod
				&& (bondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR
					|| bondEntryType == NctsPhase5DeclarationTypeList.Codes.T2SM);
	}

	IReadOnlyCollection<INumberedCustomsOffice> GetCustomsOfficeOfExitForTransitDeclared() => CheckRuleC0587()
		? nctsHeader.MovementHeader.CustomsOffices
			.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit && !x.CY_Data.IsEmpty)
			.Cast<NctsPLOfficeCode>()
			.Select((x, i) => new CustomsOfficeOfTransitProvider(i + 1, x))
			.ToArray()
		: Array.Empty<INumberedCustomsOffice>();

	bool CheckRuleC0587()
	{
		var securityType = movementHeader.BM_TypeOfSecurity;
		return securityType == NctsTypeOfSecurityList.Codes.EXI
				|| securityType == NctsTypeOfSecurityList.Codes.BTH;
	}

	IReadOnlyCollection<IGuarantee> GetGuarantee() => nctsHeader.MovementHeader.Guarantees
		.GroupBy(x => new { x.PW_BondType, x.PW_BondNumber2 })
		.Select((x, i) => new GuaranteeProvider(i + 1, x.ToArray()))
		.ToArray();
}
