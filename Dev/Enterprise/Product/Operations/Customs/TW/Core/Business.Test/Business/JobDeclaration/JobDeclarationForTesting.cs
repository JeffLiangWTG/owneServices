using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldThrowException;
		public void AllocateNextEntryNumberForTest()
		{
			AllocateNextEntryNumber();
		}

		public ZString LocalCurrencyCodeCoreExposed => LocalCurrencyCodeCore;
		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (ShouldThrowException)
			{
				throw new ApplicationException("intended");
			}
		}
	}
}
