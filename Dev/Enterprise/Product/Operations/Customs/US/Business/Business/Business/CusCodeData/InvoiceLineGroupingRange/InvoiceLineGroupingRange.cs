using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineGroupingRange : Customs.Business.CusCodeData
	{
		public InvoiceLineGroupingRange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				LoadFromCY_Data();
			}
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public const string US_StartSequenceNo = "US_StartSequenceNo";
			public const string US_EndSequenceNo = "US_EndSequenceNo";
		}

		#region US_StartSequenceNo
		public ZShort US_StartSequenceNo
		{
			get { return us_StartSequenceNo; }
			set
			{
				if (SetNonPersistentPropertyValue(US_StartSequenceNoInfo, ref us_StartSequenceNo, value))
				{
					UpdateEndSequenceFromStartSequenceIfNeeded();
					UpdateCY_Data();
				}
			}
		}
		ZShort us_StartSequenceNo;

		public ZPropertyInfo US_StartSequenceNoInfo
		{
			get { return GetZPropertyInfo(Schema.US_StartSequenceNo, "Start Sequence Number"); }
		}

		#endregion

		#region US_EndSequenceNo
		public ZShort US_EndSequenceNo
		{
			get { return us_EndSequenceNo; }
			set
			{
				if (SetNonPersistentPropertyValue(US_EndSequenceNoInfo, ref us_EndSequenceNo, value))
				{
					UpdateCY_Data();
				}
			}
		}
		ZShort us_EndSequenceNo;

		public ZPropertyInfo US_EndSequenceNoInfo
		{
			get { return GetZPropertyInfo(Schema.US_EndSequenceNo, "End Sequence Number"); }
		}
		#endregion

		public int NoOfSequences
		{
			get { return US_StartSequenceNo > 0 && US_EndSequenceNo > 0 ? US_EndSequenceNo - US_StartSequenceNo + 1 : 0; }
		}

		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set
			{
				ZString oldValue = CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != CY_Data)
				{
					LoadFromCY_Data();
				}
			}
		}

		public new JobComInvoiceLine Parent
		{
			get
			{
				if (fParent == null || fParent.PK != CY_ParentID)
				{
					fParent = (JobComInvoiceLine)base.Parent;
				}
				return fParent;
			}
			set
			{
				base.Parent = value;
				fParent = value;
			}
		}
		JobComInvoiceLine fParent;

		#region CY_ParentID
		[BusinessObjectTestExclude]
		public override ZGuid CY_ParentID
		{
			get { return base.CY_ParentID; }
			set
			{
				if (!IsCopying && value.IsEmpty)
				{
					cY_ParentIDCachedOnRelationshipResetByCore = base.CY_ParentID;
				}
				ZGuid oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID)
				{
					if (!oldValue.IsEmpty && !CY_ParentID.IsEmpty)
					{
						throw new NotSupportedException("Setting InvoiceLineGroupingRange.CY_ParentID is not supported.");
					}
					RefreshAIILines();
				}
			}
		}
		ZGuid cY_ParentIDCachedOnRelationshipResetByCore;
		#endregion

		[BusinessObjectTestExclude]
		public override ZString CY_ParentTableCode
		{
			get { return base.CY_ParentTableCode; }
			set
			{
				if (!value.IsEmpty && value != JobComInvoiceLineSchema.Constants.Prefix)
				{
					throw new NotSupportedException("Setting InvoiceLineGroupingRange.CY_ParentTableCode is not supported.");
				}
				base.CY_ParentTableCode = value;
			}
		}

		#region InvoiceLine
		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null || fInvoiceLine.PK != CY_ParentID)
				{
					ZGuid reference = CY_ParentID.IsEmpty ? cY_ParentIDCachedOnRelationshipResetByCore : CY_ParentID;
					fInvoiceLine = Factory.Load<JobComInvoiceLine>(reference);
				}
				return fInvoiceLine != null && !fInvoiceLine.IsDeleted ? fInvoiceLine : null;
			}
		}
		JobComInvoiceLine fInvoiceLine;
		#endregion

		public IReadOnlyList<AIILine> AIILines {
			get
			{
				if (isDeleteLinkedAIILinesReferenceInProgress)
				{
					return Array.Empty<AIILine>();
				}

				if (aiiLines == null)
				{
					JobComInvoiceLine invoiceLine = InvoiceLine;
					List<AIILine> result = new List<AIILine>();
					if (invoiceLine != null)
					{
						JobComInvoiceLine parentLine = invoiceLine.ParentTariffLine;
						if (parentLine != null)
						{
							invoiceLine = parentLine;
						}

						if (invoiceLine.IsValidForAII)
						{
							result.AddRange(invoiceLine.AIILines.OfType<AIILine>().Where(x => x.US_CY_LineGroupRef == PK));
						}

						foreach (JobComInvoiceLine childLine in invoiceLine.ChildLines)
						{
							if (childLine.IsValidForAII)
							{
								result.AddRange(childLine.AIILines.OfType<AIILine>().Where(x => x.US_CY_LineGroupRef == PK));
							}
						}
					}
					aiiLines = result.ToArray();
				}
				return aiiLines;
			}
		}
		AIILine[] aiiLines;

		public void RefreshAIILines()
		{
			aiiLines = null;
		}

		public new InvoiceLineGroupingRangeValidation Validation
		{
			get { return (InvoiceLineGroupingRangeValidation)base.Validation; }
		}

		public new InvoiceLineGroupingRangeLookups Lookups
		{
			get { return (InvoiceLineGroupingRangeLookups)base.Lookups; }
		}

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				JobComInvoiceLine invoiceLine = InvoiceLine;
				if (invoiceLine == null || (!invoiceLine.IsDeleted && invoiceLine.IsChildLine))
				{
					Delete();
				}
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			JobComInvoiceLine invoiceLine = null;
			if (!IsDeleted)
			{
				invoiceLine = InvoiceLine;
				DeleteLinkedAIILinesReference(invoiceLine);
			}
			base.Delete();
			if (invoiceLine != null)
			{
				invoiceLine.AIILines.UpdateLineGroupingDetails();
			}
		}

		#region Implementation

		void UpdateEndSequenceFromStartSequenceIfNeeded()
		{
			if (US_EndSequenceNo.IsEmpty)
			{
				US_EndSequenceNo = US_StartSequenceNo;
			}
		}

		void DeleteLinkedAIILinesReference(JobComInvoiceLine invoiceLine)
		{
			if (!isDeleteLinkedAIILinesReferenceInProgress)
			{
				try
				{
					var aiiLines = AIILines;
					isDeleteLinkedAIILinesReferenceInProgress = true;
					bool shouldDelete = invoiceLine == null || invoiceLine.LineGroupingRanges.Count <= 1; // it's the only line grouping
					foreach (AIILine aiiLine in aiiLines)
					{
						if (shouldDelete)
						{
							aiiLine.Delete();
						}
						else
						{
							aiiLine.US_CY_LineGroupRef = ZGuid.Empty;
						}
					}
				}
				finally
				{
					isDeleteLinkedAIILinesReferenceInProgress = false;
				}
			}
		}
		bool isDeleteLinkedAIILinesReferenceInProgress;

		void LoadFromCY_Data()
		{
			if (!updatingCY_DataInProgress)
			{
				try
				{
					loadingFromCY_DataInProgress = true;
					ZString startSequenceNoString = ZString.Empty;
					ZString endSequenceNoString = ZString.Empty;
					ZString[] elements = CY_Data.Split(rangeSeparator);
					if (elements.Length == 2)
					{
						startSequenceNoString = elements[0];
						endSequenceNoString = elements[1];
					}
					US_StartSequenceNo = ZShort.ParseSafe(startSequenceNoString, ZShort.Zero);
					US_EndSequenceNo = ZShort.ParseSafe(endSequenceNoString, ZShort.Zero);
				}
				finally
				{
					loadingFromCY_DataInProgress = false;
				}
			}
		}
		bool loadingFromCY_DataInProgress;

		void UpdateCY_Data()
		{
			if (!loadingFromCY_DataInProgress)
			{
				try
				{
					updatingCY_DataInProgress = true;
					CY_Data = US_StartSequenceNo.ToString() + rangeSeparator + US_EndSequenceNo.ToString();
				}
				finally
				{
					updatingCY_DataInProgress = false;
				}
			}
		}
		bool updatingCY_DataInProgress;
		readonly char rangeSeparator = ':';

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.InvoiceLineNumberRange;
			CY_Code = CusCodeDataTypeList.Codes.InvoiceLineNumberRange;
			CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new InvoiceLineGroupingRangeValidation(this);
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new InvoiceLineGroupingRangeLookups(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine)); }
		}

		#endregion
	}
}
