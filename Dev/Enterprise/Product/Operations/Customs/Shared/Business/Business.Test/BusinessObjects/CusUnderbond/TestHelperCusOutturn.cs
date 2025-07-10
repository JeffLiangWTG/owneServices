using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TestHelperCusOutturn : CusOutturn
	{
		public TestHelperCusOutturn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type CusUnderbondType
		{
			get { return typeof(CusUnderbondThatLinksToDummyBizo); }
		}

		protected override TypeLoaderCollection GetParentLoaders()
		{
			TypeLoaderCollection result = base.GetParentLoaders();
			result.Add(new TypeLoader(typeof(OutturnableDummy)));
			return result;
		}
	}
}
