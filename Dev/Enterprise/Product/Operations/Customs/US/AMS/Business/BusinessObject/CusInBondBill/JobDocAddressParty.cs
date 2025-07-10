using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	class JobDocAddressParty : SanitizedACEOceanManifestJobDocAddressWrapper, IParty, IEntity, INotifyPartyContact
	{
		public JobDocAddressParty(JobDocAddress docAddress, ZString entityCode)
			: base(docAddress)
		{
			this.entityCode = entityCode;
			name = E2_CompanyName.Left(35);
			var address = AddressAsASingleLineWithoutCompanyName;
			addressLine1 = address.Left(35);
			addressLine2 = address.SubstringSafe(35, 35);
			addressLine3 = address.SubstringSafe(70, 35);
			telephoneOrTelexNumberOrAddressLine4 = E2_Phone.IsEmpty ? address.SubstringSafe(105, 35) : E2_Phone;
			SetQualifierAndIDCode();
			SetupCommunicationsList();
		}

		protected new JobDocAddress DocAddress
		{
			get { return (JobDocAddress)base.DocAddress; }
		}

		void SetupCommunicationsList()
		{
			var list = new List<Tuple<ZString, ZString>>();
			if (!E2_Phone.IsEmpty)
			{
				list.Add(new Tuple<ZString, ZString>(CommunicationsNumberQualifierList.Codes.Telephone, E2_Phone));
			}
			if (!E2_Mobile.IsEmpty)
			{
				list.Add(new Tuple<ZString, ZString>(CommunicationsNumberQualifierList.Codes.CellularPhone, E2_Mobile));
			}
			if (!E2_Email.IsEmpty && E2_Email.Length <= 25)
			{
				list.Add(new Tuple<ZString, ZString>(CommunicationsNumberQualifierList.Codes.ElectronicMail, E2_Email));
			}
			if (!E2_Fax.IsEmpty)
			{
				list.Add(new Tuple<ZString, ZString>(CommunicationsNumberQualifierList.Codes.Facsimile, E2_Fax));
			}
			communicationsList = list.ToArray();
		}
		Tuple<ZString, ZString>[] communicationsList;

		void SetQualifierAndIDCode()
		{
			if (E2_AddressOverride)
			{
				SetQualifierAndIDCode(E2_GovRegNumType, E2_GovRegNum);
			}
			else
			{
				var org = (OrgHeader)Organisation;
				if (org != null)
				{
					var registrationType = OrgCusCode.CodeTypes.CarrierCode;
					var cusCodes = org.CustomsCodes.Cast<OrgCusCode>();

					switch (entityCode)
					{
						case ACEEntityIDCodeList.Codes.CustomsBroker:
							registrationType = OrgCusCode.USACodeTypes.ABIRoutingCode;
							break;
						case ACEEntityIDCodeList.Codes.ShipTo:
						case ACEEntityIDCodeList.Codes.BookingParty:
							registrationType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
							break;
					}

					if (OrgCusCode.GetPremisesAddressIsAllowed(registrationType, Core.Constants.CountryCodes.UnitedStates) && DocAddress.Address != null)
					{
						cusCodes = cusCodes.Concat(DocAddress.Address.CustomsCodes.Cast<OrgCusCode>());
					}

					var cusCode = cusCodes.FirstOrDefault(x => x.OK_CodeType.EqualsIgnoringCase(registrationType) && x.OK_RN_NKCodeCountry.EqualsIgnoringCase(Core.Constants.CountryCodes.UnitedStates));

					if (cusCode != null)
					{
						SetQualifierAndIDCode(cusCode.OK_CodeType, cusCode.OK_CustomsRegNo);
					}
				}
			}
		}

		void SetQualifierAndIDCode(ZString registrationType, ZString registrationNumber)
		{
			if (registrationType == OrgCusCode.CodeTypes.CarrierCode)
			{
				codeQualifier = CusInBondMoveDetail.SCACOrFIRMSOfSNPQualifier;
				idCode = registrationNumber.Left(4);
			}
			else if (registrationType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem)
			{
				codeQualifier = CusInBondMoveDetail.DUNSQualifier;
				idCode = registrationNumber.Left(17);
			}
			else if (registrationType == OrgCusCode.USACodeTypes.ABIRoutingCode)
			{
				codeQualifier = CusInBondMoveDetail.ABIRoutingCodeSNPQualifier;
				idCode = registrationNumber.Left(17);
			}
		}

		#region IParty Members

		ZString IParty.Name
		{
			get { return name; }
		}
		readonly ZString name;

		ZString IParty.AddressLine1
		{
			get { return addressLine1; }
		}
		readonly ZString addressLine1;

		ZString IParty.AddressLine2
		{
			get { return addressLine2; }
		}
		readonly ZString addressLine2;

		ZString IParty.AddressLine3
		{
			get { return addressLine3; }
		}
		readonly ZString addressLine3;

		ZString IParty.TelephoneOrTelexNumberOrAddressLine4
		{
			get { return telephoneOrTelexNumberOrAddressLine4; }
		}
		readonly ZString telephoneOrTelexNumberOrAddressLine4;

		#endregion

		#region IEntity Members

		ZString IEntity.EntityCode
		{
			get { return entityCode; }
		}
		readonly ZString entityCode;

		ZString IEntity.EntityName
		{
			get { return E2_CompanyName.Left(35); } // Note: It needs 35 characters
		}

		ZString IEntity.CodeQualifier
		{
			get { return codeQualifier; }
		}
		ZString codeQualifier;

		ZString IEntity.IDCode
		{
			get { return idCode; }
		}
		ZString idCode;

		#endregion

		#region IEntityAddress Members

		ZString IEntityAddress.AddressLine1
		{
			get { return E2_Address1.Left(35); }
		}

		ZString IEntityAddress.AddressLine1Part2
		{
			get { return E2_Address1.SubstringSafe(35, 35); }
		}

		ZString IEntityAddress.AddressLine2
		{
			get { return E2_Address2.Left(35); }
		}

		ZString IEntityAddress.AddressLine2Part2
		{
			get { return E2_Address2.SubstringSafe(35, 35); }
		}

		ZString IEntityAddress.CityName
		{
			get { return E2_City.Left(19); }
		}

		ZString IEntityAddress.StateProvince
		{
			get { return CountryCode == Core.Constants.CountryCodes.UnitedStates ? E2_State.Left(2) : ZString.Empty; }
		}

		ZString IEntityAddress.PostalCode
		{
			get { return CountryCode == Core.Constants.CountryCodes.UnitedStates ? E2_Postcode.Left(9) : ZString.Empty; }
		}

		ZString IEntityAddress.CountryCode
		{
			get { return CountryCode; }
		}

		INotifyPartyContact IEntityAddress.AdminContact
		{
			get { return this; }
		}

		#endregion

		#region INotifyPartyContact Members

		ZString INotifyPartyContact.ContactName
		{
			get { return E2_Contact; }
		}

		ZString INotifyPartyContact.CommNumberQualifier
		{
			get { return communicationsList.Length > 0 ? communicationsList[0].Item1 : ZString.Empty; }
		}

		ZString INotifyPartyContact.CommunicationsNumber
		{
			get { return communicationsList.Length > 0 ? communicationsList[0].Item2 : ZString.Empty; }
		}

		ZString INotifyPartyContact.CommNumberQualifier2
		{
			get { return communicationsList.Length > 1 ? communicationsList[1].Item1 : ZString.Empty; }
		}

		ZString INotifyPartyContact.CommunicationsNumber2
		{
			get { return communicationsList.Length > 1 ? communicationsList[1].Item2 : ZString.Empty; }
		}

		#endregion

		#region Sanitized wrapper properties

		public ZString E2_Mobile
		{
			get { return GetSanitizedString(DocAddress.E2_Mobile); }
		}

		public ZString E2_Contact
		{
			get { return GetSanitizedString(DocAddress.E2_Contact); }
		}

		public ZString E2_Email
		{
			get { return GetSanitizedString(DocAddress.E2_Email); }
		}

		public ZString AddressAsASingleLineWithoutCompanyName
		{
			get { return GetSanitizedString(DocAddress.AddressAsASingleLineWithoutCompanyName); }
		}

		#endregion
	}
}
