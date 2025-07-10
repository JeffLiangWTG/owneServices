using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public static class DetentionableLoader
	{
		public static ContainerMovement[] LoadDetentionableContainers(BusinessObjectFactory factory, ZGuid companyPK, ZGuid clientPK, ZGuid principalPK, string detentionType, string countryCode)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			var sql = string.Format(CultureInfo.InvariantCulture, @"
{0} in
(
	select {0}
	from DetentionableMovementsSingleClient(@company, @client, @principal)
	where
		DetentionType = @detentionType and
		CountryCode = @country
)
", JobContainerMoveSchema.Constants.PK);

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@company", companyPK, GlbCompanySchema.PK);
			parameters.Add("@detentionType", detentionType, JobContainerMoveSchema.E9_MovementType);
			parameters.Add("@principal", principalPK.IsEmpty ? DBNull.Value : principalPK.ToGuid(), JobShipmentSchema.JS_OH_DeliveryAgent);
			parameters.Add("@client", clientPK.IsEmpty ? DBNull.Value : clientPK.ToGuid(), OrgAddressSchema.OA_OH);
			parameters.Add("@country", countryCode, RefCountrySchema.RN_Code);

			ZDBOnlyQuery movementFilter = new ZDBOnlyQuery(typeof(ContainerMovement));
			movementFilter.AddFilterAndZSQLParameterCollection(sql, parameters);

			return factory.Load<ContainerMovement>(movementFilter);
		}

		public static BulkDetentionChild[] LoadClientsWithDetentionableContainers(BusinessObjectFactory factory, ZGuid companyPK, ZGuid localClientPK, ZGuid principalPK, ZString countryCode, ZString direction)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
select *
from DetentionableMovements(@company, @client, @principal)
where
	(nullif(@detentionType, '') is null or DetentionType = @detentionType) and
	(nullif(@country, '') is null or CountryCode = @country)
order by
	DetentionType,
	{0},
	{1},
	CountryCode
", JobContainerMoveSchema.Constants.E9_OH_Principal, JobContainerMoveSchema.Constants.E9_OH_ResponsibleParty);

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@company", companyPK, GlbCompanySchema.PK);
			parameters.Add("@detentionType", direction, JobContainerMoveSchema.E9_MovementType);
			parameters.Add("@client", localClientPK.IsEmpty ? DBNull.Value : localClientPK, JobHeaderSchema.JH_OA_LocalChargesAddr);
			parameters.Add("@principal", principalPK.IsEmpty ? DBNull.Value : principalPK, JobShipmentSchema.JS_OH_DeliveryAgent);
			parameters.Add("@country", countryCode, RefCountrySchema.RN_Code);

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, parameters);

			List<BulkDetentionChild> result = new List<BulkDetentionChild>();
			BulkDetentionChild current = null;

			foreach (DynamicBusinessObject dbo in collection)
			{
				ZString rowDetentionType = new ZString(dbo["DetentionType"]);
				ZGuid rowMovementPK = new ZGuid(dbo[JobContainerMoveSchema.Constants.PK]);
				ZGuid rowPrincipalPk = new ZGuid(dbo[JobContainerMoveSchema.Constants.E9_OH_Principal]);
				ZGuid rowClientPk = new ZGuid(dbo[JobContainerMoveSchema.Constants.E9_OH_ResponsibleParty]);
				ZString rowCountryCode = new ZString(dbo["CountryCode"]);

				if (current == null ||
					rowDetentionType != current.DetentionType ||
					rowPrincipalPk != current.PrincipalPK ||
					rowClientPk != current.ClientPK ||
					rowCountryCode != current.CountryCode)
				{
					current = new BulkDetentionChild(factory)
					{
						DetentionType = rowDetentionType,
						PrincipalPK = rowPrincipalPk,
						ClientPK = rowClientPk,
						CountryCode = rowCountryCode,
					};

					result.Add(current);
				}

				current.MovementPKs.Add(rowMovementPK);
			}

			return result.ToArray();
		}
	}
}









