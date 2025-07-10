using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Module;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry.Testing
{
	sealed class NZJobDeclarationFilterStripControlTest : TestCaseWithFactory
	{
		public void TestDoNotShowInvalidColumns()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new NZJobDeclarationFilterBusinessObject();
			using (var form = new ZForm())
			using (var filterStrip = new NZJobDeclarationFilterStripControl(null, declarations, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				var filteredGrid = filterStrip.FilteredGrid;
				AssertNull(filteredGrid.Columns[JobDeclaration.Schema.JE_DateOfFirstArrival]);
				AssertNull(filteredGrid.Columns[JobDeclaration.Schema.JE_RL_NKPortOfFirstArrival]);
				AssertEquals(true, filteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_DateOfFirstArrival).IsUnavailable);
				AssertEquals(true, filteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_RL_NKPortOfFirstArrival).IsUnavailable);
			}
		}

		public void TestFilterStripInheritsJobDeclarationFilterStripControl()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new NZJobDeclarationFilterBusinessObject();
			using (var filterStrip = new NZJobDeclarationFilterStripControl(null, declarations, filterBusinessObject))
			{
				Assert("Doesn't inherit JobDeclarationFilterStripControl", filterStrip is JobDeclarationFilterStripControl);
			}
		}

		public void TestMAFStatusColumnExists()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new NZJobDeclarationFilterBusinessObject();
			using (var filterStrip = new NZJobDeclarationFilterStripControl(null, declarations, filterBusinessObject))
			{
				var isMafStatusExists = false;
				var isMafConsignmentNumberExists = false;
				foreach (var columnStyleObject in filterStrip.FilteredGrid.ColumnStyles)
				{
					if (columnStyleObject is ZTextBoxColumnStyleInfo)
					{
						var textBoxColumnStyle = columnStyleObject as ZTextBoxColumnStyleInfo;
						if (textBoxColumnStyle.ColumnName == "JE_MAF_MessagingStatusDescription")
						{
							isMafStatusExists = true;
						}

						if (textBoxColumnStyle.ColumnName == "JE_MAF_ConsignmentNumber")
						{
							isMafConsignmentNumberExists = true;
						}
					}
				}

				Assert("Missing MPI Status", isMafStatusExists);
				Assert("Missing MPI Consignment Number", isMafConsignmentNumberExists);
			}
		}

		public void TestModeColumnsExists()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new NZJobDeclarationFilterBusinessObject();
			using (var filterStrip = new NZJobDeclarationFilterStripControl(null, declarations, filterBusinessObject))
			{
				var isMessagingModeExists = false;
				var isEFTModeExists = false;
				foreach (var columnStyleObject in filterStrip.FilteredGrid.ColumnStyles)
				{
					if (columnStyleObject is ZTextBoxColumnStyleInfo)
					{
						var textBoxColumnStyle = columnStyleObject as ZTextBoxColumnStyleInfo;
						if (textBoxColumnStyle.ColumnName == "JE_ApplicationCode")
						{
							isMessagingModeExists = true;
						}

						if (textBoxColumnStyle.ColumnName == "JE_ECI_LastResponseStatus")
						{
							isEFTModeExists = true;
						}
					}
				}

				Assert("Missing Messaging Mode column", isMessagingModeExists);
				Assert("Missing EFT Mode column", isEFTModeExists);
			}
		}

		public void TestTSWStatusColumns()
		{
			var currentyCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new NZJobDeclarationFilterBusinessObject();
			var tswStatusExists = false;
			var tswCombinedStatusDescExists = false;
			var nzcsStatusDescExists = false;
			var mpiFoodStatusDescExists = false;
			var mpiBioStatusDescExists = false;
			using (var filterStrip = new NZJobDeclarationFilterStripControl(null, declarations, filterBusinessObject))
			{
				foreach (var columnStyleObject in filterStrip.FilteredGrid.ColumnStyles)
				{
					if (columnStyleObject is ZTextBoxColumnStyleInfo)
					{
						var textBoxColumnStyle = columnStyleObject as ZTextBoxColumnStyleInfo;
						if (textBoxColumnStyle.ColumnName == "JE_TSWCombinedStatus")
						{
							tswStatusExists = true;
						}

						if (textBoxColumnStyle.ColumnName == "JE_TSWCombinedStatusDesc")
						{
							tswCombinedStatusDescExists = true;
						}

						if (textBoxColumnStyle.ColumnName == "NZCSStatusDesc")
						{
							nzcsStatusDescExists = true;
						}

						if (textBoxColumnStyle.ColumnName == "MPIFoodStatusDesc")
						{
							mpiFoodStatusDescExists = true;
						}

						if (textBoxColumnStyle.ColumnName == "MPIBiosecurityStatusDesc")
						{
							mpiBioStatusDescExists = true;
						}
					}
				}

				AssertEquals("JE_TSWCombinedStatus should be available when TSW is active", true, tswStatusExists);
				AssertEquals("JE_TSWCombinedStatusDesc should be available when TSW is active", true, tswCombinedStatusDescExists);
				AssertEquals("NZCSStatusDesc should be available when TSW is active", true, nzcsStatusDescExists);
				AssertEquals("MPIFoodStatusDesc should be availablet when TSW is active", true, mpiFoodStatusDescExists);
				AssertEquals("MPIBiosecurityStatusDesc should be available when TSW is active", true, mpiBioStatusDescExists);
			}
		}

		public void TestEntrySubmitDateFormat()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new NZJobDeclarationFilterBusinessObject();
			using (var userControl = new NZJobDeclarationFilterStripControl(null, declarations, filterBusinessObject))
			{
				var grid = userControl.FilteredGrid;
				var columnInfo = grid.GetColumnStyle("JE_EntrySubmittedDate");
				AssertNotNull(columnInfo);
				var columnStyle = columnInfo as ZDateEditColumnStyleInfo;
				AssertNotNull(columnStyle);
				AssertEquals(ZDateTimePickerFormat.Short, columnStyle.DateTimeFormat);
			}
		}
	}
}
