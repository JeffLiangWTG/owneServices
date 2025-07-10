using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbPersonFilterBusinessObject))]
	sealed class GlbPersonFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbPersonFilterBusinessObject();
		}

		#region Filter Securities

		public void TestEmailFilterSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			var staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffDenied.PK));

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewEmail, false, staffDenied.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewEmail.Parent, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewEmail, true, staffGranted.PK));

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter1 = new GlbPersonFilterBusinessObject();
					AssertNull(filter1["Email"]);
					AssertNotNull(filter1["Related Email Address"]);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter2 = new GlbPersonFilterBusinessObject();
					AssertNotNull(filter2["Email"]);
					AssertNotNull(filter2["Related Email Address"]);
				}
			}
		}

		public void TestCountryFilterSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			var staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffDenied.PK));

			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewHomeAddress, false, staffDenied.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewHomeAddress.Parent, true, staffDenied.PK));
			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceViewHomeAddress, true, staffGranted.PK));

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter1 = new GlbPersonFilterBusinessObject();
					AssertNull(filter1["Country"]);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter2 = new GlbPersonFilterBusinessObject();
					AssertNotNull(filter2["Country"]);
				}
			}
		}

		public void TestOrgContactFilterSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			var staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffDenied.PK));

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.OrganisationView, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.OrganisationView, false, staffDenied.PK));

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter1 = new GlbPersonFilterBusinessObject();
					AssertNull(filter1["Organization Contact"]);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter2 = new GlbPersonFilterBusinessObject();
					AssertNotNull(filter2["Organization Contact"]);
				}
			}
		}

		public void TestHRJobApplicantFilterSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			var staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffDenied.PK));

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantView, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.HRJobApplicantView, false, staffDenied.PK));

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter1 = new GlbPersonFilterBusinessObject();
					AssertNull(filter1["Job Applicant"]);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter2 = new GlbPersonFilterBusinessObject();
					AssertNotNull(filter2["Job Applicant"]);
				}
			}
		}

		public void TestGlbStaffFilterSecurity()
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			var staffGranted = Factory.NewWithValidTestData<GlbStaff>();

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.PersonIntelligenceEdit, true, staffDenied.PK));

			staffGranted.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.StaffView, true, staffGranted.PK));
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.StaffView, false, staffDenied.PK));

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter1 = new GlbPersonFilterBusinessObject();
					AssertNull(filter1["Staff"]);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffGranted.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					var filter2 = new GlbPersonFilterBusinessObject();
					AssertNotNull(filter2["Staff"]);
				}
			}
		}

		GlbSecurity GetSecurity(SecurityCheckpoint checkpoint, bool granted, ZGuid staffPk)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staffPk;

			return security;
		}

		#endregion

		#region TestTextFilter

		public void TestEmailFilter()
		{
			GlbPerson person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_EmailAddress = "a@a.com";

			GlbPerson person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_EmailAddress = "b@b.com";

			Factory.Save();

			GlbPersonFilterBusinessObject glbPersonFilter = new GlbPersonFilterBusinessObject();
			((ModuleTextFilter)glbPersonFilter["Email"]).Property = "a";
			((ModuleTextFilter)glbPersonFilter["Email"]).IsActive = true;

			GlbPersonCollection collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));
		}

		public void TestFriendlyNameFilter()
		{
			GlbPerson person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FriendlyName = "Jozsi";

			GlbPerson person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FriendlyName = "Feri";

			Factory.Save();

			GlbPersonFilterBusinessObject glbPersonFilter = new GlbPersonFilterBusinessObject();
			((ModuleTextFilter)glbPersonFilter["Friendly Name"]).Property = "Fer";
			((ModuleTextFilter)glbPersonFilter["Friendly Name"]).IsActive = true;

			GlbPersonCollection collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection1.Contains(person1));
			Assert("Collection should contain Person2", collection1.Contains(person2));
		}

		public void TestFullNameFilter()
		{
			GlbPerson person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Jozsi Kovacs";

			GlbPerson person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Feri Szabo";

			Factory.Save();

			GlbPersonFilterBusinessObject glbPersonFilter = new GlbPersonFilterBusinessObject();
			((ModuleTextFilter)glbPersonFilter["Full Name"]).Property = "Jozs";
			((ModuleTextFilter)glbPersonFilter["Full Name"]).IsActive = true;

			GlbPersonCollection collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));
		}

		public void TestRelatedEmailAddressFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var person3 = Factory.NewWithValidTestData<GlbPerson>();

			person1.PER_EmailAddress = "personA@test.com";
			person2.PER_EmailAddress = "personA@test.com";
			person3.PER_EmailAddress = "personB@test.com";

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;

			emailFilter.Property = "personA@test.com";
			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, collection.Any());

			emailFilter.Property = "nonperson@test.com";
			var noResultsCollection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, noResultsCollection.Any());
		}

		public void TestRelatedEmailAddressFilter_SingleApplicant()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();

			applicant1.HA_EmailAddress = "applicant1@test.com";
			applicant2.HA_EmailAddress = "applicant2@test.com";

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;

			emailFilter.Property = "applicant1@test.com";
			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, collection.Any());

			emailFilter.Property = "nonapplicant@test.com";
			var noResultsCollection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, noResultsCollection.Any());
		}

		public void TestRelatedEmailAddressFilter_SingleContact()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var contact3 = Factory.NewWithValidTestData<OrgContact>();

			contact1.OC_Email = "contactA@test.com";
			contact2.OC_Email = "contactA@test.com";
			contact3.OC_Email = "contactB@test.com";

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;

			emailFilter.Property = "contactA@test.com";
			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { contact1.OC_PER, contact2.OC_PER }, collection.Select(p => p.PK));

			emailFilter.Property = "noncontact@test.com";
			var noResultsCollection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, noResultsCollection.Any());
		}

		public void TestRelatedEmailAddressFilter_SingleStaff()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_EmailAddress = "staffA@test.com";
			staff2.GS_EmailAddress = "staffA@test.com";
			staff3.GS_EmailAddress = "staffB@test.com";

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;

			emailFilter.Property = "staffA@test.com";
			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff1.GS_PER, staff2.GS_PER }, collection.Select(p => p.PK));

			emailFilter.Property = "nonstaff@test.com";
			var noResultsCollection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, noResultsCollection.Any());
		}

		public void TestRelatedEmailAddressFilter_MultiplePersonsWithSingleChildren_SameEmails()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			person.PER_EmailAddress = "test@test.com";
			applicant.HA_EmailAddress = "test@test.com";
			contact.OC_Email = "test@test.com";
			staff.GS_EmailAddress = "test@test.com";

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;
			emailFilter.Property = "test@test.com";

			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { contact.OC_PER, staff.GS_PER }, collection.Select(p => p.PK));
		}

		public void TestPersonalEmailFilter_MultiplePersonsWithSingleChildren_SameEmails()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			person.PER_EmailAddress = "test@test.com";
			applicant.HA_EmailAddress = "test@test.com";
			contact.OC_Email = "test@test.com";
			staff.GS_EmailAddress = "test@test.com";

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Email"];
			emailFilter.IsActive = true;
			emailFilter.Property = "test@test.com";

			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { person.PK, applicant.HA_PER }, collection.Select(p => p.PK));
		}

		public void TestRelatedEmailAddressFilter_SinglePersonWithMultipleChildren_SameChildEmails()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			person.PER_EmailAddress = "person@test.com";
			applicant.HA_EmailAddress = "child@test.com";
			contact.OC_Email = "child@test.com";
			staff.GS_EmailAddress = "child@test.com";

			applicant.HA_PER = person.PK;
			contact.OC_PER = person.PK;
			staff.GS_PER = person.PK;

			Factory.Save();

			person.PER_EmailAddress = "person@test.com";
			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;
			emailFilter.Property = "child@test.com";

			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(person.PK, collection.Single().PK);
		}

		public void TestRelatedEmailAddressFilter_SinglePersonWithMultipleChildren_DifferentEmails()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			person.PER_EmailAddress = "person@test.com";
			applicant.HA_EmailAddress = "applicant@test.com";
			contact.OC_Email = "contact@test.com";
			staff.GS_EmailAddress = "staff@test.com";

			applicant.HA_PER = person.PK;
			contact.OC_PER = person.PK;
			staff.GS_PER = person.PK;

			Factory.Save();

			person.PER_EmailAddress = "person@test.com";
			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;

			emailFilter.Property = "person@test.com";
			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, collection1.Any());

			emailFilter.Property = "applicant@test.com";
			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, collection2.Any());

			emailFilter.Property = "contact@test.com";
			var collection3 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(person.PK, collection3.Single().PK);

			emailFilter.Property = "staff@test.com";
			var collection4 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(person.PK, collection4.Single().PK);
		}

		public void TestRelatedEmailAddressFilter_MultiplePersonsWithSingleChildren_WithSomeDuplicatedEmails()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();

			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			person1.PER_EmailAddress = "test1@test.com";
			applicant1.HA_EmailAddress = "test2@test.com";
			contact1.OC_Email = "test3@test.com";
			staff1.GS_EmailAddress = "test4@test.com";

			person2.PER_EmailAddress = "test3@test.com";
			applicant2.HA_EmailAddress = "test1@test.com";
			contact2.OC_Email = "test4@test.com";
			staff2.GS_EmailAddress = "test2@test.com";

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;

			emailFilter.Property = "test1@test.com";
			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, collection1.Any());

			emailFilter.Property = "test2@test.com";
			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff2.GS_PER }, collection2.Select(p => p.PK));

			emailFilter.Property = "test3@test.com";
			var collection3 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { contact1.OC_PER }, collection3.Select(p => p.PK));

			emailFilter.Property = "test4@test.com";
			var collection4 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { staff1.GS_PER, contact2.OC_PER }, collection4.Select(p => p.PK));
		}

		public void TestRelatedEmailAddressFilter_MultiplePersonsWithMultipleChildren_WithSomeDuplicatedEmails()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();

			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			person1.PER_EmailAddress = "test1@test.com";
			applicant1.HA_EmailAddress = "test2@test.com";
			contact1.OC_Email = "test3@test.com";
			staff1.GS_EmailAddress = "test4@test.com";

			person2.PER_EmailAddress = "test3@test.com";
			applicant2.HA_EmailAddress = "test5@test.com";
			contact2.OC_Email = "test4@test.com";
			staff2.GS_EmailAddress = "test1@test.com";

			applicant1.HA_PER = person1.PK;
			contact1.OC_PER = person1.PK;
			staff1.GS_PER = person1.PK;
			applicant2.HA_PER = person2.PK;
			contact2.OC_PER = person2.PK;
			staff2.GS_PER = person2.PK;

			Factory.Save();

			person1.PER_EmailAddress = "test1@test.com";
			person2.PER_EmailAddress = "test3@test.com";
			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;

			emailFilter.Property = "test1@test.com";
			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(person2.PK, collection1.Single().PK);

			emailFilter.Property = "test2@test.com";
			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, collection2.Any());

			emailFilter.Property = "test3@test.com";
			var collection3 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(person1.PK, collection3.Single().PK);

			emailFilter.Property = "test4@test.com";
			var collection4 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { person1.PK, person2.PK }, collection4.Select(p => p.PK));

			emailFilter.Property = "test5@test.com";
			var collection5 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, collection5.Any());
		}

		public void TestRelatedEmailAddressFilterUsesUnionAll()
		{
			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var emailFilter = (ModuleTextFilter)glbPersonFilter["Related Email Address"];
			emailFilter.IsActive = true;

			emailFilter.Property = "person@test.com";
			AssertEquals(@"PER_PK IN 
(
	SELECT GS_PER FROM dbo.GlbStaff WHERE GS_EmailAddress like N'person@test.com%' UNION ALL SELECT OC_PER FROM dbo.OrgContact WHERE 
	(
		OC_Email like N'person@test.com%' 
		AND
		OC_Email <> ''
	)
)
", glbPersonFilter.Filter.LiteralTextSqlFormatted);
		}

		public void TestRelatedOrgNameFilter_WithOrgHeader()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_FullName = "Arthur's";
			organization2.OH_FullName = "Bobby's";

			var address1 = organization1.Addresses.AddNew();
			var address2 = organization2.Addresses.AddNew();
			address1.OA_CompanyNameOverride = "Collin's";
			address2.OA_CompanyNameOverride = "Dobby's";
			address1.OA_Address1 = "321 true st";
			address2.OA_Address1 = "123 fake st";

			var contact1 = organization1.Contacts.AddNew();
			var contact2 = organization2.Contacts.AddNew();
			contact1.OC_ContactName = "Bathilda";
			contact2.OC_ContactName = "Nagini";
			contact1.OC_OA_OrgAddress = address1.PK;
			contact2.OC_OA_OrgAddress = address2.PK;

			Factory.Save();

			var person1 = contact1.Person;
			var person2 = contact2.Person;

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var orgFilter = (ModuleTextFilter)glbPersonFilter["Related Organization Name"];
			orgFilter.IsActive = true;

			orgFilter.Property = "Arthur's";
			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(person1.PK, collection.Single().PK);

			orgFilter.Property = "Malfoy's";
			var noResultsCollection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, noResultsCollection.Any());
		}

		public void TestRelatedOrgNameFilter_WithOrgAddress()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_FullName = "Arthur's";
			organization2.OH_FullName = "Bobby's";

			var address1 = organization1.Addresses.AddNew();
			var address2 = organization2.Addresses.AddNew();
			address1.OA_CompanyNameOverride = "Collin's";
			address2.OA_CompanyNameOverride = "Dobby's";
			address1.OA_Address1 = "321 true st";
			address2.OA_Address1 = "123 fake st";

			var contact1 = organization1.Contacts.AddNew();
			var contact2 = organization2.Contacts.AddNew();
			contact1.OC_ContactName = "Bathilda";
			contact2.OC_ContactName = "Nagini";
			contact1.OC_OA_OrgAddress = address1.PK;
			contact2.OC_OA_OrgAddress = address2.PK;

			Factory.Save();

			var person1 = contact1.Person;
			var person2 = contact2.Person;

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var orgFilter = (ModuleTextFilter)glbPersonFilter["Related Organization Name"];
			orgFilter.IsActive = true;

			orgFilter.Property = "Collin's";
			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(person1.PK, collection.Single().PK);

			orgFilter.Property = "Malfoy's";
			var noResultsCollection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertEquals(false, noResultsCollection.Any());
		}

		public void TestRelatedOrgNameFilter_DuplicatedOrgHeader()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_FullName = "Arthur's";
			organization2.OH_FullName = "Arthur's";

			var address1 = organization1.Addresses.AddNew();
			var address2 = organization2.Addresses.AddNew();
			address1.OA_CompanyNameOverride = "Collin's";
			address2.OA_CompanyNameOverride = "Dobby's";
			address1.OA_Address1 = "321 true st";
			address2.OA_Address1 = "123 fake st";

			var contact1 = organization1.Contacts.AddNew();
			var contact2 = organization2.Contacts.AddNew();
			contact1.OC_ContactName = "Bathilda";
			contact2.OC_ContactName = "Nagini";
			contact1.OC_OA_OrgAddress = address1.PK;
			contact2.OC_OA_OrgAddress = address2.PK;

			Factory.Save();

			var person1 = contact1.Person;
			var person2 = contact2.Person;

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var orgFilter = (ModuleTextFilter)glbPersonFilter["Related Organization Name"];
			orgFilter.IsActive = true;
			orgFilter.Property = "Arthur's";

			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { person1.PK, person2.PK }, collection.Select(p => p.PK));
		}

		public void TestRelatedOrgNameFilter_DuplicatedOrgAddress()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_FullName = "Arthur's";
			organization2.OH_FullName = "Bobby's";

			var address1 = organization1.Addresses.AddNew();
			var address2 = organization2.Addresses.AddNew();
			address1.OA_CompanyNameOverride = "Collin's";
			address2.OA_CompanyNameOverride = "Collin's";
			address1.OA_Address1 = "321 true st";
			address2.OA_Address1 = "123 fake st";

			var contact1 = organization1.Contacts.AddNew();
			var contact2 = organization2.Contacts.AddNew();
			contact1.OC_ContactName = "Bathilda";
			contact2.OC_ContactName = "Nagini";
			contact1.OC_OA_OrgAddress = address1.PK;
			contact2.OC_OA_OrgAddress = address2.PK;

			Factory.Save();

			var person1 = contact1.Person;
			var person2 = contact2.Person;

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var orgFilter = (ModuleTextFilter)glbPersonFilter["Related Organization Name"];
			orgFilter.IsActive = true;
			orgFilter.Property = "Collin's";

			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { person1.PK, person2.PK }, collection.Select(p => p.PK));
		}

		public void TestRelatedOrgNameFilter_DuplicatedNameInOrgAddressAndOrgHeader()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_FullName = "Arthur's";
			organization2.OH_FullName = "Bobby's";

			var address1 = organization1.Addresses.AddNew();
			var address2 = organization2.Addresses.AddNew();
			address1.OA_CompanyNameOverride = "Collin's";
			address2.OA_CompanyNameOverride = "Arthur's";
			address1.OA_Address1 = "321 true st";
			address2.OA_Address1 = "123 fake st";

			var contact1 = organization1.Contacts.AddNew();
			var contact2 = organization2.Contacts.AddNew();
			contact1.OC_ContactName = "Bathilda";
			contact2.OC_ContactName = "Nagini";
			contact1.OC_OA_OrgAddress = address1.PK;
			contact2.OC_OA_OrgAddress = address2.PK;

			Factory.Save();

			var person1 = contact1.Person;
			var person2 = contact2.Person;

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var orgFilter = (ModuleTextFilter)glbPersonFilter["Related Organization Name"];
			orgFilter.IsActive = true;
			orgFilter.Property = "Arthur's";

			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { person1.PK, person2.PK }, collection.Select(p => p.PK));
		}

		#endregion

		#region TestRelatedItemFilters

		public void TestIsWeb()
		{
			Globals.IsWeb = true;
			try
			{
				GlbPersonFilterBusinessObject glbPersonFilter = new GlbPersonFilterBusinessObject();
				AssertNull(glbPersonFilter["Related Context"]);
				AssertNull(glbPersonFilter["Organization Contact"]);
				AssertNull(glbPersonFilter["Job Applicant"]);
				AssertNull(glbPersonFilter["Staff"]);
				AssertNull(glbPersonFilter["Accreditation"]);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestCountryFilter()
		{
			GlbPerson person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_RN_NKCountry = "KI";

			GlbPerson person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_RN_NKCountry = "DJ";

			Factory.Save();

			GlbPersonFilterBusinessObject glbPersonFilter = new GlbPersonFilterBusinessObject();
			((ModuleNkFilter)glbPersonFilter["Country"]).Property = "KI";
			((ModuleNkFilter)glbPersonFilter["Country"]).IsActive = true;

			GlbPersonCollection collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));
		}

		public void TestRelatedContextFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Peter Pettigrew";

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = "Peter Pettigrew";
			jobApplicant.HA_PER = person1.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			staff.GS_PER = person1.PK;

			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Newt Scamander";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Newt Scamander";
			contact.OC_PER = person2.PK;

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var relatedContextFilter = (ModuleFlagsFilter)glbPersonFilter["Related Context"];
			relatedContextFilter.Property0 = true;
			relatedContextFilter.Property1 = false;
			relatedContextFilter.Property2 = false;
			relatedContextFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection1.Contains(person1));
			Assert("Collection should contain Person2", collection1.Contains(person2));

			relatedContextFilter.Property0 = true;
			relatedContextFilter.Property1 = true;
			relatedContextFilter.Property2 = false;

			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should not contain Person2", !collection2.Contains(person2));

			relatedContextFilter.Property0 = false;
			relatedContextFilter.Property1 = true;
			relatedContextFilter.Property2 = false;

			var collection3 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection3.Contains(person1));
			Assert("Collection should not contain Person2", !collection3.Contains(person2));

			relatedContextFilter.Property0 = false;
			relatedContextFilter.Property1 = true;
			relatedContextFilter.Property2 = true;

			var collection4 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection4.Contains(person1));
			Assert("Collection should not contain Person2", !collection4.Contains(person2));
		}

		public void TestOrgContactFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Newt Scamander";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Twen Rednamacs";

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Newt Scamander";
			contact1.OC_PER = person1.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Twen Rednamacs";
			contact2.OC_PER = person2.PK;

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var contactFilter = (ModuleGuidForeignCollectionFilter)glbPersonFilter["Organization Contact"];
			var contactNameFilter = contactFilter.SelectedFilters.AddTextFilterStrip("Name", person1.PER_FullName);
			contactFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			contactFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));

			contactNameFilter.Property = person2.PER_FullName;

			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should contain Person2", collection2.Contains(person2));
		}

		public void TestHRJobApplicantFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Newt Scamander";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Twen Rednamacs";

			var applicant1 = Factory.New<IHRJobApplicant>();
			applicant1.HA_FullName = "Newt Scamander";
			applicant1.HA_PER = person1.PK;
			var applicant2 = Factory.New<IHRJobApplicant>();
			applicant2.HA_FullName = "Twen Rednamacs";
			applicant2.UpdateEmailAddress("Twen Rednamacs");
			applicant2.HA_PER = person2.PK;

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var applicantFilter = (ModuleGuidForeignCollectionFilter)glbPersonFilter["Job Applicant"];
			var applicantNameFilter = applicantFilter.SelectedFilters.AddTextFilterStrip("Full Name", person1.PER_FullName);
			applicantFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			applicantFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));

			applicantNameFilter.Property = person2.PER_FullName;

			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should contain Person2", collection2.Contains(person2));
		}

		public void TestGlbStaffFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Newt Scamander";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Twen Rednamacs";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "Newt Scamander";
			staff1.GS_PER = person1.PK;
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "Twen Rednamacs";
			staff2.GS_PER = person2.PK;

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var staffFilter = (ModuleGuidForeignCollectionFilter)glbPersonFilter["Staff"];
			var staffNameFilter = staffFilter.SelectedFilters.AddTextFilterStrip("Full Name", person1.PER_FullName);
			staffFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			staffFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));

			staffNameFilter.Property = person2.PER_FullName;

			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should contain Person2", collection2.Contains(person2));
		}

		public void TestAccreditationAttemptFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Newt Scamander";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Twen Rednamacs";

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";

			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_PER = person2.PK;

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var accredFilter = (ModuleGuidForeignCollectionFilter)glbPersonFilter["Accreditation"];
			accredFilter.IsActive = true;
			var accredCodeFilter = accredFilter.SelectedFilters.AddNkFilterStrip("Accreditation Code", accreditation1.HAC_Code);
			accredCodeFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));

			accredCodeFilter.Property = accreditation2.HAC_Code;
			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should contain Person2", collection2.Contains(person2));
		}

		public void TestAccreditationCompletedFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Lilly Potter";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "James Potter";

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";

			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CompletionDate = new ZDate(2018, 1, 1);
			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_PER = person2.PK;
			attempt2.HAA_CompletionDate = new ZDate(2018, 5, 5);

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var accredFilter = (ModuleGuidForeignCollectionFilter)glbPersonFilter["Accreditation"];
			accredFilter.IsActive = true;
			var accreditationCompletedFilter = accredFilter.SelectedFilters.AddDateFilterStrip("Completion Date");
			accreditationCompletedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			accreditationCompletedFilter.Property1 = new ZDate(2018, 1, 1);
			accreditationCompletedFilter.Property2 = new ZDate(2018, 1, 1);
			accreditationCompletedFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));

			accreditationCompletedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			accreditationCompletedFilter.Property1 = new ZDate(2018, 5, 5);
			accreditationCompletedFilter.Property2 = new ZDate(2018, 5, 5);
			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should contain Person2", collection2.Contains(person2));
		}

		public void TestAccreditationExpiryFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Lilly Potter";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "James Potter";

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";

			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_ExpiryDate = new ZDate(2018, 1, 1);
			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_PER = person2.PK;
			attempt2.HAA_ExpiryDate = new ZDate(2018, 5, 5);

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var accredFilter = (ModuleGuidForeignCollectionFilter)glbPersonFilter["Accreditation"];
			accredFilter.IsActive = true;
			var accreditationExpiryFilter = accredFilter.SelectedFilters.AddDateFilterStrip("Expiry Date");
			accreditationExpiryFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			accreditationExpiryFilter.Property1 = new ZDate(2018, 1, 1);
			accreditationExpiryFilter.Property2 = new ZDate(2018, 1, 1);
			accreditationExpiryFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));

			accreditationExpiryFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			accreditationExpiryFilter.Property1 = new ZDate(2018, 5, 5);
			accreditationExpiryFilter.Property2 = new ZDate(2018, 5, 5);
			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should contain Person2", collection2.Contains(person2));
		}

		public void TestAccreditationCompletionDueDateFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Lilly Potter";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "James Potter";

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";

			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CompletionDueDate = new ZDate(2018, 1, 1);

			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_PER = person2.PK;
			attempt2.HAA_CompletionDueDate = new ZDate(2018, 5, 5);

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var accredFilter = (ModuleGuidForeignCollectionFilter)glbPersonFilter["Accreditation"];
			accredFilter.IsActive = true;
			var accreditationDueFilter = accredFilter.SelectedFilters.AddDateFilterStrip("Completion Due Date");
			accreditationDueFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			accreditationDueFilter.Property1 = new ZDate(2018, 1, 1);
			accreditationDueFilter.Property2 = new ZDate(2018, 1, 1);
			accreditationDueFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));

			accreditationDueFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			accreditationDueFilter.Property1 = new ZDate(2018, 5, 5);
			accreditationDueFilter.Property2 = new ZDate(2018, 5, 5);

			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should contain Person2", collection2.Contains(person2));
		}

		public void TestAccreditationCommenceDateFilter()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Lilly Potter";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "James Potter";

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";

			var attempt1 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt1.HAA_HAC = accreditation1.PK;
			attempt1.HAA_PER = person1.PK;
			attempt1.HAA_CommencementDate = new ZDate(2018, 1, 1);

			var attempt2 = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			attempt2.HAA_HAC = accreditation2.PK;
			attempt2.HAA_PER = person2.PK;
			attempt2.HAA_CommencementDate = new ZDate(2018, 5, 5);

			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var accredFilter = (ModuleGuidForeignCollectionFilter)glbPersonFilter["Accreditation"];
			accredFilter.IsActive = true;
			var accreditationCommencedFilter = accredFilter.SelectedFilters.AddDateFilterStrip("Commencement Date");
			accreditationCommencedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			accreditationCommencedFilter.Property1 = new ZDate(2018, 1, 1);
			accreditationCommencedFilter.Property2 = new ZDate(2018, 1, 1);
			accreditationCommencedFilter.IsActive = true;

			var collection1 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should contain Person1", collection1.Contains(person1));
			Assert("Collection should not contain Person2", !collection1.Contains(person2));

			accreditationCommencedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			accreditationCommencedFilter.Property1 = new ZDate(2018, 5, 5);
			accreditationCommencedFilter.Property2 = new ZDate(2018, 5, 5);

			var collection2 = new GlbPersonCollection(Factory, glbPersonFilter.Filter);
			Assert("Collection should not contain Person1", !collection2.Contains(person1));
			Assert("Collection should contain Person2", collection2.Contains(person2));
		}

		public void TestRelatedOrganizationFilter()
		{
			var personA = Factory.NewWithValidTestData<GlbPerson>();
			var personB = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var header = Factory.NewWithValidTestData<OrgHeader>();

			contact.OC_PER = personA.PK;
			contact.OC_OH = header.PK;
			Factory.Save();

			var glbPersonFilter = new GlbPersonFilterBusinessObject();
			var relatedOrganization = ((ModuleGuidFilter)glbPersonFilter["Related Organization"]);
			relatedOrganization.Property = header.PK;
			relatedOrganization.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			relatedOrganization.IsActive = true;

			var collection = new GlbPersonCollection(Factory, glbPersonFilter.Filter);

			CombineAssertions("Collection should not contain personA", () =>
			{
				AssertEquals(true, collection.Contains(personA));
				AssertEquals(false, collection.Contains(personB));
				AssertEquals(1, collection.Count);
			});
		}

		#endregion
	}
}
