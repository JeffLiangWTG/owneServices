using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business
{
	public class ExportCustomsManifestLines : AutoExportCustomsManifestLines, ISendersMessageReferenceProvider, ITopLevelBizOProviderForJobDocAddress
	{
		public ExportCustomsManifestLines(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ReadOnly(true)]
		public override ZShort EL_LineNo
		{
			get { return base.EL_LineNo; }
			set { base.EL_LineNo = value; }
		}

		protected void PopulateEL_UserRefNumberIfNeeded()
		{
			PopulateNumberPropertyIfRequired(EL_UserReferenceNumInfo, objectFactory =>
			{
				userReferenceAssignedFromNumberFountain = true;
				return new ZString(Env.NumberFountains.ManifestJobLineNo.GetNextFormatted(objectFactory));
			});
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateEL_UserRefNumberIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && userReferenceAssignedFromNumberFountain)
			{
				base.EL_UserReferenceNum = ZString.Empty;
			}
			userReferenceAssignedFromNumberFountain = false;
		}

		bool userReferenceAssignedFromNumberFountain;

		#region ISendersMessageReferenceProvider Members

		void ISendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded()
		{
			PopulateEL_UserRefNumberIfNeeded();
		}

		ZString ISendersMessageReferenceProvider.SendersReference
		{
			get { return EL_UserReferenceNum; }
		}

		#endregion

		public BusinessObject GetTopBusinessObject() => this;
	}
}
