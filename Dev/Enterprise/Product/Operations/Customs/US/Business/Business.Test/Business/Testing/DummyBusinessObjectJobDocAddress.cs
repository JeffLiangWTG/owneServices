using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DummyBusinessObjectJobDocAddress : DummyBusinessObject, IAddressDetails, IPGAContactDetails
	{
		public DummyBusinessObjectJobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString DummyStringValue { get; set; }

		public IAddressDetails DummyCompanyAddress { get; set; }

		public bool HasRealAddress { get; set; }

		protected override DummyBizoValidation GetNewValidation() => new DummyBusinessObjectJobDocAddressValidation(this);

		ZString IAddressDetails.CompanyName => DummyStringValue;

		ZString IAddressDetails.ContactName => DummyStringValue;

		ZString IAddressDetails.Phone => DummyStringValue;

		ZString IAddressDetails.Fax => DummyStringValue;

		ZString IPGAContactDetails.Fax => DummyStringValue;

		ZString IAddressDetails.Email => DummyStringValue;

		ZString IAddressDetails.AddressLine1 => DummyStringValue;

		ZString IAddressDetails.AddressLine2 => DummyStringValue;

		ZString IAddressDetails.City => DummyStringValue;

		ZString IAddressDetails.State => DummyStringValue;

		ZString IAddressDetails.PostCode => DummyStringValue;

		ZString IAddressDetails.Country => DummyStringValue;

		ZString IPGAContactDetails.Name => DummyStringValue;

		ZString IPGAContactDetails.PhoneNumber => DummyStringValue;

		ZString IPGAContactDetails.EmailAddress => DummyStringValue;

		IAddressDetails IPGAContactDetails.CompanyAddress => DummyCompanyAddress;
	}
}
