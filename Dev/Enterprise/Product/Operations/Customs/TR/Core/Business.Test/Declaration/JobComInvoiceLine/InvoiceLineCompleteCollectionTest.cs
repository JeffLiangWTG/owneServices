using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			return dec;
		}

		public override void TestAddNew()
		{
			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, Declaration.Branch.EntityPK.ToGuid(), Guid.Empty, false))
			{
				base.TestAddNew();
			}
		}

		protected override Customs.Business.CusEntryInstruction GetCusEntryInstruction(BaseJobDeclaration declaration) => (CusEntryInstruction)((JobDeclaration)declaration).CustomsEntryInstructions.FirstOrDefault() ?? ((JobDeclaration)declaration).CustomsEntryInstructions.AddNew();
	}
}
