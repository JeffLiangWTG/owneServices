using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class PartAttributeColumnManager
	{
		#region Constructor

		public PartAttributeColumnManager
			(ZGrid grid,
			string expiryDateColumnName,
			string packingDateColumnName,
			string partAttrib1ColumnName,
			string partAttrib2ColumnName,
			string partAttrib3ColumnName,
			string serialNumberColumnName,
			string isExpiredColumnName = null)
		{
			if (grid == null)
			{
				throw new ArgumentNullException(nameof(grid));
			}

			this.Grid = grid;
			this.ExpiryDateColumnName = expiryDateColumnName;
			this.PackingDateColumnName = packingDateColumnName;
			this.PartAttrib1ColumnName = partAttrib1ColumnName;
			this.PartAttrib2ColumnName = partAttrib2ColumnName;
			this.PartAttrib3ColumnName = partAttrib3ColumnName;
			this.SerialNumberColumnName = serialNumberColumnName;
			this.IsExpiredColumnName = isExpiredColumnName;
		}

		public PartAttributeColumnManager
			(ZGrid grid,
			string expiryDateColumnName,
			string packingDateColumnName,
			string partAttrib1ColumnName,
			string partAttrib2ColumnName,
			string partAttrib3ColumnName,
			string serialNumberColumnName,
			string releaseCapturedPartAttrib1ColumnName,
			string releaseCapturedPartAttrib2ColumnName,
			string releaseCapturedPartAttrib3ColumnName,
			string releaseCapturedSerialNumberColumnName)
		{
			if (grid == null)
			{
				throw new ArgumentNullException(nameof(grid));
			}

			this.Grid = grid;
			this.ExpiryDateColumnName = expiryDateColumnName;
			this.PackingDateColumnName = packingDateColumnName;
			this.PartAttrib1ColumnName = partAttrib1ColumnName;
			this.PartAttrib2ColumnName = partAttrib2ColumnName;
			this.PartAttrib3ColumnName = partAttrib3ColumnName;
			this.SerialNumberColumnName = serialNumberColumnName;
			this.ReleaseCapturedPartAttrib1ColumnName = releaseCapturedPartAttrib1ColumnName;
			this.ReleaseCapturedPartAttrib2ColumnName = releaseCapturedPartAttrib2ColumnName;
			this.ReleaseCapturedPartAttrib3ColumnName = releaseCapturedPartAttrib3ColumnName;
			this.ReleaseCapturedSerialNumberColumnName = releaseCapturedSerialNumberColumnName;
		}

		#endregion

		#region Column Control

		public void SetColumns(OrgHeader client)
		{
			var showExpiryDate = (client != null) && client.PartAttributeManager.IsExpiryDateUsedByOrganisation;
			var showPackingDate = (client != null) && client.PartAttributeManager.IsPackingDateUsedByOrganisation;
			var showPartAttrib1 = (client != null) && client.PartAttributeManager.IsPartAttributeUsedByOrganisation(1);
			var showPartAttrib2 = (client != null) && client.PartAttributeManager.IsPartAttributeUsedByOrganisation(2);
			var showPartAttrib3 = (client != null) && client.PartAttributeManager.IsPartAttributeUsedByOrganisation(3);
			var showSerialNumber = (client != null) && client.PartAttributeManager.IsSerialNumberUsedByOrganisation;
			var partAttrib1Name = (client == null) ? ZString.Empty : client.PartAttributeManager.PartAttributeName1;
			var partAttrib2Name = (client == null) ? ZString.Empty : client.PartAttributeManager.PartAttributeName2;
			var partAttrib3Name = (client == null) ? ZString.Empty : client.PartAttributeManager.PartAttributeName3;

			SetColumn(ExpiryDateColumnName, "", showExpiryDate);
			SetColumn(PackingDateColumnName, "", showPackingDate);
			SetColumn(PartAttrib1ColumnName, partAttrib1Name, showPartAttrib1);
			SetColumn(PartAttrib2ColumnName, partAttrib2Name, showPartAttrib2);
			SetColumn(PartAttrib3ColumnName, partAttrib3Name, showPartAttrib3);
			SetColumn(SerialNumberColumnName, "", showSerialNumber);
			SetColumn(ReleaseCapturedPartAttrib1ColumnName, partAttrib1Name + postfixForRCAColumns, showPartAttrib1);
			SetColumn(ReleaseCapturedPartAttrib2ColumnName, partAttrib2Name + postfixForRCAColumns, showPartAttrib2);
			SetColumn(ReleaseCapturedPartAttrib3ColumnName, partAttrib3Name + postfixForRCAColumns, showPartAttrib3);
			SetColumn(ReleaseCapturedSerialNumberColumnName, nameOfRCASerialColumn, showSerialNumber);
			SetColumn(IsExpiredColumnName, "", showExpiryDate);
		}

		public void SetColumns(List<OrgHeader> clients)
		{
			var showExpiryDate = false;
			var showPackingDate = false;
			var showPartAttrib1 = false;
			var showPartAttrib2 = false;
			var showPartAttrib3 = false;
			var showSerialNumber = false;
			var partAttrib1Name = new ZString();
			var partAttrib2Name = new ZString();
			var partAttrib3Name = new ZString();

			foreach (OrgHeader client in clients)
			{
				if (client.PartAttributeManager.IsExpiryDateUsedByOrganisation)
				{
					showExpiryDate = true;
				}

				if (client.PartAttributeManager.IsPackingDateUsedByOrganisation)
				{
					showPackingDate = true;
				}

				if (client.PartAttributeManager.IsSerialNumberUsedByOrganisation)
				{
					showSerialNumber = true;
				}

				if (client.PartAttributeManager.IsPartAttributeUsedByOrganisation(1) &&
					!partAttrib1Name.Contains(client.PartAttributeManager.PartAttributeName1, StringComparison.CurrentCulture))
				{
					showPartAttrib1 = true;
					if (!partAttrib1Name.IsEmpty)
					{
						partAttrib1Name += " / ";
					}

					partAttrib1Name += client.PartAttributeManager.PartAttributeName1;
				}

				if (client.PartAttributeManager.IsPartAttributeUsedByOrganisation(2) &&
					!partAttrib2Name.Contains(client.PartAttributeManager.PartAttributeName2, StringComparison.CurrentCulture))
				{
					showPartAttrib2 = true;
					if (!partAttrib2Name.IsEmpty)
					{
						partAttrib2Name += " / ";
					}

					partAttrib2Name += client.PartAttributeManager.PartAttributeName2;
				}

				if (client.PartAttributeManager.IsPartAttributeUsedByOrganisation(3) &&
					!partAttrib3Name.Contains(client.PartAttributeManager.PartAttributeName3, StringComparison.CurrentCulture))
				{
					showPartAttrib3 = true;
					if (!partAttrib3Name.IsEmpty)
					{
						partAttrib3Name += " / ";
					}

					partAttrib3Name += client.PartAttributeManager.PartAttributeName3;
				}
			}

			SetColumn(ExpiryDateColumnName, "", showExpiryDate);
			SetColumn(PackingDateColumnName, "", showPackingDate);
			SetColumn(PartAttrib1ColumnName, partAttrib1Name, showPartAttrib1);
			SetColumn(PartAttrib2ColumnName, partAttrib2Name, showPartAttrib2);
			SetColumn(PartAttrib3ColumnName, partAttrib3Name, showPartAttrib3);
			SetColumn(SerialNumberColumnName, "", showSerialNumber);
			SetColumn(IsExpiredColumnName, "", showExpiryDate);
		}

		// TODO: Make private when ICustomLabelProvider implemented
		public void SetColumn(string columnName, ZString caption, bool used)
		{
			if (!string.IsNullOrEmpty(columnName))
			{
				Grid.SetColumnVisible(used, columnName);

				if (!caption.IsEmpty)
				{
					Grid.SetColumnCaption(columnName, caption);
				}
			}
		}

		#endregion

		#region Implementation

		readonly ZGrid Grid;
		readonly string ExpiryDateColumnName;
		readonly string PackingDateColumnName;
		readonly string PartAttrib1ColumnName;
		readonly string PartAttrib2ColumnName;
		readonly string PartAttrib3ColumnName;
		readonly string SerialNumberColumnName;
		readonly string ReleaseCapturedPartAttrib1ColumnName;
		readonly string ReleaseCapturedPartAttrib2ColumnName;
		readonly string ReleaseCapturedPartAttrib3ColumnName;
		readonly string ReleaseCapturedSerialNumberColumnName;
		readonly string IsExpiredColumnName;
		readonly string postfixForRCAColumns = ResString.GetMultilingualString("93536280-f76e-4498-a406-0ad914c2d484", ": Release Captured");
		readonly string nameOfRCASerialColumn = ResString.GetMultilingualString("854823b9-2ad3-4afe-87b9-89bfc2361783", "Serial #: Release Captured");

		#endregion
	}
}
