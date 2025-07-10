using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class FuncExtensionsTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestAnd()
		{
			var queryable = new List<QueriedObject>();
			queryable.Add(new QueriedObject() { Field1 = 1, Field2 = 2, Field3 = "test 1" });
			queryable.Add(new QueriedObject() { Field1 = 1, Field2 = 3, Field3 = "test 2" });
			queryable.Add(new QueriedObject() { Field1 = 5, Field2 = 6, Field3 = "test 3" });
			queryable.Add(new QueriedObject() { Field1 = 7, Field2 = 9, Field3 = "test 4" });

			Func<QueriedObject, bool> predicate1 = x => x.Field1 == 1;
			Func<QueriedObject, bool> predicate2 = x => x.Field2 == 3;
			var andPredicate = predicate1.And(predicate2);
			var result = queryable.First(andPredicate);
			NUnit.Framework.Assert.That(result.Field3, NUnit.Framework.Is.EqualTo("test 2"));

			Func<QueriedObject, bool> predicate3 = x => x.Field1 == 7;
			Func<QueriedObject, bool> predicate4 = x => x.Field2 == 9;
			andPredicate = predicate4.And(predicate3);
			result = queryable.First(predicate4);
			NUnit.Framework.Assert.That(result.Field3, NUnit.Framework.Is.EqualTo("test 4"));
		}

		class QueriedObject
		{
			public int Field1;

			public int Field2;

			public string Field3;
		}
	}
}
