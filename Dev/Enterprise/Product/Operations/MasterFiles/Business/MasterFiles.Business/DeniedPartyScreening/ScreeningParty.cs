using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business
{
	public class ScreeningParty : IScreeningParty, IComplianceLocation
	{
		public static ScreeningParty ConvertFromComplianceParty(BusinessObject parent, string description, BusinessObject complianceParty, INaturalPerson naturalPerson)
		{
			ScreeningParty result;
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			if (description.IsNullOrEmpty())
			{
				throw new ArgumentException($"{nameof(description)} cannot be null or empty");
			}

			if (complianceParty is OrgHeader org)
			{
				result = new ScreeningParty(parent, description, org);
			}
			else if (complianceParty is IScreeningPartyForVessel vessel)
			{
				result = new ScreeningParty(parent, description, vessel);
			}
			else if (complianceParty is JobDocAddress docAddress)
			{
				result = new ScreeningParty(parent, description, docAddress);
			}
			else if (complianceParty is RefVessel refVessel)
			{
				result = new ScreeningParty(parent, description, refVessel);
			}
			else if (!string.IsNullOrEmpty(naturalPerson?.Name))
			{
				result = new ScreeningParty(parent, description, naturalPerson.Name, naturalPerson.Address1, naturalPerson.Address2, naturalPerson.City, naturalPerson.PostCode, naturalPerson.State, naturalPerson.Country, naturalPerson.AdditionalAddressLine, naturalPerson.IsConsideredAsOrganization);
			}
			else
			{
				result = new ScreeningParty(parent, description);
			}

			return result;
		}

		public ScreeningParty(BusinessObject parent, string description, IScreeningPartyForVessel screeningPartyForVessel)
			: this(parent, description)
		{
			NotLinkedVessel = screeningPartyForVessel;
			originalParty = screeningPartyForVessel as BusinessObject;
			if (screeningPartyForVessel != null)
			{
				Code = screeningPartyForVessel.Code;
				CurrentScreeningStatus = screeningPartyForVessel.CurrentScreeningStatus;
				CalculateScreeningStatusIsValid();
				Key = (screeningPartyForVessel as BusinessObject).PK;
				IsActive = true;
			}
		}

		public ScreeningParty(BusinessObject parent, string description, OrgHeader header)
			: this(parent, description)
		{
			Header = header;
			originalParty = header;
			if (header != null)
			{
				Code = header.OH_FullNameTruncated;
				OrgCode = header.OH_Code;
				CurrentScreeningStatus = header.OH_ScreeningStatus;
				CalculateScreeningStatusIsValid();
				Key = header.PK;
				IsActive = header.OH_IsActive;
			}
		}

		public ScreeningParty(BusinessObject parent, string description, RefCountry country)
			: this(parent, description)
		{
			Country = country;
			originalParty = country;
			if (country != null)
			{
				Code = country.RN_Code;
				Key = country.PK;
				IsActive = country.IsActive;
				CurrentScreeningStatus = ScreeningStatusesList.Codes.Unknown; //always perform stand-alone screening
				CalculateScreeningStatusIsValid();
			}
		}

		public ScreeningParty(BusinessObject parent, string description, JobDocAddress docAddress)
			: this(parent, description)
		{
			DocAddress = docAddress;
			originalParty = docAddress;
			if (docAddress != null && !docAddress.IsEmpty)
			{
				Code = docAddress.E2_CompanyNameTruncated;

				var org = docAddress.Organisation;
				OrgCode = org != null ? org.OH_Code : ZString.Empty;

				if (!docAddress.E2_AddressOverride && docAddress.Address != null && docAddress.Address.Header != null)
				{
					Key = docAddress.Address.OA_OH;
					CurrentScreeningStatus = docAddress.Address.Header.OH_ScreeningStatus;
				}
				else
				{
					Key = docAddress.PK;
					CurrentScreeningStatus = docAddress.E2_ScreeningStatus;
				}
				CalculateScreeningStatusIsValid();
				IsActive = org != null ? org.OH_IsActive : ZBool.True;
			}
		}

		public ScreeningParty(BusinessObject parent, string description, RefVessel vessel)
			: this(parent, description)
		{
			Vessel = vessel;
			originalParty = vessel;
			if (vessel != null)
			{
				Code = vessel.RV_Code;
				CurrentScreeningStatus = vessel.RV_ScreeningStatus;
				CalculateScreeningStatusIsValid();
				Key = vessel.PK;
				IsActive = vessel.RV_IsActive;
			}
		}

		public ScreeningParty(BusinessObject parent, string description, string name, string address1, string address2, string city, string postcode, string state, string country, string additionalAddressLine, bool shouldBeConsideredAsOrganization = false)
			: this(parent, description)
		{
			Key = parent.PK;
			Code = name;
			CurrentScreeningStatus = (parent as IScreeningStatusProvider).ScreeningStatus; //null check
			IsActive = true;
			screeningEntity = parent;
			NaturalPerson = new NaturalPerson(name, address1, address2, city, state, postcode, country, additionalAddressLine, parent.PK.ToGuid(), shouldBeConsideredAsOrganization);
		}

		ScreeningParty(BusinessObject parent, string description)
		{
			Parent = parent;
			originalDescription = description;
			Description = GetPartyDescriptionWithCountryCodeIfParentIsDeclaration(parent, description);

			Parents.Add(parent);

			string parentCode = CodePropertyAttribute.CodeFromBusinessObject(Parent);
			if (!string.IsNullOrEmpty(parentCode))
			{
				parentCode += ": ";
			}

			ParentsDescription += parentCode + Description;
		}

		public static string GetPartyDescriptionWithCountryCodeIfParentIsDeclaration(BusinessObject bizO, string description)
		{
			if (bizO is IBaseJobDeclaration declaration && description != null)
			{
				var branch = declaration.Factory.Load<GlbBranch>(declaration.JE_GB);

				if (branch != null && !branch.GB_RN_NKCountryCode.IsEmpty)
				{
					description = branch.GB_RN_NKCountryCode + " - " + description;
				}
			}

			return description;
		}

		public void AddParent(BusinessObject parentToAdd)
		{
			if (parentToAdd != null)
			{
				Parents.Add(parentToAdd);
			}
		}

		public void CalculateScreeningStatusIsValid()
		{
			IsCurrentScreeningStatusValid = CurrentScreeningStatus != ScreeningStatusesList.Codes.Unknown &&
											CurrentScreeningStatus != ScreeningStatusesList.Codes.RequiresReview &&
											CurrentScreeningStatus != ScreeningStatusesList.Codes.NotScreened;
		}

		public BusinessObject Parent { get; }
		public string Description { get; }
		readonly string originalDescription;
		readonly BusinessObject originalParty;

		public readonly OrgHeader Header;
		public readonly JobDocAddress DocAddress;
		public readonly RefVessel Vessel;
		public readonly IScreeningPartyForVessel NotLinkedVessel;
		public readonly RefCountry Country;
		public readonly NaturalPerson NaturalPerson;

		INaturalPerson IScreeningParty.NaturalPerson => NaturalPerson;

		public ZGuid Key { get; }
		public readonly ZString Code;
		public readonly ZString OrgCode;

		public readonly ZString CurrentScreeningStatus;
		public bool IsCurrentScreeningStatusValid { get; set; }
		public readonly ZBool IsActive;

		/// <summary>
		/// ℹ️ Retrieves the primary key of the job associated with the entity.
		/// </summary>
		public ZGuid AssociatedJobPK { get; internal set; }

		/// <summary>
		/// ℹ️ Associates this entity with the specified job by setting <see cref="AssociatedJobPK"/> to the job's primary key,
		/// if the entity <see cref="Code"/> is not empty and the job is not null.
		/// ✅ Use Case: Shipment attached to a declaration, or a free text vessel name derived from the transport info.
		/// </summary>
		/// <param name="job">The job entity to associate with.</param>
		public void LinkEntityToAssociatedJobIfApplicable(BusinessObject job)
		{
			if (!Code.IsEmpty && job is not null)
			{
				AssociatedJobPK = job.PK;
			}
		}

		public List<BusinessObject> Parents
		{
			get { return fParents ?? (fParents = new List<BusinessObject>()); }
		}
		List<BusinessObject> fParents;

		public ZString ParentsDescription { get; set; }

		public ZString CurrentScreeningStatusDescription
		{
			get
			{
				var list = new ScreeningStatusesList();
				return list.GetDescriptionFromCode(CurrentScreeningStatus);
			}
		}

		public ZString Summary
		{
			get
			{
				return ZString.Format("{0} : {1}{2}{3} ({4})", CurrentScreeningStatusDescription, OrgCode, OrgCode.IsEmpty ? "" : ", ", Code, Description);
			}
		}

		public BusinessObject ScreeningEntity
		{
			get
			{
				if (screeningEntity == null)
				{
					if (Header != null)
					{
						screeningEntity = Header;
					}
					else if (DocAddress != null)
					{
						if (DocAddress.E2_AddressOverride)
						{
							screeningEntity = DocAddress;
						}
						else if (DocAddress.Address != null)
						{
							screeningEntity = DocAddress.Address.Header;
						}
					}
					else if (Vessel != null)
					{
						screeningEntity = Vessel;
					}
					else if (NotLinkedVessel != null)
					{
						screeningEntity = NotLinkedVessel as BusinessObject;
					}
					else if (Country != null)
					{
						screeningEntity = Country;
					}
				}

				return screeningEntity;
			}
		}

		BusinessObject IScreeningParty.Parent => Parent;

		ZString IScreeningParty.Description => originalDescription;

		public BusinessObject Party => originalParty;

		#region IComplianceLocation

		BusinessObject IComplianceLocation.Country => Country;

		ZString IComplianceLocation.Code => Country.RN_Code;

		ZString IComplianceLocation.LocationDescription => Country.RN_Desc;

		ZBool IComplianceLocation.IsSanctioned => Country.RN_IsSanctioned;

		#endregion

		BusinessObject screeningEntity;
	}

	public class NaturalPerson : INaturalPerson
	{
		public NaturalPerson(string name, string address1, string address2, string city, string state, string postCode, string country, string additionalAddressLine, Guid businessEntityKey, bool shouldBeConsideredAsOrganization = false)
		{
			Name = name;
			Address1 = address1;
			Address2 = address2;
			City = city;
			State = state;
			PostCode = postCode;
			Country = country;
			AdditionalAddressLine = additionalAddressLine;
			BusinessEntityKey = businessEntityKey;
			HashID = (name + address1 + address2 + city + state + postCode + country + additionalAddressLine).GetHashCode();
			IsConsideredAsOrganization = shouldBeConsideredAsOrganization;
		}

		public string Name { get; }
		public string Address1 { get; }
		public string Address2 { get; }
		public string City { get; }
		public string State { get; }
		public string PostCode { get; }
		public string Country { get; }
		public string AdditionalAddressLine { get; }
		public Guid BusinessEntityKey { get; }
		public int HashID { get; }
		public bool IsConsideredAsOrganization { get; }
	}
}
