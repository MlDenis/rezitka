﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PostgreSqlMonitoringBot;

#nullable disable

namespace PostgreSqlMonitoringBot.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20231104122434_utcTime")]
    partial class utcTime
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PostgreSqlMonitoringBot.Metrica", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("activeSessionsCount")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("currentCpuUsage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("dt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("longestTransactionDuration")
                        .HasColumnType("interval");

                    b.Property<string>("sessionsWithLWLockCount")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("totalStorageSize")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.ToTable("Metrics");
                });

            modelBuilder.Entity("PostgreSqlMonitoringBot.TelegramUsers", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ChatId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("TelegramUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
