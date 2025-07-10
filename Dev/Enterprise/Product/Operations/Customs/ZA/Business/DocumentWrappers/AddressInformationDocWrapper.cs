using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Edifact.D96B.Segments;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class AddressInformationDocWrapper : NonPersistentBusinessObject, IAddressInformation
	{
		public AddressInformationDocWrapper(IAddressInformation input)
		{
			if (input != null)
			{
				OrganizationCode = input.OrganizationCode;
				OrganizationCodeQualifier = input.OrganizationCodeQualifier;
				Name = input.Name;
				Address = input.Address;
				City = input.City;
				PostCode = input.PostCode;
				VATRegistrationNo = input.VATRegistrationNo;
			}
		}

		public AddressInformationDocWrapper(IAddressInformation input, IAddressInformation fallbackInput)
		{
			OrganizationCode = Coalesce(input?.OrganizationCode, fallbackInput?.OrganizationCode);
			OrganizationCodeQualifier = Coalesce(input?.OrganizationCodeQualifier, fallbackInput?.OrganizationCodeQualifier);
			Name = Coalesce(input?.Name, fallbackInput?.Name);

			bool hasAddress = !IsEmpty(input?.Address, input?.City, input?.PostCode);

			Address = hasAddress ? (input?.Address ?? ZString.Empty) : (fallbackInput?.Address ?? ZString.Empty);
			City = hasAddress ? (input?.City ?? ZString.Empty) : (fallbackInput?.City ?? ZString.Empty);
			PostCode = hasAddress ? (input?.PostCode ?? ZString.Empty) : (fallbackInput?.PostCode ?? ZString.Empty);

			VATRegistrationNo = Coalesce(input?.VATRegistrationNo, fallbackInput?.VATRegistrationNo);
		}

		bool IsEmpty(params ZString?[] values)
		{
			foreach (ZString? value in values)
			{
				if (value != (ZString?)null && !value.Value.IsEmpty)
				{
					return false;
				}
			}
			return true;
		}

		ZString Coalesce(ZString? value1, ZString? value2)
		{
			if (value1 == (ZString?)null && value2 == (ZString?)null)
			{
				return ZString.Empty;
			}
			if (value1 != (ZString?)null && !value1.Value.IsEmpty)
			{
				return value1.Value;
			}
			if (value2 != (ZString?)null && !value2.Value.IsEmpty)
			{
				return value2.Value;
			}
			return ZString.Empty;
		}

		public AddressInformationDocWrapper(SegmentGroup6 sg6)
		{
			var input = sg6?.NAD[0];
			if (input != null)
			{
				OrganizationCode = input.PartyIdentificationDetails.PartyIdIdentification;
				OrganizationCodeQualifier = input.PartyIdentificationDetails.CodeListQualifier.ToString();
				Name = input.PartyName.PartyName1;
				var streetField = input.Street;
				Address = streetField.StreetAndNumberPOBox1 + streetField.StreetAndNumberPOBox2 + streetField.StreetAndNumberPOBox3 + streetField.StreetAndNumberPOBox4;
				City = input.CityName;
				PostCode = input.PostcodeIdentification;
				VATRegistrationNo = sg6.RFF?.Cast<RFFSegment>()?.FirstOrDefault(rff => rff.Reference.ReferenceQualifier == ReferenceQualifierList.VatRegistrationNumber)?.Reference?.ReferenceNumber ?? ZString.Empty;
			}
		}

		public ZString OrganizationCode { get; private set; }

		public ZString OrganizationCodeQualifier { get; private set; }

		public ZString Name { get; private set; }

		public ZString Address { get; private set; }

		public ZString City { get; private set; }

		public ZString PostCode { get; private set; }

		public ZString VATRegistrationNo { get; private set; }
	}
}
