using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class BookingConfirmationUniversalCodeMapper : IUniversalCodeMapper
	{
		#region Create

		public static IUniversalCodeMapper Create(ITopLevelDataObject dataObject, BusinessObjectFactory factory)
		{
			if (dataObject == null
				|| factory == null)
			{
				return null;
			}

			var consol = GetConsol(dataObject, factory);

			if (consol == null)
			{
				return null;
			}

			var organisationForMapping = consol.JK_AgentType == Core.Constants.AgentType.CoLoad
				? consol.Creditor
				: consol.ShippingLine;

			return organisationForMapping is OrgHeader orgHeader
				? new BookingConfirmationUniversalCodeMapper(orgHeader)
				: null;
		}

		static ForwardingConsol GetConsol(ITopLevelDataObject dataObject, BusinessObjectFactory factory)
		{
			if (dataObject is Shipment shipment
				&& shipment.GetMatchingDataTarget(DataContextType.ForwardingConsol) is IDataTargetDataObject dataTargetDataObject
				&& dataTargetDataObject.Key.HasValue
				&& !dataTargetDataObject.Key.Value.IsEmpty)
			{
				var consolID = dataTargetDataObject.Key;
				var query = new ZQuery(JobConsolSchema.JK_UniqueConsignRef, consolID);
				query.AddToFilter(JobConsolSchema.JK_IsCancelled, false);
				return factory.LoadTop1<ForwardingConsol>(query);
			}

			return null;
		}

		#endregion

		BookingConfirmationUniversalCodeMapper(IOrgHeader sourceOrg)
		{
			SourceOrganisation = sourceOrg ?? throw new ArgumentNullException(nameof(sourceOrg));
		}

		public string GetMappedOrInput(string input, string codeMappingRelationshipCode, int? currentLineNumber = null)
		{
			switch (codeMappingRelationshipCode)
			{
				case Core.Constants.OrgPatternMatchOverrideRelationships.Port:
					if (UnlocoOverrides.TryGetValue(input, out var patternMatchOverride))
					{
						return patternMatchOverride.GetLocalCode();
					}
					break;
			}

			return input;
		}

		public string GetMappedOrEmpty(string input, string codeMappingRelationshipCode, int? currentLineNumber = null)
		{
			switch (codeMappingRelationshipCode)
			{
				case Core.Constants.OrgPatternMatchOverrideRelationships.Port:
					if (UnlocoOverrides.TryGetValue(input, out var patternMatchOverride))
					{
						return patternMatchOverride.GetLocalCode();
					}
					break;
			}

			return string.Empty;
		}

		IReadOnlyDictionary<string, OrgPatternMatchOverride> UnlocoOverrides => unlocoOverrides ?? (unlocoOverrides = GetUnlocoOverrides());
		IReadOnlyDictionary<string, OrgPatternMatchOverride> unlocoOverrides;

		IReadOnlyDictionary<string, OrgPatternMatchOverride> GetUnlocoOverrides()
		{
			var res = new Dictionary<string, OrgPatternMatchOverride>();

			if (SourceOrganisation is OrgHeader header)
			{
				var patternMatchOverrides = new OrgPatternMatchOverrideCollection(header, header.Factory);
				patternMatchOverrides.Load();

				const string OceanCargoMessagingContext = "OCM";

				foreach (OrgPatternMatchOverride patternMatchOverride in patternMatchOverrides)
				{
					if (patternMatchOverride.OO_Context == OceanCargoMessagingContext
						|| patternMatchOverride.OO_Relationship == Core.Constants.OrgPatternMatchOverrideRelationships.Port)
					{
						res[patternMatchOverride.OO_ForeignCode] = patternMatchOverride;
					}
				}
			}

			return res;
		}

		public IOrgPatternMatchOverride CreateOrUpdateCodeMapping(string codeMappingRelationshipCode, string foreignCode, string localCode, ZGuid localGuid) => null;

		public IOrgHeader SourceOrganisation { get; }
	}
}
