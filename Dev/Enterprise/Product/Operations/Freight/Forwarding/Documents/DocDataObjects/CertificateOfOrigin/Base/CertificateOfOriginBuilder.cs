using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base
{
	abstract class CertificateOfOriginBuilder<TCertificate, TLineItem, TOriginCriterionList>
		where TCertificate : CertificateOfOriginDocDataObject<TLineItem>
		where TLineItem : CertificateOfOriginLineItemDocDataObject
		where TOriginCriterionList : CodeDescriptionPairList, new()
	{
		public CertificateOfOriginBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		protected readonly ForwardingShipment shipment;
		protected readonly IContext context;

		const int marksAndNumbersMaxLength = 18;
		const int goodsDescriptionMaxLength = 140;
		const int goodsDescriptionMaxLines = 4;

		protected abstract TCertificate CreateCertificate(ZString sourceType, ZString sourceId);
		protected abstract TLineItem CreateLineItem(object lineItemId);
		public abstract string GetDefaultOriginCriterionCode();
		public virtual ZString HarmonisedCodeCountry { get; } = ZString.Empty;
		public abstract ZString ShipmentDocumentName { get; }
		public virtual CertificateOfOriginBuilderFlags InitialFlags { get; } = new CertificateOfOriginBuilderFlags();

		protected virtual string InvalidPacklineOriginError => Res.GetString("d021badf-1a59-4b93-982b-d113cec4a471", "Origin is required.");

		public TCertificate Build()
		{
			var certificate = CreateCertificate(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);
			certificate.AgreementInfo = new AgreementInfo();

			PopulateExistingCertificateOfOriginNumber(certificate);
			SetDefaultTransportValues(certificate);
			PopulatePorts(certificate);
			PopulateDates(certificate);
			PopulateAddresses(certificate);
			PopulateApplicantAddress(certificate);
			PopulateDataFromConsol(certificate);
			PopulateLineItems(certificate);
			PopulateSignature(certificate);
			PopulateRemarks(certificate);

			GetPowerOfAttorneyEDocuments(certificate);
			PopulateDocSendingCollection(certificate);

			Populate(certificate);

			AddValidationCore(certificate);
			certificate.ValidateAllIncludingChildren();

			return certificate;
		}

		protected virtual void Populate(TCertificate certificate)
		{
		}

		protected virtual void AddValidation(TCertificate certificate)
		{
		}

		#region Implementation

		void PopulateExistingCertificateOfOriginNumber(TCertificate certificate) => certificate.CertificateOfOriginNumber = !ShipmentDocumentName.IsEmpty
			? shipment
				.Logs
				.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.MessageAccepted)
				.FirstOrDefault(log => ShipmentDocumentName.EqualsIgnoringCase(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType)))?
				.Parameters
				.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber)
			: ZString.Empty;

		protected virtual void PopulateRemarks(TCertificate certificate)
		{
			var notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description);
			var result = new StringBuilder();
			foreach (var note in notes)
			{
				result.AppendLine(note.ST_NoteDataAsText);
			}

			certificate.Remarks = result.ToString();
		}

		protected virtual void PopulateDocSendingCollection(TCertificate certificate)
		{
			if (certificate.DocSendingCollection == null)
			{
				certificate.DocSendingCollection = new DocSendingBusinessObjectCollection();
			}

			var availableEdocs = GetAvailableEDocs(shipment);

			if (availableEdocs.Count == 0 || availableEdocs.AvailableList.Count == 0)
			{
				return;
			}

			foreach (var edoc in availableEdocs.AvailableList)
			{
				if (edoc.DocType != Core.Constants.RefDocTypes.CertificateOfOrigin &&
					edoc.DocType != Core.Constants.RefDocTypes.RequestDocument)
				{
					certificate.DocSendingCollection.Add(new DocSendingBusinessObject
					{
						Id = edoc.UniqueKey,
						Name = edoc.FileName,
						Description = edoc.Description,
						DocumentType = edoc.DocType,
						Include = edoc.DocType == Core.Constants.RefDocTypes.CommercialInvoice || edoc.DocType == Core.Constants.RefDocTypes.Invoice,
						Certify = false,
						ImageData = edoc.ImageData
					});
				}
			}
		}

		protected virtual AvailableEDocList GetAvailableEDocs(IDocManagerSupport docManagerSupport)
			=> new AvailableEDocList(new List<ZString>(), EDocsHelper.GetEDocCollections(docManagerSupport).ToArray());

		void PopulateDataFromConsol(TCertificate certificate)
		{
			if (shipment.TransportsIncludingRelated.Count > 0)
			{
				var consol = GetEarliestConsolMatchingShipmentTransportMode();
				if (consol != null)
				{
					certificate.TransportMode = new CodeDescription(consol.JK_TransportMode_List)
					{
						Code = consol.TransportMode
					};
					var leg = consol.Transports.FirstTransportWithTransportModeAndExportVessel(consol.TransportMode);
					if (leg != null)
					{
						certificate.Vessel = Vessel.Create(context, leg);
						certificate.VoyageFlightNumber = leg.JW_VoyageFlight;
					}
					else
					{
						certificate.Vessel = new Vessel();
					}
				}
			}
		}

		ForwardingConsol GetEarliestConsolMatchingShipmentTransportMode()
		{
			var legs = shipment.Consols.Cast<CommonConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(legs);
			return (ForwardingConsol)legs.FirstOrDefault(i => i is ForwardingConsol && i.TransportMode == shipment.TransportMode);
		}

		protected virtual void PopulatePorts(TCertificate certificate)
		{
			certificate.PortOfOrigin = Unloco.Create(context, shipment.Origin);
			certificate.PortOfDestination = Unloco.Create(context, shipment.Destination);

			var portOfLoadingUnloco = !shipment.JS_RL_NKLoadPort.IsEmpty
				? ListHelper.GetWithName(shipment.JS_RL_NKLoadPort, shipment.Lookups.RefUNLOCO_List)
				: null;
			certificate.PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = portOfLoadingUnloco?.Code ?? ZString.Empty,
				Name = portOfLoadingUnloco?.Name ?? ZString.Empty
			};

			var portOfDischargeUnloco = !shipment.JS_RL_NKDischargePort.IsEmpty
				? ListHelper.GetWithName(shipment.JS_RL_NKDischargePort, shipment.Lookups.RefUNLOCO_List)
				: null;
			certificate.PortOfDischarge = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = portOfDischargeUnloco?.Code ?? ZString.Empty,
				Name = portOfDischargeUnloco?.Name ?? ZString.Empty
			};
		}

		protected virtual void SetDefaultTransportValues(TCertificate certificate)
		{
			certificate.TransportMode = new CodeDescription(context.TransportModes) { Code = shipment.TransportMode };
			certificate.Vessel = new Vessel();
		}

		protected virtual void PopulateDates(TCertificate certificate)
		{
			certificate.DepartureDate = shipment.JS_E_DEP;
			certificate.ArrivalDate = shipment.JS_E_ARV;
		}

		protected virtual void PopulateLineItems(TCertificate certificate)
		{
			var hasInvoices = shipment.Declarations.Cast<BaseJobDeclaration>().Any(declaration => declaration.Invoices != null && declaration.Invoices.Count > 0);
			if (shipment.OuterPackLines == null && !hasInvoices)
			{
				return;
			}

			if (hasInvoices)
			{
				PopulateInvoiceLineItems(certificate);
			}
			else
			{
				PopulatePackLineItems(certificate);
			}
		}

		protected void PopulatePackLineItems(TCertificate certificate)
		{
			var listLineItems = new List<TLineItem>();
			ZShort itemNumber = 1;

			var orderedPackingLines = shipment
				.OuterPackLines
				.Cast<ForwardingPackLine>()
				.OrderBy(p => p.JL_ContainerPackingOrder);

			foreach (var packLine in orderedPackingLines)
			{
				var certificateLineItem = CreateLineItem(packLine.PK);

				certificateLineItem.ItemNumber = itemNumber++;
				certificateLineItem.MarksAndNumbers = packLine.JL_MarksAndNumbers != ZString.Empty
					? packLine.JL_MarksAndNumbers.SubstringSafe(0, marksAndNumbersMaxLength)
					: shipment.JS_MarksAndNumbers.SubstringSafe(0, marksAndNumbersMaxLength);
				certificateLineItem.GoodsDescription = GetGoodsDescription(packLine).SubstringSafe(0, goodsDescriptionMaxLength).GetFirstNLines(goodsDescriptionMaxLines);
				certificateLineItem.HarmonizedCode = GetHarmonizedCode(packLine);
				certificateLineItem.OriginCriterion = new CodeDescription(new TOriginCriterionList())
				{
					Code = GetDefaultOriginCriterionCode()
				};
				certificateLineItem.Origin = packLine.Origin?.Description ?? string.Empty;
				certificateLineItem.OriginCode = packLine.Origin?.Code ?? string.Empty;
				certificateLineItem.Invoice = new Invoice
				{
					Amount = new Money(context)
				};
				certificateLineItem.PackageCount = packLine.JL_PackageCount;
				certificateLineItem.PackageType = new CodeDescription(packLine.Lookups.PackTypes)
				{
					Code = packLine.JL_F3_NKPackType
				};
				certificateLineItem.Quantity = new Measurement
				{
					Value = packLine.JL_ActualWeight,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = packLine.JL_ActualWeightUQ
					},
				};
				certificateLineItem.QuantityNet = new Measurement
				{
					Value = ZDecimal.Zero,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = ZString.Empty
					}
				};
				certificateLineItem.IsMarksAndNumbersEditable = false;

				PopulateLineItem(null, packLine, certificateLineItem);

				listLineItems.Add(certificateLineItem);
			}

			certificate.LineItems = listLineItems.ToArray();
			certificate.Source = LineItemSource.PackLine;
		}

		protected void PopulateInvoiceLineItems(TCertificate certificate)
		{
			var listLineItems = new List<TLineItem>();
			ZShort itemNumber = 1;

			foreach (var declaration in shipment.Declarations.Cast<BaseJobDeclaration>())
			{
				foreach (var invoiceHeader in declaration.Invoices)
				{
					foreach (var invoiceLineBO in invoiceHeader.InvoiceLines)
					{
						var invoiceLine = (BaseJobComInvoiceLine)invoiceLineBO;
						var certificateLineItem = CreateLineItem(invoiceLine.PK);

						certificateLineItem.ItemNumber = itemNumber++;
						certificateLineItem.MarksAndNumbers = string.Empty;
						certificateLineItem.GoodsDescription = invoiceLine.JI_Description.SubstringSafe(0, goodsDescriptionMaxLength).GetFirstNLines(goodsDescriptionMaxLines);
						certificateLineItem.HarmonizedCode = GetHarmonizedCode(invoiceLine);
						certificateLineItem.OriginCriterion = new CodeDescription(new TOriginCriterionList())
						{
							Code = GetDefaultOriginCriterionCode()
						};
						certificateLineItem.Origin = invoiceLine.CountryOfOrigin?.Description ?? string.Empty;
						certificateLineItem.OriginCode = invoiceLine.CountryOfOrigin?.Code ?? string.Empty;
						certificateLineItem.Invoice = new Invoice
						{
							Number = invoiceHeader.JZ_InvoiceNumber,
							Date = invoiceHeader.JZ_InvoiceDate,
							Amount = new Money
							{
								Amount = invoiceLine.JI_LinePrice,
								Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
								{
									Code = invoiceLine.LinePriceRefCurrency?.Code ?? invoiceHeader.LocalCurrency.Code
								}
							}
						};
						certificateLineItem.PackageCount = invoiceLine.JI_InvoiceQuantity.ToZInt();
						certificateLineItem.PackageType = new CodeDescription(invoiceLine.Lookups.InvoiceUQList)
						{
							Code = invoiceLine.JI_InvoiceUQ
						};
						certificateLineItem.Quantity = new Measurement
						{
							Value = invoiceLine.JI_Weight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = invoiceLine.JI_WeightUQ
							},
						};
						certificateLineItem.QuantityNet = new Measurement
						{
							Value = invoiceLine.JI_NetWeight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = invoiceLine.JI_NetWeightUQ
							}
						};
						certificateLineItem.IsMarksAndNumbersEditable = true;
						if (invoiceLineBO is Enterprise.Integration.Customs.NZ.IJobComInvoiceLine brokerageInvoiceLinenZ)
						{
							certificateLineItem.MarksAndNumbers = brokerageInvoiceLinenZ.PackagingMarks1;
							certificateLineItem.IsMarksAndNumbersEditable = false;
						}

						PopulateLineItem(invoiceLineBO, null, certificateLineItem);

						listLineItems.Add(certificateLineItem);
					}
				}
			}

			certificate.LineItems = listLineItems.ToArray();
			certificate.Source = LineItemSource.Invoice;
		}

		protected virtual void PopulateLineItem(BusinessObject brokerageInvoiceLineBO, ForwardingPackLine shipmentPackline, TLineItem outboundCertificateLineItem)
		{
		}

		protected HarmonizedCode GetHarmonizedCode(ForwardingPackLine packLine)
		{
			var harmonisedCodeString = !HarmonisedCodeCountry.IsEmpty
				? packLine.HarmonisedCodes.FirstOrDefault(hsCode => hsCode.JLH_RN_NKCountry == HarmonisedCodeCountry)
				: null;

			return new HarmonizedCode
			{
				Country = !string.IsNullOrEmpty(HarmonisedCodeCountry)
					? new Country(context.Factory, context.Countries) { Code = HarmonisedCodeCountry }
					: null,
				Code = (harmonisedCodeString?.JLH_Code ?? packLine.JL_HarmonisedCode).SubstringSafe(0, 6)
			};
		}

		protected HarmonizedCode GetHarmonizedCode(BaseJobComInvoiceLine invoiceLine)
		{
			return new HarmonizedCode
			{
				Country = !string.IsNullOrEmpty(HarmonisedCodeCountry)
					? new Country(context.Factory, context.Countries) { Code = HarmonisedCodeCountry }
					: null,
				Code = invoiceLine.JI_Tariff.Replace(".", "").SubstringSafe(0, 6)
			};
		}

		protected virtual ZString GetGoodsDescription(ForwardingPackLine pack)
		{
			return new[]
			{
				pack.JL_DetailedDescription,
				pack.JL_Description,
				pack.Shipment?.DetailedGoodsDescriptionNoteText ?? ZString.Empty,
				pack.Shipment?.JS_GoodsDescription ?? ZString.Empty
			}
			.FirstOrDefault(desc => !desc.IsEmpty);
		}

		protected virtual void PopulateAddresses(TCertificate certificate)
		{
			certificate.ExporterAddress = AddressBuilder
				.Create(context, shipment?.ConsignorDocumentaryAddress)
				.AddAsAgentInfoToCompanyName(shipment?.ConsignorDocumentaryAddress);

			certificate.ProducerAddress = AddressBuilder
				.Create(context, shipment?.ManufacturerDocAddress)
				.AddAsAgentInfoToCompanyName(shipment?.ManufacturerDocAddress);

			certificate.ImporterAddress = AddressBuilder
				.Create(context, shipment?.ConsigneeDocumentaryAddress)
				.AddAsAgentInfoToCompanyName(shipment?.ConsigneeDocumentaryAddress);

			certificate.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
		}

		protected virtual void PopulateApplicantAddress(TCertificate certificate)
		{
			var applicantAddress = new Address(context.Factory);
			applicantAddress.CompanyName = GlbCompany.CurrentCompany.CompanyName;
			applicantAddress.AddressLine1 = GlbCompany.CurrentCompany.Address1;
			applicantAddress.AddressLine2 = GlbCompany.CurrentCompany.Address2;
			applicantAddress.City = GlbCompany.CurrentCompany.City;
			applicantAddress.State = GlbCompany.CurrentCompany.State;
			applicantAddress.Postcode = GlbCompany.CurrentCompany.Postcode;
			applicantAddress.Country = Country.Create(context, GlbCompany.CurrentCompany.Country);
			applicantAddress.Fax = GlbCompany.CurrentCompany.GC_Fax;
			applicantAddress.TaxNumber = GlbCompany.CurrentCompany.GC_BusinessRegNo;

			applicantAddress.Contact = GlbStaff.CurrentUser.GS_FullName;
			applicantAddress.Phone = GlbStaff.CurrentUser.GS_WorkPhone;
			applicantAddress.Email = GlbStaff.CurrentUser.GS_EmailAddress;

			certificate.ApplicantCompanyAddress = applicantAddress;
		}

		protected virtual void PopulateSignature(TCertificate certificate)
		{
			var signature = new SignatureDetails
			{
				Signature = GlbStaff.CurrentUser.SignatureImage,
				Name = GlbStaff.CurrentUser.GS_FullName,
				DateTime = ZDateTime.Now
			};

			certificate.Signature = signature;
		}

		protected virtual void GetPowerOfAttorneyEDocuments(TCertificate certificate)
		{
			var powerOfAttorneyDocuments = GetAvailableEDocs(shipment.Consignor)
				.AvailableList
				.Where(i => i.DocType == Core.Constants.RefDocTypes.PowerOfAttorney)
				.ToList();

			if (powerOfAttorneyDocuments.Count == 0)
			{
				return;
			}

			if (certificate.DocSendingCollection == null)
			{
				certificate.DocSendingCollection = new DocSendingBusinessObjectCollection();
			}

			foreach (var edoc in powerOfAttorneyDocuments)
			{
				certificate.DocSendingCollection.Add(new DocSendingBusinessObject
				{
					Id = edoc.UniqueKey,
					Name = edoc.FileName,
					Description = edoc.Description,
					DocumentType = edoc.DocType,
					Include = true,
					Certify = false,
					ImageData = edoc.ImageData
				});
			}
		}

		#endregion

		#region Validation

		void AddValidationCore(TCertificate certificate)
		{
			AddAddressValidation(certificate);
			AddLineItemsValidationCore(certificate);
			AddPortValidation(certificate);
			AddSignatureValidation(certificate);
			AddCurrentUserValidation(certificate);
			AddTransportReferenceValidation(certificate);
			AddDateValidation(certificate);
			AddNotesValidation(certificate);

			AddValidation(certificate);
		}

		protected virtual void AddAddressValidation(TCertificate certificate)
		{
			if (InitialFlags.ValidateConsigneeAddress)
			{
				certificate.ImporterAddress?.AddressFormattedInfo.AddMessageError(
					() => string.IsNullOrWhiteSpace(certificate.ImporterAddress.CompanyName),
					Res.GetString("a243607b-75e4-43b3-921f-fe7d9354e09d", "Importer Company Name is required. (Consignee)"));
				certificate.ImporterAddress?.AddressFormattedInfo.AddMessageError(
					() => string.IsNullOrWhiteSpace(certificate.ImporterAddress.AddressLine1),
					Res.GetString("afe61967-47dc-45e6-a255-8a84cbd45080", "Importer Address 1 is required. (Consignee)"));
				certificate.ImporterAddress?.AddressFormattedInfo.AddMessageError(
					() => string.IsNullOrWhiteSpace(certificate.ImporterAddress.City),
					Res.GetString("34252535-fc04-48c1-912a-7afeb0f75892", "Importer City is required. (Consignee)"));
				certificate.ImporterAddress?.AddressFormattedInfo.AddMessageError(
					() => string.IsNullOrWhiteSpace(certificate.ImporterAddress.Country?.Name),
					Res.GetString("1bdbf0de-ddaf-4e8e-854f-61321f27ddb3", "Importer Country is required. (Consignee)"));
			}

			if (InitialFlags.ValidateConsigneeEmail == ContactValidation.Always ||
				(InitialFlags.ValidateConsigneeEmail == ContactValidation.IfAddressSet && !string.IsNullOrEmpty(certificate.ImporterAddress.AddressFormatted)))
			{
				certificate.ImporterAddress?.EmailInfo.AddMessageErrorIfEmpty(Res.GetString("3573559e-01f6-4734-855e-5693ca91e097", "Email Address is required. (Consignee)"));
			}

			if (InitialFlags.ValidateConsigneePhone == ContactValidation.Always ||
				(InitialFlags.ValidateConsigneePhone == ContactValidation.IfAddressSet && !string.IsNullOrEmpty(certificate.ImporterAddress.AddressFormatted)))
			{
				certificate.ImporterAddress?.PhoneInfo.AddMessageErrorIfEmpty(Res.GetString("bec0c707-e17c-4e9c-80a2-d17c4327fd4e", "Phone Number is required. (Consignee)"));
			}

			certificate.ExporterAddress?.AddressFormattedInfo.AddMessageError(
				() => string.IsNullOrWhiteSpace(certificate.ExporterAddress.CompanyName),
				Res.GetString("9298ae4b-eac2-4595-ab03-9f3e4565dfb4", "Exporter Company Name is required. (Consignor)"));
			certificate.ExporterAddress?.AddressFormattedInfo.AddMessageError(
				() => string.IsNullOrWhiteSpace(certificate.ExporterAddress.AddressLine1),
				Res.GetString("341fd176-2db6-4397-a097-320e67d2f3ac", "Exporter Address 1 is required. (Consignor)"));
			certificate.ExporterAddress?.AddressFormattedInfo.AddMessageError(
				() => string.IsNullOrWhiteSpace(certificate.ExporterAddress.City),
				Res.GetString("5501ea78-1385-424a-8d4c-d6f9017dcdcd", "Exporter City is required. (Consignor)"));
			certificate.ExporterAddress?.AddressFormattedInfo.AddMessageError(
				() => string.IsNullOrWhiteSpace(certificate.ExporterAddress.Country?.Name),
				Res.GetString("86ba4b39-1cca-4ba8-8b5c-1cc545d120d1", "Exporter Country is required. (Consignor)"));

			if (InitialFlags.ValidateConsignorEmail)
			{
				certificate.ExporterAddress?.EmailInfo.AddMessageErrorIfEmpty(Res.GetString("b5734855-44b7-4d74-b9e7-3c222de65125", "Email Address is required. (Consignor)"));
			}

			if (InitialFlags.ValidateConsignorPhone)
			{
				certificate.ExporterAddress?.PhoneInfo.AddMessageErrorIfEmpty(Res.GetString("6906582a-3ee4-4434-9f9d-b3baefb00b3b", "Phone Number is required. (Consignor)"));
			}

			AddProducerAddressEmailPhoneValidations(certificate);

			certificate.ApplicantCompanyAddressErrorInfo.AddMessageError(() =>
				string.IsNullOrWhiteSpace(certificate.ApplicantCompanyAddress.CompanyName),
				Res.GetString("2feb1205-1bb7-4d6e-8dc9-8e94d2a5bd3c", "Applicant Company Name is required. Please enter in CW1 profile."));
			certificate.ApplicantCompanyAddressErrorInfo.AddMessageError(() =>
				string.IsNullOrWhiteSpace(certificate.ApplicantCompanyAddress.AddressLine1),
				Res.GetString("4e73360b-5f9e-4cee-a857-6a215bbecaa1", "Applicant Address 1 is required. Please enter in CW1 profile."));
			certificate.ApplicantCompanyAddressErrorInfo.AddMessageError(() =>
				string.IsNullOrWhiteSpace(certificate.ApplicantCompanyAddress.City),
				Res.GetString("a544985f-65a3-4e5e-bc2c-a9f1d62ee478", "Applicant City is required. Please enter in CW1 profile."));
			certificate.ApplicantCompanyAddressErrorInfo.AddMessageError(() =>
				string.IsNullOrWhiteSpace(certificate.ApplicantCompanyAddress.Country?.Name),
				Res.GetString("9b6f910a-efc6-4de0-94ab-c06485534f15", "Applicant Country is required. Please enter in CW1 profile."));
			certificate.ApplicantCompanyAddressErrorInfo.AddMessageError(() =>
				string.IsNullOrWhiteSpace(certificate.ApplicantCompanyAddress.TaxNumber),
				Res.GetString("fd7d90eb-62a1-4497-a968-ced4582050f6", "Applicant Registered Number is required. Please enter in CW1 profile."));

			certificate.ApplicantCompanyAddress.AddressFormattedInfo.ValueChanged += (object sender, System.EventArgs e) => { certificate.Validate(nameof(certificate.ApplicantCompanyAddressError)); };
			certificate.ApplicantCompanyAddress.TaxNumberInfo.ValueChanged += (object sender, System.EventArgs e) => { certificate.Validate(nameof(certificate.ApplicantCompanyAddressError)); };
		}

		void AddProducerAddressEmailPhoneValidations(TCertificate certificate)
		{
			switch (InitialFlags.ValidateProducerAddress)
			{
				case AddressValidation.Complex:
					certificate.ProducerAddress?.AddressFormattedInfo.AddMessageError(() =>
					!certificate.ProducerAddressState.IsUnknown && !certificate.ProducerAddressState.IsSameAsExporter && string.IsNullOrWhiteSpace(certificate.ProducerAddress.CompanyName),
					Res.GetString("CE1D8006-ED4C-4AE6-AE14-4C52BADF5C3A", "Producer details entry is mandatory for JEVS submission. Click in box 2 for Producer options."));

					certificate.ProducerAddressState.IsUnknownInfo.ValueChanged += (object sender, System.EventArgs e) => { certificate.ProducerAddress.Validate(nameof(certificate.ProducerAddress.AddressFormatted)); };
					certificate.ProducerAddressState.IsSameAsExporterInfo.ValueChanged += (object sender, System.EventArgs e) => { certificate.ProducerAddress.Validate(nameof(certificate.ProducerAddress.AddressFormatted)); };
					break;

				case AddressValidation.FullIfDifferentFromConsignor:
				case AddressValidation.Full:
					if (InitialFlags.ValidateProducerAddress == AddressValidation.FullIfDifferentFromConsignor && certificate.ExporterAddress.AddressFormatted == certificate.ProducerAddress.AddressFormatted)
					{
						break;
					}
					certificate.ProducerAddress?.AddressFormattedInfo.AddMessageError(
						() => string.IsNullOrWhiteSpace(certificate.ProducerAddress.AddressLine1),
						Res.GetString("b1240e66-0ae1-492c-901b-2905b955480f", "Producer Address 1 is required. (Manufacturer)"));
					certificate.ProducerAddress?.AddressFormattedInfo.AddMessageError(
						() => string.IsNullOrWhiteSpace(certificate.ProducerAddress.City),
						Res.GetString("e317fdf7-0fe0-456d-9baa-eab7569de623", "Producer City is required. (Manufacturer)"));
					certificate.ProducerAddress?.AddressFormattedInfo.AddMessageError(
						() => string.IsNullOrWhiteSpace(certificate.ProducerAddress.Country?.Name),
						Res.GetString("b86c3f7a-f889-424a-932a-d606a41e8a86", "Producer Country is required. (Manufacturer)"));
					certificate.ProducerAddress?.AddressFormattedInfo.AddMessageError(
						() => string.IsNullOrWhiteSpace(certificate.ProducerAddress.CompanyName),
						Res.GetString("1a93bcff-5824-4f28-b2c2-705f5fb353d0", "Producer Company Name is required. (Manufacturer)"));
					break;
				case AddressValidation.NameOnly:
					certificate.ProducerAddress?.AddressFormattedInfo.AddMessageError(
						() => string.IsNullOrWhiteSpace(certificate.ProducerAddress.CompanyName),
						Res.GetString("1a93bcff-5824-4f28-b2c2-705f5fb353d0", "Producer Company Name is required. (Manufacturer)"));
					break;
			}

			if (!certificate.ProducerAddressState.IsSameAsExporter)
			{
				if (InitialFlags.ValidateProducerEmail == ContactValidation.Always ||
					(InitialFlags.ValidateProducerEmail == ContactValidation.IfAddressSet && !string.IsNullOrEmpty(certificate.ProducerAddress.AddressFormatted)))
				{
					certificate.ProducerAddress?.EmailInfo.AddMessageErrorIfEmpty(Res.GetString("9ff59ffc-0b5c-400d-bf5b-c8985b621e7d", "Email Address is required. (Producer)"));
				}

				if (InitialFlags.ValidateProducerPhone == ContactValidation.Always ||
					(InitialFlags.ValidateProducerPhone == ContactValidation.IfAddressSet && !string.IsNullOrEmpty(certificate.ProducerAddress.AddressFormatted)))
				{
					certificate.ProducerAddress?.PhoneInfo.AddMessageErrorIfEmpty(Res.GetString("063799ea-849d-4c51-8ee8-f6460d165ff1", "Phone Number is required. (Producer)"));
				}
			}
		}

		protected virtual void AddLineItemsValidationCore(TCertificate certificate)
		{
			var criterionList = new TOriginCriterionList();
			var criterionListOptions = string.Join(", ", criterionList.ToArray().Select(i => i.Code));

			foreach (var lineItem in certificate.LineItems)
			{
				lineItem.GoodsDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("30B8771E-E0A3-43F5-AC3F-DE09D0E50093", "Goods Description is mandatory."));

				if (InitialFlags.ValidateLineItemItemNumber)
				{
					lineItem.ItemNumberInfo.AddMessageErrorIfEmpty(Res.GetString("352573A7-CC6E-4D30-BE63-25D7B3FD3846", "Item Number is required."));
				}

				lineItem.PackageCountInfo.AddMessageErrorIfEmpty(Res.GetString("3353E65A-CAE0-408B-A633-29141553A1C3", "Number of packs is required."));

				var packageType = (CodeDescription)lineItem.PackageType;
				packageType?.DescriptionInfo.AddMessageError(() =>
					string.IsNullOrWhiteSpace(packageType?.Code),
					Res.GetString("8EAC9F9E-23F7-49FD-BB9B-0FD8D034C336", "Code of package type is required."));

				if(InitialFlags.ValidateLineHarmonizedCode)
				{
					lineItem.HarmonizedCode.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("5ecd239c-7a11-4435-a159-10de5cbfd684", "Harmonized code is required."));
				}

				lineItem.HarmonizedCode.CodeInfo.AddMessageError(
					() => !string.IsNullOrEmpty(lineItem.HarmonizedCode.Code) && lineItem.HarmonizedCode.Code.Length > 6,
					Res.GetString("c6af8a7b-301b-473e-a1f0-aa308038ce12", "Harmonized Code cannot exceed 6 characters."));

				if (InitialFlags.ValidateLineItemWeight)
				{
					if (lineItem.Quantity is Measurement quantity)
					{
						quantity.ValueInfo.AddMessageError(() =>
						quantity.Value.IsEmpty ||
						quantity.Value <= ZDecimal.Zero,
						Res.GetString("c4a3ed9f-8fcb-4686-9afc-942f84371708", "Weight is mandatory and must be greater than zero."));

						if (quantity.Unit is CodeDescription codeDescription)
						{
							codeDescription.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("D9F25538-2464-4E3A-836A-09CC48BACA0E", "Weight unit is mandatory."));
						}
					}
				}

				if (InitialFlags.ValidateLineItemOrigin)
				{
					lineItem.OriginCodeInfo.AddMessageErrorIfEmpty(InvalidPacklineOriginError);
					lineItem.OriginInfo.AddMessageErrorIfEmpty(InvalidPacklineOriginError);
				}

				if (InitialFlags.InvoiceType != InvoiceType.None)
				{
					lineItem.Invoice.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("C5C6EB56-B4E5-4E99-909B-2347C0A98395", "Invoice Number required."));

					lineItem.Invoice.DateInfo.AddMessageError(() => lineItem.Invoice.Date.IsDefault
						|| lineItem.Invoice.Date.IsEmpty
						|| lineItem.Invoice.Date == ZDateTime.Invalid,
						Res.GetString("A6C39743-15B5-420E-AADC-6F93E0F90FA8", "Invoice Date required."));

					if (InitialFlags.InvoiceType == InvoiceType.Complete)
					{
						lineItem.Invoice.Amount.AmountInfo.AddMessageError(() => lineItem.Invoice.Amount?.Amount == null
							|| lineItem.Invoice.Amount.Amount < ZDecimal.Zero,
							Res.GetString("E76C32D0-D39A-420A-826C-431308104441", "Invoice Amount required and must not be a negative value."));

						var currency = (CodeDescription)lineItem.Invoice.Amount?.Currency;
						currency.CodeInfo.AddMessageError(() => string.IsNullOrEmpty(currency.Code),
							Res.GetString("E94FE817-08A7-4626-876B-F1F8EA970E58", "Invoice Currency required."));
					}
				}

				if(InitialFlags.ValidateLineItemMarksAndNumbers)
				{
					lineItem.MarksAndNumbersInfo.AddMessageErrorIfEmpty(Res.GetString("58DEB925-8230-4C8A-A422-11D316452ACD", "Marks and numbers are required."));
				}
				
				if (InitialFlags.ValidateLineItemOriginCriterion)
				{
					var codeDescription = (CodeDescription)lineItem.OriginCriterion;
					codeDescription?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("dc914d17-0aeb-45c9-b844-ceada46aebe5", "Origin Conferring Criterion is required."));
					codeDescription?.CodeInfo.AddMessageError(() => criterionList[lineItem.OriginCriterion.Code] == null, Res.GetString("cbaa9f53-b165-4b31-9d4b-66d02718fc6e", "Origin Conferring Criterion is required to be one of the following; '{0}'.", criterionListOptions));
				}

				AddLineItemValidation(lineItem);
			}
		}

		protected virtual void AddLineItemValidation(TLineItem lineItem)
		{
		}

		protected virtual void AddPortValidation(TCertificate certificate)
		{
			if (InitialFlags.ValidatePortOfLoading)
			{
				((Unloco)certificate.PortOfLoading)?.Country?.NameInfo?.AddMessageError(
					() => string.IsNullOrWhiteSpace(certificate.PortOfLoading?.Country?.Name),
					Res.GetString("7745f3d9-29e1-4ffa-a7a5-72d6658f3fb1", "Port of Loading is required. Please enter in Details section of Shipment."));
			}

			if (InitialFlags.ValidatePortOfDischarge)
			{
				((Unloco)certificate.PortOfDischarge)?.Country?.NameInfo?.AddMessageError(
					() => string.IsNullOrWhiteSpace(certificate.PortOfDischarge?.Country?.Name),
					Res.GetString("86dc7f3d-2015-493e-a5a4-7076322339ee", "Port of Discharge is required. Please enter in Details section of Shipment."));
			}

			if (InitialFlags.ValidatePortOfOrigin)
			{
				((Unloco)certificate.PortOfOrigin)?.Country?.NameInfo?.AddMessageError(
					() => string.IsNullOrWhiteSpace(certificate.PortOfOrigin?.Country?.Name),
					Res.GetString("198ab89f-bb21-4ac6-8b4d-89bae2c32594", "Port of Origin is required."));
			}

			if (InitialFlags.ValidatePortOfDestination)
			{
				((Unloco)certificate.PortOfDestination)?.Country?.NameInfo?.AddMessageError(
					() => string.IsNullOrWhiteSpace(certificate.PortOfDestination?.Country?.Name),
					Res.GetString("13522c78-d8da-4e77-9ea1-9cfe4e10fe30", "Port of Destination is required."));
			}
		}

		protected virtual void AddSignatureValidation(TCertificate certificate)
		{
			if (InitialFlags.ValidateSignature)
			{
				certificate.SignatureErrorInfo.AddMessageError(() => certificate.Signature.Signature == null, Res.GetString("be46daa8-a5f4-453a-840f-17818654c3b7", "Signature required, please upload to your CW1 profile."));
				certificate.SignatureErrorInfo.AddMessageError(() => string.IsNullOrWhiteSpace(certificate.Signature.Name), Res.GetString("3e4daf3e-5f0d-4ac5-8ea7-afecfb364f8c", "Name of logged in user is required."));
				certificate.SignatureErrorInfo.AddMessageError(() => certificate.Signature.DateTime.IsDefault, Res.GetString("3c365012-a0fa-4498-927e-f380a3735734", "Date of signature is required."));
			}
		}

		protected virtual void AddCurrentUserValidation(TCertificate certificate)
		{
			certificate.CurrentUserErrorInfo.AddMessageError(() => string.IsNullOrWhiteSpace(certificate.CurrentUser?.CompanyName), Res.GetString("3e654783-7da2-4672-91cd-3cce0f5685df", "Company Name of logged in user is required."));
			certificate.CurrentUserErrorInfo.AddMessageError(() => string.IsNullOrWhiteSpace(certificate.CurrentUser?.Email), Res.GetString("47a12d42-0571-4e77-b2b9-f8ea3dd1ba69", "Email address required. Please enter in your CW1 profile."));
			if (InitialFlags.ValidateCurrentUserCity)
			{
				certificate.CurrentUserErrorInfo.AddMessageError(() => string.IsNullOrWhiteSpace(certificate.CurrentUser?.City), Res.GetString("c0afed00-8b54-4048-985d-0bcffd41cf81", "City of logged in user is required."));
			}
			certificate.CurrentUserErrorInfo.AddMessageError(() => string.IsNullOrWhiteSpace(certificate.CurrentUser?.Country?.Name), Res.GetString("857a5aa1-6989-46d9-951a-95fa8b3a8b76", "Country of logged in user is required."));

			certificate.CurrentUser.AddressFormattedInfo.ValueChanged += (object sender, System.EventArgs e) => { certificate.Validate(nameof(certificate.CurrentUserError)); };
			certificate.CurrentUser.EmailInfo.ValueChanged += (object sender, System.EventArgs e) => { certificate.Validate(nameof(certificate.CurrentUserError)); };
		}

		void AddTransportReferenceValidation(TCertificate certificate)
		{
			if (InitialFlags.ValidateTransportReference)
			{
				certificate.Vessel?.NameInfo.AddMessageError(
					() => IsTransportReferenceInvalid(certificate),
					Res.GetString("4C56BEBE-7C13-45AA-B8F4-64A5E974993D",
					"Vessel/Flight/Train/Vehicle Name or Number is required."));

				certificate.VoyageFlightNumberInfo.AddMessageError(
					() => IsTransportReferenceInvalid(certificate),
					Res.GetString("4C56BEBE-7C13-45AA-B8F4-64A5E974993D",
					"Vessel/Flight/Train/Vehicle Name or Number is required."));

				certificate.VoyageFlightNumberInfo.ValueChanged += (object sender, System.EventArgs e) =>
				{
					using (certificate.SuspendOnValueChanged(nameof(certificate.VoyageFlightNumber)))
					{
						certificate.Vessel.Validate(nameof(certificate.Vessel.Name));
					}
				};

				if (certificate.Vessel?.NameInfo != null)
				{
					certificate.Vessel.NameInfo.ValueChanged += (object sender, System.EventArgs e) =>
					{
						using (certificate.Vessel.SuspendOnValueChanged(nameof(certificate.Vessel.Name)))
						{
							certificate.Validate(nameof(certificate.VoyageFlightNumber));
						}
					};
				}
			}
		}

		protected virtual bool IsTransportReferenceInvalid(TCertificate certificate) => string.IsNullOrEmpty(certificate.VoyageFlightNumber) && string.IsNullOrEmpty(certificate.Vessel?.Name);

		void AddDateValidation(TCertificate certificate)
		{
			if (InitialFlags.ValidateDepartureDate)
			{
				certificate.DepartureDateInfo.AddMessageErrorIfEmpty(Res.GetString("B49B692F-1E61-4887-8118-73246EF26897", "Departure Date is mandatory."));
			}

			if (InitialFlags.ValidateArrivalDate)
			{
				certificate.ArrivalDateInfo.AddMessageErrorIfEmpty(Res.GetString("3031fcc8-da02-4791-bb7e-8cbf3e800ff6", "ETA is required."));
			}
		}

		void AddNotesValidation(TCertificate certificate)
		{
			certificate.RemarksInfo.AddMessageError(
				() => certificate.Remarks.Length > 200,
				Res.GetString("b00a3b3b-6eb8-40cf-a44f-b17913b641aa", "Remarks cannot have more than 200 characters."));
		}

		#endregion

		#region Helpers

		protected static bool IsNewZealandToChina(string origin, string destination) => ((origin ?? "") == Core.Constants.CountryCodes.NewZealand && (destination ?? "") == Core.Constants.CountryCodes.China);

		#endregion
	}
}
