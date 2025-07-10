using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeaderManufacturerAddresses))]
	sealed class CusISFHeaderManufacturerAddressesTest : ActiveBusinessObjectCollectionTestCase<CusISFHeaderManufacturerAddresses>
	{
		public void TestDeleteJobDocAddressResetReferenceInISFLines()
		{
			var header = Factory.New<CusISFHeader>();
			var manufacturer = header.ManufacturerAddresses.AddNew();
			manufacturer.OrganisationPK = Factory.New<OrgHeader>().PK;
			var line = header.Lines.AddNew();
			line.BL_ManufacturerDocAddressPK = manufacturer.PK;
			header.ManufacturerAddresses.DeleteAll();
			AssertEquals(ZGuid.Empty, line.BL_ManufacturerDocAddressPK);
		}

		public void TestIssue00221735AddingDeletedObject()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ISFDocAddress docAddress = (ISFDocAddress)((IBindingList)header.ManufacturerAddresses).AddNew();
			header.ManufacturerAddresses.Add(docAddress);
			ISFDocAddress docAddress2 = header.ManufacturerAddresses.AddNew();
			System.Data.DataRow row = ((IBusinessObjectInternals)docAddress).Row;
			AssertNoExceptionThrown(delegate
			{
				((ICancelAddNew)header.ManufacturerAddresses).EndNew(0);
			});
		}

		public void TestIsEmpty()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ISFDocAddress docAddress1 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			ISFDocAddress docAddress2 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			AssertEquals(true, docAddress2.IsEmpty);
			AssertEquals(2, header.ManufacturerAddresses.Count);
			AssertEquals(true, header.ManufacturerAddresses.IsEmpty);
			docAddress2.E2_AddressOverride = true;
			AssertEquals(false, docAddress2.IsEmpty);
			AssertEquals(false, header.ManufacturerAddresses.IsEmpty);
		}

		public void TestAddingNewManufacturerAssignsAddressSequenceNotCauseException_CS00141936()
		{
			var org1 = CreateOrg("ZZZORG1");
			var org2 = CreateOrg("ZZZORG2");
			var org3 = CreateOrg("ZZZORG3");
			var org4 = CreateOrg("ZZZORG4");
			var org5 = CreateOrg("ZZZORG5");
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.ManufacturerAddresses.ApplySort(JobDocAddress.Schema.E2_Contact, ListSortDirection.Descending);
			var manufacturer1 = header.ManufacturerAddresses.AddNew();
			manufacturer1.OrganisationPK = org1.PK;
			manufacturer1.E2_Contact = "Z";
			var manufacturer2 = header.ManufacturerAddresses.AddNew();
			manufacturer2.OrganisationPK = org5.PK;
			manufacturer2.E2_Contact = "A";
			var manufacturer3 = header.ManufacturerAddresses.AddNew();
			manufacturer3.OrganisationPK = org2.PK;
			manufacturer3.E2_Contact = "L";
			manufacturer1.E2_AddressSequence = 4;
			manufacturer2.E2_AddressSequence = 3;
			manufacturer3.E2_AddressSequence = 1;
			Factory.Save();
			AssertEquals((ZByte)4, manufacturer1.E2_AddressSequence);
			AssertEquals((ZByte)3, manufacturer2.E2_AddressSequence);
			AssertEquals((ZByte)1, manufacturer3.E2_AddressSequence);
			var manufacturer4 = header.ManufacturerAddresses.AddNew();
			manufacturer4.OrganisationPK = org4.PK;
			manufacturer4.E2_Contact = "O";
			AssertEquals(ZByte.Zero, manufacturer4.E2_AddressSequence);
			var manufacturer5 = header.ManufacturerAddresses.AddNew();
			manufacturer5.OrganisationPK = org3.PK;
			manufacturer5.E2_Contact = "B";
			AssertEquals((ZByte)2, manufacturer5.E2_AddressSequence);
			Factory.Save();
			AssertEquals((ZByte)4, manufacturer1.E2_AddressSequence);
			AssertEquals((ZByte)3, manufacturer2.E2_AddressSequence);
			AssertEquals((ZByte)1, manufacturer3.E2_AddressSequence);
			AssertEquals(ZByte.Zero, manufacturer4.E2_AddressSequence);
			AssertEquals((ZByte)2, manufacturer5.E2_AddressSequence);
		}

		[ExpectNoExceptions]
		public void TestAddressSequence_WhenZByteOverflow_SkipValidation()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var address = new CusISFHeaderManufacturerAddressesSub(Header);
			for (int i = 0; i <= byte.MaxValue; i++)
			{
				var docAddress = Factory.NewWithValidTestData<ISFDocAddress>();
				address.Add(docAddress);
				docAddress.E2_AddressSequence = new ZByte((byte)i);
				docAddress.E2_OA_Address = orgAddress.PK;
			}

			Assert(!Header.HasErrors);
			var finalDocAddress = Factory.NewWithValidTestData<ISFDocAddress>();
			address.Add(finalDocAddress);
			address.SetDefaultsForNewElementCore(finalDocAddress);
			finalDocAddress.E2_OA_Address = orgAddress.PK;
			Header.RunPreSaveValidation();
			Assert(!Header.HasErrors);
		}

		public void TestReinitializeAddressSequenceNumber()
		{
			var cusIsfHeader = Factory.New<CusISFHeader>();
			var manufacturer1 = cusIsfHeader.ManufacturerAddresses.AddNew();
			var manufacturer2 = cusIsfHeader.ManufacturerAddresses.AddNew();
			var manufacturer3 = cusIsfHeader.ManufacturerAddresses.AddNew();
			var manufacturer4 = cusIsfHeader.ManufacturerAddresses.AddNew();
			var manufacturer5 = cusIsfHeader.ManufacturerAddresses.AddNew();
			AssertEquals((ZByte)0, manufacturer1.E2_AddressSequence);
			AssertEquals((ZByte)1, manufacturer2.E2_AddressSequence);
			AssertEquals((ZByte)2, manufacturer3.E2_AddressSequence);
			AssertEquals((ZByte)3, manufacturer4.E2_AddressSequence);
			AssertEquals((ZByte)4, manufacturer5.E2_AddressSequence);
			cusIsfHeader.ManufacturerAddresses.Delete(manufacturer1);
			AssertEquals((ZByte)0, manufacturer2.E2_AddressSequence);
			AssertEquals((ZByte)2, manufacturer3.E2_AddressSequence);
			AssertEquals((ZByte)3, manufacturer4.E2_AddressSequence);
			AssertEquals((ZByte)4, manufacturer5.E2_AddressSequence);
			cusIsfHeader.ManufacturerAddresses.Delete(manufacturer3);
			cusIsfHeader.ManufacturerAddresses.Delete(manufacturer4);
			AssertEquals((ZByte)0, manufacturer2.E2_AddressSequence);
			AssertEquals((ZByte)4, manufacturer5.E2_AddressSequence);
			cusIsfHeader.ManufacturerAddresses.Delete(manufacturer2);
			AssertEquals((ZByte)0, manufacturer5.E2_AddressSequence);
		}

		protected override Type GetExpectedCollectionType() => typeof(CusISFHeaderManufacturerAddresses);

		protected override CusISFHeaderManufacturerAddresses GetCollectionToTest() => new CusISFHeaderManufacturerAddresses(Header);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ISFDocAddress docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_ParentID = Header.PK;
			docAddress.E2_ParentTableCode = Header.TablePrefix;
			return docAddress;
		}

		CusISFHeader header;
		CusISFHeader Header => header ?? (header = Factory.New<CusISFHeader>());

		OrgHeader CreateOrg(ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = code + " FULLNAME";
			org.MainAddress.OA_Address1 = code + " ADDRESS 1";
			return org;
		}

		sealed class CusISFHeaderManufacturerAddressesSub : CusISFHeaderManufacturerAddresses
		{
			public CusISFHeaderManufacturerAddressesSub(CusISFHeader header)
				: base(header)
			{
			}

			public new void SetDefaultsForNewElementCore(ISFDocAddress newElement) => base.SetDefaultsForNewElementCore(newElement);
		}
	}
}
