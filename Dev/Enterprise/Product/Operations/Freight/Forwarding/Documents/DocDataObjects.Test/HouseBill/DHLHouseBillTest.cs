using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class DHLHouseBillTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		const string Content =
			@"[2,3] BILL OF LADING
[3,3] for combined transport or port to port shipment
[5,3] Shipper
[5,42] Document No:
[5,61] B.L. No.
[6,43] S00001000/C00001000
[6,62] S00001000
[8,42] Reference No.
[13,3] Consignee
[13,11]  (not negotiable unless consigned ""to order"", to the order of a named person, or ""to bearer"" )
[13,42] Forwarding Agent - references (complete name and address)
[14,43] EDI CUSTOMS BROKERS
[15,43] 10 HUTCHESON STREET
[16,43] ALBION  QLD
[18,43] AUSTRALIA
[18,61] 4010
[22,3] Notify Party
[22,43] RECEIVED by the Carrier from the Shipper in apparent good order and condition (unless otherwise noted herein) the total number or quantity of containers or other packages or units indicated in the field below entitled ""Number and kind of packages: description of goods"" subject to all the terms hereof (INCLUDING THE TERMS AND CONDITIONS ON THE REVERSE HEREOF (""TERMS AND CONDITIONS"")) from the place of receipt or the port of loading, whichever is applicable, to the port of discharge or the place of delivery, whichever is applicable. In accepting this Bill of Lading, the Merchant (as defined in the Terms and Conditions) expressly accepts and agrees to all its terms, conditions and exceptions whether printed, stamped or written, or otherwise incorporated (including without limitation the Terms and Conditions). 

IN WITNESS WHEREOF the number of original Bills of Lading stated below all of this tenor and date has been signed, one of which being accomplished the others to stand void. The Carrier accepts a duty of reasonable care to check that any document which the Merchant surrenders as a bill of lading is genuine and original. If the Carrier complies with this duty, it will be entitled to deliver the Goods against what it reasonably believes to be a genuine and original bill of lading, such delivery discharging the Carrier's delivery obligations. Where this Bill of Lading is marked ""Express Sea Waybill"" (in which case all references in this document and the Terms and Conditions to this ""Bill of Lading"" shall be deemed to refer to this ""Express Sea Waybill""), delivery may be made (after payment of any outstanding Freight) at the sole discretion of the Carrier, to the nominated person only upon proof of identity. Such delivery shall constitute due delivery hereunder.
[30,3] Vessel
[30,23] Voyage No.
[34,3] Place of Receipt
[34,23] Port of Loading
[34,42] For the release of goods apply to:
[35,4] SYDNEY, AUSTRALIA
[35,24] SYDNEY, AUSTRALIA
[39,3] Port of Discharge
[39,23] Place of Delivery
[46,3] Marks and Nos.
[46,23] Number and kind of packages: description of goods
[46,54] Gross Weight 
in kilos
[46,65] Measurement
in cubic meters
[60,27] FREIGHT PREPAID
[62,3] Total Number of Containers/Packages:
[62,25] 0
[62,44] ABOVE PARTICULARS AS DECLARED BY SHIPPER
[63,3] Freight and Charges
[63,32] Quantity based on
[63,45] Rate
[63,50] Per
[63,54] Prepaid
[63,65] Collect
[71,32] Freight Payable at
[71,45] Place and date of issue
[72,32] SYDNEY
[72,45] BRISBANE, AUSTRALIA
[73,32] Number of Original Bs/L
[73,45] Signed on behalf of the Carrier: Danmar Lines Ltd.
[74,32] 3 / THREE 
[76,45] EDI CUSTOMS BROKERS
[80,3] The Carrier's liability is determined and limited in accordance with clause 8 of the TERMS AND CONDITIONS
[80,57] as agents
[82,3] S00001000,S00001000,HBL
[82,58] ORIGINAL
[84,3] S00001000,S00001000,HBL";

		#region TestDocumentContent

		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var dhlHBLMenuitemPK = new ZGuid("40f9b079-fe3d-4d6b-888e-85f39e960bc5");

			AssertContents(shipment, dhlHBLMenuitemPK, "Original", 0, Content);
		}

		public void TestIncludesMultipleContainersWithPacklines()
		{
			var shipment = GetNewShipment();

			var container1 = shipment.DepartureConsol.Containers.AddNew();
			var container2 = shipment.DepartureConsol.Containers.AddNew();

			container1.ContainerNumberForBinding = "CONTAINER1";
			container2.ContainerNumberForBinding = "CONTAINER2";

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			packLine1.JL_PackageCount = 1;
			packLine2.JL_PackageCount = 2;

			packLine1.Containers.Add(container1);
			packLine2.Containers.Add(container2);
			container1.PackLines.Add(packLine1);
			container2.PackLines.Add(packLine2);

			Factory.Save();

			var loadedSection = @"[48,3] 
                                      1 Pallet(s)                                      0.000                0.000

                             Loaded in Container No. CONTAINER1, Seal No.

                                      2 Pallet(s)                                      0.000                0.000

                             Loaded in Container No. CONTAINER2, Seal No.

";

			var expected = ApplyContainerSectionChanges(Content, loadedSection);
			var dhlHBLMenuitemPK = new ZGuid("40f9b079-fe3d-4d6b-888e-85f39e960bc5");

			AssertContents(shipment, dhlHBLMenuitemPK, "Original", 0, expected);
		}

		public void TestExcludesContainersWithoutPacklines()
		{
			var shipment = GetNewShipment();

			var container1 = shipment.DepartureConsol.Containers.AddNew();
			var container2 = shipment.DepartureConsol.Containers.AddNew();

			container1.ContainerNumberForBinding = "CONTAINER 1";
			container2.ContainerNumberForBinding = "CONTAINER 2";

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			packLine1.JL_PackageCount = 1;
			packLine2.JL_PackageCount = 2;

			packLine1.Containers.Add(container1);
			packLine2.Containers.Add(container1);
			container1.PackLines.Add(packLine1);
			container1.PackLines.Add(packLine2);

			Factory.Save();

			var loadedSection = @"[48,3] 
                                      1 Pallet(s)                                      0.000                0.000

                                      2 Pallet(s)                                      0.000                0.000

                             Loaded in Container No. CONTAINER 1, Seal No.

";

			var expected = ApplyContainerSectionChanges(Content, loadedSection);
			var dhlHBLMenuitemPK = new ZGuid("40f9b079-fe3d-4d6b-888e-85f39e960bc5");

			AssertContents(shipment, dhlHBLMenuitemPK, "Original", 0, expected);
		}

		String ApplyContainerSectionChanges(String content, String loadedSection)
		{
			var expected = content.Insert(content.IndexOf("[60,27]"), loadedSection);
			var regex = new Regex(@"\[62,25\]\s\d+", RegexOptions.Singleline);
			return regex.Replace(expected, "[62,25] 3");
		}

		ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "AUSYD";

			Factory.Save();
			return shipment;
		}

		#endregion

		#region TestPreprintedStaysInSync

		[SnailTest]
		public void TestPreprintedStaysInSync()
		{
			var billOfLadingTemplate = LoadBillOfLadingTemplate("BillOfLadingDHL");
			var billOfLadingPreprintedTemplate = LoadBillOfLadingTemplate("BillOfLadingDHLPreprinted");

			AssertTemplatesHaveSameBeforeDcoumentCreateScript(billOfLadingTemplate, billOfLadingPreprintedTemplate);
			AssertColumnsHaveSameColumnWidths(billOfLadingTemplate, billOfLadingPreprintedTemplate);
			AssertFirstPageSectionsHaveTheSameRowHeights(billOfLadingTemplate, billOfLadingPreprintedTemplate);
			AssertContinuationPageSectionsHaveTheSameRowHeights(billOfLadingTemplate, billOfLadingPreprintedTemplate);
		}

		void AssertTemplatesHaveSameBeforeDcoumentCreateScript(IStandardTemplate template1, IStandardTemplate template2)
		{
			var script1 = GetBeforeDcoumentCreateScript(template1);
			var script2 = GetBeforeDcoumentCreateScript(template2);

			AssertMultilineASCIIEquals($"BeforeDcoumentCreate scripts for {template1.Name} and {template2.Name} do not match",
				script1, script2);
		}

		string GetBeforeDcoumentCreateScript(IStandardTemplate template)
		{
			var builder = new StringBuilder();
			const int firstContentColumn = 2;

			for (int row = template.Config.FirstContentRow; row <= template.Config.LastContentRow; row++)
			{
				var cell = template.GetCell(row, firstContentColumn);

				if (cell != null)
				{
					var macro = Convert.ToString(cell.Value);

					if (!string.IsNullOrWhiteSpace(macro))
					{
						builder.AppendLine(macro);
					}
				}
			}

			return builder.ToString();
		}

		void AssertColumnsHaveSameColumnWidths(IStandardTemplate template1, IStandardTemplate template2)
		{
			AssertEquals("templates should have the same number of columns",
				template1.Columns.Count, template2.Columns.Count);

			CombineAssertions(() =>
			{
				for (int i = 1; i <= template1.Columns.Count; i++)
				{
					var column1 = template1.Columns.GetAt(i);
					var column2 = template2.Columns.GetAt(i);

					AssertEquals($"columns at {i} have different widths: {template1.Name}:{column1.Width}, {template2.Name}:{column2.Width}",
						column1.Width, column2.Width);
				}
			});
		}

		void AssertFirstPageSectionsHaveTheSameRowHeights(IStandardTemplate template1, IStandardTemplate template2)
		{
			AssertRowsHaveSameHeights(template1, template1.Body.First(),
				template2, template2.Body.First());
		}

		void AssertContinuationPageSectionsHaveTheSameRowHeights(IStandardTemplate template1, IStandardTemplate template2)
		{
			AssertRowsHaveSameHeights(template1, template1.Body.Last(),
				template2, template2.Body.Last());
		}

		void AssertRowsHaveSameHeights(IStandardTemplate template1, IBodySection template1BodySection, IStandardTemplate template2, IBodySection template2BodySection)
		{
			var body1NumberOfRows = template1BodySection.LastContentRow - template1BodySection.FirstContentRow;
			var body2NumberOfRows = template2BodySection.LastContentRow - template2BodySection.FirstContentRow;

			AssertEquals($"Section {template1BodySection} and {template2BodySection} should have the same number of rows",
				body1NumberOfRows, body2NumberOfRows);

			var body1Heights = GetRowHeights(template1, template1BodySection);
			var body2Heights = GetRowHeights(template2, template2BodySection);

			CombineAssertions(() =>
			{
				for (int i = 0; i < body1Heights.Length; i++)
				{
					var row1Height = body1Heights[i];
					var row2Height = body2Heights[i];

					AssertEquals($"rows should have same widths: {template1.Name}:{template1BodySection.FirstContentRow + 1}, {template2.Name}:{template2BodySection.FirstContentRow + 1}",
						row1Height, row2Height);
				}
			});
		}

		double[] GetRowHeights(IStandardTemplate template, IBodySection section)
		{
			var heights = new List<double>();

			for (int i = section.FirstContentRow; i <= section.LastContentRow; i++)
			{
				var row = template.Rows.GetAt(i);
				heights.Add(row.Height);
			}

			return heights.ToArray();
		}

		IStandardTemplate LoadBillOfLadingTemplate(string name)
		{
			var query = new ZQuery(StmTemplateSchema.SO_Name, name);
			query.AddToFilter(StmTemplateSchema.SO_TemplateType, StmTemplateTypes.Codes.Form);

			return LoadTemplate(query);
		}

		#endregion
	}
}
