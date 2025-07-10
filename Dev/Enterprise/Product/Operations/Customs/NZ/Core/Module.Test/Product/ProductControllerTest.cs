using System;
using Enterprise.Customs.NZ.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(ProductController))]
	sealed class ProductControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		public void TestPluginTabPageCaption()
		{
			AssertEquals("Customs", new ProductController().PluginTabPageCaption.Caption);
		}

		protected override string CountryCode
		{
			get
			{
				return Enterprise.Core.Constants.CountryCodes.NewZealand;
			}
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(OrgSupplierPart);
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(ProductController);
			}
		}
	}
}
