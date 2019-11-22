using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SampleCourier.Models
{
	public class RoutingSlipDbContextFactory : IDesignTimeDbContextFactory<RoutingSlipSagaDbContext>
	{
		public RoutingSlipDbContextFactory() { }

		public RoutingSlipDbContextFactory(string conStr)
		{
			_conStr = conStr;
		}

		private string _conStr = @"Data Source=localhost;Initial Catalog=EfCoreRoutingSlip;Persist Security Info=True;User ID=sa;Password=Password12345;MultipleActiveResultSets=true;";

		public RoutingSlipSagaDbContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<RoutingSlipSagaDbContext>();
			builder.UseSqlServer(_conStr);
			return new RoutingSlipSagaDbContext(builder.Options);
		}
	}
}