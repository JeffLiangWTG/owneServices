using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportNonCondensedDeclarationMessageSendingObject))]
	sealed class ExportNonCondensedDeclarationMessageSendingObjectTests : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestDeclaration_GoodsShipment()
		{
			IN5203Declaration messageSendingObject = new ExportNonCondensedDeclarationMessageSendingObject(GetEntryHeaderWithMinimumData());
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment.GetType(), NUnit.Framework.Is.EqualTo(typeof(ExportNonCondensedDeclarationGoodsShipment)));
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
			return new ExportNonCondensedDeclarationMessageSendingObject(GetEntryHeaderWithMinimumData());
		}

		[ExpectNoExceptions]
		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name == "MessageType")
			{
				NUnit.Framework.Assert.That((ZString)info.Value, NUnit.Framework.Is.EqualTo("ECD").Using(CustomComparers.TypeComparison));
			}
			else
			{
				base.TestBizObjectField(info);
			}
		}
	}
}
