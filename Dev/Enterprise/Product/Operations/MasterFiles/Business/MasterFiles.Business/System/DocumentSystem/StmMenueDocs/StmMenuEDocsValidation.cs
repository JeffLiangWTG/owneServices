//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmMenuEDocsValidation
//
//    This class should be used for overriding validation in AutoStmMenuEDocsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class StmMenuEDocsValidation : AutoStmMenuEDocsValidation
	{
		public StmMenuEDocsValidation(AutoStmMenuEDocs parent) : base(parent)
		{
		}

		protected override void CheckSX_PrintCopyType()
		{
			base.CheckSX_PrintCopyType();

			MandatoryValidation.CheckEntered(Parent.SX_PrintCopyTypeInfo);

			if (!Parent.SX_PrintCopyType.IsEmpty && !PrintCopyTypeList.ContainsCode(Parent.SX_PrintCopyType))
			{
				Parent.SX_PrintCopyTypeInfo.AddError(Res.GetString("c585c23f-bed6-4286-bfc8-a00a31b7b9cb", "Invalid type entered. Please select one from the list."));
			}
		}

		CodeDescriptionPairList PrintCopyTypeList
		{
			get
			{
				if (printCopyTypeList == null)
				{
					printCopyTypeList = new CodeDescriptionPairList(OLookUpEditType.NotifyMode);
					printCopyTypeList.AddPair(nameof(PrintCopyType.ALL), ResString.GetMultilingualString("ababed01-82ac-4d16-bc40-a623990087f9", "ALL"));
				}

				return printCopyTypeList;
			}
		}
		CodeDescriptionPairList printCopyTypeList;
	}
}
