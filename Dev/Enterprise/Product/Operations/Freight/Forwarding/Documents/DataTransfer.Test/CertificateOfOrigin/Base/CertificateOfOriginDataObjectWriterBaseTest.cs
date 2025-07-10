using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using Vessel = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Vessel;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.CertificateOfOrigin.Base
{
	abstract class CertificateOfOriginDataObjectWriterBaseTest<TCertificate, TLineItem, TOriginCriterionList> : DataObjectWriterTest
		where TCertificate : CertificateOfOriginDocDataObject<TLineItem>
		where TLineItem : CertificateOfOriginLineItemDocDataObject
		where TOriginCriterionList : CodeDescriptionPairList, new()
	{
		protected abstract string ExpectedPackingLineXml { get; }
		protected abstract string ExpectedCommercialInvoiceLineXml { get; }
		protected abstract string ShipmentDocumentName { get; }
		protected abstract CertificateOfOriginDataObjectWriter<TCertificate, TLineItem> CreateWriter(IDataWritingManager writeManager, IDocument document);
		protected abstract TCertificate CreateCertificate(ZString sourceType, ZString sourceId);
		protected abstract TLineItem CreateLineItem(object lineItemId);

		protected virtual IUnloco DefaultPortOfLoading => new DummyUnloco { Code = "NZAKL", Name = "Auckland", IATACode = "AKL" };
		protected virtual IUnloco DefaultPortOfDischarge => new DummyUnloco { Code = "CNCAN", Name = "Guangzhou", IATACode = "CAN" };
		protected virtual IUnloco DefaultPortOfOrigin => new DummyUnloco { Code = "NZAKL", Name = "Auckland", IATACode = "AKL" };
		protected virtual IUnloco DefaultPortOfDestination => new DummyUnloco { Code = "HKHKG", Name = "Hong Kong", IATACode = "HKG" };

		protected abstract string FirstLineItemOrigin { get; }
		protected abstract string FirstLineItemOriginCode { get; }
		protected abstract string SecondLineItemOrigin { get; }
		protected abstract string SecondLineItemOriginCode { get; }

		const string Type = nameof(Type);

		public void Test_PopulateDataObjectForPackLines()
		{
			var certificate = CreateCertificateCore();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = CreateWriter(manager, null);

			using (DocumentsDataRegistry.Instance.EnableVerboseLoggingForCertification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.EnableSubmissionToCustomsAuthority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dataObject = writer.GetDataObject(certificate);
				AssertUXml(dataObject, ExpectedPackingLineXml);
			}
		}

		public void Test_PopulateDataObjectForCommercialInvoiceCollection()
		{
			var certificate = CreateCertificateCore(LineItemSource.Invoice);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = CreateWriter(manager, null);

			using (DocumentsDataRegistry.Instance.EnableVerboseLoggingForCertification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.EnableSubmissionToCustomsAuthority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dataObject = writer.GetDataObject(certificate);
				AssertUXml(dataObject, ExpectedCommercialInvoiceLineXml);
			}
		}

		public void Test_PopulateDataObject_ForAmendment()
		{
			var certificate = CreateCertificateCore();
			certificate.CertificateOfOriginNumber = "180.180.180";

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = CreateWriter(manager, null);

			var dataObject = writer.GetDataObject(certificate);
			AssertEquals(certificate.CertificateOfOriginNumber, dataObject.AddInfoCollection.FirstOrDefault(x => x.Key == (ZString?)AddinfoTypes.CertificateOfOriginId)?.Value);
		}

		public void TestProducerAddressFlags()
		{
			var certificate = CreateCertificateCore();
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = CreateWriter(manager, null);

			using (DocumentsDataRegistry.Instance.EnableVerboseLoggingForCertification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.EnableSubmissionToCustomsAuthority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				certificate.ProducerAddressStateString = "True\nFalse\nTrue";
				var dataObject = writer.GetDataObject(certificate);
				AssertEquals("ExporterAddress", dataObject.OrganizationAddressCollection[3].CompanyName);

				certificate.ProducerAddressStateString = "False\nTrue\nTrue";
				dataObject = writer.GetDataObject(certificate);
				AssertEquals("UNKNOWN", dataObject.OrganizationAddressCollection[3].CompanyName);
			}
		}

		public void Test_Attachment()
		{
			var certificate = CreateCertificateCore();

			var document = new DummyDocument();
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = CreateWriter(manager, document);
			var dataObject = writer.GetDataObject(certificate);

			var attachments = dataObject.AttachedDocumentCollection;
			AssertEquals(3, attachments.Count);

			DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachments[0], new DataObjectWriterHelper.FileAttributes()
			{
				Name = ShipmentDocumentName,
				Description = CertificationParameterNames.CertificateOfOrigin,
				Code = Core.Constants.RefDocTypes.CertificateOfOrigin,
				IsPublished = true
			});

			DataObjectWriterHelperTest.AssertPDFAttachedDocumentsContextCollection(attachments[1], new List<Context>
			{
				new Context()
				{
					Type = CertificationParameterNames.Certify,
					Value = true.ToString().ToLower()
				},
				new Context()
				{
					Type = CertificationParameterNames.Originals,
					Value = 1.ToString()
				},
				new Context()
				{
					Type = CertificationParameterNames.Copies,
					Value = 0.ToString()
				},
				new Context()
				{
					Type = CertificationParameterNames.Legalized,
					Value = false.ToString().ToLower()
				}
			});

			DataObjectWriterHelperTest.AssertPDFAttachedDocumentsContextCollection(attachments[2], new List<Context>
			{
				new Context()
				{
					Type = CertificationParameterNames.Certify,
					Value = false.ToString().ToLower()
				},
				new Context()
				{
					Type = CertificationParameterNames.Originals,
					Value = 1.ToString()
				},
				new Context()
				{
					Type = CertificationParameterNames.Copies,
					Value = 0.ToString()
				},
				new Context()
				{
					Type = CertificationParameterNames.Legalized,
					Value = false.ToString().ToLower()
				}
			});
		}

		protected TCertificate CreateCertificateCore(
			LineItemSource source = LineItemSource.PackLine,
			IUnloco portOfLoading = null,
			IUnloco portOfDischarge = null,
			IUnloco portOfOrigin = null,
			IUnloco portOfDestination = null)
		{
			var certificate = CreateCertificate("ForwardingShipment", "S00001000");

			certificate.TransportMode = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = Constants.TransportModes.Sea,
			};

			certificate.DepartureDate = new ZDateTime(2020, 8, 22);
			certificate.ArrivalDate = new ZDateTime(2020, 9, 10);

			certificate.Vessel = new Vessel
			{
				Name = "VesselName",
				LloydsIMO = "ZZZ",
				RadioCallSign = "AAA",
				Type = null,
				CountryOfRegistration = null,
			};

			certificate.VoyageFlightNumber = "TT9876";

			certificate.PortOfLoading = portOfLoading ?? DefaultPortOfLoading;
			certificate.PortOfDischarge = portOfDischarge ?? DefaultPortOfDischarge;
			certificate.PortOfOrigin = portOfOrigin ?? DefaultPortOfOrigin;
			certificate.PortOfDestination = portOfDestination ?? DefaultPortOfDestination;

			certificate.ExporterAddress = CreateAddress(nameof(certificate.ExporterAddress));
			certificate.ImporterAddress = CreateAddress(nameof(certificate.ImporterAddress));
			certificate.ProducerAddress = CreateAddress(nameof(certificate.ProducerAddress));
			certificate.CurrentUser = CreateAddress(nameof(certificate.CurrentUser));
			certificate.ApplicantCompanyAddress = CreateAddress(nameof(certificate.ApplicantCompanyAddress));
			certificate.ApplicantCompanyAddress.Fax = "091593217";
			certificate.ApplicantCompanyAddress.Phone = "091593218";
			certificate.ApplicantCompanyAddress.State = "Auckland";
			certificate.ApplicantCompanyAddress.Postcode = "H91Y6CT";

			certificate.AgreementInfo = new AgreementInfo
			{
				HasBeenAcknowledged = true,
				VersionNo = 2
			};
			certificate.Remarks = "remarks for the certification of origin";

			var criterionList = new TOriginCriterionList();

			if (source == LineItemSource.PackLine)
			{
				certificate.LineItems = CreateLineItemsForPackLine(criterionList);
			}
			else
			{
				certificate.LineItems = CreateLineItemsForCommercialInvoiceCollection(criterionList);
			}

			certificate.Source = source;

			certificate.DocSendingCollection = new DocSendingBusinessObjectCollection();
			certificate.DocSendingCollection.Add(new DocSendingBusinessObject
			{
				Id = Guid.Parse("5df84b95-bc50-4998-83fd-4f3fa869fd6a"),
				Include = true,
				Certify = true,
				Name = "INV0001.pdf",
				DocumentType = "INV",
				Description = "Commercial Invoice",
				ImageData = new ZBlob(new byte[] { 0x1, 0x2 }),
			});
			certificate.DocSendingCollection.Add(new DocSendingBusinessObject
			{
				Id = Guid.Parse("11ecb807-e832-4b88-948e-6647cc93bad4"),
				Include = false,
				Certify = false,
				Name = "INV0002.pdf",
				DocumentType = "INV",
				Description = "Commercial Invoice",
				ImageData = new ZBlob(new byte[] { 0x1, 0x2 }),
			});
			certificate.DocSendingCollection.Add(new DocSendingBusinessObject
			{
				Id = Guid.Parse("17c12e7a-3289-48b6-9b57-46e1096b1fa6"),
				Include = true,
				Certify = false,
				Name = "INV0003.pdf",
				DocumentType = "INV",
				Description = "Commercial Invoice",
				ImageData = new ZBlob(new byte[] { 0x1, 0x2 }),
			});

			PopulateCertificate(certificate);

			return certificate;
		}

		protected virtual void PopulateCertificate(TCertificate certificate)
		{
		}

		IReadOnlyCollection<TLineItem> CreateLineItemsForPackLine(TOriginCriterionList criterionList)
		{
			var lineItem1 = CreateLineItem("b194952d-4732-4d79-a31f-a5eee60f8678");
			lineItem1.Quantity = new Measurement
			{
				Value = 1,
				Unit = new DummyCodeDescription
				{
					Code = Constants.Weight.Kilograms
				}
			};
			lineItem1.Invoice = new Invoice
			{
				Number = "INV0001",
				Date = new ZDateTime(2023, 07, 11),
				Amount = new Money
				{
					Amount = new ZDecimal(10.00m),
					Currency = new DummyCodeDescription
					{
						Code = "AUD"
					}
				}
			};
			lineItem1.ItemNumber = 100;
			lineItem1.MarksAndNumbers = "marks & nums (1)";
			lineItem1.GoodsDescription = "goods description (1)";
			lineItem1.OriginCode = FirstLineItemOriginCode;
			lineItem1.Origin = FirstLineItemOrigin;
			lineItem1.OriginCriterion = new CodeDescription(criterionList)
			{
				Code = criterionList.Count > 0 ? criterionList[0].Code : string.Empty
			};
			lineItem1.PackageCount = 20;
			lineItem1.HarmonizedCode = new HarmonizedCode
			{
				Code = "1234",
				Country = new DummyCountry
				{
					Code = "AU"
				}
			};

			var lineItem2 = CreateLineItem("d1893ae0-b09b-436a-adc1-6372291db6b1");
			lineItem2.Quantity = new Measurement
			{
				Value = 2,
				Unit = new DummyCodeDescription
				{
					Code = Constants.Weight.Kilograms
				}
			};
			lineItem2.Invoice = new Invoice
			{
				Number = "INV0002",
				Date = new ZDateTime(2023, 07, 11),
				Amount = new Money
				{
					Amount = new ZDecimal(13.37m),
					Currency = new DummyCodeDescription
					{
						Code = "AUD"
					}
				}
			};
			lineItem2.ItemNumber = 200;
			lineItem2.MarksAndNumbers = "marks & nums (2)";
			lineItem2.GoodsDescription = "goods description (2)";
			lineItem2.OriginCode = SecondLineItemOriginCode;
			lineItem2.Origin = SecondLineItemOrigin;
			lineItem2.OriginCriterion = new CodeDescription(criterionList)
			{
				Code = criterionList.Count > 0 ? criterionList[criterionList.Count - 1].Code : string.Empty
			};
			lineItem2.PackageCount = 10;
			lineItem2.PackageType = new DummyCodeDescription
			{
				Code = "PKG",
				Description = "Package"
			};
			lineItem1.HarmonizedCode = new HarmonizedCode
			{
				Code = "2345",
				Country = new DummyCountry
				{
					Code = "NZ"
				}
			};

			return new[]
			{
				lineItem1,
				lineItem2
			};
		}

		IReadOnlyCollection<TLineItem> CreateLineItemsForCommercialInvoiceCollection(TOriginCriterionList criterionList)
		{
			var lineItem1 = CreateLineItem("b194952d-4732-4d79-a31f-a5eee60f8678");
			lineItem1.Quantity = new Measurement
			{
				Value = 1,
				Unit = new DummyCodeDescription
				{
					Code = Constants.Weight.Kilograms
				}
			};
			lineItem1.Invoice = new Invoice
			{
				Number = "INV0001",
				Date = new ZDateTime(2023, 07, 11),
				Amount = new Money
				{
					Amount = new ZDecimal(10.00m),
					Currency = new DummyCodeDescription
					{
						Code = "AUD"
					}
				}
			};
			lineItem1.ItemNumber = 100;
			lineItem1.MarksAndNumbers = "marks & nums (1)";
			lineItem1.GoodsDescription = "goods description (1)";
			lineItem1.OriginCode = FirstLineItemOriginCode;
			lineItem1.Origin = FirstLineItemOrigin;
			lineItem1.OriginCriterion = new CodeDescription(criterionList)
			{
				Code = criterionList.Count > 0 ? criterionList[0].Code : string.Empty
			};
			lineItem1.PackageCount = 20;
			lineItem1.HarmonizedCode = new HarmonizedCode
			{
				Code = "1234",
				Country = new DummyCountry
				{
					Code = "AU"
				}
			};

			var lineItem2 = CreateLineItem("d1893ae0-b09b-436a-adc1-6372291db6b1");
			lineItem2.Quantity = new Measurement
			{
				Value = 2,
				Unit = new DummyCodeDescription
				{
					Code = Constants.Weight.Kilograms
				}
			};
			lineItem2.Invoice = new Invoice
			{
				Number = "INV0001",
				Date = new ZDateTime(2023, 07, 11),
				Amount = new Money
				{
					Amount = new ZDecimal(10.00m),
					Currency = new DummyCodeDescription
					{
						Code = "AUD"
					}
				}
			};
			lineItem2.ItemNumber = 200;
			lineItem2.MarksAndNumbers = "marks & nums (1)";
			lineItem2.GoodsDescription = "goods description (2)";
			lineItem2.OriginCode = SecondLineItemOriginCode;
			lineItem2.Origin = SecondLineItemOrigin;
			lineItem2.OriginCriterion = new CodeDescription(criterionList)
			{
				Code = criterionList.Count > 0 ? criterionList[0].Code : string.Empty
			};
			lineItem2.PackageCount = 10;
			lineItem2.PackageType = new DummyCodeDescription
			{
				Code = "PKG",
				Description = "Package"
			};
			lineItem2.HarmonizedCode = new HarmonizedCode
			{
				Code = "2345",
				Country = new DummyCountry
				{
					Code = "NZ"
				}
			};

			// LineItems within 2nd Header
			var lineItem3 = CreateLineItem("5A05F21D-4264-4AC0-A342-9F0C95EB1424");
			lineItem3.Quantity = new Measurement
			{
				Value = 1,
				Unit = new DummyCodeDescription
				{
					Code = Constants.Weight.Kilograms
				}
			};
			lineItem3.Invoice = new Invoice
			{
				Number = "INV0002",
				Date = new ZDateTime(2023, 07, 11),
				Amount = new Money
				{
					Amount = new ZDecimal(13.37m),
					Currency = new DummyCodeDescription
					{
						Code = "AUD"
					}
				}
			};
			lineItem3.ItemNumber = 100;
			lineItem3.MarksAndNumbers = "marks & nums (2)";
			lineItem3.GoodsDescription = "goods description (1)";
			lineItem3.OriginCode = FirstLineItemOriginCode;
			lineItem3.Origin = FirstLineItemOrigin;
			lineItem3.OriginCriterion = new CodeDescription(criterionList)
			{
				Code = criterionList.Count > 0 ? criterionList[criterionList.Count - 1].Code : string.Empty
			};
			lineItem3.PackageCount = 20;
			lineItem3.HarmonizedCode = new HarmonizedCode
			{
				Code = "4567",
				Country = new DummyCountry
				{
					Code = "AU"
				}
			};

			var lineItem4 = CreateLineItem("E9457988-988B-429B-B973-071F6B64B6AF");
			lineItem4.Quantity = new Measurement
			{
				Value = 2,
				Unit = new DummyCodeDescription
				{
					Code = Constants.Weight.Kilograms
				}
			};
			lineItem4.Invoice = new Invoice
			{
				Number = "INV0002",
				Date = new ZDateTime(2023, 07, 11),
				Amount = new Money
				{
					Amount = new ZDecimal(13.37m),
					Currency = new DummyCodeDescription
					{
						Code = "AUD"
					}
				}
			};
			lineItem4.ItemNumber = 200;
			lineItem4.MarksAndNumbers = "marks & nums (2)";
			lineItem4.GoodsDescription = "goods description (2)";
			lineItem4.OriginCode = SecondLineItemOriginCode;
			lineItem4.Origin = SecondLineItemOrigin;
			lineItem4.OriginCriterion = new CodeDescription(criterionList)
			{
				Code = criterionList.Count > 0 ? criterionList[criterionList.Count - 1].Code : string.Empty
			};
			lineItem4.PackageCount = 10;
			lineItem4.PackageType = new DummyCodeDescription
			{
				Code = "PKG",
				Description = "Package"
			};
			lineItem4.HarmonizedCode = new HarmonizedCode
			{
				Code = "5678",
				Country = new DummyCountry
				{
					Code = "NZ"
				}
			};

			return new[]
			{
				lineItem1,
				lineItem2,
				lineItem3,
				lineItem4
			};
		}
	}
}
