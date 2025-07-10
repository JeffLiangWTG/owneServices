using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.MasterFiles.Business.Res;

namespace Enterprise.Registry.Business
{
	public class InvoiceRemittanceConfigurationValidation
	{
		public InvoiceRemittanceConfigurationValidation(InvoiceRemittanceConfiguration parent)
		{
			Parent = parent;
		}

		readonly InvoiceRemittanceConfiguration Parent;

		public void ValidateCode()
		{
			Parent.CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.CodeInfo);
			if (!Parent.CodeInfo.HasErrors())
			{
				if (Parent.ParentCollection.Cast<InvoiceRemittanceConfiguration>().Any(x => x.PK != Parent.PK && x.Code == Parent.Code))
				{
					Parent.CodeInfo.AddError(Res.GetString("41F11559-BDDF-4276-A839-7970CF70EC66", "This code already exists."));
				}
			}
		}

		public void ValidateDescription()
		{
			Parent.DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.DescriptionInfo);
		}

		public void ValidateMaxPossibleLength()
		{
			Parent.MaxPossibleLengthInfo.ClearAllNotifications();
			if (Parent.MaxPossibleLength > 0)
			{
				if (Parent.MaxPossibleLength > AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength)
				{
					Parent.MaxPossibleLengthInfo.AddError(Res.GetString("FE0EFF1A-D69E-41FD-981C-DF943F18B892", "Max Possible Length cannot greater than {0}.", AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength));
				}
				else if (Parent.MaxPossibleLength < Parent.ReferenceNumberTotalLength)
				{
					Parent.MaxPossibleLengthInfo.AddError(Res.GetString("2AA7B628-D20A-4F4A-B153-678E944B6F15", "This calculated reference number length is greater than Max Possible Length."));
				}
			}
		}

		public void ValidateDebtorLocation()
		{
			Parent.DebtorLocationInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.DebtorLocationInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DebtorLocationInfo, Parent.DebtorLocationList);
			if (!Parent.DebtorLocationInfo.HasErrors())
			{
				if (Parent.ParentCollection.Cast<InvoiceRemittanceConfiguration>().Any(x => x.PK != Parent.PK && x.DebtorLocation == Parent.DebtorLocation))
				{
					Parent.DebtorLocationInfo.AddError(Res.GetString("026B1F48-76F7-4981-BE40-52D5402502DA", "This Debtor Location already exists."));
				}
			}
		}
	}
}
