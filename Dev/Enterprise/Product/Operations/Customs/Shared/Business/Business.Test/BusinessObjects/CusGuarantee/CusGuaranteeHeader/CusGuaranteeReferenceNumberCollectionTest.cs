using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGuaranteeReferenceNumberCollection<CusGuaranteeReferenceNumber>))]
	sealed class CusGuaranteeReferenceNumberCollectionTest : CusCodeDataCollectionTest<CusGuaranteeReferenceNumber>
	{
		protected override CusCodeDataCollection<CusGuaranteeReferenceNumber> GetCusCodeDataCollection()
		{
			return new CusGuaranteeReferenceNumberCollection<CusGuaranteeReferenceNumber>(GuaranteeHeader);
		}

		BaseCusGuaranteeHeader GuaranteeHeader
		{
			get
			{
				if (guaranteeHeader == null)
				{
					guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
				}
				return guaranteeHeader;
			}
		}
		BaseCusGuaranteeHeader guaranteeHeader;
	}
}
