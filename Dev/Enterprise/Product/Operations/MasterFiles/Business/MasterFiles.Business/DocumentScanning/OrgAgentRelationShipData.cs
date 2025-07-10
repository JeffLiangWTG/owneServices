using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(typeof(OrgAgentRelationshipData),
	Enterprise.Core.Constants.DocManagerCodes.OrgAgentRelationShip)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class OrgAgentRelationshipData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(OrgAgentRelationship);
		protected override Type CollectionType => typeof(OrgAgentRelationshipCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new OrgAgentRelationshipCollection(factory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.ProfitShare;
		public override string ReferenceType => Core.Constants.ReferenceTypes.GeneralReferenceTables;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("14d2bd55-5f77-4f1a-a91e-e83034992248", "Profit Share");
		public override bool IsAllowedForUnallocatedeDocs => true;
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new OrgAgentRelationshipEDocsViaUniversalXmlSupport();
	}

	/// <summary>
	/// OrgAgentRelationship does not have an obvious unique identifier like other business objects (such as Client Code on a ClientRate).
	/// To allow the querying of eDocs, a code of format SendingAgent-ReceivingAgent-HeadOffice can be used.
	/// </summary>
	class OrgAgentRelationshipEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			var codeParts = code.Split('-');

			if (code.IsEmpty || codeParts.Length == 0 || codeParts.Length > 3)
			{
				return null;
			}

			var query = new ZQuery();

			if (
				!AddFilterWithOrgCode(factory, query, OrgAgentRelationshipSchema.O3_OH_SendingAgent, codeParts.ElementAtOrDefault(0)) ||
				!AddFilterWithOrgCode(factory, query, OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, codeParts.ElementAtOrDefault(1)) ||
				!AddFilterWithOrgCode(factory, query, OrgAgentRelationshipSchema.O3_OH_GroupNetworkOrFranchise, codeParts.ElementAtOrDefault(2)))
			{
				return null;
			}

			return factory.Load<OrgAgentRelationship>(query).FirstOrDefault();
		}

		/// <summary>
		/// Add a filter to the query based on the given org code.
		/// orgHeaderCode must either be empty or match an OrgHeader to be valid.
		/// </summary>
		/// <returns>whether the given code is valid.</returns>
		bool AddFilterWithOrgCode(BusinessObjectFactory factory, ZQuery query, SchemaColumn column, ZString orgHeaderCode)
		{
			if (orgHeaderCode.IsEmpty)
			{
				query.AddToFilter(JoinCondition.And, column, null);

				return true;
			}

			var orgHeader = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgHeaderCode);
			if (orgHeader == null)
			{
				return false;
			}

			query.AddToFilter(JoinCondition.And, column, orgHeader.PK);
			return true;
		}

		public ZString ExpectedCodeFormat => "SendingAgent-ReceivingAgent-HeadOffice";
		public ZString ExampleCodeFormat => "YOUAUSAU-YOUAUSMEL-WTGAUBNE";
	}
}
