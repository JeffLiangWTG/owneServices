using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public static class JobRequiredDocumentHelper
	{
		public static ZString GetValueByAttributeName(this JobRequiredDocAttribCollection jobRequiredDocAttribCollection, ZString attributeName)
		{
			return jobRequiredDocAttribCollection.Cast<JobRequiredDocAttrib>().FirstOrDefault(x => x.D0_AttribName == attributeName)?.D0_AttribValue ?? ZString.Empty;
		}

		public static JobRequiredDocument GetJobRequiredDocument(this JobRequiredDocumentDependentCollection jobRequiredDocumentDependentCollection, ZString docType, ZString officeCode, ZString boxNumber, ZDateTime validToDate, string bondedID = "")
		{
			Func<JobRequiredDocument, bool> queryFunc = x => x.EQ_DocCategory == Core.Constants.ReferenceTypes.ClientSupplierRelationship
						&& x.EQ_DocType == docType
						&& x.EQ_DocUsage == JobRequiredDocument.DocUsage.Broker
						&& x.EQ_RN_NKRelatedCountry == Core.Constants.CountryCodes.Taiwan
						&& x.EQ_ValidToDate >= validToDate
						&& x.Attributes.GetValueByAttributeName(JobRequiredDocAttribTypeList.Codes.CustomsDistrict) == officeCode
						&& x.Attributes.GetValueByAttributeName(JobRequiredDocAttribTypeList.Codes.BoxNumber) == boxNumber
						&& x.Attributes.GetValueByAttributeName(JobRequiredDocAttribTypeList.Codes.BondedID) == bondedID;

			return jobRequiredDocumentDependentCollection.Cast<JobRequiredDocument>().FirstOrDefault(queryFunc);
		}
	}
}
