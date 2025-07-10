using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TriggerConditionValueParameters))]
	sealed class TriggerConditionValueParametersTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCompleteText()
		{
			var parameters = new TriggerConditionValueParameters(Events.CustomisableEvent00Code, string.Empty);
			AssertEquals(string.Empty, parameters.CompleteText);

			parameters = new TriggerConditionValueParameters(Events.CustomisableEvent00Code, "REF=FREETEXT,LOC=AUSYD,DEP=DEPARTMENT");
			AssertEquals(3, parameters.ParameterCollection.Count);
			AssertEquals("DEP=DEPARTMENT,LOC=AUSYD,REF=FREETEXT", parameters.CompleteText);

			parameters = new TriggerConditionValueParameters(Events.CustomisableEvent00Code, "REF!=FREETEXT,LOC=AUSYD,DEP=DEPARTMENT");
			AssertEquals(2, parameters.ParameterCollection.Count);
			AssertEquals("DEP=DEPARTMENT,LOC=AUSYD", parameters.CompleteText);

			parameters = new TriggerConditionValueParameters(Events.CustomisableEvent00Code, string.Empty);
			parameters.ParameterCollection.Add(new TriggerConditionValueParameter(Events.CustomisableEvent00Code, "REF", "TEST"));
			parameters.ParameterCollection.Add(new TriggerConditionValueParameter(Events.CustomisableEvent00Code, "RES", "REASON"));
			parameters.ParameterCollection.Add(new TriggerConditionValueParameter(Events.CustomisableEvent00Code, "TYP", "EMPTY"));
			AssertEquals("REF=TEST,RES=REASON,TYP=EMPTY", parameters.CompleteText);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TriggerConditionValueParameters(Events.CustomisableEvent00Code, string.Empty);
		}
	}
}
