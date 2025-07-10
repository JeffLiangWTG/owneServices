using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.NO.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NO.NCTS.Business.Arrival;
using Enterprise.MasterFiles.Business;
using CusGoodsLocation = Enterprise.Customs.EU.NCTS.Business.CusGoodsLocation;

namespace Enterprise.Customs.NO.NCTS.Business.MessageSending.Arrival;

sealed class CC007CTypeAdditionalDataProvider : ICC007CTypeAdditionalDataProvider
{
	IReadOnlyCollection<ExtensionTypeDataProviderAbstractClass> ICC007CTypeAdditionalDataProvider.GetExtensions(NctsHeaderMessageSendingObject messageSendingObject)
		=> Array.Empty<ExtensionTypeDataProviderAbstractClass>();

	string ICC007CTypeAdditionalDataProvider.GetSimplifiedProcedure(NctsHeader nctsHeader)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));

		return (nctsHeader.CusAuthorizationUsages.FirstOrDefault()?.AGC_Code.IsEmpty ?? true)
			? Constants.YesNoBitConstants.No
			: Constants.YesNoBitConstants.Yes;
	}

	string ICC007CTypeAdditionalDataProvider.GetIdentificationNumber(JobDocAddress jobDocAddress)
	{
		Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
		return jobDocAddress.Organisation?.GetEoriDetails();
	}

	EconomicOperatorType03DataProviderAbstractClass ICC007CTypeAdditionalDataProvider.GetEconomicOperator(CusGoodsLocation goodsLocation)
		=> GetEconomicOperatorCore(goodsLocation);

	string ICC007CTypeAdditionalDataProvider.GetNumberOfSeals(NctsContainer container)
		=> GetSeals(container).Count().ToString(CultureInfo.InvariantCulture);

	IReadOnlyCollection<SealType05DataProviderAbstractClass> ICC007CTypeAdditionalDataProvider.GetSeal(NctsContainer container)
	{
		var seals = GetSeals(container);

		return seals.Select(s => new SealTypeDataProvider(s))
			.Cast<SealType05DataProviderAbstractClass>()
			.ToList()
			.AsReadOnly();
	}

	string ICC007CTypeAdditionalDataProvider.GetContainerIndicator(EnRouteIncident incident)
		=> HasContainer(incident)
			? Constants.YesNoBitConstants.Yes
			: Constants.YesNoBitConstants.No;

	static EconomicOperatorType03DataProviderAbstractClass GetEconomicOperatorCore(CusGoodsLocation goodsLocation)
	{
		Argument.NotNull(goodsLocation, nameof(goodsLocation));

		string qualifier = goodsLocation.CGL_Qualifier;
		return qualifier.Equals(Customs.Business.CusGoodsLocationQualifierList.Codes.EoriNumber, StringComparison.InvariantCultureIgnoreCase)
			? CreateEconomicOperatorDataProvider()
			: null;

		EconomicOperatorType03DataProviderAbstractClass CreateEconomicOperatorDataProvider()
		{
			var cusGoodsLocationAddress = goodsLocation.Address;
			var govRegNumber = cusGoodsLocationAddress.E2_GovRegNum;
			var identificationNumber = govRegNumber.IsEmpty
				? cusGoodsLocationAddress.Organisation.GetEoriDetails()
				: govRegNumber;

			return new EconomicOperatorTypeDataProvider(identificationNumber);
		}
	}

	static IEnumerable<ZString> GetSeals(NctsContainer container)
	{
		Argument.NotNull(container, nameof(container));

		var seals = new[] { container.BC_Seal1, container.BC_Seal2, };
		foreach (var seal in seals.Union(container.Seals.Select(s => s.BK_SealNumber)))
		{
			if (!seal.IsEmpty)
			{
				yield return seal;
			}
		}
	}

	static bool HasContainer(EnRouteIncident incident)
	{
		Argument.NotNull(incident, nameof(incident));

		var incidentContainers = incident.IncidentContainers;
		return incidentContainers.Count > 0 && incidentContainers.Any(i => i.BC_Mode.EqualsIgnoringCase(Core.Constants.ContainerModes.Containerised));
	}
}
