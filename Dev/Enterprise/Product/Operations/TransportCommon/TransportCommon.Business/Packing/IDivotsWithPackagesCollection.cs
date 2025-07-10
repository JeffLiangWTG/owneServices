using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.TransportCommon.Business
{
	public interface IDivotsWithPackagesCollection : IActiveBusinessObjectCollection
	{
		new DtbTransportInstructionPkgDivot this[int index] { get; }

		void AddPackage(PkgPackage package);
		void RemovePackage(PkgPackage package);
		void AddPackages(IEnumerable<PkgPackage> packages);

		bool Contains(PkgPackage package);

		IEnumerable<PkgPackage> Packages { get; }
		IEnumerable<DtbTransportInstructionPkgDivot> Typed { get; }
	}
}
