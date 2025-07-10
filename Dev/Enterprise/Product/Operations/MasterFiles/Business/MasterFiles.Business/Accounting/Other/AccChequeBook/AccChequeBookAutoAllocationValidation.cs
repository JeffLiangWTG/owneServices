using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccChequeBookAutoAllocationValidation
	{
		public static string ChequeBookIsInActiveMessage
		{
			get { return Res.GetString("60a3a6a6-5195-4c50-877c-12f8c1e65912", "This check book is Inactive. Please enter another Check Book."); }
		}
		internal static string ChequeBookIsFullMessage
		{
			get { return Res.GetString("539c43ff-88d1-4d6d-bc4e-450457e89633", "The Check Book is full.\r\nSelect another Check Book or change the Last Number for the current Check Book."); }
		}
		internal static string ChequeBookWithoutAPrinterMessage
		{
			get { return Res.GetString("1ca1fd27-0c03-472b-bee8-796d1ae2d77c", "Can not Auto Allocate from this Check Book. Check Book does not have a printer selected."); }
		}

		public AccChequeBookAutoAllocationValidation()
		{
		}

		public ZString GetErrorsForChequeBook(AccChequeBook chequeBook, ZBool isChequeNumberAutoAllocated)
		{
			if (chequeBook != null)
			{
				if (!chequeBook.AK_IsActive)
				{
					return ChequeBookIsInActiveMessage;
				}
				else if (isChequeNumberAutoAllocated)
				{
					var nextChequeNumber = chequeBook.AK_CurrentNo;
					if (!nextChequeNumber.IsInRange(chequeBook.AK_StartNo, chequeBook.AK_LastNo))
					{
						return ChequeBookIsFullMessage;
					}
					else if (chequeBook.AK_SQ.IsEmpty || !chequeBook.AK_SQ.IsValid)
					{
						return ChequeBookWithoutAPrinterMessage;
					}
				}
			}
			return ZString.Empty;
		}
	}
}
