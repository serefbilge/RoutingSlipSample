using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCourier.WebApi.Config
{
	public class MqActivityOptions
	{
		public string ValidateAddress { get; set; }
        public string PreProcessAddress { get; set; }        
        public string RetrieveAddress { get; set; }
        public string PostProcessAddress { get; set; }
        public string ConsentAddress { get; set; }
    }
}
