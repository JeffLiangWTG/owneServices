using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(PackUnpackLoadListConsolManyToManyCollection))]
	public class PackUnpackLoadListConsolManyToManyCollectionBOCollectionTest : CFSLoadListConsolManyToManyCollectionBOCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			PackUnpackShipment parent = Factory.New<PackUnpackShipment>();
			Factory.Save();
			return new PackUnpackLoadListConsolManyToManyCollection(parent);
		}

		protected override Type ParentConsolType
		{
			get { return typeof(PackUnpackLoadListConsol); }
		}
	}
}
