using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLineLinkControllingMsgHeaderCollection))]
	public sealed class InvoiceLineLinkControllingMsgHeaderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InvoiceLineLinkControllingMsgHeaderCollection>
	{
		protected override InvoiceLineLinkControllingMsgHeaderCollection GetCollectionToTest()
		{
			return new InvoiceLineLinkControllingMsgHeaderCollection(Factory.New<JobComInvoiceLine>(), null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			return new InvoiceLineLinkControllingMsgHeader(controllingMessageHeader, invoiceLine);
		}

		public void TestRebuildElements()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";
			var invoiceLineLinkControllingMsgHeaders = new InvoiceLineLinkControllingMsgHeaderCollection(line, null);
			AssertEquals(0, invoiceLineLinkControllingMsgHeaders.Count);
			invoiceLineLinkControllingMsgHeaders.RebuildElements();
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);
			controllingMessageHeader.Delete();
			AssertEquals(2, invoiceLineLinkControllingMsgHeaders.Count);
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);
		}

		public void TestClearAndBuildElements()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";
			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);
			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;
			Factory.Save();
			AssertEquals(3, GetLinkPKs(Factory, line.PK).Length);
			var newFactory = new BusinessObjectFactory();
			line = newFactory.Load<JobComInvoiceLine>(line.PK);
			invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			invoiceLineLinkControllingMsgHeaders.ClearAndBuildElements();
			var genPivot = GetGenPivots(Factory, line.PK).First();
			newFactory.Save();
			AssertNull(Factory.Load<GenPivot>(genPivot.PK));
		}

		public void TestSaveWhenDeleteInvoiceLine()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";
			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);
			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;
			AssertEquals(3, GetLinkPKs(Factory, line.PK).Length);
			Factory.Save();
			AssertEquals(3, GetLinkPKs(Factory, line.PK).Length);
			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = false;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = false;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = false;
			AssertEquals(0, GetLinkPKs(Factory, line.PK).Length);
			Factory.Save();
			AssertEquals(0, GetLinkPKs(Factory, line.PK).Length);
			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			line = newFactory.Load<JobComInvoiceLine>(line.PK);
			invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);
			var linkPKs = GetLinkPKs(Factory, line.PK);
			Assert(linkPKs.Contains(controllingMessageHeaders[0].PK));
			Assert(linkPKs.Contains(controllingMessageHeaders[1].PK));
			Assert(linkPKs.Contains(controllingMessageHeaders[2].PK));
			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = false;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = false;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;
			newFactory.Save();
			linkPKs = GetLinkPKs(Factory, line.PK);
			AssertEquals(1, linkPKs.Length);
			Assert(!linkPKs.Contains(invoiceLineLinkControllingMsgHeaders[0].ControllingMessageHeaderPK));
			Assert(!linkPKs.Contains(invoiceLineLinkControllingMsgHeaders[1].ControllingMessageHeaderPK));
			Assert(linkPKs.Contains(invoiceLineLinkControllingMsgHeaders[2].ControllingMessageHeaderPK));
			var genPivot = GetGenPivots(Factory, line.PK).First();
			line.Delete();
			newFactory.Save();
			AssertNull(Factory.Load<GenPivot>(genPivot.PK));
		}

		public void TestSaveWhenDeleteInvoice()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "20";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";
			controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "CI";
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX03";
			controllingMessageHeader.PermitNumber = "330";
			var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders;
			AssertEquals(3, invoiceLineLinkControllingMsgHeaders.Count);
			invoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[1].IsLinkedCMHeader = true;
			invoiceLineLinkControllingMsgHeaders[2].IsLinkedCMHeader = true;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<JobComInvoiceHeader>(header.PK);
			var genPivot = GetGenPivots(Factory, line.PK).First();
			header.Delete();
			newFactory.Save();
			AssertNull(Factory.Load<GenPivot>(genPivot.PK));
		}

		public void TestSuspendLinkSettingOnLoading()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var cmHeader = controllingMessageHeaders.AddNew();
			cmHeader.TW1_CertificateType = "15";
			line.JI_InvoiceQuantity = 123m;
			line.JI_InvoiceUQ = "UNT";
			line.JI_Tariff = "123456789";
			line.AddInfoChild.TWL_DocumentaryUnitPrice = 222m;

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			CombineAssertions("Default values from Invoice Line when ", () =>
			{
				AssertEquals("JI_PermitQty", 123m, line.JI_PermitQty);
				AssertEquals("JI_PermitUQ", ZString.Empty, line.JI_PermitUQ);
				AssertEquals("JI_PermitUnitPrice", 222m, line.JI_PermitUnitPrice);
				AssertEquals("JI_IMPTariff", "12345678", line.JI_IMPTariff);
			});

			line.JI_PermitQty = 234m;
			line.JI_PermitUQ = "KG";
			line.JI_PermitUnitPrice = 333m;
			line.JI_IMPTariff = "23456789";
			Factory.Save();

			var lineReloaded = new BusinessObjectFactory().Load<JobComInvoiceLine>(line.PK);
			_ = lineReloaded.InvoiceLineLinkControllingMsgHeaders;
			CombineAssertions("Should not default values from Invoice Line on Loaded", () =>
			{
				AssertEquals("JI_PermitQty", 234m, lineReloaded.JI_PermitQty);
				AssertEquals("JI_PermitUQ", "KG", lineReloaded.JI_PermitUQ);
				AssertEquals("JI_PermitUnitPrice", 333m, lineReloaded.JI_PermitUnitPrice);
				AssertEquals("JI_IMPTariff", "23456789", lineReloaded.JI_IMPTariff);
			});
		}

		public void TestSuspendValidationOnLoading()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var cmHeader = controllingMessageHeaders.AddNew();
			cmHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cmHeader.TW1_CertificateType = "15";
			for (int i = 0; i <= 20; i++)
			{
				var line = jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
				line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			}
			Factory.Save();

			var declarationReloaded = new BusinessObjectFactory().Load<JobDeclaration>(jobDeclartion.PK);
			_ = declarationReloaded.CusEntryInstruction.ControllingMessageHeaders[0].ControllingMessageHeaderLinkInvoiceLines;
			foreach (JobComInvoiceLine lineReloaded in declarationReloaded.InvoiceLines)
			{
				AssertNoNotifications(lineReloaded.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeaderInfo);
			}
		}

		public static ZGuid[] GetLinkPKs(BusinessObjectFactory factory, ZGuid pk)
		{
			return GetGenPivots(factory, pk).Select(x => x.XX_Relation2ID).Distinct().ToArray();
		}

		public static GenPivot[] GetGenPivots(BusinessObjectFactory factory, ZGuid pk)
		{
			var genPivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.InvoiceLineRelatedControllingMessageHeaderPivot);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, pk);
			return factory.Load<GenPivot>(genPivotQuery).ToArray();
		}

		public class ControllingMsgHeaderTestHelper
		{
			public ControllingMsgHeaderTestHelper(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;
			public JobDeclaration New(string[] controllingAgencys)
			{
				var jobDeclartion = factory.NewWithValidTestData<JobDeclaration>();
				jobDeclartion.JE_MessageType = "IMP";
				var entryInstruction = jobDeclartion.CusEntryInstruction;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				AddControllingAgencysTo(controllingMessageHeaders, controllingAgencys);
				return jobDeclartion;
			}

			public static void AddControllingAgencysTo(CusTWControllingMessageHeaderCollection controllingMessageHeaders, string[] controllingAgencys)
			{
				foreach (var controllingAgency in controllingAgencys)
				{
					var controllingMessageHeader = controllingMessageHeaders.AddNew();
					controllingMessageHeader.TW1_ControllingAgency = controllingAgency;
					controllingMessageHeader.TW1_FunctionalReferenceId = controllingAgency;
					controllingMessageHeader.PermitNumber = controllingAgency;
				}
			}

			public static void AssertReadOnlyByControllingAgency(JobComInvoiceLine line, ZPropertyInfo linePropertyInfo, ZString controllingAgency)
			{
				ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, true);
				Assert(!linePropertyInfo.ReadOnly);
				ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, false);
				Assert(linePropertyInfo.ReadOnly);
			}

			public static void AssertReadOnlyAndEmptyByControllingAgency(JobComInvoiceLine line, ZPropertyInfo linePropertyInfo, ZString controllingAgency)
			{
				SetControllingAgencyIsForCAHeader(line, controllingAgency, true);
				Assert(!linePropertyInfo.ReadOnly);
				IZType value = new ZString("1");
				if (linePropertyInfo.PropertyType == typeof(ZDateTime))
				{
					value = ZDateTime.Now;
				}
				else if (linePropertyInfo.PropertyType == typeof(ZDecimal))
				{
					value = new ZDecimal(1);
				}
				else if (linePropertyInfo.PropertyType == typeof(ZInt))
				{
					value = new ZInt(1);
				}
				else if (linePropertyInfo.PropertyType == typeof(ZBool))
				{
					value = ZBool.True;
				}

				linePropertyInfo.Value = value;
				Assert(!linePropertyInfo.Value.IsEmpty);
				SetControllingAgencyIsForCAHeader(line, controllingAgency, false);
				Assert(linePropertyInfo.ReadOnly);
				Assert(linePropertyInfo.Value.IsEmpty);
			}

			public static void AssertNoReadOnlyByControllingAgency(JobComInvoiceLine line, ZPropertyInfo linePropertyInfo, ZString controllingAgency)
			{
				ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, true);
				Assert(linePropertyInfo.ReadOnly);
				ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, false);
				Assert(linePropertyInfo.ReadOnly);
			}

			public static void AssertReadOnlyAndClearValueWhenSwitchToReadOnlyByControllingAgency(JobComInvoiceLine line, ZPropertyInfo linePropertyInfo, ZString controllingAgency)
			{
				SetControllingAgencyIsForCAHeader(line, controllingAgency, true);
				linePropertyInfo.Value = new ZString("1");
				Assert(!linePropertyInfo.Value.IsEmpty);
				SetControllingAgencyIsForCAHeader(line, controllingAgency, false);
				Assert(linePropertyInfo.ReadOnly);
				Assert(linePropertyInfo.Value.IsEmpty);
			}

			public static void AssertReadOnlyAndClearValueWhenSwitchToReadOnlyByControllingAgency(JobComInvoiceLine line, BusinessObjectCollection collection, ZString controllingAgency)
			{
				ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, true);
				Assert(!collection.ReadOnly);
				collection.AddNew();
				Assert(collection.Count > 0);
				ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, false);
				Assert(collection.ReadOnly);
				Assert(collection.Count == 0);
			}

			public static void AssertNoReadOnlyByControllingAgency(JobComInvoiceLine line, BusinessObjectCollection collection, ZString controllingAgency)
			{
				ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, true);
				Assert(collection.ReadOnly);
				ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, controllingAgency, false);
				Assert(collection.ReadOnly);
			}

			static void SetMessageTypeIsForCAHeader(JobComInvoiceLine line, ZString messageType, ZBool value)
			{
				var caHeader = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.MessageType == messageType);
				if (caHeader != null)
				{
					caHeader.IsLinkedCMHeader = value;
				}
			}

			public static void AssertHasMessageErrorContainingByMessageType(JobComInvoiceLine line, ZPropertyInfo line1PropertyInfo, IZType value, IZType emptyValue, ZString messageType, ZString message)
			{
				line1PropertyInfo.Value = value;
				line1PropertyInfo.Value = emptyValue;
				AssertNoMessageErrorContaining(line1PropertyInfo, message);
				SetMessageTypeIsForCAHeader(line, messageType, true);
				AssertHasMessageErrorContaining(line1PropertyInfo, message);
				line1PropertyInfo.Value = value;
				AssertNoMessageErrorContaining(line1PropertyInfo, message);
				SetMessageTypeIsForCAHeader(line, messageType, false);
				line1PropertyInfo.Value = emptyValue;
			}

			public static void SetControllingAgencyIsForCAHeader(JobComInvoiceLine line, ZString controllingAgency, ZBool value)
			{
				var caHeader = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == controllingAgency);
				if (caHeader != null)
				{
					caHeader.IsLinkedCMHeader = value;
				}
			}

			public static void SetControllingAgencyIsForCAHeader(JobComInvoiceLine line, ZString controllingAgency, ZBool value, ZString messageType)
			{
				var caHeader = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingAgency == controllingAgency);
				if (caHeader != null)
				{
					caHeader.IsLinkedCMHeader = value;
					if (caHeader.ControllingMessageHeader is CusTWControllingMessageHeader controllingMessageHeader)
					{
						controllingMessageHeader.TW1_ControllingMessageType = messageType;
					}
				}
			}

			public static void AssertHasMessageErrorContainingByControllingAgency(JobComInvoiceLine line1, ZPropertyInfo line1PropertyInfo, IZType value, IZType emptyValue, ZString controllingAgency, ZString message)
			{
				line1PropertyInfo.Value = value;
				line1PropertyInfo.Value = emptyValue;
				AssertNoMessageErrorContaining(line1PropertyInfo, message);
				SetControllingAgencyIsForCAHeader(line1, controllingAgency, true);
				AssertHasMessageErrorContaining(line1PropertyInfo, message);
				line1PropertyInfo.Value = value;
				AssertNoMessageErrorContaining(line1PropertyInfo, message);
				SetControllingAgencyIsForCAHeader(line1, controllingAgency, false);
				line1PropertyInfo.Value = emptyValue;
			}

			public static void AssertNoMessageErrorContainingByControllingAgency(JobComInvoiceLine line1, ZPropertyInfo line1PropertyInfo, IZType value, IZType emptyValue, ZString controllingAgency, ZString message)
			{
				line1PropertyInfo.Value = value;
				line1PropertyInfo.Value = emptyValue;
				AssertNoMessageErrorContaining(line1PropertyInfo, message);
				SetControllingAgencyIsForCAHeader(line1, controllingAgency, true);
				AssertNoMessageErrorContaining(line1PropertyInfo, message);
				line1PropertyInfo.Value = value;
				AssertNoMessageErrorContaining(line1PropertyInfo, message);
				SetControllingAgencyIsForCAHeader(line1, controllingAgency, false);
				line1PropertyInfo.Value = emptyValue;
			}
		}
	}
}
