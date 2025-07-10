using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DocAddressCreatorHost))]
	sealed class DocAddressCreatorHostTest : NonPersistentBusinessObjectTestCase
	{
		#region TestAddressTypeCode

		public void TestAddressTypeCode()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			AssertEquals("CTO", host.AddressTypeCode);
			AssertEquals(DocAddressType.LocalCartageCTO, host.DocAddress.DocAddressType);

			host.AddressTypeCode = "";
			AssertEquals("", host.AddressTypeCode);
			AssertEquals(DocAddressType.None, host.DocAddress.DocAddressType);

			host.AddressTypeCode = "CFS";
			AssertEquals("CFS", host.AddressTypeCode);
			AssertEquals(DocAddressType.LocalCartageCFS, host.DocAddress.DocAddressType);

			host.AddressTypeCode = "123";
			AssertEquals("123", host.AddressTypeCode);
			AssertEquals(DocAddressType.None, host.DocAddress.DocAddressType);
		}

		#endregion

		#region TestDocAddress

		public void TestDocAddress()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			AssertEquals(DocAddressType.LocalCartageCTO, host.DocAddress.DocAddressType);
			AssertEquals(ZGuid.Empty, host.DocAddress.E2_OA_Address);
			AssertEquals(true, host.DocAddress.Parent.DocAddresses.Contains(host.DocAddress));
		}

		#endregion

		#region TestDocAddress_EnsureSequenceWhenSameType

		public void TestDocAddress_EnsureSequenceWhenSameType()
		{
			var iDocAddresses = DocAddressCreatorHelper.CreateDocAddressParent(new DocAddressType[] { DocAddressType.LocalCartageCFS, DocAddressType.LocalCartageCTO });
			JobDocAddress ctoAddress0 = iDocAddresses.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			JobDocAddress ctoAddress1 = iDocAddresses.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			JobDocAddress cfsAddress0 = iDocAddresses.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			AssertEquals("Precondition", (byte)0, ctoAddress0.E2_AddressSequence);
			AssertEquals("Precondition", (byte)1, ctoAddress1.E2_AddressSequence);
			AssertEquals("Precondition", (byte)0, cfsAddress0.E2_AddressSequence);

			DocAddressCreatorHost.DocAddressTypeCodeFormatter addressCodeConverter = delegate(DocAddressType docAddressType)
			{
				switch (docAddressType)
				{
					case DocAddressType.LocalCartageCFS:
						return "CFS";
					case DocAddressType.LocalCartageCTO:
						return "CTO";
					default:
						return "";
				}
			};

			var host = new DocAddressCreatorHost(iDocAddresses, addressCodeConverter, DocAddressType.LocalCartageCTO, Factory);
			AssertEquals(DocAddressType.LocalCartageCTO, host.DocAddress.DocAddressType);
			AssertEquals(ZGuid.Empty, host.DocAddress.E2_OA_Address);
			AssertEquals(true, iDocAddresses.DocAddresses.Contains(host.DocAddress));
			AssertEquals((byte)2, host.DocAddress.E2_AddressSequence);

			host.AddressTypeCode = "CFS";
			AssertEquals(DocAddressType.LocalCartageCFS, host.DocAddress.DocAddressType);
			AssertEquals(ZGuid.Empty, host.DocAddress.E2_OA_Address);
			AssertEquals(true, iDocAddresses.DocAddresses.Contains(host.DocAddress));
			AssertEquals((byte)1, host.DocAddress.E2_AddressSequence);

			host.AddressTypeCode = "CTO";
			AssertEquals(DocAddressType.LocalCartageCTO, host.DocAddress.DocAddressType);
			AssertEquals(ZGuid.Empty, host.DocAddress.E2_OA_Address);
			AssertEquals(true, iDocAddresses.DocAddresses.Contains(host.DocAddress));
			AssertEquals((byte)2, host.DocAddress.E2_AddressSequence);
		}

		#endregion

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			return DocAddressCreatorHelper.CreateDocAddressCreatorHost();
		}

		#endregion

		#region TestDocAddressCreatorHostEdit

		public void TestDocAddressCreatorHostEdit()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHostEditor();
			AssertEquals(DocAddressType.LocalCartageCFS, host.DocAddress.DocAddressType);
			AssertEquals(host.DocAddressParent.DocAddresses[0], host.DocAddress);
		}

		#endregion

		#region DocAddressCreatorHelper

		DocAddressCreatorHelper DocAddressCreatorHelper
		{
			get { return docAddressCreatorHelper ?? (docAddressCreatorHelper = new DocAddressCreatorHelper(Factory)); }
		}
		DocAddressCreatorHelper docAddressCreatorHelper;

		#endregion
	}
}
