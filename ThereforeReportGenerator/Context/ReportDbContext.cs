using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ThereforeReportGenerator.Models;

namespace ThereforeReportGenerator.Context
{
    public class ReportDbContext : DbContext
    {
        public DbSet<ReportConfiguration> ReportConfigurations { get; set; }
        public DbSet<SMTPMailConfig> MailConfigs { get; set; }
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
        {

        }
    }
}
