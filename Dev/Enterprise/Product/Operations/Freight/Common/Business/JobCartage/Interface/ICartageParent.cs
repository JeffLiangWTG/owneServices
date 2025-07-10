using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Common.Business
{
	public interface ICartageParent
	{
		IReadOnlyCollection<CartageType> CartageTypes { get; }
		CartageType GetLocalCartageType { get; } //On printing of Cartage Advice, may like to pass in MenuItem to determine Cartage Type

		ZString UniqueConsignmentID { get; }
		ZGuid CartageParentID { get; }
		ZString CartageParentTableCode { get; }
		ZString ServiceLevel { get; }
		ZString OrderReferenceNumber { get; }
		ZGuid BranchPK { get; }
		ControllerID ControllerID { get; }
		ZString WayBillNumber { get; }
		ZString GoodsDescription { get; }
		ZGuid JobHeaderPK { get; }
		ZGuid LocalClientAddressPK { get; }

		ZInt TotalPackages { get; }
		ZString TotalPackType { get; }
		ZDecimal TotalWeight { get; }
		ZString TotalWeightUnit { get; }
		ZDecimal TotalVolume { get; }
		ZString TotalVolumeUnit { get; }

		event EventHandler CartageTypesChanged;

		//Bizo
		bool HasChanges { get; }
		bool IsInDatabase { get; }
		BusinessObjectFactory Factory { get; }
		ZString HumanReadableName { get; }
		IStmALogParent BusinessObjectForRelatedEvents { get; }
		IDocManagerSupport BusinessObjectForRelatedEDocs { get; }

		/// <summary>
		/// This will rebuild the local cartage menu item each time it is clicked. Useful if the Cartage Parent changes depending on context.
		/// WARNING: This will remove Auto Creation of Internal Local Cartages when the TransportCo changes.
		/// </summary>
		bool RebuildLocalCartageMenuOnClick { get; }
		bool UseJobTotals { get; }

		/// <summary>
		/// When a new cartage is saved for the first time
		/// </summary>
		void CartageCreatedAndSaved();
	}
}
