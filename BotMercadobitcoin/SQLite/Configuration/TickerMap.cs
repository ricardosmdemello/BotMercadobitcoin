using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.SQLite.Configuration
{
    public class TickerMap : IEntityTypeConfiguration<Domain.Ticker>
    {
        public void Configure(EntityTypeBuilder<Domain.Ticker> builder)
        {
            builder.ToTable("Ticker");

            builder.HasKey(t => t.Id);
        }
    }
}
