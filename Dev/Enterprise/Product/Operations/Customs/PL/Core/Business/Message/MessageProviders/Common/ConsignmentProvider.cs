using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.PL.Business;

public class ConsignmentProvider : IConsignment
{
	public ConsignmentProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, $"{nameof(entryHeader)}.{nameof(entryHeader.Declaration)}");
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, $"{nameof(entryHeader)}.{nameof(entryHeader.EntryInstruction)}");
	}

	protected readonly CusEntryHeader entryHeader;
	protected readonly JobDeclaration declaration;
	protected readonly CusEntryInstruction entryInstruction;

	public int? ContainerIndicator => CachedValueHelper.GetValue(ref containerIndicator,
		() => !CheckC0836() ? ContainerIndicatorValue : null);
	CachedValue<int?> containerIndicator;

	public string InlandModeOfTransport => CachedValueHelper.GetValue(ref inlandModeOfTransport,
		() => !CheckC0843() ? InlandModeOfTransportValue : null);
	CachedValue<string> inlandModeOfTransport;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments
	{
		get
		{
			if (transportEquipments == null)
			{
				var transportEquipmentsList = new List<ITransportEquipment>();
				var sequenceNumber = 0;

				var tranportEquipment = declaration.Equipments;
				var containerIndicatorExists = containerIndicator != null;
				transportEquipmentsList.AddRange(declaration.Equipments
					.Where(x => x.Seals.Count > 0)
					.Select(x => new AESTransportEquipmentProvider(sequenceNumber += 1, entryInstruction, x, containerIndicatorExists)));

				var containers = entryHeader.InvoiceLines.SelectMany(x => x.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().Select(y => y.Container)).ToList();
				if (containers.Count > 0)
				{
					var uniqueContainers = containers.Distinct().ToList();
					var everyInvoiceLineInSameContainer = uniqueContainers.Count == 1 && containers.Count == entryHeader.InvoiceLines.Count();

					transportEquipmentsList.AddRange(uniqueContainers.Select(x =>
						new AESTransportEquipmentContainerProvider(sequenceNumber += 1, x, everyInvoiceLineInSameContainer)));
				}

				transportEquipments = transportEquipmentsList;
			}

			return transportEquipments;
		}
	}
	IReadOnlyCollection<ITransportEquipment> transportEquipments;

	public ILocationOfGoods LocationOfGoods => locationOfGoods ??= new AESLocationOfGoodsProvider(entryInstruction.GoodsLocation);
	ILocationOfGoods locationOfGoods;

	public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans
	{
		get
		{
			if (departureTransportMeans == null)
			{
				departureTransportMeans = !CheckC0834()
					? new[] { new AESDepartureTransportMeansProvider(declaration, 1) }
					: Array.Empty<IDepartureTransportMeans>();
			}
			return departureTransportMeans;
		}
	}
	IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

	protected bool IsSubStyleBOrCOrEOrF()
	{
		var subStyle = entryInstruction.CEI_SubStyle;
		return subStyle == SubStyleCodes.B
				|| subStyle == SubStyleCodes.C
				|| subStyle == SubStyleCodes.E
				|| subStyle == SubStyleCodes.F;
	}

	int? ContainerIndicatorValue
	{
		get
		{
			var containerMode = declaration.JE_ContainerMode;
			return containerMode.IsEmpty
				? null
				: containerMode == Core.Constants.ContainerModes.LCL
				|| containerMode == Core.Constants.ContainerModes.FCL
				|| containerMode == Core.Constants.ContainerModes.ULD
				|| containerMode == Core.Constants.ContainerModes.Containerised
					? 1 : 0;
		}
	}

	string InlandModeOfTransportValue => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland);

	bool CheckC0836() => IsSubStyleBOrCOrEOrF();

	bool CheckC0834()
	{
		return declaration.JE_TransportModeInland.IsEmpty
				|| ((declaration.IsMailInland || declaration.IsFixedInstallationInland)
					&& (entryInstruction.CEI_Procedure == ProcedureCodes._21 || entryInstruction.CEI_Procedure == ProcedureCodes._22));
	}

	bool CheckC0843()
	{
		var customsOfficeOfPresentationReferenceNumber = declaration.CustomsOfficeOfPresentationReferenceNumber();
		var customsOfficeOfExitDeclaredReferenceNumber = declaration.JE_OfficeOfEntryExit;
		var customsOfficeOfExportReferenceNumber = declaration.JE_CustomsOffice;

		return customsOfficeOfPresentationReferenceNumber.IsEmpty
			? CheckC0843AgainstOfficeOfExport(customsOfficeOfExportReferenceNumber, customsOfficeOfExitDeclaredReferenceNumber)
			: CheckC0843AgainstOfficeOfPresentation(customsOfficeOfPresentationReferenceNumber, customsOfficeOfExitDeclaredReferenceNumber);
	}

	bool CheckC0843AgainstOfficeOfPresentation(ZString customsOfficeOfPresentation, ZString customsOfficeOfExitDeclared)
	{
		return customsOfficeOfPresentation.Equals(customsOfficeOfExitDeclared)
				|| IsC0843EntrySubStyleOrMessageSubTypeAndProcedure();
	}

	bool CheckC0843AgainstOfficeOfExport(ZString customsOfficeOfExport, ZString customsOfficeOfExitDeclared)
	{
		return customsOfficeOfExport.Equals(customsOfficeOfExitDeclared)
				|| IsC0843EntrySubStyleOrMessageSubTypeAndProcedure();
	}

	bool IsC0843EntrySubStyleOrMessageSubTypeAndProcedure() => IsSubStyleBOrCOrEOrF()
																|| (declaration.JE_MessageSubType == EntryStyleListExport.Codes.ExportToSpecialTerritory
																	&& entryInstruction.CEI_Procedure == Constants.ProcedureCodes._10);
}
