using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCarrierCodeAttribute : AutoRefCarrierCodeAttribute
	{
		public RefCarrierCodeAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[RelatedBusinessObject("CarrierCode")]
		public override ZGuid ZZG_ZZ4_CarrierCode
		{
			get => base.ZZG_ZZ4_CarrierCode;
			set => base.ZZG_ZZ4_CarrierCode = value;
		}

		public RefCarrierCode CarrierCode => Factory.Load<RefCarrierCode>(ZZG_ZZ4_CarrierCode);

#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

#endif
	}
}
