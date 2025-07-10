using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using DataExportBatchSubTypes = Enterprise.Core.Constants.DataExportBatchSubTypes;
using DataExportBatchTypes = Enterprise.Core.Constants.DataExportBatchTypes;

namespace Enterprise.MasterFiles.Business
{
	[UniversalDataContext(DataContextType.BatchNumber)]
	public class GenExportBatchSequence : AutoGenExportBatchSequence, IJobNumber
	{
		public GenExportBatchSequence(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete
		{
			get { return false; }
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (XB_Type.IsEmpty)
			{
				XB_Type = DataExportBatchSubTypes.Codes.GeneralLedgerPost;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		#region Properties

		public ZString Type
		{
			get
			{
				ZString result = default;
				if (SubType == DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport ||
					SubType == DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport ||
					SubType == DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport)
				{
					result = DataExportBatchTypes.Codes.LegacyTransactionXML;
				}
				else if (SubType == DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost ||
					SubType == DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse)
				{
					result = DataExportBatchTypes.Codes.UniversalTransactionXML;
				}
				else if (SubType == DataExportBatchSubTypes.Codes.GeneralLedgerPost ||
					SubType == DataExportBatchSubTypes.Codes.GeneralLedgerReverse)
				{
					result = DataExportBatchTypes.Codes.GLTransactionCSV;
				}
				else if (SubType == DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredPosting ||
					SubType == DataExportBatchSubTypes.Codes.GLConsolidationsEliminationNotRequiredReversal ||
					SubType == DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredPosting ||
					SubType == DataExportBatchSubTypes.Codes.GLConsolidationsEliminationRequiredReversal)
				{
					result = DataExportBatchTypes.Codes.GLConsolidations;
				}
				else if (SubType == DataExportBatchSubTypes.Codes.PositivePayFile)
				{
					if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.UnitedStates)
					{
						result = DataExportBatchTypes.Codes.PositivePay;
					}
					else
					{
						result = DataExportBatchTypes.Codes.ExportChequePayments;
					}
				}
				return result;
			}
		}

		public ZString TypeDescription
		{
			get { return Lookups.DataExportBatchTypeList.GetDescriptionFromCode(Type); }
		}

		public ZString SubType
		{
			get { return Lookups.DataExportBatchSubTypeList.ContainsCode(XB_Type) ? XB_Type : ZString.Empty; }
		}

		public ZString SubTypeDescription
		{
			get { return Lookups.DataExportBatchSubTypeList.GetDescriptionFromCode(SubType); }
		}

		public ZInt BatchNumber
		{
			get { return XB_BatchNumber; }
		}

		string IJobNumber.JobNumber => BatchNumber.ToString();

		#endregion
	}
}
