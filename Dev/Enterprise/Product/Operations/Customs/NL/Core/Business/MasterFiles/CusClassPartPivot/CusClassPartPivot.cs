using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.MasterFiles;

public class CusClassPartPivot : EU.Business.MasterFiles.CusClassPartPivot, Integration.Customs.NL.ICusClassPartPivot
{
	public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{ }

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		return result;
	}

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);
}
