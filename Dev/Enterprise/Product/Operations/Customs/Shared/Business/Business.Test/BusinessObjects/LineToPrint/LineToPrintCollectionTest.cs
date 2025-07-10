using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(LineToPrintCollection))]
	public sealed class LineToPrintCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LineToPrintCollection>
	{
		public void TestShouldBePrintedWhenCollectionHas1Element()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var coll = new OneElementLineToPrintCollection(declaration);
			Assert(coll[0].ShouldBePrinted);
		}

		protected override LineToPrintCollection GetCollectionToTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			return new LineToPrintCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyLineToPrint(Factory.New<DummyBusinessObject>());
		}

		public class DummyLineToPrint : LineToPrint
		{
			public DummyLineToPrint(BusinessObject bizObj)
				: base(bizObj)
			{
			}

			public override ZString Organisation
			{
				get { return ""; }
			}

			public override ZString Identifier
			{
				get { return ""; }
			}

			public override ZDateTime LastPrintDate
			{
				get { return ZDateTime.Now; }
			}
		}

		class OneElementLineToPrintCollection : LineToPrintCollection
		{
			public OneElementLineToPrintCollection(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override void GetLinesToPrint(BaseJobDeclaration declaration)
			{
				Add(new DummyLineToPrint(Factory.New<DummyBusinessObject>()));
			}
		}
	}
}
