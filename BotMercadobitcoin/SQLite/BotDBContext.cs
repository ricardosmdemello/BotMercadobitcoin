using BotMercadobitcoin.API.Retorno.Dados.v3;
using BotMercadobitcoin.SQLite.Configuration;
using BotMercadobitcoin.SQLite.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.SQLite
{
    /// <summary>
    /// https://medium.com/@douglaseverton/entity-framework-2-0-separando-a-configura%C3%A7%C3%A3o-das-entidades-em-classes-distintas-664d75329003
    /// https://github.com/douglasoliveirabh/TodoApp
    /// https://docs.microsoft.com/en-us/ef/core/get-started/netcore/new-db-sqlite
    /// http://www.macoratti.net/17/05/efcore_ini2.htm
    /// </summary>
    public class BotDBContext : DbContext
    {
        public DbSet<Domain.Trade> Trades { get; set; }
        public DbSet<Domain.Log> Logs { get; set; }
        public DbSet<Domain.Ticker> Tickers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./SQLite/Data/BotDB.db"); // ;datetimeformat=UnixEpoch;datetimekind=Utc");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<Domain.Trade>(new TradeMap());
            modelBuilder.ApplyConfiguration<Domain.Log>(new LogMap());
            modelBuilder.ApplyConfiguration<Domain.Ticker>(new TickerMap());
        }
    }

    //public class Blog
    //{
    //    public int BlogId { get; set; }
    //    public string Url { get; set; }

    //    public List<Post> Posts { get; set; }
    //}

    //public class Post
    //{
    //    public int PostId { get; set; }
    //    public string Title { get; set; }
    //    public string Content { get; set; }

    //    public int BlogId { get; set; }
    //    public Blog Blog { get; set; }
    //}
}
