namespace Enterprise.Customs.US.eManifest.Business
{
	partial class ShipmentAmendmentCodes
	{
		internal ShipmentAmendmentCodes(bool isInBond)
		{
			if (!isInBond)
			{
				AddPair(Codes.C01, Descriptions.C01);
				AddPair(Codes.C02, Descriptions.C02);
				AddPair(Codes.C03, Descriptions.C03);
				AddPair(Codes.C04, Descriptions.C04);
				AddPair(Codes.C05, Descriptions.C05);
				AddPair(Codes.C06, Descriptions.C06);
				AddPair(Codes.C11, Descriptions.C11);
			}
			else
			{
				AddPair(Codes.C16, Descriptions.C16);
				AddPair(Codes.C17, Descriptions.C17);
			}

			AddPair(Codes.C18, Descriptions.C18);
			AddPair(Codes.C19, Descriptions.C19);

			if (isInBond)
			{
				AddPair(Codes.C26, Descriptions.C26);
			}

			AddPair(Codes.C27, Descriptions.C27);
			AddPair(Codes.C28, Descriptions.C28);
			AddPair(Codes.C29, Descriptions.C29);
			AddPair(Codes.C30, Descriptions.C30);
		}
	}
}
