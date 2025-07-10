using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class ReSequencer : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ReSequencer(IEnumerable<ISequenceNumber> sequences, BusinessObjectFactory factory)
			: base(factory)
		{
			this.sequences = Argument.NotNull(sequences, nameof(sequences)).OrderBy(x => x.SequenceNumber).ToArray();
		}

		public void ReSequence()
		{
			if (StartSequence > 0 && SequenceStep > 0)
			{
				var sequenceNumber = StartSequence;
				var step = SequenceStep;
				foreach (var link in sequences)
				{
					link.SequenceNumber = sequenceNumber;
					if (sequenceNumber != short.MaxValue)
					{
						try
						{
							sequenceNumber += step;
						}
						catch (OverflowException)
						{
							sequenceNumber = short.MaxValue;
						}
					}
				}
			}
		}

		[ResourceStringData("ReSequencer.StartSequence", Caption = "Start Sequence")]
		public ZShort StartSequence
		{
			get { return startSequence; }
			set
			{
				SetNonPersistentPropertyValue(StartSequenceInfo, ref startSequence, value);
				if (!IsValidationSuspended)
				{
					ValidateStartSequence();
				}
			}
		}
		ZShort startSequence;

		public ZPropertyInfo StartSequenceInfo
		{
			get { return GetZPropertyInfo(nameof(StartSequence)); }
		}

		void ValidateStartSequence()
		{
			StartSequenceInfo.ClearAllNotifications();
			CompareValidation.CheckGreaterThanOrEqualTo(StartSequenceInfo, 1);
		}

		[ResourceStringData("ReSequencer.SequenceStep", Caption = "Sequence Step")]
		public ZShort SequenceStep
		{
			get { return sequenceStep; }
			set
			{
				SetNonPersistentPropertyValue(SequenceStepInfo, ref sequenceStep, value);
				if (!IsValidationSuspended)
				{
					ValidateSequenceStep();
				}
			}
		}
		ZShort sequenceStep;

		public ZPropertyInfo SequenceStepInfo
		{
			get { return GetZPropertyInfo(nameof(SequenceStep)); }
		}

		void ValidateSequenceStep()
		{
			SequenceStepInfo.ClearAllNotifications();
			CompareValidation.CheckGreaterThanOrEqualTo(SequenceStepInfo, 1);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SequenceStep = 1;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateStartSequence();
			ValidateSequenceStep();
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("{ADB421AF-F35D-46F5-8084-F8A052A35F54}", "Re-Sequencer"); }
		}
		readonly ISequenceNumber[] sequences;
	}
}
