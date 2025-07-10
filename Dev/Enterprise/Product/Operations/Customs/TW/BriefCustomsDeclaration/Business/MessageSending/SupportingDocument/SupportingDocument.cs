using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class SupportingDocument : TW.Business.SupportingDocument
	{
		public SupportingDocument(BusinessObjectFactory factory) : base(factory)
		{
		}

		public SupportingDocument(BusinessObjectFactory factory, CodeDescriptionPairList typeList, CodeDescriptionPairList billNumberList = null)
			: base(factory, typeList, billNumberList)
		{
		}

		protected override void ValidateBillNumberCore()
		{
			if (!EDoc.IsEmpty && BillNumber.IsEmpty)
			{
				BillNumberInfo.AddError(Res.GetString("8687100B-0C24-494E-B638-1AAB88013DBF", "Please enter a Bill Number."));
			}
		}

		protected override void ValidateTypeCore()
		{
			if (!EDoc.IsEmpty && Type.IsEmpty)
			{
				TypeInfo.AddError(Res.GetString("7EB14CE1-9D70-4A39-B965-738E26ADBC8E", "Please enter a Type."));
			}
		}
	}
}
