﻿// Copyright (c) DMO Consulting LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace SampleCourier.Common.Serilog
{
	public static class WebHostBuilderExts
	{
		public static IWebHostBuilder UseSerilogFromConfig(this IWebHostBuilder builder) =>
			(builder ?? throw new ArgumentNullException(nameof(builder)))
				.UseSerilog((ctx,cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));				
	}
}
