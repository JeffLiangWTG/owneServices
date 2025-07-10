using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSupportingInfo))]
	sealed class CusSupportingInfoBaseOnlyTest : CusSupportingInfoTest<CusSupportingInfo>
	{
		public void TestSynchroniserReadOnlyMembers()
		{
			AssertEquals(0, Factory.New<CusSupportingInfo>().SynchroniserReadOnlyMembers.Count);
		}

		public void TestGetShouldPropertiesBeReadOnly() => CombineAssertions(() =>
		{
			var info = Factory.New<CusSupportingInfo>();
			info.SynchroniserReadOnlyMembers.Add(nameof(CusSupportingInfo.CSI_Code));
			AssertEquals("In SynchroniserReadOnlyMembers", true, info.CSI_CodeInfo.ReadOnly);
			info.SynchroniserReadOnlyMembers.Remove(nameof(CusSupportingInfo.CSI_Code));
			AssertEquals("Not in SynchroniserReadOnlyMembers", false, info.CSI_CodeInfo.ReadOnly);
		});

		public void TestChangedParent()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			CombineAssertions(() =>
			{
				var parent1 = Factory.New<CusEntryHeader>();
				supportingInfo.Parent = parent1;
				AssertSame("set parent", parent1, supportingInfo.Parent);

				var parent2 = Factory.New<CusEntryHeader>();
				supportingInfo.Parent = parent2;
				AssertSame("set parent with different PK", parent2, supportingInfo.Parent);

				var parent3 = Factory.NewWithPrimaryKey<CusEntryLine>(parent2.PK.ToGuid());
				supportingInfo.Parent = parent3;
				AssertSame("set parent with different TableName", parent3, supportingInfo.Parent);

				var parent4 = Factory.GetNull<CusEntryHeader>();
				supportingInfo.Parent = Factory.GetNull<CusEntryHeader>();
				AssertNullOrEmpty("No developer error should be reported", ErrorReporter.LastMessageReported);
				AssertSame("set parent to a null BO", parent4, supportingInfo.Parent);

				supportingInfo.Parent = null;
				AssertContains("Parent of CusSupportingInfo was set to null.", ErrorReporter.LastMessageReported);
				ErrorReporter.Instance.Clear();
			});
		}

		public void TestSaveWithDataModel()
		{
			CombineAssertions(() =>
			{
				var supportingInfo = Factory.New<CusSupportingInfo>();
				var jobDeclaration = Factory.New<BaseJobDeclaration>();
				supportingInfo.Parent = jobDeclaration;
				supportingInfo.OnSaving();
				AssertEquals("jobDeclaration header", "ER", supportingInfo.CSI_DataModel);

				var jobComInvoiceHeader = Factory.New<BaseJobDeclaration>().Invoices.AddNew();
				supportingInfo = Factory.New<CusSupportingInfo>();
				supportingInfo.Parent = jobComInvoiceHeader;
				supportingInfo.OnSaving();
				AssertEquals("jobComInvoiceHeader header", "ER", supportingInfo.CSI_DataModel);

				var jobComInvoiceLine = Factory.New<BaseJobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
				supportingInfo = Factory.New<CusSupportingInfo>();
				supportingInfo.Parent = jobComInvoiceLine;
				supportingInfo.OnSaving();
				AssertEquals("jobComInvoiceLine header", "ER", supportingInfo.CSI_DataModel);

				var cusEntryInstruction = Factory.New<CusEntryInstruction>();
				cusEntryInstruction.CEI_JE = jobDeclaration.PK;
				supportingInfo = Factory.New<CusSupportingInfo>();
				supportingInfo.Parent = cusEntryInstruction;
				supportingInfo.OnSaving();
				AssertEquals("cusEntryInstruction header", "ER", supportingInfo.CSI_DataModel);

				var entryHeader = Factory.New<BaseJobDeclaration>().CustomsEntryHeaders.AddNew();
				supportingInfo = Factory.New<CusSupportingInfo>();
				supportingInfo.Parent = entryHeader;
				supportingInfo.OnSaving();
				AssertEquals("entryHeader header", "ER", supportingInfo.CSI_DataModel);

				var cusEntryLine = entryHeader.MergedLines.AddNew();
				supportingInfo = Factory.New<CusSupportingInfo>();
				supportingInfo.Parent = cusEntryLine;
				supportingInfo.OnSaving();
				AssertEquals("cusEntryLine header", "ER", supportingInfo.CSI_DataModel);

				var cusClassPartPivot = Factory.New<BaseCusClassPartPivot>();
				supportingInfo = Factory.New<CusSupportingInfo>();
				supportingInfo.Parent = cusClassPartPivot;
				supportingInfo.OnSaving();
				AssertEquals("cusClassPartPivot header", "ER", supportingInfo.CSI_DataModel);
			});
		}
	}
}
