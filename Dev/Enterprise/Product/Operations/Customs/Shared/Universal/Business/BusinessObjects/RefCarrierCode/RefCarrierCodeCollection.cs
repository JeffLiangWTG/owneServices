using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal
{
	[CodeAlive("Under Development")]
	public sealed class RefCarrierCodeCollection : ActiveBusinessObjectCollection<RefCarrierCode> //Never unseal. This collection only looks at one DB. If inheritance is required inherit from the combined collection.
	{
		public RefCarrierCodeCollection(BusinessObjectFactory factory, string countryCode)
			: base(factory, new ZQuery(RefCarrierCodeSchema.ZZ4_ZZZ_NKDataGrouping, countryCode ?? string.Empty))
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
