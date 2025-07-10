using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.BaseTradeNetPermitItem;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocCASCProductCode))]
	sealed class DocCASCProductCodeTest : DocumentWrapperTestCase
	{
		public void TestSequenceNumber()
		{
			AssertEquals("   03", ProductCode.SequenceNumber);
		}

		public void TestProductCode()
		{
			AssertEquals("AAA00001", ProductCode.ProductCode);
		}

		public void TestProductQuantity()
		{
			AssertEquals("            39.5  KGM", ProductCode.ProductQuantity);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers() => new[] { ProductCode };

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocCASCProductCode.New(ItemProductCode, Factory);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			base.SetUp();
		}

		DocCASCProductCode ProductCode
		{
			get
			{
				if (productCode == null)
				{
					productCode = DocCASCProductCode.New(ItemProductCode, Factory);
				}

				return productCode;
			}
		}
		DocCASCProductCode productCode;

		ItemProductCode ItemProductCode
		{
			get
			{
				if (itemProductCode == null)
				{
					itemProductCode = new ItemProductCode(3, new Business.Messaging.Tradenet.CASCProduct()
					{
						CASCProductCode = "AAA00001",
						CASCProductQuantity = new Business.Messaging.Tradenet.CASCProductQuantity
						{
							Value = 39.5m,
							unitCode = "KGM"
						}
					});
				}

				return itemProductCode;
			}
		}
		ItemProductCode itemProductCode;

		#endregion
	}
}
