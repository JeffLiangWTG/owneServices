using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TriggerConditionValueParameterCollection))]
	sealed class TriggerConditionValueParameterCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TriggerConditionValueParameterCollection>
	{
		protected override TriggerConditionValueParameterCollection GetCollectionToTest()
		{
			return new TriggerConditionValueParameterCollection(Events.CustomisableEvent00Code);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TriggerConditionValueParameter(Events.CustomisableEvent00Code);
		}
	}
}
