using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffRelationship : AutoRefCusTariffRelationship
	{
		public RefCusTariffRelationship(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("RelatedTariffFrom")]
		public override ZGuid ZZH_ZZ1_Tariff
		{
			get { return base.ZZH_ZZ1_Tariff; }
			set { base.ZZH_ZZ1_Tariff = value; }
		}

		[RelatedBusinessObject("CusTariffType")]
		public override ZGuid ZZH_ZZI_TariffType
		{
			get { return base.ZZH_ZZI_TariffType; }
			set { base.ZZH_ZZI_TariffType = value; }
		}

		#region Related BusinessObjects

		public TariffView RelatedTariffFrom
		{
			get { return Factory.Load<TariffView>(ZZH_ZZ1_Tariff); }
		}

		public RefCusTariffType CusTariffType
		{
			get
			{
				if (cusTariffType == null || (cusTariffType.PK != ZZH_ZZI_TariffType))
				{
					cusTariffType = Factory.Load<RefCusTariffType>(ZZH_ZZI_TariffType);
				}
				return cusTariffType;
			}
		}
		RefCusTariffType cusTariffType;

		#endregion

		protected override bool SupportsCloneCore()
		{
			return false;
		}

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}
#endif
	}
}
