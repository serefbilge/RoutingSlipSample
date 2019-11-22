using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCourier.ProcessingService.Config
{
	internal class MqActivityOptions
	{
		public ActivityConfig ValidateActivity { get; set; }

        public ActivityConfig PreProcessActivity { get; set; }

        public ActivityConfig CompensatePreProcessActivity { get; set; }

        public ActivityConfig RetrieveActivity { get; set; }

		public ActivityConfig CompensateRetrieveActivity { get; set; }

        public ActivityConfig PostProcessActivity { get; set; }

        public ActivityConfig CompensatePostProcessActivity { get; set; }
    }

	internal class ActivityConfig
	{
		public int PrefetchCount { get; set; }
		public string QueueName { get; set; }
		public int RetryLimit { get; set; }
	}
}
