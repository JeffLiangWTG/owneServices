using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Manifesting = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryHeaderCollection : Customs.Business.CusEntryHeaderCollection<CusEntryHeader>
	{
		public CusEntryHeaderCollection(JobDeclaration parentDeclaration, BusinessObjectFactory factory)
			: base(parentDeclaration, factory)
		{
			Declaration = parentDeclaration;
		}
		protected new readonly JobDeclaration Declaration;

		#region AddNew

		protected override BusinessObject AddNewCore()
		{
			Type newType = typeof(FormalEntry.CusEntryHeader);
			if (Declaration != null)
			{
				newType = Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings;
			}
			return AddNew(newType);
		}

		#endregion

		#region Add
		public override void Add(BusinessObject businessObject)
		{
			Manifesting.CusEntryHeader manifestEntryHeader = businessObject as Manifesting.CusEntryHeader;
			ZGuid savedParentPK = ZGuid.Empty;
			int savedParentClusterKey = 0;
			if (manifestEntryHeader != null)
			{
				savedParentPK = manifestEntryHeader.CH_JE;
				savedParentClusterKey = manifestEntryHeader.CH_ClusterKey;
				using (manifestEntryHeader.SuspendSettingHasChanges())
				{
					manifestEntryHeader.CH_JE = Master.PK;
					manifestEntryHeader.CH_ClusterKey = Master.JE_ClusterKey;
				}
			}
			base.Add(businessObject);
			if (manifestEntryHeader != null)
			{
				using (manifestEntryHeader.SuspendSettingHasChanges())
				{
					manifestEntryHeader.CH_JE = savedParentPK;
					manifestEntryHeader.CH_ClusterKey = savedParentClusterKey;
				}
			}
		}
		#endregion

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			JobDeclaration declaration = (JobDeclaration)Master;
			if (declaration.IsECIManifestDeclarationReference)
			{
				result.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_BGMReference, SQLComparisonOperator.Equal, declaration.ECIManifestReference);
			}
			return result;
		}

		public override void RemoveAndDeleteUnNecessaryCusEntryHeaders()
		{
			foreach (CusEntryHeader entryHeader in ToArray())
			{
				entryHeader.DeleteIfContainsNoValuableData();
			}
		}
	}
}
