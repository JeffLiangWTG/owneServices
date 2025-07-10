using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CALicenceNumberCollection))]
	public class CALicenceNumberCollectionTest : CusCodeDataCollectionTest<CALicenceNumber>
	{
		public void TestAllowNew()
		{
			CALicenceNumberCollection licenceNumberCollection = new CALicenceNumberCollection(Declaration);
			AssertEquals("Max count is 5", 5, licenceNumberCollection.MaxCount);
		}

		protected override Customs.Business.CusCodeDataCollection<CALicenceNumber> GetCusCodeDataCollection()
		{
			return new CALicenceNumberCollection(Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CALicenceNumber result = Factory.New<CALicenceNumber>();
			result.CY_ParentID = Declaration.PK;
			result.CY_ParentTableCode = Declaration.TablePrefix;
			return result;
		}

		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
	}
}
