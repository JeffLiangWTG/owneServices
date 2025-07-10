using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionComparer))]
	sealed class CusEntryInstructionComparerTest : Customs.Business.Testing.CusEntryInstructionComparerAbstractTest<CusEntryInstructionComparer>
	{
		[ExpectNoExceptions]
		public override void TestGetUniquenessNotificationSeverity()
		{
			NUnit.Framework.Assert.That(comparer.GetUniquenessNotificationSeverity(), NUnit.Framework.Is.EqualTo(NotificationType.Information));
		}
	}
}
