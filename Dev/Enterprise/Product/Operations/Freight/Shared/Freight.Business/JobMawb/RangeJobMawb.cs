using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class RangeJobMawb : JobMawb
	{
		public RangeJobMawb(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : JobMawb.Schema
		{
			public const string JM_AirlinePrefixText = "JM_AirlinePrefixText";
			public const string IsNeutralMAWB = "IsNeutralMAWB";
			public const string MawbCount = "MawbCount";
			public const string NumberRangeStart = "NumberRangeStart";
			public const string NumberRangeEnd = "NumberRangeEnd";
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			IsNeutralMAWB = true;
			JM_GC_Company = GlbCompany.CurrentCompany.PK;
			JM_GB = GlbBranch.CurrentBranch.PK;
			JM_ServiceLevel = OrgCarrierServiceLevel.StandardCode;
		}

		#endregion

		#region Validation

		public new RangeJobMawbValidation Validation
		{
			get { return (RangeJobMawbValidation)base.Validation; }
		}

		protected override JobMawbValidation GetNewValidation()
		{
			return new RangeJobMawbValidation(this);
		}

		#endregion

		#region Saving

		public override bool IsSavedByFactory
		{
			get
			{
				return false;
			}
		}

		public void Save()
		{
			ZString mawbSuffix = NumberRangeStart;

			for (var i = 0; i < MawbCount; i++)
			{
				var jobMawb = Factory.New<JobMawb>();
				using (jobMawb.GetValidationSuspender())
				{
					jobMawb.JM_Airline3DigitPrefix = JM_Airline3DigitPrefix;
					jobMawb.JM_GC_Company = JM_GC_Company;
					jobMawb.JM_GB = JM_GB;
					jobMawb.JM_IsPaper = !IsNeutralMAWB;
					jobMawb.JM_OA_From = JM_OA_From;
					jobMawb.JM_MAWB = mawbSuffix;
					jobMawb.JM_ServiceLevel = JM_ServiceLevel;
				}
				jobMawb.Validation.ValidateAll();

				var mawbNumber = Convert.ToInt32(mawbSuffix);
				mawbSuffix = GetNextMAWB(mawbNumber);
			}

			Factory.Save();
		}

		#endregion

		#region Duplicates In Save Range

		public ZString DuplicatesInSaveRange()
		{
			ZString result = "";

			DuplicateJobMawbs duplicateJobMawbs = new DuplicateJobMawbs(this);
			if (duplicateJobMawbs.Count > 0)
			{
				result = Res.GetString("3d8ab9a7-8a61-44c0-9165-0fc3f4048435", "Master Bill Numbers in the range '{0}' to '{1}' already exist for Airline: {2}\r\n\r\nPlease choose a range of new numbers that does not include numbers already assigned.\r\n\r\nFirst Master Bill Number that exists:\r\n\t{2} {3}",
					NumberRangeStart, NumberRangeEnd, JM_Airline3DigitPrefix, duplicateJobMawbs.FirstDuplicate);
			}
			else
			{
				var duplicateMAWB = MasterBillValidator.DuplicateMAWB(Factory, JM_Airline3DigitPrefix + NumberRangeStart, JM_Airline3DigitPrefix + NumberRangeEnd, Core.Constants.TransportModes.Air);
				if (!string.IsNullOrEmpty(duplicateMAWB))
				{
					result = Res.GetString("52E55DEA-83A7-4B61-8C6E-C6047F73D364", "Master Bill Numbers in the range '{0}' to '{1}' have already been used in consol or shipment for Airline: {2}\r\n\r\nPlease choose a range of new numbers that does not include numbers already used.\r\n\r\nFirst Master Bill Number that exists:\r\n\t {3}",
					  NumberRangeStart, NumberRangeEnd, JM_Airline3DigitPrefix, duplicateMAWB);
				}
			}
			return result;
		}

		#endregion

		#region Properties

		#region JM_Airline3DigitPrefix

		[ReadOnly(false)]
		public override ZString JM_Airline3DigitPrefix
		{
			get { return base.JM_Airline3DigitPrefix; }
			set
			{
				base.JM_Airline3DigitPrefix = value;
				JM_AirlinePrefixTextInfo.RefreshBinding();
			}
		}

		public ZString JM_AirlinePrefixText
		{
			get { return JM_Airline3DigitPrefix; }
		}

		public ZPropertyInfo JM_AirlinePrefixTextInfo
		{
			get { return GetZPropertyInfo(Schema.JM_AirlinePrefixText); }
		}

		#endregion

		#region IsNeutralMAWB

		public ZBool IsNeutralMAWB
		{
			get { return isNeutralMAWB; }
			set { SetNonPersistentPropertyValue(IsNeutralMAWBInfo, ref isNeutralMAWB, value); }
		}
		ZBool isNeutralMAWB;

		public ZPropertyInfo IsNeutralMAWBInfo
		{
			get { return GetZPropertyInfo(Schema.IsNeutralMAWB); }
		}

		#endregion

		#region MawbCount

		public ZInt MawbCount
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return mawbCount; }
			set
			{
				if (mawbCount != value)
				{
					SetNonPersistentPropertyValue(MawbCountInfo, ref mawbCount, value < 0 ? (ZInt)0 : value);
					SetNumberRangeEnd();

					if (!IsValidationSuspended)
					{
						Validation.ValidateMawbCount();
					}
				}
			}
		}
		ZInt mawbCount;

		public ZPropertyInfo MawbCountInfo
		{
			get { return GetZPropertyInfo(Schema.MawbCount); }
		}

		#endregion

		#region NumberRangeStart

		[MaxLength(8)]
		public ZString NumberRangeStart
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return numberRangeStart; }
			set
			{
				if (numberRangeStart != value)
				{
					if (value.IsEmpty)
					{
						numberRangeStart = value;
					}
					else
					{
						CheckMaximumLength(NumberRangeStartInfo, value);
						numberRangeStart = value.PadLeft(8, '0');
					}

					HasChanges = true;

					if (!IsValidationSuspended)
					{
						Validation.ValidateNumberRangeStart();
					}

					SetNumberRangeEnd();

					if (!IsValidationSuspended)
					{
						Validation.ValidateNumberRangeStart();
					}

					NumberRangeStartInfo.RefreshBinding();
				}
			}
		}
		ZString numberRangeStart;

		public ZPropertyInfo NumberRangeStartInfo
		{
			get { return GetZPropertyInfo(Schema.NumberRangeStart); }
		}

		#endregion

		#region NumberRangeEnd

		[MaxLength(8)]
		public ZString NumberRangeEnd
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return numberRangeEnd; }
			set
			{
				if (numberRangeEnd != value)
				{
					if (value.IsEmpty)
					{
						numberRangeEnd = value;
					}
					else
					{
						CheckMaximumLength(NumberRangeEndInfo, value);
						numberRangeEnd = value.PadLeft(8, '0');
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateNumberRangeEnd();
					}

					if (!NumberRangeEndInfo.HasErrors())
					{
						SetMAWBCount();
					}

					HasChanges = true;
				}

				NumberRangeEndInfo.RefreshBinding();
			}
		}
		ZString numberRangeEnd;

		public ZPropertyInfo NumberRangeEndInfo
		{
			get { return GetZPropertyInfo(Schema.NumberRangeEnd); }
		}

		#endregion

		#endregion

		#region Implementation

		internal bool IsEndMAWBOutOfRange;

		void SetNumberRangeEnd()
		{
			if (!NumberRangeStart.IsEmpty && !NumberRangeStartInfo.HasErrors() && MawbCount > 0 && !MawbCountInfo.HasErrors())
			{
				IsEndMAWBOutOfRange = false;
				ZString currentNumber = NumberRangeStart;
				for (int i = 0; i < MawbCount - 1; i++)
				{
					int mawbNumber = Convert.ToInt32(currentNumber);
					currentNumber = GetNextMAWB(mawbNumber);
				}

				IsEndMAWBOutOfRange = (currentNumber.Length > NumberRangeEndInfo.MaxLength);
				if (!IsEndMAWBOutOfRange)
				{
					NumberRangeEnd = currentNumber.ToString();
				}
			}
			else
			{
				NumberRangeEnd = "";
			}
		}

		ZString GetNextMAWB(int mawbNumber)
		{
			mawbNumber = mawbNumber / 10; //get rid of the check digit
			mawbNumber++; //next number

			int newCheckDigit = mawbNumber % 7;
			mawbNumber = mawbNumber * 10 + newCheckDigit;

			return mawbNumber.ToString().PadLeft(8, '0');
		}

		void SetMAWBCount()
		{
			if (!NumberRangeStart.IsEmpty && !NumberRangeEnd.IsEmpty)
			{
				int startNumber = Convert.ToInt32(NumberRangeStart);
				startNumber = startNumber / 10;

				int endNumber = Convert.ToInt32(NumberRangeEnd);
				endNumber = endNumber / 10;

				MawbCount = endNumber >= startNumber ? endNumber - startNumber + 1 : 0;
			}
		}

		#endregion
	}
}
