using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TriggerConditionValueParameter))]
	sealed class TriggerConditionValueParameterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateCode()
		{
			var triggerConditionValueParameters = new TriggerConditionValueParameters(Events.CustomisableEvent00Code, ZString.Empty);

			var param1 = new TriggerConditionValueParameter(Events.CustomisableEvent00Code, "LOC", "AUMEL");
			var param2 = new TriggerConditionValueParameter(Events.CustomisableEvent00Code, "LOC", "AUMEL");

			triggerConditionValueParameters.ParameterCollection.Add(param1);
			triggerConditionValueParameters.ParameterCollection.Add(param2);

			param2.ValidateCode();
			AssertHasError(param2.CodeInfo, "The parameter LOC has been duplicated and must be unique.");
		}

		public void TestCodeList()
		{
			var lookupList = new TriggerConditionValueParameter(Events.CustomisableEvent00Code).CodeList;
			var expected = new CodeDescriptionPairList();

			foreach (FieldInfo codeField in typeof(CargoWise.EventReference.Constants.EventReferenceParameters.Codes).GetFields())
			{
				var description = typeof(Constants.EventReferenceParameters.Descriptions).GetProperties().FirstOrDefault(x => x.Name == codeField.Name);

				if (description != null && description.GetValue(null) != null)
				{
					var descriptionValue = description.GetValue(null) as MultilingualString;

					if (descriptionValue != null)
					{
						expected.AddPair(codeField.GetValue(null).ToString(), descriptionValue);
					}
					else
					{
						expected.AddPair(codeField.GetValue(null).ToString(), description.ToString());
					}
				}
				else
				{
					expected.AddPair(codeField.GetValue(null).ToString(), string.Empty);
				}
			}

			expected.Add(new CodeDescriptionPair(Constants.EventReferenceReservedParameters.Codes.Reference, Constants.EventReferenceReservedParameters.Descriptions.Reference));

			AssertContainsExactElementsInAnyOrder(expected, lookupList);
		}
	}
}
