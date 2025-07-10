using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxIdAndTaxMessageCombinationRulesCollection))]
	sealed class TaxIdAndTaxMessageCombinationRulesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TaxIdAndTaxMessageCombinationRulesCollection>
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			ValidTaxRate1 = taxRateCollection.AddNew();
			ValidTaxRate1.AT_Code = "TGST";
			ValidTaxRate1.AT_Type = "RAT";
			ValidTaxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ValidTaxRate1.SetRateNumerator_ForTestOnly(10);

			ValidTaxRate2 = taxRateCollection.AddNew();
			ValidTaxRate2.AT_Code = "TGST2";
			ValidTaxRate2.AT_Type = "RAT";
			ValidTaxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ValidTaxRate2.SetRateNumerator_ForTestOnly(10);

			var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			ValidTaxMessage = taxMessageCollection.AddNew();
			ValidTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();
		}

		AccTaxRate ValidTaxRate1;
		AccTaxRate ValidTaxRate2;

		AccInvMsg ValidTaxMessage;

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override TaxIdAndTaxMessageCombinationRulesCollection GetCollectionToTest()
		{
			return new TaxIdAndTaxMessageCombinationRulesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TaxIdAndTaxMessageCombinationRules();
		}

		#endregion
	}
}
