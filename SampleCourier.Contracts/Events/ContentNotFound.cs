using System;
using SampleCourier.Contracts;

namespace SampleCourier.Contracts.Events
{
	public interface ContentNotFound : IEvent
	{
		/// <summary>
		/// The timestamp that the event occurred
		/// </summary>
		DateTime Timestamp { get; }

		/// <summary>
		/// The source address from which the image was retrieved
		/// </summary>
		Uri Address { get; }

		/// <summary>
		/// The reason why the image was not found
		/// </summary>
		string Reason { get; }
	}
}