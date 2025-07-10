using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusUnderBondWithParentLoader : CusUnderbond
	{
		public CusUnderBondWithParentLoader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override TypeLoaderCollection GetParentLoaders()
		{
			TypeLoaderCollection result = base.GetParentLoaders();
			result.Add(new TypeLoader(typeof(DummyBizoWithUnderbondCollection)));
			return result;
		}
	}
}
