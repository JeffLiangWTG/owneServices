using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.TemporaryOrgRemover
{
	public class TemporaryOrgCollection : NonPersistentBusinessObjectCollection<TemporaryOrg>
	{
		public TemporaryOrgCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override void Load()
		{
			RemoveAndDeleteAll();

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add(ZSqlParameter.New("@DependantFields", TemporaryOrgRemover.Remover.DependentFields, CargoWise.Schema.Schema.GenericStringSchemaColumn));
			@params.Add(ZSqlParameter.New("@OnlyTemporary", ZBool.True, CargoWise.Schema.Schema.GenericBoolSchemaColumn));

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("EXECUTE GetUnreferencedOrganisations @DependantFields, @OnlyTemporary", @params);

			foreach (DynamicBusinessObject organisation in collection)
			{
				Add(new TemporaryOrg((ZGuid)organisation[OrgHeaderSchema.PK.Name], (ZString)organisation[OrgHeaderSchema.OH_Code.Name], (ZString)organisation[OrgHeaderSchema.OH_FullName.Name]));
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
