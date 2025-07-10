using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISPGA : AutoDISPGA
	{
		public DISPGA(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[List(nameof(PGACodeList))]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		public CodeDescriptionPairList PGACodeList
		{
			get { return Factory.GetCachedValue<PGAList>(); }
		}
	}
}
