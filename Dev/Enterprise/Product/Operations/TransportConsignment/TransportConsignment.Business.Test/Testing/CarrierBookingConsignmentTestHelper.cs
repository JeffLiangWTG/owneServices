#if DEBUG
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class CarrierBookingTestHelper : TransportCommonTestHelper
	{
		public CarrierBookingTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Consignment

		public DtbCarrierBookingConsignment CreateConsignment()
		{
			return CreateConsignment("LTC001");
		}

		public DtbCarrierBookingConsignment CreateConsignment(ZString jobID)
		{
			return CreateConsignment(jobID, ConsignmentStatuses.Codes.Booked);
		}

		public DtbCarrierBookingConsignment CreateConsignment(ZString jobID, ZString status)
		{
			return CreateConsignment(Constants.CartageDirection.Local, status, 1, Constants.Length.Kilometres, jobID);
		}

		public DtbCarrierBookingConsignment CreateConsignment(ZString direction, ZString status, ZDecimal distance, ZString distanceUnit, ZString jobID)
		{
			return CreateConsignment(direction, status, distance, distanceUnit, jobID, ZString.Empty);
		}

		public DtbCarrierBookingConsignment CreateConsignment(ZString direction, ZString status, ZDecimal distance, ZString distanceUnit, ZString jobID, ZString connoteNumber)
		{
			var consignment = Factory.New<DtbCarrierBookingConsignment>();
			consignment.LTC_Direction = direction;
			consignment.LTC_Distance = distance;
			consignment.LTC_DistanceUnit = distanceUnit;
			consignment.LTC_Status = status;
			consignment.LTC_JobID = jobID;
			consignment.LTC_ConnoteNumber = connoteNumber;

			return consignment;
		}

		#endregion

		#region ConsignmentAddress

		public DtbConsignmentAddress CreateConsignmentAddress()
		{
			var consignment = CreateConsignment();

			return CreateConsignmentAddress(consignment);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(ZString addressType)
		{
			var consignment = CreateConsignment();

			return CreateConsignmentAddress(consignment, addressType);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbCarrierBookingConsignment consignment)
		{
			return CreateConsignmentAddress(consignment, ZString.Empty);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbCarrierBookingConsignment consignment, ZString addressType)
		{
			return CreateConsignmentAddress(consignment, addressType, null);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbCarrierBookingConsignment consignment, ZString addressType, OrgAddress address)
		{
			return CreateConsignmentAddressWithAutoSequence(consignment, addressType, address, "PIC");
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbCarrierBookingConsignment consignment, ZString addressType, ZInt sequence, ZString orgType)
		{
			var consignmentAddress = CreateConsignmentAddress(consignment, addressType, null, ConsignmentAddressStatus.Codes.Allocated, sequence);

			return consignmentAddress;
		}

		public DtbConsignmentAddress CreateConsignmentAddressWithAutoSequence(DtbCarrierBookingConsignment consignment, ZString addressType, OrgAddress address, ZString addressStatus, DocAddressType docAddressType = DocAddressType.LocalCartageCFS)
		{
			var sequence = (consignment == null ? 1 : consignment.Addresses.Select(ca => ca.LTS_Sequence).OrderByDescending(s => s).FirstOrDefault() + 1);
			return CreateConsignmentAddress(consignment, addressType, address, addressStatus, sequence, docAddressType);
		}

		public DtbConsignmentAddress CreateConsignmentAddress(DtbCarrierBookingConsignment consignment, ZString addressType, OrgAddress address, ZString addressStatus, ZInt sequence, DocAddressType docAddressType = DocAddressType.LocalCartageCFS)
		{
			var consignmentAddress = Factory.New<DtbConsignmentAddress>();
			consignmentAddress.LTS_LTC_Consignment = consignment != null ? consignment.PK : ZGuid.Empty;
			consignmentAddress.LTS_InstructionType = addressType;

			consignmentAddress.LTS_Sequence = sequence;
			consignmentAddress.LTS_Status = addressStatus;

			if (address != null)
			{
				consignmentAddress.DocAddresses.FindOrCreateWithDocAddressType(docAddressType).E2_OA_Address = address.PK;
			}

			return consignmentAddress;
		}

		#endregion

		#region CreateOrganisation

		public OrgHeader CreateOrganisation(string orgCode, string closestPortUNLOCO)
		{
			var header = CreateOrganisation(orgCode);
			header.OH_RL_NKClosestPort = closestPortUNLOCO;
			return header;
		}

		#endregion

		#region CreateOrgAddress

		public OrgAddress CreateOrgAddress(OrgHeader header, string portCode, string addressCode = "", decimal? latitude = null, decimal? longitude = null)
		{
			var address = header.Addresses.AddNew();
			address.Address1 = "Test";
			address.AddressCode = addressCode;
			address.OA_RL_NKRelatedPortCode = portCode;
			if (latitude != null)
			{
				address.OA_Latitude = new ZDecimal(latitude);
			}
			if (longitude != null)
			{
				address.OA_Longitude = new ZDecimal(longitude);
			}
			return address;
		}

		#endregion

		#region Package

		public PkgPackage CreatePackage(DtbCarrierBookingConsignment consignment, ZDecimal weight, ZDecimal volume, int qty = 1)
		{
			return CreatePackage(consignment, PackingRegistry.Instance.OuterPackageUnit.Value, weight, volume, qty);
		}

		public PkgPackage CreatePackage(DtbCarrierBookingConsignment consignment, ZString packageID, ZString packType)
		{
			return CreatePackage(consignment, packType, 1m, 1m, 1, packageID);
		}

		public PkgPackage CreateChildPackage(PkgPackage package, ZString packageID, ZString packType)
		{
			var childPackage = package.Packages.AddNew();
			childPackage.KP_PackageQty = 1;
			childPackage.KP_F3_NKPackType = packType;
			childPackage.KP_PackageID = packageID;
			return childPackage;
		}

		public PkgPackage CreatePackage(DtbCarrierBookingConsignment consignment, ZString packType, ZDecimal weight, ZDecimal volume, int qty = 1, string packageID = "")
		{
			var result = consignment.PackageJob.Packages.AddNew();
			result.KP_PackageQty = qty;
			result.KP_F3_NKPackType = packType;
			result.KP_Weight = weight;
			result.KP_Volume = volume;
			result.KP_PackageID = packageID;

			return result;
		}

		#endregion
	}
}
#endif
