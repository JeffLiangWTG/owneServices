using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public class ContainerSummaryRow : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string Status = "Status";
			public const string StatusDescription = "StatusDescription";
			public const string Orders = "Orders";
			public const string Packages = "Packages";
			public const string Shipments = "Shipments";
			public const string Containers = "Containers";
		}

		#endregion

		public ContainerSummaryRow(ZString code, ZString description)
		{
			status = code;
			statusDescription = description;
		}

		public ContainerSummaryRow()
		{
		}

		public ContainerSummaryRow(ICodeDescription codeDesc)
		{
			status = codeDesc.Code;
			statusDescription = codeDesc.Description;
		}

		#region Status

		public ZString Status
		{
			get { return status; }
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(Schema.Status); }
		}

		readonly ZString status;

		#endregion

		#region Status Description

		public ZString StatusDescription
		{
			get { return statusDescription; }
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.StatusDescription); }
		}

		readonly ZString statusDescription;

		#endregion

		#region Orders

		public ZInt Orders
		{
			get { return orders; }
			set
			{
				orders = value;
				OrdersInfo.RefreshBinding();
			}
		}

		ZInt orders;

		public ZPropertyInfo OrdersInfo
		{
			get { return GetZPropertyInfo(nameof(Orders)); }
		}

		#endregion

		#region Packages

		public ZInt Packages
		{
			get { return packages; }
			set
			{
				packages = value;
				PackagesInfo.RefreshBinding();
			}
		}

		ZInt packages;

		public ZPropertyInfo PackagesInfo
		{
			get { return GetZPropertyInfo(Schema.Packages); }
		}

		#endregion

		#region Shipments

		public ZInt Shipments
		{
			get { return shipments; }
			set
			{
				shipments = value;
				ShipmentsInfo.RefreshBinding();
			}
		}

		ZInt shipments;

		public ZPropertyInfo ShipmentsInfo
		{
			get { return GetZPropertyInfo(Schema.Shipments); }
		}

		#endregion

		#region Containers

		public ZInt Containers
		{
			get { return containers; }
			set
			{
				containers = value;
				ContainersInfo.RefreshBinding();
			}
		}

		ZInt containers;

		public ZPropertyInfo ContainersInfo
		{
			get { return GetZPropertyInfo(nameof(Containers)); }
		}

		#endregion

	}
}
