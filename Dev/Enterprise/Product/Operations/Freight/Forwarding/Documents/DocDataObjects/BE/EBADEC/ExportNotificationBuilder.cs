using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE.EBADEC;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class ExportNotificationBuilder
	{
		public ExportNotificationBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;

		public ExportNotification Build()
		{
			var exportNotification = new ExportNotification(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			PopulateExportNotification(exportNotification);
			PopulateAddresses(exportNotification);

			AddValidationsMandatoryFields(exportNotification);
			AddValidationsOnFormat(exportNotification);

			exportNotification.ValidateAllIncludingChildren();

			return exportNotification;
		}

		void PopulateExportNotification(ExportNotification exportNotification)
		{
			exportNotification.ConsolNumber = consol.JK_UniqueConsignRef;

			exportNotification.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List)
			{
				Code = consol.JK_ConsolMode
			};
			exportNotification.ShipmentType = new CodeDescription(consol.JK_TransportMode_List)
			{
				Code = consol.JK_TransportMode
			};

			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));
			Freight.Business.Transport legPrecedingSeaLeg = null;
			foreach (var transport in consol.Transports.Cast<Freight.Business.Transport>())
			{
				if (transport.JW_TransportMode == Constants.TransportModes.Sea
					&& (transport.JW_RL_NKLoadPort == "BEANR" || transport.JW_RL_NKLoadPort == "BEZEE"))
				{
					exportNotification.PortOfOrigin = Unloco.Create(context, transport.LoadPort);
					exportNotification.PortOfDestination = Unloco.Create(context, transport.DiscPort);
					break;
				}
				legPrecedingSeaLeg = transport;
			}
			if (legPrecedingSeaLeg != null)
			{
				exportNotification.TransportModeToTerminal = new CodeDescription(legPrecedingSeaLeg?.JW_TransportMode_List)
				{
					Code = legPrecedingSeaLeg?.TransportMode ?? ZString.Empty
				};
				switch (exportNotification.TransportModeToTerminal.Code)
				{
					case Constants.TransportModes.Road:
						exportNotification.VesselType = "TR";
						break;
					case Constants.TransportModes.Rail:
						exportNotification.VesselType = "RL";
						break;
					case Constants.TransportModes.Air:
						exportNotification.VesselType = ZString.Empty;
						break;
					default:
						exportNotification.VesselType = legPrecedingSeaLeg?.Vessel?.RV_VesselType ?? ZString.Empty;
						break;
				}
			}
			else
			{
				exportNotification.TransportModeToTerminal = new CodeDescription(new CodeDescriptionPairList())
				{
					Code = ZString.Empty
				};
				exportNotification.VesselType = ZString.Empty;
			}

			var ctoTerminal = consol.DepartureCTOAddress.GetPortSystemNumber(context).Value;
			if (!ctoTerminal.IsEmpty)
			{
				exportNotification.Terminal = ctoTerminal;
			}
			else
			{
				exportNotification.Terminal = exportNotification.PortOfOrigin?.Code ?? ZString.Empty;
			}
			exportNotification.BookingReference = consol.JK_BookingReference;

			exportNotification.SendingPartyCode = GetSendingPartyCode();

			exportNotification.IsFerryTerminal = consol?.DepartureCTOAddress?.Header?.OH_IsFerryWaterTerminal ?? ZBool.False;

			PopulatePackingLine(exportNotification);
		}

		void PopulatePackingLine(ExportNotification exportNotification)
		{
			var containerBizObjs = consol.Containers.Cast<ForwardingContainer>();

			if (containerBizObjs?.Count() > 0)
			{
				exportNotification.PackLines = containerBizObjs.Select(container =>
				{
					var packLine = new PackingLine(container.PK);
					packLine.ContainerNumber = container.JC_ContainerNum;
					packLine.MovementReferenceNumbers = GetMovementReferenceNumbersFromPackingLines(container.PackLines.OfType<PackLine>()).ToArray();

					return packLine;
				}).ToArray();
			}
			else
			{
				var packLineBizObjs = consol.RelatedPackLines.Cast<PackLine>();
				if (packLineBizObjs?.Count() > 0)
				{
					exportNotification.PackLines = packLineBizObjs.Where(p => p.Shipment.PackingMode == Constants.ContainerModes.RollOnRollOff).Select(vehiclePackline =>
					{
						var packLine = new PackingLine(vehiclePackline.PK);
						packLine.VIN = vehiclePackline.JL_RefNumber;

						packLine.MovementReferenceNumbers = GetMovementReferenceNumbersFromPackingLines(new[] { vehiclePackline }).ToArray();

						return packLine;
					}).ToArray();
				}
			}
		}

		IEnumerable<MovementReferenceNumber> GetMovementReferenceNumbersFromPackingLines(IEnumerable<PackLine> packingLines)
		{
			var mrns = packingLines.Select((p) =>
			{
				var mrn = p.JL_ExportRefNumber.IsEmpty && p.Shipment.CustomsEntryNumberType == CusEntryNumberTypes.Standard.MovementReferenceNumber
					? p.Shipment.CustomsEntryNumber
					: p.JL_ExportRefNumber;

				return new { MRN = mrn, PackingLine = p };
			}).GroupBy(x => x.MRN).OrderBy(g => g.Key);

			return mrns.Select(x => PopulateMovementReferenceNumber(x.First().PackingLine.Shipment, x.Key)).ToArray();
		}

		MovementReferenceNumber PopulateMovementReferenceNumber(CommonShipment shipment, ZString customsEntryNumber)
		{
			var customsDocumentCode = new CodeDescription(new DocumentCodes())
			{
				Code = ZString.Empty
			};

			var mrn = new MovementReferenceNumber($"{shipment.PK}-{customsEntryNumber}"); // string interpolation
			mrn.CustomsOfficeCode = string.Empty;
			try
			{
				var declaration = ((Enterprise.Integration.Customs.EU.IJobDeclaration)shipment?.DeclarationForDocuments);
				mrn.CustomsOfficeCode = declaration?.CustomsOfficeCollection?.FirstOrDefault(x => x.CY_Code == BelgianPortsConstants.EuOfficeCodesTypes.OfficeOfExit)?.CY_Data ?? string.Empty;
				if (string.IsNullOrEmpty(mrn.CustomsOfficeCode))
				{
					mrn.CustomsOfficeCode = declaration?.CustomsOfficeCollection?.FirstOrDefault(x => x.CY_Code == BelgianPortsConstants.EuOfficeCodesTypes.ActualExitOffice)?.CY_Data ?? string.Empty;
				}
			}
			catch (Exception)
			{
				//ignore
			}
			mrn.MRN = customsEntryNumber;
			mrn.CustomsDocumentCode = customsDocumentCode;

			return mrn;
		}

		void PopulateAddresses(ExportNotification exportNotification)
		{
			exportNotification.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			exportNotification.SendingForwarderAddress = AddressBuilder.Create(context, GlbCompany.CurrentCompany.OrgProxy?.MainAddress);
			exportNotification.DepartureCTOAddress = AddressBuilder.Create(context, consol?.DepartureCTOAddress);
		}

		RegistrationNumber GetSendingPartyCode()
		{
			var sendingPartyCode = GetSendingPartyCodeFromBrancheOrCompany(GlbBranch.CurrentBranch.OrgProxy?.MainAddress, GlbCompany.CurrentCompany.OrgProxy?.MainAddress, address => address.GetPortSystemNumber(context));
			if (sendingPartyCode.Value.IsEmpty)
			{
				sendingPartyCode = GetSendingPartyCodeFromBrancheOrCompany(GlbBranch.CurrentBranch.OrgProxy?.MainAddress, GlbCompany.CurrentCompany.OrgProxy?.MainAddress, address => address.GetDunsNumber(context, false));
			}
			if (sendingPartyCode.Value.IsEmpty)
			{
				sendingPartyCode = GetSendingPartyCodeFromBrancheOrCompany(GlbBranch.CurrentBranch.OrgProxy?.MainAddress, GlbCompany.CurrentCompany.OrgProxy?.MainAddress, address => address.GetEoriNumber(context));
			}

			return sendingPartyCode;
		}

		RegistrationNumber GetSendingPartyCodeFromBrancheOrCompany(OrgAddress address, OrgAddress addressToFallback, Func<OrgAddress, RegistrationNumber> getRegistrationNumberFunc)
		{
			var result = getRegistrationNumberFunc.Invoke(address);
			if (result != null && result.Value.IsEmpty)
			{
				result = getRegistrationNumberFunc.Invoke(addressToFallback);
			}

			return result;
		}

		void AddValidationsMandatoryFields(ExportNotification exportNotification)
		{
			exportNotification.SendingPartyCode?.ValueInfo.AddMessageError(()
				=> exportNotification.SendingPartyCode.Value.IsEmpty,
				Res.GetString("E3495E13-3D15-4A0B-BC7B-E050C7BF44E6", "Sender's ID is mandatory. Please maintain it in branch or company organization. proxy Organization > Config > Registration Numbers/Codes - type PSN or DUN or EOR."));

			exportNotification.BookingReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("6FC4CB96-1584-4B68-A053-9CFC413B6859", "Booking Reference is required."));

			foreach (var packLine in exportNotification.PackLines)
			{
				packLine.ContainerNumberInfo.AddMessageError(()
					=> exportNotification.IsFerryTerminal && packLine.ContainerNumber.IsEmpty,
					Res.GetString("F58420E6-2662-4493-89B2-E30CCFC32FA8", "Equipment Number is required."));

				foreach (var mrn in packLine.MovementReferenceNumbers)
				{
					mrn.MRNInfo.AddMessageErrorIfEmpty(
						Res.GetString("B56F3CAB-227D-47A8-9842-638BCFCD13E6", "Please enter Document Number in Shipment > Packing > Export Ref Number or Shipment > Basic Registration > Details > Entry Details."));

					((CodeDescription)mrn.CustomsDocumentCode).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("D74070A3-3A80-407B-880B-24B78203377E", "Please select an entry type from drop down list."));

					var customsDocumentCodes = new DocumentCodes().CodesAsString;
					((CodeDescription)mrn.CustomsDocumentCode).CodeInfo.AddMessageError(()
						=> !customsDocumentCodes.Contains(mrn.CustomsDocumentCode.Code), Res.GetString("35C30173-137C-430C-8370-A0178CAAF054", "Entry type does not match with a value of the drop down list. Please select an entry type from drop down list."));

					mrn.CustomsOfficeCodeInfo.AddMessageError(() => mrn.CustomsDocumentCode.Code == DocumentCodes.Codes.T && mrn.CustomsOfficeCode.IsEmpty, Res.GetString("8590C685-A594-40F5-A35F-E86F71AD45CA", "Customs Office Code is required when Entry Type is 'T' (Transit declaration)."));
					mrn.AddValidationDependencies(mrn.CustomsOfficeCodeInfo, ((CodeDescription)mrn.CustomsDocumentCode).CodeInfo);
				}
			}
		}

		void AddValidationsOnFormat(ExportNotification exportNotification)
		{
			var formType = GetFormType(exportNotification);
			var regexContainerNumber = new Regex(@"^[a-zA-Z]{3}[uUjJzZ]{1}[0-9]{7}$");

			foreach (var packLine in exportNotification.PackLines)
			{
				if (formType == FormType.Ferry)
				{
					packLine.ContainerNumberInfo.AddMessageError(()
						=> exportNotification.IsFerryTerminal && !packLine.ContainerNumber.IsEmpty && packLine.ContainerNumber.Length > 17,
						Res.GetString("0D836144-C02F-4A4B-965A-345EC563A729", "Incorrect format - Equipment number must be maximum 17 characters long."));
				}
				if ((formType == FormType.Container || formType == FormType.Mixed))
				{
					packLine.ContainerNumberInfo.AddMessageError(()
						=> !packLine.ContainerNumber.IsEmpty && !regexContainerNumber.IsMatch(packLine.ContainerNumber),
						Res.GetString("B275F301-4A19-4656-BF5B-A0F71135AB5C", "Incorrect format - Container number must start with 4 letters, followed by 7 numbers."));
				}
				if ((formType == FormType.RollOnRollOff || formType == FormType.Mixed))
				{
					packLine.VINInfo.AddMessageError(()
						=> !packLine.VIN.IsEmpty && packLine.VIN.Length != 17,
						Res.GetString("C1B205E3-13D2-4606-B784-20B50CA1F674", "Incorrect format - VIN must be 17 characters long."));
				}

				foreach (var mrn in packLine.MovementReferenceNumbers)
				{
					mrn.CustomsOfficeCodeInfo.AddMessageError(()
					=> !mrn.CustomsOfficeCode.IsEmpty && mrn.CustomsOfficeCode.Length > 8,
					Res.GetString("6A424BC6-5CE2-41C1-804B-07BF572EC9BB", "Incorrect format - Office code must be maximum 8 characters long."));
				}
			}

			exportNotification.TerminalInfo.AddMessageError(()
				=> !exportNotification.Terminal.StartsWith("BEANR") && !exportNotification.Terminal.StartsWith("BEZEE"),
				Res.GetString("3232B2A5-775D-417D-AC74-9A0A4B41FD33", "Terminal is required and needs to start with BEANR or BEZEE (Consol > Departure > CTO Address > Registration Code PSN)."));

			exportNotification.BookingReferenceInfo.AddMessageError(()
				=> !exportNotification.BookingReference.IsEmpty && exportNotification.BookingReference.Length > 35,
					Res.GetString("144DF112-BEBD-491C-B6F9-806806FC6C7B", "Incorrect format - Booking reference must be maximum 35 characters long."));
		}

		FormType GetFormType(ExportNotification exportNotification)
		{
			bool cont = false;
			bool roro = false;

			foreach (var packLine in exportNotification.PackLines)
			{
				if (!packLine.ContainerNumber.IsEmpty)
				{
					cont = true;
				}
				if (!packLine.VIN.IsEmpty)
				{
					roro = true;
				}
			}
			if (exportNotification.IsFerryTerminal)
			{
				return FormType.Ferry;
			}
			else if (cont && roro)
			{
				return FormType.Mixed;
			}
			else if (cont)
			{
				return FormType.Container;
			}
			else
			{
				return FormType.RollOnRollOff;
			}
		}
	}
}
