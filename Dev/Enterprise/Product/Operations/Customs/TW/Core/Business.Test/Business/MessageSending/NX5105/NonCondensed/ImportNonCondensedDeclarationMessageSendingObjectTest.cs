using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImportNonCondensedDeclarationMessageSendingObject))]
	sealed class ImportNonCondensedDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestDeclaration_GoodsShipment()
		{
			INX5105Declaration messageSendingObject = new ImportNonCondensedDeclarationMessageSendingObject(GetEntryHeaderWithMinimumData());
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment.GetType(), NUnit.Framework.Is.EqualTo(typeof(ImportNonCondensedDeclarationGoodsShipment)));
		}

		CusEntryHeader GetEntryHeaderWithMinimumData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return entryHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportNonCondensedDeclarationMessageSendingObject(GetEntryHeaderWithMinimumData());
		}

		[ExpectNoExceptions]
		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name == "MessageType")
			{
				NUnit.Framework.Assert.That((ZString)info.Value, NUnit.Framework.Is.EqualTo("ICD").Using(CustomComparers.TypeComparison));
			}
			else
			{
				base.TestBizObjectField(info);
			}
		}
	}
}
