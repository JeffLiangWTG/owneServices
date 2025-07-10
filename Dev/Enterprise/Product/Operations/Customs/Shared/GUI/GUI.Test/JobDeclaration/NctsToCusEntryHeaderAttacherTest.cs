using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI.Testing
{
	public class NctsToCusEntryHeaderAttacherTest : TestCaseForAttachGUI
	{
		public void TestAttach()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
			var invoiceheader = declaration.Invoices.AddNew();
			var invoiceline = invoiceheader.InvoiceLines.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var jeCEI = declaration.CustomsEntryInstructions.AddNew();
			jeCEI.FillWithValidTestData();
			entry.FillWithValidTestData();
			entry.CH_CEI_Instruction = jeCEI.PK;

			var entryline = entry.MergedLines.AddNew();
			entryline.FillWithValidTestData();
			invoiceline.FillWithValidTestData();
			invoiceline.JI_CEI = jeCEI.PK;
			invoiceline.JI_CL = entryline.PK;

			var nctsHeader = Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
			((BusinessObject)nctsHeader)[CusInBondHeaderSchema.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
			((BusinessObject)nctsHeader)[CusInBondHeaderSchema.BH_HeaderType] = "D";
			var nctsMovement = Factory.LoadTop1<Integration.Customs.EU.NCTS.IDepartureMovementHeader>(new ZQuery(CusInBondMoveHeaderSchema.BM_BH, nctsHeader.PK));
			var nctsGoodsItem = (BusinessObject)Factory.New<Integration.Customs.EU.NCTS.IDepartureCargoDesc>();
			nctsGoodsItem[CusInBondCargoDescSchema.BY_ParentID] = nctsMovement.PK;
			nctsGoodsItem[CusInBondCargoDescSchema.BY_ParentTableCode] = CusInBondMoveHeaderSchema.Constants.Prefix;

			NctsHeaderToAttachCollection collection = new NctsHeaderToAttachCollection(entry);
			collection.Add((BusinessObject)nctsHeader);

			Factory.Save();

			using (var form = new ZForm(declaration))
			{
				var count = declaration.InvoiceLines.Count;
				var attacher = new NctsToCusEntryHeaderAttacherForTest(entry, collection);
				attacher.Show(form);
				AssertEquals("Should have called through to base Show", 1, attacher.BaseShowCoreCallCountForTesting);

				attacher.AttachCoreExposed((BusinessObject)nctsHeader, null);

				AssertEquals("The ncts should have been added to the GoodsItemsForIntegration collection.", count + 1, declaration.InvoiceLines.Count);
			}
		}

		public class NctsToCusEntryHeaderAttacherForTest : NctsToCusEntryHeaderAttacher
		{
			public NctsToCusEntryHeaderAttacherForTest(CusEntryHeader entryHeader, IBusinessObjectCollection findBoxList) : base(entryHeader, findBoxList)
			{
			}

			public bool AttachCoreExposed(BusinessObject bizO, List<BusinessObject> listToBulkAdd) => base.AttachCore(bizO, listToBulkAdd);
		}
	}
}
