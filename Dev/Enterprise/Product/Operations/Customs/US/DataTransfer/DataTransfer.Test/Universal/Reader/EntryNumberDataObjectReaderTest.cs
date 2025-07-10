using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestEntryNumberFilled_EntryNumberMatch_CanLinkEntryLine()
		{
			using (TemporarilySetCountryAndInterfaced())
			{
				var declarationObject = SetupDeclaration("BOB1", "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>()
				{
					new UniversalCustoms.EntryHeader()
					{
						Type = new EntryType() { Code = "ENS" },
						EntryLineCollection = new List<UniversalCustoms.EntryLine>() { new UniversalCustoms.EntryLine() { LineNumber = 1 } }
					}
				});
				var addInfos = new Dictionary<ZString, ZString>();
				UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryType, (ZString)"06");
				UpdateAddInfo(addInfos, USAddInfoSchema.US_EntryFilerCode, (ZString)"CJ6");
				var builder = new ZStringBuilder(addInfos.Select((KeyValuePair<ZString, ZString> pair) => AddInfoParser.Serialise(pair.Key, pair.Value)));
				declarationObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(builder.ToString()));

				var commercialInvoiceHeader = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INVOICE 1",
				};
				commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
				{
					new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, EntryLineNumber = 1, EntryNumber = "00006678" }
				}));
				declarationObject.CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { commercialInvoiceHeader })
				};
				declarationObject.SetEntryNumberCollection(() => new List<EntryNumber>());
				declarationObject.EntryNumberCollection.Add(new EntryNumber()
				{
					Number = "00006678",
					Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
					EntryIsSystemGenerated = true,
				});
				var message = GetQueuedUniversalShipmentMessage(declarationObject);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration (Master Bill='MB2343') from UniversalShipment.
Successfully saved Declaration B00001000 with 1 x CusEntryLine, 1 x CusEntryHeader.".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching JobDeclaration found, creating new JobDeclaration.
Populating JobDeclaration...
No matching CusEntryHeader found, creating new CusEntryHeader.
Populating CusEntryHeader...
No matching CusEntryLine found, creating new CusEntryLine.
Populating CusEntryLine...
Added Declaration (Master Bill='MB2343') from UniversalShipment.
Successfully saved Declaration B00001000 with 1 x CusEntryLine, 1 x CusEntryHeader.
".Trim(), message.GetLogNoteText());

				var reloadBO = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
				AssertNotNull(reloadBO);
				AssertEquals("00006678", reloadBO.ImportEntryNumber);
				var entry = reloadBO.FormalEntry;
				AssertEquals("Entry Line Count", 1, entry.AllEntryLines.Count);
			}
		}

		public void TestEntryNumberFilled_EntryNumberNotMatch_CanNotLinkEntryLine()
		{
			using (TemporarilySetCountryAndInterfaced())
			{
				var declarationObject = SetupDeclaration("BOB1", "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>()
				{
					new UniversalCustoms.EntryHeader()
					{
						Type = new EntryType() { Code = "ENS" },
						EntryLineCollection = new List<UniversalCustoms.EntryLine>() { new UniversalCustoms.EntryLine() { LineNumber = 1 } }
					}
				});
				var commercialInvoiceHeader = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INVOICE 1"
				};
				commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
				{
					new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, EntryLineNumber = 1, EntryNumber = "00006688" },
				}));
				declarationObject.CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { commercialInvoiceHeader })
				};
				declarationObject.SetEntryNumberCollection(() => new List<EntryNumber>());
				declarationObject.EntryNumberCollection.Add(new EntryNumber()
				{
					Number = "00006678",
					Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
					EntryIsSystemGenerated = true,
				});
				var message = GetQueuedUniversalShipmentMessage(declarationObject);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - No Entry Line was matched to Commercial Invoice Line '1' (InvoiceNumber='INVOICE 1', EntryNumber='00006688', EntryLineNumber='1').
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching JobDeclaration found, creating new JobDeclaration.
Populating JobDeclaration...
No matching CusEntryHeader found, creating new CusEntryHeader.
Populating CusEntryHeader...
No matching CusEntryLine found, creating new CusEntryLine.
Populating CusEntryLine...
Error - No Entry Line was matched to Commercial Invoice Line '1' (InvoiceNumber='INVOICE 1', EntryNumber='00006688', EntryLineNumber='1').
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), message.GetLogNoteText());
			}
		}

		static IDisposable TemporarilySetCountryAndInterfaced(string countryCode = Core.Constants.CountryCodes.UnitedStates)
		{
			IDisposable temporarilySetCountryToChina = null;
			IDisposable setTemporaryLocalCountryCustomsInterfaceValue = null;
			return new DisposableAction(() =>
			{
				temporarilySetCountryToChina = GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode);
				var customsInterface = new LocalCountryCustomsInterface
				{
					RecipientID = "RecipientID",
					SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced
				};
				setTemporaryLocalCountryCustomsInterfaceValue = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			}, () =>
			{
				setTemporaryLocalCountryCustomsInterfaceValue.Dispose();
				temporarilySetCountryToChina.Dispose();
			});
		}
	}
}
