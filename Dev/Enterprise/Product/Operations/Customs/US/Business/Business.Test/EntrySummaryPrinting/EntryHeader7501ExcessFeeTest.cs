using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryHeader7501ExcessFee))]
	sealed class EntryHeader7501ExcessFeeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIParentDocManagerSupportMembers()
		{
			var entryHeader7501ExcessFee = new EntryHeader7501ExcessFee(Entry, ChargeFee);

			IParentDocManagerSupport supporter = entryHeader7501ExcessFee;
			AssertEquals(Core.Constants.DocManagerCodes.CustomsEntry, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", Entry.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", Entry.TableName, supporter.ParentTableName);
		}

		public void TestIDocumentDeliveredLogSupporterMembers()
		{
			var entryHeader7501ExcessFee = new EntryHeader7501ExcessFee(Entry, ChargeFee);

			IDocumentDeliveredLogSupporter supporter = entryHeader7501ExcessFee;
			AssertEquals("BusinessObjectTypeToLogAgainst", typeof(CusEntryHeader), supporter.BusinessObjectTypeToLogAgainst);
			AssertEquals("Identifier", Entry.PK, supporter.Identifier);
		}

		protected override BusinessObject GetNewBusinessObject() => new EntryHeader7501ExcessFee(Factory.New<CusEntryHeader>(), ChargeFee);

		CusEntryHeaderCharges chargeFee;
		CusEntryHeaderCharges ChargeFee => chargeFee ?? (chargeFee = Entry.Charges.AddNew());

		CusEntryHeader entry;
		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					entry = declaration.CustomsEntryHeaders.AddNew();
				}
				return entry;
			}
		}
	}
}
