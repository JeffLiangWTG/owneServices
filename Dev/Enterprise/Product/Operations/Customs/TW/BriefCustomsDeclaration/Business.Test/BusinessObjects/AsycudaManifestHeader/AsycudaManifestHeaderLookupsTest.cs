using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderLookups))]
	sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPersonsListType()
		{
			AssertType<GlbPersonCollection>(lookups.PersonsList);
		}

		public void TestBoxNumbers()
		{
			var boxNumbers = new CusBrokerageBoxNumberCollection();
			var boxNumber = boxNumbers.AddNew();
			boxNumber.BoxNumber = "C01";
			boxNumber.CustomsOfficeArea = "C";
			boxNumber.IsDefaultBoxNumber = false;
			var boxNumber2 = boxNumbers.AddNew();
			boxNumber2.BoxNumber = "C02";
			boxNumber2.CustomsOfficeArea = "C";
			boxNumber2.IsDefaultBoxNumber = true;
			using (TWCustomsDataRegistry.Instance.CusBrokerageBoxNumber.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, boxNumbers))
			{
				manifestHeader.AMA_CustomsOffice = "CA";
				AssertSame(manifestHeader.CusBrokerageBoxNumbers.PairList, lookups.BoxNumbers);
				AssertEquals("C01, C02", lookups.BoxNumbers.CodesAsString);
			}
		}

		public void TestMailboxList()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS1";
			var passwords = TWGlbStaffWrapper.Get(staff).TWPasswordCollection;
			var password = passwords.AddNew();
			password.GP_MailBoxID = "MB1";
			var password2 = passwords.AddNew();
			password2.GP_MailBoxID = "MB2";
			manifestHeader.AMA_GS_NKCustomsAgent = "GS1";

			AssertEquals("MB1, MB2", lookups.MailboxList.CodesAsString);
		}

		public void TestNatures()
		{
			AssertEquals(Universal.Helper.ShipmentTypeList.Export22AndImport23(), lookups.Natures);
		}

		public void TestTransportModeList()
		{
			var expectLit = Factory.GetCachedValue("TWTransportModeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);
				result.AddPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air);
				return result;
			});
			AssertEquals(expectLit, lookups.TransportModeList);
		}

		public void TestPaymentMethods()
		{
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<IMPPaymentMethod>(), lookups.PaymentMethods);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			lookups = new AsycudaManifestHeaderLookups(manifestHeader);
		}

		AsycudaManifestHeader manifestHeader;
		AsycudaManifestHeaderLookups lookups;
	}
}

