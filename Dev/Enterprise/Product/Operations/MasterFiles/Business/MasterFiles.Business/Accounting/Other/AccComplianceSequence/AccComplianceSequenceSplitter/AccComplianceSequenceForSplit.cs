using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceForSplit : AccComplianceSequence
	{
		public new class Schema : AccComplianceSequence.Schema
		{
			public const string XD_Calc_BookType = "XD_Calc_BookType";
		}

		public AccComplianceSequenceForSplit(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Book Type

		public ZString XD_Calc_BookType
		{
			get
			{
				return IsNewBook ? Res.GetString("141ED318-2161-455F-9D8D-4625B6D38164", "New") : Res.GetString("BAB9B77E-254F-4E09-8C55-6B730C3CBFA1", "Existing");
			}
		}

		public ZPropertyInfo XD_BookTypeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.XD_Calc_BookType);
			}
		}

		#endregion

		#region Readonly Properties
		protected bool XD_BookType_ReadOnly
		{
			get { return true; }
		}

		override protected bool XD_Prefix_ReadOnly
		{
			get { return true; }
		}

		protected bool XD_StartNumber_ReadOnly
		{
			get { return true; }
		}

		protected bool XD_NextNumber_ReadOnly
		{
			get { return true; }
		}

		protected bool XD_EndNumber_ReadOnly
		{
			get { return IsNewBook; }
		}

		protected bool XD_StartDate_ReadOnly
		{
			get { return true; }
		}

		protected bool XD_ExpiryDate_ReadOnly
		{
			get { return IsNewBook; }
		}

		#endregion

		#region Overrides

		public override ZString XD_Calc_StartNumberString
		{
			get => (XD_StartNumber == ZDecimal.Zero) ? ZString.Empty : base.XD_Calc_StartNumberString;
			set => base.XD_Calc_StartNumberString = value;
		}

		public override ZString XD_Calc_EndNumberString
		{
			get => (XD_EndNumber == ZDecimal.Zero) ? ZString.Empty : base.XD_Calc_EndNumberString;
			set => base.XD_Calc_EndNumberString = value;
		}

		public override ZDecimal XD_EndNumber
		{
			get
			{
				return base.XD_EndNumber;
			}
			set
			{
				base.XD_EndNumber = value;
				XD_IsActive = XD_NextNumber > XD_EndNumber ? ZBool.False : ZBool.True;
				SplitController?.UpdateNewSequenceStartNumber();
			}
		}

		public override ZDateTime XD_ExpiryDate
		{
			get
			{
				return base.XD_ExpiryDate;
			}
			set
			{
				base.XD_ExpiryDate = value;
				SplitController?.UpdateNewSequenceStartDate();
			}
		}

		protected override AccComplianceSequenceValidation GetNewValidation()
		{
			return new AccComplianceSequenceForSplitValidation(this);
		}

		protected override void LogEvents()
		{
			SplitController?.CreateOnSavingEvent(IsNewBook);
		}

		#endregion

		public ZDecimal OriginalEndNumber { get; set; }
		public ZDate OriginalExpiryDate { get; set; }
		public ZBool IsNewBook { get; set; }
		public ISplitComplianceSequenceController SplitController { get; set; }
	}
}
