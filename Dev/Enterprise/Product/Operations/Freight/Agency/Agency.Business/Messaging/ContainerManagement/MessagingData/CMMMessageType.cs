namespace Enterprise.Freight.Agency.Business
{
	public enum CMMMessageType
	{
		/// <summary>
		/// Unknown / Unset message type
		/// </summary>
		Unknown,

		/// <summary>
		/// CODECO Gate In Message
		/// </summary>
		GateIn,

		/// <summary>
		/// CODECO Gate Out Message
		/// </summary>
		GateOut,

		/// <summary>
		/// COARRI Load Message
		/// </summary>
		Load,

		/// <summary>
		/// COARRI Discharge Message
		/// </summary>
		Discharge,
	}
}
