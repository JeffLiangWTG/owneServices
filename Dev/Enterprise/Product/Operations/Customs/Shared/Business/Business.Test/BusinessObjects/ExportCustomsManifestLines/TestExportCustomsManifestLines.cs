using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TestExportCustomsManifestLines : ExportCustomsManifestLines
	{
		public TestExportCustomsManifestLines(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool ShouldThrowExceptionOnSaving;
		public override void OnSaving()
		{
			base.OnSaving();
			if (ShouldThrowExceptionOnSaving)
			{
				throw new Exception("Testing");
			}
		}
	}
}
