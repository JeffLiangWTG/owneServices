using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Note = Enterprise.UniversalDataBuss.DataObjects.Universal.Note;
using PackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin.Base
{
	abstract class CertificateOfOriginDataObjectWriter<TCertificate, TLineItem>
		: DataObjectWriter<TCertificate, UniversalShipment>
		where TCertificate : CertificateOfOriginDocDataObject<TLineItem>
		where TLineItem : CertificateOfOriginLineItemDocDataObject
	{
		readonly IDocument document;
		readonly string shipmentDocumentName;

		protected abstract string DocumentType { get; }

		public CertificateOfOriginDataObjectWriter(IDataWritingManager writeManager, IDocument document, string shipmentDocumentName)
			: base(writeManager)
		{
			this.document = document;
			this.shipmentDocumentName = shipmentDocumentName;
		}

		protected override UniversalShipment PopulateDataObject(TCertificate certificate)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = certificate.CreateUXmlDataContext();

			universalShipment.PortOfLoading = certificate.PortOfLoading.ToUXmlUnloco();
			universalShipment.PortOfDischarge = certificate.PortOfDischarge.ToUXmlUnloco();
			universalShipment.PortOfOrigin = certificate.PortOfOrigin.ToUXmlUnloco();
			universalShipment.PortOfDestination = certificate.PortOfDestination.ToUXmlUnloco();

			universalShipment.TransportMode = certificate.TransportMode.ToUXmlCodeDescriptionPair();
			universalShipment.VesselName = certificate.Vessel?.Name;
			universalShipment.VoyageFlightNo = certificate.VoyageFlightNumber;

			universalShipment.SetDateCollection(() =>
			{
				var dates = new List<Date>();

				if (certificate.DepartureDate.IsValid)
				{
					dates.Add(Date.New(DateType.Departure, ZBool.True, certificate.DepartureDate));
				}

				if (certificate.ArrivalDate.IsValid)
				{
					dates.Add(Date.New(DateType.Arrival, ZBool.True, certificate.ArrivalDate));
				}

				return dates;
			});

			universalShipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfos(certificate).ToList();
				var addInfosAdditional = CreateAddInfosAdditional(certificate).ToList();

				addInfos.AddRange(addInfosAdditional);

				return addInfos.Count > 0
				  ? addInfos
				  : null;
			});

			if (!certificate.Remarks.IsEmpty)
			{
				universalShipment.SetNoteCollection(() =>
					new DataObjectList<Note>
					{
						new Note
						{
							Description = Notes.CertificateOfOriginNotes,
							IsCustomDescription = false,
							NoteText = certificate.Remarks
						}
					});
			}

			universalShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(certificate)
					.Where(x => x != null)
					.ToList();

				return addresses.Count > 0
				  ? addresses
				  : null;
			});

			if (certificate.Source == LineItemSource.Invoice)
			{
				var commercialInfo = new CommercialInfo();
				commercialInfo.SetWriterStrategy(writeManager.WriterStrategy);
				commercialInfo.SetCommercialInvoiceCollection(() =>
				{
					var commercialInvoiceHeaders = GetCommercialInfoCollectionFromLineItems(certificate);

					return commercialInvoiceHeaders.Any()
						? commercialInvoiceHeaders
						: null;
				});
				universalShipment.CommercialInfo = commercialInfo;
			}
			else
			{
				universalShipment.SetPackingLineCollection(() =>
				{
					var uxmlPackingLines = GetPackingLineCollectionFromLineItems(certificate);

					return uxmlPackingLines.Any()
						? uxmlPackingLines
						: null;
				});
			}

			AttachCOODocument(universalShipment);
			AttachSupportingDocuments(universalShipment, certificate);

			return universalShipment;
		}

		IEnumerable<AddInfo> CreateAddInfos(TCertificate certificate)
		{
			yield return CreateAddInfo(AddinfoTypes.CertificateOfOriginId, certificate.CertificateOfOriginNumber);
			yield return CreateAddInfo(AddinfoTypes.DateOfIssue, ZString.Empty);
			yield return CreateAddInfo(AddinfoTypes.IssuingBody, ZString.Empty);
			yield return CreateAddInfo(AddinfoTypes.SignatureUsed, ZString.Empty);
			yield return CreateAddInfo(AddinfoTypes.DateSigned, ZString.Empty);
			yield return CreateAddInfo(AddinfoTypes.SelfDeclaration, false);
			yield return CreateAddInfo(AddinfoTypes.BackToBackCertificateOfOrigin, certificate.IsBacktobackCertificateOfOrigin);
			yield return CreateAddInfo(AddinfoTypes.SubjectToThirdPartyInvoice, certificate.IsSubjectOfThirdPartyInvoice);
			yield return CreateAddInfo(AddinfoTypes.IssuedRetroactively, certificate.IsIssuedRetroactively);
			yield return CreateAddInfo(AddinfoTypes.DeMinimis, certificate.IsDeMinimis);
			yield return CreateAddInfo(AddinfoTypes.Accumulation, certificate.IsAccumulation);
			yield return CreateAddInfo(AddinfoTypes.VerboseLogging, DocumentsDataRegistry.Instance.EnableVerboseLoggingForCertification.Value);
			yield return CreateAddInfo(AddinfoTypes.SubmitToCustomsAuthority, DocumentsDataRegistry.Instance.EnableSubmissionToCustomsAuthority.Value);
			yield return CreateAddInfo(CertificationParameterNames.Originals, 1);
			yield return CreateAddInfo(CertificationParameterNames.Copies, 0);
			yield return CreateAddInfo(CertificationParameterNames.Legalized, false);
			yield return CreateAddInfo(CertificationParameterNames.DocumentType, DocumentType);
			yield return CreateAddInfo(CertificationParameterNames.IndemnityTermsAccepted, certificate.AgreementInfo.HasBeenAcknowledged);
			yield return CreateAddInfo(CertificationParameterNames.IndemnityTermsVersion, certificate.AgreementInfo.VersionNo);
		}

		protected virtual IEnumerable<AddInfo> CreateAddInfosAdditional(TCertificate certificate) => new List<AddInfo>();

		IEnumerable<OrganizationAddress> CreateAddresses(TCertificate certificate)
		{
			if (!certificate.ApplicantCompanyAddress.IsEmpty())
			{
				yield return certificate.ApplicantCompanyAddress.ToUXmlOrganizationAddress(nameof(certificate.ApplicantCompanyAddress), writeManager.WriterStrategy);
			}

			if (!certificate.ExporterAddress.IsEmpty())
			{
				yield return certificate.ExporterAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!certificate.ImporterAddress.IsEmpty())
			{
				yield return certificate.ImporterAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy);
			}

			yield return GetProducerAddress(certificate);

			if (!certificate.CurrentUser.IsEmpty())
			{
				yield return certificate.CurrentUser.ToUXmlOrganizationAddress(nameof(certificate.CurrentUser), writeManager.WriterStrategy);
			}

			var buyerAddress = GetBuyerAddress(certificate);
			if (buyerAddress is not null)
			{
				yield return buyerAddress;
			}
		}

		protected virtual OrganizationAddress GetBuyerAddress(TCertificate certificate) => null;

		protected OrganizationAddress GetProducerAddress(TCertificate certificate)
		{
			if (certificate.ProducerAddressState.IsSameAsExporter && !certificate.ExporterAddress.IsEmpty())
			{
				return certificate.ExporterAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.Manufacturer), writeManager.WriterStrategy);
			}
			else if (certificate.ProducerAddressState.IsUnknown)
			{
				return DocAddressType.Manufacturer.GetUnknownAddress(writeManager.WriterStrategy);
			}
			else
			{
				return !certificate.ProducerAddress.IsEmpty()
					? certificate.ProducerAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.Manufacturer), writeManager.WriterStrategy)
					: null;
			}
		}

		DataObjectList<CommercialInvoiceHeader> GetCommercialInfoCollectionFromLineItems(TCertificate certificate)
		{
			var commercialInvoiceHeaders = new DataObjectList<CommercialInvoiceHeader>();

			foreach(var lineItem in certificate.LineItems)
			{
				var commercialInvoiceHeader = new CommercialInvoiceHeader(writeManager.WriterStrategy)
				{
					InvoiceNumber = lineItem.Invoice?.Number,
					InvoiceDate = lineItem.Invoice?.Date,
					InvoiceCurrency = new Currency
					{
						Code = lineItem.Invoice?.Amount?.Currency?.Code,
						Description = lineItem.Invoice?.Amount?.Currency?.Description
					},
				};

				var commercialInvoiceLine = new CommercialInvoiceLine(writeManager.WriterStrategy)
				{
					LineNo = lineItem.ItemNumber,
					LinePrice = lineItem.Invoice?.Amount?.Amount,
					Description = lineItem.GoodsDescription,
					HarmonisedCode = lineItem.HarmonizedCode?.Code,
					CountryOfOrigin = new Country
					{
						Name = lineItem.Origin,
						Code = lineItem.OriginCode
					},
					InvoiceQuantity = (ZDecimal)lineItem.PackageCount,
					InvoiceQuantityUnit = lineItem.PackageType?.ToUXmlCodeDescriptionPair(),
					Weight = lineItem.Quantity?.Value,
					WeightUnit = lineItem.Quantity?.Unit.ToUXmlUnitOfWeight(),
					NetWeight = lineItem.QuantityNet?.Value,
					NetWeightUnit = lineItem.QuantityNet?.Unit.ToUXmlUnitOfWeight()
				};

				commercialInvoiceLine.SetAddInfoCollection(() =>
				{
					var additionalInfoCollection = CreateCommercialInvoiceAddInfo(lineItem).ToList();

					return additionalInfoCollection.Count > 0
						? additionalInfoCollection
						: null;
				});

				PopulateGoodsDetailsInvoiceLine(lineItem, commercialInvoiceLine);

				var existingHeader = commercialInvoiceHeaders.FirstOrDefault(header =>
					header.InvoiceNumber.Equals(commercialInvoiceHeader.InvoiceNumber) &&
					header.InvoiceDate.Equals(commercialInvoiceHeader.InvoiceDate)
				);

				if (existingHeader != null)
				{
					existingHeader.SetCommercialInvoiceLineCollection(() =>
					{
						var commercialInvoiceLineCollection = existingHeader.CommercialInvoiceLineCollection;
						commercialInvoiceLineCollection.Add(commercialInvoiceLine);

						return commercialInvoiceLineCollection.Count > 0
							? commercialInvoiceLineCollection
							: null;
					});
				}
				else
				{
					commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });

					commercialInvoiceHeaders.Add(commercialInvoiceHeader);
				}
			}

			return commercialInvoiceHeaders;
		}

		protected virtual void PopulateGoodsDetailsInvoiceLine(TLineItem certificateLineItem, CommercialInvoiceLine outboundInvoiceLine)
		{
		}

		DataObjectList<PackingLine> GetPackingLineCollectionFromLineItems(TCertificate certificate)
		{
			var uxmlPackingLines = new DataObjectList<PackingLine>();

			foreach (var lineItem in certificate.LineItems)
			{
				var packingLine = new PackingLine(writeManager.WriterStrategy)
				{
					ItemNo = lineItem.ItemNumber,
					DetailedDescription = lineItem.GoodsDescription,
					HarmonisedCode = lineItem.HarmonizedCode?.Code,
					MarksAndNos = lineItem.MarksAndNumbers,
					Weight = lineItem.Quantity?.Value,
					WeightUnit = lineItem.Quantity?.Unit.ToUXmlUnitOfWeight(),
					PackQty = new ZLong(lineItem.PackageCount),
					PackType = lineItem.PackageType?.ToUXmlPackType(),
					CountryOfOrigin = !lineItem.OriginCode.IsEmpty
						? new Country { Code = lineItem.OriginCode, Name = lineItem.Origin }
						: null
				};

				PopulateGoodsDetailsPackingLine(lineItem, packingLine);

				packingLine.SetAddInfoCollection(() =>
				{
					var addInfoCollection = CreatePackingAddInfo(lineItem).ToList();

					return addInfoCollection.Count > 0
						? addInfoCollection
						: null;
				});

				uxmlPackingLines.Add(packingLine);
			}

			return uxmlPackingLines;
		}

		protected virtual void PopulateGoodsDetailsPackingLine(TLineItem certificateLineItem, PackingLine outboundPackingLine)
		{
		}

		IEnumerable<AddInfo> CreateCommercialInvoiceAddInfo(TLineItem lineItem)
		{
			if (!lineItem.OriginCriterion?.Code.IsEmpty ?? false)
			{
				yield return CreateAddInfo(AddinfoTypes.OriginCriterion, lineItem.OriginCriterion.Code);
			}

			if (lineItem.MarksAndNumbers.Length > 0)
			{
				yield return CreateAddInfo(AddinfoTypes.MarksAndNumbers, lineItem.MarksAndNumbers);
			}

			var additionalAddInfos = CreateCommercialInvoiceAddInfoAdditional(lineItem);
			if (additionalAddInfos != null)
			{
				foreach (var addInfo in additionalAddInfos)
				{
					yield return addInfo;
				}
			}
		}

		IEnumerable<AddInfo> CreatePackingAddInfo(TLineItem lineItem)
		{
			if (!lineItem.OriginCriterion?.Code.IsEmpty ?? false)
			{
				yield return CreateAddInfo(AddinfoTypes.OriginCriterion, lineItem.OriginCriterion.Code);
			}

			if (lineItem.Invoice != null)
			{
				yield return CreateAddInfo(AddinfoTypes.InvoiceNumber, lineItem.Invoice.Number);
				yield return CreateAddInfo(AddinfoTypes.InvoiceDate, lineItem.Invoice.Date);

				if (lineItem.Invoice.Amount?.Amount != null)
				{
					yield return CreateAddInfo(AddinfoTypes.InvoiceAmount, lineItem.Invoice.Amount.Amount);
				}

				if (!string.IsNullOrEmpty(lineItem.Invoice.Amount?.Currency?.Code))
				{
					yield return CreateAddInfo(AddinfoTypes.InvoiceCurrency, lineItem.Invoice.Amount.Currency.Code);
				}
			}

			var additionalAddInfos = CreatePackingAddInfoAdditional(lineItem);
			if (additionalAddInfos != null)
			{
				foreach (var addInfo in additionalAddInfos)
				{
					yield return addInfo;
				}
			}
		}

		protected virtual IEnumerable<AddInfo> CreateCommercialInvoiceAddInfoAdditional(TLineItem lineItem) => null;

		protected virtual IEnumerable<AddInfo> CreatePackingAddInfoAdditional(TLineItem lineItem) => null;

		protected void AttachCOODocument(UniversalShipment uxmlShipment)
		{
			if (document == null)
			{
				return;
			}

			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = shipmentDocumentName,
				Description = CertificationParameterNames.CertificateOfOrigin,
				Code = Core.Constants.RefDocTypes.CertificateOfOrigin,
				IsPublished = true
			};

			var contextCollection = new List<Context>
			{
				CreateContext(CertificationParameterNames.Certify, false),
				CreateContext(CertificationParameterNames.Originals, 1),
				CreateContext(CertificationParameterNames.Copies, 0),
				CreateContext(CertificationParameterNames.Legalized, false)
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes, contextCollection: contextCollection);
		}

		protected void AttachSupportingDocuments(UniversalShipment uxmlShipment, ISupportingDocDataObject supportingDocDataObject)
		{
			if (supportingDocDataObject.DocSendingCollection == null)
			{
				return;
			}

			uxmlShipment.SetAttachedDocumentCollection(() =>
			{
				var attachedDocuments = uxmlShipment.AttachedDocumentCollection ?? new List<AttachedDocument>();

				foreach (var doc in supportingDocDataObject.DocSendingCollection.Where(i => i.Include).Cast<DocSendingBusinessObject>())
				{
					var attachedDocument = new AttachedDocument
					{
						IsPublished = true,
						FileName = doc.Name,
						ImageData = new MemoryStream(doc.ImageData).CopyToSubStreamableStreamAndCloseStream(),
						Type = new DocumentType
						{
							Code = doc.DocumentType,
							Description = doc.Description
						},
						ContextCollection = new List<Context>
						{
							CreateContext(CertificationParameterNames.Certify, doc.Certify),
							CreateContext(CertificationParameterNames.Originals, 1),
							CreateContext(CertificationParameterNames.Copies, 0),
							CreateContext(CertificationParameterNames.Legalized, false)
						}
					};

					attachedDocuments.Add(attachedDocument);
				}

				return attachedDocuments.Count == 0 ? null : attachedDocuments;
			});
		}

		protected AddInfo CreateAddInfo(string type, object value)
		{
			return AddInfo.New(type, ConvertValueToCOOStringValue(value));
		}

		protected Context CreateContext(string type, object value)
		{
			return new Context()
			{
				Type = type,
				Value = ConvertValueToCOOStringValue(value)
			};
		}

		string ConvertValueToCOOStringValue(object value)
		{
			return value switch
			{
				ZDateTime datetime => datetime.ToISO8601String(),
				ZBool zbool => ((bool)zbool).ToString().ToLower(),
				bool => value.ToString().ToLower(),
				_ => value.ToString()
			};
		}
	}
}
