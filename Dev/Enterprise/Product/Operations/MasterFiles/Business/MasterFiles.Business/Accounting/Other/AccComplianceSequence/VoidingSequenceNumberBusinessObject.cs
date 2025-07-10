using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class VoidingSequenceNumberBusinessObject : AutoVoidingSequenceNumberBusinessObject
	{
		public VoidingSequenceNumberBusinessObject(AccComplianceSequence complianceSequence)
		{
			ComplianceSequence = complianceSequence;
		}

		internal AccComplianceSequence ComplianceSequence { get; set; }

		[BusinessObjectTestExclude]
		public override ZString VoidingFromNumber
		{
			get
			{
				return ComplianceSequence.XD_Calc_NextNumberString;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString VoidingToNumber
		{
			get
			{
				return base.VoidingToNumber;
			}
			set
			{
				if (value.Length > 0 && value.Length < ComplianceSequence.XD_MaximumNumberDigits)
				{
					value = value.PadLeft(ComplianceSequence.XD_MaximumNumberDigits, '0');
				}
				base.VoidingToNumber = value;
			}
		}

		public void VoidNumbersInRange()
		{
			if (ZDecimal.TryParse(VoidingToNumber, out ZDecimal nextNumber))
			{
				if (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
				{
					if (ComplianceSequence.IsInDatabase)
					{
						ComplianceSequence.Reload();
					}

					var helper = ObjectFactory.Get<IVoidComplianceDocumentHeader>("IVoidComplianceDocumentHeader");
					for (int i = ComplianceSequence.XD_NextNumber.ToZInt(); i <= nextNumber.ToZInt(); i++)
					{
						var documentNumber = ComplianceSequence.XD_Prefix + i.ToString(CultureInfo.CurrentCulture).PadLeft(ComplianceSequence.XD_MaximumNumberDigits, '0');
						helper.CreateVoidedComplianceDocumentHeader(ComplianceSequence.Factory, ComplianceSequence.PK, ComplianceSequence.XD_SequenceClass, documentNumber);
					}
				}

				ComplianceSequence.XD_NextNumber = nextNumber + 1;
				if (ComplianceSequence.XD_NextNumber > ComplianceSequence.XD_EndNumber)
				{
					ComplianceSequence.XD_ExpiryDate = ZDateTime.Now;
					ComplianceSequence.XD_IsActive = false;
				}
			}
		}
	}
}
