using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USImportEntryInvoiceLine))]
	sealed class USImportEntryInvoiceLineTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2020, 3, 30)]
		public void TestIInvoiceLine_EffectiveDates()
		{
			var dataACEIMPDutyDate = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 4, 1), new ZDate(2020, 4, 2), new ZDate(2020, 4, 3), new ZDate(2020, 4, 4), new ZDate(2020, 4, 5), new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEIMPITDate = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, ZDate.Empty, new ZDate(2020, 4, 2), new ZDate(2020, 4, 3), new ZDate(2020, 4, 4), new ZDate(2020, 4, 5), new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEIMPEstimatedEntryDate = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, ZDate.Empty, ZDate.Empty, new ZDate(2020, 4, 3), new ZDate(2020, 4, 4), new ZDate(2020, 4, 5), new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEIMPPresentationDate = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, ZDate.Empty, ZDate.Empty, ZDate.Empty, new ZDate(2020, 4, 4), new ZDate(2020, 4, 5), new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEIMPEntryAuthorisationDate = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, new ZDate(2020, 4, 5), new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEIMPPreliminaryStatementPrintDate = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEIMPEntryDate = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEIMPToday = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, ZDate.Empty, new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEIMPDutyDatePrivilegedForeign = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 4, 1), new ZDate(2020, 4, 2), new ZDate(2020, 4, 3), new ZDate(2020, 4, 4), new ZDate(2020, 4, 5), new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.PrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEFTZ = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.FTZ, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 4, 1), new ZDate(2020, 4, 2), new ZDate(2020, 4, 3), new ZDate(2020, 4, 4), new ZDate(2020, 4, 5), new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), new ZDate(2020, 4, 8)), ZoneStatusList.Codes.NonPrivilegedForeign, new ZDate(2020, 4, 9));
			var dataACEFTZPrivilegedForeign = SetupInvoiceLinePrivilegedData(SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.FTZ, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 4, 1), new ZDate(2020, 4, 2), new ZDate(2020, 4, 3), new ZDate(2020, 4, 4), new ZDate(2020, 4, 5), new ZDate(2020, 4, 6), new ZDate(2020, 4, 7), ZDate.Empty), ZoneStatusList.Codes.PrivilegedForeign, new ZDate(2020, 4, 9));
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertIInvoiceLine_EffectiveDates("1 ", dataACEIMPDutyDate.invoiceLine, new ZDate(2020, 4, 1), new ZDate(2020, 4, 1), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("2 ", dataACEIMPITDate.invoiceLine, new ZDate(2020, 4, 2), new ZDate(2020, 4, 2), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("3 ", dataACEIMPEstimatedEntryDate.invoiceLine, new ZDate(2020, 4, 3), new ZDate(2020, 4, 3), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("4 ", dataACEIMPPresentationDate.invoiceLine, new ZDate(2020, 4, 5), new ZDate(2020, 4, 5), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("5 ", dataACEIMPEntryAuthorisationDate.invoiceLine, new ZDate(2020, 4, 5), new ZDate(2020, 4, 5), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("6 ", dataACEIMPPreliminaryStatementPrintDate.invoiceLine, new ZDate(2020, 4, 6), new ZDate(2020, 4, 6), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("7 ", dataACEIMPEntryDate.invoiceLine, new ZDate(2020, 4, 7), new ZDate(2020, 4, 7), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("8 ", dataACEIMPToday.invoiceLine, new ZDate(2020, 3, 30), new ZDate(2020, 3, 30), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("9 ", dataACEIMPDutyDatePrivilegedForeign.invoiceLine, new ZDate(2020, 4, 9), new ZDate(2020, 4, 1), ZDate.Invalid);
				AssertIInvoiceLine_EffectiveDates("10 ", dataACEFTZ.invoiceLine, new ZDate(2020, 4, 8), new ZDate(2020, 4, 1), new ZDate(2020, 4, 8));
				AssertIInvoiceLine_EffectiveDates("11 ", dataACEFTZPrivilegedForeign.invoiceLine, new ZDate(2020, 4, 9), new ZDate(2020, 4, 1), ZDate.Invalid);
			});
		}

		void AssertIInvoiceLine_EffectiveDates(ZString indicator, JobComInvoiceLine invoiceLine, ZDate effectiveDateForDutyRate, ZDate importEffectiveDateForDutyRate, ZDate ftzAdmissionEffectiveDateForDutyRate)
		{
			AssertIInvoiceLine(indicator, invoiceLine, effectiveDateForDutyRate, importEffectiveDateForDutyRate, ftzAdmissionEffectiveDateForDutyRate);
			AssertIInvoiceLine(indicator, Factory.Load<USImportEntryInvoiceLine>(invoiceLine.PK), effectiveDateForDutyRate, importEffectiveDateForDutyRate, ftzAdmissionEffectiveDateForDutyRate);
		}

		void AssertIInvoiceLine(ZString indicator, IInvoiceLine invoiceLine, ZDate effectiveDateForDutyRate, ZDate importEffectiveDateForDutyRate, ZDate ftzAdmissionEffectiveDateForDutyRate)
		{
			indicator += invoiceLine.GetType().Name;
			AssertEquals(indicator + ".EffectiveDateForDutyRate", effectiveDateForDutyRate, invoiceLine.EffectiveDateForDutyRate);
			AssertEquals(indicator + ".ImportEffectiveDateForDutyRate", importEffectiveDateForDutyRate, invoiceLine.ImportEffectiveDateForDutyRate);
			AssertEquals(indicator + ".FTZAdmissionEffectiveDateForDutyRate", ftzAdmissionEffectiveDateForDutyRate, invoiceLine.FTZAdmissionEffectiveDateForDutyRate);
		}

		(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine) SetupInvoiceLinePrivilegedData((JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine) data, ZString zoneStatus, ZDate privilegedStatusDate)
		{
			data.invoiceLine.US_ZoneStatus = zoneStatus;
			data.invoiceLine.US_PrivilegedStatusDate = privilegedStatusDate;
			return data;
		}

		public void TestIInvoiceLineMembers_XAndVAndPartNo_ACS()
		{
			var data = SetupInvoiceLineRelatedData(JobApplicationCodeList.Codes.ACS);
			Factory.Save();
			var importEntryInvoice1Line1 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line1.PK);
			var importEntryInvoice1Line2 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line2.PK);
			var importEntryInvoice1Line3 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line3.PK);
			var importEntryInvoice1Line4 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line4.PK);
			var importEntryInvoice1Line5 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line5.PK);
			var importEntryInvoice1Line6 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line6.PK);
			var importEntryInvoice1Line7 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line7.PK);
			var importEntryInvoice1Line8 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line8.PK);
			var importEntryInvoice1Line9 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line9.PK);
			var importEntryInvoice2Line1 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line1.PK);
			var importEntryInvoice2Line2 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line2.PK);
			var importEntryInvoice2Line3 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line3.PK);
			var importEntryInvoice2Line4 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line4.PK);
			var importEntryInvoice2Line5 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line5.PK);

			CombineAssertions("Normal", () =>
			{
				foreach (var invoiceLine in new IInvoiceLine[] { data.invoice1Line1, importEntryInvoice1Line1 })
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("1 ", invoiceLine, "PART1", "PART1", false, false, false, false, null, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
			});

			CombineAssertions("ParentProduct", () =>
			{
				foreach (var invoiceLine in new IInvoiceLine[] { data.invoice1Line2, importEntryInvoice1Line2 })
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("1 ", invoiceLine, "PART2", "PART2", false, false, false, false, null, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ProductParentTariffLine)[]
						{
							(data.invoice1Line3, data.invoice1Line2),
							(importEntryInvoice1Line3, importEntryInvoice1Line2)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("2 ", invoiceLineData.InvoiceLine, "PART2", "PART2A", false, false, false, false, null, invoiceLineData.ProductParentTariffLine, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ProductParentTariffLine)[]
						{
							(data.invoice1Line4, data.invoice1Line2),
							(importEntryInvoice1Line4, importEntryInvoice1Line2)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("3 ", invoiceLineData.InvoiceLine, "PART2", "PART2B", false, false, false, false, null, invoiceLineData.ProductParentTariffLine, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
			});

			CombineAssertions("ParentTariff", () =>
			{
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine[] ChildLines)[]
						{
							(data.invoice2Line1, new [] { data.invoice2Line2, data.invoice2Line3 }),
							(importEntryInvoice2Line1, new [] { importEntryInvoice2Line2, importEntryInvoice2Line3 })
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("1 ", invoiceLineData.InvoiceLine, "PART4", "PART4", false, false, false, false, null, null, invoiceLineData.ChildLines, Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice2Line2, data.invoice2Line1),
							(importEntryInvoice2Line2, importEntryInvoice2Line1)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("2 ", invoiceLineData.InvoiceLine, "PART4", "PART4A", false, false, false, false, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine, IInvoiceLine[] ChildLines)[]
						{
							(data.invoice2Line3, data.invoice2Line1, new [] { data.invoice2Line4, data.invoice2Line5 }),
							(importEntryInvoice2Line3, importEntryInvoice2Line1, new [] { importEntryInvoice2Line4, importEntryInvoice2Line5 })
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("3 ", invoiceLineData.InvoiceLine, "PART4", "PART4B", false, false, false, false, invoiceLineData.ParentTariffLine, null, invoiceLineData.ChildLines, Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice2Line4, data.invoice2Line3),
							(importEntryInvoice2Line4, importEntryInvoice2Line3)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("4 ", invoiceLineData.InvoiceLine, "PART4", "PART4C", false, false, false, false, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice2Line5, data.invoice2Line3),
							(importEntryInvoice2Line5, importEntryInvoice2Line3)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("5 ", invoiceLineData.InvoiceLine, "PART4", "PART4D", false, false, false, false, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
			});

			CombineAssertions("Set Tariff", () =>
			{
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine[] ChildLines, IInvoiceLine[] ChildVLines)[]
						{
							(data.invoice1Line5, new [] { data.invoice1Line6, data.invoice1Line7 }, new [] { data.invoice1Line6, data.invoice1Line7 }),
							(importEntryInvoice1Line5, new [] { importEntryInvoice1Line6, importEntryInvoice1Line7 }, new [] { importEntryInvoice1Line6, importEntryInvoice1Line7 })
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("1 ", invoiceLineData.InvoiceLine, "PART3", "PART3", false, false, true, false, null, null, invoiceLineData.ChildLines, invoiceLineData.ChildVLines);
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice1Line6, data.invoice1Line5),
							(importEntryInvoice1Line6, importEntryInvoice1Line5)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("2 ", invoiceLineData.InvoiceLine, "PART3", "PART3A", false, false, false, true, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine, IInvoiceLine[] ChildLines, IInvoiceLine[] ChildVLines)[]
						{
							(data.invoice1Line7, data.invoice1Line5, new [] { data.invoice1Line8, data.invoice1Line9 }, new [] { data.invoice1Line8 }),
							(importEntryInvoice1Line7, importEntryInvoice1Line5, new [] { importEntryInvoice1Line8, importEntryInvoice1Line9 }, new [] { importEntryInvoice1Line8 })
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("3 ", invoiceLineData.InvoiceLine, "PART3", "PART3B", true, false, false, true, invoiceLineData.ParentTariffLine, null, invoiceLineData.ChildLines, invoiceLineData.ChildVLines);
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice1Line8, data.invoice1Line7),
							(importEntryInvoice1Line8, importEntryInvoice1Line7)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("4 ", invoiceLineData.InvoiceLine, "PART3", "PART3C", false, true, false, true, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice1Line9, data.invoice1Line7),
							(importEntryInvoice1Line9, importEntryInvoice1Line7)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("5 ", invoiceLineData.InvoiceLine, "PART3", "PART3D", false, false, false, false, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
			});
		}

		public void TestIInvoiceLineMembers_XAndVAndPartNo_ACE()
		{
			var data = SetupInvoiceLineRelatedData(JobApplicationCodeList.Codes.ACE);
			Factory.Save();
			var importEntryInvoice1Line1 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line1.PK);
			var importEntryInvoice1Line2 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line2.PK);
			var importEntryInvoice1Line3 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line3.PK);
			var importEntryInvoice1Line4 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line4.PK);
			var importEntryInvoice1Line5 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line5.PK);
			var importEntryInvoice1Line6 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line6.PK);
			var importEntryInvoice1Line7 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line7.PK);
			var importEntryInvoice1Line8 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line8.PK);
			var importEntryInvoice1Line9 = Factory.Load<USImportEntryInvoiceLine>(data.invoice1Line9.PK);
			var importEntryInvoice2Line1 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line1.PK);
			var importEntryInvoice2Line2 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line2.PK);
			var importEntryInvoice2Line3 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line3.PK);
			var importEntryInvoice2Line4 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line4.PK);
			var importEntryInvoice2Line5 = Factory.Load<USImportEntryInvoiceLine>(data.invoice2Line5.PK);

			CombineAssertions("Normal", () =>
			{
				foreach (var invoiceLine in new IInvoiceLine[] { data.invoice1Line1, importEntryInvoice1Line1 })
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("1 ", invoiceLine, "PART1", "PART1", false, false, false, false, null, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
			});

			CombineAssertions("ParentProduct", () =>
			{
				foreach (var invoiceLine in new IInvoiceLine[] { data.invoice1Line2, importEntryInvoice1Line2 })
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("1 ", invoiceLine, "PART2", "PART2", false, false, false, false, null, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ProductParentTariffLine)[]
						{
							(data.invoice1Line3, data.invoice1Line2),
							(importEntryInvoice1Line3, importEntryInvoice1Line2)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("2 ", invoiceLineData.InvoiceLine, "PART2", "PART2A", false, false, false, false, null, invoiceLineData.ProductParentTariffLine, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ProductParentTariffLine)[]
						{
							(data.invoice1Line4, data.invoice1Line2),
							(importEntryInvoice1Line4, importEntryInvoice1Line2)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("3 ", invoiceLineData.InvoiceLine, "PART2", "PART2B", false, false, false, false, null, invoiceLineData.ProductParentTariffLine, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
			});

			CombineAssertions("ParentTariff", () =>
			{
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine[] ChildLines)[]
						{
							(data.invoice1Line5, new [] { data.invoice1Line6, data.invoice1Line7 }),
							(importEntryInvoice1Line5, new [] { importEntryInvoice1Line6, importEntryInvoice1Line7 })
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("1 ", invoiceLineData.InvoiceLine, "PART3", "PART3", false, false, false, false, null, null, invoiceLineData.ChildLines, Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice1Line6, data.invoice1Line5),
							(importEntryInvoice1Line6, importEntryInvoice1Line5)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("2 ", invoiceLineData.InvoiceLine, "PART3", "PART3A", false, false, false, false, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine, IInvoiceLine[] ChildLines)[]
						{
							(data.invoice1Line7, data.invoice1Line5, new [] { data.invoice1Line8, data.invoice1Line9 }),
							(importEntryInvoice1Line7, importEntryInvoice1Line5, new [] { importEntryInvoice1Line8, importEntryInvoice1Line9 })
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("3 ", invoiceLineData.InvoiceLine, "PART3", "PART3B", false, false, false, false, invoiceLineData.ParentTariffLine, null, invoiceLineData.ChildLines, Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice1Line8, data.invoice1Line7),
							(importEntryInvoice1Line8, importEntryInvoice1Line7)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("4 ", invoiceLineData.InvoiceLine, "PART3", "PART3C", false, false, false, false, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice1Line9, data.invoice1Line7),
							(importEntryInvoice1Line9, importEntryInvoice1Line7)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("5 ", invoiceLineData.InvoiceLine, "PART3", "PART3D", false, false, false, false, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
			});

			CombineAssertions("Set Tariff", () =>
			{
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine[] ChildLines, IInvoiceLine[] ChildVLines)[]
						{
							(data.invoice2Line1, new [] { data.invoice2Line2, data.invoice2Line3 }, new [] { data.invoice2Line2, data.invoice2Line3 }),
							(importEntryInvoice2Line1, new [] { importEntryInvoice2Line2, importEntryInvoice2Line3 }, new [] { importEntryInvoice2Line2, importEntryInvoice2Line3 })
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("1 ", invoiceLineData.InvoiceLine, "PART4", "PART4", false, false, true, false, null, null, invoiceLineData.ChildLines, invoiceLineData.ChildVLines);
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice2Line2, data.invoice2Line1),
							(importEntryInvoice2Line2, importEntryInvoice2Line1)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("2 ", invoiceLineData.InvoiceLine, "PART4", "PART4A", false, false, false, true, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine, IInvoiceLine[] ChildLines, IInvoiceLine[] ChildVLines)[]
						{
							(data.invoice2Line3, data.invoice2Line1, new [] { data.invoice2Line4, data.invoice2Line5 }, new [] { data.invoice2Line4 }),
							(importEntryInvoice2Line3, importEntryInvoice2Line1, new [] { importEntryInvoice2Line4, importEntryInvoice2Line5 }, new [] { importEntryInvoice2Line4 })
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("3 ", invoiceLineData.InvoiceLine, "PART4", "PART4B", true, false, false, true, invoiceLineData.ParentTariffLine, null, invoiceLineData.ChildLines, invoiceLineData.ChildVLines);
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice2Line4, data.invoice2Line3),
							(importEntryInvoice2Line4, importEntryInvoice2Line3)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("4 ", invoiceLineData.InvoiceLine, "PART4", "PART4C", false, true, false, true, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
				foreach (var invoiceLineData in new (IInvoiceLine InvoiceLine, IInvoiceLine ParentTariffLine)[]
						{
							(data.invoice2Line5, data.invoice2Line3),
							(importEntryInvoice2Line5, importEntryInvoice2Line3)
						})
				{
					AssertIInvoiceLineMembers_XAndVAndPartNo("5 ", invoiceLineData.InvoiceLine, "PART4", "PART4D", false, false, false, false, invoiceLineData.ParentTariffLine, null, Array.Empty<IInvoiceLine>(), Array.Empty<IInvoiceLine>());
				}
			});
		}

		void AssertIInvoiceLineMembers_XAndVAndPartNo(ZString indicator, IInvoiceLine invoiceLine, ZString partNo, ZString basePartNo, bool isVParentLine, bool isVChildLine, bool isSetXLine, bool isSetVLine, IInvoiceLine parentTariffLine, IInvoiceLine productParentTariffLine, IInvoiceLine[] childLines, IInvoiceLine[] childVLines)
		{
			indicator += invoiceLine.GetType().Name;
			AssertEquals(indicator + ".JI_PartNo", partNo, invoiceLine.JI_PartNo);
			AssertEquals(indicator + ".BaseJI_PartNo", basePartNo, invoiceLine.BaseJI_PartNo);
			AssertEquals(indicator + ".IsVParentLine", isVParentLine, invoiceLine.IsVParentLine);
			AssertEquals(indicator + ".IsVChildLine", isVChildLine, invoiceLine.IsVChildLine);
			AssertEquals(indicator + ".IsSetXLine", isSetXLine, invoiceLine.IsSetXLine);
			AssertEquals(indicator + ".IsSetVLine", isSetVLine, invoiceLine.IsSetVLine);
			AssertSame(indicator + ".ParentTariffLine", parentTariffLine, invoiceLine.ParentTariffLine);
			AssertSame(indicator + ".ProductParentTariffLine", productParentTariffLine, invoiceLine.ProductParentTariffLine);
			AssertArrayEqualsByElements(indicator + ".invoiceLine.ChildLines", childLines, invoiceLine.ChildLines.ToArray());
			AssertArrayEqualsByElements(indicator + ".invoiceLine.ChildVLines", childVLines, invoiceLine.ChildVLines.ToArray());
			if (invoiceLine is USImportEntryInvoiceLine)
			{
				AssertSame(indicator + ".ChildLines is cached", invoiceLine.ChildLines, invoiceLine.ChildLines);
				AssertSame(indicator + ".ChildVLines is cached", invoiceLine.ChildVLines, invoiceLine.ChildVLines);
			}
		}

		(JobComInvoiceLine invoice1Line1, JobComInvoiceLine invoice1Line2, JobComInvoiceLine invoice1Line3, JobComInvoiceLine invoice1Line4, JobComInvoiceLine invoice1Line5, JobComInvoiceLine invoice1Line6, JobComInvoiceLine invoice1Line7, JobComInvoiceLine invoice1Line8, JobComInvoiceLine invoice1Line9,
			JobComInvoiceLine invoice2Line1, JobComInvoiceLine invoice2Line2, JobComInvoiceLine invoice2Line3, JobComInvoiceLine invoice2Line4, JobComInvoiceLine invoice2Line5) SetupInvoiceLineRelatedData(ZString applicationCode)
		{
			var data = SetupInvoiceLineData(applicationCode);
			var invoice1 = data.invoice;
			var invoice1Line1 = data.invoiceLine;
			invoice1Line1.JI_PartNo = "PART1";
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_PartNo = "PART2";
			var invoice1Line3 = invoice1Line2.AddProductRelatedInvoiceLine();
			((IBusinessObjectInternals)invoice1Line3).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART2A";
			var invoice1Line4 = invoice1Line2.AddProductRelatedInvoiceLine();
			((IBusinessObjectInternals)invoice1Line4).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART2B";
			var invoice1Line5 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line5.JI_PartNo = "PART3";
			invoice1Line5.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoice1Line6 = invoice1Line5.AddSecondaryInvoiceLine();
			invoice1Line6.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			((IBusinessObjectInternals)invoice1Line6).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART3A";
			var invoice1Line7 = invoice1Line5.AddSecondaryInvoiceLine();
			invoice1Line7.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			((IBusinessObjectInternals)invoice1Line7).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART3B";
			var invoice1Line8 = invoice1Line7.AddSecondaryInvoiceLine();
			invoice1Line8.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			((IBusinessObjectInternals)invoice1Line8).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART3C";
			var invoice1Line9 = invoice1Line7.AddSecondaryInvoiceLine();
			invoice1Line9.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			((IBusinessObjectInternals)invoice1Line9).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART3D";
			var invoice2 = data.declaration.Invoices.AddNew();
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_PartNo = "PART4";
			invoice2Line1.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			var invoice2Line2 = invoice2Line1.AddSecondaryInvoiceLine();
			invoice2Line2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			((IBusinessObjectInternals)invoice2Line2).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART4A";
			var invoice2Line3 = invoice2Line1.AddSecondaryInvoiceLine();
			invoice2Line3.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			((IBusinessObjectInternals)invoice2Line3).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART4B";
			var invoice2Line4 = invoice2Line3.AddSecondaryInvoiceLine();
			invoice2Line4.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			((IBusinessObjectInternals)invoice2Line4).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART4C";
			var invoice2Line5 = invoice2Line3.AddSecondaryInvoiceLine();
			invoice2Line5.US_SetInd = SecondarySpecProgIndicatorList.Codes.F;
			((IBusinessObjectInternals)invoice2Line5).Row[JobComInvoiceLine.Schema.JI_PartNo] = "PART4D";
			return (invoice1Line1, invoice1Line2, invoice1Line3, invoice1Line4, invoice1Line5, invoice1Line6, invoice1Line7, invoice1Line8, invoice1Line9, invoice2Line1, invoice2Line2, invoice2Line3, invoice2Line4, invoice2Line5);
		}

		public void TestIInvoiceLineMembers_CurrencyConverter()
		{
			var data = SetupInvoiceLineData(JobApplicationCodeList.Codes.ACE);
			var invoiceLine1 = data.invoiceLine;
			var invoice1 = data.invoice;
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = data.declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();
			Factory.Save();
			var importEntryInvoiceLine1 = Factory.Load<USImportEntryInvoiceLine>(invoiceLine1.PK);
			var importEntryInvoiceLine2 = Factory.Load<USImportEntryInvoiceLine>(invoiceLine2.PK);
			var importEntryInvoiceLine3 = Factory.Load<USImportEntryInvoiceLine>(invoiceLine3.PK);
			var importEntryInvoiceLine4 = Factory.Load<USImportEntryInvoiceLine>(invoiceLine4.PK);
			AssertSame("CurrencyConverter should be cached for same invoice", importEntryInvoiceLine1.CurrencyConverter, importEntryInvoiceLine2.CurrencyConverter);
			AssertEquals("CurrencyConverter should not be cached for different invoice", false, object.ReferenceEquals(importEntryInvoiceLine1.CurrencyConverter, importEntryInvoiceLine3.CurrencyConverter));
			AssertSame("CurrencyConverter should be cached for same invoice", importEntryInvoiceLine3.CurrencyConverter, importEntryInvoiceLine4.CurrencyConverter);
		}

		[TestDate(2020, 4, 2)]
		public void TestIInvoiceLineMembers()
		{
			var data1 = SetupInvoiceLineData(JobApplicationCodeList.Codes.ACE);
			var data2 = SetupInvoiceLineData(JobApplicationCodeList.Codes.ACS);
			Factory.Save();
			CombineAssertions("ACE", () =>
			{
				var data = data1;
				var importEntryInvoiceLine = Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK);
				var invoiceLineDatas = new (IInvoiceLine InvoiceLine, IInvoiceHeader InvoiceHeader)[] { (data.invoiceLine, data.invoice), (importEntryInvoiceLine, importEntryInvoiceLine) };
				foreach (var invoiceLineData in invoiceLineDatas)
				{
					AssertIInvoiceLine(invoiceLineData.InvoiceLine, data.invoiceLine.PK, invoiceLineData.InvoiceHeader, true);
				}
			});
			CombineAssertions("ACS", () =>
			{
				var data = data2;
				var importEntryInvoiceLine = Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK);
				var invoiceLineDatas = new (IInvoiceLine InvoiceLine, IInvoiceHeader InvoiceHeader)[] { (data.invoiceLine, data.invoice), (importEntryInvoiceLine, importEntryInvoiceLine) };
				foreach (var invoiceLineData in invoiceLineDatas)
				{
					AssertIInvoiceLine(invoiceLineData.InvoiceLine, data.invoiceLine.PK, invoiceLineData.InvoiceHeader, false);
				}
			});
		}

		public void TestIInvoiceHeader()
		{
			var data1 = SetupInvoiceData(1, "INV1", "FOB");
			var data2 = SetupInvoiceData(2, "INV2", "CIF");
			Factory.Save();
			CombineAssertions("1", () =>
			{
				var data = data1;
				var importEntryInvoiceLine = Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK);
				var invoiceHeaderDatas = new (IInvoiceHeader InvoiceHeader, IDeclaration Declaration)[] { (data.invoice, data.declaration), (importEntryInvoiceLine, importEntryInvoiceLine) };
				foreach (var invoiceHeaderData in invoiceHeaderDatas)
				{
					AssertIInvoiceHeader(invoiceHeaderData.InvoiceHeader, data.invoice.PK, 1, "INV1", "FOB", invoiceHeaderData.Declaration);
				}
			});
			CombineAssertions("2", () =>
			{
				var data = data2;
				var importEntryInvoiceLine = Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK);
				var invoiceHeaderDatas = new (IInvoiceHeader InvoiceHeader, IDeclaration Declaration)[] { (data.invoice, data.declaration), (importEntryInvoiceLine, importEntryInvoiceLine) };
				foreach (var invoiceHeaderData in invoiceHeaderDatas)
				{
					AssertIInvoiceHeader(invoiceHeaderData.InvoiceHeader, data.invoice.PK, 2, "INV2", "CIF", invoiceHeaderData.Declaration);
				}
			});
		}

		public void TestICurrencyConverterDataProviderWithFixedExRates()
		{
			var data1 = SetupCurrencyConverterDataProviderWithFixedExRatesData(true, Core.Constants.CurrencyCodes.NewZealand, 0.75m, new ZDateTime(2020, 4, 1));
			var data2 = SetupCurrencyConverterDataProviderWithFixedExRatesData(false, Core.Constants.CurrencyCodes.Singapore, 1.02m, new ZDateTime(2020, 4, 2));
			Factory.Save();
			CombineAssertions("FixedExchangeRate", () =>
			{
				var data = data1;
				var providers = new ICurrencyConverterDataProviderWithFixedExRates[] { data.invoice, Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK) };
				foreach (var provider in providers)
				{
					AssertICurrencyConverterDataProviderWithFixedExRates(provider, Core.Constants.CurrencyCodes.NewZealand, 0.75m, new ZDateTime(2020, 4, 1));
				}
			});
			CombineAssertions("Non-FixedExchangeRate", () =>
			{
				var data = data2;
				var providers = new ICurrencyConverterDataProviderWithFixedExRates[] { data.invoice, Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK) };
				foreach (var provider in providers)
				{
					AssertICurrencyConverterDataProviderWithFixedExRates(provider, ZString.Empty, 0.56m, new ZDateTime(2020, 4, 2));
				}
			});
		}

		public void TestIDeclaration()
		{
			var data1 = SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 4, 1));
			var data2 = SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.WarehouseWithdrawalADDCVD, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 5, 1));
			var data3 = SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.BCR, new ZDate(2020, 6, 1));
			var data4 = SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionQuotaVisa, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 7, 1));
			var data5 = SetupDeclarationData(JobApplicationCodeList.Codes.ACS, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACS, new ZDate(2020, 8, 1));
			Factory.Save();
			CombineAssertions("Normal", () =>
			{
				var data = data1;
				var declarations = new IDeclaration[] { data.declaration, Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK) };
				foreach (var iDeclaration in declarations)
				{
					AssertIDeclaration(iDeclaration, data.declaration.PK, new ZDate(2020, 4, 1), false, false, true, false, new ZDateTime(2020, 4, 2), new ZDateTime(2020, 4, 3), new ZDateTime(2020, 4, 4), new ZDateTime(2020, 4, 5), new ZDateTime(2020, 4, 6), new ZDateTime(2020, 4, 7));
				}
			});
			CombineAssertions("IsExWarehouse", () =>
			{
				var data = data2;
				var declarations = new IDeclaration[] { data.declaration, Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK) };
				foreach (var iDeclaration in declarations)
				{
					AssertIDeclaration(iDeclaration, data.declaration.PK, new ZDate(2020, 5, 1), true, false, true, false, new ZDateTime(2020, 5, 2), new ZDateTime(2020, 5, 3), new ZDateTime(2020, 5, 4), new ZDateTime(2020, 5, 5), new ZDateTime(2020, 5, 6), new ZDateTime(2020, 5, 7));
				}
			});
			CombineAssertions("IsBorderMovement", () =>
			{
				var data = data3;
				var declarations = new IDeclaration[] { data.declaration, Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK) };
				foreach (var iDeclaration in declarations)
				{
					AssertIDeclaration(iDeclaration, data.declaration.PK, new ZDate(2020, 6, 1), false, false, true, true, new ZDateTime(2020, 6, 2), new ZDateTime(2020, 6, 3), new ZDateTime(2020, 6, 4), new ZDateTime(2020, 6, 5), new ZDateTime(2020, 6, 6), new ZDateTime(2020, 6, 7));
				}
			});
			CombineAssertions("IsQuota", () =>
			{
				var data = data4;
				var declarations = new IDeclaration[] { data.declaration, Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK) };
				foreach (var iDeclaration in declarations)
				{
					AssertIDeclaration(iDeclaration, data.declaration.PK, new ZDate(2020, 7, 1), false, true, true, false, new ZDateTime(2020, 7, 2), new ZDateTime(2020, 7, 3), new ZDateTime(2020, 7, 4), new ZDateTime(2020, 7, 5), new ZDateTime(2020, 7, 6), new ZDateTime(2020, 7, 7));
				}
			});
			CombineAssertions("IsACECargoCertificationMode", () =>
			{
				var data = data5;
				var declarations = new IDeclaration[] { data.declaration, Factory.Load<USImportEntryInvoiceLine>(data.invoiceLine.PK) };
				foreach (var iDeclaration in declarations)
				{
					AssertIDeclaration(iDeclaration, data.declaration.PK, new ZDate(2020, 8, 1), false, false, false, false, new ZDateTime(2020, 8, 2), new ZDateTime(2020, 8, 3), new ZDateTime(2020, 8, 4), new ZDateTime(2020, 8, 5), new ZDateTime(2020, 8, 6), new ZDateTime(2020, 8, 7));
				}
			});
		}

		(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine) SetupInvoiceLineData(ZString applicationCode)
		{
			var data = SetupDeclarationData(applicationCode, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 4, 1));
			data.declaration.JE_DateOfArrival = new ZDateTime(2020, 5, 2);
			var invoice = data.invoice;
			invoice.JZ_InvoiceNumber = "INV4";
			invoice.JZ_RX_NKInvoice_Currency = Invoice_Currency.RX_Code;
			invoice.US_DateOfExport = new ZDateTime(2020, 4, 3);
			var invoiceLine = data.invoiceLine;
			using (invoice.InvoiceLineLineNumberGenerator.GetLineNumberSuspender())
			{
				invoiceLine.JI_LineNo = 2;
			}
			invoiceLine.JI_PartNo = "PART3";
			invoiceLine.JI_Tariff = "10203040";
			invoiceLine.JI_CustomsUnitQty = "C1";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsSecondUnitQty = "C2";
			invoiceLine.JI_CustomsSecondQuantity = 12m;
			invoiceLine.JI_CustomsThirdUnitQty = "C3";
			invoiceLine.JI_CustomsThirdQuantity = 13m;
			invoiceLine.US_SupTariff = ImportSupTariff.UE_Tariff;
			invoiceLine.US_SupUQ1 = "S1";
			invoiceLine.US_SupQty1 = 21m;
			invoiceLine.US_SupUQ2 = "S2";
			invoiceLine.US_SupQty2 = 22m;
			invoiceLine.US_SupUQ3 = "S3";
			invoiceLine.US_SupQty3 = 23m;
			invoiceLine.US_CustomsValue = 1500m;
			invoiceLine.US_98GoodsValue = 2000m;
			invoiceLine.US_98ValueInvCurr = 600m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoiceLine.US_PrivilegedStatusDate = new ZDateTime(2020, 5, 1);
			invoiceLine.US_SetInd = PrimarySpecProgramIndicatorList.Codes.D;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			return data;
		}

		void AssertIInvoiceLine(IInvoiceLine invoiceLine, ZGuid invoiceLinePK, IInvoiceHeader invoiceHeader, bool isACE)
		{
			var type = invoiceLine.GetType().Name;
			AssertEquals(type + ".PK", invoiceLinePK, invoiceLine.PK);
			AssertEquals(type + ".JI_LineNo", (short)2, invoiceLine.JI_LineNo);
			AssertEquals(type + ".JI_PartNo", "PART3", invoiceLine.JI_PartNo);
			AssertEquals(type + ".JI_Tariff", "10203040", invoiceLine.JI_Tariff);
			AssertEquals(type + ".JI_CustomsQuantity", 10m, invoiceLine.JI_CustomsQuantity);
			AssertEquals(type + ".JI_CustomsUnitQty", "C1", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(type + ".InvoiceNumber", "INV4", invoiceLine.InvoiceNumber);
			AssertEquals(type + ".JI_CustomsSecondQuantity", 12m, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals(type + ".JI_CustomsSecondUnitQty", "C2", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(type + ".JI_CustomsThirdQuantity", 13m, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals(type + ".JI_CustomsThirdUnitQty", "C3", invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(type + ".BaseJI_PartNo", "PART3", invoiceLine.BaseJI_PartNo);
			AssertEquals(type + ".US_SupTariff", ImportSupTariff.UE_Tariff, invoiceLine.US_SupTariff);
			AssertEquals(type + ".US_SupQty1", 21m, invoiceLine.US_SupQty1);
			AssertEquals(type + ".US_SupUQ1", "S1", invoiceLine.US_SupUQ1);
			AssertEquals(type + ".US_SupQty2", 22m, invoiceLine.US_SupQty2);
			AssertEquals(type + ".US_SupUQ2", "S2", invoiceLine.US_SupUQ2);
			AssertEquals(type + ".US_SupQty3", 23m, invoiceLine.US_SupQty3);
			AssertEquals(type + ".US_SupUQ3", "S3", invoiceLine.US_SupUQ3);
			AssertEquals(type + ".US_CustomsValue", 1500m, invoiceLine.US_CustomsValue);
			if (invoiceLine is USImportEntryInvoiceLine)
			{
				AssertExceptionThrown<NotSupportedException>(() => _ = invoiceLine.JI_CustomsValue);
			}
			AssertEquals(type + ".TotalOriginalGoodsValueInUSD", 2300m, invoiceLine.TotalOriginalGoodsValueInUSD);
			AssertEquals(type + ".US_98GoodsValue", 2000m, invoiceLine.US_98GoodsValue);
			AssertEquals(type + ".US_98ValueInvCurr", 600m, invoiceLine.US_98ValueInvCurr);
			AssertEquals(type + ".US_ZoneStatus", ZoneStatusList.Codes.Domestic, invoiceLine.US_ZoneStatus);
			AssertEquals(type + ".US_PrivilegedStatusDate", new ZDateTime(2020, 5, 1), invoiceLine.US_PrivilegedStatusDate);
			AssertEquals(type + ".EffectiveDateForDutyRate", new ZDateTime(2020, 4, 1), invoiceLine.EffectiveDateForDutyRate);
			AssertEquals(type + ".US_SetInd", PrimarySpecProgramIndicatorList.Codes.D, invoiceLine.US_SetInd);
			AssertEquals(type + ".US_SecondarySPI", SecondarySpecProgIndicatorList.Codes.H, invoiceLine.US_SecondarySPI);
			AssertEquals(type + ".ImportSupTariff", ImportSupTariff, invoiceLine.ImportSupTariff);
			AssertEquals(type + ".Invoice_Currency", Invoice_Currency, invoiceLine.Invoice_Currency);
			AssertEquals(type + ".FTZAdmissionEffectiveDateForDutyRate", ZDateTime.Invalid, invoiceLine.FTZAdmissionEffectiveDateForDutyRate);
			AssertEquals(type + ".ImportEffectiveDateForDutyRate", new ZDateTime(2020, 4, 1), invoiceLine.ImportEffectiveDateForDutyRate);
			AssertEquals(type + ".IsACE", isACE, invoiceLine.IsACE);
			AssertEquals(type + ".IsRecon", false, invoiceLine.IsRecon);
			AssertEquals(type + ".HasDeclaration", true, invoiceLine.HasDeclaration);
			AssertEquals(type + ".InvoiceHeader", invoiceHeader, invoiceLine.InvoiceHeader);
			AssertEquals(type + ".IBaseInvoiceLine.InvoiceHeader", invoiceHeader, ((Customs.Business.IBaseInvoiceLine)invoiceLine).InvoiceHeader);
		}

		RefCurrency Invoice_Currency
		{
			get
			{
				if (invoice_Currency == null)
				{
					invoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.CapeVerde);
					var rate = invoice_Currency.ExchangeRates.AddNew();
					rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
					rate.RE_StartDate = ZDateTime.Today.AddDays(-1);
					rate.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
					rate.RE_SellRate = 0.5m;
				}
				return invoice_Currency;
			}
		}
		RefCurrency invoice_Currency;

		USCTariff ImportSupTariff
		{
			get
			{
				if (importSupTariff == null)
				{
					importSupTariff = Factory.New<USCTariff>();
					importSupTariff.UE_Tariff = "20304050";
					importSupTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
					importSupTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
				}
				return importSupTariff;
			}
		}
		USCTariff importSupTariff;

		void AssertIInvoiceHeader(IInvoiceHeader invoice, ZGuid invoicePK, ZShort invoiceDisplaySequence, ZString invoiceNumber, ZString incoTerm, IDeclaration declaration)
		{
			var type = invoice.GetType().Name;
			AssertEquals(type + ".PK", invoicePK, invoice.PK);
			AssertEquals(type + ".JZ_InvoiceDisplaySequence", invoiceDisplaySequence, invoice.JZ_InvoiceDisplaySequence);
			AssertEquals(type + ".JZ_InvoiceNumber", invoiceNumber, invoice.JZ_InvoiceNumber);
			AssertEquals(type + ".JZ_IncoTerm", incoTerm, invoice.JZ_IncoTerm);
			AssertEquals(type + ".Declaration", declaration, invoice.Declaration);
		}

		(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine) SetupInvoiceData(ZShort invoiceDisplaySequence, ZString invoiceNumber, ZString incoTerm)
		{
			var data = SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 4, 1));
			var invoice = data.invoice;
			invoice.JZ_InvoiceNumber = invoiceNumber;
			invoice.JZ_IncoTerm = incoTerm;
			using (data.declaration.GetInvoiceNumberRenumberingSuspender())
			{
				invoice.JZ_InvoiceDisplaySequence = invoiceDisplaySequence;
			}
			return data;
		}

		void AssertICurrencyConverterDataProviderWithFixedExRates(ICurrencyConverterDataProviderWithFixedExRates provider, ZString fixedExchangeRateCurrencyCode, ZDecimal fixedExchangeRate, ZDateTime dateOfValuation)
		{
			var type = provider.GetType().Name;
			AssertEquals(type + ".FixedExchangeRateCurrencyCode", fixedExchangeRateCurrencyCode, provider.FixedExchangeRateCurrencyCode);
			AssertEquals(type + ".FixedExchangeRate", fixedExchangeRate, provider.FixedExchangeRate);
			AssertEquals(type + ".DateOfValuation", dateOfValuation, provider.DateOfValuation);
			AssertEquals(type + ".RateType", ExchangeRateType.Customs, provider.RateType);
			AssertEquals(type + ".MaximumDaysToFallback", 0, provider.MaximumDaysToFallback);
			AssertEquals(type + ".Company", Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK), provider.Company);
			AssertEquals(type + ".LocalCurrencyCodeOverride", Core.Constants.CurrencyCodes.UnitedStates, provider.LocalCurrencyCodeOverride);
			AssertEquals(type + ".IsReciprocalOverride", true, provider.IsReciprocalOverride);
		}

		(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine) SetupCurrencyConverterDataProviderWithFixedExRatesData(ZBool isInvoiceCurrExRateUserEnterable, ZString invoiceCurrency, ZDecimal invoiceCurrExRate, ZDateTime dateOfExport)
		{
			var data = SetupDeclarationData(JobApplicationCodeList.Codes.ACE, JobMessageTypeList.Codes.Import, EntryTypeList.Codes.ConsumptionFreeDutiable, CargoReleaseTypeList.Codes.ACE, new ZDate(2020, 4, 1));
			var invoice = data.invoice;
			invoice.JZ_RX_NKInvoice_Currency = invoiceCurrency;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = isInvoiceCurrExRateUserEnterable;
			invoice.JZ_InvoiceCurrExRate = invoiceCurrExRate;
			invoice.US_DateOfExport = dateOfExport;
			return data;
		}

		(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine) SetupDeclarationData(ZString applicationCode, ZString messageType, ZString entryType, ZString cargoReleaseType, ZDate dutyCalcDate)
		{
			var itDate = dutyCalcDate.AddDays(1);
			var estimatedEntryDate = dutyCalcDate.AddDays(2);
			var presentationDate = dutyCalcDate.AddDays(3);
			var entryAuthorisationDate = dutyCalcDate.AddDays(4);
			var preliminaryStatementPrintDate = dutyCalcDate.AddDays(5);
			var entryDate = dutyCalcDate.AddDays(6);
			var dateOfArrival = dutyCalcDate.AddDays(-1);
			return SetupDeclarationData(applicationCode, messageType, entryType, cargoReleaseType, dutyCalcDate, itDate, estimatedEntryDate, presentationDate, entryAuthorisationDate, preliminaryStatementPrintDate, entryDate, dateOfArrival);
		}

		(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine) SetupDeclarationData(ZString applicationCode, ZString messageType, ZString entryType, ZString cargoReleaseType, ZDate dutyCalcDate, ZDateTime itDate, ZDateTime estimatedEntryDate, ZDateTime presentationDate, ZDateTime entryAuthorisationDate, ZDateTime preliminaryStatementPrintDate, ZDateTime entryDate, ZDateTime dateOfArrival)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = applicationCode;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = entryType;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = cargoReleaseType;
			declaration.US_ITDate = itDate;
			declaration.US_EstimatedEntryDate = estimatedEntryDate;
			declaration.US_EntryDate = entryDate;
			declaration.US_PresentationDate = presentationDate;
			declaration.JE_EntryAuthorisationDate = entryAuthorisationDate;
			declaration.US_PreliminaryStatementPrintDate = preliminaryStatementPrintDate;
			declaration.JE_DateOfArrival = dateOfArrival;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.US_DutyCalcDate = dutyCalcDate;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return (declaration, invoice, invoiceLine);
		}

		void AssertIDeclaration(IDeclaration iDeclaration, ZGuid declarationPK, ZDate dutyCalcDate, bool isExWarehouse, bool isQuota, bool isACECargoCertificationMode, bool isBorderMovement, ZDateTime itDate, ZDateTime estimatedEntryDate, ZDateTime presentationDate, ZDateTime entryAuthorisationDate, ZDateTime preliminaryStatementPrintDate, ZDateTime entryDate)
		{
			var type = iDeclaration.GetType().Name;
			AssertEquals(type + ".PK", declarationPK, iDeclaration.PK);
			AssertEquals(type + ".US_DutyCalcDate", dutyCalcDate, iDeclaration.US_DutyCalcDate);
			AssertEquals(type + ".ShouldCalculateMPFAndDutyDate", false, iDeclaration.ShouldCalculateMPFAndDutyDate);
			AssertEquals(type + ".IsExWarehouse", isExWarehouse, iDeclaration.IsExWarehouse);
			AssertEquals(type + ".IsQuota", isQuota, iDeclaration.IsQuota);
			AssertEquals(type + ".IsACECargoCertificationMode", isACECargoCertificationMode, iDeclaration.IsACECargoCertificationMode);
			AssertEquals(type + ".IsBorderMovement", isBorderMovement, iDeclaration.IsBorderMovement);
			AssertEquals(type + ".US_ITDate", itDate, iDeclaration.US_ITDate);
			AssertEquals(type + ".US_EstimatedEntryDate", estimatedEntryDate, iDeclaration.US_EstimatedEntryDate);
			AssertEquals(type + ".US_PreliminaryStatementPrintDate", preliminaryStatementPrintDate, iDeclaration.US_PreliminaryStatementPrintDate);
			AssertEquals(type + ".US_EntryDate", entryDate, iDeclaration.US_EntryDate);
			AssertEquals(type + ".US_PresentationDate", presentationDate, iDeclaration.US_PresentationDate);
			AssertEquals(type + ".JE_EntryAuthorisationDate", entryAuthorisationDate, iDeclaration.JE_EntryAuthorisationDate);
		}

		protected override bool IsDeleteSupported() => false;
	}
}
