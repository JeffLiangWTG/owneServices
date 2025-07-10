//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusHouseContPackInvoiceLinePivotValidation
//
//    This class should be used for overriding validation in AutoCusHouseContPackInvoiceLinePivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusHouseContPackInvoiceLinePivotValidation : AutoCusHouseContPackInvoiceLinePivotValidation
	{
		public CusHouseContPackInvoiceLinePivotValidation(AutoCusHouseContPackInvoiceLinePivot parent) : base(parent)
		{
		}

		protected override void CheckCHC_NumberOfPacks()
		{
			base.CheckCHC_NumberOfPacks();
			var dec = Parent.Factory.Load<BaseJobDeclaration>(Parent.CHC_JE);

			if (dec != null && dec.MaximumNumberOfPacksForEntryLine != -1)
			{
				var invLine = Parent.Factory.Load<BaseJobComInvoiceLine>(Parent.CHC_JI);
				if (Parent.CHC_NumberOfPacks > 0)
				{
					var maxNumberOfPacks = dec.MaximumNumberOfPacksForEntryLine;
					if (Parent.CHC_NumberOfPacks > maxNumberOfPacks)
					{
						Parent.CHC_NumberOfPacksInfo.AddMessageError(ValidationMessageForMaxPackQuantityExceeded(maxNumberOfPacks));
					}
					else
					{
						if (invLine != null)
						{
							if (invLine.CusEntryLine != null)
							{
								var packQuantityAcrossCusEntryLine = invLine.CusEntryLine.InvoiceLines.Cast<BaseJobComInvoiceLine>().SelectMany(i => i.PackagesPivot).Cast<InvoiceLinePackagePivot>().Sum(GetPackQtySafe);
								if (packQuantityAcrossCusEntryLine > maxNumberOfPacks)
								{
									Parent.CHC_NumberOfPacksInfo.AddMessageError(ValidationMessageForMaxPackQuantityExceeded(maxNumberOfPacks));
								}
							}
							else
							{
								var packQuantityAcrossInvoiceLine = invLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Sum(GetPackQtySafe);
								if (packQuantityAcrossInvoiceLine > maxNumberOfPacks)
								{
									Parent.CHC_NumberOfPacksInfo.AddMessageError(ValidationMessageForMaxPackQuantityExceeded(maxNumberOfPacks));
								}
							}
						}
					}
				}
			}

			var pivot = Parent as ICusPackagePivot;
			var supporter = pivot?.PivotSupporter;

			if (supporter == null || (pivot.Package != null && !supporter.IsSupportEmptyPackType(pivot.Package)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CHC_NumberOfPacksInfo);
			}
		}

		int GetPackQtySafe(InvoiceLinePackagePivot pivot)
		{
			return pivot?.Package?.CW_PackQty ?? 0;
		}

		public static string ValidationMessageForMaxPackQuantityExceeded(int number)
		{
			return Res.GetString("722B992C-28C9-47DE-93D3-A97A464DDE47", "The pack quantity ({0}) has been exceeded.\r\nThis number reflects other packs within the invoice line or entry line.", number);
		}
	}
}
