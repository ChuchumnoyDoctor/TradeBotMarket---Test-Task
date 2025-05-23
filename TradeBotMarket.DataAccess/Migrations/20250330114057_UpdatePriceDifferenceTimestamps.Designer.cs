﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TradeBotMarket.DataAccess.Data;

#nullable disable

namespace TradeBotMarket.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250330114057_UpdatePriceDifferenceTimestamps")]
    partial class UpdatePriceDifferenceTimestamps
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TradeBotMarket.Domain.Models.FuturePrice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsLastAvailable")
                        .HasColumnType("boolean");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Symbol", "Timestamp")
                        .IsUnique();

                    b.ToTable("FuturePrices");
                });

            modelBuilder.Entity("TradeBotMarket.Domain.Models.PriceDifference", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Difference")
                        .HasColumnType("numeric");

                    b.Property<decimal>("FirstPrice")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("FirstPriceTimestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FirstSymbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("SecondPrice")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("SecondPriceTimestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SecondSymbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FirstSymbol", "SecondSymbol", "FirstPriceTimestamp", "SecondPriceTimestamp");

                    b.ToTable("PriceDifferences");
                });
#pragma warning restore 612, 618
        }
    }
}
