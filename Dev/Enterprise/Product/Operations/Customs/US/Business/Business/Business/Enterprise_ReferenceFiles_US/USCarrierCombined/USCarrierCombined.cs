using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCarrierCombinedSchema.Constants.UI_Code), DescriptionProperty(USCarrierCombinedSchema.Constants.UI_Name)]
	public class USCarrierCombined : AutoUSCarrierCombined, IStmALogParent, ITemplateCopyable, IUSCarrierCombined
	{
		#region Constructors

		public USCarrierCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		public override bool SupportsNotes
		{
			get { return false; }
		}

		[List(nameof(Lookups) + "." + nameof(USCarrierCombinedLookups.TransportList))]
		public override ZString UI_ModeOfTransportation
		{
			get { return base.UI_ModeOfTransportation; }
			set
			{
				var oldValue = UI_ModeOfTransportation;
				base.UI_ModeOfTransportation = value;
				if (!IsCopying && oldValue != UI_ModeOfTransportation && !IsAir)
				{
					UI_AirwayBillPrefix = ZString.Empty;
				}
			}
		}

		protected bool UI_AirwayBillPrefix_ReadOnly
		{
			get { return !IsAir; }
		}

		[ReadOnly(true)]
		public override ZBool UI_IsSystem
		{
			get { return base.UI_IsSystem; }
			set { base.UI_IsSystem = value; }
		}

		public bool IsAir
		{
			get { return UI_ModeOfTransportation == TransportModeCodes.Codes.AirNonContainer; }
		}

		public bool IsTruck
		{
			get { return UI_ModeOfTransportation == TransportModeCodes.Codes.TruckNonContainer; }
		}

		public override bool ReadOnly
		{
			get { return UI_IsSystem || base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			UI_ModeOfTransportation = TransportModeCodes.Codes.VesselNonContainer;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("C0FA16AA-63CC-47B6-AC16-64450C9D5297", "Carrier {0}", UI_Code);

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !UI_IsSystem; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return CannotDeleteSystemGenerated; }
		}

		internal readonly MultilingualString CannotDeleteSystemGenerated = ResString.GetMultilingualString("DBD4900A-2FAD-4508-A5FD-A45B80F3629D", "You cannot delete a system-generated record.");

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return AutoUSCarrierCombined.Schema.TableName; }
		}

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			var result = (USCarrierCombined)Clone();
			result.UI_IsSystem = ZBool.False;
			return result;
		}

		#endregion
	}
}
