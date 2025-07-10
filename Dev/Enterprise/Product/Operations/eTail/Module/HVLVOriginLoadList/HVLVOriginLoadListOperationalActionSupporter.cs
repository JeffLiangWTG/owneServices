using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.eTail.Module
{
	public class HVLVOriginLoadListOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(HVLVOriginLoadList);

		public override BusinessContext BusinessContext => BusinessContext.HVLVOriginLoadList;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.HVLVOriginLoadList;
	}
}
