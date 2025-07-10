using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCarrierAttributeCombinedCollection : ActiveBusinessObjectCollection<ZZRefCarrierAttributeCombined>
	{
		public ZZRefCarrierAttributeCombinedCollection(ZZRefCarrierCombined refCarrier)
			: base(refCarrier.Factory, refCarrier, new ZQuery(), ZZRefCarrierAttributeCombinedSchema.ZZG_ZZ4_CarrierCode)
		{
			this.refCarrier = refCarrier;
		}

		public ZZRefCarrierAttributeCombined AddNew(ZString name, ZString value)
		{
			var result = AddNew();
			result.ZZG_Name = name;
			result.ZZG_Value = value;

			return result;
		}

		public bool HasAttribute(ZString name) => this.Any(x => x.ZZG_Name.EqualsIgnoringCase(name));

		protected override bool AllowNew => !refCarrier.ZZ4_IsSystem;
		readonly ZZRefCarrierCombined refCarrier;
	}
}
