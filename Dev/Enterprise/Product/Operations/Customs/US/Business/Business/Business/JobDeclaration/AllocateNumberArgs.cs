using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class AllocateNumberArgs
	{
		public int MaxLength;
		public Action<ZPropertyInfo, AllocateNumberExtraValidation> ValidateNumber;
		public GlbBranch Branch;
		public ZString EntryFilerCode;
		public BusinessObjectFactory Factory;
	}
}
