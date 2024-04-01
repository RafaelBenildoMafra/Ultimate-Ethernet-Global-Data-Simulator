﻿// <auto-generated />
using System;
using EthernetGlobalData.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EthernetGlobalData.Migrations
{
    [DbContext(typeof(ProtocolContext))]
    partial class ProtocolContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("EthernetGlobalData.Models.Channel", b =>
                {
                    b.Property<int>("ChannelID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ChannelName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("IP")
                        .HasColumnType("TEXT");

                    b.Property<int>("Port")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChannelID");

                    b.HasIndex("ChannelName")
                        .IsUnique();

                    b.ToTable("Channels", (string)null);
                });

            modelBuilder.Entity("EthernetGlobalData.Models.Node", b =>
                {
                    b.Property<int>("NodeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChannelID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CommunicationType")
                        .HasColumnType("TEXT");

                    b.Property<ushort>("Exchange")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("MajorSignature")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("MinorSignature")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NodeName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("NodeID");

                    b.HasIndex("ChannelID");

                    b.HasIndex("NodeName")
                        .IsUnique();

                    b.ToTable("Nodes", (string)null);
                });

            modelBuilder.Entity("EthernetGlobalData.Models.Point", b =>
                {
                    b.Property<int>("PointID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("NodeID")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("PointID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("NodeID");

                    b.ToTable("Points", (string)null);
                });

            modelBuilder.Entity("EthernetGlobalData.Models.Node", b =>
                {
                    b.HasOne("EthernetGlobalData.Models.Channel", "Channel")
                        .WithMany()
                        .HasForeignKey("ChannelID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");
                });

            modelBuilder.Entity("EthernetGlobalData.Models.Point", b =>
                {
                    b.HasOne("EthernetGlobalData.Models.Node", "Node")
                        .WithMany("Points")
                        .HasForeignKey("NodeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Node");
                });

            modelBuilder.Entity("EthernetGlobalData.Models.Node", b =>
                {
                    b.Navigation("Points");
                });
#pragma warning restore 612, 618
        }
    }
}
