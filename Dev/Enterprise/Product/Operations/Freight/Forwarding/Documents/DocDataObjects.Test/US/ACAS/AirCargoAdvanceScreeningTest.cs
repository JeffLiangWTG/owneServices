using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	[TestedType(typeof(AirCargoAdvanceScreening))]
	sealed class AirCargoAdvanceScreeningTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var acas = new AirCargoAdvanceScreening("zzz", "zzz", "AirCargoAdvanceScreening");
			acas.HarmonizedCodes = new List<ZString>();

			return acas;
		}
	}
}
