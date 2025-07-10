using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolDGRestrictionsLookups : JobConsolDGRestrictionsLookups
	{
		public ConsolDGRestrictionsLookups(ConsolDGRestrictions parent) : base(parent)
		{
		}

		public CodeDescriptionPairList DGClassLookup => UNDGDataItemLookups.GetDGClassList(Factory);

		public UNDGSubstanceCollection Substance => substance ?? (substance = new UNDGSubstanceCollection(Factory, new ZQuery()));
		UNDGSubstanceCollection substance;
	}
}
