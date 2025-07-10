using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCarrierCodeAttributeCollection : ActiveBusinessObjectCollection<RefCarrierCodeAttribute>
	{
		public RefCarrierCodeAttributeCollection(RefCarrierCode carrierCode)
			: base(carrierCode.Factory, carrierCode, new ZQuery(), RefCarrierCodeAttributeSchema.ZZG_ZZ4_CarrierCode)
		{
		}

		public RefCarrierCodeAttribute AddNew(ZString name, ZString value)
		{
			var result = AddNew();
			result.ZZG_Name = name;
			result.ZZG_Value = value;
			return result;
		}
	}
}
