using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;



namespace CoreBotCLU.Models {

public partial class TravelbotDbContext : DbContext
{
    public TravelbotDbContext()
    {
    }

    public TravelbotDbContext(DbContextOptions<TravelbotDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Airport> Airports { get; set; }

    public virtual DbSet<FlightsDemand> FlightsDemands { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){

            var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json");

            var config = builder.Build();

            var connectionString = config.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");


        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        optionsBuilder.UseSqlServer(connectionString);

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__airports__3213E83F88DC24BA");

            entity.ToTable("airports");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.City)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.CityIt)
                .IsRequired()
                .HasMaxLength(51)
                .IsUnicode(false)
                .HasColumnName("city_it");
            entity.Property(e => e.Country)
                .HasMaxLength(21)
                .IsUnicode(false)
                .HasColumnName("country");
            entity.Property(e => e.IataCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("iata_code");
            entity.Property(e => e.IcaoCode)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("icao_code");
            entity.Property(e => e.Name)
                .HasMaxLength(47)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<FlightsDemand>(entity =>
        {
            entity.HasKey(e => e.DemandId);

            entity.ToTable("flights_demands");

            entity.Property(e => e.DemandId).HasColumnName("demandID");
            entity.Property(e => e.DepartureDate)
            .HasMaxLength(10)
            .HasColumnName("departureDate");
            entity.Property(e => e.Destination)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("destination");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Origin)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("origin");
            entity.Property(e => e.Passengers)
                .HasDefaultValueSql("((1))")
                .HasColumnName("passengers");
            entity.Property(e => e.CurrentPrice).HasColumnName("currentPrice");
            entity.Property(e => e.NewPrice).HasColumnName("newPrice");
            entity.Property(e => e.Notify)
             .HasDefaultValueSql("N")
             .HasMaxLength(1)
            .HasColumnName("notify");
            entity.Property(e => e.ReturnDate)
            .HasMaxLength(10)
            .HasColumnName("returnDate");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
}
