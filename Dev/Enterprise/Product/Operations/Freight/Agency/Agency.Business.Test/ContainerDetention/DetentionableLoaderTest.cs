using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DetentionableLoaderTest : BaseAgencyTest
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLoadDetentionableContainerMovements()
		{
			DetentionTestMovements movements = DetentionTestMovements.CreateAndSave(Factory, "AUBNE", "NLAMS");
			AssertLoadDetentionableContainers("Import AU", movements, DetentionInvoiceType.Codes.Import, "AU", movements.ImportP1C1, movements.ImportOP1C1, movements.ImportP1OC1, movements.ImportFP1C1);
			AssertLoadDetentionableContainers("Export AU", movements, DetentionInvoiceType.Codes.Export, "AU", movements.ExportP1C1, movements.ExportOP1C1, movements.ExportP1OC1, movements.ExportFP1C1);
			AssertLoadDetentionableContainers("Import NL", movements, DetentionInvoiceType.Codes.Import, "NL", movements.ImportP1C1OS);
			AssertLoadDetentionableContainers("Export NL", movements, DetentionInvoiceType.Codes.Export, "NL", movements.ExportP1C1OS);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLoadImportClientsWithDetentionableContainers()
		{
			DetentionTestMovements movements = DetentionTestMovements.CreateAndSave(Factory, "AUBNE", "NLAMS");
			#region Expected All
			const string expected_All = @"
AU|EXP|Client1|Principal1
  EXP-FP1C1
  EXP-OP1C1
  EXP-P1C1
  EXP-P1OC1

AU|EXP|Client1|Principal2
  EXP-P2C1

AU|EXP|Client1|Principal3
  EXP-OP3C1

AU|EXP|Client2|Principal1
  EXP-P1C2

AU|EXP|Client3|Principal1
  EXP-P1OC3

AU|IMP|Client1|Principal1
  IMP-FP1C1
  IMP-OP1C1
  IMP-P1C1
  IMP-P1OC1

AU|IMP|Client1|Principal2
  IMP-P2C1

AU|IMP|Client1|Principal3
  IMP-OP3C1

AU|IMP|Client2|Principal1
  IMP-P1C2

AU|IMP|Client3|Principal1
  IMP-P1OC3

NL|EXP|Client1|Principal1
  EXP-P1C1-OS

NL|EXP|Client2|Principal1
  EXP-FP1C2

NL|IMP|Client1|Principal1
  IMP-P1C1-OS

NL|IMP|Client2|Principal1
  IMP-FP1C2
";
			#endregion
			#region Expected AU
			const string expected_AU = @"
AU|EXP|Client1|Principal1
  EXP-FP1C1
  EXP-OP1C1
  EXP-P1C1
  EXP-P1OC1

AU|EXP|Client1|Principal2
  EXP-P2C1

AU|EXP|Client1|Principal3
  EXP-OP3C1

AU|EXP|Client2|Principal1
  EXP-P1C2

AU|EXP|Client3|Principal1
  EXP-P1OC3

AU|IMP|Client1|Principal1
  IMP-FP1C1
  IMP-OP1C1
  IMP-P1C1
  IMP-P1OC1

AU|IMP|Client1|Principal2
  IMP-P2C1

AU|IMP|Client1|Principal3
  IMP-OP3C1

AU|IMP|Client2|Principal1
  IMP-P1C2

AU|IMP|Client3|Principal1
  IMP-P1OC3
";
			#endregion
			#region Expected NL
			const string expected_NL = @"
NL|EXP|Client1|Principal1
  EXP-P1C1-OS

NL|EXP|Client2|Principal1
  EXP-FP1C2

NL|IMP|Client1|Principal1
  IMP-P1C1-OS

NL|IMP|Client2|Principal1
  IMP-FP1C2
";
			#endregion
			#region Expected Client1
			const string expected_Client1 = @"
AU|EXP|Client1|Principal1
  EXP-FP1C1
  EXP-OP1C1
  EXP-P1C1
  EXP-P1OC1

AU|EXP|Client1|Principal2
  EXP-P2C1

AU|EXP|Client1|Principal3
  EXP-OP3C1

AU|IMP|Client1|Principal1
  IMP-FP1C1
  IMP-OP1C1
  IMP-P1C1
  IMP-P1OC1

AU|IMP|Client1|Principal2
  IMP-P2C1

AU|IMP|Client1|Principal3
  IMP-OP3C1

NL|EXP|Client1|Principal1
  EXP-P1C1-OS

NL|IMP|Client1|Principal1
  IMP-P1C1-OS
";
			#endregion
			#region Expected Principal1
			const string expected_Principal1 = @"
AU|EXP|Client1|Principal1
  EXP-FP1C1
  EXP-OP1C1
  EXP-P1C1
  EXP-P1OC1

AU|EXP|Client2|Principal1
  EXP-P1C2

AU|EXP|Client3|Principal1
  EXP-P1OC3

AU|IMP|Client1|Principal1
  IMP-FP1C1
  IMP-OP1C1
  IMP-P1C1
  IMP-P1OC1

AU|IMP|Client2|Principal1
  IMP-P1C2

AU|IMP|Client3|Principal1
  IMP-P1OC3

NL|EXP|Client1|Principal1
  EXP-P1C1-OS

NL|EXP|Client2|Principal1
  EXP-FP1C2

NL|IMP|Client1|Principal1
  IMP-P1C1-OS

NL|IMP|Client2|Principal1
  IMP-FP1C2
";
			#endregion
			#region Expected IMP
			const string expected_IMP = @"
AU|IMP|Client1|Principal1
  IMP-FP1C1
  IMP-OP1C1
  IMP-P1C1
  IMP-P1OC1

AU|IMP|Client1|Principal2
  IMP-P2C1

AU|IMP|Client1|Principal3
  IMP-OP3C1

AU|IMP|Client2|Principal1
  IMP-P1C2

AU|IMP|Client3|Principal1
  IMP-P1OC3

NL|IMP|Client1|Principal1
  IMP-P1C1-OS

NL|IMP|Client2|Principal1
  IMP-FP1C2
";
			#endregion
			#region Expected EXP
			const string expected_EXP = @"
AU|EXP|Client1|Principal1
  EXP-FP1C1
  EXP-OP1C1
  EXP-P1C1
  EXP-P1OC1

AU|EXP|Client1|Principal2
  EXP-P2C1

AU|EXP|Client1|Principal3
  EXP-OP3C1

AU|EXP|Client2|Principal1
  EXP-P1C2

AU|EXP|Client3|Principal1
  EXP-P1OC3

NL|EXP|Client1|Principal1
  EXP-P1C1-OS

NL|EXP|Client2|Principal1
  EXP-FP1C2
";
			#endregion
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("No Filter", expected_All, Render(DetentionableLoader.LoadClientsWithDetentionableContainers(Factory, movements.CompanyLO.PK, ZGuid.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty)));
				AssertMultilineASCIIEquals("AU", expected_AU, Render(DetentionableLoader.LoadClientsWithDetentionableContainers(Factory, movements.CompanyLO.PK, ZGuid.Empty, ZGuid.Empty, "AU", ZString.Empty)));
				AssertMultilineASCIIEquals("NL", expected_NL, Render(DetentionableLoader.LoadClientsWithDetentionableContainers(Factory, movements.CompanyLO.PK, ZGuid.Empty, ZGuid.Empty, "NL", ZString.Empty)));
				AssertMultilineASCIIEquals("Client1", expected_Client1, Render(DetentionableLoader.LoadClientsWithDetentionableContainers(Factory, movements.CompanyLO.PK, movements.Client1.PK, ZGuid.Empty, ZString.Empty, ZString.Empty)));
				AssertMultilineASCIIEquals("Principal1", expected_Principal1, Render(DetentionableLoader.LoadClientsWithDetentionableContainers(Factory, movements.CompanyLO.PK, ZGuid.Empty, movements.Principal1.PK, ZString.Empty, ZString.Empty)));
				AssertMultilineASCIIEquals("IMP", expected_IMP, Render(DetentionableLoader.LoadClientsWithDetentionableContainers(Factory, movements.CompanyLO.PK, ZGuid.Empty, ZGuid.Empty, ZString.Empty, DetentionInvoiceType.Codes.Import)));
				AssertMultilineASCIIEquals("EXP", expected_EXP, Render(DetentionableLoader.LoadClientsWithDetentionableContainers(Factory, movements.CompanyLO.PK, ZGuid.Empty, ZGuid.Empty, ZString.Empty, DetentionInvoiceType.Codes.Export)));
			});
		}

		#region Implementation
		void AssertLoadDetentionableContainers(string message, DetentionTestMovements movements, string detentionType, string country, params ContainerMovement[] expected)
		{
			AssertContainsExactElementsInAnyOrder(message, (m) => string.Format("{0} ({1})", m.Stock.R6_ContainerNum, m.E9_MovementType), expected, DetentionableLoader.LoadDetentionableContainers(Factory, movements.CompanyLO.PK, movements.Client1.PK, movements.Principal1.PK, detentionType, country));
		}

		string Render(BulkDetentionChild[] children)
		{
			string[] labels = Array.ConvertAll(children, (c) => string.Format("{0}|{1}|{2}|{3}", c.CountryCode, c.DetentionType, c.Client == null ? string.Empty : c.Client.OH_Code.ToString(), c.Principal == null ? string.Empty : c.Principal.OH_Code.ToString()));
			Array.Sort(labels, children);
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < children.Length; i++)
			{
				builder.AppendLine();
				builder.AppendLine(labels[i]);
				ContainerMovement[] movements = Factory.Load<ContainerMovement>(new ZQuery(JobContainerMoveSchema.PK, children[i].MovementPKs));
				string[] containerNumbers = Array.ConvertAll(movements, (m) => m.Stock.R6_ContainerNum.ToString());
				Array.Sort(containerNumbers, movements);
				for (int j = 0; j < containerNumbers.Length; j++)
				{
					builder.Append("  ");
					builder.AppendLine(containerNumbers[j]);
				}
			}

			return builder.ToString();
		}
		#endregion
	}
}
