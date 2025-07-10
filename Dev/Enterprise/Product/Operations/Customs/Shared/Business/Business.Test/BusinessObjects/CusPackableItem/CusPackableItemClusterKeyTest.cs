using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackableItem))]
	sealed class CusPackableItemClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var packingList = (CusPackingList)NewParentObject();
			var invoice = packingList.Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = invoiceLine.CreateNewCusPackableItem();
			packingList.PackableItems.Add(cusPackableItem);
			return cusPackableItem;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var packingList = declaration.CreateCusPackingList(Factory);
			return packingList;
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override bool IsFkToParentMandatory() => true;
	}
}
