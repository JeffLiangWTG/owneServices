using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class WhsLocationDataHelperTest : LocationValidationBaseTest
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
				var helper = new WhsLocationDataHelper();
				return helper.FindLocationPK(factory, locationString, whsPK);
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
				var helper = new WhsLocationDataHelper();
				helper.ValidateLocation(parent, parent.LocationStringInfo);
			}
		}
	}
}
