using System;

namespace CargoWise.Blazor.Common;

/// <summary>
/// Options for <EntryPointPool />
/// </summary>
public class EntryPointPoolOptions
{
	/// <summary>
	/// If true pooling is enabled, when false windows are launched as forms are created
	/// </summary>
	public bool Enabled { get; init; } = true;

	/// <summary>
	/// Size of the pool
	/// Determines the number entry point components to launch in advance of form creation
	/// </summary>
	[System.ComponentModel.DataAnnotations.Range(1, uint.MaxValue, ErrorMessage = "Pool size must always be greater than or equal to 1")]
	public uint MinimumPoolSize { get; init; } = 2;

	/// <summary>
	/// The amount of time the entry point pool should wait before reattempt form launch
	/// A default value inline with maximum recommended roundtrip times is set
	/// </summary>
	/// <returns></returns>
	public TimeSpan PoolFillWaitPeriod { get; init; } = TimeSpan.FromMilliseconds(220);
}