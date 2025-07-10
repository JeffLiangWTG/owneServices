using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal sealed class ContainerDetentionAdviceProvider : IEnumerable<DetentionAdviceHeader>
	{
		public ContainerDetentionAdviceProvider(GlbCompany company, ZDateTime asAt)
		{
			this.company = company;
			this.asAt = asAt;
		}

		public IEnumerator<DetentionAdviceHeader> GetEnumerator()
		{
			const string ParentPK = "ParentPK";
			const string ParentCode = "ParentCode";
			const string ClientPK = "ClientPK";
			const string sql = @"SELECT * FROM dbo.AgencyDetentionAdvice(@CompanyPK, @Country, @AsAt) ORDER BY ClientPK";

			var factory = new BusinessObjectFactory();
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@CompanyPK", company.PK, GlbCompanySchema.PK);
			parameters.Add("@Country", company.GC_RN_NKCountryCode, RefCountrySchema.RN_Code);
			parameters.Add("@AsAT", asAt, JobContainerMoveSchema.E9_MovementDate);

			var list = new DynamicBusinessObjectCollection(factory);
			list.Load(sql, parameters);

			if (list.Count > 0)
			{
				var containerPKList = new List<ZGuid>();
				var movementsPKList = new List<ZGuid>();
				var lastClientPK = new ZGuid(list[0][ClientPK]);
				int returnCount = 0;

				foreach (DynamicBusinessObject row in list)
				{
					var parentPK = new ZGuid(row[ParentPK]);
					var parentCode = new ZString(row[ParentCode]);
					var clientPK = new ZGuid(row[ClientPK]);

					if (clientPK != lastClientPK)
					{
						if (returnCount > 10)
						{
							factory = new BusinessObjectFactory();
							returnCount = 0;
						}
						else
						{
							returnCount++;
						}

						var advice = CreateAdvice(factory, lastClientPK, asAt, containerPKList, movementsPKList);
						if (advice != null)
						{
							yield return advice;
						}

						containerPKList.Clear();
						movementsPKList.Clear();
						lastClientPK = clientPK;
					}

					switch (parentCode)
					{
						case JobContainerSchema.Constants.Prefix:
							containerPKList.Add(parentPK);
							break;

						case JobContainerMoveSchema.Constants.Prefix:
							movementsPKList.Add(parentPK);
							break;
					}
				}

				if (containerPKList.Count > 0 || movementsPKList.Count > 0)
				{
					var advice = CreateAdvice(factory, lastClientPK, asAt, containerPKList, movementsPKList);
					if (advice != null)
					{
						yield return advice;
					}
				}
			}
		}

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation

		static DetentionAdviceHeader CreateAdvice(BusinessObjectFactory factory, ZGuid clientPK, ZDateTime asAt, List<ZGuid> containerPKs, List<ZGuid> movementPKs)
		{
			var client = factory.Load<OrgHeader>(clientPK);
			var advice = (client == null) ? new DetentionAdviceHeader(factory) : new DetentionAdviceHeader(client);

			advice.AsAt = asAt;
			advice.Containers.AddRange(factory.Load<BillOfLadingContainer>(new ZQuery(JobContainerSchema.PK, containerPKs)));
			advice.Movements.AddRange(factory.Load<ContainerMovement>(new ZQuery(JobContainerMoveSchema.PK, movementPKs)));

			return (advice.Containers.Count > 0 || advice.Movements.Count > 0) ? advice : null;
		}

		readonly GlbCompany company;
		readonly ZDateTime asAt;

		#endregion
	}
}


