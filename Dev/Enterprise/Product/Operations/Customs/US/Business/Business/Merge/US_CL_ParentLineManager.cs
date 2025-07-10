using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// When CusEntryLines for all invoice lines are generated, this sets relationships between CusEntryLines.
	/// 
	/// 1. Watch Repair
	///		Line 1		9802		
	///		Line 1/1	Movement	--> Points to Line 1
	///		Line 1/2	9802		--> Points to Line 1
	///		Line 1/3	Case		--> Points to Line 1
	///		etc
	///		
	///	2. X/V Assembly
	///		Line 1		9802		
	///		Line 1/1	X Tariff	--> Points to Line 1
	///		Line 2		9802		--> Points to Line 1
	///		Line 2/1	V Tariff	--> Points to **Line 2**
	///		Line 3		V Tariff	--> Points to **Line 1**
	///		
	/// 3. Watch
	///		Line 1		Movement
	///		Line 1/1	Case		--> Points to Line 1
	///		Line 1/2	Battery		--> Points to Line 1
	///		etc
	/// </summary>
	class US_CL_ParentLineManager
	{
		public void Manage(JobDeclaration declaration)
		{
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (ShouldBeManaged(entry))
				{
					foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
					{
						var entryLine = invoiceLine.GetEntryLineFor(entry.CH_MessageType, false);

						if (entryLine != null)
						{
							var parentTariffLine = invoiceLine.ParentTariffLine;

							if (parentTariffLine != null)
							{
								var parentLineForSupEntryLine = parentTariffLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine) ?? parentTariffLine.GetEntryLineFor(entry.CH_MessageType, true) ?? parentTariffLine.GetEntryLineFor(entry.CH_MessageType, false);
								var parentLineForRegularEntryLine = parentLineForSupEntryLine;
								var supAdditionalLine = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine);
								if (supAdditionalLine != null)
								{
									supAdditionalLine.US_CL_ParentLine = parentLineForSupEntryLine.PK;

									if (!invoiceLine.HasEmptySupTariff && invoiceLine.IsSetVLine)
									{
										parentLineForSupEntryLine = supAdditionalLine;
										parentLineForRegularEntryLine = supAdditionalLine;
									}
								}

								var supLine = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true);
								if (supLine != null && supLine != supAdditionalLine)
								{
									supLine.US_CL_ParentLine = parentLineForSupEntryLine.PK;

									if (supAdditionalLine == null && !invoiceLine.HasEmptySupTariff && invoiceLine.IsSetVLine)
									{
										parentLineForRegularEntryLine = supLine;
									}
								}

								var supAdditionalLine2 = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine2);
								if (supAdditionalLine2 != null)
								{
									supAdditionalLine2.US_CL_ParentLine = parentLineForSupEntryLine.PK;
								}

								var supAdditionalLine3 = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine3);
								if (supAdditionalLine3 != null)
								{
									supAdditionalLine3.US_CL_ParentLine = parentLineForSupEntryLine.PK;
								}

								var supAdditionalLine4 = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine4);
								if (supAdditionalLine4 != null)
								{
									supAdditionalLine4.US_CL_ParentLine = parentLineForSupEntryLine.PK;
								}

								var supAdditionalLine5 = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine5);
								if (supAdditionalLine5 != null)
								{
									supAdditionalLine5.US_CL_ParentLine = parentLineForSupEntryLine.PK;
								}

								entryLine.US_CL_ParentLine = parentLineForRegularEntryLine.PK;
							}
							else
							{
								var parentLinePKForRegularEntryLine = ZGuid.Empty;
								var parentLinePKForSupEntryLine = ZGuid.Empty;

								var supLine = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true);
								if (supLine != null)
								{
									parentLinePKForRegularEntryLine = supLine.PK;
									supLine.US_CL_ParentLine = ZGuid.Empty;
								}

								var supAdditionalLine = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine);
								if (supAdditionalLine != null)
								{
									parentLinePKForRegularEntryLine = supAdditionalLine.PK;
									parentLinePKForSupEntryLine = supAdditionalLine.PK;
									supAdditionalLine.US_CL_ParentLine = ZGuid.Empty;
								}

								if (supLine != null && supLine != supAdditionalLine)
								{
									supLine.US_CL_ParentLine = parentLinePKForSupEntryLine;
								}

								var supAdditionalLine2 = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine2);
								if (supAdditionalLine2 != null)
								{
									supAdditionalLine2.US_CL_ParentLine = parentLinePKForSupEntryLine;
								}

								var supAdditionalLine3 = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine3);
								if (supAdditionalLine3 != null)
								{
									supAdditionalLine3.US_CL_ParentLine = parentLinePKForSupEntryLine;
								}

								var supAdditionalLine4 = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine4);
								if (supAdditionalLine4 != null)
								{
									supAdditionalLine4.US_CL_ParentLine = parentLinePKForSupEntryLine;
								}

								var supAdditionalLine5 = invoiceLine.GetEntryLineFor(entry.CH_MessageType, true, (x) => x.US_SupAdditionalLine5);
								if (supAdditionalLine5 != null)
								{
									supAdditionalLine5.US_CL_ParentLine = parentLinePKForSupEntryLine;
								}

								entryLine.US_CL_ParentLine = parentLinePKForRegularEntryLine;
							}
						}
					}

					foreach (CusEntryLine entryLine in entry.MergedLines)
					{
						entryLine.RefreshChildLines();
					}
				}
			}
		}

		static bool ShouldBeManaged(CusEntryHeader entry)
		{
			return entry.IsFormalEntry || entry.IsFTZAdmission || entry.IsACECargoRelease;
		}
	}
}
