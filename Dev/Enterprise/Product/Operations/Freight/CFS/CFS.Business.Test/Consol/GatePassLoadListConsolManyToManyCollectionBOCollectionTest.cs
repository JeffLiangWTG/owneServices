using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassLoadListConsolManyToManyCollection))]
	public class GatePassLoadListConsolManyToManyCollectionBOCollectionTest : CFSLoadListConsolManyToManyCollectionBOCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GatePassShipment parent = Factory.New<GatePassShipment>();
			Factory.Save();
			return new GatePassLoadListConsolManyToManyCollection(parent);
		}

		protected override Type ParentConsolType
		{
			get { return typeof(GatePassLoadListConsol); }
		}
	}
}
