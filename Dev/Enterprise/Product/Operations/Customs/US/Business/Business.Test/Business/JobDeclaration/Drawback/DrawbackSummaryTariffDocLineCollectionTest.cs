using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.JobDeclarationDrawbackSupporter;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackSummaryTariffDocLineCollection))]
	class DrawbackSummaryTariffDocLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DrawbackSummaryTariffDocLineCollection>
	{
		protected override DrawbackSummaryTariffDocLineCollection GetCollectionToTest()
		{
			var tariffs = new List<ZString>();
			return new DrawbackSummaryTariffDocLineCollection(tariffs);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DrawbackSummaryTariffDocLine(new List<ZString>());
		}
	}
}
