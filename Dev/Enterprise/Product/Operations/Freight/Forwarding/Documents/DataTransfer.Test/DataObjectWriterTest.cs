using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	abstract class DataObjectWriterTest : TestCaseWithFactory
	{
		#region AssertUXml

		protected void AssertUXml(UniversalShipment universalShipment, string expectedXml)
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(universalShipment, stream);

				using (var reader = new StreamReader(stream))
				{
					var result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("UXml", expectedXml, result);
				}
			}
		}

		protected void AssertNotInUXml(UniversalShipment universalShipment, string notExpectedInXml)
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(universalShipment, stream);

				using (var reader = new StreamReader(stream))
				{
					var result = reader.ReadToEnd();
					AssertNotContains(notExpectedInXml, result);
				}
			}
		}

		protected void AssertInUXml(UniversalShipment universalShipment, string notExpectedInXml)
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(universalShipment, stream);

				using (var reader = new StreamReader(stream))
				{
					var result = reader.ReadToEnd();
					AssertContains(notExpectedInXml, result);
				}
			}
		}

		#endregion

		#region CreateAddress

		protected Address CreateAddress(string organizationType)
		{
			var dummyList = new CodeDescriptionPairList();
			dummyList.AddPair("AAA", "AAA desc");
			dummyList.AddPair("BBB", "BBB desc");

			return new Address(Factory)
			{
				AddressIdentifier = new ZGuid("939AC365-5FBB-41E0-8D89-3CBCA40264A0"),
				HeaderIdentifier = new ZGuid("DE645D17-5EA0-4EC2-8A53-0CF241F2AB27"),
				CompanyName = organizationType,
				AddressLine1 = $"{organizationType} address line 1",
				AddressLine2 = $"{organizationType} address line 2",
				AdditionalAddressInformation = $"{organizationType} additional info",
				City = $"{organizationType} city",
				State = $"{organizationType} state",
				Postcode = $"{organizationType} postcode",
				Phone = $"{organizationType} phone",
				Fax = $"{organizationType} fax",
				Email = $"{organizationType} email",
				Contact = $"{organizationType} contact",
				TaxNumber = $"{organizationType} tax number",
				TaxNumberType = new DummyCodeDescription
				{
					Code = "GST",
					Description = "Goods and Services Tax"
				},
				Country = new Country(Context.Factory, Context.Countries)
				{
					Code = "AU"
				},
				Unloco = new Unloco(Context.Factory, Context.Unlocos, Context.Countries)
				{
					Code = "AUSYD"
				},
				RegistrationNumbers = new[]
				{
					new DummyRegistrationNumber
					{
						Type = new CodeDescription(dummyList)
						{
							Code = "AAA"
						},
						CountryOfIssue = new Country(Context.Factory, Context.Countries)
						{
							Code = "NZ"
						},
						Value = "12345"
					}
				}
			};
		}

		#endregion

		#region CreateTaxInfo

		protected TaxInfo CreateTaxInfo()
		{
			return new TaxInfo
			{
				Code = "VAT",
				Description = "Tax Identification Number",
				Number = "TAX123456"
			};
		}

		#endregion

		#region CreateRegistrationNumber

		protected virtual RegistrationNumber CreateRegistrationNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = type
				},
				Value = value
			};
		}

		#endregion

		#region CreatePackingLine

		protected BookingPackingLine CreatePackingLine()
		{
			var packingLine = new BookingPackingLine(ZGuid.NewZGuid());

			packingLine.PackingLineID = "1";
			packingLine.Quantity = 225;
			packingLine.PackageType = new DummyCodeDescription
			{
				Code = "PKG",
				Description = "Package"
			};
			packingLine.Weight = new Measurement()
			{
				Value = 7896.670,
				Unit = new DummyCodeDescription
				{
					Code = "KG",
					Description = "Kilograms"
				}
			};
			packingLine.Volume = new Measurement()
			{
				Value = 33,
				Unit = new DummyCodeDescription
				{
					Code = "M3",
					Description = "Cubic Meters"
				}
			};
			packingLine.Height = new Measurement()
			{
				Value = 56,
				Unit = new DummyCodeDescription
				{
					Code = "CM",
					Description = "Centimeters"
				}
			};
			packingLine.Width = new Measurement()
			{
				Value = 57,
				Unit = new DummyCodeDescription
				{
					Code = "CM",
					Description = "Centimeters"
				}
			};
			packingLine.Length = new Measurement()
			{
				Value = 58,
				Unit = new DummyCodeDescription
				{
					Code = "CM",
					Description = "Centimeters"
				}
			};

			packingLine.GoodsDescription = "Goods Description";
			packingLine.MarksAndNumbers = "marks & nums";
			packingLine.ReferenceNumber = "reference number";
			packingLine.ImportReferenceNumber = "import reference number";

			return packingLine;
		}

		#endregion

		#region Implementation

		protected IContext Context => context ?? (context = new CommonContext(Factory));
		IContext context;

		#endregion
	}
}
