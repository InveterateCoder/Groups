﻿// <auto-generated />
using Chat.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Chat.Web.Migrations
{
    [DbContext(typeof(ChatterersDb))]
    partial class ChatterersDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Chat.Web.Models.ChatterersDb+Chatterer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ConnectionId")
                        .HasMaxLength(64);

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<string>("Group")
                        .HasMaxLength(64);

                    b.Property<long>("GroupLastCleaned")
                        .HasMaxLength(32);

                    b.Property<string>("GroupPassword");

                    b.Property<int>("InGroupId");

                    b.Property<string>("InGroupPassword")
                        .HasMaxLength(32);

                    b.Property<long>("LastActive");

                    b.Property<string>("Name")
                        .HasMaxLength(64);

                    b.Property<string>("Password")
                        .HasMaxLength(32);

                    b.Property<string>("Token")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("Chatterers");
                });

            modelBuilder.Entity("Chat.Web.Models.ChatterersDb+Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("From")
                        .HasMaxLength(64);

                    b.Property<int>("GroupId");

                    b.Property<string>("Text")
                        .HasMaxLength(2048);

                    b.Property<long>("Time");

                    b.HasKey("Id");

                    b.ToTable("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
