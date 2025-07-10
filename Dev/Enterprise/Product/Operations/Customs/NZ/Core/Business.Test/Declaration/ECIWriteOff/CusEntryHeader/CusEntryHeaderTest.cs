using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.NZ;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Testing
{
	using System.Linq;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.ZArchitecture.Business;
	using NUnit.Framework;

	[TestedType(typeof(CusEntryHeader))]
	class CusEntryHeaderBaseOnlyTest : CusEntryHeaderTest<CusEntryHeader>
	{
	}

	public abstract class CusEntryHeaderTest<T> : Declaration.Testing.CusEntryHeaderTest<T>
		where T : CusEntryHeader
	{
		#region TestEntryStatusListGetsRightType
		public void TestGetNewLookups()
		{
			AssertType<CusEntryHeaderLookups>(EntryHeader.Lookups);
		}
		#endregion

		#region TestLastCustomsStatusIsImpediment
		public virtual void TestLastCustomsStatusIsImpediment()
		{
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
			AssertEquals("EntryHeader.LastCustomsStatusIsImpediment", true, EntryHeader.LastCustomsStatusIsImpediment);
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			AssertEquals("EntryHeader.LastCustomsStatusIsImpediment", false, EntryHeader.LastCustomsStatusIsImpediment);
		}
		#endregion

		#region TestTotalAmountPayable
		public void TestTotalAmountPayable()
		{
			EntryHeader.CH_TotalPaid = 123.54m;
			AssertEquals("EntryHeader.TotalAmountPayable", 123.54m, EntryHeader.TotalAmountPayable);
		}
		#endregion

		#region TestPackages
		public virtual void TestPackages()
		{
			Declaration.JE_TotalNoOfPacks = 22;
			AssertEquals("EntryHeader.Packages", 22, EntryHeader.PackagesCount);
		}
		#endregion

		#region TestSetDefaultValues
		public virtual void TestSetDefaultValues()
		{
			AssertEquals("EntryHeader.CH_MessageType", CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOff, EntryHeader.CH_MessageType);
			AssertEquals("EntryHeader.IsManifestEntry", false, EntryHeader.IsManifestEntry);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, EntryHeader.CH_EntryStatus);
		}
		#endregion

		#region TestSettingDeclarationNumberSetsRightType
		public void TestSettingEntryNumberSetsRightType()
		{
			EntryHeader.EntryNumber = "NZ111";
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, EntryHeader.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypeList.Codes.ECIWriteOff);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			CusEntryNumber[] entryNums = (CusEntryNumber[])Factory.Load(typeof(CusEntryNumber), filter);
			AssertEquals("One entry number", 1, entryNums.Length);
			AssertEquals("Entry number", "NZ111", entryNums[0].CE_EntryNum);
		}
		#endregion

		public void TestCustomsClearedEventLoggedImportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not have been created yet", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.ExportCustomsCleared.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.NCC;
			Declaration.SaveHandlingSaveExceptions();
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Only 1 SCM event should be created", 1, logEntries.Length);
			AssertEquals("Log Entry event has been created for this cleared Import write-off entry", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
			var logEventTime = logEntries[0].SL_EventTime;

			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Only 1 CLR event should be created", 1, logEntries.Length);
			AssertEquals("CLR Log Entry event should have been created - CLR will be created when SCM is created", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
			AssertEquals("SCM & CLR Log events should have been created at the same time", logEventTime, logEntries[0].SL_EventTime);
		}

		public void TestCustomsClearedEventLoggedExportECI()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "B00001985";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			Declaration.SaveHandlingSaveExceptions();
			var logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not have been created yet", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.ExportCustomsCleared.Code));

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			Declaration.AgencyMessageBeingProcessed = ResponsibleGovernmentAgencyList.Codes.NZCS;
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CLR;
			Declaration.SaveHandlingSaveExceptions();

			logEntries = Declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			AssertEquals("Only 1 CLR event should be created", 1, logEntries.Length);
			AssertEquals("ECC (Export Customs Cleared) Log Entry event should have been created - Clearance event will be created when SCM is created", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.ExportCustomsCleared.Code));
		}

		public void TestSetDeclarationStatusesForManifestJob()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_DeclarationReference = "M00001985-1";
			Declaration.JE_MasterBill = "08600394881";
			Declaration.JE_HouseBill = "HB1";
			Declaration.JE_EntryStatus = "WOF";
			Declaration.JE_TSWCombinedStatus = "HLD";
			Declaration.JE_ManifestBioStatus = "HLD";
			Declaration.JE_ManifestNZCSStatus = "WOF";
			EntryHeader.SetDeclarationStatusesWhenSetToCurrent(Declaration);
			AssertEquals("Entry Status should reflect the heirachy of the combined status of both Bio & NZCS", LowValueConsignmentStatusList.Codes.ConsignmentHeld, Declaration.JE_EntryStatus);
		}

		#region Implementation

		#region Declaration
		protected new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}
		#endregion

		#region EntryHeader
		protected new T EntryHeader
		{
			get { return (T)base.EntryHeader; }
		}
		#endregion

		class ECIJobDeclaration : JobDeclaration
		{
			public ECIJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
			public override Type TypeOfEntryHeaderRequiredForCurrentDeclarationSettings
			{
				get { return typeof(T); }
			}
		}

		#region GetNewJobDeclaration
		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<ECIJobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			return declaration;
		}
		#endregion
		#endregion
	}
}
