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

public class CC013CProvider : CCProviderBase, ICC013C
{
	public CC013CProvider(NctsDepartureMovementHeader movementHeader, string messageType, MessageSendingObject messageSendingObject)
		: base(movementHeader, messageType)
	{
		this.movementHeader = movementHeader;
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;

	public IExtendedTransitOperation TransitOperation => transitOperation ??= new CC013CTransitOperationProvider(movementHeader, messageSendingObject);
	IExtendedTransitOperation transitOperation;

	public ICustomsOffice CustomsOfficeOfDeparture => CachedValueHelper.GetValue(ref customsOfficeOfDeparture, () => CustomsOfficeProvider.NewOrNull(GetCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)));
	CachedValue<ICustomsOffice> customsOfficeOfDeparture;

	public ICustomsOffice CustomsOfficeOfDestinationDeclared => CachedValueHelper.GetValue(ref customsOfficeOfDestinationDeclared, () => CustomsOfficeProvider.NewOrNull(GetCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination)));
	CachedValue<ICustomsOffice> customsOfficeOfDestinationDeclared;

	public IReadOnlyCollection<ICustomsOfficeOfTransit> CustomsOfficeOfTransitDeclared => customsOfficeOfTransitDeclared ??= GetCustomsOfficeOfTransitDeclared();
	IReadOnlyCollection<ICustomsOfficeOfTransit> customsOfficeOfTransitDeclared;

	public IReadOnlyCollection<INumberedCustomsOffice> CustomsOfficeOfExitForTransitDeclared => customsOfficeOfExitForTransitDeclared ??= GetCustomsOfficeOfExitForTransitDeclared();
	IReadOnlyCollection<INumberedCustomsOffice> customsOfficeOfExitForTransitDeclared;

	public IHolderOfTheTransitProcedureWithMaxLength HolderOfTheTransitProcedure => holderOfTheTransitProcedure ??= new HolderOfTheTransitProcedureProvider(nctsHeader.Principal, movementHeader);
	IHolderOfTheTransitProcedureWithMaxLength holderOfTheTransitProcedure;

	public IRepresentative Representative => representative ??= new RepresentativeProvider(movementHeader.Representative, HolderOfTheTransitProcedure);
	IRepresentative representative;

	public IReadOnlyCollection<IGuarantee> Guarantee => guarantee ??= GetGuarantee();
	IReadOnlyCollection<IGuarantee> guarantee;

	public IConsignment Consignment => consignment ??= new CC013CConsignmentProvider(movementHeader, this);
	IConsignment consignment;

	public PhaseID? PhaseID => null;

	bool InPhase5TransitionPeriod => CachedValueHelper.GetValue(ref notInPhase5TransitionPeriod, () => movementHeader.IsInPhase5TransitionPeriod);
	CachedValue<bool> notInPhase5TransitionPeriod;

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
