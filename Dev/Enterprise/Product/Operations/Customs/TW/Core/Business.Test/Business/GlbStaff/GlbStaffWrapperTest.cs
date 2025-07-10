using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWGlbStaffWrapper))]
	sealed class GlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<TWGlbStaffWrapper>
	{
		[ExpectNoExceptions]
		public void TestTWPasswordCollection()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company1.GC_Code = "TC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "TB1";
			branch1.GB_IsActive = true;
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";
			var password1 = Factory.NewWithValidTestData<GlbExternalPassword>();
			password1.GP_GS = staff1.PK;
			password1.GP_UserID = "1";
			password1.GP_GC = company1.PK;
			var password2 = Factory.NewWithValidTestData<GlbExternalPassword>();
			password2.GP_GS = staff1.PK;
			password2.GP_UserID = "2";
			password2.GP_GC = company1.PK;
			var password3 = Factory.NewWithValidTestData<GlbExternalPassword>();
			password3.GP_GS = staff1.PK;
			password3.GP_UserID = "3";
			password3.GP_GC = GlbCompany.CurrentCompany.PK;
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";
			var password4 = Factory.NewWithValidTestData<GlbExternalPassword>();
			password4.GP_GS = staff2.PK;
			password4.GP_UserID = "4";
			password4.GP_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			staff1 = factory.Load<GlbStaff>(staff1.PK);
			password1 = factory.Load<GlbExternalPassword>(password1.PK);
			password2 = factory.Load<GlbExternalPassword>(password2.PK);
			password3 = factory.Load<GlbExternalPassword>(password3.PK);
			password4 = factory.Load<GlbExternalPassword>(password4.PK);
			var wrapper1 = CreateNewWrapper(staff1);
			NUnit.Framework.Assert.That(wrapper1.TWPasswordCollection, NUnit.Framework.Has.None.EqualTo(password1));
			NUnit.Framework.Assert.That(wrapper1.TWPasswordCollection, NUnit.Framework.Has.None.EqualTo(password2));
			NUnit.Framework.Assert.That(wrapper1.TWPasswordCollection, NUnit.Framework.Has.Some.EqualTo(password3));
			NUnit.Framework.Assert.That(wrapper1.TWPasswordCollection, NUnit.Framework.Has.None.EqualTo(password4));
			NUnit.Framework.Assert.That(wrapper1.TWPasswordCollection.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(wrapper1.TWPasswordCollection.FindByPK(password3.PK), NUnit.Framework.Is.Not.EqualTo(default(CargoWise.EntityFramework.BusinessObject)));
			using (DisposableEnvironment.ForCompany(company1.GC_Code))
			{
				var wrapper1Cached = CreateNewWrapper(staff1);
				NUnit.Framework.Assert.That(wrapper1Cached.TWPasswordCollection, NUnit.Framework.Has.Some.EqualTo(password3));
				NUnit.Framework.Assert.That(wrapper1Cached.TWPasswordCollection.Count, NUnit.Framework.Is.EqualTo(1));
				Factory.ClearCachedValue<TWGlbStaffWrapper>(staff1.PK.ToString());
				factory = new BusinessObjectFactory();
				company1 = factory.Load<GlbCompany>(company1.PK);
				staff1 = factory.Load<GlbStaff>(staff1.PK);
				password1 = factory.Load<GlbExternalPassword>(password1.PK);
				password2 = factory.Load<GlbExternalPassword>(password2.PK);
				password3 = factory.Load<GlbExternalPassword>(password3.PK);
				password4 = factory.Load<GlbExternalPassword>(password4.PK);
				var wrapper2 = CreateNewWrapper(staff1);
				NUnit.Framework.Assert.That(wrapper2.TWPasswordCollection, NUnit.Framework.Has.Some.EqualTo(password1));
				NUnit.Framework.Assert.That(wrapper2.TWPasswordCollection, NUnit.Framework.Has.Some.EqualTo(password2));
				NUnit.Framework.Assert.That(wrapper2.TWPasswordCollection, NUnit.Framework.Has.None.EqualTo(password3));
				NUnit.Framework.Assert.That(wrapper2.TWPasswordCollection, NUnit.Framework.Has.None.EqualTo(password4));
				NUnit.Framework.Assert.That(wrapper2.TWPasswordCollection.Count, NUnit.Framework.Is.EqualTo(2));
			}
		}

		protected override TWGlbStaffWrapper CreateNewWrapper(GlbStaff staff)
		{
			return TWGlbStaffWrapper.Get(staff);
		}
	}
}
