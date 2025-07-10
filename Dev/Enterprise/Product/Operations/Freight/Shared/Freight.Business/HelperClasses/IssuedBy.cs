using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class BillIssuedBy
	{
		public BillIssuedBy(RefUNLOCO portOfLoading, bool preferEnglish = false)
		{
			GlbBranch branch = null;

			if (portOfLoading != null)
			{
				branch = GlbCompany.CurrentCompany.FirstActiveBranchForUnLoco(portOfLoading);
			}

			if (branch != null)
			{
				issuedByOrg = branch.OrgProxy;
			}

			issuedByOrg = issuedByOrg ?? GlbCompany.CurrentCompany.OrgProxy;
			this.preferEnglish = preferEnglish;
		}

		public BillIssuedBy(OrgHeader issuedByOrg, bool preferEnglish = false)
		{
			this.issuedByOrg = issuedByOrg;
			this.preferEnglish = preferEnglish;
		}

		protected readonly OrgHeader issuedByOrg;
		readonly bool preferEnglish;
		AddressDetails AddressDetails => AddressDetails.Get(issuedByOrg?.MainAddress, preferEnglish);

		public virtual ZString Address1
		{
			get
			{
				return AddressDetails?.Address1.ToUpper() ?? ZString.Empty;
			}
		}

		public virtual ZString Address2
		{
			get
			{
				return AddressDetails?.Address2.ToUpper() ?? ZString.Empty;
			}
		}

		public virtual ZString Name
		{
			get { return issuedByOrg != null ? issuedByOrg.OH_FullNameTruncated.ToUpper() : ZString.Empty; }
		}

		public virtual ZString City
		{
			get
			{
				return AddressDetails?.City.ToUpper() ?? ZString.Empty;
			}
		}

		public virtual ZString State
		{
			get
			{
				return AddressDetails?.State.ToUpper() ?? ZString.Empty;
			}
		}

		public virtual ZString PostCode
		{
			get
			{
				return AddressDetails?.Postcode.ToUpper() ?? ZString.Empty;
			}
		}

		public RefUNLOCO UNLOCO
		{
			get { return issuedByOrg != null ? issuedByOrg.UNLOCO : null; }
		}

		public virtual ZString CountryName
		{
			get
			{
				var country = (UNLOCO != null)
					? UNLOCO.GetCountryFromCode(UNLOCO.RL_RN_NKCountryCode)
					: null;

				return country != null
					? country.RN_Desc
					: ZString.Empty;
			}
		}

		public ZString CountryCode
		{
			get
			{
				if (issuedByOrg != null && !CountryName.IsEmpty)
				{
					var country = issuedByOrg.Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Desc, CountryName));
					return country?.RN_Code ?? ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		public ZString ItalianIVA
		{
			get
			{
				if (issuedByOrg != null)
				{
					return issuedByOrg.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.IVA, Core.Constants.CountryCodes.Italy);
				}

				return ZString.Empty;
			}
		}
	}

	public class BillIssuedByAirline : BillIssuedBy
	{
		public BillIssuedByAirline(RefAirline airline) : base(airline != null ? airline.GetCorrespondingCarrierOrganisation() : null)
		{
			this.airline = airline;
		}

		readonly RefAirline airline;

		public override ZString Name
		{
			get { return airline != null ? airline.RM_AirlineName1 : ZString.Empty; }
		}

		public override ZString Address1
		{
			get { return airline != null ? airline.RM_AddressLine1 : ZString.Empty; }
		}

		public override ZString Address2
		{
			get { return airline != null ? airline.RM_AddressLine2 : ZString.Empty; }
		}

		public override ZString City
		{
			get { return airline != null ? airline.RM_AirlineCity : ZString.Empty; }
		}

		public override ZString CountryName
		{
			get { return airline != null ? airline.RM_AirlineCountry : ZString.Empty; }
		}

		public override ZString State
		{
			get { return airline != null ? airline.RM_AirlineState : ZString.Empty; }
		}

		public override ZString PostCode
		{
			get { return airline != null ? airline.RM_AirlinePostalCode : ZString.Empty; }
		}
	}
}
