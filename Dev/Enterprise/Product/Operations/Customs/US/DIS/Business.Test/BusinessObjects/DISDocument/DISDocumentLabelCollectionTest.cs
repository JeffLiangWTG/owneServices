using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISDocumentLabelCollection))]
	sealed class DISDocumentLabelCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(DISDocumentLabelCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new DISDocumentLabelCollection(Factory, null);
	}
}
