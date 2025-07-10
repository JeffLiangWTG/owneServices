using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public class RateImportHelper
	{
		#region Instance

		protected RateImportHelper() { }

		public static RateImportHelper Instance
		{
			get { return helper ?? (helper = new RateImportHelper()); }
		}
		[ThreadStatic]
		static RateImportHelper helper;

		#endregion

		public OrgAddress GetOrgAddress(Xsd.AddressReference addressReference, OrgHeader organisation)
		{
			OrgAddress result = null;

			Xsd.OrgAddress orgAddressValue = null;

			foreach (Xsd.OrgAddress current in addressReference.Organisation.OrganisationDetails.Addresses)
			{
				if (current.Sequence == addressReference.AddressSequenceRef)
				{
					orgAddressValue = current;
					break;
				}
			}

			if (orgAddressValue != null)
			{
				var query = new ZQuery(OrgAddressSchema.OA_Code, orgAddressValue.AddressCode);
				var matchedAddresses = (OrgAddress[])organisation.Addresses.Find(query);
				result = (matchedAddresses.Length > 0) ? matchedAddresses[0] : null;
			}

			return result;
		}

		public AccChargeCode FindChargeCode(ZString xsdValue, IValueObjectImportContext context)
		{
			var trimmedXsdValue = xsdValue.Trim();

			if (!trimmedXsdValue.IsEmpty)
			{
				var chargeCodePK = context.Converter.GetPKFromNKGivenFKType(context.Factory, trimmedXsdValue, ForeignKeyType.ChargeCodeNK, context);

				if (chargeCodePK.IsEmpty)
				{
					var query = new ZQuery(AccChargeCodeSchema.AC_Code, trimmedXsdValue);
					query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
					return context.Factory.LoadTop1<AccChargeCode>(query);
				}
				else
				{
					return context.Factory.Load<AccChargeCode>(chargeCodePK);
				}
			}

			return null;
		}
	}
}

