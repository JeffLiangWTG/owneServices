using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocumentForTest : JobRequiredDocument
	{
		public JobRequiredDocumentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public Action AfterOnSaving { get; set; }

		public override void OnSaving()
		{
			base.OnSaving();
			AfterOnSaving?.Invoke();
		}
	}
}
