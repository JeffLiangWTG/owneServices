using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddressDependentCollection))]
	public class JobDocAddressDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestAddNew_Multiple

		public void TestAddNew_Multiple()
		{
			JobDocAddress address1 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress);
			JobDocAddress address2 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress);

			AssertEquals("ParentType", typeof(JobDocAddressParentForTesting), address1.ParentType);
			AssertEquals("ParentID", Parent.PK, address1.E2_ParentID);
			AssertEquals("ParentTableCode", "Z0", address1.E2_ParentTableCode);
			AssertEquals("Sequence", (ZByte)0, address1.E2_AddressSequence);

			AssertEquals("ParentType", typeof(JobDocAddressParentForTesting), address2.ParentType);
			AssertEquals("ParentID", Parent.PK, address2.E2_ParentID);
			AssertEquals("ParentTableCode", "Z0", address2.E2_ParentTableCode);
			AssertEquals("Sequence", (ZByte)1, address2.E2_AddressSequence);

			AssertNotEquals("Shouldn't be the same address", address1.PK, address2.PK);
		}

		#endregion

		#region TestRemove

		public void TestRemove()
		{
			var address1 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress);
			var address2 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress);
			var address3 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress);
			var address4 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress);
			var address5 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress);

			AssertEquals((ZByte)0, address1.E2_AddressSequence);
			AssertEquals((ZByte)1, address2.E2_AddressSequence);
			AssertEquals((ZByte)2, address3.E2_AddressSequence);
			AssertEquals((ZByte)3, address4.E2_AddressSequence);
			AssertEquals((ZByte)4, address5.E2_AddressSequence);

			DocAddresses.Remove(address2);
			AssertEquals((ZByte)0, address1.E2_AddressSequence);
			AssertEquals((ZByte)2, address3.E2_AddressSequence);
			AssertEquals((ZByte)3, address4.E2_AddressSequence);
			AssertEquals((ZByte)4, address5.E2_AddressSequence);

			DocAddresses.Remove(address3);
			DocAddresses.Remove(address4);
			AssertEquals((ZByte)0, address1.E2_AddressSequence);
			AssertEquals((ZByte)4, address5.E2_AddressSequence);

			DocAddresses.Remove(address1);
			AssertEquals((ZByte)0, address5.E2_AddressSequence);
		}

		#endregion

		#region TestAddNew_WithSequenceSpecified

		public void TestAddNew_WithSequenceSpecified()
		{
			JobDocAddress address1 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 1);
			JobDocAddress address2 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 4);

			AssertEquals("ParentType", typeof(JobDocAddressParentForTesting), address1.ParentType);
			AssertEquals("ParentID", Parent.PK, address1.E2_ParentID);
			AssertEquals("ParentTableCode", "Z0", address1.E2_ParentTableCode);
			AssertEquals("Sequence", (ZByte)1, address1.E2_AddressSequence);

			AssertEquals("ParentType", typeof(JobDocAddressParentForTesting), address2.ParentType);
			AssertEquals("ParentID", Parent.PK, address2.E2_ParentID);
			AssertEquals("ParentTableCode", "Z0", address2.E2_ParentTableCode);
			AssertEquals("Sequence", (ZByte)4, address2.E2_AddressSequence);

			AssertNotEquals("Shouldn't be the same address", address1.PK, address2.PK);
		}

		#endregion

		#region TestFindDocAddressesByType

		public void TestFindDocAddressesByType()
		{
			var address1 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 3);
			var address2 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 1);
			var address3 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 2);
			var address4 = DocAddresses.AddNew(DocAddressType.ArrivalCTOAddress, 2);
			var address5 = DocAddresses.AddNew(DocAddressType.ArrivalCYDAddress, 2);

			var docAddresses = DocAddresses.FindDocAddressesByType(DocAddressType.ArrivalCFSAddress);
			AssertEquals("Should have returned 3", 3, docAddresses.Length);
			AssertEquals("Should be in order of sequence", address2.PK, docAddresses[0].PK);
			AssertEquals("Should be in order of sequence", address3.PK, docAddresses[1].PK);
			AssertEquals("Should be in order of sequence", address1.PK, docAddresses[2].PK);

			var docAddressesSequence1 = DocAddresses.FindDocAddressesByType(new[] { DocAddressType.ArrivalCFSAddress, DocAddressType.ArrivalCTOAddress });
			AssertEquals("Should have returned 4", 4, docAddressesSequence1.Length);
			AssertEquals("Should be in order of sequence", address2.PK, docAddressesSequence1[0].PK);
			AssertEquals("Should be in order of sequence", address3.PK, docAddressesSequence1[1].PK);
			AssertEquals("Should be in order of sequence", address1.PK, docAddressesSequence1[2].PK);
			AssertEquals("Should be in order of sequence", address4.PK, docAddressesSequence1[3].PK);

			var docAddressesSequence2 = DocAddresses.FindDocAddressesByType(new[] { DocAddressType.ArrivalCTOAddress, DocAddressType.ArrivalCFSAddress });
			AssertEquals("Should have returned 4", 4, docAddressesSequence2.Length);
			AssertEquals("Should be in order of sequence", address4.PK, docAddressesSequence2[0].PK);
			AssertEquals("Should be in order of sequence", address2.PK, docAddressesSequence2[1].PK);
			AssertEquals("Should be in order of sequence", address3.PK, docAddressesSequence2[2].PK);
			AssertEquals("Should be in order of sequence", address1.PK, docAddressesSequence2[3].PK);

			var docAddressesSequence3 = DocAddresses.FindDocAddressesByType(new[] { DocAddressType.ArrivalCTOAddress, DocAddressType.ArrivalCFSAddress, DocAddressType.ArrivalCTOAddress });
			AssertEquals("Should have returned 4", 4, docAddressesSequence3.Length);
			AssertEquals("Should be in order of sequence", address4.PK, docAddressesSequence3[0].PK);
			AssertEquals("Should be in order of sequence", address2.PK, docAddressesSequence3[1].PK);
			AssertEquals("Should be in order of sequence", address3.PK, docAddressesSequence3[2].PK);
			AssertEquals("Should be in order of sequence", address1.PK, docAddressesSequence3[3].PK);

			var docAddressesSequence4 = DocAddresses.FindDocAddressesByType(
				new[] { DocAddressType.ArrivalCTOAddress, DocAddressType.ArrivalCFSAddress },
				new[] { address2, address5 });
			AssertEquals("Should have returned 3", 3, docAddressesSequence4.Length);
			AssertEquals("Should be in order of sequence", address4.PK, docAddressesSequence4[0].PK);
			AssertEquals("Should be in order of sequence", address3.PK, docAddressesSequence4[1].PK);
			AssertEquals("Should be in order of sequence", address1.PK, docAddressesSequence4[2].PK);
		}

		#endregion

		#region TestFindByDocAddressType_Order

		public void TestFindByDocAddressType_Order()
		{
			JobDocAddress address1 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 3);
			JobDocAddress address2 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 1);
			JobDocAddress address3 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 2);
			JobDocAddress address4 = DocAddresses.AddNew(DocAddressType.ArrivalCTOAddress, 2);
			JobDocAddress address5 = DocAddresses.AddNew(DocAddressType.ArrivalCYDAddress, 2);

			AssertEquals("Should be first docAddress", address2.PK, DocAddresses.FindByDocAddressType(DocAddressType.ArrivalCFSAddress).PK);
		}

		#endregion

		#region TestFindByDocAddressType_Sequence

		public void TestFindByDocAddressType_Sequence()
		{
			JobDocAddress address1 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 3);
			JobDocAddress address2 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 1);
			JobDocAddress address3 = DocAddresses.AddNew(DocAddressType.ArrivalCFSAddress, 2);
			JobDocAddress address4 = DocAddresses.AddNew(DocAddressType.ArrivalCTOAddress, 2);
			JobDocAddress address5 = DocAddresses.AddNew(DocAddressType.ArrivalCYDAddress, 2);

			AssertEquals("Should be 3rd docAddress", address3.PK, DocAddresses.FindByDocAddressType(DocAddressType.ArrivalCFSAddress, 2).PK);
		}

		#endregion

		#region TestCreateWithAddressType

		public void TestCreateWithAddressType()
		{
			JobDocAddress address1 = DocAddresses.CreateWithAddressType(DocAddressType.ArrivalCFSAddress);
			JobDocAddress address2 = DocAddresses.CreateWithAddressType(DocAddressType.ArrivalCFSAddress);

			AssertEquals("ParentType", typeof(JobDocAddressParentForTesting), address1.ParentType);
			AssertEquals("ParentID", Parent.PK, address1.E2_ParentID);
			AssertEquals("ParentTableCode", "Z0", address1.E2_ParentTableCode);
			AssertEquals("Sequence", (ZByte)0, address1.E2_AddressSequence);

			AssertEquals("ParentType", typeof(JobDocAddressParentForTesting), address2.ParentType);
			AssertEquals("ParentID", Parent.PK, address2.E2_ParentID);
			AssertEquals("ParentTableCode", "Z0", address2.E2_ParentTableCode);
			AssertEquals("Sequence", (ZByte)1, address2.E2_AddressSequence);

			AssertNotEquals("Shouldn't be the same address", address1.PK, address2.PK);
		}

		#endregion

		#region TestCreateWithRequirement

		public void TestCreateWithRequirement()
		{
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.ArrivalCFSAddress, AddressType.APM, ContactType.NotifyParty, true, 2);

			JobDocAddress address1 = DocAddresses.CreateWithRequirement(requirement);
			JobDocAddress address2 = DocAddresses.CreateWithRequirement(requirement);
			JobDocAddress address3 = DocAddresses.CreateWithRequirement(requirement);

			AssertEquals("1- ParentType", typeof(JobDocAddressParentForTesting), address1.ParentType);
			AssertEquals("1- ParentID", Parent.PK, address1.E2_ParentID);
			AssertEquals("1- ParentTableCode", "Z0", address1.E2_ParentTableCode);
			AssertEquals("1- Sequence", (ZByte)0, address1.E2_AddressSequence);
			AssertEquals("1- DocAddressType", DocAddressType.ArrivalCFSAddress, address1.DocAddressType);

			AssertEquals("2- ParentType", typeof(JobDocAddressParentForTesting), address2.ParentType);
			AssertEquals("2- ParentID", Parent.PK, address2.E2_ParentID);
			AssertEquals("2- ParentTableCode", "Z0", address2.E2_ParentTableCode);
			AssertEquals("2- Sequence", (ZByte)1, address2.E2_AddressSequence);
			AssertEquals("2- DocAddressType", DocAddressType.ArrivalCFSAddress, address2.DocAddressType);

			AssertEquals("3- ParentType", typeof(JobDocAddressParentForTesting), address3.ParentType);
			AssertEquals("3- ParentID", Parent.PK, address3.E2_ParentID);
			AssertEquals("3- ParentTableCode", "Z0", address3.E2_ParentTableCode);
			AssertEquals("3- Sequence", (ZByte)2, address3.E2_AddressSequence);
			AssertEquals("3- DocAddressType", DocAddressType.ArrivalCFSAddress, address3.DocAddressType);

			AssertNotEquals("Shouldn't be the same address 1-2", address1.PK, address2.PK);
			AssertNotEquals("Shouldn't be the same address 2-3", address2.PK, address3.PK);
		}

		public void TestFindOrCreateWithRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.ArrivalCFSAddress, AddressType.APM, ContactType.NotifyParty, true, 2);

			JobDocAddress address1 = DocAddresses.FindOrCreateWithRequirement(requirement);

			AssertEquals("1- ParentType", typeof(JobDocAddressParentForTesting), address1.ParentType);
			AssertEquals("1- ParentID", Parent.PK, address1.E2_ParentID);
			AssertEquals("1- ParentTableCode", "Z0", address1.E2_ParentTableCode);
			AssertEquals("1- Sequence", (ZByte)0, address1.E2_AddressSequence);
			AssertEquals("1- DocAddressType", DocAddressType.ArrivalCFSAddress, address1.DocAddressType);
			AssertEquals("Requirement should be set", requirement, address1.Requirement);

			JobDocAddress address2 = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.LocalCartageExporter);

			AssertNull("Requirement should not be set", address2.Requirement);

			var requirement2 = new JobDocAddressRequirement(DocAddressType.LocalCartageExporter, AddressType.APM, ContactType.NotifyParty, true, 2);

			JobDocAddress address3 = DocAddresses.FindOrCreateWithRequirement(requirement2);

			AssertEquals("Requirement should be set", requirement2, address2.Requirement);
			AssertEquals("Addresses should be same", address2, address3);
		}

		#endregion

		#region Load

		public void TestMandatoryAddressTypes()
		{
			JobDocAddressMandatoryForTesting parent = Factory.New<JobDocAddressMandatoryForTesting>();
			parent.DocAddresses.Load();
			AssertEquals("Mandatory address type automatically added", 1, parent.DocAddresses.Count);

			parent.DocAddresses[0].Validation.ValidateOrganisationPK();
			AssertHasErrors(parent.DocAddresses[0].OrganisationPKInfo);

			parent.DocAddresses[0].OrganisationPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertNoErrors(parent.DocAddresses[0].OrganisationPKInfo);
		}

		class JobDocAddressMandatoryForTesting : DummyBusinessObject, IDocAddresses
		{
			public JobDocAddressMandatoryForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public JobDocAddressDependentCollection DocAddresses
			{
				get { return fDocAddresses ?? (fDocAddresses = new JobDocAddressDependentCollection(this)); }
			}
			JobDocAddressDependentCollection fDocAddresses;

			ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
			{
				return null;
			}

			Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
			{
				return null;
			}

			IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
			{
				get { return new DocAddressType[] { DocAddressType.SupplierPickupDeliveryAddress, DocAddressType.TransportBillToAddress }; }
			}

			JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
			{
				JobDocAddressRequirement requirement = new JobDocAddressRequirement();
				if (addressType == DocAddressType.TransportBillToAddress)
				{
					requirement.IsMandatory = true;
				}
				return requirement;
			}

			void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
			{
			}

			bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
			{
				return false;
			}

			OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
			{
				return null;
			}
		}

		#endregion

		public void TestAddExistingSetsParentType()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			AssertEquals("precondition:", null, address.ParentType);
			AssertEquals("precondition:", ZGuid.Empty, address.E2_ParentID);
			AssertEquals("precondition:", "", address.E2_ParentTableCode);

			DocAddresses.Add(address);

			AssertEquals("ParentType", typeof(JobDocAddressParentForTesting), address.ParentType);
			AssertEquals("ParentID", Parent.PK, address.E2_ParentID);
			AssertEquals("ParentTableCode", "Z0", address.E2_ParentTableCode);
		}

		public void TestCreateProxyAddresses()
		{
			JobDocAddress proxyDA = DocAddresses.AddNew(OrgAddress1);
			AssertNotNull("Proxy JobDocAddress should exist.", proxyDA);
			AssertEquals("Proxy JobDocAddress should have ParentID set.", Parent.PK, proxyDA.E2_ParentID);
			AssertEquals("Proxy JobDocAddress should have OrgAddress set.", OrgAddress1.PK, proxyDA.E2_OA_Address);
		}

		public void TestFindByDocAddressType()
		{
			JobDocAddress proxyDA1 = DocAddresses.AddNew(OrgAddress1, DocAddressType.LocalCartageExporter);
			JobDocAddress proxyDA2 = DocAddresses.AddNew(OrgAddress2, DocAddressType.LocalCartageImporter);
			AssertEquals("Proxy JobDocAddress should exist.", true, proxyDA1 != null && proxyDA2 != null);
			AssertEquals("Proxy JobDocAddress should have ParentID set.", Parent.PK, proxyDA1.E2_ParentID);
			AssertEquals("Proxy JobDocAddress should have OrgAddress set.", OrgAddress2.PK, proxyDA2.E2_OA_Address);

			AssertEquals("Proxy1 should be found by AddressType.", proxyDA1.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter).PK);
			AssertEquals("Proxy2 should be found by AddressType.", proxyDA2.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter).PK);
		}

		public void TestFindOrCreateWithAddressType()
		{
			JobDocAddress dA1 = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.LocalCartageExporter);
			JobDocAddress dA2 = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.LocalCartageImporter);
			AssertEquals("JobDocAddresses should exist.", true, dA1 != null && dA2 != null);
			AssertEquals("JobDocAddress should have ParentID set.", Parent.PK, dA1.E2_ParentID);

			AssertEquals("DocAddress1 should be found by AddressType.", dA1.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter).PK);
			AssertEquals("DocAddress2 should be found by AddressType.", dA2.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter).PK);
		}

		public void TestFindOrCreateWithOrgAddress()
		{
			OrgAddress orgA = Factory.New<OrgAddress>();
			JobDocAddress dA1 = DocAddresses.FindOrCreateWithDocAddressType(orgA.PK, DocAddressType.LocalCartageExporter);
			JobDocAddress dA2 = DocAddresses.FindOrCreateWithDocAddressType(orgA.PK, DocAddressType.LocalCartageImporter);
			JobDocAddress dA3 = DocAddresses.FindOrCreateWithDocAddressType(ZGuid.Empty, DocAddressType.LocalCartageCTO);
			AssertEquals("JobDocAddresses should exist.", true, dA1 != null && dA2 != null && dA3 != null);
			AssertEquals("JobDocAddress should have ParentID set.", Parent.PK, dA1.E2_ParentID);

			AssertNotNull("DocAddress1 should have a valid OrgAddress.", dA1.Address);
			AssertNotNull("DocAddress2 should have a valid OrgAddress.", dA2.Address);
			AssertNull("DocAddress3 should have invalid OrgAddress.", dA3.Address);

			AssertEquals("JobDocAddress1 should have OrgAddress set.", orgA.PK, dA1.Address.PK);
			AssertEquals("JobDocAddress2 should have OrgAddress set.", orgA.PK, dA2.Address.PK);

			AssertEquals("DocAddress1 should have OA_Address set.", dA1.Address.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter).E2_OA_Address);
			AssertEquals("DocAddress2 should have OA_Address set.", dA2.Address.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter).E2_OA_Address);
			AssertEquals("DocAddress3 should have no OA_Address set.", dA2.Address.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter).E2_OA_Address);
		}

		public void TestAddNewWithAddressType()
		{
			JobDocAddress dA1 = DocAddresses.AddNew(DocAddressType.LocalCartageExporter);
			JobDocAddress dA2 = DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			AssertEquals("JobDocAddresses should exist.", true, dA1 != null && dA2 != null);
			AssertEquals("JobDocAddress should have ParentID set.", Parent.PK, dA1.E2_ParentID);

			AssertEquals("DocAddress1 should be found by AddressType.", dA1.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter).PK);
			AssertEquals("DocAddress2 should be found by AddressType.", dA2.PK, DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter).PK);
		}

		public void TestContainsAddressType()
		{
			JobDocAddress dA1 = DocAddresses.AddNew(DocAddressType.LocalCartageExporter);
			JobDocAddress dA2 = DocAddresses.AddNew(DocAddressType.LocalCartageImporter);

			AssertNotNull("JobDocAddresses should exist.", dA1);
			AssertNotNull("JobDocAddresses should exist.", dA2);
			AssertEquals("JobDocAddress should have ParentID set.", Parent.PK, dA1.E2_ParentID);

			dA1.E2_OA_Address = ZGuid.NewZGuid();
			dA2.E2_OA_Address = ZGuid.NewZGuid();

			AssertEquals("DocAddress1 should be found by AddressType.", true, DocAddresses.ContainsDocAddressType(DocAddressType.LocalCartageExporter));
			AssertEquals("DocAddress2 should be found by AddressType.", true, DocAddresses.ContainsDocAddressType(DocAddressType.LocalCartageImporter));
			AssertEquals("Bad DocAddress should not be found by AddressType.", false, DocAddresses.ContainsDocAddressType(DocAddressType.LocalCartageAddress1));
		}

		public void TestUpdateByDataRefreshRemovesNotInDatabaseSimilarJobDocAddress()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			OrgHeader header = factory1.NewWithValidTestData<OrgHeader>();
			OrgAddress orgAddress = header.Addresses.MainAddress;
			orgAddress.OA_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			orgAddress.OA_Address1 = "dummy address";
			JobDocAddressPersistentParentForTesting parent1 = factory1.New<JobDocAddressPersistentParentForTesting>();
			JobDocAddressDependentCollection docAddresses1 = parent1.DocAddresses;
			factory1.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDocAddressPersistentParentForTesting parent1InDiffFactory = factory2.Load<JobDocAddressPersistentParentForTesting>(parent1.PK);
			JobDocAddressDependentCollection docAddresses1InDiffFactory = parent1InDiffFactory.DocAddresses;

			JobDocAddress docAddress1 = docAddresses1.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			JobDocAddress docAddress1InDiffFactor = docAddresses1InDiffFactory.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			AssertNotEquals("PreCondition:docAddress pk", docAddress1.PK, docAddress1InDiffFactor.PK);
			docAddress1InDiffFactor.E2_OA_Address = orgAddress.PK;
			factory2.Save();

			AssertEquals("docAddress1.IsDeleted", true, docAddress1.IsDeleted);
			docAddress1 = docAddresses1.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			AssertEquals("docAddress pk", docAddress1.PK, docAddress1InDiffFactor.PK);
			AssertEquals("docAddress E2_OA_Address", docAddress1.E2_OA_Address, docAddress1InDiffFactor.E2_OA_Address);

			docAddress1.E2_Address1 = "hello people";
			factory1.Save();
			AssertEquals("docAddress1InDiffFactor.IsDeleted", false, docAddress1InDiffFactor.IsDeleted);
			AssertEquals("docAddress pk", docAddress1.PK, docAddress1InDiffFactor.PK);
			AssertEquals("docAddress E2_Address1", docAddress1.E2_Address1, docAddress1InDiffFactor.E2_Address1);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ExternalParent = new JobDocAddressParentForTesting(Factory);
			TestCollection = new JobDocAddressDependentCollection(ExternalParent);
			return TestCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobDocAddress>();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Parent = new JobDocAddressParentForTesting(Factory);
			OrgAddress1 = Factory.New<OrgAddress>();
			OrgAddress2 = Factory.New<OrgAddress>();
			DocAddresses = new JobDocAddressDependentCollection(Parent);
		}

		JobDocAddressParentForTesting Parent;
		OrgAddress OrgAddress1;
		OrgAddress OrgAddress2;
		JobDocAddressDependentCollection DocAddresses;

		JobDocAddressParentForTesting ExternalParent;
		JobDocAddressDependentCollection TestCollection;
		#endregion
	}
}
