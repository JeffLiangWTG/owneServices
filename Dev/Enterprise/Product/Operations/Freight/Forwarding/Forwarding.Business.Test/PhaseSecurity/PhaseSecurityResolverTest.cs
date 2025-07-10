using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest("AU")]
	sealed class PhaseSecurityResolverTest : TestCaseWithFactory
	{
		public void TestCtorDoesNotAcceptNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PhaseSecurityResolver(null));
		}

		public void TestIsPhaseSecurityApplicable_IsUserInteractive()
		{
			bool isUserInteractive = Globals.IsUserInteractive;
			using (new DisposableAction(() => Globals.IsUserInteractive = isUserInteractive))
			{
				Globals.IsUserInteractive = false;

				var resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
				resolver.GetPhaseSecurityDelegate = () => { throw new Exception("Should not access phase security"); };

				AssertEquals(true, resolver.IsEditingAllowed);

				Globals.IsUserInteractive = true;
				bool getPhaseSecurityDelegateCalled = false;

				resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
				resolver.GetPhaseSecurityDelegate = () =>
				{
					getPhaseSecurityDelegateCalled = true;
					return CreatePhaseSecurity();
				};

				AssertEquals(true, resolver.IsEditingAllowed);
				AssertEquals(true, getPhaseSecurityDelegateCalled);
			}
		}

		public void TestIsPhaseSecurityApplicable_ReadOnlyBusinessObjectFactory()
		{
			var readonlyFactory = new ReadOnlyBusinessObjectFactory();

			var resolver = new MockResolver(readonlyFactory.New<MockPhaseSecuritySupportable>());
			resolver.GetPhaseSecurityDelegate = () => { throw new Exception("Should not access phase security"); };

			AssertEquals("PhaseSecurity should not be applicable for ReadOnlyBusinessObjectFactory", true, resolver.IsEditingAllowed);

			bool getPhaseSecurityDelegateCalled = false;

			resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			resolver.GetPhaseSecurityDelegate = () =>
			{
				getPhaseSecurityDelegateCalled = true;
				return CreatePhaseSecurity();
			};

			AssertEquals(true, resolver.IsEditingAllowed);
			AssertEquals(true, getPhaseSecurityDelegateCalled);
		}

		public void TestShouldNotAccessPhaseSecurityWhenPhaseIsALL()
		{
			var resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			resolver.GetPhaseSecurityDelegate = () => { throw new Exception("Phase security getter accessed"); };

			resolver.SetPhaseCodeAndResetIsEditingAllowed(PhaseConstants.Phase.ALL);
			AssertEquals(true, resolver.IsEditingAllowed);

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertExceptionThrown(typeof(Exception), "Phase security getter accessed", () => AssertEquals(true, resolver.IsEditingAllowed));
		}

		public void TestReadOnlyDependantsCache()
		{
			var phase1 = new MockPhase() { Code = "ONE" };
			var rule11 = new MockPhaseRule()
			{
				DepartmentPK = GlbDepartment.CurrentDepartment.PK,
				Location = PhaseConstants.Locations.AnyLocation
			};

			rule11.Dependants.Add(new MockPhaseDependant()
			{
				Name = "Dumb",
				IsReadOnly = true
			});

			rule11.Dependants.Add(new MockPhaseDependant()
			{
				Name = "Dumber",
				IsReadOnly = true
			});

			var rule12 = new MockPhaseRule()
			{
				DepartmentPK = ZGuid.NewZGuid(),
				Location = PhaseConstants.Locations.AnyLocation
			};

			phase1.Rules_Exposed.Add(rule11);
			phase1.Rules_Exposed.Add(rule12);

			var phase2 = new MockPhase() { Code = "TWO" };
			var rule21 = new MockPhaseRule()
			{
				DepartmentPK = GlbDepartment.CurrentDepartment.PK,
				Location = PhaseConstants.Locations.AnyLocation
			};

			rule21.Dependants.Add(new MockPhaseDependant()
			{
				Name = "Banana",
				IsReadOnly = true
			});

			phase2.Rules_Exposed.Add(rule21);

			var phaseSecurity = new MockPhaseSecurity();
			phaseSecurity.Phases_Exposed.Add(phase1);
			phaseSecurity.Phases_Exposed.Add(phase2);
			phaseSecurity.IsEnabled = true;

			Func<string, MockResolver> createNewResolverWithGivenPhase = (phase) =>
				{
					var resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
					resolver.GetPhaseSecurityDelegate = () => { return phaseSecurity; };
					resolver.SetPhaseCodeAndResetIsEditingAllowed(phase);

					bool resolverInitialised = resolver.IsEditingAllowed;

					return resolver;
				};

			var resolver1 = createNewResolverWithGivenPhase("ONE");

			Func<Dictionary<string, IEnumerable<IPhaseDependant>>> getReadonlyDependantsCache = () =>
			{
				string expectedCacheNameInFactory = "IPhaseSecurityCache" + nameof(MockPhaseSecuritySupportable);
				return Factory.GetCachedValue<Dictionary<string, IEnumerable<IPhaseDependant>>>(
						expectedCacheNameInFactory,
						() => { throw new Exception("Cache should be initialised by now"); });
			};

			var reaonlyDependantsCache = getReadonlyDependantsCache();
			AssertEquals("Cache contains one record", 1, reaonlyDependantsCache.Count);

			string expectedPhaseRuleCacheKey = string.Format("{0}-{1}-{2}", "ONE", GlbDepartment.CurrentDepartment.PK.ToString(), PhaseConstants.Locations.AnyLocation);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Dumb", "Dumber" }, reaonlyDependantsCache[expectedPhaseRuleCacheKey].Select(x => x.Name));

			var resolver2 = createNewResolverWithGivenPhase("ONE");
			reaonlyDependantsCache = getReadonlyDependantsCache();
			AssertEquals("No new seeds in cache, still one record", 1, reaonlyDependantsCache.Count);

			var resolver3 = createNewResolverWithGivenPhase("TWO");
			reaonlyDependantsCache = getReadonlyDependantsCache();
			AssertEquals("Record for the new rule was seeded", 2, reaonlyDependantsCache.Count);

			var resolver4 = createNewResolverWithGivenPhase("TWO");
			reaonlyDependantsCache = getReadonlyDependantsCache();
			AssertEquals("No new seeds in cache", 2, reaonlyDependantsCache.Count);
		}

		public void TestIsEditingAllowed()
		{
			MockPhaseSecurity security = new MockPhaseSecurity();

			MockResolver resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			resolver.GetPhaseSecurityDelegate = () => { return security; };

			AssertEquals(false, security.IsEnabled);

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Security is not enabled, so every phase is allowed", true, resolver.IsEditingAllowed);

			resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
			AssertEquals("Security is not enabled, so every phase is allowed", true, resolver.IsEditingAllowed);

			security.IsEnabled = true;
			AssertEquals(true, security.IsEnabled);

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Security is enabled, but phase not in the security list => is allowed", true, resolver.IsEditingAllowed);

			resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
			AssertEquals("Security is enabled, but phase not in the security list => is allowed", true, resolver.IsEditingAllowed);

			resolver.SetPhaseCodeAndResetIsEditingAllowed(PhaseConstants.Phase.ALL);
			AssertEquals("ALL phase is always allowed", true, resolver.IsEditingAllowed);

			MockPhase phase1 = new MockPhase();
			phase1.Code = "AAA";

			security.Phases_Exposed.Add(phase1);
			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Phase in the security list w/o permission for the department/location pair => NOT allowed", false, resolver.IsEditingAllowed);

			resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
			AssertEquals("Phase not in the security list => is allowed", true, resolver.IsEditingAllowed);

			MockPhaseRule rule11 = new MockPhaseRule();
			rule11.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			phase1.Rules_Exposed.Add(rule11);
			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Phase has rule for the current department, but location is unresolved => NOT allowed", false, resolver.IsEditingAllowed);

			rule11.Location = "location11";
			resolver.LocationToUNLOCO.AddPair("location11", "USLAX");

			AssertEquals("Precondition: current branch's port", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Phase has rule for the current department, location is resolved, but unloco not matched => NOT allowed", false, resolver.IsEditingAllowed);

			GlbBranch uslaxBranch = Factory.New<GlbBranch>();
			uslaxBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			uslaxBranch.GB_Code = "LAX";
			uslaxBranch.GB_RL_NKHomePort = "USLAX";
			Factory.Save();

			using (uslaxBranch.SetAsTemporaryContext())
			{
				AssertEquals("Precondition: current branch's port", "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Current department is matched, location is resolved and matched => is allowed", true, resolver.IsEditingAllowed);

				rule11.DepartmentPK = ZGuid.NewZGuid();
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Current department not matched => NOT allowed", false, resolver.IsEditingAllowed);

				rule11.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Current department is matched, location is resolved and matched => is allowed", true, resolver.IsEditingAllowed);

				resolver.LocationToUNLOCO.Clear();
				resolver.LocationToUNLOCO.AddPair("location11", "US");
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Location resolved as a country, matched => is allowed", true, resolver.IsEditingAllowed);

				resolver.LocationToUNLOCO.Clear();
				resolver.LocationToUNLOCO.AddPair("location11", "NZ");
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Location resolved as a country, but unmatched => NOT allowed", false, resolver.IsEditingAllowed);
			}

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Location resolved, but unmatched => NOT allowed", false, resolver.IsEditingAllowed);

			GlbBranch.CurrentBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "NZAKL";
			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Location resolved and in additional branch's ports => is allowed", true, resolver.IsEditingAllowed);

			resolver.LocationToUNLOCO.Clear();
			resolver.LocationToUNLOCO.AddPair("location11", "NZABC");
			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Location resolved, but unmatched => NOT allowed", false, resolver.IsEditingAllowed);

			rule11.Location = PhaseConstants.Locations.AnyLocation;
			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Location is 'Any Location' => is allowed", true, resolver.IsEditingAllowed);
		}

		public void TestIsEditingAllowed_AnotherTestWithPopulatedRules()
		{
			GlbDepartment anotherDepartment = Factory.New<GlbDepartment>();
			anotherDepartment.GE_Code = "AND";

			Factory.Save();

			MockPhase phase1 = new MockPhase();
			phase1.Code = "AAA";

			MockPhaseRule rule11 = new MockPhaseRule();
			rule11.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule11.Location = "load port";

			MockPhaseRule rule12 = new MockPhaseRule();
			rule12.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule12.Location = "discharge port";

			MockPhaseRule rule13 = new MockPhaseRule();
			rule13.DepartmentPK = anotherDepartment.PK;
			rule13.Location = "transit port";

			phase1.Rules_Exposed.Add(rule11);
			phase1.Rules_Exposed.Add(rule12);
			phase1.Rules_Exposed.Add(rule13);

			MockPhase phase2 = new MockPhase();
			phase2.Code = "BBB";

			MockPhaseRule rule21 = new MockPhaseRule();
			rule21.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule21.Location = PhaseConstants.Locations.AnyLocation;

			MockPhaseRule rule22 = new MockPhaseRule();
			rule22.DepartmentPK = anotherDepartment.PK;
			rule22.Location = "discharge port";

			phase2.Rules_Exposed.Add(rule21);
			phase2.Rules_Exposed.Add(rule22);

			MockPhaseSecurity security = new MockPhaseSecurity();
			security.IsEnabled = true;
			security.Phases_Exposed.Add(phase1);
			security.Phases_Exposed.Add(phase2);

			MockResolver resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			resolver.GetPhaseSecurityDelegate = () => { return security; };
			resolver.LocationToUNLOCO.AddPair("load port", "AUSYD");
			resolver.LocationToUNLOCO.AddPair("discharge port", "USLAX");
			resolver.LocationToUNLOCO.AddPair("transit port", "AUBNE");

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Matched for department, but unmatched for locations", false, resolver.IsEditingAllowed);

			resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
			AssertEquals("Matched for department & any location rule", true, resolver.IsEditingAllowed);

			using (Env.SetTemporaryUserContext("chuck norris", GlbBranch.CurrentBranch.PK.ToGuid(), anotherDepartment.PK.ToGuid()))
			{
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Matched for department, matched for AUBNE unloco", true, resolver.IsEditingAllowed);

				resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
				AssertEquals("Matched for department, but unmatched for locations", false, resolver.IsEditingAllowed);
			}

			GlbBranch uslaxBranch = Factory.New<GlbBranch>();
			uslaxBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			uslaxBranch.GB_Code = "LAX";
			uslaxBranch.GB_RL_NKHomePort = "USLAX";

			Factory.Save();

			using (Env.SetTemporaryUserContext("bilbo baggins", uslaxBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Matched for department, matched for USLAX unloco", true, resolver.IsEditingAllowed);

				resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
				AssertEquals("Matched for department & any location rule", true, resolver.IsEditingAllowed);
			}

			using (Env.SetTemporaryUserContext("eric cartman", uslaxBranch.PK.ToGuid(), anotherDepartment.PK.ToGuid()))
			{
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Matched for department, but unmatched for locations", false, resolver.IsEditingAllowed);

				resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
				AssertEquals("Matched for department, matched for USLAX unloco", true, resolver.IsEditingAllowed);

				uslaxBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "USNYC";
				uslaxBranch.Factory.Save();
				resolver.LocationToUNLOCO.AddOverwriteIfExists(new CodeDescriptionPair("transit port", "USNYC"));

				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Matched for department, matched for USNYC unloco", true, resolver.IsEditingAllowed);

				resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
				AssertEquals("Matched for department, matched for USLAX unloco", true, resolver.IsEditingAllowed);
			}

			GlbDepartment randomDepartment = Factory.New<GlbDepartment>();
			anotherDepartment.GE_Code = "RND";
			Factory.Save();

			using (Env.SetTemporaryUserContext("", GlbBranch.CurrentBranch.PK.ToGuid(), randomDepartment.PK.ToGuid()))
			{
				resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
				AssertEquals("Department unmatched", false, resolver.IsEditingAllowed);

				resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
				AssertEquals("Department unmatched", false, resolver.IsEditingAllowed);
			}
		}

		public void TestTakeSpecificLocationRuleBeforeAnyLocationRule()
		{
			var phase = new MockPhase();
			phase.Code = "AAA";

			var rule1 = new MockPhaseRule();
			rule1.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule1.Location = PhaseConstants.Locations.AnyLocation;

			var dependant1 = new MockPhaseDependant();
			dependant1.Name = "Cookies";
			dependant1.IsReadOnly = true;
			rule1.Dependants.Add(dependant1);

			var rule2 = new MockPhaseRule();
			rule2.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule2.Location = "load port";

			var dependant2 = new MockPhaseDependant();
			dependant2.Name = "Cookies";
			dependant2.IsReadOnly = false;
			rule2.Dependants.Add(dependant2);

			phase.Rules_Exposed.Add(rule1);
			phase.Rules_Exposed.Add(rule2);

			var security = new MockPhaseSecurity();
			security.IsEnabled = true;
			security.Phases_Exposed.Add(phase);

			MockResolver resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			resolver.GetPhaseSecurityDelegate = () => { return security; };
			resolver.LocationToUNLOCO.AddPair("load port", "AUBNE");

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");

			AssertEquals("rule with specific location should take precedence over rule with AnyLocation", false, resolver.IsPropertyReadOnly("Cookies"));
		}

		public void TestIsPropertyReadOnlyOrMandatory()
		{
			var resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			resolver.GetPhaseSecurityDelegate = () => CreatePhaseSecurity();

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Precondition: editing allowed", true, resolver.IsEditingAllowed);

			AssertEquals("Readonly: defined in phase security", true, resolver.IsPropertyReadOnly("Cookies"));
			AssertEquals("Readonly: defined in phase security", true, resolver.IsPropertyReadOnly("IceCream"));
			AssertEquals("Mandatory: defined in phase security", true, resolver.IsPropertyMandatory("Chocolate"));

			AssertEquals("Readonly property: containing object marked as readonly", true, resolver.IsPropertyReadOnly("IceCream.Chocolate"));
			AssertEquals("Readonly property: containing object marked as readonly", true, resolver.IsPropertyReadOnly("IceCream.Vanilla"));
			AssertEquals("Not Mandatory: containing object mandatory doesn't imply object is mandatory", false, resolver.IsPropertyMandatory("Chocolate.Milk"));

			AssertEquals("Not readonly", false, resolver.IsPropertyReadOnly("Chocolate"));
			AssertEquals("Not readonly", false, resolver.IsPropertyReadOnly("Just.A.Random.Property"));

			AssertEquals("Not readonly: property name not matched", false, resolver.IsPropertyReadOnly("COOKIES"));
			AssertEquals("Not readonly: property name not matched", false, resolver.IsPropertyReadOnly("CooKieS"));
			AssertEquals("Not readonly: property name not matched", false, resolver.IsPropertyReadOnly("Cookies "));
			AssertEquals("Not readonly: property name not matched", false, resolver.IsPropertyReadOnly("Cookies123"));
			AssertEquals("Not readonly: property name not matched", false, resolver.IsPropertyReadOnly("IceCreamChocolate"));
			AssertEquals("Not readonly: property name not matched", false, resolver.IsPropertyReadOnly("IceCream Chocolate"));

			resolver.SetPhaseCodeAndResetIsEditingAllowed("BBB");
			AssertEquals("Precondition", false, resolver.IsEditingAllowed);

			AssertEquals("Readonly: editing not allowed", true, resolver.IsPropertyReadOnly("Cookies"));
			AssertEquals("Readonly: editing not allowed", true, resolver.IsPropertyReadOnly("IceCream"));
			AssertEquals("Readonly: editing not allowed", true, resolver.IsPropertyReadOnly("Chocolate"));
			AssertEquals("Readonly: editing not allowed", true, resolver.IsPropertyReadOnly("Just.A.Random.Property"));

			resolver.SetPhaseCodeAndResetIsEditingAllowed("XXX");
			AssertEquals("Precondition", true, resolver.IsEditingAllowed);

			AssertEquals("Not readonly", false, resolver.IsPropertyReadOnly("Cookies"));
			AssertEquals("Not readonly", false, resolver.IsPropertyReadOnly("IceCream"));
			AssertEquals("Not readonly", false, resolver.IsPropertyReadOnly("Chocolate"));
			AssertEquals("Not readonly", false, resolver.IsPropertyReadOnly("Just.A.Random.Property"));
		}

		public void TestIsChildPropertyReadOnly()
		{
			var security = CreatePhaseSecurity();

			var dependant1 = new MockPhaseDependant();
			dependant1.Name = "Dummy1.Z0_Code";
			dependant1.IsReadOnly = true;

			var dependant2 = new MockPhaseDependant();
			dependant2.Name = "Dummy1.Z0_Description";
			dependant2.IsReadOnly = true;

			var dependant3 = new MockPhaseDependant();
			dependant3.Name = "Z0_VarCharMax";
			dependant3.IsMandatory = true;

			var rule = (MockPhaseRule)security.Phases.FirstOrDefault(phase => phase.Code == "AAA").Rules.First();
			rule.Dependants.Add(dependant1);
			rule.Dependants.Add(dependant2);

			var resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			resolver.GetPhaseSecurityDelegate = () => { return security; };

			resolver.SetPhaseCodeAndResetIsEditingAllowed("AAA");
			AssertEquals("Precondition: editing allowed", true, resolver.IsEditingAllowed);

			DummyBusinessObject dummy1 = Factory.New<MockPhaseSecuritySupportable>();
			DummyBusinessObject dummy2 = Factory.New<MockPhaseSecuritySupportable>();

			AssertEquals("Not readonly: child not registered", false, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_Code"));
			AssertEquals("Not readonly: child not registered", false, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_Description"));
			AssertEquals("Not readonly: child not registered", false, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_VarCharMax"));

			AssertEquals("Not readonly: child not registered", false, resolver.IsChildPropertyReadOnly(dummy2.PK, "Z0_Code"));
			AssertEquals("Not readonly: child not registered", false, resolver.IsChildPropertyReadOnly(dummy2.PK, "Z0_VarCharMax"));

			resolver.RegisterEditableChild(dummy1, "Dummy1");

			AssertEquals("Readonly: child registered & property name matched", true, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_Code"));
			AssertEquals("Readonly: child registered & property name matched", true, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_Description"));
			AssertEquals("Not readonly: child registered, property matched but not read only", false, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_VarCharMax"));
			AssertEquals("Not readonly: child registered, but property name not matched", false, resolver.IsChildPropertyReadOnly(dummy1.PK, "TheProperty"));

			AssertEquals("Not readonly: child not registered", false, resolver.IsChildPropertyReadOnly(dummy2.PK, "Z0_Code"));
			AssertEquals("Not readonly: child not registered", false, resolver.IsChildPropertyReadOnly(dummy2.PK, "Z0_VarCharMax"));

			resolver.RegisteredChildren.Clear();
			resolver.RegisterEditableChild(dummy1, "DummyRegisteredWithAnotherName");

			AssertEquals("Not readonly: child registered, but property name not matched", false, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_Code"));
			AssertEquals("Not readonly: child registered, but property name not matched", false, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_Description"));
			AssertEquals("Not readonly: child registered, but property name not matched", false, resolver.IsChildPropertyReadOnly(dummy1.PK, "Z0_VarCharMax"));
		}

		public void TestRegisterEditableChild()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();

			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory);

			MockResolver resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			AssertEquals("Precondition: no registered children", 0, resolver.RegisteredChildren.Count);

			resolver.RegisterEditableChild(dummy1, "Dummy1");
			AssertEquals("Child registered", 1, resolver.RegisteredChildren.Count);
			AssertEquals("Dummy1", resolver.RegisteredChildren[dummy1.PK]);

			resolver.RegisterEditableChild(dummy2, "Dummy2");
			AssertEquals("Child registered", 2, resolver.RegisteredChildren.Count);
			AssertEquals("Dummy2", resolver.RegisteredChildren[dummy2.PK]);

			resolver.RegisterEditableChild(dummy1, "Dummy1");
			AssertEquals("Already registered child not registered again", 2, resolver.RegisteredChildren.Count);

			resolver.RegisterEditableChild(dummyCollection, "Dummies");
			AssertEquals("Only BusinessObjects are registered", 2, resolver.RegisteredChildren.Count);
		}

		public void TestGetCountryFromUNLOCO()
		{
			MockResolver resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			AssertEquals("MD", resolver.GetCountryFromUNLOCO("MDKIV"));
			AssertEquals("AU", resolver.GetCountryFromUNLOCO("AUSYD"));

			AssertEquals("", resolver.GetCountryFromUNLOCO(" AUSYD"));
			AssertEquals("", resolver.GetCountryFromUNLOCO("AUSYDNEY"));
			AssertEquals("", resolver.GetCountryFromUNLOCO(""));
		}

		public void TestIsTransit()
		{
			MockResolver resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			AssertEquals(true, resolver.IsTransit("MD", "AU", "US"));
			AssertEquals(true, resolver.IsTransit("NZ", "AU", "US"));

			AssertEquals(false, resolver.IsTransit("AU", "AU", "US"));
			AssertEquals(false, resolver.IsTransit("US", "AU", "US"));
			AssertEquals(false, resolver.IsTransit("", "AU", "US"));
		}

		public void TestGetTransitCountries()
		{
			ITransportParent parent = Factory.New<ForwardingConsol>();

			Transport transport1 = parent.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "NZAKL";

			Transport transport2 = parent.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "SGSIN";

			Transport transport3 = parent.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "SGSIN";
			transport3.JW_RL_NKDiscPort = "DEFRA";

			Transport transport4 = parent.Transports.AddNew();
			transport4.JW_RL_NKLoadPort = "DEFRA";
			transport4.JW_RL_NKDiscPort = "USLAX";

			MockResolver resolver = new MockResolver(Factory.New<MockPhaseSecuritySupportable>());
			IEnumerable<Transport> transports = parent.Transports.Cast<Transport>();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZ", "SG", "DE" }, resolver.GetTransitCountries("AU", "US", transports));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "SG", "DE", "US" }, resolver.GetTransitCountries("AU", "NZ", transports));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "SG", "US" }, resolver.GetTransitCountries("NZ", "DE", transports));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
		}

		IPhaseSecurity CreatePhaseSecurity()
		{
			var phase1 = new MockPhase();
			phase1.Code = "AAA";
			phase1.Description = "AAA Phase Description";

			var rule11 = new MockPhaseRule();
			rule11.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			rule11.Location = PhaseConstants.Locations.AnyLocation;
			phase1.Rules_Exposed.Add(rule11);

			var dependant11 = new MockPhaseDependant();
			dependant11.Name = "Cookies";
			dependant11.IsReadOnly = true;
			rule11.Dependants.Add(dependant11);

			var dependant12 = new MockPhaseDependant();
			dependant12.Name = "IceCream";
			dependant12.IsReadOnly = true;
			rule11.Dependants.Add(dependant12);

			var dependant13 = new MockPhaseDependant();
			dependant13.Name = "Chocolate";
			dependant13.IsMandatory = true;
			rule11.Dependants.Add(dependant13);

			var phase2 = new MockPhase();
			phase2.Code = "BBB";
			phase2.Description = "BBB Phase Description";

			var rule21 = new MockPhaseRule();
			rule21.DepartmentPK = ZGuid.NewZGuid();
			rule21.Location = "Hobbiton";
			phase2.Rules_Exposed.Add(rule21);

			var security = new MockPhaseSecurity();
			security.Phases_Exposed.Add(phase1);
			security.Phases_Exposed.Add(phase2);
			security.IsEnabled = true;

			return security;
		}

		class MockPhaseSecuritySupportable : DummyBusinessObject, IPhaseSecuritySupportable
		{
			public MockPhaseSecuritySupportable(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			bool IPhaseSecuritySupportable.IsPropertyReadOnlyDueToPhase(ZString propertyName)
			{
				return false;
			}

			bool IPhaseSecuritySupportable.IsReadOnlyDueToPhase
			{
				get { return false; }
			}
		}

		class MockResolver : PhaseSecurityResolver
		{
			public MockResolver(IPhaseSecuritySupportable master)
				: base(master)
			{
			}

			protected override ZString PhaseCode
			{
				get { return phaseCode_Exposed; }
			}

			public void SetPhaseCodeAndResetIsEditingAllowed(ZString phaseCode)
			{
				phaseCode_Exposed = phaseCode;
				isEditingAllowed = null;
			}
			ZString phaseCode_Exposed;

			protected override IPhaseSecurity GetPhaseSecurity()
			{
				return GetPhaseSecurityDelegate();
			}
			public Func<IPhaseSecurity> GetPhaseSecurityDelegate { get; set; }

			protected override IEnumerable<ZString> ResolveUNLOCOs(ZString locationCode)
			{
				ZString result = LocationToUNLOCO.ContainsCode(locationCode) ? LocationToUNLOCO.GetDescriptionFromCode(locationCode) : "";
				return new ZString[] { result };
			}

			public CodeDescriptionPairList LocationToUNLOCO
			{
				get { return locationToUNLOCO ?? (locationToUNLOCO = new CodeDescriptionPairList()); }
			}
			CodeDescriptionPairList locationToUNLOCO;

			public new Dictionary<ZGuid, ZString> RegisteredChildren
			{
				get { return base.RegisteredChildren; }
			}

			#region Implementation

			public new ZString GetCountryFromUNLOCO(ZString unloco)
			{
				return base.GetCountryFromUNLOCO(unloco);
			}

			public new bool IsTransit(ZString country, ZString loadCountry, ZString dischargeCountry)
			{
				return base.IsTransit(country, loadCountry, dischargeCountry);
			}

			public new IEnumerable<ZString> GetTransitCountries(ZString loadCountry, ZString dischargeCountry, IEnumerable<Transport> transports)
			{
				return base.GetTransitCountries(loadCountry, dischargeCountry, transports);
			}

			#endregion
		}

		class MockPhaseSecurity : IPhaseSecurity
		{
			public bool IsEnabled { get; set; }

			public IEnumerable<IPhase> Phases
			{
				get { return Phases_Exposed.Cast<IPhase>(); }
			}

			public List<MockPhase> Phases_Exposed
			{
				get { return phases_Exposed ?? (phases_Exposed = new List<MockPhase>()); }
			}
			List<MockPhase> phases_Exposed;
		}

		class MockPhase : IPhase
		{
			public ZString Code { get; set; }
			public ZString Description { get; set; }

			public IEnumerable<IPhaseRule> Rules
			{
				get { return Rules_Exposed.Cast<IPhaseRule>(); }
			}

			public List<MockPhaseRule> Rules_Exposed
			{
				get { return rules_Exposed ?? (rules_Exposed = new List<MockPhaseRule>()); }
			}
			List<MockPhaseRule> rules_Exposed;
		}

		class MockPhaseRule : IPhaseRule
		{
			public ZGuid DepartmentPK { get; set; }
			public ZString Location { get; set; }

			public List<MockPhaseDependant> Dependants
			{
				get { return dependants ?? (dependants = new List<MockPhaseDependant>()); }
			}
			List<MockPhaseDependant> dependants;

			public IEnumerable<IPhaseDependant> MandatoryDependants
			{
				get { return Dependants.Cast<IPhaseDependant>().Where(x => x.IsMandatory); }
			}

			public IEnumerable<IPhaseDependant> ReadOnlyDependants
			{
				get { return Dependants.Cast<IPhaseDependant>().Where(x => x.IsReadOnly); }
			}

			IEnumerable<IPhaseDependant> IPhaseRule.Dependants
			{
				get { return Dependants.Cast<IPhaseDependant>(); }
			}
		}

		class MockPhaseDependant : IPhaseDependant
		{
			public ZString DependantType { get; set; }
			public ZString Name { get; set; }
			public ZString Description { get; set; }
			public ZBool IsMandatory { get; set; }
			public ZBool IsReadOnly { get; set; }
		}

		#endregion
	}
}
