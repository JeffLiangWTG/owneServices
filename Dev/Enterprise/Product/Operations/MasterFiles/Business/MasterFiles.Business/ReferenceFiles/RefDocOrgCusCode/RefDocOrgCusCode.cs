using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefDocOrgCusCode : AutoRefDocOrgCusCode
	{
		public RefDocOrgCusCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.DocumentTypeList")]
		public override ZString DOC_DocumentType
		{
			get => base.DOC_DocumentType;
			set => base.DOC_DocumentType = value;
		}

		[List("Lookups.RegistrationTypeList")]
		public override ZString DOC_CodeType
		{
			get => base.DOC_CodeType;
			set => base.DOC_CodeType = value;
		}

		[List("Lookups.DirectionList")]
		public override ZString DOC_Direction
		{
			get => base.DOC_Direction;
			set => base.DOC_Direction = value;
		}

		public ZString DOC_Calc_CodeTypeDescription => Lookups.RegistrationTypeList.GetDescriptionFromCode(DOC_CodeType) ?? "";

		#region Implementation

		protected override ZString HumanReadableNameCore => Res.GetString("25a1c0ec-1020-f0a5-40e5-cb025c1b3708",
			"Registration Mapping - {0} {1} {2} {3}", DOC_RN_NKRegulatingCountry, DOC_RN_NKCodeCountry, DOC_CodeType, DOC_DocumentType);

		#endregion
	}
}
