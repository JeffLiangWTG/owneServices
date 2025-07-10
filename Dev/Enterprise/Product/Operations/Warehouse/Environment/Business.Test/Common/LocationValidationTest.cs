using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class LocationValidationTest : LocationValidationBaseTest
	{
		protected override LocationCrashTestDummy LocationCrashTestDummy => Factory.New<CrashTestDummy>();

		public class CrashTestDummy : LocationCrashTestDummy
		{
			public CrashTestDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZGuid FindLocationPK(BusinessObjectFactory factory, ZString locationString, ZGuid whsPK)
			{
				return WhsLocation.FindLocationPK(factory, locationString, whsPK);
			}

			protected override void ValidateLocationString()
			{
				new CrashTestDummyValidation(this).ValidateLocationString();
			}
		}

		public class CrashTestDummyValidation : LocationCrashTestDummyValidation
		{
			public CrashTestDummyValidation(CrashTestDummy parent)
				: base(parent)
			{
			}

			protected override void CheckLocationString()
			{
				var parent = (CrashTestDummy)Parent;
				WhsLocation.ValidateLocation(parent, parent.LocationStringInfo);
			}
		}
	}
}
