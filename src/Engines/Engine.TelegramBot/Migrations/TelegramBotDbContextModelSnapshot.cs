﻿// <auto-generated />
using System;
using Engine.TelegramBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Engine.TelegramBot.Migrations
{
    [DbContext(typeof(TelegramBotDbContext))]
    partial class TelegramBotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("Engine.TelegramBot.Data.Entities.ProviderForChat", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ProviderId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ProviderForChats");
                });

            modelBuilder.Entity("Engine.TelegramBot.Data.Entities.TelegramUser", b =>
                {
                    b.Property<long>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ChatId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Engine.TelegramBot.Data.Entities.UserAttribute", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.ToTable("UserAttributes");
                });

            modelBuilder.Entity("Engine.TelegramBot.Data.Entities.UserAttribute", b =>
                {
                    b.HasOne("Engine.TelegramBot.Data.Entities.TelegramUser", "User")
                        .WithMany("Attributes")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Engine.TelegramBot.Data.Entities.TelegramUser", b =>
                {
                    b.Navigation("Attributes");
                });
#pragma warning restore 612, 618
        }
    }
}
