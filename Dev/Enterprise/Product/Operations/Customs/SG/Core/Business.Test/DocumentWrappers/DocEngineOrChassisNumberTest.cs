using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.BaseTradeNetPermitItem;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocEngineOrChassisNumber))]
	sealed class DocEngineOrChassisNumberTest : DocumentWrapperTestCase
	{
		public void TestSequenceNumber()
		{
			AssertEquals("   03", EngineOrChassisNumber.SequenceNumber);
		}

		public void TestNumber()
		{
			AssertEquals("T1006520 / T2550650", EngineOrChassisNumber.Number);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers() => new[] { EngineOrChassisNumber };

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocEngineOrChassisNumber.New(ItemEngineOrChassisNumber, Factory);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			base.SetUp();
		}

		DocEngineOrChassisNumber EngineOrChassisNumber
		{
			get
			{
				if (engineOrChassisNumber == null)
				{
					engineOrChassisNumber = DocEngineOrChassisNumber.New(ItemEngineOrChassisNumber, Factory);
				}

				return engineOrChassisNumber;
			}
		}
		DocEngineOrChassisNumber engineOrChassisNumber;

		ItemEngineOrChassisNumber ItemEngineOrChassisNumber
		{
			get
			{
				if (itemEngineOrChassisNumber == null)
				{
					itemEngineOrChassisNumber = new ItemEngineOrChassisNumber(3, new Business.Messaging.Tradenet.CASCProductAdditionalCASCIdentification
					{
						CASCCodeOne = "T1006520",
						CASCCodeTwo = "T2550650",
						CASCCodeThree = "T3W85203R"
					});
				}

				return itemEngineOrChassisNumber;
			}
		}
		ItemEngineOrChassisNumber itemEngineOrChassisNumber;

		#endregion
	}
}
