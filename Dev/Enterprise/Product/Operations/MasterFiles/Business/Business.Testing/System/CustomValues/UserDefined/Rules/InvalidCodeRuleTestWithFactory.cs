using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class InvalidCodeRuleTestWithFactory : TestCaseWithFactory
	{
		public void TestGetObjectForBinding()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("A1", "AAA 111");
			list.AddPair("B1", "BBB 111");
			list.AddPair("C1", "CCC 111");

			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();
			addOnRule.SetRules(new InvalidCodeRule() { List = list });

			AvailableRule availableRule = addOnRule.AllRules.Cast<AvailableRule>().First(r => r.Rule.Code.Equals(CargoWise.Workflow.CustomAddOnRuleTypes.InvalidCode));
			availableRule.IsEnabled = true;
			InvalidCodeRule rule = (InvalidCodeRule)availableRule.Rule;
			BusinessObject objForBinding = rule.GetObjectForBinding(Factory);
			availableRule.RegisterEditableChildObject(objForBinding);

			IBusinessObjectCollection collection = (IBusinessObjectCollection)objForBinding["Collection"];
			AssertEquals(3, collection.Count);

			collection.RemoveAt(1);

			BusinessObject elem = collection.AddNew();
			elem["Code"] = "A1";
			AssertHasError(elem.FindPropertyInfo("Code"), "The Code has been duplicated and must be unique.");
			elem["Code"] = "";
			AssertHasError(elem.FindPropertyInfo("Code"), "Please enter a value.");
			elem["Code"] = "D1";
			AssertNoErrors(elem.FindPropertyInfo("Code"));
			elem["Description"] = "DDD 111";

			Factory.Save();

			ICustomAddOnRule[] newRules = addOnRule.GetRules();
			AssertEquals(4, newRules.Length);
			CodeDescriptionPairList newList = (CodeDescriptionPairList)(newRules.OfType<InvalidCodeRule>().Single()).List;
			AssertEquals(3, newList.Count);
			AssertEquals("A1", newList[0].Code);
			AssertEquals("AAA 111", newList[0].Description);
			AssertEquals("C1", newList[1].Code);
			AssertEquals("CCC 111", newList[1].Description);
			AssertEquals("D1", newList[2].Code);
			AssertEquals("DDD 111", newList[2].Description);
		}
	}
}
