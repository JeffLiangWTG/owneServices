using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class NMFSDocumentValidation : CusCodeDataValidation
	{
		public NMFSDocumentValidation(NMFSDocument parent)
			: base(parent)
		{
		}

		new NMFSDocument Parent
		{
			get { return (NMFSDocument)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			var nmfsLine = Parent.NMFSLine;
			if (nmfsLine != null)
			{
				nmfsLine.AddInfoValidation.ValidateUS_DocumentType();
				nmfsLine.AddInfoValidation.ValidateUS_DISDocumentID();
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
			ListValidation.WarnIfInvalidCode(Parent.CY_DataInfo, Parent.Lookups.DISDocumentIDList);
		}
	}
}
