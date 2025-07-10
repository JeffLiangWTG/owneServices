using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USDeclarationDocManagerInfo : Customs.Business.BaseJobDeclaration.DeclarationDocManagerInfo
	{
		public USDeclarationDocManagerInfo(JobDeclaration parent)
			: base(parent)
		{
		}

		public new JobDeclaration BusinessEntity => (JobDeclaration)base.BusinessEntity;

		protected override BusinessObject[] GetRelatedObjects()
		{
			var businessObjects = new List<BusinessObject>(base.GetRelatedObjects());

			if (BusinessEntity.InBondHeader is BusinessObject inBondHeader)
			{
				businessObjects.Add(inBondHeader);
			}

			return businessObjects.ToArray();
		}
	}
}
