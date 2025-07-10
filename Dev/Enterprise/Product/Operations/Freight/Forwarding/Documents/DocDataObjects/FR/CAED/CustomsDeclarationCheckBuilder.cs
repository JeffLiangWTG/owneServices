using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirBookingRequestValidation;
using Enterprise.MasterFiles.Business;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class CustomsDeclarationCheckBuilder
	{
		public CustomsDeclarationCheckBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			Argument.NotNull(parameters, nameof(parameters));
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
			isImportMessage = string.Compare(parameters.DataStoreName, ShipmentDocumentDataStoreNames.CustomsClearanceCheckImport, System.StringComparison.OrdinalIgnoreCase) == 0;
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;
		readonly bool isImportMessage;

		#region Related Business Objects

		ForwardingConsol Consol => isImportMessage ? shipment.ArrivalConsol : shipment.DepartureConsol;

		IEnumerable<ForwardingContainer> Containers => Consol != null ? shipment.ContainersOnConsol(Consol).OfType<ForwardingContainer>() : Enumerable.Empty<ForwardingContainer>();

		#endregion

		public CustomsDeclarationCheck Build()
		{
			var documentName = isImportMessage ? FrenchPortsConstants.DocumentNames.CAEDImport : FrenchPortsConstants.DocumentNames.CAEDExport;

			var customsDeclarationCheck = new CustomsDeclarationCheck(
				nameof(ForwardingShipment),
				shipment.JS_UniqueConsignRef);

			PopulateAddresses(customsDeclarationCheck);
			PopulateHeaderFields(customsDeclarationCheck);
			PopulateConsolInformation(customsDeclarationCheck);
			PopulateCommonAccessRef(customsDeclarationCheck);

			AddValidationsMandatoryFields(customsDeclarationCheck);
			AddValidationsOnFormat(customsDeclarationCheck);

			customsDeclarationCheck.ValidateAllIncludingChildren();

			return customsDeclarationCheck;
		}

		void PopulateHeaderFields(CustomsDeclarationCheck customsDeclarationCheck)
		{
			customsDeclarationCheck.IsImport = isImportMessage;
			customsDeclarationCheck.ShipmentNumber = shipment.JS_UniqueConsignRef;
			customsDeclarationCheck.TotalNumberOfPacks = shipment.JS_OuterPacks;
			customsDeclarationCheck.PackageType = new CodeDescription(shipment.Lookups.PackTypes)
			{
				Code = shipment.JS_F3_NKPackType
			};
			customsDeclarationCheck.CustomsOfficeCode = new CodeDescription(new CustomsOfficeCodes());
			if (isImportMessage && shipment.ImportBroker?.MainAddress != null)
			{
				customsDeclarationCheck.DeclarantsSIRETNumber = shipment.ImportBroker.MainAddress.GetSiretNumber(context);
			}
			if (!isImportMessage && shipment.ExportBroker?.MainAddress != null)
			{
				customsDeclarationCheck.DeclarantsSIRETNumber = shipment.ExportBroker.MainAddress.GetSiretNumber(context);
			}
			if (customsDeclarationCheck.DeclarantsSIRETNumber == null)
			{
				customsDeclarationCheck.DeclarantsSIRETNumber = new RegistrationNumber()
				{
					Value = ZString.Empty,
					Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
					{
						Code = OrgCusCode.FranceCodeTypes.Siret
					},
					CountryOfIssue = new Country(context.Factory, context.Countries)
					{
						Code = Core.Constants.CountryCodes.France
					}
				};
			}

			customsDeclarationCheck.ContainerMode = new CodeDescription(shipment.Lookups.JS_PackingMode_List)
			{
				Code = shipment.JS_PackingMode
			};

			customsDeclarationCheck.ShipmentType = new CodeDescription(shipment.Lookups.JS_ShipmentType_List)
			{
				Code = shipment.JS_ShipmentType
			};
			customsDeclarationCheck.PortOfOrigin = Unloco.Create(context, shipment.Origin);
			customsDeclarationCheck.PortOfDestination = Unloco.Create(context, shipment.Destination);
		}

		RegistrationNumber GetSendingPartyRegistrationNumber(string codeType, IUnloco unloco)
		{
			var registrationNumber = new RegistrationNumber();
			if (GlbBranch.CurrentBranch.OrgProxy?.MainAddress != null)
			{
				registrationNumber.Value = GlbBranch.CurrentBranch.OrgProxy.MainAddress.GetRegistrationNumberWithFallbackToOrgHeader(codeType, unloco?.Country?.Code);
				registrationNumber.Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = codeType
				};
				registrationNumber.CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				};
			}

			if (registrationNumber.Value.IsEmpty && GlbCompany.CurrentCompany.OrgProxy?.MainAddress != null)
			{
				registrationNumber.Value = GlbCompany.CurrentCompany.OrgProxy.MainAddress.GetRegistrationNumberWithFallbackToOrgHeader(codeType, unloco?.Country?.Code);
				registrationNumber.Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = codeType
				};
				registrationNumber.CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				};
			}

			return registrationNumber;
		}

		void PopulateConsolInformation(CustomsDeclarationCheck customsDeclarationCheck)
		{
			if (isImportMessage)
			{
				if (Consol != null)
				{
					var dischargePort = new TransportOrderHelper(Consol.Transports)
						.LastLegMatching(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea && Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKDiscPort.SubstringSafe(0, 2)))
						?.DiscPort;
					if (dischargePort != null)
					{
						customsDeclarationCheck.Port = Unloco.Create(context, dischargePort);
					}
					customsDeclarationCheck.PortDuesCurrency = new CodeDescription(shipment.Lookups.RefCurrency_List)
					{
						Code = dischargePort?.Country?.RN_RX_NKLocalCurrency ?? ZString.Empty
					};

					customsDeclarationCheck.SendingPartySONCode = GetSendingPartyRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, customsDeclarationCheck.Port);
					customsDeclarationCheck.SendingPartyCI5Code = GetSendingPartyRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, customsDeclarationCheck.Port);

					if (Consol.ArrivalCTOAddress != null)
					{
						customsDeclarationCheck.CTOSONCode = Consol.ArrivalCTOAddress.GetSONNumber(context);
						customsDeclarationCheck.CTOCI5Code = Consol.ArrivalCTOAddress.GetCi5Number(context);
					}
				}
			}
			else
			{
				if (Consol != null)
				{
					var loadingPort = new TransportOrderHelper(Consol.Transports)
						.FirstLegMatching(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea && Core.Constants.CountryCodes.IsFranceOrTerritory(t.JW_RL_NKLoadPort.SubstringSafe(0, 2)))
						?.LoadPort;
					if (loadingPort != null)
					{
						customsDeclarationCheck.Port = Unloco.Create(context, loadingPort);
					}
					customsDeclarationCheck.PortDuesCurrency = new CodeDescription(shipment.Lookups.RefCurrency_List)
					{
						Code = loadingPort?.Country?.RN_RX_NKLocalCurrency ?? ZString.Empty
					};

					customsDeclarationCheck.SendingPartySONCode = GetSendingPartyRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, customsDeclarationCheck.Port);
					customsDeclarationCheck.SendingPartyCI5Code = GetSendingPartyRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, customsDeclarationCheck.Port);

					if (Consol.DepartureCTOAddress != null)
					{
						customsDeclarationCheck.CTOSONCode = Consol.DepartureCTOAddress.GetSONNumber(context);
						customsDeclarationCheck.CTOCI5Code = Consol.DepartureCTOAddress.GetCi5Number(context);
					}
				}
			}

			PopulateContainers(customsDeclarationCheck);
		}

		void PopulateContainers(CustomsDeclarationCheck customsDeclarationCheck)
		{
			if (Consol == null)
			{
				customsDeclarationCheck.Containers = new List<Container>().AsReadOnly();
			}
			else
			{
				var containerBuilder = new ContainerBuilder();
				var containers = Containers
					.Select(container => containerBuilder.Build(container, context))
					.ToArray();

				customsDeclarationCheck.Containers = containers;
			}
		}

		void PopulateCommonAccessRef(CustomsDeclarationCheck customsDeclarationCheck)
		{
			var referenceExportConventional = shipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ExportConventional, Core.Constants.CountryCodes.France);
			if (referenceExportConventional != null && !referenceExportConventional.CE_EntryNum.IsEmpty)
			{
				customsDeclarationCheck.CommonAccessRef = referenceExportConventional.CE_EntryNum;
			}
			else
			{
				var referenceImportConventional = shipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ImportConventional, Core.Constants.CountryCodes.France);
				if (referenceImportConventional != null && !referenceImportConventional.CE_EntryNum.IsEmpty)
				{
					customsDeclarationCheck.CommonAccessRef = referenceImportConventional.CE_EntryNum;
				}
			}

			var containers = customsDeclarationCheck.Containers;
			if (customsDeclarationCheck.CommonAccessRef.IsEmpty && containers.Count == 1)
			{
				if (isImportMessage && !containers.First().ImportDepotCustomsReference.IsEmpty)
				{
					customsDeclarationCheck.CommonAccessRef = containers.First().ImportDepotCustomsReference;
				}
				if (!isImportMessage && !containers.First().ExportDepotCustomsReference.IsEmpty)
				{
					customsDeclarationCheck.CommonAccessRef = containers.First().ExportDepotCustomsReference;
				}
			}
		}

		void PopulateAddresses(CustomsDeclarationCheck customsDeclarationCheck)
		{
			customsDeclarationCheck.CurrentUser = AddressBuilder.CreateForCurrentUser(context);

			customsDeclarationCheck.SendingForwarderAddress = AddressBuilder.Create(context, Consol?.SendingForwarderAddress);
			customsDeclarationCheck.ReceivingForwarderAddress = AddressBuilder.Create(context, Consol?.ReceivingForwarderAddress);

			customsDeclarationCheck.ExportBrokerAddress = AddressBuilder.Create(context, shipment.ExportBroker?.MainAddress);
			customsDeclarationCheck.ImportBrokerAddress = AddressBuilder.Create(context, shipment.ImportBroker?.MainAddress);

			customsDeclarationCheck.DepartureCTOAddress = AddressBuilder.Create(context, Consol?.DepartureCTOAddress);
			customsDeclarationCheck.ArrivalCTOAddress = AddressBuilder.Create(context, Consol?.ArrivalCTOAddress);
		}

		void AddValidationsMandatoryFields(CustomsDeclarationCheck customsDeclarationCheck)
		{
			void AddMissingAPPlusIDValidation(ZPropertyInfo sonPropertyInfo, ZPropertyInfo ci5PropertyInfo)
			{
				if (ci5PropertyInfo != null)
				{
					ci5PropertyInfo.AddMessageError(() => ci5PropertyInfo.Value.IsEmpty && sonPropertyInfo.Value.IsEmpty, Res.GetString("B3547905-9AED-429B-B634-BB940A2A359E", "AP+ ID is missing from this organization > Config > Registration Numbers/Codes - type CI5."));
				}

				if (sonPropertyInfo != null)
				{
					sonPropertyInfo.AddMessageError(() => ci5PropertyInfo.Value.IsEmpty && sonPropertyInfo.Value.IsEmpty, Res.GetString("485FB60C-9151-47D8-8685-7AEEC018D5BA", "AP+ ID is missing from this organization > Config > Registration Numbers/Codes - type SON."));
				}
			}

			AddMissingAPPlusIDValidation(customsDeclarationCheck.SendingPartySONCode?.ValueInfo, customsDeclarationCheck.SendingPartyCI5Code?.ValueInfo);
			if (customsDeclarationCheck.CTOSONCode != null && customsDeclarationCheck.CTOCI5Code != null)
			{
				AddMissingAPPlusIDValidation(customsDeclarationCheck.CTOSONCode.ValueInfo, customsDeclarationCheck.CTOCI5Code.ValueInfo);
			}
			customsDeclarationCheck.ShipmentNumberInfo.AddMessageErrorIfEmpty(Res.GetString("6AB5ADF5-F7D3-4CC3-AA1E-7EA1CC8E0E02", "Shipment Number is required."));
			customsDeclarationCheck.TotalNumberOfPacksInfo.AddMessageErrorIfEmpty(Res.GetString("D252E5F2-B733-4FDD-9253-66E14E41FD1F", "Total Number of Packs is required."));
			((CodeDescription)customsDeclarationCheck.PackageType).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("B80546F1-6648-46C1-827B-A7268E5A8743", "Package Type is required."));
			customsDeclarationCheck.DeclarationTypeInfo.AddMessageErrorIfEmpty(Res.GetString("62C07EBF-6540-49AD-88C7-F299BA60F944", "Declaration Type is required."));

			customsDeclarationCheck.DeclarantsSIRETNumber.ValueInfo.AddMessageErrorIfEmpty(isImportMessage
				? Res.GetString("A5DF8C29-67DE-49D6-A1DC-98C6C83D9113", "Declarant's SIRET number is required. Save it against the Shipment > Delivery > Import Broker organization.")
				: Res.GetString("d695a196-04c1-4ef4-b13d-06776524dd44", "Declarant's SIRET number is required. Save it against the Shipment > Pickup > Export Broker organization."));

			customsDeclarationCheck.ContainerMessageInfo.AddMessageError(() => !customsDeclarationCheck.Containers.Any(), Res.GetString("e58437d5-0812-40b4-bd81-9ff9176e9678", "At least one container is required."));

			customsDeclarationCheck.CommonAccessRefInfo.AddMessageError(
				() => customsDeclarationCheck.CommonAccessRef.IsEmpty,
				Res.GetString("01EEFEFE-A5EE-496B-8412-358575DCD54C", "DOC number is required when the Shipment is packed into more than 1 container or ECV/ICV number when LCL."));
		}

		void AddValidationsOnFormat(CustomsDeclarationCheck customsDeclarationCheck)
		{
			customsDeclarationCheck.ShipmentNumberInfo.AddMaximumLengthValidation(Res.GetString("400DCDA1-FD84-443A-9849-90128C1A75F4", "Document Ref Number is too long. Maximum 17 characters allowed."), 17);

			var regexCustomsOfficeCode = new Regex(@"FR[0-9]{6}");
			((CodeDescription)customsDeclarationCheck.CustomsOfficeCode).CodeInfo.AddMessageError(() =>
					!customsDeclarationCheck.CustomsOfficeCode.Code.IsEmpty &&
					!regexCustomsOfficeCode.IsMatch(customsDeclarationCheck.CustomsOfficeCode.Code),
				Res.GetString("A22574F1-5391-455A-A663-50173FE2DA5B", "Incorrect format - Customs Office Code should start with 'FR' followed by six numbers."));

			var regexDeclarationType = new Regex(@"^[a-zA-Z0-9]{3}$");
			customsDeclarationCheck.DeclarationTypeInfo.AddMessageError(() => !customsDeclarationCheck.DeclarationType.IsEmpty && !regexDeclarationType.IsMatch(customsDeclarationCheck.DeclarationType), Res.GetString("FE3590FA-C3F3-46D9-901A-53E47B97319C", "Incorrect format - Declaration Type has 3 characters."));

			((CodeDescription)customsDeclarationCheck.PortDuesCurrency)?.CodeInfo.AddInvalidCodeValidation();
		}
	}
}
