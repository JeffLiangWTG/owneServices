using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.TransitWarehouse;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class DummyBizOWithPackLines : DummyBusinessObject, ITransitWarehouseParent
	{
		public DummyBizOWithPackLines(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			TransportCompanyAddressPK = ZGuid.Empty;
		}

		public string JobNumber = "SHP1234";

		string ITransitWarehouseParent.JobNumber => JobNumber;

		public ZString ConsignorCompanyName { get; set; }

		ZString ITransitWarehouseParent.ConsignorCompanyName => ConsignorCompanyName;

		public ZString ConsigneeCompanyName { get; set; }

		ZString ITransitWarehouseParent.ConsigneeCompanyName => ConsigneeCompanyName;

		public Action<IEnumerable<ITransitPackage>> AttachPackagesCalled;
		public Action<IEnumerable<ITransitPackage>> RemovePackagesCalled;

		void ITransitWarehouseParent.AttachPackages(ITransitPackage[] transitPackages)
		{
			AttachPackagesCalled(transitPackages);
		}

		void ITransitWarehouseParent.RemovePackages(ITransitPackage[] packagesToRemove)
		{
			RemovePackagesCalled(packagesToRemove);
		}

		void ITransitWarehouseParent.SetTransportCompany(ZGuid transportCompanyAddressPK)
		{
			TransportCompanyAddressPK = transportCompanyAddressPK;
		}
		public ZGuid TransportCompanyAddressPK;

		public ZGuid GetPickupCFSOrgAddressPK()
		{
			return TransitWarehouseAddressPK;
		}

		ZGuid ITransitWarehouseParent.GetPickupCFSOrgAddressPK()
		{
			return TransitWarehouseAddressPK;
		}

		SortedList<int, ZGuid> ITransitWarehouseParent.GetOrderedCFSOrgAddressPKs()
		{
			return CFSAddressPksInOrder ?? new SortedList<int, ZGuid>();
		}

		public ZString GetPreAttachingValidationMessage(ITransitPackage[] packagesToAttach)
		{
			return PreAttachingValidationMessage;
		}

		public ZBool CanAttachPackages(out ZString errorMessage)
		{
			errorMessage = AdditionalErrorMessage;
			return errorMessage.IsEmpty;
		}

		public ZBool CanDetachPackages(out ZString errorMessage)
		{
			errorMessage = AdditionalErrorMessage;
			return errorMessage.IsEmpty;
		}

		public ZString AdditionalErrorMessage { get; set; } = ZString.Empty;
		public ZString PreAttachingValidationMessage { get; set; }
		public ZGuid TransitWarehouseAddressPK;
		public SortedList<int, ZGuid> CFSAddressPksInOrder;
	}
}
