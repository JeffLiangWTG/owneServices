using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class LicensingMessageSendingObjectLookupsAbstractTest<TLicensingMessageSendingObject, TCodeDescriptionPairList> : TestCaseWithFactory
		where TLicensingMessageSendingObject : LicensingMessageSendingObject
		where TCodeDescriptionPairList : CodeDescriptionPairList, new()
	{
		[ExpectNoExceptions]
		public void TestActionList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var parent = (TLicensingMessageSendingObject)Activator.CreateInstance(typeof(TLicensingMessageSendingObject), header);
			NUnit.Framework.Assert.That(parent.Lookups.ActionList, NUnit.Framework.Is.EquivalentTo(Factory.GetCachedValue<TCodeDescriptionPairList>()).Using(CustomComparers.TypeComparison));
		}
	}
}
