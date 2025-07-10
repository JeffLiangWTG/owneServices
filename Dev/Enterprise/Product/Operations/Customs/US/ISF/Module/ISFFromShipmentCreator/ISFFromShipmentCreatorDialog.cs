using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Module
{
	public partial class ISFFromShipmentCreatorDialog : ZChildForm
	{
		public ISFFromShipmentCreatorDialog()
		{
		}

		public ISFFromShipmentCreatorDialog(ISFFromShipmentCreator creator)
			: base(creator)
		{
		}

		public new ISFFromShipmentCreator BusinessEntity
		{
			get { return (ISFFromShipmentCreator)base.BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return Text; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		void CreateButton_Click(object sender, System.EventArgs e)
		{
			ValidateAll(ValidationType.Full);
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
			}
			else if (IsOkToCreateNewISF(BusinessEntity.Shipment))
			{
				createClicked = true;
				Close();
			}
		}

		protected override DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			using (var msgBox = new ZErrorMessageBox(BusinessEntityForValidation, "ISF Job", "create", "created", includeIgnoreOption))
			{
				return ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
			}
		}

		bool IsOkToCreateNewISF(ForwardingShipment selectedShipment)
		{
			Dictionary<ZString, ZString[]> existingISFs = new Dictionary<ZString, ZString[]>();
			foreach (ZString billNumber in GetBillNumbers(selectedShipment))
			{
				if (!existingISFs.ContainsKey(billNumber))
				{
					ZDBOnlyQuery isfQuery = new ZDBOnlyQuery(typeof(CusISFHeader));
					ZDBOnlySubQuery billQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF);
					billQuery.AddToFilter(CusISFBillSchema.BB_BillNum, billNumber);
					billQuery.AddToFilter(CusISFBillSchema.BB_BillType, new ZString[] { BillTypeList.Codes.HouseBillOfLading, BillTypeList.Codes.MasterBillOfLading, BillTypeList.Codes.OceanBillOfLading });
					isfQuery.AddSubQuery(billQuery, JoinCondition.Or);
					isfQuery.OrderBy = CusISFHeader.Schema.BF_JobReference;
					List<ZString> isfList = new List<ZString>();
					foreach (CusISFHeader existingISF in selectedShipment.Factory.Load<CusISFHeader>(isfQuery))
					{
						isfList.Add(existingISF.BF_JobReference);
					}

					if (isfList.Count > 0)
					{
						existingISFs.Add(billNumber, isfList.ToArray());
					}
				}
			}
			return existingISFs.Count == 0 || Globals.Message.ShowConfirmation(GetExistingISFMessage(existingISFs), "ISF Job Matching Bill Number Exist", "Please type the following if you still want to create a new ISF Job: ", "yes", MessageBoxIcon.Warning) == DialogResult.OK;
		}

		IEnumerable<ZString> GetBillNumbers(ForwardingShipment selectedShipment)
		{
			JobDeclaration declaration = selectedShipment.DeclarationForDocuments as JobDeclaration;
			if (declaration != null)
			{
				foreach (Bill bill in declaration.Bills)
				{
					if (bill.IsHouseBill || bill.IsMasterBill)
					{
						yield return bill.CU_BillNum;
					}
				}
			}
			else
			{
				ForwardingConsol consol = Enterprise.Customs.Business.ShipmentExtensions.ConsolForCountry(selectedShipment, Core.Constants.CountryCodes.UnitedStates);
				if (consol != null)
				{
					yield return consol.JK_MasterBillNum;
				}
				yield return selectedShipment.JS_HouseBill;
			}
		}

		string GetExistingISFMessage(Dictionary<ZString, ZString[]> existingISFs)
		{
			ZStringBuilder builder = new ZStringBuilder();
			builder.Append(string.Format("The following bill number{0} already in use on following ISF Job(s):", existingISFs.Count > 1 ? "s are" : " is"));
			builder.Append("");
			foreach (KeyValuePair<ZString, ZString[]> pair in existingISFs)
			{
				builder.Append("Bill '" + pair.Key + "':");
				ZStringBuilder jobReferences = new ZStringBuilder();
				int isfCount = 0;
				foreach (ZString jobReference in pair.Value)
				{
					jobReferences.Append(jobReference);
					if (++isfCount % 10 == 0)
					{
						builder.Append(jobReferences.ToStringWithDelimiterBetweenAppends(", "));
						jobReferences = new ZStringBuilder();
					}
				}
				if (!jobReferences.IsEmpty)
				{
					builder.Append(jobReferences.ToStringWithDelimiterBetweenAppends(", "));
				}
				builder.Append("");
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		public bool CreateClicked
		{
			get { return createClicked; }
		}
		bool createClicked;
	}
}
