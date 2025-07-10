using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Business.RequiredTaxNumbers;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public sealed class FreightLibrary : MacroLibrary
	{
		#region SuppressResourceStringsCheckRegion

		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<MacroClosure>>(
					"IsValidContainerNumber",
					"Returns true if given container number conforms with ISO standard.",
					() => new MacroClosure(scope => IsValidContainerNumber(scope)));

				yield return new Handler<Func<MacroClosure>>(
					"IsValidISOContainer",
					"Returns true if given container code corresponds to a valid ISO container type.",
					() => new MacroClosure(scope => IsValidISOContainer(scope)));

				yield return new Handler<Func<MacroClosure>>(
					"IsValidPackType",
					"Returns true if given container code corresponds to a valid pack type.",
					() => new MacroClosure(scope => IsValidPackType(scope)));

				yield return new Handler<Func<IDynamicData, IDynamicData>>(
					"GetDepartureConsol",
					"Return departure consol",
					(shipmentDataObject) => GetDepartureConsol(shipmentDataObject));

				yield return new Handler<Func<decimal, string, string, decimal>>(
					"GetRoundedValue",
					"Return rounded value",
					(originalValue, transportMode, unit) => GetRoundedValue(originalValue, transportMode, unit));

				yield return new Handler<Func<string, string, OrganizationAddress, string, string, string>>(
					"GetRequiredTaxInfo",
					"Get required tax type and number.",
					(sourceCountryCode, destinationCountryCode, org, orgType, docType) => GetRequiredTaxInfo(sourceCountryCode, destinationCountryCode, org, orgType, docType));

				yield return new Handler<Func<IDynamicData, string>>(
					"GetTaxNumberForChineseCustoms",
					"Returns the tax number that is required by Chinese Customs.",
					(organization) => GetTaxNumberForChineseCustoms(organization));

				yield return new Handler<Func<IDynamicData, string, string>>(
					"GetTaxNumberTypesForChineseCustoms",
					"Returns the tax number descriptions that are required by Chinese Customs.",
					(organization, separator) => GetTaxNumberTypesForChineseCustoms(organization, separator));
			}
		}

		#endregion

		#region IsValidContainerNumber

		bool IsValidContainerNumber(IMacroScope scope)
		{
			if (scope?.Data == null)
			{
				return false;
			}

			var containerNumber = System.Convert.ToString(scope.Data, CultureInfo.InvariantCulture);

			return ContainerNumberValidation.IsValidContainerNumber(containerNumber);
		}

		#endregion

		#region IsValidISOContainer

		bool IsValidISOContainer(IMacroScope scope)
		{
			if (scope == null || scope.Data == null)
			{
				return false;
			}

			var containerISOCode = System.Convert.ToString(scope.Data, CultureInfo.InvariantCulture);
			var isoType = new ContainerISOType();
			isoType.ISOCode = containerISOCode;

			return isoType.IsKnown;
		}

		#endregion

		#region IsValidPackType

		bool IsValidPackType(IMacroScope scope)
		{
			if (scope?.Data == null)
			{
				return false;
			}

			var packType = System.Convert.ToString(scope.Data, CultureInfo.InvariantCulture);

			return PackTypes.ContainsCode(packType);
		}

		CodeDescriptionPairList PackTypes
		{
			get
			{
				if (packTypes == null)
				{
					var packTypeList = new RefPackTypeCollection(new ReadOnlyBusinessObjectFactory())
					{
						AdditionalFilter = new ZQuery(RefPackTypeSchema.F3_IsSystem, true)
					};

					packTypes = packTypeList.GetAsCodeDescriptionPair();
				}

				return packTypes;
			}
		}

		CodeDescriptionPairList packTypes;

		#endregion

		#region GetDepartureConsol
		#region SuppressResourceStringsCheckRegion

		IDynamicData GetDepartureConsol(IDynamicData shipment)
		{
			var consols = GetConsols(shipment)
				.ToArray();

			if (!consols.Any())
			{
				return null;
			}

			var typedShipment = new TypedUXmlShipment(shipment);

			if (string.IsNullOrEmpty(typedShipment.PortOfLoading))
			{
				return null;
			}

			var typedConsols = consols
				.Select(consol => new TypedUXmlShipment(consol))
				.Where(x => x.TransportMode == typedShipment.TransportMode)
				.ToArray();

			var fallbackConsols = new List<TypedUXmlShipment>();

			foreach (var typedConsol in typedConsols)
			{
				if (typedConsol.PortOfLoading == typedShipment.PortOfLoading)
				{
					return typedConsol.DynamicData;
				}

				if (typedConsol.PortOfLoading.Left(2) == typedShipment.PortOfLoading.Left(2))
				{
					fallbackConsols.Add(typedConsol);
				}
			}

			var fallback = GetFirstInFreghtChain(fallbackConsols.ToArray());

			if (fallback != null)
			{
				return fallback.DynamicData;
			}

			return GetFirstInFreghtChain(typedConsols)?.DynamicData;
		}

		IEnumerable<IDynamicData> GetConsols(IDynamicData shipment)
		{
			if (shipment == null)
			{
				yield break;
			}

			var dataSourceCollection = (IEnumerable<IDynamicData>)shipment.GetDynamicProperty("DataContext.DataSourceCollection");

			if (dataSourceCollection.Any(dd => Convert.ToString(dd.GetDynamicProperty("Type"), CultureInfo.InvariantCulture) == nameof(DataContextType.ForwardingConsol)))
			{
				yield return shipment;
			}

			var parentShipmentCollection = (IEnumerable<IDynamicData>)shipment.GetDynamicProperty("ParentShipmentCollection");

			if (parentShipmentCollection != null)
			{
				foreach (var parentShipment in parentShipmentCollection)
				{
					foreach (var resultShipment in GetConsols(parentShipment))
					{
						yield return resultShipment;
					}
				}
			}
		}

		TypedUXmlShipment GetFirstInFreghtChain(TypedUXmlShipment[] consols)
		{
			return consols
				.FirstOrDefault(consol => consols.All(fallbackConsol => consol.PortOfLoading != fallbackConsol.PortOfDischarge));
		}

		#region TypedUXmlShipment

		sealed class TypedUXmlShipment
		{
			public TypedUXmlShipment(IDynamicData dynamicData)
			{
				this.dynamicData = dynamicData;
			}

			readonly IDynamicData dynamicData;

			public ZString PortOfLoading
			{
				get
				{
					if (!portOfLoading.HasValue)
					{
						portOfLoading = Convert.ToString(dynamicData.GetDynamicProperty("PortOfLoading.Code"), CultureInfo.InvariantCulture);
					}

					return portOfLoading.Value;
				}
			}

			ZString? portOfLoading;

			public ZString PortOfDischarge
			{
				get
				{
					if (!portOfDischarge.HasValue)
					{
						portOfDischarge = Convert.ToString(dynamicData.GetDynamicProperty("PortOfDischarge.Code"), CultureInfo.InvariantCulture);
					}

					return portOfDischarge.Value;
				}
			}

			ZString? portOfDischarge;

			public ZString TransportMode
			{
				get
				{
					if (!transportMode.HasValue)
					{
						transportMode = Convert.ToString(dynamicData.GetDynamicProperty("TransportMode.Code"), CultureInfo.InvariantCulture);
					}

					return transportMode.Value;
				}
			}

			ZString? transportMode;

			public IDynamicData DynamicData => dynamicData;
		}

		#endregion

		#endregion
		#endregion

		#region GetRoundedValue

		decimal GetRoundedValue(decimal originalValue, string transportMode, string unit)
		{
			var unitOfMeasureTypeList = new UnitOfMeasureTypeList();
			var transportModeList = new List<string>
			{
				Constants.TransportModes.Air,
				Constants.TransportModes.Sea,
				Constants.TransportModes.Road,
				Constants.TransportModes.Rail
			};

			if (!transportModeList.Contains(transportMode))
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.CurrentCulture, "Invalid transport mode: {0}.", transportMode));
			}

			if (!unitOfMeasureTypeList.ContainsCode(unit))
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.CurrentCulture, "Invalid unit: {0}.", unit));
			}

			var numberOfDecimals = FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetNumberOfDecimals(transportMode, unit);
			var roundingMode = FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetRoundingMode(transportMode, unit);

			return DefaultNumberOfDecimals.GetRoundedValue(originalValue, roundingMode, numberOfDecimals);
		}

		#endregion

		#region GetRequiredTaxDetail

		ZString GetRequiredTaxInfo(ZString sourceCountryCode, ZString destinationCountryCode, OrganizationAddress org, ZString orgType, ZString docType)
		{
			TaxOrgType type;
			RequiredTaxNumbers.DocumentType documentType;

			if (!Enum.TryParse(orgType, true, out type))
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "Value of '{0}' is not valid. Valid values are {1}.", orgType, string.Join(", ", Enum.GetNames(typeof(TaxOrgType)))));
			}
			if (!Enum.TryParse(docType, out documentType))
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "Value of '{0}' is not valid. Valid values are {1}.", docType, string.Join(", ", Enum.GetNames(typeof(RequiredTaxNumbers.DocumentType)))));
			}

			return GetRequiredTaxNumberWithType(destinationCountryCode, sourceCountryCode, new OrganizationAddressRegistrationNumberProvider(org), type, documentType);
		}

		#endregion

		#region GetTaxNumberForChineseCustoms

		ZString GetTaxNumberForChineseCustoms(IDynamicData organization)
		{
			var countryCode = Convert.ToString(organization.GetDynamicProperty("Country.Code"), CultureInfo.InvariantCulture);
			var organizationAddress = organization.Value as OrganizationAddress;

			return ChinaCustomsTaxNumberHelper.GetTaxNumberWithLabelFormatted(countryCode, new OrganizationAddressRegistrationNumberProvider(organizationAddress));
		}

		ZString GetTaxNumberTypesForChineseCustoms(IDynamicData organization, ZString separator)
		{
			var countryCode = Convert.ToString(organization.GetDynamicProperty("Country.Code"), CultureInfo.InvariantCulture);
			var taxNumberTypes = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(countryCode);

			return string.Join(separator, taxNumberTypes);
		}

		#endregion
	}
}
