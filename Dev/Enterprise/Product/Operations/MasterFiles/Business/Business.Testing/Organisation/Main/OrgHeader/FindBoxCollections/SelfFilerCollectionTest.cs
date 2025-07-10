using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SelfFilerCollection))]
	sealed class SelfFilerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new SelfFilerCollection(new BusinessObjectFactory(), orgDefaults);
		}

		public void TestCollectionReturnsSelfFiler()
		{
			var selfFiler = Factory.NewWithValidTestData<OrgHeader>();
			selfFiler.OH_IsForwarder = true;
			selfFiler.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var selfFiler2 = Factory.NewWithValidTestData<OrgHeader>();
			selfFiler2.OH_IsConsignee = true;
			selfFiler2.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var noSelfFiler = Factory.NewWithValidTestData<OrgHeader>();
			noSelfFiler.OH_IsConsignee = false;
			noSelfFiler.OH_IsForwarder = false;
			noSelfFiler.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = false;
			noSelfFiler.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = false;

			var noSelfFiler2 = Factory.NewWithValidTestData<OrgHeader>();
			noSelfFiler2.OH_IsConsignee = false;
			noSelfFiler2.OH_IsForwarder = false;
			noSelfFiler2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;
			noSelfFiler2.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			Factory.Save();

			SelfFilers.Load();

			Assert(SelfFilers.Contains(selfFiler.PK));
			Assert(SelfFilers.Contains(selfFiler2.PK));
			Assert(!SelfFilers.Contains(noSelfFiler.PK));
			Assert(!SelfFilers.Contains(noSelfFiler2.PK));
		}

		public void TestNewChildDefaults()
		{
			var org = SelfFilers.AddNew();
			Assert(org.OH_IsForwarder);
			Assert(org.MiscServ.OM_FWAdvanceCargoReportingSelfFiler);
			Assert(org.OH_IsConsignee);
			Assert(org.MiscServ.OM_IMAdvanceCargoReportingSelfFiler);
		}

		public void TestSelfFilerError()
		{
			var errorMessage = "This organization is not a Self-Filer organization.";

			var selfFiler = Factory.NewWithValidTestData<OrgHeader>();
			selfFiler.OH_IsForwarder = true;
			selfFiler.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = false;

			var actualErrorMessage = SelfFilers.GetAllNotificationsWhenAdditionalFilterNotMet(selfFiler);
			AssertEquals(errorMessage, actualErrorMessage);
		}

		#region Implementation

		SelfFilerCollection SelfFilers;

		protected override void SetUp()
		{
			base.SetUp();
			SelfFilers = new SelfFilerCollection(new BusinessObjectFactory());
		}

		#endregion
	}
}
