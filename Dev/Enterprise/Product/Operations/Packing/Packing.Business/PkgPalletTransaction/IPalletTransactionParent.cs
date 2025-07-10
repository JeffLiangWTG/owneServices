using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Packing.Business
{
	public interface IPalletTransactionParent
	{
		IDocAddress TransferFrom(string transferType);
		IDocAddress TransferTo(string transferType);
		GlbBranch RelevantBranch { get; }
		ZGuid PK { get; }
		string TablePrefix { get; }
		ZString GetJobDescription(Type contextType = null);
		BusinessObjectFactory Factory { get; }
		IEnumerable<string> JobReferences { get; }
	}
}
