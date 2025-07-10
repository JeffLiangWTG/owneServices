using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Business
{
	public interface IHVLVConsignmentCollectionParent : IBusiness, IStmALogProvider
	{
		IHVLVConsignmentCollection Consignments { get; }
		HVLVConsignmentFilteredCollectionView ConsignmentsFilteredView { get; }
		IDisposable SetIsDataBinding();
		ZBool ConsignmentsNotLoaded { get; }
		ZPropertyInfo ConsignmentsNotLoadedInfo { get; }
	}
}
