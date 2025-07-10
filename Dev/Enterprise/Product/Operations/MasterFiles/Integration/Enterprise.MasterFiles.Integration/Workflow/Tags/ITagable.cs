using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITagable : IBusiness
	{
		ZGuid PK { get; }
		string TablePrefix { get; }
		ICollection<ITagLink> TagLinks { get; }
		ITagable Parent { get; }
		string Description { get; }
	}
}
