using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	public static class BIRDUpdateTestToolForUntestedFields
	{
		public static void TestUnpopulatedFields(Type typeofMessageBlock, string[] excludedFields, IBIRDRecord[] populatedRecords)
		{
			List<FieldInfo> unpopulatedFields = GetUnpopulatedAndUnTestedFields(typeofMessageBlock, excludedFields, populatedRecords);

			//all the fields in the messageblock are populated and tested
			if (unpopulatedFields.Count == 0)
			{
				TestCaseWithFactory.Assert(true);
			}
			else
			{
				ZStringBuilder message = new ZStringBuilder();
				message.Append(@"The following fields have not been populated and therefore, untested. 
If a field is unapproapriate to test in this context, you should have an end-to-end test somewhere and  
include the field name in GetUnpopulatedAndUnTestedFields:");

				foreach (FieldInfo field in unpopulatedFields)
				{
					message.Append(field.Name + ",");
				}

				TestCaseWithFactory.Fail(message.ToString().TrimEnd(','));
			}
		}

		static List<FieldInfo> GetUnpopulatedAndUnTestedFields(Type typeofMessageBlock, string[] excludedFieldNames, IBIRDRecord[] populatedRecords)
		{
			List<FieldInfo> result = new List<FieldInfo>();

			List<string> excludedFields = new List<string>(excludedFieldNames);

			foreach (FieldInfo field in typeofMessageBlock.GetFields(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!excludedFields.Contains(field.Name) && field.GetCustomAttributes(typeof(MessageBlockAttribute), false).Length > 0)
				{
					result.Add(field);
				}
			}

			foreach (IBIRDRecord lineRecord in populatedRecords)
			{
				foreach (FieldInfo field in result.ToArray())
				{
					IZType value = (IZType)field.GetValue(lineRecord);

					if (!value.IsEmpty)
					{
						result.Remove(field);
					}
				}
			}

			return result;
		}
	}
}
