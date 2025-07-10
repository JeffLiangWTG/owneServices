namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public enum CargoIMPTransmissionMethod
	{
		SMTP,   // default
		FTP,    // supported only by client specific functionality (Yusen)
		eHub,
		eAdaptor
	}
}
