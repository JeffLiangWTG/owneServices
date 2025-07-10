using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(NonPersistentBusinessObjectCollectionHolder<LocalReferenceNumberHolder>))]
	sealed class NonPersistentBusinessObjectCollectionHolderTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentBusinessObjectCollectionHolder<LocalReferenceNumberHolder>>
	{
		protected override NonPersistentBusinessObjectCollectionHolder<LocalReferenceNumberHolder> GetCollectionToTest()
		{
			return new NonPersistentBusinessObjectCollectionHolder<LocalReferenceNumberHolder>(VATInstruction, (parent) => new LocalReferenceNumberHolder(parent));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LocalReferenceNumberHolder(VATInstruction);
		}

		VAT404DocumentInstruction VATInstruction => vatInstruction ?? (vatInstruction = new VAT404DocumentInstruction(Factory));
		VAT404DocumentInstruction vatInstruction;
	}
}
