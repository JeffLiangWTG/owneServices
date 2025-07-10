using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeaderExtraShipToPartyAddresses))]
	sealed class CusISFHeaderExtraShipToPartyAddressesTest : ActiveBusinessObjectCollectionTestCase<CusISFHeaderExtraShipToPartyAddresses>
	{
		[ExpectNoExceptions]
		public void TestEndNewDoesNotThrowOutOfRangeException()
		{
			var header = Factory.New<CusISFHeader>();
			((IBindingList)header.ExtraShipToPartyAddresses).AddNew();
			((ICancelAddNew)header.ExtraShipToPartyAddresses).EndNew(0);
		}

		public void TestNotIncludeMainShipToParty()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertNotNull(header.MainShipToParty);
			ISFDocAddress docAddress1 = header.DocAddresses.CreateWithAddressType(DocAddressType.ShipToParty);
			ISFDocAddress docAddress2 = header.DocAddresses.CreateWithAddressType(DocAddressType.ShipToParty);
			AssertEquals(2, header.ExtraShipToPartyAddresses.Count);
			AssertContainsExactElementsInAnyOrder(new ISFDocAddress[] { docAddress1, docAddress2 }, header.ExtraShipToPartyAddresses);
		}

		public void TestIsEmpty()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ISFDocAddress docAddress1 = header.DocAddresses.CreateWithAddressType(DocAddressType.ShipToParty);
			docAddress1.E2_AddressSequence = 1;
			ISFDocAddress docAddress2 = header.DocAddresses.CreateWithAddressType(DocAddressType.ShipToParty);
			docAddress2.E2_AddressSequence = 1;
			AssertEquals(true, docAddress2.IsEmpty);
			AssertEquals(2, header.ExtraShipToPartyAddresses.Count);
			AssertEquals(true, header.ExtraShipToPartyAddresses.IsEmpty);
			docAddress2.E2_AddressOverride = true;
			AssertEquals(false, docAddress2.IsEmpty);
			AssertEquals(false, header.ExtraShipToPartyAddresses.IsEmpty);
		}

		protected override Type GetExpectedCollectionType() => typeof(CusISFHeaderExtraShipToPartyAddresses);

		protected override CusISFHeaderExtraShipToPartyAddresses GetCollectionToTest() => new CusISFHeaderExtraShipToPartyAddresses(Factory.New<CusISFHeader>());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			var header = Factory.New<CusISFHeader>();
			docAddress.E2_ParentID = header.PK;
			docAddress.E2_ParentTableCode = header.TablePrefix;
			docAddress.E2_AddressSequence = 255;
			return docAddress;
		}
	}
}
