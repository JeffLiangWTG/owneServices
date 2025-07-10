using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Internal
{
	internal class RefCusTariff : AutoRefCusTariff
	{
		public RefCusTariff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CusTariffType")]
		public override ZGuid ZZ1_ZZI_TariffType
		{
			get { return base.ZZ1_ZZI_TariffType; }
			set { base.ZZ1_ZZI_TariffType = value; }
		}

		#region Related BusinessObjects

		public RefCusTariffType CusTariffType => Factory.Load<RefCusTariffType>(ZZ1_ZZI_TariffType);

		#endregion
	}
}
