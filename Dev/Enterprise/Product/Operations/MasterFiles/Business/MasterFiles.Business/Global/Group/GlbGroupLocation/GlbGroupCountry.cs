using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupCountry : GlbGroupLocation
	{
		public GlbGroupCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ILocation Location
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, GGL_PortOrCountry); }
		}

		#endregion

#if DEBUG
		#region FillWithValidTestData

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			GGL_PortOrCountry = Factory.LoadTop1<RefCountry>(new ZQuery()).RN_Code;
		}

		#endregion
#endif
	}
}
