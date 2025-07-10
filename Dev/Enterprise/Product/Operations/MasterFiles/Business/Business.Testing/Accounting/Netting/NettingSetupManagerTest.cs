using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.Accounting.Netting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Netting
{
	[TestedType(typeof(NettingSetupManager))]
	sealed class NettingSetupManagerTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2017, 11, 4)]
		public void TestNettingSetup()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			var setupManager = new NettingSetupManager(Factory, companies.Select(x => x.PK).ToArray());

			var ediCompanyCode = "EDI";
			var ediCompany = companies.FirstOrDefault(x => x.GC_Code == ediCompanyCode);

			AssertNotNull("Precondition", ediCompany);

			setupManager.NettingSystemCompanyCode = ediCompanyCode;
			setupManager.NettingSystemCode = "NTI";
			setupManager.NettingSystemDescription = "Netting for Transport International";
			setupManager.NettingCycleStartDate = ZDateTime.Today;

			setupManager.SetupNetting();
			NettingSystem[] nettingSystems = Factory.Load<NettingSystem>(new ZQuery());
			AssertEquals("1 netting system should be created", 1, nettingSystems.Length);

			AssertSetupForNettingSystem(nettingSystems.First(), companies, setupManager, ediCompany);

			ReleaseFactory();

			setupManager.SetupNetting(); //Try to create a Netting System with the same code

			nettingSystems = Factory.Load<NettingSystem>(new ZQuery());
			AssertEquals("We do not create a second Netting System with the same code", 1, nettingSystems.Length);

			AssertSetupForNettingSystem(nettingSystems.First(), companies, setupManager, ediCompany);

			ReleaseFactory();

			setupManager.NettingSystemCode = "OIR";

			setupManager.SetupNetting(); //Try to create a Netting System with a different code but netting company remains the same

			nettingSystems = Factory.Load<NettingSystem>(new ZQuery());
			AssertEquals("We do not create a second Netting System where a Netting system is already configured for a Company", 1, nettingSystems.Length);

			AssertSetupForNettingSystem(nettingSystems.First(), companies, setupManager, ediCompany);

			ReleaseFactory();

			var secondNettingCompany = companies.FirstOrDefault(x => x.GC_Code != ediCompanyCode);
			setupManager = new NettingSetupManager(Factory, companies.Select(x => x.PK).ToArray());

			setupManager.NettingSystemCompanyCode = secondNettingCompany.GC_Code;
			setupManager.NettingSystemCode = "NEW";
			setupManager.NettingSystemDescription = "NEW Netting for Transport International";
			setupManager.NettingCycleStartDate = ZDateTime.Today;

			setupManager.SetupNetting(); //Try to create a Netting System with a new code and new company

			nettingSystems = Factory.Load<NettingSystem>(new ZQuery());
			AssertEquals("Another Netting company is created", 2, nettingSystems.Length);
			var secondNettingSystem = nettingSystems.FirstOrDefault(x => x.NS_Code == "NEW");

			AssertNotNull(secondNettingSystem);

			AssertSetupForNettingSystem(secondNettingSystem, companies, setupManager, secondNettingCompany);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NettingSetupManager(new BusinessObjectFactory(), new ZGuid[] { ZGuid.Empty });
		}

		void AssertSetupForNettingSystem(NettingSystem nettingSystem, GlbCompany[] companies, NettingSetupManager setupManager, GlbCompany nettingCompany)
		{
			var nettingSystemPeriod = Factory.Load<NettingSystemPeriod>(new ZQuery(NettingSystemPeriodSchema.NSP_NS_NettingSystem, nettingSystem.PK.ToGuid()));
			AssertEquals("12 Netting System Period is created for the netting system", 12, nettingSystemPeriod.Length);

			var rego = ObjectFactory.Get<IProductRegistration>();
			var registrationKey = rego.Key;

			var registry = ObjectFactory.Get<IAccounting>().Registry;

			AssertEquals(true, registry.IsNettingSystem.GetValueWithoutFallback(nettingCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			var nettingControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, NettingSetupManager.NettingControlAccountNumber));
			AssertNotNull(nettingControlAccount);

			AssertEquals(nettingControlAccount.PK.ToGuid(), registry.NettingControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			foreach (GlbCompany company in companies)
			{
				var eHubID = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", registrationKey.EnterpriseCode, company.GC_Code, registrationKey.ServerCode);

				var orgCusCode = Factory.Load<OrgCusCode>(new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, eHubID));
				AssertNotNull("1 OrgCusCode record should be created per participating company, including the Netting centre. Netting centre is treated as a participant in this case.", orgCusCode);

				var ediCommunicationsMode = Factory.Load<EDICommunicationsMode>(new ZQuery(EDICommunicationsModeSchema.EK_Destination, eHubID));
				if (company.GC_Code == nettingCompany.GC_Code)
				{
					AssertEquals("1 EDICommunicationsMode record should be created per participating company for the netting company, excluding the Netting centre.", companies.Length - 1, ediCommunicationsMode.Length);
				}
				else
				{
					AssertEquals("1 EDICommunicationsMode record should be created per participating company for each of the participating companies", 1, ediCommunicationsMode.Length);
					AssertEquals("This EDICommunicationsMode should be created for the Netting company", nettingCompany.GC_OH_OrgProxy, ediCommunicationsMode.First().EK_ParentID);
				}

				AssertEquals(setupManager.NettingCycleStartDate.ToDateTime(), registry.NettingParticipationStartDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals(nettingCompany.GC_OH_OrgProxy.ToGuid(), registry.NettingSystemOrganisation.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}

			AssertEquals(true, RawDataRegistry.Instance.EHubTesting.Value);
		}
	}
}
