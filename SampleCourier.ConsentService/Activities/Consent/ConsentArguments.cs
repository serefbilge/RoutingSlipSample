using System;

namespace SampleCourier.ConsentService.Activities.Consent
{
	public interface ConsentArguments
	{
		/// <summary>
		/// The requestId for eventing
		/// </summary>
		Guid RequestId { get; }

		/// <summary>
		/// The address of the content to retrieve
		/// </summary>
		Uri Address { get; }
	}
}