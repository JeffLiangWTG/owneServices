using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressNumber : AutoJobDocAddressNumber
	{
		public JobDocAddressNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ReadOnly(true)]
		public override ZGuid E2N_E2 { get => base.E2N_E2; set => base.E2N_E2 = value; }

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			yield return JobDocAddressNumberSchema.Constants.E2N_E2;
		}
	}
}
