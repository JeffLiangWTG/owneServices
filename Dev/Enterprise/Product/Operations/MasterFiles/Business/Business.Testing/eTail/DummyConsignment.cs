using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing;

public class DummyConsignment : AutoDummyBizo, Enterprise.Integration.Customs.IConsignmentAddressProvider
{
	public DummyConsignment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public virtual bool ShipperIsOrganisation { get; set; }
	public virtual bool ConsigneeIsOrganisation { get; set; }

	public virtual ZGuid ConsigneeAddressId { get; set; }
	public virtual ZGuid ShipperAddressId { get; set; }
	public virtual string WaybillNumber { get; set; }

	// Consignee Details
	public virtual string ConsigneeName { get; set; }
	public virtual string ConsigneeAddress1 { get; set; }
	public virtual string ConsigneeAddress2 { get; set; }
	public virtual string ConsigneeCity { get; set; }
	public virtual string ConsigneeState { get; set; }
	public virtual string ConsigneePostcode { get; set; }
	public virtual string ConsigneeCountryCode { get; set; }
	public virtual string ConsigneePhone { get; set; }
	public virtual string ConsigneeMobile { get; set; }
	public virtual string ConsigneeFax { get; set; }
	public virtual string ConsigneeEmail { get; set; }

	// Shipper Details
	public virtual string ShipperName { get; set; }
	public virtual string ShipperAddress1 { get; set; }
	public virtual string ShipperAddress2 { get; set; }
	public virtual string ShipperCity { get; set; }
	public virtual string ShipperState { get; set; }
	public virtual string ShipperPostcode { get; set; }
	public virtual string ShipperCountryCode { get; set; }
	public virtual string ShipperPhone { get; set; }
	public virtual string ShipperMobile { get; set; }
	public virtual string ShipperFax { get; set; }
	public virtual string ShipperEmail { get; set; }
}
