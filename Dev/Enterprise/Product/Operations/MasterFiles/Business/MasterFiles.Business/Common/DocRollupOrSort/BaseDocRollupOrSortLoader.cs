using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.DocRollupOrSort
{
	public abstract partial class BaseDocRollupOrSortLoader<T>
		: BusinessObject.Loader
		where T : BusinessObject
	{
		public BaseDocRollupOrSortLoader(BusinessObjectFactory factory, OrgHeader orgHeader, GlbBranch branch, GlbDepartment department)
			: base(factory)
		{
			OrgHeader = orgHeader;
			Branch = branch;
			Department = department;
		}

		OrgHeader OrgHeader { get; }

		GlbBranch Branch { get; }

		GlbDepartment Department { get; }

		protected ZString Get(SchemaStringColumn orgField, ZString registryField, ZString defaultSetting, ZString serviceDirection, ZString transportMode, ZString containerMode, params ZString[] jobTypes)
			=> GetFromOrganisation(orgField, defaultSetting, serviceDirection, transportMode, containerMode, jobTypes)
			?? GetFromRegistry(registryField, serviceDirection, transportMode, containerMode, jobTypes);

		protected string GetFromRegistry(ZString registryField, ZString serviceDirection, ZString transportMode, ZString mode, ZString[] jobTypesInOrderOfPreference, CodeDescriptionPairList validValueList = null, string defaultValueIfRegistryIsNotValid = null)
		{
			var companyPK = Branch != null ? Branch.GB_GC.ToGuid() : Guid.Empty;
			var branchPK = Branch != null ? Branch.PK.ToGuid() : Guid.Empty;
			var departmentPK = Department != null ? Department.PK.ToGuid() : Guid.Empty;

			var registrySettings = GetRegistrySettings(companyPK, branchPK, departmentPK);
			var registrySetting = DocRollupOrGroupBestMatcher<IDocRollupOrGroupForBestMatcher>.GetBestMatch
			(
				registrySettings.Cast<IDocRollupOrGroupForBestMatcher>(),
				serviceDirection,
				transportMode,
				mode,
				jobTypesInOrderOfPreference
			);

			var registryBusinessObjectTemplate = (RegistryBusinessObjectTemplate)registrySetting;
			var valueFromRegistry = (ZString)registryBusinessObjectTemplate[registryField];
			if (validValueList != null && validValueList.Count > 0)
			{
				if (!validValueList.ContainsCode(valueFromRegistry))
				{
					valueFromRegistry = defaultValueIfRegistryIsNotValid;
				}
			}
			return valueFromRegistry;
		}

		protected abstract RegistryBusinessObjectCollectionTemplate GetRegistrySettings(Guid companyPK, Guid branchPK, Guid departmentPK);

		protected string GetFromOrganisation(SchemaStringColumn orgField, ZString defaultString, ZString serviceDirection, ZString transportMode, ZString containerMode, params ZString[] jobTypesInOrderOfPreference)
		{
			string result = null;

			if (OrgHeader != null)
			{
				var setting = Load(serviceDirection, transportMode, containerMode, jobTypesInOrderOfPreference);

				if (setting != null && (ZString)setting[orgField] != defaultString)
				{
					result = (ZString)setting[orgField];
				}
			}

			return result;
		}

		T Load(ZString serviceDirection, ZString transportMode, ZString containerMode, params ZString[] jobTypesInOrderOfPreference)
		{
			var ranker = new ColumnValueRanker();

			var jobTypes = new List<IZType>();
			foreach (var jobType in jobTypesInOrderOfPreference)
			{
				jobTypes.Add(jobType);

				if (jobType == JobInvoicingConsumerTypes.Shipment.Code || jobType == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					jobTypes.Add((ZString)OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code);
				}
			}
			jobTypes.Add((ZString)OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			ranker.Add(JobTypeSchemaColumn, jobTypes.ToArray());

			// Service Direction
			if (ServiceDirectionSchemaColumn != null)
			{
				ranker.Add(ServiceDirectionSchemaColumn, serviceDirection, (ZString)OrgConstants.ServiceDirection.Code.All);
			}

			// Transport Modes
			var containerModes = new List<IZType> { containerMode };
			var seaContainerModeList = ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
			if (transportMode == OrgConstants.ModesForGroupOrSubTotal.Codes.Sea && seaContainerModeList.ContainsCode(containerMode))
			{
				containerModes.Add((ZString)OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);
			}
			containerModes.Add((ZString)OrgConstants.ModesForGroupOrSubTotal.Codes.All);
			ranker.Add(TransportModeSchemaColumn, containerModes.ToArray());

			// Result
			var mainQuery = new ZQuery(CompanyDataSchemaColumn, OrgHeader.CompanyData.PK);
			return ranker.GetBestMatch<T>(OrgHeader.CompanyData.Factory, mainQuery);
		}

		protected abstract SchemaColumn JobTypeSchemaColumn { get; }

		protected abstract SchemaColumn ServiceDirectionSchemaColumn { get; }

		protected abstract SchemaColumn TransportModeSchemaColumn { get; }

		protected abstract SchemaColumn CompanyDataSchemaColumn { get; }
	}
}
