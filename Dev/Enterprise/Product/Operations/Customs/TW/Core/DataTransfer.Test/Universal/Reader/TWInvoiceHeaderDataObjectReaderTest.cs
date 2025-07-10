using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using WTG.NUnit;
using Core = Enterprise.Core;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	[TestedType(typeof(TWInvoiceHeaderDataObjectReader))]
	sealed class TWInvoiceHeaderDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestFillInvoiceLineDetailedDescription()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				invoiceLineData.DetailedDescription = "Detailed Description";

				var logger = new TestErrorLogger();
				var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
				var input = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance).AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { invoiceLineData }));
				var output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				var line = output.InvoiceLines[0] as JobComInvoiceLine;

				NUnit.Framework.Assert.That(line.JI_DeclarationGoodsDescription, Is.EqualTo("Detailed Description").Using(CustomComparers.TypeComparison));

				invoiceLineData.DetailedDescription = ZString.Empty;
				output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				line = output.InvoiceLines[0] as JobComInvoiceLine;

				AssertNullOrEmpty(line.JI_DeclarationGoodsDescription);

				invoiceLineData.DetailedDescription = null;
				output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				line = output.InvoiceLines[0] as JobComInvoiceLine;

				AssertNullOrEmpty(line.JI_DeclarationGoodsDescription);
			}
		}

		[ExpectNoExceptions]
		public void TestFillInvoiceLineAddInfoCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				invoiceLineData.AddInfoCollection.Add(new AddInfo { Key = "PreviousPermitNumber", Value = "PRE1" });

				var logger = new TestErrorLogger();
				var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
				var input = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance).AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { invoiceLineData }));
				var output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				var line = output.InvoiceLines[0] as JobComInvoiceLine;

				NUnit.Framework.Assert.That(line.PreviousPermitNo, Is.EqualTo("PRE1").Using(CustomComparers.TypeComparison));

				invoiceLineData.AddInfoCollection.Clear();
				output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				line = output.InvoiceLines[0] as JobComInvoiceLine;

				NUnit.Framework.Assert.That(line.PreviousPermitNo, Is.EqualTo("").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestFillChassisNumbers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Reference = "0" });
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Type = new CodeDescriptionPair { Code = "CCN" }, Reference = "1" });
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Type = new CodeDescriptionPair { Code = "CCN" }, Reference = "2" });

				var logger = new TestErrorLogger();
				var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
				var input = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance).AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { invoiceLineData }));
				var output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				var line = output.InvoiceLines[0] as JobComInvoiceLine;

				NUnit.Framework.Assert.That(line.ChassisJobComInvLineRefsCollection.Count, Is.EqualTo(2));
				NUnit.Framework.Assert.That(line.ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>().Any(num => num.JG_ReferenceNumber == "1"), Is.True);
				NUnit.Framework.Assert.That(line.ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>().Any(num => num.JG_ReferenceNumber == "2"), Is.True);

				invoiceLineData.CustomsReferenceCollection.Clear();
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Type = new CodeDescriptionPair { Code = "CCN" }, Reference = "3" });
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Type = new CodeDescriptionPair { Code = "CCN" }, Reference = null });

				output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				line = output.InvoiceLines[0] as JobComInvoiceLine;

				NUnit.Framework.Assert.That(line.ChassisJobComInvLineRefsCollection.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(line.ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>().Any(num => num.JG_ReferenceNumber == "3"), Is.True);
			}
		}

		[ExpectNoExceptions]
		public void TestFillAssignedNumbers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Reference = "ASSIGNED NUMBER0" });
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Type = new CodeDescriptionPair { Code = "AGA" }, Reference = "ASSIGNED NUMBER1" });
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Type = new CodeDescriptionPair { Code = "AGA" }, Reference = "ASSIGNED NUMBER2" });

				var logger = new TestErrorLogger();
				var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
				var input = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance).AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { invoiceLineData }));
				var output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				var line = output.InvoiceLines[0] as JobComInvoiceLine;

				NUnit.Framework.Assert.That(line.AssignedJobComInvLineRefsCollection.Count, Is.EqualTo(2));
				NUnit.Framework.Assert.That(line.AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().Any(num => num.JG_ReferenceNumber == "ASSIGNED NUMBER1"), Is.True);
				NUnit.Framework.Assert.That(line.AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().Any(num => num.JG_ReferenceNumber == "ASSIGNED NUMBER2"), Is.True);

				invoiceLineData.CustomsReferenceCollection.Clear();
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Type = new CodeDescriptionPair { Code = "AGA" }, Reference = "ASSIGNED NUMBER3" });
				invoiceLineData.CustomsReferenceCollection.Add(new CustomsReference { Type = new CodeDescriptionPair { Code = "AGA" }, Reference = null });

				output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.InvoiceLines.Count, Is.EqualTo(1));
				line = output.InvoiceLines[0] as JobComInvoiceLine;

				NUnit.Framework.Assert.That(line.AssignedJobComInvLineRefsCollection.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(line.AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().Any(num => num.JG_ReferenceNumber == "ASSIGNED NUMBER3"), Is.True);
			}
		}

		[ExpectNoExceptions]
		public void TestFillMarksAndNumbers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var logger = new TestErrorLogger();
				var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
				var aLength514 = new ZString('A', 514);
				var input = new CommercialInvoiceHeader { MarksAndNumbers = aLength514 };
				var output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;
				NUnit.Framework.Assert.That(output.JZ_MarksAndNumbers, Is.EqualTo(new ZString('A', 512)));
				NUnit.Framework.Assert.That(output.TW_MarksAndNumbers, Is.EqualTo(aLength514));
			}
		}

		[ExpectNoExceptions]
		public void TestFillSupplierDocumentaryAddressIfNeeded()
		{
			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "SUP01";

			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV001",
				Supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance),
			};

			invoiceHeaderDataObject.Supplier.OrganizationCode = new ZCodeMappedZString("SUP01");
			invoiceHeaderDataObject.OrganizationAddressCollection = null;
			Factory.SaveForTesting();

			var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
			var reader = new TWInvoiceHeaderDataObjectReader(groupHeader, invoiceHeaderDataObject, logger, helper);
			var invoiceBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var newInvoiceBO = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoiceBO.PK);
				NUnit.Framework.Assert.That(newInvoiceBO.DocAddresses.Count, Is.EqualTo(1), "Count");
				NUnit.Framework.Assert.That(newInvoiceBO.SupplierDocumentaryAddress.E2_OA_Address, Is.EqualTo(consigneeOrg.MainAddress.PK), "E2_OA_Address");
			});
		}

		[ExpectNoExceptions]
		public void TestFillBuyerDocumentaryAddressIfNeeded()
		{
			var consignorOrg = Factory.New<OrgHeader>();
			consignorOrg.OH_Code = "BUY01";

			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV001",
				Buyer = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance),
			};

			invoiceHeaderDataObject.Buyer.OrganizationCode = new ZCodeMappedZString("BUY01");
			invoiceHeaderDataObject.OrganizationAddressCollection = null;
			Factory.SaveForTesting();

			var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
			var reader = new TWInvoiceHeaderDataObjectReader(groupHeader, invoiceHeaderDataObject, logger, helper);
			var invoiceBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var newInvoiceBO = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoiceBO.PK);
				NUnit.Framework.Assert.That(newInvoiceBO.DocAddresses.Count, Is.EqualTo(1), "Count");
				NUnit.Framework.Assert.That(newInvoiceBO.BuyerDocumentaryAddress.E2_OA_Address, Is.EqualTo(consignorOrg.MainAddress.PK), "E2_OA_Address");
			});
		}

		public void TestCreateNewCustomsSupportingInformationCollectionDataObjectReader()
		{
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV001",
			};
			var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
			var reader = new TWInvoiceHeaderDataObjectReaderForTest(groupHeader, invoiceHeaderDataObject, logger, helper);
			AssertType<Enterprise.Customs.TW.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader>(reader.CreateNewCustomsSupportingInformationCollectionDataObjectReader_Exposed());
		}

		public void TestPermitCusSupportingItemNumberSetterSuspender()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var category = new CodeDescriptionPair { Code = "CPC", Description = "Permit Number" };
				invoiceLineData.CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>
				{
					new()
					{
						Category = category,
						ItemNumber = 1,
						LineNo = 11,
						ReferenceNumber = "PER00000000001"
					},
					new()
					{
						Category = category,
						ItemNumber = 2,
						LineNo = 22,
						ReferenceNumber = "PER00000000002"
					},
					new()
					{
						Category = category,
						ItemNumber = 3,
						LineNo = 33,
						ReferenceNumber = "PER00000000003"
					},
					new()
					{
						Category = category,
						ItemNumber = 4,
						LineNo = 44,
						ReferenceNumber = "PER00000000004"
					},
					new()
					{
						Category = category,
						ItemNumber = 5,
						LineNo = 55,
						ReferenceNumber = "PER00000000005"
					}
				};

				var logger = new TestErrorLogger();
				var helper = new TWDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan);
				var input = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance).AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { invoiceLineData }));
				var output = new TWInvoiceHeaderDataObjectReader(groupHeader, input, logger, helper).ReadIntoBusinessObject() as JobComInvoiceHeader;

				CombineAssertions(() =>
				{
					AssertEquals("Line count", 1, output.InvoiceLines.Count);

					var collection = new PermitCusSupportingCollection(output.InvoiceLines[0]);
					collection.Load();
					var permitCusSupportings = collection.Cast<PermitCusSupporting>().ToList();
					AssertEquals("CPC count", 5, permitCusSupportings.Count);
					AssertEquals("CSI_ItemNumber 1", 1, permitCusSupportings[0].CSI_ItemNumber);
					AssertEquals("CSI_ReferenceNumber 1", "PER00000000001", permitCusSupportings[0].CSI_ReferenceNumber);
					AssertEquals("CSI_ItemNumber 2", 2, permitCusSupportings[1].CSI_ItemNumber);
					AssertEquals("CSI_ReferenceNumber 2", "PER00000000002", permitCusSupportings[1].CSI_ReferenceNumber);
					AssertEquals("CSI_ItemNumber 3", 3, permitCusSupportings[2].CSI_ItemNumber);
					AssertEquals("CSI_ReferenceNumber 3", "PER00000000003", permitCusSupportings[2].CSI_ReferenceNumber);
					AssertEquals("CSI_ItemNumber 4", 4, permitCusSupportings[3].CSI_ItemNumber);
					AssertEquals("CSI_ReferenceNumber 4", "PER00000000004", permitCusSupportings[3].CSI_ReferenceNumber);
					AssertEquals("CSI_ItemNumber 5", 5, permitCusSupportings[4].CSI_ItemNumber);
					AssertEquals("CSI_ReferenceNumber 5", "PER00000000005", permitCusSupportings[4].CSI_ReferenceNumber);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			invoiceLineData = new CommercialInvoiceLine
			{
				AddInfoGroupCollection = new List<AddInfoGroup>(),
				CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>(),
				AddInfoCollection = new List<AddInfo>(),
				CustomsReferenceCollection = new List<CustomsReference>()
			};

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "BLT";
			groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();

			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			var permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			permitCusSupporting.CSI_LineNo = 1;
			permitCusSupporting.CSI_ReferenceNumber = "REF1";

			var chassis = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "REF2";

			var assigned = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assigned.JG_ReferenceNumber = "REF3";

			invoiceLine.CertificateOfOriginCusSupporting.CSI_LineNo = 4;
			invoiceLine.CertificateOfOriginCusSupporting.CSI_ReferenceNumber = "REF4";

			invoiceLine.PrePermitNoCusSupporting.CSI_LineNo = 5;
			invoiceLine.PrePermitNoCusSupporting.CSI_ReferenceNumber = "REF5";

			invoiceLine.PreviousBondedCusSupporting.CSI_LineNo = 6;
			invoiceLine.PreviousBondedCusSupporting.CSI_ReferenceNumber = "REF6";
		}

		JobComInvoiceGroupHeader groupHeader;
		CommercialInvoiceLine invoiceLineData;
	}

	class TWInvoiceHeaderDataObjectReaderForTest : TWInvoiceHeaderDataObjectReader
	{
		public TWInvoiceHeaderDataObjectReaderForTest(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null) : base(groupHeader, invoiceDataObject, logger, helper, topLevelObject, landedCostDataReader)
		{
		}

		public Enterprise.Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader_Exposed()
		{
			return base.CreateNewCustomsSupportingInformationCollectionDataObjectReader();
		}
	}
}
