using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace ClientSpecificSQLFromXLSX
{
	class Program
	{
		static void Main(string[] args)
		{
			// Process args
			if (args.Count() < 2)
			{
				Console.WriteLine("Please run as:");
				Console.WriteLine("    ClientSpecificSQLFromXLSX interfaces.xlsx XXX");
				Console.WriteLine("Where XXX is the 3 letter code in the title of the WI.");
				return;
			}
			var file_name = args[0];
			var simple_code = args[1];

			// Setup excel interop
			var app = new Excel.Application();
			var workbook = app.Workbooks.Open(Directory.GetCurrentDirectory() + file_name, Password: "ehubrocks", ReadOnly: true);
			var sheets = workbook.Sheets;
			var sheet = sheets[1];
			var range = sheet.UsedRange;
			var values = (object[,])range.Value2;

			try
			{
				// Find the row with the matching simple_code and the status is WIP
				int row = 2;
				while (values[row,1] == null || values[row, 1].ToString().Trim() != simple_code || values[row, 17] == null || values[row, 17].ToString().Trim() != "WIP")
				{
					// Check for this magic string to see when the rows end.
					var magic = values[row, 1];
					if (magic != null && magic.ToString() == "LOST eHub Interfaces")
					{
						Console.WriteLine("Could not find any WIP interfaces for client {0}.", simple_code);
						return;
					}
					row++;
				}

				var sql = @"USE eHubTransactions
BEGIN tran";
				string sql_format;

				// retrieve the OrgHeader PK from production ediProdCache
				var connection = new SqlConnection("user id=ehubreader;password=ehubrocks;server=wg1-vsql-1.wg.cargowise.com;database=ediProdCache");
				connection.Open();
				var command = connection.CreateCommand();
				var ediprod_client_name = values[row, 2].ToString().Trim();
				command.CommandText = String.Format("SELECT OH_PK FROM OrgHeader WHERE OH_Code = '{0}'", ediprod_client_name);
				var oh_pk = command.ExecuteScalar();

				var billing_element = values[row, 14].ToString().Trim();
				var billing_xpath = "TODO";
				if (billing_element == "per message")
				{
					billing_xpath = "/*";
				}
				else
				{
					Console.WriteLine("ERROR: Unknown billing XPATH. Please add it to this script and try again.");
				}

				var interface_name = values[row, 23].ToString().Trim();
				// This is a really dumb heuristic, maybe I should just get Kirsten to put these in a standard format.
				var billing_name = values[row, 5].ToString().Trim();
				using (StringReader sr = new StringReader(billing_name))
				{
					var line1 = sr.ReadLine();
					var line2 = sr.ReadLine();
					var line3 = sr.ReadLine();
					if (line3 != null)
					{
						Console.WriteLine("ERROR: Billing Name has 3 or more lines, please fill it out yourself.");
						billing_name = "TODO";
					}
					else if (line2 != null)
					{
						billing_name = line2;
					}
					else if (line1 != null)
					{
						billing_name = line1;
					}
				}

				var interface_number = 0;

				// for each client in the interface
				do
				{
					if (values[row, 19] == null) {
						Console.WriteLine("ERROR: Sender ID field is empty, interface skipped.");
						row++;
						continue;
					}
					var cc_id_sender = values[row, 19].ToString().Trim();
					var cc_id_recipient = values[row, 21].ToString().Trim();

					// sometimes relies on an earlier value
					dynamic cc_friendly_name_sender_cell = null;
					var try_previous = 0;
					while (cc_friendly_name_sender_cell == null)
					{
						cc_friendly_name_sender_cell = values[row - try_previous, 20];
						try_previous++;
					}
					var cc_friendly_name_sender = cc_friendly_name_sender_cell.ToString().Trim();

					// sometimes relies on an earlier value
					dynamic cc_friendly_name_recipient_cell = null;
					try_previous = 0;
					while (cc_friendly_name_recipient_cell == null)
					{
						cc_friendly_name_recipient_cell = values[row - try_previous, 22];
						try_previous++;
					}
					var cc_friendly_name_recipient = cc_friendly_name_recipient_cell.ToString().Trim();

					var cc_pk_sender = Guid.NewGuid().ToString();
					var cc_pk_recipient = Guid.NewGuid().ToString();

					sql += String.Format("\n\n-- interface #{0}", interface_number);

					// CW1 clients (identifiable by Length == 9) are inserted to eHubTransactions via a service task, we should NEVER insert them ourself.
					if (cc_id_sender.Length != 9)
					{
						sql_format = @"
INSERT INTO eHubClient(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_DistributionZone)
	VALUES('{0}', '{1}', '{2}', '{3}', '', '', 'Client', 'Third Party', '75419F4C-C522-4890-BD5D-BCA5E12268F6')";
						sql += String.Format(sql_format, cc_pk_sender, cc_id_sender, cc_friendly_name_sender, oh_pk);
					}

					// CW1 clients (identifiable by Length == 9) are inserted to eHubTransactions via a service task, we should NEVER insert them ourself.
					if (cc_id_recipient.Length != 9)
					{
						sql_format = @"
INSERT INTO eHubClient(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_DistributionZone)
	VALUES('{0}', '{1}', '{2}', '{3}', '', '', 'Client', 'Third Party', '75419F4C-C522-4890-BD5D-BCA5E12268F6')";
						sql += String.Format(sql_format, cc_pk_recipient, cc_id_recipient, cc_friendly_name_recipient, oh_pk);
					}

					var ts_pk = Guid.NewGuid().ToString();
					var ts_bill_sender = "0";
					var ts_bill_recipient = "0";
					var ts_cc_sender = "null";
					var ts_cc_recipient = "null";
					if (cc_id_sender.Length == 9)
					{
						ts_bill_sender = "1";
						ts_cc_recipient = "'" + cc_pk_recipient + "'";
					}
					else
					{
						ts_bill_recipient = "1";
						ts_cc_sender = "'" + cc_pk_sender + "'";
					}
					sql_format = @"

INSERT INTO eHubTransformationSet(TS_PK, TS_CC_Sender, TS_CC_Recipient, TS_Name, TS_BillingInterfaceName, TS_BillingElement, TS_BillingXPathSource, TS_BillSender, TS_BillRecipient)
	VALUES('{0}', {1}, {2}, '{3}', '{4}', '{5}', '{6}', {7}, {8})";
					sql += String.Format(sql_format, ts_pk, ts_cc_sender, ts_cc_recipient, interface_name, billing_name, billing_element, billing_xpath, ts_bill_sender, ts_bill_recipient);

					var tt_pk = Guid.NewGuid().ToString();
					sql_format = @"

INSERT INTO eHubTransformationType(TT_PK, TT_DT_Source, TT_DT_Target, TT_TransformationType, TT_Target_Version)
	VALUES('{0}', 'TODO', 'TODO', 'CargoWise.eHub.Clients.{1}.TODO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350', NULL)

INSERT INTO eHubTransformationMapping(TM_TS_PK, TM_Order, TM_TT_PK)
	VALUES('{2}', 0, '{0}')
";
					sql += String.Format(sql_format, tt_pk, simple_code, ts_pk);

					row++;
					interface_number++;
				} while (values[row, 1] == null || values[row, 1].ToString().Trim() == simple_code); // The first column only contains a value on the first row of an interface.
				sql += @"

--COMMIT
ROLLBACK";
				Console.WriteLine(sql);
			}
			finally
			{
				// Cleanup
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Marshal.ReleaseComObject(range);
				Marshal.ReleaseComObject(sheet);
				Marshal.ReleaseComObject(sheets);
				workbook.Close();
				Marshal.ReleaseComObject(workbook);
				app.Quit();
				Marshal.ReleaseComObject(app);
			}
		}
	}
}
