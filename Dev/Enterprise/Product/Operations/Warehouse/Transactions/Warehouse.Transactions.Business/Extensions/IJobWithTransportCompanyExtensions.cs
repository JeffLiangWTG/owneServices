using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class IJobWithTransportCompanyExtensions
	{
		#region Carrier Service Level

		// tested in WhsDocketTest
		internal static OrgCarrierServiceLevel GetCarrierServiceLevel<T>(this T jobWithTransportCompany, ZString carrierServiceLevel)
			where T : IDocAddresses, IJobWithTransportCompany
		{
			var transportCoDocAddress = jobWithTransportCompany.LoadJobDocAddressQuickly(TransportCoConstants.AddressType);
			return transportCoDocAddress?.GetOrganisation()?.MiscServ.CarrierServiceLevels.Cast<OrgCarrierServiceLevel>().FirstOrDefault(s => s.PL_Code.EqualsIgnoringCase(carrierServiceLevel));
		}

		// tested in WhsDocketLookupsTest
		internal static OrgCarrierServiceLevelCollection GetCarrierServiceLevels(this IJobWithTransportCompany jobWithTransportCompany)
		{
			OrgCarrierServiceLevelCollection result = null;

			var transportCo = jobWithTransportCompany.GetTransportCo();
			if (transportCo != null)
			{
				result = new OrgCarrierServiceLevelCollection(transportCo.MiscServ, false);
				result.Load();
			}

			return result;
		}

		#endregion

		#region TransportCo DocAddress

		// tested in WhsDocketTest
		internal static JobDocAddress GetTransportCoDocAddress<T>(this T jobWithTransportCompany, ref JobDocAddress transportCompanyDocAddress)
			where T : BusinessObject, IJobWithTransportCompany, IDocAddresses
		{
			if (transportCompanyDocAddress == null || transportCompanyDocAddress.IsDeleted)
			{
				var requirement = jobWithTransportCompany.GetDocAddressRequirement(TransportCoConstants.AddressType);
				transportCompanyDocAddress = jobWithTransportCompany.DocAddresses.FindOrCreateWithRequirement(requirement);

				Func<bool> readOnly = () => jobWithTransportCompany.ReadOnly || jobWithTransportCompany.GetTransportCoDocAddressReadOnly();
				transportCompanyDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(readOnly);
			}

			return transportCompanyDocAddress;
		}

		// tested in WhsDocketTest
		internal static JobDocAddressRequirement GetTransportCoRequirement<T>(this T jobWithTransportCompany, DocAddressType addressType)
			where T : BusinessObject, IJobWithTransportCompany
		{
			return addressType == TransportCoConstants.AddressType
				? jobWithTransportCompany.Factory.GetCachedValue("TransportCoRequirement|" + jobWithTransportCompany.PK, () => TransportCoConstants.Requirement)
				: null;
		}

		#endregion

		#region GetTransportCoOrganisationPropertyInfo

		// tested in WhsDocketRatingAdapterTest
		internal static ZPropertyInfo GetTransportCoOrganisationPropertyInfo<T>(this T jobWithTransportCompany)
			where T : IDocAddresses, IJobWithTransportCompany
		{
			var docAddress = jobWithTransportCompany.LoadJobDocAddressQuickly(TransportCoConstants.AddressType);
			return docAddress?.GetOrganisation() != null ? docAddress.OrganisationPKInfo : null;
		}

		#endregion

		#region GetTransportCo

		public static OrgHeader GetTransportCo(this IJobWithTransportCompany jobWithTransportCompany)
		{
			return jobWithTransportCompany.TransportCoDocAddress.GetOrganisation();
		}

		#endregion

		#region TransportCoName

		// tested in WhsDocketTest
		internal static ZString GetTransportCoName(this IJobWithTransportCompany jobWithTransportCompany)
		{
			return jobWithTransportCompany.TransportCoDocAddress.E2_CompanyNameTruncated;
		}

		#endregion

		#region TransportCoPK

		// tested in WhsDocketTest
		internal static ZGuid GetTransportCoPK(this IJobWithTransportCompany jobWithTransportCompany)
		{
			return jobWithTransportCompany.TransportCoDocAddress.OrganisationPK;
		}

		// tested in WhsDocketTest
		internal static void SetTransportCoPK<T>(this T jobWithTransportCompany, ZGuid value)
			where T : BusinessObject, IJobWithTransportCompany
		{
			jobWithTransportCompany.TransportCoDocAddress.OrganisationPK = value;
		}

		#endregion

		#region TransportCoNameOrPK

		// tested in WhsDocketTest
		internal static ZString GetTransportCoFieldType(this IJobWithTransportCompany jobWithTransportCompany)
		{
			return jobWithTransportCompany.TransportCoDocAddress.E2_AddressOverride ? nameof(FieldType.Text) : nameof(FieldType.Guid);
		}

		// tested in WhsDocketTest
		internal static ZString GetTransportCoNameOrPK(this IJobWithTransportCompany jobWithTransportCompany)
		{
			ZString result;

			var docAddress = jobWithTransportCompany.TransportCoDocAddress;
			if (docAddress.E2_AddressOverride)
			{
				result = docAddress.E2_CompanyNameTruncated;
			}
			else
			{
				result = docAddress.Organisation?.PK.ToString() ?? ZGuid.Empty.ToString();
			}

			return result;
		}

		// tested in WhsDocketTest
		internal static void SetTransportCoNameOrPK(this IJobWithTransportCompany jobWithTransportCompany, string value)
		{
			var docAddress = jobWithTransportCompany.TransportCoDocAddress;
			if (docAddress.E2_AddressOverride)
			{
				docAddress.E2_CompanyName = value;
			}
			else
			{
				try
				{
					docAddress.OrganisationPK = new Guid(value);
				}
				catch (FormatException)
				{
				}
			}

			jobWithTransportCompany.TransportCoNameOrPKInfo.RefreshBinding();
		}

		// tested in WhsDocketTest
		internal static ZPropertyInfo GetTransportCoNameOrPKInfo(this IJobWithTransportCompany jobWithTransportCompany, Func<string, string, ZPropertyInfo> getPropertyInfo)
		{
			return getPropertyInfo(nameof(IJobWithTransportCompany.TransportCoNameOrPK), jobWithTransportCompany.TransportCoDocAddress.OrganisationPKInfo.HumanReadableName);
		}

		// tested in WhsDocketValidationTest
		internal static void ValidateTransportCoNameOrPKInfo(this IJobWithTransportCompany jobWithTransportCompany)
		{
			var address = jobWithTransportCompany.TransportCoDocAddress;
			if (address != null)
			{
				if (address.E2_AddressOverride)
				{
					address.Validation.ValidateE2_CompanyName();
					jobWithTransportCompany.TransportCoNameOrPKInfo.AddAllNotificationsFrom(address.E2_CompanyNameInfo);
				}
				else
				{
					address.Validation.ValidateOrganisationPK();
					jobWithTransportCompany.TransportCoNameOrPKInfo.AddAllNotificationsFrom(address.OrganisationPKInfo);
				}
			}
		}

		// tested in WhsDocketTest
		internal static int GetTransportCoNameOrPKMaxLength(this IJobWithTransportCompany jobWithTransportCompany)
		{
			const int GuidCharLength = 36;
			return jobWithTransportCompany.TransportCoDocAddress.E2_AddressOverride ? JobDocAddress.Schema.E2_CompanyNameTruncatedLength : GuidCharLength;
		}

		#endregion
	}
}
