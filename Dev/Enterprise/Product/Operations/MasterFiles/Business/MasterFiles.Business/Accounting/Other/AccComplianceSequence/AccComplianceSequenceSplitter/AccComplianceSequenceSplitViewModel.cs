using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceSplitViewModel : NonPersistentBusinessObject, ISplitComplianceSequenceController
	{
		public AccComplianceSequenceSplitViewModel(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccComplianceSequenceSplitViewModel(BusinessObjectFactory factory, ZGuid sequencePK) : base(factory)
		{
			SequencePK = sequencePK;
		}

		#region Sequences

		public AccComplianceSequenceForSplitCollection Sequences
		{
			get
			{
				if (sequences == null)
				{
					existSequence = Factory.Load<AccComplianceSequenceForSplit>(SequencePK);
					ZQuery filter = null;

					if (existSequence != null)
					{
						newSequence = Factory.New<AccComplianceSequenceForSplit>();

						using (existSequence.GetValidationSuspender())
						using (newSequence.GetValidationSuspender())
						{
							Initialize(existSequence, newSequence);
						}

						filter = new ZQuery(AccComplianceSequenceSchema.PK, existSequence.PK).
							AddToFilter(JoinCondition.Or, AccComplianceSequenceSchema.PK, newSequence.PK);
					}
					else
					{
						filter = new ZQuery(AccComplianceSequenceSchema.PK, ZGuid.Empty);
					}

					sequences = new AccComplianceSequenceForSplitCollection(Factory, filter);
					RegisterEditableChildObject(sequences);
				}

				return sequences;
			}
		}

		AccComplianceSequenceForSplitCollection sequences;
		readonly ZGuid SequencePK;
		AccComplianceSequenceForSplit newSequence, existSequence;

		#endregion

		#region ISplitComplianceSequenceController

		void ISplitComplianceSequenceController.UpdateNewSequenceStartNumber()
		{
			newSequence.XD_StartNumber = (existSequence.XD_EndNumber == ZDecimal.Zero) ? existSequence.XD_EndNumber : (ZDecimal)(existSequence.XD_EndNumber + 1);
			newSequence.XD_NextNumber = newSequence.XD_StartNumber;
		}

		void ISplitComplianceSequenceController.UpdateNewSequenceStartDate()
		{
			newSequence.XD_StartDate = (existSequence.XD_ExpiryDate.IsValid) ? existSequence.XD_ExpiryDate.Date.AddDays(1) : ZDate.Empty;
		}

		AccComplianceSequenceForSplit ISplitComplianceSequenceController.ExistSequence => existSequence;
		AccComplianceSequenceForSplit ISplitComplianceSequenceController.NewSequence => newSequence;

		void ISplitComplianceSequenceController.CreateOnSavingEvent(bool isNewBook)
		{
			if (!newSequence.IsInDatabase)
			{
				if (isNewBook)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					newSequence.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Book split from {0} with Start Number = {1}, Valid From Date = {2}",
						existSequence.XD_Code, newSequence.XD_Calc_StartNumberString, newSequence.XD_StartDate.ToString()));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					existSequence.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Book split into {0} with End Number = {1}, Expiry Date = {2}",
						newSequence.XD_Code, existSequence.XD_Calc_EndNumberString, existSequence.XD_ExpiryDate.Date.ToString()));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		#endregion

		#region Implementation

		void Initialize(AccComplianceSequenceForSplit existBook, AccComplianceSequenceForSplit newBook)
		{
			existBook.IsNewBook = false;
			existBook.SplitController = this;
			existBook.OriginalEndNumber = existBook.XD_EndNumber;
			existBook.OriginalExpiryDate = existBook.XD_ExpiryDate.Date;

			existBook.XD_EndNumber = ZDecimal.Zero;
			existBook.XD_ExpiryDate = ZDateTime.Empty;

			newBook.IsNewBook = true;
			newBook.SplitController = this;
			newBook.OriginalEndNumber = existBook.OriginalEndNumber;
			newBook.OriginalExpiryDate = existBook.OriginalExpiryDate;

			newBook.XD_SequenceClass = existBook.XD_SequenceClass;
			newBook.XD_Description = existBook.XD_Description;
			newBook.XD_AllocationLevel = existBook.XD_AllocationLevel;
			newBook.XD_GC_Company = existBook.XD_GC_Company;
			newBook.XD_GB_BranchOwner = existBook.XD_GB_BranchOwner;
			newBook.XD_GE_Department = existBook.XD_GE_Department;
			newBook.XD_MaximumNumberDigits = existBook.XD_MaximumNumberDigits;
			newBook.XD_SU_MenuItem = existBook.XD_SU_MenuItem;
			newBook.XD_MaxChargesPerTransaction = existBook.XD_MaxChargesPerTransaction;
			newBook.XD_RollupBehaviourWhenMaxExceeded = existBook.XD_RollupBehaviourWhenMaxExceeded;
			newBook.XD_SO_ComplianceTemplate = existBook.XD_SO_ComplianceTemplate;
			newBook.XD_SQ_DocumentPrintQueue = existBook.XD_SQ_DocumentPrintQueue;
			newBook.XD_Prefix = existBook.XD_Prefix;
			newBook.XD_StartNumber = existBook.XD_EndNumber;
			newBook.XD_EndNumber = existBook.OriginalEndNumber;
			newBook.XD_NextNumber = existBook.XD_EndNumber;
			newBook.XD_StartDate = existBook.XD_ExpiryDate.Date;
			newBook.XD_ExpiryDate = existBook.OriginalExpiryDate;
			newBook.XD_NumberFormat = existBook.XD_NumberFormat;
		}

		#endregion
	}

	public interface ISplitComplianceSequenceController
	{
		void UpdateNewSequenceStartNumber();
		void UpdateNewSequenceStartDate();
		AccComplianceSequenceForSplit ExistSequence { get; }
		AccComplianceSequenceForSplit NewSequence { get; }
		void CreateOnSavingEvent(bool isNewbook);
	}
}
