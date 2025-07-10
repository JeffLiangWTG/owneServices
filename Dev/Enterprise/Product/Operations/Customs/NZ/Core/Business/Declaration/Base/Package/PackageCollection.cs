using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class PackageCollection : BasePackageCollection
	{
		public PackageCollection(PackingGroup master)
			: base(master)
		{
			try
			{
				declaration = (JobDeclaration)master.Declaration;
			}
			catch (InvalidCastException ex)
			{
				string ValueAsString(object obj) => obj?.ToString() ?? "null";

				var declaration = master.Declaration;
				var builder = new ZStringBuilder(ex.Message);
				builder.Append(" Debug info [ ");
				builder.AppendFormat("Current Country/Region Code: {0}, ", Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.Country.Code);
				builder.AppendFormat("Declaration Is In Database: {0}, ", ValueAsString((ZBool)declaration.IsInDatabase));
				builder.AppendFormat("Declaration Application Code: {0}, ", declaration.JE_ApplicationCode);
				builder.AppendFormat("Declaration Country/Region Code: {0}, ", declaration.CountryCode);
				builder.AppendFormat("Declaration Branch Country/Region Code: {0}, ", ValueAsString(declaration.Branch?.Company?.GC_RN_NKCountryCode));
				builder.AppendFormat("Declaration Branch Code: {0}, ", ValueAsString(declaration.Branch?.GB_Code));
				builder.AppendFormat("Bill Number: {0}, ", ValueAsString(master.Bill?.CU_BillNum));
				builder.AppendFormat("Container Number: {0}, ", ValueAsString(master.Container?.CO_ContainerNumber));
				builder.AppendFormat("Bill Dec PK: '{0}', ", ValueAsString(master.Bill?.CU_JE));
				builder.AppendFormat("Container Dec PK: '{0}', ", ValueAsString(master.Container?.CO_JE));
				builder.Append("]");

				ErrorReporter.ReportOnce("NZ.Business.Declaration.PackageCollection CTOR", builder.ToString(), ex);
				throw;
			}

			if (declaration != null)
			{
				UpdatePackageCountValidation();
				declaration.JE_MessageSubTypeInfo.ValueChanged += new EventHandler(JE_MessageSubTypeInfo_ValueChanged);
			}
		}

		readonly JobDeclaration declaration;

		public PackingGroup Master
		{
			get { return (PackingGroup)base.PackingGroup; }
		}

		public new Package this[int index]
		{
			get { return (Package)Elements[index]; }
		}

		public new Package AddNew()
		{
			return (Package)base.AddNew();
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			if (!IsRebuilding && PackingGroup != null && !PackingGroup.IsDeleted && PackingGroup.Bill != null)
			{
				PackingGroup.Bill.Validation.ValidateCU_BillNum();
			}
		}

		void JE_MessageSubTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdatePackageCountValidation();
		}

		void UpdatePackageCountValidation()
		{
			if (declaration != null && declaration.IsECIWriteoff)
			{
				MaxCountValidationEnable(1, "You can only have 1 Package Type per House Bill/Container combination for a write off declaration.");
			}
			else
			{
				MaxCountValidationDisable();
			}
		}
	}
}
