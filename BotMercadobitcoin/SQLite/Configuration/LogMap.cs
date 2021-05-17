using BotMercadobitcoin.SQLite.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.SQLite.Configuration
{
    public class LogMap : IEntityTypeConfiguration<Log>
    {
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.ToTable("Log");

            builder.HasKey(t => t.Id);

            //builder.Property(_ => _.Data).HasColumnType()
        }
    }
}
