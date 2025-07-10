using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(USVoyagePortCollection))]
	sealed class USVoyagePortCollectionTest : NonPersistentBusinessObjectCollectionViewTestCase<USVoyagePortCollection>
	{
		protected override USVoyagePortCollection GetCollectionToTest()
		{
			return new USVoyagePortCollection(new VoyagePortCollection(Voyage));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new VoyagePort(Voyage, "USLAX");
		}

		JobVoyage Voyage
		{
			get
			{
				return fVoyage ?? (fVoyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage fVoyage;
	}
}
