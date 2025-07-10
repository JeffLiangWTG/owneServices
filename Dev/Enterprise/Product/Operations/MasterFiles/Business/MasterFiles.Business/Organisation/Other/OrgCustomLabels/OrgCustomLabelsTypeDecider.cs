using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCustomLabelsTypeDecider : TypeDecider
	{
		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			var otType = row[OrgCustomLabelsSchema.OT_Type.Name].ToString();
			if (otType == OrgConstants.CustomLabelType.OverrideExportDoc || otType == OrgConstants.CustomLabelType.OverrideImportDoc)
			{
				result = ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgCustomLabels>();
			}
			return result ?? typeof(OrgCustomLabels);
		}

		public override Type GetTypeForNew() => typeof(OrgCustomLabels);

		public override Type GetTypeForBinding() => typeof(OrgCustomLabels);
	}
}
