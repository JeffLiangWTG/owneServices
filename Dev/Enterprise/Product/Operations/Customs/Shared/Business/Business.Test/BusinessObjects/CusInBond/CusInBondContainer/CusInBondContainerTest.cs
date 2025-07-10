using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusInBondContainer))]
	public abstract class CusInBondContainerTest<T> : BaseCusInBondContainerTest<T>
		where T : CusInBondContainer
	{
		protected override IEnumerable<Type> GetBaseTypes()
		{
			yield return typeof(BaseCusInBondContainer);
			yield return typeof(CusInBondContainer);
		}
	}
}
