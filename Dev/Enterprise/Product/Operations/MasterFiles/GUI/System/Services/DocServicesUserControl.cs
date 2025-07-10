using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DocServicesUserControl : ZUserControl, IBindingMemberForCompileTimeCheckProvider
	{
		public DocServicesUserControl()
		{
			InitializeComponent();
			BindToServices = ServicesControl.ServicesCollectionDefaultBinding;
		}

		public bool ContextColumnVisibleInGrid { get; set; }

		#region Binding

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToServices
		{
			get { return ServicesGrid.BindTo; }
			set
			{
				ServicesGrid.BindTo = value;

				contractorFindBox.BindTo = value + "." + JobServiceSchema.Constants.ES_OH_Contractor;
				notesTextBox.BindTo = value + "." + JobServiceSchema.Constants.ES_ServiceNote;
				referenceTextBox.BindTo = value + "." + JobServiceSchema.Constants.ES_References;
				durationTimeEdit.BindTo = value + "." + JobServiceSchema.Constants.ES_Duration;
				serviceCountCalcEdit.BindTo = value + "." + JobServiceSchema.Constants.ES_ServiceCount;
				serviceLocationAddressControl.BindToAddress = value + "." + JobServiceSchema.Constants.ES_OA_Location;
				serviceLocationAddressControl.BindToOrgList = value + ".Lookups.ServiceProvider";
				completedDateEdit.BindTo = value + "." + JobServiceSchema.Constants.ES_Completed;
				bookedDateEdit.BindTo = value + "." + JobServiceSchema.Constants.ES_Booked;
				serviceTypeDropEdit.BindTo = value + "." + JobServiceSchema.Constants.ES_ServiceCode;
				measurementBasisDropEdit.BindTo = value + "." + JobServiceSchema.Constants.ES_MeasurementBasis;
				rateAndCurrencyCalcFindBox.BindToAmount = value + "." + JobServiceSchema.Constants.ES_ServiceRate;
				rateAndCurrencyCalcFindBox.BindToUnit = value + "." + JobServiceSchema.Constants.ES_RX_NKServiceRateCurrency;
				rateAndCurrencyCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
				rateAndCurrencyCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
				subLocationTextBox.BindTo = value + "." + JobServiceSchema.Constants.ES_SubLocation;
			}
		}

		public IDocsAndCartageParent DocsAndCartageParent
		{
			get { return (IDocsAndCartageParent)CurrentDataItem; }
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			CompileTimeCheckBindingMemberCollection result = new CompileTimeCheckBindingMemberCollection();
			if (!string.IsNullOrEmpty(BindToServices))
			{
				result.Add(new CompileTimeCheckBindingMember(dataSourceType, typeof(JobServiceDependentCollection), new KBindingMemberInfo(dataMember, BindToServices).BindingMember));
			}
			return result;
		}

		#endregion

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			ServicesGrid.SetAvailability(ContextColumnVisibleInGrid, "ParentContextID");

			var contextColumn = ServicesGrid.Columns["ParentContextID"];
			if (contextColumn != null && contextColumn.IsVisible != ContextColumnVisibleInGrid)
			{
				contextColumn.IsVisible = ContextColumnVisibleInGrid;
				ServicesGrid.RefreshTableStyles();
			}
		}
	}
}
