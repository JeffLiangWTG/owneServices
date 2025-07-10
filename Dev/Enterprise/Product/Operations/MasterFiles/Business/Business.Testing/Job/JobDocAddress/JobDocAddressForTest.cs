using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressForTest : JobDocAddress
	{
		public JobDocAddressForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool UnrestrictedAdditionalAddressInformation_ReadOnlyExposed => UnrestrictedAdditionalAddressInformation_ReadOnly;

		public override bool SupportsDocAddressNumbers => true;

		public void RunPreSaveValidationCore_Exposed()
		{
			base.RunPreSaveValidationCore();
		}

		public Type GetParentTypeExposed(string prefix)
		{
			Type type = base.GetParentType(prefix);
			PrefixToTypeHash["RN"] = typeof(JobDocAddressPersistentParentForTesting);
			return type;
		}
	}
}
