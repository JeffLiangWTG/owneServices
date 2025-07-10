using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class AccountingAssertionHelper
	{
		public static void AssertContainsInOrderNewLineSensitive(string message, string actualString, params string[] expectedElementInOrder)
		{
			const string regexConnector = "([^\n]*)";
			var stringBuilder = new ZStringBuilder(expectedElementInOrder);
			var regexPattern = stringBuilder.ToStringWithDelimiterBetweenAppends(regexConnector);

			var result = Regex.IsMatch(actualString, regexPattern);

			var assertionMessage = new ZStringBuilder().
				AppendIfNotEmpty(message).
				Append("Expected String Containing in Order:").
				Append(stringBuilder.ToStringWithNewLineBetweenAppends()).
				Append("Actual Result:").
				Append(actualString).
				ToStringWithNewLineBetweenAppends();

			NUnit.Framework.Assertion.Assert(assertionMessage, result);
		}

		public static void AssertFetchHint<MasterBizoType, ChildBizoType>(SchemaColumn column, int dbHits = 2, Tuple<SchemaColumn, string[]> valuesToAvoidUniqueIndexCheck = null)
			where MasterBizoType : BusinessObject
			where ChildBizoType : BusinessObject
		{
			var factory = new BusinessObjectFactory();
			var masterBizos = new List<MasterBizoType>();
			SchemaGuidColumn masterBizoPKColumn = null;
			int masterBizoCount = 3;
			int childBizoCount = 2;
			for (int i = 0; i < masterBizoCount; i++)
			{
				var masterBizo = factory.NewWithValidTestData<MasterBizoType>();
				if (masterBizoPKColumn == null)
				{
					masterBizoPKColumn = masterBizo.PKSchemaColumn;
				}
				for (int j = 0; j < childBizoCount; j++)
				{
					var childBizo = factory.NewWithValidTestData<ChildBizoType>();
					childBizo[column] = masterBizo.PK;

					if (valuesToAvoidUniqueIndexCheck != null && valuesToAvoidUniqueIndexCheck.Item2.Length == childBizoCount)
					{
						childBizo[valuesToAvoidUniqueIndexCheck.Item1] = valuesToAvoidUniqueIndexCheck.Item2[j];
					}
				}
				masterBizos.Add(masterBizo);
			}
			factory.Save();

			factory = new BusinessObjectFactory();
			factory.ResetDatabaseLoadCount();
			var masterBizosInNewFactory = factory.Load<MasterBizoType>(new ZQuery(masterBizoPKColumn, masterBizos.Select(x => x.PK)));
			foreach (var masterBizo in masterBizosInNewFactory)
			{
				var childBizosInNewFactory = factory.Load<ChildBizoType>(new ZQuery(column, masterBizo.PK));
				Assertion.AssertEquals(childBizoCount, childBizosInNewFactory.Length);
			}

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			TestCaseWithFactory.AssertMaxDbHits(dbHits, factory);
		}
	}
}
