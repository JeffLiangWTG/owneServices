using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry
{
	public class CusEntryHeader : Declaration.CusEntryHeader, Integration.Customs.NZ.IFormalEntryCusEntryHeader
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			IsMarkAsNeedingValidationSuspended = true;
		}

		protected override bool HasAmountToPayOrRefundOtherThanEntryFee
		{
			get { return base.HasAmountToPayOrRefundOtherThanEntryFee || !DepositRefundAmount.IsEmpty; }
		}

		#region MergedLines
		public new Customs.Business.CusEntryLineCollection<CusEntryLine> MergedLines
		{
			get
			{
				return base.MergedLines;
			}
		}

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection()
		{
			return new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);
		}
		#endregion

		#region Total Charges proxied from EntryLines Collection
		#region ALACLevyCreditAmount
		public ZDecimal ALACLevyCreditAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.ALACLevyCreditAmount;
				}
				return result;
			}
		}
		#endregion
		#region ACCLevyCreditAmount
		public ZDecimal ACCLevyCreditAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.ACCLevyCreditAmount;
				}
				return result;
			}
		}
		#endregion
		#region HERALevyCreditAmount
		public ZDecimal HERALevyCreditAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.HERALevyCreditAmount;
				}
				return result;
			}
		}
		#endregion
		#region PFMLFuelLevyCreditAmount
		public ZDecimal PFMLFuelLevyCreditAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.PFMLFuelLevyCreditAmount;
				}
				return result;
			}
		}
		#endregion

		#region AntiDumpingDutyAmount
		public ZDecimal AntiDumpingDutyAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.AntiDumpingDutyAmount;
				}
				return result;
			}
		}
		#endregion
		#region CountervailingDutyAmount
		public ZDecimal CountervailingDutyAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.CountervailingDutyAmount;
				}
				return result;
			}
		}
		#endregion
		#region DepositRefundAmount
		public ZDecimal DepositRefundAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.DepositRefundAmount;
				}
				return result;
			}
		}
		#endregion
		#region ExciseDutyCreditAmount
		public ZDecimal ExciseDutyCreditAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.ExciseDutyCreditAmount;
				}
				return result;
			}
		}
		#endregion

		#region ALACLevyAmount
		public ZDecimal ALACLevyAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.ALACLevyAmount;
				}
				return result;
			}
		}
		#endregion
		#region ACCFuelLevyAmount
		public ZDecimal ACCFuelLevyAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.ACCFuelLevyAmount;
				}
				return result;
			}
		}
		#endregion

		#region PFMLFuelLevyAmount

		public ZDecimal PFMLFuelLevyAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.PFMLFuelLevyAmount;
				}
				return result;
			}
		}

		#endregion

		#region SyntheticGreenhouseGasesLevyAmount

		public ZDecimal SyntheticGreenhouseGasesLevyAmount
		{
			get
			{
				ZDecimal result = 0.00m;

				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.SyntheticGreenhouseGasesLevyAmount;
				}

				return result;
			}
		}

		#endregion
		#region HERALevyAmount
		public ZDecimal HERALevyAmount
		{
			get
			{
				ZDecimal result = 0.00m;
				foreach (CusEntryLine mergedLine in MergedLines)
				{
					result += mergedLine.HERALevyAmount;
				}
				return result;
			}
		}
		#endregion
		#region GSTAmount
		public override ZDecimal GSTAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (CusEntryLine entryLine in MergedLines)
				{
					result += entryLine.GSTAmount;
				}
				return result;
			}
		}
		#endregion
		#endregion

		#region Packages Proxy from Declaration
		public override ZInt PackagesCount
		{
			get { return Declaration.PackageCountFromPackagesCollection; }
		}
		#endregion

		#region EntryNumberType

		protected override ZString EntryNumberType
		{
			get { return CusEntryNumberTypeList.Codes.FormalEntry; }
		}

		#endregion

		#region SetDefaultValues
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CH_MessageType = EntryHeaderTypes.NZ.FormalEntry;
		}
		#endregion

		#region IsECIWriteOff
		public override bool IsECIWriteOff
		{
			get { return false; }
		}
		#endregion

		#region SetDeclarationStatusesWhenSetToCurrent
		public override void SetDeclarationStatusesWhenSetToCurrent(JobDeclaration declaration)
		{
			declaration.JE_EntryStatus = CH_EntryStatus;
			if (Extensions.RemoveConsolidatedStatus(declaration.JE_EntryStatus) == LowValueManifestStatusList.Codes.NotSentToCustoms)
			{
				declaration.JE_ManifestBioStatus = ZString.Empty;
				declaration.JE_ManifestNZCSStatus = ZString.Empty;
				declaration.JE_TSWCombinedStatus = ZString.Empty;
				CH_MPIBioStatus = ZString.Empty;
				CH_MPIFoodStatus = ZString.Empty;
				CH_MPIBioMovementStatus = ZString.Empty;
				CH_NZCSStatus = ZString.Empty;
				CH_NZCSMovementStatus = ZString.Empty;
				CH_MPIBioResponseTime = ZDateTime.Empty;
				CH_MPIFoodResponseTime = ZDateTime.Empty;
				CH_MPIBioMovementStatusTime = ZDateTime.Empty;
				CH_NZCSResponseTime = ZDateTime.Empty;
				CH_NZCSMovementStatusTime = ZDateTime.Empty;
			}
		}
		#endregion

		#region EntryLinesExistWithoutFreight

		public bool EntryLinesExistWithoutFreight
		{
			get
			{
				bool result = false;
				foreach (CusEntryLine line in MergedLines)
				{
					if (line.FreightWholeNZD == 0)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		#endregion

		public override ZGuid CH_JE
		{
			get { return base.CH_JE; }
			set
			{
				bool hasChanged = CH_JE != value;
				base.CH_JE = value;
				if (hasChanged && Declaration != null)
				{
					if (IsMarkAsNeedingValidationSuspended)
					{
						IsMarkAsNeedingValidationSuspended = false;
					}
					else
					{
						Declaration.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected override void SetMessagingStatusToNotSentInternal()
		{
			CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
		}

		bool IsMarkAsNeedingValidationSuspended
		{
			get { return fIsMarkAsNeedingValidationSuspended; }
			set { fIsMarkAsNeedingValidationSuspended = value; }
		}
		bool fIsMarkAsNeedingValidationSuspended;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);
	}
}
